using CargoWise.EntityFramework;
using static Enterprise.Integration.Customs.TemporaryStorage;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;

public class CusTempStorageRegLineCollection<T> : CusTempStorageRegLineCollection
	where T : CusTempStorageRegLine
{
	public CusTempStorageRegLineCollection(CusTempStorageRegHeader header)
		: base(header)
	{
	}

	public new T this[int i] => (T)base[i];

	public new T AddNew() => (T)base.AddNew();
}

public abstract class CusTempStorageRegLineCollection : ActiveBusinessObjectCollection<CusTempStorageRegLine>, ICusTempStorageRegLineCollection<CusTempStorageRegLine>
{
	protected CusTempStorageRegLineCollection(CusTempStorageRegHeader header)
		: base(header.Factory, header)
	{
	}
}
