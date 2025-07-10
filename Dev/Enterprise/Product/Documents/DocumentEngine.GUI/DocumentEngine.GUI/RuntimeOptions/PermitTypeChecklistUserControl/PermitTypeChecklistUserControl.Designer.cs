using Enterprise.ZArchitecture.Core;
using System.Windows.Forms;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	partial class PermitTypeChecklistUserControl
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
			this.splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.grid1 = new Enterprise.ZArchitecture.ZGrid();
			this.subTypePanel = new CargoWise.Windows.UI.KPanel();
			this.grid2 = new Enterprise.ZArchitecture.ZGrid();
			this.fieldLabelPanel = new CargoWise.Windows.UI.KPanel();
			this.fieldLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.grid1)).BeginInit();
			this.grid1.SuspendLayout();
			this.subTypePanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.grid2)).BeginInit();
			this.grid2.SuspendLayout();
			this.fieldLabelPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentEngine.RuntimeOptions.PermitTypeChecklistField);
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
			this.splitContainer1.Panel2.Controls.Add(this.subTypePanel);
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
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngine.RuntimeOptions.PermitTypeChecklistField)(null)).RootItemsCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentEngine.RuntimeOptions.PermitTypePartItem)(((System.Collections.IList)(((Enterprise.DocumentEngine.RuntimeOptions.PermitTypeChecklistField)(null)).RootItemsCollection)).SyncRoot)).Include)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.RuntimeOptions.PermitTypePartItem)(((System.Collections.IList)(((Enterprise.DocumentEngine.RuntimeOptions.PermitTypeChecklistField)(null)).RootItemsCollection)).SyncRoot)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.RuntimeOptions.PermitTypePartItem)(((System.Collections.IList)(((Enterprise.DocumentEngine.RuntimeOptions.PermitTypeChecklistField)(null)).RootItemsCollection)).SyncRoot)).Description)));
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
			this.grid1.GridId = "E6CE62DE-6862-490A-BF13-1C1F7CF3F4E2";
			this.grid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.grid1.LayoutKey = "grid1";
			this.grid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.grid1.Name = "grid1";
			this.grid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 80, true);
			this.grid1.TabIndex = 0;
			// 
			// subTypeGroupBox
			// 
			this.subTypePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.subTypePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 0, true);
			this.subTypePanel.Name = "subTypeGroupBox";
			this.subTypePanel.Controls.Add(this.grid2);
			// 
			// grid2
			// 
			this.grid2.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.grid2, "RootItemsCollection.SubItemsCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngine.RuntimeOptions.PermitTypePartItem)(((System.Collections.IList)(((Enterprise.DocumentEngine.RuntimeOptions.PermitTypeChecklistField)(null)).RootItemsCollection)).SyncRoot)).SubItemsCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentEngine.RuntimeOptions.PermitTypePartItem)(((System.Collections.IList)(((Enterprise.DocumentEngine.RuntimeOptions.PermitTypePartItem)(((System.Collections.IList)(((Enterprise.DocumentEngine.RuntimeOptions.PermitTypeChecklistField)(null)).RootItemsCollection)).SyncRoot)).SubItemsCollection)).SyncRoot)).Include)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.RuntimeOptions.PermitTypePartItem)(((System.Collections.IList)(((Enterprise.DocumentEngine.RuntimeOptions.PermitTypePartItem)(((System.Collections.IList)(((Enterprise.DocumentEngine.RuntimeOptions.PermitTypeChecklistField)(null)).RootItemsCollection)).SyncRoot)).SubItemsCollection)).SyncRoot)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.RuntimeOptions.PermitTypePartItem)(((System.Collections.IList)(((Enterprise.DocumentEngine.RuntimeOptions.PermitTypePartItem)(((System.Collections.IList)(((Enterprise.DocumentEngine.RuntimeOptions.PermitTypeChecklistField)(null)).RootItemsCollection)).SyncRoot)).SubItemsCollection)).SyncRoot)).Description)));
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
			this.grid2.GridId = "F1019286-BADC-4145-9C58-AD9921C6D5D1";
			this.grid2.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.grid2.LayoutKey = "grid1";
			this.grid2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.grid2.Name = "grid2";
			this.grid2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 80, true);
			this.grid2.TabIndex = 0;
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
			this.BindingSource.SetBindingMember(this.fieldLabel, "DisplayName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.DocumentEngine.RuntimeOptions.PermitTypeChecklistField)(null)).DisplayName)));
			this.fieldLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.fieldLabel, false);
			this.fieldLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.fieldLabel.Name = "fieldLabel";
			this.fieldLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 30, true);
			this.fieldLabel.TabIndex = 0;
			this.fieldLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// PermitTypeChecklistUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.splitContainer1);
			this.Controls.Add(this.fieldLabelPanel);
			this.Name = "PermitTypeChecklistUserControl";
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
			this.subTypePanel.ResumeLayout(false);
			this.subTypePanel.ResumeLayout(false);
			this.subTypePanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.grid2)).EndInit();
			this.grid2.ResumeLayout(false);
			this.grid2.PerformLayout();
			this.fieldLabelPanel.ResumeLayout(false);
			this.fieldLabelPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer splitContainer1;
		private ZArchitecture.ZGrid grid1;
		private CargoWise.Windows.UI.KPanel subTypePanel;
		private ZArchitecture.ZGrid grid2;
		private CargoWise.Windows.UI.KPanel fieldLabelPanel;
		private ZArchitecture.ZLabel fieldLabel;
	}
}
