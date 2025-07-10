using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class PackageLookups : Customs.Business.CusDecHouseContainerPackLookups
	{
		public PackageLookups(Package parent) : base(parent)
		{
		}

		public CodeDescriptionPairList BulkAndBreakBulkPackingUnitTypesList
			=> GetPackingUnitTypesList(Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk);

		public CodeDescriptionPairList BulkOnlyPackingUnitTypesList
			=> GetPackingUnitTypesList(Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk);

		public CodeDescriptionPairList BreakBulkOnlyPackingUnitTypesList
			=> GetPackingUnitTypesList(Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk);

		CodeDescriptionPairList GetPackingUnitTypesList(params ZString[] attributeNamesToMatch)
		{
			return Universal.RefCusCodeListTypes.GetCachedListMatchAnyAttributes(Factory,
				Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				UniversalReferenceConstants.UNPackTypeStartDate,
				attributeNamesToMatch.Select(attributeName => new KeyValuePair<ZString, ZString>(attributeName, ZString.Empty)).ToArray());
		}
	}
}
