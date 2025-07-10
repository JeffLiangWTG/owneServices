using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.DE.Business
{
	public class CusGuaranteeRule : EU.Business.CusGuaranteeRule, Integration.Customs.DE.ICusGuaranteeRule
	{
		public CusGuaranteeRule(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
		public new CusGuaranteeHeader GuaranteeHeader => (CusGuaranteeHeader)base.GuaranteeHeader;

		protected override Customs.Business.AccessCodePinRuleValidation AccessCodePinRuleValidation => new AccessCodePinRuleValidation(this);
	}
}
