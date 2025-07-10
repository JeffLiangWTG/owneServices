using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business.MessageGeneration;

namespace Enterprise.Customs.GB.GovernmentGateway.GatewayApplications.NCTS
{
	public class NctsMessageSender : INctsMessageSender
	{
		public NctsMessageSender() { }

		#region INctsMessageSender Members

		public bool CreateMessage(EU.NCTS.Business.NctsHeader nctsHeader, ISendsMessagesToCustoms sendMessagesToCustoms, NctsMessageFunctionSet messageFunction)
		{
			if (nctsHeader != null)
			{
				var transmissionGenerator = new NctsTransmissionMessageGenerator(messageFunction);
				var messageManager = new NctsMessageManager(nctsHeader, transmissionGenerator);
				messageManager.SendNctsMessage(sendMessagesToCustoms);
			}
			return true;
		}

		#endregion
	}
}
