namespace Enterprise.Registry.GUI
{
	partial class DirectorySearchControl : RegistryBusinessObjectTemplateZUserControl
	{
		Enterprise.ZArchitecture.ZTextBox DirectoryTextBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox SearchSubdirectoriesCheckBox;

		void InitializeComponent()
		{
			this.DirectoryTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SearchSubdirectoriesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.DirectorySearch);
			// 
			// DirectoryTextBox
			// 
			this.DirectoryTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DirectoryTextBox, "DirectoryPath");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.DirectorySearch)(null)).DirectoryPath)));
			this.DirectoryTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DirectoryTextBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("DirectorySearchControl|7717950f-b39d-451d-9952-9f604b710290", "Directory Path");
			this.DirectoryTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 0, true);
			this.DirectoryTextBox.Name = "DirectoryTextBox";
			this.DirectoryTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 20, true);
			this.DirectoryTextBox.TabIndex = 0;
			// 
			// SearchSubdirectoriesCheckBox
			// 
			this.BindingSource.SetBindingMember(this.SearchSubdirectoriesCheckBox, "SearchSubdirectories");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.DirectorySearch)(null)).SearchSubdirectories)));
			this.SearchSubdirectoriesCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.SearchSubdirectoriesCheckBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("DirectorySearchControl|52579ac5-5f80-4d47-9462-bd084174079b", "Search Subdirectories");
			this.SearchSubdirectoriesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SearchSubdirectoriesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 30, true);
			this.SearchSubdirectoriesCheckBox.Name = "SearchSubdirectoriesCheckBox";
			this.SearchSubdirectoriesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(133, 24, true);
			this.SearchSubdirectoriesCheckBox.TabIndex = 1;
			this.SearchSubdirectoriesCheckBox.Visible = false;
			// 
			// DirectorySearchControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SearchSubdirectoriesCheckBox);
			this.Controls.Add(this.DirectoryTextBox);
			this.Name = "DirectorySearchControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 56, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
