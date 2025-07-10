namespace Enterprise.Registry.GUI
{
	partial class WebThemeSelectorControl
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
			this.kSplitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.kSplitContainer2 = new CargoWise.Windows.UI.KSplitContainer();
			this.ThemeListGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ThemeListGrid = new Enterprise.ZArchitecture.ZGrid();
			this.webThemeChildControl = new Enterprise.Registry.GUI.WebThemeSelectorChildControl();
			this.DeleteThemeButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CreateThemeButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CopyThemeButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.kSplitContainer1)).BeginInit();
			this.kSplitContainer1.Panel1.SuspendLayout();
			this.kSplitContainer1.Panel2.SuspendLayout();
			this.kSplitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.kSplitContainer2)).BeginInit();
			this.kSplitContainer2.Panel1.SuspendLayout();
			this.kSplitContainer2.Panel2.SuspendLayout();
			this.kSplitContainer2.SuspendLayout();
			this.ThemeListGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ThemeListGrid)).BeginInit();
			this.ThemeListGrid.SuspendLayout();
			this.webThemeChildControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.WebThemeCustomObjectCollection);
			// 
			// kSplitContainer1
			// 
			this.kSplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.kSplitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.kSplitContainer1.Name = "kSplitContainer1";
			this.kSplitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// kSplitContainer1.Panel1
			// 
			this.kSplitContainer1.Panel1.Controls.Add(this.kSplitContainer2);
			// 
			// kSplitContainer1.Panel2
			// 
			this.kSplitContainer1.Panel2.Controls.Add(this.DeleteThemeButton);
			this.kSplitContainer1.Panel2.Controls.Add(this.CreateThemeButton);
			this.kSplitContainer1.Panel2.Controls.Add(this.CopyThemeButton);
			this.kSplitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(522, 415, true);
			this.kSplitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(382);
			this.kSplitContainer1.TabIndex = 12;
			// 
			// kSplitContainer2
			// 
			this.kSplitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.kSplitContainer2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.kSplitContainer2.Name = "kSplitContainer2";
			// 
			// kSplitContainer2.Panel1
			// 
			this.kSplitContainer2.Panel1.Controls.Add(this.ThemeListGroupBox);
			// 
			// kSplitContainer2.Panel2
			// 
			this.kSplitContainer2.Panel2.Controls.Add(this.webThemeChildControl);
			this.kSplitContainer2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(522, 382, true);
			this.kSplitContainer2.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(174);
			this.kSplitContainer2.TabIndex = 0;
			// 
			// ThemeListGroupBox
			// 
			this.ThemeListGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("42e2553d-3d0a-44d2-a389-6220b3b543de", "Theme Templates");
			this.ThemeListGroupBox.Controls.Add(this.ThemeListGrid);
			this.ThemeListGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ThemeListGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ThemeListGroupBox.Name = "ThemeListGroupBox";
			this.ThemeListGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(174, 382, true);
			this.ThemeListGroupBox.TabIndex = 0;
			this.ThemeListGroupBox.TabStop = false;
			// 
			// ThemeListGrid
			// 
			this.ThemeListGrid.AllowNavigation = false;
			this.ThemeListGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ThemeListGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.WebThemeCustomObject)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.WebThemeCustomObject)(null)).ThemeName)));
			this.ThemeListGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("e1d1b9e4-3dae-46cd-bf75-7626f8a72e49", "Theme Name");
			zTextBoxColumnStyleInfo1.ColumnName = "ThemeName";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.ThemeListGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ThemeListGrid.GridId = "c5767d97-d44c-4b36-82c9-2bf2b295583d";
			this.ThemeListGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ThemeListGrid.LayoutKey = "ThemesListGrid";
			this.ThemeListGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.ThemeListGrid.Name = "ThemeListGrid";
			this.ThemeListGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(165, 357, true);
			this.ThemeListGrid.TabIndex = 0;
			// 
			// webThemeChildControl
			// 
			this.webThemeChildControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.webThemeChildControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Registry.Business.WebThemeCustomObject)(((Enterprise.Registry.Business.WebThemeCustomObject)(null)))));
			this.webThemeChildControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.webThemeChildControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.webThemeChildControl.Name = "webThemeChildControl";
			this.webThemeChildControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(344, 382, true);
			this.webThemeChildControl.TabIndex = 0;
			// 
			// DeleteThemeButton
			// 
			this.DeleteThemeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.DeleteThemeButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("86ef5f87-e3dd-449a-ae62-1da21712459b", "&Delete Theme");
			this.DeleteThemeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(429, 3, true);
			this.DeleteThemeButton.Name = "DeleteThemeButton";
			this.DeleteThemeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 23, true);
			this.DeleteThemeButton.TabIndex = 2;
			this.DeleteThemeButton.UseVisualStyleBackColor = true;
			this.DeleteThemeButton.Click += new System.EventHandler(this.DeleteThemeButton_Click);
			// 
			// CreateThemeButton
			// 
			this.CreateThemeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CreateThemeButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("922d9387-d863-4aa4-a717-13f1f0ceed79", "C&reate Theme");
			this.CreateThemeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(252, 3, true);
			this.CreateThemeButton.Name = "CreateThemeButton";
			this.CreateThemeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 23, true);
			this.CreateThemeButton.TabIndex = 0;
			this.CreateThemeButton.UseVisualStyleBackColor = true;
			this.CreateThemeButton.Click += new System.EventHandler(this.CreateThemeButton_Click);
			// 
			// CopyThemeButton
			// 
			this.CopyThemeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CopyThemeButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("aa83e635-72ee-4681-9f5a-a13249f72934", "&Copy Theme");
			this.CopyThemeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(348, 3, true);
			this.CopyThemeButton.Name = "CopyThemeButton";
			this.CopyThemeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CopyThemeButton.TabIndex = 1;
			this.CopyThemeButton.UseVisualStyleBackColor = true;
			this.CopyThemeButton.Click += new System.EventHandler(this.CopyThemeButton_Click);
			// 
			// WebThemeSelectorControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.kSplitContainer1);
			this.Name = "WebThemeSelectorControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(522, 415, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.kSplitContainer1.Panel1.ResumeLayout(false);
			this.kSplitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.kSplitContainer1)).EndInit();
			this.kSplitContainer1.ResumeLayout(false);
			this.kSplitContainer1.PerformLayout();
			this.kSplitContainer2.Panel1.ResumeLayout(false);
			this.kSplitContainer2.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.kSplitContainer2)).EndInit();
			this.kSplitContainer2.ResumeLayout(false);
			this.kSplitContainer2.PerformLayout();
			this.ThemeListGroupBox.ResumeLayout(false);
			this.ThemeListGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ThemeListGrid)).EndInit();
			this.ThemeListGrid.ResumeLayout(false);
			this.ThemeListGrid.PerformLayout();
			this.webThemeChildControl.ResumeLayout(true);
			this.webThemeChildControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer kSplitContainer1;
		private CargoWise.Windows.UI.KSplitContainer kSplitContainer2;
		internal ZArchitecture.GUI.ZButton CopyThemeButton;
		private ZArchitecture.GUI.ZGroupBox ThemeListGroupBox;
		private ZArchitecture.ZGrid ThemeListGrid;
		internal ZArchitecture.GUI.ZButton CreateThemeButton;
		private WebThemeSelectorChildControl webThemeChildControl;
		internal ZArchitecture.GUI.ZButton DeleteThemeButton;
	}
}
