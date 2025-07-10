using System;
using Enterprise.Customs.ES.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.GUI
{
	public partial class GoodsItemPackagesUserControl : EU.NCTS.GUI.GoodsItemPackagesUserControl
	{
		public GoodsItemPackagesUserControl() : base()
		{
		}

		bool IsPhase5Vehicle
		{
			get
			{
				var isVehicle = false;
				if (CurrentDataItem is NctsDepartureCargoDesc goodsItem)
				{
					isVehicle = goodsItem.IsVehicles;
				}

				return (DataSource?.IsPhase5 ?? false) && isVehicle;
			}
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			UnhookEvents();
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			HookEvents();
			SetPackagesGridColumnsVisibility();
		}

		void HookEvents()
		{
			if (CurrentDataItem is NctsDepartureCargoDesc goodsItem)
			{
				goodsItem.IsVehiclesInfo.ValueChanged += IsVehicle_ValueChanged;
			}
		}

		void UnhookEvents()
		{
			if (CurrentDataItem is NctsDepartureCargoDesc goodsItem)
			{
				goodsItem.IsVehiclesInfo.ValueChanged -= IsVehicle_ValueChanged;
			}
		}

		void IsVehicle_ValueChanged(object sender, EventArgs e) => SetPackagesGridColumnsVisibility();

		void SetPackagesGridColumnsVisibility()
		{
			var isPhase5Vehicle = IsPhase5Vehicle;

			PackagesGrid.SuspendLayout();

			PackagesGrid.SetColumnVisible(isPhase5Vehicle, NctsPackage.Schema.B5_PackageID);
			PackagesGrid.SetColumnVisible(isPhase5Vehicle, NctsPackage.Schema.B5_Brand);
			PackagesGrid.SetColumnVisible(isPhase5Vehicle, NctsPackage.Schema.B5_Model);

			PackagesGrid.SetColumnVisible(!isPhase5Vehicle, NctsPackage.Schema.B5_UnitType);
			PackagesGrid.SetColumnVisible(!isPhase5Vehicle, NctsPackage.Schema.B5_UnitCount);
			PackagesGrid.SetColumnVisible(!isPhase5Vehicle, NctsPackage.Schema.B5_MarksAndNumbers);

			PackagesGrid.ResumeLayout();
			PackagesGrid.PerformLayout();
		}
	}
}
