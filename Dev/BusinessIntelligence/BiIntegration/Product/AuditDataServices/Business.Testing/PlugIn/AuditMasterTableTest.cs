using CargoWise.EntityFramework.Testing;

namespace Enterprise.AuditDataServices.Business.Testing
{
	using System;
	using System.Linq;
	using CargoWise.EntityFramework;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Business;
	using Enterprise.ZArchitecture.Schema;
	using NUnit.Framework;

	class AuditMasterTableTest : TestCase
	{
		public void TestConstructorValidation()
		{
			AssertExceptionThrown(
				"Constructor Throws Exception if businessEntity is null",
				typeof(ArgumentNullException),
#if NETFRAMEWORK
				"Value cannot be null.\r\nParameter name: businessEntity",
#else
				"Value cannot be null. (Parameter 'businessEntity')",
#endif
				() => new AuditMasterTable(businessEntity: null));
		}

		public void TestAllProperties()
		{
			var factory = new BusinessObjectFactory();
			var bizObj = factory.New<GlbStaff>();
			var testMasterTable = new AuditMasterTable(bizObj);

			AssertEquals("Factory = bizObj.Factory", true, Object.ReferenceEquals(testMasterTable.Factory, bizObj.Factory));
			AssertEquals("PkValue", bizObj.PK, testMasterTable.PkValue);
			AssertEquals("TableSchema", bizObj.PKSchemaColumn.TableSchema, testMasterTable.TableSchema);
			AssertEquals("HasLastEditField", true, testMasterTable.HasLastEditField);
			AssertEquals("AuditEntities has elements", true, testMasterTable.AuditEntities.Any());
			AssertEquals("AuditEntities.First", GlbStaffSchema.PK.Name, testMasterTable.AuditEntities.First().KeyColumn.Name);
			AssertEquals("AuditEntities has other related audit objects", true, testMasterTable.AuditEntities.Skip(1).Any());
		}

		public void TestHasLastEditField()
		{
			var factory = new BusinessObjectFactory();
			AssertEquals("HasLastEditField (GlbStaff)", true, new AuditMasterTable(factory.New<GlbStaff>()).HasLastEditField);
			AssertEquals("HasLastEditField (StmALog)", false, new AuditMasterTable(factory.New<StmALog>()).HasLastEditField);
		}

		public void TestAuditEntities()
		{
			var factory = new BusinessObjectFactory();

			var testAddressCapabilityMasterTable = new AuditMasterTable(factory.New<OrgAddressCapability>());
			AssertEquals("OrgAddressCapability first audit entity", OrgAddressCapabilitySchema.PK.Name, testAddressCapabilityMasterTable.AuditEntities.First().KeyColumn.Name);
			AssertEquals("OrgAddressCapability has other related audit objetcs", false, testAddressCapabilityMasterTable.AuditEntities.Skip(1).Any());

			var testGlbStaffMasterTable = new AuditMasterTable(factory.New<GlbStaff>());
			AssertEquals("GlbStaff first audit entity", GlbStaffSchema.PK.Name, testGlbStaffMasterTable.AuditEntities.First().KeyColumn.Name);
			AssertEquals("GlbStaff has other related audit objects", true, testGlbStaffMasterTable.AuditEntities.Skip(1).Any());
			AssertEquals("GlbStaffHoliday is one of GlbStaff's related audit objects", true, testGlbStaffMasterTable.AuditEntities.Any((e) => e.KeyColumn.Name == GlbStaffHolidaySchema.Constants.GA_GS));
		}

		public void TestClusterKeyValue()
		{
			var factory = new BusinessObjectFactory();
			var businessEntity = factory.New<DummyWithDependentsAndClusterKeyBusinessObject>();
			businessEntity.Z0_Number = 2;
			var testMasterTable = new AuditMasterTable(businessEntity);
			AssertEquals("ClusterKeyValue", 2, testMasterTable.ClusterKeyValue);

			testMasterTable = new AuditMasterTable(factory.New<GlbStaff>());
			AssertEquals("ClusterKeyValue", 0, testMasterTable.ClusterKeyValue);
		}

		public void TestClusterKeySchemaColumn()
		{
			var factory = new BusinessObjectFactory();
			var businessEntity = factory.New<DummyWithDependentsAndClusterKeyBusinessObject>();
			var testMasterTable = new AuditMasterTable(businessEntity);
			AssertEquals("ClusterKeySchemaColumn", DummyBizoSchema.Z0_Number, testMasterTable.ClusterKeySchemaColumn);

			testMasterTable = new AuditMasterTable(factory.New<GlbStaff>());
			AssertEquals("ClusterKeySchemaColumn", null, testMasterTable.ClusterKeySchemaColumn);
		}
	}
}
