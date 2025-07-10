using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.GUI.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using CusEntryLine = Enterprise.Customs.ES.Business.Declaration.CusEntryLine;

namespace Enterprise.Customs.ES.GUI.Test;

sealed class ImportEntryLineAdditionalDataUserControlTest : TestCaseWithFactory
{
	public void TestDutyAndTaxDetailsUserControlType()
	{
		using (var form = new Form())
		using (var entryLineAdditionalDataUserControl = new ImportEntryLineAdditionalDataUserControl())
		{
			form.Controls.Add(entryLineAdditionalDataUserControl);
			form.Show();
			AssertEquals("Using Correct EntryLineTaxAndFeeUserControl", typeof(ImportEntryLineTaxAndFeeUserControl), entryLineAdditionalDataUserControl.DutyAndTaxDetails.UserControlType);
		}
	}

	public void TestEntryLinePreviousDocumentsGrid()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.ActiveEntryHeaders.AddNew();

		using (var form = new ZForm(declaration))
		using (var userControl = new ImportEntryLineAdditionalDataUserControl())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();
			var previousDocsTabPage = (ZTabPage)userControl.Controls.Find("PreviousDocumentsTabPage", true).SingleOrDefault();
			var previousDocsGrid = (ZGrid)userControl.Controls.Find("EntryLinePreviousDocumentsGrid", true).SingleOrDefault();

