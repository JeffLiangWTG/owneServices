using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.Testing
{
	class NewConsolidatedLineUserControlTest : TestCaseWithFactory
	{
		public void TestDestinationPlaceTextBox_CharacterCasing()
		{
			using (var control = new NewConsolidatedLineUserControl())
			{
				AssertEquals(CharacterCasing.Normal, control.FindSingle<ZTextBox>("DestinationPlaceTextBox").CharacterCasing);
			}
		}

		public void TestGoodsDescriptionTextBox_CharacterCasing()
		{
			using (var control = new NewConsolidatedLineUserControl())
			{
				AssertEquals(CharacterCasing.Normal, control.FindSingle<ZTextBox>("GoodsDescriptionTextBox").CharacterCasing);
			}
		}

		public void TestOwnerReferenceNumberTextBox_CharacterCasing()
		{
			using (var control = new NewConsolidatedLineUserControl())
			{
				AssertEquals(CharacterCasing.Normal, control.FindSingle<ZTextBox>("OwnerReferenceNumberTextBox").CharacterCasing);
			}
		}
	}
}
