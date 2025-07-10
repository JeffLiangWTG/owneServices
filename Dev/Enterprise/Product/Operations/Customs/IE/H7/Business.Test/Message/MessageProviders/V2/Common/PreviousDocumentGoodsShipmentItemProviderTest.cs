using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	sealed class PreviousDocumentGoodsShipmentItemProviderTest : DataProviderTestCase<PreviousDocumentGoodsShipmentItemProvider>
	{
		public void TestDateOfAcceptance()
		{
			AssertEquals("Date Of Acceptance", DateTime.MinValue, previousDocumentGoodsShipmentItemProvider.DateOfAcceptance);
		}

		public void TestCcQualifier()
		{
			AssertNull("CcQualifier", previousDocumentGoodsShipmentItemProvider.CcQualifier);
		}

		public void TestType()
		{
			AssertEquals("Type", "Code", previousDocumentGoodsShipmentItemProvider.Type);
		}

		public void TestReferenceNumber()
		{
			AssertEquals("Reference Number", "Description", previousDocumentGoodsShipmentItemProvider.Reference);
		}

		public void TestTypeOfPackages()
		{
			AssertNull("Type Of Packages", previousDocumentGoodsShipmentItemProvider.TypeOfPackages);
		}

		public void TestNumberOfPackages()
		{
			AssertNull("Number Of Packages", previousDocumentGoodsShipmentItemProvider.NumberOfPackages);
		}

		public void TestMeasurementUnitAndQualifier()
		{
			AssertNull("Measurement Unit And Qualifier", previousDocumentGoodsShipmentItemProvider.MeasurementUnitAndQualifier);
		}

		public void TestQuantity()
		{
			AssertEquals("Quantity", 0m, previousDocumentGoodsShipmentItemProvider.Quantity);
		}

		public void TestGoodsItemIdentifier()
		{
			AssertNull("Goods Item Identifier", previousDocumentGoodsShipmentItemProvider.GoodsItemIdentifier);
		}

		protected override void SetUp()
		{
			base.SetUp();

			supportingInfo = Factory.New<CusSupportingInfo>();
			supportingInfo.CSI_Code = "Code";
			supportingInfo.CSI_ReferenceNumber = "Description";

			previousDocumentGoodsShipmentItemProvider = new PreviousDocumentGoodsShipmentItemProvider(supportingInfo);
		}
		CusSupportingInfo supportingInfo;
		PreviousDocumentGoodsShipmentItemProvider previousDocumentGoodsShipmentItemProvider;

		protected sealed override PreviousDocumentGoodsShipmentItemProvider GetProvider()
		{
			return previousDocumentGoodsShipmentItemProvider;
		}
	}
}
