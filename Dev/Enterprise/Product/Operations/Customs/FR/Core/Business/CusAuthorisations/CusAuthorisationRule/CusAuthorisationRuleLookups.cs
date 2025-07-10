using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business
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
				var authorisationType = Parent.AuthorisationHeader.CPH_Type;
				var isAdHoc = Parent.AuthorisationHeader.CPH_IsAdHoc;
				var provider = (CusAuthorisationHeaderProvider)Parent.AuthorisationHeader.Provider;
				return Factory.GetCachedValue($"CusAuthorisationRule|FR|{authorisationType}|{isAdHoc}", () =>
				{
					CodeDescriptionPairList result;
					switch (authorisationType)
					{
						case Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1:
						case Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2:
						case Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP:
							result = provider.CusAuthorisationRuleTypeListForCW1CW2CWP;
							break;
						case Customs.Business.CusAuthorizationHeaderTypeList.Codes.TemporaryStorage:
							result = provider.CusAuthorisationRuleTypeListForTST;
							break;
						case CusAuthorizationHeaderTypeList.Codes.AuthorizedLocation:
							result = provider.CusAuthorisationRuleTypeListForAUL;
							break;
						case Customs.Business.CusAuthorizationHeaderTypeList.Codes.InwardProcessing:
						case Customs.Business.CusAuthorizationHeaderTypeList.Codes.TemporaryAdmission:
							result = provider.CusAuthorisationRuleTypeListForIPOTEA;
							break;
						case Customs.Business.CusAuthorizationHeaderTypeList.Codes.OutwardProcessing:
							result = provider.CusAuthorisationRuleTypeListForOPO;
							break;
						case CusAuthorizationHeaderTypeList.Codes.OtherThanOpo:
						case CusAuthorizationHeaderTypeList.Codes.TemporaryExportation:
							result = provider.CusAuthorisationRuleTypeListForOTOAndTEE;
							break;
						case Customs.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit:
						case Customs.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit:
							result = provider.CusAuthorisationRuleTypeListForTransit;
							break;
						case Customs.Business.CusAuthorizationHeaderTypeList.Codes.EndUse:
							result = provider.CusAuthorisationRuleTypeListForEUS;
							break;

						default:
							result = new Customs.Business.CusAuthorisationRuleTypeList();
							break;
					}
					result.Sort();

					if (isAdHoc)
					{
						foreach (CodeDescriptionPair ruleNotSuitableForAdHocCodePair in provider.CusAuthorisationRuleTypeNotSuitableForAdHoc)
						{
							result.Remove(ruleNotSuitableForAdHocCodePair);
						}
					}
					return result;
				});
			}
		}

		protected override Dictionary<ZString, Func<ICollection>> GetValueListFromRuleCodeCore()
		{
			var result = base.GetValueListFromRuleCodeCore();
			result.Add(CusAuthorisationRuleTypeList.Codes.USE, () => GetValueListAuthorizationUSE);
			result.Add(CusAuthorisationRuleTypeList.Codes.OFC, () => GetValueListAuthorizationOFC());
			result.Add(CusAuthorisationRuleTypeList.Codes.CLE, () => GetValueListAuthorizationCLE());
			result.Add(CusAuthorisationRuleTypeList.Codes.CON, () => GetValueListAuthorizationCON());
			result.Add(CusAuthorisationRuleTypeList.Codes.TRA, () => GetValueListAuthorizationTRA());
			return result;
		}

		protected CodeDescriptionPairList GetValueListAuthorizationUSE
		{
			get
			{
				var provider = (CusAuthorisationHeaderProvider)Parent.AuthorisationHeader.Provider;
				var authorisationType = Parent.AuthorisationHeader.CPH_Type;
				return Factory.GetCachedValue("GetValueListUSE" + authorisationType, () =>
				{
					CodeDescriptionPairList result;
					switch (authorisationType)
					{
						case Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1:
						case Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2:
							result = provider.CusAuthorisationUSEValueFromListForCW1CW2;
							break;
						case Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP:
							result = provider.CusAuthorisationUSEValueFromListForCWP;
							break;
						case Customs.Business.CusAuthorizationHeaderTypeList.Codes.TemporaryStorage:
							result = provider.CusAuthorisationUSEValueFromListForTST;
							break;
						default:
							result = new CodeDescriptionPairList();
							break;
					}
					result.Sort();
					return result;
				});
			}
		}

		ICollection GetValueListAuthorizationOFC()
		{
			var cusoffice = Universal.ZZRefCusCodeListCombinedCollection.GetCachedCollection(Parent.Factory, Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);
			return cusoffice;
		}
		ICollection GetValueListAuthorizationCLE() => new YesNoList();

		ICollection GetValueListAuthorizationTRA() => new RuleCodeTRAValueFromList();

		ICollection GetValueListAuthorizationCON()
		{
			var provider = (CusAuthorisationHeaderProvider)Parent.AuthorisationHeader.Provider;
			var authorisationType = Parent.AuthorisationHeader.CPH_Type;
			return Factory.GetCachedValue("GetValueListCON" + authorisationType, () =>
			{
				CodeDescriptionPairList result;
				switch (authorisationType)
				{
					case Customs.Business.CusAuthorizationHeaderTypeList.Codes.InwardProcessing:
						result = provider.RuleCodeCONValueFromListForIPO;
						break;
					case Customs.Business.CusAuthorizationHeaderTypeList.Codes.EndUse:
						result = provider.RuleCodeCONValueFromListForEndUse;
						break;
					default:
						result = new CodeDescriptionPairList();
						break;
				}
				result.Sort();
				return result;
			});
		}
	}
}