			CombineAssertions(() =>
			{
				AssertNotNull("Previous Documents TabPage", previousDocsTabPage);
				AssertEquals("Previous Documents TabPage Caption", "[40] Previous Documents", previousDocsTabPage.CaptionResourceString.Caption);
				TestHelper.AssertColumnStyle<ZTextBoxColumnStyleInfo>("Type", previousDocsGrid, "CSI_Code");
				TestHelper.AssertColumnStyle<ZTextBoxColumnStyleInfo>("Class", previousDocsGrid, "CSI_SubType");
				TestHelper.AssertColumnStyle<ZTextBoxColumnStyleInfo>("Reference", previousDocsGrid, "CSI_ReferenceNumber");
				TestHelper.AssertColumnStyle<ZDateEditColumnStyleInfo>("Date of Issue", previousDocsGrid, "CSI_DateOfIssue");
				TestHelper.AssertColumnStyle<ZCalcEditColumnStyleInfo>("Line No.", previousDocsGrid, "CSI_LineNo");
				TestHelper.AssertColumnStyle<ZTextBoxColumnStyleInfo>("Unit Of Quantity", previousDocsGrid, "CSI_UnitOfQuantity");
				TestHelper.AssertColumnStyle<ZCalcEditColumnStyleInfo>("Quantity", previousDocsGrid, "CSI_Quantity");
				TestHelper.AssertColumnStyle<ZTextBoxColumnStyleInfo>("Status", previousDocsGrid, "CSI_Status");
				TestHelper.AssertColumnStyle<ZCalcEditColumnStyleInfo>("Number of Packages", previousDocsGrid, "CSI_PackQty");
				TestHelper.AssertColumnStyle<ZTextBoxColumnStyleInfo>("Type of Packages", previousDocsGrid, "CSI_PackType");
			});
		}
	}

	public void TestPackColumnsVisibility()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var entryHeader = declaration.ActiveEntryHeaders.AddNew();

		var entryLine = Factory.New<CusEntryLine>();
		entryLine.CL_CH = entryHeader.PK;
		entryHeader.AllEntryLines.Add(entryLine);

		var line = entryLine.InvoiceLines.AddNew();
		line.JI_CEI = entryInstruction.PK;

		using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6())
		using (var form = new ZForm(declaration))
		using (var control = new ImportEntryLineAdditionalDataUserControl())
		{
			form.Controls.Add(control);
			form.SetDataBinding(declaration, ".");
			form.Show();
			var previousDocumentsGrid = (ZGrid)control.Controls.Find("EntryLinePreviousDocumentsGrid", true).SingleOrDefault();
			previousDocumentsGrid.SetDataBinding(entryLine.ReadOnlyPreviousDocuments, "");
			CombineAssertions(() =>
			{
				AssertNotNull("Import declaration, CSI_PackQty should be visible when declaration is Import H1.", previousDocumentsGrid.Columns[ReadOnlyPreviousDocument.Schema.CSI_PackQty]);
				AssertNotNull("Import declaration, CSI_PackType should be visible when declaration is Import H1.", previousDocumentsGrid.Columns[ReadOnlyPreviousDocument.Schema.CSI_PackType]);
			});
			var columns = previousDocumentsGrid.ColumnStyles.ToArray().Cast<ZGridColumnInfo>().Where(x => !x.IsUnavailable);
		}

		using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(false))
		using (var form = new ZForm(declaration))
		using (var control = new ImportEntryLineAdditionalDataUserControl())
		{
			form.Controls.Add(control);
			form.SetDataBinding(declaration, ".");
			form.Show();
			var previousDocumentsGrid = (ZGrid)control.Controls.Find("EntryLinePreviousDocumentsGrid", true).SingleOrDefault();
			previousDocumentsGrid.SetDataBinding(entryLine.ReadOnlyPreviousDocuments, "");
			CombineAssertions(() =>
			{
				AssertNull("Import declaration, CSI_PackQty should be not visible when declaration is not H1.", previousDocumentsGrid.Columns[ReadOnlyPreviousDocument.Schema.CSI_PackQty]);
				AssertNull("Import declaration, CSI_PackType should be not visible when declaration is not H1.", previousDocumentsGrid.Columns[ReadOnlyPreviousDocument.Schema.CSI_PackType]);
			});
			var columns = previousDocumentsGrid.ColumnStyles.ToArray().Cast<ZGridColumnInfo>().Where(x => !x.IsUnavailable);
		}
	}

	public void TestSetUpEntryLineSupportingDocumentsGridColumns()
	{
		var testDec = Factory.New<JobDeclaration>();
		testDec.JE_MessageType = MessageTypeList.Codes.Import;

		using (var form = new ZForm(testDec))
		using (var userControl = new ImportEntryLineAdditionalDataUserControl())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(testDec, ".");
			form.Show();
			var supportingDocsGrid = (ZGrid)userControl.Controls.Find("EntryLineSupportingDocumentsGrid", true).SingleOrDefault();

			CombineAssertions(() =>
			{
				TestHelper.AssertColumnStyle<ZTextBoxColumnStyleInfo>("Type", supportingDocsGrid, "CSI_Code");
				TestHelper.AssertColumnStyle<ZTextBoxColumnStyleInfo>("Reference", supportingDocsGrid, "CSI_ReferenceNumber");
				TestHelper.AssertColumnStyle<ZTextBoxColumnStyleInfo>("Status", supportingDocsGrid, "CSI_Status");
				TestHelper.AssertColumnStyle<ZCalcEditColumnStyleInfo>("Quantity", supportingDocsGrid, "CSI_Quantity");
				TestHelper.AssertColumnStyle<ZTextBoxColumnStyleInfo>("Unit Of Quantity", supportingDocsGrid, "CSI_UnitOfQuantity");
				TestHelper.AssertColumnStyle<ZCalcEditColumnStyleInfo>("2nd/Estimated Quantity", supportingDocsGrid, "CSI_Quantity2");
				TestHelper.AssertColumnStyle<ZTextBoxColumnStyleInfo>("2nd/Estimated UQ", supportingDocsGrid, "CSI_UnitOfQuantity2");
				TestHelper.AssertColumnStyle<ZCalcEditColumnStyleInfo>("Value", supportingDocsGrid, "CSI_Value");
				TestHelper.AssertColumnStyle<ZTextBoxColumnStyleInfo>("Currency", supportingDocsGrid, "CSI_RX_NKCurrency");
				TestHelper.AssertColumnStyle<ZDateEditColumnStyleInfo>("Date of Issue", supportingDocsGrid, "CSI_DateOfIssue");
				TestHelper.AssertColumnStyle<ZDateEditColumnStyleInfo>("Date of Expiry", supportingDocsGrid, "CSI_DateOfExpiry");
				TestHelper.AssertColumnStyle<ZTextBoxColumnStyleInfo>("Procedure (DJP)", supportingDocsGrid, "CSI_Procedure");
				TestHelper.AssertColumnStyle<ZCheckBoxColumnStyleInfo>("Header", supportingDocsGrid, "IsDocumentHeader");
			});
		}
	}
}
