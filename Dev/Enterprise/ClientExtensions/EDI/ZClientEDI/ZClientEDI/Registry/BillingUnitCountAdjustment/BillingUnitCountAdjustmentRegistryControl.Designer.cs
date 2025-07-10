namespace Enterprise.Client.EDI.Registry.GUI
{
	partial class BillingUnitCountAdjustmentRegistryControl
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
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.SettingsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.PriceCodeGrid = new Enterprise.ZArchitecture.ZGrid();
			this.PriceCodeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.AdjustmentGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
			this.SplitContainer.Panel1.SuspendLayout();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PriceCodeGrid)).BeginInit();
			this.PriceCodeGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AdjustmentGrid)).BeginInit();
			this.AdjustmentGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Registry.Business.BillingUnitCountAdjustmentCollection);
			// 
			// SettingsLabel
			// 
			this.SettingsLabel.CaptionResourceString = ZClientEDI.Res.GetData("5d45a8df-34f2-487f-9392-e13d2fd66f6a", "Settings");
			this.SettingsLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.SettingsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.SettingsLabel.IsFontBold = true;
			this.SettingsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SettingsLabel.Name = "SettingsLabel";
			this.SettingsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 26, true);
			this.SettingsLabel.TabIndex = 0;
			// 
			// SplitContainer
			// 
			this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SplitContainer.Name = "SplitContainer";
			this.SplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// SplitContainer.Panel1
			// 
			this.SplitContainer.Panel1.Controls.Add(this.PriceCodeGrid);
			this.SplitContainer.Panel1.Controls.Add(this.PriceCodeLabel);
			// 
			// SplitContainer.Panel2
			// 
			this.SplitContainer.Panel2.Controls.Add(this.AdjustmentGrid);
			this.SplitContainer.Panel2.Controls.Add(this.SettingsLabel);
			this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 480, true);
			this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(150);
			this.SplitContainer.TabIndex = 1;
			// 
			// PriceCodeGrid
			// 
			this.PriceCodeGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PriceCodeGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.BillingUnitCountAdjustment)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.BillingUnitCountAdjustment)(null)).PriceCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.Registry.Business.BillingUnitCountAdjustment)(null)).DefaultAdjustedIncrement)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.BillingUnitCountAdjustment)(null)).Comment)));
			this.PriceCodeGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "PriceCode";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "DefaultAdjustedIncrement";
			zCalcEditColumnStyleInfo1.Decimals = 4;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "Comment";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			this.PriceCodeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PriceCodeGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.PriceCodeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.PriceCodeGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PriceCodeGrid.GridId = "2af63adc-ca9f-4468-b49d-22f95e27cbe9";
			this.PriceCodeGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PriceCodeGrid.LayoutKey = "zGrid1";
			this.PriceCodeGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 26, true);
			this.PriceCodeGrid.Name = "PriceCodeGrid";
			this.PriceCodeGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 124, true);
			this.PriceCodeGrid.TabIndex = 4;
			// 
			// PriceCodeLabel
			// 
			this.PriceCodeLabel.CaptionResourceString = ZClientEDI.Res.GetData("65b4a00b-51bd-42bc-9457-3b2933be3b27", "Price Code");
			this.PriceCodeLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.PriceCodeLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.PriceCodeLabel.IsFontBold = true;
			this.PriceCodeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PriceCodeLabel.Name = "PriceCodeLabel";
			this.PriceCodeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 26, true);
			this.PriceCodeLabel.TabIndex = 3;
			// 
			// AdjustmentGrid
			// 
			this.AdjustmentGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AdjustmentGrid, "AdjustmentSettings");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.BillingUnitCountAdjustment)(null)).AdjustmentSettings)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.Registry.Business.BillingUnitCountAdjustmentSetting)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.BillingUnitCountAdjustment)(null)).AdjustmentSettings)).SyncRoot)).OriginalUnitCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.Registry.Business.BillingUnitCountAdjustmentSetting)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.BillingUnitCountAdjustment)(null)).AdjustmentSettings)).SyncRoot)).AdjustedUnitCount)));
			this.AdjustmentGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "OriginalUnitCount";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "AdjustedUnitCount";
			zCalcEditColumnStyleInfo3.Decimals = 4;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			this.AdjustmentGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.AdjustmentGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.AdjustmentGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdjustmentGrid.GridId = "2af63adc-ca9f-4468-b49d-22f95e27cbe9";
			this.AdjustmentGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AdjustmentGrid.LayoutKey = "zGrid1";
			this.AdjustmentGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 26, true);
			this.AdjustmentGrid.Name = "AdjustmentGrid";
			this.AdjustmentGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 300, true);
			this.AdjustmentGrid.TabIndex = 3;
			// 
			// BillingUnitCountAdjustmentRegistryControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SplitContainer);
			this.Name = "BillingUnitCountAdjustmentRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 480, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SplitContainer.Panel1.ResumeLayout(false);
			this.SplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
			this.SplitContainer.ResumeLayout(false);
			this.SplitContainer.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PriceCodeGrid)).EndInit();
			this.PriceCodeGrid.ResumeLayout(false);
			this.PriceCodeGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AdjustmentGrid)).EndInit();
			this.AdjustmentGrid.ResumeLayout(false);
			this.AdjustmentGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZLabel SettingsLabel;
		internal CargoWise.Windows.UI.KSplitContainer SplitContainer;
		internal ZArchitecture.ZGrid AdjustmentGrid;
		internal ZArchitecture.ZGrid PriceCodeGrid;
		private ZArchitecture.ZLabel PriceCodeLabel;
	}
}
