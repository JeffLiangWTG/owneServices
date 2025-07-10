using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.AsycudaCustoms.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AsycudaCustoms.GUI.Testing
{
	class BaseCustomsSupplierHeaderUserControlTest : TestCaseWithFactory
	{
		public void TestComponentsVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var testForm = new JobDeclarationForm(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoicesTabPage;
				var supplierHeaderUserControl = (BaseCustomsSupplierHeaderUserControl)testForm.CustomsBrokerageUserControl.SupplierHeaderUserControl;
				var baseGroupChargesGroupBox = supplierHeaderUserControl.FindSingle<ZGroupBox>("BaseGroupChargesGroupBox");
				Assert("baseGroupChargesGroupBox is visible.", baseGroupChargesGroupBox.Visible);
				AssertNotNull(supplierHeaderUserControl.Controls.Find("SupportingDocumentsUserControl", true).FirstOrDefault());

				var tniBoundInvoiceCurrencyControl = supplierHeaderUserControl.FindSingle<ConvertToLocalCurrencyControl>("JZ_Calc_TNIBoundInvoiceCurrencyControl");
				AssertEquals("JZ_Calc_TNIBoundInvoiceCurrencyControl is not visible.", false, tniBoundInvoiceCurrencyControl.Visible);
			}
		}

		public void TestColumnsRemoval()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
				var supplierHeaderUserControl = (BaseCustomsSupplierHeaderUserControl)form.CustomsBrokerageUserControl.SupplierHeaderUserControl;
				var jobComInvoiceHeadersBoundGridColumns = supplierHeaderUserControl.JobComInvoiceHeadersBoundGrid.ColumnStyles;

				CombineAssertions(() =>
				{
					AssertNull("Remove Payment # column", jobComInvoiceHeadersBoundGridColumns.Cast<ZTextBoxColumnStyleInfo>().FirstOrDefault(x => x.ColumnName == JobComInvoiceHeaderSchema.JZ_PaymentNo.Name));
					AssertNull("Remove Payment Amount column", jobComInvoiceHeadersBoundGridColumns.Cast<ZTextBoxColumnStyleInfo>().FirstOrDefault(x => x.ColumnName == JobComInvoiceHeaderSchema.JZ_PaymentAmount.Name));
					AssertNull("Remove Payment Ex Rate column", jobComInvoiceHeadersBoundGridColumns.Cast<ZTextBoxColumnStyleInfo>().FirstOrDefault(x => x.ColumnName == JobComInvoiceHeaderSchema.JZ_PaymentExRate.Name));
					AssertNull("Remove Bill column", jobComInvoiceHeadersBoundGridColumns.Cast<ZTextBoxColumnStyleInfo>().FirstOrDefault(x => x.ColumnName == JobComInvoiceHeaderSchema.JZ_CU_RelatedHouseBill.Name));
					AssertNull("Remove Payment Date column", jobComInvoiceHeadersBoundGridColumns.Cast<ZTextBoxColumnStyleInfo>().FirstOrDefault(x => x.ColumnName == JobComInvoiceHeaderSchema.JZ_PaymentDate.Name));
				});
			}
		}

		public void TestColumnsDefaultOrderAndVisibility_ChargeGrid()
		{
			using (var form = new ZForm())
			using (var userControl = new BaseCustomsSupplierHeaderUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();
				userControl.Show();
				var chargeGrid = userControl.InvoiceChargesGrid;

				var index = 0;
				AssertColumn(chargeGrid, InvoiceCharge.Schema.J7_ChargeType, true, index++);
				AssertColumn(chargeGrid, InvoiceCharge.Schema.J7_ChargeDescription, true, index++);
				AssertColumn(chargeGrid, InvoiceCharge.Schema.ChargeCodeDescription, false, index++, true);
				AssertColumn(chargeGrid, InvoiceCharge.Schema.J7_Amount, true, index++);
				AssertColumn(chargeGrid, InvoiceCharge.Schema.J7_RX_NKCurrency, true, index++);
				AssertColumn(chargeGrid, InvoiceCharge.Schema.J7_IsIncludedInITOT, true, index++);
				AssertColumn(chargeGrid, InvoiceCharge.Schema.J7_IsDutiable, true, index++);
				AssertColumn(chargeGrid, InvoiceCharge.Schema.J7_IsGSTApplicable, true, index++);
				AssertColumn(chargeGrid, InvoiceCharge.Schema.J7_Calc_IsIncludedInInvoiceAmount, true, index++);
				AssertColumn(chargeGrid, InvoiceCharge.Schema.J7_PrepaidCollect, false, index++);
				AssertColumn(chargeGrid, InvoiceCharge.Schema.J7_Percentage, false, index++);
				AssertColumn(chargeGrid, InvoiceCharge.Schema.J7_DistributeBy, true, index++);
			}
		}

		public void TestColumnsDefaultOrderAndVisibility_ApportionedChargesGrid()
		{
			using (var form = new ZForm())
			using (var userControl = new BaseCustomsSupplierHeaderUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();
				userControl.Show();

				var chargeGrid = userControl.ApportionedChargesGrid;
				var index = 0;
				AssertColumn(chargeGrid, InvoiceCharge.Schema.J7_ChargeType, true, index++);
				AssertColumn(chargeGrid, InvoiceCharge.Schema.J7_ChargeDescription, true, index++);
				AssertColumn(chargeGrid, InvoiceCharge.Schema.ChargeCodeDescription, false, index++, true);
				AssertColumn(chargeGrid, InvoiceCharge.Schema.J7_Amount, true, index++);
				AssertColumn(chargeGrid, InvoiceCharge.Schema.J7_RX_NKCurrency, true, index++);
				AssertColumn(chargeGrid, InvoiceCharge.Schema.J7_IsDutiable, true, index++);
				AssertColumn(chargeGrid, InvoiceCharge.Schema.J7_IsGSTApplicable, true, index++);
				AssertColumn(chargeGrid, InvoiceCharge.Schema.J7_IsIncludedInITOT, true, index++);
				AssertColumn(chargeGrid, InvoiceCharge.Schema.J7_Calc_IsIncludedInInvoiceAmount, true, index++);
				AssertColumn(chargeGrid, InvoiceCharge.Schema.J7_FullOrPartialApportionment, false, index++);
			}
		}

		public void TestColumnsDefaultOrderAndVisibility_BaseGroupChargesGrid()
		{
			using (var form = new ZForm())
			using (var userControl = new BaseCustomsSupplierHeaderUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();
				userControl.Show();

				var chargeGrid = userControl.BaseGroupChargesGrid;
				var index = 0;
				AssertColumn(chargeGrid, InvoiceCharge.Schema.J7_ChargeType, true, index++);
				AssertColumn(chargeGrid, InvoiceCharge.Schema.J7_ChargeDescription, true, index++);
				AssertColumn(chargeGrid, InvoiceCharge.Schema.ChargeCodeDescription, false, index++, true);
				AssertColumn(chargeGrid, InvoiceCharge.Schema.J7_Amount, true, index++);
				AssertColumn(chargeGrid, InvoiceCharge.Schema.J7_RX_NKCurrency, true, index++);
				AssertColumn(chargeGrid, InvoiceCharge.Schema.J7_IsDutiable, true, index++);
				AssertColumn(chargeGrid, InvoiceCharge.Schema.J7_IsGSTApplicable, true, index++);
				AssertColumn(chargeGrid, InvoiceCharge.Schema.J7_Percentage, true, index++);
				AssertColumn(chargeGrid, InvoiceCharge.Schema.J7_PrepaidCollect, false, index++);
				AssertColumn(chargeGrid, InvoiceCharge.Schema.J7_DistributeBy, true, index++);
				AssertColumn(chargeGrid, InvoiceCharge.Schema.J7_FullOrPartialApportionment, true, index++);
				AssertColumn(chargeGrid, BaseGroupInvoiceCharge.Schema.J7_Calc_IsIncludedInITOT, true, index++);
			}
		}

		static void AssertColumn(ZGrid lineGrid, string columnName, bool isVisible, int index = -1, bool isUnavailable = false)
		{
			var column = lineGrid.GetColumnStyle(columnName);
			AssertEquals($"Column {columnName} visible.", isVisible, column.IsVisible);
			AssertEquals($"Column {columnName} isUnavailable.", isUnavailable, column.IsUnavailable);
			if (index >= 0)
			{
				AssertEquals($"Column {columnName} should be at index {index}", index,
					lineGrid.ColumnStyles.IndexOf(lineGrid.GetColumnStyle(columnName)));
			}
		}

		public void TestColumnsAddition()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
				var supplierHeaderUserControl = (BaseCustomsSupplierHeaderUserControl)form.CustomsBrokerageUserControl.SupplierHeaderUserControl;
				var column = supplierHeaderUserControl.JobComInvoiceHeadersBoundGrid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == "JZ_ValuationDateOverride");
				AssertNotNull(column);
				AssertEquals(false, column.IsVisible);
			}
		}
	}
}
