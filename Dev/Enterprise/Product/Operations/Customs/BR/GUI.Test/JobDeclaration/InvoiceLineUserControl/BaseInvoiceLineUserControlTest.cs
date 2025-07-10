using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI.Testing
{
	sealed class BaseInvoiceLineUserControlBaseOnlyTest : BaseInvoiceLineUserControlAbstractTest
	{
		public void TestFilterBusinessObjectType()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclartion.Invoices.AddNew().InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(jobDeclartion))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoiceLineUserControl = (BaseInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				AssertType(typeof(InvoiceLineFilterBusinessObject), invoiceLineUserControl.FilterBusinessObject);
			}
		}

		public void TestInvoiceAmountVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			declaration.Invoices.AddNew().InvoiceLines.AddNew();

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();

				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var oBrokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				oBrokerageControl.LoadInvoiceLinesTabPage();
				var oInvoiceLineControl = oBrokerageControl.InvoiceLinesUserControl as BaseInvoiceLineUserControl;
				Assert("JI_Calc_InvAmount should be visible for EXP", oInvoiceLineControl.JI_Calc_InvAmountControl.Visible);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				oBrokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				oBrokerageControl.LoadInvoiceLinesTabPage();
				oInvoiceLineControl = oBrokerageControl.InvoiceLinesUserControl as BaseInvoiceLineUserControl;
				Assert("JI_Calc_InvAmount should be visible for IMP", oInvoiceLineControl.JI_Calc_InvAmountControl.Visible);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.ExWarehouse;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				oBrokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				oBrokerageControl.LoadInvoiceLinesTabPage();
				oInvoiceLineControl = oBrokerageControl.InvoiceLinesUserControl as BaseInvoiceLineUserControl;
				Assert("JI_Calc_InvAmount shouldn't be visible for EXW", !oInvoiceLineControl.JI_Calc_InvAmountControl.Visible);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				oBrokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				oBrokerageControl.LoadInvoiceLinesTabPage();
				oInvoiceLineControl = oBrokerageControl.InvoiceLinesUserControl as BaseInvoiceLineUserControl;
				Assert("JI_Calc_InvAmount should be visible for ISW", oInvoiceLineControl.JI_Calc_InvAmountControl.Visible);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				oBrokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				oBrokerageControl.LoadInvoiceLinesTabPage();
				oInvoiceLineControl = oBrokerageControl.InvoiceLinesUserControl as BaseInvoiceLineUserControl;
				Assert("JI_Calc_InvAmount should be visible for LIC", oInvoiceLineControl.JI_Calc_InvAmountControl.Visible);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.LPCO;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				oBrokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				oBrokerageControl.LoadInvoiceLinesTabPage();
				oInvoiceLineControl = oBrokerageControl.InvoiceLinesUserControl as BaseInvoiceLineUserControl;
				Assert("JI_Calc_InvAmount shouldn't be visible for LPCO", !oInvoiceLineControl.JI_Calc_InvAmountControl.Visible);
			}
		}

		public void TestCustomsInvoiceLinesBoundGridColumnsVisibility_CommercialInvoiceForm()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();

			using (var form = new CommercialInvoiceForm(invoice))
			{
				form.Show();
				form.MainTabControl.SelectedTab = form.LinesTabPage;
				AssertEquals("CustomsInvoiceLinesBoundGrid Shouldn't have JI_CEI column", false, form.InvoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns.Contains(JobComInvoiceLine.Schema.JI_CEI));
			}
		}

		public void TestCustomsInvoiceLinesBoundGridColumnsVisibility_JobDeclarationForm()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew();

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var grid = form.CustomsBrokerageUserControl.InvoiceLinesUserControl.CustomsInvoiceLinesBoundGrid;
				var columns = grid.Columns;

				AssertEquals("CustomsInvoiceLinesBoundGrid should have JI_CEI column", true, columns.Contains(JobComInvoiceLine.Schema.JI_CEI));
				AssertEquals("JI_CEI column should only show code", ZDropEdit.ShowInDropDownList.OnlyShowCode, (grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CEI) as ZGuidDropEditColumnStyleInfo).ShowInDropDown);
				AssertEquals("CustomsInvoiceLinesBoundGrid should not have JI_Description column", false, columns.Contains(JobComInvoiceLine.Schema.JI_Description));
				AssertEquals("CustomsInvoiceLinesBoundGrid should have FullGoodsDescription column", true, columns.Contains(JobComInvoiceLine.Schema.FullGoodsDescription));
			}
		}

		protected override string JobMessageType => BRJobMessageTypeList.Codes.LPCO;

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
						JobComInvoiceLine.Schema.JI_InvoiceQuantity ,
						JobComInvoiceLine.Schema.JI_InvoiceUQ ,
						JobComInvoiceLine.Schema.JI_CustomsQuantity ,
						JobComInvoiceLine.Schema.JI_CustomsUnitQty,
						JobComInvoiceLine.Schema.JI_LinePrice ,
						JobComInvoiceLine.Schema.FullGoodsDescription ,
						JobComInvoiceLine.Schema.JI_CountryOfOrigin ,
						JobComInvoiceLine.Schema.JI_RH_NKCommodity_Code ,
						JobComInvoiceLine.Schema.JI_Weight,
						JobComInvoiceLine.Schema.JI_WeightUQ,
						JobComInvoiceLine.Schema.JI_NetWeight ,
						JobComInvoiceLine.Schema.JI_NetWeightUQ ,
						JobComInvoiceLine.Schema.JI_Volume,
						JobComInvoiceLine.Schema.JI_VolumeUQ,
						JobComInvoiceLine.Schema.JI_OrderNumber ,
						JobComInvoiceLine.Schema.JI_Calc_OrderLineNumberAndSubLine,
						JobComInvoiceLine.Schema.JI_PartAttrib1 ,
						JobComInvoiceLine.Schema.JI_PartAttrib2 ,
						JobComInvoiceLine.Schema.JI_PartAttrib3 ,
						JobComInvoiceLine.Schema.JI_SerialNumber,
						JobComInvoiceLine.Schema.JI_CustomAttrib1 ,
						JobComInvoiceLine.Schema.JI_CustomAttrib2 ,
						JobComInvoiceLine.Schema.JI_CustomAttrib3 ,
						JobComInvoiceLine.Schema.JI_CustomAttrib4 ,
						JobComInvoiceLine.Schema.JI_CustomAttrib5 ,
						JobComInvoiceLine.Schema.JI_CustomAttrib6 ,
						JobComInvoiceLine.Schema.JI_CustomTextBlob1 ,
						JobComInvoiceLine.Schema.JI_CEI ,
						JobComInvoiceLine.Schema.JI_NFeNumber,
						JobComInvoiceLine.Schema.JI_NFeItemNumber,
						JobComInvoiceLine.Schema.JI_NFeLinePrice ,
						JobComInvoiceLine.Schema.JI_Procedure ,
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

	public abstract class BaseInvoiceLineUserControlAbstractTest : TestCaseWithFactory
	{
		public void TestCustomsInvoiceLinesBoundGridColumnsOrder()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageType;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var customsBrokerageUserControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				customsBrokerageUserControl.InvoiceGroupingTabPage.TabVisible = true;
				customsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoiceLineUserControl = form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				var customsInvoiceLinesBoundGrid = invoiceLineUserControl.CustomsInvoiceLinesBoundGrid;
				customsInvoiceLinesBoundGrid.ResetColumns();
				AssertContainsExactElementsInExactOrder(ExpectedColumnNamesListInOrder, customsInvoiceLinesBoundGrid.Columns.Where(c => c.IsVisible).Select(c => c.ColumnStyle.MappingName));
			}
		}

		protected abstract string JobMessageType { get; }

		protected abstract List<ZString> ExpectedColumnNamesListInOrder { get; }
	}
}
