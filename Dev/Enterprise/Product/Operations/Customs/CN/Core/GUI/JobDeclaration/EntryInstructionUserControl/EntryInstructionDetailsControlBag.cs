using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CN.GUI
{
	public sealed class EntryInstructionDetailsControlBag : ControlBag
	{
		public EntryInstructionDetailsControlBag()
		{
			DetailsSeparatorUserControl = RegisterControl(nameof(EntryInstructionDetailsUserControl.DetailsSeparatorUserControl));
			LevyTypeDropEdit = RegisterControl(nameof(EntryInstructionDetailsUserControl.LevyTypeDropEdit));
			RelatedEntrySeparatorUserControl = RegisterControl(nameof(EntryInstructionDetailsUserControl.RelatedEntrySeparatorUserControl));
			RelatedMRNTextBox = RegisterControl(nameof(EntryInstructionDetailsUserControl.RelatedMRNTextBox));
			RelatedManualNoTextBox = RegisterControl(nameof(EntryInstructionDetailsUserControl.RelatedManualNoTextBox));
			RemarksTextBox = RegisterControl(nameof(EntryInstructionDetailsUserControl.RemarksTextBox));
			ManualNoTextBox = RegisterControl(nameof(EntryInstructionDetailsUserControl.ManualNoTextBox));
			BillOfLadingAndDateUserControl = RegisterControl(nameof(EntryInstructionDetailsUserControl.BillOfLadingAndDateUserControl));
			PackagesAndTypeUserControl = RegisterControl(nameof(EntryInstructionDetailsUserControl.PackagesAndTypeUserControl));
			ParentInstructionGuidDropEdit = RegisterControl(nameof(EntryInstructionDetailsUserControl.ParentInstructionGuidDropEdit));
			DocumentSubmissionTypeDropEdit = RegisterControl(nameof(EntryInstructionDetailsUserControl.DocumentSubmissionTypeDropEdit));
			CIQDetailsSeparatorUserControl = RegisterControl(nameof(EntryInstructionDetailsUserControl.CIQDetailsSeparatorUserControl));
			SpecialBusinessIdentifiersUserControl = RegisterControl(nameof(EntryInstructionDetailsUserControl.SpecialBusinessIdentifiersUserControl));
			OperationMattersUserControl = RegisterControl(nameof(EntryInstructionDetailsUserControl.OperationMattersUserControl));
			OtherPackagesUserControl = RegisterControl(nameof(EntryInstructionDetailsUserControl.OtherPackagesUserControl));
			CIQRequiresCheckBox = RegisterControl(nameof(EntryInstructionDetailsUserControl.CIQRequiresCheckBox));
			TwoStageAccessApplicationSeparatorUserControl = RegisterControl(nameof(EntryInstructionDetailsUserControl.TwoStageAccessApplicationSeparatorUserControl));
			ApplyForTransitionCheckBox = RegisterControl(nameof(EntryInstructionDetailsUserControl.ApplyForTransitionCheckBox));
			TransitionSiteDropEdit = RegisterControl(nameof(EntryInstructionDetailsUserControl.TransitionSiteDropEdit));
			ApplyForConditionalPickupCheckBox = RegisterControl(nameof(EntryInstructionDetailsUserControl.ApplyForConditionalPickupCheckBox));
			ApplyForCombinedInspectionsCheckBox = RegisterControl(nameof(EntryInstructionDetailsUserControl.ApplyForCombinedInspectionsCheckBox));
			CIQRelatedReasonDropEdit = RegisterControl(nameof(EntryInstructionDetailsUserControl.CIQRelatedReasonDropEdit));
			CIQRelatedNumTextBox = RegisterControl(nameof(EntryInstructionDetailsUserControl.CIQRelatedNumTextBox));
			AttachmentsSeparatorUserControl = RegisterControl(nameof(EntryInstructionDetailsUserControl.AttachmentsSeparatorUserControl));
			AttachmentsUserControl = RegisterControl(nameof(EntryInstructionDetailsUserControl.AttachmentsUserControl));
			RequiredDocumentsSeparatorUserControl = RegisterControl(nameof(EntryInstructionDetailsUserControl.RequiredDocumentsSeparatorUserControl));
			RequiredDocumentsUserControl = RegisterControl(nameof(EntryInstructionDetailsUserControl.RequiredDocumentsUserControl));
			EnterpriseQualificationsSeparatorUserControl = RegisterControl(nameof(EntryInstructionDetailsUserControl.EnterpriseQualificationsSeparatorUserControl));
			EnterpriseQualificationsUserControl = RegisterControl(nameof(EntryInstructionDetailsUserControl.EnterpriseQualificationsUserControl));
		}

		public static EntryInstructionDetailsControlBag Instance => instance ??= new EntryInstructionDetailsControlBag();

		[ThreadStatic]
		static EntryInstructionDetailsControlBag instance;

		protected override Control CreateTemplate() => new EntryInstructionDetailsUserControl();

		public ControlReference DetailsSeparatorUserControl { get; }
		public ControlReference LevyTypeDropEdit { get; }
		public ControlReference RelatedEntrySeparatorUserControl { get; }
		public ControlReference RelatedMRNTextBox { get; }
		public ControlReference RelatedManualNoTextBox { get; }
		public ControlReference RemarksTextBox { get; }
		public ControlReference ManualNoTextBox { get; }
		public ControlReference BillOfLadingAndDateUserControl { get; }
		public ControlReference PackagesAndTypeUserControl { get; }
		public ControlReference ParentInstructionGuidDropEdit { get; }
		public ControlReference DocumentSubmissionTypeDropEdit { get; }
		public ControlReference CIQDetailsSeparatorUserControl { get; }
		public ControlReference SpecialBusinessIdentifiersUserControl { get; }
		public ControlReference OperationMattersUserControl { get; }
		public ControlReference OtherPackagesUserControl { get; }
		public ControlReference CIQRequiresCheckBox { get; }
		public ControlReference TwoStageAccessApplicationSeparatorUserControl { get; }
		public ControlReference ApplyForTransitionCheckBox { get; }
		public ControlReference TransitionSiteDropEdit { get; }
		public ControlReference ApplyForConditionalPickupCheckBox { get; }
		public ControlReference ApplyForCombinedInspectionsCheckBox { get; }
		public ControlReference CIQRelatedReasonDropEdit { get; }
		public ControlReference CIQRelatedNumTextBox { get; }
		public ControlReference AttachmentsSeparatorUserControl { get; }
		public ControlReference AttachmentsUserControl { get; }
		public ControlReference RequiredDocumentsSeparatorUserControl { get; }
		public ControlReference RequiredDocumentsUserControl { get; }
		public ControlReference EnterpriseQualificationsSeparatorUserControl { get; }
		public ControlReference EnterpriseQualificationsUserControl { get; }
	}
}
