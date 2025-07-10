using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	sealed class SupportingDocumentGoodsShipmentItemProviderTest : DataProviderTestCase<SupportingDocumentGoodsShipmentItemProvider>
	{
		public void TestType()
		{
			AssertEquals("Type", "Code", supportingDocumentGoodsShipmentItemProvider.Type);
		}

		public void TestReferenceNumber()
		{
			AssertEquals("Reference Number", "Description", supportingDocumentGoodsShipmentItemProvider.Reference);
		}

		public void TestMeasurementUnitAndQualifier()
		{
			AssertNull("Measurement Unit And Qualifier", supportingDocumentGoodsShipmentItemProvider.MeasurementUnitAndQualifier);
		}

		public void TestQuantity()
		{
			AssertEquals("Quantity", 0m, supportingDocumentGoodsShipmentItemProvider.Quantity);
		}

		public void TestCurrency()
		{
			AssertNull("Currency", supportingDocumentGoodsShipmentItemProvider.Currency);
		}

		public void TestAmount()
		{
			AssertEquals("Amount", 0m, supportingDocumentGoodsShipmentItemProvider.Amount);
		}

		public void TestDocumentLineItemNumber()
		{
			AssertNull("Document Line ItemNumber", supportingDocumentGoodsShipmentItemProvider.DocumentLineItemNumber);
		}

		public void TestIssuingAuthorityName()
		{
			AssertNull("Issuing Authority Name", supportingDocumentGoodsShipmentItemProvider.IssuingAuthorityName);
		}

		public void TestDateOfValidity()
		{
			AssertEquals("Date Of Validity", DateTime.MinValue, supportingDocumentGoodsShipmentItemProvider.DateOfValidity);
		}

		public void TestCcQualifier()
		{
			AssertNull("CcQualifier", supportingDocumentGoodsShipmentItemProvider.CcQualifier);
		}

		protected override void SetUp()
		{
			base.SetUp();

			supportingInfo = Factory.New<CusSupportingInfo>();
			supportingInfo.CSI_Code = "Code";
			supportingInfo.CSI_ReferenceNumber = "Description";

			supportingDocumentGoodsShipmentItemProvider = new SupportingDocumentGoodsShipmentItemProvider(supportingInfo);
		}
		CusSupportingInfo supportingInfo;
		SupportingDocumentGoodsShipmentItemProvider supportingDocumentGoodsShipmentItemProvider;

		protected sealed override SupportingDocumentGoodsShipmentItemProvider GetProvider()
		{
			return supportingDocumentGoodsShipmentItemProvider;
		}
	}
}
