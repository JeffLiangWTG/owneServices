using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	sealed class IE013AndIE015ConsignmentItemPreviousDocumentProviderTest : Customs.Business.Testing.DataProviderTestCase<IE013AndIE015ConsignmentItemPreviousDocumentProvider>
	{
		public void TestType()
		{
			previousDocument.CSI_Code = "123";
			AssertEquals("Type", "123", Provider.Type);
		}

		public void TestReference()
		{
			previousDocument.CSI_ReferenceNumber = "123";
			AssertEquals("Reference", "123", Provider.Reference);
		}

		public void TestGoodsItemNumber()
		{
			previousDocument.CSI_ItemNumber = 12;
			AssertEquals("GoodsItemNumber", 12, Provider.GoodsItemNumber);
		}

		public void TestTypeOfPackages()
		{
			previousDocument.CSI_UnitOfQuantity2 = "KG";
			AssertEquals("TypeOfPackages", "KG", Provider.TypeOfPackages);
		}

		public void TestNumberOfPackages()
		{
			previousDocument.CSI_Quantity2 = 23.64m;
			AssertEquals("NumberOfPackages", 23, Provider.NumberOfPackages);
		}

		public void TestMeasurementUnitAndQualifier()
		{
			previousDocument.CSI_UnitOfQuantity = "KG";
			AssertEquals("MeasurementUnitAndQualifier", "KG", Provider.MeasurementUnitAndQualifier);
		}

		public void TestQuantity()
		{
			previousDocument.CSI_Quantity = 56.78m;
			AssertEquals("Quantity", 56.78m, Provider.Quantity);
		}

		public void TestComplementOfInformation()
		{
			previousDocument.CSI_ReferenceNumber2 = "456";
			AssertEquals("ComplementOfInformation", "456", Provider.ComplementOfInformation);
		}

		protected override IE013AndIE015ConsignmentItemPreviousDocumentProvider GetProvider() => new IE013AndIE015ConsignmentItemPreviousDocumentProvider(previousDocument);

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			previousDocument = goodsItem.PreviousDocuments.AddNew();
		}
		NctsHeader nctsHeader;
		NctsPreviousDocument previousDocument;
	}
}
