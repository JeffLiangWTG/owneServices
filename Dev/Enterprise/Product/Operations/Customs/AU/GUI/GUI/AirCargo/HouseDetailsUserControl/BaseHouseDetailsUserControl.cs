using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.AirCargo.GUI
{
	public partial class BaseHouseDetailsUserControl : ZUserControl
	{
		public BaseHouseDetailsUserControl()
		{
			InitializeComponent();
		}

		public virtual void SetBindPrepend(ZString bindPrepend)
		{
			HouseBillTextBox.BindTo = bindPrepend + HouseBillTextBox.BindTo;
			OriginFindBox.BindTo = bindPrepend + OriginFindBox.BindTo;
			OriginFindBox.BindToList = bindPrepend + OriginFindBox.BindToList;
			DestinationFindBox.BindTo = bindPrepend + DestinationFindBox.BindTo;
			DestinationFindBox.BindToList = bindPrepend + DestinationFindBox.BindToList;
			MasterHouseBillCheckBox.BindTo = bindPrepend + MasterHouseBillCheckBox.BindTo;
			WeightCalcDropEdit.BindToAmount = bindPrepend + WeightCalcDropEdit.BindToAmount;
			WeightCalcDropEdit.BindToList = bindPrepend + WeightCalcDropEdit.BindToList;
			WeightCalcDropEdit.BindToUnit = bindPrepend + WeightCalcDropEdit.BindToUnit;
			GoodsDescriptionTextBox.BindTo = bindPrepend + GoodsDescriptionTextBox.BindTo;
			PiecesManifestedCalcEdit.BindTo = bindPrepend + PiecesManifestedCalcEdit.BindTo;
			PrepaidCollectDropEdit.BindTo = bindPrepend + PrepaidCollectDropEdit.BindTo;
			PrepaidCollectDropEdit.BindToList = bindPrepend + PrepaidCollectDropEdit.BindToList;
			WarehouseLocationTextBox.BindTo = bindPrepend + WarehouseLocationTextBox.BindTo;
			WarehouseLocationLabel.BindTo = bindPrepend + WarehouseLocationLabel.BindTo;
			FolioReferenceTextBox.BindTo = bindPrepend + FolioReferenceTextBox.BindTo;
			ServiceLevelCodeFindBox.BindTo = bindPrepend + ServiceLevelCodeFindBox.BindTo;
			ServiceLevelCodeFindBox.BindToList = bindPrepend + ServiceLevelCodeFindBox.BindToList;
			ChargeableWeightCalcEdit.BindTo = bindPrepend + ChargeableWeightCalcEdit.BindTo;
			ChargableWeightLabel.BindTo = bindPrepend + ChargableWeightLabel.BindTo;
			PrepaidCollectLabel.BindTo = bindPrepend + PrepaidCollectLabel.BindTo;
			shipmentTypeDropEdit.BindTo = bindPrepend + shipmentTypeDropEdit.BindTo;
			shipmentTypeDropEdit.BindToList = bindPrepend + shipmentTypeDropEdit.BindToList;
		}

		void zLabel31_Click(object sender, System.EventArgs e)
		{
		}

		void zLabel1_Click(object sender, System.EventArgs e)
		{
		}
	}
}
