using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.CN.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.CN.GUI.Testing
{
	[TestedType(typeof(CodeDescriptionSelectionUserControl))]
	class CodeDescriptionSelectionUserControlTest : TestCaseWithFactory
	{
		public void TestEditButton_Click()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			using var form = new ZForm(entryInstruction);
			using var control = new CodeDescriptionSelectionUserControl();
			form.Controls.Add(control);
			control.SetBindingMember(nameof(CusEntryInstruction.OperationMattersAsString), x => (x as CusEntryInstruction)?.OperationMatters);
			AssertEquals("OperationMattersAsString", control.TextBox.GetBindingMember());
			form.Show();

			control.HideCodeOnSelectionForm = false;
			control.EditButton.PerformClick();
			AssertType<CodeDescriptionOptionForm>(ZFormModaliser.LastFormShownDialogForTest);
			AssertEquals(false, (ZFormModaliser.LastFormShownDialogForTest as CodeDescriptionOptionForm).OptionsGrid.GetColumnStyle("Code").IsUnavailable);

			control.HideCodeOnSelectionForm = true;
			control.EditButton.PerformClick();
			AssertType<CodeDescriptionOptionForm>(ZFormModaliser.LastFormShownDialogForTest);
			AssertEquals(true, (ZFormModaliser.LastFormShownDialogForTest as CodeDescriptionOptionForm).OptionsGrid.GetColumnStyle("Code").IsUnavailable);
		}
	}
}
