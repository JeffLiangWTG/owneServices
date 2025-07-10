using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;

namespace Enterprise.Client.EDI.FeatureControl.Business.Testing
{
	public class FeatureControlRuleLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLicenceDatabaseNotLinked()
		{
			var db1 = Factory.NewWithValidTestData<LicenceDatabase>();
			db1.LD_ServerCode = "DB1";
			db1.LD_Product = "CW1";
			var db2 = Factory.NewWithValidTestData<LicenceDatabase>();
			db2.LD_ServerCode = "DB2";
			db2.LD_Product = "ENT";
			var db3 = Factory.NewWithValidTestData<LicenceDatabase>();
			db3.LD_ServerCode = "DB3";
			db3.LD_Product = "WTA";
			var db4 = Factory.NewWithValidTestData<LicenceDatabase>();
			db4.LD_ServerCode = "DB4";
			db4.LD_Product = "CWN";
			var db5 = Factory.NewWithValidTestData<LicenceDatabase>();
			db5.LD_ServerCode = "DB5";
			db5.LD_Product = "CGW";

			var header = Factory.NewWithValidTestData<FeatureControlHeader>();
			var rule1 = header.FeatureControlRules.AddNew();
			rule1.FCR_Description = "rule 1";
			rule1.FCR_StartDateUtc = new CargoWise.Types.ZDateTime(2000, 1, 1);
			var pivot1 = rule1.LicenceDatabasePivots.AddNew();
			pivot1.FCD_LD_LicenceDatabase = db1.PK;
			var pivot2 = rule1.LicenceDatabasePivots.AddNew();
			pivot2.FCD_LD_LicenceDatabase = db4.PK;
			Factory.Save();

			var lookup = new FeatureControlRuleLookups(rule1);
			AssertEquals(false, lookup.LicenceDatabaseNotLinked.Contains(db1));
			AssertEquals(true, lookup.LicenceDatabaseNotLinked.Contains(db2));
			AssertEquals(false, lookup.LicenceDatabaseNotLinked.Contains(db3));
			AssertEquals(false, lookup.LicenceDatabaseNotLinked.Contains(db4));
			AssertEquals(true, lookup.LicenceDatabaseNotLinked.Contains(db5));
		}
	}
}
