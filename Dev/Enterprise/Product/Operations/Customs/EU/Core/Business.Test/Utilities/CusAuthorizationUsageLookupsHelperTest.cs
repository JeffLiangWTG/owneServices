using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing;

sealed class CusAuthorizationUsageLookupsHelperTest : TestCaseWithFactory
{
	[TestDate(2023, 8, 23)]
	public void TestGetAuthorizationUsageCodePairByCustomsCode()
	{
		CreateCusCodeMapData();

		CombineAssertions(() =>
		{
			var codeDescriptionPair = CusAuthorizationUsageLookupsHelper.GetAuthorizationUsageCodePairByCustomsCode(Factory, "C521", "ACR - Test1");
			AssertEquals("Code when the mapped cw1 code = the reference", "ACR", codeDescriptionPair.Code);
			AssertEquals("Description when the mapped cw1 code = the reference", "C521 - Test1", codeDescriptionPair.Description);

			codeDescriptionPair = CusAuthorizationUsageLookupsHelper.GetAuthorizationUsageCodePairByCustomsCode(Factory, "C521", "ACR -");
			AssertEquals("Code when the mapped cw1 code = the reference, no description", "ACR", codeDescriptionPair.Code);
			AssertEquals("Description when the mapped cw1 code = the reference, no description", "C521", codeDescriptionPair.Description);

			codeDescriptionPair = CusAuthorizationUsageLookupsHelper.GetAuthorizationUsageCodePairByCustomsCode(Factory, "C521", "- Test1");
			AssertEquals("Code when mapped cw1 code exists, no reference cw1 code, description exists", "ACE", codeDescriptionPair.Code);
			AssertEquals("Description when mapped cw1 code exists, no reference cw1 code, description exists", "C521 - Test1", codeDescriptionPair.Description);

			codeDescriptionPair = CusAuthorizationUsageLookupsHelper.GetAuthorizationUsageCodePairByCustomsCode(Factory, "C521", "-");
			AssertEquals("Code when mapped cw1 code exists, no reference cw1 code, no description", "ACE", codeDescriptionPair.Code);
			AssertEquals("Description when mapped cw1 code exists, no reference cw1 code, no description", "C521", codeDescriptionPair.Description);

			codeDescriptionPair = CusAuthorizationUsageLookupsHelper.GetAuthorizationUsageCodePairByCustomsCode(Factory, "C521", "ACRRR - Test1");
			AssertEquals("Code when mapped cw1 code, invalid reference cw1 code", "ACE", codeDescriptionPair.Code);
			AssertEquals("Description when mapped cw1 code, invalid reference cw1 code", "C521 - Test1", codeDescriptionPair.Description);

			codeDescriptionPair = CusAuthorizationUsageLookupsHelper.GetAuthorizationUsageCodePairByCustomsCode(Factory, "123", "ACRRR - Test1");
			AssertEquals("Code when no mapped cw1 code, invalid reference cw1 code", "123", codeDescriptionPair.Code);
			AssertEquals("Description when no mapped cw1 code, invalid reference cw1 code", "123 - Test1", codeDescriptionPair.Description);

			codeDescriptionPair = CusAuthorizationUsageLookupsHelper.GetAuthorizationUsageCodePairByCustomsCode(Factory, "D019", "Test2");
			AssertEquals("Code when reference cw1 code is empty and customs code exist in mapping", "D019", codeDescriptionPair.Code);
			AssertEquals("Description when reference cw1 code is empty and customs code exist in mapping", "D019 - Test2", codeDescriptionPair.Description);

			codeDescriptionPair = CusAuthorizationUsageLookupsHelper.GetAuthorizationUsageCodePairByCustomsCode(Factory, "123", "Test3");
			AssertEquals("Code when reference cw1 code is empty and customs code not exist in mapping", "123", codeDescriptionPair.Code);
			AssertEquals("Description when reference cw1 code is empty and customs code not exist in mapping", "123 - Test3", codeDescriptionPair.Description);

			codeDescriptionPair = CusAuthorizationUsageLookupsHelper.GetAuthorizationUsageCodePairByCustomsCode(Factory, "QQQ", "abc - Test3");
			AssertEquals("Code when reference cw1 code is not empty but customs code not exist in mapping", "abc", codeDescriptionPair.Code);
			AssertEquals("Description when reference cw1 code is not empty but customs code not exist in mapping", "QQQ - Test3", codeDescriptionPair.Description);

			codeDescriptionPair = CusAuthorizationUsageLookupsHelper.GetAuthorizationUsageCodePairByCustomsCode(Factory, "C521", "ACT - Test1");
			AssertEquals("Code when customs code exist in mapping and reference cw1 code is not empty but not contain in mapping", "ACE", codeDescriptionPair.Code);
			AssertEquals("Description when customs code exist in mapping and reference cw1 code is not empty but not contain in mapping", "C521 - Test1", codeDescriptionPair.Description);
		});
	}

