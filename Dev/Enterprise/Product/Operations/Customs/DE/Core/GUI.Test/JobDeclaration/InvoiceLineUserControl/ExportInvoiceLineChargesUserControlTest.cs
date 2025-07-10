using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.GUI.Testing
{
	public sealed class ExportInvoiceLineChargesUserControlTest : TestCaseWithFactory
	{
		public void TestChargesGrid_Columns()
		{
			using (var control = new ExportInvoiceLineChargesUserControl())
			{
				var chargesGrid = control.ChargesGrid;
				var isDutiableColumnStyle = chargesGrid.GetColumnStyle(InvoiceLineCharge.Schema.J7_IsDutiable);
				var isGSTApplicableColumnStyle = chargesGrid.GetColumnStyle(InvoiceLineCharge.Schema.J7_IsGSTApplicable);
				CombineAssertions(() =>
				{
					Assert(isDutiableColumnStyle.ColumnName, isDutiableColumnStyle.IsUnavailable);
					Assert(isGSTApplicableColumnStyle.ColumnName, isGSTApplicableColumnStyle.IsUnavailable);
				});
			}
		}

		public void TestApportionedChargesGrid_Columns()
		{
			using (var control = new ExportInvoiceLineChargesUserControl())
			{
				var apportionedChargesGrid = control.ApportionedChargesGrid;
				var isDutiableColumnStyle = apportionedChargesGrid.GetColumnStyle(InvoiceLineCharge.Schema.J7_IsDutiable);
				var isGSTApplicableColumnStyle = apportionedChargesGrid.GetColumnStyle(InvoiceLineCharge.Schema.J7_IsGSTApplicable);
				CombineAssertions(() =>
				{
					Assert(isDutiableColumnStyle.ColumnName, isDutiableColumnStyle.IsUnavailable);
					Assert(isGSTApplicableColumnStyle.ColumnName, isGSTApplicableColumnStyle.IsUnavailable);
				});
			}
		}
	}
}
