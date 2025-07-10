using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business
{
	public class EuOfficeCodeLookups : CusCodeDataLookups
	{
		public EuOfficeCodeLookups(EuOfficeCode officeCode) : base(officeCode)
		{
		}

		public virtual CustomsOfficeCodeCollection OfficeCodeList
		{
			get
			{
				return GetOfficeCodeList(Parent.CY_RoleCodes.ToArray());
			}
		}

		protected CustomsOfficeCodeCollection GetOfficeCodeList(ZString[] roleCodes)
		{
			var isLocalCountryOnly = Parent.Requirement?.IsLocalCountryOnly ?? false;
			var isForeignCountryOnly = Parent.Requirement?.IsForeignCountryOnly ?? false;
			return isLocalCountryOnly
				? CustomsOfficeCodeCollection.LocalCountryOnlyCustomsOfficesWithRequiredRoles(Factory, ParentOfficeCodeProvider.CountryCode, roleCodes)
				: (isForeignCountryOnly
					? EUCustomsOfficeCodeCollection.AllEuropeanUnionCustomsOfficesWithRequiredRolesExceptLocal(Factory, ParentOfficeCodeProvider.CountryCode, roleCodes)
					: EUCustomsOfficeCodeCollection.AllEuropeanUnionCustomsOfficesWithRequiredRoles(Factory, roleCodes));
		}

		public override CodeDescriptionPairList CY_CodeList
		{
			get
			{
				var list = new CodeDescriptionPairList();
				if (ParentOfficeCodeProvider != null)
				{
					if (ParentOfficeCodeProvider.IsEMCS)
					{
						var useDesInsteadOfCaa = Parent.OfficeCodesUseDesInsteadOfCaa;
						list = Factory.GetCachedValue($"EU_OfficeCodes_EMCS_EMCSUseDesInsteadofCaa_{useDesInsteadOfCaa}", () =>
						{
							var result = new OfficeCodes_EMCS();
							if (useDesInsteadOfCaa)
							{
								result.RemoveCode(OfficeCodes_EMCS.Codes.CompetentAuthorityOfArrival);
							}
							else
							{
								result.RemoveCode(OfficeCodes_EMCS.Codes.OfficeOfDestination);
							}
							return result;
						});
					}
					else
					{
						list = Factory.GetCachedValue(ParentOfficeCodeProvider.CustomsOfficeRequirementHelper.CacheKeyCombination, () =>
						{
							var result = new CodeDescriptionPairList();
							foreach (var requirement in ParentOfficeCodeProvider.CustomsOfficeRequirementHelper.OtherRequirements)
							{
								result.AddPair(requirement.OfficeRole, requirement.FriendlyName);
							}
							return result;
						});
					}
				}
				return list;
			}
		}

		protected new EuOfficeCode Parent => (EuOfficeCode)base.Parent;

		protected IEuOfficeCodeProvider ParentOfficeCodeProvider => this.Parent.Parent as IEuOfficeCodeProvider;
	}
}
