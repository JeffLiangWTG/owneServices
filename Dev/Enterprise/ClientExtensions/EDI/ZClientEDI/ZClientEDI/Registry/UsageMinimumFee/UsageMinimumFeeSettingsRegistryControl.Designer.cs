using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Client.EDI.Registry.GUI
{
	partial class UsageMinimumFeeSettingsRegistryControl
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
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.MinimumFeeUsageGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MinimumFeeUsageGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MinimumFeeUsageGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MinimumFeeUsageGrid)).BeginInit();
			this.MinimumFeeUsageGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Registry.Business.UsageMinimumFeeSettings);
			// 
			// MinimumFeeUsageGroupBox
			//
			this.MinimumFeeUsageGroupBox.CaptionResourceString = ZClientEDI.Res.GetData("D1F0636B-A0B5-4A13-8EC3-778D8674F8D2", "Enable Product Minimum Fee Usage");
			this.MinimumFeeUsageGroupBox.Controls.Add(this.MinimumFeeUsageGrid);
			this.MinimumFeeUsageGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MinimumFeeUsageGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MinimumFeeUsageGroupBox.Name = "MinimumFeeUsageGroupBox";
			this.MinimumFeeUsageGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(416, 512, true);
			this.MinimumFeeUsageGroupBox.TabIndex = 0;
			this.MinimumFeeUsageGroupBox.TabStop = false;
			// 
			// MinimumFeeUsageGrid
			// 
			this.MinimumFeeUsageGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.MinimumFeeUsageGrid, "MinimumFeeList");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.UsageMinimumFeeSettings)(null)).MinimumFeeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.UsageMinimumFee)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.UsageMinimumFeeSettings)(null)).MinimumFeeList)).SyncRoot)).ProductCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.UsageMinimumFee)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.UsageMinimumFeeSettings)(null)).MinimumFeeList)).SyncRoot)).PriceListCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.UsageMinimumFee)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.UsageMinimumFeeSettings)(null)).MinimumFeeList)).SyncRoot)).MinimumFeeCode)));
			this.MinimumFeeUsageGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "ProductCode";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.ColumnName = "PriceListCode";
			zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "MinimumFeeCode";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.MinimumFeeUsageGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.MinimumFeeUsageGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.MinimumFeeUsageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.MinimumFeeUsageGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MinimumFeeUsageGrid.GridId = "63259777-ea32-4c36-bd4e-56d6b9e352c1";
			this.MinimumFeeUsageGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MinimumFeeUsageGrid.LayoutKey = "MinimumFeeUsageGrid";
			this.MinimumFeeUsageGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 13, true);
			this.MinimumFeeUsageGrid.Name = "MinimumFeeUsageGrid";
			this.MinimumFeeUsageGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(413, 497, true);
			this.MinimumFeeUsageGrid.TabIndex = 0;
			// 
			// UsageMinimumFeeSettingsRegistryControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.MinimumFeeUsageGroupBox);
			this.Name = "UsageMinimumFeeSettingsRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(416, 512, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MinimumFeeUsageGroupBox.ResumeLayout(false);
			this.MinimumFeeUsageGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MinimumFeeUsageGrid)).EndInit();
			this.MinimumFeeUsageGrid.ResumeLayout(false);
			this.MinimumFeeUsageGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox MinimumFeeUsageGroupBox;
		private ZArchitecture.ZGrid MinimumFeeUsageGrid;

		public ZArchitecture.ZGrid ExposedMinimumFeeUsageGridForTesting => this.MinimumFeeUsageGrid;
	}
}
