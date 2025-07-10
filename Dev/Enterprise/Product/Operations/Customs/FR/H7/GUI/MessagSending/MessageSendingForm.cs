using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.H7.GUI
{
	public sealed class MessageSendingForm : EU.H7.GUI.MessageSendingForm
	{
		public MessageSendingForm(BaseMessageSendingObjectParent parent)
			: base(parent)
		{
		}

		protected override IGridColumnLayoutProvider MessageSendingGridColumnLayoutProvider => new MessageSendingGridColumnLayout();
	}
}
