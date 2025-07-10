using CargoWise.Types;
using Enterprise.DocumentScanning.Business;

namespace Enterprise.DocumentScanning.GUI
{
	sealed class AllocateMenuItemTest : eDocMenuItemTestCase
	{
		protected override void RunAssertionsForGetEnabledStatus(eDocMenuItem menuItem, StorageDocsBase bizO)
		{
			if ((bizO is StorageDocs && !(bizO is StorageDocsUnallocated)) || bizO is StorageFile)
			{
				Assert(true);
				return;
			}

			AssertEquals("Case: no BizO selected under mouse, Multiple rows NOT selected", false, menuItem.GetEnabledStatus(null, false));
			AssertEquals("Case: no BizO selected under mouse, Multiple rows selected", true, menuItem.GetEnabledStatus(null, true));

			AssertEquals("Case: BizO selected under mouse, Multiple rows NOT selected", true, menuItem.GetEnabledStatus(bizO, false));
			AssertEquals("Case: BizO selected under mouse, Multiple rows selected", true, menuItem.GetEnabledStatus(bizO, true));

			bizO.SC_IsDeleted = true;
			AssertEquals("Case: BizO selected under mouse is deleted, Multiple rows NOT selected", false, menuItem.GetEnabledStatus(bizO, false));
			AssertEquals("Case: BizO selected under mouse is deleted, Multiple rows selected (allowed because multiple rows take precedence)", true, menuItem.GetEnabledStatus(bizO, true));

			bizO.SC_IsDeleted = false;
			bizO.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			bizO.SC_ParentID = ZGuid.NewZGuid();
			AssertEquals("Case: BizO selected under mouse is allocated, Multiple rows NOT selected", true, menuItem.GetEnabledStatus(bizO, false));
			AssertEquals("Case: BizO selected under mouse is allocated, Multiple rows selected (allowed because multiple rows take precedence)", true, menuItem.GetEnabledStatus(bizO, true));
		}

		protected override eDocMenuItem GetMenuItem()
		{
			return new AllocateMenuItem();
		}
	}
}
