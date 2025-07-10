using System;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	sealed class EMCSMessageSendingActionParentForTesting : EMCSMessageSendingActionParent<EMCSMessageSendingAction>
	{
		public EMCSMessageSendingActionParentForTesting(EMCSJobDeclaration declaration) : base(declaration) { }

		protected override Type SendingObjectCollectionType => typeof(EMCSMessageSendingActionCollectionForTesting);
	}

	sealed class EMCSMessageSendingActionCollectionForTesting : EMCSMessageSendingActionCollection<EMCSMessageSendingAction>
	{
		public EMCSMessageSendingActionCollectionForTesting(EMCSMessageSendingActionParentForTesting sendingParent) : base(sendingParent) { }
	}
}
