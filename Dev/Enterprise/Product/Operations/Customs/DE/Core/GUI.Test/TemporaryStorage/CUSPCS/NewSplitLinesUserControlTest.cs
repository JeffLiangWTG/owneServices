using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.Testing
{
	public class NewSplitLinesUserControlTest : TestCaseWithFactory
	{
		public void TestDestinationPlaceTextBox_CharacterCasing()
		{
			using (var control = new NewSplitLinesUserControl())
			{
				AssertEquals(System.Windows.Forms.CharacterCasing.Normal, control.FindSingle<ZTextBox>("DestinationPlaceTextBox").CharacterCasing);
			}
		}

		public void TestOwneRefNumberTextBox_CharacterCasing()
		{
			using (var control = new NewSplitLinesUserControl())
			{
				AssertEquals(System.Windows.Forms.CharacterCasing.Normal, control.FindSingle<ZTextBox>("OwneRefNumberTextBox").CharacterCasing);
			}
		}
	}
}
