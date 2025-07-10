using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.GUI;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.BufferManagement.GUI
{
	public partial class WorkflowCategoriesControl
	{
		void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ParentGroupBox = new ZGroupBox();
			this.ParentGrid = new ZArchitecture.ZGrid();
			this.ChildGroupBox = new ZGroupBox();
			this.ChildGrid = new ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ParentGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ParentGrid)).BeginInit();
			this.ParentGrid.SuspendLayout();
			this.ChildGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ChildGrid)).BeginInit();
			this.ChildGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.CategorisedWorkflowCategoriesCollection);
			// 
			// ParentGroupBox
			// 
			this.ParentGroupBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("WorkflowCategoriesControl|51d0bbb8-f728-4232-b2c9-fb9ad6cb8894", "Workflow Types");
			this.ParentGroupBox.Controls.Add(this.ParentGrid);
			this.ParentGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.ParentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ParentGroupBox.Name = "ParentGroupBox";
			this.ParentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(481, 152, true);
			this.ParentGroupBox.TabIndex = 1;
			this.ParentGroupBox.TabStop = false;
			// 
			// ParentGrid
			// 
			this.ParentGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ParentGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.CategorisedWorkflowCategories)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.CategorisedWorkflowCategories)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.CategorisedWorkflowCategories)(null)).EnglishDescription)));
			this.ParentGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("WorkflowCategoriesControl|a281449a-0228-4d88-8de2-b8347b05a8b3", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("WorkflowCategoriesControl|9f8b3532-9248-4e3d-b35e-3c617123d8fd", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "EnglishDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			this.ParentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ParentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ParentGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ParentGrid.GridId = "ad3fb8ed-f3d4-4dc1-b5b8-6d570ce3ab04";
			this.ParentGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ParentGrid.LayoutKey = "zGrid1";
			this.ParentGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ParentGrid.Name = "ParentGrid";
			this.ParentGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(475, 133, true);
			this.ParentGrid.TabIndex = 0;
			// 
			// ChildGroupBox
			// 
			this.ChildGroupBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("WorkflowCategoriesControl|7d51012b-568f-4d28-89bb-bfcf1440ef43", "Categories");
			this.ChildGroupBox.Controls.Add(this.ChildGrid);
			this.ChildGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ChildGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 152, true);
			this.ChildGroupBox.Name = "ChildGroupBox";
			this.ChildGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(481, 204, true);
			this.ChildGroupBox.TabIndex = 4;
			this.ChildGroupBox.TabStop = false;
			// 
			// ChildGrid
			// 
			this.ChildGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ChildGrid, "Categories");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.CategorisedWorkflowCategories)(null)).Categories)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.WorkflowCategory)(((System.Collections.IList)(((Business.CategorisedWorkflowCategories)(null)).Categories)).SyncRoot)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.WorkflowCategory)(((System.Collections.IList)(((Business.CategorisedWorkflowCategories)(null)).Categories)).SyncRoot)).EnglishDescription)));
			this.ChildGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("WorkflowCategoriesControl|a281449a-0228-4d88-8de2-b8347b05a8b3", "Code");
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "Code";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("WorkflowCategoriesControl|9f8b3532-9248-4e3d-b35e-3c617123d8fd", "Description");
			zTextBoxColumnStyleInfo4.ColumnName = "EnglishDescription";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			this.ChildGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ChildGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ChildGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ChildGrid.GridId = "5e397659-cb49-4420-beea-f419f610e11f";
			this.ChildGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ChildGrid.LayoutKey = "zGrid1";
			this.ChildGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ChildGrid.Name = "ChildGrid";
			this.ChildGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(475, 185, true);
			this.ChildGrid.TabIndex = 1;
			// 
			// WorkflowCategoriesControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ChildGroupBox);
			this.Controls.Add(this.ParentGroupBox);
			this.Name = "WorkflowCategoriesControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(481, 356, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ParentGroupBox.ResumeLayout(false);
			this.ParentGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ParentGrid)).EndInit();
			this.ParentGrid.ResumeLayout(false);
			this.ParentGrid.PerformLayout();
			this.ChildGroupBox.ResumeLayout(false);
			this.ChildGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ChildGrid)).EndInit();
			this.ChildGrid.ResumeLayout(false);
			this.ChildGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

	}
}
