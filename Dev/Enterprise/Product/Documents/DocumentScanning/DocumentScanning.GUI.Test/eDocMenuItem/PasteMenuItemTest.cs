using Enterprise.DocumentScanning.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentScanning.GUI.Testing
{
	sealed class PasteMenuItemTest : eDocMenuItemTestCase
	{
		protected override void RunAssertionsForGetEnabledStatus(eDocMenuItem menuItem, StorageDocsBase bizO)
		{
			// put something on the clipboard
			SafeClipboard.SetDataObject("a");
			AssertEquals("Case: data on clipboard, no BizO selected under mouse, Multiple rows NOT selected", true, menuItem.GetEnabledStatus(null, false));
			AssertEquals("Case: data on clipboard, no BizO selected under mouse, Multiple rows selected", true, menuItem.GetEnabledStatus(null, true));

			AssertEquals("Case: data on clipboard, BizO selected under mouse, Multiple rows NOT selected", true, menuItem.GetEnabledStatus(bizO, false));
			AssertEquals("Case: data on clipboard, BizO selected under mouse, Multiple rows selected", true, menuItem.GetEnabledStatus(bizO, true));

			bizO.SC_IsDeleted = true;
			AssertEquals("Case: data on clipboard, BizO selected under mouse is deleted, Multiple rows NOT selected", false, menuItem.GetEnabledStatus(bizO, false));
			AssertEquals("Case: data on clipboard, BizO selected under mouse is deleted, Multiple rows selected", false, menuItem.GetEnabledStatus(bizO, true));

			bizO.SC_IsDeleted = false;
			bizO.SC_IsSystemGenerated = true;
			AssertEquals("Case: data on clipboard, BizO selected under mouse is system generated, Multiple rows NOT selected", false, menuItem.GetEnabledStatus(bizO, false));
			AssertEquals("Case: data on clipboard, BizO selected under mouse is system generated, Multiple rows selected", false, menuItem.GetEnabledStatus(bizO, true));

			bizO.SC_IsSystemGenerated = false;
		}

		protected override eDocMenuItem GetMenuItem()
		{
			return new PasteMenuItem();
		}

		public override void TestGetEnabledStatusWhenMultipleElementsSelected()
		{
			PasteMenuItem pasteMenuItem = new PasteMenuItem();

			StorageDocs document1 = StorageDocs.NewWithParentWithoutFK_DEBUG(MasterFactory);
			StorageDocs document2 = StorageDocs.NewWithParentWithoutFK_DEBUG(MasterFactory);
			StorageDocs[] selectedDocs = new StorageDocs[] { document1, document2 };

			Assert(!pasteMenuItem.GetEnabledStatusWhenMultipleElementsSelected(selectedDocs));
		}
	}
}
