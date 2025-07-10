using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestsSubclassesOf(typeof(ISupportSubAccount))]
	public abstract class SubAccountImplementationTest<T> : TestCaseWithFactory where T : BusinessObject, ISupportSubAccount
	{
		protected virtual T GetSubAccountSupportedBusinessObject()
		{
			return GetSubAccountSupportedBusinessObject(Factory);
		}

		protected T GetSubAccountSupportedBusinessObject(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData<T>();
		}

		public void TestChangingSubAccountTypeParentTableCode_Resets_SubAccountParentId()
		{
			var bizO = GetSubAccountSupportedBusinessObject();
			bizO.SubAccountTypeParentTableCode = OrgHeaderSchema.Constants.Prefix;
			bizO.SubAccountParentId = TestObjectCreator.ABIGAS.PK;
			AssertNotEquals("Pre-condition", Guid.Empty, bizO.SubAccountParentId);

			bizO.SubAccountTypeParentTableCode = GlbGroupSchema.Constants.Prefix;
			AssertEquals("Changing ParentTableCode should reset SubAccountParentId", Guid.Empty, bizO.SubAccountParentId);

			bizO.SubAccountParentId = ZGuid.NewZGuid();
			AssertNotEquals("Pre-condition", Guid.Empty, bizO.SubAccountParentId);

			bizO.SubAccountTypeParentTableCode = string.Empty;
			AssertEquals("Reset ParentTableCode should reset SubAccountParentId", Guid.Empty, bizO.SubAccountParentId);
		}

		public void TestSubAccountTypeDisplayCodeGetter()
		{
			var bizO = GetSubAccountSupportedBusinessObject();
			bizO.SubAccountTypeParentTableCode = OrgHeaderSchema.Constants.Prefix;
			AssertEquals(Core.Constants.SubAccountType.Organization, bizO.SubAccountTypeDisplayCode);

			bizO.SubAccountTypeParentTableCode = AccGroupsSchema.Constants.Prefix;
			AssertEquals(Core.Constants.SubAccountType.SalesGroup, bizO.SubAccountTypeDisplayCode);

			bizO.SubAccountTypeParentTableCode = GlbStaffSchema.Constants.Prefix;
			AssertEquals(Core.Constants.SubAccountType.StaffAndResources, bizO.SubAccountTypeDisplayCode);

			bizO.SubAccountTypeParentTableCode = GlbGroupSchema.Constants.Prefix;
			AssertEquals(Core.Constants.SubAccountType.StaffGroup, bizO.SubAccountTypeDisplayCode);
		}

		public void TestSubAccountTypeDisplayCodeSetter()
		{
			var bizO = GetSubAccountSupportedBusinessObject();
			bizO.SubAccountTypeDisplayCode = Core.Constants.SubAccountType.Organization;
			AssertEquals(OrgHeaderSchema.Constants.Prefix, bizO.SubAccountTypeParentTableCode);

			bizO.SubAccountTypeDisplayCode = Core.Constants.SubAccountType.SalesGroup;
			AssertEquals(AccGroupsSchema.Constants.Prefix, bizO.SubAccountTypeParentTableCode);

			bizO.SubAccountTypeDisplayCode = Core.Constants.SubAccountType.StaffAndResources;
			AssertEquals(GlbStaffSchema.Constants.Prefix, bizO.SubAccountTypeParentTableCode);

			bizO.SubAccountTypeDisplayCode = Core.Constants.SubAccountType.StaffGroup;
			AssertEquals(GlbGroupSchema.Constants.Prefix, bizO.SubAccountTypeParentTableCode);
		}

		public void TestSubClassParentIdInActive()
		{
			var bizO = GetSubAccountSupportedBusinessObject();
			//OrgHeader
			var orgHeader = TestObjectCreator.CreateOrgHeader("org123", false, false);
			orgHeader.OH_Code = "org123";
			orgHeader.OH_IsActive = false;

			bizO.SubAccountTypeParentTableCode = OrgHeaderSchema.Constants.Prefix;
			bizO.SubAccountParentId = orgHeader.PK;

			AssertHasError(bizO.SubAccountParentIdInfo, "This Sub Account is inactive - it may not be used.");

			//GlbStaff
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TS";
			staff.GS_IsActive = false;

			bizO.SubAccountTypeParentTableCode = GlbStaffSchema.Constants.Prefix;
			bizO.SubAccountParentId = staff.PK;

			AssertHasError(bizO.SubAccountParentIdInfo, "This Sub Account is inactive - it may not be used.");

			//GlbGroup
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Code = "RANDOM";
			group.GG_IsActive = false;

			bizO.SubAccountTypeParentTableCode = GlbGroupSchema.Constants.Prefix;
			bizO.SubAccountParentId = group.PK;

			AssertHasError(bizO.SubAccountParentIdInfo, "This Sub Account is inactive - it may not be used.");
		}

		protected TestObjectCreator TestObjectCreator
		{
			get
			{
				return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
			}
		}
		TestObjectCreator testObjectCreator;
	}

	[TestedType(typeof(TransactionLineSubAccount))]
	public class TransactionLineSubAccountImplemntationTest : SubAccountImplementationTest<TransactionLineSubAccount>
	{
	}

	[TestedType(typeof(JournalSubAccount))]
	public class JournalSubAccountImplemntationTest : SubAccountImplementationTest<JournalSubAccount>
	{
	}
}
