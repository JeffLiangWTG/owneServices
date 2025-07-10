namespace Enterprise.ZArchitecture.GUI.DataMapping
{
	partial class SettingsControl
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
			this.SettingsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FormattingPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ExportButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ImportButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SettingsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RemoveSettingButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AddSettingButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SettingsGroupBox.SuspendLayout();
			this.FormattingPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ZArchitecture.DataMapping.ImportExportWizard);
			// 
			// SettingsGroupBox
			// 
			this.SettingsGroupBox.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("SettingsControl|22a1e724-c60b-41a7-a5b8-97c8b4fae18a", "Settings");
			this.SettingsGroupBox.Controls.Add(this.FormattingPanel);
			this.SettingsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SettingsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SettingsGroupBox.Name = "SettingsGroupBox";
			this.SettingsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 49, true);
			this.SettingsGroupBox.TabIndex = 1;
			this.SettingsGroupBox.TabStop = false;
			// 
			// FormattingPanel
			// 
			this.FormattingPanel.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.FormattingPanel.Controls.Add(this.SettingsDropEdit);
			this.FormattingPanel.Controls.Add(this.AddSettingButton);
			this.FormattingPanel.Controls.Add(this.RemoveSettingButton);
			this.FormattingPanel.Controls.Add(this.ImportButton);
			this.FormattingPanel.Controls.Add(this.ExportButton);
			this.FormattingPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 17, true);
			this.FormattingPanel.Name = "FormattingPanel";
			this.FormattingPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(750, 23, true);
			this.FormattingPanel.TabIndex = 5;
			// 
			// ExportButton
			// 
			this.ExportButton.AutoSize = true;
			this.ExportButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("SettingsControl|a257705f-562a-4e3b-8f06-6bead09ff540", "Export...");
			this.ExportButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.ExportButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(470, 0, true);
			this.ExportButton.Name = "ExportButton";
			this.ExportButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(61, 23, true);
			this.ExportButton.TabIndex = 4;
			this.ExportButton.UseVisualStyleBackColor = true;
			this.ExportButton.Click += new System.EventHandler(this.ExportButton_Click);
			// 
			// ImportButton
			// 
			this.ImportButton.AutoSize = true;
			this.ImportButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("SettingsControl|18e55dee-ea2a-4c5a-b390-80675140fa1d", "Import...");
			this.ImportButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.ImportButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(531, 0, true);
			this.ImportButton.Name = "ImportButton";
			this.ImportButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 23, true);
			this.ImportButton.TabIndex = 3;
			this.ImportButton.UseVisualStyleBackColor = true;
			this.ImportButton.Click += new System.EventHandler(this.ImportButton_Click);
			// 
			// SettingsDropEdit
			// 
			this.BindingSource.SetBindingMember(this.SettingsDropEdit, "Setting");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ZArchitecture.DataMapping.ImportExportWizard)(null)).Setting)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ZArchitecture.DataMapping.ImportExportWizard)(null)).Settings)));
			this.SettingsDropEdit.BindToList = "Settings";
			this.SettingsDropEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.SettingsDropEdit, false);
			this.SettingsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SettingsDropEdit.Name = "SettingsDropEdit";
			this.SettingsDropEdit.PreBoundMaxLength = 50;
			this.SettingsDropEdit.ShowDescriptionBox = false;
			this.SettingsDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.SettingsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 17, true);
			this.SettingsDropEdit.TabIndex = 0;
			// 
			// RemoveSettingButton
			// 
			this.RemoveSettingButton.AutoSize = true;
			this.RemoveSettingButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("SettingsControl|5bb32243-7889-4967-bec8-da843993ac95", "Remove");
			this.RemoveSettingButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.RemoveSettingButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(408, 0, true);
			this.RemoveSettingButton.Name = "RemoveSettingButton";
			this.RemoveSettingButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 23, true);
			this.RemoveSettingButton.TabIndex = 2;
			this.RemoveSettingButton.UseVisualStyleBackColor = true;
			this.RemoveSettingButton.Click += new System.EventHandler(this.RemoveSettingButton_Click);
			// 
			// AddSettingButton
			// 
			this.AddSettingButton.AutoSize = true;
			this.AddSettingButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("SettingsControl|0da30ea8-2d83-497f-bdf1-8508dbae6503", "New");
			this.AddSettingButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.AddSettingButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(372, 0, true);
			this.AddSettingButton.Name = "AddSettingButton";
			this.AddSettingButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(37, 23, true);
			this.AddSettingButton.TabIndex = 1;
			this.AddSettingButton.UseVisualStyleBackColor = true;
			this.AddSettingButton.Click += new System.EventHandler(this.AddSettingButton_Click);
			// 
			// SettingsControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SettingsGroupBox);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 49, true);
			this.Name = "SettingsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 49, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SettingsGroupBox.ResumeLayout(false);
			this.FormattingPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private ZGroupBox SettingsGroupBox;
		private ZButton ExportButton;
		private ZButton ImportButton;
		private ZDropEdit SettingsDropEdit;
		private ZButton RemoveSettingButton;
		private ZButton AddSettingButton;
		private ZPanel FormattingPanel;
	}
}
