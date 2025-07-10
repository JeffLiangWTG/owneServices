using Enterprise.Customs.CH.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public static class MessageManagerFactory
{
	public static IMessageManager CreateNew(IMessageSendingObject messageSendingObject)
	{
		switch (messageSendingObject)
		{
			case NctsHeaderDepartureMessageSendingObject departureObjectToSend:
				switch (departureObjectToSend.MessageType)
				{
					case PassarMessageTypeList.Codes.NT013:
						return new NT013MessageManager(departureObjectToSend);
					case PassarMessageTypeList.Codes.NT014:
						return new NT014MessageManager(departureObjectToSend);
					case PassarMessageTypeList.Codes.NT015:
						return new NT015MessageManager(departureObjectToSend);
					case PassarMessageTypeList.Codes.NT141:
						return new NT141MessageManager(departureObjectToSend);
					case PassarMessageTypeList.Codes.NT513:
						return new NT513MessageManager(departureObjectToSend);
					case PassarMessageTypeList.Codes.NT515:
						return new NT515MessageManager(departureObjectToSend);
					case PassarMessageTypeList.Codes.NC016:
						return new NC016MessageManager(departureObjectToSend);
					case PassarMessageTypeList.Codes.NC123:
						return new NC123MessageManager(departureObjectToSend);
				}
				break;
			case NctsHeaderArrivalMessageSendingObject arrivalObjectToSend:
				switch (arrivalObjectToSend.MessageType)
				{
					case PassarMessageTypeList.Codes.NT007:
						return new NT007MessageManager(arrivalObjectToSend);
					case PassarMessageTypeList.Codes.NT044:
						return new NT044MessageManager(arrivalObjectToSend);
				}
				break;
			case CharteraOutputDocumentSearchSendingObject charteraOutputDocumentSearchSendingObjectToSend:
				return new CharteraOutputDocumentSearchRequestMessageManager(charteraOutputDocumentSearchSendingObjectToSend);
		}
		return null;
	}
}
