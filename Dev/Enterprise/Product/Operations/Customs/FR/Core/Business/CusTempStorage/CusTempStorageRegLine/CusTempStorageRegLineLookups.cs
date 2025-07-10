using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.CusTempStorage
{
	public class CusTempStorageRegLineLookups : EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineLookups
	{
		public CusTempStorageRegLineLookups(CusTempStorageRegLine parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList OwnerReferenceTypeList => Factory.GetCachedValue<OwnerReferenceTypeList>();

		public override CodeDescriptionPairList PackageTypeList => UniversalReferenceDataHelper.GetCachedPackageTypeList(Parent.Factory);

		public override CodeDescriptionPairList UnionStatusList => Factory.GetCachedValue<UnionStatusList>();
	}
}
