using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IL.Manifest.Business.MessagesWrappers;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class DeclarationConsignmentGoodsReceiptPlaceWrapperTest : DataProviderTestCase<DeclarationConsignmentGoodsReceiptPlaceWrapper>
	{
		public void TestId()
		{
			AssertNotNull("ID", Provider.Id);
			AssertEquals("ID should be equal to the expected value", "DEST1", Provider.Id.Value);

			var manifest = Factory.New<AsycudaManifestHeader>();
			var asycudaBill = manifest.Bills.AddNew();
			asycudaBill.ABL_RL_NKFinalDestination = "DEST2";
			var declarationConsignmentGoodsReceiptPlaceWrapper = DeclarationConsignmentGoodsReceiptPlaceWrapper.NewOrNull(asycudaBill);
			AssertNotNull("When asycudaBill exist, wrapper should build", declarationConsignmentGoodsReceiptPlaceWrapper);
			AssertNull("When standalone manifest, GoodsReceiptPlace should be empty", declarationConsignmentGoodsReceiptPlaceWrapper.Id);
		}

		public void TestNewOrNull()
		{
			AssertNull("When asycudaBill is null", DeclarationConsignmentGoodsReceiptPlaceWrapper.NewOrNull(null));
			AssertNotNull("When asycudaBill is not null", DeclarationConsignmentGoodsReceiptPlaceWrapper.NewOrNull(Factory.New<AsycudaBill>()));
		}

		protected override DeclarationConsignmentGoodsReceiptPlaceWrapper GetProvider()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKDischargePort = "DEST1";

			var manifest = Factory.New<AsycudaManifestHeader>();
			manifest.SetParent(consol);

			var shipment = consol.Shipments.AddNew();
			var asycudaBill = manifest.Bills.AddNew();
			asycudaBill.ABL_JS_Shipment = shipment.PK;
			asycudaBill.ABL_RL_NKFinalDestination = "DEST2";

			return DeclarationConsignmentGoodsReceiptPlaceWrapper.NewOrNull(asycudaBill);
		}
	}
}
