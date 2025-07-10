using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class InvoiceLineLayoutSupportingDocumentsUserControlTest : TestCaseWithFactory
{
	public void TestAvailableGridColumnStyles()
	{
		using (var form = new ZForm(declaration))
		using (var supportingDocumentsUserControl = new InvoiceLineLayoutSupportingDocumentsUserControl())
		{
			supportingDocumentsUserControl.SetDataBinding(declaration, null);
			form.Controls.Add(supportingDocumentsUserControl);
			form.Show();

			var columnStyles = supportingDocumentsUserControl.SupportingDocumentsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Where(x => !x.IsUnavailable).ToArray();
			ColumnStyleTestHelper.AssertColumn(columnStyles, SupportingDocument.Schema.CSI_Code, 0, typeof(ZCodeFindBoxColumnStyle), 100, CharacterCasing.Upper);
			ColumnStyleTestHelper.AssertColumn(columnStyles, SupportingDocument.Schema.CSI_ReferenceNumber, 1, typeof(ZMultiControlColumnStyle), 100, CharacterCasing.Normal);
			ColumnStyleTestHelper.AssertColumn(columnStyles, SupportingDocument.Schema.CSI_Quantity, 2, typeof(ZCalcEditColumnStyle), 100, CharacterCasing.Upper);
			ColumnStyleTestHelper.AssertColumn(columnStyles, SupportingDocument.Schema.CSI_UnitOfQuantity, 3, typeof(ZMultiControlColumnStyle), 100, CharacterCasing.Upper);
			ColumnStyleTestHelper.AssertColumn(columnStyles, SupportingDocument.Schema.CSI_YearOfIssue, 4, typeof(ZTextBoxColumnStyle), 100, CharacterCasing.Upper);
			ColumnStyleTestHelper.AssertColumn(columnStyles, SupportingDocument.Schema.CSI_RN_NKCountryCode, 5, typeof(ZCodeFindBoxColumnStyle), 100, CharacterCasing.Upper);
			ColumnStyleTestHelper.AssertColumn(columnStyles, SupportingDocument.Schema.CSI_Status, 6, typeof(ZDropEditColumnStyle), 100, CharacterCasing.Upper);
		}
	}

	public void TestAvailableGridColumnStylesForImport()
	{
		declaration.JE_MessageType = "IMP";
		using (var form = new ZForm(declaration))
		using (var supportingDocumentsUserControl = new InvoiceLineLayoutSupportingDocumentsUserControl())
		{
			supportingDocumentsUserControl.SetDataBinding(declaration, null);
			form.Controls.Add(supportingDocumentsUserControl);
			form.Show();

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

	public void TestAvailableGridColumnStylesForExportUCC6()
	{
		declaration.JE_MessageType = "EXP";
		var testCases = new (string Column, Type StyleType, int MaxLength, CharacterCasing Casing)[]
		{
			(SupportingDocument.Schema.CSI_Code, typeof(ZCodeFindBoxColumnStyle), 100, CharacterCasing.Upper),
			(SupportingDocument.Schema.CSI_ReferenceNumber, typeof(ZMultiControlColumnStyle), 100, CharacterCasing.Normal),
			(SupportingDocument.Schema.CSI_YearOfIssue, typeof(ZTextBoxColumnStyle), 100, CharacterCasing.Upper),
			(SupportingDocument.Schema.CSI_RN_NKCountryCode, typeof(ZCodeFindBoxColumnStyle), 100, CharacterCasing.Upper),
			(SupportingDocument.Schema.CSI_ReferenceNumber2, typeof(ZTextBoxColumnStyle), 100, CharacterCasing.Upper),
			(SupportingDocument.Schema.CSI_LineNo, typeof(ZCalcEditColumnStyle), 100, CharacterCasing.Upper),
			(SupportingDocument.Schema.CSI_Quantity, typeof(ZCalcEditColumnStyle), 100, CharacterCasing.Upper),
			(SupportingDocument.Schema.CSI_UnitOfQuantity, typeof(ZMultiControlColumnStyle), 100, CharacterCasing.Upper),
			(SupportingDocument.Schema.CSI_Value, typeof(ZCalcEditColumnStyle), 100, CharacterCasing.Upper),
			(SupportingDocument.Schema.CSI_RX_NKCurrency, typeof(ZCodeFindBoxColumnStyle), 100, CharacterCasing.Upper),
			(SupportingDocument.Schema.CSI_DateOfExpiry, typeof(ZDateEditColumnStyle), 100, CharacterCasing.Upper)
		};

		using (var form = new ZForm(declaration))
		using (var supportingDocumentsUserControl = new InvoiceLineLayoutSupportingDocumentsUserControl())
		{
			using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				supportingDocumentsUserControl.SetDataBinding(declaration, null);
				form.Controls.Add(supportingDocumentsUserControl);
				form.Show();

				var columnStyles = supportingDocumentsUserControl.SupportingDocumentsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Where(x => !x.IsUnavailable).ToArray();

				for (int i = 0; i < testCases.Length; i++)
				{
					var (column, styleType, maxLength, casing) = testCases[i];
					ColumnStyleTestHelper.AssertColumn(columnStyles, column, i, styleType, maxLength, casing);
				}
			}
		}
	}

	public void TestSetSupportingDocumentContextMenuIsVisible()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.JobComInvoiceLines.AddNew();
		invoiceLine.SupportingDocuments.AddNew();

		AssertSetSupportingDocumentContextMenuIsVisible("IMP");
		AssertSetSupportingDocumentContextMenuIsVisible("EXP");

		void AssertSetSupportingDocumentContextMenuIsVisible(ZString messageType)
		{
			declaration.JE_MessageType = messageType;
			using (var form = new JobDeclarationForm(declaration))
			{
				CombineAssertions(() =>
				{
					form.Show();
					var customsBrokerageUserControl = form.CustomsBrokerageUserControl;

					customsBrokerageUserControl.MainTabControl.SelectedTab = customsBrokerageUserControl.InvoiceLinesTabPage;
					var supportingDocumentsTabPage = customsBrokerageUserControl.InvoiceLinesUserControl.LineDetailTabControl.Controls.Find("SupportingDocumentsTabPage", true).First() as ZTabPage;
					customsBrokerageUserControl.InvoiceLinesUserControl.LineDetailTabControl.SelectedTab = supportingDocumentsTabPage;
					var invoiceLineSupportingDocumentsGrid = supportingDocumentsTabPage.Controls.Find("SupportingDocumentsGrid", true).First() as ZGrid;
					var setSupportingDocumentMenuItem = GetSetSupportingDocumentMenuItemAndPerformMenuContextPopup(invoiceLineSupportingDocumentsGrid);
					AssertEquals($"When MessageType is: {messageType}, Set Supporting Document menu option should be available for SupportingDocumentsGrid in InvoiceLine Tab, setSupportingDocumentMenuItem.Visible", true, setSupportingDocumentMenuItem.Visible);

					if (messageType == "EXP")
					{
						customsBrokerageUserControl.MainTabControl.SelectedTab = customsBrokerageUserControl.MiscOptionsTabPage;
						var jobDeclarationSupportingDocumentsGrid = customsBrokerageUserControl.DynamicMiscOptions.Controls.Find("SupportingDocumentsGrid", true).First() as ZGrid;
						setSupportingDocumentMenuItem = GetSetSupportingDocumentMenuItemAndPerformMenuContextPopup(jobDeclarationSupportingDocumentsGrid);
						AssertNull("Set Supporting Document menu option should be not available for SupportingDocumentsGrid in JobDeclaration Tab, setSupportingDocumentMenuItem.Visible", setSupportingDocumentMenuItem);
					}
				});
			}
		}
	}

	MenuItem GetSetSupportingDocumentMenuItemAndPerformMenuContextPopup(ZGrid supportingDocumentsGrid)
	{
		var contextMenu = supportingDocumentsGrid.ContextMenu;
		contextMenu.DoPopup();
		return contextMenu.MenuItems.FindByText("Set Supporting Documents");
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
	}

	JobDeclaration declaration;
}
