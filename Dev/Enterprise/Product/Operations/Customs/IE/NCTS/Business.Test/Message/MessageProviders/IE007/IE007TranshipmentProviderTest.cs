using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	public class IE007TranshipmentProviderTest : Customs.Business.Testing.DataProviderTestCase<IE007TranshipmentProvider>
	{
		public void TestHasContainer()
		{
			Assert("HasContainer", Provider.HasContainer);
		}

		public void TestTransportMeans()
		{
			incident.BN_TransportAtDepartureType = "10";
			AssertEquals("TransportMeans", "10", Provider.TransportMeans.TypeOfIdentification);
		}

		protected override IE007TranshipmentProvider GetProvider() => new IE007TranshipmentProvider(incident, true);

		protected override void SetUp()
		{
			base.SetUp();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			incident = nctsHeader.EnRouteIncidents.AddNew();
		}
		EnRouteIncident incident;
	}
}
