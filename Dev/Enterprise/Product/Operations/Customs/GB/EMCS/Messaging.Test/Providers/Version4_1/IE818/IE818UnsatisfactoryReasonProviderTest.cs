using System;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie818;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1.Testing
{
	sealed class IE818UnsatisfactoryReasonProviderTest : Business.Testing.DataProviderTestCase<IE818UnsatisfactoryReasonProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new IE818UnsatisfactoryReasonProvider(null));
		}

		public void TestReasonCode()
		{
			AssertEquals("UFRC01", Provider.ReasonCode);
		}

		public void TestComplementaryInformation()
		{
			AssertEquals("CI001", Provider.ComplementaryInformation);
		}

		protected override IE818UnsatisfactoryReasonProvider GetProvider()
		{
			reason = new UnsatisfactoryReasonType
			{
				UnsatisfactoryReasonCode = "UFRC01",
				ComplementaryInformation = new LsdComplementaryInformationType
				{
					Language = "en",
					Value = "CI001",
				},
			};

			return new IE818UnsatisfactoryReasonProvider(reason);
		}
		UnsatisfactoryReasonType reason;
	}
}
