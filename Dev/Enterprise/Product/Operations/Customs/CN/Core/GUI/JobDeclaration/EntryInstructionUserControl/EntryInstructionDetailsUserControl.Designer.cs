namespace Enterprise.Customs.CN.GUI
{
	public partial class EntryInstructionDetailsUserControl
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
			this.CIQRelatedNumTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CIQRelatedReasonDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CIQDetailsSeparatorUserControl = new Enterprise.ZArchitecture.GUI.SeparatorUserControl();
			this.SpecialBusinessIdentifiersUserControl = new Enterprise.Customs.CN.GUI.CodeDescriptionSelectionUserControl();
			this.CIQRequiresCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DetailsSeparatorUserControl = new Enterprise.ZArchitecture.GUI.SeparatorUserControl();
			this.OperationMattersUserControl = new Enterprise.Customs.CN.GUI.CodeDescriptionSelectionUserControl();
			this.DocumentSubmissionTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ParentInstructionGuidDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			this.LevyTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PackagesAndTypeUserControl = new Enterprise.Customs.CN.GUI.PackagesAndTypeUserControl();
			this.BillOfLadingAndDateUserControl = new BillOfLadingAndDateUserControl();
			this.ManualNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TwoStageAccessApplicationSeparatorUserControl = new Enterprise.ZArchitecture.GUI.SeparatorUserControl();
			this.ApplyForCombinedInspectionsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ApplyForConditionalPickupCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ApplyForTransitionCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.TransitionSiteDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RelatedEntrySeparatorUserControl = new Enterprise.ZArchitecture.GUI.SeparatorUserControl();
			this.OtherPackagesUserControl = new Enterprise.Customs.CN.GUI.CodeDescriptionSelectionUserControl();
			this.RelatedMRNTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RelatedManualNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RemarksTextBox = new Enterprise.Customs.GUI.LongTextControl();
			this.AttachmentsSeparatorUserControl = new Enterprise.ZArchitecture.GUI.SeparatorUserControl();
			this.AttachmentsUserControl = new Enterprise.Customs.CN.GUI.AttachmentsUserControl();
			this.RequiredDocumentsSeparatorUserControl = new Enterprise.ZArchitecture.GUI.SeparatorUserControl();
			this.RequiredDocumentsUserControl = new Enterprise.Customs.CN.GUI.RequiredDocumentsUserControl();
			this.EnterpriseQualificationsSeparatorUserControl = new Enterprise.ZArchitecture.GUI.SeparatorUserControl();
			this.EnterpriseQualificationsUserControl = new Enterprise.Customs.CN.GUI.EnterpriseQualificationsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CIQRelatedReasonDropEdit.SuspendLayout();
			this.CIQDetailsSeparatorUserControl.SuspendLayout();
			this.SpecialBusinessIdentifiersUserControl.SuspendLayout();
			this.DetailsSeparatorUserControl.SuspendLayout();
			this.OperationMattersUserControl.SuspendLayout();
			this.DocumentSubmissionTypeDropEdit.SuspendLayout();
			this.ParentInstructionGuidDropEdit.SuspendLayout();
			this.LevyTypeDropEdit.SuspendLayout();
			this.PackagesAndTypeUserControl.SuspendLayout();
			this.TwoStageAccessApplicationSeparatorUserControl.SuspendLayout();
			this.TransitionSiteDropEdit.SuspendLayout();
			this.RelatedEntrySeparatorUserControl.SuspendLayout();
			this.OtherPackagesUserControl.SuspendLayout();
			this.RemarksTextBox.SuspendLayout();
			this.AttachmentsSeparatorUserControl.SuspendLayout();
			this.AttachmentsUserControl.SuspendLayout();
			this.RequiredDocumentsSeparatorUserControl.SuspendLayout();
			this.RequiredDocumentsUserControl.SuspendLayout();
			this.EnterpriseQualificationsSeparatorUserControl.SuspendLayout();
			this.EnterpriseQualificationsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CN.Business.CusEntryInstruction);
			// 
			// CIQRelatedNumTextBox
			// 
			this.BindingSource.SetBindingMember(this.CIQRelatedNumTextBox, "CEI_CIQRelatedNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.CusEntryInstruction)(null)).CEI_CIQRelatedNum)));
			this.CIQRelatedNumTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(498, 212, true);
			this.CIQRelatedNumTextBox.Name = "CIQRelatedNumTextBox";
			this.CIQRelatedNumTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(218, 20, true);
			this.CIQRelatedNumTextBox.TabIndex = 1;
			// 
			// CIQRelatedReasonDropEdit
			// 
			this.CIQRelatedReasonDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CIQRelatedReasonDropEdit, "CEI_CIQRelatedReason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CN.Business.CusEntryInstruction)(null)).CEI_CIQRelatedReason)));
			this.CIQRelatedReasonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(498, 237, true);
			this.CIQRelatedReasonDropEdit.Name = "CIQRelatedReasonDropEdit";
			this.CIQRelatedReasonDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(218, 20, true);
			this.CIQRelatedReasonDropEdit.TabIndex = 2;
			// 
			// CIQDetailsSeparatorUserControl
			// 
			this.CIQDetailsSeparatorUserControl.AllowDrop = true;
			this.CIQDetailsSeparatorUserControl.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("f718b2e4-ceff-41ba-abce-a70389d732bb", "CIQ Details");
			this.CIQDetailsSeparatorUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(396, 191, true);
			this.CIQDetailsSeparatorUserControl.Name = "CIQDetailsSeparatorUserControl";
			this.CIQDetailsSeparatorUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 15, true);
			this.CIQDetailsSeparatorUserControl.TabIndex = 0;
			// 
			// SpecialBusinessIdentifiersUserControl
			// 
			this.SpecialBusinessIdentifiersUserControl.AllowDrop = true;
			this.SpecialBusinessIdentifiersUserControl.HideCodeOnSelectionForm = true;
			this.SpecialBusinessIdentifiersUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(382, 263, true);
			this.SpecialBusinessIdentifiersUserControl.Name = "SpecialBusinessIdentifiersUserControl";
			this.SpecialBusinessIdentifiersUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 20, true);
			this.SpecialBusinessIdentifiersUserControl.TabIndex = 0;
			// 
			// CIQRequiresCheckBox
			// 
			this.CIQRequiresCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.CIQRequiresCheckBox, "CEI_CIQRequires");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CN.Business.CusEntryInstruction)(null)).CEI_CIQRequires)));
			this.CIQRequiresCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(396, 170, true);
			this.CIQRequiresCheckBox.Name = "CIQRequiresCheckBox";
			this.CIQRequiresCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(89, 17, true);
			this.CIQRequiresCheckBox.TabIndex = 4;
			this.CIQRequiresCheckBox.UseVisualStyleBackColor = true;
			// 
			// DetailsSeparatorUserControl
			// 
			this.DetailsSeparatorUserControl.AllowDrop = true;
			this.DetailsSeparatorUserControl.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("E79FB1A9-E1A8-41C9-9FC4-F2159C8A42C7", "Details");
			this.DetailsSeparatorUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 6, true);
			this.DetailsSeparatorUserControl.Name = "DetailsSeparatorUserControl";
			this.DetailsSeparatorUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 15, true);
			this.DetailsSeparatorUserControl.TabIndex = 5;
			// 
			// OperationMattersUserControl
			// 
			this.OperationMattersUserControl.AllowDrop = true;
			this.OperationMattersUserControl.HideCodeOnSelectionForm = true;
			this.OperationMattersUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 226, true);
			this.OperationMattersUserControl.Name = "OperationMattersUserControl";
			this.OperationMattersUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(326, 20, true);
			this.OperationMattersUserControl.TabIndex = 11;
			// 
			// DocumentSubmissionTypeDropEdit
			// 
			this.DocumentSubmissionTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DocumentSubmissionTypeDropEdit, "CEI_DocumentSubmissionType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CN.Business.CusEntryInstruction)(null)).CEI_DocumentSubmissionType)));
			this.DocumentSubmissionTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 200, true);
			this.DocumentSubmissionTypeDropEdit.Name = "DocumentSubmissionTypeDropEdit";
			this.DocumentSubmissionTypeDropEdit.PreBoundMaxLength = 4;
			this.DocumentSubmissionTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(218, 20, true);
			this.DocumentSubmissionTypeDropEdit.TabIndex = 9;
			// 
			// ParentInstructionGuidDropEdit
			// 
			this.ParentInstructionGuidDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ParentInstructionGuidDropEdit, "CEI_CEI_Parent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CN.Business.CusEntryInstruction)(null)).CEI_CEI_Parent)));
			this.ParentInstructionGuidDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 75, true);
			this.ParentInstructionGuidDropEdit.Name = "ParentInstructionGuidDropEdit";
			this.ParentInstructionGuidDropEdit.PreBoundMaxLength = 4;
			this.ParentInstructionGuidDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(218, 20, true);
			this.ParentInstructionGuidDropEdit.TabIndex = 2;
			// 
			// LevyTypeDropEdit
			// 
			this.LevyTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LevyTypeDropEdit, "CEI_LevyType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CN.Business.CusEntryInstruction)(null)).CEI_LevyType)));
			this.LevyTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 150, true);
			this.LevyTypeDropEdit.Name = "LevyTypeDropEdit";
			this.LevyTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(218, 20, true);
			this.LevyTypeDropEdit.TabIndex = 6;
			// 
			// PackagesAndTypeUserControl
			// 
			this.PackagesAndTypeUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PackagesAndTypeUserControl, ".");
			this.PackagesAndTypeUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 174, true);
			this.PackagesAndTypeUserControl.Name = "PackagesAndTypeUserControl";
			this.PackagesAndTypeUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(339, 20, true);
			this.PackagesAndTypeUserControl.TabIndex = 7;
			// 
			// BillOfLadingAndDateUserControl
			// 
			this.BillOfLadingAndDateUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 100, true);
			this.BillOfLadingAndDateUserControl.Name = "BillOfLadingAndDateUserControl";
			this.BillOfLadingAndDateUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.BillOfLadingAndDateUserControl.TabIndex = 3;
			// 
			// ManualNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.ManualNoTextBox, "CEI_ManualNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.CusEntryInstruction)(null)).CEI_ManualNo)));
			this.ManualNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 125, true);
			this.ManualNoTextBox.Name = "ManualNoTextBox";
			this.ManualNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(218, 20, true);
			this.ManualNoTextBox.TabIndex = 5;
			// 
			// TwoStageAccessApplicationSeparatorUserControl
			// 
			this.TwoStageAccessApplicationSeparatorUserControl.AllowDrop = true;
			this.TwoStageAccessApplicationSeparatorUserControl.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("d550b95d-a26e-47f1-82ed-df8f5a4ddb6d", "Two-stage Access Application");
			this.TwoStageAccessApplicationSeparatorUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(396, 289, true);
			this.TwoStageAccessApplicationSeparatorUserControl.Name = "TwoStageAccessApplicationSeparatorUserControl";
			this.TwoStageAccessApplicationSeparatorUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 15, true);
			this.TwoStageAccessApplicationSeparatorUserControl.TabIndex = 6;
			// 
			// ApplyForCombinedInspectionsCheckBox
			// 
			this.ApplyForCombinedInspectionsCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ApplyForCombinedInspectionsCheckBox, "CNE_ApplyForCombinedInspections");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CN.Business.CusEntryInstruction)(null)).CNE_ApplyForCombinedInspections)));
			this.ApplyForCombinedInspectionsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(498, 311, true);
			this.ApplyForCombinedInspectionsCheckBox.Name = "ApplyForCombinedInspectionsCheckBox";
			this.ApplyForCombinedInspectionsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(292, 17, true);
			this.ApplyForCombinedInspectionsCheckBox.TabIndex = 5;
			this.ApplyForCombinedInspectionsCheckBox.UseVisualStyleBackColor = true;
			// 
			// ApplyForConditionalPickupCheckBox
			// 
			this.ApplyForConditionalPickupCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ApplyForConditionalPickupCheckBox, "CNE_ApplyForConditionalPickup");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CN.Business.CusEntryInstruction)(null)).CNE_ApplyForConditionalPickup)));
			this.ApplyForConditionalPickupCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(498, 331, true);
			this.ApplyForConditionalPickupCheckBox.Name = "ApplyForConditionalPickupCheckBox";
			this.ApplyForConditionalPickupCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 17, true);
			this.ApplyForConditionalPickupCheckBox.TabIndex = 4;
			this.ApplyForConditionalPickupCheckBox.UseVisualStyleBackColor = true;
			// 
			// ApplyForTransitionCheckBox
			// 
			this.ApplyForTransitionCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ApplyForTransitionCheckBox, "CNE_ApplyForTransition");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CN.Business.CusEntryInstruction)(null)).CNE_ApplyForTransition)));
			this.ApplyForTransitionCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(498, 351, true);
			this.ApplyForTransitionCheckBox.Name = "ApplyForTransitionCheckBox";
			this.ApplyForTransitionCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 17, true);
			this.ApplyForTransitionCheckBox.TabIndex = 2;
			this.ApplyForTransitionCheckBox.UseVisualStyleBackColor = true;
			// 
			// TransitionSiteDropEdit
			// 
			this.TransitionSiteDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransitionSiteDropEdit, "CNE_TransitionSite");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CN.Business.CusEntryInstruction)(null)).CNE_TransitionSite)));
			this.TransitionSiteDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(498, 374, true);
			this.TransitionSiteDropEdit.Name = "TransitionSiteDropEdit";
			this.TransitionSiteDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(218, 20, true);
			this.TransitionSiteDropEdit.TabIndex = 3;
			// 
			// RelatedEntrySeparatorUserControl
			// 
			this.RelatedEntrySeparatorUserControl.AllowDrop = true;
			this.RelatedEntrySeparatorUserControl.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("1DCD88C6-F3D4-47CB-9465-316EB2AB5094", "More Details");
			this.RelatedEntrySeparatorUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(396, 3, true);
			this.RelatedEntrySeparatorUserControl.Name = "RelatedEntrySeparatorUserControl";
			this.RelatedEntrySeparatorUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 15, true);
			this.RelatedEntrySeparatorUserControl.TabIndex = 8;
			// 
			// OtherPackagesUserControl
			// 
			this.OtherPackagesUserControl.AllowDrop = true;
			this.OtherPackagesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(396, 24, true);
			this.OtherPackagesUserControl.Name = "OtherPackagesUserControl";
			this.OtherPackagesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.OtherPackagesUserControl.TabIndex = 1;
			// 
			// RelatedMRNTextBox
			// 
			this.BindingSource.SetBindingMember(this.RelatedMRNTextBox, "CEI_RelatedMRN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.CusEntryInstruction)(null)).CEI_RelatedMRN)));
			this.RelatedMRNTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(498, 50, true);
			this.RelatedMRNTextBox.Name = "RelatedMRNTextBox";
			this.RelatedMRNTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(218, 20, true);
			this.RelatedMRNTextBox.TabIndex = 2;
			// 
			// RelatedManualNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.RelatedManualNoTextBox, "CEI_RelatedManualNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.CusEntryInstruction)(null)).CEI_RelatedManualNo)));
			this.RelatedManualNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(498, 74, true);
			this.RelatedManualNoTextBox.Name = "RelatedManualNoTextBox";
			this.RelatedManualNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(218, 20, true);
			this.RelatedManualNoTextBox.TabIndex = 3;
			// 
			// RemarksTextBox
			// 
			this.RemarksTextBox.AllowDrop = true;
			this.RemarksTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.RemarksTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(498, 98, true);
			this.RemarksTextBox.Name = "RemarksTextBox";
			this.RemarksTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(218, 20, true);
			this.RemarksTextBox.TabIndex = 5;
			// 
			// AttachmentsSeparatorUserControl
			// 
			this.AttachmentsSeparatorUserControl.AllowDrop = true;
			this.AttachmentsSeparatorUserControl.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("184590b7-2709-4083-9122-0cf17b85d191", "Attachments");
			this.AttachmentsSeparatorUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 263, true);
			this.AttachmentsSeparatorUserControl.Name = "AttachmentsSeparatorUserControl";
			this.AttachmentsSeparatorUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 15, true);
			this.AttachmentsSeparatorUserControl.TabIndex = 12;
			// 
			// AttachmentsUserControl
			// 
			this.AttachmentsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AttachmentsUserControl, ".");
			this.AttachmentsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 287, true);
			this.AttachmentsUserControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.AttachmentsUserControl.Name = "AttachmentsUserControl";
			this.AttachmentsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(339, 153, true);
			this.AttachmentsUserControl.TabIndex = 2;
			this.AttachmentsUserControl.TabStop = false;
			// 
			// RequiredDocumentsSeparatorUserControl
			// 
			this.RequiredDocumentsSeparatorUserControl.AllowDrop = true;
			this.RequiredDocumentsSeparatorUserControl.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("37a99024-341b-436b-b0e2-a88165136610", "Required Documents");
			this.RequiredDocumentsSeparatorUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(792, 251, true);
			this.RequiredDocumentsSeparatorUserControl.Name = "RequiredDocumentsSeparatorUserControl";
			this.RequiredDocumentsSeparatorUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 15, true);
			this.RequiredDocumentsSeparatorUserControl.TabIndex = 13;
			// 
			// RequiredDocumentsUserControl
			// 
			this.RequiredDocumentsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RequiredDocumentsUserControl, ".");
			this.RequiredDocumentsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(792, 275, true);
			this.RequiredDocumentsUserControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.RequiredDocumentsUserControl.Name = "RequiredDocumentsUserControl";
			this.RequiredDocumentsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(394, 153, true);
			this.RequiredDocumentsUserControl.TabIndex = 8;
			this.RequiredDocumentsUserControl.TabStop = false;
			// 
			// EnterpriseQualificationsSeparatorUserControl
			// 
			this.EnterpriseQualificationsSeparatorUserControl.AllowDrop = true;
			this.EnterpriseQualificationsSeparatorUserControl.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("62b4dd8c-cef4-4079-80b5-217cf517f759", "Enterprise Qualifications");
			this.EnterpriseQualificationsSeparatorUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(794, 3, true);
			this.EnterpriseQualificationsSeparatorUserControl.Name = "EnterpriseQualificationsSeparatorUserControl";
			this.EnterpriseQualificationsSeparatorUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 15, true);
			this.EnterpriseQualificationsSeparatorUserControl.TabIndex = 14;
			// 
			// EnterpriseQualificationsUserControl
			// 
			this.EnterpriseQualificationsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EnterpriseQualificationsUserControl, ".");
			this.EnterpriseQualificationsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(792, 24, true);
			this.EnterpriseQualificationsUserControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.EnterpriseQualificationsUserControl.Name = "EnterpriseQualificationsUserControl";
			this.EnterpriseQualificationsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(394, 212, true);
			this.EnterpriseQualificationsUserControl.TabIndex = 8;
			this.EnterpriseQualificationsUserControl.TabStop = false;
			// 
			// EntryInstructionDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CIQDetailsSeparatorUserControl);
			this.Controls.Add(this.CIQRequiresCheckBox);
			this.Controls.Add(this.DetailsSeparatorUserControl);
			this.Controls.Add(this.TwoStageAccessApplicationSeparatorUserControl);
			this.Controls.Add(this.RelatedEntrySeparatorUserControl);
			this.Controls.Add(this.OperationMattersUserControl);
			this.Controls.Add(this.DocumentSubmissionTypeDropEdit);
			this.Controls.Add(this.ParentInstructionGuidDropEdit);
			this.Controls.Add(this.LevyTypeDropEdit);
			this.Controls.Add(this.PackagesAndTypeUserControl);
			this.Controls.Add(this.BillOfLadingAndDateUserControl);
			this.Controls.Add(this.ManualNoTextBox);
			this.Controls.Add(this.ApplyForCombinedInspectionsCheckBox);
			this.Controls.Add(this.ApplyForConditionalPickupCheckBox);
			this.Controls.Add(this.ApplyForTransitionCheckBox);
			this.Controls.Add(this.TransitionSiteDropEdit);
			this.Controls.Add(this.OtherPackagesUserControl);
			this.Controls.Add(this.RelatedMRNTextBox);
			this.Controls.Add(this.RelatedManualNoTextBox);
			this.Controls.Add(this.RemarksTextBox);
			this.Controls.Add(this.AttachmentsSeparatorUserControl);
			this.Controls.Add(this.AttachmentsUserControl);
			this.Controls.Add(this.RequiredDocumentsSeparatorUserControl);
			this.Controls.Add(this.RequiredDocumentsUserControl);
			this.Controls.Add(this.EnterpriseQualificationsSeparatorUserControl);
			this.Controls.Add(this.EnterpriseQualificationsUserControl);
			this.Controls.Add(this.CIQRelatedNumTextBox);
			this.Controls.Add(this.CIQRelatedReasonDropEdit);
			this.Controls.Add(this.SpecialBusinessIdentifiersUserControl);
			this.Name = "EntryInstructionDetailsUserControl";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1198, 715, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CIQRelatedReasonDropEdit.ResumeLayout(true);
			this.CIQRelatedReasonDropEdit.PerformLayout();
			this.CIQDetailsSeparatorUserControl.ResumeLayout(true);
			this.CIQDetailsSeparatorUserControl.PerformLayout();
			this.SpecialBusinessIdentifiersUserControl.ResumeLayout(true);
			this.SpecialBusinessIdentifiersUserControl.PerformLayout();
			this.DetailsSeparatorUserControl.ResumeLayout(true);
			this.DetailsSeparatorUserControl.PerformLayout();
			this.OperationMattersUserControl.ResumeLayout(true);
			this.OperationMattersUserControl.PerformLayout();
			this.DocumentSubmissionTypeDropEdit.ResumeLayout(true);
			this.DocumentSubmissionTypeDropEdit.PerformLayout();
			this.ParentInstructionGuidDropEdit.ResumeLayout(true);
			this.ParentInstructionGuidDropEdit.PerformLayout();
			this.LevyTypeDropEdit.ResumeLayout(true);
			this.LevyTypeDropEdit.PerformLayout();
			this.PackagesAndTypeUserControl.ResumeLayout(true);
			this.PackagesAndTypeUserControl.PerformLayout();
			this.TwoStageAccessApplicationSeparatorUserControl.ResumeLayout(true);
			this.TwoStageAccessApplicationSeparatorUserControl.PerformLayout();
			this.TransitionSiteDropEdit.ResumeLayout(true);
			this.TransitionSiteDropEdit.PerformLayout();
			this.RelatedEntrySeparatorUserControl.ResumeLayout(true);
			this.RelatedEntrySeparatorUserControl.PerformLayout();
			this.OtherPackagesUserControl.ResumeLayout(true);
			this.OtherPackagesUserControl.PerformLayout();
			this.RemarksTextBox.ResumeLayout(true);
			this.RemarksTextBox.PerformLayout();
			this.AttachmentsSeparatorUserControl.ResumeLayout(true);
			this.AttachmentsSeparatorUserControl.PerformLayout();
			this.AttachmentsUserControl.ResumeLayout(true);
			this.AttachmentsUserControl.PerformLayout();
			this.RequiredDocumentsSeparatorUserControl.ResumeLayout(true);
			this.RequiredDocumentsSeparatorUserControl.PerformLayout();
			this.RequiredDocumentsUserControl.ResumeLayout(true);
			this.RequiredDocumentsUserControl.PerformLayout();
			this.EnterpriseQualificationsSeparatorUserControl.ResumeLayout(true);
			this.EnterpriseQualificationsSeparatorUserControl.PerformLayout();
			this.EnterpriseQualificationsUserControl.ResumeLayout(true);
			this.EnterpriseQualificationsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.SeparatorUserControl DetailsSeparatorUserControl;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit LevyTypeDropEdit;
		internal Enterprise.ZArchitecture.GUI.SeparatorUserControl RelatedEntrySeparatorUserControl;
		internal Enterprise.ZArchitecture.ZTextBox RelatedMRNTextBox;
		internal Enterprise.ZArchitecture.ZTextBox RelatedManualNoTextBox;
		internal Enterprise.Customs.GUI.LongTextControl RemarksTextBox;
		internal Enterprise.ZArchitecture.ZTextBox ManualNoTextBox;
		internal BillOfLadingAndDateUserControl BillOfLadingAndDateUserControl;
		internal PackagesAndTypeUserControl PackagesAndTypeUserControl;
		internal ZArchitecture.GUI.ZGuidDropEdit ParentInstructionGuidDropEdit;
		internal ZArchitecture.GUI.ZDropEdit DocumentSubmissionTypeDropEdit;
		internal Enterprise.ZArchitecture.GUI.SeparatorUserControl CIQDetailsSeparatorUserControl;
		internal CodeDescriptionSelectionUserControl SpecialBusinessIdentifiersUserControl;
		internal CodeDescriptionSelectionUserControl OperationMattersUserControl;
		internal CodeDescriptionSelectionUserControl OtherPackagesUserControl;
		internal ZArchitecture.GUI.ZCheckBox CIQRequiresCheckBox;
		internal Enterprise.ZArchitecture.GUI.SeparatorUserControl TwoStageAccessApplicationSeparatorUserControl;
		internal ZArchitecture.GUI.ZCheckBox ApplyForTransitionCheckBox;
		internal ZArchitecture.GUI.ZDropEdit TransitionSiteDropEdit;
		internal ZArchitecture.GUI.ZCheckBox ApplyForConditionalPickupCheckBox;
		internal ZArchitecture.GUI.ZCheckBox ApplyForCombinedInspectionsCheckBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit CIQRelatedReasonDropEdit;
		internal Enterprise.ZArchitecture.ZTextBox CIQRelatedNumTextBox;
		internal Enterprise.ZArchitecture.GUI.SeparatorUserControl AttachmentsSeparatorUserControl;
		internal AttachmentsUserControl AttachmentsUserControl;
		internal Enterprise.ZArchitecture.GUI.SeparatorUserControl RequiredDocumentsSeparatorUserControl;
		internal RequiredDocumentsUserControl RequiredDocumentsUserControl;
		internal Enterprise.ZArchitecture.GUI.SeparatorUserControl EnterpriseQualificationsSeparatorUserControl;
		internal EnterpriseQualificationsUserControl EnterpriseQualificationsUserControl;
	}
}
