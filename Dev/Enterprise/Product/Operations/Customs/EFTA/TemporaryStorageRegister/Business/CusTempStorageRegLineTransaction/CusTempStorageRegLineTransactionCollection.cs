using CargoWise.EntityFramework;
using static Enterprise.Integration.Customs.TemporaryStorage;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;

public class CusTempStorageRegLineTransactionCollection<T> : CusTempStorageRegLineTransactionCollection
	where T : CusTempStorageRegLineTransaction
{
	public CusTempStorageRegLineTransactionCollection(CusTempStorageRegLine line)
		: base(line)
	{
	}

	public new T this[int i] => (T)base[i];

	public new T AddNew() => (T)base.AddNew();
}

public abstract class CusTempStorageRegLineTransactionCollection : ActiveBusinessObjectCollection<CusTempStorageRegLineTransaction>
	, ICusTempStorageRegLineTransactionCollection<CusTempStorageRegLineTransaction>
{
	protected CusTempStorageRegLineTransactionCollection(CusTempStorageRegLine line)
		: base(line.Factory, line)
	{
	}
}
