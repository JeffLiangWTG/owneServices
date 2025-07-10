using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	public partial class LPCODetailUserControl : ZUserControl
	{
		public LPCODetailUserControl()
		{
			InitializeComponent();
		}

		public void HideIssuanceCountry()
		{
			zLabelIssuanceCountryCaption.Hide();
			zLabelIssuanceCountry.Hide();
		}

		public void HideCountryOfAuth()
		{
			zLabelCountryOfAuthCaption.Hide();
			zLabelCountryOfAuth.Hide();
		}

		public void HideMixed()
		{
			zLabelMixedCaption.Hide();
			zLabelMixed.Hide();
		}

		public void HideExpiryDate()
		{
			zLabel1ExpiryDateCaption.Hide();
			zLabelExpiryDate.Hide();
		}
	}
}
