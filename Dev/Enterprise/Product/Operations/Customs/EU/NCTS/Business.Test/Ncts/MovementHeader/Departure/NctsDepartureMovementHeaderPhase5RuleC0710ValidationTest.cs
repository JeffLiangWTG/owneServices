using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.Business.Testing;

sealed class NctsDepartureMovementHeaderPhase5RuleC0710ValidationTest : TestCaseWithFactory
{
	public void TestCheckGoodsLocationDescription_C0710()
	{
		using var deciderTestContext = new MovementHeaderValidationDeciderTestContext<INctsDepartureMovementHeaderPhase5ValidationDecider>(Factory);
		deciderTestContext.ClearCachedValidationDecider(departureMovement);
		var propertyInfo = departureMovement.GoodsLocationDescriptionInfo;
		var ruleNotActiveTestCases = new Action[] { TestCase1, TestCase2, TestCase3 };

		deciderTestContext.DisableRule(c => c.IsRuleC0710Active);

		foreach (var testCase in ruleNotActiveTestCases)
		{
			testCase.Invoke();
			AssertNoMessageErrorContaining(propertyInfo, ExpectedMessageError);
		}

		deciderTestContext.EnableRule(c => c.IsRuleC0710Active);

		CombineAssertions(() =>
		{
			TestCase1();
			AssertHasMessageErrorContaining(propertyInfo, ExpectedMessageError);

			testContext.SetNctsTransitionPeriod(true, RefDataGroupingCodes.EuropeanUnionEUN);
			departureMovement.Validation.ValidateGoodsLocationDescription();
			AssertNoMessageErrorContaining(propertyInfo, ExpectedMessageError);
			testContext.SetNctsTransitionPeriod(false, RefDataGroupingCodes.EuropeanUnionEUN);

			departureMovement.BM_AdditionalDeclarationType = "D";
			departureMovement.Validation.ValidateGoodsLocationDescription();
			AssertNoMessageErrorContaining(propertyInfo, ExpectedMessageError);

			TestCase2();
			AssertHasMessageErrorContaining(propertyInfo, ExpectedMessageError);

			var departureOffice = departureMovement.DepartureCustomsOffice as NctsEuOfficeCode;

			departureOffice.CY_Data = "";
			departureMovement.Validation.ValidateGoodsLocationDescription();
			AssertHasMessageErrorContaining(propertyInfo, ExpectedMessageError);

			TestCase3();
			AssertHasMessageErrorContaining(propertyInfo, ExpectedMessageError);

			departureOffice.CY_Data = "AT123456";
			departureMovement.Validation.ValidateGoodsLocationDescription();
			AssertNoMessageErrorContaining(propertyInfo, ExpectedMessageError);
		});

		void TestCase1()
		{
			departureMovement.BM_AdditionalDeclarationType = "";
			departureMovement.Validation.ValidateGoodsLocationDescription();
		}

		void TestCase2()
		{
			departureMovement.BM_AdditionalDeclarationType = "E";
			departureMovement.Validation.ValidateGoodsLocationDescription();
		}

		void TestCase3()
		{
			var departureOffice = departureMovement.DepartureCustomsOffice as NctsEuOfficeCode;
			departureOffice.CY_Data = "DE123456";
			departureMovement.Validation.ValidateGoodsLocationDescription();
		}
	}

	protected override void SetUp()
	{
		base.SetUp();

		SetUpOfficeList();
		Factory.Save();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		departureMovement = nctsHeader.MovementHeader;

		NCTSTestHelper.CreateCustomsOfficeForTest(departureMovement, OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "AT123456", ZDateTime.Empty, clearOffices: true);
		NCTSTestHelper.CreateCustomsOfficeForTest(departureMovement, OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "DE123456", ZDateTime.Empty, clearOffices: true);

		testContext = departureMovement.CreateDeparturePhase5ValidationTestContext();
	}

	protected override void TearDown()
	{
		base.TearDown();
		testContext?.Dispose();
	}

	MovementHeaderValidationDeciderTestContext<INctsDepartureMovementHeaderPhase5ValidationDecider> testContext;

	void SetUpOfficeList()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var startDate = ZDate.Today.AddMonths(-1);
		var endDate = ZDate.Today.AddMonths(1);

		var tradeGroup = helper.LoadOrCreateTradeGroup("EUN", "EUC", startDate, endDate);
		helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.France, startDate, endDate);
		helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.UnitedKingdom, startDate, endDate);

		var tradeGroupCUAM = helper.CreateTradeGroup("EUN", "EUCTP", startDate, endDate);
		helper.AddCountry(tradeGroupCUAM, Core.Constants.CountryCodes.UnitedKingdom, startDate, endDate);

		var tradeGroupEUSEC = helper.CreateTradeGroup("EUN", "EUSEC", startDate, endDate);
		helper.AddCountry(tradeGroupEUSEC, Core.Constants.CountryCodes.Austria, startDate, endDate);
	}

	NctsHeader nctsHeader;
	NctsDepartureMovementHeader departureMovement;

	const string ExpectedMessageError = "[C0710] You have not entered a Location of Goods";
}
