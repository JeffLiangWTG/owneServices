using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("Soon to be used")]
	public static class EDIMessageSupporter
	{
		public static GlbStaff GetOriginalSender(this EDIMessage[] messages, ZString transmitMessageType)
		{
			var filteredMessages = messages.Where(x => x.EM_ApplicationCode == MessageApplicationCode
										&& x.EM_ReceiveTransmit == EDIMessage.Direction.Transmit
										&& x.EM_MessageType == transmitMessageType);
			var latestMessage = EDIMessageComparer.GetSortedMessages(filteredMessages, System.ComponentModel.ListSortDirection.Descending).FirstOrDefault();
			return latestMessage != null ? latestMessage.UserWhoQueuedThisRecord : null;
		}

		static string MessageApplicationCode => EDIMessage.ApplicationCodes.KRCustoms;
	}
}
