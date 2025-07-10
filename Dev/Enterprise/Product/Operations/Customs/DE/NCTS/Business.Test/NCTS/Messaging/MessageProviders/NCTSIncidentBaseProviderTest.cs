using System;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class NCTSIncidentBaseProviderTest : Customs.Business.Testing.DataProviderTestCase<NCTSIncidentBaseProvider>
	{
		public void TestNew()
		{
			CombineAssertions(() =>
			{
				AssertNull("NULL", NCTSIncidentBaseProvider.NewOrNull(null));
				AssertNotNull("Not NULL", dataProvider);
			});
		}

		public void TestCode()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Default", null, dataProvider.Code);
				incident.BN_IncidentCode = "6";
				AssertEquals("Entered", "6", dataProvider.Code);
			});
		}

		public void TestInformation()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Default", null, dataProvider.Information);
				incident.BN_Information = "Information";
				AssertEquals("Entered", "Information", dataProvider.Information);
			});
		}

		public void TestEndorsement()
		{
			CombineAssertions(() =>
			{
				incident.BN_EndorsementDate = DateTime.Now;
				incident.BN_EndorsementPlace = "PLACE";
				incident.BN_EndorsementAuthority = "ABC";
				incident.BN_EndorsementCountryCode = "DE";
				var endorsement = dataProvider.Endorsement;
				AssertNotNull("Endorsement", endorsement);
				AssertSame("Cached", endorsement, dataProvider.Endorsement);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			incident = header.EnRouteIncidents.AddNew();

			dataProvider = NCTSIncidentBaseProvider.NewOrNull(incident);
		}
		EnRouteIncident incident;
		INCTSIncidentBase dataProvider;

		protected override NCTSIncidentBaseProvider GetProvider() => (NCTSIncidentBaseProvider)dataProvider;
	}
}
