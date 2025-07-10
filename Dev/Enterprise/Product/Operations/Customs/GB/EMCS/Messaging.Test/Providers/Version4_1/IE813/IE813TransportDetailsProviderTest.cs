using System;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie813;
using CargoWise.Types;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1.Testing
{
	sealed class IE813TransportDetailsProviderTest : Business.Testing.DataProviderTestCase<IE813TransportDetailsProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new IE813TransportDetailsProvider(null));
		}

		public void TestUnitCode()
		{
			message.TransportUnitCode = "4";
			AssertEquals("4", Provider.UnitCode);
		}

		public void TestIdentityOfUnit()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not specified, No exception", ZString.Empty, Provider.IdentityOfUnit);

				message.IdentityOfTransportUnits = "PONU2864065";
				AssertEquals("PONU2864065", Provider.IdentityOfUnit);
			});
		}

		public void TestCommercialSealIdentification()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not specified, No exception", ZString.Empty, Provider.CommercialSealIdentification);

				message.CommercialSealIdentification = "4419151";
				AssertEquals("4419151", Provider.CommercialSealIdentification);
			});
		}

		public void TestComplementaryInformation()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not specified, No exception", ZString.Empty, Provider.ComplementaryInformation);

				message.ComplementaryInformation = new LsdComplementaryInformationType { Value = "COMMENT ABOUT THE CONTAINER" };
				AssertEquals("COMMENT ABOUT THE CONTAINER", Provider.ComplementaryInformation);
			});
		}

		public void TestSealInformation()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not specified, No exception", ZString.Empty, Provider.SealInformation);

				message.SealInformation = new LsdSealInformationType { Value = "SEAL INFORMATION" };
				AssertEquals("SEAL INFORMATION", Provider.SealInformation);
			});
		}

		protected override IE813TransportDetailsProvider GetProvider() => new IE813TransportDetailsProvider(message);

		protected override void SetUp()
		{
			base.SetUp();
			message = new TransportDetailsType();
		}
		TransportDetailsType message;
	}
}
