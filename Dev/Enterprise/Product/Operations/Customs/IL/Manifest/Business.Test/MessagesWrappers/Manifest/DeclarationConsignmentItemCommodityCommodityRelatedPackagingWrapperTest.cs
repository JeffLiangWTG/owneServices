using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class DeclarationConsignmentItemCommodityCommodityRelatedPackagingWrapperTest : Customs.Business.Testing.DataProviderTestCase<IDeclarationConsignmentConsignmentItemCommodityCommodityRelatedPackaging>
	{
		public void TestNewOrNull()
		{
			AssertNull("When dataItemUNDG is null", DeclarationConsignmentItemCommodityCommodityRelatedPackagingWrapper.NewOrNull(null));
			AssertNull("When dataItemUNDG is not valid", DeclarationConsignmentItemCommodityCommodityRelatedPackagingWrapper.NewOrNull(Factory.New<UNDGDataItem>()));
			AssertNotNull("When dataItemUNDG is valid", DeclarationConsignmentItemCommodityCommodityRelatedPackagingWrapper.NewOrNull(dataItemUNDG));
		}

		public void TestDangerousGoodsPackingRequirementGroupCode()
		{
			AssertEquals("When High Danger", "47", Provider.DangerousGoodsPackingRequirementGroupCode.Value);

			subs3267B.DG_PG = "II";
			AssertEquals("When Medium Danger", "48", GetProvider().DangerousGoodsPackingRequirementGroupCode.Value);

			subs3267B.DG_PG = "III";
			AssertEquals("When Low Danger", "49", GetProvider().DangerousGoodsPackingRequirementGroupCode.Value);
		}

		protected override IDeclarationConsignmentConsignmentItemCommodityCommodityRelatedPackaging GetProvider()
			=> DeclarationConsignmentItemCommodityCommodityRelatedPackagingWrapper.NewOrNull(dataItemUNDG);

		protected override void SetUp()
		{
			var factory = Factory;
			subs3267B = factory.New<UNDGSubstance>();
			subs3267B.DG_UNNO = "3267";
			subs3267B.DG_PG = "I";

			var packedItem = factory.New<AsycudaPackedItem>();
			dataItemUNDG = packedItem.UNDGs.AddNew();
			dataItemUNDG.DI_DG = subs3267B.PK;
			dataItemUNDG.DI_TechnicalName = "TN";
			dataItemUNDG.DI_IsLimitedQuantity = true;
		}

		UNDGDataItem dataItemUNDG;
		UNDGSubstance subs3267B;
	}
}
