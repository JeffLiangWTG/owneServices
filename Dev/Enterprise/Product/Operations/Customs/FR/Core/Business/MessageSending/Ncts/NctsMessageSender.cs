using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business.MessageGeneration;

namespace Enterprise.Customs.FR.Business.MessageSending
{
	public class NctsMessageSender : INctsMessageSender
	{
		public NctsMessageSender() { }

		#region INctsMessageSender Members

		public bool CreateMessage(EU.NCTS.Business.NctsHeader nctsHeader, ISendsMessagesToCustoms sendMessagesToCustoms, NctsMessageFunctionSet messageFunction)
		{
			if (nctsHeader != null)
			{
				var transmissionGenerator = new FrNctsTransmissionMessageGenerator(messageFunction);
				var messageManager = new NctsMessageManager(nctsHeader, transmissionGenerator);
				return messageManager.SendNctsMessage(sendMessagesToCustoms);
			}
			return false;
		}

		#endregion
	}
}
