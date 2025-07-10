using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(JobComInvoiceLineLookups))]
sealed class JobComInvoiceLineLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestAccessoryStatusList()
	{
		RefDataSetupTestHelper.SetupExportAccessoryStatusCodes(Factory);
		RefDataSetupTestHelper.SetupImportAccessoryStatusCodes(Factory);

		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var accessoryStatusList = invoiceLine.Lookups.AccessoryStatusList;
		CombineAssertions("Export", () =>
		{
			AssertContainsExactElementsInExactOrder(new[] { "0", "1" }, accessoryStatusList.GetAllCodes());
			AssertSame("Cached", accessoryStatusList, invoiceLine.Lookups.AccessoryStatusList);
		});

		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		accessoryStatusList = invoiceLine.Lookups.AccessoryStatusList;
		CombineAssertions("Import", () =>
		{
			AssertContainsExactElementsInExactOrder(new[] { "1", "2" }, accessoryStatusList.GetAllCodes());
			AssertSame("Cached", accessoryStatusList, invoiceLine.Lookups.AccessoryStatusList);
		});
	}

	public void TestEndUseCodes()
	{
		var testHelper = new UniversalReferenceTestDataHelper(Factory);
		var dataGrouping = testHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.India, "IN");
		var codeType = testHelper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.INCustomsEndUseCode, "End Use", Core.Constants.CountryCodes.India);
		var code1 = testHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.India, codeType.ZZK_CodeType, "HXU001", "DESC", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		var code2 = testHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.India, codeType.ZZK_CodeType, "HXU002", "DESC", ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5));
		Factory.Save();

		var endUseCodes = invoiceLine.Lookups.EndUseCodes;
		endUseCodes.Load();
		AssertContainsExactElementsInAnyOrder(new[] { code1.ZZD_Code, code2.ZZD_Code }, endUseCodes.Select(x => x.ZZD_Code));

		var instruction = declaration.CustomsEntryInstructions.AddNew();
		instruction.CEI_DateForDuty = ZDateTime.Today.AddDays(-3);
		invoiceLine.JI_CEI = instruction.PK;
		endUseCodes = invoiceLine.Lookups.EndUseCodes;
		endUseCodes.Load();
		AssertContainsExactElementsInAnyOrder(new[] { code2.ZZD_Code }, endUseCodes.Select(x => x.ZZD_Code));
	}

	public void TestOriginStateList()
	{
		var originStateList = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.India).States.Select(x => x.RW_Code);
		CombineAssertions(() =>
		{
			Assert("Precondition: India States", originStateList.Any());
			AssertContainsExactElementsInAnyOrder(originStateList, invoiceLine.Lookups.OriginStateList.GetAllCodesZString());
		});
	}

	public void TestRewardItemList()
	{
		var rewardItemList = invoiceLine.Lookups.RewardItemList;
		CombineAssertions(() =>
		{
			AssertContainsExactElementsInAnyOrder("RewardItemList codes", new[] { "Y", "N" }, rewardItemList.GetAllCodes());
			AssertSame("RewardItemList cached", rewardItemList, invoiceLine.Lookups.RewardItemList);
		});
	}

	public void TestInvoiceUQList()
	{
		RefDataSetupTestHelper.SetupCustomsUnitOfQuantityCode(Factory);
		var invoiceLine = Factory.New<JobComInvoiceLine>();
		var invoiceUQList = invoiceLine.Lookups.InvoiceUQList;
		var actualsCodes = invoiceUQList.GetAllCodes();
		CombineAssertions(() =>
		{
			AssertEquals("Count", actualsCodes.Length, 2);
			var expectedCodes = new[] { "KGS", "PCS" };
			AssertContainsExactElementsInExactOrder(expectedCodes, actualsCodes);
			AssertSame("Cached", invoiceUQList, invoiceLine.Lookups.InvoiceUQList);
		});
	}

	[TestDate(2025, 05, 04)]
	public void TestUnitUQList()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.CustomsUQ, "IN Customs D_QTY_CODE List", Core.Constants.CountryCodes.India);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.India, RefCusCodeListTypes.Codes.CustomsUQ, "CMM", "CMM Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.India, RefCusCodeListTypes.Codes.CustomsUQ, "CM1", "CM1 Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.India, RefCusCodeListTypes.Codes.CustomsUQ, "CM2", "CM2 Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.India, RefCusCodeListTypes.Codes.CustomsUQ, "CR1", "CR1 Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Factory.Save();
		RefDataSetupTestHelper.SetupCusPack(Factory, "CMM", "CM1", 1);
		RefDataSetupTestHelper.SetupCusPack(Factory, "CMM", "CM2", 1);
		RefDataSetupTestHelper.SetupCusPack(Factory, "CMM", "CM3", 1);
		RefDataSetupTestHelper.SetupCusPack(Factory, "CR1", "CR1", 1);
		Factory.Save();
		var invoiceLine = Factory.New<JobComInvoiceLine>();
		var unitUQList1 = invoiceLine.Lookups.UnitUQList;
		AssertEquals(0, unitUQList1.Count);
		invoiceLine.JI_InvoiceUQ = "CMM";
		var unitUQList2 = invoiceLine.Lookups.UnitUQList;
		AssertEquals(3, unitUQList2.Count);
		AssertContainsExactElementsInExactOrder(new[] { "CM1", "CM2", "CMM" }, unitUQList2.GetAllCodes());
		invoiceLine.JI_InvoiceUQ = "";
		var unitUQList3 = invoiceLine.Lookups.UnitUQList;
		AssertSame("Cached", unitUQList1, unitUQList3);
		invoiceLine.JI_InvoiceUQ = "CMM";
		var unitUQList4 = invoiceLine.Lookups.UnitUQList;
		AssertSame("Cached", unitUQList2, unitUQList4);
	}

	public void TestJobWorkNotificationNoList()
	{
		AssertEquals("JobWorkNotificationNoList", 0, invoiceLine.Lookups.JobWorkNotificationNoList.Count);
	}

	public void TestOrganisationAddressList()
	{
		AssertType<OrganisationsFindBoxCollection>(invoiceLine.Lookups.OrganizationList);
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
	}
	JobDeclaration declaration;
	JobComInvoiceLine invoiceLine;
}
