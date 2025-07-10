using System;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class CusTempStorageDecCollection<T, MasterT> : CusTempStorageDecCollection
		where T : CusTempStorageDec
		where MasterT : CusTempStorageJobHeader
	{
		public CusTempStorageDecCollection(MasterT parentStorageHeader)
			: base(parentStorageHeader)
		{
		}

		public new T this[int i] => (T)base[i];

		public new MasterT Master => (MasterT)base.Master;

		public new T AddNew() => (T)base.AddNew();

		public new T AddNew(Type bizObjType) => (T)base.AddNew(bizObjType);

		public override Type GetTypeOfElementsFromPK(ZGuid pk) => typeof(T);
	}
}
