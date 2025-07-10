using System.Threading;
using Enterprise.BatchProcessor;

namespace Enterprise.Customs.CH.Business;

public static class CompanyMessageSender
{
	public static void SendRequests(LoggingInformation logger, CancellationToken token)
	{
		if (GlbCompanyWrapper.CurrentCompanyTokenCredentialsEnabled)
		{
			new PassarMessageListRequestSender(logger).Send(token);
			new PassarGetMessageRequestSender(logger).Send(token);
			new CharteraOutputMessageListRequestSender(logger).Send(token);
			new CharteraOutputGetMessageRequestSender(logger).Send(token);
			new CharteraOutputDocumentDeliveryRequestSender(logger).Send(token);
			if (CHCustomsDataRegistry.Instance.PassarSearchRequestConfig.Value.IsEnabled)
			{
				new CharteraOutputDocumentSearchRequestSender(logger).Send(token);
				new CharteraOutputDocumentSearchRetrySender(logger).Send(token);
			}
		}
	}

	public static void SendTokensRefresh(LoggingInformation logger, CancellationToken token)
	{
		if (GlbCompanyWrapper.CurrentCompanyTokenCredentialsEnabled)
		{
			new TokensRefreshMessageSender(logger).Send(token);
		}
	}

	public static void SendBordereauRequest(LoggingInformation logger, CancellationToken token)
	{
		if (CHCustomsDataRegistry.Instance.EdecBordereauConfig.Value.IsEnabled)
		{
			new BordereauListRequestSender(logger).Send(token);
		}
	}
}
