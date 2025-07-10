
namespace Enterprise.Client.JAS.Registry.GUI
{
	partial class CognosModeMappingControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.SelectedDeptGrid = new Enterprise.ZArchitecture.ZGrid();
			this.AvailableDeptGrid = new Enterprise.ZArchitecture.ZGrid();
			this.MapButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.UnmapButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ModeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zGroupBox2 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SelectedDeptGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AvailableDeptGrid)).BeginInit();
			this.zGroupBox1.SuspendLayout();
			this.zGroupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.JAS.Registry.Business.CognosModeMapping);
			// 
			// SelectedDeptGrid
			// 
			this.SelectedDeptGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SelectedDeptGrid, "MappedDepartments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.JAS.Registry.Business.CognosModeMapping)(null)).MappedDepartments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbDepartment)(((System.Collections.IList)(((Enterprise.Client.JAS.Registry.Business.CognosModeMapping)(null)).MappedDepartments)).SyncRoot)).GE_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbDepartment)(((System.Collections.IList)(((Enterprise.Client.JAS.Registry.Business.CognosModeMapping)(null)).MappedDepartments)).SyncRoot)).GE_Desc)));
			this.SelectedDeptGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "Code";
			zTextBoxColumnStyleInfo1.ColumnName = "GE_Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo2.Caption = "Description";
			zTextBoxColumnStyleInfo2.ColumnName = "GE_Desc";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			this.SelectedDeptGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.SelectedDeptGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.SelectedDeptGrid.GridId = "9481a5b0-31ad-4094-9329-a70c9b574adf";
			this.SelectedDeptGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SelectedDeptGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SelectedDeptGrid.IsWholeRowSelectedOnClick = true;
			this.SelectedDeptGrid.LayoutKey = "CognosModesGrid";
			this.SelectedDeptGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.SelectedDeptGrid.Name = "SelectedDeptGrid";
			this.SelectedDeptGrid.ReadOnly = true;
			this.SelectedDeptGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(385, 117, true);
			this.SelectedDeptGrid.TabIndex = 0;
			// 
			// AvailableDeptGrid
			// 
			this.AvailableDeptGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AvailableDeptGrid, "AvailableDepartments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.JAS.Registry.Business.CognosModeMapping)(null)).AvailableDepartments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbDepartment)(((System.Collections.IList)(((Enterprise.Client.JAS.Registry.Business.CognosModeMapping)(null)).AvailableDepartments)).SyncRoot)).GE_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbDepartment)(((System.Collections.IList)(((Enterprise.Client.JAS.Registry.Business.CognosModeMapping)(null)).AvailableDepartments)).SyncRoot)).GE_DescMultilingual)));
			this.AvailableDeptGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo3.Caption = "Code";
			zTextBoxColumnStyleInfo3.ColumnName = "GE_Code";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo4.Caption = "Description";
			zTextBoxColumnStyleInfo4.ColumnName = "GE_DescMultilingual";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			this.AvailableDeptGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.AvailableDeptGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.AvailableDeptGrid.GridId = "4eb33677-51f5-4bb7-8bf2-1a5693cd0430";
			this.AvailableDeptGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AvailableDeptGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AvailableDeptGrid.IsWholeRowSelectedOnClick = true;
			this.AvailableDeptGrid.LayoutKey = "CognosModesGrid";
			this.AvailableDeptGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.AvailableDeptGrid.Name = "AvailableDeptGrid";
			this.AvailableDeptGrid.ReadOnly = true;
			this.AvailableDeptGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(385, 141, true);
			this.AvailableDeptGrid.TabIndex = 0;
			// 
			// MapButton
			// 
			this.MapButton.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.MapButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(153, 177, true);
			this.MapButton.Name = "MapButton";
			this.MapButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(49, 23, true);
			this.MapButton.TabIndex = 1;
			this.MapButton.Text = ".";
			this.MapButton.UseVisualStyleBackColor = true;
			this.MapButton.Click += new System.EventHandler(this.MapButton_Click);
			// 
			// UnmapButton
			// 
			this.UnmapButton.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.UnmapButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(208, 177, true);
			this.UnmapButton.Name = "UnmapButton";
			this.UnmapButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(49, 23, true);
			this.UnmapButton.TabIndex = 1;
			this.UnmapButton.Text = ".";
			this.UnmapButton.UseVisualStyleBackColor = true;
			this.UnmapButton.Click += new System.EventHandler(this.UnmapButton_Click);
			// 
			// ModeDropEdit
			// 
			this.ModeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ModeDropEdit, "SelectedMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.JAS.Registry.Business.CognosModeMapping)(null)).SelectedMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.JAS.Registry.Business.CognosModeMapping)(null)).Modes)));
			this.ModeDropEdit.BindToList = "Modes";
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ModeDropEdit, false);
			this.ModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 9, true);
			this.ModeDropEdit.Name = "ModeDropEdit";
			this.ModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(315, 20, true);
			this.ModeDropEdit.TabIndex = 2;
			// 
			// ModeLabel
			// 
			this.ModeLabel.AutoSize = true;
			this.ModeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 12, true);
			this.ModeLabel.Name = "ModeLabel";
			this.ModeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 13, true);
			this.ModeLabel.TabIndex = 3;
			this.ModeLabel.Text = "Mode to Map";
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.zGroupBox1.Controls.Add(this.SelectedDeptGrid);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 33, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(391, 136, true);
			this.zGroupBox1.TabIndex = 4;
			this.zGroupBox1.TabStop = false;
			this.zGroupBox1.Text = "Mapped Departments";
			// 
			// zGroupBox2
			// 
			this.zGroupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.zGroupBox2.Controls.Add(this.AvailableDeptGrid);
			this.zGroupBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 204, true);
			this.zGroupBox2.Name = "zGroupBox2";
			this.zGroupBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(391, 160, true);
			this.zGroupBox2.TabIndex = 5;
			this.zGroupBox2.TabStop = false;
			this.zGroupBox2.Text = "Available Departments";
			// 
			// CognosModeMappingControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.zGroupBox2);
			this.Controls.Add(this.zGroupBox1);
			this.Controls.Add(this.ModeLabel);
			this.Controls.Add(this.ModeDropEdit);
			this.Controls.Add(this.UnmapButton);
			this.Controls.Add(this.MapButton);
			this.Name = "CognosModeMappingControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 370, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SelectedDeptGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AvailableDeptGrid)).EndInit();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox2.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.ZGrid SelectedDeptGrid;
		internal Enterprise.ZArchitecture.ZGrid AvailableDeptGrid;
		internal Enterprise.ZArchitecture.GUI.ZButton MapButton;
		internal Enterprise.ZArchitecture.GUI.ZButton UnmapButton;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit ModeDropEdit;
		internal Enterprise.ZArchitecture.ZLabel ModeLabel;
		internal Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBox1;
		internal Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBox2;
	}
}
