namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	partial class SalesTradeLaneChecklistUserControl
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
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.grid1 = new Enterprise.ZArchitecture.ZGrid();
			this.splitContainer2 = new CargoWise.Windows.UI.KSplitContainer();
			this.grid2 = new Enterprise.ZArchitecture.ZGrid();
			this.grid3 = new Enterprise.ZArchitecture.ZGrid();
			this.fieldLabelPanel = new CargoWise.Windows.UI.KPanel();
			this.fieldLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.grid1)).BeginInit();
			this.grid1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
			this.splitContainer2.Panel1.SuspendLayout();
			this.splitContainer2.Panel2.SuspendLayout();
			this.splitContainer2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.grid2)).BeginInit();
			this.grid2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.grid3)).BeginInit();
			this.grid3.SuspendLayout();
			this.fieldLabelPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentEngine.RuntimeOptions.SalesTradeLaneChecklistField);
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 0, true);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.grid1);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
			this.splitContainer1.Panel2.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(10, 0, 0, 0, true);
			this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 240, true);
			this.splitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.splitContainer1.TabIndex = 0;
			// 
			// grid1
			// 
			this.grid1.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.grid1, "RootItemsCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngine.RuntimeOptions.SalesTradeLaneChecklistField)(null)).RootItemsCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentEngine.RuntimeOptions.SalesTradeLanePartItem)(((System.Collections.IList)(((Enterprise.DocumentEngine.RuntimeOptions.SalesTradeLaneChecklistField)(null)).RootItemsCollection)).SyncRoot)).Include)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.RuntimeOptions.SalesTradeLanePartItem)(((System.Collections.IList)(((Enterprise.DocumentEngine.RuntimeOptions.SalesTradeLaneChecklistField)(null)).RootItemsCollection)).SyncRoot)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.RuntimeOptions.SalesTradeLanePartItem)(((System.Collections.IList)(((Enterprise.DocumentEngine.RuntimeOptions.SalesTradeLaneChecklistField)(null)).RootItemsCollection)).SyncRoot)).Description)));
			this.grid1.CaptionVisible = false;
			zCheckBoxColumnStyleInfo4.ColumnName = "Include";
			zCheckBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo7.ColumnName = "Code";
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo8.ColumnName = "Description";
			zTextBoxColumnStyleInfo8.IsReadOnly = true;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.grid1.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.grid1.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.grid1.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.grid1.CopySelectedRowsAllowed = true;
			this.grid1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.grid1.GridId = "41f28ec7-4a09-4913-b7c7-b9fbeb276527";
			this.grid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.grid1.LayoutKey = "grid1";
			this.grid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.grid1.Name = "grid1";
			this.grid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 80, true);
			this.grid1.TabIndex = 0;
			// 
			// splitContainer2
			// 
			this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 0, true);
			this.splitContainer2.Name = "splitContainer2";
			this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer2.Panel1
			// 
			this.splitContainer2.Panel1.Controls.Add(this.grid2);
			// 
			// splitContainer2.Panel2
			// 
			this.splitContainer2.Panel2.Controls.Add(this.grid3);
			this.splitContainer2.Panel2.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(10, 0, 0, 0, true);
			this.splitContainer2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 157, true);
			this.splitContainer2.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.splitContainer2.TabIndex = 1;
			// 
			// grid2
			// 
			this.grid2.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.grid2, "RootItemsCollection.SubItemsCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngine.RuntimeOptions.SalesTradeLanePartItem)(((System.Collections.IList)(((Enterprise.DocumentEngine.RuntimeOptions.SalesTradeLaneChecklistField)(null)).RootItemsCollection)).SyncRoot)).SubItemsCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentEngine.RuntimeOptions.SalesTradeLanePartItem)(((System.Collections.IList)(((Enterprise.DocumentEngine.RuntimeOptions.SalesTradeLanePartItem)(((System.Collections.IList)(((Enterprise.DocumentEngine.RuntimeOptions.SalesTradeLaneChecklistField)(null)).RootItemsCollection)).SyncRoot)).SubItemsCollection)).SyncRoot)).Include)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.RuntimeOptions.SalesTradeLanePartItem)(((System.Collections.IList)(((Enterprise.DocumentEngine.RuntimeOptions.SalesTradeLanePartItem)(((System.Collections.IList)(((Enterprise.DocumentEngine.RuntimeOptions.SalesTradeLaneChecklistField)(null)).RootItemsCollection)).SyncRoot)).SubItemsCollection)).SyncRoot)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.RuntimeOptions.SalesTradeLanePartItem)(((System.Collections.IList)(((Enterprise.DocumentEngine.RuntimeOptions.SalesTradeLanePartItem)(((System.Collections.IList)(((Enterprise.DocumentEngine.RuntimeOptions.SalesTradeLaneChecklistField)(null)).RootItemsCollection)).SyncRoot)).SubItemsCollection)).SyncRoot)).Description)));
			this.grid2.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.ColumnName = "Include";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo2.ColumnName = "Description";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.grid2.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.grid2.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid2.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid2.CopySelectedRowsAllowed = true;
			this.grid2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.grid2.GridId = "41f28ec7-4a09-4913-b7c7-b9fbeb276527";
			this.grid2.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.grid2.LayoutKey = "grid1";
			this.grid2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.grid2.Name = "grid2";
			this.grid2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 80, true);
			this.grid2.TabIndex = 0;
			// 
			// grid3
			// 
			this.grid3.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.grid3, "RootItemsCollection.SubItemsCollection.SubItemsCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngine.RuntimeOptions.SalesTradeLanePartItem)(((System.Collections.IList)(((Enterprise.DocumentEngine.RuntimeOptions.SalesTradeLanePartItem)(((System.Collections.IList)(((Enterprise.DocumentEngine.RuntimeOptions.SalesTradeLaneChecklistField)(null)).RootItemsCollection)).SyncRoot)).SubItemsCollection)).SyncRoot)).SubItemsCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentEngine.RuntimeOptions.SalesTradeLanePartItem)(((System.Collections.IList)(((Enterprise.DocumentEngine.RuntimeOptions.SalesTradeLanePartItem)(((System.Collections.IList)(((Enterprise.DocumentEngine.RuntimeOptions.SalesTradeLanePartItem)(((System.Collections.IList)(((Enterprise.DocumentEngine.RuntimeOptions.SalesTradeLaneChecklistField)(null)).RootItemsCollection)).SyncRoot)).SubItemsCollection)).SyncRoot)).SubItemsCollection)).SyncRoot)).Include)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.RuntimeOptions.SalesTradeLanePartItem)(((System.Collections.IList)(((Enterprise.DocumentEngine.RuntimeOptions.SalesTradeLanePartItem)(((System.Collections.IList)(((Enterprise.DocumentEngine.RuntimeOptions.SalesTradeLanePartItem)(((System.Collections.IList)(((Enterprise.DocumentEngine.RuntimeOptions.SalesTradeLaneChecklistField)(null)).RootItemsCollection)).SyncRoot)).SubItemsCollection)).SyncRoot)).SubItemsCollection)).SyncRoot)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.RuntimeOptions.SalesTradeLanePartItem)(((System.Collections.IList)(((Enterprise.DocumentEngine.RuntimeOptions.SalesTradeLanePartItem)(((System.Collections.IList)(((Enterprise.DocumentEngine.RuntimeOptions.SalesTradeLanePartItem)(((System.Collections.IList)(((Enterprise.DocumentEngine.RuntimeOptions.SalesTradeLaneChecklistField)(null)).RootItemsCollection)).SyncRoot)).SubItemsCollection)).SyncRoot)).SubItemsCollection)).SyncRoot)).Description)));
			this.grid3.CaptionVisible = false;
			zCheckBoxColumnStyleInfo2.ColumnName = "Include";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo3.ColumnName = "Code";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo4.ColumnName = "Description";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.grid3.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.grid3.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid3.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.grid3.CopySelectedRowsAllowed = true;
			this.grid3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.grid3.GridId = "41f28ec7-4a09-4913-b7c7-b9fbeb276527";
			this.grid3.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.grid3.LayoutKey = "grid1";
			this.grid3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 0, true);
			this.grid3.Name = "grid3";
			this.grid3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(310, 75, true);
			this.grid3.TabIndex = 1;
			// 
			// fieldLabelPanel
			// 
			this.fieldLabelPanel.Controls.Add(this.fieldLabel);
			this.fieldLabelPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.fieldLabelPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.fieldLabelPanel.Name = "fieldLabelPanel";
			this.fieldLabelPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 240, true);
			this.fieldLabelPanel.TabIndex = 1;
			// 
			// fieldLabel
			// 
			this.BindingSource.SetBindingMember(this.fieldLabel, "DisplayNameLocalized");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.DocumentEngine.RuntimeOptions.SalesTradeLaneChecklistField)(null)).DisplayName)));
			this.fieldLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.fieldLabel, false);
			this.fieldLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.fieldLabel.Name = "fieldLabel";
			this.fieldLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 30, true);
			this.fieldLabel.TabIndex = 0;
			this.fieldLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// SalesTradeLaneChecklistUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.splitContainer1);
			this.Controls.Add(this.fieldLabelPanel);
			this.Name = "SalesTradeLaneChecklistUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(440, 240, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.splitContainer1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.grid1)).EndInit();
			this.grid1.ResumeLayout(false);
			this.grid1.PerformLayout();
			this.splitContainer2.Panel1.ResumeLayout(false);
			this.splitContainer2.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
			this.splitContainer2.ResumeLayout(false);
			this.splitContainer2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.grid2)).EndInit();
			this.grid2.ResumeLayout(false);
			this.grid2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.grid3)).EndInit();
			this.grid3.ResumeLayout(false);
			this.grid3.PerformLayout();
			this.fieldLabelPanel.ResumeLayout(false);
			this.fieldLabelPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer splitContainer1;
		private ZArchitecture.ZGrid grid1;
		private CargoWise.Windows.UI.KSplitContainer splitContainer2;
		private ZArchitecture.ZGrid grid2;
		private ZArchitecture.ZGrid grid3;
		private CargoWise.Windows.UI.KPanel fieldLabelPanel;
		private ZArchitecture.ZLabel fieldLabel;
	}
}
