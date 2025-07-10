using System;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class SupportingDocumentGoodsShipmentItemProviderTest : DataProviderTestCase<SupportingDocumentGoodsShipmentItemProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new SupportingDocumentGoodsShipmentItemProvider(null));
		}

		public void TestProviderInterface()
		{
			Assert(Provider is ISupportingDocumentGoodsShipmentItem);
		}

		public void TestType()
		{
			AssertEquals("123", Provider.Type);
		}

		public void TestCcQualifier()
		{
			// node should not be populated
			AssertEquals(null, Provider.CcQualifier);
		}

		public void TestReference()
		{
			AssertEquals("REFNO1", Provider.Reference);
		}

		public void TestDocumentLineItemNumber()
		{
			AssertEquals("1", Provider.DocumentLineItemNumber);
		}

		public void TestIssuingAuthorityName()
		{
			AssertEquals("AUTH01", Provider.IssuingAuthorityName);
		}

		public void TestDateOfValidity()
		{
			AssertEquals(ZDateTime.BrettsBirthday, Provider.DateOfValidity);
		}

		public void TestMeasurementUnitAndQualifier()
		{
			AssertEquals("BOX", Provider.MeasurementUnitAndQualifier);
		}

		public void TestQuantity()
		{
			AssertEquals(3m, Provider.Quantity);
		}

		public void TestCurrency()
		{
			AssertEquals("CUR", Provider.Currency);
		}

		public void TestAmount()
		{
			AssertEquals(10m, Provider.Amount);
		}

		protected override SupportingDocumentGoodsShipmentItemProvider GetProvider() => new SupportingDocumentGoodsShipmentItemProvider(supportingDocument);

		protected override void SetUp()
		{
			supportingDocument = Factory.New<SupportingDocument>();
			supportingDocument.CSI_Code = "123";
			supportingDocument.CSI_ReferenceNumber = "REFNO1";
			supportingDocument.CSI_ItemNumber = 1;
			supportingDocument.CSI_AdditionalDescription = "AUTH01";
			supportingDocument.CSI_DateOfExpiry = ZDateTime.BrettsBirthday;
			supportingDocument.CSI_UnitOfQuantity = "BOX";
			supportingDocument.CSI_Quantity = 3;
			supportingDocument.CSI_RX_NKCurrency = "CUR";
			supportingDocument.CSI_Value = 10m;
		}
		SupportingDocument supportingDocument;
	}
}
