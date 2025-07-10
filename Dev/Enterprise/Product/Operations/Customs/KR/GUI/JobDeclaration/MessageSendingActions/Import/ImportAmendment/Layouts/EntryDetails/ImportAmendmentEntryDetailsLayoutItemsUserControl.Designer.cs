using System.Windows.Forms;

namespace Enterprise.Customs.KR.GUI
{
	partial class ImportAmendmentEntryDetailsLayoutItemsUserControl
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
            this.VersionNoCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.ReasonCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.AmendmentReasonTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.FaultPartyDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.FaultReasonTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.PenaltyPaymentReasonCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.TotalAmendedCountItemCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.TotalAmendedCountDutyTaxCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.BeforeTotalDutyTaxCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.AfterTotalDutyTaxCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.DutyTaxDifferenceCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.BeforeCustomsValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.AfterCustomsValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.CustomsValueDifferenceCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.AmendmentTypeTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.AmendmentTypeDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.DTYPenaltyTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.DTYPenaltyReducedYNDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.DomesticTaxPenaltyTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.PenaltyExemptReqDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.PenaltyExemptReasonCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.PenaltyExemptReasonMultiLineTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.PenaltyExemptSequenceCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.PenaltyExemptAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.RefundRequestYNDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.ReasonCodeDropEdit.SuspendLayout();
            this.FaultPartyDropEdit.SuspendLayout();
            this.PenaltyPaymentReasonCodeFindBox.SuspendLayout();
            this.DTYPenaltyTypeDropEdit.SuspendLayout();
            this.DTYPenaltyReducedYNDropEdit.SuspendLayout();
            this.DomesticTaxPenaltyTypeDropEdit.SuspendLayout();
            this.PenaltyExemptReqDropEdit.SuspendLayout();
            this.PenaltyExemptReasonCodeDropEdit.SuspendLayout();
            this.RefundRequestYNDropEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObjectParent);
            // 
            // VersionNoCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.VersionNoCalcEdit, "SendingObjectsCollection.AmendmentVersion");
			this.VersionNoCalcEdit.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("8d8068c0-6bb8-48f9-8864-7c76eff35db6", "Version No / Amendment Type");
			this.VersionNoCalcEdit.DecimalPlaces = 2;
            this.VersionNoCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(458, 306, true);
            this.VersionNoCalcEdit.Name = "VersionNoCalcEdit";
            this.VersionNoCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 17, true);
            this.VersionNoCalcEdit.TabIndex = 0;
            this.VersionNoCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.VersionNoCalcEdit.TrackDisposedAccess = true;
            // 
            // ReasonCodeDropEdit
            // 
            this.ReasonCodeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ReasonCodeDropEdit, "SendingObjectsCollection.ReasonCode");
			this.ReasonCodeDropEdit.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("489da492-889a-4d97-8f8b-7d9f9bece414", "Reason Code");
			this.ReasonCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(458, 164, true);
            this.ReasonCodeDropEdit.Name = "ReasonCodeDropEdit";
            this.ReasonCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(311, 17, true);
            this.ReasonCodeDropEdit.TabIndex = 2;
            // 
            // AmendmentReasonTextBox
            // 
            this.BindingSource.SetBindingMember(this.AmendmentReasonTextBox, "SendingObjectsCollection.AmendmentReason");
			this.AmendmentReasonTextBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("586b9927-e936-4290-8a7b-2407c65c3563", "Amendment Reason");
			this.AmendmentReasonTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.AmendmentReasonTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(458, 186, true);
            this.AmendmentReasonTextBox.Multiline = true;
            this.AmendmentReasonTextBox.Name = "AmendmentReasonTextBox";
            this.AmendmentReasonTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(311, 41, true);
            this.AmendmentReasonTextBox.TabIndex = 3;
            // 
            // FaultPartyDropEdit
            // 
            this.FaultPartyDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.FaultPartyDropEdit, "SendingObjectsCollection.FaultParty");
			this.FaultPartyDropEdit.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("165b8123-b662-4956-bbd0-9306bea82e10", "Fault Party");
			this.FaultPartyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(458, 229, true);
            this.FaultPartyDropEdit.Name = "FaultPartyDropEdit";
            this.FaultPartyDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(311, 17, true);
            this.FaultPartyDropEdit.TabIndex = 4;
            // 
            // FaultReasonTextBox
            // 
            this.BindingSource.SetBindingMember(this.FaultReasonTextBox, "SendingObjectsCollection.FaultPartyOtherDescription");
			this.FaultReasonTextBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("40517246-5ddb-46db-9da3-98ab2f85ab62", "Fault Reason");
			this.FaultReasonTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.FaultReasonTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(458, 250, true);
            this.FaultReasonTextBox.Multiline = true;
            this.FaultReasonTextBox.Name = "FaultReasonTextBox";
            this.FaultReasonTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(311, 52, true);
            this.FaultReasonTextBox.TabIndex = 5;
            // 
            // PenaltyPaymentReasonCodeFindBox
            // 
            this.PenaltyPaymentReasonCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.PenaltyPaymentReasonCodeFindBox, "SendingObjectsCollection.PenaltyPaymentReasonCode");
            this.PenaltyPaymentReasonCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(458, 21, true);
            this.PenaltyPaymentReasonCodeFindBox.Name = "PenaltyPaymentReasonCodeFindBox";
            this.PenaltyPaymentReasonCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.PenaltyPaymentReasonCodeFindBox.ParentType = null;
            this.PenaltyPaymentReasonCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(236, 17, true);
            this.PenaltyPaymentReasonCodeFindBox.TabIndex = 6;
            // 
            // TotalAmendedCountItemCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.TotalAmendedCountItemCalcEdit, "SendingObjectsCollection.TotalAmendedItemsCount");
			this.TotalAmendedCountItemCalcEdit.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("b1994ada-d367-4bb9-9905-7f51edc74f10", "Total Amended Count (Item / Duty Tax)");
            this.TotalAmendedCountItemCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(458, 43, true);
            this.TotalAmendedCountItemCalcEdit.Name = "TotalAmendedCountItemCalcEdit";
            this.TotalAmendedCountItemCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 17, true);
            this.TotalAmendedCountItemCalcEdit.TabIndex = 7;
            this.TotalAmendedCountItemCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.TotalAmendedCountItemCalcEdit.TrackDisposedAccess = true;
            // 
            // TotalAmendedCountDutyTaxCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.TotalAmendedCountDutyTaxCalcEdit, "SendingObjectsCollection.TotalAmendedTaxCount");
			this.TotalAmendedCountDutyTaxCalcEdit.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("3b933f66-b7af-4cf9-820d-baaa4b68ed76", "/");
            this.TotalAmendedCountDutyTaxCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(587, 43, true);
            this.TotalAmendedCountDutyTaxCalcEdit.Name = "TotalAmendedCountDutyTaxCalcEdit";
            this.TotalAmendedCountDutyTaxCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(107, 17, true);
            this.TotalAmendedCountDutyTaxCalcEdit.TabIndex = 8;
            this.TotalAmendedCountDutyTaxCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.TotalAmendedCountDutyTaxCalcEdit.TrackDisposedAccess = true;
            // 
            // BeforeTotalDutyTaxCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.BeforeTotalDutyTaxCalcEdit, "SendingObjectsCollection.BeforeTotalDutyTaxAmount");
			this.BeforeTotalDutyTaxCalcEdit.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("8e818df1-89de-47f9-b346-4692af1cb151", "Total Duty Tax Amount (Before / After)");
            this.BeforeTotalDutyTaxCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(458, 65, true);
            this.BeforeTotalDutyTaxCalcEdit.Name = "BeforeTotalDutyTaxCalcEdit";
            this.BeforeTotalDutyTaxCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 17, true);
            this.BeforeTotalDutyTaxCalcEdit.TabIndex = 9;
            this.BeforeTotalDutyTaxCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.BeforeTotalDutyTaxCalcEdit.TrackDisposedAccess = true;
            // 
            // AfterTotalDutyTaxCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.AfterTotalDutyTaxCalcEdit, "SendingObjectsCollection.AfterTotalDutyTaxAmount");
			this.AfterTotalDutyTaxCalcEdit.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("afca906a-fa98-49fe-96be-1d17add7c436", "/");
            this.AfterTotalDutyTaxCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(587, 65, true);
            this.AfterTotalDutyTaxCalcEdit.Name = "AfterTotalDutyTaxCalcEdit";
            this.AfterTotalDutyTaxCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(107, 17, true);
            this.AfterTotalDutyTaxCalcEdit.TabIndex = 10;
            this.AfterTotalDutyTaxCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.AfterTotalDutyTaxCalcEdit.TrackDisposedAccess = true;
            // 
            // DutyTaxDifferenceCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.DutyTaxDifferenceCalcEdit, "SendingObjectsCollection.DutyTaxDifference");
			this.DutyTaxDifferenceCalcEdit.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("2f5d1347-ba7c-4876-a168-50ef6016dbd2", "Duty Tax Difference");
			this.DutyTaxDifferenceCalcEdit.DecimalPlaces = 2;
            this.DutyTaxDifferenceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(458, 87, true);
            this.DutyTaxDifferenceCalcEdit.Name = "DutyTaxDifferenceCalcEdit";
            this.DutyTaxDifferenceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 17, true);
            this.DutyTaxDifferenceCalcEdit.TabIndex = 11;
            this.DutyTaxDifferenceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.DutyTaxDifferenceCalcEdit.TrackDisposedAccess = true;
            // 
            // BeforeCustomsValueCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.BeforeCustomsValueCalcEdit, "SendingObjectsCollection.BeforeCustomsValue");
			this.BeforeCustomsValueCalcEdit.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("18492172-12e2-46a2-9445-3e7963f993af", "Customs Value (Before /After)");
            this.BeforeCustomsValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(458, 108, true);
            this.BeforeCustomsValueCalcEdit.Name = "BeforeCustomsValueCalcEdit";
            this.BeforeCustomsValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 17, true);
            this.BeforeCustomsValueCalcEdit.TabIndex = 12;
            this.BeforeCustomsValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.BeforeCustomsValueCalcEdit.TrackDisposedAccess = true;
            // 
            // AfterCustomsValueCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.AfterCustomsValueCalcEdit, "SendingObjectsCollection.AfterCustomsValue");
			this.AfterCustomsValueCalcEdit.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("642c8db3-29c4-49aa-b2a9-7009fd8afcc5", "/");
			this.AfterCustomsValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(587, 108, true);
            this.AfterCustomsValueCalcEdit.Name = "AfterCustomsValueCalcEdit";
            this.AfterCustomsValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(107, 17, true);
            this.AfterCustomsValueCalcEdit.TabIndex = 13;
            this.AfterCustomsValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.AfterCustomsValueCalcEdit.TrackDisposedAccess = true;
            // 
            // CustomsValueDifferenceCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.CustomsValueDifferenceCalcEdit, "SendingObjectsCollection.CustomsValueDifference");
			this.CustomsValueDifferenceCalcEdit.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("d2b00daf-65b6-48ed-a111-f1d5702d6b3b", "Customs Value Difference");
			this.CustomsValueDifferenceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(458, 129, true);
            this.CustomsValueDifferenceCalcEdit.Name = "CustomsValueDifferenceCalcEdit";
            this.CustomsValueDifferenceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 17, true);
            this.CustomsValueDifferenceCalcEdit.TabIndex = 14;
            this.CustomsValueDifferenceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.CustomsValueDifferenceCalcEdit.TrackDisposedAccess = true;
            // 
            // AmendmentTypeTextBox
            // 
            this.BindingSource.SetBindingMember(this.AmendmentTypeTextBox, "SendingObjectsCollection.AmendmentType");
			this.AmendmentTypeTextBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("d8989552-494d-4269-9189-8c021cdea032", "/");
			this.AmendmentTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(548, 306, true);
            this.AmendmentTypeTextBox.Name = "AmendmentTypeTextBox";
            this.AmendmentTypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(59, 17, true);
            this.AmendmentTypeTextBox.TabIndex = 15;
            // 
            // AmendmentTypeDescriptionTextBox
            // 
            this.BindingSource.SetBindingMember(this.AmendmentTypeDescriptionTextBox, "SendingObjectsCollection.AmendmentTypeDescription");
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AmendmentTypeDescriptionTextBox, false);
            this.AmendmentTypeDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(613, 306, true);
            this.AmendmentTypeDescriptionTextBox.Name = "AmendmentTypeDescriptionTextBox";
            this.AmendmentTypeDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(156, 17, true);
            this.AmendmentTypeDescriptionTextBox.TabIndex = 16;
            // 
            // DTYPenaltyTypeDropEdit
            // 
            this.DTYPenaltyTypeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.DTYPenaltyTypeDropEdit, "SendingObjectsCollection.DutyPenaltyCause");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).DutyPenaltyCause)));
            this.DTYPenaltyTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 17, true);
            this.DTYPenaltyTypeDropEdit.Name = "DTYPenaltyTypeDropEdit";
            this.DTYPenaltyTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(311, 17, true);
            this.DTYPenaltyTypeDropEdit.TabIndex = 0;
            // 
            // DTYPenaltyReducedYNDropEdit
            // 
            this.DTYPenaltyReducedYNDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.DTYPenaltyReducedYNDropEdit, "SendingObjectsCollection.ApplyDutyPenaltyReduction");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).ApplyDutyPenaltyReduction)));
            this.DTYPenaltyReducedYNDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 39, true);
            this.DTYPenaltyReducedYNDropEdit.Name = "DTYPenaltyReducedYNDropEdit";
            this.DTYPenaltyReducedYNDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(311, 17, true);
            this.DTYPenaltyReducedYNDropEdit.TabIndex = 1;
            // 
            // DomesticTaxPenaltyTypeDropEdit
            // 
            this.DomesticTaxPenaltyTypeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.DomesticTaxPenaltyTypeDropEdit, "SendingObjectsCollection.TaxPenaltyCause");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).TaxPenaltyCause)));
            this.DomesticTaxPenaltyTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 73, true);
            this.DomesticTaxPenaltyTypeDropEdit.Name = "DomesticTaxPenaltyTypeDropEdit";
            this.DomesticTaxPenaltyTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(311, 17, true);
            this.DomesticTaxPenaltyTypeDropEdit.TabIndex = 2;
            // 
            // PenaltyExemptReqDropEdit
            // 
            this.PenaltyExemptReqDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.PenaltyExemptReqDropEdit, "SendingObjectsCollection.PenaltyExemptionIndicator");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).PenaltyExemptionIndicator)));
            this.PenaltyExemptReqDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 94, true);
            this.PenaltyExemptReqDropEdit.Name = "PenaltyExemptReqDropEdit";
            this.PenaltyExemptReqDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(311, 17, true);
            this.PenaltyExemptReqDropEdit.TabIndex = 3;
            // 
            // PenaltyExemptReasonCodeDropEdit
            // 
            this.PenaltyExemptReasonCodeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.PenaltyExemptReasonCodeDropEdit, "SendingObjectsCollection.PenaltyExemptionReasonCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).PenaltyExemptionReasonCode)));
            this.PenaltyExemptReasonCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 125, true);
            this.PenaltyExemptReasonCodeDropEdit.Name = "PenaltyExemptReasonCodeDropEdit";
            this.PenaltyExemptReasonCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
            this.PenaltyExemptReasonCodeDropEdit.TabIndex = 4;
            // 
            // PenaltyExemptReasonMultiLineTextBox
            // 
            this.BindingSource.SetBindingMember(this.PenaltyExemptReasonMultiLineTextBox, "SendingObjectsCollection.PenaltyExemptionReason");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).PenaltyExemptionReason)));
			this.PenaltyExemptReasonMultiLineTextBox.CharacterCasing = CharacterCasing.Normal;
			this.PenaltyExemptReasonMultiLineTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 154, true);
            this.PenaltyExemptReasonMultiLineTextBox.Multiline = true;
            this.PenaltyExemptReasonMultiLineTextBox.Name = "PenaltyExemptReasonMultiLineTextBox";
            this.PenaltyExemptReasonMultiLineTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 54, true);
            this.PenaltyExemptReasonMultiLineTextBox.TabIndex = 5;
			// 
			// PenaltyExemptSequenceCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PenaltyExemptSequenceCalcEdit, "SendingObjectsCollection.PenaltyExemptionReqSequence");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).PenaltyExemptionReqSequence)));
            this.PenaltyExemptSequenceCalcEdit.DecimalPlaces = 2;
            this.PenaltyExemptSequenceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 221, true);
            this.PenaltyExemptSequenceCalcEdit.Name = "PenaltyExemptSequenceCalcEdit";
            this.PenaltyExemptSequenceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 17, true);
            this.PenaltyExemptSequenceCalcEdit.TabIndex = 6;
            this.PenaltyExemptSequenceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.PenaltyExemptSequenceCalcEdit.TrackDisposedAccess = true;
            // 
            // PenaltyExemptAmountCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.PenaltyExemptAmountCalcEdit, "SendingObjectsCollection.PenaltyExemptionAmount");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).PenaltyExemptionAmount)));
            this.PenaltyExemptAmountCalcEdit.DecimalPlaces = 2;
            this.PenaltyExemptAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 250, true);
            this.PenaltyExemptAmountCalcEdit.Name = "PenaltyExemptAmountCalcEdit";
            this.PenaltyExemptAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
            this.PenaltyExemptAmountCalcEdit.TabIndex = 7;
            this.PenaltyExemptAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.PenaltyExemptAmountCalcEdit.TrackDisposedAccess = true;
            // 
            // RefundRequestYNDropEdit
            // 
            this.RefundRequestYNDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.RefundRequestYNDropEdit, "SendingObjectsCollection.RefundRequestSubmissionYN");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).RefundRequestSubmissionYN)));
            this.RefundRequestYNDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 279, true);
            this.RefundRequestYNDropEdit.Name = "RefundRequestYNDropEdit";
            this.RefundRequestYNDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
            this.RefundRequestYNDropEdit.TabIndex = 8;
            // 
            // ImportAmendmentEntryDetailsLayoutItemsUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.AmendmentTypeDescriptionTextBox);
            this.Controls.Add(this.AmendmentTypeTextBox);
            this.Controls.Add(this.CustomsValueDifferenceCalcEdit);
            this.Controls.Add(this.AfterCustomsValueCalcEdit);
            this.Controls.Add(this.BeforeCustomsValueCalcEdit);
            this.Controls.Add(this.DutyTaxDifferenceCalcEdit);
            this.Controls.Add(this.AfterTotalDutyTaxCalcEdit);
            this.Controls.Add(this.BeforeTotalDutyTaxCalcEdit);
            this.Controls.Add(this.TotalAmendedCountDutyTaxCalcEdit);
            this.Controls.Add(this.TotalAmendedCountItemCalcEdit);
            this.Controls.Add(this.PenaltyPaymentReasonCodeFindBox);
            this.Controls.Add(this.FaultReasonTextBox);
            this.Controls.Add(this.FaultPartyDropEdit);
            this.Controls.Add(this.AmendmentReasonTextBox);
            this.Controls.Add(this.ReasonCodeDropEdit);
            this.Controls.Add(this.VersionNoCalcEdit);
            this.Controls.Add(this.RefundRequestYNDropEdit);
            this.Controls.Add(this.PenaltyExemptAmountCalcEdit);
            this.Controls.Add(this.PenaltyExemptSequenceCalcEdit);
            this.Controls.Add(this.PenaltyExemptReasonMultiLineTextBox);
            this.Controls.Add(this.PenaltyExemptReasonCodeDropEdit);
            this.Controls.Add(this.PenaltyExemptReqDropEdit);
            this.Controls.Add(this.DomesticTaxPenaltyTypeDropEdit);
            this.Controls.Add(this.DTYPenaltyReducedYNDropEdit);
            this.Controls.Add(this.DTYPenaltyTypeDropEdit);
            this.Name = "ImportAmendmentEntryDetailsLayoutItemsUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 349, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ReasonCodeDropEdit.ResumeLayout(true);
            this.ReasonCodeDropEdit.PerformLayout();
            this.FaultPartyDropEdit.ResumeLayout(true);
            this.FaultPartyDropEdit.PerformLayout();
            this.PenaltyPaymentReasonCodeFindBox.ResumeLayout(true);
            this.PenaltyPaymentReasonCodeFindBox.PerformLayout();
            this.DTYPenaltyTypeDropEdit.ResumeLayout(true);
            this.DTYPenaltyTypeDropEdit.PerformLayout();
            this.DTYPenaltyReducedYNDropEdit.ResumeLayout(true);
            this.DTYPenaltyReducedYNDropEdit.PerformLayout();
            this.DomesticTaxPenaltyTypeDropEdit.ResumeLayout(true);
            this.DomesticTaxPenaltyTypeDropEdit.PerformLayout();
            this.PenaltyExemptReqDropEdit.ResumeLayout(true);
            this.PenaltyExemptReqDropEdit.PerformLayout();
            this.PenaltyExemptReasonCodeDropEdit.ResumeLayout(true);
            this.PenaltyExemptReasonCodeDropEdit.PerformLayout();
            this.RefundRequestYNDropEdit.ResumeLayout(true);
            this.RefundRequestYNDropEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		public ZArchitecture.ZCalcEdit VersionNoCalcEdit;
		public ZArchitecture.GUI.ZDropEdit ReasonCodeDropEdit;
		public ZArchitecture.ZTextBox AmendmentReasonTextBox;
		public ZArchitecture.GUI.ZDropEdit FaultPartyDropEdit;
		public ZArchitecture.ZTextBox FaultReasonTextBox;
		public ZArchitecture.GUI.ZCodeFindBox PenaltyPaymentReasonCodeFindBox;
		public ZArchitecture.ZCalcEdit TotalAmendedCountItemCalcEdit;
		public ZArchitecture.ZCalcEdit TotalAmendedCountDutyTaxCalcEdit;
		public ZArchitecture.ZCalcEdit BeforeTotalDutyTaxCalcEdit;
		public ZArchitecture.ZCalcEdit AfterTotalDutyTaxCalcEdit;
		public ZArchitecture.ZCalcEdit DutyTaxDifferenceCalcEdit;
		public ZArchitecture.ZCalcEdit BeforeCustomsValueCalcEdit;
		public ZArchitecture.ZCalcEdit AfterCustomsValueCalcEdit;
		public ZArchitecture.ZCalcEdit CustomsValueDifferenceCalcEdit;
		public ZArchitecture.ZTextBox AmendmentTypeTextBox;
		public ZArchitecture.ZTextBox AmendmentTypeDescriptionTextBox;
		internal ZArchitecture.GUI.ZDropEdit DTYPenaltyTypeDropEdit;
		internal ZArchitecture.GUI.ZDropEdit DTYPenaltyReducedYNDropEdit;
		internal ZArchitecture.GUI.ZDropEdit DomesticTaxPenaltyTypeDropEdit;
		internal ZArchitecture.GUI.ZDropEdit PenaltyExemptReqDropEdit;
		internal ZArchitecture.GUI.ZDropEdit PenaltyExemptReasonCodeDropEdit;
		internal ZArchitecture.ZTextBox PenaltyExemptReasonMultiLineTextBox;
		internal ZArchitecture.ZCalcEdit PenaltyExemptSequenceCalcEdit;
		internal ZArchitecture.ZCalcEdit PenaltyExemptAmountCalcEdit;
		internal ZArchitecture.GUI.ZDropEdit RefundRequestYNDropEdit;
	}
}
