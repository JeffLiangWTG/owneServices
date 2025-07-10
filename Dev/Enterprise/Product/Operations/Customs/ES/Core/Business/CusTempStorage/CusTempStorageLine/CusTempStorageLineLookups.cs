using CargoWise.Types;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business.CusTempStorage
{
	public class CusTempStorageLineLookups : EU.Business.CusTempStorage.CusTempStorageLineLookups
	{
		public CusTempStorageLineLookups(AutoCusTempStorageLine parent) : base(parent)
		{
		}

		public CodeDescriptionPairList OwnerReferenceTypeList => Factory.GetCachedValue<OwnerReferenceTypeList>();

		public CodeDescriptionPairList UnionStatusList => Factory.GetCachedValue<UnionStatusList>();

		public CodeDescriptionPairList GoodsTypeList => Factory.GetCachedValue<GoodsTypeList>();

		public CodeDescriptionPairList WeightUQList => Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight);

		public CodeDescriptionPairList PackageTypeList =>
			Universal.RefCusCodeListTypes.GetCachedList(Parent.Factory,
				Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				ZDateTime.Today);
	}
}
