using Enterprise.Registry.GUI.HotSpotPictureBox;

namespace Enterprise.Registry.GUI
{
	partial class DocBuilderThemeRegistryControl
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
			this.ThemeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ThemeItemLabel = new Enterprise.ZArchitecture.ZLabel();
			this.FillLabel = new Enterprise.ZArchitecture.ZLabel();
			this.BorderLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ThemeComboBox = new CargoWise.Windows.UI.KComboBox();
			this.PreviewGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PreviewPictureBox = new Enterprise.Registry.GUI.HotSpotPictureBox.PictureBoxWithHotSpots();
			this.CustomizeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FillButton = new CargoWise.Windows.UI.KButton();
			this.FontColorLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SizeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.FontLabel = new Enterprise.ZArchitecture.ZLabel();
			this.FontSizeNumericUpDown = new CargoWise.Windows.UI.KNumericUpDown();
			this.FontColorButton = new CargoWise.Windows.UI.KButton();
			this.ThemeItemComboBox = new CargoWise.Windows.UI.KComboBox();
			this.BorderButton = new CargoWise.Windows.UI.KButton();
			this.ItalicCheckBox = new CargoWise.Windows.UI.KCheckBox();
			this.BoldCheckBox = new CargoWise.Windows.UI.KCheckBox();
			this.FontFamilyComboBox = new CargoWise.Windows.UI.KComboBox();
			this.SerializeButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CopyButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.NewButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.RemoveButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PreviewGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PreviewPictureBox)).BeginInit();
			this.CustomizeGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.FontSizeNumericUpDown)).BeginInit();
			this.FontSizeNumericUpDown.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentEngineCore.Registry.DocBuilderThemeRegistry);
			// 
			// ThemeLabel
			// 
			this.ThemeLabel.AutoSize = true;
			this.ThemeLabel.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("e9c9bb2c-6b1d-49a5-98fe-0c3b6a1d30b1", "Theme");
			this.ThemeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
			this.ThemeLabel.Name = "ThemeLabel";
			this.ThemeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 13, true);
			this.ThemeLabel.TabIndex = 0;
			// 
			// ThemeItemLabel
			// 
			this.ThemeItemLabel.AutoSize = true;
			this.ThemeItemLabel.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("f48928c5-01bb-44dc-a4a7-a8d258089caf", "Item");
			this.ThemeItemLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 16, true);
			this.ThemeItemLabel.Name = "ThemeItemLabel";
			this.ThemeItemLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(27, 13, true);
			this.ThemeItemLabel.TabIndex = 0;
			// 
			// FillLabel
			// 
			this.FillLabel.AutoSize = true;
			this.FillLabel.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("4b2af2e2-1190-42d6-93ce-e67e3cf65fc9", "Fill");
			this.FillLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(267, 16, true);
			this.FillLabel.Name = "FillLabel";
			this.FillLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(19, 13, true);
			this.FillLabel.TabIndex = 2;
			// 
			// BorderLabel
			// 
			this.BorderLabel.AutoSize = true;
			this.BorderLabel.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ad53a44d-8dcf-41f5-be12-dab0b62ee8da", "Border");
			this.BorderLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(324, 16, true);
			this.BorderLabel.Name = "BorderLabel";
			this.BorderLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(38, 13, true);
			this.BorderLabel.TabIndex = 4;
			// 
			// ThemeComboBox
			// 
			this.ThemeComboBox.DisplayMember = "NameMultilingual";
			this.ThemeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.ThemeComboBox.FormattingEnabled = true;
			this.ThemeComboBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ThemeComboBox.Name = "ThemeComboBox";
			this.ThemeComboBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(237, 21, true);
			this.ThemeComboBox.TabIndex = 1;
			this.ThemeComboBox.ValueMember = "Name";
			// 
			// PreviewGroupBox
			// 
			this.PreviewGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("a5b04a34-9e74-47e1-919d-060bd951e7a6", "Preview");
			this.PreviewGroupBox.Controls.Add(this.PreviewPictureBox);
			this.PreviewGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 65, true);
			this.PreviewGroupBox.Name = "PreviewGroupBox";
			this.PreviewGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(375, 195, true);
			this.PreviewGroupBox.TabIndex = 2;
			this.PreviewGroupBox.TabStop = false;
			// 
			// PreviewPictureBox
			// 
			this.PreviewPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.PreviewPictureBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.PreviewPictureBox.MapBitmap = null;
			this.PreviewPictureBox.Name = "PreviewPictureBox";
			this.PreviewPictureBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(363, 170, true);
			this.PreviewPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.PreviewPictureBox.TabIndex = 0;
			this.PreviewPictureBox.TabStop = false;
			// 
			// CustomizeGroupBox
			// 
			this.CustomizeGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("abd278d8-9b58-492c-9994-00bc7de97bba", "Customize");
			this.CustomizeGroupBox.Controls.Add(this.FillButton);
			this.CustomizeGroupBox.Controls.Add(this.FontColorLabel);
			this.CustomizeGroupBox.Controls.Add(this.SizeLabel);
			this.CustomizeGroupBox.Controls.Add(this.FontLabel);
			this.CustomizeGroupBox.Controls.Add(this.FontSizeNumericUpDown);
			this.CustomizeGroupBox.Controls.Add(this.FontColorButton);
			this.CustomizeGroupBox.Controls.Add(this.ThemeItemComboBox);
			this.CustomizeGroupBox.Controls.Add(this.ThemeItemLabel);
			this.CustomizeGroupBox.Controls.Add(this.FillLabel);
			this.CustomizeGroupBox.Controls.Add(this.BorderLabel);
			this.CustomizeGroupBox.Controls.Add(this.BorderButton);
			this.CustomizeGroupBox.Controls.Add(this.ItalicCheckBox);
			this.CustomizeGroupBox.Controls.Add(this.BoldCheckBox);
			this.CustomizeGroupBox.Controls.Add(this.FontFamilyComboBox);
			this.CustomizeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 261, true);
			this.CustomizeGroupBox.Name = "CustomizeGroupBox";
			this.CustomizeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(375, 107, true);
			this.CustomizeGroupBox.TabIndex = 3;
			this.CustomizeGroupBox.TabStop = false;
			// 
			// FillButton
			// 
			this.FillButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(267, 30, true);
			this.FillButton.Name = "FillButton";
			this.FillButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 24, true);
			this.FillButton.TabIndex = 3;
			this.FillButton.UseVisualStyleBackColor = true;
			this.FillButton.Click += new System.EventHandler(this.Color1Button_Click);
			// 
			// FontColorLabel
			// 
			this.FontColorLabel.AutoSize = true;
			this.FontColorLabel.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("6dbe04aa-542c-4681-8860-c297691d0d07", "Color");
			this.FontColorLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(267, 62, true);
			this.FontColorLabel.Name = "FontColorLabel";
			this.FontColorLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(31, 13, true);
			this.FontColorLabel.TabIndex = 10;
			// 
			// SizeLabel
			// 
			this.SizeLabel.AutoSize = true;
			this.SizeLabel.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("95234f88-1c82-41a2-a383-bc32a234a08b", "Size");
			this.SizeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(212, 62, true);
			this.SizeLabel.Name = "SizeLabel";
			this.SizeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(27, 13, true);
			this.SizeLabel.TabIndex = 8;
			// 
			// FontLabel
			// 
			this.FontLabel.AutoSize = true;
			this.FontLabel.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("0b0909ea-6edf-4467-b0f7-df133fbebe0a", "Font");
			this.FontLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 62, true);
			this.FontLabel.Name = "FontLabel";
			this.FontLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 13, true);
			this.FontLabel.TabIndex = 6;
			// 
			// FontSizeNumericUpDown
			// 
			this.FontSizeNumericUpDown.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FontSizeNumericUpDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(212, 77, true);
			this.FontSizeNumericUpDown.Maximum = new decimal(new int[] {
						28,
						0,
						0,
						0});
			this.FontSizeNumericUpDown.Minimum = new decimal(new int[] {
						6,
						0,
						0,
						0});
			this.FontSizeNumericUpDown.Name = "FontSizeNumericUpDown";
			this.FontSizeNumericUpDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(49, 22, true);
			this.FontSizeNumericUpDown.TabIndex = 9;
			this.FontSizeNumericUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.FontSizeNumericUpDown.Value = new decimal(new int[] {
						6,
						0,
						0,
						0});
			this.FontSizeNumericUpDown.ValueChanged += new System.EventHandler(this.FontSizeNumericUpDown_ValueChanged);
			// 
			// FontColorButton
			// 
			this.FontColorButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(267, 75, true);
			this.FontColorButton.Name = "FontColorButton";
			this.FontColorButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 24, true);
			this.FontColorButton.TabIndex = 11;
			this.FontColorButton.UseVisualStyleBackColor = true;
			this.FontColorButton.Click += new System.EventHandler(this.FontColorButton_Click);
			// 
			// ThemeItemComboBox
			// 
			this.ThemeItemComboBox.DisplayMember = "Description";
			this.ThemeItemComboBox.BackColor = System.Drawing.SystemColors.Window;
			this.ThemeItemComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.ThemeItemComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ThemeItemComboBox.FormattingEnabled = true;
			this.ThemeItemComboBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 32, true);
			this.ThemeItemComboBox.Name = "ThemeItemComboBox";
			this.ThemeItemComboBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 21, true);
			this.ThemeItemComboBox.TabIndex = 1;
			this.ThemeItemComboBox.ValueMember = "Name";// 
																									// BorderButton
																									// 
			this.BorderButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(321, 30, true);
			this.BorderButton.Name = "BorderButton";
			this.BorderButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 24, true);
			this.BorderButton.TabIndex = 5;
			this.BorderButton.UseVisualStyleBackColor = true;
			this.BorderButton.Click += new System.EventHandler(this.Color2Button_Click);
			// 
			// ItalicCheckBox
			// 
			this.ItalicCheckBox.Appearance = System.Windows.Forms.Appearance.Button;
			this.ItalicCheckBox.Font = new System.Drawing.Font("Times New Roman", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ItalicCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(345, 75, true);
			this.ItalicCheckBox.Name = "ItalicCheckBox";
			this.ItalicCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 24, true);
			this.ItalicCheckBox.TabIndex = 13;
			this.ItalicCheckBox.Text = "I";
			this.ItalicCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.ItalicCheckBox.UseVisualStyleBackColor = true;
			this.ItalicCheckBox.CheckedChanged += new System.EventHandler(this.ItalicCheckBox_CheckedChanged);
			// 
			// BoldCheckBox
			// 
			this.BoldCheckBox.Appearance = System.Windows.Forms.Appearance.Button;
			this.BoldCheckBox.Font = new System.Drawing.Font("Times New Roman", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.BoldCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(321, 75, true);
			this.BoldCheckBox.Name = "BoldCheckBox";
			this.BoldCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 24, true);
			this.BoldCheckBox.TabIndex = 12;
			this.BoldCheckBox.Text = "B";
			this.BoldCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.BoldCheckBox.UseVisualStyleBackColor = true;
			this.BoldCheckBox.CheckedChanged += new System.EventHandler(this.BoldCheckBox_CheckedChanged);
			// 
			// FontFamilyComboBox
			// 
			this.FontFamilyComboBox.BackColor = System.Drawing.SystemColors.Window;
			this.FontFamilyComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.FontFamilyComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FontFamilyComboBox.FormattingEnabled = true;
			this.FontFamilyComboBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 78, true);
			this.FontFamilyComboBox.Name = "FontFamilyComboBox";
			this.FontFamilyComboBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 21, true);
			this.FontFamilyComboBox.TabIndex = 7;
			this.FontFamilyComboBox.SelectedIndexChanged += new System.EventHandler(this.FontFamilyComboBox_SelectedIndexChanged);
			// 
			// SerializeButton
			// 
			this.SerializeButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("4e7b1d59-eae5-49e1-b4bb-ec69e06844c2", "Serialize to Clipboard");
			this.SerializeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(247, 16, true);
			this.SerializeButton.Name = "SerializeButton";
			this.SerializeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 46, true);
			this.SerializeButton.TabIndex = 4;
			this.SerializeButton.UseVisualStyleBackColor = true;
			this.SerializeButton.Visible = false;
			// 
			// CopyButton
			// 
			this.CopyButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("b2e5cd32-2020-4bb1-9876-42c41324703d", "Copy");
			this.CopyButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 39, true);
			this.CopyButton.Name = "CopyButton";
			this.CopyButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CopyButton.TabIndex = 5;
			this.CopyButton.UseVisualStyleBackColor = true;
			this.CopyButton.Click += new System.EventHandler(this.CopyButton_Click);
			// 
			// NewButton
			// 
			this.NewButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("50219550-e4b2-431e-b0d4-07cb8770fbe7", "New");
			this.NewButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 39, true);
			this.NewButton.Name = "NewButton";
			this.NewButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.NewButton.TabIndex = 6;
			this.NewButton.UseVisualStyleBackColor = true;
			this.NewButton.Click += new System.EventHandler(this.NewButton_Click);
			// 
			// RemoveButton
			// 
			this.RemoveButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("0f69159c-c92a-4510-a11c-ffc9f0fd3974", "Remove");
			this.RemoveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 39, true);
			this.RemoveButton.Name = "RemoveButton";
			this.RemoveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.RemoveButton.TabIndex = 7;
			this.RemoveButton.UseVisualStyleBackColor = true;
			this.RemoveButton.Click += new System.EventHandler(this.RemoveButton_Click);
			// 
			// DocBuilderThemeRegistryControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.Transparent;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.RemoveButton);
			this.Controls.Add(this.NewButton);
			this.Controls.Add(this.CopyButton);
			this.Controls.Add(this.SerializeButton);
			this.Controls.Add(this.CustomizeGroupBox);
			this.Controls.Add(this.PreviewGroupBox);
			this.Controls.Add(this.ThemeComboBox);
			this.Controls.Add(this.ThemeLabel);
			this.Name = "DocBuilderThemeRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(390, 369, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PreviewGroupBox.ResumeLayout(false);
			this.PreviewGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PreviewPictureBox)).EndInit();
			this.CustomizeGroupBox.ResumeLayout(false);
			this.CustomizeGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.FontSizeNumericUpDown)).EndInit();
			this.FontSizeNumericUpDown.ResumeLayout(false);
			this.FontSizeNumericUpDown.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZLabel ThemeLabel;
		private Enterprise.ZArchitecture.ZLabel ThemeItemLabel;
		private Enterprise.ZArchitecture.ZLabel FillLabel;
		private Enterprise.ZArchitecture.ZLabel BorderLabel;
		private Enterprise.ZArchitecture.GUI.ZGroupBox CustomizeGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox PreviewGroupBox;
		internal CargoWise.Windows.UI.KButton BorderButton;
		private CargoWise.Windows.UI.KComboBox FontFamilyComboBox;
		internal CargoWise.Windows.UI.KCheckBox BoldCheckBox;
		internal CargoWise.Windows.UI.KCheckBox ItalicCheckBox;
		internal CargoWise.Windows.UI.KButton FontColorButton;
		internal CargoWise.Windows.UI.KComboBox ThemeItemComboBox;
		private Enterprise.ZArchitecture.ZLabel FontLabel;
		private CargoWise.Windows.UI.KNumericUpDown FontSizeNumericUpDown;
		private Enterprise.ZArchitecture.ZLabel FontColorLabel;
		private Enterprise.ZArchitecture.ZLabel SizeLabel;
		internal CargoWise.Windows.UI.KComboBox ThemeComboBox;
		internal CargoWise.Windows.UI.KButton FillButton;
		internal PictureBoxWithHotSpots PreviewPictureBox;
		internal Enterprise.ZArchitecture.GUI.ZButton SerializeButton;
		internal Enterprise.ZArchitecture.GUI.ZButton CopyButton;
		internal Enterprise.ZArchitecture.GUI.ZButton NewButton;
		internal Enterprise.ZArchitecture.GUI.ZButton RemoveButton;
	}
}
