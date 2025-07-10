using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsDepartureHeaderContainerPhase4ValidationTest : TestCaseWithFactory
	{
		public void TestCheckContainerNumberNoWarningsWhenEmpty()
		{
			AssertEquals("Precondition: Phase4", headerContainer.Header.IsPhase5, false);

			headerContainer.BC_ContainerNum = "";
			AssertNoWarnings("When ContainerNum is empty", headerContainer.BC_ContainerNumInfo);
		}

		public void TestCheckSeal1MandatoryIfAdditionalSealsExist()
		{
			CombineAssertions(() =>
			{
				headerContainer.Header.SetMovementType(NctsMovementType.Codes.Departure);
				headerContainer.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				headerContainer.BC_Seal1 = ZString.Empty;
				AssertNoErrorContaining("Seal1 empty value, no AdditionalSeals", headerContainer.BC_Seal1Info, MandatoryValidation.MustBeEntered);

				headerContainer.AdditionalSeals.AddNew();
				headerContainer.Validation.ValidateBC_Seal1();
				AssertHasErrorContaining("Seal1 empty value, has AdditionalSeals", headerContainer.BC_Seal1Info, MandatoryValidation.MustBeEntered);

				header = Factory.New<NctsHeader>();
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				header.BH_HeaderType = NctsMovementType.Codes.Arrival;
				headerContainer = header.DepartureHeaderContainers.AddNew();

				headerContainer.Validation.ValidateBC_Seal1();
				AssertNoErrorContaining("Seal1 empty value, has AdditionalSeals", headerContainer.BC_Seal1Info, MandatoryValidation.MustBeEntered);

				header = Factory.New<NctsHeader>();
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				headerContainer = header.DepartureHeaderContainers.AddNew();

				header.BH_HeaderType = NctsMovementType.Codes.Departure;

				headerContainer.BC_Seal1 = "SEAL1";
				AssertNoErrorContaining("Seal1 has value", headerContainer.BC_Seal1Info, MandatoryValidation.MustBeEntered);
			});
		}

		public void TestCheckSeal2MandatoryIfAdditionalSealsExist()
		{
			CombineAssertions(() =>
			{
				headerContainer.Header.SetMovementType(NctsMovementType.Codes.Departure);
				headerContainer.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				headerContainer.BC_Seal2 = ZString.Empty;
				AssertNoErrorContaining("Seal2 empty value, no AdditionalSeals", headerContainer.BC_Seal2Info, MandatoryValidation.MustBeEntered);

				headerContainer.AdditionalSeals.AddNew();
				headerContainer.Validation.ValidateBC_Seal2();
				AssertHasErrorContaining("Seal2 empty value, has AdditionalSeals", headerContainer.BC_Seal2Info, MandatoryValidation.MustBeEntered);

				header = Factory.New<NctsHeader>();
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				header.BH_HeaderType = NctsMovementType.Codes.Arrival;
				headerContainer = header.DepartureHeaderContainers.AddNew();
				headerContainer.AdditionalSeals.AddNew();
				headerContainer.Validation.ValidateBC_Seal2();
				AssertNoErrorContaining("Seal2 empty value, has AdditionalSeals", headerContainer.BC_Seal2Info, MandatoryValidation.MustBeEntered);

				header = Factory.New<NctsHeader>();
				header.BH_HeaderType = NctsMovementType.Codes.Departure;

				headerContainer.BC_Seal2 = "SEAL2";
				AssertNoErrorContaining("Seal2 has value", headerContainer.BC_Seal2Info, MandatoryValidation.MustBeEntered);
			});
		}

		public void TestCheckContainerNumberInvalidCheckDigitWarning()
		{
			AssertEquals("Precondition: Phase4", headerContainer.Header.IsPhase5, false);
			const string warningMessage = "Container number does not have a valid check (last) digit. The check digit should be";
			headerContainer.BC_Mode = Constants.ContainerModes.Containerised;
			CombineAssertions(() =>
			{
				headerContainer.BC_ContainerNum = "CRXU1234561";
				AssertHasWarningContaining("BC_Mode CNT, BC_ContainerNum doesn't have valid check digit",
					headerContainer.BC_ContainerNumInfo, warningMessage);

				headerContainer.BC_ContainerNum = "BICU1234565";
				AssertNoWarningContaining("BC_Mode CNT, BC_ContainerNum has valid check digit",
					headerContainer.BC_ContainerNumInfo, warningMessage);

				headerContainer.BC_Mode = Constants.ContainerModes.NonContainerised;
				headerContainer.BC_ContainerNum = "CRXU1234561";
				AssertHasWarningContaining("BC_Mode NCT, BC_ContainerNum doesn't have valid check digit",
					headerContainer.BC_ContainerNumInfo, warningMessage);
			});
		}

		public void TestCheckContainerNumberDoesNotConformToISOStandardWarning()
		{
			AssertEquals("Precondition: Phase4", headerContainer.Header.IsPhase5, false);
			const string warningMessage = "Container number does not conform to ISO standard of 4 letters followed by 6 digits and a check digit.";
			headerContainer.BC_Mode = Constants.ContainerModes.Containerised;
			CombineAssertions(() =>
			{
				headerContainer.BC_ContainerNum = "ABC12345DE";
				AssertHasWarningContaining("BC_Mode CNT, BC_ContainerNum doesn't conform to ISO",
					headerContainer.BC_ContainerNumInfo, warningMessage);

				headerContainer.BC_ContainerNum = "BICU1234565";
				AssertNoWarningContaining("BC_Mode CNT, BC_ContainerNum conforms to ISO",
					headerContainer.BC_ContainerNumInfo, warningMessage);

				headerContainer.BC_Mode = Constants.ContainerModes.NonContainerised;
				headerContainer.BC_ContainerNum = "ABC12345DE";
				AssertHasWarningContaining("BC_Mode NCT, BC_ContainerNum doesn't conform to ISO",
					headerContainer.BC_ContainerNumInfo, warningMessage);
			});
		}

		public void TestCheckSeal1IsDuplicate()
		{
			var expectedWarningActiveRule = "Duplicate Seal 1 Number entered.";

			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			var container1 = header.DepartureHeaderContainers.AddNew();
			container1.Seal1 = "123";
			container1.Seal2 = "456";
			container1.AdditionalSeals.AddNew().BK_SealNumber = "789";

			var container2 = header.DepartureHeaderContainers.AddNew();
			CombineAssertions(() =>
			{
				container2.Seal1 = "123";
				AssertHasWarning("Duplicate of container1.Seal1", container2.Seal1Info, expectedWarningActiveRule);

				container2.Seal1 = "456";
				AssertHasWarning("Duplicate of container1.Seal2", container2.Seal1Info, expectedWarningActiveRule);

				container2.Seal1 = "789";
				AssertHasWarning("Duplicate in container1.AdditionalSeals", container2.Seal1Info, expectedWarningActiveRule);

				container2.Seal1 = "ABC";
				AssertNoWarning("New Seal1 number", container2.Seal1Info, expectedWarningActiveRule);
			});
		}

		public void TestCheckSeal2IsDuplicate()
		{
			var expectedWarningActiveRule = "Duplicate Seal 2 Number entered.";

			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			var container1 = header.DepartureHeaderContainers.AddNew();
			container1.Seal1 = "123";
			container1.Seal2 = "456";
			container1.AdditionalSeals.AddNew().BK_SealNumber = "789";

			var container2 = header.DepartureHeaderContainers.AddNew();
			container2.Seal1 = "XYZ";
			CombineAssertions(() =>
			{
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
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			headerContainer = header.DepartureHeaderContainers.AddNew();
		}

		NctsDepartureHeaderContainer headerContainer;
		NctsHeader header;
	}
}
