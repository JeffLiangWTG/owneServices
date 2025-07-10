namespace Enterprise.Customs.AU.AirCargo.GUI
{
	public partial class CTOHouseDetailsUserControl
	{
		private void InitializeComponent()
		{
			this.LoadPortCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.LoadLabel = new Enterprise.ZArchitecture.ZLabel();
			this.HouseGroupBox.SuspendLayout();
			this.shipmentTypeDropEdit.SuspendLayout();
			this.WeightCalcDropEdit.SuspendLayout();
			this.DestinationFindBox.SuspendLayout();
			this.OriginFindBox.SuspendLayout();
			this.PrepaidCollectDropEdit.SuspendLayout();
			this.goodsValueCalcDropEdit.SuspendLayout();
			this.NonCustomsItemsGroupBox.SuspendLayout();
			this.ServiceLevelCodeFindBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LoadPortCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// ResponsiblePartyLabel
			// 
			this.responsiblePartyLabel.TabIndex = 22;
			// 
			// ResponsiblePartyTextBox
			// 
			this.responsiblePartyTextBox.TabIndex = 23;
			// 
			// MessageStatusTextBox
			// 
			this.MessageStatusTextBox.TabIndex = 25;
			// 
			// WeightUnitLabel
			// 
			this.WeightUnitLabel.TabIndex = 6;
			// 
			// TranshipmentTextBox
			// 
			this.TranshipmentTextBox.TabIndex = 17;
			// 
			// TranshipmentLabel
			// 
			this.TranshipmentLabel.TabIndex = 16;
			// 
			// HouseGroupBox
			// 
			this.HouseGroupBox.Controls.Add(this.LoadPortCodeFindBox);
			this.HouseGroupBox.Controls.Add(this.LoadLabel);
			// 
			// ShipmentTypeDropEdit
			// 
			this.shipmentTypeDropEdit.TabIndex = 20;
			// 
			// shipmentTypeLabel
			// 
			this.shipmentTypeLabel.TabIndex = 19;
			// 
			// WeightCalcDropEdit
			// 
			this.WeightCalcDropEdit.TabIndex = 9;
			// 
			// PiecesManifestedCalcEdit
			// 
			this.PiecesManifestedCalcEdit.TabIndex = 11;
			// 
			// GoodsDescriptionTextBox
			// 
			this.GoodsDescriptionTextBox.TabIndex = 12;
			// 
			// DestinationFindBox
			// 
			this.DestinationFindBox.TabIndex = 7;
			// 
			// OriginFindBox
			// 
			this.OriginFindBox.TabIndex = 5;
			// 
			// HouseBillTextBox
			// 
			this.HouseBillTextBox.TabIndex = 1;
			// 
			// HouseBillLabel
			// 
			this.HouseBillLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(58, 13, true);
			this.HouseBillLabel.Text = "Master Bill:";
			// 
			// PrepaidCollectLabel
			// 
			this.PrepaidCollectLabel.TabIndex = 17;
			// 
			// PrepaidCollectDropEdit
			// 
			this.PrepaidCollectDropEdit.TabIndex = 18;
			// 
			// StatusTextBox
			// 
			this.StatusTextBox.TabIndex = 22;
			// 
			// DestLabel
			// 
			this.DestLabel.TabIndex = 6;
			// 
			// OriginLabel
			// 
			this.OriginLabel.TabIndex = 4;
			// 
			// GoodsValueCalcDropEdit
			// 
			this.goodsValueCalcDropEdit.TabIndex = 15;
			// 
			// GoodsValLabel
			// 
			this.GoodsValLabel.TabIndex = 14;
			// 
			// WeightLabel
			// 
			this.WeightLabel.TabIndex = 8;
			// 
			// SACCheckBox
			// 
			this.SACCheckBox.TabIndex = 16;
			// 
			// PackagesLabel
			// 
			this.PackagesLabel.TabIndex = 10;
			// 
			// PersonalEffectsCheckBox
			// 
			this.PersonalEffectsCheckBox.TabIndex = 21;
			// 
			// NonCustomsItemsGroupBox
			// 
			this.NonCustomsItemsGroupBox.TabIndex = 18;
			// 
			// ServiceLevelCodeFindBox
			// 
			this.ServiceLevelCodeFindBox.TabIndex = 8;
			// 
			// WarehouseLocationTextBox
			// 
			this.WarehouseLocationTextBox.TabIndex = 1;
			// 
			// FolioReferenceTextBox
			// 
			this.FolioReferenceTextBox.TabIndex = 3;
			// 
			// ChargeableWeightCalcEdit
			// 
			this.ChargeableWeightCalcEdit.TabIndex = 5;
			// 
			// GoodsDescriptionLabel
			// 
			this.GoodsDescriptionLabel.TabIndex = 12;
			// 
			// LoadPortCodeFindBox
			// 
			this.LoadPortCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LoadPortCodeFindBox, "CS_RL_NKLoadPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(null)).CS_RL_NKLoadPort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(null)).Lookups.OriginList)));
			this.LoadPortCodeFindBox.BindToList = "Lookups+OriginList";
			this.LoadPortCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 34, true);
			this.LoadPortCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.LoadPortCodeFindBox.Name = "LoadPortCodeFindBox";
			this.LoadPortCodeFindBox.PreBoundMaxLength = 5;
			this.LoadPortCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 20, true);
			this.LoadPortCodeFindBox.TabIndex = 3;
			// 
			// LoadLabel
			// 
			this.LoadLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 34, true);
			this.LoadLabel.Name = "LoadLabel";
			this.LoadLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 16, true);
			this.LoadLabel.TabIndex = 2;
			this.LoadLabel.Text = "Load:";
			// 
			// CTOHouseDetailsUserControl
			// 
			this.Name = "CTOHouseDetailsUserControl";
			this.HouseGroupBox.ResumeLayout(false);
			this.HouseGroupBox.PerformLayout();
			this.shipmentTypeDropEdit.ResumeLayout(true);
			this.shipmentTypeDropEdit.PerformLayout();
			this.WeightCalcDropEdit.ResumeLayout(true);
			this.WeightCalcDropEdit.PerformLayout();
			this.DestinationFindBox.ResumeLayout(true);
			this.DestinationFindBox.PerformLayout();
			this.OriginFindBox.ResumeLayout(true);
			this.OriginFindBox.PerformLayout();
			this.PrepaidCollectDropEdit.ResumeLayout(true);
			this.PrepaidCollectDropEdit.PerformLayout();
			this.goodsValueCalcDropEdit.ResumeLayout(true);
			this.goodsValueCalcDropEdit.PerformLayout();
			this.NonCustomsItemsGroupBox.ResumeLayout(false);
			this.NonCustomsItemsGroupBox.PerformLayout();
			this.ServiceLevelCodeFindBox.ResumeLayout(true);
			this.ServiceLevelCodeFindBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LoadPortCodeFindBox.ResumeLayout(true);
			this.LoadPortCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		protected internal ZArchitecture.GUI.ZCodeFindBox LoadPortCodeFindBox;
		protected internal ZArchitecture.ZLabel LoadLabel;
	}
}
