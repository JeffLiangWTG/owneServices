using System;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_4.Testing
{
	[TestedType(typeof(ED818ReportOfReceiptProvider))]
	public class ED818ReportOfReceiptProviderTest : InboundDataProviderTestCase<IED818ReportOfReceipt, ED818ReportOfReceiptProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new ED818ReportOfReceiptProvider(null));
		}

		public void TestLineNumber()
		{
			AssertEquals("1", dataProvider.LineNumber);
		}

		public void TestIndicatorOfShortageOrExcess()
		{
			AssertEquals("E", dataProvider.IndicatorOfShortageOrExcess);
		}

		public void TestIndicatorOfShortageOrExcess_NotSpecified()
		{
			reportOfReceipt.IndicatorOfShortageOrExcessSpecified = false;
			AssertEquals(ZString.Empty, dataProvider.IndicatorOfShortageOrExcess);
		}

		public void TestObservedQuantity()
		{
			AssertEquals(2m, dataProvider.ObservedQuantity);
		}

		public void TestObservedQuantity_NotSpecified()
		{
			reportOfReceipt.ObservedShortageOrExcessSpecified = false;
			AssertEquals(0m, dataProvider.ObservedQuantity);
		}

		public void TestRefusedQuantity()
		{
			AssertEquals(3m, dataProvider.RefusedQuantity);
		}

		public void TestRefusedQuantity_NotSpecified()
		{
			reportOfReceipt.RefusedQuantitySpecified = false;
			AssertEquals(0m, dataProvider.RefusedQuantity);
		}

		public void TestUnsatisfactoryReasons()
		{
			CombineAssertions(() =>
			{
				var unsatisfactoryReasons = dataProvider.UnsatisfactoryReasons;
				AssertEquals("1 Record", 1, unsatisfactoryReasons.Count);
				AssertSame("Cached", unsatisfactoryReasons, dataProvider.UnsatisfactoryReasons);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			reportOfReceipt = new ED818CBodyAcceptedOrRejectedReportOfReceiptBodyReportOfReceipt
			{
				BodyRecordUniqueReference = "1",
				IndicatorOfShortageOrExcessSpecified = true,
				IndicatorOfShortageOrExcess = ED818CBodyAcceptedOrRejectedReportOfReceiptBodyReportOfReceiptIndicatorOfShortageOrExcess.E,
				ObservedShortageOrExcessSpecified = true,
				ObservedShortageOrExcess = 2m,
				RefusedQuantitySpecified = true,
				RefusedQuantity = 3m,
				UnsatisfactoryReason = new ED818CBodyAcceptedOrRejectedReportOfReceiptBodyReportOfReceiptUnsatisfactoryReason[]
				{
					new ED818CBodyAcceptedOrRejectedReportOfReceiptBodyReportOfReceiptUnsatisfactoryReason
					{
						UnsatisfactoryReasonCode = "UFRC01",
						ComplementaryInformation = "CI001",
					},
				}
			};
			dataProvider = new ED818ReportOfReceiptProvider(reportOfReceipt);
		}
		ED818CBodyAcceptedOrRejectedReportOfReceiptBodyReportOfReceipt reportOfReceipt;
		IED818ReportOfReceipt dataProvider;

		protected override ED818ReportOfReceiptProvider GetProvider() => (ED818ReportOfReceiptProvider)dataProvider;
	}
}
