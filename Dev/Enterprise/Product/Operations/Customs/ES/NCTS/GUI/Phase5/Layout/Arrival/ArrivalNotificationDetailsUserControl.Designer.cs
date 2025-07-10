namespace Enterprise.Customs.ES.NCTS.GUI
{
	partial class ArrivalNotificationDetailsUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ArrivalGoodsLocationCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.RepresentativeTraderZDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.BrokerCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CertificateDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AdditionalArrivalNotificationDetailsUserControl = new Enterprise.Customs.ES.NCTS.GUI.AdditionalArrivalNotificationDetailsUserControl();
			this.TrainingCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ArrivalGoodsLocationCodeFindBox.SuspendLayout();
			this.RepresentativeTraderZDocAddressControl.SuspendLayout();
			this.BrokerCodeFindBox.SuspendLayout();
			this.CertificateDropEdit.SuspendLayout();
			this.AdditionalArrivalNotificationDetailsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.NCTS.Business.NctsHeader);
			// 
			// ArrivalGoodsLocationCodeFindBox
			// 
			this.ArrivalGoodsLocationCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ArrivalGoodsLocationCodeFindBox, "ArrivalMovementHeader.GoodsLocation.CGL_AdditionalIdentifier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.GoodsLocation.CGL_AdditionalIdentifier)));
			this.ArrivalGoodsLocationCodeFindBox.CaptionResourceString = Enterprise.Customs.ES.NCTS.GUI.Res.GetData("B68462D4-7627-465D-B238-279BDCCFF977", "Arrival Goods Location");
			this.ArrivalGoodsLocationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ArrivalGoodsLocationCodeFindBox.Name = "ArrivalGoodsLocationCodeFindBox";
			this.ArrivalGoodsLocationCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ArrivalGoodsLocationCodeFindBox.ParentType = null;
			this.ArrivalGoodsLocationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 17, true);
			this.ArrivalGoodsLocationCodeFindBox.TabIndex = 0;
			// 
			// RepresentativeTraderZDocAddressControl
			// 
			this.RepresentativeTraderZDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RepresentativeTraderZDocAddressControl, "ArrivalMovementHeader.Representative");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.ES.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.Representative)));
			this.RepresentativeTraderZDocAddressControl.BindToOrganisations = "Lookups.Organisations";
			this.RepresentativeTraderZDocAddressControl.CaptionResourceString = Enterprise.Customs.ES.NCTS.GUI.Res.GetData("E0FB3323-4BF2-4274-8888-B42755C90C99", "Rep. Trader");
			this.RepresentativeTraderZDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
			this.RepresentativeTraderZDocAddressControl.SingleLineNoGroupBoxPanelWidth = 320;
			this.RepresentativeTraderZDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 30, true);
			this.RepresentativeTraderZDocAddressControl.Name = "RepresentativeTraderZDocAddressControl";
			this.RepresentativeTraderZDocAddressControl.ReadOnly = false;
			this.RepresentativeTraderZDocAddressControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.RepresentativeTraderZDocAddressControl.TabIndex = 1;
			this.RepresentativeTraderZDocAddressControl.SingleLineNoGroupBoxPanelWidth = 240;
			// 
			// BrokerCodeFindBox
			// 
			this.BrokerCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BrokerCodeFindBox, "ArrivalMovementHeader.BM_GS_NKCusAgent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.BM_GS_NKCusAgent)));
			this.BrokerCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 39, true);
			this.BrokerCodeFindBox.Name = "BrokerCodeFindBox";
			this.BrokerCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.BrokerCodeFindBox.ParentType = null;
			this.BrokerCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 17, true);
			this.BrokerCodeFindBox.TabIndex = 1;
			// 
			// CertificateDropEdit
			// 
			this.CertificateDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CertificateDropEdit, "BH_CustomsProfile");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.NCTS.Business.NctsHeader)(null)).BH_CustomsProfile)));
			this.CertificateDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 72, true);
			this.CertificateDropEdit.Name = "CertificateDropEdit";
			this.CertificateDropEdit.ShouldResizeByMaxLength = false;
			this.CertificateDropEdit.ShowDescriptionBox = false;
			this.CertificateDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 17, true);
			this.CertificateDropEdit.TabIndex = 13;
			this.CertificateDropEdit.UseFullWidthForCodeBox = true;
			// 
			// TrainingCheckBox
			// 
			this.BindingSource.SetBindingMember(this.TrainingCheckBox, "TrainingEntry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.NCTS.Business.NctsHeader)(null)).TrainingEntry)));
			this.TrainingCheckBox.CaptionResourceString = Enterprise.Customs.ES.NCTS.GUI.Res.GetData("8E28AC1C-F2A5-45F7-AAB2-CE0B969E19A4", englishCaption: "Training Entry", englishMediumCaption: "Training Entry", englishShortCaption: "Training Entry", englishFullDescription: "When checked the declaration will be sent to Test");
			this.TrainingCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.TrainingCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 86, true);
			this.TrainingCheckBox.Name = "TrainingCheckBox";
			this.TrainingCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 19, true);
			this.TrainingCheckBox.TabIndex = 4;
			this.TrainingCheckBox.UseVisualStyleBackColor = true;
			// 
			// AdditionalArrivalNotificationDetailsUserControl
			// 
			this.AdditionalArrivalNotificationDetailsUserControl.AllowDrop = true;
			this.AdditionalArrivalNotificationDetailsUserControl.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AdditionalArrivalNotificationDetailsUserControl, ".");
			this.AdditionalArrivalNotificationDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(362, 0, true);
			this.AdditionalArrivalNotificationDetailsUserControl.Name = "AdditionalArrivalNotificationDetailsUserControl";
			this.AdditionalArrivalNotificationDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(521, 75, true);
			this.AdditionalArrivalNotificationDetailsUserControl.TabIndex = 14;
			// 
			// ArrivalNotificationDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ArrivalGoodsLocationCodeFindBox);
			this.Controls.Add(this.RepresentativeTraderZDocAddressControl);
			this.Controls.Add(this.BrokerCodeFindBox);
			this.Controls.Add(this.CertificateDropEdit);
			this.Controls.Add(this.AdditionalArrivalNotificationDetailsUserControl);
			this.Controls.Add(this.TrainingCheckBox);

			this.Name = "ArrivalNotificationDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 116, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ArrivalGoodsLocationCodeFindBox.ResumeLayout(true);
			this.ArrivalGoodsLocationCodeFindBox.PerformLayout();
			this.RepresentativeTraderZDocAddressControl.ResumeLayout(true);
			this.RepresentativeTraderZDocAddressControl.PerformLayout();
			this.BrokerCodeFindBox.ResumeLayout(true);
			this.BrokerCodeFindBox.PerformLayout();
			this.CertificateDropEdit.ResumeLayout(true);
			this.CertificateDropEdit.PerformLayout();
			this.AdditionalArrivalNotificationDetailsUserControl.ResumeLayout(true);
			this.AdditionalArrivalNotificationDetailsUserControl.PerformLayout();

			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZCodeFindBox ArrivalGoodsLocationCodeFindBox;
		internal MasterFiles.GUI.ZDocAddressControl RepresentativeTraderZDocAddressControl;
		internal ZArchitecture.GUI.ZCodeFindBox BrokerCodeFindBox;
		internal ZArchitecture.GUI.ZDropEdit CertificateDropEdit;
		internal Enterprise.Customs.ES.NCTS.GUI.AdditionalArrivalNotificationDetailsUserControl AdditionalArrivalNotificationDetailsUserControl;
		internal ZArchitecture.GUI.ZCheckBox TrainingCheckBox;
	}
}
