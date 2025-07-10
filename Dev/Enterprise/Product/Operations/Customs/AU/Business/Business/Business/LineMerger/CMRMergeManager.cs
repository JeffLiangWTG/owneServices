using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRMergeManager : Customs.Business.MergeManager
	{
		public CMRMergeManager(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override ZString HumanReadableNameForMergeCore => "merge";

		protected override bool PersistsMergeState => false;

		protected override bool SupportsAmendments => true;

		protected override bool SupportsAutoMergeCore => true;

		protected override bool ShouldCheckExistenceOfInvoices => !Declaration.SubmitWeeklyNilReturnN30;

		protected override bool ShouldCheckExistenceOfInvoiceLineForAllInvoices => !Declaration.SubmitWeeklyNilReturnN30;

		protected override Customs.Business.LineMerger GetNewLineMergerCore() => new LineMerger(Declaration);

		protected override List<HasChangesHunterExclusionDetails> GetTypesWhichDoNotEffectMerge()
		{
			List<HasChangesHunterExclusionDetails> result = base.GetTypesWhichDoNotEffectMerge();
			result.Add(new HasChangesHunterExclusionDetails(typeof(CMRCusEntryCPDec)));
			result.Add(new HasChangesHunterExclusionDetails(typeof(AllEntryLineCPDecQuestion)));
			result.Add(new HasChangesHunterExclusionDetails(typeof(CMRCusEntryCPDecCollection)));
			result.Add(new HasChangesHunterExclusionDetails(typeof(JobDeclaration), false));//JobComInvoiceHeader/JobComInvoiceLine should be considered
			return result;
		}

		protected override bool RequiresMergeCore
		{
			get
			{
				bool result = false;
				if (Declaration != null && Declaration.CustomsEntryHeaders.Count > 0 && HasChangesSinceMergeHunter != null)
				{
					result = HasChangesSinceMergeHunter.HasChangesSinceLastMark || ChangesToJobDeclarationDoRequireMerge;
				}
				return result;
			}
		}

		protected bool ChangesToJobDeclarationDoRequireMerge => HasChangesSinceMergeHunter.BusinessObjectHasChangesSinceLastMark(Declaration) &&
																(Declaration.JE_MergeByInfo.HasChanges ||
																Declaration.JE_MessageTypeInfo.HasChanges ||
																Declaration.JE_MessageSubTypeInfo.HasChanges ||
																Declaration.JE_ExportDateInfo.HasChanges);

		protected override string GetAbnormalDataWarningMessage()
		{
			var result = base.GetAbnormalDataWarningMessage();
			if (string.IsNullOrEmpty(result))
			{
				result = GetDeferredDutyAndGSTStatusWarning();
			}
			return result;
		}

		string GetDeferredDutyAndGSTStatusWarning()
		{
			var result = new ZStringBuilder();

			var dutyWarning = Declaration.GetDeferredDutyStatusWarning();
			var gstWarning = Declaration.GetDeferredGSTStatusWarning();

			var dutyInconsistant = dutyWarning.Length > 0;
			var gstInconsistant = gstWarning.Length > 0;

			if (dutyInconsistant)
			{
				result.Append(dutyWarning);
			}
			if (gstInconsistant)
			{
				result.Append(gstWarning);
			}

			var combine = "";
			if (dutyInconsistant && gstInconsistant)
			{
				combine = "duty and GST";
			}
			else if (dutyInconsistant)
			{
				combine = "duty";
			}
			else if (gstInconsistant)
			{
				combine = "GST";
			}

			if (!string.IsNullOrEmpty(combine))
			{
				result.Append($"If you choose to continue then the {combine} deferred status of this job will change.");
				result.Append($"This will cause the entry print to show {combine} deferred information that is inconsistent with Customs,");
				result.Append($"and auto-rated {combine} details are likely to be incorrect.");
				result.Append($"It is recommended that you do not continue, but you correct the Importer organization {combine} setting,");
				result.Append("and then perform the Generate Entries (Merge) option from the brokerage menu.");
				result.Append("Do you wish to continue with the merge?");
			}
			return result.ToStringWithNewLineBetweenAppends();
		}

		protected override string GetReasonCannotMerge()
		{
			var reason = base.GetReasonCannotMerge();
			if (string.IsNullOrEmpty(reason) && Declaration.IsEXPDeclaration && Declaration.DeclarationExportCusEntryNumber != null)
			{
				reason = "Export merge functionality is only available for new declarations.";
			}

			return reason;
		}

		protected override bool ShouldPackingGroupsBeTypesThatAffectMerge => false;

		protected override bool ShouldContainersBeTypesThatAffectMerge => false;

		protected new JobDeclaration Declaration => (JobDeclaration)base.Declaration;
	}
}
