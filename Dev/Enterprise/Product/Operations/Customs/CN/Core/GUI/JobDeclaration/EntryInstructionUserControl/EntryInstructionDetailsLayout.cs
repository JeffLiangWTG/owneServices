using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.CN.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CN.GUI
{
	public sealed class EntryInstructionDetailsLayout : IPanelLayoutProvider
	{
		PanelLayout EntryInstructionType { get; }

		public PanelLayout Layout => EntryInstructionType;

		public EntryInstructionDetailsLayout()
		{
			EntryInstructionType = CreateLayout();
		}

		static PanelLayout CreateLayout()
		{
			var builder = new EntryInstructionDetailsLayoutBuilder();
			var cnBag = builder.CNBag;
			var commonBag = builder.CommonBag;
			builder.AddControlBag(cnBag);

			builder.AddColumn();
			builder.Add(cnBag.DetailsSeparatorUserControl, widthClass: ControlWidthClass.LongNoCaption);
			builder.Add(commonBag.StyleDropEdit, widthClass: ControlWidthClass.Long);
			builder.Add(commonBag.DescriptionTextBox, widthClass: ControlWidthClass.Long);
			builder.Add(cnBag.ParentInstructionGuidDropEdit, widthClass: ControlWidthClass.Long);
			builder.Add(cnBag.BillOfLadingAndDateUserControl, widthClass: ControlWidthClass.Long);
			builder.Add(cnBag.ManualNoTextBox, widthClass: ControlWidthClass.Long);
			builder.Add(cnBag.LevyTypeDropEdit, widthClass: ControlWidthClass.Long);
			builder.Add(cnBag.PackagesAndTypeUserControl, widthClass: ControlWidthClass.Long);
			builder.Add(cnBag.DocumentSubmissionTypeDropEdit, widthClass: ControlWidthClass.Long);
			builder.Add(cnBag.OperationMattersUserControl, widthClass: ControlWidthClass.Long);
			builder.Add(commonBag.SubStyleDropEdit, widthClass: ControlWidthClass.Long);

			builder.Add(cnBag.AttachmentsSeparatorUserControl, widthClass: ControlWidthClass.LongNoCaption);
			builder.Add(cnBag.AttachmentsUserControl, widthClass: ControlWidthClass.LongNoCaption);

			builder.AddColumn();
			builder.Add(cnBag.RelatedEntrySeparatorUserControl, widthClass: ControlWidthClass.LongNoCaption);
			builder.Add(cnBag.OtherPackagesUserControl, widthClass: ControlWidthClass.Long);
			builder.Add(cnBag.RelatedMRNTextBox, widthClass: ControlWidthClass.Long);
			builder.Add(cnBag.RelatedManualNoTextBox, widthClass: ControlWidthClass.Long);
			builder.Add(cnBag.RemarksTextBox, widthClass: ControlWidthClass.Long);

			builder.Add(cnBag.CIQRequiresCheckBox, widthClass: ControlWidthClass.LongNoCaption);

			builder.Add(cnBag.CIQDetailsSeparatorUserControl, widthClass: ControlWidthClass.LongNoCaption);
			builder.Add(cnBag.CIQRelatedNumTextBox, widthClass: ControlWidthClass.Long);
			builder.Add(cnBag.CIQRelatedReasonDropEdit, widthClass: ControlWidthClass.Long);
			builder.Add(cnBag.SpecialBusinessIdentifiersUserControl, widthClass: ControlWidthClass.Long);

			builder.Add(cnBag.TwoStageAccessApplicationSeparatorUserControl, widthClass: ControlWidthClass.LongNoCaption);
			builder.Add(cnBag.ApplyForTransitionCheckBox, widthClass: ControlWidthClass.LongNoCaption);
			builder.Add(cnBag.TransitionSiteDropEdit, widthClass: ControlWidthClass.Long);
			builder.Add(cnBag.ApplyForConditionalPickupCheckBox, widthClass: ControlWidthClass.LongNoCaption);
			builder.Add(cnBag.ApplyForCombinedInspectionsCheckBox, widthClass: ControlWidthClass.LongNoCaption);

			builder.AddColumn();
			builder.Add(cnBag.EnterpriseQualificationsSeparatorUserControl, widthClass: ControlWidthClass.LongNoCaption);
			builder.Add(cnBag.EnterpriseQualificationsUserControl, widthClass: ControlWidthClass.LongNoCaption);
			builder.Add(cnBag.RequiredDocumentsSeparatorUserControl, widthClass: ControlWidthClass.LongNoCaption);
			builder.Add(cnBag.RequiredDocumentsUserControl, widthClass: ControlWidthClass.LongNoCaption);

			builder.SetVisibility(cnBag.ParentInstructionGuidDropEdit, x => x.JobDeclaration?.WillGenerateBothEntries ?? false, x => x.JobDeclaration?.JE_MessageSubTypeInfo);

			var entryTypeDependencies = new Func<CusEntryInstruction, ZPropertyInfo>[] { x => x.JobDeclaration?.JE_MessageTypeInfo, x => x.JobDeclaration?.JE_MessageSubTypeInfo, x => x.CEI_CEI_ParentInfo };
			builder.SetVisibility(cnBag.CIQRequiresCheckBox, x => x.IsCIQDataAllowed, entryTypeDependencies);

			builder.SetVisibility(cnBag.CIQDetailsSeparatorUserControl, x => x.CEI_CIQRequires, x => x.CEI_CIQRequiresInfo);
			builder.SetVisibility(cnBag.CIQRelatedNumTextBox, x => x.CEI_CIQRequires, x => x.CEI_CIQRequiresInfo);
			builder.SetVisibility(cnBag.CIQRelatedReasonDropEdit, x => x.CEI_CIQRequires, x => x.CEI_CIQRequiresInfo);
			builder.SetVisibility(cnBag.SpecialBusinessIdentifiersUserControl, x => x.CEI_CIQRequires, x => x.CEI_CIQRequiresInfo);
			builder.SetVisibility(cnBag.EnterpriseQualificationsSeparatorUserControl, x => x.CEI_CIQRequires, x => x.CEI_CIQRequiresInfo);
			builder.SetVisibility(cnBag.EnterpriseQualificationsUserControl, x => x.CEI_CIQRequires, x => x.CEI_CIQRequiresInfo);
			builder.SetVisibility(cnBag.EnterpriseQualificationsSeparatorUserControl, x => x.CEI_CIQRequires, x => x.CEI_CIQRequiresInfo);
			builder.SetVisibility(cnBag.EnterpriseQualificationsUserControl, x => x.CEI_CIQRequires, x => x.CEI_CIQRequiresInfo);
			builder.SetVisibility(cnBag.RequiredDocumentsSeparatorUserControl, x => x.CEI_CIQRequires, x => x.CEI_CIQRequiresInfo);
			builder.SetVisibility(cnBag.RequiredDocumentsUserControl, x => x.CEI_CIQRequires, x => x.CEI_CIQRequiresInfo);

			var twoStageDependencies = entryTypeDependencies.Append(x => x.CEI_CIQRequiresInfo).ToArray();
			builder.SetVisibility(cnBag.TwoStageAccessApplicationSeparatorUserControl, x => x.TwoStageAccessApplicable, twoStageDependencies);
			builder.SetVisibility(cnBag.ApplyForTransitionCheckBox, x => x.TwoStageAccessApplicable, twoStageDependencies);
			builder.SetVisibility(cnBag.ApplyForConditionalPickupCheckBox, x => x.TwoStageAccessApplicable, twoStageDependencies);
			builder.SetVisibility(cnBag.ApplyForCombinedInspectionsCheckBox, x => x.TwoStageAccessApplicable, twoStageDependencies);
			builder.SetVisibility(cnBag.TransitionSiteDropEdit, x => x.TwoStageAccessApplicable, twoStageDependencies);

			return builder.Build();
		}
	}
}
