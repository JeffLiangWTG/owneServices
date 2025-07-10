using Enterprise.Customs.GUI;
using Enterprise.Customs.IE.PBN.Business;

namespace Enterprise.Customs.IE.PBN.GUI
{
	public partial class PBNMessageSendingForm : MessageSendingFormWithValidationDetails
	{
		public PBNMessageSendingForm(PBNMessageSendingObjectParent messageSendingObjectParent) : base(messageSendingObjectParent)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}
	}
}
