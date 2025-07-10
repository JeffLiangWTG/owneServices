using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business.CusTempStorage
{
	public class CusTempStorageRegHeaderLookups : EU.TemporaryStorage.Business.CusTempStorageRegHeaderLookups
	{
		public CusTempStorageRegHeaderLookups(CusTempStorageRegHeader parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList StatusList => Factory.GetCachedValue<TempStorageDeclarationStatusList>();

		public override CodeDescriptionPairList PreviousReferenceTypeList => Factory.GetCachedValue<PreviousReferenceTypeCodeList>();
	}
}
