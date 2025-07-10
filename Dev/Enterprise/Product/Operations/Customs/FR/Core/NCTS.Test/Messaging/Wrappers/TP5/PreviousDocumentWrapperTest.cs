namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class PreviousDocumentWrapperTest : Customs.Business.Testing.DataProviderTestCase<PreviousDocumentWrapper>
	{
		public void TestType()
		{
			AssertEquals("Type should be mapped to CSI_Code.", "Code", Provider.Type);
		}

		public void TestReferenceNumber()
		{
			AssertEquals("ReferenceNumber should be mapped to CSI_ReferenceNumber.", "ReferenceNumber", Provider.ReferenceNumber);
		}

		public void TestGoodsItemNumber()
		{
			AssertEquals("GoodsItemNumber should be mapped to CSI_ItemNumber.", 99, Provider.GoodsItemNumber);
		}

		public void TestTypeOfPackages()
		{
			AssertEquals("TypeOfPackages should be mapped to CSI_UnitOfQuantity2.", "CTN", Provider.TypeOfPackages);
		}

		public void TestNumberOfPackages()
		{
			AssertEquals("NumberOfPackages should be mapped to CSI_Quantity2.", 45m, Provider.NumberOfPackages);
		}

		public void TestMeasurementUnitAndQualifier()
		{
			AssertEquals("MeasurementUnitAndQualifier should be mapped to CSI_UnitOfQuantity.", "HLT", Provider.MeasurementUnitAndQualifier);
		}

		public void TestQuantity()
		{
			AssertEquals("Quantity should be mapped to CSI_Quantity.", 99.999m, Provider.Quantity);
		}

		public void TestComplementOfInformation()
		{
			AssertEquals("ComplementOfInformation should be mapped to CSI_ReferenceNumber2.", "ReferenceNumber2", Provider.ComplementOfInformation);
		}

		protected override PreviousDocumentWrapper GetProvider()
		{
			var previousDocument = Factory.New<EU.NCTS.Business.NctsPreviousDocument>();
			previousDocument.CSI_Code = "Code";
			previousDocument.CSI_ReferenceNumber = "ReferenceNumber";
			previousDocument.CSI_ItemNumber = 99;
			previousDocument.CSI_UnitOfQuantity2 = "CTN";
			previousDocument.CSI_Quantity2 = 45;
			previousDocument.CSI_UnitOfQuantity = "HLT";
			previousDocument.CSI_Quantity = 99.999m;
			previousDocument.CSI_ReferenceNumber2 = "ReferenceNumber2";
			return PreviousDocumentWrapper.New(previousDocument);
		}
	}
}
