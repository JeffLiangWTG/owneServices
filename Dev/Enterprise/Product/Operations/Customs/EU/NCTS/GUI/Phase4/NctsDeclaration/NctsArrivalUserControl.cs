using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class NctsArrivalUserControl : ZUserControl
	{
		public NctsArrivalUserControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			header = dataSource as NctsHeader;
			if (header != null)
			{
				header.BH_OverrideFreightDefaultsInfo.ValueChanged -= BH_OverrideFreightDefaultsInfo_ValueChanged;
				header.BH_OverrideFreightDefaultsInfo.ValueChanged += BH_OverrideFreightDefaultsInfo_ValueChanged;
				SetFreightDefaultsVisibility();
			}
		}

		void BH_OverrideFreightDefaultsInfo_ValueChanged(object sender, System.EventArgs e)
		{
			SetFreightDefaultsVisibility();
		}

		void SetFreightDefaultsVisibility()
		{
			OverrideFreightDefaults.Visible = header.IsPluggedIn;
		}

		NctsHeader header;
	}
}
