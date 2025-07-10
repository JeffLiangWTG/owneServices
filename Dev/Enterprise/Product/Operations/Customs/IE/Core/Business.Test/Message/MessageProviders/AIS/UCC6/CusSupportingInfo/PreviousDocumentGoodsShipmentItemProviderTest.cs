using System;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class PreviousDocumentGoodsShipmentItemProviderTest : DataProviderTestCase<PreviousDocumentGoodsShipmentItemProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new PreviousDocumentGoodsShipmentItemProvider(null));
		}

		public void TestProviderInterface()
		{
			Assert(Provider is IPreviousDocumentGoodsShipmentItem);
		}

		public void TestType()
		{
			AssertEquals("123", Provider.Type);
		}

		public void TestReference()
		{
			AssertEquals("REFNO1", Provider.Reference);
		}

		public void TestDateOfAcceptance()
		{
			AssertEquals("CSI_DateOfIssue", ZDateTime.BrettsBirthday, Provider.DateOfAcceptance);
		}

		public void TestCcQualifier()
		{
			// node should not be populated
			AssertEquals(null, Provider.CcQualifier);
		}

		public void TestGoodsItemIdentifier()
		{
			AssertEquals("1", Provider.GoodsItemIdentifier);
		}

		public void TestMeasurementUnitAndQualifier()
		{
			AssertEquals("BOX", Provider.MeasurementUnitAndQualifier);
		}

		public void TestNumberOfPackages()
		{
			AssertEquals("5", Provider.NumberOfPackages);
		}

		public void TestQuantity()
		{
			AssertEquals(3m, Provider.Quantity);
		}

		public void TestTypeOfPackages()
		{
			AssertEquals("PAK", Provider.TypeOfPackages);
		}

		protected override PreviousDocumentGoodsShipmentItemProvider GetProvider() => new PreviousDocumentGoodsShipmentItemProvider(previousDocument);

		protected override void SetUp()
		{
			previousDocument = Factory.New<PreviousDocument>();
			previousDocument.CSI_Code = "123";
			previousDocument.CSI_ReferenceNumber = "REFNO1";
			previousDocument.CSI_DateOfIssue = ZDateTime.BrettsBirthday;
			previousDocument.CSI_UnitOfQuantity2 = "PAK";
			previousDocument.CSI_ItemNumber = 1;
			previousDocument.CSI_UnitOfQuantity = "BOX";
			previousDocument.CSI_Quantity2 = 5;
			previousDocument.CSI_Quantity = 3;
		}
		PreviousDocument previousDocument;
	}
}
