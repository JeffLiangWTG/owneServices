using Enterprise.Customs.FR.NCTS.Messaging;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.NCTS
{
	public partial class MessageSendingForm : EU.NCTS.GUI.MessageSendingForm
	{
		public MessageSendingForm(TP5MessageSendingObjectParent sendingObjectWrapper) : base(sendingObjectWrapper)
		{
			InitializeComponent();
		}

		public new TP5MessageSendingObjectParent MessageSendingObjectParent => base.MessageSendingObjectParent as TP5MessageSendingObjectParent;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		protected override ZUserControl GetBottomSectionUserControl()
		{
			if (bottomSectionUserControl == null)
			{
				bottomSectionUserControl = new MessageSendingFormBottomSectionUserControl();
			}
			return bottomSectionUserControl;
		}
		MessageSendingFormBottomSectionUserControl bottomSectionUserControl;
	}
}
