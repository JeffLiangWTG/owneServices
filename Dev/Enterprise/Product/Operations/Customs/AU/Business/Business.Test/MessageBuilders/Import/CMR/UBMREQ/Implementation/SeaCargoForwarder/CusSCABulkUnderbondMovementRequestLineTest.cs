using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSCABulkUnderbondMovementRequestLineTest : TestCaseWithFactory
	{
		public void TestContainerNumber()
		{
			AssertEquals("ContainerNumber", ZString.Empty, Line.ContainerNumber);
		}

		public void TestHouseBillOfLading()
		{
			AssertEquals("HouseBillOfLading", ZString.Empty, Line.HouseBillOfLading);
		}

		public void TestHouseAirWaybillNumber()
		{
			AssertEquals("HouseAirWaybillNumber", ZString.Empty, Line.HouseAirWaybillNumber);
		}

		public void TestOceanBillOfLading()
		{
			AssertEquals("OceanBillOfLading", "OBLBB", Line.OceanBillOfLading);
		}

		public void TestMasterAirWaybillNumber()
		{
			AssertEquals("MasterAirWaybillNumber", ZString.Empty, Line.MasterAirWaybillNumber);
		}

		public void TestUniqueConsignmentReferenceNumber()
		{
			AssertEquals("UniqueConsignmentReferenceNumber", ZString.Empty, Line.UniqueConsignmentReferenceNumber);
		}

		public void TestNumberOfPackages()
		{
			Pivot.CV_PackageCount = 10;
			AssertEquals("NumberOfPackages", 10, Line.NumberOfPackages);
		}

		public void TestPackageType()
		{
			Pivot.CV_PackageType = "BOX";
			AssertEquals("PackageType", "BOX", Line.PackageType);
		}

		public void TestImportCargoType()
		{
			AssertEquals("ImportCargoType", "B/B", Line.ImportCargoType);
		}

		CusSCABulkUnderbondMovementRequestLine line;
		CusSCABulkUnderbondMovementRequestLine Line => line ?? (line = new CusSCABulkUnderbondMovementRequestLine(Container));

		CusSCAContainer container;
		CusSCAContainer Container
		{
			get
			{
				if (container == null)
				{
					var oceanBill = Factory.New<CusSCAOceanBill>();
					oceanBill.CB_OceanBill = "OBLBB";
					container = oceanBill.Containers.AddNew();
					container.CN_ContainerMode = Core.Constants.ContainerModes.BreakBulk;
					container.CN_ContainerNumber = "BREAK BULK";
				}
				return container;
			}
		}

		CusSCAPivot pivot;
		CusSCAPivot Pivot => pivot ?? (pivot = Container.Pivots.AddNew());
	}
}
