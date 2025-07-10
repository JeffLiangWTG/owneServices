using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class CustomsWarehouseGoodsItemProviderTest : Customs.Business.Testing.DataProviderTestCase<CustomsWarehouseGoodsItemProvider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertNull("NULL", CustomsWarehouseGoodsItemProvider.NewOrNull(null));
				AssertNotNull("Not NULL", CustomsWarehouseGoodsItemProvider.NewOrNull(previousDocument));
			});
		}

		public void TestReferencedRegistrationNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Empty", ZString.Empty, Provider.ReferencedRegistrationNumber);

				previousDocument.CSI_ReferenceNumber = "REF12345";
				AssertEquals("Not empty", "REF12345", Provider.ReferencedRegistrationNumber);
			});
		}

		public void TestReferencedSequenceNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Empty", ZInt.Zero, Provider.ReferencedSequenceNumber);

				previousDocument.CSI_LineNo = 2;
				AssertEquals("Not empty", 2, Provider.ReferencedSequenceNumber);
			});
		}

		public void TestAccessViaATLASFlag()
		{
			CombineAssertions(() =>
			{
				AssertEquals("false", false, Provider.AccessViaATLASFlag);

				previousDocument.Status = true;
				AssertEquals("true", true, Provider.AccessViaATLASFlag);
			});
		}

		public void TestCommodityCode()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Empty", ZString.Empty, Provider.CommodityCode);

				previousDocument.CSI_Tariff = "12345678901";
				AssertEquals("Not empty", "12345678901", Provider.CommodityCode);
			});
		}

		public void TestUsualProcessingFlag()
		{
			CombineAssertions(() =>
			{
				AssertEquals("false", false, Provider.UsualProcessingFlag);

				previousDocument.UsualProcessingFlag = true;
				AssertEquals("true", true, Provider.UsualProcessingFlag);
			});
		}

		public void TestComplement()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Empty", ZString.Empty, Provider.Complement);

				previousDocument.CSI_Description = "Description";
				AssertEquals("Not empty", "Description", Provider.Complement);
			});
		}

		public void TestCommercialAmount()
		{
			CombineAssertions(() =>
			{
				AssertNull("UsualProcessingFlag: false, Quantity: 0", Provider.CommercialAmount);

				previousDocument.UsualProcessingFlag = true;
				AssertNull("UsualProcessingFlag: true, Quantity: 0", Provider.CommercialAmount);

				previousDocument.CSI_Quantity = 1m;
				AssertNotNull("UsualProcessingFlag: true, Quantity: 1", Provider.CommercialAmount);

				previousDocument.UsualProcessingFlag = false;
				AssertNull("UsualProcessingFlag: false, Quantity: 1", Provider.CommercialAmount);
			});
		}

		public void TestDebitAmount()
		{
			AssertNotNull(Provider.DebitAmount);
		}

		protected override CustomsWarehouseGoodsItemProvider GetProvider() => CustomsWarehouseGoodsItemProvider.NewOrNull(previousDocument);

		protected override void SetUp()
		{
			base.SetUp();
			previousDocument = Factory.New<PreviousDocument>();
		}
		PreviousDocument previousDocument;
	}
}
