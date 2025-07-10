using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE818;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.tcl;
using CargoWise.Types;

namespace Enterprise.Customs.IE.EMCS.Messaging.Phase4_1.Testing
{
	sealed class IE818ReportOfReceiptProviderTest : Business.Testing.DataProviderTestCase<IE818ReportOfReceiptProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new IE818ReportOfReceiptProvider(null));
		}

		public void TestLineNumber()
		{
			AssertEquals("1", Provider.LineNumber);
		}

		public void TestIndicatorOfShortageOrExcess()
		{
			AssertEquals("E", Provider.IndicatorOfShortageOrExcess);
		}

		public void TestIndicatorOfShortageOrExcess_NotSpecified()
		{
			var provider = Provider;
			reportOfReceipt.IndicatorOfShortageOrExcessValueSpecified = false;
			AssertEquals(ZString.Empty, provider.IndicatorOfShortageOrExcess);
		}

		public void TestObservedQuantity()
		{
			AssertEquals(2m, Provider.ObservedQuantity);
		}

		public void TestObservedQuantity_NotSpecified()
		{
			var provider = Provider;
			reportOfReceipt.ObservedShortageOrExcessValueSpecified = false;
			AssertEquals(0m, provider.ObservedQuantity);
		}

		public void TestRefusedQuantity()
		{
			AssertEquals(3m, Provider.RefusedQuantity);
		}

		public void TestRefusedQuantity_NotSpecified()
		{
			var provider = Provider;
			reportOfReceipt.RefusedQuantityValueSpecified = false;
			AssertEquals(0m, provider.RefusedQuantity);
		}

		public void TestUnsatisfactoryReasons()
		{
			CombineAssertions(() =>
			{
				var unsatisfactoryReasons = Provider.UnsatisfactoryReasons;
				AssertEquals("1 Record", 1, unsatisfactoryReasons.Count);
			});
		}

		protected override IEnumerable<Expression<Func<IE818ReportOfReceiptProvider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.UnsatisfactoryReasons;
		}

		protected override IE818ReportOfReceiptProvider GetProvider()
		{
			reportOfReceipt = new BodyReportOfReceiptExportType
			{
				BodyRecordUniqueReference = "1",
				IndicatorOfShortageOrExcessValueSpecified = true,
				IndicatorOfShortageOrExcessValue = IndicatorOfShortageOrExcess.E,
				ObservedShortageOrExcessValueSpecified = true,
				ObservedShortageOrExcessValue = 2m,
				RefusedQuantityValueSpecified = true,
				RefusedQuantityValue = 3m,
				UnsatisfactoryReason = new Collection<UnsatisfactoryReasonType>
				{
					new UnsatisfactoryReasonType
					{
						UnsatisfactoryReasonCode = "UFRC01",
						ComplementaryInformation = new LsdComplementaryInformationType
						{
							Language = "en",
							Value = "CI001"
						},
					},
				}
			};
			return new IE818ReportOfReceiptProvider(reportOfReceipt);
		}
		BodyReportOfReceiptExportType reportOfReceipt;
	}
}
