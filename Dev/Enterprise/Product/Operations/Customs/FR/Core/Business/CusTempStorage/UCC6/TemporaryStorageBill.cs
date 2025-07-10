using System;
using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.FR.Business.CusTempStorage
{
	[DependentBusinessObject(typeof(TemporaryStorageHeader), "Bills")]
	public class TemporaryStorageBill : EU.Business.CusTempStorage.TemporaryStorageBill, Integration.Customs.FR.ITemporaryStorageBill
	{
		public TemporaryStorageBill(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ABL_GrossWeightUQ = Core.Constants.Weight.Kilograms;
		}

		#region Overrides

		[ReadOnly(true)]
		public override ZString ABL_GrossWeightUQ { get => base.ABL_GrossWeightUQ; set => base.ABL_GrossWeightUQ = value; }

		#endregion

		protected override Type GetPackTypeCore() => typeof(TemporaryStoragePack);

		protected override Type GetPackedItemTypeCore() => typeof(TemporaryStoragePackedItem);

		public new EU.Business.CusTempStorage.ITemporaryStoragePackedItemCollection<TemporaryStoragePackedItem, TemporaryStorageBill> PackedItems =>
			(EU.Business.CusTempStorage.ITemporaryStoragePackedItemCollection<TemporaryStoragePackedItem, TemporaryStorageBill>)base.PackedItems;

		protected override IAsycudaBillPackedItemCollection<AsycudaPackedItem, AsycudaBill> CreateNewAsycudaBillPackedItemCollection() =>
			new EU.Business.CusTempStorage.TemporaryStoragePackedItemCollection<TemporaryStoragePackedItem, TemporaryStorageBill>(this);
	}
}
