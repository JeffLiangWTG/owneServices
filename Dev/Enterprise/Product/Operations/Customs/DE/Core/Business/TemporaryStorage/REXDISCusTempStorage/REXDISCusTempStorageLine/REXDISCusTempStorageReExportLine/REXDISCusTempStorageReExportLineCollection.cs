using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class REXDISCusTempStorageReExportLineCollection : DecCusTempStorageLineCollectionFrom<REXDISCusTempStorageReExportLine, REXDISCusTempStorageDec>
	{
		public REXDISCusTempStorageReExportLineCollection(REXDISCusTempStorageDec parentStorageDec)
			: base(parentStorageDec)
		{
		}

		protected override void OnRemoving(BusinessObject child)
		{
			base.OnRemoving(child);
			var line = child as REXDISCusTempStorageReExportLine;
			if (line != null && line.SequenceNumberEnabled)
			{
				line.Dec?.LineNumberGenerator?.RecalculateWhenAboutToBeDetachedOrDeleted(line);
			}
		}
	}
}
