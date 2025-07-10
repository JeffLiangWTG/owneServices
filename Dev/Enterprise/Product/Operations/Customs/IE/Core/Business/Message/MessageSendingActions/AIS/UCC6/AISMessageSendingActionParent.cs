using System;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business
{
	public sealed class AISMessageSendingActionParent : CusEntryHeaderMessageSendingActionParent<AISMessageSendingAction>
	{
		public AISMessageSendingActionParent(JobDeclaration declaration) : base(declaration) { }

		protected override Type SendingObjectCollectionType => typeof(AISMessageSendingActionCollection);
	}
}
