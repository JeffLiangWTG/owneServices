using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	public partial class CcsukHawbControl : ZUserControl
	{
		public CcsukHawbControl()
		{
			InitializeComponent();
			this.HouseGroupBox.BringToFront();

			HouseGroupBox.AllowOverlap(ccsukMasterControl1);
			awbStatusUserControl1.AllowOutsideOfParent();
			ccsukAirportsAndPartiesControl1.AllowOutsideOfParent();
			ccsukAirportsAndPartiesControl1.AllowOverlap(ccsukMasterControl1);
		}

		public CcsukHawbControl(CusHAWB cusHawb)
			: this()
		{
			ccsukMainHouseUserControlForPlugin1.SetCusHawb(cusHawb);
		}

		protected override void OnLoad(System.EventArgs e)
		{
			base.OnLoad(e);
			var hawb = HawbBindingSource;
			if (hawb == null || (hawb.MAWB != null && hawb.MAWB.Consol != null))
			{
				LinkLabelMawb.Visible = false;
			}
		}

		void LinkLabelEntry_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			var hawb = HawbBindingSource;
			if (hawb != null && hawb.Declaration != null)
			{
				new DependentObjectControllerHelper(new DependentFormPresenter()).EditExisting(hawb.Declaration, ControllerIDs.Customs.JobDeclaration);
			}
		}

		void LinkLabelMawb_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			var hawb = HawbBindingSource;
			if (hawb != null)
			{
				new DependentObjectControllerHelper(new DependentFormPresenter()).EditExisting(HawbBindingSource.MAWB, ControllerIDs.Customs.GB.CcsukAirInventory);
			}
		}

		CusHAWB HawbBindingSource
		{
			get { return BindingSource.DataSource as CusHAWB; }
		}
	}
}
