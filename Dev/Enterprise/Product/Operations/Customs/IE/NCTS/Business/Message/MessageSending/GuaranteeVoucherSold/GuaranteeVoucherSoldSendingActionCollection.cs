using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class GuaranteeVoucherSoldSendingActionCollection : NonPersistentBusinessObjectCollection<GuaranteeVoucherSoldSendingAction>
	{
		public GuaranteeVoucherSoldSendingActionCollection(GuaranteeVoucherSoldSendingActionParent sendingParent) : base(sendingParent.TopLevelBusinessObject.Factory)
		{
			Parent = sendingParent;
			AddSendingAction();
		}

		void AddSendingAction()
		{
			var sendingAction = new GuaranteeVoucherSoldSendingAction(Parent.Header);
			sendingAction.MessageType = Parent.MessageType;
			Add(sendingAction);
		}

		protected GuaranteeVoucherSoldSendingActionParent Parent { get; }

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new InvalidOperationException("Message Sending Action Collection should not support adding.");
		}

		protected sealed override bool AllowNewCore => false;

		protected sealed override bool AllowRemoveCore => false;
	}
}
