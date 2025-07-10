using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class CusTempStorageRegLineTransactionLookups : EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransactionLookups
	{
		public CusTempStorageRegLineTransactionLookups(CusTempStorageRegLineTransaction parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList TransactionTypeList => Factory.GetCachedValue<TransactionTypes>();

		public override CodeDescriptionPairList ReferenceTypeList => Factory.GetCachedValue<TransactionReferenceTypes>();
	}
}
