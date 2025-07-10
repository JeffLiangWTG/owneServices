namespace Enterprise.UniversalCopy.GUI
{
	partial class UniversalCopyTemplateForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.ucTemplateUserControl = new Enterprise.UniversalCopy.GUI.UniversalCopyTemplateUserControl();
			this.panel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.textBoxFilter = new Enterprise.ZArchitecture.ZTextBox();
			this.checkBoxGlobal = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.zCheckBox1 = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.checkBoxSystem = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.checkBoxPublished = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.textBoxName = new Enterprise.ZArchitecture.ZTextBox();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ucTemplateUserControl.SuspendLayout();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1056, 606, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.ucTemplateUserControl);
			this.MainTabPage.Controls.Add(this.panel1);
			this.MainTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1048, 579, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1048, 579, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1056, 606, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1056, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.UniversalCopy.Business.UniversalCopyTemplate);
			// 
			// ucTemplateUserControl
			// 
			this.ucTemplateUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ucTemplateUserControl, ".");
			this.ucTemplateUserControl.CopyManager = null;
			this.ucTemplateUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ucTemplateUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 32, true);
			this.ucTemplateUserControl.Name = "ucTemplateUserControl";
			this.ucTemplateUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1042, 544, true);
			this.ucTemplateUserControl.TabIndex = 2;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.textBoxFilter);
			this.panel1.Controls.Add(this.checkBoxGlobal);
			this.panel1.Controls.Add(this.zCheckBox1);
			this.panel1.Controls.Add(this.checkBoxSystem);
			this.panel1.Controls.Add(this.checkBoxPublished);
			this.panel1.Controls.Add(this.textBoxName);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.panel1.Name = "panel1";
			this.panel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1042, 29, true);
			this.panel1.TabIndex = 3;
			// 
			// textBoxFilter
			// 
			this.textBoxFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.textBoxFilter, "CopyTemplateTree.FilterList");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.UniversalCopy.Business.UniversalCopyTemplate)(null)).CopyTemplateTree.FilterList)));
			this.textBoxFilter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(951, 3, true);
			this.textBoxFilter.Name = "textBoxFilter";
			this.textBoxFilter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(89, 20, true);
			this.textBoxFilter.TabIndex = 9;
			// 
			// checkBoxGlobal
			// 
			this.checkBoxGlobal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkBoxGlobal.AutoSize = true;
			this.BindingSource.SetBindingMember(this.checkBoxGlobal, "IsPublishedGlobal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.UniversalCopy.Business.UniversalCopyTemplate)(null)).IsPublishedGlobal)));
			this.checkBoxGlobal.CaptionResourceString = Enterprise.UniversalCopy.GUI.Res.GetData("7cc6ca67-7000-4d82-b993-3d0caddf8ac5", "Published for all companies", "Published for all users across all registered companies.");
			this.checkBoxGlobal.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.checkBoxGlobal.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(701, 5, true);
			this.checkBoxGlobal.Name = "checkBoxGlobal";
			this.checkBoxGlobal.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(154, 17, true);
			this.checkBoxGlobal.TabIndex = 8;
			this.checkBoxGlobal.UseVisualStyleBackColor = true;
			// 
			// zCheckBox1
			// 
			this.zCheckBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.zCheckBox1.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zCheckBox1, "S9_IsPublished");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.UniversalCopy.Business.UniversalCopyTemplate)(null)).S9_IsPublished)));
			this.zCheckBox1.CaptionResourceString = Enterprise.UniversalCopy.GUI.Res.GetData("7e454f42-539e-45f7-8922-19ad2bfd5009", "Published", "Published for all users.");
			this.zCheckBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zCheckBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(576, 5, true);
			this.zCheckBox1.Name = "zCheckBox1";
			this.zCheckBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 17, true);
			this.zCheckBox1.TabIndex = 7;
			this.zCheckBox1.UseVisualStyleBackColor = true;
			// 
			// checkBoxSystem
			// 
			this.checkBoxSystem.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkBoxSystem.AutoSize = true;
			this.BindingSource.SetBindingMember(this.checkBoxSystem, "S9_IsSystem");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.UniversalCopy.Business.UniversalCopyTemplate)(null)).S9_IsSystem)));
			this.checkBoxSystem.CaptionResourceString = Enterprise.UniversalCopy.GUI.Res.GetData("af51a041-f631-48dd-b7d4-23b324e48a55", "System", "System template.");
			this.checkBoxSystem.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.checkBoxSystem.ForeColor = System.Drawing.SystemColors.GrayText;
			this.checkBoxSystem.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(483, 5, true);
			this.checkBoxSystem.Name = "checkBoxSystem";
			this.checkBoxSystem.ReadOnly = true;
			this.checkBoxSystem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(61, 17, true);
			this.checkBoxSystem.TabIndex = 6;
			this.checkBoxSystem.UseVisualStyleBackColor = true;
			// 
			// checkBoxPublished
			// 
			this.checkBoxPublished.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkBoxPublished.AutoSize = true;
			this.BindingSource.SetBindingMember(this.checkBoxPublished, "IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.UniversalCopy.Business.UniversalCopyTemplate)(null)).IsActive)));
			this.checkBoxPublished.CaptionResourceString = Enterprise.UniversalCopy.GUI.Res.GetData("a1cbe3b0-7f27-4d89-8571-b0d01cced4a6", "Active", "Specifies if this template can be used for copying objects.");
			this.checkBoxPublished.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.checkBoxPublished.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(391, 5, true);
			this.checkBoxPublished.Name = "checkBoxPublished";
			this.checkBoxPublished.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 17, true);
			this.checkBoxPublished.TabIndex = 3;
			this.checkBoxPublished.UseVisualStyleBackColor = true;
			// 
			// textBoxName
			// 
			this.textBoxName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.textBoxName, "CopyTemplateTree.ConfigurationName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.UniversalCopy.Business.UniversalCopyTemplate)(null)).CopyTemplateTree.ConfigurationName)));
			this.textBoxName.CaptionResourceString = Enterprise.UniversalCopy.GUI.Res.GetData("6aadf1a3-e32d-40c0-bf3b-bcb1cfb035e4", "Template Name", "Copy template name. Use Backslash \"\\\" to specify path in Copy menu.");
			this.textBoxName.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.textBoxName.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 3, true);
			this.textBoxName.Name = "textBoxName";
			this.textBoxName.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 20, true);
			this.textBoxName.TabIndex = 2;
			// 
			// UniversalCopyTemplateForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.UniversalCopy.GUI.Res.GetData("a67d6b2f-079d-4a68-9fa0-6e50ea7987c7", "Universal Copy Template");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1056, 662, true);
			this.DataSourceType = typeof(Enterprise.UniversalCopy.Business.UniversalCopyTemplate);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 700, true);
			this.Name = "UniversalCopyTemplateForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ucTemplateUserControl.ResumeLayout(true);
			this.ucTemplateUserControl.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private UniversalCopyTemplateUserControl ucTemplateUserControl;
		private Enterprise.ZArchitecture.GUI.ZPanel panel1;
		private ZArchitecture.ZTextBox textBoxName;
		private ZArchitecture.GUI.ZCheckBox checkBoxPublished;
		private ZArchitecture.GUI.ZCheckBox checkBoxSystem;
		private ZArchitecture.GUI.ZCheckBox zCheckBox1;
		private ZArchitecture.GUI.ZCheckBox checkBoxGlobal;
		private ZArchitecture.ZTextBox textBoxFilter;
	}
}