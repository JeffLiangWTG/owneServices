using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsGuarantee))]
	sealed class NctsGuaranteeWithLapPermitRuleTest : TestCaseWithFactory
	{
		public void TestCusGuaranteeWithoutPermitHolder()
		{
			CombineAssertions("CusGuaranteeWithoutPermitHolder value depends on existing matching permit header", () =>
			{
				var guarantee = GetNewGuaranteeAndClearOldOnes(nctsHeader, "0000");
				AssertNull("When PW_BondNumber does not match any existing Permit Header, CusGuaranteeWithoutPermitHolder is null", guarantee.CusGuaranteeWithoutPermitHolder);

				guarantee = GetNewGuaranteeAndClearOldOnes(nctsHeader, "1234");
				AssertNotNull("When PW_BondNumber matches an existing Permit header, CusGuaranteeWithoutPermitHolder is valid", guarantee.CusGuaranteeWithoutPermitHolder);
				AssertEquals("When PW_BondNumber matches an existing Permit header, its number is taken", "1234", guarantee.CusGuaranteeWithoutPermitHolder.CPH_Number);
				AssertEquals("When PW_BondNumber matches an existing Permit header, its type is TRA", "TRA", guarantee.CusGuaranteeWithoutPermitHolder.CPH_Type);
			});
		}

		public void TestCusGuaranteeWithoutPermitHolder_Phase5Arrival()
		{
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;

			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_OH_PermitHolder = organization.PK;
			guaranteeHeader.CPH_RN_NKCountryCode = "LV";
			guaranteeHeader.CPH_Number = "4321";
			guaranteeHeader.CPH_StartDate = ZDate.Today.AddYears(-20);
			guaranteeHeader.CPH_EndDate = ZDate.Today.AddYears(20);
			guaranteeHeader.CPH_QtyValIndicator = Customs.Business.PermitQtyValIndicatorList.Codes.VAL;
			guaranteeHeader.CPH_Type = "TST";
			guaranteeHeader.CPH_ApplicationCode = "GUA";
			guaranteeHeader.CPH_UnitOfMeasure = "EUR";

			CombineAssertions("CusGuaranteeWithoutPermitHolder value depends on existing matching permit header", () =>
			{
				var guarantee = GetNewGuaranteeAndClearOldOnes(nctsHeader, "0000");
				AssertNull("When PW_BondNumber does not match any existing Permit Header, CusGuaranteeWithoutPermitHolder is null", guarantee.CusGuaranteeWithoutPermitHolder);

				guarantee = GetNewGuaranteeAndClearOldOnes(nctsHeader, "4321");
				AssertNotNull("When PW_BondNumber matches an existing Permit header, CusGuaranteeWithoutPermitHolder is valid", guarantee.CusGuaranteeWithoutPermitHolder);
				AssertEquals("When PW_BondNumber matches an existing Permit header, its number is taken", "4321", guarantee.CusGuaranteeWithoutPermitHolder.CPH_Number);
				AssertEquals("When PW_BondNumber matches an existing Permit header, its type is TST", "TST", guarantee.CusGuaranteeWithoutPermitHolder.CPH_Type);

				guarantee = GetNewGuaranteeAndClearOldOnes(nctsHeader, "1234");
				AssertNull("When PW_BondNumber does not match any existing Permit Header (for TST), CusGuaranteeWithoutPermitHolder is null", guarantee.CusGuaranteeWithoutPermitHolder);
			});
		}

		public void TestLapPermitRule()
		{
			var permitRule = permitHeader.CusGuaranteeRules.AddNew();
			permitRule.CPR_RuleCode = "INV";
			permitRule.CPR_ValueFrom = "XXX";

			CombineAssertions("LapPermitRule value depends on existing matching permit rule", () =>
			{
				var guarantee = GetNewGuaranteeAndClearOldOnes(nctsHeader, "1234");
				AssertNull("When permit rule is not LAP, LapPermitRule is null", guarantee.LapPermitRule);

				permitRule.CPR_RuleCode = "LAP";
				permitRule.CPR_ValueFrom = "THI";
				guarantee = GetNewGuaranteeAndClearOldOnes(nctsHeader, "1234");
				AssertNotNull("When permit rule is LAP, LapPermitRule is valid", guarantee.LapPermitRule);
				AssertEquals("When permit rule is LAP, its value is taken", "THI", guarantee.LapPermitRule.CPR_ValueFrom);
			});
		}

		public void TestHasMatchingPermitRule()
		{
			var permitRule = permitHeader.CusGuaranteeRules.AddNew();
			permitRule.CPR_RuleCode = "INV";
			permitRule.CPR_ValueFrom = "XXX";

			CombineAssertions("Only LAP permit rules can match with guarantee", () =>
			{
				var guarantee = GetNewGuaranteeAndClearOldOnes(nctsHeader, "1234");
				AssertEquals("When permit rule is not LAP, it cannot match with guarantee", false, guarantee.HasMatchingLapPermitRule);

				permitRule.CPR_RuleCode = "LAP";
				permitRule.CPR_ValueFrom = "THI";
				guarantee = GetNewGuaranteeAndClearOldOnes(nctsHeader, "1234");
				AssertEquals("When permit rule is LAP, it matches with guarantee", true, guarantee.HasMatchingLapPermitRule);
			});
		}

		public void TestSetLiabilityWithDefaultSuretyCode()
		{
			var permitRule = permitHeader.CusGuaranteeRules.AddNew();
			permitRule.CPR_RuleCode = "LAP";
			permitRule.CPR_ValueFrom = "FUL";

			CombineAssertions("Liability amounts depend on existing permit rule", () =>
			{
				var guarantee = GetNewGuaranteeAndClearOldOnes(nctsHeader, "1234");
				guarantee.PW_SuretyCode = "ZER";
				guarantee.DefaultSuretyCodeIfHasMatchLapPermitRule();
				guarantee.SetLiabilityAmount(100);
				AssertEquals("When FUL Permit Rule exists full amount is taken", 100m, guarantee.PW_BondAmount);

				permitRule.CPR_ValueFrom = "HAL";
				guarantee.PW_SuretyCode = "ZER";
				guarantee.DefaultSuretyCodeIfHasMatchLapPermitRule();
				guarantee.SetLiabilityAmount(100);
				AssertEquals("When HAL Permit Rule exists half amount is taken", 50m, guarantee.PW_BondAmount);

				permitRule.CPR_ValueFrom = "THI";
				guarantee.PW_SuretyCode = "ZER";
				guarantee.DefaultSuretyCodeIfHasMatchLapPermitRule();
				guarantee.SetLiabilityAmount(100);
				AssertEquals("When THI Permit Rule exists a third of amount is taken", 30.0m, guarantee.PW_BondAmount);

				permitRule.CPR_ValueFrom = "ZER";
				guarantee.PW_SuretyCode = "FUL";
				guarantee.DefaultSuretyCodeIfHasMatchLapPermitRule();
				guarantee.SetLiabilityAmount(100);
				AssertEquals("When ZER Permit Rule exists zero is taken as amount", 0m, guarantee.PW_BondAmount);
			});
		}

		public void TestSuretyCodeIsDefaultedWhenPermitRuleExists()
		{
			var permitRule = permitHeader.CusGuaranteeRules.AddNew();
			permitRule.CPR_RuleCode = "LAP";
			permitRule.CPR_ValueFrom = "FUL";

			CombineAssertions("Liability amounts depend on existing permit rule", () =>
			{
				var guarantee = GetNewGuaranteeAndClearOldOnes(nctsHeader, "1234");
				guarantee.PW_SuretyCode = "ZER";
				guarantee.DefaultSuretyCodeIfHasMatchLapPermitRule();
				AssertEquals("When FUL Permit Rule exists FUL surety code is taken", "FUL", guarantee.PW_SuretyCode);

				permitRule.CPR_ValueFrom = "HAL";
				guarantee.PW_SuretyCode = "ZER";
				guarantee.DefaultSuretyCodeIfHasMatchLapPermitRule();
				AssertEquals("When HAL Permit Rule exists HAL surety code is taken", "HAL", guarantee.PW_SuretyCode);

				permitRule.CPR_ValueFrom = "THI";
				guarantee.PW_SuretyCode = "ZER";
				guarantee.DefaultSuretyCodeIfHasMatchLapPermitRule();
				AssertEquals("When THI Permit Rule exists THI surety code is taken", "THI", guarantee.PW_SuretyCode);

				permitRule.CPR_ValueFrom = "ZER";
				guarantee.PW_SuretyCode = "FUL";
				guarantee.DefaultSuretyCodeIfHasMatchLapPermitRule();
				AssertEquals("When ZER Permit Rule exists ZER surety code is taken", "ZER", guarantee.PW_SuretyCode);
			});
		}

		public void TestSetLiabilityAmountWhenPermitRuleDoesNotExist()
		{
			CombineAssertions("Liability amounts depend on guarantee", () =>
			{
				var guarantee = GetNewGuaranteeAndClearOldOnes(nctsHeader, "1234");
				guarantee.PW_SuretyCode = "FUL";
				guarantee.SetLiabilityAmount(100);

				AssertEquals("When PW_SuretyCode is FUL full amount is taken", 100m, guarantee.PW_BondAmount);

				guarantee.PW_SuretyCode = "HAL";
				guarantee.SetLiabilityAmount(100);
				AssertEquals("When PW_SuretyCode is HAL half amount is taken", 50m, guarantee.PW_BondAmount);

				guarantee.PW_SuretyCode = "THI";
				guarantee.SetLiabilityAmount(100);
				AssertEquals("When PW_SuretyCode is THI third of amount is taken", 30.0m, guarantee.PW_BondAmount);

				guarantee.PW_SuretyCode = "ZER";
				guarantee.SetLiabilityAmount(100);
				AssertEquals("When PW_SuretyCode is ZER taken as amount", 0m, guarantee.PW_BondAmount);
			});
		}

		public void TestSuretyCodeAndAmountWithExistingPermitRule()
		{
			var permitRule = permitHeader.CusGuaranteeRules.AddNew();
			permitRule.CPR_RuleCode = "LAP";
			permitRule.CPR_ValueFrom = "FUL";

			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
			goodsItem.BY_HarmonisedTariff = NCTSTestHelper.TestTariffCode;
			goodsItem.BY_MonetaryValue = 100;

			CombineAssertions("liability percentages - permit rule always wins on guaranty", () =>
			{
				var guarantee = GetNewGuaranteeAndClearOldOnes(nctsHeader, "1234");
				guarantee.PW_SuretyCode = "ZER";
				AssertEquals("When existing permit rule is FUL, full Amount is taken", 34.40000m, guarantee.PW_BondAmount);
				AssertEquals("When existing permit rule is FUL, FUL Surety Code is taken", "FUL", guarantee.PW_SuretyCode);

				permitRule.CPR_ValueFrom = "HAL";
				guarantee.PW_SuretyCode = "ZER";
				AssertEquals("When existing permit rule is HAL, half Amount is taken", 17.20000m, guarantee.PW_BondAmount);
				AssertEquals("When existing permit rule is HAL, HAL Surety Code is taken", "HAL", guarantee.PW_SuretyCode);

				permitRule.CPR_ValueFrom = "THI";
				guarantee.PW_SuretyCode = "FUL";
				AssertEquals("When existing permit rule is THI, a third of the Amount is taken", 10.32m, guarantee.PW_BondAmount);
				AssertEquals("When existing permit rule is THI, THI Surety Code is taken", "THI", guarantee.PW_SuretyCode);

				permitRule.CPR_ValueFrom = "ZER";
				guarantee.PW_SuretyCode = "HAL";
				AssertEquals("When existing permit rule is ZER, zero is taken as Amount", 0m, guarantee.PW_BondAmount);
				AssertEquals("When existing permit rule is ZER, ZER Surety Code is taken", "ZER", guarantee.PW_SuretyCode);
			});
		}

		public void TestLiabilityAmountIsCalculatedOnSuretyCodeChange()
		{
			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
			goodsItem.BY_HarmonisedTariff = NCTSTestHelper.TestTariffCode;
			goodsItem.BY_MonetaryValue = 100;

			CombineAssertions("liability percentages with no permit rule - guaranty is used", () =>
			{
				var guarantee = GetNewGuaranteeAndClearOldOnes(nctsHeader, "1234");
				guarantee.PW_SuretyCode = "FUL";
				AssertEquals("When PW_SuretyCode is FUL, full amount is taken", 34.40000m, guarantee.PW_BondAmount);

				guarantee.PW_SuretyCode = "HAL";
				AssertEquals("When PW_SuretyCode is HAL, half amount is taken", 17.20000m, guarantee.PW_BondAmount);

				guarantee.PW_SuretyCode = "THI";
				AssertEquals("When PW_SuretyCode is THI, a third of the amount is taken", 10.32m, guarantee.PW_BondAmount);

				guarantee.PW_SuretyCode = "ZER";
				AssertEquals("When PW_SuretyCode is ZER, zero is taken as Amount", 0m, guarantee.PW_BondAmount);
			});
		}

		public void TestSuretyCodeAndBondAmountReadOnly()
		{
			var guarantee = nctsHeader.Guarantees.AddNew();
			guarantee.PW_BondNumber = "1234";
			guarantee.PW_SuretyCode = "ZER";

			var permitRule = permitHeader.CusGuaranteeRules.AddNew();
			permitRule.CPR_RuleCode = "LAP";
			permitRule.CPR_ValueFrom = "FUL";

			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
			goodsItem.BY_HarmonisedTariff = NCTSTestHelper.TestTariffCode;
			goodsItem.BY_MonetaryValue = 100;

			using (NctsConfigurationTestHelper.TemporarilySetGuaranteeOverrideSupportConfiguration(Factory, true))
			{
				AssertEquals("After modification: GuaranteeConfiguration OverrideSupport", true, nctsHeader.Configuration.GuaranteeConfiguration.OverrideSupport(nctsHeader));
				CombineAssertions(() =>
				{
					AssertEquals("When permit rule exists, then SuretyCode is readonly", true, guarantee.PW_SuretyCodeInfo.ReadOnly);
					AssertEquals("When permit rule exists, then BondAmount is readonly", true, guarantee.PW_BondAmountInfo.ReadOnly);

					permitHeader.CusGuaranteeRules.DeleteAll();
					guarantee = GetNewGuaranteeAndClearOldOnes(nctsHeader, "ZER");
					AssertEquals("When no permit rule exists, then SuretyCode is not readonly", false, guarantee.PW_SuretyCodeInfo.ReadOnly);
					AssertEquals("When no permit rule exists, then BondAmount is not readonly", false, guarantee.PW_BondAmountInfo.ReadOnly);
				});
			}
		}

		public void TestPW_SuretyCodeReCalculatedAfterPW_BondNumberChange()
		{
			var guarantee = nctsHeader.Guarantees.AddNew();
			guarantee.PW_BondNumber = "1234";
			guarantee.PW_SuretyCode = "ZER";

			var permitRule = permitHeader.CusGuaranteeRules.AddNew();
			permitRule.CPR_RuleCode = "LAP";
			permitRule.CPR_ValueFrom = "FUL";

			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
			goodsItem.BY_HarmonisedTariff = NCTSTestHelper.TestTariffCode;
			goodsItem.BY_MonetaryValue = 100;

			using (NctsConfigurationTestHelper.TemporarilySetGuaranteeOverrideSupportConfiguration(Factory, true))
			{
				AssertEquals("After modification: GuaranteeConfiguration OverrideSupport", true, nctsHeader.Configuration.GuaranteeConfiguration.OverrideSupport(nctsHeader));
				CombineAssertions("SuretyCode recalculated on bond number change", () =>
				{
					AssertEquals("When LapPermitRule exist SuretyCode is readonly", true, guarantee.PW_SuretyCodeInfo.ReadOnly);

					permitHeader.CusGuaranteeRules.DeleteAll();
					guarantee.PW_BondNumber = "9999";

					AssertEquals("When LapPermitRule doesn't exist SuretyCode is not readonly", false, guarantee.PW_SuretyCodeInfo.ReadOnly);
				});
			}
		}

		NctsGuarantee GetNewGuaranteeAndClearOldOnes(NctsHeader nctsHeader, ZString bondNumber)
		{
			nctsHeader.Guarantees.RemoveAndDeleteAll();
			var guarantee = nctsHeader.Guarantees.AddNew();
			guarantee.PW_BondNumber = bondNumber;

			return guarantee;
		}

		protected override void SetUp()
		{
			organization = Factory.New<OrgHeader>();
			organization.OH_Code = "ARGO";

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.BH_JobReference = "ARG001001";
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			nctsHeader.BH_CustomsProfile = "1234";

			nctsHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "IT000000");

			NCTSTestHelper.SetUpTariff(Factory);

			permitHeader = NCTSTestHelper.SetupGuarantee(organization);
			Factory.Save();
		}

		OrgHeader organization;
		NctsHeader nctsHeader;
		CusGuaranteeHeader permitHeader;
	}
}
