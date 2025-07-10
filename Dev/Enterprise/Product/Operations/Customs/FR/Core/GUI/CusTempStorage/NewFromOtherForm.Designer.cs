namespace Enterprise.Customs.FR.GUI.CusTempStorage
{
	partial class NewFromOtherForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
			this.shipmentFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.deltaTFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.declarationFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.okButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.shipmentFindBox.SuspendLayout();
			this.deltaTFindBox.SuspendLayout();
			this.declarationFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 204, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(351, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.FR.Business.CusTempStorage.TemporaryStorageWrapperFromParentHelper);
			// 
			// shipmentFindBox
			// 
			this.shipmentFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.shipmentFindBox, "ShipmentPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.FR.Business.CusTempStorage.TemporaryStorageWrapperFromParentHelper)(null)).ShipmentPK)));
			this.shipmentFindBox.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("938cfc67-473e-419a-8a2a-ec78fa35a97b", "Shipment");
			this.shipmentFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(67, 25, true);
			this.shipmentFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.JobShipment;
			this.shipmentFindBox.Name = "shipmentFindBox";
			this.shipmentFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.shipmentFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(269, 20, true);
			this.shipmentFindBox.TabIndex = 1;
			// 
			// deltaTFindBox
			// 
			this.deltaTFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.deltaTFindBox, "DeltaTPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.FR.Business.CusTempStorage.TemporaryStorageWrapperFromParentHelper)(null)).DeltaTPK)));
			this.deltaTFindBox.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("5f971d7a-14b1-4004-9622-3d565a751d49", "Delta T");
			this.deltaTFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(67, 105, true);
			this.deltaTFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.EU.NctsMovementModule;
			this.deltaTFindBox.Name = "deltaTFindBox";
			this.deltaTFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.deltaTFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(269, 20, true);
			this.deltaTFindBox.TabIndex = 3;
			// 
			// declarationFindBox
			// 
			this.declarationFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.declarationFindBox, "DeclarationPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.FR.Business.CusTempStorage.TemporaryStorageWrapperFromParentHelper)(null)).DeclarationPK)));
			this.declarationFindBox.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("1c35fe76-3a78-4825-ad2e-dbbf57275c27", "Declaration");
			this.declarationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(67, 63, true);
			this.declarationFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.JobDeclaration;
			this.declarationFindBox.Name = "declarationFindBox";
			this.declarationFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.declarationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(269, 20, true);
			this.declarationFindBox.TabIndex = 2;
			// 
			// okButton
			// 
			this.okButton.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("3244c62e-c70e-4d38-96b8-a17a731703f0", "OK");
			this.okButton.IsCaptionOverridden = false;
			this.okButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(188, 147, true);
			this.okButton.Name = "okButton";
			this.okButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.okButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 23, true);
			this.okButton.TabIndex = 4;
			this.okButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.okButton.ToolTipCaption = null;
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.OkButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("077ab635-5dbe-4cb7-b40e-a217bda0d309", "Cancel");
			this.cancelButton.IsCaptionOverridden = false;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(257, 147, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 23, true);
			this.cancelButton.TabIndex = 5;
			this.cancelButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.cancelButton.ToolTipCaption = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// NewFromOtherForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(351, 228, true);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.declarationFindBox);
			this.Controls.Add(this.deltaTFindBox);
			this.Controls.Add(this.shipmentFindBox);
			this.DataSourceAssemblyName = "Enterprise.Customs.FR.Business.CusTempStorage";
			this.DataSourceType = typeof(Enterprise.Customs.FR.Business.CusTempStorage.TemporaryStorageWrapperFromParentHelper);
			this.DataSourceTypeName = "Enterprise.Customs.FR.Business.CusTempStorage.TemporaryStorageFromParentHelper";
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(367, 267, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(367, 267, true);
			this.Name = "NewFromOtherForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.shipmentFindBox, 0);
			this.Controls.SetChildIndex(this.deltaTFindBox, 0);
			this.Controls.SetChildIndex(this.declarationFindBox, 0);
			this.Controls.SetChildIndex(this.okButton, 0);
			this.Controls.SetChildIndex(this.cancelButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.shipmentFindBox.ResumeLayout(true);
			this.shipmentFindBox.PerformLayout();
			this.deltaTFindBox.ResumeLayout(true);
			this.deltaTFindBox.PerformLayout();
			this.declarationFindBox.ResumeLayout(true);
			this.declarationFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGuidFindBox shipmentFindBox;
		private ZArchitecture.GUI.ZGuidFindBox deltaTFindBox;
		private ZArchitecture.GUI.ZGuidFindBox declarationFindBox;
		protected Enterprise.ZArchitecture.GUI.ZButton okButton;
		private Enterprise.ZArchitecture.GUI.ZButton cancelButton;
	}
}

