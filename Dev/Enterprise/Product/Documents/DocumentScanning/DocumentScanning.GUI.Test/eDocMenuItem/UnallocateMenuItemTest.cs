using CargoWise.Types;
using Enterprise.DocumentScanning.Business;

namespace Enterprise.DocumentScanning.GUI
{
	sealed class UnallocateMenuItemTest : eDocMenuItemTestCase
	{
		protected override void RunAssertionsForGetEnabledStatus(eDocMenuItem menuItem, StorageDocsBase bizO)
		{
			if (bizO.IsImageFile)
			{
				RunAssertionsForStorageDocs(menuItem, bizO);
			}
			else
			{
				RunAssertionsForStorageFile(menuItem, bizO);
			}
		}

		void RunAssertionsForStorageDocs(eDocMenuItem menuItem, StorageDocsBase bizO)
		{
			if (bizO is StorageDocsUnallocated || bizO is StorageFile)
			{
				Assert(true);
				return;
			}

			AssertEquals("Case: no BizO selected under mouse, Multiple rows NOT selected", false, menuItem.GetEnabledStatus(null, false));
			AssertEquals("Case: no BizO selected under mouse, Multiple rows selected", true, menuItem.GetEnabledStatus(null, true));

			AssertEquals("Case: BizO selected under mouse, Multiple rows NOT selected, not allowed because doc is not allocated", false, menuItem.GetEnabledStatus(bizO, false));
			AssertEquals("Case: BizO selected under mouse, Multiple rows selected (allowed because multiple rows take precedence)", true, menuItem.GetEnabledStatus(bizO, true));

			bizO.SC_IsDeleted = true;
			AssertEquals("Case: BizO selected under mouse is deleted, Multiple rows NOT selected", false, menuItem.GetEnabledStatus(bizO, false));
			AssertEquals("Case: BizO selected under mouse is deleted, Multiple rows selected (allowed because multiple rows take precedence)", true, menuItem.GetEnabledStatus(bizO, true));

			bizO.SC_IsDeleted = false;
			bizO.SC_IsSystemGenerated = true;
			AssertEquals("Case: BizO selected under mouse is system generated, Multiple rows NOT selected", false, menuItem.GetEnabledStatus(bizO, false));
			AssertEquals("Case: BizO selected under mouse is system generated, Multiple rows selected (allowed because multiple rows take precedence)", true, menuItem.GetEnabledStatus(bizO, true));

			bizO.SC_IsSystemGenerated = false;
			bizO.ParentMain.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			bizO.ParentMain.SM_ParentFK = ZGuid.NewZGuid();
			AssertEquals("Precondition: BizO is allocated", true, bizO.IsAllocated);
			AssertEquals("Case: BizO selected under mouse is allocated, multiple rows NOT selected", true, menuItem.GetEnabledStatus(bizO, false));
			AssertEquals("Case: BizO selected under mouse is allocated, multiple rows selected", true, menuItem.GetEnabledStatus(bizO, false));
		}

		void RunAssertionsForStorageFile(eDocMenuItem menuItem, StorageDocsBase bizO)
		{
			AssertEquals("Case: no BizO selected under mouse, Multiple rows NOT selected", false, menuItem.GetEnabledStatus(null, false));
			AssertEquals("Case: no BizO selected under mouse, Multiple rows selected", true, menuItem.GetEnabledStatus(null, true));

			AssertEquals("Case: BizO selected under mouse, Multiple rows NOT selected, not allowed because doc is not allocated", false, menuItem.GetEnabledStatus(bizO, false));
			AssertEquals("Case: BizO selected under mouse, Multiple rows selected (allowed because multiple rows take precedence)", true, menuItem.GetEnabledStatus(bizO, true));

			bizO.SC_IsDeleted = true;
			AssertEquals("Case: BizO selected under mouse is deleted, Multiple rows NOT selected", false, menuItem.GetEnabledStatus(bizO, false));
			AssertEquals("Case: BizO selected under mouse is deleted, Multiple rows selected (allowed because multiple rows take precedence)", true, menuItem.GetEnabledStatus(bizO, true));

			bizO.SC_IsDeleted = false;
			bizO.SC_IsSystemGenerated = true;
			AssertEquals("Case: BizO selected under mouse is system generated, Multiple rows NOT selected", false, menuItem.GetEnabledStatus(bizO, false));
			AssertEquals("Case: BizO selected under mouse is system generated, Multiple rows selected (allowed because multiple rows take precedence)", true, menuItem.GetEnabledStatus(bizO, true));

			bizO.SC_IsSystemGenerated = false;
			bizO.ParentMain.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			bizO.ParentMain.SM_ParentFK = ZGuid.NewZGuid();
			AssertEquals("Precondition: BizO is allocated", true, bizO.IsAllocated);
			AssertEquals("Case: BizO selected under mouse is allocated, multiple rows NOT selected - not tif files can't be unallocated at the moment", false, menuItem.GetEnabledStatus(bizO, false));
			AssertEquals("Case: BizO selected under mouse is allocated, multiple rows selected", true, menuItem.GetEnabledStatus(bizO, true));
		}

		protected override eDocMenuItem GetMenuItem()
		{
			return new UnallocateMenuItem();
		}
	}
}
