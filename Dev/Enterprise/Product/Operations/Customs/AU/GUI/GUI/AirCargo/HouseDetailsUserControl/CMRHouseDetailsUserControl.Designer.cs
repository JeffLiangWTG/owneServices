namespace Enterprise.Customs.AU.AirCargo.GUI
{
	public partial class CMRHouseDetailsUserControl
	{
		private void InitializeComponent()
		{
			this.coLoadMasterTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.masterBillLabel = new Enterprise.ZArchitecture.ZLabel();
			this.HVLVSpecialReporterCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.masterHouseBillCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.remailSpecialReporterCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.HouseGroupBox.SuspendLayout();
			this.shipmentTypeDropEdit.SuspendLayout();
			this.WeightCalcDropEdit.SuspendLayout();
			this.GoodsDescriptionTextBox.SuspendLayout();
			this.DestinationFindBox.SuspendLayout();
			this.OriginFindBox.SuspendLayout();
			this.PrepaidCollectDropEdit.SuspendLayout();
			this.goodsValueCalcDropEdit.SuspendLayout();
			this.NonCustomsItemsGroupBox.SuspendLayout();
			this.ServiceLevelCodeFindBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// HouseGroupBox
			// 
			this.HouseGroupBox.Controls.Add(this.remailSpecialReporterCheckBox);
			this.HouseGroupBox.Controls.Add(this.HVLVSpecialReporterCheckBox);
			this.HouseGroupBox.Controls.Add(this.masterBillLabel);
			this.HouseGroupBox.Controls.Add(this.coLoadMasterTextBox);
			this.HouseGroupBox.Controls.Add(this.masterHouseBillCheckBox);
			this.HouseGroupBox.Controls.SetChildIndex(this.masterHouseBillCheckBox, 0);
			this.HouseGroupBox.Controls.SetChildIndex(this.coLoadMasterTextBox, 0);
			this.HouseGroupBox.Controls.SetChildIndex(this.masterBillLabel, 0);
			this.HouseGroupBox.Controls.SetChildIndex(this.HVLVSpecialReporterCheckBox, 0);
			this.HouseGroupBox.Controls.SetChildIndex(this.remailSpecialReporterCheckBox, 0);
			this.HouseGroupBox.Controls.SetChildIndex(this.PackagesLabel, 0);
			this.HouseGroupBox.Controls.SetChildIndex(this.WeightLabel, 0);
			this.HouseGroupBox.Controls.SetChildIndex(this.GoodsValLabel, 0);
			this.HouseGroupBox.Controls.SetChildIndex(this.PrepaidCollectLabel, 0);
			this.HouseGroupBox.Controls.SetChildIndex(this.HouseBillLabel, 0);
			this.HouseGroupBox.Controls.SetChildIndex(this.OriginLabel, 0);
			this.HouseGroupBox.Controls.SetChildIndex(this.DestLabel, 0);
			this.HouseGroupBox.Controls.SetChildIndex(this.HouseBillTextBox, 0);
			this.HouseGroupBox.Controls.SetChildIndex(this.OriginFindBox, 0);
			this.HouseGroupBox.Controls.SetChildIndex(this.DestinationFindBox, 0);
			this.HouseGroupBox.Controls.SetChildIndex(this.PiecesManifestedCalcEdit, 0);
			this.HouseGroupBox.Controls.SetChildIndex(this.WeightCalcDropEdit, 0);
			this.HouseGroupBox.Controls.SetChildIndex(this.shipmentTypeLabel, 0);
			this.HouseGroupBox.Controls.SetChildIndex(this.shipmentTypeDropEdit, 0);
			this.HouseGroupBox.Controls.SetChildIndex(this.StatusTextBox, 0);
			this.HouseGroupBox.Controls.SetChildIndex(this.goodsValueCalcDropEdit, 0);
			this.HouseGroupBox.Controls.SetChildIndex(this.SACCheckBox, 0);
			this.HouseGroupBox.Controls.SetChildIndex(this.PersonalEffectsCheckBox, 0);
			this.HouseGroupBox.Controls.SetChildIndex(this.PrepaidCollectDropEdit, 0);
			this.HouseGroupBox.Controls.SetChildIndex(this.NonCustomsItemsGroupBox, 0);
			this.HouseGroupBox.Controls.SetChildIndex(this.GoodsDescriptionLabel, 0);
			this.HouseGroupBox.Controls.SetChildIndex(this.MessageStatusTextBox, 0);
			this.HouseGroupBox.Controls.SetChildIndex(this.GoodsDescriptionTextBox, 0);
			this.HouseGroupBox.Controls.SetChildIndex(this.TranshipmentTextBox, 0);
			this.HouseGroupBox.Controls.SetChildIndex(this.TranshipmentLabel, 0);
			this.HouseGroupBox.Controls.SetChildIndex(this.responsiblePartyTextBox, 0);
			this.HouseGroupBox.Controls.SetChildIndex(this.responsiblePartyLabel, 0);
			this.HouseGroupBox.Controls.SetChildIndex(this.ConRefLabel, 0);
			// 
			// ShipmentTypeDropEdit
			// 
			this.shipmentTypeDropEdit.TabIndex = 22;
			// 
			// shipmentTypeLabel
			// 
			this.shipmentTypeLabel.TabIndex = 21;
			// 
			// WeightCalcDropEdit
			// 
			this.WeightCalcDropEdit.TabIndex = 11;
			// 
			// PiecesManifestedCalcEdit
			// 
			this.PiecesManifestedCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(280, 105, true);
			this.PiecesManifestedCalcEdit.TabIndex = 13;
			// 
			// GoodsDescriptionTextBox
			// 
			this.GoodsDescriptionTextBox.TabIndex = 15;
			// 
			// DestinationFindBox
			// 
			this.DestinationFindBox.TabIndex = 9;
			// 
			// OriginFindBox
			// 
			this.OriginFindBox.TabIndex = 7;
			// 
			// PrepaidCollectLabel
			// 
			this.PrepaidCollectLabel.TabIndex = 19;
			// 
			// PrepaidCollectDropEdit
			// 
			this.PrepaidCollectDropEdit.TabIndex = 20;
			// 
			// StatusTextBox
			// 
			this.StatusTextBox.TabIndex = 24;
			// 
			// DestLabel
			// 
			this.DestLabel.TabIndex = 8;
			// 
			// OriginLabel
			// 
			this.OriginLabel.TabIndex = 6;
			// 
			// GoodsValueCalcDropEdit
			// 
			this.goodsValueCalcDropEdit.TabIndex = 17;
			// 
			// GoodsValLabel
			// 
			this.GoodsValLabel.TabIndex = 16;
			// 
			// WeightLabel
			// 
			this.WeightLabel.TabIndex = 10;
			// 
			// SACCheckBox
			// 
			this.SACCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(216, 152, true);
			this.SACCheckBox.TabIndex = 18;
			// 
			// PackagesLabel
			// 
			this.PackagesLabel.TabIndex = 12;
			// 
			// PersonalEffectsCheckBox
			// 
			this.PersonalEffectsCheckBox.TabIndex = 23;
			// 
			// GoodsDescriptionLabel
			// 
			this.GoodsDescriptionLabel.TabIndex = 14;
			// 
			// CoLoadMasterTextBox
			// 
			this.BindingSource.SetBindingMember(this.coLoadMasterTextBox, "CS_MasterHouseBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(null)).CS_MasterHouseBill)));
			this.coLoadMasterTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 35, true);
			this.coLoadMasterTextBox.Name = "CoLoadMasterTextBox";
			this.coLoadMasterTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.coLoadMasterTextBox.TabIndex = 4;
			// 
			// MasterBillLabel
			// 
			this.masterBillLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 32, true);
			this.masterBillLabel.Name = "MasterBillLabel";
			this.masterBillLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 23, true);
			this.masterBillLabel.TabIndex = 3;
			this.masterBillLabel.Text = "Master Bill:";
			// 
			// HVLVSpecialReporterCheckBox
			// 
			this.HVLVSpecialReporterCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.HVLVSpecialReporterCheckBox, "CS_IsSpecialReporter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(null)).CS_IsSpecialReporter)));
			this.HVLVSpecialReporterCheckBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("c32a3114-be7f-4d2e-bcb0-6f9a92493b8e", "HVLV", "HVLV Special Reporter", "");
			this.HVLVSpecialReporterCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.HVLVSpecialReporterCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(183, 36, true);
			this.HVLVSpecialReporterCheckBox.Name = "HVLVSpecialReporterCheckBox";
			this.HVLVSpecialReporterCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(54, 17, true);
			this.HVLVSpecialReporterCheckBox.TabIndex = 5;
			this.HVLVSpecialReporterCheckBox.Text = "HVLV";
			// 
			// MasterHouseBillCheckBox
			// 
			this.masterHouseBillCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.masterHouseBillCheckBox, "CS_IsMasterHouse");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(null)).CS_IsMasterHouse)));
			this.masterHouseBillCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.masterHouseBillCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(225, 13, true);
			this.masterHouseBillCheckBox.Name = "MasterHouseBillCheckBox";
			this.masterHouseBillCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 17, true);
			this.masterHouseBillCheckBox.TabIndex = 2;
			this.masterHouseBillCheckBox.Text = "Is a M-House?";
			// 
			// RemailSpecialReporterCheckBox
			// 
			this.remailSpecialReporterCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.remailSpecialReporterCheckBox, "CS_IsRemailReporter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(null)).CS_IsRemailReporter)));
			this.remailSpecialReporterCheckBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("5dad62ff-3843-4877-9018-84bba9f137be", "Re-mail", "Re-mail Special Reporter", "");
			this.remailSpecialReporterCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.remailSpecialReporterCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(234, 36, true);
			this.remailSpecialReporterCheckBox.Name = "RemailSpecialReporterCheckBox";
			this.remailSpecialReporterCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(81, 17, true);
			this.remailSpecialReporterCheckBox.TabIndex = 33;
			this.remailSpecialReporterCheckBox.Text = "Remail Rptr";
			this.remailSpecialReporterCheckBox.UseVisualStyleBackColor = true;
			// 
			// CMRHouseDetailsUserControl
			// 
			this.Name = "CMRHouseDetailsUserControl";
			this.HouseGroupBox.ResumeLayout(false);
			this.HouseGroupBox.PerformLayout();
			this.shipmentTypeDropEdit.ResumeLayout(true);
			this.shipmentTypeDropEdit.PerformLayout();
			this.WeightCalcDropEdit.ResumeLayout(true);
			this.WeightCalcDropEdit.PerformLayout();
			this.GoodsDescriptionTextBox.ResumeLayout(true);
			this.GoodsDescriptionTextBox.PerformLayout();
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
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private ZArchitecture.GUI.ZCheckBox masterHouseBillCheckBox;
		private ZArchitecture.ZTextBox coLoadMasterTextBox;
		private ZArchitecture.ZLabel masterBillLabel;
		internal ZArchitecture.GUI.ZCheckBox HVLVSpecialReporterCheckBox;
		private ZArchitecture.GUI.ZCheckBox remailSpecialReporterCheckBox;
	}
}
