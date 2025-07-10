using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1.Test
{
	public class IM413AndIM415GoodsShipmentProviderTest : DataProviderTestCase<IM413AndIM415GoodsShipmentProvider>
	{
		public void TestParties()
		{
			AssertType<Parties02Provider>(Provider.Parties);
		}

		public void TestDocumentsAuth()
		{
			AssertType<DocumentsAuthProvider>(Provider.DocumentsAuth);
		}

		public void TestGoodsInformation()
		{
			SetUpTestData();
			bill.ABL_GrossWeight = 10m;
			bill.ABL_GrossWeightUQ = "KG";
			CombineAssertions(() =>
			{
				AssertType<GoodsInformation02Provider>(Provider.GoodsInformation);
				AssertEquals(10m, Provider.GoodsInformation.GrossMass);
			});
		}

		public void TestDatesPlaces()
		{
			SetUpTestData();
			var goodsLocation = bill.CusGoodsLocation;
			goodsLocation.CGL_Type = "A";
			goodsLocation.CGL_Qualifier = "T";
			goodsLocation.CGL_AdditionalIdentifier = "AID";
			CombineAssertions(() =>
			{
				AssertType<LocationOfGoodsGNSSProvider>(Provider.DatesPlaces);
				AssertEquals("A", Provider.DatesPlaces.GoodsLocation.TypeOfLocation);
				AssertEquals("T", Provider.DatesPlaces.GoodsLocation.QualifierOfIdentification);
				AssertEquals("AID", Provider.DatesPlaces.GoodsLocation.AdditionalIdentifier);
			});
		}

		public void TestValuationInformation()
		{
			SetUpTestData();
			bill.ABL_RX_NKTransportValueCurrency = "USD";
			bill.ABL_InsuranceValue = 100;
			bill.ABL_TransportValue = 2;
			CombineAssertions(() =>
			{
				AssertType<ValuationInformationProvider>(Provider.ValuationInformation);
				AssertEquals(102m, Provider.ValuationInformation.TransportCosts.Amount);
				AssertEquals("USD", Provider.ValuationInformation.TransportCosts.Currency);
			});
		}

		public void TestGoodsShipmentItems()
		{
			var providerWithNoItems = GetProvider();
			AssertEquals(0, providerWithNoItems.GoodsShipmentItems.Count);

			bill.PackedItems.AddNew();
			bill.PackedItems.AddNew();
			AssertEquals("Goods Shipment Items Count", 2, Provider.GoodsShipmentItems.Count);
			AssertType<IM413AndIM415GoodsShipmentItemProvider[]>(Provider.GoodsShipmentItems);
		}

		protected override IM413AndIM415GoodsShipmentProvider GetProvider()
		{
			SetUpTestData();
			return new IM413AndIM415GoodsShipmentProvider(bill);
		}

		void SetUpTestData()
		{
			if (bill == null)
			{
				var header = Factory.New<AsycudaManifestHeader>();
				bill = header.Bills.AddNew();
			}
		}
		AsycudaBill bill;
	}
}
