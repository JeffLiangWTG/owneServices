namespace Enterprise.Customs.IT.NCTS.Business;

public class ValidationRuleMessages : EU.NCTS.Business.ValidationRuleMessages
{
	protected override string R0994_1RuleCodeCore() => "NR0077";

	protected override string GetR0994_1MessageCore(string billsTotalGrossMass)
	{
		return Res.GetString("0FB327A4-984D-4CA7-B64B-8436167F9375", "Total Gross Weight is different from the sum of House Consignments Gross Weight ({0} Kg)", billsTotalGrossMass);
	}
}
