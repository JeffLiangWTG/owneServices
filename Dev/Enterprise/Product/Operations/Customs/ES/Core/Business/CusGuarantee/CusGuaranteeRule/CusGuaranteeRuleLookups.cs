using System;
using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.ES.Business.CusGuarantee
{
	class CusGuaranteeRuleLookups : EU.Business.CusGuaranteeRuleLookups
	{
		public CusGuaranteeRuleLookups(CusGuaranteeRule parent)
			: base(parent)
		{
		}

		public override ICollection CPR_ValueFromList
		{
			get
			{
				switch (Parent.CPR_RuleCode)
				{
					case PermitRuleCodeList.Codes.CUS:
						return EUCustomsOfficeCodeCollection.AllEuropeanUnionCustomsOfficesWithRequiredRoles(Factory, Array.Empty<ZString>());
					default:
						return base.CPR_ValueFromList;
				}
			}
		}
	}
}
