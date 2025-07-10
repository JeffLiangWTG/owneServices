namespace Enterprise.Customs.CA.GUI
{
	partial class InvoiceLineAVSQueryUserControl
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
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			this.AVSQueryGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AVSQueryGrid)).BeginInit();
			this.AVSQueryGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.AVSQueryResult);
			// 
			// AVSQueryGrid
			// 
			this.AVSQueryGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AVSQueryGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.AVSQueryResult)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.CA.Business.AVSQueryResult)(null)).QueryTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.AVSQueryResult)(null)).QueryResult)));
			this.AVSQueryGrid.CaptionVisible = false;
			zDateEditColumnStyleInfo1.ColumnName = "QueryTime";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zMultiLineTextBoxColumnInfo1.ColumnName = "QueryResult";
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(500);
			this.AVSQueryGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.AVSQueryGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.AVSQueryGrid.CopySelectedRowsAllowed = true;
			this.AVSQueryGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AVSQueryGrid.GridId = "30613c66-72ea-4e43-9f89-30bf06efeca4";
			this.AVSQueryGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AVSQueryGrid.LayoutKey = "AVSQueryGrid";
			this.AVSQueryGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AVSQueryGrid.Name = "AVSQueryGrid";
			this.AVSQueryGrid.ReadOnly = true;
			this.AVSQueryGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(606, 335, true);
			this.AVSQueryGrid.TabIndex = 0;
			// 
			// InvoiceLineAVSQueryUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.AVSQueryGrid);
			this.Name = "InvoiceLineAVSQueryUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(606, 335, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AVSQueryGrid)).EndInit();
			this.AVSQueryGrid.ResumeLayout(false);
			this.AVSQueryGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZGrid AVSQueryGrid;
	}
}
