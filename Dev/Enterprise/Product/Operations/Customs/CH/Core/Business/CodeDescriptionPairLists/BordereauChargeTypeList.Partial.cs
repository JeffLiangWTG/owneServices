namespace Enterprise.Customs.CH.Business;

public partial class BordereauChargeTypeList
{
	public static string GetChargeType(string messageSubType)
	{
		return messageSubType switch
		{
			MessageSubTypeCodeList.Codes.TaxationDecisionCustomsDuties => BordereauChargeTypeList.Codes.Duties,
			MessageSubTypeCodeList.Codes.TaxationDecisionVat => BordereauChargeTypeList.Codes.Vat,
			MessageSubTypeCodeList.Codes.TaxationDecisionReimbursementCustomsDuties => BordereauChargeTypeList.Codes.DutiesRefund,
			MessageSubTypeCodeList.Codes.TaxationDecisionReimbursementVat => BordereauChargeTypeList.Codes.VatRefund,
			_ => string.Empty
		};
	}

	public static string GetMessageSubType(string chargeType)
	{
		return chargeType switch
		{
			BordereauChargeTypeList.Codes.Duties => MessageSubTypeCodeList.Codes.TaxationDecisionCustomsDuties,
			BordereauChargeTypeList.Codes.Vat => MessageSubTypeCodeList.Codes.TaxationDecisionVat,
			BordereauChargeTypeList.Codes.DutiesRefund => MessageSubTypeCodeList.Codes.TaxationDecisionReimbursementCustomsDuties,
			BordereauChargeTypeList.Codes.VatRefund => MessageSubTypeCodeList.Codes.TaxationDecisionReimbursementVat,
			_ => string.Empty
		};
	}
}
