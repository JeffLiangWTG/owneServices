using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsContainerValidationTest : TestCaseWithFactory
	{
		public void TestPhase5Validations()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var incident = header.EnRouteIncidents.AddNew();
			var container = incident.IncidentContainers.AddNew();

			CombineAssertions(() =>
			{
				AssertType<NctsContainerPhase5Validation>("Inside phase 5 validation", container.Validation);
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				AssertType<NctsContainerPhase4Validation>("Outside phase 5 validation", container.Validation);
			});
		}

		public void TestCheckContainerNumberNoWarningsWhenEmpty()
		{
			container.BC_ContainerNum = ZString.Empty;
			AssertNoWarnings("When ContainerNum is empty", container.BC_ContainerNumInfo);
		}

		public void TestCheckContainerNumberInvalidCheckDigitWarning()
		{
			const string warningMessage = "Container number does not have a valid check (last) digit. The check digit should be";
			var incident = CreateArrivalIncident();
			var container = incident.IncidentContainers.AddNew();
			container.BC_Mode = Core.Constants.ContainerModes.Containerised;
			CombineAssertions(() =>
			{
				container.BC_ContainerNum = "CRXU1234561";
				AssertHasWarningContaining("BC_Mode CNT, BC_ContainerNum doesn't have valid check digit", container.BC_ContainerNumInfo, warningMessage);

				container.BC_ContainerNum = "BICU1234565";
				AssertNoWarningContaining("BC_Mode CNT, BC_ContainerNum has valid check digit", container.BC_ContainerNumInfo, warningMessage);

				container.BC_Mode = Core.Constants.ContainerModes.NonContainerised;
				container.BC_ContainerNum = "CRXU1234561";
				AssertNoWarningContaining("BC_Mode NCT, BC_ContainerNum doesn't have valid check digit", container.BC_ContainerNumInfo, warningMessage);
			});
		}

		public void TestCheckContainerNumberDoesNotConformToISOStandardWarning()
		{
			const string warningMessage = "Container number does not conform to ISO standard of 4 letters followed by 6 digits and a check digit.";
			var incident = CreateArrivalIncident();
			var container = incident.IncidentContainers.AddNew();
			CombineAssertions(() =>
			{
				container.BC_Mode = Core.Constants.ContainerModes.Containerised;
				container.BC_ContainerNum = "ABC12345DE";
				AssertHasWarningContaining("BC_Mode CNT, BC_ContainerNum doesn't conform to ISO", container.BC_ContainerNumInfo, warningMessage);

				container.BC_ContainerNum = "BICU1234565";
				AssertNoWarningContaining("BC_Mode CNT, BC_ContainerNum conforms to ISO", container.BC_ContainerNumInfo, warningMessage);

				container.BC_Mode = Core.Constants.ContainerModes.NonContainerised;
				container.BC_ContainerNum = "ABC12345DE";
				AssertNoWarningContaining("BC_Mode NCT, BC_ContainerNum doesn't conform to ISO", container.BC_ContainerNumInfo, warningMessage);
			});
		}

		public void TestCheckSeal1MandatoryIfAdditionalSealsExist()
		{
			CombineAssertions(() =>
			{
				var incident = CreateArrivalIncident();
				var container = incident.IncidentContainers.AddNew();
				container.BC_Seal1 = ZString.Empty;
				AssertNoMessageErrorContaining("Seal1 empty value, no AdditionalSeals", container.BC_Seal1Info, MandatoryValidation.YouHaveNotEntered);

				container.Seals.AddNew();
				container.Validation.ValidateBC_Seal1();
				AssertHasMessageErrorContaining("Seal1 empty value, has AdditionalSeals", container.BC_Seal1Info, MandatoryValidation.YouHaveNotEntered);

				container.BC_Seal1 = "SEAL1";
				AssertNoMessageErrorContaining("Seal1 has value", container.BC_Seal1Info, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckSeal2MandatoryIfAdditionalSealsExist()
		{
			CombineAssertions(() =>
			{
				var incident = CreateArrivalIncident();
				var container = incident.IncidentContainers.AddNew();
				container.BC_Seal2 = ZString.Empty;
				AssertNoMessageErrorContaining("Seal2 empty value, no AdditionalSeals", container.BC_Seal2Info, MandatoryValidation.YouHaveNotEntered);

				container.Seals.AddNew();
				container.Validation.ValidateBC_Seal2();
				AssertHasMessageErrorContaining("Seal2 empty value, has AdditionalSeals", container.BC_Seal2Info, MandatoryValidation.YouHaveNotEntered);

				container.BC_Seal2 = "SEAL2";
				AssertNoMessageErrorContaining("Seal2 has value", container.BC_Seal2Info, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestBC_ContainerNumber_ArrivalDetailsReadOnly()
		{
			SetUpContainerOnArrivalIncident();

			CombineAssertions(() =>
			{
				container.BC_ContainerNum = "CRXU1234561";
				AssertNoNotifications("BC_Mode CNT, BC_ContainerNum doesn't have valid check digit", container.BC_ContainerNumInfo);

				container.BC_ContainerNum = "BICU1234565";
				AssertNoNotifications("BC_Mode CNT, BC_ContainerNum has valid check digit", container.BC_ContainerNumInfo);

				container.BC_Mode = Core.Constants.ContainerModes.NonContainerised;
				container.BC_ContainerNum = "CRXU1234561";
				AssertNoNotifications("BC_Mode NCT, BC_ContainerNum doesn't have valid check digit", container.BC_ContainerNumInfo);

				container.BC_Mode = Core.Constants.ContainerModes.Containerised;
				container.BC_ContainerNum = "ABC12345DE";
				AssertNoNotifications("BC_Mode CNT, BC_ContainerNum doesn't conform to ISO", container.BC_ContainerNumInfo);

				container.BC_ContainerNum = "BICU1234565";
				AssertNoNotifications("BC_Mode CNT, BC_ContainerNum conforms to ISO", container.BC_ContainerNumInfo);

				container.BC_Mode = Core.Constants.ContainerModes.NonContainerised;
				container.BC_ContainerNum = "ABC12345DE";
				AssertNoNotifications("BC_Mode NCT, BC_ContainerNum doesn't conform to ISO", container.BC_ContainerNumInfo);
			});
		}

		public void TestBC_Seal1_ArrivalDetailsReadOnly()
		{
			SetUpContainerOnArrivalIncident();

			CombineAssertions(() =>
			{
				container.BC_Seal1 = ZString.Empty;
				AssertNoNotifications("Seal1 empty value, no AdditionalSeals", container.BC_Seal1Info);

				container.Seals.AddNew();
				container.Validation.ValidateBC_Seal1();
				AssertNoNotifications("Seal1 empty value, has AdditionalSeals", container.BC_Seal1Info);

				container.BC_Seal1 = "SEAL1";
				AssertNoNotifications("Seal1 has value", container.BC_Seal1Info);
			});
		}

		public void TestBC_Seal2_ArrivalDetailsReadOnly()
		{
			SetUpContainerOnArrivalIncident();

			CombineAssertions(() =>
			{
				container.BC_Seal2 = ZString.Empty;
				AssertNoNotifications("Seal2 empty value, no AdditionalSeals", container.BC_Seal2Info);

				container.Seals.AddNew();
				container.Validation.ValidateBC_Seal2();
				AssertNoNotifications("Seal2 empty value, has AdditionalSeals", container.BC_Seal2Info);

				container.BC_Seal2 = "SEAL2";
				AssertNoNotifications("Seal2 has value", container.BC_Seal2Info);
			});
		}

		public void TestBC_ModeIsRequired_ContainerNumEntered()
		{
			TestBC_ModeIsRequired(x => x.BC_ContainerNum = "MSKU1234565");
		}

		public void TestBC_ModeIsRequired_Seal1Entered()
		{
			TestBC_ModeIsRequired(x => x.BC_Seal1 = "123");
		}

		public void TestBC_ModeIsRequired_Seal2Entered()
		{
			TestBC_ModeIsRequired(x => x.BC_Seal2 = "123");
		}

		void TestBC_ModeIsRequired(Action<NctsContainer> setupProperty)
		{
			var expectedError = "Please enter a Container/Equipment Mode.";

			var incident = CreateArrivalIncident();
			var container = incident.IncidentContainers.AddNew();

			CombineAssertions(() =>
			{
				AssertNoErrorContaining(container.BC_ModeInfo, expectedError);
				setupProperty(container);
				ValidationTestHelper.AssertErrorIfNotEntered(container.BC_ModeInfo, expectedError);
			});
		}

		public void TestBC_ModeIsFromList()
		{
			var expectedError = "Enter a valid Container/Equipment Mode.";

			var incident = CreateArrivalIncident();
			var container = incident.IncidentContainers.AddNew();
			ValidationTestHelper.AssertErrorIfInvalidCode(container.BC_ModeInfo, "ABC", "CNT", expectedError);
		}

		EnRouteIncident CreateArrivalIncident()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			return header.EnRouteIncidents.AddNew();
		}

		void SetUpContainerOnArrivalIncident()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.EffectiveMessageStatus = LogicalStatusList.Codes.Sent;
			var incident = header.EnRouteIncidents.AddNew();
			container = incident.IncidentContainers.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();
			container = Factory.New<NctsContainer>();
		}
		NctsContainer container;
	}
}
