using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSCAPivotUnderbondMovementRequestLineTest : TestCaseWithFactory
	{
		[ExpectNoExceptions()]
		public void TestContainerNumberNullReference()
		{
			Pivot.CV_CN = ZGuid.Empty;
			AssertEquals("Container number empty when container null", string.Empty, Line.ContainerNumber);
		}

		public void TestContainerNumber()
		{
			Container.CN_ContainerNumber = "123";
			AssertEquals("ContainerNumber", "123", Line.ContainerNumber);
		}

		public void TestHouseBillOfLading()
		{
			Pivot.HouseBill.CA_HouseBill = "54321";
			AssertEquals("HouseBillOfLading", "54321", Line.HouseBillOfLading);
		}

		public void TestHouseBillOfLadingForMultiOceanUnpack()
		{
			OceanBill.CB_MultiOBLUnpack = true;
			Pivot.HouseBill.CA_HouseBill = "54321";
			AssertEquals("HouseBillOfLading", string.Empty, Line.HouseBillOfLading);
		}

		[ExpectNoExceptions()]
		public void TestHouseBillOfLadingNullReference()
		{
			OceanBill.CB_MultiOBLUnpack = false;
			Pivot.CV_CA = ZGuid.Empty;
			AssertEquals("HouseBill Number when oceanbill not multiunpack and housebill null", string.Empty, Line.HouseBillOfLading);
		}

		public void TestHouseAirWaybillNumber()
		{
			AssertEquals("HouseAirWaybillNumber", string.Empty, Line.HouseAirWaybillNumber);
		}

		public void TestOceanBillOfLading()
		{
			OceanBill.CB_OceanBill = "12345";
			AssertEquals("OceanBillOfLading", "12345", Line.OceanBillOfLading);
		}

		public void TestOceanBillOfLadingForMultiOceanUnpack()
		{
			OceanBill.CB_MultiOBLUnpack = true;
			OceanBill.CB_OceanBill = "12345";
			Pivot.HouseBill.CA_HouseBill = "54321";
			AssertEquals("OceanBillOfLading", "54321", Line.OceanBillOfLading);
		}

		[ExpectNoExceptions()]
		public void TestOceanBillOfLadingNullReference()
		{
			OceanBill.CB_MultiOBLUnpack = true;
			Pivot.CV_CA = ZGuid.Empty;
			AssertEquals("OceanBill Number when oceanbill multiunpack and housebill null", string.Empty, Line.OceanBillOfLading);
		}

		public void TestMasterAirWaybillNumber()
		{
			AssertEquals("MasterAirWaybillNumber", string.Empty, Line.MasterAirWaybillNumber);
		}

		public void TestUniqueConsignmentReferenceNumber()
		{
			AssertEquals("UniqueConsignmentReferenceNumber", string.Empty, Line.UniqueConsignmentReferenceNumber);
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
			Pivot.Container.CN_ContainerMode = Core.Constants.ContainerModes.LCL;
			AssertEquals("ImportCargoType", Core.Constants.ContainerModes.LCL, Line.ImportCargoType);
		}

		[ExpectNoExceptions()]
		public void TestImportCargoTypeNullReference()
		{
			Container.CN_ContainerMode = "FAP";
			Pivot.CV_CN = ZGuid.Empty;
			AssertEquals("Import Cargo Type when container is null", string.Empty, Line.OceanBillOfLading);
		}

		public void TestBreakBulkUnderbondRequest()
		{
			Container.CN_ContainerMode = CMRImportCargoTypes.Codes.BreakBulk;
			AssertEquals("Container", string.Empty, Line.ContainerNumber);
			Container.CN_ContainerMode = CMRImportCargoTypes.Codes.Bulk;
			AssertEquals("Container", string.Empty, Line.ContainerNumber);
		}

		CusSCAPivotUnderbondMovementRequestLine line;
		CusSCAPivotUnderbondMovementRequestLine Line => line ?? (line = new CusSCAPivotUnderbondMovementRequestLine(Pivot));

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
