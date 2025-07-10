using CargoWise.Customs.CH.MessageContracts.MessageProviders.Ebd;

namespace Enterprise.Customs.CH.Business;

public static class IDocumentImportResponseAnalyzerExtensions
{
	public static string GetMessageSubType(this IDocumentImportResponseAnalyzer analyzer)
	{
		var messageSubType = "";

		if (analyzer.IsAcceptanceResponse)
		{
			messageSubType = MessageSubTypeCodeList.Codes.Accepted;
		}
		else if (analyzer.IsRejectionResponse)
		{
			messageSubType = MessageSubTypeCodeList.Codes.CustomsRejected;
		}
		return messageSubType;
	}
}
