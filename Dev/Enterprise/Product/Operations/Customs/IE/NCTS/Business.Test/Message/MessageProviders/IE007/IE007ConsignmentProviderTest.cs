using System;
using System.Linq;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	sealed class IE007ConsignmentProviderTest : Customs.Business.Testing.DataProviderTestCase<IE007ConsignmentProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>("NctsHeader missing", () => new IE007ConsignmentProvider(null));
		}

		public void TestLocationOfGoods()
		{
			AssertType<LocationOfGoodsProvider>(Provider.LocationOfGoods);
		}

		public void TestIncidents()
		{
			var incident1 = nctsHeader.EnRouteIncidents.AddNew();
			incident1.BN_IncidentCode = "1";
			incident1.BN_Information = "Incident 1 Information";
			var incident2 = nctsHeader.EnRouteIncidents.AddNew();
			incident2.BN_IncidentCode = "2";
			incident2.BN_Information = "Incident 2 Information";
			AssertEquals("Incidents count", 2, Provider.Incidents.Count);
			AssertEquals("First incident", "1", Provider.Incidents.First().Code);
			AssertEquals("Second incident", "Incident 2 Information", Provider.Incidents.Last().Text);
		}

		protected override IE007ConsignmentProvider GetProvider() => new IE007ConsignmentProvider(nctsHeader);

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		}
		NctsHeader nctsHeader;
	}
}
