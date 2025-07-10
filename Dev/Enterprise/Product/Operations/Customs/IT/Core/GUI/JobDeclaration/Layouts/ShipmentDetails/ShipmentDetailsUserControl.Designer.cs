namespace Enterprise.Customs.IT.GUI
{
	partial class ShipmentDetailsUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ShipmentDetailsOriginUserControl = new Enterprise.Customs.IT.GUI.ShipmentDetailsOriginUserControl();
			this.ShipmentDetailsFinalDestinationUserControl = new Enterprise.Customs.IT.GUI.ShipmentDetailsFinalDestinationUserControl();
			this.LocationQualifierDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.GoodsLocationDUserControl = new Enterprise.Customs.IT.GUI.GoodsLocationDUserControl();
			this.GoodsLocationFUserControl = new Enterprise.Customs.IT.GUI.GoodsLocationFUserControl();
			this.GoodsLocationFCUserControl = new Enterprise.Customs.IT.GUI.GoodsLocationFCUserControl();
			this.GoodsLocationLBLCUserControl = new Enterprise.Customs.IT.GUI.GoodsLocationLBLCUserControl();
			this.SubLocationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LocationOfGoodsUserControl = new Enterprise.Customs.IT.GUI.LocationOfGoodsUserControl();
			this.AdditionalDeliveryTermsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ShipmentDetailsOriginUserControl.SuspendLayout();
			this.ShipmentDetailsFinalDestinationUserControl.SuspendLayout();
			this.LocationQualifierDropEdit.SuspendLayout();
			this.GoodsLocationDUserControl.SuspendLayout();
			this.GoodsLocationFUserControl.SuspendLayout();
			this.GoodsLocationFCUserControl.SuspendLayout();
			this.GoodsLocationLBLCUserControl.SuspendLayout();
			this.LocationOfGoodsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IT.Business.Declaration.JobDeclaration);
			// 
			// ShipmentDetailsOriginUserControl
			// 
			this.ShipmentDetailsOriginUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipmentDetailsOriginUserControl, ".");
			this.ShipmentDetailsOriginUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 29, true);
			this.ShipmentDetailsOriginUserControl.Name = "ShipmentDetailsOriginUserControl";
			this.ShipmentDetailsOriginUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
			this.ShipmentDetailsOriginUserControl.TabIndex = 0;
			// 
			// ShipmentDetailsFinalDestinationUserControl
			// 
			this.ShipmentDetailsFinalDestinationUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipmentDetailsFinalDestinationUserControl, ".");
			this.ShipmentDetailsFinalDestinationUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ShipmentDetailsFinalDestinationUserControl.Name = "ShipmentDetailsFinalDestinationUserControl";
			this.ShipmentDetailsFinalDestinationUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
			this.ShipmentDetailsFinalDestinationUserControl.TabIndex = 1;
			// 
			// LocationQualifierDropEdit
			// 
			this.LocationQualifierDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LocationQualifierDropEdit, "JE_LocationQualifier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)).JE_LocationQualifier)));
			this.LocationQualifierDropEdit.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("0EF3D939-747A-41B6-BF25-03A38954AB69", "[30] Goods Location");
			this.LocationQualifierDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 58, true);
			this.LocationQualifierDropEdit.Name = "LocationQualifierDropEdit";
			this.LocationQualifierDropEdit.PreBoundMaxLength = 2;
			this.LocationQualifierDropEdit.ShowDescriptionBox = false;
			this.LocationQualifierDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 20, true);
			this.LocationQualifierDropEdit.TabIndex = 2;
			// 
			// GoodsLocationDUserControl
			// 
			this.GoodsLocationDUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsLocationDUserControl, ".");
			this.GoodsLocationDUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 84, true);
			this.GoodsLocationDUserControl.Name = "GoodsLocationDUserControl";
			this.GoodsLocationDUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(309, 23, true);
			this.GoodsLocationDUserControl.TabIndex = 3;
			// 
			// GoodsLocationFUserControl
			// 
			this.GoodsLocationFUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsLocationFUserControl, ".");
			this.GoodsLocationFUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 113, true);
			this.GoodsLocationFUserControl.Name = "GoodsLocationFUserControl";
			this.GoodsLocationFUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(355, 23, true);
			this.GoodsLocationFUserControl.TabIndex = 4;
			// 
			// GoodsLocationFCUserControl
			// 
			this.GoodsLocationFCUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsLocationFCUserControl, ".");
			this.GoodsLocationFCUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 142, true);
			this.GoodsLocationFCUserControl.Name = "GoodsLocationFCUserControl";
			this.GoodsLocationFCUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(217, 23, true);
			this.GoodsLocationFCUserControl.TabIndex = 5;
			// 
			// GoodsLocationLBLCUserControl
			// 
			this.GoodsLocationLBLCUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsLocationLBLCUserControl, ".");
			this.GoodsLocationLBLCUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(-1, 171, true);
			this.GoodsLocationLBLCUserControl.Name = "GoodsLocationLBLCUserControl";
			this.GoodsLocationLBLCUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(339, 23, true);
			this.GoodsLocationLBLCUserControl.TabIndex = 6;
			// 
			// SubLocationTextBox
			// 
			this.BindingSource.SetBindingMember(this.SubLocationTextBox, "JE_SubLocationOfGoods");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)).JE_SubLocationOfGoods)));
			this.SubLocationTextBox.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("87DD5966-9DE8-4C4E-B13E-438296401D30", "Place Of Unloading");
			this.SubLocationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(-1, 200, true);
			this.SubLocationTextBox.Name = "SubLocationTextBox";
			this.SubLocationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.SubLocationTextBox.TabIndex = 7;
			// 
			// LocationOfGoodsUserControl
			// 
			this.LocationOfGoodsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LocationOfGoodsUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.Business.ICusGoodsLocationProvider)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)))));
			this.LocationOfGoodsUserControl.CusGoodsLocationProviderType = null;
			this.LocationOfGoodsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 226, true);
			this.LocationOfGoodsUserControl.Name = "LocationOfGoodsUserControl";
			this.LocationOfGoodsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 21, true);
			this.LocationOfGoodsUserControl.TabIndex = 8;
			// 
			// AdditionalDeliveryTermsTextBox
			// 
			this.BindingSource.SetBindingMember(this.AdditionalDeliveryTermsTextBox, "ZG_AdditionalDeliveryTerms");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)).ZG_AdditionalDeliveryTerms)));
			this.AdditionalDeliveryTermsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(-1, 253, true);
			this.AdditionalDeliveryTermsTextBox.Name = "AdditionalDeliveryTermsTextBox";
			this.AdditionalDeliveryTermsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.AdditionalDeliveryTermsTextBox.TabIndex = 9;
			// 
			// ShipmentDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ShipmentDetailsOriginUserControl);
			this.Controls.Add(this.ShipmentDetailsFinalDestinationUserControl);
			this.Controls.Add(this.LocationQualifierDropEdit);
			this.Controls.Add(this.GoodsLocationDUserControl);
			this.Controls.Add(this.GoodsLocationFUserControl);
			this.Controls.Add(this.GoodsLocationFCUserControl);
			this.Controls.Add(this.GoodsLocationLBLCUserControl);
			this.Controls.Add(this.SubLocationTextBox);
			this.Controls.Add(this.LocationOfGoodsUserControl);
			this.Controls.Add(this.AdditionalDeliveryTermsTextBox);
			this.Name = "ShipmentDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(466, 291, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ShipmentDetailsOriginUserControl.ResumeLayout(true);
			this.ShipmentDetailsOriginUserControl.PerformLayout();
			this.ShipmentDetailsFinalDestinationUserControl.ResumeLayout(true);
			this.ShipmentDetailsFinalDestinationUserControl.PerformLayout();
			this.LocationQualifierDropEdit.ResumeLayout(true);
			this.LocationQualifierDropEdit.PerformLayout();
			this.GoodsLocationDUserControl.ResumeLayout(true);
			this.GoodsLocationDUserControl.PerformLayout();
			this.GoodsLocationFUserControl.ResumeLayout(true);
			this.GoodsLocationFUserControl.PerformLayout();
			this.GoodsLocationFCUserControl.ResumeLayout(true);
			this.GoodsLocationFCUserControl.PerformLayout();
			this.GoodsLocationLBLCUserControl.ResumeLayout(true);
			this.GoodsLocationLBLCUserControl.PerformLayout();
			this.LocationOfGoodsUserControl.ResumeLayout(true);
			this.LocationOfGoodsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.Customs.IT.GUI.ShipmentDetailsOriginUserControl ShipmentDetailsOriginUserControl;
		internal Enterprise.Customs.IT.GUI.ShipmentDetailsFinalDestinationUserControl ShipmentDetailsFinalDestinationUserControl;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit LocationQualifierDropEdit;
		internal Enterprise.Customs.IT.GUI.GoodsLocationDUserControl GoodsLocationDUserControl;
		internal Enterprise.Customs.IT.GUI.GoodsLocationFUserControl GoodsLocationFUserControl;
		internal Enterprise.Customs.IT.GUI.GoodsLocationFCUserControl GoodsLocationFCUserControl;
		internal Enterprise.Customs.IT.GUI.GoodsLocationLBLCUserControl GoodsLocationLBLCUserControl;
		internal Enterprise.ZArchitecture.ZTextBox SubLocationTextBox;
		internal Enterprise.Customs.IT.GUI.LocationOfGoodsUserControl LocationOfGoodsUserControl;
		internal Enterprise.ZArchitecture.ZTextBox AdditionalDeliveryTermsTextBox;
	}
}
