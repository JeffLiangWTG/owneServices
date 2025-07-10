using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.NCTS.Business.MessageGeneration
{
	public class NctsMessageSenderChooser : NctsChooser<INctsMessageSender>, INctsMessageSender
	{
		public NctsMessageSenderChooser(NctsHeader nctsHeader, NctsMessageFunctionSet messageFunction)
			: base("EU.NCTS.Business.MessageGeneration.INctsMessageSender", GetNctsCountryFromNctsHeader(nctsHeader, messageFunction))
		{
		}

		#region INctsMessageSender Members

		public bool CreateMessage(NctsHeader nctsHeader, ISendsMessagesToCustoms sendMessagesToCustoms, NctsMessageFunctionSet messageFunction)
		{
			var couldFindCountrySpecificSender = false;
			var messageSender = CountrySpecificChooser;
			if (messageSender != null)
			{
				messageSender.CreateMessage(nctsHeader, sendMessagesToCustoms, messageFunction);
				couldFindCountrySpecificSender = true;
			}
			return couldFindCountrySpecificSender;
		}

		#endregion
	}
}
