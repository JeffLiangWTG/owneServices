using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusMAWBLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestUnpackDepotOrganisations()
		{
			AssertEquals(typeof(OrganisationsFindBoxCollection), MAWB.Lookups.UnpackDepotOrganisations.GetType());
		}

		public void TestCollection()
		{
			AssertNotNull("MAWB PortOfDischarge", MAWB.Lookups.PortOfDischargeList);
			AssertNotNull("MAWB PortOfLoading", MAWB.Lookups.PortOfLoadingList);
		}

		public void TestResponsiblePartiesCollection()
		{
			AssertEquals(typeof(OrgHeaderCollection), MAWB.Lookups.ResponsibleParties.GetType());
		}

		public void TestApplicationCodeList()
		{
			AssertEquals(2, MAWB.Lookups.ApplicationCodeList.Count);
			AssertEquals("Send Legacy Message", MAWB.Lookups.ApplicationCodeList.GetDescriptionFromCode(Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages));
			AssertEquals("Send CMR Message", MAWB.Lookups.ApplicationCodeList.GetDescriptionFromCode(Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages));
		}

		CusMAWB mawb;
		CusMAWB MAWB => mawb ?? (mawb = Factory.New<CusMAWB>());
	}
}
