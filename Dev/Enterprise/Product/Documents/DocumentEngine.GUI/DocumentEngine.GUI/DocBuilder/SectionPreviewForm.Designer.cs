namespace Enterprise.DocumentEngine.GUI.DocBuilder
{
	partial class SectionPreviewForm
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
		new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.infoPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.sectionPreviewTruncatedLabel = new Enterprise.ZArchitecture.ZLabel();
			this.zoomNumericUpDown = new Enterprise.ZArchitecture.GUI.ZNumericUpDown();
			this.categoryTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.sectionNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.viewLabel = new Enterprise.ZArchitecture.ZLabel();
			this.languageDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.customizedViewCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.originalViewCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.closeButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.previewPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.mainSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.customizedSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.customizedRenderPictureBox = new Enterprise.DocumentEngine.GUI.ZScrollablePictureBox();
			this.customizedRenderLabel = new Enterprise.ZArchitecture.ZLabel();
			this.customizedTemplatePictureBox = new Enterprise.DocumentEngine.GUI.ZScrollablePictureBox();
			this.customizedTemplateLabel = new Enterprise.ZArchitecture.ZLabel();
			this.originalSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.originalRenderPictureBox = new Enterprise.DocumentEngine.GUI.ZScrollablePictureBox();
			this.originalRenderLabel = new Enterprise.ZArchitecture.ZLabel();
			this.originalTemplatePictureBox = new Enterprise.DocumentEngine.GUI.ZScrollablePictureBox();
			this.originalTemplateLabel = new Enterprise.ZArchitecture.ZLabel();
			this.controller = new Enterprise.DocumentEngine.DocBuilder.SectionPreviewController(this.components);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.infoPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.zoomNumericUpDown)).BeginInit();
			this.previewPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).BeginInit();
			this.mainSplitContainer.Panel1.SuspendLayout();
			this.mainSplitContainer.Panel2.SuspendLayout();
			this.mainSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.customizedSplitContainer)).BeginInit();
			this.customizedSplitContainer.Panel1.SuspendLayout();
			this.customizedSplitContainer.Panel2.SuspendLayout();
			this.customizedSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.originalSplitContainer)).BeginInit();
			this.originalSplitContainer.Panel1.SuspendLayout();
			this.originalSplitContainer.Panel2.SuspendLayout();
			this.originalSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 538, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(844, 24, true);
			this.MainStatusBar.TabIndex = 2;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentEngine.DocBuilder.SectionPreviewManager);
			// 
			// infoPanel
			// 
			this.infoPanel.Controls.Add(this.sectionPreviewTruncatedLabel);
			this.infoPanel.Controls.Add(this.zoomNumericUpDown);
			this.infoPanel.Controls.Add(this.categoryTextBox);
			this.infoPanel.Controls.Add(this.sectionNameTextBox);
			this.infoPanel.Controls.Add(this.viewLabel);
			this.infoPanel.Controls.Add(this.languageDropEdit);
			this.infoPanel.Controls.Add(this.customizedViewCheckBox);
			this.infoPanel.Controls.Add(this.originalViewCheckBox);
			this.infoPanel.Controls.Add(this.closeButton);
			this.infoPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.infoPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.infoPanel.Name = "infoPanel";
			this.infoPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(844, 94, true);
			this.infoPanel.TabIndex = 0;
			// 
			// sectionPreviewTruncatedLabel
			// 
			this.sectionPreviewTruncatedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 61, true);
			this.sectionPreviewTruncatedLabel.Name = "sectionPreviewTruncatedLabel";
			this.sectionPreviewTruncatedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 23, true);
			this.sectionPreviewTruncatedLabel.TabIndex = 8;
			// 
			// zoomNumericUpDown
			// 
			this.BindingSource.SetBindingMember(this.zoomNumericUpDown, "Zoom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((decimal)(((Enterprise.DocumentEngine.DocBuilder.SectionPreviewManager)(null)).Zoom)));
			this.zoomNumericUpDown.BindTo = "Zoom";
			this.zoomNumericUpDown.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("SectionPreviewForm|1fadcabc-0389-42ed-9fae-f223094f3dff", "Zoom");
			this.zoomNumericUpDown.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
			this.zoomNumericUpDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(504, 38, true);
			this.zoomNumericUpDown.Maximum = new decimal(new int[] {
            400,
            0,
            0,
            0});
			this.zoomNumericUpDown.Minimum = new decimal(new int[] {
            20,
            0,
            0,
            0});
			this.zoomNumericUpDown.Name = "zoomNumericUpDown";
			this.zoomNumericUpDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.zoomNumericUpDown.TabIndex = 3;
			this.zoomNumericUpDown.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
			// 
			// categoryTextBox
			// 
			this.BindingSource.SetBindingMember(this.categoryTextBox, "Category");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.DocBuilder.SectionPreviewManager)(null)).Category)));
			this.categoryTextBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("SectionPreviewForm|54ca8a14-c44b-4b87-9e3b-e9157d2d95ba", "Category");
			this.categoryTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.categoryTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 38, true);
			this.categoryTextBox.Name = "categoryTextBox";
			this.categoryTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(138, 20, true);
			this.categoryTextBox.TabIndex = 1;
			// 
			// sectionNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.sectionNameTextBox, "SectionName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.DocBuilder.SectionPreviewManager)(null)).SectionName)));
			this.sectionNameTextBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("SectionPreviewForm|473caf27-e6ce-40b4-ba21-aa76577fb440", "Section Name");
			this.sectionNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.sectionNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 12, true);
			this.sectionNameTextBox.Name = "sectionNameTextBox";
			this.sectionNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(458, 20, true);
			this.sectionNameTextBox.TabIndex = 0;
			// 
			// viewLabel
			// 
			this.viewLabel.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("SectionPreviewForm|222949ad-b3d4-4db7-915a-f257ce22506f", "View:");
			this.viewLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(560, 10, true);
			this.viewLabel.Name = "viewLabel";
			this.viewLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(58, 23, true);
			this.viewLabel.TabIndex = 4;
			this.viewLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// languageDropEdit
			// 
			this.languageDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.languageDropEdit, "Language");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.DocumentEngine.DocBuilder.SectionPreviewManager)(null)).Language)));
			this.languageDropEdit.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("SectionPreviewForm|66cdc330-3602-46e4-85cf-e1faf3079e17", "Language");
			this.languageDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(311, 38, true);
			this.languageDropEdit.Name = "languageDropEdit";
			this.languageDropEdit.PreBoundMaxLength = 3;
			this.languageDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.languageDropEdit.TabIndex = 2;
			// 
			// customizedViewCheckBox
			// 
			this.customizedViewCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.customizedViewCheckBox, "IsCustomizedViewVisible");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentEngine.DocBuilder.SectionPreviewManager)(null)).IsCustomizedViewVisible)));
			this.customizedViewCheckBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("SectionPreviewForm|03e1a30a-b86b-43d2-a9ea-991be8d2201b", "Customized");
			this.customizedViewCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.customizedViewCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(624, 35, true);
			this.customizedViewCheckBox.Name = "customizedViewCheckBox";
			this.customizedViewCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(81, 17, true);
			this.customizedViewCheckBox.TabIndex = 6;
			this.customizedViewCheckBox.UseVisualStyleBackColor = true;
			this.customizedViewCheckBox.CheckedChanged += new System.EventHandler(this.HandleCheckBoxValueChanged);
			// 
			// originalViewCheckBox
			// 
			this.originalViewCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.originalViewCheckBox, "IsOriginalViewVisible");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentEngine.DocBuilder.SectionPreviewManager)(null)).IsOriginalViewVisible)));
			this.originalViewCheckBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("SectionPreviewForm|e4328602-0b91-4b83-968d-b4a5a82af60c", "System");
			this.originalViewCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.originalViewCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(624, 15, true);
			this.originalViewCheckBox.Name = "originalViewCheckBox";
			this.originalViewCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(61, 17, true);
			this.originalViewCheckBox.TabIndex = 5;
			this.originalViewCheckBox.UseVisualStyleBackColor = true;
			this.originalViewCheckBox.CheckedChanged += new System.EventHandler(this.HandleCheckBoxValueChanged);
			// 
			// closeButton
			// 
			this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.closeButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("SectionPreviewForm|7b2e697a-fd52-4443-b87b-db5e3ce72e71", "Close");
			this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.closeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(757, 12, true);
			this.closeButton.Name = "closeButton";
			this.closeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.closeButton.TabIndex = 7;
			this.closeButton.UseVisualStyleBackColor = true;
			// 
			// previewPanel
			// 
			this.previewPanel.BackColor = System.Drawing.SystemColors.AppWorkspace;
			this.previewPanel.Controls.Add(this.mainSplitContainer);
			this.previewPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.previewPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 94, true);
			this.previewPanel.Name = "previewPanel";
			this.previewPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 6, 0, 0, true);
			this.previewPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(844, 444, true);
			this.previewPanel.TabIndex = 1;
			// 
			// mainSplitContainer
			// 
			this.mainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 6, true);
			this.mainSplitContainer.Name = "mainSplitContainer";
			this.mainSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// mainSplitContainer.Panel1
			// 
			this.mainSplitContainer.Panel1.Controls.Add(this.customizedSplitContainer);
			// 
			// mainSplitContainer.Panel2
			// 
			this.mainSplitContainer.Panel2.Controls.Add(this.originalSplitContainer);
			this.mainSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(844, 438, true);
			this.mainSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(209);
			this.mainSplitContainer.SplitterWidth = 20;
			this.mainSplitContainer.TabIndex = 0;
			// 
			// customizedSplitContainer
			// 
			this.customizedSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.customizedSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.customizedSplitContainer.Name = "customizedSplitContainer";
			this.customizedSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// customizedSplitContainer.Panel1
			// 
			this.customizedSplitContainer.Panel1.BackColor = System.Drawing.Color.White;
			this.customizedSplitContainer.Panel1.Controls.Add(this.customizedRenderPictureBox);
			this.customizedSplitContainer.Panel1.Controls.Add(this.customizedRenderLabel);
			// 
			// customizedSplitContainer.Panel2
			// 
			this.customizedSplitContainer.Panel2.BackColor = System.Drawing.Color.White;
			this.customizedSplitContainer.Panel2.Controls.Add(this.customizedTemplatePictureBox);
			this.customizedSplitContainer.Panel2.Controls.Add(this.customizedTemplateLabel);
			this.customizedSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(844, 209, true);
			this.customizedSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(101);
			this.customizedSplitContainer.SplitterWidth = 6;
			this.customizedSplitContainer.TabIndex = 0;
			// 
			// customizedRenderPictureBox
			// 
			this.customizedRenderPictureBox.AllowDrop = true;
			this.customizedRenderPictureBox.AutoScroll = true;
			this.customizedRenderPictureBox.BackColor = System.Drawing.Color.Transparent;
			this.customizedRenderPictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.customizedRenderPictureBox.Image = null;
			this.customizedRenderPictureBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 23, true);
			this.customizedRenderPictureBox.Name = "customizedRenderPictureBox";
			this.customizedRenderPictureBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(844, 78, true);
			this.customizedRenderPictureBox.TabIndex = 1;
			this.customizedRenderPictureBox.Zoom = 100;
			// 
			// customizedRenderLabel
			// 
			this.customizedRenderLabel.BackColor = System.Drawing.SystemColors.Control;
			this.customizedRenderLabel.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("SectionPreviewForm|0f7192c1-9d98-49b3-a8a3-fb8944746606", "Customized Preview Using Data from Current Job");
			this.customizedRenderLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.customizedRenderLabel.IsFontBold = true;
			this.customizedRenderLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.customizedRenderLabel.Name = "customizedRenderLabel";
			this.customizedRenderLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(844, 23, true);
			this.customizedRenderLabel.TabIndex = 0;
			// 
			// customizedTemplatePictureBox
			// 
			this.customizedTemplatePictureBox.AllowDrop = true;
			this.customizedTemplatePictureBox.AutoScroll = true;
			this.customizedTemplatePictureBox.BackColor = System.Drawing.Color.Transparent;
			this.customizedTemplatePictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.customizedTemplatePictureBox.Image = null;
			this.customizedTemplatePictureBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 23, true);
			this.customizedTemplatePictureBox.Name = "customizedTemplatePictureBox";
			this.customizedTemplatePictureBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(844, 79, true);
			this.customizedTemplatePictureBox.TabIndex = 1;
			this.customizedTemplatePictureBox.Zoom = 100;
			// 
			// customizedTemplateLabel
			// 
			this.customizedTemplateLabel.BackColor = System.Drawing.SystemColors.Control;
			this.customizedTemplateLabel.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("SectionPreviewForm|a7a99d4e-ee85-4826-a766-29edc20f9598", "Customized Section Source Showing Macros and Section Tags");
			this.customizedTemplateLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.customizedTemplateLabel.IsFontBold = true;
			this.customizedTemplateLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.customizedTemplateLabel.Name = "customizedTemplateLabel";
			this.customizedTemplateLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(844, 23, true);
			this.customizedTemplateLabel.TabIndex = 0;
			// 
			// originalSplitContainer
			// 
			this.originalSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.originalSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.originalSplitContainer.Name = "originalSplitContainer";
			this.originalSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// originalSplitContainer.Panel1
			// 
			this.originalSplitContainer.Panel1.BackColor = System.Drawing.Color.White;
			this.originalSplitContainer.Panel1.Controls.Add(this.originalRenderPictureBox);
			this.originalSplitContainer.Panel1.Controls.Add(this.originalRenderLabel);
			// 
			// originalSplitContainer.Panel2
			// 
			this.originalSplitContainer.Panel2.BackColor = System.Drawing.Color.White;
			this.originalSplitContainer.Panel2.Controls.Add(this.originalTemplatePictureBox);
			this.originalSplitContainer.Panel2.Controls.Add(this.originalTemplateLabel);
			this.originalSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(844, 209, true);
			this.originalSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.originalSplitContainer.SplitterWidth = 6;
			this.originalSplitContainer.TabIndex = 0;
			// 
			// originalRenderPictureBox
			// 
			this.originalRenderPictureBox.AllowDrop = true;
			this.originalRenderPictureBox.AutoScroll = true;
			this.originalRenderPictureBox.BackColor = System.Drawing.Color.Transparent;
			this.originalRenderPictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.originalRenderPictureBox.Image = null;
			this.originalRenderPictureBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 23, true);
			this.originalRenderPictureBox.Name = "originalRenderPictureBox";
			this.originalRenderPictureBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(844, 77, true);
			this.originalRenderPictureBox.TabIndex = 1;
			this.originalRenderPictureBox.Zoom = 100;
			// 
			// originalRenderLabel
			// 
			this.originalRenderLabel.BackColor = System.Drawing.SystemColors.Control;
			this.originalRenderLabel.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("SectionPreviewForm|70850ae5-0e98-44b8-be70-342893df6779", "Preview Using Data from Current Job");
			this.originalRenderLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.originalRenderLabel.IsFontBold = true;
			this.originalRenderLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.originalRenderLabel.Name = "originalRenderLabel";
			this.originalRenderLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(844, 23, true);
			this.originalRenderLabel.TabIndex = 0;
			// 
			// originalTemplatePictureBox
			// 
			this.originalTemplatePictureBox.AllowDrop = true;
			this.originalTemplatePictureBox.AutoScroll = true;
			this.originalTemplatePictureBox.BackColor = System.Drawing.Color.Transparent;
			this.originalTemplatePictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.originalTemplatePictureBox.Image = null;
			this.originalTemplatePictureBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 23, true);
			this.originalTemplatePictureBox.Name = "originalTemplatePictureBox";
			this.originalTemplatePictureBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(844, 80, true);
			this.originalTemplatePictureBox.TabIndex = 1;
			this.originalTemplatePictureBox.Zoom = 100;
			// 
			// originalTemplateLabel
			// 
			this.originalTemplateLabel.BackColor = System.Drawing.SystemColors.Control;
			this.originalTemplateLabel.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("SectionPreviewForm|25cb16a8-c1a5-49c0-8529-8be171018792", "Section Source Showing Macros and Section Tags");
			this.originalTemplateLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.originalTemplateLabel.IsFontBold = true;
			this.originalTemplateLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.originalTemplateLabel.Name = "originalTemplateLabel";
			this.originalTemplateLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(844, 23, true);
			this.originalTemplateLabel.TabIndex = 0;
			// 
			// SectionPreviewForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.closeButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("SectionPreviewForm|8d960977-61bc-4f44-8a82-9ad706025791", "Preview Section");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(844, 562, true);
			this.Controls.Add(this.previewPanel);
			this.Controls.Add(this.infoPanel);
			this.DataSourceType = typeof(Enterprise.DocumentEngine.DocBuilder.SectionPreviewManager);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(860, 600, true);
			this.Name = "SectionPreviewForm";
			this.Shown += new System.EventHandler(this.SectionPreviewForm_Shown);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.infoPanel, 0);
			this.Controls.SetChildIndex(this.previewPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.infoPanel.ResumeLayout(false);
			this.infoPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.zoomNumericUpDown)).EndInit();
			this.previewPanel.ResumeLayout(false);
			this.mainSplitContainer.Panel1.ResumeLayout(false);
			this.mainSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).EndInit();
			this.mainSplitContainer.ResumeLayout(false);
			this.customizedSplitContainer.Panel1.ResumeLayout(false);
			this.customizedSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.customizedSplitContainer)).EndInit();
			this.customizedSplitContainer.ResumeLayout(false);
			this.originalSplitContainer.Panel1.ResumeLayout(false);
			this.originalSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.originalSplitContainer)).EndInit();
			this.originalSplitContainer.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.GUI.ZPanel infoPanel;
		private ZArchitecture.GUI.ZPanel previewPanel;
		private CargoWise.Windows.UI.KSplitContainer mainSplitContainer;
		private CargoWise.Windows.UI.KSplitContainer customizedSplitContainer;
		private CargoWise.Windows.UI.KSplitContainer originalSplitContainer;
		private ZArchitecture.GUI.ZButton closeButton;
		private ZArchitecture.ZLabel customizedRenderLabel;
		private ZArchitecture.ZLabel customizedTemplateLabel;
		private ZArchitecture.ZLabel originalRenderLabel;
		private ZArchitecture.ZLabel originalTemplateLabel;
		private ZArchitecture.GUI.ZCheckBox customizedViewCheckBox;
		private ZArchitecture.GUI.ZCheckBox originalViewCheckBox;
		private ZArchitecture.GUI.ZDropEdit languageDropEdit;
		private ZArchitecture.ZLabel viewLabel;
		private ZArchitecture.ZTextBox sectionNameTextBox;
		private ZArchitecture.ZTextBox categoryTextBox;
		private DocumentEngine.DocBuilder.SectionPreviewController controller;
		private ZArchitecture.GUI.ZNumericUpDown zoomNumericUpDown;
		private ZScrollablePictureBox originalTemplatePictureBox;
		private ZScrollablePictureBox customizedRenderPictureBox;
		private ZScrollablePictureBox customizedTemplatePictureBox;
		private ZScrollablePictureBox originalRenderPictureBox;
		private ZArchitecture.ZLabel sectionPreviewTruncatedLabel;
	}
}
