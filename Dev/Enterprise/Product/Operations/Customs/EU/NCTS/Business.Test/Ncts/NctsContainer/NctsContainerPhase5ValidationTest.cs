using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsContainerPhase5ValidationTest : TestCaseWithFactory
	{
		public void TestConstructor() => CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentException>("parent: null", () => new NctsContainerPhase5Validation(parent: null));
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var incident = header.EnRouteIncidents.AddNew();
			var container = incident.IncidentContainers.AddNew();
			AssertNoExceptionThrown("correct headerContainer", () => new NctsContainerPhase5Validation(container));
		});

		public void TestCheckBC_Mode_RuleTR0043()
		{
			var expectedError = "[TR0043] Please enter a Container/Equipment Mode.";

			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var incident = header.EnRouteIncidents.AddNew();
			var container = incident.IncidentContainers.AddNew();
			container.BC_ContainerNum = "12";

			CombineAssertions(() =>
			{
				using var ruleTestContext = new NctsContainerValidationDeciderTestContext<INctsContainerPhase5ValidationDecider>(Factory);
				ruleTestContext.EnableRule(decider => decider.IsRuleTR0043Active);
				container.Validation.ValidateBC_Mode();
				AssertHasError("The BC_Mode is not entered", container.BC_ModeInfo, expectedError);
				container.BC_Mode = Constants.ContainerModes.Containerised;
				AssertNoError("The BC_Mode is entered", container.BC_ModeInfo, expectedError);

				ruleTestContext.DisableRule(decider => decider.IsRuleTR0043Active);
				container.Validation.ValidateBC_Mode();
				AssertNoError("The BC_Mode is not entered but rule is inactive", container.BC_ModeInfo, expectedError);
				container.BC_Mode = Constants.ContainerModes.Containerised;
				AssertNoError("The BC_Mode is entered", container.BC_ModeInfo, expectedError);
			});
		}

		public void TestCheckContainerNumberIsUnique()
		{
			var expectedError = "[TR0044] Duplicate Container Number is entered.";

			var incident = CreateArrivalIncident();

			using var ruleTestContext = new NctsContainerValidationDeciderTestContext<INctsContainerPhase5ValidationDecider>(Factory);
			ruleTestContext.EnableRule(x => x.IsRuleTR0044Active);
			var first = incident.IncidentContainers.AddNew();
			first.BC_Mode = Core.Constants.ContainerModes.Containerised;
			first.BC_ContainerNum = "MSKU1234565";

			var second = incident.IncidentContainers.AddNew();
			second.BC_Mode = Core.Constants.ContainerModes.Containerised;
			second.BC_ContainerNum = "MSKU1234565";

			CombineAssertions(() =>
			{
				AssertHasMessageError("Container number is duplicated", second.BC_ContainerNumInfo, expectedError);

				second.BC_ContainerNum = "MSKU1234570";
				AssertNoMessageError("Container number is unique", second.BC_ContainerNumInfo, expectedError);

				ruleTestContext.DisableRule(x => x.IsRuleTR0044Active);
				first.BC_ContainerNum = "MSKU1234565";
				AssertNoMessageError("Container number is duplicated but rule TR0044 is inactive", second.BC_ContainerNumInfo, expectedError);
			});
		}

		public void TestCheckContainerNumberIsUniqueShouldBeIgnoredForNonContainerisedMode()
		{
			var expectedError = "[TR0044] Duplicate Container Number is entered.";

			var incident = CreateArrivalIncident();

			var first = incident.IncidentContainers.AddNew();
			first.BC_Mode = Core.Constants.ContainerModes.NonContainerised;
			first.BC_ContainerNum = "MSKU1234565";

			var second = incident.IncidentContainers.AddNew();
			second.BC_Mode = Core.Constants.ContainerModes.NonContainerised;
			second.BC_ContainerNum = "MSKU1234565";

			CombineAssertions(() =>
			{
				using var ruleTestContext = new NctsContainerValidationDeciderTestContext<INctsContainerPhase5ValidationDecider>(Factory);
				ruleTestContext.EnableRule(x => x.IsRuleTR0044Active);
				AssertNoMessageError("Mode is 'NonConteinerised'", second.BC_ContainerNumInfo, expectedError);

				second.BC_Mode = Core.Constants.ContainerModes.Containerised;
				second.BC_ContainerNum = "MSKU1234565";
				AssertHasMessageError("Mode is 'Conteinerised'", second.BC_ContainerNumInfo, expectedError);

				ruleTestContext.DisableRule(x => x.IsRuleTR0044Active);
				first.BC_Mode = Core.Constants.ContainerModes.Containerised;
				first.BC_ContainerNum = "MSKU1234565";
				AssertNoMessageError("Mode is 'Conteinerised' but rule TR0044 is inactive", first.BC_ContainerNumInfo, expectedError);
			});
		}

		public void TestCheckSealNumberIsUniqueForSeal1()
		{
			var expectedWarning = "[TR0045] Duplicate Seal 1 Number entered.";

			var incident = CreateArrivalIncident();
			var container = incident.IncidentContainers.AddNew();
			container.BC_Seal2 = "222";

			CombineAssertions(() =>
			{
				using var ruleContext = new NctsContainerValidationDeciderTestContext<INctsContainerPhase5ValidationDecider>(Factory);
				ruleContext.EnableRule(x => x.IsRuleTR0045Active);
				container.BC_Seal1 = "222";
				AssertHasWarning("Seal1 duplicates Seal2", container.BC_Seal1Info, expectedWarning);

				container.BC_Seal1 = "111";
				AssertNoWarning("Seal1 is unique", container.BC_Seal1Info, expectedWarning);

				ruleContext.DisableRule(x => x.IsRuleTR0045Active);
				container.BC_Seal1 = "222";
				AssertNoWarning("Seal1 duplicates Seal2 but rule TR0045 is inactive", container.BC_Seal1Info, expectedWarning);
			});
		}

		public void TestCheckSealNumberIsUniqueForSeal2()
		{
			var expectedWarning = "[TR0045] Duplicate Seal 2 Number entered.";

			var incident = CreateArrivalIncident();
			var container = incident.IncidentContainers.AddNew();
			container.BC_Seal1 = "111";

			CombineAssertions(() =>
			{
				using var ruleContext = new NctsContainerValidationDeciderTestContext<INctsContainerPhase5ValidationDecider>(Factory);
				ruleContext.EnableRule(x => x.IsRuleTR0045Active);
				container.BC_Seal2 = "111";
				AssertHasWarning("Seal2 duplicates Seal1", container.BC_Seal2Info, expectedWarning);

				container.BC_Seal2 = "222";
				AssertNoWarning("Seal2 is unique", container.BC_Seal2Info, expectedWarning);

				ruleContext.DisableRule(x => x.IsRuleTR0045Active);
				container.BC_Seal1 = "111";
				AssertNoWarning("Seal1 duplicates Seal2 but rule TR0045 is inactive", container.BC_Seal2Info, expectedWarning);
			});
		}

		public void TestCheckBC_Mode_RuleTR0046()
		{
			var expectedWarning = "[TR0046] You have selected Both Mode 'CNT' and 'NCT'";

			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var incident = header.EnRouteIncidents.AddNew();
			var container = incident.IncidentContainers.AddNew();
			container.BC_Mode = Constants.ContainerModes.Containerised;

			CombineAssertions(() =>
			{
				using var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory);
				ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleTR0046Active));
				var container1 = incident.IncidentContainers.AddNew();
				container1.BC_Mode = Constants.ContainerModes.Containerised;
				AssertNoWarning("No warning for first container", container1.BC_ModeInfo, expectedWarning);

				var container2 = incident.IncidentContainers.AddNew();
				container2.BC_Mode = Constants.ContainerModes.NonContainerised;
				AssertHasWarning("Warning when container mode is different from previous containers", container2.BC_ModeInfo, expectedWarning);

				var container3 = incident.IncidentContainers.AddNew();
				container3.BC_Mode = "XYZ";
				AssertHasErrorContaining("Error when container with invalid mode is added", container3.BC_ModeInfo, ListValidation.InvalidCodeError);

				ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleTR0046Active));
				container2.BC_Mode = Constants.ContainerModes.Containerised;
				container1.BC_Mode = Constants.ContainerModes.NonContainerised;
				AssertNoWarning("No Warning when container mode is different from previous containers and rule TR0046 is inactive", container1.BC_ModeInfo, expectedWarning);
			});
		}

		EnRouteIncident CreateArrivalIncident()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			return header.EnRouteIncidents.AddNew();
		}
	}
}
