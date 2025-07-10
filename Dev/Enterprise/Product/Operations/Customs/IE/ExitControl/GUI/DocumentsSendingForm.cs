using Enterprise.Customs.GUI;
using Enterprise.Customs.IE.ExitControl.Business;

namespace Enterprise.Customs.IE.ExitControl.GUI
{
	public partial class DocumentsSendingForm : MessageSendingObjectForm
	{
		public DocumentsSendingForm()
		{
			InitializeComponent();
		}

		public DocumentsSendingForm(DocumentsSendingActionParent parent) : base(parent)
		{
			InitializeComponent();
		}

		public override string FormHeading => Res.GetString("700EC983-4CCD-401C-98C0-AC7DDBDF0A41", "Upload Supporting Documents");
	}
}
