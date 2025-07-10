using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Bordereau;

namespace Enterprise.Customs.CH.Business;
	public static class IEdecBordereauMessageAnalyzerExtensions
	{
	public static string GetMessageSubType(this IEdecBordereauResponseAnalyzer analyzer)
	{
		var messageSubType = string.Empty;

		if (analyzer.IsBordereauType)
		{
			messageSubType = MessageSubTypeCodeList.Codes.BordereauResponse;
		}
		else if (analyzer.IsBordereauListType)
		{
			messageSubType = MessageSubTypeCodeList.Codes.BordereauList;
		}
		else if (analyzer.IsBordereaRequestRejectionRuleErrorsType)
		{
			messageSubType = MessageSubTypeCodeList.Codes.RuleError;
		}
		else if (analyzer.IsBordereaRequestRejectionXMLSchemaErrorsType)
		{
			messageSubType = MessageSubTypeCodeList.Codes.XmlSchemaError;
		}

		return messageSubType;
	}
}
