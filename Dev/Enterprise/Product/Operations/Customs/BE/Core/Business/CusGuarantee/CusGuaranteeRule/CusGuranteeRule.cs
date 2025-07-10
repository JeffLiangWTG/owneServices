using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.BE.Business;

public class CusGuaranteeRule : EU.Business.CusGuaranteeRule, Integration.Customs.BE.ICusGuaranteeRule
{
	public CusGuaranteeRule(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	protected override Customs.Business.AccessCodePinRuleValidation AccessCodePinRuleValidation => new AccessCodePinRuleValidation(this);
}
