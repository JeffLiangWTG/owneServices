using System.Diagnostics;
using System.IO;
using System.Linq;
using CargoWise.IO;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Macros.Testing
{
	sealed class PerformanceTest : TestCase
	{
		[DeveloperOnlyTest]
		public void TestEvaluationTime_MostCommonExpr()
		{
			var expressions = Enumerable
				.Repeat("\"abc\" == \"123\"", 10000)
				.ToArray();

			AssertEvaluationTime(expressions);
		}

		[DeveloperOnlyTest]
		public void TestEvaluationTime_MainfreightMenuItemFilters()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var filePath = resourceRetriever.SaveResourceToFile("Enterprise.DocumentEngine.Macros.Testing.mainfreight-menu-filters.txt");
				var expressions = File.ReadAllLines(filePath);

				AssertEvaluationTime(expressions);
			}
		}

		void AssertEvaluationTime(string[] expressions)
		{
			var stopWatch = new Stopwatch();
			stopWatch.Start();

			foreach (var expression in expressions)
			{
				expression.Evaluate();
			}

			stopWatch.Stop();

			var evalTime = stopWatch.ElapsedMilliseconds;

			stopWatch.Restart();

			foreach (var expression in expressions)
			{
				Utilities.ExpressionEvaluator.EvaluateJS(expression);
			}

			stopWatch.Stop();

			var jsEvalTime = stopWatch.ElapsedMilliseconds;

			Fail($"Macros: {expressions.Length}, eval time {evalTime}, JS eval time {jsEvalTime}");
		}
	}
}
