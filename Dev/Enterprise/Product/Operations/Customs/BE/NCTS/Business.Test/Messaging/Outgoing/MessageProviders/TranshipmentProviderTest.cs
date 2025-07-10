using System;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(TranshipmentProvider))]
	sealed class TranshipmentProviderTest : Customs.Business.Testing.DataProviderTestCase<TranshipmentProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new TranshipmentProvider(null));
		}

		public void TestContainerIndicator()
		{
			incident.IncidentContainers.AddNew();
			Assert(Provider.ContainerIndicator);
		}

		public void TestTypeOfIdentification()
		{
			incident.BN_TransportAtDepartureType = "11";
			AssertEquals(11, Provider.TypeOfIdentification);
		}

		public void TestIdentificationNumber()
		{
			incident.BN_TransportAtDepartureID = "DepID";
			AssertEquals("DepID", Provider.IdentificationNumber);
		}

		public void TestNationality()
		{
			incident.BN_RN_NKTransportAtDepartureIDNationality = Core.Constants.CountryCodes.Belgium;
			AssertEquals("BE", Provider.Nationality);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			incident = header.EnRouteIncidents.AddNew();

			provider = new TranshipmentProvider(incident);
		}

		EnRouteIncident incident;
		TranshipmentProvider provider;

		protected override TranshipmentProvider GetProvider() => provider;
	}
}
