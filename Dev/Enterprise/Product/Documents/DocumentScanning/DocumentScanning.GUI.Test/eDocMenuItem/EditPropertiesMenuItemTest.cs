using CargoWise.Types;
using Enterprise.DocumentScanning.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentScanning.GUI
{
	sealed class EditPropertiesMenuItemTest : eDocMenuItemTestCase
	{
		protected override eDocMenuItem GetMenuItem()
		{
			return new EditPropertiesMenuItem();
		}

		protected override void RunAssertionsForGetEnabledStatus(eDocMenuItem menuItem, StorageDocsBase bizO)
		{
			AssertEquals("Case: no BizO selected under mouse, Multiple rows NOT selected", false, menuItem.GetEnabledStatus(null, false));
			AssertEquals("Case: no BizO selected under mouse, Multiple rows selected", false, menuItem.GetEnabledStatus(null, true));

			AssertEquals("Case: BizO selected under mouse, Multiple rows NOT selected", true, menuItem.GetEnabledStatus(bizO, false));
			AssertEquals("Case: BizO selected under mouse, Multiple rows selected", false, menuItem.GetEnabledStatus(bizO, true));

			bizO.SC_IsDeleted = true;
			AssertEquals("Case: BizO selected under mouse is deleted, Multiple rows NOT selected", false, menuItem.GetEnabledStatus(bizO, false));
			AssertEquals("Case: BizO selected under mouse is deleted, Multiple rows selected", false, menuItem.GetEnabledStatus(bizO, true));
		}

		public void TestEnableStatusForCompanyBranchDepartmentSpecificDocument()
		{
			Env.Security.EditAllCompanySpecificDocuments.IsAllowed = false;
			Env.Security.EditAllBranchSpecificDocuments.IsAllowed = false;
			Env.Security.EditAllDepartmentSpecificDocuments.IsAllowed = false;
			eDocMenuItem menuItem = GetMenuItem();

			StorageDocs document = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			document.SC_GC_Company = ZGuid.Empty;
			document.SC_GB_Branch = ZGuid.Empty;
			document.SC_GE_Department = ZGuid.Empty;
			AssertEquals(true, menuItem.GetEnabledStatus(document, false));

			document.SC_GC_Company = GlbCompany.CurrentCompany.PK;
			document.SC_GB_Branch = GlbBranch.CurrentBranch.PK;
			document.SC_GE_Department = GlbDepartment.CurrentDepartment.PK;
			AssertEquals(true, menuItem.GetEnabledStatus(document, false));

			document.SC_GC_Company = ZGuid.NewZGuid();
			AssertEquals(false, menuItem.GetEnabledStatus(document, false));
			Env.Security.EditAllCompanySpecificDocuments.IsAllowed = true;
			AssertEquals(true, menuItem.GetEnabledStatus(document, false));

			document.SC_GB_Branch = ZGuid.NewZGuid();
			document.SC_GE_Department = ZGuid.NewZGuid();
			AssertEquals(false, menuItem.GetEnabledStatus(document, false));

			Env.Security.EditAllBranchSpecificDocuments.IsAllowed = true;
			Env.Security.EditAllDepartmentSpecificDocuments.IsAllowed = true;
			AssertEquals(true, menuItem.GetEnabledStatus(document, false));
		}
	}
}
