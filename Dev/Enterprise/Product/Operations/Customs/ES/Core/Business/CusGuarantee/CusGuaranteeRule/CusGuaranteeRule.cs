using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ES.Business.CusGuarantee
{
	public class CusGuaranteeRule : EU.Business.CusGuaranteeRule, Integration.Customs.ES.ICusGuaranteeRule
	{
		public CusGuaranteeRule(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
		public new CusGuaranteeHeader GuaranteeHeader => (CusGuaranteeHeader)base.GuaranteeHeader;

		protected override CusPermitRuleLookups GetNewLookups() => new CusGuaranteeRuleLookups(this);
	}
}
