using System.Linq;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IL.Manifest.Business.MessagesWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class DeclarationConsignmentConsignmentItemCommodityWrapperTest : DataProviderTestCase<IDeclarationConsignmentConsignmentItemCommodity>
	{
		public void TestCargoDescription()
		{
			asycudaPackedItem.API_GoodsDescription = "Goods Description";
			var wrapper = GetProvider();
			AssertEquals("Wrapper cargo description", "Goods Description", wrapper.CargoDescription.Value);
		}

		public void TestClassification()
		{
			AssertNotNull("Commodity Classification should not be null", Provider.Classification);
			AssertEquals("Commodity Classification should initially have 0 elements", 0, Provider.Classification.Count);

			asycudaPackedItem.API_Tariff = "1234567890";
			AssertEquals("Commodity Classification should have 1 element after setting API_Tariff", 1, Provider.Classification.Count);

			var regularClassification = Provider.Classification.FirstOrDefault(r => r.IdentificationTypeCode.Value == IL.Business.Constants.CustomsDeclaration.ClassificationIdentificationTypeCodeRegular);
			AssertNotNull("Regular Commodity Classification element should not be null", regularClassification);
			AssertType<DeclarationConsignmentConsignmentItemCommodityClassificationWrapper>("Regular Commodity Classification element should be of expected type", regularClassification);

			asycudaPackedItem.API_PackStatus = ILPackStatusList.Codes.D;
			var factory = Factory;
			var dangerousSubstance = factory.New<UNDGSubstance>();
			dangerousSubstance.DG_UNNO = "3267";
			dangerousSubstance.DG_PG = "I";
			var dangerousGoodsItem = asycudaPackedItem.UNDGs.AddNew();
			dangerousGoodsItem.DI_DG = dangerousSubstance.PK;
			dangerousGoodsItem.DI_TechnicalName = "TN";
			dangerousGoodsItem.DI_IsLimitedQuantity = true;

			AssertEquals("Commodity Classification should have 2 elements after adding dangerous goods", 2, Provider.Classification.Count);

			regularClassification = Provider.Classification.FirstOrDefault(r => r.IdentificationTypeCode.Value == IL.Business.Constants.CustomsDeclaration.ClassificationIdentificationTypeCodeRegular);
			AssertNotNull("Regular Commodity Classification element should not be null after adding dangerous goods", regularClassification);

			var dangerousClassification = Provider.Classification.FirstOrDefault(r => r.IdentificationTypeCode.Value == IL.Business.Constants.CustomsDeclaration.ClassificationIdentificationTypeCodeDangerous);
			AssertNotNull("Dangerous Commodity Classification element should not be null", dangerousClassification);
		}

		public void TestCategoryCode()
		{
			AssertNull("CategoryCode", Provider.CategoryCode);
		}

		public void TestCategoryQualifierCode()
		{
			AssertNull("CategoryQualifierCode", Provider.CategoryQualifierCode);
		}

		public void TestCommodityRelatedPackaging()
		{
			AssertNotNull("CommodityRelatedPackaging should not be null", Provider.CommodityRelatedPackaging);
			AssertEquals("CommodityRelatedPackaging should initially have 0 elements", 0, Provider.CommodityRelatedPackaging.Count);
			var factory = Factory;
			var substance3267 = factory.New<UNDGSubstance>();
			substance3267.DG_UNNO = "3267";
			substance3267.DG_PG = "I";
			var substance1104 = factory.New<UNDGSubstance>();
			substance1104.DG_UNNO = "1104";
			substance1104.DG_PG = "II";
			var dangerousGoodsItem1 = asycudaPackedItem.UNDGs.AddNew();
			dangerousGoodsItem1.DI_DG = substance3267.PK;
			dangerousGoodsItem1.DI_TechnicalName = "TN1";
			dangerousGoodsItem1.DI_IsLimitedQuantity = true;
			var dangerousGoodsItem2 = asycudaPackedItem.UNDGs.AddNew();
			dangerousGoodsItem2.DI_DG = substance1104.PK;
			dangerousGoodsItem2.DI_TechnicalName = "TN2";
			dangerousGoodsItem2.DI_IsLimitedQuantity = true;
			var commodityRelatedPackaging = Provider.CommodityRelatedPackaging;
			AssertNotNull("CommodityRelatedPackaging should not be null after adding dangerous goods", commodityRelatedPackaging);
			AssertEquals("CommodityRelatedPackaging should have 2 elements after adding dangerous goods", 2, commodityRelatedPackaging.Count);
			AssertType<DeclarationConsignmentItemCommodityCommodityRelatedPackagingWrapper>("First element should be of expected type", commodityRelatedPackaging.First());
		}

		public void TestId()
		{
			var asycudaManifestHeader = asycudaPackedItem.Header;
			asycudaManifestHeader.AMA_TransportMode = Core.Constants.TransportModes.Road;
			var transportMeans1 = asycudaManifestHeader.TransportMeans.AddNew();
			transportMeans1.JW_RL_NKDiscPort = "ZZFDE";
			transportMeans1.JW_Vessel = "123";
			var wrapper = GetProvider();
			AssertNull("ID is null when there is no TransportMean with arrival port that starts with 'IL'", wrapper.Id);
			var transportMeans2 = asycudaManifestHeader.TransportMeans.AddNew();
			transportMeans2.JW_RL_NKDiscPort = "ILJOR";
			transportMeans2.JW_Vessel = "456";
			wrapper = GetProvider();
			AssertEquals("ID is equal to vehicle ID '456' from TransportMean with arrival port that starts with 'IL'", "456", wrapper.Id.Value);
			asycudaManifestHeader.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			wrapper = GetProvider();
			AssertNull("ID is null when Transport Mode is not Road ('ROA')", wrapper.Id);
		}

		public void TestIdentityQualifierCode()
		{
			AssertNull("IdentityQualifierCode", Provider.IdentityQualifierCode);
		}

		public void TestIDTypeCode()
		{
			AssertNull("IDTypeCode", Provider.IDTypeCode);
		}

		public void TestTemperature()
		{
			AssertNotNull("Temperature", Provider.Temperature);
			AssertEquals("Temperature should have 0 elements", 0, Provider.Temperature.Count);
		}

		public void TestNewOrNull()
		{
			AssertNull(DeclarationConsignmentConsignmentItemCommodityWrapper.NewOrNull(null));
			AssertNotNull(DeclarationConsignmentConsignmentItemCommodityWrapper.NewOrNull(Factory.New<AsycudaPackedItem>()));
		}

		protected override IDeclarationConsignmentConsignmentItemCommodity GetProvider() => DeclarationConsignmentConsignmentItemCommodityWrapper.NewOrNull(asycudaPackedItem);

		protected override void SetUp()
		{
			base.SetUp();
			var asycudaManifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			asycudaPackedItem = asycudaManifestHeader.Bills.AddNew().PackedItems.AddNew();
		}

		AsycudaPackedItem asycudaPackedItem;
	}
}
