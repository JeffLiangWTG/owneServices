using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class TCPGAHeaderAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestProgramCodesList()
		{
			AssertEquals(typeof(TCPGADepartmentCodes), header.AddInfoLookups.ProgramCodesList.GetType());
		}

		public void TestSubProgramCodesList()
		{
			AssertEquals(typeof(TCPGAVehicleProgramCodes), header.AddInfoLookups.SubProgramCodesList.GetType());
		}

		public void TestProductClassList()
		{
			var list1 = new[]
			{
				TCProductCategories.Codes.TC01,
				TCProductCategories.Codes.TC02,
				TCProductCategories.Codes.TC03
			};

			var list2 = new TCTireTypes().GetAllCodes();

			header.CA_TPRProgramInd = YesNoList.Codes.Yes;
			header.CA_VPRProgramInd = YesNoList.Codes.No;
			AssertContainsExactElementsInAnyOrder(header.AddInfoLookups.ProductClassList.GetAllCodes(), list1);

			header.CA_TPRProgramInd = YesNoList.Codes.No;
			header.CA_VPRProgramInd = YesNoList.Codes.Yes;
			AssertContainsExactElementsInAnyOrder(header.AddInfoLookups.ProductClassList.GetAllCodes(), list2);

			header.CA_TPRProgramInd = YesNoList.Codes.Yes;
			header.CA_VPRProgramInd = YesNoList.Codes.Yes;
			AssertContainsExactElementsInAnyOrder(header.AddInfoLookups.ProductClassList.GetAllCodes(), list1.Union(list2));
		}

		public void TestProductTypeList()
		{
			AssertContainsExactElementsInAnyOrder(header.AddInfoLookups.ProductTypeList.GetAllCodes(), new[] { TCProductCategories.Codes.TC04, TCProductCategories.Codes.TC05 });
		}

		public void ProductSizeList()
		{
			var expectedList = new[]
			{
				TCProductCategories.Codes.TC06,
				TCProductCategories.Codes.TC07,
				TCProductCategories.Codes.TC08,
				TCProductCategories.Codes.TC09,
				TCProductCategories.Codes.TC10,
				TCProductCategories.Codes.TC11,
				TCProductCategories.Codes.TC12,
				TCProductCategories.Codes.TC13,
				TCProductCategories.Codes.TC14,
				TCProductCategories.Codes.TC15
			};

			AssertContainsExactElementsInAnyOrder(header.AddInfoLookups.ProductSizeList.GetAllCodes(), expectedList);
		}

		public void TitleStatusList()
		{
			var expectedList = new[]
			{
				TCProductCategories.Codes.TC14,
				TCProductCategories.Codes.TC15,
				TCProductCategories.Codes.TC16,
				TCProductCategories.Codes.TC17
			};

			AssertContainsExactElementsInAnyOrder(header.AddInfoLookups.TitleStatusList.GetAllCodes(), expectedList);
		}

		public void TestVehicleConditionList()
		{
			AssertEquals(typeof(TCVehicleConditions), header.AddInfoLookups.VehicleConditionList.GetType());
		}

		public void TestCriteriaConformanceList()
		{
			var list1 = new[]
			{
				TCComplicanceStatements.Codes.TC03
			};

			var list2 = new[]
			{
				TCComplicanceStatements.Codes.TC03,
				TCComplicanceStatements.Codes.TC05
			};

			header.CA_TPRProgramInd = YesNoList.Codes.Yes;
			header.CA_VPRProgramInd = YesNoList.Codes.No;
			AssertContainsExactElementsInAnyOrder(header.AddInfoLookups.CriteriaConformanceList.GetAllCodes(), list1);

			header.CA_TPRProgramInd = YesNoList.Codes.No;
			header.CA_VPRProgramInd = YesNoList.Codes.Yes;
			AssertContainsExactElementsInAnyOrder(header.AddInfoLookups.CriteriaConformanceList.GetAllCodes(), list2);

			header.CA_TPRProgramInd = YesNoList.Codes.Yes;
			header.CA_VPRProgramInd = YesNoList.Codes.Yes;
			AssertContainsExactElementsInAnyOrder(header.AddInfoLookups.CriteriaConformanceList.GetAllCodes(), list1.Union(list2));
		}

		[TestDate(2018, 9, 16)]
		public void TestChassisYearList()
		{
			AssertSame(Factory.New<TCPGAHeader>().AddInfoLookups.ChassisYearList, header.AddInfoLookups.ChassisYearList);
			AssertEquals("2020", header.AddInfoLookups.ChassisYearList[0].Code);
		}

		[TestDate(2018, 9, 16)]
		public void TestManufactureYearList()
		{
			AssertSame(Factory.New<TCPGAHeader>().AddInfoLookups.ManufactureYearList, header.AddInfoLookups.ManufactureYearList);
			AssertEquals("2019", header.AddInfoLookups.ManufactureYearList[0].Code);
		}

		public void TestCountryOfOriginsLookup()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_TCInd = "Y";
			var header = invoiceLine.TCPGAHeader;

			AssertEquals(typeof(RefCountryCollection), header.AddInfoLookups.CountryOfOriginsLookup.GetType());
		}

		public void TestThereIsNoExceptionWhenRequirementsParentIsNull()
		{
			AssertNoExceptionThrown(() =>
			{
				_ = header.AddInfoLookups.CountryOfOriginsLookup;
			});
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<TCPGAHeader>();
		}
		TCPGAHeader header;

		#endregion
	}
}
