namespace Enterprise.Client.EDI.Registry.GUI
{
	partial class ProductAreaAssignmentsControl
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
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			this.AssignmentGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AssignmentGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Registry.Business.ProductAreaAssignmentCollection);
			// 
			// AssignmentGrid
			// 
			this.AssignmentGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AssignmentGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.ProductAreaAssignment)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.ProductAreaAssignment)(null)).ProductArea)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.ProductAreaAssignment)(null)).Staff)));
			this.AssignmentGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = ZClientEDI.Res.GetData("61723011-0841-42db-a0a3-f282f1151deb", "Product Area");
			zDropEditColumnStyleInfo1.ColumnName = "ProductArea";
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = ZClientEDI.Res.GetData("8f51a212-53c1-435e-91c3-86ff0104beba", "Staff");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "Staff";
			this.AssignmentGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.AssignmentGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.AssignmentGrid.CopySelectedRowsAllowed = true;
			this.AssignmentGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AssignmentGrid.GridId = "c3793ccc-0847-4b35-9e5f-4316803391c3";
			this.AssignmentGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AssignmentGrid.LayoutKey = "AssignmentGrid";
			this.AssignmentGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AssignmentGrid.Name = "AssignmentGrid";
			this.AssignmentGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 263, true);
			this.AssignmentGrid.TabIndex = 0;
			// 
			// ProductAreaAssignmentsControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.AssignmentGrid);
			this.Name = "ProductAreaAssignmentsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 263, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AssignmentGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		internal ZArchitecture.ZGrid AssignmentGrid;
	}
}
