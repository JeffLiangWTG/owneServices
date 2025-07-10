namespace Enterprise.AuditDataServices.Business.Testing
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Schema;
	using NUnit.Framework;

	class AuditEntityTest : TestCase
	{
		public void TestConstructorValidation()
		{
			AssertExceptionThrown(
				"Constructor Throws Exception if keyColumn is null",
				typeof(ArgumentNullException),
#if NETFRAMEWORK
				"Value cannot be null.\r\nParameter name: keyColumn",
#else
				"Value cannot be null. (Parameter 'keyColumn')",
#endif
				() => new AuditEntity(keyColumn: null, infoColumn: null));

			AssertExceptionThrown(
				"Constructor Throws Exception if keyColumn is null",
				typeof(ArgumentException),
				"keyColumn must be a Guid or Int column.\r\nParameter name: keyColumn",
				() => new AuditEntity(keyColumn: DummyBizoSchema.Z0_Bool, infoColumn: null));

			AssertExceptionThrown(
				"Constructor Throws Exception if keyColumn is null",
				typeof(ArgumentException),
				"Int keyColumns must be cluster key columns.\r\nParameter name: keyColumn",
				() => new AuditEntity(keyColumn: DummyBizoSchema.Z0_Number, infoColumn: null));
		}

		public void TestWithPkColumn()
		{
			var entity = new AuditEntity(GlbStaffSchema.PK, null);
			AssertEquals("KeyColumn", GlbStaffSchema.PK.Name, entity.KeyColumn.Name);
			AssertEquals("ToString()", DataBoundResourceStrings.GetTableDescriptiveName(GlbStaffSchema.Constants.TableName), entity.ToString());
		}

		public void TestWhenChildOnlyHasOneFkToParent()
		{
			var entity = new AuditEntity(GlbStaffHolidaySchema.GA_GS, null);
			AssertEquals("KeyColumn", GlbStaffHolidaySchema.GA_GS.Name, entity.KeyColumn.Name);
			AssertEquals("ToString()", DataBoundResourceStrings.GetTableDescriptiveName(GlbStaffHolidaySchema.Constants.TableName), entity.ToString());
		}

		public void TestWhenChildHasMultipleFksToParent()
		{
			var buyerOrgEntity = new AuditEntity(OrgSupplierBuyerLinkSchema.OL_OH_Buyer, null);
			AssertEquals("KeyColumn", OrgSupplierBuyerLinkSchema.OL_OH_Buyer.Name, buyerOrgEntity.KeyColumn.Name);
			AssertEquals("ToString()",
				DataBoundResourceStrings.GetTableDescriptiveName(OrgSupplierBuyerLinkSchema.Constants.TableName) + " | " + ZPropertyInfo.GetFriendlyColumnNameShared(OrgSupplierBuyerLinkSchema.OL_OH_Buyer.Name),
				buyerOrgEntity.ToString());

			var supplierOrgEntity = new AuditEntity(OrgSupplierBuyerLinkSchema.OL_OH_Supplier, null);
			AssertEquals("KeyColumn", OrgSupplierBuyerLinkSchema.OL_OH_Supplier.Name, supplierOrgEntity.KeyColumn.Name);
			AssertEquals("ToString()",
				DataBoundResourceStrings.GetTableDescriptiveName(OrgSupplierBuyerLinkSchema.Constants.TableName) + " | " + ZPropertyInfo.GetFriendlyColumnNameShared(OrgSupplierBuyerLinkSchema.OL_OH_Supplier.Name),
				supplierOrgEntity.ToString());
		}

		public void TestToString()
		{
			AssertEquals(DataBoundResourceStrings.GetTableDescriptiveName(OrgSupplierBuyerLinkSchema.Constants.TableName) + " | " + ZPropertyInfo.GetFriendlyColumnNameShared(OrgSupplierBuyerLinkSchema.OL_OH_Buyer.Name),
				new AuditEntity(OrgSupplierBuyerLinkSchema.OL_OH_Buyer, null).ToString());
			AssertEquals(DataBoundResourceStrings.GetTableDescriptiveName(OrgSupplierBuyerLinkSchema.Constants.TableName) + " | " + ZPropertyInfo.GetFriendlyColumnNameShared(OrgSupplierBuyerLinkSchema.OL_OH_Supplier.Name),
				new AuditEntity(OrgSupplierBuyerLinkSchema.OL_OH_Supplier, null).ToString());
			AssertEquals(DataBoundResourceStrings.GetTableDescriptiveName(JobDeclarationSchema.Constants.TableName),
				new AuditEntity(JobDeclarationSchema.JE_ClusterKey, null).ToString());
			AssertEquals(DataBoundResourceStrings.GetTableDescriptiveName(JobDeclarationSchema.Constants.TableName),
				new AuditEntity(JobDeclarationSchema.JE_JS, null).ToString());
		}
	}
}
