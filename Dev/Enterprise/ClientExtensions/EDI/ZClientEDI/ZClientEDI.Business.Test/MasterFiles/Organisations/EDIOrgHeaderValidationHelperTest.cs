using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	class EDIOrgHeaderValidationHelperTest : TestCaseWithFactory
	{
		public void TestValidateENTCodeForOrganisationNode()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			var org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			var org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			var org3 = Factory.NewWithValidTestData<EDIOrgHeader>();
			var org4 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.LicenceEnterpriseCode = "LE0";
			org1.LicenceEnterpriseCode = "LE0";
			org2.LicenceEnterpriseCode = "LE0";
			org3.LicenceEnterpriseCode = "LE0";
			org4.LicenceEnterpriseCode = "LE0";
			Factory.Save();

			var rootNode = org.OrgManagementGroupingModel.RootNodes.Single();
			var warningMsg = EDIOrgHeaderValidationHelper.ValidateENTCodeForOrganisationNode(rootNode, new OrgHeader[] { org1, org2, org3, org4 }, Factory);
			AssertEquals("The warning msg should be empty.", string.Empty, warningMsg);

			org1.LicenceEnterpriseCode = "LE1";
			org2.LicenceEnterpriseCode = "LE2";
			org3.LicenceEnterpriseCode = "LE3";
			org4.LicenceEnterpriseCode = "LE4";
			Factory.Save();

			warningMsg = EDIOrgHeaderValidationHelper.ValidateENTCodeForOrganisationNode(rootNode, new OrgHeader[] { org1, org2, org3, org4 }, Factory);
			AssertEquals("Should show warning msg.", @"One or more organisations in your selection has a different enterprise license code than the parent organisation(s). 
As this is a rare scenario, please only proceed after verifying that it is correct. Following are the mismatches:

Parent Organisation:
 (XVBQP68SIYXQ)  Ent.Code: LE0
Child Orgnisations:
 (H5ZX52PAMCOI)  Ent.Code: LE1
 (ZGP5LX5SQPEB)  Ent.Code: LE2
 (IRED1TLAV242)  Ent.Code: LE3
 (013LIP1SZFVU)  Ent.Code: LE4
", warningMsg);
		}
	}
}
