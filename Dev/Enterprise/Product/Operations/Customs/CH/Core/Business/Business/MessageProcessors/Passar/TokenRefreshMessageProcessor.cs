using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CH.Business;

internal class TokenRefreshMessageProcessor : BaseMessageProcessor
{
	public TokenRefreshMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string ApplicationCodeCore => ApplicationCodeList.Codes.CHCustomsPassar;

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeCodeList.Codes.TRE };

	protected override string MessageFriendlyNameCore => Res.GetString("8A20951F-9EEE-4337-A662-FAAF049B5AB5", "Token Refresh Message Processor");

	protected override void ProcessMessageCore(CHEDIMessage message)
	{
		if (FindLinkedObjectByOutgoingSessionID(message) is GlbCompany company)
		{
			message.EM_LinkedObject = company;
			ProcessResponseMessage(message, company);
		}
	}

	void ProcessResponseMessage(CHEDIMessage message, GlbCompany company)
	{
		var wrapper = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(company);

		if (message.EM_MessageSubType == MessageSubTypeCodeList.Codes.Accepted)
		{
			var response = JsonSerializer.Deserialize<TokenResponseSuccess>(message.EM_MessageText);

			var tokenCredentials = UpdatePasswordStatus(wrapper);
			tokenCredentials.GP_Certificate = new ZBlob(Encoding.ASCII.GetBytes(response.access_token));
			tokenCredentials.RefreshToken.GP_Certificate = new ZBlob(Encoding.ASCII.GetBytes(response.refresh_token));
			tokenCredentials.GP_ExpiryDate = ZDateTime.UtcNow.AddSeconds(response.expires_in);
			tokenCredentials.ShouldUpdateExpiryDateOnSaving = false;

			company.Logs.AddNew(Events.CommunicationTokensRefresh, "Succeded");
		}
		else if (message.EM_MessageSubType == MessageSubTypeCodeList.Codes.CustomsRejected)
		{
			UpdatePasswordStatus(wrapper);

			var response = JsonSerializer.Deserialize<TokenResponseError>(message.EM_MessageText);
			company.Logs.AddNew(Events.CommunicationTokensRefresh, $"Failed|{response.error_description}|InterchangeNum={message.Interchange.EI_InterchangeNum}");
		}
		else if (message.EM_MessageSubType == MessageSubTypeCodeList.Codes.Rejected)
		{
			UpdatePasswordStatus(wrapper);
		}
		else
		{
			MarkMessageAsDiscardedAndLogWarning(message, $"Unknown Message Sub Type: '{message.EM_MessageSubType}'.");
		}
	}

	GlbCompanyTokenCredentials UpdatePasswordStatus(GlbCompanyWrapper wrapper)
	{
		wrapper.TokenCredentialsEnabled = true;
		var tokenCredentials = wrapper.TokenCredentials;
		tokenCredentials.GP_PasswordStatus = UniversalReferenceConstants.PasswordStatusList.Received;
		return tokenCredentials;
	}

	internal class TokenResponseSuccess
	{
		public string access_token { get; set; }
		public string refresh_token { get; set; }
		public string token_type { get; set; }
		public int expires_in { get; set; }
	}

	internal class TokenResponseError
	{
		public string error_description { get; set; }
		public string error { get; set; }
	}
}
