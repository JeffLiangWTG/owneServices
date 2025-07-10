using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class DepotCusOutturnOutturnReportLineInformationTest : TestCaseWithFactory
	{
		public void TestGoodsDescription()
		{
			Line.C5_GoodsDescription = "foo bar";
			AssertEquals("comes from GoodsDescription", "foo bar", LineInfo.GoodsDescription);
		}

		public void TestContanerNumber()
		{
			Line.C5_ContainerNumber = "6654545";
			AssertEquals("comes from containernumber", "6654545", LineInfo.ContainerNumber);
		}

		public void TestHouseBillOfLading()
		{
			Line.C5_HouseBill = "spam";
			AssertEquals("comes from housebill", "spam", LineInfo.HouseBillOfLading);
		}

		public void TestOceanBillOfLading()
		{
			Line.C5_MasterBill = "eggs";
			AssertEquals("comes from masterbill", "eggs", LineInfo.OceanBillOfLading);
		}

		public void TestSealNumber()
		{
			Line.C5_ContainerSeal = "blah";
			AssertEquals("comes from containerseal", "blah", LineInfo.SealNumber);
		}

		public void TestSealIntactIndicator()
		{
			Line.C5_SealIntactIndicator = true;
			AssertEquals("comes from sealintactindicator", true, LineInfo.SealIntactIndicator);
		}

		public void TestVesselDischargeUnderbondIndicator()
		{
			AssertEquals("is always falso", false, LineInfo.VesselDischargeUnderbondIndicator);
		}

		public void TestUnpackIndicator()
		{
			Line.C5_CargoUnpackDate = ZDateTime.Now;
			AssertEquals("true when cargounpackdate is not empty", true, LineInfo.UnpackIndicator);
			Line.C5_CargoUnpackDate = ZDateTime.Empty;
			AssertEquals("false when cargounpackdate is empty", false, LineInfo.UnpackIndicator);
		}

		[TestDate(2005, 06, 06)]
		public void TestDateTimeOfCargoReceiptUnload()
		{
			Line.C5_CargoReceiptDate = new ZDateTime(2005, 06, 06);
			AssertEquals("comes from cargo receiptdate", new ZDateTime(2005, 06, 06).AddHours(-10), LineInfo.DateTimeOfCargoReceiptUnload);
		}

		[TestDate(2005, 02, 02)]
		public void TestDateTimeOfOutturn()
		{
			Line.C5_CargoUnpackDate = new ZDateTime(2005, 02, 02);
			AssertEquals("comes from cargo unpackdate", new ZDateTime(2005, 02, 02).AddHours(-10), LineInfo.DateTimeOfOutturn);
		}

		public void TestQuantity()
		{
			Line.C5_CargoType = Core.Constants.ContainerModes.Bulk;
			Line.C5_PackagesOutturned = 5;
			AssertEquals("comes from packages outturned when bulk", 5, LineInfo.Quantity);

			Line.C5_CargoType = "foo";
			AssertEquals("empty when not bulk", 0, LineInfo.Quantity);
		}

		public void TestQuantityUnit()
		{
			Line.C5_CargoType = Core.Constants.ContainerModes.Bulk;
			Line.C5_PackagesUnits = "FO";
			AssertEquals("comes from outerpackunits when bulk", "FO", LineInfo.QuantityUnit);

			Line.C5_CargoType = "foo";
			AssertEquals("comes from outerpackunits when not bulk", ZString.Empty, LineInfo.QuantityUnit);
		}

		public void TestImportCargoType()
		{
			Line.C5_CargoType = "BAR";
			AssertEquals("comes from cargotype", "BAR", LineInfo.ImportCargoType);
		}

		public void TestNumberOfPackages()
		{
			Line.C5_PackagesOutturned = 5;
			AssertEquals("comes from packagesoutturned", 5, LineInfo.NumberOfPackages);

			Line.C5_CargoType = Core.Constants.ContainerModes.Bulk;
			AssertEquals("blank when bulk", 0, LineInfo.NumberOfPackages);
		}

		public void TestPackageType()
		{
			Line.C5_PackagesUnits = "SP";
			AssertEquals("comes from packageunits", "SP", LineInfo.PackageType);

			Line.C5_CargoType = Core.Constants.ContainerModes.Bulk;
			AssertEquals("empty when bulk", ZString.Empty, LineInfo.PackageType);
		}

		public void TestMarksAndNumbers()
		{
			Line.C5_MarksAndNumbers = "eggs";
			AssertEquals("comes from marksandnumbers", "eggs", LineInfo.MarksAndNumbers);
		}

		DepotCusOutturnOutturnReportLineInformation lineInfo;
		DepotCusOutturnOutturnReportLineInformation LineInfo => lineInfo ?? (lineInfo = new DepotCusOutturnOutturnReportLineInformation(Line));

		DepotCusOutturn line;
		DepotCusOutturn Line
		{
			get
			{
				if (line == null)
				{
					var header = CusOutturnHeader.New(Factory);
					line = header.Outturns.AddNew();
				}
				return line;
			}
		}
	}
}
