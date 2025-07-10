using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.IT.GUI.Testing;
using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

sealed class NctsSupportingDocumentsUserControlTest : TestCaseWithFactory
{
	public void TestSupDocReasonTextBoxNotVisible()
	{
		using (var control = new NctsSupportingDocumentsUserControl())
		{
			var supDocReasonTextBox = control.FindSingle<ZTextBox>("SupDocReasonTextBox");
			AssertEquals(nameof(supDocReasonTextBox.Visible), false, supDocReasonTextBox.Visible);
		}
	}

	public void TestChildControlsProperties()
	{
		using (var control = new NctsSupportingDocumentsUserControl())
		{
			CombineAssertions(() =>
			{
				AssertEquals("SupDocTypeFindBox: CharacterCasing", CharacterCasing.Upper, control.FindSingleOrDefault<ZCodeFindBox>("SupDocTypeFindBox").CodeBox.CharacterCasing);
				AssertEquals("SupDocReferenceTextBox: CharacterCasing", CharacterCasing.Normal, control.FindSingleOrDefault<ZTextBox>("SupDocReferenceTextBox").CharacterCasing);
				AssertEquals("CSI_Status: CharacterCasing", CharacterCasing.Upper, control.FindSingleOrDefault<ZDropEdit>("CSI_StatusDropEdit").CharacterCasing);
				AssertEquals("CSI_UnitOfQuantity: CharacterCasing", CharacterCasing.Upper, control.FindSingleOrDefault<ZTextBox>("CSI_UnitOfQuantityTextBox").CharacterCasing);
				AssertEquals("CSI_YearOfIssue: CharacterCasing", CharacterCasing.Upper, control.FindSingleOrDefault<ZTextBox>("CSI_YearOfIssueTextBox").CharacterCasing);
				AssertEquals("CSI_RN_NKCountryCode: CharacterCasing", CharacterCasing.Upper, control.FindSingleOrDefault<ZCodeFindBox>("CSI_RN_NKCountryCodeCodeFindBox").CodeBox.CharacterCasing);
				AssertEquals("CSI_Quantity: Decimals", 5, control.FindSingleOrDefault<ZCalcEdit>("CSI_QuantityCalcEdit").Decimals);
			});
		}
	}

	public void TestGridColumns()
	{
		using (var control = new NctsSupportingDocumentsUserControl())
		{
			var grid = control.FindSingle<ZGrid>("SupportingDocumentsGrid");
			AssertNotNull("Grid found by name = SupportingDocumentsGrid", grid);

			CombineAssertions("POST-CONDITION", () =>
			{
				var columnStyles = grid.ColumnStyles.Cast<ZGridColumnInfo>().Where(x => !x.IsUnavailable).ToArray();
				AssertEquals("ColumnStyles.Length", 7, columnStyles.Length);
				ColumnStyleTestHelper.AssertColumn(columnStyles, NctsSupportingDocument.Schema.CSI_Code, 0, typeof(ZCodeFindBoxColumnStyle), 100, CharacterCasing.Upper);
				ColumnStyleTestHelper.AssertColumn(columnStyles, NctsSupportingDocument.Schema.CSI_ReferenceNumber, 1, typeof(ZTextBoxColumnStyle), 100, CharacterCasing.Normal);
				ColumnStyleTestHelper.AssertColumn(columnStyles, NctsSupportingDocument.Schema.CSI_Status, 2, typeof(ZDropEditColumnStyle), 100, CharacterCasing.Upper);
				ColumnStyleTestHelper.AssertColumn(columnStyles, NctsSupportingDocument.Schema.CSI_Quantity, 3, typeof(ZCalcEditColumnStyle), 100, CharacterCasing.Upper);
				ColumnStyleTestHelper.AssertColumn(columnStyles, NctsSupportingDocument.Schema.CSI_UnitOfQuantity, 4, typeof(ZTextBoxColumnStyle), 100, CharacterCasing.Upper);
				ColumnStyleTestHelper.AssertColumn(columnStyles, NctsSupportingDocument.Schema.CSI_YearOfIssue, 5, typeof(ZTextBoxColumnStyle), 100, CharacterCasing.Upper);
				ColumnStyleTestHelper.AssertColumn(columnStyles, NctsSupportingDocument.Schema.CSI_RN_NKCountryCode, 6, typeof(ZCodeFindBoxColumnStyle), 100, CharacterCasing.Upper);
			});
		}
	}
}
