namespace Enterprise.Client.EDI.TrustedMessaging.Business.Testing
{
	using System;
	using System.Linq;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
	using Enterprise.Client.EDI.TrustedMessaging.Business;
	using Enterprise.Registry.Business;
	using Enterprise.ZArchitecture.Business.Testing;
	using Enterprise.ZArchitecture.Schema;
	using NUnit.Framework;
	using WTG.TrustedMessaging.MyAccount.Models;

	[TestedType(typeof(EdiTrustedSystem))]
	public class EdiTrustedSystemTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGetOrCreateCertificateConfig()
		{
			var sys = Factory.New<EdiTrustedSystem>();
			sys.ETS_Product = "CW1";
			AssertEquals(true, sys.ETS_ETM_Certificate.IsEmpty);
			var config = sys.GetOrCreateCertificateConfig();
			AssertEquals("CW1", config.ETM_Product);
			AssertEquals(sys.ETS_ETM_Certificate, config.PK);
			AssertEquals(sys.CertificateConfig.PK, config.PK);
		}

		public void TestLoadEdiTrustedSystem()
		{
			var db1 = Factory.NewWithValidTestData<LicenceDatabase>();
			db1.LD_Product = "CW1";
			var sys1 = db1.GetOrCreateTrustedSystem();

			var db2 = Factory.NewWithValidTestData<LicenceDatabase>();
			db2.LD_Product = "CSP";
			db2.LD_TenantID = "CSP#1";
			var sys2 = db2.GetOrCreateTrustedSystem();
			sys2.ETS_SystemID = "CSP#1_#2";

			var db3 = Factory.NewWithValidTestData<LicenceDatabase>();
			db3.LD_Product = "ABU";
			db3.LD_TenantID = "ABU#2";
			var sys3 = db3.GetOrCreateTrustedSystem();

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var sys = EdiTrustedSystem.Load(newFactory, "CW1", db1.LD_DatabaseNumber.ToString());
			AssertEquals(sys1.PK, sys.PK);
			AssertEquals(db1.PK, sys.FindTenantDatabaseByTrustedInfo(new TrustedUserInfo() { }).PK);

			sys = EdiTrustedSystem.Load(newFactory, "CSP", "CSP#1_#2");
			AssertEquals(sys2.PK, sys.PK);
			sys = EdiTrustedSystem.Load(newFactory, "CSP", "CSP#1");
			AssertEquals(sys2.PK, sys.PK);
			AssertEquals(db2.PK, sys.FindTenantDatabaseByTrustedInfo(new TrustedUserInfo() { SystemId = "CSP#1" }).PK);
			AssertEquals(db2.PK, sys.FindTenantDatabaseByTrustedInfo(new TrustedUserInfo() { SystemId = "CSP#1_#2", TenantId = "CSP#1" }).PK);

			sys = EdiTrustedSystem.Load(newFactory, "ABU", "ABU#2");
			AssertEquals(sys3.PK, sys.PK);
			AssertEquals(db3.PK, sys.FindTenantDatabaseByTrustedInfo(new TrustedUserInfo() { }).PK);
		}

		public void TestLoadTrustedServiceAsSystem()
		{
			EDIDataRegistry.Instance.MyAccountTrustedServices.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CodeDescriptionBoolCollection() { "CCC", "DDD" });

			var service1 = Factory.New<EdiTrustedSystem>();
			service1.ETS_Product = "DDD";
			service1.ETS_SystemID = "DDD_01";
			service1.GetOrCreateCertificateConfig();

			var service2 = Factory.New<EdiTrustedSystem>();
			service2.ETS_Product = "DDD";
			service2.ETS_SystemID = "DDD_02";
			service2.GetOrCreateCertificateConfig();

			Factory.Save();

			var newFactory = new BusinessObjectFactory();

			var sys1 = EdiTrustedSystem.Load(newFactory, "CCC", "");
			Assert("Trusted system is created on the fly and saved", sys1.IsInDatabase);
			AssertNull(sys1.FindTenantDatabaseByTrustedInfo(new TrustedUserInfo() { }));
			AssertNotNull(sys1.CertificateConfig);

			var sys2 = EdiTrustedSystem.Load(newFactory, "DDD", "DDD_01");
			AssertEquals("Existing system should be loaded", sys2.PK, service1.PK);
			AssertNull(sys2.FindTenantDatabaseByTrustedInfo(new TrustedUserInfo() { }));
			AssertNotNull(sys2.CertificateConfig);

