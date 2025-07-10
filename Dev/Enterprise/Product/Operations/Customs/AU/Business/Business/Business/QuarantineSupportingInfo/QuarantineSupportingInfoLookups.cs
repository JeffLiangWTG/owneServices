using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class QuarantineSupportingInfoLookups : CusSupportingInfoLookups
	{
		public QuarantineSupportingInfoLookups(QuarantineSupportingInfo parent)
			: base(parent)
		{
		}

		public ZZRefCusCodeListCombinedCollection CSIDescriptionList
		{
			get
			{
				var result = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory,
					Core.Constants.CountryCodes.Australia,
					Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NEXDOCSDeclarationCode,
					ZDateTime.Now);

				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.Description, "Property", ZString.Empty, false));
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.ListType, "Property", (ZString)Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NEXDOCSDeclarationCode, false));
				return result;
			}
		}

		public CodeDescriptionPairList DeclarationCodeList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Australia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NEXDOCSDeclarationCode, ZDate.Today);
	}
}
