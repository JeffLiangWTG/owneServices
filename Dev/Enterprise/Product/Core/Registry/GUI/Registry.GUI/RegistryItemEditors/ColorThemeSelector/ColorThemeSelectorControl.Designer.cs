using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class ColorThemeSelectorControl : RegistryZUserControl
	{
		ZButton CopyColorsButton;
		ZDropEditWithFixedWidth CopyThemeDropEdit;
		internal ZGroupBox MainGroupBox;
		internal CargoWise.Windows.UI.Layout.RowLayoutPanel RowLayoutPanel;
		internal ZDropEditWithFixedWidth ThemeDropEdit;

		void InitializeComponent()
		{
			this.ThemeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
			this.MainGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CopyColorsButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CopyThemeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
			this.RowLayoutPanel = new CargoWise.Windows.UI.Layout.RowLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.ColorThemeSelector);
			// 
			// ThemeDropEdit
			// 
			this.ThemeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ThemeDropEdit, "ChosenThemeName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Registry.Business.ColorThemeSelector)(null)).ChosenThemeName)));
			this.ThemeDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ThemeDropEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ColorThemeSelectorControl|f357b949-ddf2-4f10-a1d6-a39571e20e66", "Theme", "Color Theme", "Specifies the color theme to use for the application.\r\nYou must restart for this setting to take effect.");
			this.ThemeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 13, true);
			this.ThemeDropEdit.Name = "ThemeDropEdit";
			this.ThemeDropEdit.PreBoundMaxLength = 30;
			this.ThemeDropEdit.ShowDescriptionBox = false;
			this.ThemeDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.ThemeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.ThemeDropEdit.TabIndex = 0;
			// 
			// MainGroupBox
			// 
			this.MainGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.MainGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ColorThemeSelectorControl|035636de-47a8-450d-a57a-62ef67dece9e", "Colors");
			this.MainGroupBox.Controls.Add(this.CopyColorsButton);
			this.MainGroupBox.Controls.Add(this.CopyThemeDropEdit);
			this.MainGroupBox.Controls.Add(this.RowLayoutPanel);
			this.MainGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 39, true);
			this.MainGroupBox.Name = "MainGroupBox";
			this.MainGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(371, 334, true);
			this.MainGroupBox.TabIndex = 1;
			this.MainGroupBox.TabStop = false;
			// 
			// CopyColorsButton
			// 
			this.CopyColorsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.CopyColorsButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ColorThemeSelectorControl|c6f7c89c-4185-40b1-ba2a-d45af6e8061a", "Copy Theme");
			this.CopyColorsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(290, 305, true);
			this.CopyColorsButton.Name = "CopyColorsButton";
			this.CopyColorsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CopyColorsButton.TabIndex = 5;
			this.CopyColorsButton.UseVisualStyleBackColor = true;
			this.CopyColorsButton.Click += new System.EventHandler(this.CopyColorsButton_Click);
			// 
			// CopyThemeDropEdit
			// 
			this.CopyThemeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CopyThemeDropEdit, "ColorThemeToCopy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Registry.Business.ColorThemeSelector)(null)).ColorThemeToCopy)));
			this.CopyThemeDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CopyThemeDropEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ColorThemeSelectorControl|cec9dede-7202-44c5-bbe4-9a21b24866d0", "Copy", "Copy Theme", "Select a theme to copy from. This will let you copy a theme into one of the custom themes available.");
			this.CopyThemeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 307, true);
			this.CopyThemeDropEdit.Name = "CopyThemeDropEdit";
			this.CopyThemeDropEdit.PreBoundMaxLength = 30;
			this.CopyThemeDropEdit.ShowDescriptionBox = false;
			this.CopyThemeDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.CopyThemeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.CopyThemeDropEdit.TabIndex = 4;
			// 
			// RowLayoutPanel
			// 
			this.RowLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.RowLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.RowLayoutPanel.Name = "RowLayoutPanel";
			this.RowLayoutPanel.RowHeight = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(18);
			this.RowLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(365, 285, true);
			this.RowLayoutPanel.TabIndex = 0;
			// 
			// ColorThemeSelectorControl
			// 
			this.BackColor = System.Drawing.Color.Transparent;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainGroupBox);
			this.Controls.Add(this.ThemeDropEdit);
			this.Name = "ColorThemeSelectorControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(382, 376, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		System.ComponentModel.Container components = null;
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
