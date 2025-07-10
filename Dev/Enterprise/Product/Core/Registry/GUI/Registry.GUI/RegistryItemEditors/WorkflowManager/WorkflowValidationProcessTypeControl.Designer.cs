namespace Enterprise.Registry.GUI
{
	partial class WorkflowValidationProcessTypeControl
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
			this.WorkflowValidationProcessTypesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.WorkflowValidationProcessTypesGrid)).BeginInit();
			this.WorkflowValidationProcessTypesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.WorkflowValidationProcessTypeCollection);
			// 
			// WorkflowValidationProcessTypesGrid
			// 
			this.WorkflowValidationProcessTypesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.WorkflowValidationProcessTypesGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.WorkflowValidationProcessType)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.WorkflowValidationProcessType)(null)).ProcessType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.WorkflowValidationProcessType)(null)).ProcessTypeDescription)));
			this.WorkflowValidationProcessTypesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("04FC65EB-BC32-43E2-BF57-3EA177A12C73", "Process Type");
			zDropEditColumnStyleInfo1.ColumnName = "ProcessType";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("F179951B-8812-42F7-93A1-A900B89FCD43", "Process Type Description");
			zTextBoxColumnStyleInfo1.ColumnName = "ProcessTypeDescription";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.WorkflowValidationProcessTypesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.WorkflowValidationProcessTypesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.WorkflowValidationProcessTypesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.WorkflowValidationProcessTypesGrid.GridId = "295766B1-4CCB-431A-930E-00C8082ADB76";
			this.WorkflowValidationProcessTypesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.WorkflowValidationProcessTypesGrid.LayoutKey = "WorkflowValidationProcessTypesGrid";
			this.WorkflowValidationProcessTypesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.WorkflowValidationProcessTypesGrid.Name = "WorkflowValidationProcessTypesGrid";
			this.WorkflowValidationProcessTypesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 150, true);
			this.WorkflowValidationProcessTypesGrid.TabIndex = 1;
			// 
			// WorkflowValidationProcessTypeControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.WorkflowValidationProcessTypesGrid);
			this.Name = "WorkflowValidationProcessTypeControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 150, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.WorkflowValidationProcessTypesGrid)).EndInit();
			this.WorkflowValidationProcessTypesGrid.ResumeLayout(false);
			this.WorkflowValidationProcessTypesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid WorkflowValidationProcessTypesGrid;
	}
}
