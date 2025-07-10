using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Module.Testing
{
	class SubAccountModuleFilterValidationTest : BusinessObjectValidationTestCase
	{
		public void TestSubAccountType()
		{
			Filter.SubAccountType = ZString.Empty;
			AssertNoNotifications(Filter.SubAccountTypeInfo);

			Filter.SubAccountType = "123";
			AssertHasError(Filter.SubAccountTypeInfo, "Enter a valid selection.");

			Filter.SubAccountType = AccountingMasterFilesConstants.SubAccountTypeList.Organization.Code;
			AssertNoNotifications(Filter.SubAccountTypeInfo);

			Filter.SubAccountType = "123";
			AssertHasError(Filter.SubAccountTypeInfo, "Enter a valid selection.");

			Filter.SubAccountType = AccountingMasterFilesConstants.SubAccountTypeList.SalesGroup.Code;
			AssertNoNotifications(Filter.SubAccountTypeInfo);

			Filter.SubAccountType = "123";
			AssertHasError(Filter.SubAccountTypeInfo, "Enter a valid selection.");

			Filter.SubAccountType = AccountingMasterFilesConstants.SubAccountTypeList.StaffAndResources.Code;
			AssertNoNotifications(Filter.SubAccountTypeInfo);

			Filter.SubAccountType = "123";
			AssertHasError(Filter.SubAccountTypeInfo, "Enter a valid selection.");

			Filter.SubAccountType = AccountingMasterFilesConstants.SubAccountTypeList.StaffGroup.Code;
			AssertNoNotifications(Filter.SubAccountTypeInfo);
		}

		public void TestSubAccount()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var org = testObjectCreator.CreateOrgHeader("TSTORG1", false, false);
			Factory.Save();

			Filter.SubAccountType = AccountingMasterFilesConstants.SubAccountTypeList.Organization.Code;
			Filter.SubAccount = org.PK;
			AssertNoNotifications(Filter.SubAccountInfo);

			Filter.SubAccount = ZGuid.Invalid;
			AssertHasError(Filter.SubAccountInfo, "Enter a valid selection.");

			var salesGroup = testObjectCreator.CreateSalesGroup("GRP1");
			Factory.Save();

			Filter.SubAccountType = AccountingMasterFilesConstants.SubAccountTypeList.SalesGroup.Code;
			Filter.SubAccount = salesGroup.PK;
			AssertNoNotifications(Filter.SubAccountInfo);

			Filter.SubAccount = ZGuid.Invalid;
			AssertHasError(Filter.SubAccountInfo, "Enter a valid selection.");

			var staff = testObjectCreator.CreateStaff("TS");
			Factory.Save();

			Filter.SubAccountType = AccountingMasterFilesConstants.SubAccountTypeList.StaffAndResources.Code;
			Filter.SubAccount = staff.PK;
			AssertNoNotifications(Filter.SubAccountInfo);

			Filter.SubAccount = ZGuid.Invalid;
			AssertHasError(Filter.SubAccountInfo, "Enter a valid selection.");

			var staffGroup = testObjectCreator.CreateStaffGroup("TSGroup");
			Factory.Save();

			Filter.SubAccountType = AccountingMasterFilesConstants.SubAccountTypeList.StaffGroup.Code;
			filter.SubAccount = staffGroup.PK;
			AssertNoNotifications(Filter.SubAccountInfo);

			Filter.SubAccount = ZGuid.Invalid;
			AssertHasError(Filter.SubAccountInfo, "Enter a valid selection.");

			Filter.SubAccount = ZGuid.Empty;
			AssertNoNotifications(Filter.SubAccountInfo);
		}

		#region Implementation

		SubAccountFilter Filter
		{
			get { return filter ?? (filter = new SubAccountFilter("Sub account")); }
		}
		SubAccountFilter filter;

		#endregion
	}
}
