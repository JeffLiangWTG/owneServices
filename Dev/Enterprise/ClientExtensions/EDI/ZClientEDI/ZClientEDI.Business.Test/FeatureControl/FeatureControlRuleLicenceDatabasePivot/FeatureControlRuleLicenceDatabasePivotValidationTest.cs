using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.FeatureControl.Business.Testing
{
	public class FeatureControlRuleLicenceDatabasePivotValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCanAttachLicenceDatabase()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "TEST_ORG1";
			var db1 = Factory.NewWithValidTestData<LicenceDatabase>();
			db1.LD_ServerCode = "DB1";
			db1.LD_DatabaseNumber = 100;
			db1.LD_OH_WebAccessOrg = org.PK;
			var db2 = Factory.NewWithValidTestData<LicenceDatabase>();
			db2.LD_ServerCode = "DB2";

			var header = Factory.NewWithValidTestData<FeatureControlHeader>();
			var rule1 = header.FeatureControlRules.AddNew();
			var pivot = rule1.LicenceDatabasePivots.AddNew();
			pivot.FCD_LD_LicenceDatabase = db1.PK;

			var rule2 = header.FeatureControlRules.AddNew();
			var pivot2_1 = rule2.LicenceDatabasePivots.AddNew();
			pivot2_1.FCD_LD_LicenceDatabase = db1.PK;
			var pivot2_2 = rule2.LicenceDatabasePivots.AddNew();
			pivot2_2.FCD_LD_LicenceDatabase = db2.PK;

			header.RunPreSaveValidation();
			AssertHasRowError(pivot2_1, "License Database 100 for client TEST_ORG1 is already attached to an active rule");
			AssertNoRowErrors(pivot2_2);
		}
	}
}
