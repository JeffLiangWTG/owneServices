using System.Linq;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IL.Manifest.Business.MessagesWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class DeclarationConsignmentWrapperTest : DataProviderTestCase<DeclarationConsignmentWrapper>
	{
		public void TestAcceptancePlace()
		{
			AssertNotNull("AcceptancePlace", Provider.AcceptancePlace);
			AssertEquals("AcceptancePlace should have exactly 1 entry", 1, Provider.AcceptancePlace.Count);
		}

		public void TestAdditionalDocument()
		{
			AssertNotNull("AdditionalDocument", Provider.AdditionalDocument);
			AssertEquals("AdditionalDocument should have 0 entries", 0, Provider.AdditionalDocument.Count);
		}

		public void TestAssociatedTransportDocument()
		{
			AssertNotNull("AssociatedTransportDocument", Provider.AssociatedTransportDocument);
			AssertEquals("AssociatedTransportDocument should have 0 entries", 0, Provider.AssociatedTransportDocument.Count);
		}

		public void TestBoardedQuantity()
		{
			AssertNull("BoardedQuantity", Provider.BoardedQuantity);
		}

		public void TestCarrier()
		{
			AssertNotNull("Carrier", Provider.Carrier);
			AssertEquals("Carrier should have 0 entries", 0, Provider.Carrier.Count);
		}

		public void TestConsignee()
		{
			AssertNotNull("Consignee", Provider.Consignee);
			AssertEquals("Consignee should have exactly 1 entry", 1, Provider.Consignee.Count);
		}

		public void TestConsignmentItem()
		{
			AssertNotNull("ConsignmentItem", Provider.ConsignmentItem);
			AssertEquals("ConsignmentItem should have 3 entries", 3, Provider.ConsignmentItem.Count);
		}

		public void TestConsignor()
		{
			AssertNotNull("Consignor", Provider.Consignor);
			AssertEquals("Consignor should have exactly 1 entry", 1, Provider.Consignor.Count);
		}

		public void TestFreight()
		{
			AssertNotNull("Freight", Provider.Freight);
			AssertEquals("Freight should have 0 entries", 0, Provider.Freight.Count);
		}

		public void TestGoodsConsignedPlace()
		{
			AssertNotNull("GoodsConsignedPlace", Provider.GoodsConsignedPlace);
			AssertEquals("GoodsConsignedPlace should have 0 entries", 0, Provider.GoodsConsignedPlace.Count);
		}

		public void TestGoodsReceiptPlace()
		{
			AssertNotNull("GoodsReceiptPlace", Provider.GoodsReceiptPlace);
			AssertEquals("GoodsReceiptPlace should have exactly 1 entry", 1, Provider.GoodsReceiptPlace.Count);
		}

		public void TestGovernmentAgencyGoodsItem()
		{
			AssertNotNull("GovernmentAgencyGoodsItem", Provider.GovernmentAgencyGoodsItem);
			AssertEquals("GovernmentAgencyGoodsItem should have exactly 1 entry", 1, Provider.GovernmentAgencyGoodsItem.Count);
		}

		public void TestGrossVolumeMeasure()
		{
			AssertNotNull("GrossVolumeMeasure", Provider.GrossVolumeMeasure);
			AssertEquals("GrossVolumeMeasure should be equal to the expected value", 10m, Provider.GrossVolumeMeasure.Value);
			AssertEquals("GrossVolumeMeasure's language should be equal to the expected value", MeasurementUnitCommonCodeContentType.Kg, Provider.GrossVolumeMeasure.UnitCode);
		}

		public void TestLoadingLocation()
		{
			CombineAssertions("LoadingLocation without header", () =>
			{
				AssertNotNull(Provider.LoadingLocation);
				AssertEquals(1, Provider.LoadingLocation.Count);
				AssertNull(Provider.LoadingLocation.Single());
			});

			var header = Factory.New<AsycudaManifestHeader>();
			header.MasterBill.ABL_RL_NKPortOfLoading = "XYZ";
			var asycudaBill = header.Bills.AddNew();

			var wrapper = DeclarationConsignmentWrapper.NewOrNull(asycudaBill);
			CombineAssertions("LoadingLocation without header", () =>
			{
				AssertNotNull(wrapper.LoadingLocation);
				AssertEquals(1, wrapper.LoadingLocation.Count);
				AssertNotNull(wrapper.LoadingLocation.Single());
			});
		}

		public void TestNotifyParty()
		{
			AssertNotNull("NotifyParty", Provider.NotifyParty);
			AssertEquals("NotifyParty should have exactly 1 entry", 1, Provider.NotifyParty.Count);
		}

		public void TestSequenceNumeric()
		{
			AssertNotNull("SequenceNumeric", Provider.SequenceNumeric);
			AssertEquals("SequenceNumeric should be equal to the expected value", 3m, Provider.SequenceNumeric.Value);
		}

		public void TestTotalPackageQuantity()
		{
			AssertNotNull("TotalPackageQuantity", Provider.TotalPackageQuantity);
			AssertEquals("TotalPackageQuantity should be equal to the expected value", 17m, Provider.TotalPackageQuantity.Value);
			AssertEquals("TotalPackageQuantity unit should be null", null, Provider.TotalPackageQuantity.UnitCode);
		}

		public void TestTranshipmentLocation()
		{
			AssertNotNull("TranshipmentLocation", Provider.TranshipmentLocation);
			AssertEquals("TranshipmentLocation should have 0 entries", 0, Provider.TranshipmentLocation.Count);
		}

		public void TestTransitDestination()
		{
			AssertNotNull("TransitDestination", Provider.TransitDestination);
			AssertEquals("TransitDestination should have 0 entries", 0, Provider.TransitDestination.Count);
		}

		public void TestTransportContractDocument()
		{
			AssertNotNull("TransportContractDocument", Provider.TransportContractDocument);
			AssertEquals("TransportContractDocument should have exactly 3 entry", 3, Provider.TransportContractDocument.Count);
			AssertNotNull("ConditionCode", Provider.TransportContractDocument.FirstOrDefault().ConditionCode);
			AssertEquals("ConditionCode should be equal to the expected value", "27", Provider.TransportContractDocument.FirstOrDefault().ConditionCode.Value);
			AssertNull("ConditionCode", Provider.TransportContractDocument.Skip(1).First().ConditionCode);
			AssertNull("ConditionCode", Provider.TransportContractDocument.Skip(2).First().ConditionCode);
		}

		public void TestTransportSplitIndicator()
		{
			AssertNull("TransportSplitIndicator", Provider.TransportSplitIndicator);
		}

		public void TestUndgContact()
		{
			AssertNotNull("UNDGContact", Provider.UndgContact);
			AssertEquals("UNDGContact should have 3 entries", 3, Provider.UndgContact.Count);
		}

		public void TestUnloadingLocation()
		{
			AssertNotNull("UnloadingLocation", Provider.UnloadingLocation);
			AssertEquals("UnloadingLocation should have exactly 1 entry", 1, Provider.UnloadingLocation.Count);
		}

		public void TestValueAmount()
		{
			AssertNotNull("TotalPackageQuantity", Provider.ValueAmount);
			AssertEquals("TotalPackageQuantity should be equal to the expected value", 11m, Provider.ValueAmount.Value);
			AssertEquals("TotalPackageQuantity unit should be null", Iso3AlphaCurrencyCodeContentType.Usd, Provider.ValueAmount.CurrencyId);
		}

		public void TestNewOrNull()
		{
			AssertNull("When asycudaBill is null", DeclarationConsignmentWrapper.NewOrNull(null));
			AssertNotNull("When asycudaBill is not null", DeclarationConsignmentWrapper.NewOrNull(Factory.New<AsycudaBill>()));
		}

		protected override DeclarationConsignmentWrapper GetProvider()
		{
			var asycudaBill = Factory.New<AsycudaBill>();
			asycudaBill.ABL_Volume = 10;
			asycudaBill.ABL_VolumeUQ = "KG";
			asycudaBill.ABL_SequenceNumber = 3;
			asycudaBill.ABL_ManifestQty = 17;
			asycudaBill.ABL_CustomsValue = 11;
			asycudaBill.ABL_RX_NKCustomsValueCurrency = "USD";
			asycudaBill.ABL_OA_NotifyParty = ZGuid.NewZGuid();

			asycudaBill.ABL_Condition = "27";
			var firstTransportDocument = asycudaBill.TransportDocuments.AddNew();
			firstTransportDocument.CSI_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-1);
			var secondTransportDocument = asycudaBill.TransportDocuments.AddNew();
			secondTransportDocument.CSI_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-2);
			asycudaBill.TransportDocuments.AddNew();

			var packedItem1 = asycudaBill.PackedItems.AddNew();

			var dangerousGoodItem1_1 = packedItem1.UNDGs.AddNew();
			var dgContact1_1 = Factory.New<OrgContact>();
			dangerousGoodItem1_1.DI_OC_DGContact = dgContact1_1.PK;

			var dangerousGoodItem1_2 = packedItem1.UNDGs.AddNew();
			var dgContact1_2 = Factory.New<OrgContact>();
			dangerousGoodItem1_2.DI_OC_DGContact = dgContact1_2.PK;

			var packedItem2 = asycudaBill.PackedItems.AddNew();
			var dangerousGoodItem2 = packedItem2.UNDGs.AddNew();

			var packedItem3 = asycudaBill.PackedItems.AddNew();
			var dangerousGoodItem3 = packedItem3.UNDGs.AddNew();
			var dgContact3 = Factory.New<OrgContact>();
			dangerousGoodItem3.DI_OC_DGContact = dgContact3.PK;

			return DeclarationConsignmentWrapper.NewOrNull(asycudaBill);
		}
	}
}
