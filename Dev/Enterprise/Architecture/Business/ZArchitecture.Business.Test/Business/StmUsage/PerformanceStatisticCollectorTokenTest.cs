using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class PerformanceStatisticCollectorTokenTest : TestCaseWithFactory
	{
		[TestDate(2018, 6, 6, 12, 07, 21)]
		public void TestTokenSuspendBehaviour()
		{
			ObjectFactory.Substitute<IStopwatch>(() => new TestDateStopwatch());

			var token = new PerformanceStatisticCollectorToken("name", "subname");
			AssertEquals(false, token.IsSuspended);
			TestDateAttribute.AddMilliseconds(5);

			using (token.Suspend())
			{
				AssertEquals(true, token.IsSuspended);
				AssertEquals(5, (int)token.ElapsedExcludingChildren.TotalMilliseconds);
				TestDateAttribute.AddMilliseconds(250);
				using (token.Suspend())
				{
					AssertEquals(true, token.IsSuspended);
					AssertEquals(5, (int)token.ElapsedExcludingChildren.TotalMilliseconds);
					TestDateAttribute.AddMilliseconds(250);
				}
				AssertEquals(true, token.IsSuspended);
				AssertEquals(5, (int)token.ElapsedExcludingChildren.TotalMilliseconds);
				TestDateAttribute.AddMilliseconds(250);
			}

			AssertEquals(false, token.IsSuspended);
			AssertEquals(5, (int)token.ElapsedExcludingChildren.TotalMilliseconds);
			TestDateAttribute.AddMilliseconds(5);
			token.Stop();
			AssertEquals(10, (int)token.ElapsedExcludingChildren.TotalMilliseconds);
		}

		// test need stable date so minute is not triggered
		[TestDate(2012, 8, 12)]
		public void TestStructuralEquivalence()
		{
			{
				var e1 =
					new PerformanceStatisticCollectorToken("Item1", "tabs",
						new PerformanceStatisticCollectorToken("Item2", "tabs"),
						new PerformanceStatisticCollectorToken("Item3", "tabs",
							new PerformanceStatisticCollectorToken("Item4", "tabs")),
						new PerformanceStatisticCollectorToken("Item5", "tabs")
					);

				var e2 =
					new PerformanceStatisticCollectorToken("Item1", "tabs",
						new PerformanceStatisticCollectorToken("Item2", "tabs"),
						new PerformanceStatisticCollectorToken("Item3", "tabs",
							new PerformanceStatisticCollectorToken("Item4", "tabs")),
						new PerformanceStatisticCollectorToken("Item5", "tabs")
					);

				AssertEquals("Elements are structurally equivalent", true, e1.CanFold(e2));
			}
			{
				var e1 =
					new PerformanceStatisticCollectorToken("Item1", "tabs",
						new PerformanceStatisticCollectorToken("Item2", "tabs"),
						new PerformanceStatisticCollectorToken("Item3", "tabs",
							new PerformanceStatisticCollectorToken("Item4", "tabs")),
						new PerformanceStatisticCollectorToken("Item5", "tabs")
					);

				var e2 =
					new PerformanceStatisticCollectorToken("Item1", "tabs",
						new PerformanceStatisticCollectorToken("Item2", "tabs"),
						new PerformanceStatisticCollectorToken("Item3", "tabs",
							new PerformanceStatisticCollectorToken("Item4", "tabs"))
					);

				AssertEquals("Elements are not structurally equivalent", false, e1.CanFold(e2));
			}
		}

		public void TestRecursiveFolding() // specific to this problem, not the generalised one
		{
			var ranch =
				new PerformanceStatisticCollectorToken("Ranch", 1,
					new PerformanceStatisticCollectorToken("Item3TotallyDifferent", 1,
						new PerformanceStatisticCollectorToken("Item5", 1),
						new PerformanceStatisticCollectorToken("Item4", 1)),
					new PerformanceStatisticCollectorToken("Item3", 1,
						new PerformanceStatisticCollectorToken("Item5", 1),
						new PerformanceStatisticCollectorToken("Item4", 1)));
			var item5 =
				new PerformanceStatisticCollectorToken("Ranch", 1,
					new PerformanceStatisticCollectorToken("Item3TotallyDifferent", 1,
						new PerformanceStatisticCollectorToken("Item5", 1),
						new PerformanceStatisticCollectorToken("Item4", 1)),
					new PerformanceStatisticCollectorToken("Item3", 1,
						new PerformanceStatisticCollectorToken("Item5", 1),
						new PerformanceStatisticCollectorToken("Item4", 1)));
			var expected =
				new PerformanceStatisticCollectorToken("Ranch", 2,
					new PerformanceStatisticCollectorToken("Item3TotallyDifferent", 2,
						new PerformanceStatisticCollectorToken("Item5", 2),
						new PerformanceStatisticCollectorToken("Item4", 2)),
					new PerformanceStatisticCollectorToken("Item3", 2,
						new PerformanceStatisticCollectorToken("Item5", 2),
						new PerformanceStatisticCollectorToken("Item4", 2)));
			PerformanceStatisticCollectorToken.ZipSecondIntoFirst(ranch, item5);
			Assert("XML folded Correctly", expected.CanFold(ranch)); // Says that all is well.
		}

		public void TestNoErrorUsingTokenWithoutSettingBranch()
		{
			var env = Environment.EnvProxy.Instance;
			using (env.TemporaryServiceTaskContext("XYZ", true))
			{
				var token = new PerformanceStatisticCollectorToken("Hello", "There");
			}
			AssertNull(ErrorReporter.LastExceptionReported);
		}
	}
}
