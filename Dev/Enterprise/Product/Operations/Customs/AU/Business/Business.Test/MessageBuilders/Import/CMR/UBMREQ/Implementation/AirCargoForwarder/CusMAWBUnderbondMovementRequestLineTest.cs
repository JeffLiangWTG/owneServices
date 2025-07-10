using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class CusMAWBUnderbondMovementRequestLineTest : TestCaseWithFactory
	{
		[ExpectException(typeof(ArgumentNullException))]
		public void TestConstructWithNullMAWBThrowsException()
		{
			new CusMAWBUnderbondMovementRequestLine(null);
		}

		public void TestContainerNumber()
		{
			AssertEquals("ContainerNumber", ZString.Empty, Line.ContainerNumber);
		}

		public virtual void TestHouseBillOfLading()
		{
			AssertEquals("HouseBillOfLading", ZString.Empty, Line.HouseBillOfLading);
		}

		public virtual void TestHouseBillOfLadingComesFromCM_MasterHouseBill()
		{
			MAWB.CM_MasterHouseBill = "321";
			AssertEquals("HouseBillOfLading", "321", Line.HouseAirWaybillNumber);
		}

		public void TestHouseAirWaybillNumber()
		{
			AssertEquals("HouseAirWaybillNumber", ZString.Empty, Line.HouseAirWaybillNumber);
		}

		public void TestOceanBillOfLading()
		{
			AssertEquals("OceanBillOfLading", ZString.Empty, Line.OceanBillOfLading);
		}

		public virtual void TestMasterAirWaybillNumber()
		{
			MAWB.CM_MAWB = "123";
			AssertEquals("MasterAirWaybillNumber", "123", Line.MasterAirWaybillNumber);
		}

		public void TestUniqueConsignmentReferenceNumber()
		{
			AssertEquals("UniqueConsignmentReferenceNumber", ZString.Empty, Line.UniqueConsignmentReferenceNumber);
		}

		public virtual void TestNumberOfPackages()
		{
			var hAWB1 = ((CusMAWB)MAWB).ChildBills.AddNew();
			var hAWB2 = ((CusMAWB)MAWB).ChildBills.AddNew();
			hAWB1.CS_PiecesManifested = 5;
			hAWB2.CS_PiecesManifested = 5;
			AssertEquals("NumberOfPackages", 10, Line.NumberOfPackages);
		}

		public void TestPackageType()
		{
			AssertEquals("PackageType", ZString.Empty, Line.PackageType);
		}

		public void TestImportCargoType()
		{
			AssertEquals("ImportCargoType", ZString.Empty, Line.ImportCargoType);
		}

		CusMAWBUnderbondMovementRequestLine line;
		protected virtual CusMAWBUnderbondMovementRequestLine Line => line ?? (line = new CusMAWBUnderbondMovementRequestLine(MAWB));

		CusMAWB mawb;
		protected virtual CusMAWBBase MAWB => mawb ?? (mawb = Factory.New<CusMAWB>());
	}
}
