using System.Collections.Generic;
using Enterprise.Customs.CN.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.GUI.Testing
{
	[TestedType(typeof(EntryInstructionDetailsLayout))]
	sealed class EntryInstructionDetailsLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 2;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new EntryInstructionDetailsLayoutBuilder();

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
				yield return ThirdColumnControls;
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (EntryInstructionDetailsControlBag.Instance.DetailsSeparatorUserControl, ControlWidthClass.LongNoCaption);
				yield return (EntryInstructionBasicDetailsControlBag.Instance.StyleDropEdit, ControlWidthClass.Long);
				yield return (EntryInstructionBasicDetailsControlBag.Instance.DescriptionTextBox, ControlWidthClass.Long);
				yield return (EntryInstructionDetailsControlBag.Instance.ParentInstructionGuidDropEdit, ControlWidthClass.Long);
				yield return (EntryInstructionDetailsControlBag.Instance.BillOfLadingAndDateUserControl, ControlWidthClass.Long);
				yield return (EntryInstructionDetailsControlBag.Instance.ManualNoTextBox, ControlWidthClass.Long);
				yield return (EntryInstructionDetailsControlBag.Instance.LevyTypeDropEdit, ControlWidthClass.Long);
				yield return (EntryInstructionDetailsControlBag.Instance.PackagesAndTypeUserControl, ControlWidthClass.Long);
				yield return (EntryInstructionDetailsControlBag.Instance.DocumentSubmissionTypeDropEdit, ControlWidthClass.Long);
				yield return (EntryInstructionDetailsControlBag.Instance.OperationMattersUserControl, ControlWidthClass.Long);
				yield return (EntryInstructionBasicDetailsControlBag.Instance.SubStyleDropEdit, ControlWidthClass.Long);
				yield return (EntryInstructionDetailsControlBag.Instance.AttachmentsSeparatorUserControl, ControlWidthClass.LongNoCaption);
				yield return (EntryInstructionDetailsControlBag.Instance.AttachmentsUserControl, ControlWidthClass.LongNoCaption);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (EntryInstructionDetailsControlBag.Instance.RelatedEntrySeparatorUserControl, ControlWidthClass.LongNoCaption);
				yield return (EntryInstructionDetailsControlBag.Instance.OtherPackagesUserControl, ControlWidthClass.Long);
				yield return (EntryInstructionDetailsControlBag.Instance.RelatedMRNTextBox, ControlWidthClass.Long);
				yield return (EntryInstructionDetailsControlBag.Instance.RelatedManualNoTextBox, ControlWidthClass.Long);
				yield return (EntryInstructionDetailsControlBag.Instance.RemarksTextBox, ControlWidthClass.Long);
				yield return (EntryInstructionDetailsControlBag.Instance.CIQRequiresCheckBox, ControlWidthClass.LongNoCaption);
				yield return (EntryInstructionDetailsControlBag.Instance.CIQDetailsSeparatorUserControl, ControlWidthClass.LongNoCaption);
				yield return (EntryInstructionDetailsControlBag.Instance.CIQRelatedNumTextBox, ControlWidthClass.Long);
				yield return (EntryInstructionDetailsControlBag.Instance.CIQRelatedReasonDropEdit, ControlWidthClass.Long);
				yield return (EntryInstructionDetailsControlBag.Instance.SpecialBusinessIdentifiersUserControl, ControlWidthClass.Long);
				yield return (EntryInstructionDetailsControlBag.Instance.TwoStageAccessApplicationSeparatorUserControl, ControlWidthClass.LongNoCaption);
				yield return (EntryInstructionDetailsControlBag.Instance.ApplyForTransitionCheckBox, ControlWidthClass.LongNoCaption);
				yield return (EntryInstructionDetailsControlBag.Instance.TransitionSiteDropEdit, ControlWidthClass.Long);
				yield return (EntryInstructionDetailsControlBag.Instance.ApplyForConditionalPickupCheckBox, ControlWidthClass.LongNoCaption);
				yield return (EntryInstructionDetailsControlBag.Instance.ApplyForCombinedInspectionsCheckBox, ControlWidthClass.LongNoCaption);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
		{
			get
			{
				yield return (EntryInstructionDetailsControlBag.Instance.EnterpriseQualificationsSeparatorUserControl, ControlWidthClass.LongNoCaption);
				yield return (EntryInstructionDetailsControlBag.Instance.EnterpriseQualificationsUserControl, ControlWidthClass.LongNoCaption);
				yield return (EntryInstructionDetailsControlBag.Instance.RequiredDocumentsSeparatorUserControl, ControlWidthClass.LongNoCaption);
				yield return (EntryInstructionDetailsControlBag.Instance.RequiredDocumentsUserControl, ControlWidthClass.LongNoCaption);
			}
		}

		public void TestParentInstructionGuidDropEdit()
		{
			CombineAssertions(() =>
			{
				Declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
				Assert("WillGenerateBothEntries=false", !Layout.IsVisible(EntryInstructionDetailsControlBag.Instance.ParentInstructionGuidDropEdit, EntryInstruction));
				Declaration.JE_MessageSubType = DecTypeList.Codes.RecordListing;
				Assert("WillGenerateBothEntries=false", !Layout.IsVisible(EntryInstructionDetailsControlBag.Instance.ParentInstructionGuidDropEdit, EntryInstruction));
				Declaration.JE_MessageSubType = DecTypeList.Codes.Both;
				Assert("WillGenerateBothEntries=true", Layout.IsVisible(EntryInstructionDetailsControlBag.Instance.ParentInstructionGuidDropEdit, EntryInstruction));
			});
		}

		public void TestCIQRequiresCheckBox()
		{
			CombineAssertions(() =>
			{
				Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
				Declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
				Assert("IMP+CUS", Layout.IsVisible(EntryInstructionDetailsControlBag.Instance.CIQRequiresCheckBox, EntryInstruction));
				Declaration.JE_MessageSubType = DecTypeList.Codes.RecordListing;
				Assert("IMP+REC", Layout.IsVisible(EntryInstructionDetailsControlBag.Instance.CIQRequiresCheckBox, EntryInstruction));

				Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
				Declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
				Assert("EXP+CUS", Layout.IsVisible(EntryInstructionDetailsControlBag.Instance.CIQRequiresCheckBox, EntryInstruction));
				Declaration.JE_MessageSubType = DecTypeList.Codes.RecordListing;
				Assert("EXP+REC", Layout.IsVisible(EntryInstructionDetailsControlBag.Instance.CIQRequiresCheckBox, EntryInstruction));

				Declaration.JE_MessageSubType = DecTypeList.Codes.Both;
				var instruction2 = Declaration.CustomsEntryInstructions.AddNew();
				Assert("BTH+Entering", !Layout.IsVisible(EntryInstructionDetailsControlBag.Instance.CIQRequiresCheckBox, EntryInstruction));
				Assert("BTH+Exiting", Layout.IsVisible(EntryInstructionDetailsControlBag.Instance.CIQRequiresCheckBox, instruction2));

				Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
				Assert("BTH+Exiting", Layout.IsVisible(EntryInstructionDetailsControlBag.Instance.CIQRequiresCheckBox, EntryInstruction));
				Assert("BTH+Entering", !Layout.IsVisible(EntryInstructionDetailsControlBag.Instance.CIQRequiresCheckBox, instruction2));
			});
		}

		public void TestCIQControls()
		{
			EntryInstruction.CEI_CIQRequires = false;
			AssertVisiblities("CEI_CIQRequires=false", expected: false);

			EntryInstruction.CEI_CIQRequires = true;
			AssertVisiblities("CEI_CIQRequires=true", expected: true);

			void AssertVisiblities(string message, bool expected)
			{
				CombineAssertions(message, () =>
				{
					AssertEquals("CIQDetailsSeparatorUserControl", expected, Layout.IsVisible(EntryInstructionDetailsControlBag.Instance.CIQDetailsSeparatorUserControl, EntryInstruction));
					AssertEquals("CIQRelatedNumTextBox", expected, Layout.IsVisible(EntryInstructionDetailsControlBag.Instance.CIQRelatedNumTextBox, EntryInstruction));
					AssertEquals("CIQRelatedReasonDropEdit", expected, Layout.IsVisible(EntryInstructionDetailsControlBag.Instance.CIQRelatedReasonDropEdit, EntryInstruction));
					AssertEquals("SpecialBusinessIdentifiersUserControl", expected, Layout.IsVisible(EntryInstructionDetailsControlBag.Instance.SpecialBusinessIdentifiersUserControl, EntryInstruction));
					AssertEquals("EnterpriseQualificationsSeparatorUserControl", expected, Layout.IsVisible(EntryInstructionDetailsControlBag.Instance.EnterpriseQualificationsSeparatorUserControl, EntryInstruction));
					AssertEquals("EnterpriseQualificationsUserControl", expected, Layout.IsVisible(EntryInstructionDetailsControlBag.Instance.EnterpriseQualificationsUserControl, EntryInstruction));
					AssertEquals("EnterpriseQualificationsSeparatorUserControl", expected, Layout.IsVisible(EntryInstructionDetailsControlBag.Instance.EnterpriseQualificationsSeparatorUserControl, EntryInstruction));
					AssertEquals("EnterpriseQualificationsUserControl", expected, Layout.IsVisible(EntryInstructionDetailsControlBag.Instance.EnterpriseQualificationsUserControl, EntryInstruction));
					AssertEquals("RequiredDocumentsSeparatorUserControl", expected, Layout.IsVisible(EntryInstructionDetailsControlBag.Instance.RequiredDocumentsSeparatorUserControl, EntryInstruction));
					AssertEquals("RequiredDocumentsUserControl", expected, Layout.IsVisible(EntryInstructionDetailsControlBag.Instance.RequiredDocumentsUserControl, EntryInstruction));
				});
			}
		}

		public void TestTwoStageAccessApplicationControls()
		{
			EntryInstruction.CEI_CIQRequires = true;
			Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			AssertVisiblities("IMP+CUS", expected: true);
			Declaration.JE_MessageSubType = DecTypeList.Codes.RecordListing;
			AssertVisiblities("IMP+REC", expected: true);

			Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			AssertVisiblities("EXP+CUS", expected: false);
			Declaration.JE_MessageSubType = DecTypeList.Codes.RecordListing;
			AssertVisiblities("EXP+REC", expected: false);

			void AssertVisiblities(string message, bool expected)
			{
				CombineAssertions(message, () =>
				{
					AssertEquals("TwoStageAccessApplicationSeparatorUserControl", expected, Layout.IsVisible(EntryInstructionDetailsControlBag.Instance.TwoStageAccessApplicationSeparatorUserControl, EntryInstruction));
					AssertEquals("ApplyForTransitionCheckBox", expected, Layout.IsVisible(EntryInstructionDetailsControlBag.Instance.ApplyForTransitionCheckBox, EntryInstruction));
					AssertEquals("ApplyForConditionalPickupCheckBox", expected, Layout.IsVisible(EntryInstructionDetailsControlBag.Instance.ApplyForConditionalPickupCheckBox, EntryInstruction));
					AssertEquals("ApplyForCombinedInspectionsCheckBox", expected, Layout.IsVisible(EntryInstructionDetailsControlBag.Instance.ApplyForCombinedInspectionsCheckBox, EntryInstruction));
					AssertEquals("TransitionSiteDropEdit", expected, Layout.IsVisible(EntryInstructionDetailsControlBag.Instance.TransitionSiteDropEdit, EntryInstruction));
				});
			}
		}

		PanelLayout Layout => layout ??= new EntryInstructionDetailsLayout().Layout;
		PanelLayout layout;

		CusEntryInstruction EntryInstruction => entryInstruction ??= Declaration.CustomsEntryInstructions.AddNew();
		CusEntryInstruction entryInstruction;

		JobDeclaration Declaration => declaration ??= Factory.New<JobDeclaration>();
		JobDeclaration declaration;
	}
}
