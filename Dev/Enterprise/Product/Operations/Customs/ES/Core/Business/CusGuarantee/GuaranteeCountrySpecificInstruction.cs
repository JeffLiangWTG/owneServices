using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business
{
	public class GuaranteeCountrySpecificInstruction : EU.Business.GuaranteeCountrySpecificInstruction
	{
		public GuaranteeCountrySpecificInstruction(BusinessObjectFactory factory) : base(factory)
		{
		}

		public override Customs.Business.PermitRuleCodeList GetRuleCodeList(ZString permitType, ZString permitSubType)
		{
			return Factory.GetCachedValue<PermitRuleCodeList>();
		}

		public override Customs.Business.PermitRuleCodeList GetRuleCodeListForModule()
		{
			return Factory.GetCachedValue<PermitRuleCodeList>();
		}

		protected override CodeDescriptionPairList GetTypeCodeDescriptionPairList() => Factory.GetCachedValue<EUGuaranteeTypeList>();
	}
}