	public void TestGetAuthorizationUsageCodePairByCustomsCode_NoRefMapData_NoMappedCw1Code()
	{
		CombineAssertions(() =>
		{
			var codeDescriptionPair = CusAuthorizationUsageLookupsHelper.GetAuthorizationUsageCodePairByCustomsCode(Factory, "C521", "ACR - Test1");
			AssertEquals("Code when reference cw1 code and description exist", "ACR", codeDescriptionPair.Code);
			AssertEquals("Description when reference cw1 code and description exist", "C521 - Test1", codeDescriptionPair.Description);

			codeDescriptionPair = CusAuthorizationUsageLookupsHelper.GetAuthorizationUsageCodePairByCustomsCode(Factory, "C521", "ACR -");
			AssertEquals("Code when reference cw1 code exists, no description", "ACR", codeDescriptionPair.Code);
			AssertEquals("Description when reference cw1 code exists, no description", "C521", codeDescriptionPair.Description);

			codeDescriptionPair = CusAuthorizationUsageLookupsHelper.GetAuthorizationUsageCodePairByCustomsCode(Factory, "C521", "- Test1");
			AssertEquals("Code when no reference cw1 code and description exists", "C521", codeDescriptionPair.Code);
			AssertEquals("Description when no reference cw1 code and description exists", "C521 - Test1", codeDescriptionPair.Description);

			codeDescriptionPair = CusAuthorizationUsageLookupsHelper.GetAuthorizationUsageCodePairByCustomsCode(Factory, "C521", "-");
			AssertEquals("Code when no reference cw1 code and no description", "C521", codeDescriptionPair.Code);
			AssertEquals("Description when no reference cw1 code and no description", "C521", codeDescriptionPair.Description);

			codeDescriptionPair = CusAuthorizationUsageLookupsHelper.GetAuthorizationUsageCodePairByCustomsCode(Factory, "C521", "Test1");
			AssertEquals("Code when no reference cw1 code and description exists", "C521", codeDescriptionPair.Code);
			AssertEquals("Description when no reference cw1 code and description exists", "C521 - Test1", codeDescriptionPair.Description);

			codeDescriptionPair = CusAuthorizationUsageLookupsHelper.GetAuthorizationUsageCodePairByCustomsCode(Factory, "123", "ACRRR - Test1");
			AssertEquals("Code when invalid reference cw1 code", "123", codeDescriptionPair.Code);
			AssertEquals("Description when invalid reference cw1 code", "123 - Test1", codeDescriptionPair.Description);
		});
	}

	[TestDate(2023, 9, 23)]
	public void TestGetAuthorizationUsageCodePairByCustomsCode_SpecialDate()
	{
		CreateCusCodeMapData();

		CombineAssertions(() =>
		{
			var codeDescriptionPair = CusAuthorizationUsageLookupsHelper.GetAuthorizationUsageCodePairByCustomsCode(Factory, "C521", "ACR - Test1");
			AssertEquals("Code when special date", "ACB", codeDescriptionPair.Code);
			AssertEquals("Description when special date", "C521 - Test1", codeDescriptionPair.Description);
		});
	}

	[TestDate(2023, 8, 23)]
	public void TestGetAuthorizationUsageCodePairByCustomsCodePair()
	{
		CreateCusCodeMapData();

		CombineAssertions(() =>
		{
			var pair = new CodeDescriptionPair(string.Empty, "ACR - Test1");
			AssertExceptionThrown<ArgumentException>("Exception expected when customs code is null", () => CusAuthorizationUsageLookupsHelper.GetAuthorizationUsageCodePairByCustomsCode(Factory, pair));

			pair = new CodeDescriptionPair("D019", string.Empty);
			AssertExceptionThrown<ArgumentException>("Exception expected when description is null", () => CusAuthorizationUsageLookupsHelper.GetAuthorizationUsageCodePairByCustomsCode(Factory, pair));

			pair = null;
			AssertExceptionThrown<ArgumentNullException>("Exception expected when pair is null", () => CusAuthorizationUsageLookupsHelper.GetAuthorizationUsageCodePairByCustomsCode(Factory, pair));
		});
	}

	[TestDate(2023, 9, 23)]
	public void TestGetAuthorizationUsageCodePairByCustomsCodePair_SpecialDate()
	{
		CreateCusCodeMapData();

		CombineAssertions(() =>
		{
			var pair = new CodeDescriptionPair("C521", "ACB - Test1");
			var codeDescriptionPair = CusAuthorizationUsageLookupsHelper.GetAuthorizationUsageCodePairByCustomsCode(Factory, pair);
			AssertEquals("Code when special date", "ACB", codeDescriptionPair.Code);
			AssertEquals("Description when special date", "C521 - Test1", codeDescriptionPair.Description);
		});
	}

