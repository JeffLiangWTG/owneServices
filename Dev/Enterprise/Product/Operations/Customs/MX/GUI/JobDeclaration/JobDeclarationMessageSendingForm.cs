using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.MX.GUI
{
	public partial class JobDeclarationMessageSendingForm : MessageSendingFormWithValidationDetails
	{
		public JobDeclarationMessageSendingForm(BaseMessageSendingObjectParent messageSendingObjectParent)
			: base(messageSendingObjectParent)
		{
			InitializeComponent();
		}
	}
}
