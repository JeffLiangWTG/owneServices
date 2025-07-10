using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.APAutomation.APReconciliation;
using Enterprise.Accounting.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.APAutomation.Testing
{
	public class AccrualSummatorTest : TestCaseWithFactory
	{
		public void TestWhenExactlyOneMatchFoundUsingOSAmount()
		{
			IAccrualSummator c = new AccrualSummator();
			var result = c.Sumup(reconciliationLines, 25.00m, line => line.OSExTaxAmount).ToList();
			AssertEquals(1, result.Count);
			var matchedAccruals = result.Select(x => x.Accruals).First();
			AssertContainsExactElementsInAnyOrder(new List<ZGuid>() { new ZGuid("498F75F4-A9D7-4CB8-A453-FAE522AD92AD"), new ZGuid("C30A7230-24FF-4ACF-B584-D8EE69CAD614") }, matchedAccruals.Select(x => x.LineIdentifier));
		}

		public void TestWhenExactlyOneMatchFoundUsingLocalAmount()
		{
			IAccrualSummator c = new AccrualSummator();
			var result = c.Sumup(reconciliationLines, 27.00m, line => line.LocalExTaxAmount).ToList();
			AssertEquals(1, result.Count);
			var matchedAccruals = result.Select(x => x.Accruals).First();
			AssertContainsExactElementsInAnyOrder(new List<ZGuid>() { new ZGuid("498F75F4-A9D7-4CB8-A453-FAE522AD92AD"), new ZGuid("C30A7230-24FF-4ACF-B584-D8EE69CAD614") }, matchedAccruals.Select(x => x.LineIdentifier));
		}

		public void TestWhenNoMatchFound()
		{
			IAccrualSummator c = new AccrualSummator();
			var osAmountResult = c.Sumup(reconciliationLines, 128.00m, line => line.OSExTaxAmount);
			AssertEquals(0, osAmountResult.Count());

			var localAmountResult = c.Sumup(reconciliationLines, 128.00m, line => line.LocalExTaxAmount);
			AssertEquals(0, localAmountResult.Count());
		}

		public void TestExceptionThrownWhenMultipleMatchFound()
		{
			IAccrualSummator c = new AccrualSummator();
			AssertExceptionThrown<APAReconciliationTooManyMatchesFoundException>("Expect to get exception due to multiple matches found", () => c.Sumup(reconciliationLines, 120.00m, line => line.OSExTaxAmount));
		}

		[ExpectNoExceptions("No OutOfMemory Exception expected")]
		public void TestPerformanceWithLargeNumberOfLines()
		{
			var largeCollectionOfLines = new List<APReconciliationLine>();
			for (var i = 1; i < 200; i++)
			{
				largeCollectionOfLines.Add(new APReconciliationLine() { LineIdentifier = ZGuid.NewZGuid(), OSExTaxAmount = i });
			}
			IAccrualSummator c = new AccrualSummator();
			var result = Enumerable.Empty<(string, IEnumerable<APReconciliationLine>)>();
			try
			{
				result = c.Sumup(largeCollectionOfLines, 96.7m, line => line.OSExTaxAmount);
			}
			catch (APAReconciliationTimeoutException)
			{
				//timeout happen before OutOfMemory, we are fine with timeout.
			}
			finally
			{
				AssertEquals(0, result.Count());
				Assert("Expect recursive algorithm has been called!", ((AccrualSummator)c).EnterRecursiveCall_ForTestOnly);
			}
		}

		public void TestExceptionThrownWhenTimeout()
		{
			var largeCollectionOfLines = new List<APReconciliationLine>();
			for (var i = 1; i < 10; i++)
			{
				largeCollectionOfLines.Add(new APReconciliationLine() { LineIdentifier = ZGuid.NewZGuid(), OSExTaxAmount = i });
			}
			IAccrualSummator c = new AccrualSummator();
			((AccrualSummator)c).TimeOutInMilliSeconds = 2000;
			((AccrualSummator)c).SimulateLongDelayInMillisec_ForTestOnly = (int)((AccrualSummator)c).TimeOutInMilliSeconds + 1000;
			AssertExceptionThrown<APAReconciliationTimeoutException>("Expect to get exception due to time out error", () => c.Sumup(largeCollectionOfLines, 6.7m, line => line.OSExTaxAmount));
			Assert("Expect recursive algorithm has been called!", ((AccrualSummator)c).EnterRecursiveCall_ForTestOnly);
		}

		public void TestPerformanceWithLargeNumberOfLines_TargetIsTooBig()
		{
			var largeCollectionOfLines = new List<APReconciliationLine>();
			for (var i = 1; i < 10; i++)
			{
				largeCollectionOfLines.Add(new APReconciliationLine() { LineIdentifier = ZGuid.NewZGuid(), OSExTaxAmount = i });
			}
			IAccrualSummator c = new AccrualSummator();
			var w = Stopwatch.StartNew();
			var result = c.Sumup(largeCollectionOfLines, 1000000m, line => line.OSExTaxAmount);
			w.Stop();
			AssertEquals(0, result.Count());
			Assert("Expect recursive algorithm call is skipped!", !((AccrualSummator)c).EnterRecursiveCall_ForTestOnly);
		}

		public void TestPerformanceWithLargeNumberOfLines_TargetIsTooSmall()
		{
			var largeCollectionOfLines = new List<APReconciliationLine>();
			for (var i = 1; i < 10; i++)
			{
				largeCollectionOfLines.Add(new APReconciliationLine() { LineIdentifier = ZGuid.NewZGuid(), OSExTaxAmount = i });
			}
			IAccrualSummator c = new AccrualSummator();
			var w = Stopwatch.StartNew();
			var result = c.Sumup(largeCollectionOfLines, -20m, line => line.OSExTaxAmount);
			w.Stop();
			AssertEquals(0, result.Count());
			Assert("Expect recursive algorithm call is skipped!", !((AccrualSummator)c).EnterRecursiveCall_ForTestOnly);
		}

		public void TestWhenAmountsAreNegative()
		{
			IAccrualSummator c = new AccrualSummator();
			var result = c.Sumup(allNegativeReconciliationLines, -70.00m, line => line.OSExTaxAmount).ToList();
			AssertEquals(1, result.Count);
			var matchedAccruals = result.Select(x => x.Accruals).First();
			AssertContainsExactElementsInAnyOrder(new List<ZGuid>() { new ZGuid("FB237943-0DCE-47E7-91D2-4233A04799E2"), new ZGuid("3221E3AB-A8BF-4CCF-B9A8-E775522D717D") }, matchedAccruals.Select(x => x.LineIdentifier));
		}

		public void TestWhenAmountSignsAreMixed()
		{
			IAccrualSummator c = new AccrualSummator();
			var result = c.Sumup(mixedReconciliationLines, 60.00m, line => line.OSExTaxAmount).ToList();
			AssertEquals(1, result.Count);
			var matchedAccruals = result.Select(x => x.Accruals).First();
			AssertContainsExactElementsInAnyOrder(new List<ZGuid>() { new ZGuid("FB237943-0DCE-47E7-91D2-4233A04799E2"), new ZGuid("3221E3AB-A8BF-4CCF-B9A8-E775522D717D"), new ZGuid("C30A7230-24FF-4ACF-B584-D8EE69CAD614") }, matchedAccruals.Select(x => x.LineIdentifier));
		}

		static readonly List<APReconciliationLine> reconciliationLines = new List<APReconciliationLine>()
		{
			new APReconciliationLine() { LineIdentifier = new ZGuid("498F75F4-A9D7-4CB8-A453-FAE522AD92AD"), OSExTaxAmount = 12.00m, LocalExTaxAmount = 13.00m },
			new APReconciliationLine() { LineIdentifier = new ZGuid("C30A7230-24FF-4ACF-B584-D8EE69CAD614"), OSExTaxAmount = 13.00m, LocalExTaxAmount = 14.00m },
			new APReconciliationLine() { LineIdentifier = new ZGuid("FB237943-0DCE-47E7-91D2-4233A04799E2"), OSExTaxAmount = 30.00m, LocalExTaxAmount = 31.00m },
			new APReconciliationLine() { LineIdentifier = new ZGuid("3221E3AB-A8BF-4CCF-B9A8-E775522D717D"), OSExTaxAmount = 40.00m, LocalExTaxAmount = 41.00m },
			new APReconciliationLine() { LineIdentifier = new ZGuid("599C442E-9372-4EA2-8E5A-9FC74867EDED"), OSExTaxAmount = 50.00m, LocalExTaxAmount = 51.00m },
			new APReconciliationLine() { LineIdentifier = new ZGuid("938C8CF6-080C-47E5-A55F-B657FB719C7D"), OSExTaxAmount = 60.00m, LocalExTaxAmount = 61.00m },
			new APReconciliationLine() { LineIdentifier = new ZGuid("05F67D83-3CC9-4EA9-9197-E3FF334D452A"), OSExTaxAmount = 70.00m, LocalExTaxAmount = 71.00m },
			new APReconciliationLine() { LineIdentifier = new ZGuid("19BE6448-B24E-45F5-8B35-772131A73D91"), OSExTaxAmount = 80.00m, LocalExTaxAmount = 81.00m }
		};

		static readonly List<APReconciliationLine> allNegativeReconciliationLines = new List<APReconciliationLine>()
		{
			new APReconciliationLine() { LineIdentifier = new ZGuid("498F75F4-A9D7-4CB8-A453-FAE522AD92AD"), OSExTaxAmount = -12.00m, LocalExTaxAmount = -13.00m },
			new APReconciliationLine() { LineIdentifier = new ZGuid("C30A7230-24FF-4ACF-B584-D8EE69CAD614"), OSExTaxAmount = -13.00m, LocalExTaxAmount = -14.00m },
			new APReconciliationLine() { LineIdentifier = new ZGuid("FB237943-0DCE-47E7-91D2-4233A04799E2"), OSExTaxAmount = -30.00m, LocalExTaxAmount = -31.00m },
			new APReconciliationLine() { LineIdentifier = new ZGuid("3221E3AB-A8BF-4CCF-B9A8-E775522D717D"), OSExTaxAmount = -40.00m, LocalExTaxAmount = -41.00m },
		};

		static readonly List<APReconciliationLine> mixedReconciliationLines = new List<APReconciliationLine>()
		{
			new APReconciliationLine() { LineIdentifier = new ZGuid("498F75F4-A9D7-4CB8-A453-FAE522AD92AD"), OSExTaxAmount = -12.00m, LocalExTaxAmount = -13.00m },
			new APReconciliationLine() { LineIdentifier = new ZGuid("C30A7230-24FF-4ACF-B584-D8EE69CAD614"), OSExTaxAmount = -10.00m, LocalExTaxAmount = -14.00m },
			new APReconciliationLine() { LineIdentifier = new ZGuid("FB237943-0DCE-47E7-91D2-4233A04799E2"), OSExTaxAmount = 30.00m, LocalExTaxAmount = 31.00m },
			new APReconciliationLine() { LineIdentifier = new ZGuid("3221E3AB-A8BF-4CCF-B9A8-E775522D717D"), OSExTaxAmount = 40.00m, LocalExTaxAmount = 41.00m },
		};
	}
}
