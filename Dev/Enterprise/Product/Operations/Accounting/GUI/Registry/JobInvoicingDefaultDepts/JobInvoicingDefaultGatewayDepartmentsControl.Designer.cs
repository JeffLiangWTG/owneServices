using Enterprise.Accounting.Registry.Business;

namespace Enterprise.Accounting.Registry.GUI
{
	partial class JobInvoicingDefaultGatewayDepartmentsControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo consolTypeZDropEditColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo directionZDropEditColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo transportModeZDropEditColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();

			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo departmentZGuidFindBoxColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			this.Grid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Grid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Registry.Business.JobInvoicingDefaultGatewayDepartmentsCollection);
			// 
			// Grid
			// 
			this.Grid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.Grid, ".");
			this.Grid.CaptionVisible = false;			

			directionZDropEditColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			directionZDropEditColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobInvoicingDefaultDepartmentsControl|503B641B-5650-41E9-9554-3544F731246A", "Direction");
			directionZDropEditColumnStyleInfo.ColumnName = nameof(JobInvoicingDefaultGatewayDepartments.Direction);

			transportModeZDropEditColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			transportModeZDropEditColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobInvoicingDefaultDepartmentsControl|17FCB97C-63E0-4E2A-BC68-DC8A330AF76C", "Transport Mode");
			transportModeZDropEditColumnStyleInfo.ColumnName = nameof(JobInvoicingDefaultGatewayDepartments.TransportMode);

			consolTypeZDropEditColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			consolTypeZDropEditColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobInvoicingDefaultDepartmentsControl|593db6a2-ab46-412d-a82a-fbd31c6dad0e", "Consol Type");
			consolTypeZDropEditColumnStyleInfo.ColumnName = nameof(JobInvoicingDefaultGatewayDepartments.ConsolType);

			departmentZGuidFindBoxColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobInvoicingDefaultDepartmentsControl|d62f355c-06a5-4180-ac08-9d32d3a9bf8c", "Department");
			departmentZGuidFindBoxColumnStyleInfo.ColumnName = nameof(JobInvoicingDefaultGatewayDepartments.Department);
			this.Grid.ColumnStyles.Add(directionZDropEditColumnStyleInfo);
			this.Grid.ColumnStyles.Add(transportModeZDropEditColumnStyleInfo);
			this.Grid.ColumnStyles.Add(consolTypeZDropEditColumnStyleInfo);
			this.Grid.ColumnStyles.Add(departmentZGuidFindBoxColumnStyleInfo);
			this.Grid.GridId = "c5868921-4fdf-4206-9f8c-b76037c4dd9c";
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

