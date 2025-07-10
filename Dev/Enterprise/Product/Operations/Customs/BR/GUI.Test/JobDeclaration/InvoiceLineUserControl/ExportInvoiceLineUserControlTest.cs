using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI.Testing;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.BR.Registry;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BR.GUI.Testing
{
	sealed class ExportInvoiceLineUserControlTest : BaseInvoiceLineUserControlAbstractTest
	{
		public void TestColumnLayoutContextForInvoiceLinesGrid()
		{
			using (var control = new ExportInvoiceLineUserControl())
			{
				AssertEquals("Column layout context should be export", nameof(Customs.GUI.DeclarationType.Export), control.CustomsInvoiceLinesBoundGrid.ColumnLayoutContext);
			}
		}

		public void TestTabPagesForExport()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclartion.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			jobDeclartion.Invoices.AddNew().InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(jobDeclartion))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoiceLineUserControl = ((ExportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl);

				invoiceLineUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.LPCOTab;
				Assert(invoiceLineUserControl.LPCOConcatenatedTextBox.Visible);
				Assert(invoiceLineUserControl.LPCOEditButton.Visible);

				invoiceLineUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.SuspensionDrawbackTab;
				Assert(invoiceLineUserControl.SuspensionDrawbackUserControl.Visible);

				invoiceLineUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.BRPreviousDocumentTabPage;
				Assert(invoiceLineUserControl.BRPreviousDocumentUserControl.Visible);

				invoiceLineUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.BRReferenceInvoiceManualTabPage;
				Assert(invoiceLineUserControl.ReferenceInvoiceManualUserControl.Visible);

				invoiceLineUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.BRElectronicLogisticInvoiceTabPage;
				Assert(invoiceLineUserControl.BRElectronicLogisticInvoiceUserControl.Visible);

				invoiceLineUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.BRComplementaryLogisticInvoiceTabPage;
				Assert(invoiceLineUserControl.BRComplementaryLogisticInvoiceUserControl.Visible);

				invoiceLineUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.AttributesTab;
				Assert(invoiceLineUserControl.AttributesUserControl.Visible);

				invoiceLineUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.BRAdditionalInformationTab;
				Assert(invoiceLineUserControl.BRAdditionalInformationUserControl.Visible);

				invoiceLineUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.CertificateOfOriginTabPage;
				Assert(invoiceLineUserControl.CertificateOfOriginUserControl.Visible);

				Assert(invoiceLineUserControl.InvoiceLinesSummaryGroupBox.Visible);
			}
		}

		public void TestTabPagesForLPCO()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclartion.JE_MessageType = BRJobMessageTypeList.Codes.LPCO;
			jobDeclartion.Invoices.AddNew().InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(jobDeclartion))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoiceLineUserControl = ((ExportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl);

				invoiceLineUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.LPCOTab;
				Assert(invoiceLineUserControl.LPCOConcatenatedTextBox.Visible);
				Assert(invoiceLineUserControl.LPCOEditButton.Visible);

				invoiceLineUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.SuspensionDrawbackTab;
				Assert(invoiceLineUserControl.SuspensionDrawbackUserControl.Visible);

				invoiceLineUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.BRPreviousDocumentTabPage;
				Assert(invoiceLineUserControl.BRPreviousDocumentUserControl.Visible);

				invoiceLineUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.BRReferenceInvoiceManualTabPage;
				Assert(invoiceLineUserControl.ReferenceInvoiceManualUserControl.Visible);

				invoiceLineUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.BRElectronicLogisticInvoiceTabPage;
				Assert(invoiceLineUserControl.BRElectronicLogisticInvoiceUserControl.Visible);

				invoiceLineUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.BRComplementaryLogisticInvoiceTabPage;
				Assert(invoiceLineUserControl.BRComplementaryLogisticInvoiceUserControl.Visible);

				invoiceLineUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.AttributesTab;
				Assert(invoiceLineUserControl.AttributesUserControl.Visible);

				invoiceLineUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.BRAdditionalInformationTab;
				Assert(invoiceLineUserControl.BRAdditionalInformationUserControl.Visible);

				invoiceLineUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.CertificateOfOriginTabPage;
				Assert(invoiceLineUserControl.CertificateOfOriginUserControl.Visible);

				Assert(!invoiceLineUserControl.InvoiceLinesSummaryGroupBox.Visible);
			}
		}

		public void TestDefaultColumns()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclartion.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			var header = jobDeclartion.Invoices.AddNew();
			var line = header.InvoiceLines.AddNew();

			using (var form = new JobDeclarationForm(jobDeclartion))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				jobDeclartion.JE_MessageType = BRJobMessageTypeList.Codes.Export;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var invoiceLineUserControl = (ExportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl)
				{
					var visibleColumnCount = invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns.Count(x => x.IsVisible);
					AssertEquals(41, visibleColumnCount);

					var customsInvoiceLinesBoundGrid = invoiceLineUserControl.CustomsInvoiceLinesBoundGrid;
					customsInvoiceLinesBoundGrid.ResetColumns();

					Assert("Should have JI_LineNo column in CustomsInvoiceLinesBoundGrid", customsInvoiceLinesBoundGrid.Columns.Contains(JobComInvoiceLine.Schema.JI_LineNo));
					Assert("Should have JI_LinePrice column in CustomsInvoiceLinesBoundGrid", customsInvoiceLinesBoundGrid.Columns.Contains(JobComInvoiceLine.Schema.JI_LinePrice));

					Assert("Should have JI_RN_NKCountryOfExport column in CustomsInvoiceLinesBoundGrid", customsInvoiceLinesBoundGrid.Columns.Contains(JobComInvoiceLine.Schema.JI_RN_NKCountryOfExport));
					AssertEquals("JI_RN_NKCountryOfExport caption", "Destination", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.JI_RN_NKCountryOfExport].ColumnStyle.HeaderText);
					AssertEquals("JI_SerialNumber column is present when using schema redesign registry setting", true, customsInvoiceLinesBoundGrid.Columns.Contains(JobComInvoiceLine.Schema.JI_SerialNumber));
					AssertEquals("JI_CargoPriority should be hidden", false, customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.JI_CargoPriority].IsVisible);
					AssertEquals("ComplementaryDescription should be hidden", false, customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.ComplementaryDescription].IsVisible);
					AssertEquals("JI_ExportJustificationInfo should be hidden", false, customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.JI_ExportJustificationInfo].IsVisible);
					AssertEquals("UnitPrice should be hidden", false, customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.UnitPrice].IsVisible);
					AssertEquals("JI_ThirdCPC should be hidden", false, customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.JI_ThirdCPC].IsVisible);
					AssertEquals("JI_ThirdCPC caption", "Third CPC", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.JI_ThirdCPC].ColumnStyle.HeaderText);
					AssertEquals("JI_FourthCPC should be hidden", false, customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.JI_FourthCPC].IsVisible);
					AssertEquals("JI_FourthCPC caption", "Fourth CPC", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.JI_FourthCPC].ColumnStyle.HeaderText);
					AssertEquals("JI_IntendedTermDays should be hidden", false, invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.JI_IntendedTermDays].IsVisible);
				}
			}
		}

		public void TestCustomsInvoiceLinesBoundGridColumnsTypes()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclartion.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			jobDeclartion.Invoices.AddNew().InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(jobDeclartion))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				jobDeclartion.JE_MessageType = BRJobMessageTypeList.Codes.Export;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoiceLineUserControl = (ExportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				var grid = invoiceLineUserControl.CustomsInvoiceLinesBoundGrid;
				AssertType<ZMultiLineTextBoxColumnInfo>(grid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_CustomTextBlob1));
				AssertType<ZCodeFindBoxColumnStyleInfo>(grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_RN_NKCountryOfExport));
				AssertType<ZDropEditColumnStyleInfo>(grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_Procedure));
				AssertType<ZDropEditColumnStyleInfo>(grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_SecondCPC));
				AssertType<ZDropEditColumnStyleInfo>(grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_ThirdCPC));
				AssertType<ZDropEditColumnStyleInfo>(grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_FourthCPC));
				AssertType<ZTextBoxColumnStyleInfo>(grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_NFeNumber));
				AssertType<ZTextBoxColumnStyleInfo>(grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_NFeItemNumber));
				AssertType<ZGuidDropEditColumnStyleInfo>(grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CEI));
				AssertType<ZCalcEditColumnStyleInfo>(grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_AgentCommissionPercentage));
				AssertType<ZCalcEditColumnStyleInfo>(grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_NFeLinePrice));
				AssertType<ZTextBoxColumnStyleInfo>(grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_IntendedTermDays));
			}
		}

		public void TestDefaultColumnWidths()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclartion.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			var header = jobDeclartion.Invoices.AddNew();
			var line = header.InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(jobDeclartion))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				jobDeclartion.JE_MessageType = BRJobMessageTypeList.Codes.Export;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var invoiceLineUserControl = (ExportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl)
				{
					var grid = invoiceLineUserControl.CustomsInvoiceLinesBoundGrid;
					grid.ResetColumns();
					AssertEquals("JobComInvoiceLine.Schema.JI_Procedure", 80, grid.GetColumnWidth(JobComInvoiceLine.Schema.JI_Procedure));
					AssertEquals("JobComInvoiceLine.Schema.JI_RN_NKCountryOfExport", 90, grid.GetColumnWidth(JobComInvoiceLine.Schema.JI_RN_NKCountryOfExport));
					AssertEquals("JobComInvoiceLine.Schema.JI_FinancedValue", 90, grid.GetColumnWidth(JobComInvoiceLine.Schema.JI_FinancedValue));
					AssertEquals("JobComInvoiceLine.Schema.BR_CPCSecond", 80, grid.GetColumnWidth(JobComInvoiceLine.Schema.JI_SecondCPC));
					AssertEquals("JobComInvoiceLine.Schema.BR_CPCThird", 80, grid.GetColumnWidth(JobComInvoiceLine.Schema.JI_ThirdCPC));
					AssertEquals("JobComInvoiceLine.Schema.BR_CPCFourth", 80, grid.GetColumnWidth(JobComInvoiceLine.Schema.JI_FourthCPC));
					AssertEquals("JobComInvoiceLine.Schema.BR_NFENumber", 280, grid.GetColumnWidth(JobComInvoiceLine.Schema.JI_NFeNumber));
					AssertEquals("JobComInvoiceLine.Schema.BR_NFEItemNumber", 80, grid.GetColumnWidth(JobComInvoiceLine.Schema.JI_NFeItemNumber));
					AssertEquals("JobComInvoiceLine.Schema.JI_CargoPriority", 80, grid.GetColumnWidth(JobComInvoiceLine.Schema.JI_CargoPriority));
					AssertEquals("JobComInvoiceLine.Schema.ComplementaryDescriptionExport", 80, grid.GetColumnWidth(JobComInvoiceLine.Schema.ComplementaryDescription));
					AssertEquals("JobComInvoiceLine.Schema.BR_JustificationInfoExport", 90, grid.GetColumnWidth(JobComInvoiceLine.Schema.JI_ExportJustificationInfo));
					AssertEquals("JobComInvoiceLine.Schema.JI_CEI", 80, grid.GetColumnWidth(JobComInvoiceLine.Schema.JI_CEI));
					AssertEquals("JobComInvoiceLine.Schema.BR_AgentCommission", 90, grid.GetColumnWidth(JobComInvoiceLine.Schema.JI_AgentCommissionPercentage));
					AssertEquals("JobComInvoiceLine.Schema.JI_NFeLinePrice", 120, grid.GetColumnWidth(JobComInvoiceLine.Schema.JI_NFeLinePrice));
					AssertEquals("JobComInvoiceLine.Schema.JI_IntendedTermDays", 80, grid.GetColumnWidth(JobComInvoiceLine.Schema.JI_IntendedTermDays));
				}
			}
		}

		public void TestCommercialInvoiceTabVisibility()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			using (var form = new CommercialInvoiceForm(invoice))
			{
				form.Show();
				form.MainTabControl.SelectedTab = form.LinesTabPage;
				var invoiceLineUserControl = (ExportInvoiceLineUserControl)form.InvoiceLineUserControl;
				AssertEquals(true, invoiceLineUserControl.LPCOTab.TabVisible);
				AssertEquals(false, invoiceLineUserControl.SuspensionDrawbackTab.TabVisible);
				AssertEquals(false, invoiceLineUserControl.BRPreviousDocumentTabPage.TabVisible);
				AssertEquals(false, invoiceLineUserControl.BRReferenceInvoiceManualTabPage.TabVisible);
				AssertEquals(false, invoiceLineUserControl.BRElectronicLogisticInvoiceTabPage.TabVisible);
			}
		}

		public void TestCreatePermitMenu()
		{
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			declaration.Invoices.AddNew().InvoiceLines.AddNew();

			using (BRCustomsDataRegistry.Instance.EnableLPCO.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoiceLineUserControl = (ExportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;

				var grid = invoiceLineUserControl.CustomsInvoiceLinesBoundGrid;
				grid.Select(0);

				var lpcoMenu = grid.ContextMenu.MenuItems.FindByText("Create Permit");

				grid.ContextMenu.OnPopup_ForTest();
				AssertNull("Create Permit Menu Item should Not be created", lpcoMenu);
			}

			using (BRCustomsDataRegistry.Instance.EnableLPCO.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoiceLineUserControl = (ExportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;

				var grid = invoiceLineUserControl.CustomsInvoiceLinesBoundGrid;
				var lpcoMenu = grid.ContextMenu.MenuItems.FindByText("Create Permit");
				grid.ContextMenu.OnPopup_ForTest();
				AssertEquals("Create Permit Menu Item should be disable when no element selected", false, lpcoMenu.Enabled);

				grid.SelectAllElements();
				grid.ContextMenu.OnPopup_ForTest();
				AssertEquals("Create Permit Menu Item should be disable when more than one elements selected", false, lpcoMenu.Enabled);

				grid.UnSelectAll();
				grid.Select(0);
				grid.ContextMenu.OnPopup_ForTest();
				AssertEquals("Create Permit Menu Item should be Enabled", true, lpcoMenu.Enabled);

				lpcoMenu.PerformClick();
				AssertEquals("Ask saving job", "The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);

				var lpcoForm = invoiceLineUserControl.LPCOController.LastShownForm as ZForm;
				AssertType<LPCOForm>(lpcoForm);

				var lpco = lpcoForm.BusinessEntity as CusLPCOHeader;
				AssertEquals("A new CusLPCOHeader created", false, lpco.IsInDatabase);
				AssertNotEquals("CusLPCOHeader created in a new factory ", declaration.Factory, lpco.Factory);
				AssertEquals("Populate data from Invoice Line", supplier.PK, lpco.CPH_OH_PermitHolder);

				lpcoForm.CancelButton.PerformClick();
				Factory.Save();

				AssertEquals("CusLPCOHeader not saved after Cancel", false, lpco.IsInDatabase);
			}
		}

		protected override string JobMessageType => BRJobMessageTypeList.Codes.Export;

		protected override List<ZString> ExpectedColumnNamesListInOrder
		{
			get
			{
				if (expectedColumnNamesListOnThisOrder == null)
				{
					expectedColumnNamesListOnThisOrder = new List<ZString>
					{
						JobComInvoiceLine.Schema.JI_LineNo,
						JobComInvoiceLine.Schema.JI_Calc_Invoice,
						JobComInvoiceLine.Schema.JI_PartNo,
						JobComInvoiceLine.Schema.JI_CC,
						JobComInvoiceLine.Schema.JI_Tariff,
						JobComInvoiceLine.Schema.JI_InvoiceQuantity,
						JobComInvoiceLine.Schema.JI_InvoiceUQ,
						JobComInvoiceLine.Schema.JI_CustomsQuantity,
						JobComInvoiceLine.Schema.JI_CustomsUnitQty,
						JobComInvoiceLine.Schema.JI_LinePrice,
						JobComInvoiceLine.Schema.FullGoodsDescription,
						JobComInvoiceLine.Schema.JI_CountryOfOrigin,
						JobComInvoiceLine.Schema.JI_RH_NKCommodity_Code,
						JobComInvoiceLine.Schema.JI_Weight,
						JobComInvoiceLine.Schema.JI_WeightUQ,
						JobComInvoiceLine.Schema.JI_NetWeight,
						JobComInvoiceLine.Schema.JI_NetWeightUQ,
						JobComInvoiceLine.Schema.JI_Volume,
						JobComInvoiceLine.Schema.JI_VolumeUQ,
						JobComInvoiceLine.Schema.JI_OrderNumber,
						JobComInvoiceLine.Schema.JI_Calc_OrderLineNumberAndSubLine,
						JobComInvoiceLine.Schema.JI_PartAttrib1,
						JobComInvoiceLine.Schema.JI_PartAttrib2,
						JobComInvoiceLine.Schema.JI_PartAttrib3,
						JobComInvoiceLine.Schema.JI_SerialNumber,
						JobComInvoiceLine.Schema.JI_CustomAttrib1,
						JobComInvoiceLine.Schema.JI_CustomAttrib2,
						JobComInvoiceLine.Schema.JI_CustomAttrib3,
						JobComInvoiceLine.Schema.JI_CustomAttrib4,
						JobComInvoiceLine.Schema.JI_CustomAttrib5,
						JobComInvoiceLine.Schema.JI_CustomAttrib6,
						JobComInvoiceLine.Schema.JI_CustomTextBlob1,
						JobComInvoiceLine.Schema.JI_CEI,
						JobComInvoiceLine.Schema.JI_NFeNumber,
						JobComInvoiceLine.Schema.JI_NFeItemNumber,
						JobComInvoiceLine.Schema.JI_NFeLinePrice,
						JobComInvoiceLine.Schema.JI_Procedure,
						JobComInvoiceLine.Schema.JI_RN_NKCountryOfExport,
						JobComInvoiceLine.Schema.JI_FinancedValue,
						JobComInvoiceLine.Schema.JI_SecondCPC,
						JobComInvoiceLine.Schema.JI_AgentCommissionPercentage
					};
				}
				return expectedColumnNamesListOnThisOrder;
			}
		}
		List<ZString> expectedColumnNamesListOnThisOrder;
	}
}
