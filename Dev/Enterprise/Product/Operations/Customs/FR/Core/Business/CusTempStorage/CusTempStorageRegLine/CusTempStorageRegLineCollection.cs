using System;

namespace Enterprise.Customs.FR.Business.CusTempStorage
{
	public class CusTempStorageRegLineCollection : EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineCollection<CusTempStorageRegLine>
	{
		public CusTempStorageRegLineCollection(CusTempStorageRegHeader header) : base(header)
		{
		}

		public Action<CusTempStorageRegLine> OnLoadedIntoCollection;

		protected override void OnLoadedIntoCollectionCore(EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine loadedObject)
		{
			OnLoadedIntoCollection?.Invoke((CusTempStorageRegLine)loadedObject);
		}
	}
}
