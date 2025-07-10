using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.ExitControl.Business;
using Enterprise.Customs.EU.ExitControl.GUI;

namespace Enterprise.Customs.DE.ExitControl.GUI.Testing
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
				AssertEquals(4, additionalMainMenuItems.Count);
				AssertEquals("CreateExitReportMenuItem", CreateExitReportMenuItemCreator.CreateExitReportMenuItemName, additionalMainMenuItems[0].Name);
				AssertEquals("SelectEditReportItemsMenuItem", SelectReportItemMenuItemCreator.SelectEditReportItemsMenuItemName, additionalMainMenuItems[1].Name);
				AssertEquals("SendToCustomsMenuItemSeparatorMenuItem", ExitControlMenuItem.SendToCustomsMenuItemSeparatorMenuItemName, additionalMainMenuItems[2].Name);
				AssertEquals("SendToCustomsMenuItem", ExitControlSendToCustomsMenuCreator.SendToCustomsMenuItemName, additionalMainMenuItems[3].Name);
			});
		}
	}
}
