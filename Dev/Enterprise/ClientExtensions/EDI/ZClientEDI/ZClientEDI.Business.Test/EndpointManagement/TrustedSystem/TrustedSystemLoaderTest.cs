using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;

namespace Enterprise.Client.EDI.TrustedMessaging.Business.Testing
{
	class TrustedSystemLoaderTest : TestCaseWithFactory
	{
		public void TestLoad_LicenceDatabase()
		{
			var db1 = Factory.NewWithValidTestData<LicenceDatabase>();
			db1.LD_Product = "CW1";
			db1.LD_DatabaseNumber = 1001;
			db1.GetOrCreateTrustedSystem();

			var db2 = Factory.NewWithValidTestData<LicenceDatabase>();
			db2.LD_Product = "CSP";
			db2.LD_DatabaseNumber = 1002;
			db2.LD_TenantID = "CSP1002";
			db2.GetOrCreateTrustedSystem();

			Factory.Save();

			var system = EdiTrustedSystem.Load(Factory, "CW1", "1001");
			AssertEquals(db1.LD_ETS_TrustedSystem, system.PK);

			system = EdiTrustedSystem.Load(Factory, "CSP", "CSP1002");
			AssertEquals(db2.LD_ETS_TrustedSystem, system.PK);

			system = EdiTrustedSystem.Load(Factory, "CSP", "1002");
			AssertNull(system);
		}
	}
}
