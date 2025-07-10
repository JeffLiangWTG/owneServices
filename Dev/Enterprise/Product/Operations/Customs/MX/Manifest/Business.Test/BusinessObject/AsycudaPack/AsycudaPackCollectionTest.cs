using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaPackCollection))]
	class AsycudaPackCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override Type GetExpectedCollectionType() => typeof(AsycudaPackCollection);

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			return bill.Packs;
		}

		public void TestIfAPackIsDeletedTotalPacksGrossWeightChange()
		{
			var bill = Factory.NewWithValidTestData<AsycudaBill>();
			bill.ABL_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			bill.ABL_GrossWeight = 6;

			var pack1 = bill.Packs.AddNew();
			pack1.APA_Weight = 2;
			pack1.APA_WeightUQ = Core.Constants.Weight.Kilograms;

			var pack2 = bill.Packs.AddNew();
			pack2.APA_Weight = 4000;
			pack2.APA_WeightUQ = Core.Constants.Weight.Grams;

			AssertEquals("TotalPacksGrossWeight should be equal to the sum of the weights of the packs.", (ZDecimal)6, bill.TotalPacksGrossWeight);

			bill.Packs.RemoveAndDelete(pack2);
			bill.Validation.ValidateAll();

			AssertEquals("TotalPacksGrossWeight should be updated if one pack is deleted.", (ZDecimal)2, bill.TotalPacksGrossWeight);
		}

		public void TestIfAPackIsDeletedTotalPacksVolumeChange()
		{
			var bill = Factory.NewWithValidTestData<AsycudaBill>();
			bill.ABL_VolumeUQ = Core.Constants.Volume.Litre;
			bill.ABL_Volume = 6;

			var pack1 = bill.Packs.AddNew();
			pack1.APA_Volume = 2;
			pack1.APA_VolumeUQ = Core.Constants.Volume.Litre;

			var pack2 = bill.Packs.AddNew();
			pack2.APA_Volume = 4000;
			pack2.APA_VolumeUQ = Core.Constants.Volume.CubicCentimeters;

			AssertEquals("TotalPacksVolume should be equal to the sum of the volume of the packs.", (ZDecimal)6, bill.TotalPacksVolume);

			bill.Packs.RemoveAndDelete(pack2);
			bill.Validation.ValidateAll();

			AssertEquals("TotalPacksVolume should be updated if one pack is deleted.", (ZDecimal)2, bill.TotalPacksVolume);
		}
	}
}
