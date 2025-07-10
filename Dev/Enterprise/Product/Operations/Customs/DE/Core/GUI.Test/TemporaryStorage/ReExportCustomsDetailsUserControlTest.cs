using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.Testing
{
	class ReExportCustomsDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestAdditionalInfoTextBox_CharacterCasing()
		{
			using (var control = new ReExportCustomsDetailsUserControl())
			{
				AssertEquals(CharacterCasing.Normal, control.FindSingle<ZTextBox>("AdditionalInfoTextBox").CharacterCasing);
			}
		}
	}
}
