using System.Linq;
using CargoWise.Customs.BR.MessageContracts.Mercante.Outgoing;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;

namespace Enterprise.Customs.BR.Manifest.Business.Testing
{
	class PackWrapperTest : TestCaseWithFactory
	{
		public void TestPackWrapper()
		{
			var manifestHeader = CreateAndPopulateManifestHeader();
			IManifest wrapper = new ManifestWrapper(manifestHeader);

			AssertEquals(2, wrapper.Bills.ElementAt(0).Packs.Count);

			manifestHeader.AMA_ContainerMode = Core.Constants.ContainerModes.Containerised;
			var pack = wrapper.Bills.ElementAt(0).Packs.ElementAt(0);
			CombineAssertions(() =>
			{
				AssertEquals(1, pack.CargoItemType);
				AssertEquals(1, pack.ItemSequentialNumber);
				AssertEquals(90.718m, pack.GrossWeight);
				AssertEquals("22G0", pack.ContainerType);
				AssertEquals("CAIU305178-6", pack.ContainerNumber);
				AssertEquals(1000m, pack.ContainerTare);
				AssertEquals(ZBool.True, pack.PartialUseContainerIndicator);
				AssertEquals(ZString.Empty, pack.LooseCargoPackageType);
				AssertEquals(ZString.Empty, pack.LooseCargoItemQuantity);
				AssertEquals(ZString.Empty, pack.PackageTypeBulk);
				AssertEquals(ZString.Empty, pack.DescriptionBulk);
				AssertEquals(ZString.Empty, pack.VehicleChassisNumber);
				AssertEquals(ZString.Empty, pack.BrandName);
				AssertEquals(ZString.Empty, pack.BrandNameCounterMarkPending);
				AssertEquals("12345", pack.DangerousGoodsCode);
				AssertEquals("1.1D", pack.DangerousGoodsClassCode);
				AssertEquals(0.003m, pack.Volume);
				AssertEquals("SEAL1", pack.ContainerSealNumber.ElementAt(0));
				AssertEquals("SEAL2", pack.ContainerSealNumber.ElementAt(1));
				AssertEquals("SEAL3", pack.ContainerSealNumber.ElementAt(2));
				AssertEquals("COMMODITY", pack.NCMGoodsCode.ElementAt(0));
			});

			manifestHeader.AMA_ContainerMode = Core.Constants.ContainerModes.BreakBulk;
			pack = wrapper.Bills.ElementAt(0).Packs.ElementAt(0);
			CombineAssertions(() =>
			{
				AssertEquals(2, pack.CargoItemType);
				AssertEquals(1, pack.ItemSequentialNumber);
				AssertEquals(ZString.Empty, pack.ContainerType);
				AssertEquals(ZString.Empty, pack.ContainerNumber);
				AssertEquals(0m, pack.ContainerTare);
				AssertEquals(ZBool.False, pack.PartialUseContainerIndicator);
				AssertEquals("XX", pack.LooseCargoPackageType);
				AssertEquals("10", pack.LooseCargoItemQuantity);
				AssertEquals(ZString.Empty, pack.DescriptionBulk);
				AssertEquals("12345", pack.DangerousGoodsCode);
				AssertEquals(ZString.Empty, pack.PackageTypeBulk);
			});

			manifestHeader.AMA_ContainerMode = Core.Constants.ContainerModes.Bulk;
			pack = wrapper.Bills.ElementAt(0).Packs.ElementAt(0);
			CombineAssertions(() =>
			{
				AssertEquals(3, pack.CargoItemType);
				AssertEquals(1, pack.ItemSequentialNumber);
				AssertEquals(ZString.Empty, pack.ContainerType);
				AssertEquals(ZString.Empty, pack.ContainerNumber);
				AssertEquals(0m, pack.ContainerTare);
				AssertEquals(ZBool.False, pack.PartialUseContainerIndicator);
				AssertEquals(ZString.Empty, pack.LooseCargoPackageType);
				AssertEquals(ZString.Empty, pack.LooseCargoItemQuantity);
				AssertEquals("GOODS", pack.DescriptionBulk);
				AssertEquals("12345", pack.DangerousGoodsCode);
				AssertEquals(BRBulkTypeList.Codes._99, pack.PackageTypeBulk);
			});

			manifestHeader.AMA_ContainerMode = ZString.Empty;
			pack = wrapper.Bills.ElementAt(0).Packs.ElementAt(0);
			CombineAssertions(() =>
			{
				AssertEquals(0, pack.CargoItemType);
				AssertEquals(1, pack.ItemSequentialNumber);
				AssertEquals(ZString.Empty, pack.ContainerType);
				AssertEquals(ZString.Empty, pack.ContainerNumber);
				AssertEquals(0m, pack.ContainerTare);
				AssertEquals(ZBool.False, pack.PartialUseContainerIndicator);
				AssertEquals(ZString.Empty, pack.LooseCargoPackageType);
				AssertEquals(ZString.Empty, pack.LooseCargoItemQuantity);
				AssertEquals(ZString.Empty, pack.DescriptionBulk);
				AssertEquals(ZString.Empty, pack.DangerousGoodsCode);
				AssertEquals(ZString.Empty, pack.PackageTypeBulk);
			});

			pack = wrapper.Bills.ElementAt(0).Packs.ElementAt(1);
			CombineAssertions(() =>
			{
				AssertEquals(2, pack.ItemSequentialNumber);

				AssertEquals(0, pack.ContainerSealNumber.Count);
			});

			manifestHeader.AMA_ContainerMode = Core.Constants.ContainerModes.Containerised;
			pack = wrapper.Bills.ElementAt(0).Packs.ElementAt(1);
			CombineAssertions(() =>
			{
				AssertEquals(0, pack.ContainerSealNumber.Count);
			});
		}

