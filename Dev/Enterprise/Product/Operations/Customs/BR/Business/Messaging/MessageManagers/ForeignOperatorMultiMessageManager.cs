using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BR.Business
{
	public class ForeignOperatorMultiMessageManager : MultiMessageManager, IMessageManager
	{
		public ForeignOperatorMultiMessageManager(ForeignOperatorMessageSendingObject messageSendingObject)
		{
			MessageSendingObject = Argument.NotNull(messageSendingObject, nameof(messageSendingObject));
		}

		public override string CannotSaveWhenWaitingForResponse => Res.GetString("C474080A-550C-4819-B8CB-8551CB679A1F", "This Foreign Operator cannot be edited because a message is awaiting a response (Message Status: Awaiting Response).");

		public override IMessageManageableBizObj TopLevelBizObjToManage => MessageSendingObject.ForeignOperator;

		protected override bool SendWheneverPossibleOnceMessagingActive => false;

		protected override SingleMessageManager[] GetAllMessageManagers()
		{
			var list = new List<SingleMessageManager>();
			list.Add(new ForeignOperatorMessageManager(MessageSendingObject));
			return list.ToArray();
		}

		public readonly ForeignOperatorMessageSendingObject MessageSendingObject;
	}
}
