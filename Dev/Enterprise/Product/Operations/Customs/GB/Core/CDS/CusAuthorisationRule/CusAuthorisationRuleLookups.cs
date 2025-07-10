using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.CDS
{
	public class CusAuthorisationRuleLookups : Customs.Business.CusAuthorisationRuleLookups
	{
		public CusAuthorisationRuleLookups(CusAuthorisationRule parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList RuleCodeList
		{
			get
			{
				return Factory.GetCachedValue("Enterprise.Customs.GB.CDS.CusAuthorisationRuleLookups.RuleCodeList_" + Parent.AuthorisationHeader.CPH_Type,
					() =>
					{
						var result = new GBCusAuthorisationRuleTypeBaseList();
						var header = Parent.AuthorisationHeader;
						if (header != null && (header.CPH_Type == CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration || header.CPH_Type == CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords))
						{
							result = new GBCusAuthorisationRuleTypeList();
						}
						result.Sort();
						return result;
					});
			}
		}

		protected override Dictionary<ZString, Func<ICollection>> GetValueListFromRuleCodeCore()
		{
			var result = base.GetValueListFromRuleCodeCore();
			result.Add(GBCusAuthorisationRuleTypeList.Codes.ORG, () => new OrgHeaderCollection(Factory));
			result.Add(GBCusAuthorisationRuleTypeBaseList.Codes.CTY, () => new GBCusAuthorisationCountryCodePrefixList());
			return result;
		}
	}
}
