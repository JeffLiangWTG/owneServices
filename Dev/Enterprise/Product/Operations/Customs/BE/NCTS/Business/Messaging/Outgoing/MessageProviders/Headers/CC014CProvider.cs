using System;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class CC014CProvider : NctsDepartureHeaderProvider, ICC014C
	{
		readonly MessageSendingAction sendingAction;
		public CC014CProvider(MessageSendingAction sendingAction) : base(Argument.NotNull(sendingAction, nameof(sendingAction)).Header)
		{
			this.sendingAction = sendingAction;
		}

		public DateTime InvalidationRequestDateAndTime => DateTimeProviderHelper.ConvertToUnspecifiedDateTimeKindIfPossible(ZDateTime.UtcNow.ToDateTime(), removeMillisecond: true);

		public DateTime InvalidationDecisionDateAndTime => MessageRecipient.StartsWith(Constants.RecepientTypes.NTA) ? DateTime.MinValue : DateTimeProviderHelper.ConvertToUnspecifiedDateTimeKindIfPossible(ZDateTime.UtcNow.ToDateTime(), removeMillisecond: true);

		public bool InvalidationDecision => !MessageRecipient.StartsWith(Constants.RecepientTypes.NTA);

		public bool InvalidationInitiatedByCustoms => false;

		public string InvalidationJustification => sendingAction.Justification;

		public override string MessageType => Constants.MessageTypes.CC014C;

		public override string LRN => MRN.IsEmpty() ? base.LRN : null;
	}
}
