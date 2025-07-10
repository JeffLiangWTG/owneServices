using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	public class CusCNEntryInstructionLookupsTest : BusinessObjectLookupsTestCase
	{
		[TestDate(2021, 9, 9)]
		public void TestGetTransitionSiteList()
		{
			var countryCode = Core.Constants.CountryCodes.China;
			var codeDateMin = ZDateTime.MinSmallDateTimeValue;
			var codeDateMax = ZDateTime.MaxSmallDateTime;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var code_DSSSF_Office42_NoCodeSuffix_1 = helper.CreateNewOrGetExistingCusCodeList(countryCode, "DSSSF", "CNWEH42S202_01", "威海海纳食品有限公司进口水产品存储冷库", codeDateMin, codeDateMax);
			helper.CreateCusCodeListAttribute(code_DSSSF_Office42_NoCodeSuffix_1.PK, "CustomsOffice", "42");
			var code_DSSSF_Office42_NoCodeSuffix_2 = helper.CreateNewOrGetExistingCusCodeList(countryCode, "DSSSF", "CNSHD42S101_21", "荣成泰广进出口有限公司", codeDateMin, codeDateMax);
			helper.CreateCusCodeListAttribute(code_DSSSF_Office42_NoCodeSuffix_2.PK, "CustomsOffice", "42");

			var code_DuplicatedCode_1 = helper.CreateNewOrGetExistingCusCodeList(countryCode, "DSSPS", "CNCDU790046", "Site_CNCDU790046", codeDateMin, codeDateMax);
			helper.CreateCusCodeListAttribute(code_DuplicatedCode_1.PK, "CustomsOffice", "99");
			var code_DuplicatedCode_2 = helper.CreateNewOrGetExistingCusCodeList(countryCode, "DSSSF", "CNCDU790046", "Site_CNCDU790046_2", codeDateMin, codeDateMax);
			helper.CreateCusCodeListAttribute(code_DuplicatedCode_2.PK, "CustomsOffice", "99");
			var code_DuplicatedCode_WithCodeSuffix = helper.CreateNewOrGetExistingCusCodeList(countryCode, "DSSMT", "CNCDU790046", "Site_CNCDU790046_WithSuffix", codeDateMin, codeDateMax);
			helper.CreateCusCodeListAttribute(code_DuplicatedCode_WithCodeSuffix.PK, "CustomsOffice", "99");
			helper.CreateCusCodeListAttribute(code_DuplicatedCode_WithCodeSuffix.PK, "CodeSuffix", "_爱");

			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			declaration.JE_OfficeOfEntryExit = "4299";
			var testList42 = instruction.AddInfoChildLookups.TransitionSiteList;
			AssertEquals("2 items for CustomsOffice 42", 2, testList42.Count);
			AssertEquals("code_DSSSF_Office42_NoCodeSuffix_1, code & description", "威海海纳食品有限公司进口水产品存储冷库", testList42.GetDescriptionFromCode("CNWEH42S202_01"));
			AssertEquals("code_DSSSF_Office42_NoCodeSuffix_2, code & description", "荣成泰广进出口有限公司", testList42.GetDescriptionFromCode("CNSHD42S101_21"));

			declaration.JE_OfficeOfEntryExit = "9999";
			var testList99 = instruction.AddInfoChildLookups.TransitionSiteList;
			AssertEquals("2 items for CustomsOffice 99", 2, testList99.Count);
			AssertEquals("code_DuplicatedCode_1&2, code & description", "Site_CNCDU790046/Site_CNCDU790046_2", testList99.GetDescriptionFromCode("CNCDU790046"));
			AssertEquals("code_DuplicatedCode_WithCodeSuffix, code & description", "Site_CNCDU790046_WithSuffix", testList99.GetDescriptionFromCode("CNCDU790046_爱"));

			var anotherDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var anotherInstruction = anotherDeclaration.CustomsEntryInstructions.AddNew();
			anotherDeclaration.JE_OfficeOfEntryExit = "4288";
			AssertSame("TransitionSiteList should have been chached for same office 42", testList42, anotherInstruction.AddInfoChildLookups.TransitionSiteList);
		}
	}
}
