using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	sealed class IM413AndIM415ProducedDocumentsWritingOffProviderTest : DataProviderTestCase<IM413AndIM415ProducedDocumentsWritingOffProvider>
	{
		public void TestIGoodsShipmentItemTypeProducedDocumentsWritingOff()
		{
			Assert("Should implement IGoodsShipmentItemTypeProducedDocumentsWritingOff", Provider is IGoodsShipmentItemTypeProducedDocumentsWritingOff);
		}

		public void TestId()
		{
			var supportingDocument = Factory.New<SupportingDocument>();
			supportingDocument.CSI_ReferenceNumber = "REF1";
			var provider = IM413AndIM415ProducedDocumentsWritingOffProvider.New(supportingDocument);
			AssertEquals("REF1", provider.Id);

			supportingDocument.CSI_ReferenceNumber = "Test Reference";
			AssertEquals("Test Reference", provider.Id);
		}

		public void TestType()
		{
			var supportingDocument = Factory.New<SupportingDocument>();
			supportingDocument.CSI_Code = "ACB";
			var provider = IM413AndIM415ProducedDocumentsWritingOffProvider.New(supportingDocument);
			AssertEquals("ACB", provider.Type);

			supportingDocument.CSI_Code = "XYZ";
			AssertEquals("XYZ", provider.Type);
		}

		public void TestIssuingAuthorityNameSubmitter()
		{
			var supportingDocument = Factory.New<SupportingDocument>();
			supportingDocument.CSI_AdditionalDescription = "Description";
			var provider = IM413AndIM415ProducedDocumentsWritingOffProvider.New(supportingDocument);
			AssertEquals("Description", provider.IssuingAuthorityNameSubmitter);

			supportingDocument.CSI_AdditionalDescription = "IssuingAuthorityNameSubmitter";
			AssertEquals("IssuingAuthorityNameSubmitter", provider.IssuingAuthorityNameSubmitter);
		}

		public void TestIssuingAuthorityNameRoleCode()
		{
			var supportingDocument = Factory.New<SupportingDocument>();
			supportingDocument.CSI_ReferenceNumber2 = "REF2";
			var provider = IM413AndIM415ProducedDocumentsWritingOffProvider.New(supportingDocument);
			AssertEquals("REF2", provider.IssuingAuthorityNameRoleCode);

			supportingDocument.CSI_ReferenceNumber2 = "Issuing Authority Name Role Code";
			AssertEquals("Issuing Authority Name Role Code", provider.IssuingAuthorityNameRoleCode);
		}

		public void TestDateOfValidity()
		{
			var supportingDocument = Factory.New<SupportingDocument>();
			var provider = IM413AndIM415ProducedDocumentsWritingOffProvider.New(supportingDocument);
			AssertNull(provider.DateOfValidity);

			supportingDocument.CSI_DateOfExpiry = new CargoWise.Types.ZDateTime(2021, 12, 31);
			AssertEquals(new System.DateTime(2021, 12, 31), provider.DateOfValidity);

			supportingDocument.CSI_DateOfExpiry = new CargoWise.Types.ZDateTime(2024, 4, 9);
			AssertEquals(new System.DateTime(2024, 4, 9), provider.DateOfValidity);
		}

		public void TestMeasurementUnit()
		{
			var supportingDocument = Factory.New<SupportingDocument>();
			supportingDocument.CSI_UnitOfQuantity = "MEA";
			var provider = IM413AndIM415ProducedDocumentsWritingOffProvider.New(supportingDocument);
			AssertEquals("MEA", provider.MeasurementUnit);

			supportingDocument.CSI_UnitOfQuantity = "UOQ";
			AssertEquals("UOQ", provider.MeasurementUnit);
		}

		public void TestQuantity()
		{
			var supportingDocument = Factory.New<SupportingDocument>();
			supportingDocument.CSI_Quantity = 1500;
			var provider = IM413AndIM415ProducedDocumentsWritingOffProvider.New(supportingDocument);
			AssertEquals(1500m, provider.Quantity);

			supportingDocument.CSI_Quantity = 1200.36;
			AssertEquals(1200.36m, provider.Quantity);
		}

		public void TestCurrency()
		{
			var supportingDocument = Factory.New<SupportingDocument>();
			supportingDocument.CSI_RX_NKCurrency = "EUR";
			var provider = IM413AndIM415ProducedDocumentsWritingOffProvider.New(supportingDocument);
			AssertEquals("EUR", provider.Currency);

			supportingDocument.CSI_RX_NKCurrency = "GBP";
			AssertEquals("GBP", provider.Currency);
		}

		public void TestAmount()
		{
			var supportingDocument = Factory.New<SupportingDocument>();
			supportingDocument.CSI_Value = 2750;
			var provider = IM413AndIM415ProducedDocumentsWritingOffProvider.New(supportingDocument);
			AssertEquals(2750m, provider.Amount);

			supportingDocument.CSI_Value = 1250.45;
			AssertEquals(1250.45m, provider.Amount);
		}

		protected override IM413AndIM415ProducedDocumentsWritingOffProvider GetProvider() => IM413AndIM415ProducedDocumentsWritingOffProvider.New(Factory.New<SupportingDocument>());
	}
}
