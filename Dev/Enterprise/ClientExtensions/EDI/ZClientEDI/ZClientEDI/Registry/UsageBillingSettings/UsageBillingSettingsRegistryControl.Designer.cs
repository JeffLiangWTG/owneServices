namespace Enterprise.Client.EDI.Registry.GUI
{
	partial class UsageBillingSettingsRegistryControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			this.PriceListGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PriceListGrid = new Enterprise.ZArchitecture.ZGrid();
			this.BranchGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.BranchGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PriceListGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PriceListGrid)).BeginInit();
			this.PriceListGrid.SuspendLayout();
			this.BranchGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BranchGrid)).BeginInit();
			this.BranchGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Registry.Business.UsageBillingSettings);
			// 
			// PriceListGroupBox
			// 
			this.PriceListGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.PriceListGroupBox.CaptionResourceString = ZClientEDI.Res.GetData("fb2633bb-2704-4a9b-84ef-d30d90c177e1", "Enable Product and Price List");
			this.PriceListGroupBox.Controls.Add(this.PriceListGrid);
			this.PriceListGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.PriceListGroupBox.Name = "PriceListGroupBox";
			this.PriceListGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(405, 269, true);
			this.PriceListGroupBox.TabIndex = 7;
			this.PriceListGroupBox.TabStop = false;
			this.PriceListGroupBox.Text = "Enable Product and Price List";
			// 
			// PriceListGrid
			// 
			this.PriceListGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PriceListGrid, "PriceLists");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.UsageBillingSettings)(null)).PriceLists)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.UsageBillingPriceList)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.UsageBillingSettings)(null)).PriceLists)).SyncRoot)).ProductCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.UsageBillingPriceList)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.UsageBillingSettings)(null)).PriceLists)).SyncRoot)).RawUsageCategory)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.UsageBillingPriceList)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.UsageBillingSettings)(null)).PriceLists)).SyncRoot)).PriceListCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.UsageBillingPriceList)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.UsageBillingSettings)(null)).PriceLists)).SyncRoot)).Description)));
			this.PriceListGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "ProductCode";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.ColumnName = "RawUsageCategory";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "PriceListCode";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "Description";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.PriceListGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.PriceListGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.PriceListGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PriceListGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.PriceListGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PriceListGrid.GridId = "70321c6c-ed6b-4732-b71b-be087b44819e";
			this.PriceListGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PriceListGrid.LayoutKey = "PriceListGrid";
			this.PriceListGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.PriceListGrid.Name = "PriceListGrid";
			this.PriceListGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(399, 250, true);
			this.PriceListGrid.TabIndex = 1;
			// 
			// BranchGroupBox
			// 
			this.BranchGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BranchGroupBox.CaptionResourceString = ZClientEDI.Res.GetData("571ab77f-b500-4bde-9637-7c26c779c1e0", "Restrict Issuing Branch");
			this.BranchGroupBox.Controls.Add(this.BranchGrid);
			this.BranchGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 280, true);
			this.BranchGroupBox.Name = "BranchGroupBox";
			this.BranchGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(399, 222, true);
			this.BranchGroupBox.TabIndex = 8;
			this.BranchGroupBox.TabStop = false;
			// 
			// BranchGrid
			// 
			this.BranchGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.BranchGrid, "BranchRestrictions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.UsageBillingSettings)(null)).BranchRestrictions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.UsageBillingBranchRestriction)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.UsageBillingSettings)(null)).BranchRestrictions)).SyncRoot)).ProductCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.Registry.Business.UsageBillingBranchRestriction)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.UsageBillingSettings)(null)).BranchRestrictions)).SyncRoot)).InvoicingBranch)));
			this.BranchGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo2.ColumnName = "ProductCode";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "InvoicingBranch";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.BranchGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.BranchGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.BranchGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BranchGrid.GridId = "70321c6c-ed6b-4732-b71b-be087b44819e";
			this.BranchGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.BranchGrid.LayoutKey = "BranchGrid";
			this.BranchGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.BranchGrid.Name = "BranchGrid";
			this.BranchGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(393, 203, true);
			this.BranchGrid.TabIndex = 2;
			// 
			// UsageBillingSettingsRegistryControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.BranchGroupBox);
			this.Controls.Add(this.PriceListGroupBox);
			this.Name = "UsageBillingSettingsRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(416, 512, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PriceListGroupBox.ResumeLayout(false);
			this.PriceListGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PriceListGrid)).EndInit();
			this.PriceListGrid.ResumeLayout(false);
			this.PriceListGrid.PerformLayout();
			this.BranchGroupBox.ResumeLayout(false);
			this.BranchGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BranchGrid)).EndInit();
			this.BranchGrid.ResumeLayout(false);
			this.BranchGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private ZArchitecture.GUI.ZGroupBox PriceListGroupBox;
		internal ZArchitecture.ZGrid PriceListGrid;
		private ZArchitecture.GUI.ZGroupBox BranchGroupBox;
		internal ZArchitecture.ZGrid BranchGrid;
	}
}
