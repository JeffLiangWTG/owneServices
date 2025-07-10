using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsEuOfficeCodeLookups : EuOfficeCodeLookups
	{
		public NctsEuOfficeCodeLookups(NctsEuOfficeCode officeCode) : base(officeCode)
		{
		}

		public override CustomsOfficeCodeCollection OfficeCodeList
		{
			get
			{
				var isLocalCountryOnly = Parent.Requirement?.IsLocalCountryOnly ?? false;
				var isForeignCountryOnly = Parent.Requirement?.IsForeignCountryOnly ?? false;
				return isLocalCountryOnly ? base.OfficeCodeList
					: (isForeignCountryOnly
						? EUCustomsOfficeCodeCollection.AllEuropeanUnionAndCtCountriesCustomsOfficesWithRequiredRolesExceptLocal(Factory, ParentOfficeCodeProvider.CountryCode, Parent.CY_RoleCodes.ToArray())
						: EUCustomsOfficeCodeCollection.AllEuropeanUnionAndCtCountriesCustomsOfficesWithRequiredRoles(Factory, Parent.CY_RoleCodes.ToArray()));
			}
		}

		public CodeDescriptionPairList CodeList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				foreach (CodeDescriptionPair item in CY_CodeList)
				{
					if (item.Code != OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival)
					{
						result.Add(item);
					}
				}
				return result;
			}
		}
	}
}
