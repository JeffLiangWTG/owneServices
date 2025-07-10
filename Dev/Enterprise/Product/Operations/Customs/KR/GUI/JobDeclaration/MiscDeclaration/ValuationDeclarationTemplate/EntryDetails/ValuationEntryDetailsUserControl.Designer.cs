namespace Enterprise.Customs.KR.GUI
{
	partial class ValuationEntryDetailsUserControl
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
            this.MessageStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.EntrySubmittedDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.EntryNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.AcceptedDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.ApprovalDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.EffectiveToDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.EntryStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.ApprovalNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.ResultReasonTextBox = new Enterprise.ZArchitecture.ZTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.MessageStatusDropEdit.SuspendLayout();
            this.EntrySubmittedDateDateEdit.SuspendLayout();
            this.AcceptedDateDateEdit.SuspendLayout();
            this.ApprovalDateDateEdit.SuspendLayout();
            this.EffectiveToDateDateEdit.SuspendLayout();
            this.EntryStatusDropEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobDeclaration);
            // 
            // MessageStatusDropEdit
            // 
            this.MessageStatusDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.MessageStatusDropEdit, "CustomsEntryHeaders.CH_Status");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CH_Status)));
            this.MessageStatusDropEdit.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("2cd3ace2-48cf-4650-8a2e-de4f79b5086d", "Message status");
            this.MessageStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 27, true);
            this.MessageStatusDropEdit.Name = "MessageStatusDropEdit";
            this.MessageStatusDropEdit.PreBoundMaxLength = 3;
			this.MessageStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(111, 20, true);
            this.MessageStatusDropEdit.TabIndex = 0;
            // 
            // EntrySubmittedDateDateEdit
            // 
            this.EntrySubmittedDateDateEdit.AllowDrop = true;
            this.EntrySubmittedDateDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.EntrySubmittedDateDateEdit, "CustomsEntryHeaders.CH_EntrySubmittedDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CH_EntrySubmittedDate)));
            this.EntrySubmittedDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(183, 3, true);
            this.EntrySubmittedDateDateEdit.Name = "EntrySubmittedDateDateEdit";
            this.EntrySubmittedDateDateEdit.TabIndex = 1;
            // 
            // EntryNumberTextBox
            // 
            this.BindingSource.SetBindingMember(this.EntryNumberTextBox, "CustomsEntryHeaders.FormattedEntryNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).FormattedEntryNumber)));
            this.EntryNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
            this.EntryNumberTextBox.Name = "EntryNumberTextBox";
            this.EntryNumberTextBox.ReadOnly = true;
			this.EntryNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(93, 20, true);
            this.EntryNumberTextBox.TabIndex = 2;
            // 
            // AcceptedDateDateEdit
            // 
            this.AcceptedDateDateEdit.AllowDrop = true;
            this.AcceptedDateDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.AcceptedDateDateEdit, "CustomsEntryHeaders.AcceptedDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AcceptedDate)));
            this.AcceptedDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(183, 29, true);
            this.AcceptedDateDateEdit.Name = "AcceptedDateDateEdit";
            this.AcceptedDateDateEdit.TabIndex = 3;
            // 
            // ApprovalDateDateEdit
            // 
            this.ApprovalDateDateEdit.AllowDrop = true;
            this.ApprovalDateDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.ApprovalDateDateEdit, "CustomsEntryHeaders.MostRecentCustomsReviewDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).MostRecentCustomsReviewDate)));
            this.ApprovalDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(183, 54, true);
            this.ApprovalDateDateEdit.Name = "ApprovalDateDateEdit";
            this.ApprovalDateDateEdit.TabIndex = 4;
            // 
            // EffectiveToDateDateEdit
            // 
            this.EffectiveToDateDateEdit.AllowDrop = true;
            this.EffectiveToDateDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.EffectiveToDateDateEdit, "CustomsEntryHeaders.DueDateofLoading");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).DueDateofLoading)));
            this.EffectiveToDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(351, 3, true);
            this.EffectiveToDateDateEdit.Name = "EffectiveToDateDateEdit";
            this.EffectiveToDateDateEdit.TabIndex = 5;
            // 
            // EntryStatusDropEdit
            // 
            this.EntryStatusDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.EntryStatusDropEdit, "CustomsEntryHeaders.CH_EntryStatus");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CH_EntryStatus)));
            this.EntryStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 53, true);
            this.EntryStatusDropEdit.Name = "EntryStatusDropEdit";
            this.EntryStatusDropEdit.PreBoundMaxLength = 3;
			this.EntryStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(111, 20, true);
            this.EntryStatusDropEdit.TabIndex = 6;
            // 
            // ApprovalNumberTextBox
            // 
            this.BindingSource.SetBindingMember(this.ApprovalNumberTextBox, "CustomsEntryHeaders.FormattedRefNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).FormattedRefNumber)));
            this.ApprovalNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(351, 27, true);
            this.ApprovalNumberTextBox.Name = "ApprovalNumberTextBox";
            this.ApprovalNumberTextBox.ReadOnly = true;
			this.ApprovalNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(93, 20, true);
            this.ApprovalNumberTextBox.TabIndex = 7;
            // 
            // ResultReasonTextBox
            // 
            this.BindingSource.SetBindingMember(this.ResultReasonTextBox, "CustomsEntryHeaders.CH_CustomsMessageRemarks");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CH_CustomsMessageRemarks)));
            this.ResultReasonTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(351, 51, true);
            this.ResultReasonTextBox.Name = "ResultReasonTextBox";
            this.ResultReasonTextBox.ReadOnly = true;
			this.ResultReasonTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(93, 20, true);
            this.ResultReasonTextBox.TabIndex = 8;
            // 
            // ValuationEntryDetailsUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.ResultReasonTextBox);
            this.Controls.Add(this.ApprovalNumberTextBox);
            this.Controls.Add(this.EntryStatusDropEdit);
            this.Controls.Add(this.EffectiveToDateDateEdit);
            this.Controls.Add(this.ApprovalDateDateEdit);
            this.Controls.Add(this.AcceptedDateDateEdit);
            this.Controls.Add(this.EntryNumberTextBox);
            this.Controls.Add(this.EntrySubmittedDateDateEdit);
            this.Controls.Add(this.MessageStatusDropEdit);
            this.Name = "ValuationEntryDetailsUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(513, 82, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.MessageStatusDropEdit.ResumeLayout(true);
            this.MessageStatusDropEdit.PerformLayout();
            this.EntrySubmittedDateDateEdit.ResumeLayout(true);
            this.EntrySubmittedDateDateEdit.PerformLayout();
            this.AcceptedDateDateEdit.ResumeLayout(true);
            this.AcceptedDateDateEdit.PerformLayout();
            this.ApprovalDateDateEdit.ResumeLayout(true);
            this.ApprovalDateDateEdit.PerformLayout();
            this.EffectiveToDateDateEdit.ResumeLayout(true);
            this.EffectiveToDateDateEdit.PerformLayout();
            this.EntryStatusDropEdit.ResumeLayout(true);
            this.EntryStatusDropEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		public ZArchitecture.GUI.ZDropEdit MessageStatusDropEdit;
		public ZArchitecture.GUI.ZDateEdit EntrySubmittedDateDateEdit;
		public ZArchitecture.ZTextBox EntryNumberTextBox;
		public ZArchitecture.GUI.ZDateEdit AcceptedDateDateEdit;
		public ZArchitecture.GUI.ZDateEdit ApprovalDateDateEdit;
		public ZArchitecture.GUI.ZDateEdit EffectiveToDateDateEdit;
		public ZArchitecture.GUI.ZDropEdit EntryStatusDropEdit;
		public ZArchitecture.ZTextBox ApprovalNumberTextBox;
		public ZArchitecture.ZTextBox ResultReasonTextBox;
	}
}
