using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class CusTempStorageRegLineLookups : EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineLookups
	{
		public CusTempStorageRegLineLookups(CusTempStorageRegLine parent)
			: base(parent)
		{
		}

		public override CodeDescriptionPairList OwnerReferenceTypeList => Factory.GetCachedValue("DE|CusTempStorageRegLineLookups|OwnerReferenceTypeList", () => new CodeDescriptionPairList
		{
			new CodeDescriptionPair(Business.OwnerReferenceTypeList.Codes.AWB, Business.OwnerReferenceTypeList.Descriptions.AWB),
			new CodeDescriptionPair(Business.OwnerReferenceTypeList.Codes.ULD, Business.OwnerReferenceTypeList.Descriptions.ULD),
			new CodeDescriptionPair(Business.OwnerReferenceTypeList.Codes.ZZZ, Business.OwnerReferenceTypeList.Descriptions.ZZZ)
		});

		public OrganisationsFindBoxCollection OrganizationsFindBoxList => new OrganisationsFindBoxCollection(Factory);

		public override CodeDescriptionPairList CustomsStatusList => Factory.GetCachedValue<CustomsStatusList>();

		public override CodeDescriptionPairList PackageTypeList =>
			Universal.RefCusCodeListTypes.GetCachedList(Parent.Factory,
				Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				ZDateTime.Today);

		public override CodeDescriptionPairList UnionStatusList => Factory.GetCachedValue<DEUnionStatusList>();
	}
}
