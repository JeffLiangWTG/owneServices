using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.GUI
{
	partial class GlobalTrackingShipmentVisibilityOptionsRegistryItemControl
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
			this.optionGroupBox = new CargoWise.Windows.UI.KGroupBox();
			this.ContainerAutomationGroupBox = new CargoWise.Windows.UI.KGroupBox();
			this.noRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.yesRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.overrideDefault = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.shipmentVisibilityServiceEhubIDsGrid = new Enterprise.ZArchitecture.ZGrid();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ContainerAutomationGroupBox.SuspendLayout();
			this.optionGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.GlobalTrackingShipmentVisibilityOptions);
			// 
			// OptionGroupBox
			// 
			this.optionGroupBox.Controls.Add(this.noRadioButton);
			this.optionGroupBox.Controls.Add(this.yesRadioButton);
			this.optionGroupBox.Font = new System.Drawing.Font(OFont.NormalFontName, 8F, System.Drawing.FontStyle.Bold);
			this.optionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.optionGroupBox.Name = "optionGroupBox";
			this.optionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 70, true);
			this.optionGroupBox.TabIndex = 0;
			this.optionGroupBox.TabStop = false;
			// 
			// NoRadioButton
			//
			this.BindingSource.SetBindingMember(this.noRadioButton, "InActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.GlobalTrackingShipmentVisibilityOptions)(null)).InActive)));
			this.noRadioButton.AutoCheck = false;
			this.noRadioButton.AutoSize = true;
			this.noRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.noRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(81, 24, true);
			this.noRadioButton.Name = "noRadioButton";
			this.noRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 13, true);
			this.noRadioButton.TabIndex = 3;
			this.noRadioButton.TabStop = true;
			this.noRadioButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("59774443-1887-4756-99f1-e7cfef7fdd22", "No");
			this.noRadioButton.BackColor = System.Drawing.Color.Transparent;
			this.noRadioButton.UseVisualStyleBackColor = false;
			// 
			// YesRadioButton
			// 
			this.yesRadioButton.AutoCheck = false;
			this.yesRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.yesRadioButton, "Active");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.GlobalTrackingShipmentVisibilityOptions)(null)).Active)));
			this.yesRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.yesRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 24, true);
			this.yesRadioButton.Name = "yesRadioButton";
			this.yesRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 13, true);
			this.yesRadioButton.TabIndex = 2;
			this.yesRadioButton.TabStop = true;
			this.yesRadioButton.BackColor = System.Drawing.Color.Transparent;
			this.yesRadioButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("44774443-1887-4751-99f1-e7cfef7fdd10", "YES");
			this.yesRadioButton.UseVisualStyleBackColor = false;
			//
			// ContainerAutomationGroupBox
			//
			this.ContainerAutomationGroupBox.Controls.Add(this.overrideDefault);
			this.ContainerAutomationGroupBox.Controls.Add(this.shipmentVisibilityServiceEhubIDsGrid);
			this.ContainerAutomationGroupBox.Font = new System.Drawing.Font(OFont.NormalFontName, 8F, System.Drawing.FontStyle.Bold);
			this.ContainerAutomationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 72, true);
			this.ContainerAutomationGroupBox.Name = "ContainerAutomationGroupBox";
			this.ContainerAutomationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(568, 200, true);
			this.ContainerAutomationGroupBox.TabIndex = 4;
			this.ContainerAutomationGroupBox.TabStop = false;
			this.ContainerAutomationGroupBox.Text = "Shipment Visibility eHub ID";
			// 
			// overrideDefault
			//
			this.BindingSource.SetBindingMember(this.overrideDefault, "IsDefaultContainerAutomation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.GlobalTrackingShipmentVisibilityOptions)(null)).IsDefaultContainerAutomation)));
			this.overrideDefault.AutoSize = true;
			this.overrideDefault.Font = new System.Drawing.Font(OFont.NormalFontName, 8F, System.Drawing.FontStyle.Regular);
			this.overrideDefault.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.overrideDefault.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 25, true);
			this.overrideDefault.Name = "overrideDefault";
			this.overrideDefault.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 13, true);
			this.overrideDefault.TabIndex = 5;
			this.overrideDefault.TabStop = true;
			this.overrideDefault.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("59771143-1887-4756-99f1-e7cfef7fdd11", "Override Default");
			this.overrideDefault.BackColor = System.Drawing.Color.Transparent;
			this.overrideDefault.UseVisualStyleBackColor = false;
			// 
			// shipmentVisibilityEhubGrid
			//
			this.BindingSource.SetBindingMember(this.shipmentVisibilityServiceEhubIDsGrid, "ServiceEhubIDs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.GlobalTrackingShipmentVisibilityOptions)(null)).ServiceEhubIDs)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.GlobalTrackingShipmentVisibilityServiceEhubID)(((System.Collections.IList)(((Enterprise.Registry.Business.GlobalTrackingShipmentVisibilityOptions)(null)).ServiceEhubIDs)).SyncRoot)).Service)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.GlobalTrackingShipmentVisibilityServiceEhubID)(((System.Collections.IList)(((Enterprise.Registry.Business.GlobalTrackingShipmentVisibilityOptions)(null)).ServiceEhubIDs)).SyncRoot)).EhubID)));
			this.shipmentVisibilityServiceEhubIDsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("19f4b789-43ce-44b1-90b7-937fb69e88a9", "Service");
			zTextBoxColumnStyleInfo1.ColumnName = "Service";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("736b2d98-4bac-4f17-9620-45df1b1185c7", "eHub ID");
			zTextBoxColumnStyleInfo2.ColumnName = "EhubID";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(220);
			
			this.shipmentVisibilityServiceEhubIDsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.shipmentVisibilityServiceEhubIDsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.shipmentVisibilityServiceEhubIDsGrid.GridId = "ade54994-8108-41e7-9999-361a0d5980bd";
			this.shipmentVisibilityServiceEhubIDsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.shipmentVisibilityServiceEhubIDsGrid.LayoutKey = "shipmentVisibilityEhubGrid";
			this.shipmentVisibilityServiceEhubIDsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 50, true);
			this.shipmentVisibilityServiceEhubIDsGrid.Name = "shipmentVisibilityEhubGrid";
			this.shipmentVisibilityServiceEhubIDsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(553, 120, true);
			this.shipmentVisibilityServiceEhubIDsGrid.Font = new System.Drawing.Font(OFont.NormalFontName, 8F, System.Drawing.FontStyle.Regular);
			this.shipmentVisibilityServiceEhubIDsGrid.TabIndex = 5;
			// 
			// GlobalTrackingShipmentVisibilityOptionsRegistryItemControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.optionGroupBox);
			this.Controls.Add(this.ContainerAutomationGroupBox);
			this.Name = "GlobalTrackingShipmentVisibilityOptionsRegistryItemControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(650, 300, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ContainerAutomationGroupBox.ResumeLayout(false);
			this.optionGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		protected Enterprise.ZArchitecture.GUI.ZRadioButton noRadioButton;
		protected CargoWise.Windows.UI.KGroupBox optionGroupBox;
		protected CargoWise.Windows.UI.KGroupBox ContainerAutomationGroupBox;
		protected ZArchitecture.GUI.ZRadioButton yesRadioButton;
		protected ZArchitecture.GUI.ZCheckBox overrideDefault;
		protected ZArchitecture.ZGrid shipmentVisibilityServiceEhubIDsGrid;
		#endregion
	}
}
