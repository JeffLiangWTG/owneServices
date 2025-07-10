using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business.CusTempStorage
{
	public class CusTempStorageRegLineLookups : EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineLookups
	{
		public CusTempStorageRegLineLookups(CusTempStorageRegLine parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList CustomsStatusList => Factory.GetCachedValue<TempStorageDeclarationStatusList>();

		public override CodeDescriptionPairList UnionStatusList => Factory.GetCachedValue<ESUnionStatusList>();
	}
}
