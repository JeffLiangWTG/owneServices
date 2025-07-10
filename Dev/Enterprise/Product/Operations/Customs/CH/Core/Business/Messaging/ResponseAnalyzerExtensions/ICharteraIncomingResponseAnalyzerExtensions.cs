using CargoWise.Customs.CH.MessageContracts.MessageProviders.Chartera;

namespace Enterprise.Customs.CH.Business;

public static class ICharteraIncomingResponseAnalyzerExtensions
{
	public static string GetMessageSubType(this ICharteraResponseAnalyzer analyzer)
	{
		var messageSubType = string.Empty;

		if (analyzer.IsDocument)
		{
			messageSubType = MessageSubTypeCodeList.Codes.CharteraOutputDocumentDeliveryResult;
		}
		else if (analyzer.IsSearchResult)
		{
			messageSubType = MessageSubTypeCodeList.Codes.CharteraOutputDocumentSearchResult;
		}
		else if (analyzer.IsError)
		{
			messageSubType = MessageSubTypeCodeList.Codes.CharteraOutputError;
		}

		return messageSubType;
	}
}
