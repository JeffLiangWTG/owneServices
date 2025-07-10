
namespace Enterprise.Registry.GUI
{
	partial class NotificationEmailTemplateControl
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
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.HelpPictureBox = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			this.splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.EmailSubjectTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EmailBodyTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DocumentFieldsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.PreviewButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zGroupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.HelpPictureBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DocumentFieldsGrid)).BeginInit();
			this.DocumentFieldsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.NotificationEmailTemplate);
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("NotificationEmailTemplateControl|ea0cc8d8-ec1c-469e-b404-ac795fe7ca33", "Email Content");
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
			this.HelpPictureBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 14, true);
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
			this.splitContainer1.Panel1.Controls.Add(this.EmailSubjectTextBox);
			this.splitContainer1.Panel1.Controls.Add(this.EmailBodyTextBox);
			this.splitContainer1.Panel1MinSize = 200;
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.DocumentFieldsGrid);
			this.splitContainer1.Panel2.Controls.Add(this.PreviewButton);
			this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 385, true);
			this.splitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(218);
			this.splitContainer1.TabIndex = 0;
			// 
			// EmailSubjectTextBox
			// 
			this.EmailSubjectTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.EmailSubjectTextBox, "EmailSubject");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.NotificationEmailTemplate)(null)).EmailSubject)));
			this.EmailSubjectTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.EmailSubjectTextBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("NotificationEmailTemplateControl|38eb967f-3371-4aad-b8bc-1fa698c55506", "Subject");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.EmailSubjectTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.EmailSubjectTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 21, true);
			this.EmailSubjectTextBox.Name = "EmailSubjectTextBox";
			this.EmailSubjectTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(779, 20, true);
			this.EmailSubjectTextBox.TabIndex = 0;
			this.EmailSubjectTextBox.Leave += new System.EventHandler(this.EmailTemplateTextBox_Leave);
			// 
			// EmailBodyTextBox
			// 
			this.EmailBodyTextBox.AcceptsReturn = true;
			this.EmailBodyTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.EmailBodyTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.EmailBodyTextBox, "EmailBody");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.NotificationEmailTemplate)(null)).EmailBody)));
			this.EmailBodyTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.EmailBodyTextBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("NotificationEmailTemplateControl|d40cceab-b0b6-4dc8-9f7f-e87318361fa7", "Body");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.EmailBodyTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.EmailBodyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 63, true);
			this.EmailBodyTextBox.Multiline = true;
			this.EmailBodyTextBox.Name = "EmailBodyTextBox";
			this.EmailBodyTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.EmailBodyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(779, 152, true);
			this.EmailBodyTextBox.TabIndex = 1;
			this.EmailBodyTextBox.Leave += new System.EventHandler(this.EmailTemplateTextBox_Leave);
			// 
			// DocumentFieldsGrid
			// 
			this.DocumentFieldsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.DocumentFieldsGrid, "DocumentFields");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.NotificationEmailTemplate)(null)).DocumentFields)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Integration.IDocumentField)(((System.Collections.IList)(((Enterprise.Registry.Business.NotificationEmailTemplate)(null)).DocumentFields)).SyncRoot)).FieldName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Integration.IDocumentField)(((System.Collections.IList)(((Enterprise.Registry.Business.NotificationEmailTemplate)(null)).DocumentFields)).SyncRoot)).FieldDescription)));
			this.DocumentFieldsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("NotificationEmailTemplateControl|2e52b00f-9248-4174-9d57-a6a8359f5f34", "Field Name");
			zTextBoxColumnStyleInfo1.ColumnName = "FieldName";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("NotificationEmailTemplateControl|0386eb3c-7582-4c4e-8693-a6b696a6a705", "Field Description");
			zTextBoxColumnStyleInfo2.ColumnName = "FieldDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(270);
			this.DocumentFieldsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.DocumentFieldsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.DocumentFieldsGrid.GridId = "889c7172-56fa-4240-82a3-4df3cd374eb5";
			this.DocumentFieldsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DocumentFieldsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DocumentFieldsGrid.IsWholeRowSelectedOnClick = true;
			this.DocumentFieldsGrid.LayoutKey = "zGrid1";
			this.DocumentFieldsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DocumentFieldsGrid.Name = "DocumentFieldsGrid";
			this.DocumentFieldsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 163, true);
			this.DocumentFieldsGrid.TabIndex = 0;
			this.DocumentFieldsGrid.DoubleClick += new System.EventHandler(this.DocumentFieldsGrid_DoubleClick);
			// 
			// previewButton
			// 
			this.PreviewButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("E0BC7BD4-A6B2-4EC5-9F46-4E50AB9F554D", "Preview");
			this.PreviewButton.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.PreviewButton.IsCaptionOverridden = false;
			this.PreviewButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 143, true);
			this.PreviewButton.Name = "previewButton";
			this.PreviewButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.PreviewButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(787, 23, true);
			this.PreviewButton.TabIndex = 1;
			this.PreviewButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.PreviewButton.ToolTipCaption = null;
			this.PreviewButton.UseVisualStyleBackColor = true;
			this.PreviewButton.Visible = false;
			this.PreviewButton.Click += new System.EventHandler(this.previewButton_Click);
			// 
			// NotificationEmailTemplateControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zGroupBox1);
			this.Name = "NotificationEmailTemplateControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(791, 404, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.HelpPictureBox)).EndInit();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel1.PerformLayout();
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.splitContainer1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.DocumentFieldsGrid)).EndInit();
			this.DocumentFieldsGrid.ResumeLayout(false);
			this.DocumentFieldsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBox1;
		internal CargoWise.Windows.UI.KSplitContainer splitContainer1;
		internal Enterprise.ZArchitecture.ZTextBox EmailBodyTextBox;
		internal Enterprise.ZArchitecture.ZGrid DocumentFieldsGrid;
		internal Enterprise.ZArchitecture.ZTextBox EmailSubjectTextBox;
		internal Enterprise.ZArchitecture.GUI.ZPictureBox HelpPictureBox;
		internal ZArchitecture.GUI.ZButton PreviewButton;

	}
}
