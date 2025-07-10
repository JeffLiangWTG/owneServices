using System;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_5.Testing
{
	[TestedType(typeof(ED818UnsatisfactoryReasonProvider))]
	public class ED818UnsatisfactoryReasonProviderTest : InboundDataProviderTestCase<IED818UnsatisfactoryReason, ED818UnsatisfactoryReasonProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new ED818UnsatisfactoryReasonProvider(null));
		}

		public void TestReasonCode()
		{
			AssertEquals("UFRC01", dataProvider.ReasonCode);
		}

		public void TestComplementaryInformation()
		{
			AssertEquals("CI001", dataProvider.ComplementaryInformation);
		}

		protected override void SetUp()
		{
			base.SetUp();

			reason = new ED818DBodyAcceptedOrRejectedReportOfReceiptBodyReportOfReceiptUnsatisfactoryReason
			{
				UnsatisfactoryReasonCode = "UFRC01",
				ComplementaryInformation = "CI001",
			};
			dataProvider = new ED818UnsatisfactoryReasonProvider(reason);
		}
		ED818DBodyAcceptedOrRejectedReportOfReceiptBodyReportOfReceiptUnsatisfactoryReason reason;
		IED818UnsatisfactoryReason dataProvider;

		protected override ED818UnsatisfactoryReasonProvider GetProvider() => (ED818UnsatisfactoryReasonProvider)dataProvider;
	}
}
