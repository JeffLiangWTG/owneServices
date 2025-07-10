using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public class ProjectLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLicenceHeaderList()
		{
			EDIOrgHeader orgHeader = Factory.New<EDIOrgHeader>();
			orgHeader.CreateAndLoadLicenceForOrg();
			LicenceHeader licence = orgHeader.LicCompany.GetHeader(orgHeader.LicCompany.LicDatabases.AddNew());

			var project = Factory.New<EDIProject>();
			LicenceHeaderCollection licenceHeaderList = project.Lookups.LicenceHeaderList;
			licenceHeaderList.Load();
			AssertEquals("LicenceHeaderList should contain the generic LicenceHeader.", true, licenceHeaderList.Contains(licence));
			AssertEquals("LicenceHeaderList.FilterBusinessObjectDefaults.ContainsDefaultFor[\"Organisation\"]", false, licenceHeaderList.FilterBusinessObjectDefaults.ContainsDefaultFor("Organisation" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));

			EDIOrgHeader clientOrgHeader = Factory.New<EDIOrgHeader>();
			clientOrgHeader.CreateAndLoadLicenceForOrg();
			LicenceHeader clientLicence = clientOrgHeader.LicCompany.GetHeader(clientOrgHeader.LicCompany.LicDatabases.AddNew());

			project.WKP_OA_ClientAddress = clientOrgHeader.Addresses.MainAddress.PK;
			LicenceHeaderCollection clientLicenceHeaderList = project.Lookups.LicenceHeaderList;
			clientLicenceHeaderList.Load();
			Assert("LicenceHeaderList should not be the generic list.", clientLicenceHeaderList != licenceHeaderList);
			AssertEquals("LicenceHeaderList should contain the client's LicenceHeader.", true, clientLicenceHeaderList.Contains(clientLicence));
			AssertEquals("LicenceHeaderList should not contain the generic LicenceHeader.", false, clientLicenceHeaderList.Contains(licence));
			AssertEquals("LicenceHeaderList.FilterBusinessObjectDefaults[\"Organisation\"].Value", clientOrgHeader.PK, clientLicenceHeaderList.FilterBusinessObjectDefaults["Organisation" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value);

			project.WKP_OA_ClientAddress = ZGuid.Empty;
			AssertEquals("LicenceHeaderList should revert back to the generic list.", licenceHeaderList, project.Lookups.LicenceHeaderList);
		}
	}
}