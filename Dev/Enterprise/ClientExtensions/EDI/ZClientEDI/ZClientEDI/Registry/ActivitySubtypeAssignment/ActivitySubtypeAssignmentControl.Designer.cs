namespace Enterprise.Client.EDI.Registry.GUI
{
	partial class ActivitySubtypeAssignmentsControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZArchitecture.ZCheckBoxColumnStyleInfo();

			this.AssignmentGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AssignmentGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Registry.Business.ActivitySubtypeAssignmentCollection);
			// 
			// AssignmentGrid
			// 
			this.AssignmentGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AssignmentGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.ActivitySubtypeAssignment)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.ActivitySubtypeAssignment)(null)).ActivitySubtype)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.ActivitySubtypeAssignment)(null)).Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.Registry.Business.ActivitySubtypeAssignment)(null)).Capitalization)));
			this.AssignmentGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = ZClientEDI.Res.GetData("ec19587a-734f-459d-8103-a8ce4dde39d2", "Activity Sub Type");
			zDropEditColumnStyleInfo1.ColumnName = "ActivitySubtype";
			zTextBoxColumnStyleInfo1.CaptionResourceString = ZClientEDI.Res.GetData("26707907-27c0-4358-b25a-c0af72105513", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "Description";
			zCheckBoxColumnStyleInfo1.CaptionResourceString = ZClientEDI.Res.GetData("19514239-252d-4221-8a25-6ef258188492", "Capitalization");
			zCheckBoxColumnStyleInfo1.ColumnName = "Capitalization";

			this.AssignmentGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.AssignmentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AssignmentGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.AssignmentGrid.CopySelectedRowsAllowed = true;
			this.AssignmentGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AssignmentGrid.GridId = "fb4df9b9-979d-4291-8778-8c0ac10c3fac";
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
