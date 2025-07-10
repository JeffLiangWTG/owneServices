using System;
using System.Windows.Forms;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class UnloadingHeaderDifferencesTabUserControl : ZUserControl
	{
		public UnloadingHeaderDifferencesTabUserControl()
		{
			InitializeComponent();
		}

		void ReCalcTotalsButton_Click(object sender, EventArgs e)
		{
			var nctsHeader = (NctsHeader)BindingSource.DataSource;
			var unloadingMovementHeader = nctsHeader.UnloadingMovementHeader;
			unloadingMovementHeader.TotalNumberOfItemsInfo.RefreshBinding();
			unloadingMovementHeader.TotalNumberOfPackagesInfo.RefreshBinding();
			unloadingMovementHeader.SumAndStoreLinesGrossMassIfNotConforming();
			unloadingMovementHeader.TotalGrossMassInKilogramsInfo.RefreshBinding();
		}

		void ResetUnloadedItemsButton_Click(object sender, EventArgs e)
		{
			var nctsMovement = (NctsHeader)BindingSource.DataSource;
			if (Globals.Message.ShowConfirmation(
				Res.GetString("E15D7CFB-C682-4EFA-A472-0F7F170B31D2", "Are you sure you want to reset the unloaded items and lose all of the changes?"),
				Res.GetString("96569368-D783-46FF-9DD1-FD05CEA1DCEB", "Unloaded Items Reset"),
				Res.GetString("F16AD969-4232-4789-8724-8A2D4C6C52FF", "yes"),
				MessageBoxIcon.Question) == DialogResult.OK)
			{
				nctsMovement.ResetUnloadedValues();
			}
		}

		void VehicleIdChangedValueTextBox_ReadOnlyChanged(object sender, EventArgs e)
		{
			var value = VehicleIdChangedValueTextBox.ReadOnly;
			ResetUnloadedItemsButton.Enabled = !value;
			ReCalcTotalsButton.Enabled = !value;
		}
	}
}
