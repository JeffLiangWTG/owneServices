using CargoWise.Types;
using Enterprise.Dash.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Dash.GUI
{
	partial class WorkflowConfigurationStepCollectionControl
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
			this.workflowConfigurationStepGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.workflowConfigurationStepGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Dash.Business.WorkflowConfigurationStepCollection);
			// 
			// workflowConfigurationStepGrid
			// 
			this.workflowConfigurationStepGrid.AllowNavigation = false;
			this.workflowConfigurationStepGrid.AllowSorting = false;
			this.BindingSource.SetBindingMember(this.workflowConfigurationStepGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Dash.Business.WorkflowConfigurationStep)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Dash.Business.WorkflowConfigurationStep)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Dash.Business.WorkflowConfigurationStep)(null)).Codes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Dash.Business.WorkflowConfigurationStep)(null)).Description)));
			this.workflowConfigurationStepGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.BindToList = "Codes";
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Dash.GUI.Res.GetData("WorkflowConfigurationStepCollectionControl|62105D39-C9A9-4CC6-8254-F8F67D527E4B", "Code");
			zDropEditColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Dash.GUI.Res.GetData("WorkflowConfigurationStepCollectionControl|622637E0-D5F7-4721-B06E-93ADB338D1F1", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "Description";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.workflowConfigurationStepGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.workflowConfigurationStepGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.workflowConfigurationStepGrid.GridId = "74D77A14-B638-4BE0-8967-F68F4C9CB9E4";
			this.workflowConfigurationStepGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.workflowConfigurationStepGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.workflowConfigurationStepGrid.LayoutKey = "workflowConfigurationStepGrid";
			this.workflowConfigurationStepGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.workflowConfigurationStepGrid.Name = "workflowConfigurationStepGrid";
			this.workflowConfigurationStepGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(318, 266, true);
			this.workflowConfigurationStepGrid.TabIndex = 0;
			// 
			// WorkflowConfigurationStepCollectionControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.workflowConfigurationStepGrid);
			this.Name = "WorkflowConfigurationStepCollectionControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(318, 266, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.workflowConfigurationStepGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		internal Enterprise.ZArchitecture.ZGrid workflowConfigurationStepGrid;
	}
}
