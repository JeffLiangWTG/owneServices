using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class ExportEntryInstructionLayoutSupportingDocumentsUserControlTest
	: TestCaseWithFactory
{
	public void TestSupportingDocumentsFieldsControl()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";

		using (var form = new ZForm(declaration))
		using (var userControl = new ExportEntryInstructionLayoutSupportingDocumentsUserControl())
		{
			form.Controls.Add(userControl);
			form.Show();

			AssertType<ExportEntryInstructionLayoutSupportingDocumentsFieldsControl>("SupportingDocumentsFieldsControl Type", userControl.SupportingDocumentsFieldsControl);
		}
	}

	public void TestAvailableColumnNames()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";

		using (var form = new ZForm(declaration))
		using (var userControl = new ExportEntryInstructionLayoutSupportingDocumentsUserControl())
		{
			form.Controls.Add(userControl);
			form.Show();

			var columnStyles = userControl.SupportingDocumentsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Where(x => !x.IsUnavailable).ToArray();
			ColumnStyleTestHelper.AssertColumn(columnStyles, SupportingDocument.Schema.CSI_Code, 0, typeof(ZCodeFindBoxColumnStyle), 60, CharacterCasing.Upper);
			ColumnStyleTestHelper.AssertColumn(columnStyles, SupportingDocument.Schema.CSI_ReferenceNumber, 1, typeof(ZMultiControlColumnStyle), 140, CharacterCasing.Upper);
			ColumnStyleTestHelper.AssertColumn(columnStyles, SupportingDocument.Schema.CSI_YearOfIssue, 2, typeof(ZTextBoxColumnStyle), 100, CharacterCasing.Upper);
			ColumnStyleTestHelper.AssertColumn(columnStyles, SupportingDocument.Schema.CSI_RN_NKCountryCode, 3, typeof(ZCodeFindBoxColumnStyle), 80, CharacterCasing.Upper);
			ColumnStyleTestHelper.AssertColumn(columnStyles, SupportingDocument.Schema.CSI_ReferenceNumber2, 4, typeof(ZTextBoxColumnStyle), 140, CharacterCasing.Upper);
			ColumnStyleTestHelper.AssertColumn(columnStyles, SupportingDocument.Schema.CSI_DateOfExpiry, 5, typeof(ZDateEditColumnStyle), 80, CharacterCasing.Normal);
			ColumnStyleTestHelper.AssertColumn(columnStyles, SupportingDocument.Schema.CSI_LineNo, 6, typeof(ZCalcEditColumnStyle), 80, CharacterCasing.Normal);
		}
	}
}
