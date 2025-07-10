using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	class CustomsOfficeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCY_CodeList()
		{
			AssertType<CustomsOfficeTypeList>(Factory.New<CustomsOffice>().Lookups.CY_CodeList);
		}

		[TestDate(2018, 8, 9)]
		public void TestCustomsOfficeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("CUSOF", "Customs Office Code");
			helper.CreateNewOrGetExistingCusCodeType("CIQOF", "CIQ Office Code");
			Factory.Save();
			helper.CreateNewOrGetExistingCusCodeList("CN", "CUSOF", "OF1", "CIQ Office 1", new ZDateTime(2018, 8, 1), new ZDateTime(2018, 8, 8));
			helper.CreateNewOrGetExistingCusCodeList("CN", "CUSOF", "OF2", "CIQ Office 2", new ZDateTime(2018, 8, 1), new ZDateTime(2018, 8, 31));
			helper.CreateNewOrGetExistingCusCodeList("CN", "CIQOF", "OFX", "CIQ Office 3", new ZDateTime(2018, 8, 1), new ZDateTime(2018, 8, 31));
			Factory.Save();
			var testItem = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => Factory.Save());
			testItem.JobDeclaration.JE_ExportDate = ZDateTime.Today;
			var testList = testItem.JobDeclaration.CustomsOffices.AddNew().Lookups.CustomsOfficeList;
			testList.Load();
			AssertEquals(1, testList.Count);
			var testCode = testList.Cast<ZZRefCusCodeListCombined>().FirstOrDefault();
			AssertEquals("CUSOF", testCode.ZZD_CodeType);
			AssertEquals("OF2", testCode.ZZD_Code);
			AssertEquals("CIQ Office 2", testCode.ZZD_Description);
		}
	}
}
