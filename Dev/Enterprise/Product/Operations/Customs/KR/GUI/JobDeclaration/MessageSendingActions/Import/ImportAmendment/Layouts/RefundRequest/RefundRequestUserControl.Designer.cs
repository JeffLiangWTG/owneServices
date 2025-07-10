namespace Enterprise.Customs.KR.GUI
{
	partial class RefundRequestUserControl
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
			this.RefundAmountOfValueForVATCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.RefundAmountOfVATExemptionValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.VersionNoCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CustomsDisbursementBillTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TaxOfficeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.RefundSentWith5FEDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RefundReasonDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RefundCauseDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RefundTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TaxOfficeCodeFindBox.SuspendLayout();
			this.RefundSentWith5FEDropEdit.SuspendLayout();
			this.RefundReasonDropEdit.SuspendLayout();
			this.RefundCauseDropEdit.SuspendLayout();
			this.RefundTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObjectParent);
			// 
			// RefundAmountOfValueForVATCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.RefundAmountOfValueForVATCalcEdit, "SendingObjectsCollection.RefundAmountOfValueForVAT");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).RefundAmountOfValueForVAT)));
			this.RefundAmountOfValueForVATCalcEdit.DecimalPlaces = 2;
			this.RefundAmountOfValueForVATCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 225, true);
			this.RefundAmountOfValueForVATCalcEdit.Name = "RefundAmountOfValueForVATCalcEdit";
			this.RefundAmountOfValueForVATCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 11, true);
			this.RefundAmountOfValueForVATCalcEdit.TabIndex = 15;
			this.RefundAmountOfValueForVATCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.RefundAmountOfValueForVATCalcEdit.TrackDisposedAccess = true;
			// 
			// RefundAmountOfVATExemptionValueCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.RefundAmountOfVATExemptionValueCalcEdit, "SendingObjectsCollection.RefundAmountOfVATExemptionValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).RefundAmountOfVATExemptionValue)));
			this.RefundAmountOfVATExemptionValueCalcEdit.DecimalPlaces = 2;
			this.RefundAmountOfVATExemptionValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 253, true);
			this.RefundAmountOfVATExemptionValueCalcEdit.Name = "RefundAmountOfVATExemptionValueCalcEdit";
			this.RefundAmountOfVATExemptionValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 11, true);
			this.RefundAmountOfVATExemptionValueCalcEdit.TabIndex = 16;
			this.RefundAmountOfVATExemptionValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.RefundAmountOfVATExemptionValueCalcEdit.TrackDisposedAccess = true;
			// 
			// VersionNoCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.VersionNoCalcEdit, "SendingObjectsCollection.AmendSeqNo5WN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).AmendSeqNo5WN)));
			this.VersionNoCalcEdit.DecimalPlaces = 2;
			this.VersionNoCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 193, true);
			this.VersionNoCalcEdit.Name = "VersionNoCalcEdit";
			this.VersionNoCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 11, true);
			this.VersionNoCalcEdit.TabIndex = 68;
			this.VersionNoCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.VersionNoCalcEdit.TrackDisposedAccess = true;
			// 
			// CustomsDisbursementBillTextBox
			// 
			this.BindingSource.SetBindingMember(this.CustomsDisbursementBillTextBox, "SendingObjectsCollection.FormattedCustomsDisbursementBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).FormattedCustomsDisbursementBill)));
			this.CustomsDisbursementBillTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 165, true);
			this.CustomsDisbursementBillTextBox.Name = "CustomsDisbursementBillTextBox";
			this.CustomsDisbursementBillTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 11, true);
			this.CustomsDisbursementBillTextBox.TabIndex = 67;
			// 
			// TaxOfficeCodeFindBox
			// 
			this.TaxOfficeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TaxOfficeCodeFindBox, "SendingObjectsCollection.TaxOffice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).TaxOffice)));
			this.TaxOfficeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 129, true);
			this.TaxOfficeCodeFindBox.Name = "TaxOfficeCodeFindBox";
			this.TaxOfficeCodeFindBox.ParentType = null;
			this.TaxOfficeCodeFindBox.PreBoundMaxLength = 3;
			this.TaxOfficeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 11, true);
			this.TaxOfficeCodeFindBox.TabIndex = 66;
			// 
			// RefundSentWith5FEDropEdit
			// 
			this.RefundSentWith5FEDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RefundSentWith5FEDropEdit, "SendingObjectsCollection.Is5ULSentWith5FE");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).Is5ULSentWith5FE)));
			this.RefundSentWith5FEDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 102, true);
			this.RefundSentWith5FEDropEdit.Name = "RefundSentWith5FEDropEdit";
			this.RefundSentWith5FEDropEdit.PreBoundMaxLength = 1;
			this.RefundSentWith5FEDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 11, true);
			this.RefundSentWith5FEDropEdit.TabIndex = 65;
			// 
			// RefundReasonDropEdit
			// 
			this.RefundReasonDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RefundReasonDropEdit, "SendingObjectsCollection.RefundReason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).RefundReason)));
			this.RefundReasonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 73, true);
			this.RefundReasonDropEdit.Name = "RefundReasonDropEdit";
			this.RefundReasonDropEdit.PreBoundMaxLength = 2;
			this.RefundReasonDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 11, true);
			this.RefundReasonDropEdit.TabIndex = 64;
			// 
			// RefundCauseDropEdit
			// 
			this.RefundCauseDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RefundCauseDropEdit, "SendingObjectsCollection.RefundCause");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).RefundCause)));
			this.RefundCauseDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 45, true);
			this.RefundCauseDropEdit.Name = "RefundCauseDropEdit";
			this.RefundCauseDropEdit.PreBoundMaxLength = 2;
			this.RefundCauseDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 11, true);
			this.RefundCauseDropEdit.TabIndex = 63;
			// 
			// RefundTypeDropEdit
			// 
			this.RefundTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RefundTypeDropEdit, "SendingObjectsCollection.RefundType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).RefundType)));
			this.RefundTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 18, true);
			this.RefundTypeDropEdit.Name = "RefundTypeDropEdit";
			this.RefundTypeDropEdit.PreBoundMaxLength = 1;
			this.RefundTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 11, true);
			this.RefundTypeDropEdit.TabIndex = 62;
			// 
			// RefundRequestUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.VersionNoCalcEdit);
			this.Controls.Add(this.CustomsDisbursementBillTextBox);
			this.Controls.Add(this.TaxOfficeCodeFindBox);
			this.Controls.Add(this.RefundSentWith5FEDropEdit);
			this.Controls.Add(this.RefundReasonDropEdit);
			this.Controls.Add(this.RefundCauseDropEdit);
			this.Controls.Add(this.RefundTypeDropEdit);
			this.Controls.Add(this.RefundAmountOfVATExemptionValueCalcEdit);
			this.Controls.Add(this.RefundAmountOfValueForVATCalcEdit);
			this.Name = "RefundRequestUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 303, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TaxOfficeCodeFindBox.ResumeLayout(true);
			this.TaxOfficeCodeFindBox.PerformLayout();
			this.RefundSentWith5FEDropEdit.ResumeLayout(true);
			this.RefundSentWith5FEDropEdit.PerformLayout();
			this.RefundReasonDropEdit.ResumeLayout(true);
			this.RefundReasonDropEdit.PerformLayout();
			this.RefundCauseDropEdit.ResumeLayout(true);
			this.RefundCauseDropEdit.PerformLayout();
			this.RefundTypeDropEdit.ResumeLayout(true);
			this.RefundTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		internal ZArchitecture.ZCalcEdit RefundAmountOfValueForVATCalcEdit;
		internal ZArchitecture.ZCalcEdit RefundAmountOfVATExemptionValueCalcEdit;
		internal ZArchitecture.ZCalcEdit VersionNoCalcEdit;
		internal ZArchitecture.ZTextBox CustomsDisbursementBillTextBox;
		internal ZArchitecture.GUI.ZCodeFindBox TaxOfficeCodeFindBox;
		internal ZArchitecture.GUI.ZDropEdit RefundSentWith5FEDropEdit;
		internal ZArchitecture.GUI.ZDropEdit RefundReasonDropEdit;
		internal ZArchitecture.GUI.ZDropEdit RefundCauseDropEdit;
		internal ZArchitecture.GUI.ZDropEdit RefundTypeDropEdit;
	}
}
