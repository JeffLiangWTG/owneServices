using System;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business
{
	public class CopyAndSendToCustomsSendingActionParent : CusEntryHeaderMessageSendingActionParent<CopyAndSendToCustomsSendingAction>
	{
		public CopyAndSendToCustomsSendingActionParent(JobDeclaration declaration, ZString messageType) : base(declaration)
		{
			this.MessageType = messageType;
			MergeIfNeeded(declaration);
		}
		public readonly ZString MessageType;

		void MergeIfNeeded(JobDeclaration declaration)
		{
			var needMerge = declaration != null && (!declaration.IsMergeDone || declaration.MergeManager.RequiresMerge);

			if (needMerge)
			{
				declaration.DoMerge();
			}
		}

		protected override Type SendingObjectCollectionType => typeof(CopyAndSendToCustomsSendingActionCollection);
	}
}
