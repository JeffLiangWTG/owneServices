using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	[TestedType(typeof(UpdateImportEntryNumberForm))]
	class UpdateImportEntryNumberFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter("TST1", ZDateTime.Now);

			return new UpdateImportEntryNumberForm(new UpdateImportEntryNumberObject(declaration));
		}

		public void TestFormComponents()
		{
			using (var form = GetFormToBashCore() as UpdateImportEntryNumberForm)
			{
				AssertEquals("Update Entry Number", form.CaptionResourceString.Caption);
				AssertType<ZLabel>(form.InstructionLabel);
				AssertType<ZButton>(form.UpdateButton);
				AssertType<ZButton>(form.CloseButton);
				AssertType<ZTextBox>(form.EntryNumberTextBox);
				AssertType<ZDateEdit>(form.RegistrationDateDateEdit);
			}
		}

		public void TestOnOkButton_Click()
		{
			var date = ZDateTime.Now;
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter("TST1", date);

			var form = new UpdateImportEntryNumberFormForTest(new UpdateImportEntryNumberObject(declaration));

			using (form)
			{
				var source = form.DataSource as UpdateImportEntryNumberObject;
				var declaratiom = source.Declaration;
				using (declaratiom.SuspendSettingHasChanges())
				{
					source.EntryNumber = "1111111111";
					source.RegistrationDate = ZDateTime.Now.AddDays(-1);
				}
				form.Show();

				AssertEquals("MovementReferenceNumber should be", "TST1", entryHeader.MovementReferenceNumber);
				AssertEquals("MovementReferenceNumberIssueDate should be", date, entryHeader.MovementReferenceNumberIssueDate);

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				form.PerformClickOkButton();

				AssertEquals("MovementReferenceNumber should be", source.EntryNumber, entryHeader.MovementReferenceNumber);
				AssertEquals("MovementReferenceNumberIssueDate should be", source.RegistrationDate, entryHeader.MovementReferenceNumberIssueDate);
			}
		}

		class UpdateImportEntryNumberFormForTest : UpdateImportEntryNumberForm
		{
			public UpdateImportEntryNumberFormForTest(UpdateImportEntryNumberObject declaration)
				: base(declaration)
			{
			}

			public void PerformClickOkButton() => UpdateButton.PerformClick();
		}
	}
}

