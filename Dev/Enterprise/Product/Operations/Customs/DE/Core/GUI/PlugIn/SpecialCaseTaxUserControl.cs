using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.PlugIn
{
	public partial class SpecialCaseTaxUserControl : EU.GUI.PlugIn.TaxUserControl
	{
		public SpecialCaseTaxUserControl()
			: base()
		{
			InitializeComponent();
			SetPropertiesAfterInit();
		}

		protected override void SetPropertiesAfterInit()
		{
			var columnStyles = TaxGrid.ColumnStyles;
			columnStyles.RemoveRange(0, columnStyles.Count);

			var zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo()
			{
				ColumnName = JobComInvoiceLineTax.Schema.JLT_Type,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60)
			};

			var zDropEditColumnStyleInfo2 = new ZDropEditColumnStyleInfo()
			{
				ColumnName = JobComInvoiceLineTax.Schema.JLT_MethodOfCalculation,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
			};

			var zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo()
			{
				ColumnName = JobComInvoiceLineTax.Schema.JLT_Rate,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
			};

			TaxGrid.ColumnStyles.AddRange(
				new IZColumnStyleInfo[]
				{
					zDropEditColumnStyleInfo1,
					zDropEditColumnStyleInfo2,
					zCalcEditColumnStyleInfo1
				});
		}
	}
}
