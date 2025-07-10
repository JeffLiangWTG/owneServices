namespace Enterprise.Registry.GUI
{
	partial class StaffReportingRoleRegistryControl
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
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.StaffReportingRoleGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.StaffReportingRoleGrid)).BeginInit();
			this.StaffReportingRoleGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.StaffReportingRoleCollection);
			// 
			// OpportunityStatusGrid
			// 
			this.StaffReportingRoleGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.StaffReportingRoleGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.StaffReportingRole)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.StaffReportingRole)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.StaffReportingRole)(null)).EnglishDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.StaffReportingRole)(null)).IsMandatory)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.StaffReportingRole)(null)).SharedRoleAllowed)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.StaffReportingRole)(null)).Bool)));
			this.StaffReportingRoleGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("1162ffd2-69a7-4537-8048-18af4b0f7267", "Code");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("53d64a89-6f49-43ca-b315-8829b2c40032", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "EnglishDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("c1b6d4b7-13a1-4faa-a8c1-23b315c1478d", "Mandatory");
			zCheckBoxColumnStyleInfo1.ColumnName = "IsMandatory";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("cf9e9850-b98d-412b-960a-fb94bcffa29a", "Shared Role Allowed");
			zCheckBoxColumnStyleInfo2.ColumnName = "SharedRoleAllowed";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zCheckBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("5e73ec97-4005-4fce-982e-9b6a0c28a4b7", "Enabled");
			zCheckBoxColumnStyleInfo3.ColumnName = "Bool";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			this.StaffReportingRoleGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.StaffReportingRoleGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.StaffReportingRoleGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.StaffReportingRoleGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.StaffReportingRoleGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.StaffReportingRoleGrid.CopySelectedRowsAllowed = true;
			this.StaffReportingRoleGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.StaffReportingRoleGrid.GridId = "0535be6a-62f1-49de-847e-4376422265a9";
			this.StaffReportingRoleGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.StaffReportingRoleGrid.LayoutKey = "StaffReportingRoleGrid";
			this.StaffReportingRoleGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.StaffReportingRoleGrid.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.StaffReportingRoleGrid.Name = "StaffReportingRoleGrid";
			this.StaffReportingRoleGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 300, true);
			this.StaffReportingRoleGrid.TabIndex = 0;
			// 
			// StaffReportingRoleRegistryControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.StaffReportingRoleGrid);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.Name = "StaffReportingRoleRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 300, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.StaffReportingRoleGrid)).EndInit();
			this.StaffReportingRoleGrid.ResumeLayout(false);
			this.StaffReportingRoleGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid StaffReportingRoleGrid;
	}
}