		AsycudaManifestHeader CreateAndPopulateManifestHeader()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			CreateAndPopulateContainer(header);
			CreateAndPopulatePack(bill);

			_ = bill.Packs.AddNew();

			return header;
		}

		void CreateAndPopulateContainer(AsycudaManifestHeader header)
		{
			var refContainer = Factory.New<RefContainer>();
			refContainer.RC_Code = "22G0";
			refContainer.RC_ContainerType = Core.Constants.ContainerTypes.FlatRack;
			refContainer.RC_TareWeight = 1000m;

			var containerMap = Factory.New<RefContainerCodeMap>();
			containerMap.RCM_RC_Container = refContainer.PK;
			containerMap.RCM_Code = "1891";
			containerMap.RCM_RN_NKCountry = Core.Constants.CountryCodes.Brazil;

			var container = header.Containers.AddNew();
			container.ACN_RC_ContainerType = refContainer.PK;
			container.ACN_ContainerNumber = "CAIU305178-6";
			container.ACN_EmptyFullIndicator = EmptyFullIndicatorList.Codes.LessThanFullContainerLoad;
			container.ACN_Seal1 = "SEAL1";
			container.ACN_Seal2 = "SEAL2";
			container.ACN_Seal3 = "SEAL3";
		}

		void CreateAndPopulatePack(AsycudaBill bill)
		{
			var pack = bill.Packs.AddNew();

			pack.ContainerPK = bill.Header.Containers[0].PK;
			pack.APA_CommodityCode = "COMMODITY";
			pack.APA_GoodsDescription = "GOODS";
			pack.APA_PackQty = 10;
			pack.APA_PackUQ = Core.Constants.PkgUnit.Bag;
			pack.APA_Volume = 200;
			pack.APA_VolumeUQ = Core.Constants.Volume.CubicInches;
			pack.APA_Weight = 200;
			pack.APA_WeightUQ = Core.Constants.Weight.Pounds;
			pack.BulkType = BRBulkTypeList.Codes._99;

			var undg = pack.UNDGs.AddNew();
			undg.DI_DG_NKSubs = "12345";
			undg.DI_IMOClass = "1.1D";
		}

		protected override void SetUp()
		{
			base.SetUp();

			var pack1 = Factory.New<CusRefPacks>();
			pack1.RP_CustomsPack = "XX";
			pack1.RP_Type = RPTypeList.Codes.GlobalManifestLine;
			pack1.RP_CustomsCountry = Core.Constants.CountryCodes.Brazil;
			pack1.RP_ConversionFactor = 1;
			pack1.RP_CommercialPack = Core.Constants.PkgUnit.Bag;

			Factory.Save();
		}
	}
}
