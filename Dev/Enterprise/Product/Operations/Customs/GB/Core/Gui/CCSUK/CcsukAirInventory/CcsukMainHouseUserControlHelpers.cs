using Enterprise.Customs.GB.Ccsuk.AirCargoInventory;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	public partial class CcsukMainHouseUserControlHelpers : ZUserControl
	{
		public CcsukMainHouseUserControlHelpers()
		{
			InitializeComponent();
		}

		protected override void OnLoad(System.EventArgs e)
		{
			// Is there a better way?
			base.OnLoad(e);
			if (BindingSource.DataSource != null)
			{
				if (BindingSource.DataSource as CusHAWB != null)
				{
					SetCusHawb((CusHAWB)BindingSource.DataSource);
				}
			}
			else
			{
				BindingSource.DataSourceChanged -= new System.EventHandler(BindingSource_DataSourceChanged);
				BindingSource.DataSourceChanged += new System.EventHandler(BindingSource_DataSourceChanged);
			}
		}

		void BindingSource_DataSourceChanged(object sender, System.EventArgs e)
		{
			if (BindingSource.DataSource as CusHAWB != null)
			{
				SetCusHawb((CusHAWB)BindingSource.DataSource);
			}
		}

		internal void SetCusHawb(CusHAWB cusHawb)
		{
			SplitsTabPage.TabVisible = cusHawb != null && cusHawb.HasSplits;
			UpdateDeliveryTabPageVisibility(cusHawb);
			UpdateSplitsTabVisibility(cusHawb);
			cusHawb.OnPimaChanged += UpdateDeliveryTabPageVisibility;
			cusHawb.OnSplitsCountChanged += UpdateSplitsTabVisibility;
		}

		void UpdateSplitsTabVisibility(ICcsukCusAwb awb)
		{
			SplitsTabPage.TabVisible = awb != null && awb.HasSplits;
		}

		void UpdateDeliveryTabPageVisibility(ICcsukCusAwb awb)
		{
			DeliveryTabPage.TabVisible = awb != null && awb.IsInDatabase && (LicenceAndPimaHelper.IsFullShed(awb) || LicenceAndPimaHelper.IsFallbackShed(awb));
		}
	}
}
