using System;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie801;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1.Testing
{
	sealed class IE801TransportDetailsProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new IE801TransportDetailsProvider(null));
		}

		public void TestUnitCode()
		{
			transportDetails.TransportUnitCode = "4";
			AssertEquals("4", ie801TransportDetailsProvider.UnitCode);
		}

		public void TestIdentityOfUnit()
		{
			transportDetails.IdentityOfTransportUnits = "PONU2864065";
			AssertEquals("PONU2864065", ie801TransportDetailsProvider.IdentityOfUnit);
		}

		public void TestCommercialSealIdentification()
		{
			transportDetails.CommercialSealIdentification = "4419151";
			AssertEquals("4419151", ie801TransportDetailsProvider.CommercialSealIdentification);
		}

		public void TestComplementaryInformation()
		{
			transportDetails.ComplementaryInformation = new LsdComplementaryInformationType
			{
				Value = "COMMENT ABOUT THE CONTAINER",
				Language = "en",
			};
			AssertEquals("COMMENT ABOUT THE CONTAINER", ie801TransportDetailsProvider.ComplementaryInformation);
		}

		public void TestSealInformation()
		{
			transportDetails.SealInformation = new LsdSealInformationType
			{
				Value = "SEAL INFORMATION",
				Language = "en",
			};
			AssertEquals("SEAL INFORMATION", ie801TransportDetailsProvider.SealInformation);
		}

		protected override void SetUp()
		{
			base.SetUp();
			transportDetails = new TransportDetailsType();
			ie801TransportDetailsProvider = new IE801TransportDetailsProvider(transportDetails);
		}
		TransportDetailsType transportDetails;
		IEMCSTransportDetails ie801TransportDetailsProvider;
	}
}
