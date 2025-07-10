using Enterprise.Customs.ES.Business.MessageSending;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.ES.GUI
{
	public partial class ECSMessageSendingForm : MessageSendingFormWithValidationDetails
	{
		public ECSMessageSendingForm(ECSExitHeaderMessageSendingObjectParent exitHeaderWrapper)
			: base(exitHeaderWrapper)
		{
			InitializeComponent();
		}

		public new ECSExitHeaderMessageSendingObjectParent BusinessEntity => (ECSExitHeaderMessageSendingObjectParent)base.BusinessEntity;
	}
}
