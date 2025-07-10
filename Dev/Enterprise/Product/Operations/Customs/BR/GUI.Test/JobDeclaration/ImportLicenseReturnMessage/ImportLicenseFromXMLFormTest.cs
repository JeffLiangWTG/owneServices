using System.Windows.Forms;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	[TestedType(typeof(ImportLicenseFromXMLForm))]
	public class ImportLicenseFromXMLFormTest : ZFormBasherTest
	{
		public void TestCaptions()
		{
			using (var form = GetFormToBashCore() as ImportLicenseFromXMLForm)
			{
				AssertEquals("Caption should be", "Load Import License(s)", form.FormCaption);
				AssertEquals("FileContentGroupBox should be", "Import License", form.FileContentGroupBox.CaptionResourceString.Caption);
			}
		}

		public void TestEntryHeaderGrid()
		{
			using (var form = GetFormToBashCore() as ImportLicenseFromXMLForm)
			{
				form.Show();
				CombineAssertions(() =>
				{
					AssertEquals(12, form.ImportLicensesGrid.Columns.Count);
					AssertEquals(ImportLicenseLoadingObject.Schema.ImportLicenseNo, form.ImportLicensesGrid.Columns[0].ColumnName);
					AssertEquals(ImportLicenseLoadingObject.Schema.RegistrationDate, form.ImportLicensesGrid.Columns[1].ColumnName);
					AssertEquals(ImportLicenseLoadingObject.Schema.InvoiceHeaderPK, form.ImportLicensesGrid.Columns[2].ColumnName);
					AssertEquals(ImportLicenseLoadingObject.Schema.ImportLicenseType, form.ImportLicensesGrid.Columns[3].ColumnName);
					AssertEquals(ImportLicenseLoadingObject.Schema.ImportLicenseAuthorizationDate, form.ImportLicensesGrid.Columns[4].ColumnName);
					AssertEquals(ImportLicenseLoadingObject.Schema.ImportLicenseFeeType, form.ImportLicensesGrid.Columns[5].ColumnName);
					AssertEquals(ImportLicenseLoadingObject.Schema.Incoterm, form.ImportLicensesGrid.Columns[6].ColumnName);
					AssertEquals(ImportLicenseLoadingObject.Schema.Currency, form.ImportLicensesGrid.Columns[7].ColumnName);
					AssertEquals(ImportLicenseLoadingObject.Schema.VMLE, form.ImportLicensesGrid.Columns[8].ColumnName);
					AssertEquals(ImportLicenseLoadingObject.Schema.VMCV, form.ImportLicensesGrid.Columns[9].ColumnName);
					AssertEquals(ImportLicenseLoadingObject.Schema.NetWeight, form.ImportLicensesGrid.Columns[10].ColumnName);
					AssertEquals(ImportLicenseLoadingObject.Schema.UQ, form.ImportLicensesGrid.Columns[11].ColumnName);
				});
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			var importLicenseParent = new ImportLicenseLoadingObjectParent(declaration);
			return new ImportLicenseFromXMLForm(importLicenseParent);
		}

		protected override bool ShouldIgnoreMissingBindingMember(Control control)
		{
			return control.Name == "FileNameTextBox";
		}

		#endregion
	}
}
