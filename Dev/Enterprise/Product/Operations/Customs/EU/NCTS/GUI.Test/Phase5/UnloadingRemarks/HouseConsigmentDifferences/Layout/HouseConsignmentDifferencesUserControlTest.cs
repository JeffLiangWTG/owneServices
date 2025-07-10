using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class HouseConsignmentDifferencesUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(NctsBill), userControl.BindingSource.DataSourceType);
		}

		public void TestSequenceNumberTextBox()
		{
			var control = userControl.SequenceNumberTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", control);
				AssertEquals("BindingMember", "MovementDetail+B9_SeqNo", control.GetBindingMember());
			});
		}

		public void TestSecurityCheckBox()
		{
			var control = userControl.SecurityCheckBox;
			CombineAssertions(() =>
			{
				AssertType<ZCheckBox>("Type", control);
				AssertEquals("BindingMember", "B0_SecurityIndicatorFromExport", control.GetBindingMember());
			});
		}

		public void TestHouseConsignmentTextBox()
		{
			var control = userControl.HouseConsignmentTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", control);
				AssertEquals("BindingMember", "B0_ReferenceID", control.GetBindingMember());
			});
		}

		public void TestUnloadedStateDropEdit()
		{
			var control = userControl.UnloadedStateDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", control);
				AssertEquals("BindingMember", "MovementDetail+B9_UnloadedState", control.GetBindingMember());
			});
		}

		public void TestGrossWeightCalcDropEdit()
		{
			var control = userControl.GrossWeightCalcDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZCalcDropEdit>("Type", control);
				AssertEquals("BindToAmount", "B0_Weight", control.BindToAmount);
				AssertEquals("BindToList", "Lookups.WeightUnitList", control.BindToList);
				AssertEquals("BindToUnit", "B0_WeightUQ", control.BindToUnit);
			});
		}

		public void TestGrossWeightUnloadedCalcDropEdit()
		{
			var control = userControl.GrossWeightUnloadedCalcDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZCalcDropEdit>("Type", control);
				AssertEquals("BindingMember", ".", control.GetBindingMember());
				AssertEquals("BindToAmount", "B0_GrossWeightUnloaded", control.BindToAmount);
				AssertEquals("BindToList", "Lookups.WeightUnitList", control.BindToList);
				AssertEquals("BindToUnit", "MovementDetail.DifferenceMoveDetailCollection.DifferenceWeightUnit", control.BindToUnit);
			});
		}

		public void TestConsignorDocAddressControl()
		{
			var control = userControl.ConsignorDocAddressControl;
			CombineAssertions(() =>
			{
				AssertType<MasterFiles.GUI.ZDocAddressControl>("Type", control);
				AssertEquals("BindingMember", "MovementDetail.ConsignorDocAddress", control.GetBindingMember());
				AssertEquals("BindToOrganisations", "Lookups.Consignors", control.BindToOrganisations);
			});
		}

		public void TestConsigneeDocAddressControl()
		{
			var control = userControl.ConsigneeDocAddressControl;
			CombineAssertions(() =>
			{
				AssertType<MasterFiles.GUI.ZDocAddressControl>("Type", control);
				AssertEquals("BindingMember", "MovementDetail.ConsigneeDocAddress", control.GetBindingMember());
				AssertEquals("BindToOrganisations", "Lookups.Consignees", control.BindToOrganisations);
			});
		}

		public void TestTransportInfoUserControl()
		{
			var control = userControl.ArrivalTransportInfosUserControl;
			CombineAssertions(() =>
			{
				AssertType<ArrivalTransportInfosUserControl>("Type", control);
				AssertEquals("BindingMember", "ArrivalTransportInfos", control.GetBindingMember());
			});
		}

		public void TestDeclaredValueLabel()
		{
			var control = userControl.DeclaredValueLabel;
			CombineAssertions(() =>
			{
				AssertType<ZLabel>("Type", control);
				AssertEquals(false, new LabelCaptionRenderProvider().GetLabelCaptionVisible(control));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new HouseConsignmentDifferencesUserControl();
		}
		HouseConsignmentDifferencesUserControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
