using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class Phase5ArrivalContainersAndSealsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType() => AssertEquals(typeof(NctsHeader), control.BindingSource.DataSourceType);

		public void TestContainersUserControlGroupBox()
		{
			var groupBox = control.ContainersEquipmentAndSealsGroupBox;
			AssertEquals("Caption", "Containers/Equipment and Seals", groupBox.CaptionResourceString.Caption);
		}

		public void TestContainersGrid()
		{
			var containerGrid = control.ContainersEquipmentAndSealsGroupBox.FindSingle<Phase5ArrivalContainersEquipmentGridUserControl>();
			AssertEquals("BindingMember", nameof(NctsHeader.ArrivalHeaderContainers), containerGrid.GetBindingMember());
		}

		public void TestSealsGrid()
		{
			var containerGrid = control.ContainersEquipmentAndSealsGroupBox.FindSingle<Phase5ArrivalSealsGridUserControl>();
			AssertEquals("BindingMember", nameof(NctsHeader.ArrivalHeaderContainers) + "." + nameof(NctsArrivalHeaderContainer.Seals), containerGrid.GetBindingMember());
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new Phase5ArrivalContainersAndSealsUserControl();
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.ArrivalMovementHeader.BM_SealType = SealTypeList.Codes.ContainerSeal;
			header.ArrivalHeaderContainers.AddNew();
			control.SetDataBinding(header, null);
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}

		Phase5ArrivalContainersAndSealsUserControl control;
	}
}
