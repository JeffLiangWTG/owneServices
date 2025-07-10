using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class REXDISCusTempStorageSumALineCollection : EU.Business.CusTempStorage.CusTempStorageLineCollectionTo<REXDISCusTempStorageSumALine, REXDISCusTempStorageReExportLine>
	{
		public REXDISCusTempStorageSumALineCollection(REXDISCusTempStorageReExportLine reExportLine)
			: base(reExportLine)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			var cusTempStorageLine = (REXDISCusTempStorageSumALine)child;
			if (FromLine.IsREGDeclaration)
			{
				using (cusTempStorageLine.SuspendSettingHasChanges())
				{
					cusTempStorageLine.TSL_OwnerReferenceType = TemporaryStorageIdentificationIndicatorList.Codes.REG;
				}
			}

			base.SetDefaultsForNewChild(child);
		}
	}
}
