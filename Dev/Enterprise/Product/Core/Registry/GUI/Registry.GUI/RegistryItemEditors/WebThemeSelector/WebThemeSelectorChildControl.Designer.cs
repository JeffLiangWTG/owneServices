namespace Enterprise.Registry.GUI
{
	partial class WebThemeSelectorChildControl
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
			this.kSplitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ExportCSSButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ImportCSSButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CssTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zGroupBox2 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PreviewLabel = new CargoWise.Windows.UI.KLabel();
			this.ExportButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DeleteButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ImportButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PreviewPictureBox = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			this.ImageList = new CargoWise.Windows.UI.KListView();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.kSplitContainer1)).BeginInit();
			this.kSplitContainer1.Panel1.SuspendLayout();
			this.kSplitContainer1.Panel2.SuspendLayout();
			this.kSplitContainer1.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			this.zGroupBox2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PreviewPictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.WebThemeCustomObject);
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
			this.kSplitContainer1.Panel1.Controls.Add(this.zGroupBox1);
			// 
			// kSplitContainer1.Panel2
			// 
			this.kSplitContainer1.Panel2.Controls.Add(this.zGroupBox2);
			this.kSplitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(344, 382, true);
			this.kSplitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			this.kSplitContainer1.TabIndex = 0;
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("5ac18c0f-6c4f-4cdb-af39-9e1327536c1c", "CSS");
			this.zGroupBox1.Controls.Add(this.ExportCSSButton);
			this.zGroupBox1.Controls.Add(this.ImportCSSButton);
			this.zGroupBox1.Controls.Add(this.CssTextBox);
			this.zGroupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(344, 180, true);
			this.zGroupBox1.TabIndex = 0;
			this.zGroupBox1.TabStop = false;
			// 
			// ExportCSSButton
			// 
			this.ExportCSSButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ExportCSSButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("5c5369c5-62a7-4dd1-bdcb-2c0dba94b751", "Export");
			this.ExportCSSButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 150, true);
			this.ExportCSSButton.Name = "ExportCSSButton";
			this.ExportCSSButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ExportCSSButton.TabIndex = 2;
			this.ExportCSSButton.UseVisualStyleBackColor = true;
			this.ExportCSSButton.Click += new System.EventHandler(this.ExportCSSButton_Click);
			// 
			// ImportCSSButton
			// 
			this.ImportCSSButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ImportCSSButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("bc0bf1e1-d656-4efa-9ee4-80a7740728eb", "Import");
			this.ImportCSSButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 150, true);
			this.ImportCSSButton.Name = "ImportCSSButton";
			this.ImportCSSButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 23, true);
			this.ImportCSSButton.TabIndex = 1;
			this.ImportCSSButton.UseVisualStyleBackColor = true;
			this.ImportCSSButton.Click += new System.EventHandler(this.ImportCSSButton_Click);
			// 
			// CssTextBox
			// 
			this.CssTextBox.AcceptsReturn = true;
			this.CssTextBox.AcceptsTab = true;
			this.CssTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CssTextBox, "CSS");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.WebThemeCustomObject)(null)).CSS)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CssTextBox, false);
			this.CssTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CssTextBox.Font = new System.Drawing.Font("Courier New", 8.25F);
			this.CssTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.CssTextBox.Multiline = true;
			this.CssTextBox.Name = "CssTextBox";
			this.CssTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.CssTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(332, 125, true);
			this.CssTextBox.TabIndex = 0;
			// 
			// zGroupBox2
			// 
			this.zGroupBox2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("e284e2dd-c65c-4f65-8dbd-578db331797e", "Images");
			this.zGroupBox2.Controls.Add(this.PreviewLabel);
			this.zGroupBox2.Controls.Add(this.ExportButton);
			this.zGroupBox2.Controls.Add(this.DeleteButton);
			this.zGroupBox2.Controls.Add(this.ImportButton);
			this.zGroupBox2.Controls.Add(this.PreviewPictureBox);
			this.zGroupBox2.Controls.Add(this.ImageList);
			this.zGroupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGroupBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGroupBox2.Name = "zGroupBox2";
			this.zGroupBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(344, 198, true);
			this.zGroupBox2.TabIndex = 0;
			this.zGroupBox2.TabStop = false;
			// 
			// PreviewLabel
			// 
			this.PreviewLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.PreviewLabel.AutoSize = true;
			this.PreviewLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(246, 177, true);
			this.PreviewLabel.Name = "PreviewLabel";
			this.PreviewLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 13, true);
			this.PreviewLabel.TabIndex = 4;
			this.PreviewLabel.Text = "Label";
			// 
			// ExportButton
			// 
			this.ExportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ExportButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("a7895de3-9a66-458e-86a9-db88452ced11", "&Export");
			this.ExportButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 172, true);
			this.ExportButton.Name = "ExportButton";
			this.ExportButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ExportButton.TabIndex = 2;
			this.ExportButton.UseVisualStyleBackColor = true;
			this.ExportButton.Click += new System.EventHandler(this.ExportButton_Click);
			// 
			// DeleteButton
			// 
			this.DeleteButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.DeleteButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("d76cae9b-ec49-4330-be1d-3218b75099c2", "&Delete");
			this.DeleteButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 172, true);
			this.DeleteButton.Name = "DeleteButton";
			this.DeleteButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.DeleteButton.TabIndex = 3;
			this.DeleteButton.UseVisualStyleBackColor = true;
			this.DeleteButton.Click += new System.EventHandler(this.DeleteButton_Click);
			// 
			// ImportButton
			// 
			this.ImportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ImportButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("077d242d-7bf4-4876-8d9b-6c05a803f630", "&Import");
			this.ImportButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 172, true);
			this.ImportButton.Name = "ImportButton";
			this.ImportButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ImportButton.TabIndex = 1;
			this.ImportButton.UseVisualStyleBackColor = true;
			this.ImportButton.Click += new System.EventHandler(this.ImportButton_Click);
			// 
			// PreviewPictureBox
			// 
			this.PreviewPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.PreviewPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.PreviewPictureBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 19, true);
			this.PreviewPictureBox.Name = "PreviewPictureBox";
			this.PreviewPictureBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(182, 147, true);
			this.PreviewPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.PreviewPictureBox.TabIndex = 18;
			this.PreviewPictureBox.TabStop = false;
			// 
			// ImageList
			// 
			this.ImageList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)));
			this.ImageList.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.ImageList.Name = "ImageList";
			this.ImageList.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 147, true);
			this.ImageList.TabIndex = 0;
			this.ImageList.UseCompatibleStateImageBehavior = false;
			this.ImageList.View = System.Windows.Forms.View.Details;
			this.ImageList.SelectedIndexChanged += new System.EventHandler(this.RefreshPreviewPictureBox);
			this.ImageList.SizeChanged += new System.EventHandler(this.RefreshPreviewPictureBox);
			// 
			// WebThemeSelectorChildControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.kSplitContainer1);
			this.Name = "WebThemeSelectorChildControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(344, 382, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.kSplitContainer1.Panel1.ResumeLayout(false);
			this.kSplitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.kSplitContainer1)).EndInit();
			this.kSplitContainer1.ResumeLayout(false);
			this.kSplitContainer1.PerformLayout();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.zGroupBox2.ResumeLayout(false);
			this.zGroupBox2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PreviewPictureBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer kSplitContainer1;
		private ZArchitecture.GUI.ZGroupBox zGroupBox1;
		internal ZArchitecture.ZTextBox CssTextBox;
		private ZArchitecture.GUI.ZGroupBox zGroupBox2;
		internal Enterprise.ZArchitecture.GUI.ZPictureBox PreviewPictureBox;
		internal CargoWise.Windows.UI.KListView ImageList;
		internal ZArchitecture.GUI.ZButton ExportButton;
		internal ZArchitecture.GUI.ZButton DeleteButton;
		internal ZArchitecture.GUI.ZButton ImportButton;
		internal ZArchitecture.GUI.ZButton ExportCSSButton;
		internal ZArchitecture.GUI.ZButton ImportCSSButton;
		internal CargoWise.Windows.UI.KLabel PreviewLabel;
	}
}
