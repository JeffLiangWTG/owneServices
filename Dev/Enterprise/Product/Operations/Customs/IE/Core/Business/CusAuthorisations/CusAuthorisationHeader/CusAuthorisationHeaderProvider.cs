using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business
{
	public class CusAuthorisationHeaderProvider : EU.Business.CusAuthorisationHeaderProvider
	{
		public CusAuthorisationHeaderProvider(ZString countryCode) : base(countryCode)
		{
		}

		protected override bool ShowCustomsCodeCore => true;

		protected override CodeDescriptionPairList GetAuthorisationTypeListCore(BusinessObjectFactory factory) =>
			RefCusCodeListTypes.GetCachedList(
				factory: factory,
				country: CountryCode,
				codeType: EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_AUTH,
				date: ZDateTime.Today,
				includeParentDataGrouping: false
				);

		protected override string GetRuleValueFromFieldTypeCore(CusAuthorisationRule cusAuthorisationRule)
		{
			var result = base.GetRuleValueFromFieldTypeCore(cusAuthorisationRule);

			if (cusAuthorisationRule != null)
			{
				var header = cusAuthorisationRule.AuthorisationHeader;
				var authTypes = new ZString[] { CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit, CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit };
				if (header != null && cusAuthorisationRule.CPR_RuleCode == CusAuthorisationRuleTypeList.Codes.Location)
				{
					result = authTypes.Contains(header.CPH_Type) ? nameof(FieldType.TextCodeFindBox) : nameof(FieldType.Text);
				}
			}
			return result;
		}

		protected override Customs.Business.CusAuthorisationRuleLookups GetNewLookupsCore(CusAuthorisationRule cusAuthorisationRule) => new CusAuthorisationRuleLookups(cusAuthorisationRule);
	}
}
