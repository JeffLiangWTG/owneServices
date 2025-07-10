using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	[TestedType(typeof(GenerateImportLicenseForm))]
	class GenerateImportLicenseFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var date = ZDateTime.Now;
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Description = "Inst-1";
			var invHeader = declaration.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			invLine.JI_CEI = entryInstruction.PK;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();
			invLine.JI_CL = entryLine.PK;

			entryInstruction.EntryHeader.MovementReferenceNumberSetter("TST1", date);
			Factory.Save();

			return new GenerateImportLicenseForm(new GenerateImportLicenseObject(entryLine));
		}

		public void TestOnAttachButton_Click()
		{
			var licDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			licDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			using (var form = GetFormToBashCore() as GenerateImportLicenseForm)
			{
				form.Show();

				var declaration = form.DataSource as GenerateImportLicenseObject;
				form.OkButton.PerformClick();
				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);

				declaration.ImportLicenseDeclarationPK = licDeclaration.PK;

				form.OkButton.PerformClick();
				AssertEquals("Generate Import License Completed!", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestFormComponents()
		{
			using (var form = GetFormToBashCore() as GenerateImportLicenseForm)
			{
				AssertEquals("Select Import License", form.CaptionResourceString.Caption);
				AssertType<ZLabel>(form.SelectAnILLabel);
				AssertType<ZGuidFindBox>(form.GuidFindBox);
				AssertType<ZButton>(form.OkButton);
				AssertType<ZButton>(form.CloseButton);
			}
		}
	}
}
