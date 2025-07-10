using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	[TestedType(typeof(ImportLicenseAttachingForm))]
	class ImportLicenseAttachingFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_JE = declaration.PK;
			entryInstruction.CEI_Description = "TEST1";

			var invHeader = declaration.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			invLine.JI_CEI = entryInstruction.PK;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entryHeader.AllEntryLines.AddNew();
			invLine.JI_CL = entryLine.PK;

			entryInstruction.EntryHeader.MovementReferenceNumberSetter("TST1", ZDateTime.Now);
			Factory.Save();
			var parent = new ImportLicenseAttachingObjectParent(declaration);
			var destinationDeclaration = Factory.New<JobDeclaration>();
			destinationDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			return new ImportLicenseAttachingForm(parent, destinationDeclaration);
		}

		public void TestAttachingObjectParent()
		{
			using (var form = GetFormToBashCore() as ImportLicenseAttachingForm)
			{
				var attachingObjectParent = form.AttachingObjectParent;
				AssertEquals("Should have 1 registry", 1, attachingObjectParent.ImportLicenses.Count);
			}
		}

		public void TestOnAttachButton_Click()
		{
			using (var form = GetFormToBashCore() as ImportLicenseAttachingForm)
			{
				form.Show();

				var destinationDeclaration = form.DestinationDeclaration;
				AssertEquals("Precondition:", 0, destinationDeclaration.AttachedImportLicenseEntries.Count);
				AssertEquals("Precondition:", 0, destinationDeclaration.Invoices.Count);
				AssertEquals("Precondition:", 0, destinationDeclaration.InvoiceLines.Count);

				var importLicense = form.AttachingObjectParent.ImportLicenses[0];
				var instruction = importLicense.EntryInstruction;
				instruction.EntryHeader.MovementReferenceNumberSetter("01234567891", ZDateTime.Now);

				importLicense.ShouldAttach = true;
				form.AttachButton.PerformClick();
				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);

				importLicense.ShouldAttach = false;
				form.AttachButton.PerformClick();
				AssertEquals("Please select at least one License to attach.", UnitTestUserNotification.Instance.LastMessage.Text);

				instruction.EntryHeader.MovementReferenceNumberSetter("0123456789", ZDateTime.Now);
				importLicense.ShouldAttach = true;
				form.AttachButton.PerformClick();

				AssertEquals("One Import License Entry attached", 1, destinationDeclaration.AttachedImportLicenseEntries.Count);
				AssertEquals("One Invoice attached", 1, destinationDeclaration.Invoices.Count);
				AssertEquals("One Invoice Line attached", 1, destinationDeclaration.InvoiceLines.Count);
			}
		}

		public void TestFormComponents()
		{
			using (var form = GetFormToBashCore() as ImportLicenseAttachingForm)
			{
				CombineAssertions(() =>
				{
					AssertEquals("Form Caption should be", "Attaching Licenses", form.CaptionResourceString.Caption);
					AssertType<ZGroupBox>("LicensesGroupBox type should be ZGroupBox", form.LicensesGroupBox);
					AssertType<ZGrid>("LicensesGrid type should be ZGrid", form.LicensesGrid);
					AssertType<ZButton>("AttachButton type should be ZButton", form.AttachButton);
					AssertType<ZButton>("CloseButton type should be ZButton", form.CloseButton);
					AssertEquals("LicensesGrid should have ", 6, form.LicensesGrid.ColumnStyles.Count);
					Assert("Should not show Mass Update menu item", !form.LicensesGrid.ShowMassUpdateMenuItem);
					Assert("Should not show Import Data menu item", form.LicensesGrid.DisableImportDataMenuItem);
					AssertColumnStyle<ZCheckBoxColumnStyleInfo>(form.LicensesGrid, "ShouldAttach");
					AssertColumnStyle<ZTextBoxColumnStyleInfo>(form.LicensesGrid, "ImportLicenseEntryDescription");
					AssertColumnStyle<ZTextBoxColumnStyleInfo>(form.LicensesGrid, "ImportLicenseEntryMRN");
					AssertColumnStyle<ZDateEditColumnStyleInfo>(form.LicensesGrid, "ImportLicenseEntryRegistrationDate");
					AssertColumnStyle<ZTextBoxColumnStyleInfo>(form.LicensesGrid, "ImportLicenseEntryStatusDescription");
					AssertColumnStyle<ZDropEditColumnStyleInfo>(form.LicensesGrid, "FeeType");
				});
			}
		}

		void AssertColumnStyle<T>(ZGrid grid, ZString columnName) where T : ZGridColumnInfo
		{
			var column = grid.ColumnStyles.Cast<ZGridColumnInfo>().SingleOrDefault(x => x.ColumnName == columnName);
			AssertNotNull(column);
			AssertType<T>($"{column} type should be {nameof(T)}", column);
		}
	}
}
