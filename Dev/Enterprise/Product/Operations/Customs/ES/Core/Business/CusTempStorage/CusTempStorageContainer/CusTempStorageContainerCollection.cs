using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ES.Business.CusTempStorage
{
	public class CusTempStorageContainerCollection : CusCodeDataCollection<CusTempStorageContainer>
	{
		public CusTempStorageContainerCollection(CusTempStorageDec master)
			: base(master, TempStorageContainerCode)
		{
		}
		const string TempStorageContainerCode = "TSC";

		protected override void OnCountChanged(CollectionCountChangedEventArgs e)
		{
			base.OnCountChanged(e);

			if (Master is CusTempStorageDec tempStorageDec)
			{
				var storageHeader = tempStorageDec.StorageHeader;
				if (storageHeader != null)
				{
					storageHeader.SJH_ContainerCount = Count;
				}
			}
		}
	}
}
