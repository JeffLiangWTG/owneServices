using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.BR.GUI.Testing
{
	class ExportSupplierHeaderUserControlTest : TestCaseWithFactory
	{
		public void TestColumnLayoutContext()
		{
			using (ExportSupplierHeaderUserControl control = new ExportSupplierHeaderUserControl())
			{
				AssertEquals("Column layout context should have been set", nameof(Customs.GUI.DeclarationType.Export), control.InvoiceHeadersBoundGrid.ColumnLayoutContext);
			}
		}

		public void TestInvoiceHeaderGridColumnsOrder()
		{
			var oDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			oDeclaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Export;
			using (var oForm = new JobDeclarationForm(oDeclaration))
			{
				oForm.Show();
				var oBrokerageControl = oForm.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				using (var oJobDeclarationUserControl = (CustomsDeclarationUserControl)oBrokerageControl.DeclarationUserControlForTesting)
				{
					if (oJobDeclarationUserControl != null)
					{
						oBrokerageControl.MainTabControl.SelectedTab = oBrokerageControl.InvoicesTabPage;
						var oInvoiceHeaderGrid = oBrokerageControl.SupplierHeaderUserControl.JobComInvoiceHeadersBoundGrid.InnerGrid;

						Assert("Column list count", ExpectedColumnNamesInSortOrderList.Count <= oInvoiceHeaderGrid.Columns.Count);

						int index = 0;
						foreach (var expectedColumnName in ExpectedColumnNamesInSortOrderList.Keys)
						{
							var oColumn = oInvoiceHeaderGrid.Columns[index];
							AssertNotNull(oColumn);

							ExpectedColumnNamesInSortOrderList.TryGetValue(expectedColumnName, out bool isVisible);
							AssertEquals("Expected Column Name", expectedColumnName, oColumn.ColumnStyle.MappingName);
							AssertEquals("Expected IsVisible", isVisible, oColumn.IsVisible);
							index++;
						}
					}
				}
			}
		}

		public void TestControlLabels()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			using (var form = new JobDeclarationForm(jobDeclaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
				form.CustomsBrokerageUserControl.SupplierHeaderUserControl.ChargesTabControl.SelectedTab = form.CustomsBrokerageUserControl.SupplierHeaderUserControl.ApportionedTabPage;
				using (var invoiceHeaderUserControl = (BaseCustomsSupplierHeaderUserControl)form.CustomsBrokerageUserControl.SupplierHeaderUserControl)
				{
					AssertEquals("Expected Column Name", "Dutiable", invoiceHeaderUserControl.ApportionedChargesGrid.Columns[BaseInvoiceLineCharge.Schema.J7_IsDutiable].ColumnStyle.HeaderText);
					AssertEquals("Expected Column Name", "Dutiable", invoiceHeaderUserControl.BaseGroupChargesGrid.Columns[BaseInvoiceLineCharge.Schema.J7_IsDutiable].ColumnStyle.HeaderText);
					AssertEquals("Expected Column Name", "Dutiable", invoiceHeaderUserControl.InvoiceChargesGrid.Columns[BaseInvoiceLineCharge.Schema.J7_IsDutiable].ColumnStyle.HeaderText);

					var fobAmountBoundCurrencyControl = invoiceHeaderUserControl.Controls.Find("JZ_FOBAmountBoundCurrencyControl", true).FirstOrDefault();
					AssertEquals("FOB Value", ((ConvertToLocalCurrencyControl)fobAmountBoundCurrencyControl)?.CaptionResourceString?.Caption);

					var cifAmountBoundCurrencyControl = invoiceHeaderUserControl.Controls.Find("JZ_CIFAmountBoundCurrencyControl", true).FirstOrDefault();
					AssertEquals("Customs Value", ((ConvertToLocalCurrencyControl)cifAmountBoundCurrencyControl)?.CaptionResourceString?.Caption);
				}
			}
		}

		protected Dictionary<ZString, bool> ExpectedColumnNamesInSortOrderList
		{
			get
			{
				if (expectedColumnNamesInSortOrderList == null)
				{
					expectedColumnNamesInSortOrderList = new Dictionary<ZString, bool>();
					expectedColumnNamesInSortOrderList.Add(JobComInvoiceHeader.Schema.JZ_InvoiceNumber, true);
					expectedColumnNamesInSortOrderList.Add(JobComInvoiceHeader.Schema.JZ_OH_Supplier, true);
					expectedColumnNamesInSortOrderList.Add(JobComInvoiceHeader.Schema.JZ_OH_Buyer, true);
					expectedColumnNamesInSortOrderList.Add(JobComInvoiceHeader.Schema.JZ_IncoTerm, true);
					expectedColumnNamesInSortOrderList.Add(JobComInvoiceHeader.Schema.JZ_Calc_FOBAmount, true);
					expectedColumnNamesInSortOrderList.Add(JobComInvoiceHeader.Schema.JZ_Calc_FOBCurrency, true);
					expectedColumnNamesInSortOrderList.Add(JobComInvoiceHeader.Schema.JZ_InvoiceAmount, true);
					expectedColumnNamesInSortOrderList.Add(JobComInvoiceHeader.Schema.JZ_RX_NKInvoice_Currency, true);
					expectedColumnNamesInSortOrderList.Add(JobComInvoiceHeader.Schema.InvoiceLineTotal, true);
					expectedColumnNamesInSortOrderList.Add(JobComInvoiceHeader.Schema.JZ_Calc_BalanceString, true);
					expectedColumnNamesInSortOrderList.Add(JobComInvoiceHeader.Schema.JZ_CU_RelatedHouseBill, true);
					expectedColumnNamesInSortOrderList.Add(JobComInvoiceHeader.Schema.JZ_Weight, true);
					expectedColumnNamesInSortOrderList.Add(JobComInvoiceHeader.Schema.JZ_WeightUQ, true);
					expectedColumnNamesInSortOrderList.Add(JobComInvoiceHeader.Schema.JZ_NetWeight, true);
					expectedColumnNamesInSortOrderList.Add(JobComInvoiceHeader.Schema.JZ_NetWeightUQ, true);
					expectedColumnNamesInSortOrderList.Add(JobComInvoiceHeader.Schema.JZ_Calc_GroupInvoice, true);
				}

				return expectedColumnNamesInSortOrderList;
			}
		}

		Dictionary<ZString, bool> expectedColumnNamesInSortOrderList;
	}
}
