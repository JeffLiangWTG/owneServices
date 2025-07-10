using Enterprise.Customs.BR.Business;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.BR.GUI
{
	public partial class LPCOMessageSendingForm : MessageSendingFormWithValidationDetails
	{
		public LPCOMessageSendingForm(LPCOMessageSendingObjectParent sendingObjectParent)
			: base(sendingObjectParent)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected override bool CheckIsOKToSend()
		{
			return base.CheckIsOKToSend() && MessageSendingEnviromentChecker.CheckIsOKToSend();
		}
	}
}
