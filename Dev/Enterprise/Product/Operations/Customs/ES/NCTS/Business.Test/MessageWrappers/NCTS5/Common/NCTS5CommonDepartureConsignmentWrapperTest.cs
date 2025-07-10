using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NCTS5CommonDepartureConsignmentWrapperTest : WrapperHelperTest<NCTS5CommonDepartureConsignmentWrapper>
	{
		public void TestContainerIndicator()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected false ContainerIndicator when no containers or goodsItems declared", false, wrapper.ContainerIndicator);

				var bill = nctsHeader.Bills.AddNew();
				var container = nctsHeader.DepartureHeaderContainers.AddNew();
				container.BC_Mode = Core.Constants.ContainerModes.Containerised;
				container.BC_ContainerNum = "CNT1";

				var container2 = nctsHeader.DepartureHeaderContainers.AddNew();
				container2.BC_Mode = Core.Constants.ContainerModes.NonContainerised;
				container2.BC_ContainerNum = "NCT1";

				var goodItem1 = bill.GoodsItems.AddNew();
				var package1 = goodItem1.Packages.AddNew();
				var genPivot1 = package1.ContainersPivotsForBindingOnly[0];
				genPivot1.ContainerSelected = true;
				var genPivot11 = package1.ContainersPivotsForBindingOnly[1];
				genPivot11.ContainerSelected = false;

				var goodItem2 = bill.GoodsItems.AddNew();
				var package2 = goodItem2.Packages.AddNew();
				goodItem2.BY_DeclarationGoodsItemNumber = 2;
				var genPivot2 = package2.ContainersPivotsForBindingOnly[0];
				genPivot2.ContainerSelected = false;
				var genPivot21 = package2.ContainersPivotsForBindingOnly[1];
				genPivot21.ContainerSelected = false;

				wrapper = GetWrapper(nctsHeader);
				AssertEquals("Expected true ContainerIndicator when at least one goodsItem has a container selected", true, wrapper.ContainerIndicator);

				genPivot1.ContainerSelected = false;
				wrapper = GetWrapper(nctsHeader);
				AssertEquals("Expected false ContainerIndicator when no goodsItem has a container selected", false, wrapper.ContainerIndicator);

				genPivot11.ContainerSelected = true;
				wrapper = GetWrapper(nctsHeader);
				AssertEquals("Expected false ContainerIndicator when a goodsItem has a container selected and the container has BC_MODE = NCT", false, wrapper.ContainerIndicator);

				genPivot21.ContainerSelected = true;
				wrapper = GetWrapper(nctsHeader);
				AssertEquals("Expected false ContainerIndicator when goodsItem has containers selected, but are BC_MODE = NCT", false, wrapper.ContainerIndicator);

				genPivot11.ContainerSelected = false;
				genPivot21.ContainerSelected = false;
				genPivot2.ContainerSelected = true;
				wrapper = GetWrapper(nctsHeader);
				AssertEquals("Expected true ContainerIndicator when goodsItem has containers selected and one is BC_MODE = CNT", true, wrapper.ContainerIndicator);
			});
		}

		public void TestInlandModeOfTransport()
		{
			CombineAssertions(() =>
			{
				departureMovement.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._1_SeaTransport;
				departureMovement.BM_ReducedDatasetIndicator = false;
				AssertEquals("Expected filled InlandModeOfTransport when BM_ReducedDatasetIndicator is false", "1", wrapper.InlandModeOfTransport);

				departureMovement.BM_ReducedDatasetIndicator = true;
				AssertEquals("Expected filled InlandModeOfTransport when BM_ReducedDatasetIndicator is true", "1", wrapper.InlandModeOfTransport);
			});
		}

		public void TestModeOfTransportAtTheBorder()
		{
			departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
			AssertEquals("Expected filled ModeOfTransportAtTheBorder", "1", wrapper.ModeOfTransportAtTheBorder);
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			departureMovement = nctsHeader.MovementHeader;

			wrapper = GetWrapper(nctsHeader);
		}

		NctsHeader nctsHeader;
		NctsDepartureMovementHeader departureMovement;
		NCTS5CommonDepartureConsignmentWrapper wrapper;

		NCTS5CommonDepartureConsignmentWrapper GetWrapper(NctsHeader header) => new NCTS5CommonDepartureConsignmentWrapper(header);

		protected override NCTS5CommonDepartureConsignmentWrapper GetProvider() => wrapper;
	}
}
