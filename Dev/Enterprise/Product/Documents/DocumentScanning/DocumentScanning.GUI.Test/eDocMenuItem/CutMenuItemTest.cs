using Enterprise.DocumentScanning.Business;

namespace Enterprise.DocumentScanning.GUI
{
	sealed class CutMenuItemTest : eDocMenuItemTestCase
	{
		protected override void RunAssertionsForGetEnabledStatus(eDocMenuItem menuItem, StorageDocsBase bizO)
		{
			AssertEquals("Case: no BizO selected under mouse, Multiple rows NOT selected", false, menuItem.GetEnabledStatus(null, false));

			AssertEquals("Case: BizO selected under mouse, Multiple rows NOT selected", true, menuItem.GetEnabledStatus(bizO, false));
			AssertEquals("Case: BizO selected under mouse, Multiple rows selected", true, menuItem.GetEnabledStatus(bizO, true));

			bizO.SC_IsDeleted = true;
			AssertEquals("Case: BizO selected under mouse is deleted, Multiple rows NOT selected", false, menuItem.GetEnabledStatus(bizO, false));
			AssertEquals("Case: BizO selected under mouse is deleted, Multiple rows selected (allowed because multiple rows take precedence)", true, menuItem.GetEnabledStatus(bizO, true));

			bizO.SC_IsDeleted = false;
			bizO.SC_IsSystemGenerated = true;
			AssertEquals("Case: BizO selected under mouse is system generated, Multiple rows NOT selected", false, menuItem.GetEnabledStatus(bizO, false));
			AssertEquals("Case: BizO selected under mouse is system generated, Multiple rows selected (allowed because multiple rows take precedence)", true, menuItem.GetEnabledStatus(bizO, true));
		}

		protected override eDocMenuItem GetMenuItem()
		{
			return new CutMenuItem();
		}

		public override void TestGetEnabledStatusWhenMultipleElementsSelected()
		{
			CutMenuItem cutMenuItem = new CutMenuItem();

			StorageDocs document1 = StorageDocs.NewWithParentWithoutFK_DEBUG(MasterFactory);
			StorageDocs document2 = StorageDocs.NewWithParentWithoutFK_DEBUG(MasterFactory);
			StorageDocs[] selectedDocs = new StorageDocs[] { document1, document2 };

			Assert(cutMenuItem.GetEnabledStatusWhenMultipleElementsSelected(selectedDocs));

			document1.SC_IsSystemGenerated = true;
			Assert(!cutMenuItem.GetEnabledStatusWhenMultipleElementsSelected(selectedDocs));

			document1.SC_IsSystemGenerated = false;
			document2.Delete();
			Assert(!cutMenuItem.GetEnabledStatusWhenMultipleElementsSelected(selectedDocs));
		}
	}
}
