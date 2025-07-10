using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class UnloadedItemNewUserControlTest : TestCaseWithFactory
	{
		public void TestControlsCharacterCasing()
		{
			using (var control = new UnloadedItemNewUserControl())
			{
				CombineAssertions(() =>
				{
					var unloadingNotesTextBox = control.FindSingle<ZTextBox>("UnloadingNotesTextBox");
					AssertEquals("UnloadingNotesTextBox", System.Windows.Forms.CharacterCasing.Normal, unloadingNotesTextBox.CharacterCasing);

					var descriptionOfGoodsTextBox = control.FindSingle<ZTextBox>("DescriptionOfGoodsTextBox");
					AssertEquals("DescriptionOfGoodsTextBox", System.Windows.Forms.CharacterCasing.Normal, descriptionOfGoodsTextBox.CharacterCasing);
				});
			}
		}

		public void TestCommodityCodeTextBox()
		{
			using (var control = new UnloadedItemNewUserControl())
			{
				var commodityCodeTextBox = control.FindSingle<ZTextBox>("CommodityCodeTextBox");
				AssertEquals("CommodityCode is bound to the formatted tariff", "UnloadingMovementHeader.GoodsItems.BY_FormattedHarmonisedTariff", commodityCodeTextBox.GetBindingMember());
			}
		}
	}
}
