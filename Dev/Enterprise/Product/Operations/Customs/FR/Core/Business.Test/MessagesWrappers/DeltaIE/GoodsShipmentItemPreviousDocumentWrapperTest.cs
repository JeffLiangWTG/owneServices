using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.FR.Business.Declaration;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class GoodsShipmentItemPreviousDocumentWrapperTest : DataProviderTestCase<GoodsShipmentItemPreviousDocumentWrapper>
	{
		public void TestCcQualifier()
		{
			AssertEquals("CcQualifier should equal CountryCodes.France.", CountryCodes.France, Provider.CcQualifier);
		}

		public void TestGoodsItemIdentifier()
		{
			AssertEquals("GoodsItemIdentifier should equal document.CSI_ItemNumber.", "1", Provider.GoodsItemIdentifier);
		}

		public void TestMeasurementUnitAndQualifier()
		{
			AssertEquals("MeasurementUnitAndQualifier should equal document.CSI_UnitOfQuantity.", "KG", Provider.MeasurementUnitAndQualifier);
		}

		public void TestNumberOfPackages()
		{
			AssertEquals("NumberOfPackages should equal document.CSI_PackQty.", "2", Provider.NumberOfPackages);
		}

		public void TestQuantity()
		{
			AssertEquals("Quantity should equal document.CSI_Quantity.", 3d, Provider.Quantity);
		}

		public void TestReferenceNumber()
		{
			AssertEquals("ReferenceNumber should equal document.CSI_ReferenceNumber.", "001", Provider.ReferenceNumber);
		}

		public void TestType()
		{
			AssertEquals("Type should equal document.CSI_Code.", "XX", Provider.Type);
		}

		public void TestTypeOfPackages()
		{
			AssertEquals("TypeOfPackages should equal document.CSI_PackType.", "YY", Provider.TypeOfPackages);
		}

		protected override GoodsShipmentItemPreviousDocumentWrapper GetProvider()
		{
			var document = Factory.New<PreviousDocument>();
			document.CSI_ItemNumber = 1;
			document.CSI_UnitOfQuantity = "KG";
			document.CSI_PackQty = 2;
			document.CSI_Quantity = 3d;
			document.CSI_ReferenceNumber = "001";
			document.CSI_LineNo = 4;
			document.CSI_Code = "XX";
			document.CSI_PackType = "YY";

			return GoodsShipmentItemPreviousDocumentWrapper.New(document);
		}
	}
}
