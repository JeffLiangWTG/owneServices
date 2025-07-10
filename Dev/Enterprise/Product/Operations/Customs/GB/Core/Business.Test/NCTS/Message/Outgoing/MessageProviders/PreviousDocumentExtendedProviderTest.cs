using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	[TestedType(typeof(PreviousDocumentExtendedProvider))]
	class PreviousDocumentExtendedProviderTest : DocumentProviderAbstractTest<PreviousDocumentExtendedProvider>
	{
		public void TestComplementOfInformation()
		{
			info.CSI_ReferenceNumber2 = "PrevDocCOI";
			AssertEquals("PrevDocCOI", Provider.ComplementOfInformation);
		}

		public void TestGoodsItemNumber()
		{
			info.CSI_ItemNumber = 0;
			AssertEquals(null, Provider.GoodsItemNumber);

			info.CSI_ItemNumber = 3;
			AssertEquals(3, Provider.GoodsItemNumber);
		}

		public void TestTypeOfPackages()
		{
			info.CSI_UnitOfQuantity2 = "PAK";
			AssertEquals(null, Provider.TypeOfPackages);

			SetupValidPackageData();
			AssertEquals("PAK", Provider.TypeOfPackages);
		}

		public void TestNumberOfPackages()
		{
			info.CSI_Quantity2 = 10;
			AssertEquals(null, Provider.NumberOfPackages);

			SetupValidPackageData();
			AssertEquals(10, Provider.NumberOfPackages);
		}

		public void TestMeasurementUnitAndQualifier()
		{
			info.CSI_UnitOfQuantity = "GR";
			AssertEquals(null, Provider.MeasurementUnitAndQualifier);

			SetupValidPackageData();
			AssertEquals("GR", Provider.MeasurementUnitAndQualifier);
		}

		public void TestQuantity()
		{
			info.CSI_Quantity = 20;
			AssertEquals(null, Provider.Quantity);

			SetupValidPackageData();
			AssertEquals(20m, Provider.Quantity);
		}

		void SetupValidPackageData()
		{
			info.CSI_UnitOfQuantity2 = "PAK";
			info.CSI_Quantity2 = 10;
		}

		protected override string SubType => "PRE";
	}
}
