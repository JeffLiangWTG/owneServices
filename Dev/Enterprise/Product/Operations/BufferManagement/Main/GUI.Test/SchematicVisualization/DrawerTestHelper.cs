using System;
using System.Collections.Generic;
using CargoWise.Windows.UI;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	static class DrawerTestHelper
	{
		public static void Test_Sizes_ShouldScaleToDpi(Func<List<int>> drawAndReturnSizesFunc)
		{
			var dpiMultiplierTests = new double[] { 0.5, 0.75, 0.9, 1, 1.25, 1.5, 2 };

			var baselineResults = DrawAndReturn(dpiMultiplier: 1, drawAndReturnSizesFunc);

			TestCase.CombineAssertions(() =>
			{
				foreach (var test in dpiMultiplierTests)
				{
					var currentBaselineIndex = 0;
					var testResults = DrawAndReturn(test, drawAndReturnSizesFunc);

					foreach (var result in testResults)
					{
						int expectedSize = (int)(baselineResults[currentBaselineIndex] * test);

						TestCase.AssertCloseEnough($"Size should scale at dpi multiplier {test}.",
							expectedSize,
							result);

						currentBaselineIndex++;
					}
				}
			});
		}

		static List<int> DrawAndReturn(double dpiMultiplier, Func<List<int>> drawAndReturnFunc)
		{
			using (ControlDpiScalingHelper.OverrideDPI_ForTesting(96 * (float)dpiMultiplier, 96 * (float)dpiMultiplier))
			{
				return drawAndReturnFunc();
			}
		}
	}
}
