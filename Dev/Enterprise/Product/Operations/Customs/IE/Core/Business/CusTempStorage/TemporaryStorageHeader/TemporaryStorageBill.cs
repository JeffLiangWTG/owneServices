using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.IE.Business.CusTempStorage
{
	[DependentBusinessObject(typeof(TemporaryStorageHeader), nameof(TemporaryStorageHeader.Bills))]
	public class TemporaryStorageBill : EU.Business.CusTempStorage.TemporaryStorageBill, Integration.Customs.IE.ITemporaryStorageBill
	{
		public TemporaryStorageBill(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override Type GetPackedItemTypeCore() => typeof(TemporaryStoragePackedItem);

		public new EU.Business.CusTempStorage.ITemporaryStoragePackedItemCollection<TemporaryStoragePackedItem, TemporaryStorageBill> PackedItems =>
			(EU.Business.CusTempStorage.ITemporaryStoragePackedItemCollection<TemporaryStoragePackedItem, TemporaryStorageBill>)base.PackedItems;

		protected override IAsycudaBillPackedItemCollection<AsycudaPackedItem, AsycudaBill> CreateNewAsycudaBillPackedItemCollection() =>
			new EU.Business.CusTempStorage.TemporaryStoragePackedItemCollection<TemporaryStoragePackedItem, TemporaryStorageBill>(this);

		protected override EU.Business.CusTempStorage.ITemporaryStorageAdditionalInfoCollection<EU.Business.CusTempStorage.TemporaryStorageAdditionalInfo> CreateNewAdditionalInfoCollection() =>
			new EU.Business.CusTempStorage.TemporaryStorageAdditionalInfoCollection<TemporaryStorageAdditionalInfo>(this);

		protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
		{
			var result = base.GetCusSupportingInfoTypes();
			result[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(TemporaryStorageAdditionalInfo);
			return result;
		}

		protected override AsycudaBillValidation GetNewValidation()
		{
			if (Header?.AMA_ManifestType.EqualsIgnoringCase(ImportDeclarationApplicationCodeList.Codes.V1) ?? false)
			{
				return new TemporaryStorageBillValidationUCC5(this);
			}
			else
			{
				return new TemporaryStorageBillValidation(this);
			}
		}
		public new AsycudaBillValidation Validation => GetNewValidation();
	}
}
