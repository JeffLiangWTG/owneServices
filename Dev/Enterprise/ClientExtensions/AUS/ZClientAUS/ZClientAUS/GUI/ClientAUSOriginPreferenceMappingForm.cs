using System;
using CargoWise.Windows.UI;
using Enterprise.Client.AUS.Business;
using Enterprise.ZArchitecture.GUI;
#if DEBUG
#endif

namespace Enterprise.Client.AUS.GUI
{
	public partial class ClientAUSOriginPreferenceMappingForm : ZForm
	{
		public ClientAUSOriginPreferenceMappingForm(ClientAUSOriginPreferenceMapping orgPrefMapping) : base(orgPrefMapping)
		{
			InitializeComponent();
			this.OrgPrefMapping = orgPrefMapping;
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
			MinimumSize = Size;
		}

		public readonly ClientAUSOriginPreferenceMapping OrgPrefMapping;

		public override string FormCaption
		{
			get { return "Origin-Preference Mapping"; }
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			ControlDpiScalingHelper.SetTop(ref PostingButtonsUserControl, MainStatusBar.Top - PostingButtonsUserControl.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
		}
	}
}
