using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	partial class WorkflowManagerAssistWithThisTaskControl
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
            this.ParentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.ParentGrid = new Enterprise.ZArchitecture.ZGrid();
            this.ChildGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.WorkflowTypeBox = new Enterprise.ZArchitecture.ZTextBox();
            this.VariationFactorBox = new Enterprise.ZArchitecture.ZCalcEdit();
            this.LowEstimateMinutesBox = new Enterprise.ZArchitecture.ZCalcEdit();
            this.TaskTypeBox = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.ParentGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ParentGrid)).BeginInit();
            this.ParentGrid.SuspendLayout();
            this.ChildGroupBox.SuspendLayout();
            this.TaskTypeBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.CategorisedAssistWithThisTaskSettingCollection);
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
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.CategorisedAssistWithThisTaskSetting)(null)))));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.CategorisedAssistWithThisTaskSetting)(null)).Code)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.CategorisedAssistWithThisTaskSetting)(null)).EnglishDescription)));
            this.ParentGrid.CaptionVisible = false;
            zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("a8e831f6-0dfc-4d7d-b494-5370dc200431", "Code");
            zTextBoxColumnStyleInfo1.ColumnName = "Code";
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
            zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("93b94a21-37e0-47d5-aabf-9ff8b0b6f949", "Description");
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
            this.ParentGrid.TabIndex = 2;
            // 
            // ChildGroupBox
            // 
            this.ChildGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("c9e6a3d0-410b-46e3-bcc0-545471264fc1", "Settings for selected workflow type");
            this.ChildGroupBox.Controls.Add(this.WorkflowTypeBox);
            this.ChildGroupBox.Controls.Add(this.VariationFactorBox);
            this.ChildGroupBox.Controls.Add(this.LowEstimateMinutesBox);
            this.ChildGroupBox.Controls.Add(this.TaskTypeBox);
            this.ChildGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ChildGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 152, true);
            this.ChildGroupBox.Name = "ChildGroupBox";
            this.ChildGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(481, 204, true);
            this.ChildGroupBox.TabIndex = 3;
            this.ChildGroupBox.TabStop = false;
            // 
            // WorkflowTypeBox
            // 
            this.BindingSource.SetBindingMember(this.WorkflowTypeBox, "Code");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.CategorisedAssistWithThisTaskSetting)(null)).Code)));
            this.WorkflowTypeBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("c9befa31-b9fe-4533-afaf-4d0fae10b062", "Workflow Type");
            this.WorkflowTypeBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.WorkflowTypeBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 19, true);
            this.WorkflowTypeBox.Name = "WorkflowTypeBox";
            this.WorkflowTypeBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(39, 20, true);
            this.WorkflowTypeBox.TabIndex = 4;
            this.WorkflowTypeBox.Text = "0";
            this.WorkflowTypeBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // VariationFactorBox
            // 
            this.BindingSource.SetBindingMember(this.VariationFactorBox, "Setting.VariationFactor");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.CategorisedAssistWithThisTaskSetting)(null)).Setting.VariationFactor)));
            this.VariationFactorBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("3497ace4-2311-4003-87fb-0bd50b19aca2", "Variation Factor");
            this.VariationFactorBox.DecimalPlaces = 0;
            this.VariationFactorBox.Decimals = 0;
            this.VariationFactorBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 97, true);
            this.VariationFactorBox.Name = "VariationFactorBox";
            this.VariationFactorBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(39, 20, true);
            this.VariationFactorBox.TabIndex = 7;
            this.VariationFactorBox.Text = "0";
            this.VariationFactorBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // LowEstimateMinutesBox
            // 
            this.BindingSource.SetBindingMember(this.LowEstimateMinutesBox, "Setting.LowEstimateMinutes");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.CategorisedAssistWithThisTaskSetting)(null)).Setting.LowEstimateMinutes)));
            this.LowEstimateMinutesBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("03c7ba04-319f-41a9-bbcb-ec7c85f5cb32", "Low Estimate Minutes");
            this.LowEstimateMinutesBox.DecimalPlaces = 0;
            this.LowEstimateMinutesBox.Decimals = 0;
            this.LowEstimateMinutesBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 71, true);
            this.LowEstimateMinutesBox.Name = "LowEstimateMinutesBox";
            this.LowEstimateMinutesBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(39, 20, true);
            this.LowEstimateMinutesBox.TabIndex = 6;
            this.LowEstimateMinutesBox.Text = "0";
            this.LowEstimateMinutesBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // TaskTypeBox
            // 
            this.TaskTypeBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.TaskTypeBox, "Setting.TaskType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Registry.Business.CategorisedAssistWithThisTaskSetting)(null)).Setting.TaskType)));
            this.TaskTypeBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("0489251d-697e-4d01-897b-07ab0bf5ad73", "Task Type");
            this.TaskTypeBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 45, true);
            this.TaskTypeBox.Name = "TaskTypeBox";
            this.TaskTypeBox.ShouldResizeByMaxLength = true;
            this.TaskTypeBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
            this.TaskTypeBox.TabIndex = 5;
            // 
            // WorkflowManagerAssistWithThisTaskControl
            // 
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.ChildGroupBox);
            this.Controls.Add(this.ParentGroupBox);
            this.Name = "WorkflowManagerAssistWithThisTaskControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(481, 356, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ParentGroupBox.ResumeLayout(false);
            this.ParentGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ParentGrid)).EndInit();
            this.ParentGrid.ResumeLayout(false);
            this.ParentGrid.PerformLayout();
            this.ChildGroupBox.ResumeLayout(false);
            this.ChildGroupBox.PerformLayout();
            this.TaskTypeBox.ResumeLayout(true);
            this.TaskTypeBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		ZGroupBox ParentGroupBox;
		ZGroupBox ChildGroupBox;
		ZArchitecture.ZGrid ParentGrid;
		ZDropEdit TaskTypeBox;
		ZCalcEdit LowEstimateMinutesBox;
		ZCalcEdit VariationFactorBox;
		ZTextBox WorkflowTypeBox;
	}
}
