using System.Collections.Generic;
using Enterprise.ZArchitecture.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class MergeManager : Customs.Business.MergeManager
	{
		public MergeManager(JobDeclaration declaration)
			: base(declaration)
		{
		}

		#region Implementation
		protected new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
		}

		protected override Customs.Business.LineMerger GetNewLineMergerCore()
		{
			return new LineMerger(Declaration);
		}

		protected override List<HasChangesHunterExclusionDetails> GetTypesWhichDoNotEffectMerge()
		{
			List<HasChangesHunterExclusionDetails> result = base.GetTypesWhichDoNotEffectMerge();

			result.Add(new HasChangesHunterExclusionDetails(typeof(AddInfoCusEntryHeader)));
			result.Add(new HasChangesHunterExclusionDetails(typeof(AddInfoCusEntryLine)));

			return result;
		}

		protected override bool RequiresMergeCore
		{
			get
			{
				var requiresMerge = false;
				if (Declaration is JobDeclaration declaration && !declaration.IsThrowingAwayMerge)
				{
					requiresMerge = base.RequiresMergeCore || (!declaration.IsB2OrIM2OrB3X && declaration.FilteredInvoiceLines.Count > 0 && HasChangesSinceMergeHunter.HasChangesSinceLastMark);
				}
				return requiresMerge;
			}
		}

		protected override string GetAbnormalDataWarningMessage()
		{
			var result = base.GetAbnormalDataWarningMessage();

			if (string.IsNullOrEmpty(result))
			{
				if (!Declaration.IsCADEnabled && !Declaration.IsB2OrIM2OrB3X && Declaration.CA_K84AccountingDate.IsValid)
				{
					result = Res.GetString("6B6463AE-651C-4C57-8BD2-A74652FB6408", "Changes have been made which result in a merge being required, however this declaration has already been 'accounted for' (B3 accepted) and so these changes should not be made. Do you want to continue with this save?");
				}
			}
			return result;
		}
		#endregion Implementation
	}
}
