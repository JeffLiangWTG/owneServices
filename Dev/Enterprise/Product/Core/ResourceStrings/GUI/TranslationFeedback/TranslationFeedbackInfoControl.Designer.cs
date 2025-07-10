namespace Enterprise.ResourceStrings.GUI.TranslationFeedback
{
	partial class TranslationFeedbackInfoControl
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
			this.statusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.statusTimeDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.companyTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.reportedByTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.dateReported = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.commentsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.reviewCommentsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.screenShotPictureBox = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.screenShotPictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ResourceStrings.Business.StmTranslationFeedback);
			// 
			// statusDropEdit
			// 
			this.statusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.statusDropEdit, "XT_Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ResourceStrings.Business.StmTranslationFeedback)(null)).XT_Status)));
			this.statusDropEdit.CaptionResourceString = Enterprise.ResourceStrings.GUI.Res.GetData("a3fb7fe0-2136-4574-a94c-eb232f53d5d9", "Status");
			this.statusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 57, true);
			this.statusDropEdit.Name = "statusDropEdit";
			this.statusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 20, true);
			this.statusDropEdit.TabIndex = 6;
			// 
			// statusTimeDateEdit
			// 
			this.statusTimeDateEdit.AllowDrop = true;
			this.statusTimeDateEdit.AutoCompleteMonthThreshold = 1;
			this.statusTimeDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.statusTimeDateEdit, "LocalStatusTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ResourceStrings.Business.StmTranslationFeedback)(null)).LocalStatusTime)));
			this.statusTimeDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.statusTimeDateEdit, false);
			this.statusTimeDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(285, 57, true);
			this.statusTimeDateEdit.Name = "statusTimeDateEdit";
			this.statusTimeDateEdit.TabIndex = 7;
			// 
			// companyTextBox
			// 
			this.BindingSource.SetBindingMember(this.companyTextBox, "CompanyLicenceCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ResourceStrings.Business.StmTranslationFeedback)(null)).CompanyLicenceCode)));
			this.companyTextBox.CaptionResourceString = Enterprise.ResourceStrings.GUI.Res.GetData("f4ff23e4-f637-4046-9748-7e76c062e322", "Company");
			this.companyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 4, true);
			this.companyTextBox.Name = "companyTextBox";
			this.companyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.companyTextBox.TabIndex = 2;
			// 
			// reportedByTextBox
			// 
			this.BindingSource.SetBindingMember(this.reportedByTextBox, "XT_ClientStaffInitial");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ResourceStrings.Business.StmTranslationFeedback)(null)).XT_ClientStaffInitial)));
			this.reportedByTextBox.CaptionResourceString = Enterprise.ResourceStrings.GUI.Res.GetData("672b29d3-d5bb-4fe3-a561-999464d9ac03", "Staff");
			this.reportedByTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(246, 4, true);
			this.reportedByTextBox.Name = "reportedByTextBox";
			this.reportedByTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 20, true);
			this.reportedByTextBox.TabIndex = 3;
			// 
			// dateReported
			// 
			this.dateReported.AllowDrop = true;
			this.dateReported.AutoCompleteMonthThreshold = 1;
			this.dateReported.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.dateReported, "XT_SystemCreateTimeUtc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ResourceStrings.Business.StmTranslationFeedback)(null)).XT_SystemCreateTimeUtc)));
			this.dateReported.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.dateReported, false);
			this.dateReported.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(294, 4, true);
			this.dateReported.Name = "dateReported";
			this.dateReported.TabIndex = 4;
			// 
			// commentsTextBox
			// 
			this.commentsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.commentsTextBox, "XT_Comments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ResourceStrings.Business.StmTranslationFeedback)(null)).XT_Comments)));
			this.commentsTextBox.CaptionResourceString = Enterprise.ResourceStrings.GUI.Res.GetData("a82ab99c-b034-497f-afb1-943914ba21b8", "Comments");
			this.commentsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.commentsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 31, true);
			this.commentsTextBox.Name = "commentsTextBox";
			this.commentsTextBox.ReadOnly = true;
			this.commentsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(333, 20, true);
			this.commentsTextBox.TabIndex = 5;
			// 
			// reviewCommentsTextBox
			// 
			this.reviewCommentsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.reviewCommentsTextBox, "XT_ReviewComment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ResourceStrings.Business.StmTranslationFeedback)(null)).XT_ReviewComment)));
			this.reviewCommentsTextBox.CaptionResourceString = Enterprise.ResourceStrings.GUI.Res.GetData("f18fc72c-4a72-4ead-8132-020fe4ae1f46", "Review Comment");
			this.reviewCommentsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.reviewCommentsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 84, true);
			this.reviewCommentsTextBox.Name = "reviewCommentsTextBox";
			this.reviewCommentsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(333, 20, true);
			this.reviewCommentsTextBox.TabIndex = 8;
			// 
			// screenShotPictureBox
			// 
			this.screenShotPictureBox.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.screenShotPictureBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.screenShotPictureBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(437, 4, true);
			this.screenShotPictureBox.Name = "screenShotPictureBox";
			this.screenShotPictureBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 100, true);
			this.screenShotPictureBox.TabIndex = 7;
			this.screenShotPictureBox.TabStop = false;
			this.screenShotPictureBox.Click += new System.EventHandler(this.screenShotPictureBox_Click);
			// 
			// TranslationFeedbackInfoControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.screenShotPictureBox);
			this.Controls.Add(this.reviewCommentsTextBox);
			this.Controls.Add(this.commentsTextBox);
			this.Controls.Add(this.dateReported);
			this.Controls.Add(this.reportedByTextBox);
			this.Controls.Add(this.companyTextBox);
			this.Controls.Add(this.statusTimeDateEdit);
			this.Controls.Add(this.statusDropEdit);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(540, 108, true);
			this.Name = "TranslationFeedbackInfoControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(540, 108, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.screenShotPictureBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZDropEdit statusDropEdit;
		private ZArchitecture.GUI.ZDateEdit statusTimeDateEdit;
		private ZArchitecture.ZTextBox companyTextBox;
		private ZArchitecture.ZTextBox reportedByTextBox;
		private ZArchitecture.GUI.ZDateEdit dateReported;
		private ZArchitecture.ZTextBox commentsTextBox;
		private ZArchitecture.ZTextBox reviewCommentsTextBox;
		private Enterprise.ZArchitecture.GUI.ZPictureBox screenShotPictureBox;
	}
}
