using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.GUI.PlugIn
{
	public class TaxUserControl : EU.GUI.PlugIn.TaxUserControl
	{
		protected override void SetPropertiesAfterInit()
		{
			base.SetPropertiesAfterInit();

			CaptionRenderingEnabled = true;
			RemoveMethodOfPayment();
			RemoveTaxRateGroupBox();
			TurnMethodOfCalculationTextBoxToDropEdit();
			AddTariffBypassColumn();
			TurnTaxRateDutyControlToMethodOfCalculation();
		}

		void RemoveMethodOfPayment()
		{
			var methodOfPaymentColumn = TaxGrid.GetColumnStyle("Data+G4_MethodOfPayment");
			TaxGrid.ColumnStyles.Remove(methodOfPaymentColumn);
			TaxMethodOfPaymentDropEdit.Visible = false;
		}
		void RemoveTaxRateGroupBox()
		{
			TaxRateGroupBox.Visible = false;
			TaxRateGroupBox.Controls.Remove(TaxRateDutyDropEdit);
		}

		void TurnMethodOfCalculationTextBoxToDropEdit()
		{
			var methodOfCalculationColumn = TaxGrid.GetColumnStyle(zTextBoxColumnStyleInfoForMethodOfCalculation.ColumnName);
			TaxGrid.ColumnStyles.Remove(methodOfCalculationColumn);

			var zDropEditColumnStyleInfo1 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			zDropEditColumnStyleInfo1.ColumnName = JobComInvoiceLineTax.Schema.JLT_MethodOfCalculation;
			TaxGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
		}

		void AddTariffBypassColumn()
		{
			var zDropEditColumnStyleInfo2 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			zDropEditColumnStyleInfo2.ColumnName = "TariffBypassCode";
			zDropEditColumnStyleInfo2.IsReadOnly = true;
			TaxGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
		}

		void TurnTaxRateDutyControlToMethodOfCalculation()
		{
			TaxGroupBox.Controls.Add(TaxRateDutyDropEdit);
			TaxRateDutyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 45, true);
			BindingSource.SetBindingMember(TaxRateDutyDropEdit, "FilteredInvoiceLines.Taxes.Data.JLT_MethodOfCalculation");
		}
	}
}
