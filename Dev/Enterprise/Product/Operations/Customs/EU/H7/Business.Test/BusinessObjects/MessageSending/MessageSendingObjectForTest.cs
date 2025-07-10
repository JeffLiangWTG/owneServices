using System;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	public class MessageSendingObjectForTest : MessageSendingObject
	{
		public MessageSendingObjectForTest(AsycudaBill bill)
			: base(bill)
		{
		}

		public Func<MessageSendingObjectForTest, MessageSender> CreateSenderForTesting;

		public override MessageSender CreateSender() => CreateSenderForTesting?.Invoke(this) ?? new MessageSenderForTest(this);

		public override bool IsAmendmentAction => Action == "H7M";

		public override bool IsCancellationAction => Action == "H7C";

		protected override CodeDescriptionPairList GetActionList() => new CodeDescriptionPairList();

		protected override CodeDescriptionPairList GetSubStyleList() => new CodeDescriptionPairList();

		protected override CodeDescriptionPairList GetAmendmentReasonCodeList()
		{
			var codeList = new CodeDescriptionPairList();

			if (IsAmendmentAction)
			{
				codeList.AddPair("A1", "Amendment Reason 1");
				codeList.AddPair("A2", "Amendment Reason 2");
			}
			else if (IsCancellationAction)
			{
				codeList.AddPair("C1", "Cancellation Reason 1");
				codeList.AddPair("C2", "Cancellation Reason 2");
			}

			return codeList;
		}

		public bool AmendmentInvalidationReason_ReadOnlyForTest => AmendmentInvalidationReason_ReadOnly;
	}
}
