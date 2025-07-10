using Enterprise.Client.EDI.Billing.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.Billing.GUI
{
	public partial class BillingExcludeOrgForm : ZChildForm
	{
		public BillingExcludeOrgForm()
		{
			InitializeComponent();
		}

		public BillingExcludeOrgForm(ClientLicenceBillingExcludeOrgUpdater updater)
			: base(updater)
		{
			InitializeComponent();
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
		}

		protected override bool AllowNew
		{
			get { return false; }
		}
	}
}

