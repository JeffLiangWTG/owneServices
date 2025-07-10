using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsArrivalHeaderContainerValidationTest : TestCaseWithFactory
	{
		public void TestPhase5Validations()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			headerContainer = header.ArrivalHeaderContainers.AddNew();

			CombineAssertions(() =>
			{
				AssertType<NctsArrivalHeaderContainerPhase5Validation>("Inside phase 5 validation", headerContainer.Validation);
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				AssertType<NctsArrivalHeaderContainerValidation>("Outside phase 5 validation", headerContainer.Validation);
			});
		}

		public void TestCheckContainerNumberNoWarningsWhenEmpty()
		{
			headerContainer.BC_ContainerNum = "";
			AssertNoWarnings("When ContainerNum is empty", headerContainer.BC_ContainerNumInfo);
		}

		public void TestCheckContainerNumberInvalidCheckDigitWarning()
		{
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

		public void TestCheckContainerNumberDoesNotConformToISOStandardWarning()
		{
			const string warningMessage = "Container number does not conform to ISO standard of 4 letters followed by 6 digits and a check digit.";
			headerContainer.BC_Mode = Core.Constants.ContainerModes.Containerised;

			CombineAssertions(() =>
			{
				headerContainer.BC_ContainerNum = "ABC12345DE";
				AssertHasWarningContaining("BC_Mode CNT, BC_ContainerNum doesn't conform to ISO", headerContainer.BC_ContainerNumInfo, warningMessage);

				headerContainer.BC_ContainerNum = "BICU1234565";
				AssertNoWarningContaining("BC_Mode CNT, BC_ContainerNum conforms to ISO", headerContainer.BC_ContainerNumInfo, warningMessage);

				headerContainer.BC_Mode = Core.Constants.ContainerModes.NonContainerised;
				headerContainer.BC_ContainerNum = "ABC12345DE";
				AssertNoWarningContaining("BC_Mode NCT, BC_ContainerNum doesn't conform to ISO", headerContainer.BC_ContainerNumInfo, warningMessage);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			headerContainer = Factory.New<NctsArrivalHeaderContainer>();
		}
		NctsArrivalHeaderContainer headerContainer;
	}
}
