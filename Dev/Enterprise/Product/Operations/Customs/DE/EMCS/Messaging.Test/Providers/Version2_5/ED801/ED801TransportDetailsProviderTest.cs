using System;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_5.Testing
{
	public class ED801TransportDetailsProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new ED801TransportDetailsProvider(null));
		}

		public void TestUnitCode()
		{
			transportDetails.TransportUnitCode = "4";
			AssertEquals("4", eD801TransportDetailsProvider.UnitCode);
		}

		public void TestIdentityOfUnit()
		{
			transportDetails.IdentityOfTransportUnits = "PONU2864065";
			AssertEquals("PONU2864065", eD801TransportDetailsProvider.IdentityOfUnit);
		}

		public void TestCommercialSealIdentification()
		{
			transportDetails.CommercialSealIdentification = "4419151";
			AssertEquals("4419151", eD801TransportDetailsProvider.CommercialSealIdentification);
		}

		public void TestComplementaryInformation()
		{
			transportDetails.ComplementaryInformation = "COMMENT ABOUT THE CONTAINER";
			AssertEquals("COMMENT ABOUT THE CONTAINER", eD801TransportDetailsProvider.ComplementaryInformation);
		}

		public void TestSealInformation()
		{
			transportDetails.SealInformation = "SEAL INFORMATION";
			AssertEquals("SEAL INFORMATION", eD801TransportDetailsProvider.SealInformation);
		}

		protected override void SetUp()
		{
			base.SetUp();
			transportDetails = new ED801EBodyEadContainerTransportDetails();
			eD801TransportDetailsProvider = new ED801TransportDetailsProvider(transportDetails);
		}
		ED801EBodyEadContainerTransportDetails transportDetails;
		IEMCSTransportDetails eD801TransportDetailsProvider;
	}
}
