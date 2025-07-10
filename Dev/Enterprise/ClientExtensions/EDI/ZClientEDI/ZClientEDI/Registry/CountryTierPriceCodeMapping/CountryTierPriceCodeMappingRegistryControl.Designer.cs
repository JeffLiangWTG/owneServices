namespace Enterprise.Client.EDI.Registry.GUI
{
	partial class CountryTierPriceCodeMappingRegistryControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			this.MappingLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.PriceCodeGrid = new Enterprise.ZArchitecture.ZGrid();
			this.PriceCodeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MappingLineGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
			this.SplitContainer.Panel1.SuspendLayout();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PriceCodeGrid)).BeginInit();
			this.PriceCodeGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MappingLineGrid)).BeginInit();
			this.MappingLineGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Registry.Business.CountryTierPriceCodeMappingCollection);
			// 
			// MappingLabel
			// 
			this.MappingLabel.CaptionResourceString = ZClientEDI.Res.GetData("167e6fde-2337-455d-b125-8f77cb867e94", "Country Tier Code Mapping");
			this.MappingLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.MappingLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.MappingLabel.IsFontBold = true;
			this.MappingLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MappingLabel.Name = "MappingLabel";
			this.MappingLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 26, true);
			this.MappingLabel.TabIndex = 0;
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
			this.SplitContainer.Panel2.Controls.Add(this.MappingLineGrid);
			this.SplitContainer.Panel2.Controls.Add(this.MappingLabel);
			this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 480, true);
			this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(150);
			this.SplitContainer.TabIndex = 1;
			// 
			// PriceCodeGrid
			// 
			this.PriceCodeGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PriceCodeGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.CountryTierPriceCodeMapping)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.CountryTierPriceCodeMapping)(null)).SystemCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.CountryTierPriceCodeMapping)(null)).PriceCode)));
			this.PriceCodeGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "SystemCode";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "PriceCode";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.PriceCodeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PriceCodeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.PriceCodeGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PriceCodeGrid.GridId = "06845f02-adfc-4252-89d8-98efdea926f2";
			this.PriceCodeGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PriceCodeGrid.LayoutKey = "zGrid1";
			this.PriceCodeGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 26, true);
			this.PriceCodeGrid.Name = "PriceCodeGrid";
			this.PriceCodeGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 124, true);
			this.PriceCodeGrid.TabIndex = 4;
			// 
			// PriceCodeLabel
			// 
			this.PriceCodeLabel.CaptionResourceString = ZClientEDI.Res.GetData("6df1199a-b6c0-4bee-a0aa-a3ac87efc10d", "Price Code");
			this.PriceCodeLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.PriceCodeLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.PriceCodeLabel.IsFontBold = true;
			this.PriceCodeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PriceCodeLabel.Name = "PriceCodeLabel";
			this.PriceCodeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 26, true);
			this.PriceCodeLabel.TabIndex = 3;
			// 
			// MappingLineGrid
			// 
			this.MappingLineGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.MappingLineGrid, "MappingLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.CountryTierPriceCodeMapping)(null)).MappingLines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.CountryTierPriceCodeMappingLine)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.CountryTierPriceCodeMapping)(null)).MappingLines)).SyncRoot)).CountryCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.CountryTierPriceCodeMappingLine)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.CountryTierPriceCodeMapping)(null)).MappingLines)).SyncRoot)).CountryTierCode)));
			this.MappingLineGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CountryCode";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "CountryTierCode";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.MappingLineGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.MappingLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.MappingLineGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MappingLineGrid.GridId = "aa479543-d867-479b-bec5-0110374d1989";
			this.MappingLineGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MappingLineGrid.LayoutKey = "zGrid1";
			this.MappingLineGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 26, true);
			this.MappingLineGrid.Name = "MappingLineGrid";
			this.MappingLineGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 300, true);
			this.MappingLineGrid.TabIndex = 3;
			// 
			// CountryTierPriceCodeMappingRegistryControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SplitContainer);
			this.Name = "CountryTierPriceCodeMappingRegistryControl";
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
			((System.ComponentModel.ISupportInitialize)(this.MappingLineGrid)).EndInit();
			this.MappingLineGrid.ResumeLayout(false);
			this.MappingLineGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZLabel MappingLabel;
		internal CargoWise.Windows.UI.KSplitContainer SplitContainer;
		internal ZArchitecture.ZGrid MappingLineGrid;
		internal ZArchitecture.ZGrid PriceCodeGrid;
		private ZArchitecture.ZLabel PriceCodeLabel;
	}
}
