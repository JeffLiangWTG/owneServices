using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.GUI.Testing
{
	[TestedType(typeof(EntryInstructionDetailsControlBag))]
	sealed class EntryInstructionDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(EntryInstructionDetailsControlBag.DetailsSeparatorUserControl);
				yield return nameof(EntryInstructionDetailsControlBag.LevyTypeDropEdit);
				yield return nameof(EntryInstructionDetailsControlBag.RelatedEntrySeparatorUserControl);
				yield return nameof(EntryInstructionDetailsControlBag.RelatedMRNTextBox);
				yield return nameof(EntryInstructionDetailsControlBag.RelatedManualNoTextBox);
				yield return nameof(EntryInstructionDetailsControlBag.RemarksTextBox);
				yield return nameof(EntryInstructionDetailsControlBag.ManualNoTextBox);
				yield return nameof(EntryInstructionDetailsControlBag.BillOfLadingAndDateUserControl);
				yield return nameof(EntryInstructionDetailsControlBag.PackagesAndTypeUserControl);
				yield return nameof(EntryInstructionDetailsControlBag.ParentInstructionGuidDropEdit);
				yield return nameof(EntryInstructionDetailsControlBag.DocumentSubmissionTypeDropEdit);
				yield return nameof(EntryInstructionDetailsControlBag.CIQDetailsSeparatorUserControl);
				yield return nameof(EntryInstructionDetailsControlBag.SpecialBusinessIdentifiersUserControl);
				yield return nameof(EntryInstructionDetailsControlBag.OperationMattersUserControl);
				yield return nameof(EntryInstructionDetailsControlBag.OtherPackagesUserControl);
				yield return nameof(EntryInstructionDetailsControlBag.CIQRequiresCheckBox);
				yield return nameof(EntryInstructionDetailsControlBag.TwoStageAccessApplicationSeparatorUserControl);
				yield return nameof(EntryInstructionDetailsControlBag.ApplyForTransitionCheckBox);
				yield return nameof(EntryInstructionDetailsControlBag.TransitionSiteDropEdit);
				yield return nameof(EntryInstructionDetailsControlBag.ApplyForConditionalPickupCheckBox);
				yield return nameof(EntryInstructionDetailsControlBag.ApplyForCombinedInspectionsCheckBox);
				yield return nameof(EntryInstructionDetailsControlBag.CIQRelatedReasonDropEdit);
				yield return nameof(EntryInstructionDetailsControlBag.CIQRelatedNumTextBox);
				yield return nameof(EntryInstructionDetailsControlBag.AttachmentsSeparatorUserControl);
				yield return nameof(EntryInstructionDetailsControlBag.AttachmentsUserControl);
				yield return nameof(EntryInstructionDetailsControlBag.RequiredDocumentsSeparatorUserControl);
				yield return nameof(EntryInstructionDetailsControlBag.RequiredDocumentsUserControl);
				yield return nameof(EntryInstructionDetailsControlBag.EnterpriseQualificationsSeparatorUserControl);
				yield return nameof(EntryInstructionDetailsControlBag.EnterpriseQualificationsUserControl);
			}
		}

		protected override ControlBag GetControlBagForTesting() => EntryInstructionDetailsControlBag.Instance;
	}
}
