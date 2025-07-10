using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.NCTS.Testing
{
	public class FRNctsDepartureHeaderContainerPhase4ValidationTest : TestCaseWithFactory
	{
		public void TestCheckContainerType()
		{
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

		public void TestCheckContainerMode()
		{
			CombineAssertions(() =>
			{
				container.BC_Mode = "1";
				AssertHasError(container.BC_ModeInfo, "Enter a valid Mode.");
				container.BC_Mode = Core.Constants.ContainerModes.LCL;
				AssertNoError(container.BC_ModeInfo, "Enter a valid Mode.");
			});
		}

		public void TestCheckContainersForCONSealType()
		{
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

		protected override void SetUp()
		{
			base.SetUp();

			departure = Factory.New<NctsHeader>();
			departure.SetMovementType(NctsMovementType.Codes.Departure);
			departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			container = departure.DepartureHeaderContainers.AddNew();
		}
		FRNctsDepartureHeaderContainer container;
		NctsHeader departure;
	}
}
