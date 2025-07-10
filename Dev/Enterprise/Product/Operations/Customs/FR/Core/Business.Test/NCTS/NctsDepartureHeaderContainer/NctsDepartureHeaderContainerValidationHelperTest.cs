using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.NCTS.Testing
{
	public class NctsDepartureHeaderContainerValidationHelperTest : TestCaseWithFactory
	{
		public void TestCheckInvalidRC_Phase4()
		{
			var (_, container) = GetHeaderAndContainer(CusInBondApplicationCodeList.Codes.NCTS4);

			var refContainer = Factory.New<RefContainer>();
			refContainer.RC_Code = "TST";

			CombineAssertions(() =>
			{
				container.BC_RC = ZGuid.BrettsGuid;
				AssertHasError(container.BC_RCInfo, "Please enter a valid Container Type.");
				container.BC_RC = refContainer.PK;
				AssertNoError(container.BC_RCInfo, "Please enter a valid Container Type.");
			});
		}

		public void TestCheckInvalidRC_Phase5()
		{
			var (_, container) = GetHeaderAndContainer(CusInBondApplicationCodeList.Codes.NCTS5);

			var refContainer = Factory.New<RefContainer>();
			refContainer.RC_Code = "TST";

			CombineAssertions(() =>
			{
				container.BC_RC = ZGuid.BrettsGuid;
				AssertHasError(container.BC_RCInfo, "Please enter a valid Container Type.");
				container.BC_RC = refContainer.PK;
				AssertNoError(container.BC_RCInfo, "Please enter a valid Container Type.");
			});
		}

		public void TestCheckInvalidMode_Phase4()
		{
			var (_, container) = GetHeaderAndContainer(CusInBondApplicationCodeList.Codes.NCTS4);

			CombineAssertions(() =>
			{
				container.BC_Mode = "1";
				AssertHasError(container.BC_ModeInfo, "Enter a valid Mode.");
				container.BC_Mode = Core.Constants.ContainerModes.LCL;
				AssertNoError(container.BC_ModeInfo, "Enter a valid Mode.");
			});
		}

		public void TestCheckInvalidMode_Phase5()
		{
			var (_, container) = GetHeaderAndContainer(CusInBondApplicationCodeList.Codes.NCTS5);

			CombineAssertions(() =>
			{
				container.BC_Mode = "1";
				AssertHasError(container.BC_ModeInfo, "Enter a valid Mode.");
				container.BC_Mode = Core.Constants.ContainerModes.LCL;
				AssertNoError(container.BC_ModeInfo, "Enter a valid Mode.");
			});
		}

		public void TestCheckMandatoryContainerNum_Phase4()
		{
			var (departure, container) = GetHeaderAndContainer(CusInBondApplicationCodeList.Codes.NCTS4);

			CombineAssertions(() =>
			{
				AssertNoMessageError(container.BC_ContainerNumInfo, "You have not entered a container number.");

				var moveHeader = departure.MovementHeader;
				moveHeader.BM_SealType = SealTypeList.Codes.ContainerSeal;

				container.Validation.ValidateBC_ContainerNum();
				AssertHasMessageError(container.BC_ContainerNumInfo, "You have not entered a container number.");

				container.BC_ContainerNum = "CNT001";
				AssertNoMessageError(container.BC_ContainerNumInfo, "You have not entered a container number.");
			});
		}

		public void TestCheckMandatoryContainerNum_Phase5()
		{
			var (departure, container) = GetHeaderAndContainer(CusInBondApplicationCodeList.Codes.NCTS5);

			CombineAssertions(() =>
			{
				AssertNoMessageError(container.BC_ContainerNumInfo, "You have not entered a container number.");

				var moveHeader = departure.MovementHeader;
				moveHeader.BM_SealType = SealTypeList.Codes.ContainerSeal;

				container.Validation.ValidateBC_ContainerNum();
				AssertHasMessageError(container.BC_ContainerNumInfo, "You have not entered a container number.");

				container.BC_ContainerNum = "CNT001";
				AssertNoMessageError(container.BC_ContainerNumInfo, "You have not entered a container number.");
			});
		}

		(NctsHeader, FRNctsDepartureHeaderContainer) GetHeaderAndContainer(ZString phase)
		{
			var departure = Factory.New<NctsHeader>();
			departure.SetMovementType(NctsMovementType.Codes.Departure);
			departure.BH_ApplicationCode = phase;
			return (departure, departure.DepartureHeaderContainers.AddNew());
		}
	}
}
