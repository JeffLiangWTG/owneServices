using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaPack))]
	class AsycudaPackTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIAsycudaPack()
		{
			var bizObj = GetNewBusinessObject();
			Factory.Save();
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<Integration.Customs.ASYCUDA.MXManifest.IAsycudaPack>(bizObj.PK).GetType());
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<AsycudaPack>(bizObj.PK).GetType());
		}

		public void TestBill()
		{
			var pack = (AsycudaPack)GetNewBusinessObject();
			AssertType<AsycudaBill>(pack.Bill);
		}

		public void TestContainer()
		{
			var pack = (AsycudaPack)GetNewBusinessObject();
			AssertType<AsycudaContainer>(pack.Container);
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var manifestHeader = factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var container = manifestHeader.Containers.AddNew();
			pack.ContainerPK = container.PK;
			return pack;
		}

		public void TestIfAPA_GrossWeightOrUQChangedTotalPacksGrossWeightChange()
		{
			var bill = Factory.NewWithValidTestData<AsycudaBill>();
			bill.ABL_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			bill.ABL_GrossWeight = 5;

			var pack1 = bill.Packs.AddNew();
			pack1.APA_Weight = 3;
			pack1.APA_WeightUQ = Core.Constants.Weight.Kilograms;

			AssertEquals("TotalPacksGrossWeight should be equal to the sum of the weights of the packs.", (ZDecimal)3, bill.TotalPacksGrossWeight);

			pack1.APA_WeightUQ = Core.Constants.Weight.Hectograms;

			AssertEquals("TotalPacksGrossWeight should be updated if APA_WeightUQ or APA_Weight changes.", (ZDecimal)0.3, bill.TotalPacksGrossWeight);

			pack1.APA_Weight = 30;

			AssertEquals("TotalPacksGrossWeight should be updated if APA_WeightUQ or APA_Weight changes.", (ZDecimal)3, bill.TotalPacksGrossWeight);
		}

		public void TestIfAPA_VolumeOrUQChangedTotalPacksVolumeChange()
		{
			var bill = Factory.NewWithValidTestData<AsycudaBill>();
			bill.ABL_VolumeUQ = Core.Constants.Volume.Litre;
			bill.ABL_Volume = 5;

			var pack1 = bill.Packs.AddNew();
			pack1.APA_Volume = 3;
			pack1.APA_VolumeUQ = Core.Constants.Volume.Litre;

			AssertEquals("TotalPacksVolume should be equal to the sum of the volume of the packs.", (ZDecimal)3, bill.TotalPacksVolume);

			pack1.APA_VolumeUQ = Core.Constants.Volume.CubicCentimeters;
			AssertEquals("TotalPacksVolume should be updated if APA_VolumeUQ or APA_Volume changes.", (ZDecimal)0.003, bill.TotalPacksVolume);

			pack1.APA_Volume = 30;
			AssertEquals("TotalPacksVolume should be updated if APA_VolumeUQ or APA_Volume changes.", (ZDecimal)0.03, bill.TotalPacksVolume);
		}
	}
}
