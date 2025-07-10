using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Freight.Forwarding.Business;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsPackagePhase5ValidationBaseOnlyTest : BusinessObjectValidationTestCase
	{
		public void TestCheckB5_GrossWeight_RuleNR0061()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			var nctsBill = nctsHeader.Bills.AddNew();
			var goodsItem = nctsBill.ArrivalGoodsItems.AddNew();
			var nctsPackage = goodsItem.Packages.AddNew();

			const string expectedWarning = "[NR0061] Gross Weight per package type should be provided if goods are managed in Temporary Storage module. Otherwise, the total Gross Weight will be apportioned among all package types";
			using var ruleContext = nctsPackage.CreateArrivalPhase5ValidationTestContext();
			CombineAssertions(() =>
			{
				ruleContext.EnableRule(c => c.IsRuleNR0061Active);

				nctsPackage.UnloadedStatus = "DIF";
				var nctsPackDifference = nctsPackage.PackDifference;
				nctsPackage.B5_GrossWeight = ZDecimal.Zero;
				nctsPackDifference.B5_GrossWeight = ZDecimal.Zero;
				AssertHasWarningContaining("Rule Active and Gross is 0", nctsPackage.B5_GrossWeightInfo, expectedWarning);
				AssertNoWarningContaining("Rule Active and Gross is 0 but is PackDifference", nctsPackDifference.B5_GrossWeightInfo, expectedWarning);

				nctsPackage.B5_GrossWeight = 2;
				AssertNoWarningContaining("Rule Active and Gross is not 0", nctsPackage.B5_GrossWeightInfo, expectedWarning);

				ruleContext.DisableRule(c => c.IsRuleNR0061Active);
				nctsPackage.B5_GrossWeight = ZDecimal.Zero;
				AssertNoWarningContaining("Rule Inactive and Gross is 0", nctsPackage.B5_GrossWeightInfo, expectedWarning);

				nctsPackage.B5_GrossWeight = 2;
				AssertNoWarningContaining("Rule Inactive and Gross is not 0", nctsPackage.B5_GrossWeightInfo, expectedWarning);
			});
		}

		public void TestCheckB5_UnitType_Mandatory()
		{
			using var ruleContext = nctsPackage.CreateDeparturePhase5ValidationTestContext();

			CombineAssertions(() =>
			{
				ruleContext.EnableRule(c => c.IsRuleTR0083Active);
				nctsPackage.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DIF;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(nctsPackage.B5_UnitTypeInfo);

				nctsPackage.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(nctsPackage.B5_UnitTypeInfo);

				nctsPackage.B5_TypeOfDifference = ZString.Empty;
				ValidationTestHelper.AssertFieldIsNotMandatory(nctsPackage.B5_UnitTypeInfo);

				ruleContext.DisableRule(c => c.IsRuleTR0083Active);
				nctsPackage.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DIF;
				ValidationTestHelper.AssertFieldIsNotMandatory(nctsPackage.B5_UnitTypeInfo);

				nctsPackage.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;
				ValidationTestHelper.AssertFieldIsNotMandatory(nctsPackage.B5_UnitTypeInfo);
			});
		}

		public void TestCheckB5_UnitCount_C0060Rule()
		{
			const string expectedWarning = "[C0060] You have not entered a Number of Packages - valid for goods in shared packaging.";
			var bulkType = Factory.SetupBulkCusCode();
			var unpackedType = Factory.SetupUnpackCusCode();
			CombineAssertions(() =>
			{
				using var deciderTestContext = nctsPackage.CreateDeparturePhase5ValidationTestContext();
				deciderTestContext.EnableRule(decider => decider.IsRuleC0060Active);

				nctsPackage.B5_UnitCount = 1;
				AssertNoWarningContaining("Empty Type - No warning", nctsPackage.B5_UnitCountInfo, expectedWarning);
				nctsPackage.B5_UnitCount = 0;
				AssertNoWarningContaining("Empty Type - No warning", nctsPackage.B5_UnitCountInfo, expectedWarning);

				nctsPackage.B5_UnitType = bulkType;
				AssertNoWarningContaining("Bulk Type - No warning", nctsPackage.B5_UnitCountInfo, expectedWarning);

				nctsPackage.B5_UnitType = unpackedType;
				nctsPackage.B5_UnitCount = 1;
				AssertNoWarningContaining("Unpacked Type - No warning when unit count > 0", nctsPackage.B5_UnitCountInfo, expectedWarning);
				nctsPackage.B5_UnitCount = 0;
				AssertHasWarningContaining("Unpacked Type - Warning when unit count is 0", nctsPackage.B5_UnitCountInfo, expectedWarning);

				nctsPackage.B5_UnitType = "AB";
				nctsPackage.B5_UnitCount = 1;
				AssertNoWarningContaining("Other Type - No warning when unit count > 0", nctsPackage.B5_UnitCountInfo, expectedWarning);
				nctsPackage.B5_UnitCount = 0;
				AssertHasWarningContaining("Other Type - Warning when unit count is 0", nctsPackage.B5_UnitCountInfo, expectedWarning);

				deciderTestContext.DisableRule(decider => decider.IsRuleC0060Active);
				nctsPackage.B5_UnitType = "AB";
				nctsPackage.B5_UnitCount = 0;
				AssertNoWarningContaining("Rule disabled - IsBulk: false and Count: 0d", nctsPackage.B5_UnitCountInfo, expectedWarning);

				nctsPackage.B5_UnitType = bulkType;
				nctsPackage.B5_UnitCount = 1;
				AssertNoWarningContaining("Rule disabled - IsBulk: true and Count: > 0", nctsPackage.B5_UnitCountInfo, expectedWarning);
			});
		}

		public void TestCheckB5_UnitCount_C0060_1Rule()
		{
			const string message = "[C0060-1] You have entered a non-countable Package Type.";

			var bulkType = Factory.SetupBulkCusCode();
			CombineAssertions(() =>
			{
				using var deciderTestContext = nctsPackage.CreateDeparturePhase5ValidationTestContext();
				deciderTestContext.EnableRule(decider => decider.IsRuleC0060_1Active);

				nctsPackage.B5_UnitType = bulkType;
				nctsPackage.B5_UnitCount = 1;
				AssertHasMessageError("Has message", nctsPackage.B5_UnitCountInfo, message);

				nctsPackage.B5_UnitType = "AB";
				nctsPackage.Validation.ValidateB5_UnitCount();
				AssertNoMessageError("IsBulk is false", nctsPackage.B5_UnitCountInfo, message);

				nctsPackage.B5_UnitType = bulkType;
				AssertNoMessageError("B5_UnitCount is zero", nctsPackage.B5_UnitCountInfo, message);

				deciderTestContext.DisableRule(decider => decider.IsRuleC0060_1Active);
				nctsPackage.B5_UnitType = bulkType;
				nctsPackage.B5_UnitCount = 1;
				AssertNoMessageError("Rule disabled", nctsPackage.B5_UnitCountInfo, message);
			});
		}

		public void TestCheckB5_UnitCount_C0060_2Rule()
		{
			const string message = "[C0060-2] You have not entered a Number of Packages.";

			var unpackedType = Factory.SetupUnpackCusCode();
			CombineAssertions(() =>
			{
				using var deciderTestContext = nctsPackage.CreateDeparturePhase5ValidationTestContext();
				deciderTestContext.EnableRule(decider => decider.IsRuleC0060_2Active);
				deciderTestContext.DisableRule(decider => decider.IsRuleC0060Active);

				nctsPackage.B5_UnitType = unpackedType;
				nctsPackage.B5_UnitCount = 0;
				AssertHasMessageError("Has message error", nctsPackage.B5_UnitCountInfo, message);

				nctsPackage.B5_UnitType = "AB";
				nctsPackage.Validation.ValidateB5_UnitCount();
				AssertNoMessageError("IsUnpacked is false", nctsPackage.B5_UnitCountInfo, message);

				nctsPackage.B5_UnitType = unpackedType;
				nctsPackage.B5_UnitCount = 1;
				AssertNoMessageError("B5_UnitCount isn't empty", nctsPackage.B5_UnitCountInfo, message);

				deciderTestContext.DisableRule(decider => decider.IsRuleC0060_2Active);
				nctsPackage.B5_UnitType = unpackedType;
				nctsPackage.B5_UnitCount = 0;
				AssertNoMessageError("Rule disabled", nctsPackage.B5_UnitCountInfo, message);
			});
		}

		public void TestCheckB5_UnitCount_NR0003Rule()
		{
			var message = "[NR0003] Pack Quantity > 0 in Combination with an Inner Pack on the same Goods Item is not allowed.";
			var bulkType = Factory.SetupBulkCusCode();
			CombineAssertions(() =>
			{
				using var rule = nctsPackage.CreateDeparturePhase5ValidationTestContext();
				rule.EnableRule(decider => decider.IsRuleNR0003Active);
				var nctsPackage2 = goodsItem.Packages.AddNew();

				nctsPackage.B5_UnitType = "AB";
				nctsPackage.B5_UnitCount = 0;
				AssertHasMessageError("Has message", nctsPackage.B5_UnitCountInfo, message);

				nctsPackage.B5_UnitType = bulkType;
				nctsPackage.Validation.ValidateB5_UnitCount();
				AssertNoMessageError("IsBulk is true", nctsPackage.B5_UnitCountInfo, message);

				nctsPackage.B5_UnitType = "AB";
				nctsPackage.B5_UnitCount = 1;
				AssertNoMessageError("B5_UnitCount > 0", nctsPackage.B5_UnitCountInfo, message);

				nctsPackage2.B5_UnitCount = 0;
				nctsPackage.B5_UnitCount = 0;
				AssertNoMessageError("No other Package with B5_UnitCount > 0", nctsPackage.B5_UnitCountInfo, message);

				rule.DisableRule(decider => decider.IsRuleNR0003Active);
				nctsPackage.B5_UnitType = "AB";
				nctsPackage.B5_UnitCount = 0;
				AssertNoMessageError("Rule inactive", nctsPackage.B5_UnitCountInfo, message);
			});
		}

		public void TestCheckB5_UnitCount_R0364_1Rule()
		{
			var message = "[R0364-1] You have not entered a Package Quantity or a Main Pack on another Line of this House Consignment.";
			var bulkType = Factory.SetupBulkCusCode();
			var unpackedType = Factory.SetupUnpackCusCode();
			CombineAssertions(() =>
			{
				using var ruleContext = nctsPackage.CreateDeparturePhase5ValidationTestContext();
				ruleContext.EnableRule(x => x.IsRuleR0364_1Active);

				nctsPackage.B5_UnitType = "AB";
				nctsPackage.B5_UnitCount = 0;
				AssertHasMessageError("Has message", nctsPackage.B5_UnitCountInfo, message);

				nctsPackage.B5_UnitType = bulkType;
				nctsPackage.Validation.ValidateB5_UnitCount();
				AssertNoMessageError("IsBulk is true", nctsPackage.B5_UnitCountInfo, message);

				nctsPackage.B5_UnitType = unpackedType;
				nctsPackage.Validation.ValidateB5_UnitCount();
				AssertNoMessageError("IsUnpacked is true", nctsPackage.B5_UnitCountInfo, message);

				nctsPackage.B5_UnitType = "AB";
				nctsPackage.B5_UnitCount = 1;
				AssertNoMessageError("B5_UnitCount > 0", nctsPackage.B5_UnitCountInfo, message);

				var goodsItem2 = goodsItem.Bill.GoodsItems.AddNew();
				goodsItem2.BY_IsMainPack = true;
				var nctsPackage2 = goodsItem2.Packages.AddNew();
				nctsPackage2.B5_MarksAndNumbers = "MN001";

				nctsPackage.B5_MarksAndNumbers = "MN001";
				nctsPackage.B5_UnitCount = 0;
				AssertNoMessageError("Has Package that satisfies the condition", nctsPackage.B5_UnitCountInfo, message);

				goodsItem2.BY_IsMainPack = false;
				nctsPackage.Validation.ValidateB5_UnitCount();
				AssertHasMessageError("goodsItem2.BY_IsMainPack is false", nctsPackage.B5_UnitCountInfo, message);

				goodsItem2.BY_IsMainPack = true;
				nctsPackage2.B5_UnitCount = 0;
				nctsPackage.Validation.ValidateB5_UnitCount();
				AssertHasMessageError("B5_UnitCount of other Package is zero", nctsPackage.B5_UnitCountInfo, message);

				nctsPackage2.B5_UnitCount = 1;
				nctsPackage2.B5_MarksAndNumbers = "MN002";
				nctsPackage.Validation.ValidateB5_UnitCount();
				AssertHasMessageError("B5_MarksAndNumbers of other Package is different", nctsPackage.B5_UnitCountInfo, message);

				ruleContext.DisableRule(x => x.IsRuleR0364_1Active);
				nctsPackage.B5_UnitType = "AB";
				nctsPackage.B5_UnitCount = 0;
				AssertNoMessageError("Rule disabled", nctsPackage.B5_UnitCountInfo, message);
			});
		}

		public void TestCheckB5_TypeOfDifference_ListValueOrEmpty()
		{
			var typeOfDifferenceInfo = nctsPackage.B5_TypeOfDifferenceInfo;
			CombineAssertions(() =>
			{
				nctsPackage.B5_TypeOfDifference = ZString.Empty;
				AssertHasErrorContaining("Mandatory", typeOfDifferenceInfo, MandatoryValidation.MustBeEntered);

				nctsPackage.B5_TypeOfDifference = "N/A";
				AssertNoErrorContaining("Entered", typeOfDifferenceInfo, MandatoryValidation.MustBeEntered);
				AssertHasErrorContaining("Invalid Code", typeOfDifferenceInfo, ListValidation.InvalidCodeError);

				nctsPackage.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DEC;
				AssertNoErrorContaining("Valid Code DEC", typeOfDifferenceInfo, ListValidation.InvalidCodeError);

				nctsPackage.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DIF;
				AssertHasErrorContaining("Invalid Code DIF when no parent", typeOfDifferenceInfo, ListValidation.InvalidCodeError);

				nctsPackage.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;
				AssertNoErrorContaining("Valid Code NEW", typeOfDifferenceInfo, ListValidation.InvalidCodeError);

				nctsPackage.B5_TypeOfDifference = NctsUnloadedStateList.Codes.MIS;
				AssertNoErrorContaining("Valid Code MIS", typeOfDifferenceInfo, ListValidation.InvalidCodeError);

				nctsPackage.B5_B5_ParentPackage = Factory.New<NctsPackage>().PK;
				nctsPackage.B5_TypeOfDifference = ZString.Empty;
				AssertNoErrorContaining("Entered", typeOfDifferenceInfo, MandatoryValidation.MustBeEntered);
				nctsPackage.B5_TypeOfDifference = "DIF";
				AssertNoErrorContaining("Valid Code when has parent", typeOfDifferenceInfo, ListValidation.InvalidCodeError);

				nctsPackage.B5_TypeOfDifference = "DAM";
				AssertHasErrorContaining("Invalid Code when has parent", typeOfDifferenceInfo, ListValidation.InvalidCodeError);

				nctsPackage.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DEC;
				AssertNoErrorContaining("Valid Code DEC when has parent", typeOfDifferenceInfo, ListValidation.InvalidCodeError);

				nctsPackage.B5_TypeOfDifference = NctsUnloadedStateList.Codes.MIS;
				AssertNoErrorContaining("Valid Code MIS when has parent", typeOfDifferenceInfo, ListValidation.InvalidCodeError);

				nctsPackage.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;
				AssertNoErrorContaining("Valid Code NEW when has parent", typeOfDifferenceInfo, ListValidation.InvalidCodeError);
			});
		}

		public void TestCheckB5_TypeOfDifference_New()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Should be NEW at start", "NEW", nctsPackage.B5_TypeOfDifference);
				AssertNoErrorContaining("Being NEW at start shouldn't give an error", nctsPackage.B5_TypeOfDifferenceInfo, ListValidation.InvalidCodeError);
				Factory.Save();
				nctsPackage.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DEC;
				nctsPackage.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;
				AssertNoErrorContaining("Changing to NEW shouldn't give an error when the previous value was NEW", nctsPackage.B5_TypeOfDifferenceInfo, ListValidation.InvalidCodeError);
				nctsPackage.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DEC;
				Factory.Save();
				nctsPackage.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;
				AssertHasErrorContaining("Switching from a non NEW value to NEW should give an error", nctsPackage.B5_TypeOfDifferenceInfo, ListValidation.InvalidCodeError);

				nctsPackage.B5_B5_ParentPackage = Factory.New<NctsPackage>().PK;
				nctsPackage.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DEC;
				nctsPackage.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;

				AssertNoErrorContaining("Switching from a non NEW value to NEW does not apply to package with parent", nctsPackage.B5_TypeOfDifferenceInfo, ListValidation.InvalidCodeError);
			});
		}

		public void TestCheckB5_MarksAndNumbers_Mandatory_Arrival() => CombineAssertions(() =>
		{
			var bulkType = Factory.SetupBulkCusCode();
			var unpackType = Factory.SetupUnpackCusCode();
			var standardType = Factory.SetupStandardPackCusCode();

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			var goodsItem = nctsHeader.Bills.AddNew().ArrivalGoodsItems.AddNew();
			var nctsPackage = goodsItem.Packages.AddNew();

			using var ruleContext = nctsPackage.CreateArrivalPhase5ValidationTestContext();

			AssertMandatory(false, NctsUnloadedStateList.Codes.DIF, bulkType);
			AssertMandatory(false, NctsUnloadedStateList.Codes.DIF, unpackType);
			AssertMandatory(true, NctsUnloadedStateList.Codes.DIF, standardType);

			AssertMandatory(false, NctsUnloadedStateList.Codes.NEW, bulkType);
			AssertMandatory(false, NctsUnloadedStateList.Codes.NEW, unpackType);
			AssertMandatory(true, NctsUnloadedStateList.Codes.NEW, standardType);

			void AssertMandatory(bool mandatoryExpected, string typeOfDifference, string unitType)
			{
				nctsPackage.B5_TypeOfDifference = typeOfDifference;
				nctsPackage.B5_UnitType = unitType;
				var packType = unitType == bulkType ? "Bulk" : unitType == unpackType ? "Unpack" : "Standard";

				ruleContext.EnableRule(c => c.IsRuleTR0097Active);

				nctsPackage.B5_MarksAndNumbers = "X";
				AssertNoMessageErrorContaining(AssertionMessage(), nctsPackage.B5_MarksAndNumbersInfo, MandatoryValidation.YouHaveNotEntered);

				nctsPackage.B5_MarksAndNumbers = ZString.Empty;
				if (mandatoryExpected)
				{
					AssertHasMessageErrorContaining(AssertionMessage(), nctsPackage.B5_MarksAndNumbersInfo, MandatoryValidation.YouHaveNotEntered);
				}
				else
				{
					AssertNoMessageErrorContaining(AssertionMessage(), nctsPackage.B5_MarksAndNumbersInfo, MandatoryValidation.YouHaveNotEntered);
				}

				ruleContext.DisableRule(c => c.IsRuleTR0097Active);
				nctsPackage.Validation.ValidateB5_MarksAndNumbers();
				AssertNoMessageErrorContaining(AssertionMessage(), nctsPackage.B5_MarksAndNumbersInfo, MandatoryValidation.YouHaveNotEntered);

				string AssertionMessage() => $"TR0097={(nctsPackage.ValidationDecider as INctsPackageArrivalPhase5ValidationDecider).IsRuleTR0097Active} B5_TypeOfDifference={typeOfDifference} B5_UnitType={packType}";
			}
		});

		public void TestCheckB5_MarksAndNumbers_MaxLength()
		{
			CombineAssertions(() =>
			{
				UniversalValidationHelperTest.AssertMaxLengthDuringTransitionPeriod(nctsPackage.B5_MarksAndNumbersInfo, 42);
			});
		}

		public void TestCheckB5_MarksAndNumbers_C0060Rule()
		{
			const string messageError = "[C0060] You have not entered a Marks and Numbers.";
			var bulkType = Factory.SetupBulkCusCode();
			var unpackedType = Factory.SetupUnpackCusCode();
			CombineAssertions(() =>
			{
				using var deciderTestContext = nctsPackage.CreateDeparturePhase5ValidationTestContext();
				deciderTestContext.EnableRule(decider => decider.IsRuleC0060Active);

				nctsPackage.Validation.ValidateB5_MarksAndNumbers();
				AssertNoMessageErrorContaining("Empty Type", nctsPackage.B5_MarksAndNumbersInfo, messageError);

				nctsPackage.B5_UnitType = bulkType;
				nctsPackage.Validation.ValidateB5_MarksAndNumbers();
				AssertNoMessageErrorContaining("Bulk Type", nctsPackage.B5_MarksAndNumbersInfo, messageError);

				nctsPackage.B5_UnitType = unpackedType;
				nctsPackage.Validation.ValidateB5_MarksAndNumbers();
				AssertNoMessageErrorContaining("Unpacked Type", nctsPackage.B5_MarksAndNumbersInfo, messageError);

				nctsPackage.B5_UnitType = "AB";
				nctsPackage.Validation.ValidateB5_MarksAndNumbers();
				AssertHasMessageErrorContaining("Other Type", nctsPackage.B5_MarksAndNumbersInfo, messageError);

				deciderTestContext.DisableRule(decider => decider.IsRuleC0060Active);
				nctsPackage.B5_UnitType = "AB";
				nctsPackage.Validation.ValidateB5_MarksAndNumbers();
				AssertNoMessageErrorContaining("Rule disabled", nctsPackage.B5_MarksAndNumbersInfo, messageError);
			});
		}

		public void TestCheckB5_MarksAndNumbers_C0060_3Rule()
		{
			const string message = "[C0060-3] You have not entered a Marks and Numbers.";
			var bulkType = Factory.SetupBulkCusCode();
			var unpackedType = Factory.SetupUnpackCusCode();
			CombineAssertions(() =>
			{
				using var deciderTestContext = nctsPackage.CreateDeparturePhase5ValidationTestContext();
				deciderTestContext.DisableRule(decider => decider.IsRuleC0060Active);
				deciderTestContext.EnableRule(decider => decider.IsRuleC0060_3Active);

				nctsPackage.B5_UnitType = "AB";
				nctsPackage.Validation.ValidateB5_MarksAndNumbers();
				AssertHasMessageError("Has message error", nctsPackage.B5_MarksAndNumbersInfo, message);

				nctsPackage.B5_UnitType = bulkType;
				nctsPackage.Validation.ValidateB5_MarksAndNumbers();
				AssertNoMessageError("IsBulk is true", nctsPackage.B5_MarksAndNumbersInfo, message);

				nctsPackage.B5_UnitType = unpackedType;
				nctsPackage.Validation.ValidateB5_MarksAndNumbers();
				AssertNoMessageError("IsUnpacked is true", nctsPackage.B5_MarksAndNumbersInfo, message);

				nctsPackage.B5_UnitType = "AB";
				nctsPackage.B5_MarksAndNumbers = "MN001";
				AssertNoMessageError("B5_MarksAndNumbers isn't empty", nctsPackage.B5_MarksAndNumbersInfo, message);

				deciderTestContext.DisableRule(decider => decider.IsRuleC0060_3Active);
				nctsPackage.B5_UnitType = "AB";
				nctsPackage.Validation.ValidateB5_MarksAndNumbers();
				AssertNoMessageError("Rule disabled", nctsPackage.B5_MarksAndNumbersInfo, message);
			});
		}

		public void TestCheckCheckB5_MarksAndNumbers_PluggedIntoShipment()
		{
			const string MarksAndNosRequiredMessage = "Instead package marks should be supplied on the shipment's packing tab.";

			CombineAssertions(() =>
			{
				nctsPackage.Validation.ValidateB5_MarksAndNumbers();
				AssertNoMessageErrorContaining("Not plugged into shipment - B5_MarksAndNumber empty", nctsPackage.B5_MarksAndNumbersInfo, MarksAndNosRequiredMessage);

				nctsHeader.BH_ParentID = Factory.New<ForwardingShipment>().PK;
				nctsPackage.Validation.ValidateB5_MarksAndNumbers();
				AssertHasMessageErrorContaining("Plugged into shipment - B5_MarksAndNumber empty", nctsPackage.B5_MarksAndNumbersInfo, MarksAndNosRequiredMessage);
				nctsPackage.Validation.ValidateB5_MarksAndNumbers();
				nctsPackage.B5_MarksAndNumbers = "MN002";
				AssertNoMessageErrorContaining("Plugged into shipment - B5_MarksAndNumber not empty", nctsPackage.B5_MarksAndNumbersInfo, MarksAndNosRequiredMessage);
			});
		}

		public void TestCheckB5_UnitCount_E1111Rule_TransitionPeriod() => CombineAssertions(() =>
		{
			const string message = "[E1111] Entered Number Of Packages exceeding the max value (5 digits) supported Inside Transition Period.";

			using var ruleContext = nctsPackage.CreateDeparturePhase5ValidationTestContext();
			ruleContext.SetNctsTransitionPeriod(true);
			ruleContext.EnableRule(c => c.IsRuleE1111Active);

			nctsPackage.B5_UnitCount = 99999;
			nctsPackage.Validation.ValidateB5_UnitCount();
			AssertNoMessageError("Transition period unit count does not exceed 5 digits", nctsPackage.B5_UnitCountInfo, message);

			nctsPackage.B5_UnitCount = 1000000;
			AssertHasMessageError("Transition period unit count exceeds 5 digits in transition period", nctsPackage.B5_UnitCountInfo, message);

			ruleContext.DisableRule(c => c.IsRuleE1111Active);
			nctsPackage.Validation.ValidateB5_UnitCount();
			AssertNoMessageError("Transition period unit count exceeds 5 digits in transition period - rule disabled", nctsPackage.B5_UnitCountInfo, message);

			ruleContext.SetNctsTransitionPeriod(false);
			ruleContext.EnableRule(c => c.IsRuleE1111Active);
			nctsPackage.Validation.ValidateB5_UnitCount();
			AssertNoMessageError("Transition period unit count exceeds 5 digits but is not in transition period", nctsPackage.B5_UnitCountInfo, message);
		});

		public void TestCheckB5_UnitCount_TR0066Rule_NotInTransitionPeriod()
		{
			var tr0066message = "[TR0066] Please enter a 'Number of Packages' less than or equal to 99999999.";
			var message = "Please enter a 'Number of Packages' less than or equal to 99999999.";
			CombineAssertions(() =>
			{
				using var testContext = nctsPackage.CreateDeparturePhase5ValidationTestContext();
				using (TemporarilySetTransitionPeriod(false))
				{
					testContext.EnableRule(decider => decider.IsRuleTR0066Active);
					nctsPackage.B5_UnitCount = 99999999;
					nctsPackage.Validation.ValidateB5_UnitCount();
					AssertNoMessageError("Not In Transition Period, unit count not exceeded 8 digits and rule is active", nctsPackage.B5_UnitCountInfo, tr0066message);

					nctsPackage.B5_UnitCount = 100000000;
					AssertHasMessageError("Not In Transition Period, unit count exceeded 8 digits and rule is active", nctsPackage.B5_UnitCountInfo, tr0066message);

					testContext.DisableRule(decider => decider.IsRuleTR0066Active);
					nctsPackage.Validation.ValidateB5_UnitCount();
					AssertNoMessageError("Not In Transition Period, unit count exceeded 8 digits and rule is inactive", nctsPackage.B5_UnitCountInfo, tr0066message);
					AssertHasMessageErrorContaining("Not In Transition Period, unit count exceeded 8 digits and rule is inactive", nctsPackage.B5_UnitCountInfo, message);
				}

				using (TemporarilySetTransitionPeriod(true))
				{
					testContext.EnableRule(decider => decider.IsRuleTR0066Active);
					nctsPackage.Validation.ValidateB5_UnitCount();
					AssertNoMessageError("In Transition period, unit count exceeds 8 digits and rule is active", nctsPackage.B5_UnitCountInfo, tr0066message);
					AssertHasMessageErrorContaining("In Transition period, unit count exceeds 8 digits and rule is active", nctsPackage.B5_UnitCountInfo, message);
				}
			});
		}

		public void TestCheckB5_UnitCount_R0364_2Rule()
		{
			var notUnpackedOrBulkPackageType = "RC";
			var unpackPackType = Factory.SetupUnpackCusCode();
			var bulkPackType = Factory.SetupBulkCusCode();
			var packTypes = $"{nctsPackage.Lookups.UnpackedPackageUnitTypeList.CodesAsString}, {nctsPackage.Lookups.BulkPackageUnitTypeList.CodesAsString}";
			var message = ValidationRuleMessages.R0364_2Message(packTypes);
			Factory.Save();

			CombineAssertions(() =>
			{
				using var ruleContext = nctsPackage.CreateDeparturePhase5ValidationTestContext();
				ruleContext.EnableRule(x => x.IsRuleR0364_2Active);

				assertHasMessageError("Not Unpacked or Bulk Package Type - Count 0", notUnpackedOrBulkPackageType, 0);
				assertNoMessageError("Unpacked Package Type - Count 0", unpackPackType, 0);
				assertNoMessageError("Bulk Package Type - Count 0", bulkPackType, 0);
				assertNoMessageError("Not Unpacked or Bulk Package Type - Count 1", notUnpackedOrBulkPackageType, 1);

				var goodsItem2 = goodsItem.Bill.GoodsItems.AddNew();
				var nctsPackage2 = goodsItem2.Packages.AddNew();
				nctsPackage2.B5_UnitType = notUnpackedOrBulkPackageType;
				nctsPackage2.B5_UnitCount = 1;
				nctsPackage2.B5_MarksAndNumbers = "MN001";
				nctsPackage.B5_MarksAndNumbers = "MN001";

				assertNoMessageError("has other Package that satisfies the condition", notUnpackedOrBulkPackageType, 0);

				nctsPackage.B5_UnitType = "PX";
				nctsPackage.B5_UnitCount = 0;
				nctsPackage2.B5_UnitType = "CT";
				nctsPackage2.B5_UnitCount = 1;

				assertNoMessageError("UnitType of other Package is different", notUnpackedOrBulkPackageType, 0);

				nctsPackage2.B5_UnitType = notUnpackedOrBulkPackageType;
				nctsPackage2.B5_UnitCount = 0;

				assertHasMessageError("UnitType of other Package is same", notUnpackedOrBulkPackageType, 0);

				nctsPackage2.B5_UnitCount = 0;

				assertHasMessageError("UnitCount of other Package is zero", notUnpackedOrBulkPackageType, 0);

				nctsPackage2.B5_UnitCount = 1;
				nctsPackage2.B5_MarksAndNumbers = "MN002";

				assertHasMessageError("MarksAndNumbers of other Package is different", notUnpackedOrBulkPackageType, 0);

				ruleContext.DisableRule(x => x.IsRuleR0364_2Active);
				assertNoMessageError("Rule disabled", notUnpackedOrBulkPackageType, 0);
			});

			void assertHasMessageError(string hint, string packageType, ZLong unitCount)
			{
				nctsPackage.B5_UnitType = packageType;
				nctsPackage.B5_UnitCount = unitCount;
				AssertHasMessageError($"{hint} - has message error", nctsPackage.B5_UnitCountInfo, message);
			}

			void assertNoMessageError(string hint, string packageType, ZLong unitCount)
			{
				nctsPackage.B5_UnitType = packageType;
				nctsPackage.B5_UnitCount = unitCount;
				AssertNoMessageError($"{hint} - no message error", nctsPackage.B5_UnitCountInfo, message);
			}
		}

		public void TestCheckB5_UnitCount_R0364_3Rule()
		{
			var bulkPackType = Factory.SetupBulkCusCode();
			var message = ValidationRuleMessages.R0364_3Message();

			using var ruleContext = nctsPackage.CreateDeparturePhase5ValidationTestContext();
			CombineAssertions(() =>
			{
				ruleContext.EnableRule(c => c.IsRuleR0364_3Active);
				nctsPackage.B5_UnitType = bulkPackType;
				nctsPackage.B5_UnitCount = ZLong.Zero;
				AssertNoMessageError("No error when is bulk type", nctsPackage.B5_UnitCountInfo, message);

				nctsPackage.B5_UnitType = "BX";
				nctsPackage.Validation.ValidateB5_UnitCount();
				AssertHasMessageError("Error when is not bulk type and Qty is 0", nctsPackage.B5_UnitCountInfo, message);

				nctsPackage.B5_UnitCount = 1;
				nctsPackage.Validation.ValidateB5_UnitCount();
				AssertNoMessageError("No error when is not bulk type and Qty is more than 0", nctsPackage.B5_UnitCountInfo, message);

				var goodItem2 = goodsItem.Bill.GoodsItems.AddNew();
				var nctsPackage2 = goodItem2.Packages.AddNew();
				nctsPackage.B5_UnitCount = ZLong.Zero;
				nctsPackage.B5_MarksAndNumbers = "Marks";
				nctsPackage2.B5_UnitCount = ZLong.Zero;
				nctsPackage2.B5_MarksAndNumbers = "Marks";
				nctsPackage.Validation.ValidateB5_UnitCount();
				AssertHasMessageError("Error when exists other package on another GoodItem but its Qty is 0 and same Marks", nctsPackage.B5_UnitCountInfo, message);

				nctsPackage2.B5_UnitCount = 1;
				nctsPackage.Validation.ValidateB5_UnitCount();
				AssertNoMessageError("No Error when exists other package on another GoodItem with its Qty is more than 0 and same Marks", nctsPackage.B5_UnitCountInfo, message);

				nctsPackage2.B5_MarksAndNumbers = "Marks2";
				nctsPackage.Validation.ValidateB5_UnitCount();
				AssertHasMessageError("Error when exists other package on another GoodItem but its Qty is more than 0 but different Marks", nctsPackage.B5_UnitCountInfo, message);
			});

			CombineAssertions(() =>
			{
				ruleContext.DisableRule(c => c.IsRuleR0364_3Active);

				nctsPackage.B5_UnitType = "BX";
				nctsPackage.Validation.ValidateB5_UnitCount();
				AssertNoMessageError("No Error when exists other package on another GoodItem with its Qty is more than 0 and same Marks", nctsPackage.B5_UnitCountInfo, message);

				var goodItem2 = goodsItem.Bill.GoodsItems.AddNew();
				var nctsPackage2 = goodItem2.Packages.AddNew();
				nctsPackage.B5_UnitCount = ZLong.Zero;
				nctsPackage.B5_MarksAndNumbers = "Marks";
				nctsPackage2.B5_UnitCount = ZLong.Zero;
				nctsPackage2.B5_MarksAndNumbers = "Marks";
				nctsPackage.Validation.ValidateB5_UnitCount();
				AssertNoMessageError("No Error when exists other package on another GoodItem with its Qty is more than 0 and same Marks", nctsPackage.B5_UnitCountInfo, message);

				nctsPackage2.B5_MarksAndNumbers = "Marks2";
				nctsPackage.Validation.ValidateB5_UnitCount();
				AssertNoMessageError("No Error when exists other package on another GoodItem with its Qty is more than 0 and same Marks", nctsPackage.B5_UnitCountInfo, message);
			});
		}

		public void TestCheckRuleC0670()
		{
			nctsHeader.DepartureHeaderContainers.AddNew().BC_Mode = Core.Constants.ContainerModes.Containerised;
			nctsHeader.DepartureHeaderContainers.AddNew().BC_Mode = Core.Constants.ContainerModes.Containerised;

			nctsPackage.ContainersPivotsForBindingOnly[0].ContainerSelected = false;

			using var deciderTestContext = nctsPackage.CreatePhase5ValidationTestContext();

			deciderTestContext.DisableRule(decider => decider.IsRuleC0670Active);
			nctsPackage.Validation.ValidateAll();
			AssertNoRowMessageError("Rule C0670 is disabled", nctsPackage, RuleC0670MessageError);

			deciderTestContext.EnableRule(decider => decider.IsRuleC0670Active);
			nctsPackage.Validation.ValidateAll();
			AssertHasRowMessageError("Rule C0670 is enabled", nctsPackage, RuleC0670MessageError);
		}

		public void TestCheckRuleC0670_SingleContainer()
		{
			var container = nctsHeader.DepartureHeaderContainers.AddNew();

			var pivot = nctsPackage.ContainersPivotsForBindingOnly[0];

			CombineAssertions(() =>
			{
				pivot.ContainerSelected = true;
				AssertNoRowMessageError("Only one container and pivot is selected.", nctsPackage, RuleC0670MessageError);

				pivot.ContainerSelected = false;
				AssertNoRowMessageError("Only one container and pivot is unselected.", nctsPackage, RuleC0670MessageError);

				container.BC_Mode = Core.Constants.ContainerModes.Containerised;
				pivot.ContainerSelected = true;
				AssertNoRowMessageError("Only one container (in CNT mode) and pivot is selected", nctsPackage, RuleC0670MessageError);

				pivot.ContainerSelected = false;
				AssertNoRowMessageError("Only one container (in CNT mode) and pivot is unselected", nctsPackage, RuleC0670MessageError);

				nctsHeader.DepartureHeaderContainers.AddNew().BC_Mode = Core.Constants.ContainerModes.Containerised;
				nctsPackage.Validation.ValidateAll();
				AssertHasRowMessageError("Two containers (in CNT mode) and pivots are unselected", nctsPackage, RuleC0670MessageError);
			});
		}

		public void TestCheckRuleC0670_MultipleContainersOnSinglePackage()
		{
			var container1 = nctsHeader.DepartureHeaderContainers.AddNew();
			var container2 = nctsHeader.DepartureHeaderContainers.AddNew();

			var pivot1 = nctsPackage.ContainersPivotsForBindingOnly[0];
			var pivot2 = nctsPackage.ContainersPivotsForBindingOnly[1];

			CombineAssertions(() =>
			{
				pivot1.ContainerSelected = true;
				pivot2.ContainerSelected = false;
				nctsPackage.Validation.ValidateAll();
				AssertNoRowMessageError("Two containers are not in CNT mode and one pivot is selected.", nctsPackage, RuleC0670MessageError);

				pivot1.ContainerSelected = false;
				nctsPackage.Validation.ValidateAll();
				AssertNoRowMessageError("Two containers are not in CNT mode and all pivots are unselected.", nctsPackage, RuleC0670MessageError);

				container1.BC_Mode = Core.Constants.ContainerModes.Containerised;
				nctsPackage.Validation.ValidateAll();
				AssertNoRowMessageError("One container is in CNT mode and all pivots are unselected.", nctsPackage, RuleC0670MessageError);

				container2.BC_Mode = Core.Constants.ContainerModes.Containerised;
				nctsPackage.Validation.ValidateAll();
				AssertHasRowMessageError("Two containers are in CNT mode and all pivots are unselected.", nctsPackage, RuleC0670MessageError);

				pivot1.ContainerSelected = true;
				nctsPackage.Validation.ValidateAll();
				AssertNoRowMessageError("Two containers are in CNT mode and only the first pivot is selected.", nctsPackage, RuleC0670MessageError);

				pivot1.ContainerSelected = false;
				pivot2.ContainerSelected = true;
				nctsPackage.Validation.ValidateAll();
				AssertNoRowMessageError("Two containers are in CNT mode and only the second pivot is selected.", nctsPackage, RuleC0670MessageError);

				pivot1.ContainerSelected = true;
				nctsPackage.Validation.ValidateAll();
				AssertNoRowMessageError("Two containers are in CNT mode and two pivots are selected.", nctsPackage, RuleC0670MessageError);
			});
		}

		public void TestRuleB1819() => CombineAssertions(() =>
		{
			const string error = "[R0219] Either all pack quantity should be equal to ‘0’ Or all pack quantity should be greater than ‘0’.";

			using var validationDeciderContext = nctsPackage.CreateDeparturePhase5ValidationTestContext();
			validationDeciderContext.EnableRule(c => c.IsRuleB1819Active);
			validationDeciderContext.EnableRule(c => c.IsRuleR0219Active);

			nctsPackage.B5_UnitCount = 0;
			var secondPackage = goodsItem.Packages.AddNew();
			secondPackage.B5_UnitCount = 1;

			validationDeciderContext.SetNctsTransitionPeriod(true);
			AssertCase("B1819 active, inside the transition period", errorExpected: false);

			validationDeciderContext.SetNctsTransitionPeriod(false);
			AssertCase("B1819 active, outside of the transition period", errorExpected: true);

			validationDeciderContext.DisableRule(c => c.IsRuleB1819Active);

			validationDeciderContext.SetNctsTransitionPeriod(true);
			AssertCase("B1819 inactive, inside the transition period", errorExpected: true);

			validationDeciderContext.SetNctsTransitionPeriod(false);
			AssertCase("B1819 inactive, outside of the transition period", errorExpected: true);

			void AssertCase(string caseDescription, bool errorExpected)
			{
				nctsPackage.Validation.ValidateB5_UnitCount();
				secondPackage.Validation.ValidateB5_UnitCount();

				if (errorExpected)
				{
					AssertHasMessageError(caseDescription + ", first package", nctsPackage.B5_UnitCountInfo, error);
					AssertHasMessageError(caseDescription + ", second package", secondPackage.B5_UnitCountInfo, error);
				}
				else
				{
					AssertNoMessageError(caseDescription + ", first package", nctsPackage.B5_UnitCountInfo, error);
					AssertNoMessageError(caseDescription + ", second package", secondPackage.B5_UnitCountInfo, error);
				}
			}
		});

		public void TestRuleR0219() => CombineAssertions(() =>
		{
			const string error = "[R0219] Either all pack quantity should be equal to ‘0’ Or all pack quantity should be greater than ‘0’.";

			using var validationDeciderContext = nctsPackage.CreateDeparturePhase5ValidationTestContext();
			validationDeciderContext.EnableRule(c => c.IsRuleR0219Active);
			nctsPackage.B5_UnitCount = 0;
			AssertNoMessageError("1 package, zero count, R0219 active", nctsPackage.B5_UnitCountInfo, error);

			var secondNctsPackage = goodsItem.Packages.AddNew();
			secondNctsPackage.B5_UnitCount = 0;
			nctsPackage.Validation.ValidateB5_UnitCount();
			secondNctsPackage.Validation.ValidateB5_UnitCount();
			AssertNoMessageError("2 packages with zero count, R0219 active. First package error message check", nctsPackage.B5_UnitCountInfo, error);
			AssertNoMessageError("2 packages with zero count, R0219 active. Second package error message check", secondNctsPackage.B5_UnitCountInfo, error);

			secondNctsPackage.B5_UnitCount = 1;
			nctsPackage.Validation.ValidateB5_UnitCount();
			secondNctsPackage.Validation.ValidateB5_UnitCount();
			AssertHasMessageError("Mixed package counts (0 and 1), R0219 active. First package error message check", nctsPackage.B5_UnitCountInfo, error);
			AssertHasMessageError("Mixed package counts (0 and 1), R0219 active. Second package error message check", secondNctsPackage.B5_UnitCountInfo, error);

			nctsPackage.B5_UnitCount = 2;
			secondNctsPackage.B5_UnitCount = 3;
			nctsPackage.Validation.ValidateB5_UnitCount();
			secondNctsPackage.Validation.ValidateB5_UnitCount();
			AssertNoMessageError("2 packages with non-zero count, R0219 active. First package error message check", nctsPackage.B5_UnitCountInfo, error);
			AssertNoMessageError("2 packages with non-zero count, R0219 active. Second package error message check", secondNctsPackage.B5_UnitCountInfo, error);

			validationDeciderContext.DisableRule(c => c.IsRuleR0219Active);
			nctsPackage.B5_UnitCount = 0;
			secondNctsPackage.B5_UnitCount = 1;
			nctsPackage.Validation.ValidateB5_UnitCount();
			secondNctsPackage.Validation.ValidateB5_UnitCount();
			AssertNoMessageError("Mixed package counts (0 and 1), R0219 inactive. First package error message check", nctsPackage.B5_UnitCountInfo, error);
			AssertNoMessageError("Mixed package counts (0 and 1), R0219 inactive. Second package error message check", secondNctsPackage.B5_UnitCountInfo, error);
		});

		public void TestRuleB1919()
		{
			const string error = "[R0220] Package Type is not valid when number of packages is zero '0'.";
			using var ruleContext = nctsPackage.CreatePhase5ValidationTestContext();

			var unpackCusCode = Factory.SetupUnpackCusCode();

			var nctsPackageOnDepartureMovement = nctsPackage;
			AssertRuleB1919(nctsPackageOnDepartureMovement);

			var nctsPackageOnArrivalMovement = GetTestNctsPackageForArrivalMovement();
			AssertRuleB1919(nctsPackageOnArrivalMovement);

			void AssertRuleB1919(NctsPackage nctsPackage) => CombineAssertions(() =>
			{
				nctsPackage.B5_UnitCount = 0;
				nctsPackage.B5_UnitType = unpackCusCode;

				ruleContext.EnableRule(x => x.IsRuleB1919Active);
				ruleContext.EnableRule(x => x.IsRuleR0220Active);

				ruleContext.SetNctsTransitionPeriod(true);
				AssertCase("B1919 active, inside the transition period", errorMessageExpected: false);

				ruleContext.SetNctsTransitionPeriod(false);
				AssertCase("B1919 active, outside of the transition period", errorMessageExpected: true);

				ruleContext.DisableRule(x => x.IsRuleB1919Active);

				ruleContext.SetNctsTransitionPeriod(true);
				AssertCase("B1919 inactive, inside the transition period", errorMessageExpected: true);

				ruleContext.SetNctsTransitionPeriod(false);
				AssertCase("B1919 inactive, outside of the transition period", errorMessageExpected: true);

				void AssertCase(string caseDescription, bool errorMessageExpected)
				{
					nctsPackage.Validation.ValidateB5_UnitType();
					if (errorMessageExpected)
					{
						AssertHasMessageError(caseDescription, nctsPackage.B5_UnitTypeInfo, error);
					}
					else
					{
						AssertNoMessageError(caseDescription, nctsPackage.B5_UnitTypeInfo, error);
					}
				}
			});
		}

		public void TestRuleR0220()
		{
			const string error = "[R0220] Package Type is not valid when number of packages is zero '0'.";

			var standardPackCusCode = Factory.SetupStandardPackCusCode();
			var unpackCusCode = Factory.SetupUnpackCusCode();

			var nctsPackageOnDepartureMovement = nctsPackage;
			AssertRuleR0220(nctsPackageOnDepartureMovement);

			var nctsPackageOnArrivalMovement = GetTestNctsPackageForArrivalMovement();
			AssertRuleR0220(nctsPackageOnArrivalMovement);

			void AssertRuleR0220(NctsPackage nctsPackage) => CombineAssertions(() =>
			{
				using var ruleContext = nctsPackage.CreatePhase5ValidationTestContext();

				ruleContext.EnableRule(x => x.IsRuleR0220Active);
				nctsPackage.B5_UnitCount = 0;
				nctsPackage.B5_UnitType = unpackCusCode;
				AssertHasMessageError("Zero count, unpacked, R0220 active", nctsPackage.B5_UnitTypeInfo, error);

				nctsPackage.B5_UnitCount = 1;
				nctsPackage.Validation.ValidateB5_UnitType();
				AssertNoMessageError("Non-zero count, unpacked, R0220 active", nctsPackage.B5_UnitTypeInfo, error);

				nctsPackage.B5_UnitCount = 0;
				nctsPackage.B5_UnitType = standardPackCusCode;
				AssertNoMessageError("Zero count, packed, R0220 active", nctsPackage.B5_UnitTypeInfo, error);

				ruleContext.DisableRule(x => x.IsRuleR0220Active);
				nctsPackage.B5_UnitCount = 0;
				nctsPackage.B5_UnitType = unpackCusCode;
				AssertNoMessageError("Zero count, unpacked, R0220 inactive", nctsPackage.B5_UnitTypeInfo, error);
			});
		}

		public void TestCheckB5_UnitCount_NR0027Rule() => CombineAssertions(() =>
		{
			var errorMessage = "[NR0027] Number of packages can be 0 on all the package lines of the Goods Item or on none.";

			using var ruleContext = nctsPackage.CreateDeparturePhase5ValidationTestContext();
			ruleContext.EnableRule(x => x.IsRuleNR0027Active);

			nctsPackage.B5_UnitCount = 0;
			var secondNctsPackage = goodsItem.Packages.AddNew();
			secondNctsPackage.B5_UnitCount = 0;
			goodsItem.Packages.AddNew().B5_UnitCount = 0;
			nctsPackage.Validation.ValidateB5_UnitCount();
			AssertNoMessageError("If B5_UnitCount is 0 on all nctsPackage, then no error message should be displayed", nctsPackage.B5_UnitCountInfo, errorMessage);
			AssertNoMessageError("If B5_UnitCount is 0 on all nctsPackage, then no error message should be displayed", secondNctsPackage.B5_UnitCountInfo, errorMessage);

			secondNctsPackage.B5_UnitCount = 1;
			nctsPackage.Validation.ValidateB5_UnitCount();
			AssertNoMessageError("If B5_UnitCount is 0, then no error message should be displayed", nctsPackage.B5_UnitCountInfo, errorMessage);
			AssertHasMessageError("If B5_UnitCount is not 0 but a different B5_UnitCount is, then the error message should be displayed", secondNctsPackage.B5_UnitCountInfo, errorMessage);

			secondNctsPackage.B5_UnitCount = 0;
			nctsPackage.B5_UnitCount = 1;
			nctsPackage.Validation.ValidateB5_UnitCount();
			secondNctsPackage.Validation.ValidateB5_UnitCount();
			AssertHasMessageError("If B5_UnitCount is not 0 but a different B5_UnitCount is, then the error message should be displayed", nctsPackage.B5_UnitCountInfo, errorMessage);
			AssertNoMessageError("If B5_UnitCount is 0, then no error message should be displayed", secondNctsPackage.B5_UnitCountInfo, errorMessage);

			ruleContext.DisableRule(x => x.IsRuleNR0027Active);

			nctsPackage.B5_UnitCount = 0;
			secondNctsPackage.B5_UnitCount = 0;
			nctsPackage.Validation.ValidateB5_UnitCount();
			AssertNoMessageError("Rule disabled", nctsPackage.B5_UnitCountInfo, errorMessage);

			secondNctsPackage.B5_UnitCount = 1;
			nctsPackage.Validation.ValidateB5_UnitCount();
			AssertNoMessageError("Rule disabled", nctsPackage.B5_UnitCountInfo, errorMessage);

			secondNctsPackage.B5_UnitCount = 0;
			nctsPackage.B5_UnitCount = 1;
			nctsPackage.Validation.ValidateB5_UnitCount();
			AssertNoMessageError("Rule disabled", nctsPackage.B5_UnitCountInfo, errorMessage);
		});

		NctsPackage GetTestNctsPackageForArrivalMovement()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			return nctsHeader.Bills.AddNew().ArrivalGoodsItems.AddNew().Packages.AddNew();
		}

		public void TestCheckB5_UnitType_ConditionNR0029()
		{
			using var ruleContext = nctsPackage.CreateArrivalPhase5ValidationTestContext();
			ruleContext.EnableRule(x => x.IsRuleNR0029Active);

			const string errorMessage = "[NR0029] If Package Unloaded State is NEW, Package Type cannot be empty.";

			CombineAssertions(() =>
			{
				nctsPackage.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;
				nctsPackage.B5_UnitType = ZString.Empty;
				nctsPackage.Validation.ValidateB5_UnitType();
				AssertHasMessageError("If B5_TypeOfDifference is NEW and B5_UnitType is empty, then error NR0029 should be displayed.",
					nctsPackage.B5_UnitTypeInfo, errorMessage);

				nctsPackage.B5_TypeOfDifference = NctsUnloadedStateList.Codes.MIS;
				nctsPackage.Validation.ValidateB5_UnitType();
				AssertNoMessageError("If B5_TypeOfDifference is MIS and B5_UnitType is empty, then error NR0029 should not be displayed.",
					nctsPackage.B5_UnitTypeInfo, errorMessage);

				nctsPackage.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;
				nctsPackage.B5_UnitType = Factory.SetupStandardPackCusCode();
				nctsPackage.Validation.ValidateB5_UnitType();
				AssertNoMessageError("If B5_TypeOfDifference is NEW and B5_UnitType is not empty, then error NR0029 should not be displayed.",
					nctsPackage.B5_UnitTypeInfo, errorMessage);

				nctsPackage.B5_TypeOfDifference = NctsUnloadedStateList.Codes.MIS;
				nctsPackage.Validation.ValidateB5_UnitType();
				AssertNoMessageError("If B5_TypeOfDifference is MIS and B5_UnitType is not empty, then error NR0029 should not be displayed.",
					nctsPackage.B5_UnitTypeInfo, errorMessage);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			var nctsBill = nctsHeader.Bills.AddNew();
			goodsItem = nctsBill.GoodsItems.AddNew();
			nctsPackage = goodsItem.Packages.AddNew();
		}

		IDisposable TemporarilySetTransitionPeriod(bool isTransitionPeriodActive) => ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, isTransitionPeriodActive);

		NctsPackage nctsPackage;
		NctsHeader nctsHeader;
		NctsDepartureCargoDesc goodsItem;

		ValidationRuleMessages ValidationRuleMessages => (validationRuleMessages ?? new ValidationRuleMessages());
		readonly ValidationRuleMessages validationRuleMessages;

		const string RuleC0670MessageError = "[C0670] You have not selected at least one Container Number for this package.";
	}
}
