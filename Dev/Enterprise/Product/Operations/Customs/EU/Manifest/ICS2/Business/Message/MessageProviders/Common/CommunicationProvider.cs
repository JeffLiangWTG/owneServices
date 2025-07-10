using CargoWise.Customs.EU.MessageContracts.ICS2;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class CommunicationProvider : IIdentifierTypePair
	{
		public CommunicationProvider(string communicationInfo, string type)
		{
			Identifier = communicationInfo;
			Type = type;
		}

		public string Identifier { get; }

		public string Type { get; }
	}
}
