using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public partial class SeaCargoDepotOutturnBackdoorForSavingOnAmendmentForm : ZChildForm
	{
		public SeaCargoDepotOutturnBackdoorForSavingOnAmendmentForm()
		{
			InitializeComponent();
		}

		public SeaCargoDepotOutturnBackdoorForSavingOnAmendmentForm(DeferredCusOutturnHeaderSavingOptions options)
			: base(options)
		{
			InitializeComponent();
			this.OutturnNameLabel.Text = "Outturn Report for " + options.OutturnReference;
		}

		void AgreeButton_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}
	}
}
