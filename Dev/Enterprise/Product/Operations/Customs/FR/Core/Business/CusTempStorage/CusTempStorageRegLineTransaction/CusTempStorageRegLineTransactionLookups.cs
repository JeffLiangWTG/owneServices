using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.CusTempStorage
{
	public class CusTempStorageRegLineTransactionLookups : EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransactionLookups
	{
		public CusTempStorageRegLineTransactionLookups(CusTempStorageRegLineTransaction parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList ReferenceTypeList => Factory.GetCachedValue<CusTempStorageRegLineTransactionReferenceTypeList>();
	}
}
