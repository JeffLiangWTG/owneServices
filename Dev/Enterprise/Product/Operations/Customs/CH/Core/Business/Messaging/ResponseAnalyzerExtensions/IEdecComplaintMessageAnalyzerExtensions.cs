using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.ECom;

namespace Enterprise.Customs.CH.Business;

public static class IEdecComplaintMessageAnalyzerExtensions
{
	public static string GetMessageSubType(this IEdecComplaintMessageAnalyzer analyzer)
	{
		var messageSubType = string.Empty;

		if (analyzer.IsAcceptanceResponse)
		{
			messageSubType = MessageSubTypeCodeList.Codes.Accepted;
		}
		else if (analyzer.IsRuleErrorsResponse)
		{
			messageSubType = MessageSubTypeCodeList.Codes.RuleError;
		}
		else if (analyzer.IsXMLSchemaErrorsResponse)
		{
			messageSubType = MessageSubTypeCodeList.Codes.XmlSchemaError;
		}
		else if (analyzer.IsComplaintRequest)
		{
			messageSubType = MessageSubTypeCodeList.Codes.Request;
		}

		return messageSubType;
	}
}
