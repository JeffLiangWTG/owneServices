using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSeaManOBLDetailUnderbondMovementRequestLineTest : TestCaseWithFactory
	{
		public void TestContainerNumber()
		{
			Detail.BD_ContainerNumber = "123";
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
			Detail.BD_NoOfPacks = 10;
			AssertEquals("NumberOfPackages", 10, Line.NumberOfPackages);
		}

		public void TestPackageType()
		{
			Detail.BD_PackType = "BOX";
			AssertEquals("PackageType", "BOX", Line.PackageType);
		}

		public void TestImportCargoType()
		{
			Detail.BD_LineCargoType = Core.Constants.ContainerModes.FCL;
			AssertEquals("ImportCargoType", CMRCargoTypes.Codes.FullContainerLoad, Line.ImportCargoType);
		}

		public void TestBreakBulk()
		{
			Detail.BD_PackType = CMRCargoTypes.Codes.BreakBulk;
			Detail.BD_ContainerNumber = "BREAK BULK";
			Detail.Header.BO_OceanBill = "OCEANBILL";

			AssertEquals("Container Number Line should be empty", ZString.Empty, Line.ContainerNumber);
			AssertEquals("Ocean Bill must be sent for Break Bulk", "OCEANBILL", Line.OceanBillOfLading);
		}

		CusSeaManOBLDetailUnderbondMovementRequestLine line;
		CusSeaManOBLDetailUnderbondMovementRequestLine Line => line ?? (line = new CusSeaManOBLDetailUnderbondMovementRequestLine(Detail));

		CusSeaManOBLDetail detail;
		CusSeaManOBLDetail Detail
		{
			get
			{
				if (detail == null)
				{
					var transportHeader = Factory.New<CusSeaManTranHead>();
					detail = transportHeader.OceanBills.AddNew().Details.AddNew();
				}
				return detail;
			}
		}
	}
}
