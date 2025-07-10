using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.Testing
{
	class SumACustomsDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestAdditionalInfoTextBox_CharacterCasing()
		{
			using (var control = new SumACustomsDetailsUserControl())
			{
				AssertEquals(CharacterCasing.Normal, control.FindSingle<ZTextBox>("AdditionalInfoTextBox").CharacterCasing);
			}
		}

		public void TestPresentationDateFormat()
		{
			using (var control = new SumACustomsDetailsUserControl())
			{
				AssertEquals(Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long, control.FindSingle<ZDateEdit>("PresentationDateDateEdit").DateTimeFormat);
			}
		}
	}
}
