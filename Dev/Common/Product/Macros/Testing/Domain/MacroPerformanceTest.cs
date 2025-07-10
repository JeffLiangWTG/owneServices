using System;
using System.Diagnostics;
using NUnit.Framework;

namespace CargoWise.Macros.Testing
{
	class MacroPerformanceTest : TestCase
	{
		public void TestMacroCompilationPerformance()
		{
			var scope = new MacroScope(GetData());

			var stopwatch = new Stopwatch();
			stopwatch.Start();

			var macroExpr = macro.CreateExpression();
			var expr = macroExpr.Expression;

			stopwatch.Stop();

			const int maxAllowedSeconds = 1;

			if (stopwatch.Elapsed.Seconds > maxAllowedSeconds)
			{
				Fail(string.Format(@"Time taken to compile macro {0}. Expected to be {1} sec or under", stopwatch.Elapsed, maxAllowedSeconds));
			}
			else if (!TestingState.IsRunningOnDAT)
			{
				Fail(string.Format(@"DEVELOPER ONLY... Time taken to compile macro {0}", stopwatch.Elapsed));
			}
			else
			{
				Assert(true);
			}
		}

		[SnailTest]
		public void TestBinaryOperationPerformance()
		{
			var scope = new MacroScope(GetData());

			var compilationTime = TimeSpan.Zero;

			var stopwatch = new Stopwatch();
			stopwatch.Start();

			var macroExpr = macro.CreateExpression();
			var macroHandler = macroExpr.Handler;

			compilationTime = stopwatch.Elapsed;

			for (int i = 0; i < interations; i++)
			{
				macroHandler(scope);
			}

			stopwatch.Stop();

			const int maxAllowedSeconds = 30;

			if (stopwatch.Elapsed.Seconds > maxAllowedSeconds)
			{
				Fail(string.Format(@"Time taken to run macro {0} times {1}. This time includes one off parsing and compilation which took {2}. Expected to be {3} sec or under.",
					interations, stopwatch.Elapsed, compilationTime, maxAllowedSeconds));
			}
			else if (!TestingState.IsRunningOnDAT)
			{
				Fail(string.Format(@"DEVELOPER ONLY... Time taken to run macro {0} times {1}. This time includes one off parsing and compilation which took {2}.",
					interations, stopwatch.Elapsed, compilationTime));
			}
			else
			{
				Assert(true);
			}
		}

		#region Implementation

		const string macro = "1.2 * 2 > Integer / 1.1 && String == none || Char == \"x\"";
		const int interations = 1000000;

		class Data
		{
			public int Integer { get; set; }
			public bool Bool { get; set; }
			public string String { get; set; }
			public char Char { get; set; }
		}

		Data GetData()
		{
			return new Data
			{
				Integer = 2,
				Bool = false,
				String = null,
				Char = 'x'
			};
		}

		#endregion
	}
}