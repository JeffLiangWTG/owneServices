using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;

public class CusTempStorageRegLineTransactionLookups : AutoCusTempStorageRegLineTransactionLookups
{
	public CusTempStorageRegLineTransactionLookups(AutoCusTempStorageRegLineTransaction parent) : base(parent)
	{
	}

	public virtual CodeDescriptionPairList TransactionTypeList => Factory.GetCachedValue<CusTempStorageRegLineTransactionTypeList>();

	public virtual CodeDescriptionPairList TransactionStatusList => Factory.GetCachedValue<CusTempStorageRegLineTransactionStatusList>();

	public virtual CodeDescriptionPairList ReferenceTypeList => Factory.GetCachedValue<CusTempStorageRegLineTransactionReferenceTypeList>();

	public virtual CodeDescriptionPairList InternalReferenceTypeList => Factory.GetCachedValue<CusTempStorageRegLineTransactionInternalReferenceTypeList>();
}
