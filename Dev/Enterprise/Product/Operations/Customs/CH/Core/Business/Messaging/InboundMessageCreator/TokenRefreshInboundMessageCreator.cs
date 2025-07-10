using System.Text.Json.Nodes;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using static Enterprise.Customs.CH.Business.TokenRefreshMessageProcessor;

namespace Enterprise.Customs.CH.Business;

public class TokenRefreshInboundMessageCreator : BaseInboundMessageCreator
{
	public TokenRefreshInboundMessageCreator(LoggingInformation logger) : base(logger)
	{
	}

	protected override ZString GetMessageSubTypeFromResponse(string jsonResponse)
	{
		var json = JsonNode.Parse(jsonResponse);
		if (json?[nameof(TokenResponseSuccess.access_token)] != null)
		{
			return MessageSubTypeCodeList.Codes.Accepted;
		}
		else if (json?[nameof(TokenResponseError.error)] != null)
		{
			return MessageSubTypeCodeList.Codes.CustomsRejected;
		}
		else
		{
			return base.GetMessageSubTypeFromResponse(jsonResponse);
		}
	}
}
