namespace Enterprise.Security.ActiveDirectory.GUI.Registry
{
	partial class AttributeMapControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.attributeMapGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.attributeMapGrid)).BeginInit();
			this.attributeMapGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Security.ActiveDirectory.AttributeMap);
			// 
			// attributeMapGrid
			// 
			this.attributeMapGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.attributeMapGrid, "MapItems");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Security.ActiveDirectory.AttributeMap)(null)).MapItems)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Security.ActiveDirectory.AttributeMapItem)(((System.Collections.IList)(((Enterprise.Security.ActiveDirectory.AttributeMap)(null)).MapItems)).SyncRoot)).EnterpriseColumnName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Security.ActiveDirectory.AttributeMapItem)(((System.Collections.IList)(((Enterprise.Security.ActiveDirectory.AttributeMap)(null)).MapItems)).SyncRoot)).ActiveDirectoryAttributeName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Security.ActiveDirectory.AttributeMapItem)(((System.Collections.IList)(((Enterprise.Security.ActiveDirectory.AttributeMap)(null)).MapItems)).SyncRoot)).IsSynced)));
			this.attributeMapGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "EnterpriseColumnName";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zDropEditColumnStyleInfo2.ColumnName = "ActiveDirectoryAttributeName";
			zDropEditColumnStyleInfo2.IsReadOnly = false;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(184);
			zDropEditColumnStyleInfo2.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zCheckBoxColumnStyleInfo1.ColumnName = "IsSynced";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.attributeMapGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.attributeMapGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.attributeMapGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.attributeMapGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.attributeMapGrid.GridId = "486e20a8-5f64-488e-a06a-4d6feafec39a";
			this.attributeMapGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.attributeMapGrid.LayoutKey = "zGrid1";
			this.attributeMapGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.attributeMapGrid.Name = "attributeMapGrid";
			this.attributeMapGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(438, 379, true);
			this.attributeMapGrid.TabIndex = 0;
			// 
			// AttributeMapControl
			// 
			this.BackColor = System.Drawing.SystemColors.Window;
			this.Controls.Add(this.attributeMapGrid);
			this.Name = "AttributeMapControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(438, 379, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.attributeMapGrid)).EndInit();
			this.attributeMapGrid.ResumeLayout(false);
			this.attributeMapGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private Enterprise.ZArchitecture.ZGrid attributeMapGrid;

		#endregion
	}
}
