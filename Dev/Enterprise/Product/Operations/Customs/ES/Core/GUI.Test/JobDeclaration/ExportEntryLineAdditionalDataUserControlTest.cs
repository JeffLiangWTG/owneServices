using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI.Test;

class ExportEntryLineAdditionalDataUserControlTest : TestCaseWithFactory
{
	public void TestEntryLinePreviousDocumentsGrid()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.ActiveEntryHeaders.AddNew();

		using (var form = new ZForm(declaration))
		using (var userControl = new ExportEntryLineAdditionalDataUserControl())
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
			});
		}
	}

	public void TestSetUpEntryLineSupportingDocumentsGridColumns()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.ActiveEntryHeaders.AddNew();

		using (var form = new ZForm(declaration))
		using (var userControl = new ExportEntryLineAdditionalDataUserControl())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
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
				TestHelper.AssertColumnStyle<ZTextBoxColumnStyleInfo>("Issuing Authority", supportingDocsGrid, "CSI_AdditionalDescription");
				TestHelper.AssertColumnStyle<ZCalcEditColumnStyleInfo>("Line No.", supportingDocsGrid, "CSI_ItemNumber");
				TestHelper.AssertColumnStyle<ZCheckBoxColumnStyleInfo>("Header", supportingDocsGrid, "IsDocumentHeader");
			});
		}
	}

	public void TestEntryLineAdditionalInfosGrid()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.ActiveEntryHeaders.AddNew();

		using (var form = new ZForm(declaration))
		using (var userControl = new ExportEntryLineAdditionalDataUserControl())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();
			var addInfosTabPage = (ZTabPage)userControl.Controls.Find("AdditionalInfosTabPage", true).SingleOrDefault();
			var addInfosGrid = (ZGrid)userControl.Controls.Find("EntryLineAdditionalInfosGrid", true).SingleOrDefault();

			CombineAssertions(() =>
			{
				AssertNotNull("Additional Infos TabPage", addInfosTabPage);
				AssertEquals("Additional Infos TabPage Caption", "Additional Documents", addInfosTabPage.CaptionResourceString.Caption);
				TestHelper.AssertColumnStyle<ZTextBoxColumnStyleInfo>("Full Type", addInfosGrid, "CSI_Code");
				TestHelper.AssertColumnStyle<ZTextBoxColumnStyleInfo>("Description", addInfosGrid, "CSI_Description");
				TestHelper.AssertColumnStyle<ZTextBoxColumnStyleInfo>("Kind", addInfosGrid, "CSI_SubType");
				TestHelper.AssertColumnStyle<ZTextBoxColumnStyleInfo>("Reference", addInfosGrid, "CSI_ReferenceNumber");
				TestHelper.AssertColumnStyle<ZTextBoxColumnStyleInfo>("Detail", addInfosGrid, "CSI_ReferenceNumber2");
				TestHelper.AssertColumnStyle<ZTextBoxColumnStyleInfo>("Currency", addInfosGrid, "CSI_RX_NKCurrency");
				TestHelper.AssertColumnStyle<ZCalcEditColumnStyleInfo>("Amount", addInfosGrid, "CSI_Value");
				TestHelper.AssertColumnStyle<ZTextBoxColumnStyleInfo>("Status", addInfosGrid, "CSI_Status");
			});
		}
	}
}
