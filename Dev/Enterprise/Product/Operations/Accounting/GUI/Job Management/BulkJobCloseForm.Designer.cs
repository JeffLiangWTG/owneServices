namespace Enterprise.Accounting.GUI.JobManagement
{
	partial class BulkJobCloseForm
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
            this.zDropEdit1 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.zDateRangeControl1 = new Enterprise.ZArchitecture.GUI.Internal.ZDateRangeControl();
            this.zDateRangeControl2 = new Enterprise.ZArchitecture.GUI.Internal.ZDateRangeControl();
            this.btnClear = new Enterprise.ZArchitecture.GUI.ZButton();
            this.btnFind = new Enterprise.ZArchitecture.GUI.ZButton();
            this.btnClose = new Enterprise.ZArchitecture.GUI.ZButton();
            this.btnCloseJob = new Enterprise.ZArchitecture.GUI.ZButton();
            this.zlblTotalNumberOfJobMessage = new Enterprise.ZArchitecture.ZLabel();
            this.zDateEdit1 = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.DateFilterLabel = new Enterprise.ZArchitecture.ZLabel();
            this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
            this.zGroupBoxSrchCondition = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.zNoteLabel1 = new Enterprise.ZArchitecture.ZLabel();
            this.zNote4 = new Enterprise.ZArchitecture.ZLabel();
            this.zNote1 = new Enterprise.ZArchitecture.ZLabel();
            this.zGroupBoxSrchResult = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.zGroupBoxJobCloseDate = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.zNote5 = new Enterprise.ZArchitecture.ZLabel();
            this.zNote3 = new Enterprise.ZArchitecture.ZLabel();
            this.zNoteLabel2 = new Enterprise.ZArchitecture.ZLabel();
            this.zNote2 = new Enterprise.ZArchitecture.ZLabel();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.zDropEdit1.SuspendLayout();
            this.zDateRangeControl1.SuspendLayout();
            this.zDateRangeControl2.SuspendLayout();
            this.zDateEdit1.SuspendLayout();
            this.zGroupBoxSrchCondition.SuspendLayout();
            this.zGroupBoxSrchResult.SuspendLayout();
            this.zGroupBoxJobCloseDate.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 473, true);
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(570, 24, true);
            this.MainStatusBar.SizingGrip = false;
            this.MainStatusBar.TabIndex = 3;
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.JobInvoicing.BulkJobCloseProcessor);
            // 
            // zDropEdit1
            // 
            this.zDropEdit1.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.zDropEdit1, "JobStatusFilter");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.JobInvoicing.BulkJobCloseProcessor)(null)).JobStatusFilter)));
            this.zDropEdit1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("e970ecab-09e5-4534-a8cc-fc1a54fd8736", "Job Status");
            this.zDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 28, true);
            this.zDropEdit1.Name = "zDropEdit1";
            this.zDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 15, true);
            this.zDropEdit1.TabIndex = 0;
            // 
            // zDateRangeControl1
            // 
            this.zDateRangeControl1.AllowDrop = true;
            this.zDateRangeControl1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.zDateRangeControl1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("088ec5e8-13a9-46b2-b82f-613a9335850d", "Job Open date");
            this.zDateRangeControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 55, true);
            this.zDateRangeControl1.Name = "zDateRangeControl1";
            this.zDateRangeControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(428, 21, true);
            this.zDateRangeControl1.TabIndex = 1;
            // 
            // zDateRangeControl2
            // 
            this.zDateRangeControl2.AllowDrop = true;
            this.zDateRangeControl2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("d768c46f-d995-43c5-bf13-cc86e3424cde", "Job Last Edit Time");
            this.zDateRangeControl2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 82, true);
            this.zDateRangeControl2.Name = "zDateRangeControl2";
            this.zDateRangeControl2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(428, 20, true);
            this.zDateRangeControl2.TabIndex = 2;
            // 
            // btnClear
            // 
            this.btnClear.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("b73bdfaa-6d66-4081-8608-594a6de60c4b", "Clear");
            this.btnClear.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(464, 108, true);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
            this.btnClear.TabIndex = 4;
            this.btnClear.ToolTipCaption = null;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnFind
            // 
            this.btnFind.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("04274634-cd20-4eab-91ae-c07076763454", "Find");
            this.btnFind.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(383, 108, true);
            this.btnFind.Name = "btnFind";
            this.btnFind.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
            this.btnFind.TabIndex = 3;
            this.btnFind.ToolTipCaption = null;
            this.btnFind.Click += new System.EventHandler(this.btnFind_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("907de32c-099a-4116-abf5-f275776f0a22", "Cancel");
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(464, 156, true);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 24, true);
            this.btnClose.TabIndex = 5;
            this.btnClose.ToolTipCaption = null;
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnCloseJob
            // 
            this.btnCloseJob.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("15c85ca3-16b6-486f-a166-d8121c22f6ae", "Close Jobs");
            this.btnCloseJob.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(383, 156, true);
            this.btnCloseJob.Name = "btnCloseJob";
            this.btnCloseJob.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 24, true);
            this.btnCloseJob.TabIndex = 4;
            this.btnCloseJob.ToolTipCaption = null;
            this.btnCloseJob.UseVisualStyleBackColor = true;
            this.btnCloseJob.Click += new System.EventHandler(this.btnCloseJob_Click);
            // 
            // zlblTotalNumberOfJobMessage
            // 
            this.BindingSource.SetBindingMember(this.zlblTotalNumberOfJobMessage, "TotalNumberOfJobMessage");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.JobInvoicing.BulkJobCloseProcessor)(null)).TotalNumberOfJobMessage)));
            this.zlblTotalNumberOfJobMessage.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zlblTotalNumberOfJobMessage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 20, true);
            this.zlblTotalNumberOfJobMessage.Name = "zlblTotalNumberOfJobMessage";
            this.zlblTotalNumberOfJobMessage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(493, 23, true);
            this.zlblTotalNumberOfJobMessage.TabIndex = 0;
            // 
            // zDateEdit1
            // 
            this.zDateEdit1.AllowDrop = true;
            this.zDateEdit1.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.zDateEdit1, "JobCloseDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.JobInvoicing.BulkJobCloseProcessor)(null)).JobCloseDate)));
            this.zDateEdit1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("fbb7c1fc-8965-440a-b2c9-2b6acd2d7b4e", "Job Close Date");
            this.zDateEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 29, true);
            this.zDateEdit1.Name = "zDateEdit1";
            this.zDateEdit1.TabIndex = 0;
            // 
            // DateFilterLabel
            // 
            this.DateFilterLabel.AutoSize = true;
            this.DateFilterLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("2ea1a01b-bcbc-47c4-bfe7-d28d3172e70a", "Job Open Date:");
            this.DateFilterLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.DateFilterLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(26, 55, true);
            this.DateFilterLabel.Name = "DateFilterLabel";
            this.DateFilterLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 13, true);
            this.DateFilterLabel.TabIndex = 7;
            this.DateFilterLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // zLabel2
            // 
            this.zLabel2.AutoSize = true;
            this.zLabel2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("e20ca964-8b72-4310-a388-b3f5763cad4c", "Job Last Edit Time:");
            this.zLabel2.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 82, true);
            this.zLabel2.Name = "zLabel2";
            this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(99, 13, true);
            this.zLabel2.TabIndex = 8;
            this.zLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // zGroupBoxSrchCondition
            // 
            this.zGroupBoxSrchCondition.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("a5de0c5c-d4fe-4f15-ab1a-9c5833c99033", "Search Conditions");
            this.zGroupBoxSrchCondition.Controls.Add(this.zNoteLabel1);
            this.zGroupBoxSrchCondition.Controls.Add(this.zNote4);
            this.zGroupBoxSrchCondition.Controls.Add(this.zNote1);
            this.zGroupBoxSrchCondition.Controls.Add(this.DateFilterLabel);
            this.zGroupBoxSrchCondition.Controls.Add(this.zLabel2);
            this.zGroupBoxSrchCondition.Controls.Add(this.zDropEdit1);
            this.zGroupBoxSrchCondition.Controls.Add(this.zDateRangeControl1);
            this.zGroupBoxSrchCondition.Controls.Add(this.zDateRangeControl2);
            this.zGroupBoxSrchCondition.Controls.Add(this.btnFind);
            this.zGroupBoxSrchCondition.Controls.Add(this.btnClear);
            this.zGroupBoxSrchCondition.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 6, true);
            this.zGroupBoxSrchCondition.Name = "zGroupBoxSrchCondition";
            this.zGroupBoxSrchCondition.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(561, 205, true);
            this.zGroupBoxSrchCondition.TabIndex = 0;
            this.zGroupBoxSrchCondition.TabStop = false;
            // 
            // zNoteLabel1
            // 
            this.zNoteLabel1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("2505f63c-a2e1-401b-af80-ee9097451f72", "Notes:");
            this.zNoteLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.zNoteLabel1.IsFontBold = true;
            this.zNoteLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 138, true);
            this.zNoteLabel1.Name = "zNoteLabel1";
            this.zNoteLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 17, true);
            this.zNoteLabel1.TabIndex = 5;
            this.zNoteLabel1.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            // 
            // zNote4
            // 
            this.zNote4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("1ECC9AB7-EFF8-4D66-BD89-345D0B919C66", "2. Jobs that contain posted disbursement clearing balance will be automatically excluded.");
            this.zNote4.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zNote4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 176, true);
            this.zNote4.Name = "zNote4";
            this.zNote4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(495, 18, true);
            this.zNote4.TabIndex = 6;
            this.zNote4.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            // 
            // zNote1
            // 
            this.zNote1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("02188104-7412-4d12-9bda-c793b68156df", "1. Jobs with open WIP/ACR or Unrecognized Revenue/Cost will be automatically excluded.");
            this.zNote1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zNote1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 158, true);
            this.zNote1.Name = "zNote1";
            this.zNote1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(495, 18, true);
            this.zNote1.TabIndex = 6;
            this.zNote1.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            // 
            // zGroupBoxSrchResult
            // 
            this.zGroupBoxSrchResult.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("89f064df-2d12-47a9-9aa2-628a81dac308", "Search Result");
            this.zGroupBoxSrchResult.Controls.Add(this.zlblTotalNumberOfJobMessage);
            this.zGroupBoxSrchResult.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.zGroupBoxSrchResult.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 215, true);
            this.zGroupBoxSrchResult.Name = "zGroupBoxSrchResult";
            this.zGroupBoxSrchResult.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(561, 55, true);
            this.zGroupBoxSrchResult.TabIndex = 1;
            this.zGroupBoxSrchResult.TabStop = false;
            // 
            // zGroupBoxJobCloseDate
            // 
            this.zGroupBoxJobCloseDate.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("32bf4796-8c90-4213-ac33-648b66d0c53a", "Set ‘Job Close Date’");
            this.zGroupBoxJobCloseDate.Controls.Add(this.zNote5);
            this.zGroupBoxJobCloseDate.Controls.Add(this.zNote3);
            this.zGroupBoxJobCloseDate.Controls.Add(this.zNoteLabel2);
            this.zGroupBoxJobCloseDate.Controls.Add(this.btnClose);
            this.zGroupBoxJobCloseDate.Controls.Add(this.zNote2);
            this.zGroupBoxJobCloseDate.Controls.Add(this.zDateEdit1);
            this.zGroupBoxJobCloseDate.Controls.Add(this.btnCloseJob);
            this.zGroupBoxJobCloseDate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.zGroupBoxJobCloseDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 275, true);
            this.zGroupBoxJobCloseDate.Name = "zGroupBoxJobCloseDate";
            this.zGroupBoxJobCloseDate.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(561, 192, true);
            this.zGroupBoxJobCloseDate.TabIndex = 2;
            this.zGroupBoxJobCloseDate.TabStop = false;
            // 
            // zNote5
            // 
            this.zNote5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("3b2158fe-0318-46cb-ba73-f4ba1f97ed29", "3. Inactive Jobs will not be closed.");
            this.zNote5.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zNote5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 136, true);
            this.zNote5.Name = "zNote5";
            this.zNote5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(487, 15, true);
            this.zNote5.TabIndex = 6;
            this.zNote5.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            // 
            // zNote3
            // 
            this.zNote3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("8bd98e4f-4185-4d48-b627-2df47ef6a66c", "2. Closing Jobs will not run workflow trigger.");
            this.zNote3.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zNote3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 120, true);
            this.zNote3.Name = "zNote3";
            this.zNote3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(487, 15, true);
            this.zNote3.TabIndex = 3;
            this.zNote3.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            // 
            // zNoteLabel2
            // 
            this.zNoteLabel2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("d99450c3-64ef-47be-9f0d-05621c2229c3", "Notes:");
            this.zNoteLabel2.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.zNoteLabel2.IsFontBold = true;
            this.zNoteLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 71, true);
            this.zNoteLabel2.Name = "zNoteLabel2";
            this.zNoteLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 19, true);
            this.zNoteLabel2.TabIndex = 1;
            this.zNoteLabel2.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            // 
            // zNote2
            // 
            this.zNote2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("e2bf0a9d-0d97-4796-90ab-a6870dfc0a80", "1. If Job Open Date for a job is later than the Job Close Date specified above, then the later date will be used as Job Close Date.");
            this.zNote2.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zNote2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 90, true);
            this.zNote2.Name = "zNote2";
            this.zNote2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(487, 30, true);
            this.zNote2.TabIndex = 2;
            this.zNote2.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            // 
            // BulkJobCloseForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.CancelButton = this.btnClose;
            this.CaptionRenderingEnabled = true;
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(570, 497, true);
            this.Controls.Add(this.zGroupBoxJobCloseDate);
            this.Controls.Add(this.zGroupBoxSrchCondition);
            this.Controls.Add(this.zGroupBoxSrchResult);
            this.Cursor = System.Windows.Forms.Cursors.Default;
            this.DataSourceType = typeof(Enterprise.Accounting.Business.JobInvoicing.BulkJobCloseProcessor);
            this.DisplayMode = Enterprise.ZArchitecture.Core.ODisplayMode.New;
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "BulkJobCloseForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Controls.SetChildIndex(this.zGroupBoxSrchResult, 0);
            this.Controls.SetChildIndex(this.MainStatusBar, 0);
            this.Controls.SetChildIndex(this.zGroupBoxSrchCondition, 0);
            this.Controls.SetChildIndex(this.zGroupBoxJobCloseDate, 0);
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.zDropEdit1.ResumeLayout(true);
            this.zDropEdit1.PerformLayout();
            this.zDateRangeControl1.ResumeLayout(true);
            this.zDateRangeControl1.PerformLayout();
            this.zDateRangeControl2.ResumeLayout(true);
            this.zDateRangeControl2.PerformLayout();
            this.zDateEdit1.ResumeLayout(true);
            this.zDateEdit1.PerformLayout();
            this.zGroupBoxSrchCondition.ResumeLayout(false);
            this.zGroupBoxSrchCondition.PerformLayout();
            this.zGroupBoxSrchResult.ResumeLayout(false);
            this.zGroupBoxSrchResult.PerformLayout();
            this.zGroupBoxJobCloseDate.ResumeLayout(false);
            this.zGroupBoxJobCloseDate.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.Internal.ZDateRangeControl zDateRangeControl1;
		private ZArchitecture.GUI.Internal.ZDateRangeControl zDateRangeControl2;
		private ZArchitecture.GUI.ZButton btnClear;
		private ZArchitecture.GUI.ZButton btnFind;
		private ZArchitecture.GUI.ZButton btnClose;
		private ZArchitecture.GUI.ZButton btnCloseJob;
		private ZArchitecture.GUI.ZDropEdit zDropEdit1;
		private ZArchitecture.ZLabel zlblTotalNumberOfJobMessage;
		private ZArchitecture.GUI.ZDateEdit zDateEdit1;
		private ZArchitecture.ZLabel DateFilterLabel;
		private ZArchitecture.ZLabel zLabel2;
		private ZArchitecture.ZLabel zNoteLabel1;
		private ZArchitecture.ZLabel zNote1;
		private ZArchitecture.GUI.ZGroupBox zGroupBoxSrchResult;
		private ZArchitecture.GUI.ZGroupBox zGroupBoxJobCloseDate;
		private ZArchitecture.ZLabel zNote3;
		private ZArchitecture.ZLabel zNoteLabel2;
		private ZArchitecture.ZLabel zNote2;
		private System.Windows.Forms.ToolTip toolTip1;
		protected ZArchitecture.GUI.ZGroupBox zGroupBoxSrchCondition;
		private ZArchitecture.ZLabel zNote4;
		private ZArchitecture.ZLabel zNote5;
	}
}
