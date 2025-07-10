using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AsycudaCustoms.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AsycudaCustoms.GUI.Testing
{
	class BaseInvoiceLineChargesUserControlTest : TestCaseWithFactory
	{
		public void TestGridCaption()
		{
			using (var userControl = new BaseInvoiceLineChargesUserControl())
			{
				AssertEquals("VAT Apply", userControl.ChargesGrid.GetColumnStyle("J7_IsGSTApplicable").Caption);
				AssertEquals("VAT Apply", userControl.ApportionedChargesGrid.GetColumnStyle("J7_IsGSTApplicable").Caption);
			}
		}

		public void TestColumnsDefaultOrderAndVisibility_ChargeGrid()
		{
			using (var form = new ZForm())
			using (var userControl = new BaseInvoiceLineChargesUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();
				userControl.Show();

				var chargeGrid = userControl.ChargesGrid;
				var index = 0;
				AssertColumn(chargeGrid, InvoiceCharge.Schema.J7_ChargeType, true, index++);
				AssertColumn(chargeGrid, InvoiceCharge.Schema.J7_ChargeDescription, true, index++);
				AssertColumn(chargeGrid, InvoiceCharge.Schema.ChargeCodeDescription, false, index++, true);
				AssertColumn(chargeGrid, InvoiceCharge.Schema.J7_Amount, true, index++);
				AssertColumn(chargeGrid, InvoiceCharge.Schema.J7_RX_NKCurrency, true, index++);
				AssertColumn(chargeGrid, InvoiceCharge.Schema.J7_IsDutiable, true, index++);
				AssertColumn(chargeGrid, InvoiceCharge.Schema.J7_IsGSTApplicable, true, index++);
				AssertColumn(chargeGrid, InvoiceCharge.Schema.J7_Percentage, true, index++);
				AssertColumn(chargeGrid, InvoiceCharge.Schema.J7_IsIncludedInITOT, false, index++);
				AssertColumn(chargeGrid, InvoiceCharge.Schema.J7_Calc_IsIncludedInInvoiceAmount, true, index++);
			}
		}

		public void TestColumnsDefaultOrderAndVisibility_ApportionedChargesGrid()
		{
			using (var form = new ZForm())
			using (var userControl = new BaseInvoiceLineChargesUserControl())
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
				AssertColumn(chargeGrid, InvoiceCharge.Schema.J7_IsIncludedInITOT, false, index++);
				AssertColumn(chargeGrid, InvoiceCharge.Schema.J7_FullOrPartialApportionment, false, index++);
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
	}
}
