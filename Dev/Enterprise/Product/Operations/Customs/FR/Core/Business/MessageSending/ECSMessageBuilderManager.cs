using System;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Customs.FR.Business.MessagesWrappers.ECS;
using Enterprise.Customs.FR.Messaging.MessageBuilders.ECS;

namespace Enterprise.Customs.FR.Business.MessageSending
{
	public class ECSMessageBuilderManager
	{
		public ECSMessageBuilderManager(CusExitDetail cusExitDetail)
		{
			ecsExitDetail = Argument.NotNull(cusExitDetail, nameof(cusExitDetail));
		}

		public IMessageBuilderBase NewMessageBuilder(ZString messageSubType)
		{
			if (messageSubType.EqualsIgnoringCase(MessageSubTypeList.Codes.ARR))
			{
				return new ECSSendIE507MessageBuilder(new IE507DataWrapper(ecsExitDetail), Messaging.MessageBuilders.TransactionTypes.Original);
			}
			else if (messageSubType.EqualsIgnoringCase(MessageSubTypeList.Codes.DEP))
			{
				return new ECSSendIE618MessageBuilder(new IE618DataWrapper(ecsExitDetail), Messaging.MessageBuilders.TransactionTypes.Original);
			}
			throw new NotImplementedException("CW1 doesn't yet support building message type ECS " + messageSubType);
		}

		public ECSFREDIMessage AddNewMessage(ZString messageSubType)
		{
			ECSFREDIMessage message;

			if (messageSubType.EqualsIgnoringCase(MessageSubTypeList.Codes.ARR))
			{
				message = ecsExitDetail.Factory.New<ECSArrivalFREDIMessage>();
			}
			else if (messageSubType.EqualsIgnoringCase(MessageSubTypeList.Codes.DEP))
			{
				message = ecsExitDetail.Factory.New<ECSDepartureFREDIMessage>();
			}
			else
			{
				throw new NotImplementedException("CW1 doesn't yet support building message type ECS " + messageSubType);
			}

			if (message != null)
			{
				ecsExitDetail.Messages.Add(message);
			}

			return message;
		}

		readonly CusExitDetail ecsExitDetail;
	}
}
