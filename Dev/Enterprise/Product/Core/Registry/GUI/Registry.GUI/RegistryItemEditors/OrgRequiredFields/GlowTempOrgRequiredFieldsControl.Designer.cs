namespace Enterprise.Registry.GUI
{
	partial class GlowTempOrgRequiredFieldsControl
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
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.GlowTempOrgRequiredFieldsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.GlowTempOrgRequiredFieldsGrid)).BeginInit();
			this.GlowTempOrgRequiredFieldsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.GlowTempOrgRequiredFieldCollection);
			// 
			// GlowTempOrgRequiredFieldsGrid
			// 
			this.GlowTempOrgRequiredFieldsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.GlowTempOrgRequiredFieldsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.GlowTempOrgRequiredField)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.GlowTempOrgRequiredField)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.GlowTempOrgRequiredField)(null)).EnglishDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.GlowTempOrgRequiredField)(null)).Bool)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.GlowTempOrgRequiredField)(null)).IsMandatory)));
			this.GlowTempOrgRequiredFieldsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("b7abf6d1-d9d3-4759-b971-ac26ee366b30", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "EnglishDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("fb657296-a806-41a5-b853-9c0d506121fa", "Enabled");
			zCheckBoxColumnStyleInfo1.ColumnName = "Bool";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("98A3B6D1-1D94-41E7-A48C-992CAE73C621", "Is Mandatory");
			zCheckBoxColumnStyleInfo2.ColumnName = "IsMandatory";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.GlowTempOrgRequiredFieldsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.GlowTempOrgRequiredFieldsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.GlowTempOrgRequiredFieldsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.GlowTempOrgRequiredFieldsGrid.CopySelectedRowsAllowed = true;
			this.GlowTempOrgRequiredFieldsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GlowTempOrgRequiredFieldsGrid.GridId = "6D987563-F201-49C6-9F79-1EA1B8ABE7B4";
			this.GlowTempOrgRequiredFieldsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.GlowTempOrgRequiredFieldsGrid.LayoutKey = "GlowTempOrgRequiredFieldsGrid";
			this.GlowTempOrgRequiredFieldsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GlowTempOrgRequiredFieldsGrid.Name = "GlowTempOrgRequiredFieldsGrid";
			this.GlowTempOrgRequiredFieldsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(331, 183, true);
			this.GlowTempOrgRequiredFieldsGrid.TabIndex = 0;
			// 
			// GlowOrgRequiredFieldsControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.GlowTempOrgRequiredFieldsGrid);
			this.Name = "GlowOrgRequiredFieldsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(331, 183, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.GlowTempOrgRequiredFieldsGrid)).EndInit();
			this.GlowTempOrgRequiredFieldsGrid.ResumeLayout(false);
			this.GlowTempOrgRequiredFieldsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid GlowTempOrgRequiredFieldsGrid;
	}
}
