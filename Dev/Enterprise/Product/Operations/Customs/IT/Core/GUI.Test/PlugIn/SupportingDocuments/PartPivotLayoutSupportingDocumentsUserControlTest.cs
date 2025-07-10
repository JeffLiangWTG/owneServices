using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class PartPivotLayoutSupportingDocumentsUserControlTest : TestCaseWithFactory
{
	public void TestAvailableGridColumnStyles()
	{
		var orgSupplierPart = Factory.NewWithValidTestData<OrgSupplierPart>();
		var cusClassPart = orgSupplierPart.PivotsForBinding.AddNew();
		cusClassPart.CI_ChildType = "AAA";

		using (var form = new ZForm(orgSupplierPart))
		using (var control = new OrgSupplierPartFormCustomsControl())
		{
			form.Controls.Add(control);
			form.Show();
			control.Controls.Find("supportingDocsTabPage", true).First().Show();
			var supportingDocumentsDynamicUserControl = control.Controls.Find("SupportingDocumentsUserControl", true).First() as ZDynamicControlCreationUserControl;
			var supportingDocumentsUserControl = (PartPivotLayoutSupportingDocumentsUserControl)supportingDocumentsDynamicUserControl.HostedControl;

			var columnStyles = supportingDocumentsUserControl.SupportingDocumentsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Where(x => !x.IsUnavailable).ToArray();
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

	public void TestSupportingDocumentsFieldsControlBindingString()
	{
		using (var control = new PartPivotLayoutSupportingDocumentsUserControlForTest())
		{
			AssertEquals("PivotsForBinding.SupportingDocuments", control.GetSupportingDocumentsFieldsControlBindingStringExposed());
		}
	}

	class PartPivotLayoutSupportingDocumentsUserControlForTest : PartPivotLayoutSupportingDocumentsUserControl
	{
		public string GetSupportingDocumentsFieldsControlBindingStringExposed()
			=> base.GetSupportingDocumentsFieldsControlBindingString();
	}
}
