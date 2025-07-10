namespace Enterprise.Registry.GUI
{
	partial class HBLDeliveryPriorityControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		private ZArchitecture.ZLabel hblDeliveryConfigurationLabel;
		protected ZArchitecture.ZGrid hblDeliveryConfigurationGrid;
		private ZArchitecture.ZLabel prioritySettingsLabel;
		protected ZArchitecture.ZGrid prioritySettingsGrid;
		private Enterprise.ZArchitecture.GUI.ZButton UpButton;
		private Enterprise.ZArchitecture.GUI.ZButton DownButton;

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
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.hblDeliveryConfigurationLabel = new Enterprise.ZArchitecture.ZLabel();
			this.prioritySettingsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.prioritySettingsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.hblDeliveryConfigurationGrid = new Enterprise.ZArchitecture.ZGrid();
			this.UpButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DownButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.prioritySettingsGrid)).BeginInit();
			this.prioritySettingsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.hblDeliveryConfigurationGrid)).BeginInit();
			this.hblDeliveryConfigurationGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.HBLDeliveryPriorityConfig);
			// 
			// hblDeliveryConfigurationLabel
			// 
			this.hblDeliveryConfigurationLabel.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("4d781d95-65db-49cb-a5d2-b63912a1514b", "Shipment Container Mode / HBL Delivery Mode");
			this.hblDeliveryConfigurationLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.hblDeliveryConfigurationLabel.IsFontBold = true;
			this.hblDeliveryConfigurationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.hblDeliveryConfigurationLabel.Name = "hblDeliveryConfigurationLabel";
			this.hblDeliveryConfigurationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 23, true);
			this.hblDeliveryConfigurationLabel.TabIndex = 8;
			// 
			// hblDeliveryConfigurationGrid
			// 
			this.hblDeliveryConfigurationGrid.AllowNavigation = false;
			this.hblDeliveryConfigurationGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.hblDeliveryConfigurationGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.HBLDeliveryPriorityConfig)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.HBLDeliveryPriorityConfig)(null)).ContainerMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.HBLDeliveryPriorityConfig)(null)).ContainerModeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.HBLDeliveryPriorityConfig)(null)).HBLDeliveryMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.HBLDeliveryPriorityConfig)(null)).HBLDeliveryModeList)));
			this.hblDeliveryConfigurationGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo2.ColumnName = "ContainerMode";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo3.ColumnName = "HBLDeliveryMode";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.hblDeliveryConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.hblDeliveryConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.hblDeliveryConfigurationGrid.GridId = "ce4ebc77-659d-49e4-a454-6075c4a8f871";
			this.hblDeliveryConfigurationGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.hblDeliveryConfigurationGrid.LayoutKey = "hblDeliveryConfigurationGrid";
			this.hblDeliveryConfigurationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 24, true);
			this.hblDeliveryConfigurationGrid.Name = "hblDeliveryConfigurationGrid";
			this.hblDeliveryConfigurationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(480, 208, true);
			this.hblDeliveryConfigurationGrid.TabIndex = 5;
			// 
			// prioritySettingsLabel
			// 
			this.prioritySettingsLabel.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("6bc9ac23-7ac3-4342-ad10-7d48ce2ebe20", "Matching HBL Delivery Modes on Rates with Priority Sequence");
			this.prioritySettingsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.prioritySettingsLabel.IsFontBold = true;
			this.prioritySettingsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 232, true);
			this.prioritySettingsLabel.Name = "prioritySettingsLabel";
			this.prioritySettingsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 23, true);
			this.prioritySettingsLabel.TabIndex = 9;
			// 
			// prioritySettingsGrid
			// 
			this.prioritySettingsGrid.AllowNavigation = false;
			this.prioritySettingsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.prioritySettingsGrid, "Settings");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.HBLDeliveryPriorityConfig)(null)).Settings)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.HBLDeliveryPrioritySetting)(((System.Collections.IList)(((Enterprise.Registry.Business.HBLDeliveryPriorityConfig)(null)).Settings)).SyncRoot)).HBLDeliveryModePriority)));
			this.prioritySettingsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "HBLDeliveryModePriority";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);

			this.prioritySettingsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);

			this.prioritySettingsGrid.GridId = "ce4ebc77-659d-49e4-a454-6075c4a8f871";
			this.prioritySettingsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.prioritySettingsGrid.LayoutKey = "prioritySettingsGrid";
			this.prioritySettingsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 256, true);
			this.prioritySettingsGrid.Name = "prioritySettingsGrid";
			this.prioritySettingsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(198, 88, true);
			this.prioritySettingsGrid.TabIndex = 6;
			this.prioritySettingsGrid.CopySelectedRowsAllowed = false;
			// 
			// UpButton
			// 
			this.UpButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("e3f99d43-984a-45fb-b2c4-0254dc96aa1e", "Move Up");
			this.UpButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(205, 276, true);
			this.UpButton.Name = "UpButton";
			this.UpButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.UpButton.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
			this.UpButton.TabIndex = 7;
			this.UpButton.Click += new System.EventHandler(this.UpButton_Click);
			// 
			// DownButton
			// 
			this.DownButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("595656ea-5b41-404f-8c6a-ec33bd92d111", "Move Down");
			this.DownButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(205, 304, true);
			this.DownButton.Name = "DownButton";
			this.DownButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.DownButton.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
			this.DownButton.TabIndex = 8;
			this.DownButton.Click += new System.EventHandler(this.DownButton_Click);
			// 
			// HBLDeliveryPriorityControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.prioritySettingsLabel);
			this.Controls.Add(this.prioritySettingsGrid);
			this.Controls.Add(this.hblDeliveryConfigurationLabel);
			this.Controls.Add(this.hblDeliveryConfigurationGrid);
						this.Controls.Add(this.DownButton);
			this.Controls.Add(this.UpButton);
			this.Name = "HBLDeliveryPriorityControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(480, 344, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.prioritySettingsGrid)).EndInit();
			this.prioritySettingsGrid.ResumeLayout(false);
			this.prioritySettingsGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.hblDeliveryConfigurationGrid)).EndInit();
			this.hblDeliveryConfigurationGrid.ResumeLayout(false);
			this.hblDeliveryConfigurationGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}
