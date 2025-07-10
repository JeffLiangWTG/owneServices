using Enterprise.DocumentScanning.Business;

namespace Enterprise.DocumentScanning.GUI
{
	sealed class DeletePermanentlyMenuItemTest : eDocMenuItemTestCase
	{
		protected override void RunAssertionsForGetEnabledStatus(eDocMenuItem menuItem, StorageDocsBase bizO)
		{
			AssertEquals("Case: no BizO selected under mouse, Multiple rows NOT selected", false, menuItem.GetEnabledStatus(null, false));
			AssertEquals("Case: no BizO selected under mouse, Multiple rows selected", true, menuItem.GetEnabledStatus(null, true));

			AssertEquals("Case: BizO selected under mouse, Multiple rows NOT selected", true, menuItem.GetEnabledStatus(bizO, false));
			AssertEquals("Case: BizO selected under mouse, Multiple rows selected", true, menuItem.GetEnabledStatus(bizO, true));
		}

		protected override eDocMenuItem GetMenuItem()
		{
			return new DeletePermanentlyMenuItem();
		}
	}
}
