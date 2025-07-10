using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class UnloadedItemDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestControlsCharacterCasing()
		{
			CombineAssertions(() =>
			{
				var otherNotesTextBox = control.FindSingle<ZTextBox>("OtherNotesTextBox");
				AssertEquals("OtherNotesTextBox", System.Windows.Forms.CharacterCasing.Normal, otherNotesTextBox.CharacterCasing);

				var unloadedDescriptionOfGoodsTextBox = control.FindSingle<ZTextBox>("UnloadedDescriptionOfGoodsTextBox");
				AssertEquals("UnloadedDescriptionOfGoodsTextBox", System.Windows.Forms.CharacterCasing.Normal, unloadedDescriptionOfGoodsTextBox.CharacterCasing);

				var originalDescriptionOfGoodsTextBox = control.FindSingle<ZTextBox>("OriginalDescriptionOfGoodsTextBox");
				AssertEquals("OriginalDescriptionOfGoodsTextBox", System.Windows.Forms.CharacterCasing.Normal, originalDescriptionOfGoodsTextBox.CharacterCasing);
			});
		}

		public void TestUnloadedCommodityCodeTextBox()
		{
			var unloadedCommodityCodeTextBox = control.FindSingle<ZTextBox>("UnloadedCommodityCodeTextBox");
			AssertEquals("CommodityCode is bound to the formatted tariff", "UnloadingMovementHeader.GoodsItems.BY_FormattedHarmonisedTariff", unloadedCommodityCodeTextBox.GetBindingMember());
		}

		public void TestOriginalCommodityCodeTextBox()
		{
			var originalCommodityCodeTextBox = control.FindSingle<ZTextBox>("OriginalCommodityCodeTextBox");
			AssertEquals("CommodityCode is bound to the formatted tariff", "ArrivalMovementHeader.GoodsItems.BY_FormattedHarmonisedTariff", originalCommodityCodeTextBox.GetBindingMember());
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new UnloadedItemDetailsUserControl();
		}
		UnloadedItemDetailsUserControl control;

		protected override void TearDown()
		{
			base.TearDown();
			control?.Dispose();
		}
	}
}
