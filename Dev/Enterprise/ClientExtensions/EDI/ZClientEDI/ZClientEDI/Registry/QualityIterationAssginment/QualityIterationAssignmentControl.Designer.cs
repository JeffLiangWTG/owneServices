
namespace Enterprise.Client.EDI.Registry.GUI
{
	partial class QualityIterationAssignmentControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.AssignmentGrid = new Enterprise.ZArchitecture.ZGrid();
			this.IsDefaultOptionSelectedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.kSplitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AssignmentGrid)).BeginInit();
			this.AssignmentGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.kSplitContainer1)).BeginInit();
			this.kSplitContainer1.Panel1.SuspendLayout();
			this.kSplitContainer1.Panel2.SuspendLayout();
			this.kSplitContainer1.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Registry.Business.QualityIterationAssignmentHeader);
			// 
			// AssignmentGrid
			// 
			this.AssignmentGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AssignmentGrid, "AssignmentCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.QualityIterationAssignmentHeader)(null)).AssignmentCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.QualityIterationAssignment)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.QualityIterationAssignmentHeader)(null)).AssignmentCollection)).SyncRoot)).ReleaseGroup)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.Registry.Business.QualityIterationAssignment)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.QualityIterationAssignmentHeader)(null)).AssignmentCollection)).SyncRoot)).IsQiEnabled)));
			this.AssignmentGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = ZClientEDI.Res.GetData("6647dbdf-df49-4e04-b042-d3afa28f0887", "Release Group");
			zDropEditColumnStyleInfo1.ColumnName = "ReleaseGroup";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = ZClientEDI.Res.GetData("cdf54c05-b540-41db-afe2-98622b23ccd6", "Create Quality Iterations");
			zCheckBoxColumnStyleInfo1.ColumnName = "IsQiEnabled";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			this.AssignmentGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.AssignmentGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.AssignmentGrid.CopySelectedRowsAllowed = true;
			this.AssignmentGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AssignmentGrid.GridId = "2b59de9e-1973-41b2-9fef-6bcca6477f99";
			this.AssignmentGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AssignmentGrid.LayoutKey = "AssignmentGrid";
			this.AssignmentGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AssignmentGrid.Name = "AssignmentGrid";
			this.AssignmentGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 260, true);
			this.AssignmentGrid.TabIndex = 0;
			// 
			// IsDefaultOptionSelectedCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsDefaultOptionSelectedCheckBox, "IsDefaultOptionSelected");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.Registry.Business.QualityIterationAssignmentHeader)(null)).IsDefaultOptionSelected)));
			this.IsDefaultOptionSelectedCheckBox.CaptionResourceString = ZClientEDI.Res.GetData("baad9d99-c4e2-430d-afd1-81586c91e126", "When this box is ticked, quality iterations will be created for failed shelves for any release group that is not specified in the list below.");
			this.IsDefaultOptionSelectedCheckBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.IsDefaultOptionSelectedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsDefaultOptionSelectedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.IsDefaultOptionSelectedCheckBox.Name = "IsDefaultOptionSelectedCheckBox";
			this.IsDefaultOptionSelectedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 50, true);
			this.IsDefaultOptionSelectedCheckBox.TabIndex = 1;
			this.IsDefaultOptionSelectedCheckBox.UseVisualStyleBackColor = true;
			// 
			// kSplitContainer1
			// 
			this.kSplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.kSplitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.kSplitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.kSplitContainer1.Name = "kSplitContainer1";
			this.kSplitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// kSplitContainer1.Panel1
			// 
			this.kSplitContainer1.Panel1.Controls.Add(this.IsDefaultOptionSelectedCheckBox);
			// 
			// kSplitContainer1.Panel2
			// 
			this.kSplitContainer1.Panel2.Controls.Add(this.AssignmentGrid);
			this.kSplitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 314, true);
			this.kSplitContainer1.TabIndex = 2;
			// 
			// QualityIterationAssignmentControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.kSplitContainer1);
			this.Name = "QualityIterationAssignmentControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 314, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AssignmentGrid)).EndInit();
			this.AssignmentGrid.ResumeLayout(false);
			this.AssignmentGrid.PerformLayout();
			this.kSplitContainer1.Panel1.ResumeLayout(false);
			this.kSplitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.kSplitContainer1)).EndInit();
			this.kSplitContainer1.ResumeLayout(false);
			this.kSplitContainer1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid AssignmentGrid;
		private ZArchitecture.GUI.ZCheckBox IsDefaultOptionSelectedCheckBox;
		private CargoWise.Windows.UI.KSplitContainer kSplitContainer1;

	}
}
