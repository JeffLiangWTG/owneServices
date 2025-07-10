namespace Enterprise.Client.EDI.Registry.GUI
{
	partial class ConsolidatedBillingSettingsRegistryUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.groupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.grid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.groupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.grid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Registry.Business.ConsolidatedBillingSettingCollection);
			// 
			// groupBox
			// 
			this.groupBox.CaptionResourceString = ZClientEDI.Res.GetData("bfe7db9c-8e51-45ba-aae5-9589453c5621", "Enable Product for Consolidation");
			this.groupBox.Controls.Add(this.grid);
			this.groupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.groupBox.Name = "groupBox";
			this.groupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(313, 272, true);
			this.groupBox.TabIndex = 0;
			this.groupBox.TabStop = false;
			this.groupBox.Text = "Enable Product for Consolidation";
			// 
			// grid
			// 
			this.grid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.grid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.ConsolidatedBillingSetting)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.ConsolidatedBillingSetting)(null)).ProductCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.ConsolidatedBillingSetting)(null)).Description)));
			this.grid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = ZClientEDI.Res.GetData("8db4ba71-cb23-4595-a52f-b223001c8a86", "Product Code");
			zDropEditColumnStyleInfo1.ColumnName = "ProductCode";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = ZClientEDI.Res.GetData("bef762f5-e212-451e-9bff-6e231152f7c3", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "Description";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.grid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.grid.GridId = "3ffb5fe4-06c8-44b7-9017-331e93a5663d";
			this.grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.grid.LayoutKey = "grid";
			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 13, true);
			this.grid.Name = "grid";
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(310, 257, true);
			this.grid.TabIndex = 0;
			// 
			// ConsolidatedBillingSettingsRegistryUserControl
			//
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.groupBox);
			this.Name = "ConsolidatedBillingSettingsRegistryUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(416, 350, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.groupBox.ResumeLayout(false);
			this.groupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			this.grid.ResumeLayout(false);
			this.grid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		ZArchitecture.GUI.ZGroupBox groupBox;
		internal ZArchitecture.ZGrid grid;
	}
}
