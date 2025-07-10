using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.ExitControl.GUI;
using Enterprise.Customs.IE.ExitControl.Business;

namespace Enterprise.Customs.IE.ExitControl.GUI.Testing
{
	sealed class ExitControlMainMenuProviderTest : TestCaseWithFactory
	{
		public void TestAdditionalMainMenuItems()
		{
			var header = Factory.New<CusExitHeader>();
			IExitControlMainMenuProvider provider = new ExitControlMainMenuProvider(header);
			var additionalMainMenuItems = provider.AdditionalMainMenuItems;
			CombineAssertions(() =>
			{
				AssertEquals(5, additionalMainMenuItems.Count);
				AssertEquals("CreateExitReportMenuItem", EU.ExitControl.GUI.CreateExitReportMenuItemCreator.CreateExitReportMenuItemName, additionalMainMenuItems[0].Name);
				AssertEquals("SelectEditReportItemsMenuItem", EU.ExitControl.GUI.SelectReportItemMenuItemCreator.SelectEditReportItemsMenuItemName, additionalMainMenuItems[1].Name);
				AssertEquals("SendToCustomsMenuItemSeparatorMenuItem", EU.ExitControl.GUI.ExitControlMenuItem.SendToCustomsMenuItemSeparatorMenuItemName, additionalMainMenuItems[2].Name);
				AssertEquals("SendToCustomsMenuItem", EU.ExitControl.GUI.ExitControlSendToCustomsMenuCreator.SendToCustomsMenuItemName, additionalMainMenuItems[3].Name);
				AssertEquals("UploadSupportingDocumentsMenuItem", ExitControlUploadSupportingDocumentsMenuCreator.UploadSupportingDocumentsMenuItemName, additionalMainMenuItems[4].Name);
			});
		}
	}
}
