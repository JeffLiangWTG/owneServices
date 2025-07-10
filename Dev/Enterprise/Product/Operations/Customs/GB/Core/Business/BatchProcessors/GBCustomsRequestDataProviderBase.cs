using CargoWise.Customs.GB.MessageDefinitions;
using CargoWise.Types;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;

namespace Enterprise.Customs.GB.Business
{
	public abstract class GBCustomsRequestDataProviderBase
	{
		protected abstract ZString GetCredentialsKey();
		protected abstract CredentialsSetting GetCredentialsSetting();
		protected virtual ZString GetCredentialPartyID() => ZString.Empty;
		protected abstract ZString GatewayCore { get; }
		protected abstract ZString JobNumberCore { get; }

		public ZString JobNumber => JobNumberCore;
		public ZString Gateway => GatewayCore;

		public Credentials Credentials
		{
			get
			{
				Credentials credentials;

				var key = GetCredentialsKey();
				var partyID = GetCredentialPartyID();
				switch (GatewayCore)
				{
					case GatewayList.Codes.CCSUKviaNTMsgGW:
					case GatewayList.Codes.CNS_CUSDECOnly:
					case GatewayList.Codes.MCP_CUSDECOnly:
					case GatewayList.Codes.Pentant:
						var credentialsSetting = GetCredentialsSetting();
						var password = credentialsSetting?.Password ?? ZString.Empty;
						credentials = new Credentials
						{
							Key = key,
							User = credentialsSetting?.Username ?? ZString.Empty,
							Password = password.IsEmpty ? null : CredentialSender.EncryptPasswordAsString(password),
							Topic = credentialsSetting?.Printer ?? ZString.Empty,
							Badge = GatewayCore == GatewayList.Codes.Pentant ? credentialsSetting?.BadgeCode ?? ZString.Empty : credentialsSetting?.Company ?? ZString.Empty,
							PartyID = partyID.IsEmpty ? null : partyID
						};
						break;
					default:
						credentials = new Credentials
						{
							Key = key
						};
						break;
				}

				return credentials;
			}
		}
	}
}
