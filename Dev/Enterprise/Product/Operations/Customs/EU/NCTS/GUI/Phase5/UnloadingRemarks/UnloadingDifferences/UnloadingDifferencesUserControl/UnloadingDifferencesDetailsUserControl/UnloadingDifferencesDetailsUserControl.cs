using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class UnloadingDifferencesDetailsUserControl : ZUserControl
	{
		public UnloadingDifferencesDetailsUserControl()
		{
			InitializeComponent();
		}

		void RecalculateTotalsButton_Click(object sender, EventArgs e)
		{
			var arrivalMovementHeader = (NctsArrivalMovementHeader)(((CargoWise.Windows.UI.KUserControl)RecalculateTotalsButton.Parent).CurrentDataItem);
			arrivalMovementHeader.TotalGrossMassInKilogramsInfo.RefreshBinding();
			arrivalMovementHeader.TotalNumberOfPackagesInfo.RefreshBinding();
			arrivalMovementHeader.UpdateBM_GrossWeightUnloadedFromTotalGrossMassInKilograms();
			arrivalMovementHeader.TotalUnloadedNumberOfPackagesInfo.RefreshBinding();
		}
	}
}
