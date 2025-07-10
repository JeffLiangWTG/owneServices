using System.Linq;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	sealed class MConsignment04ProviderTest : DataProviderTestCase<MConsignment04Provider>
	{
		public void TestContainerIndicator()
		{
			InitBillAndProvider();
			AssertNull("Container Indicator", mConsignment04Provider.ContainerIndicator);
		}

		public void TestInlandModeOfTransport()
		{
			InitBillAndProvider();
			AssertNull("Inland Mode Of Transport", mConsignment04Provider.InlandModeOfTransport);
		}

		public void TestModeOfTransportAtTheBorder()
		{
			InitBillAndProvider();
			AssertNull("Mode Of Transport At The Border", mConsignment04Provider.ModeOfTransportAtTheBorder);
		}

		public void TestGrossMass()
		{
			bill = Factory.New<AsycudaBill>();
			bill.ABL_GrossWeight = 98;
			bill.ABL_GrossWeightUQ = "KG";
			mConsignment04Provider = new MConsignment04Provider(bill);

			AssertEquals("Gross Mass", 98m, mConsignment04Provider.GrossMass);

			bill.ABL_GrossWeightUQ = "G";
			AssertEquals("Gross Mass", 0.098m, mConsignment04Provider.GrossMass);
		}

		public void TestTotalPackageNumber()
		{
			bill = Factory.New<AsycudaBill>();
			bill.ABL_ManifestQty = 99;
			mConsignment04Provider = new MConsignment04Provider(bill);

			AssertEquals("Total Package Number", 99, mConsignment04Provider.TotalPackageNumber);
		}

		public void TestReferenceNumberUCR()
		{
			bill = Factory.New<AsycudaBill>();
			bill.ABL_UCRNumber = "UCRNumber";
			mConsignment04Provider = new MConsignment04Provider(bill);

			AssertEquals("Reference Number UCR", "UCRNumber", mConsignment04Provider.ReferenceNumberUCR);
		}

		public void TestTransportEquipments()
		{
			InitBillAndProvider();
			AssertEquals("Transport equipment should always be empty for H7 message", 0, mConsignment04Provider.TransportEquipments.Count);
		}

		public void TestLocationOfGoods()
		{
			bill = Factory.New<AsycudaBill>();
			bill.CusGoodsLocation.CGL_AdditionalIdentifier = "ABC";
			mConsignment04Provider = new MConsignment04Provider(bill);

			CombineAssertions(() =>
			{
				AssertNotNull("Location Of Goods", mConsignment04Provider.LocationOfGoods);
				AssertEquals("ABC", mConsignment04Provider.LocationOfGoods.AdditionalIdentifier);
			});
		}

		public void TestArrivalTransportMeans()
		{
			InitBillAndProvider();
			AssertNull("Arrival Transport Means", mConsignment04Provider.ArrivalTransportMeans);
		}

		public void TestActiveBorderTransportMeansNationality()
		{
			InitBillAndProvider();
			AssertNull("Active Border Transport Means Nationality", mConsignment04Provider.ActiveBorderTransportMeansNationality);
		}

		public void TestTransportDocuments()
		{
			InitBillAndProvider();
			var additionalDocument1 = bill.AdditionalDocuments.AddNew();
			additionalDocument1.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.TransportDocument;
			additionalDocument1.CSI_Code = "N704";
			additionalDocument1.CSI_ReferenceNumber = "RN123";
			var additionalDocument2 = bill.AdditionalDocuments.AddNew();
			additionalDocument2.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.TransportDocument;
			additionalDocument2.CSI_Code = "N714";
			additionalDocument2.CSI_ReferenceNumber = "RN456";
			mConsignment04Provider = new MConsignment04Provider(bill);

			AssertNotNull("Transport Documents", mConsignment04Provider.TransportDocuments);
			var transportDocuments = mConsignment04Provider.TransportDocuments.ToList();

			CombineAssertions(() =>
			{
				AssertEquals("Transport Documents Count", 2, transportDocuments.Count);
				AssertEquals("Transport Document Type", "N704", transportDocuments[0].Type);
				AssertEquals("Transport Document ReferenceNumber", "RN123", transportDocuments[0].Reference);
				AssertEquals("Transport Document Type", "N714", transportDocuments[1].Type);
				AssertEquals("Transport Document ReferenceNumber", "RN456", transportDocuments[1].Reference);
			});
		}

		public void TestTransportAndInsuranceCostsToTheDestination()
		{
			InitBillAndProvider();
			var transportAndInsuranceCostsToTheDestination = mConsignment04Provider.TransportAndInsuranceCostsToTheDestination;

			CombineAssertions(() =>
			{
				AssertNotNull("Transport And Insurance Costs To The Destination", transportAndInsuranceCostsToTheDestination);
				AssertEquals(0m, transportAndInsuranceCostsToTheDestination.Amount);
				AssertNull(transportAndInsuranceCostsToTheDestination.Currency);
			});

			bill.ABL_InsuranceValue = 10;
			bill.ABL_TransportValue = 10;
			bill.ABL_RX_NKTransportValueCurrency = "USD";
			bill.ABL_RX_NKInsuranceValueCurrency = "USD";
			bill.ABL_RX_NKGoodsValueCurrency = "AUD";
			mConsignment04Provider = new MConsignment04Provider(bill);

			transportAndInsuranceCostsToTheDestination = mConsignment04Provider.TransportAndInsuranceCostsToTheDestination;
			CombineAssertions(() =>
			{
				AssertNotNull(transportAndInsuranceCostsToTheDestination);
				AssertEquals(20m, transportAndInsuranceCostsToTheDestination.Amount);
				AssertEquals("USD", transportAndInsuranceCostsToTheDestination.Currency);
			});
		}

		protected override MConsignment04Provider GetProvider()
		{
			InitBillAndProvider();
			return mConsignment04Provider;
		}

		AsycudaBill bill;
		MConsignment04Provider mConsignment04Provider;

		void InitBillAndProvider()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			bill = header.Bills.AddNew();
			mConsignment04Provider = new MConsignment04Provider(bill);
		}
	}
}
