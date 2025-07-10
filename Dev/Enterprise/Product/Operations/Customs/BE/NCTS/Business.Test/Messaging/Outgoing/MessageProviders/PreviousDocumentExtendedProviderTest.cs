using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(PreviousDocumentExtendedProvider))]
	sealed class PreviousDocumentExtendedProviderTest : DocumentProviderAbstractTest<PreviousDocumentExtendedProvider>
	{
		protected override string SubType => "PRE";

		public void TestComplementOfInformation()
		{
			info.CSI_ReferenceNumber2 = "PrevDocCOI";
			AssertEquals("PrevDocCOI", Provider.ComplementOfInformation);
		}

		public void TestGoodsItemNumber()
		{
			info.CSI_ItemNumber = 3;
			AssertEquals(3, Provider.GoodsItemNumber);
		}

		public void TestGoodsItemNumber_Null()
		{
			info.CSI_ItemNumber = 0;
			AssertNull(Provider.GoodsItemNumber);
		}

		public void TestTypeOfPackages()
		{
			info.CSI_UnitOfQuantity2 = "PAK";
			AssertEquals("PAK", Provider.TypeOfPackages);
		}

		public void TestNumberOfPackages()
		{
			info.CSI_Quantity2 = 10;
			info.CSI_UnitOfQuantity2 = "PAK";
			AssertEquals(10, Provider.NumberOfPackages);
		}

		public void TestNumberOfPackages_Null()
		{
			info.CSI_Quantity2 = 0;
			AssertNull(Provider.NumberOfPackages);
		}

		public void TestMeasurementUnitAndQualifier()
		{
			info.CSI_UnitOfQuantity = "GR";
			AssertEquals("GR", Provider.MeasurementUnitAndQualifier);
		}

		public void TestQuantity()
		{
			info.CSI_Quantity = 20;
			info.CSI_UnitOfQuantity = "GR";
			AssertEquals(20M, Provider.Quantity);
		}

		public void TestQuantity_Null()
		{
			info.CSI_Quantity = 0;
			AssertNull(Provider.Quantity);
		}

		public void TestGoodsItemIdentifier()
		{
			AssertNullOrEmpty(Provider.GoodsItemIdentifier);
		}
	}
}
