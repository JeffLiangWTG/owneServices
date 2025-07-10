using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	public partial class LVSSubHeaderAllDetailsUserControl : ZUserControl
	{
		public LVSSubHeaderAllDetailsUserControl()
		{
			InitializeComponent();
		}

		internal void SetGroupBoxText(string text)
		{
			this.DetailsGroupBox.Text = text;
		}

		protected override void OnAfterFirstBinding(System.EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			var declaratoin = (JobDeclaration)DataSource;
			if (declaratoin == null || !declaratoin.IsLVX)
			{
				this.DeliveryDocAddressControl.Visible = false;
				this.ShipperDocAddressControl.Visible = false;
			}
		}
	}
}
