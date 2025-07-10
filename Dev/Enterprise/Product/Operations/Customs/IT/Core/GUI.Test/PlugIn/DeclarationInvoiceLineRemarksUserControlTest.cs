using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class DeclarationInvoiceLineRemarksUserControlTest : TestCaseWithFactory
{
	public void TestControls()
	{
		using (var control = new DeclarationInvoiceLineRemarksUserControl())
		{
			var remarksTextBox = control.FindSingleOrDefault<ZTextBox>("RemarksTextBox");
			AssertNotNull("RemarksTextBox", remarksTextBox);
		}
	}

	public void TestRemarksTextBoxIsVisible()
	{
		using (var control = new DeclarationInvoiceLineRemarksUserControl())
		{
			var remarksTextBox = control.FindSingleOrDefault<ZTextBox>("RemarksTextBox");
			AssertNotNull("RemarksTextBox", remarksTextBox);
			Assert("Remarks text box should be visible", remarksTextBox.Visible);
		}
	}
}
