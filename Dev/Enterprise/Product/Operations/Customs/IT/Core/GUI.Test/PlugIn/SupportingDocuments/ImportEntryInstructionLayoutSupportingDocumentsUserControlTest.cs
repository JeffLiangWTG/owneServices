using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class ImportEntryInstructionLayoutSupportingDocumentsUserControlTest : TestCaseWithFactory
{
	public void TestSupportingDocumentsFieldsControl()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";

		using (var form = new ZForm(declaration))
		using (var userControl = new ImportEntryInstructionLayoutSupportingDocumentsUserControl())
		{
			form.Controls.Add(userControl);
			form.Show();

			AssertType<ImportEntryInstructionLayoutSupportingDocumentsFieldsControl>("SupportingDocumentsFieldsControl Type", userControl.SupportingDocumentsFieldsControl);
		}
	}

	public void TestAvailableColumnNames()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";

		using (var form = new ZForm(declaration))
		using (var userControl = new ImportEntryInstructionLayoutSupportingDocumentsUserControl())
		{
			form.Controls.Add(userControl);
			form.Show();

			var columnStyles = userControl.SupportingDocumentsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Where(x => !x.IsUnavailable).ToArray();
			ColumnStyleTestHelper.AssertColumn(columnStyles, SupportingDocument.Schema.CSI_Code, 0, typeof(ZCodeFindBoxColumnStyle), 100, CharacterCasing.Upper);
			ColumnStyleTestHelper.AssertColumn(columnStyles, SupportingDocument.Schema.CSI_ReferenceNumber, 1, typeof(ZMultiControlColumnStyle), 100, CharacterCasing.Normal);
			ColumnStyleTestHelper.AssertColumn(columnStyles, SupportingDocument.Schema.CSI_Quantity, 2, typeof(ZCalcEditColumnStyle), 100, CharacterCasing.Upper);
			ColumnStyleTestHelper.AssertColumn(columnStyles, SupportingDocument.Schema.CSI_UnitOfQuantity, 3, typeof(ZMultiControlColumnStyle), 100, CharacterCasing.Upper);
			ColumnStyleTestHelper.AssertColumn(columnStyles, SupportingDocument.Schema.CSI_YearOfIssue, 4, typeof(ZTextBoxColumnStyle), 100, CharacterCasing.Upper);
			ColumnStyleTestHelper.AssertColumn(columnStyles, SupportingDocument.Schema.CSI_RN_NKCountryCode, 5, typeof(ZCodeFindBoxColumnStyle), 100, CharacterCasing.Upper);
			ColumnStyleTestHelper.AssertColumn(columnStyles, SupportingDocument.Schema.CSI_DateOfExpiry, 6, typeof(ZDateEditColumnStyle), 100, CharacterCasing.Upper);
			ColumnStyleTestHelper.AssertColumn(columnStyles, SupportingDocument.Schema.CSI_ReferenceNumber2, 7, typeof(ZTextBoxColumnStyle), 100, CharacterCasing.Upper);
			ColumnStyleTestHelper.AssertColumn(columnStyles, SupportingDocument.Schema.CSI_Value, 8, typeof(ZCalcEditColumnStyle), 100, CharacterCasing.Upper);
			ColumnStyleTestHelper.AssertColumn(columnStyles, SupportingDocument.Schema.CSI_RX_NKCurrency, 9, typeof(ZCodeFindBoxColumnStyle), 100, CharacterCasing.Upper);
		}
	}
}
