using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	partial class WorkflowManagerIterationReasonsControl : RegistryZUserControl
	{
		ZGroupBox ParentGroupBox;
		ZGroupBox ValidationGroupBox;
		ZGroupBox ChildGroupBox;
		ZArchitecture.ZGrid ChildGrid;
		ZDropEdit ValidationDropEdit;
		ZArchitecture.ZGrid ParentGrid;

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ParentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ParentGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ValidationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ValidationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ChildGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ChildGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ParentGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ParentGrid)).BeginInit();
			this.ChildGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ChildGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.CategorisedWorkflowIterationReasonsCollection);
			// 
			// ParentGroupBox
			// 
			this.ParentGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("WorkflowManagerIterationReasonsControl|a0437d9e-cd27-4abb-8899-5f252ef6a7d9", "Workflow Types");
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
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.CategorisedWorkflowIterationReasons)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.CategorisedWorkflowIterationReasons)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.CategorisedWorkflowIterationReasons)(null)).EnglishDescription)));
			this.ParentGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("WorkflowManagerIterationReasonsControl|a281449a-0228-4d88-8de2-b8347b05a8b3", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("WorkflowManagerIterationReasonsControl|9f8b3532-9248-4e3d-b35e-3c617123d8fd", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "EnglishDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			this.ParentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ParentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ParentGrid.CopySelectedRowsAllowed = true;
			this.ParentGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ParentGrid.GridId = "ad3fb8ed-f3d4-4dc1-b5b8-6d570ce3ab04";
			this.ParentGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ParentGrid.LayoutKey = "zGrid1";
			this.ParentGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ParentGrid.Name = "ParentGrid";
			this.ParentGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(475, 133, true);
			this.ParentGrid.TabIndex = 0;
			// 
			// ValidationGroupBox
			// 
			this.ValidationGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("WorkflowManagerIterationReasonsControl|63611566-7c37-4fe8-8f8e-5458fa3acd6b", "Validation Type");
			this.ValidationGroupBox.Controls.Add(this.ValidationDropEdit);
			this.ValidationGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.ValidationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 152, true);
			this.ValidationGroupBox.Name = "ValidationGroupBox";
			this.ValidationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(481, 39, true);
			this.ValidationGroupBox.TabIndex = 3;
			this.ValidationGroupBox.TabStop = false;
			// 
			// ValidationDropEdit
			// 
			this.ValidationDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ValidationDropEdit, "IterationReasonValidation");
			this.ValidationDropEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("WorkflowManagerIterationReasonsControl|34cfd018-80c3-468e-8b20-248270947067", "", "The list of reasons that can be given for triggering a quality iteration. You may select whether a reason is not required, gives a warning if not supplied, or gives an error.");
			this.ValidationDropEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ValidationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ValidationDropEdit.Name = "ValidationDropEdit";
			this.ValidationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(475, 20, true);
			this.ValidationDropEdit.TabIndex = 0;
			// 
			// ChildGroupBox
			// 
			this.ChildGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("WorkflowManagerIterationReasonsControl|7d51012b-568f-4d28-89bb-bfcf1440ef43", "Iteration Reasons");
			this.ChildGroupBox.Controls.Add(this.ChildGrid);
			this.ChildGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ChildGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 191, true);
			this.ChildGroupBox.Name = "ChildGroupBox";
			this.ChildGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(481, 165, true);
			this.ChildGroupBox.TabIndex = 4;
			this.ChildGroupBox.TabStop = false;
			// 
			// ChildGrid
			// 
			this.ChildGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ChildGrid, "IterationReasons");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.CategorisedWorkflowIterationReasons)(null)).IterationReasons)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.WorkflowIterationReason)(((System.Collections.IList)(((Enterprise.Registry.Business.CategorisedWorkflowIterationReasons)(null)).IterationReasons)).SyncRoot)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.WorkflowIterationReason)(((System.Collections.IList)(((Enterprise.Registry.Business.CategorisedWorkflowIterationReasons)(null)).IterationReasons)).SyncRoot)).EnglishDescription)));
			this.ChildGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("WorkflowManagerIterationReasonsControl|a281449a-0228-4d88-8de2-b8347b05a8b3", "Code");
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "Code";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("WorkflowManagerIterationReasonsControl|9f8b3532-9248-4e3d-b35e-3c617123d8fd", "Description");
			zTextBoxColumnStyleInfo4.ColumnName = "EnglishDescription";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			this.ChildGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ChildGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ChildGrid.CopySelectedRowsAllowed = true;
			this.ChildGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ChildGrid.GridId = "5e397659-cb49-4420-beea-f419f610e11f";
			this.ChildGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ChildGrid.LayoutKey = "zGrid1";
			this.ChildGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ChildGrid.Name = "ChildGrid";
			this.ChildGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(475, 146, true);
			this.ChildGrid.TabIndex = 1;
			// 
			// WorkflowManagerIterationReasonsControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ChildGroupBox);
			this.Controls.Add(this.ValidationGroupBox);
			this.Controls.Add(this.ParentGroupBox);
			this.Name = "WorkflowManagerIterationReasonsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(481, 356, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ParentGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ParentGrid)).EndInit();
			this.ChildGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ChildGrid)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
