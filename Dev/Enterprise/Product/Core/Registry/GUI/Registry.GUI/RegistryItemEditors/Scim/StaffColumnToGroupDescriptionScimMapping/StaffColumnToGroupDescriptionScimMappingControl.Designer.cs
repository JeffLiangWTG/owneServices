namespace Enterprise.Registry.GUI;
partial class StaffColumnToGroupDescriptionScimMappingControl
{
	/// <summary> 
	/// Required designer variable.
	/// </summary>
	System.ComponentModel.IContainer components = null;

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
		Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
		this.Grid = new Enterprise.ZArchitecture.ZGrid();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		((System.ComponentModel.ISupportInitialize)(this.Grid)).BeginInit();
		this.Grid.SuspendLayout();
		this.SuspendLayout();
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.StaffColumnToGroupDescriptionScimMappingCollection);
		// 
		// Grid
		// 
		this.Grid.AllowNavigation = false;
		this.BindingSource.SetBindingMember(this.Grid, ".");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.StaffColumnToGroupDescriptionScimMapping)(null)))));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.StaffColumnToGroupDescriptionScimMapping)(null)).GroupDescriptionMapping)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.StaffColumnToGroupDescriptionScimMapping)(null)).StaffColumnName)));
		this.Grid.CaptionVisible = false;
		zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("EA25FD32-5D4F-4635-9A4F-EF7472604BEB", "Group Description");
		zTextBoxColumnStyleInfo1.ColumnName = "GroupDescriptionMapping";
		zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
		zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("0F9D2CDD-E467-46AC-AD19-6B4A9DAB148F", "Staff Column");
		zDropEditColumnStyleInfo1.ColumnName = "StaffColumnName";
		zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
		zDropEditColumnStyleInfo1.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
		this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
		this.Grid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
		this.Grid.Dock = System.Windows.Forms.DockStyle.Fill;
		this.Grid.GridId = "304361CF-2465-407F-B3E5-CA334DD9A950";
		this.Grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
		this.Grid.LayoutKey = "zGrid1";
		this.Grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
		this.Grid.Name = "Grid";
		this.Grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 400, true);
		this.Grid.TabIndex = 0;
		// 
		// StaffColumnToGroupDescriptionScimMappingControl
		// 
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.CaptionRenderingEnabled = true;
		this.Controls.Add(this.Grid);
		this.Name = "StaffColumnToGroupDescriptionScimMappingControl";
		this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(305, 400, true);
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		((System.ComponentModel.ISupportInitialize)(this.Grid)).EndInit();
		this.Grid.ResumeLayout(false);
		this.Grid.PerformLayout();
		this.ResumeLayout(false);
		this.PerformLayout();
	}

	#endregion

	internal ZArchitecture.ZGrid Grid;
}
