using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.GUI.Testing;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing
{
	sealed class G5V1TemporaryStoragePackedItemControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using (var control = new G5V1TemporaryStoragePackedItemControl())
			{
				var tabControl = control.FindSingle<ZTemplateTabControl>("PackedItemTabControl");
				CombineAssertions(() =>
				{
					AssertNotNull(tabControl);

					AssertEquals("Tab Pages count", 5, tabControl.TabCount);

					AssertContainsExactElementsInExactOrder("Tab Pages names", new[]
					{
						"PackedItemDetailsTabPage",
						"PackPackedItemPivotTabPage",
						"SupportingDocumentsTabPage",
						"PreviousDocumentsTabPage",
						"AdditionalInfoTabPage"
					}, tabControl.TabPages.ToList<ZTabPage>().Select(x => x.Name));
				});
			}
		}

		public void TestPackedItemDetailsTabPageCaption()
		{
			using (var control = new G5V1TemporaryStoragePackedItemControl())
			{
				var tabControl = control.FindSingle<ZTemplateTabControl>("PackedItemTabControl");
				var packedItemDetailsTabPage = tabControl.FindSingle<ZTabPage>("PackedItemDetailsTabPage");
				CombineAssertions(() =>
				{
					packedItemDetailsTabPage.AssertThisControl(x => x.WithCaption("Details")
																	 .WithTabVisible(true));
				});
			}
		}

		public void TestSetNoTabsForTSM()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			using (var control = new G5V1TemporaryStoragePackedItemControl())
			{
				header.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
				var tabControl = control.FindSingle<ZTemplateTabControl>("PackedItemTabControl");
				var additionalInfoTabPage = tabControl.GetTabPage("AdditionalInfoTabPage");
				var previousDocumentsTabPage = tabControl.GetTabPage("PreviousDocumentsTabPage");
				CombineAssertions(() =>
				{
					AssertEquals("additionalInfoTabPage for TSM", true, additionalInfoTabPage.TabVisible);
					AssertEquals("PreviousDocumentsTabPage for TSM", true, previousDocumentsTabPage.TabVisible);
				});
			}
		}

		public void TestSetNoTabsForLAME()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			using (var control = new G5V1TemporaryStoragePackedItemControl())
			{
				header.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;

				var tabControl = control.FindSingle<ZTemplateTabControl>("PackedItemTabControl");
				CombineAssertions(() =>
				{
					AssertEquals("additionalInfoTabPage for LAME", false, tabControl.Controls.Find("AdditionalInfoTabPage", true).First().Visible);
					AssertEquals("additionalInfoTabPage for LAME", false, tabControl.Controls.Find("PreviousDocumentsTabPage", true).First().Visible);
				});
			}
		}
	}
}