	[TestDate(2023, 8, 23)]
	public void TestGetAuthorizationUsageCodePairByCW1Code()
	{
		CreateCusCodeMapData();

		CombineAssertions(() =>
		{
			var codeDescriptionPair = CusAuthorizationUsageLookupsHelper.GetAuthorizationUsageCodePairByCW1Code(Factory, "ACR", "Test1");
			AssertEquals("Code when cw1 code has more than one customs code in mapping", "ACR", codeDescriptionPair.Code);
			AssertEquals("Description when cw1 code has more than one customs code in mapping", "C521 - Test1", codeDescriptionPair.Description);

			codeDescriptionPair = CusAuthorizationUsageLookupsHelper.GetAuthorizationUsageCodePairByCW1Code(Factory, "D019", "Test2");
			AssertEquals("Code when cw1 code has just one customs code in mapping", "D019", codeDescriptionPair.Code);
			AssertEquals("Description when cw1 code has just one customs code in mapping", "D019 - Test2", codeDescriptionPair.Description);

			codeDescriptionPair = CusAuthorizationUsageLookupsHelper.GetAuthorizationUsageCodePairByCW1Code(Factory, "123", "Test3");
			AssertEquals("Code when cw1 code is not exist in mapping", "123", codeDescriptionPair.Code);
			AssertEquals("Description when cw1 code is not exist in mapping", "Test3", codeDescriptionPair.Description);
		});
	}

	public void TestGetAuthorizationUsageCodePairByCW1Code_WhenCW1CodeOrDescriptionIsEmpty()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentException>("Exception expected when code is null", () => CusAuthorizationUsageLookupsHelper.GetAuthorizationUsageCodePairByCW1Code(Factory, ZString.Empty, "Test"));
			AssertExceptionThrown<ArgumentException>("Exception expected when description is null", () => CusAuthorizationUsageLookupsHelper.GetAuthorizationUsageCodePairByCW1Code(Factory, "ACR", ZString.Empty));
		});
	}

	[TestDate(2023, 9, 23)]
	public void TestGetAuthorizationUsageCodePairByCw1Code_SpecialDate()
	{
		CreateCusCodeMapData();

		CombineAssertions(() =>
		{
			var codeDescriptionPair = CusAuthorizationUsageLookupsHelper.GetAuthorizationUsageCodePairByCW1Code(Factory, "ACB", "Test1");
			AssertEquals("Code when special date", "ACB", codeDescriptionPair.Code);
			AssertEquals("Description when special date", "C521 - Test1", codeDescriptionPair.Description);
		});
	}

	public void TestCreateAuthorizationUsageCodePair()
	{
		var description = CusAuthorizationUsageLookupsHelper.CreateAuthorizationUsageCodePair("ARA", "C521", "Test");

		CombineAssertions(() =>
		{
			AssertEquals("Code", "ARA", description.Code);
			AssertEquals("Description", "C521 - Test", description.Description);
		});
	}

	public void TestCreateAuthorizationUsageCodePair_CustomsCodeIsEmpty()
	{
		var description = CusAuthorizationUsageLookupsHelper.CreateAuthorizationUsageCodePair("ARA", string.Empty, "Test");

		CombineAssertions(() =>
		{
			AssertEquals("Code", "ARA", description.Code);
			AssertEquals("Description", "Test", description.Description);
		});
	}

	public void TestCreateAuthorizationUsageCodePair_DescriptionIsEmpty()
	{
		var description = CusAuthorizationUsageLookupsHelper.CreateAuthorizationUsageCodePair("ARA", "C521", string.Empty);

		CombineAssertions(() =>
		{
			AssertEquals("Code", "ARA", description.Code);
			AssertEquals("Description", "C521", description.Description);
		});
	}

	public void TestCreateAuthorizationUsageCodePair_CustomsCodeIsEmpty_DescriptionIsEmpty()
	{
		var description = CusAuthorizationUsageLookupsHelper.CreateAuthorizationUsageCodePair("ARA", string.Empty, string.Empty);

		CombineAssertions(() =>
		{
			AssertEquals("Code", "ARA", description.Code);
			AssertEquals("Description", string.Empty, description.Description);
		});
	}

	void CreateCusCodeMapData()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var eunau = EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU;
		helper.CreateCusMapType(eunau, MapDirectionList.Codes.OUT, "EUNAU", true);
		helper.CreateCusMap(eunau, "ACR", "C521", new ZDateTime(2023, 8, 20), new ZDateTime(2023, 8, 30), Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		helper.CreateCusMap(eunau, "ACE", "C521", new ZDateTime(2023, 8, 20), new ZDateTime(2023, 8, 30), Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		helper.CreateCusMap(eunau, "D019", "D019", new ZDateTime(2023, 8, 20), new ZDateTime(2023, 8, 30), Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		helper.CreateCusMap(eunau, "ACB", "C521", new ZDateTime(2023, 9, 20), new ZDateTime(2023, 9, 30), Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		Factory.Save();
	}
}
