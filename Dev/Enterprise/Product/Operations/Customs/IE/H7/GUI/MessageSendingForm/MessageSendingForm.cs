using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.H7.GUI
{
	public sealed partial class MessageSendingForm : EU.H7.GUI.MessageSendingForm
	{
		public MessageSendingForm(Customs.Business.BaseMessageSendingObjectParent parent)
			: base(parent)
		{
		}

		protected override IGridColumnLayoutProvider GetNewColumnLayoutProvider() => new MessageSendingGridColumnLayout();
	}
}
