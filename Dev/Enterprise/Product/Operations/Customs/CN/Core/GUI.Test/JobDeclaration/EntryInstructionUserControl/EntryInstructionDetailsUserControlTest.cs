using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.CN.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.CN.GUI.Testing
{
	[TestedType(typeof(EntryInstructionDetailsUserControl))]
	class EntryInstructionDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestControlsExistence()
		{
			using var control = new EntryInstructionDetailsUserControl();
			TestUtility.AssertControlExistance(control, "RemarksTextBox", "CustomsMessageRemarks");
		}

		public void TestSpecialBusinessIdentifiersEditButton_Click()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_CIQRequires = true;
			using var form = new JobDeclarationForm(declaration);
			form.Show();
			var entryInstructionUserControl = form.FindEntryInstructionUserControl();
			var entryInstructionsGrid = entryInstructionUserControl.FindSingle<ZGrid>("EntryInstructionsGrid");
			entryInstructionsGrid.Select(0);

			var control = entryInstructionUserControl.FindSingle<CodeDescriptionSelectionUserControl>("SpecialBusinessIdentifiersUserControl");
			AssertEquals("SpecialBusinessIdentifiersAsString", control.TextBox.GetBindingMember());
			control.EditButton.PerformClick();
			AssertType<CodeDescriptionOptionForm>(ZFormModaliser.LastFormShownDialogForTest);
			AssertSame(entryInstruction.SpecialBusinessIdentifiers, (ZFormModaliser.LastIBusinessShownOnDialogForTest as CodeDescriptionOptionCollectionParent).OptionCollection.Storage);
		}

		public void TestOtherPackagesEditButton_Click()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			using var form = new JobDeclarationForm(declaration);
			form.Show();
			var entryInstructionUserControl = form.FindEntryInstructionUserControl();
			var entryInstructionsGrid = entryInstructionUserControl.FindSingle<ZGrid>("EntryInstructionsGrid");
			entryInstructionsGrid.Select(0);

			var control = entryInstructionUserControl.FindSingle<CodeDescriptionSelectionUserControl>("OtherPackagesUserControl");
			AssertEquals("OtherPackagesAsString", control.TextBox.GetBindingMember());
			control.EditButton.PerformClick();
			AssertType<CodeDescriptionOptionForm>(ZFormModaliser.LastFormShownDialogForTest);
			AssertSame(entryInstruction.OtherPackages, (ZFormModaliser.LastIBusinessShownOnDialogForTest as CodeDescriptionOptionCollectionParent).OptionCollection.Storage);
		}

		public void TestOperationMattersEditButton_Click()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			using var form = new JobDeclarationForm(declaration);
			form.Show();
			var entryInstructionUserControl = form.FindEntryInstructionUserControl();
			var entryInstructionsGrid = entryInstructionUserControl.FindSingle<ZGrid>("EntryInstructionsGrid");
			entryInstructionsGrid.Select(0);

			var control = entryInstructionUserControl.FindSingle<CodeDescriptionSelectionUserControl>("OperationMattersUserControl");
			AssertEquals("OperationMattersAsString", control.TextBox.GetBindingMember());
			control.EditButton.PerformClick();
			AssertType<CodeDescriptionOptionForm>(ZFormModaliser.LastFormShownDialogForTest);
			AssertSame(entryInstruction.OperationMatters, (ZFormModaliser.LastIBusinessShownOnDialogForTest as CodeDescriptionOptionCollectionParent).OptionCollection.Storage);
		}
	}
}
