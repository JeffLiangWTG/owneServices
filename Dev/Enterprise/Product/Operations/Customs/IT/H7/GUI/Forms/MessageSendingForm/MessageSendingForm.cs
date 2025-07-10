using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.H7.GUI;

public partial class MessageSendingForm : EU.H7.GUI.MessageSendingForm
{
	public MessageSendingForm(BaseMessageSendingObjectParent parent)
		: base(parent)
	{
	}

	protected override IGridColumnLayoutProvider GetNewColumnLayoutProvider() => new MessageSendingGridColumnLayout();

	protected override bool AllowOverrideAmendmentReason => true;

	protected override bool AllowOverrideCancellationReason => true;
}
