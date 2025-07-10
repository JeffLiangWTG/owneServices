using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class DEOfficeCodeLookups : EuOfficeCodeLookups
	{
		public DEOfficeCodeLookups(DEOfficeCode parent) : base(parent)
		{
		}

		public override CustomsOfficeCodeCollection OfficeCodeList
		{
			get
			{
				var result = base.OfficeCodeList;
				if (Parent.CY_Code == EuOfficeCodesTypes.Codes.SupplementaryDeclarationOffice)
				{
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(ZZRefCusCodeListFilters.CountryOrGrouping, "Property", (ZString)Core.Constants.CountryCodes.Germany, false));
				}
				return result;
			}
		}
	}
}
