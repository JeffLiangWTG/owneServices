using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.Testing
{
	class REXDISAWBDeclarationUserControlTest : TestCaseWithFactory
	{
		public void TestOwnerRefNumberTextBox_CharacterCasing()
		{
			using (var control = new REXDISAWBDeclarationUserControl())
			{
				AssertEquals(CharacterCasing.Normal, control.FindSingle<ZTextBox>("OwnerRefNumberTextBox").CharacterCasing);
			}
		}
	}
}
