namespace Enterprise.Client.EDI.Registry.GUI
{
	partial class IncidentEmailTemplatePairControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.HelpPictureBox = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			this.splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.templatesSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.legacyAndERequestV1TemplateGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.legacyAndERequestV1EmailSubjectTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.legacyAndERequestV1EmailBodyTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.eRequestV2TemplateGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.eRequestV2EmailSubjectTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.eRequestV2EmailBodyTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DocumentFieldsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zGroupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.HelpPictureBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.templatesSplitContainer)).BeginInit();
			this.templatesSplitContainer.Panel1.SuspendLayout();
			this.templatesSplitContainer.Panel2.SuspendLayout();
			this.templatesSplitContainer.SuspendLayout();
			this.legacyAndERequestV1TemplateGroupBox.SuspendLayout();
			this.eRequestV2TemplateGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DocumentFieldsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Registry.Business.IncidentEmailTemplatePair);
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.CaptionResourceString = ZClientEDI.Res.GetData("77637fdf-2fb0-488d-b545-2c1c6d7d8dae", "Email Content");
			this.zGroupBox1.Controls.Add(this.HelpPictureBox);
			this.zGroupBox1.Controls.Add(this.splitContainer1);
			this.zGroupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(791, 404, true);
			this.zGroupBox1.TabIndex = 9;
			this.zGroupBox1.TabStop = false;
			// 
			// HelpPictureBox
			// 
			this.HelpPictureBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 0, true);
			this.HelpPictureBox.Name = "HelpPictureBox";
			this.HelpPictureBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 15, true);
			this.HelpPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.HelpPictureBox.TabIndex = 10;
			this.HelpPictureBox.TabStop = false;
			this.HelpPictureBox.Click += new System.EventHandler(this.HelpPictureBox_Click);
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.templatesSplitContainer);
			this.splitContainer1.Panel1MinSize = 200;
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.DocumentFieldsGrid);
			this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 385, true);
			this.splitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(218);
			this.splitContainer1.TabIndex = 0;
			// 
			// templatesSplitContainer
			// 
			this.templatesSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.templatesSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.templatesSplitContainer.Name = "templatesSplitContainer";
			// 
			// templatesSplitContainer.Panel1
			// 
			this.templatesSplitContainer.Panel1.Controls.Add(this.legacyAndERequestV1TemplateGroupBox);
			this.templatesSplitContainer.Panel1.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			// 
			// templatesSplitContainer.Panel2
			// 
			this.templatesSplitContainer.Panel2.Controls.Add(this.eRequestV2TemplateGroupBox);
			this.templatesSplitContainer.Panel2.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.templatesSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 218, true);
			this.templatesSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(390);
			this.templatesSplitContainer.TabIndex = 2;
			// 
			// legacyAndERequestV1TemplateGroupBox
			// 
			this.legacyAndERequestV1TemplateGroupBox.CaptionResourceString = ZClientEDI.Res.GetData("135c2879-4cc8-492a-99ba-19244edd519f", "eConversation not supported");
			this.legacyAndERequestV1TemplateGroupBox.Controls.Add(this.legacyAndERequestV1EmailSubjectTextBox);
			this.legacyAndERequestV1TemplateGroupBox.Controls.Add(this.legacyAndERequestV1EmailBodyTextBox);
			this.legacyAndERequestV1TemplateGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.legacyAndERequestV1TemplateGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.legacyAndERequestV1TemplateGroupBox.Name = "legacyAndERequestV1TemplateGroupBox";
			this.legacyAndERequestV1TemplateGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(380, 208, true);
			this.legacyAndERequestV1TemplateGroupBox.TabIndex = 2;
			this.legacyAndERequestV1TemplateGroupBox.TabStop = false;
			// 
			// legacyEmailSubjectTextBox
			// 
			this.legacyAndERequestV1EmailSubjectTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.legacyAndERequestV1EmailSubjectTextBox, "LegacyAndERequestV1EmailTemplate.EmailSubject");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.IncidentEmailTemplatePair)(null)).LegacyAndERequestV1EmailTemplate.EmailSubject)));
			this.legacyAndERequestV1EmailSubjectTextBox.CaptionResourceString = ZClientEDI.Res.GetData("c2c25aa0-bbe0-4d18-ac01-e3afdabf8c0a", "Subject");
			this.legacyAndERequestV1EmailSubjectTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.legacyAndERequestV1EmailSubjectTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.legacyAndERequestV1EmailSubjectTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 33, true);
			this.legacyAndERequestV1EmailSubjectTextBox.Name = "legacyEmailSubjectTextBox";
			this.legacyAndERequestV1EmailSubjectTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 20, true);
			this.legacyAndERequestV1EmailSubjectTextBox.TabIndex = 0;
			this.legacyAndERequestV1EmailSubjectTextBox.Leave += new System.EventHandler(this.EmailTemplateTextBox_Leave);
			// 
			// legacyEmailBodyTextBox
			// 
			this.legacyAndERequestV1EmailBodyTextBox.AcceptsReturn = true;
			this.legacyAndERequestV1EmailBodyTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.legacyAndERequestV1EmailBodyTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.legacyAndERequestV1EmailBodyTextBox, "LegacyAndERequestV1EmailTemplate.EmailBody");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.IncidentEmailTemplatePair)(null)).LegacyAndERequestV1EmailTemplate.EmailBody)));
			this.legacyAndERequestV1EmailBodyTextBox.CaptionResourceString = ZClientEDI.Res.GetData("f247c545-0cfe-44f7-95fe-ea0f22c21b0f", "Body");
			this.legacyAndERequestV1EmailBodyTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.legacyAndERequestV1EmailBodyTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.legacyAndERequestV1EmailBodyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 75, true);
			this.legacyAndERequestV1EmailBodyTextBox.Multiline = true;
			this.legacyAndERequestV1EmailBodyTextBox.Name = "legacyEmailBodyTextBox";
			this.legacyAndERequestV1EmailBodyTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.legacyAndERequestV1EmailBodyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 127, true);
			this.legacyAndERequestV1EmailBodyTextBox.TabIndex = 1;
			this.legacyAndERequestV1EmailBodyTextBox.Leave += new System.EventHandler(this.EmailTemplateTextBox_Leave);
			// 
			// eRequestV2TemplateGroupBox
			// 
			this.eRequestV2TemplateGroupBox.CaptionResourceString = ZClientEDI.Res.GetData("18ebdc33-f224-48b7-9529-e4a74db1160e", "eConversation supported");
			this.eRequestV2TemplateGroupBox.Controls.Add(this.eRequestV2EmailSubjectTextBox);
			this.eRequestV2TemplateGroupBox.Controls.Add(this.eRequestV2EmailBodyTextBox);
			this.eRequestV2TemplateGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.eRequestV2TemplateGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.eRequestV2TemplateGroupBox.Name = "eRequestV2TemplateGroupBox";
			this.eRequestV2TemplateGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(381, 208, true);
			this.eRequestV2TemplateGroupBox.TabIndex = 3;
			this.eRequestV2TemplateGroupBox.TabStop = false;
			// 
			// eRequestEmailSubjectTextBox
			// 
			this.eRequestV2EmailSubjectTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.eRequestV2EmailSubjectTextBox, "ERequestV2EmailTemplate.EmailSubject");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.IncidentEmailTemplatePair)(null)).ERequestV2EmailTemplate.EmailSubject)));
			this.eRequestV2EmailSubjectTextBox.CaptionResourceString = ZClientEDI.Res.GetData("b6704636-8f96-4363-88ad-c5afa1444fdd", "Subject");
			this.eRequestV2EmailSubjectTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.eRequestV2EmailSubjectTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.eRequestV2EmailSubjectTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 33, true);
			this.eRequestV2EmailSubjectTextBox.Name = "eRequestEmailSubjectTextBox";
			this.eRequestV2EmailSubjectTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(369, 20, true);
			this.eRequestV2EmailSubjectTextBox.TabIndex = 0;
			this.eRequestV2EmailSubjectTextBox.Leave += new System.EventHandler(this.EmailTemplateTextBox_Leave);
			// 
			// eRequestEmailBodyTextBox
			// 
			this.eRequestV2EmailBodyTextBox.AcceptsReturn = true;
			this.eRequestV2EmailBodyTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.eRequestV2EmailBodyTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.eRequestV2EmailBodyTextBox, "ERequestV2EmailTemplate.EmailBody");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.IncidentEmailTemplatePair)(null)).ERequestV2EmailTemplate.EmailBody)));
			this.eRequestV2EmailBodyTextBox.CaptionResourceString = ZClientEDI.Res.GetData("f869e9fc-9663-48da-9272-630d27867bd9", "Body");
			this.eRequestV2EmailBodyTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.eRequestV2EmailBodyTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.eRequestV2EmailBodyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 75, true);
			this.eRequestV2EmailBodyTextBox.Multiline = true;
			this.eRequestV2EmailBodyTextBox.Name = "eRequestEmailBodyTextBox";
			this.eRequestV2EmailBodyTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.eRequestV2EmailBodyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(369, 127, true);
			this.eRequestV2EmailBodyTextBox.TabIndex = 1;
			this.eRequestV2EmailBodyTextBox.Leave += new System.EventHandler(this.EmailTemplateTextBox_Leave);
			// 
			// DocumentFieldsGrid
			// 
			this.DocumentFieldsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.DocumentFieldsGrid, "DocumentFields");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.IncidentEmailTemplatePair)(null)).DocumentFields)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Integration.IDocumentField)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.IncidentEmailTemplatePair)(null)).DocumentFields)).SyncRoot)).FieldName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Integration.IDocumentField)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.IncidentEmailTemplatePair)(null)).DocumentFields)).SyncRoot)).FieldDescription)));
			this.DocumentFieldsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = ZClientEDI.Res.GetData("68309a52-617b-4704-a197-38389a64657f", "Field Name");
			zTextBoxColumnStyleInfo1.ColumnName = "FieldName";
			zTextBoxColumnStyleInfo2.CaptionResourceString = ZClientEDI.Res.GetData("496e3f4b-481a-45bf-b843-a82765a26db2", "Field Description");
			zTextBoxColumnStyleInfo2.ColumnName = "FieldDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(270);
			this.DocumentFieldsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.DocumentFieldsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.DocumentFieldsGrid.CopySelectedRowsAllowed = true;
			this.DocumentFieldsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DocumentFieldsGrid.GridId = "889c7172-56fa-4240-82a3-4df3cd374eb5";
			this.DocumentFieldsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DocumentFieldsGrid.IsWholeRowSelectedOnClick = true;
			this.DocumentFieldsGrid.LayoutKey = "zGrid1";
			this.DocumentFieldsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DocumentFieldsGrid.Name = "DocumentFieldsGrid";
			this.DocumentFieldsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 163, true);
			this.DocumentFieldsGrid.TabIndex = 0;
			this.DocumentFieldsGrid.DoubleClick += new System.EventHandler(this.DocumentFieldsGrid_DoubleClick);
			// 
			// CustomerServiceIncidentEmailTemplatesControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zGroupBox1);
			this.Name = "CustomerServiceIncidentEmailTemplatesControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(791, 404, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zGroupBox1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.HelpPictureBox)).EndInit();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.templatesSplitContainer.Panel1.ResumeLayout(false);
			this.templatesSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.templatesSplitContainer)).EndInit();
			this.templatesSplitContainer.ResumeLayout(false);
			this.legacyAndERequestV1TemplateGroupBox.ResumeLayout(false);
			this.legacyAndERequestV1TemplateGroupBox.PerformLayout();
			this.eRequestV2TemplateGroupBox.ResumeLayout(false);
			this.eRequestV2TemplateGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.DocumentFieldsGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBox1;
		private CargoWise.Windows.UI.KSplitContainer splitContainer1;
		protected Enterprise.ZArchitecture.ZTextBox legacyAndERequestV1EmailBodyTextBox;
		protected Enterprise.ZArchitecture.ZGrid DocumentFieldsGrid;
		protected Enterprise.ZArchitecture.ZTextBox legacyAndERequestV1EmailSubjectTextBox;
		private Enterprise.ZArchitecture.GUI.ZPictureBox HelpPictureBox;
		private CargoWise.Windows.UI.KSplitContainer templatesSplitContainer;
		private ZArchitecture.GUI.ZGroupBox legacyAndERequestV1TemplateGroupBox;
		private ZArchitecture.GUI.ZGroupBox eRequestV2TemplateGroupBox;
		protected ZArchitecture.ZTextBox eRequestV2EmailSubjectTextBox;
		protected ZArchitecture.ZTextBox eRequestV2EmailBodyTextBox;
	}
}
