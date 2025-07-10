using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ManifestBase.Testing
{
	[TestedType(typeof(AsycudaPackedItemTaxCollection<AsycudaTax, AsycudaPackedItemWithIAsycudaTaxTypeSupporter>))]
	class AsycudaPackedItemTaxCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override Type GetExpectedCollectionType() => typeof(AsycudaPackedItemTaxCollection<AsycudaTax, AsycudaPackedItemWithIAsycudaTaxTypeSupporter>);

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var packedItemed = Factory.New<AsycudaPackedItemWithIAsycudaTaxTypeSupporter>();
			bill.PackedItems.Add(packedItemed);
			return new AsycudaPackedItemTaxCollection<AsycudaTax, AsycudaPackedItemWithIAsycudaTaxTypeSupporter>(packedItemed);
		}
	}

	class AsycudaPackedItemWithIAsycudaTaxTypeSupporter : AsycudaPackedItem
		, IAsycudaTaxTypeSupporter
	{
		public AsycudaPackedItemWithIAsycudaTaxTypeSupporter(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Type GetAsycudaTaxTypeCore() => typeof(AsycudaTax);
	}
}
