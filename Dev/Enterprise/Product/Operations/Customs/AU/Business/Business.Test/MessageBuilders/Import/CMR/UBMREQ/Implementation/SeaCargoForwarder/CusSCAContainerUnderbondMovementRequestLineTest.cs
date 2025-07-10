using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSCAContainerUnderbondMovementRequestLineTest : TestCaseWithFactory
	{
		public void TestContainerNumber()
		{
			Container.CN_ContainerNumber = "123";
			AssertEquals("ContainerNumber", "123", Line.ContainerNumber);
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
			AssertEquals("OceanBillOfLading", ZString.Empty, Line.OceanBillOfLading);
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
			AssertEquals("ImportCargoType", CMRCargoTypes.Codes.FullContainerLoad, Line.ImportCargoType);
		}

		CusSCAContainerUnderbondMovementRequestLine line;
		CusSCAContainerUnderbondMovementRequestLine Line => line ?? (line = new CusSCAContainerUnderbondMovementRequestLine(Container));

		CusSCAOceanBill oceanBill;
		CusSCAOceanBill OceanBill => oceanBill ?? (oceanBill = Factory.New<CusSCAOceanBill>());

		CusSCAContainer container;
		CusSCAContainer Container => container ?? (container = OceanBill.Containers.AddNew());

		CusSCAPivot pivot;
		CusSCAPivot Pivot
		{
			get
			{
				if (pivot == null)
				{
					var house = OceanBill.HouseBills.AddNew();
					pivot = house.Pivot.AddNew();
					pivot.CV_CN = Container.PK;
				}
				return pivot;
			}
		}
	}
}
