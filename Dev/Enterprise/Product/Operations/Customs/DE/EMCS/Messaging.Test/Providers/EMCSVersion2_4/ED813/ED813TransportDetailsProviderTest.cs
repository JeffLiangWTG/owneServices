using System;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_4.Testing
{
	public class ED813TransportDetailsProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new ED813TransportDetailsProvider(null));
		}

		public void TestUnitCode()
		{
			transportDetails.TransportUnitCode = "4";
			AssertEquals("4", eD813TransportDetailsProvider.UnitCode);
		}

		public void TestIdentityOfUnit()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not specified, No exception", ZString.Empty, eD813TransportDetailsProvider.IdentityOfUnit);

				transportDetails.IdentityOfTransportUnits = "PONU2864065";
				AssertEquals("PONU2864065", eD813TransportDetailsProvider.IdentityOfUnit);
			});
		}

		public void TestCommercialSealIdentification()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not specified, No exception", ZString.Empty, eD813TransportDetailsProvider.CommercialSealIdentification);

				transportDetails.CommercialSealIdentification = "4419151";
				AssertEquals("4419151", eD813TransportDetailsProvider.CommercialSealIdentification);
			});
		}

		public void TestComplementaryInformation()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not specified, No exception", ZString.Empty, eD813TransportDetailsProvider.ComplementaryInformation);

				transportDetails.ComplementaryInformation = "COMMENT ABOUT THE CONTAINER";
				AssertEquals("COMMENT ABOUT THE CONTAINER", eD813TransportDetailsProvider.ComplementaryInformation);
			});
		}

		public void TestSealInformation()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not specified, No exception", ZString.Empty, eD813TransportDetailsProvider.SealInformation);

				transportDetails.SealInformation = "SEAL INFORMATION";
				AssertEquals("SEAL INFORMATION", eD813TransportDetailsProvider.SealInformation);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			transportDetails = new ED813EBodyChangeOfDestinationTransportDetails();
			eD813TransportDetailsProvider = new ED813TransportDetailsProvider(transportDetails);
		}
		ED813EBodyChangeOfDestinationTransportDetails transportDetails;
		IEMCSTransportDetails eD813TransportDetailsProvider;
	}
}
