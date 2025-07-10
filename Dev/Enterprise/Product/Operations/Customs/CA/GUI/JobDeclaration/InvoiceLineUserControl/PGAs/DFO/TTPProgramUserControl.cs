using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	public partial class TTPProgramUserControl : ZUserControl
	{
		public TTPProgramUserControl(bool isOnInvoiceLine)
		{
			InitializeComponent();
			this.isOnInvoiceLine = isOnInvoiceLine;

			InitializeLazyCreate();
			UpdateDataBinding();
		}

		readonly bool isOnInvoiceLine;

		protected void InitializeLazyCreate()
		{
			LPCOGridUserControl.RemoveExceptAvailableColumns(DFOPGAHeader.AvailableLPCOFields);
		}

		void UpdateDataBinding()
		{
			if (!isOnInvoiceLine)
			{
				CommonNameTextBox.Dispose();
				VolumeCalcDropEdit.Dispose();
				WeightCalcDropEdit.Dispose();
				CountIntEdit.Dispose();

				CommissionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(478, 62, true);
			}
		}
	}
}
