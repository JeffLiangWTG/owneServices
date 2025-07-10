using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsArrivalHeaderContainerPhase5ValidationTest : TestCaseWithFactory
	{
		public void TestConstructor() => CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentException>("parent: null", () => new NctsArrivalHeaderContainerPhase5Validation(parent: null));
			AssertNoExceptionThrown("correct headerContainer", () => new NctsArrivalHeaderContainerPhase5Validation(headerContainer));
		});

		public void TestCheckContainerNumberDuplicateError()
		{
			using (var deciderTestContext = new NctsContainerValidationDeciderTestContext<INctsArrivalHeaderContainerPhase5ValidationDecider>(Factory))
			{
				const string expectedError = "[TR0044] Duplicate Container Number is entered.";
				var header = Factory.New<NctsHeader>();
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				var headerContainerFirst = header.ArrivalHeaderContainers.AddNew();
				var headerContainerSecond = header.ArrivalHeaderContainers.AddNew();

				CombineAssertions(() =>
				{
					deciderTestContext.EnableRule(c => c.IsRuleTR0044Active);
					headerContainerFirst.BC_Mode = Core.Constants.ContainerModes.Containerised;
					headerContainerFirst.BC_ContainerNum = "1";
					headerContainerSecond.BC_Mode = Core.Constants.ContainerModes.Containerised;
					headerContainerSecond.BC_ContainerNum = "1";
					AssertHasMessageError("Duplicate Container Number", headerContainerSecond.BC_ContainerNumInfo, expectedError);

					headerContainerSecond.BC_ContainerNum = "2";
					AssertNoMessageError("Container Number is different", headerContainerSecond.BC_ContainerNumInfo, expectedError);

					headerContainerSecond.BC_Mode = Core.Constants.ContainerModes.NonContainerised;
					headerContainerSecond.BC_ContainerNum = "1";
					AssertNoMessageError("BC_Mode is not CNT", headerContainerSecond.BC_ContainerNumInfo, expectedError);

					headerContainerSecond.BC_ContainerNum = "2";
					deciderTestContext.DisableRule(c => c.IsRuleTR0044Active);
					headerContainerSecond.BC_Mode = Core.Constants.ContainerModes.Containerised;
					headerContainerSecond.BC_ContainerNum = "1";
					AssertNoMessageError("Duplicate Container Number but rule TR0044 is inactive", headerContainerSecond.BC_ContainerNumInfo, expectedError);
				});
			}
		}

		public void TestCheckBC_Mode_Entered()
		{
			var expectError = "[TR0043] Please enter a Container/Equipment Mode.";
			using (var deciderTestContext = new NctsContainerValidationDeciderTestContext<INctsArrivalHeaderContainerPhase5ValidationDecider>(Factory))
			{
				CombineAssertions(() =>
				{
					deciderTestContext.EnableRule(c => c.IsRuleTR0043Active);
					AssertNoError("Default", headerContainer.BC_ModeInfo, expectError);

					headerContainer.BC_ContainerNum = "1";
					ValidationTestHelper.AssertErrorIfNotEntered(headerContainer.BC_ModeInfo, expectError);

					headerContainer.BC_ContainerNum = ZString.Empty;
					headerContainer.Seals.AddNew().BK_SealNumber = "1";
					ValidationTestHelper.AssertErrorIfNotEntered(headerContainer.BC_ModeInfo, expectError);

					headerContainer.BC_Mode = "CNT";
					AssertNoError("BC_Mode is not empty", headerContainer.BC_ModeInfo, expectError);

					deciderTestContext.DisableRule(c => c.IsRuleTR0043Active);
					headerContainer.BC_Mode = ZString.Empty;
					AssertNoError("BC_Mode is empty but rule TR0043 is inactive", headerContainer.BC_ModeInfo, expectError);
				});
			}
		}

		public void TestCheckBC_Mode_RuleTR0046()
		{
			var expectError = "[TR0046] You have selected Both Mode 'CNT' and 'NCT'";
			using (var deciderTestContext = new NctsContainerValidationDeciderTestContext<INctsArrivalHeaderContainerPhase5ValidationDecider>(Factory))
			{
				deciderTestContext.EnableRule(c => c.IsRuleTR0046Active);
				var header = Factory.New<NctsHeader>();
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				var headerContainerFirst = header.ArrivalHeaderContainers.AddNew();
				var headerContainerSecond = header.ArrivalHeaderContainers.AddNew();
				headerContainerSecond.BC_ContainerNum = "2";

				CombineAssertions(() =>
				{
					headerContainerFirst.BC_Mode = Constants.ContainerModes.Containerised;
					headerContainerSecond.BC_Mode = Constants.ContainerModes.NonContainerised;
					AssertHasWarning("Has warning when different", headerContainerSecond.BC_ModeInfo, expectError);
					headerContainerSecond.BC_Mode = Constants.ContainerModes.Containerised;
					AssertNoWarning("Has no warning when same", headerContainerSecond.BC_ModeInfo, expectError);

					deciderTestContext.DisableRule(c => c.IsRuleTR0046Active);
					headerContainerSecond.BC_Mode = Constants.ContainerModes.NonContainerised;
					AssertNoWarning("Has no warning when different and inactive rule TR0046", headerContainerSecond.BC_ModeInfo, expectError);
				});
			}
		}

		public void TestCheckBC_UnloadedState()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var headerContainer = nctsHeader.ArrivalHeaderContainers.AddNew();
			headerContainer.BC_Mode = Constants.ContainerModes.Containerised;
			headerContainer.BC_ContainerNum = "MSCU1234567";

			CombineAssertions(() =>
			{
				headerContainer.BC_UnloadedState = ZString.Empty;
				AssertHasErrorContaining("Unloaded State is empty", headerContainer.BC_UnloadedStateInfo, MandatoryValidation.MustBeEntered);

				headerContainer.BC_UnloadedState = NctsUnloadedStateList.Codes.NEW;
				AssertNoErrorContaining("Unloaded State is entered (NEW)", headerContainer.BC_UnloadedStateInfo, MandatoryValidation.MustBeEntered);
			});
		}

		public void TestCheckBC_UnloadedState_ConditionNR0029()
		{
			using (var deciderTestContext = new NctsContainerValidationDeciderTestContext<INctsArrivalHeaderContainerPhase5ValidationDecider>(Factory))
			{
				deciderTestContext.EnableRule(c => c.IsRuleNR0029Active);

				const string errorMessage = "[NR0029] If Container Unloaded State is NEW, at least one Seal is required.";

				var header = Factory.New<NctsHeader>();
				header.SetMovementType(NctsMovementType.Codes.Arrival);
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				headerContainer = header.ArrivalHeaderContainers.AddNew();

				CombineAssertions(() =>
				{
					headerContainer.BC_UnloadedState = NctsUnloadedStateList.Codes.NEW;
					headerContainer.Validation.ValidateBC_UnloadedState();
					AssertHasMessageError("BC_UnloadedState is set to NEW and the are no Seals, the error message NR0029 should be displayed",
						headerContainer.BC_UnloadedStateInfo, errorMessage);

					headerContainer.BC_UnloadedState = NctsUnloadedStateList.Codes.MIS;
					headerContainer.Validation.ValidateBC_UnloadedState();
					AssertNoMessageError("BC_UnloadedState is set to MIS and the are no Seals, the error message NR0029 should not be displayed",
						headerContainer.BC_UnloadedStateInfo, errorMessage);

					headerContainer.BC_UnloadedState = NctsUnloadedStateList.Codes.NEW;
					headerContainer.Seals.AddNew();
					headerContainer.Validation.ValidateBC_UnloadedState();
					AssertNoMessageError("BC_UnloadedState is set to NEW and the is one Seals, the error message NR0029 should not be displayed",
						headerContainer.BC_UnloadedStateInfo, errorMessage);

					headerContainer.BC_UnloadedState = NctsUnloadedStateList.Codes.MIS;
					headerContainer.Validation.ValidateBC_UnloadedState();
					AssertNoMessageError("BC_UnloadedState is set to MIS and the are one Seals, the error message NR0029 should not be displayed",
						headerContainer.BC_UnloadedStateInfo, errorMessage);
				});
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			headerContainer = header.ArrivalHeaderContainers.AddNew();
		}

		NctsArrivalHeaderContainer headerContainer;
	}
}
