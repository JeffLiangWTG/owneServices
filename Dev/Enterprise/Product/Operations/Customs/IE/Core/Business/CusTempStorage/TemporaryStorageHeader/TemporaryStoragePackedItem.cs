using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.IE.Business.CusTempStorage
{
	public class TemporaryStoragePackedItem : EU.Business.CusTempStorage.TemporaryStoragePackedItem
	{
		public TemporaryStoragePackedItem(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[ChildEditable]
		public new EU.Business.CusTempStorage.TemporaryStorageAdditionalInfoCollection<TemporaryStorageAdditionalInfo> AdditionalInfos =>
			(EU.Business.CusTempStorage.TemporaryStorageAdditionalInfoCollection<TemporaryStorageAdditionalInfo>)base.AdditionalInfos;

		protected override EU.Business.CusTempStorage.ITemporaryStorageAdditionalInfoCollection<EU.Business.CusTempStorage.TemporaryStorageAdditionalInfo> CreateNewAdditionalInfoCollection() =>
			new EU.Business.CusTempStorage.TemporaryStorageAdditionalInfoCollection<TemporaryStorageAdditionalInfo>(this);

		protected override AsycudaPackedItemValidation GetNewValidation() => new TemporaryStoragePackedItemValidation(this);
	}
}
