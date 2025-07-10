using Enterprise.Client.UPE.Business;

namespace Enterprise.Client.UPE.GUI
{
	public partial class RTSTranshipmentForm : CustomFlagForm
	{
		public RTSTranshipmentForm(RTSTranshipmentDetails flagDetails)
			: base(flagDetails)
		{
		}

		protected override void OnOKDialogResult()
		{
			FlagDetails.UpdateCusHAWBDetails();
		}

		RTSTranshipmentDetails FlagDetails
		{
			get { return (RTSTranshipmentDetails)BusinessEntity; }
		}

		protected override void InitialiseForm()
		{
			base.InitializeComponent();
			InitializeComponent();
		}
	}
}
