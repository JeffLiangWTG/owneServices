using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;

namespace Enterprise.Client.EDI.FeatureControl.Business.Testing
{
	public class FeatureControlSetLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestActiveCargoWiseDatabases()
		{
			var db1 = Factory.NewWithValidTestData<LicenceDatabase>();
			db1.LD_ServerCode = "DB1";
			db1.LD_Product = "CW1";
			var db2 = Factory.NewWithValidTestData<LicenceDatabase>();
			db2.LD_ServerCode = "DB2";
			db2.LD_Product = "ENT";
			db2.LD_IsActive = false;
			var db3 = Factory.NewWithValidTestData<LicenceDatabase>();
			db3.LD_ServerCode = "DB3";
			db3.LD_Product = "WTA";
			var db4 = Factory.NewWithValidTestData<LicenceDatabase>();
			db4.LD_ServerCode = "DB4";
			db4.LD_Product = "CWN";

			Factory.Save();

			var featureSet = Factory.NewWithValidTestData<FeatureControlSet>();
			var lookup = new FeatureControlSetLookups(featureSet);
			var databases = lookup.ActiveCargoWiseDatabases;
			AssertEquals(true, databases.Contains(db1));
			AssertEquals(false, databases.Contains(db2));
			AssertEquals(false, databases.Contains(db3));
			AssertEquals(true, databases.Contains(db4));
		}
	}
}
