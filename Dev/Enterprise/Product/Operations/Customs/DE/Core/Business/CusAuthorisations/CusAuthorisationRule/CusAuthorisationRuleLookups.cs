using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business
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
				var authorisationHeader = Parent.AuthorisationHeader;
				var authorisationType = authorisationHeader.CPH_Type;
				var addMRE = authorisationHeader.RequiresAuthorizationRuleMandateReference();
				var addREL = authorisationHeader.RequiresAuthorizationRuleRelease();

				return Factory.GetCachedValue("CusAuthorisationRuleCodeList|DE|" + authorisationType + addMRE + addREL, () =>
				{
					var result = new CodeDescriptionPairList(base.RuleCodeList);
					if (authorisationType == CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration)
					{
						result.AddPair(CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationRuleTypeList.Descriptions.Usage);
						if (addMRE)
						{
							result.AddPair(CusAuthorisationRuleTypeList.Codes.MandateReference, CusAuthorisationRuleTypeList.Descriptions.MandateReference);
						}
						result.Sort();
					}
					else if (authorisationType == CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords)
					{
						result.AddPair(CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationRuleTypeList.Descriptions.Usage);
						result.AddPair(CusAuthorisationRuleTypeList.Codes.BusinessReference, CusAuthorisationRuleTypeList.Descriptions.BusinessReference);
						if (addMRE)
						{
							result.AddPair(CusAuthorisationRuleTypeList.Codes.MandateReference, CusAuthorisationRuleTypeList.Descriptions.MandateReference);
						}
						if (addREL)
						{
							result.AddPair(CusAuthorisationRuleTypeList.Codes.Release, CusAuthorisationRuleTypeList.Descriptions.Release);
						}
						result.Sort();
					}
					return result;
				});
			}
		}

		protected override Dictionary<ZString, Func<ICollection>> GetValueListFromRuleCodeCore()
		{
			var result = base.GetValueListFromRuleCodeCore();
			result.Add(CusAuthorisationRuleTypeList.Codes.Usage, () => GetValueListAuthorizationUsage);
			result.Add(CusAuthorisationRuleTypeList.Codes.Release, () => GetValueListAuthorizationRelease);
			return result;
		}

		CodeDescriptionPairList GetValueListAuthorizationUsage
		{
			get
			{
				var authorisationType = Parent.AuthorisationHeader.CPH_Type;
				return Factory.GetCachedValue("GetValueListUSE" + authorisationType, () =>
				{
					CodeDescriptionPairList result;
					switch (authorisationType)
					{
						case CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords:
							result = new CusAuthorisationUsageRuleList();
							result.RemoveCode(CusAuthorisationUsageRuleList.Codes.OutwardProcessingProcedure);
							break;
						case CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration:
							result = new CusAuthorisationUsageRuleList();
							break;
						default:
							result = new CodeDescriptionPairList();
							break;
					}
					return result;
				});
			}
		}

		CodeDescriptionPairList GetValueListAuthorizationRelease => Factory.GetCachedValue<CusAuthorisationReleaseRuleList>();
	}
}
