using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using CargoWise.Customs.Shared.MessageContracts;
using Enterprise.Customs.CH.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public static class MessageBuilderFactory
{
	public static IXmlMessageBuilder NewMessageBuilder(IMessageSendingObject objectToSend)
	{
		IXmlMessageBuilder messageBuilder = null;

		switch (objectToSend)
		{
			case NctsHeaderDepartureMessageSendingObject departureMessageSendingObject:
				switch (departureMessageSendingObject.MessageType)
				{
					case PassarMessageTypeList.Codes.NT013 when FuncsHelper.IsCHNT015V4Active:
						messageBuilder = new NT013_v4MessageBuilder(new NT013DataProvider(departureMessageSendingObject));
						break;
					case PassarMessageTypeList.Codes.NT013:
						messageBuilder = new NT013_v5MessageBuilder(new NT013DataProvider(departureMessageSendingObject));
						break;
					case PassarMessageTypeList.Codes.NT014:
						messageBuilder = new NT014_v3MessageBuilder(new NT014DataProvider(departureMessageSendingObject));
						break;
					case PassarMessageTypeList.Codes.NT015 when FuncsHelper.IsCHNT015V4Active:
						messageBuilder = new NT015_v4MessageBuilder(new NT015DataProvider(departureMessageSendingObject));
						break;
					case PassarMessageTypeList.Codes.NT015:
						messageBuilder = new NT015_v5MessageBuilder(new NT015DataProvider(departureMessageSendingObject));
						break;
					case PassarMessageTypeList.Codes.NT141:
						messageBuilder = new NT141_v2MessageBuilder(new NT141DataProvider(departureMessageSendingObject));
						break;
					case PassarMessageTypeList.Codes.NT513 when FuncsHelper.IsCHNT515V4Active:
						messageBuilder = new NT513_v4MessageBuilder(new NT513DataProvider(departureMessageSendingObject));
						break;
					case PassarMessageTypeList.Codes.NT513:
						messageBuilder = new NT513_v5MessageBuilder(new NT513DataProvider(departureMessageSendingObject));
						break;
					case PassarMessageTypeList.Codes.NT515 when FuncsHelper.IsCHNT515V4Active:
						messageBuilder = new NT515_v4MessageBuilder(new NT515DataProvider(departureMessageSendingObject));
						break;
					case PassarMessageTypeList.Codes.NT515:
						messageBuilder = new NT515_v5MessageBuilder(new NT515DataProvider(departureMessageSendingObject));
						break;
					case PassarMessageTypeList.Codes.NC016:
						messageBuilder = new NC016_v1MessageBuilder(new NC016DataProvider(departureMessageSendingObject));
						break;
					case PassarMessageTypeList.Codes.NC123:
						messageBuilder = new NC123_v1MessageBuilder(new NC123DataProvider(departureMessageSendingObject));
						break;
				}
				break;
			case NctsHeaderArrivalMessageSendingObject arrivalMessageSendingObject:
				switch (arrivalMessageSendingObject.MessageType)
				{
					case PassarMessageTypeList.Codes.NT007:
						messageBuilder = new NT007_v3MessageBuilder(new NT007DataProvider(arrivalMessageSendingObject));
						break;
					case PassarMessageTypeList.Codes.NT044 when FuncsHelper.IsCHNT044V4Active:
						messageBuilder = new NT044_v4MessageBuilder(new NT044DataProvider(arrivalMessageSendingObject));
						break;
					case PassarMessageTypeList.Codes.NT044:
						messageBuilder = new NT044_v5MessageBuilder(new NT044DataProvider(arrivalMessageSendingObject));
						break;
				}
				break;
		}

		return messageBuilder;
	}
}
