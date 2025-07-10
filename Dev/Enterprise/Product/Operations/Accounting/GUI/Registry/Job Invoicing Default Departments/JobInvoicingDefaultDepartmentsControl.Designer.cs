namespace Enterprise.Accounting.Registry.GUI
{
	partial class JobInvoicingDefaultDepartmentsControl
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
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			this.Grid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Grid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Registry.Business.JobInvoicingDefaultDepartmentsCollection);
			// 
			// Grid
			// 
			this.Grid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.Grid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Registry.Business.JobInvoicingDefaultDepartments)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Registry.Business.JobInvoicingDefaultDepartments)(null)).ConsolType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Registry.Business.JobInvoicingDefaultDepartments)(null)).Department)));
			this.Grid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobInvoicingDefaultDepartmentsControl|593db6a2-ab46-412d-a82a-fbd31c6dad0e", "Consol Type");
			zDropEditColumnStyleInfo1.ColumnName = "ConsolType";
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobInvoicingDefaultDepartmentsControl|d62f355c-06a5-4180-ac08-9d32d3a9bf8c", "Department");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "Department";
			this.Grid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.Grid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.Grid.GridId = "A9BFEE13-7E29-49B8-912F-178EDEE4F93A";
			this.Grid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.Grid.LayoutKey = "Grid";
			this.Grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.Grid.Name = "Grid";
			this.Grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(277, 150, true);
			this.Grid.TabIndex = 0;
			// 
			// JobInvoicingDefaultDepartmentsControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.Grid);
			this.Name = "JobInvoicingDefaultDepartmentsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(277, 150, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Grid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.ZGrid Grid;
	}
}
