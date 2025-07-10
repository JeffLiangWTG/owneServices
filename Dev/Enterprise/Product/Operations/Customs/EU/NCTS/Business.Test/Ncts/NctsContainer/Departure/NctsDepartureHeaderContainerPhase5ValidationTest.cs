using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal;
using static Enterprise.Customs.EU.NCTS.Business.NctsConstants;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsDepartureHeaderContainerPhase5ValidationTest : TestCaseWithFactory
	{
		public void TestConstructor() => CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentException>("parent: null", () => new NctsDepartureHeaderContainerPhase5Validation(parent: null));
			AssertNoExceptionThrown("correct headerContainer", () => new NctsDepartureHeaderContainerPhase5Validation(headerContainer));
		});

		public void TestContainerNumIsMandatoryWhenModeIsCntPhase5()
		{
			headerContainer.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			AssertEquals("Precondition: Phase5", headerContainer.Header.IsPhase5, true);
			CombineAssertions(() =>
			{
				headerContainer.BC_Mode = Core.Constants.ContainerModes.Containerised;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(headerContainer.BC_ContainerNumInfo);

				headerContainer.BC_Mode = Core.Constants.ContainerModes.NonContainerised;
				ValidationTestHelper.AssertFieldIsNotMandatory(headerContainer.BC_ContainerNumInfo);
			});
		}

		public void TestCheckContainerNumberInvalidCheckDigitWarningPhase5()
		{
			headerContainer.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			AssertEquals("Precondition: Phase5", headerContainer.Header.IsPhase5, true);
			const string warningMessage = "Container number does not have a valid check (last) digit. The check digit should be";
			headerContainer.BC_Mode = Core.Constants.ContainerModes.Containerised;
			CombineAssertions(() =>
			{
				headerContainer.BC_ContainerNum = "CRXU1234561";
				AssertHasWarningContaining("BC_Mode CNT, BC_ContainerNum doesn't have valid check digit", headerContainer.BC_ContainerNumInfo, warningMessage);

				headerContainer.BC_ContainerNum = "BICU1234565";
				AssertNoWarningContaining("BC_Mode CNT, BC_ContainerNum has valid check digit", headerContainer.BC_ContainerNumInfo, warningMessage);

				headerContainer.BC_Mode = Core.Constants.ContainerModes.NonContainerised;
				headerContainer.BC_ContainerNum = "CRXU1234561";
				AssertNoWarningContaining("BC_Mode NCT, BC_ContainerNum doesn't have valid check digit", headerContainer.BC_ContainerNumInfo, warningMessage);
			});
		}

		public void TestCheckContainerNumberDoesNotConformToISOStandardWarningPhase5()
		{
			headerContainer.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			AssertEquals("Precondition: Phase5", headerContainer.Header.IsPhase5, true);
			const string warningMessage = "Container number does not conform to ISO standard of 4 letters followed by 6 digits and a check digit.";
			CombineAssertions(() =>
			{
				headerContainer.BC_Mode = Core.Constants.ContainerModes.Containerised;
				headerContainer.BC_ContainerNum = "ABC12345DE";
				AssertHasWarningContaining("BC_Mode CNT, BC_ContainerNum doesn't conform to ISO", headerContainer.BC_ContainerNumInfo, warningMessage);

				headerContainer.BC_ContainerNum = "BICU1234565";
				AssertNoWarningContaining("BC_Mode CNT, BC_ContainerNum conforms to ISO", headerContainer.BC_ContainerNumInfo, warningMessage);

				headerContainer.BC_Mode = Core.Constants.ContainerModes.NonContainerised;
				headerContainer.BC_ContainerNum = "ABC12345DE";
				AssertNoWarningContaining("BC_Mode NCT, BC_ContainerNum doesn't conform to ISO", headerContainer.BC_ContainerNumInfo, warningMessage);
			});
		}

		public void TestCheckContainerNumIsDuplicate()
		{
			var expectedError = "[TR0044] Duplicate Container Number is entered.";
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var container1 = header.DepartureHeaderContainers.AddNew();
			container1.BC_ContainerNum = "MSKU1234565";
			container1.BC_Mode = Core.Constants.ContainerModes.NonContainerised;

			CombineAssertions(() =>
			{
				using var ruleTestContext = new NctsContainerValidationDeciderTestContext<INctsDepartureHeaderContainerPhase5ValidationDecider>(Factory);
				ruleTestContext.EnableRule(x => x.IsRuleTR0044Active);
				var container2 = header.DepartureHeaderContainers.AddNew();
				container2.BC_ContainerNum = "MSKU1234565";
				AssertNoMessageError("Non Containerised Mode, no error", container2.BC_ContainerNumInfo, expectedError);

				container1.BC_Mode = Core.Constants.ContainerModes.Containerised;
				container2.BC_Mode = Core.Constants.ContainerModes.Containerised;
				container2.Validation.ValidateBC_ContainerNum();
				AssertHasMessageError("Containerised Mode, Duplicate container number", container2.BC_ContainerNumInfo, expectedError);

				container2.BC_ContainerNum = "MSKU1234570";
				AssertNoMessageError("New container number", container2.BC_ContainerNumInfo, expectedError);

				ruleTestContext.DisableRule(x => x.IsRuleTR0044Active);
				container2.BC_ContainerNum = "MSKU1234565";
				AssertNoMessageError("Have Containerised Mode and Duplicate container number but rule TR0044 is inactive", container2.BC_ContainerNumInfo, expectedError);
			});
		}

		public void TestCheckC0055() => CombineAssertions(() =>
		{
			const string warningMessage = "[C0055] Container/Equipment Number not required when Container Mode is NCT.";

			using var ruleTestContext = new NctsContainerValidationDeciderTestContext<INctsDepartureHeaderContainerPhase5ValidationDecider>(Factory);
			header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			headerContainer = header.DepartureHeaderContainers.AddNew();

			ruleTestContext.EnableRule(decider => decider.IsRuleC0055Active);
			headerContainer.BC_Mode = Core.Constants.ContainerModes.NonContainerised;
			headerContainer.BC_ContainerNum = "CRXU1234561";
			AssertHasWarning("NonContainerised, not empty number", headerContainer.BC_ContainerNumInfo, warningMessage);

			ruleTestContext.DisableRule(decider => decider.IsRuleC0055Active);
			headerContainer.Validation.ValidateBC_ContainerNum();
			AssertNoWarning("NonContainerised, not empty number", headerContainer.BC_ContainerNumInfo, warningMessage);
			ruleTestContext.EnableRule(decider => decider.IsRuleC0055Active);

			headerContainer.BC_ContainerNum = CusInBondContainer.NonContainerizedNumber;
			AssertNoWarning("NonContainerised, NonContainerizedNumber", headerContainer.BC_ContainerNumInfo, warningMessage);

			headerContainer.BC_ContainerNum = ZString.Empty;
			AssertNoWarning("NonContainerised, NonContainerizedNumber", headerContainer.BC_ContainerNumInfo, warningMessage);

			headerContainer.BC_Mode = Core.Constants.ContainerModes.Containerised;
			headerContainer.BC_ContainerNum = "CRXU1234561";
			AssertNoWarning("NonContainerised, not empty number", headerContainer.BC_ContainerNumInfo, warningMessage);
		});

		public void TestCheckBC_Mode_RuleTR0046()
		{
			var warning = "[TR0046] You have selected Both Mode 'CNT' and 'NCT'";

			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var firstContainer = header.DepartureHeaderContainers.AddNew();
			firstContainer.BC_Mode = Core.Constants.ContainerModes.Containerised;
			var secondContainer = header.DepartureHeaderContainers.AddNew();

			using var deciderTestContext = new NctsContainerValidationDeciderTestContext<INctsDepartureHeaderContainerPhase5ValidationDecider>(Factory);
			CombineAssertions(() =>
			{
				secondContainer.ClearAllCachedValues();
				deciderTestContext.EnableRule(d => d.IsRuleTR0046Active);
				secondContainer.BC_Mode = Core.Constants.ContainerModes.NonContainerised;
				AssertHasWarning("Has warning when different", secondContainer.BC_ModeInfo, warning);
				secondContainer.BC_Mode = Core.Constants.ContainerModes.Containerised;
				AssertNoWarning("Has no warning when same", secondContainer.BC_ModeInfo, warning);

				deciderTestContext.DisableRule(d => d.IsRuleTR0046Active);
				secondContainer.BC_Mode = Core.Constants.ContainerModes.NonContainerised;
				AssertNoWarning("Has no warning when different and inactive TR0046 rule", secondContainer.BC_ModeInfo, warning);
			});
		}

		public void TestBcModeListValidationPhase5()
		{
			var messageError = "[TR0043] You have not entered a Container/Equipment Mode.";
			headerContainer.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			AssertEquals("Precondition: Phase5", headerContainer.Header.IsPhase5, true);
			CombineAssertions(() =>
			{
				using var ruleTestContext = new NctsContainerValidationDeciderTestContext<INctsDepartureHeaderContainerPhase5ValidationDecider>(Factory);
				ruleTestContext.EnableRule(decider => decider.IsRuleTR0043Active);
				ValidationTestHelper.AssertErrorIfInvalidCode(headerContainer.BC_ModeInfo, "XYZ", Core.Constants.ContainerModes.Containerised);
				headerContainer.BC_ContainerNum = "123";
				headerContainer.BC_Mode = ZString.Empty;
				AssertHasMessageError("Error when mode is empty", headerContainer.BC_ModeInfo, messageError);
				headerContainer.BC_Mode = "CNT";
				AssertNoMessageError("No error when mode is not empty", headerContainer.BC_ModeInfo, messageError);
				ruleTestContext.DisableRule(decider => decider.IsRuleTR0043Active);
				headerContainer.BC_Mode = ZString.Empty;
				AssertNoMessageError("No Error when mode is empty and rule TR0043 is inactive", headerContainer.BC_ModeInfo, messageError);
			});
		}

		public void TestCheckTotalSealCount_Phase5_R0448()
		{
			using var ruleTestContext = new NctsContainerValidationDeciderTestContext<INctsDepartureHeaderContainerPhase5ValidationDecider>(Factory);
			const string error = "[R0448] Number of Seals can't be 0 if Container Identification Number is empty.";
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var container = header.DepartureHeaderContainers.AddNew();
			CombineAssertions(() =>
			{
				ruleTestContext.EnableRule(decider => decider.IsRuleR0448Active);
				container.Validation.ValidateTotalSealCount();
				AssertHasMessageError($"Container Number is empty and Total Seal Count is {container.TotalSealCount}", container.TotalSealCountInfo, error);

				container.BC_Seal1 = "SEAL1";
				container.Validation.ValidateTotalSealCount();
				AssertNoMessageError($"Container Number is empty and Total Seal Count is {container.TotalSealCount}", container.TotalSealCountInfo, error);

				container.BC_ContainerNum = "MSKU1234570";
				container.Validation.ValidateTotalSealCount();
				AssertNoMessageError($"Container Number is not empty and Total Seal Count is {container.TotalSealCount}", container.TotalSealCountInfo, error);

				container.BC_Seal1 = ZString.Empty;
				container.Validation.ValidateTotalSealCount();
				AssertNoMessageError($"Container Number is not empty and Total Seal Count is {container.TotalSealCount}", container.TotalSealCountInfo, error);

				ruleTestContext.DisableRule(decider => decider.IsRuleR0448Active);
				container.BC_ContainerNum = ZString.Empty;
				container.Validation.ValidateTotalSealCount();
				AssertNoMessageError($"Container Number is empty and Total Seal Count is {container.TotalSealCount}", container.TotalSealCountInfo, error);
			});
		}

		public void TestCheckSeal1IsDuplicate()
		{
			var expectedWarningActiveRule = "[TR0045] Duplicate Seal 1 Number entered.";
			using var ruleContext = new NctsContainerValidationDeciderTestContext<INctsDepartureHeaderContainerPhase5ValidationDecider>(Factory);

			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var container1 = header.DepartureHeaderContainers.AddNew();
			container1.Seal1 = "123";
			container1.Seal2 = "456";
			container1.AdditionalSeals.AddNew().BK_SealNumber = "789";

			var container2 = header.DepartureHeaderContainers.AddNew();
			CombineAssertions(() =>
			{
				ruleContext.EnableRule(x => x.IsRuleTR0045Active);
				container2.Seal1 = "123";
				AssertHasWarning("Duplicate of container1.Seal1", container2.Seal1Info, expectedWarningActiveRule);

				container2.Seal1 = "456";
				AssertHasWarning("Duplicate of container1.Seal2", container2.Seal1Info, expectedWarningActiveRule);

				container2.Seal1 = "789";
				AssertHasWarning("Duplicate in container1.AdditionalSeals", container2.Seal1Info, expectedWarningActiveRule);

				container2.Seal1 = "ABC";
				AssertNoWarning("New Seal1 number", container2.Seal1Info, expectedWarningActiveRule);

				ruleContext.DisableRule(x => x.IsRuleTR0045Active);
				container2.Seal1 = "123";
				AssertNoWarning("Duplicate of container1.Seal1 but rule TR0045 is inactive", container2.Seal1Info, expectedWarningActiveRule);
			});
		}

		public void TestCheckSeal2IsDuplicate()
		{
			var expectedWarningActiveRule = "[TR0045] Duplicate Seal 2 Number entered.";
			using var ruleContext = new NctsContainerValidationDeciderTestContext<INctsDepartureHeaderContainerPhase5ValidationDecider>(Factory);

			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var container1 = header.DepartureHeaderContainers.AddNew();
			container1.Seal1 = "123";
			container1.Seal2 = "456";
			container1.AdditionalSeals.AddNew().BK_SealNumber = "789";

			var container2 = header.DepartureHeaderContainers.AddNew();
			container2.Seal1 = "XYZ";
			CombineAssertions(() =>
			{
				ruleContext.EnableRule(x => x.IsRuleTR0045Active);
				container2.Seal2 = "XYZ";
				AssertHasWarning("Duplicate of Seal1", container2.Seal2Info, expectedWarningActiveRule);

				container2.Seal2 = "123";
				AssertHasWarning("Duplicate of container1.Seal1", container2.Seal2Info, expectedWarningActiveRule);

				container2.Seal2 = "456";
				AssertHasWarning("Duplicate of container1.Seal2", container2.Seal2Info, expectedWarningActiveRule);

				container2.Seal2 = "789";
				AssertHasWarning("Duplicate in container1.AdditionalSeals", container2.Seal2Info, expectedWarningActiveRule);

				container2.Seal2 = "ABC";
				AssertNoWarning("New Seal2 number", container2.Seal2Info, expectedWarningActiveRule);

				ruleContext.DisableRule(x => x.IsRuleTR0045Active);
				container2.Seal2 = "123";
				AssertNoWarning("Duplicate of container1.Seal1 but rule TR0045 is inactive", container2.Seal2Info, expectedWarningActiveRule);
			});
		}

		public void TestCheckBC_Seal_RuleN0003()
		{
			using var ruleTestContext = new NctsContainerValidationDeciderTestContext<INctsDepartureHeaderContainerPhase5ValidationDecider>(Factory);
			const string expectedErrorActiveRule = "[N0003] Supporting Document Code 67YY has been entered, therefore Seals must not be entered.";
			const string expectedErrorSeal1InActiveRule = "You have not entered a Container Seal 1.";
			const string expectedErrorSeal2InActiveRule = "You have not entered a Container Seal 2.";

			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var container1 = header.DepartureHeaderContainers.AddNew();

			header.MovementHeader.SupportingDocuments.AddNew().CSI_Code = NctsTypeOfSupportingDocument.Codes._67YY;
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: false))
			{
				CombineAssertions("When RuleN0003 is active", () =>
				{
					ruleTestContext.EnableRule(x => x.IsRuleN0003Active);
					container1.Seal1 = "123";
					AssertHasMessageError("When Seal1 is filled", container1.Seal1Info, expectedErrorActiveRule);

					container1.Seal1 = "";
					AssertNoMessageError("When Seal1 is empty", container1.Seal1Info, expectedErrorActiveRule);

					container1.Seal2 = "456";
					AssertHasMessageError("When Seal2 is filled", container1.Seal2Info, expectedErrorActiveRule);

					container1.Seal2 = "";
					AssertNoMessageError("When Seal2 is empty", container1.Seal2Info, expectedErrorActiveRule);
				});

				CombineAssertions("When RuleN0003 is not active", () =>
				{
					ruleTestContext.DisableRule(x => x.IsRuleN0003Active);
					container1.Seal1 = "123";
					container1.AdditionalSeals.AddNew().BK_SealNumber = "789";

					AssertNoMessageError("When Seal1 is filled", container1.Seal1Info, expectedErrorActiveRule);

					container1.Seal1 = "";
					AssertHasMessageError("When Seal1 is empty", container1.Seal1Info, expectedErrorSeal1InActiveRule);

					container1.Seal2 = "456";
					AssertNoMessageError("When Seal2 is filled", container1.Seal2Info, expectedErrorActiveRule);

					container1.Seal2 = "";
					AssertHasMessageError("When Seal2 is empty", container1.Seal2Info, expectedErrorSeal2InActiveRule);
				});
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: true))
			{
				CombineAssertions("When RuleN0003 is active", () =>
				{
					ruleTestContext.EnableRule(x => x.IsRuleN0003Active);

					container1.Seal1 = "123";
					AssertNoMessageError("When Seal1 is filled", container1.Seal1Info, expectedErrorActiveRule);

					container1.Seal1 = "";
					AssertNoMessageError("When Seal1 is empty", container1.Seal1Info, expectedErrorActiveRule);

					container1.Seal2 = "456";
					AssertNoMessageError("When Seal2 is filled", container1.Seal2Info, expectedErrorActiveRule);

					container1.Seal2 = "";
					AssertNoMessageError("When Seal2 is empty", container1.Seal2Info, expectedErrorActiveRule);
				});

				CombineAssertions("When RuleN0003 is not active", () =>
				{
					ruleTestContext.DisableRule(x => x.IsRuleN0003Active);
					container1.Seal1 = "123";
					AssertNoMessageError("When Seal1 is filled", container1.Seal1Info, expectedErrorActiveRule);

					container1.Seal1 = "";
					AssertNoMessageError("When Seal1 is empty", container1.Seal1Info, expectedErrorActiveRule);

					container1.Seal2 = "456";
					AssertNoMessageError("When Seal2 is filled", container1.Seal2Info, expectedErrorActiveRule);

					container1.Seal2 = "";
					AssertNoMessageError("When Seal2 is empty", container1.Seal2Info, expectedErrorActiveRule);
				});
			}
		}

		public void TestValidateAllContainers()
		{
			using var ruleTestContext = new NctsContainerValidationDeciderTestContext<INctsDepartureHeaderContainerPhase5ValidationDecider>(Factory);
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var container1 = header.DepartureHeaderContainers.AddNew();
			container1.BC_ContainerNum = "CONT1";

			var container2 = header.DepartureHeaderContainers.AddNew();
			container2.BC_ContainerNum = "CONT2";

			var bill = header.Bills.AddNew();
			var goodsItem = bill.GoodsItems.AddNew();
			var package = goodsItem.Packages.AddNew();
			package.ContainersPivot.AddPivotFor(container1);

			const string cont1MessageError = "[TR0095] The container/equipment #1 CONT1 is not assigned to any Goods Item";
			const string cont2MessageError = "[TR0095] The container/equipment #2 CONT2 is not assigned to any Goods Item";

			CombineAssertions(() =>
			{
				ruleTestContext.EnableRule(x => x.IsRuleTR0095Active);
				package.ContainersPivotsForBindingOnly[0].ContainerSelected = true;
				container1.Validation.ValidateAll();
				container2.Validation.ValidateAll();

				AssertNoRowMessageError(container1, cont1MessageError);
				AssertHasRowMessageError(container2, cont2MessageError);

				container1.ClearRowNotifications();
				container2.ClearRowNotifications();

				package.ContainersPivotsForBindingOnly[0].ContainerSelected = false;
				container1.Validation.ValidateAll();
				container2.Validation.ValidateAll();

				AssertHasRowMessageError(container1, cont1MessageError);
				AssertHasRowMessageError(container2, cont2MessageError);

				container1.ClearRowNotifications();
				container2.ClearRowNotifications();

				ruleTestContext.DisableRule(x => x.IsRuleTR0095Active);
				package.ContainersPivotsForBindingOnly[0].ContainerSelected = true;
				container1.Validation.ValidateAll();
				container2.Validation.ValidateAll();

				AssertNoRowMessageError(container1, cont1MessageError);
				AssertNoRowMessageError(container2, cont2MessageError);

				container1.ClearRowNotifications();
				container2.ClearRowNotifications();

				package.ContainersPivotsForBindingOnly[0].ContainerSelected = false;
				container1.Validation.ValidateAll();
				container2.Validation.ValidateAll();

				AssertNoRowMessageError(container1, cont1MessageError);
				AssertNoRowMessageError(container2, cont2MessageError);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			headerContainer = header.DepartureHeaderContainers.AddNew();
		}

		NctsDepartureHeaderContainer headerContainer;
		NctsHeader header;
	}
}
