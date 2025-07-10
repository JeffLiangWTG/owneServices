using System.Linq;
using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.Customs.IT.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class GroupedPreviousDocumentsUserControlTest : TestCase
{
	public void TestM2LinesGrid()
	{
		using (var userControl = new GroupedPreviousDocumentsUserControl())
		{
			var m2LinesGrid = userControl.FindSingleOrDefault<ZGrid>("M2LinesGrid");
			AssertNotNull(nameof(m2LinesGrid), m2LinesGrid);
			AssertEquals($"{nameof(m2LinesGrid)} visibility", true, m2LinesGrid.Visible);

			var columnStyles = m2LinesGrid.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();
			AssertEquals("Number of column styles", 22, columnStyles.Length);

			ColumnStyleTestHelper.AssertColumn(columnStyles, GroupedPreviousDocument.Schema.EntryLineNumber, 0, typeof(ZTextBoxColumnStyle), 60, CharacterCasing.Normal);
			ColumnStyleTestHelper.AssertColumn(columnStyles, GroupedPreviousDocument.Schema.EntryLineCustomsStatusDescription, 1, typeof(ZTextBoxColumnStyle), 70, CharacterCasing.Normal);
			ColumnStyleTestHelper.AssertColumn(columnStyles, GroupedPreviousDocument.Schema.SummaryDeclarationDocumentRegister, 2, typeof(ZTextBoxColumnStyle), 35, CharacterCasing.Normal);
			ColumnStyleTestHelper.AssertColumn(columnStyles, GroupedPreviousDocument.Schema.SummaryDeclarationDocumentReferenceNumber, 3, typeof(ZTextBoxColumnStyle), 78, CharacterCasing.Normal);
			ColumnStyleTestHelper.AssertColumn(columnStyles, GroupedPreviousDocument.Schema.SummaryDeclarationDocumentReferenceCIN, 4, typeof(ZTextBoxColumnStyle), 45, CharacterCasing.Normal);
			ColumnStyleTestHelper.AssertColumn(columnStyles, GroupedPreviousDocument.Schema.SummaryDeclarationDocumentDate, 5, typeof(ZDateEditColumnStyle), 78, CharacterCasing.Normal);
			ColumnStyleTestHelper.AssertColumn(columnStyles, GroupedPreviousDocument.Schema.SummaryDeclarationDocumentSeries, 6, typeof(ZTextBoxColumnStyle), 60, CharacterCasing.Normal);
			ColumnStyleTestHelper.AssertColumn(columnStyles, GroupedPreviousDocument.Schema.SummaryDeclarationDocumentCustomsOffice, 7, typeof(ZTextBoxColumnStyle), 65, CharacterCasing.Normal);
			ColumnStyleTestHelper.AssertColumn(columnStyles, GroupedPreviousDocument.Schema.SummaryDeclarationDocumentMRN, 8, typeof(ZTextBoxColumnStyle), 78, CharacterCasing.Normal);
			ColumnStyleTestHelper.AssertColumn(columnStyles, GroupedPreviousDocument.Schema.SummaryDeclarationDocumentItemNumber, 9, typeof(ZTextBoxColumnStyle), 95, CharacterCasing.Normal);
			ColumnStyleTestHelper.AssertColumn(columnStyles, GroupedPreviousDocument.Schema.PreviousProcedureDocumentRegister, 10, typeof(ZTextBoxColumnStyle), 35, CharacterCasing.Normal);
			ColumnStyleTestHelper.AssertColumn(columnStyles, GroupedPreviousDocument.Schema.PreviousProcedureDocumentReferenceNumber, 11, typeof(ZTextBoxColumnStyle), 78, CharacterCasing.Normal);
			ColumnStyleTestHelper.AssertColumn(columnStyles, GroupedPreviousDocument.Schema.PreviousProcedureDocumentReferenceCIN, 12, typeof(ZTextBoxColumnStyle), 45, CharacterCasing.Normal);
			ColumnStyleTestHelper.AssertColumn(columnStyles, GroupedPreviousDocument.Schema.PreviousProcedureDocumentDate, 13, typeof(ZDateEditColumnStyle), 78, CharacterCasing.Normal);
			ColumnStyleTestHelper.AssertColumn(columnStyles, GroupedPreviousDocument.Schema.PreviousProcedureDocumentSeries, 14, typeof(ZTextBoxColumnStyle), 60, CharacterCasing.Normal);
			ColumnStyleTestHelper.AssertColumn(columnStyles, GroupedPreviousDocument.Schema.PreviousProcedureDocumentCustomsOffice, 15, typeof(ZTextBoxColumnStyle), 65, CharacterCasing.Normal);
			ColumnStyleTestHelper.AssertColumn(columnStyles, GroupedPreviousDocument.Schema.PreviousProcedureDocumentItemNumber, 16, typeof(ZTextBoxColumnStyle), 95, CharacterCasing.Normal);
			ColumnStyleTestHelper.AssertColumn(columnStyles, GroupedPreviousDocument.Schema.PackageQuantity, 17, typeof(ZTextBoxColumnStyle), 78, CharacterCasing.Normal);
			ColumnStyleTestHelper.AssertColumn(columnStyles, GroupedPreviousDocument.Schema.GrossMass, 18, typeof(ZTextBoxColumnStyle), 78, CharacterCasing.Normal);
			ColumnStyleTestHelper.AssertColumn(columnStyles, GroupedPreviousDocument.Schema.Tariff, 19, typeof(ZTextBoxColumnStyle), 78, CharacterCasing.Normal);
			ColumnStyleTestHelper.AssertColumn(columnStyles, GroupedPreviousDocument.Schema.NetMass, 20, typeof(ZTextBoxColumnStyle), 78, CharacterCasing.Normal);
			ColumnStyleTestHelper.AssertColumn(columnStyles, GroupedPreviousDocument.Schema.SupplementaryQuantity, 21, typeof(ZTextBoxColumnStyle), 78, CharacterCasing.Normal);
		}
	}
}
