using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class ItemSecurityTabUserControlTest : TestCaseWithFactory
	{
		public void TestCommercialReferenceNumberTextBoxAllowsNormalCase()
		{
			using (var control = new ItemSecurityTabUserControl())
			{
				var editControl = control.FindSingle<ZTextBox>("CommercialReferenceNumberTextBox");
				AssertEquals("normal casing", CharacterCasing.Normal, editControl.CharacterCasing);
			}
		}
	}
}
