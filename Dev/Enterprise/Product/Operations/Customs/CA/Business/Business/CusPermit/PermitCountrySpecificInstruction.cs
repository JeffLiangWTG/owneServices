using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	public class PermitCountrySpecificInstruction : Customs.Business.PermitCountrySpecificInstruction
	{
		public PermitCountrySpecificInstruction(BusinessObjectFactory factory) : base(factory)
		{
		}

		public override Customs.Business.PermitTypeList GetTypeList()
		{
			return Factory.GetCachedValue<PermitTypeList>();
		}

		public override Customs.Business.PermitRuleCodeList GetRuleCodeList(ZString permitType, ZString permitSubType)
		{
			return GetRuleCodeListForModule();
		}

		public override Customs.Business.PermitRuleCodeList GetRuleCodeListForModule()
		{
			return Factory.GetCachedValue<CAPermitRuleCodeList>();
		}
	}
}
