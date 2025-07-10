using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	partial class CalculateDeliveryDueDateOptionsRegistryItemControl
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
			this.noRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.yesRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.transportModeGrid = new Enterprise.ZArchitecture.ZGrid();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.optionGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.CalculateDeliveryDueDateOptions);
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
			this.BindingSource.SetBindingMember(this.noRadioButton, "Inactive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.CalculateDeliveryDueDateOptions)(null)).Inactive)));
			this.noRadioButton.AutoCheck = false;
			this.noRadioButton.AutoSize = true;
			this.noRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.noRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(81, 24, true);
			this.noRadioButton.Name = "noRadioButton";
			this.noRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 13, true);
			this.noRadioButton.TabIndex = 3;
			this.noRadioButton.TabStop = true;
			this.noRadioButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("19771113-1887-4756-99f1-e7cfef7fdd88", "No");
			this.noRadioButton.BackColor = System.Drawing.Color.Transparent;
			this.noRadioButton.UseVisualStyleBackColor = false;
			// 
			// YesRadioButton
			// 
			this.yesRadioButton.AutoCheck = false;
			this.yesRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.yesRadioButton, "Active");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.CalculateDeliveryDueDateOptions)(null)).Active)));
			this.yesRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.yesRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 24, true);
			this.yesRadioButton.Name = "yesRadioButton";
			this.yesRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 13, true);
			this.yesRadioButton.TabIndex = 2;
			this.yesRadioButton.TabStop = true;
			this.yesRadioButton.BackColor = System.Drawing.Color.Transparent;
			this.yesRadioButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("54776663-1887-4751-99f1-e7cfef7fdd22", "Yes");
			this.yesRadioButton.UseVisualStyleBackColor = false;
			// 
			// zPanel1
			// 
			this.zPanel1.Controls.Add(this.transportModeGrid);
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 72, true);
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 100, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.TabIndex = 3;
			//
			// transportModeGrid
			//
			transportModeGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(transportModeGrid, "TransportModes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.CalculateDeliveryDueDateOptions)(null)).TransportModes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.CalculateDeliveryDueDateTransportMode)(((System.Collections.IList)(((Enterprise.Registry.Business.CalculateDeliveryDueDateOptions)(null)).TransportModes)).SyncRoot)).Enabled)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.CalculateDeliveryDueDateTransportMode)(((System.Collections.IList)(((Enterprise.Registry.Business.CalculateDeliveryDueDateOptions)(null)).TransportModes)).SyncRoot)).EnglishDescription)));
			transportModeGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "EnglishDescription";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("14773363-1887-4721-99f1-e7cfef7fdd66", "Description");
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zCheckBoxColumnStyleInfo1.ColumnName = "Enabled";
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("12773363-1887-4711-99f1-e7cfef7fdd12", "Enabled");
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			transportModeGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			transportModeGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			transportModeGrid.GridId = "ae60b322-9e35-48f2-b15f-dee40cec3e61";
			transportModeGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			transportModeGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			transportModeGrid.LayoutKey = "transportModeGrid";
			transportModeGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			transportModeGrid.Name = "transportModeGrid";
			transportModeGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 100, true);
			transportModeGrid.TabIndex = 0;
			// 
			// CalculateDeliveryDueDateOptionsRegistryItemControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.optionGroupBox);
			this.Controls.Add(this.zPanel1);
			this.Name = "CalculateDeliveryDueDateOptionsRegistryItemControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(351, 180, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.optionGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		protected Enterprise.ZArchitecture.GUI.ZRadioButton noRadioButton;
		protected CargoWise.Windows.UI.KGroupBox optionGroupBox;
		protected ZArchitecture.GUI.ZRadioButton yesRadioButton;
		protected Enterprise.ZArchitecture.ZGrid transportModeGrid;
		protected Enterprise.ZArchitecture.GUI.ZPanel zPanel1;

		#endregion
	}
}

