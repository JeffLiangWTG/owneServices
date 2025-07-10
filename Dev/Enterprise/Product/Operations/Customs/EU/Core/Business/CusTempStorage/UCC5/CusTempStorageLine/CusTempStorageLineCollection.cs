using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class CusTempStorageLineCollection<T, MasterT> : CusTempStorageLineCollection
		where T : CusTempStorageLine
		where MasterT : CusTempStorageDec
	{
		public CusTempStorageLineCollection(MasterT parentStorageDec)
			: base(parentStorageDec)
		{
		}

		public new T this[int i] => (T)base[i];

		public new MasterT Master => (MasterT)base.Master;

		public new T AddNew() => (T)base.AddNew();

		public new T AddNew(Type bizObjType) => (T)base.AddNew(bizObjType);

		public override Type GetTypeOfElementsFromPK(ZGuid pk) => typeof(T);

		protected override void OnRemoving(BusinessObject child)
		{
			base.OnRemoving(child);
			var line = child as CusTempStorageLine;
			if (line != null && line.SequenceNumberEnabled)
			{
				line.Dec?.LineNumberGenerator?.RecalculateWhenAboutToBeDetachedOrDeleted(line);
			}
		}
	}
}
