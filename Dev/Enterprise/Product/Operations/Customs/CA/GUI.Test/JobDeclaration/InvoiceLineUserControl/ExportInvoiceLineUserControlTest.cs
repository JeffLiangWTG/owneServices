using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class ExportInvoiceLineUserControlTest : TestCaseWithFactory
	{
		public void TestTariffUserControlForCAGlobalTariff()
		{
			var gridName = "CustomsInvoiceLinesBoundGrid";
			var columnName = JobComInvoiceLine.Schema.JI_FormattedTariff;
			var tariffFindBoxName_TrfCA = "TariffFindBox";
			var tariffFindBoxName_SRDb = "TariffFromSRDbFindBox";
			var tariffColumnInfoType = typeof(Universal.GUI.TariffColumnStyleInfo);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.Invoices.AddNew().InvoiceLines.AddNew();

			using (var form = new ZForm(declaration))
			using (var control = new CAExportInvoiceLineUserControl())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();
				var tariffColumnInfoCaption = ClassificationTariffUserControlTestHelper.AssertTariffColumnInfo_GetTariffFromTrfCA(control, gridName, columnName, tariffColumnInfoType);
				ClassificationTariffUserControlTestHelper.AssertTariffColumnInfo_GetTariffFromSRDb(control, gridName, columnName, tariffColumnInfoCaption);
				ClassificationTariffUserControlTestHelper.AssertTariffFindBox_GetTariffFromSRDb(control, tariffFindBoxName_TrfCA, tariffFindBoxName_SRDb);
			}
		}

		public void TestGridLayoutContext()
		{
			using (CAExportInvoiceLineUserControl control = new CAExportInvoiceLineUserControl())
			{
				AssertEquals("context is set", nameof(Customs.GUI.DeclarationType.Export), control.CustomsInvoiceLinesBoundGrid.ColumnLayoutContext);
			}
		}

		public void TestDefaultVisibleColumnsInSortOrder()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				CustomsBrokerageUserControl brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
				Assert("brokerageControl.InvoiceLinesUserControl.CustomsInvoiceLinesBoundGrid.ColumnStyles.Count must have at least " + ExpectedColumnNamesInSortOrderList.Count.ToString(), ExpectedColumnNamesInSortOrderList.Count <= brokerageControl.InvoiceLinesUserControl.CustomsInvoiceLinesBoundGrid.ColumnStyles.Count);
				for (int i = 0; i < ExpectedColumnNamesInSortOrderList.Count; i++)
				{
					ZGridColumnInfo columnInfo = brokerageControl.InvoiceLinesUserControl.CustomsInvoiceLinesBoundGrid.ColumnStyles[i] as ZGridColumnInfo;
					AssertNotNull(columnInfo);
					ZString expectedColumnName = ExpectedColumnNamesInSortOrderList[i];
					AssertEquals("Expected", expectedColumnName, columnInfo.ColumnName);
					AssertEquals("Visible", true, columnInfo.IsVisible);
				}
			}
		}

		public void TestColumnWidths()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				CustomsBrokerageUserControl brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
				AssertEquals("JobComInvoiceLine.Schema.JI_Calc_Invoice", 70, brokerageControl.InvoiceLinesUserControl.CustomsInvoiceLinesBoundGrid.GetColumnWidth(JobComInvoiceLine.Schema.JI_Calc_Invoice));
				AssertEquals("JobComInvoiceLine.Schema.JI_PartNo", 70, brokerageControl.InvoiceLinesUserControl.CustomsInvoiceLinesBoundGrid.GetColumnWidth(JobComInvoiceLine.Schema.JI_PartNo));
				AssertEquals("JobComInvoiceLine.Schema.JI_CC", 105, brokerageControl.InvoiceLinesUserControl.CustomsInvoiceLinesBoundGrid.GetColumnWidth(JobComInvoiceLine.Schema.JI_CC));
				AssertEquals("JobComInvoiceLine.Schema.JI_FormattedTariff", 73, brokerageControl.InvoiceLinesUserControl.CustomsInvoiceLinesBoundGrid.GetColumnWidth(JobComInvoiceLine.Schema.JI_FormattedTariff));
				AssertEquals("JobComInvoiceLine.Schema.JI_InvoiceQuantity", 65, brokerageControl.InvoiceLinesUserControl.CustomsInvoiceLinesBoundGrid.GetColumnWidth(JobComInvoiceLine.Schema.JI_InvoiceQuantity));
				AssertEquals("JobComInvoiceLine.Schema.JI_CustomsQuantity", 70, brokerageControl.InvoiceLinesUserControl.CustomsInvoiceLinesBoundGrid.GetColumnWidth(JobComInvoiceLine.Schema.JI_CustomsQuantity));
				AssertEquals("JobComInvoiceLine.Schema.JI_CountryOfOrigin", 93, brokerageControl.InvoiceLinesUserControl.CustomsInvoiceLinesBoundGrid.GetColumnWidth(JobComInvoiceLine.Schema.JI_CountryOfOrigin));
				AssertEquals("JobComInvoiceLine.Schema.JI_StateOrRegionOfOrigin", 93, brokerageControl.InvoiceLinesUserControl.CustomsInvoiceLinesBoundGrid.GetColumnWidth(JobComInvoiceLine.Schema.JI_StateOrRegionOfOrigin));
				AssertEquals("JobComInvoiceLine.Schema.JI_LinePrice", 70, brokerageControl.InvoiceLinesUserControl.CustomsInvoiceLinesBoundGrid.GetColumnWidth(JobComInvoiceLine.Schema.JI_LinePrice));
				AssertEquals("JobComInvoiceLine.Schema.JI_Description", 80, brokerageControl.InvoiceLinesUserControl.CustomsInvoiceLinesBoundGrid.GetColumnWidth(JobComInvoiceLine.Schema.JI_Description));
				AssertEquals("JobComInvoiceLine.Schema.CA_ConveyanceIdentificationNumber", 82, brokerageControl.InvoiceLinesUserControl.CustomsInvoiceLinesBoundGrid.GetColumnWidth(JobComInvoiceLine.Schema.CA_ConveyanceIdentificationNumber));
			}
		}

		public void TestVisibility()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			CusContainer container = declaration.CusContainers.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				CustomsBrokerageUserControl brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
				AssertEquals("Container Tab should not be visible", false, brokerageControl.InvoiceLinesUserControl.ContainersTabPage.TabVisible);
				Assert("DUTY not visible", !brokerageControl.InvoiceLinesUserControl.JI_Calc_DutyConvertToLocalCurrencyControl.Visible);
				Assert("TAX not visible", !brokerageControl.InvoiceLinesUserControl.JI_Calc_GSTConvertToLocalCurrencyControl.Visible);
			}
		}

		List<ZString> ExpectedColumnNamesInSortOrderList
		{
			get
			{
				if (fExpectedColumnNamesInSortOrderList == null)
				{
					fExpectedColumnNamesInSortOrderList = new List<ZString>();
					fExpectedColumnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.JI_LineNo);
					fExpectedColumnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.JI_Calc_Invoice);
					fExpectedColumnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.JI_PartNo);
					fExpectedColumnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.JI_CC);
					fExpectedColumnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.JI_FormattedTariff);
					fExpectedColumnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.JI_InvoiceQuantity);
					fExpectedColumnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.JI_InvoiceUQ);
					fExpectedColumnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.JI_CustomsQuantity);
					fExpectedColumnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.JI_CustomsUnitQty);
					fExpectedColumnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.JI_CountryOfOrigin);
					fExpectedColumnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.JI_StateOrRegionOfOrigin);
					fExpectedColumnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.JI_LinePrice);
					fExpectedColumnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.JI_Description);
					fExpectedColumnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.CA_ConveyanceIdentificationNumber);
					fExpectedColumnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.JI_MatchingKey);
				}
				return fExpectedColumnNamesInSortOrderList;
			}
		}
		List<ZString> fExpectedColumnNamesInSortOrderList;
	}
}
