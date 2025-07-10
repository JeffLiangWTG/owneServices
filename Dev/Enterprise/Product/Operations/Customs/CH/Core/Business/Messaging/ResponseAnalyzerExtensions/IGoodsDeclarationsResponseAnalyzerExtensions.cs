using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.GoodsDeclarations;

namespace Enterprise.Customs.CH.Business;

public static class IGoodsDeclarationsResponseAnalyzerExtensions
{
	public static string GetMessageSubType(this IGoodsDeclarationsResponseAnalyzer analyzer)
	{
		var messageSubType = "";

		if (analyzer.IsAcceptanceResponse)
		{
			messageSubType = MessageSubTypeCodeList.Codes.Accepted;
		}
		else if (analyzer.IsXMLSchemaErrorsResponse)
		{
			messageSubType = MessageSubTypeCodeList.Codes.XmlSchemaError;
		}
		else if (analyzer.IsCustomsRejectionResponse)
		{
			messageSubType = MessageSubTypeCodeList.Codes.CustomsRejected;
		}
		else if (analyzer.IsRuleErrorResponse)
		{
			messageSubType = MessageSubTypeCodeList.Codes.RuleError;
		}
		else if (analyzer.IsStatusResponse)
		{
			messageSubType = MessageSubTypeCodeList.Codes.Status;
		}
		return messageSubType;
	}
}
