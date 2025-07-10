using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public static class LocalExportEntryHeaderWrapperDecorator
	{
		public static void Decorate(this LocalExportEntryHeaderWrapper wrapper, CusEntryHeader entry)
		{
			wrapper.EntryStatus = entry.CH_Status;
			wrapper.RR3Message = GetMessage(wrapper.HeaderType, entry.Messages, ElectronicDocumentTypeList.Codes._RR3);
			wrapper.R38Message = GetMessage(wrapper.HeaderType, entry.Messages, ElectronicDocumentTypeList.Codes._R38);
			wrapper.MostRecentCESLogSLEventTime = entry.MostRecentCESLog?.SL_EventTime ?? ZDateTime.Empty;
			wrapper.CustomsOfficeName = MessageFunctions.GetCustomsOffice(entry.Factory, entry.Declaration.JE_CustomsOffice);
			wrapper.Broker = new OrganizationDocWrapper(entry.Declaration.BrokerAddress);
		}

		static EDIMessage GetMessage(ZString headerType, EDIMessageCollection messages, ZString messageType)
		{
			EDIMessage result = null;

			var latestSendMessage = messages.Cast<EDIMessage>().Where(x => x.EM_MessageType == headerType).OrderBy(x => x.EM_SystemCreateTimeUtc).LastOrDefault();
			if (latestSendMessage != null)
			{
				result = messages.Cast<EDIMessage>().FirstOrDefault(x => x.EM_MessageType == messageType && x.EM_ApplicationReference == latestSendMessage.EM_MessageNum && x.EM_ReceiveTransmit == EDIMessage.Direction.Receive);
			}

			return result;
		}
	}
}
