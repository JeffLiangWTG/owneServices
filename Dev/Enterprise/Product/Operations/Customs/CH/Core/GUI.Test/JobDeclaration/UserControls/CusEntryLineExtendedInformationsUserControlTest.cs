using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CH.GUI.Testing;

class EntryInstructionExtendedInformationsUserControlTest : TestCaseWithFactory
{
	public void TestTariffCodeVisibility()
	{
		using (var control = new CusEntryLineExtendedInformationsUserControl())
		{
			var codeTextBox = control.CL_AdValoremTariffTextBox;
			AssertNotNull(nameof(codeTextBox), codeTextBox);
			AssertEquals(nameof(codeTextBox.Visible), true, codeTextBox.Visible);
		}
	}

	public void TestTariffCodeDescriptionVisibility()
	{
		using (var control = new CusEntryLineExtendedInformationsUserControl())
		{
			var codeTextBox = control.CL_DescriptionTextBox;
			AssertNotNull(nameof(codeTextBox), codeTextBox);
			AssertEquals(nameof(codeTextBox.Visible), true, codeTextBox.Visible);
		}
	}
}
