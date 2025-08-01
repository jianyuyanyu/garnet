﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Garnet.common
{
    /// <summary>
    /// Exception injection helper - used only in debug mode for testing
    /// </summary>
    public static class ExceptionInjectionHelper
    {
        /// <summary>
        /// Array of exception injection types
        /// </summary>
        static readonly bool[] ExceptionInjectionTypes =
            Enum.GetValues<ExceptionInjectionType>().Select(_ => false).ToArray();

        /// <summary>
        /// Check if exception is enabled
        /// </summary>
        /// <param name="exceptionType"></param>
        /// <returns></returns>
        public static bool IsEnabled(ExceptionInjectionType exceptionType) => ExceptionInjectionTypes[(int)exceptionType];

        /// <summary>
        /// Enable exception scenario (NOTE: enable at beginning of test to trigger the exception at runtime)
        /// </summary>
        /// <param name="exceptionType"></param>
        [Conditional("DEBUG")]
        public static void EnableException(ExceptionInjectionType exceptionType) => ExceptionInjectionTypes[(int)exceptionType] = true;

        /// <summary>
        /// Disable exception scenario (NOTE: for tests you need to always call disable at the end of the test to avoid breaking other tests in the line)
        /// </summary>
        /// <param name="exceptionType"></param>
        [Conditional("DEBUG")]
        public static void DisableException(ExceptionInjectionType exceptionType) => ExceptionInjectionTypes[(int)exceptionType] = false;

        /// <summary>
        /// Trigger exception scenario (NOTE: add this to the location where the exception should be emulated/triggered)
        /// </summary>
        /// <param name="exceptionType"></param>
        /// <exception cref="GarnetException"></exception>
        [Conditional("DEBUG")]
        public static void TriggerException(ExceptionInjectionType exceptionType)
        {
            if (ExceptionInjectionTypes[(int)exceptionType])
                throw new GarnetException($"Exception injection triggered {exceptionType}");
        }

        /// <summary>
        /// Trigger condition and reset it
        /// </summary>
        /// <param name="exceptionType"></param>
        /// <returns></returns>
        public static bool TriggerCondition(ExceptionInjectionType exceptionType)
        {
#if DEBUG
            if (ExceptionInjectionTypes[(int)exceptionType])
            {
                ExceptionInjectionTypes[(int)exceptionType] = false;
                return true;
            }
            return false;
#else
            return false;
#endif
        }

        /// <summary>
        /// Wait on set condition
        /// </summary>
        /// <param name="exceptionType"></param>
        /// <returns></returns>
        public static async Task WaitOnSet(ExceptionInjectionType exceptionType)
        {
            var flag = ExceptionInjectionTypes[(int)exceptionType];
            if (flag)
            {
                // Reset and wait to signaled to go forward
                ExceptionInjectionTypes[(int)exceptionType] = false;
                while (!ExceptionInjectionTypes[(int)exceptionType])
                    await Task.Yield();
            }
        }

        /// <summary>
        /// Wait on clear condition
        /// </summary>
        /// <param name="exceptionType"></param>
        /// <returns></returns>
        public static async Task WaitOnClearAsync(ExceptionInjectionType exceptionType)
        {
            while (ExceptionInjectionTypes[(int)exceptionType])
                await Task.Yield();
        }
    }
}