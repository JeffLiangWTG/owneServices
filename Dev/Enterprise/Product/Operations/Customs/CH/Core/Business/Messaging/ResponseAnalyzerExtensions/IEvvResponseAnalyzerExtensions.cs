using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv;

namespace Enterprise.Customs.CH.Business;

public static class IEvvResponseAnalyzerExtensions
{
	public static string GetMessageSubType(this IEvvResponseAnalyzer analyzer)
	{
		var messageSubType = string.Empty;

		if (analyzer.IsDuties)
		{
			messageSubType = MessageSubTypeCodeList.Codes.TaxationDecisionCustomsDuties;
		}
		else if (analyzer.IsVAT)
		{
			messageSubType = MessageSubTypeCodeList.Codes.TaxationDecisionVat;
		}
		else if (analyzer.IsRuleErrors)
		{
			messageSubType = MessageSubTypeCodeList.Codes.RuleError;
		}
		else if (analyzer.IsXmlSchemaErrors)
		{
			messageSubType = MessageSubTypeCodeList.Codes.XmlSchemaError;
		}
		else if (analyzer.IsRefundVAT)
		{
			messageSubType = MessageSubTypeCodeList.Codes.TaxationDecisionReimbursementVat;
		}
		else if (analyzer.IsRefundDuties)
		{
			messageSubType = MessageSubTypeCodeList.Codes.TaxationDecisionReimbursementCustomsDuties;
		}

		return messageSubType;
	}
}
