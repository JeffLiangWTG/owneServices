using System.Collections.Specialized;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.AU.AirCargo.GUI.Testing
{
	abstract class BaseHouseDetailsUserControlAbstractTest : TestCaseWithFactory
	{
		public void TestSetBindPrepend()
		{
			UserControl.SetBindPrepend(Prepend);
			AssertPrepended("HouseBillTextBox.BindTo", UserControl.HouseBillTextBox.BindTo);
			AssertPrepended("OriginFindBox.BindTo", UserControl.OriginFindBox.BindTo);
			AssertPrepended("OriginFindBox.BindToList", UserControl.OriginFindBox.BindToList);
			AssertPrepended("DestinationFindBox.BindTo", UserControl.DestinationFindBox.BindTo);
			AssertPrepended("DestinationFindBox.BindToList", UserControl.DestinationFindBox.BindToList);
			AssertPrepended("MasterHouseBillCheckBox.BindTo", UserControl.MasterHouseBillCheckBox.BindTo);
			AssertPrepended("WeightCalcDropEdit.BindToAmount", UserControl.WeightCalcDropEdit.BindToAmount);
			AssertPrepended("WeightCalcDropEdit.BindToList", UserControl.WeightCalcDropEdit.BindToList);
			AssertPrepended("WeightCalcDropEdit.BindToUnit", UserControl.WeightCalcDropEdit.BindToUnit);
			AssertPrepended("GoodsDescriptionTextBox.BindTo", UserControl.GoodsDescriptionTextBox.BindTo);
			AssertPrepended("PiecesManifestedCalcEdit.BindTo", UserControl.PiecesManifestedCalcEdit.BindTo);
			AssertPrepended("PrepaidCollectDropEdit.BindTo", UserControl.PrepaidCollectDropEdit.BindTo);
			AssertPrepended("PrepaidCollectDropEdit.BindToList", UserControl.PrepaidCollectDropEdit.BindToList);
			AssertPrepended("WarehouseLocationTextBox.BindTo", UserControl.WarehouseLocationTextBox.BindTo);
			AssertPrepended("WarehouseLocationLabel.BindTo", UserControl.WarehouseLocationLabel.BindTo);
			AssertPrepended("FolioReferenceTextBox.BindTo", UserControl.FolioReferenceTextBox.BindTo);
			AssertPrepended("ServiceLevelCodeFindBox.BindTo", UserControl.ServiceLevelCodeFindBox.BindTo);
			AssertPrepended("ServiceLevelCodeFindBox.BindToList", UserControl.ServiceLevelCodeFindBox.BindToList);
			AssertPrepended("ChargeableWeightCalcEdit.BindTo", UserControl.ChargeableWeightCalcEdit.BindTo);
			AssertPrepended("ChargableWeightLabel.BindTo", UserControl.ChargableWeightLabel.BindTo);
			AssertPrepended("PrepaidCollectLabel.BindTo", UserControl.PrepaidCollectLabel.BindTo);
			AssertPrepended("ShipmentTypeDropEdit.BindTo", UserControl.shipmentTypeDropEdit.BindTo);
			AssertPrepended("ShipmentTypeDropEdit.BindToList", UserControl.shipmentTypeDropEdit.BindToList);
			for (int i = 0; i < AdditionalBindPropertiesToBePrepended.Count; i++)
			{
				AssertPrepended(AdditionalBindPropertiesToBePrepended.Keys[i], AdditionalBindPropertiesToBePrepended[i]);
			}
		}

		protected abstract BaseHouseDetailsUserControl GetNewHouseDetailsUserControl();

		protected abstract NameValueCollection AdditionalBindPropertiesToBePrepended { get; }

		BaseHouseDetailsUserControl userControl;
		protected BaseHouseDetailsUserControl UserControl => userControl ?? (userControl = GetNewHouseDetailsUserControl());

		protected override void TearDown()
		{
			UserControl.Dispose();
			base.TearDown();
		}

		void AssertPrepended(ZString name, ZString binding)
		{
			Assert(name + " should be prepended with \"" + Prepend + "\"", binding.StartsWith(Prepend));
			Assert(name + " should not contain just the prepend", (binding.Length - Prepend.Length) > 0);
		}

		ZString Prepend => "XXX.";
	}
}
