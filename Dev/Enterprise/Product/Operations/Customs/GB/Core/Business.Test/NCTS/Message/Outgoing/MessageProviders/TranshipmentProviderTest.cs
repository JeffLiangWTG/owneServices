using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
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

			incident.BN_TransportAtDepartureType = "1A";
			AssertEquals(0, Provider.TypeOfIdentification);

			AssertExceptionThrown<MaxLengthExceededException>(() => incident.BN_TransportAtDepartureType = "111");
			ErrorReporter.Clear();

			AssertExceptionThrown<MaxLengthExceededException>(() => incident.BN_TransportAtDepartureType = "AAA");
			AssertContains("The maximum length of 'BN_TransportAtDepartureType' has been exceeded", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestIdentificationNumber()
		{
			incident.BN_TransportAtDepartureID = "DepID";
			AssertEquals("DepID", Provider.IdentificationNumber);
		}

		public void TestNationality()
		{
			incident.BN_RN_NKTransportAtDepartureIDNationality = Core.Constants.CountryCodes.UnitedKingdom;
			AssertEquals("GB", Provider.Nationality);
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
