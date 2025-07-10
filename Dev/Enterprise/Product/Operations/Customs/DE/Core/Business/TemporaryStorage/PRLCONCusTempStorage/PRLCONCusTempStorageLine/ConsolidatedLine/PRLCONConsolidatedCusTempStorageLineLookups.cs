using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class PRLCONConsolidatedCusTempStorageLineLookups : CusTempStorageLineLookups
	{
		public PRLCONConsolidatedCusTempStorageLineLookups(AutoCusTempStorageLine parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList UnionStatusList
		{
			get
			{
				return Factory.GetCachedValue("DE|PRLCONNewConsolidatedCusTempStorageLineLookups|UnionStatusList", () => new CodeDescriptionPairList
				{
					new CodeDescriptionPair(Business.DEUnionStatusList.Codes.F, Business.DEUnionStatusList.Descriptions.F),
					new CodeDescriptionPair(Business.DEUnionStatusList.Codes.N, Business.DEUnionStatusList.Descriptions.N),
				});
			}
		}

		public override CodeDescriptionPairList OwnerReferenceTypeList
		{
			get
			{
				return Factory.GetCachedValue("DE|PRLCONNewConsolidatedCusTempStorageLineLookups|OwnerReferenceTypeList", () => new CodeDescriptionPairList
				{
					new CodeDescriptionPair(Business.OwnerReferenceTypeList.Codes.AWB, Business.OwnerReferenceTypeList.Descriptions.AWB),
					new CodeDescriptionPair(Business.OwnerReferenceTypeList.Codes.ULD, Business.OwnerReferenceTypeList.Descriptions.ULD),
					new CodeDescriptionPair(Business.OwnerReferenceTypeList.Codes.ZZZ, Business.OwnerReferenceTypeList.Descriptions.ZZZ)
				});
			}
		}
	}
}