			var sys3 = EdiTrustedSystem.Load(newFactory, "DDD", "DDD_03");
			AssertNull("Return null if no matched", sys3);

			var sys4 = EdiTrustedSystem.Load(newFactory, "DDD", "");
			AssertNull("There are more than one systems with product code DDD, system_id is required", sys4);
		}

		public void TestProperties()
		{
			var sys = Factory.New<EdiTrustedSystem>();
			sys.ETS_Product = "CW1";
			AssertEquals(false, sys.HasSecretKey);
			AssertEquals(false, sys.HasTSCCertificate);

			sys.ETS_SecretKey = new ZBlob(new byte[] { 1, 2, 3 });
			sys.ETS_ETM_Certificate = Factory.New<EdiTrustedMessagingConfig>().PK;
			AssertEquals(true, sys.HasSecretKey);
			AssertEquals(true, sys.HasTSCCertificate);
		}

		public void TestSecretKey()
		{
			var sys = Factory.New<EdiTrustedSystem>();
			sys.ETS_Product = "CW1";
			sys.ETS_SystemID = "1234";
			sys.ETS_SecretKey = new ZBlob(new byte[] { 0x50, 0x5A, 0x0E, 0x50, 0x73, 0xE1, 0xA0, 0x57, 0x25, 0x56, 0xC1, 0x9E, 0x85, 0xE2, 0x6C, 0xD8, 0xB6, 0xE2, 0x2C, 0x65 });
			Factory.Save();

			var loadedSys = new BusinessObjectFactory().Load<EdiTrustedSystem>(sys.PK);
			AssertNotNull(loadedSys.ETS_SecretKey);
		}

		public void TestHumanReadableShortcutName()
		{
			var sys = Factory.New<EdiTrustedSystem>();
			sys.ETS_Description = "CargoWise One 16928";
			AssertEquals("Trusted System - CargoWise One 16928", sys.HumanReadableShortcutName);
		}

		public void TestETS_SystemNumber()
		{
			var sys = Factory.New<EdiTrustedSystem>();
			sys.ETS_Product = "CW1";
			sys.ETS_SystemID = "SID99";
			sys.ETS_Description = "CargoWise One 16928";
			AssertEquals("", sys.ETS_SystemNumber);
			Factory.Save();
			AssertEquals("ETS000001", sys.ETS_SystemNumber);

			var sys2 = Factory.New<EdiTrustedSystem>();
			sys2.ETS_Product = "CW1";
			sys2.ETS_SystemID = "SID88";
			sys2.ETS_Description = "CargoWise One 1420";
			AssertEquals("", sys2.ETS_SystemNumber);
			Factory.Save();
			AssertEquals("ETS000002", sys2.ETS_SystemNumber);
		}

		public void TestLoadLinkedLicenceDatabases()
		{
			var db1 = Factory.NewWithValidTestData<LicenceDatabase>();
			db1.LD_Product = "CW1";
			var sys1 = db1.GetOrCreateTrustedSystem();

			var db2 = Factory.NewWithValidTestData<LicenceDatabase>();
			db2.LD_Product = "CSP";
			var sys2 = db2.GetOrCreateTrustedSystem();

			var db3 = Factory.NewWithValidTestData<LicenceDatabase>();
			db3.LD_Product = "ABU";
			db3.LD_ETS_TrustedSystem = sys2.PK;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();

			var query1 = new ZQuery(EdiTrustedSystemSchema.PK, sys1.PK);
			var ts1 = newFactory.LoadTop1<EdiTrustedSystem>(query1);
			AssertEquals(1, ts1.AllLinkedLicenceDatabases.Count);
			AssertEquals("CW1", ts1.AllLinkedLicenceDatabases[0].LD_Product);

			var query2 = new ZQuery(EdiTrustedSystemSchema.PK, sys2.PK);
			var ts2 = newFactory.LoadTop1<EdiTrustedSystem>(query2);
			AssertEquals(2, ts2.AllLinkedLicenceDatabases.Count);
			AssertEquals(1, ts2.AllLinkedLicenceDatabases.Count(l => l.LD_Product == "CSP"));
			AssertEquals(1, ts2.AllLinkedLicenceDatabases.Count(l => l.LD_Product == "ABU"));
		}
	}
}
