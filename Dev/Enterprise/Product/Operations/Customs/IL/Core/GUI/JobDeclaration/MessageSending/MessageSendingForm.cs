using Enterprise.Customs.GUI;
using Enterprise.Customs.IL.Business;

namespace Enterprise.Customs.IL.GUI
{
	public partial class MessageSendingForm : MessageSendingFormWithValidationDetails
	{
		public MessageSendingForm(JobDeclarationMessageSendingObjectParent declarationWrapper)
			: base(declarationWrapper)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}
	}
}
