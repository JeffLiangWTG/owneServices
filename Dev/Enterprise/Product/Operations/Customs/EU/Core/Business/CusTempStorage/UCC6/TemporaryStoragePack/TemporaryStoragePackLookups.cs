using CargoWise.Types;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class TemporaryStoragePackLookups : AsycudaPackLookups
	{
		public TemporaryStoragePackLookups(TemporaryStoragePack parent) : base(parent)
		{
		}

		public new TemporaryStoragePack Parent => (TemporaryStoragePack)base.Parent;

		public override CodeDescriptionPairList PackUQList
			=> RefCusCodeListTypes.GetCachedListValidBeforeDate(Factory, Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, UniversalReferenceConstants.UNPackTypeStartDate);

		internal CodeDescriptionPairList BulkOnlyPackingUnitTypesList => GetPackingUnitTypesList(Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk);

		internal CodeDescriptionPairList BreakBulkOnlyPackingUnitTypesList => GetPackingUnitTypesList(Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk);

		CodeDescriptionPairList GetPackingUnitTypesList(ZString attributeNameToMatch) => RefCusCodeListTypes.GetCachedListMatchAnyAttributes(Factory,
			Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
			Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			UniversalReferenceConstants.UNPackTypeStartDate,
			[new(attributeNameToMatch, ZString.Empty)]);
	}
}
