namespace Enterprise.Registry.GUI
{
	partial class AutoratingViaPortControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		private ZArchitecture.ZLabel viaConfigurationLabel;
		protected ZArchitecture.ZGrid viaConfigurationGrid;
		private ZArchitecture.ZLabel viaSettingsLabel;
		protected ZArchitecture.ZGrid viaSettingsGrid;

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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.viaConfigurationLabel = new Enterprise.ZArchitecture.ZLabel();
			this.viaSettingsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.viaSettingsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.viaConfigurationGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.viaSettingsGrid)).BeginInit();
			this.viaSettingsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.viaConfigurationGrid)).BeginInit();
			this.viaConfigurationGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.AutoratingViaPortConfiguration);
			// 
			// viaConfigurationLabel
			// 
			this.viaConfigurationLabel.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("4F529BDD-75F5-49FE-9520-EC11510A8561", "Job Type / Transport Mode");
			this.viaConfigurationLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.viaConfigurationLabel.IsFontBold = true;
			this.viaConfigurationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.viaConfigurationLabel.Name = "viaConfigurationLabel";
			this.viaConfigurationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 23, true);
			this.viaConfigurationLabel.TabIndex = 8;
			// 
			// viaSettingsLabel
			// 
			this.viaSettingsLabel.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("BBD385BD-42C3-4FBB-9F5F-865A029B2F17", "Autorating Via Port Settings");
			this.viaSettingsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.viaSettingsLabel.IsFontBold = true;
			this.viaSettingsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 152, true);
			this.viaSettingsLabel.Name = "viaSettingsLabel";
			this.viaSettingsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 23, true);
			this.viaSettingsLabel.TabIndex = 9;
			// 
			// viaSettingsGrid
			// 
			this.viaSettingsGrid.AllowNavigation = false;
			this.viaSettingsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.viaSettingsGrid, "Settings");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.AutoratingViaPortConfiguration)(null)).Settings)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.AutoratingViaPortSetting)(((System.Collections.IList)(((Enterprise.Registry.Business.AutoratingViaPortConfiguration)(null)).Settings)).SyncRoot)).Direction)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.AutoratingViaPortSetting)(((System.Collections.IList)(((Enterprise.Registry.Business.AutoratingViaPortConfiguration)(null)).Settings)).SyncRoot)).OriginSourceOption)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.AutoratingViaPortSetting)(((System.Collections.IList)(((Enterprise.Registry.Business.AutoratingViaPortConfiguration)(null)).Settings)).SyncRoot)).DestinationSourceOption)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.AutoratingViaPortSetting)(((System.Collections.IList)(((Enterprise.Registry.Business.AutoratingViaPortConfiguration)(null)).Settings)).SyncRoot)).ViaSourceOption)));
			this.viaSettingsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "Direction";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.ColumnName = "OriginSourceOption";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.ColumnName = "DestinationSourceOption";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo4.ColumnName = "ViaSourceOption";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.viaSettingsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.viaSettingsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.viaSettingsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.viaSettingsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.viaSettingsGrid.GridId = "72F20A87-0018-422F-A0DE-8220C53A71CA";
			this.viaSettingsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.viaSettingsGrid.LayoutKey = "viaSettingsGrid";
			this.viaSettingsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 176, true);
			this.viaSettingsGrid.Name = "viaSettingsGrid";
			this.viaSettingsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(480, 168, true);
			this.viaSettingsGrid.TabIndex = 6;
			// 
			// viaConfigurationGrid
			// 
			this.viaConfigurationGrid.AllowNavigation = false;
			this.viaConfigurationGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.viaConfigurationGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.AutoratingViaPortConfiguration)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.AutoratingViaPortConfiguration)(null)).JobType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.AutoratingViaPortConfiguration)(null)).JobTypeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.AutoratingViaPortConfiguration)(null)).TransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.AutoratingViaPortConfiguration)(null)).TransportModeList)));
			this.viaConfigurationGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo5.ColumnName = "JobType";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo6.ColumnName = "TransportMode";
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.viaConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.viaConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.viaConfigurationGrid.GridId = "91C714FB-AC35-40A7-B53C-067683C0747F";
			this.viaConfigurationGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.viaConfigurationGrid.LayoutKey = "typeModeGrid";
			this.viaConfigurationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 24, true);
			this.viaConfigurationGrid.Name = "viaConfigurationGrid";
			this.viaConfigurationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(480, 128, true);
			this.viaConfigurationGrid.TabIndex = 5;
			// 
			// AutoratingViaPortControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.viaSettingsLabel);
			this.Controls.Add(this.viaSettingsGrid);
			this.Controls.Add(this.viaConfigurationLabel);
			this.Controls.Add(this.viaConfigurationGrid);
			this.Name = "AutoratingViaPortControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(480, 344, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.viaSettingsGrid)).EndInit();
			this.viaSettingsGrid.ResumeLayout(false);
			this.viaSettingsGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.viaConfigurationGrid)).EndInit();
			this.viaConfigurationGrid.ResumeLayout(false);
			this.viaConfigurationGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}
