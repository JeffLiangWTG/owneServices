using System.Linq;

namespace Enterprise.Customs.JP.Business
{
	public sealed class LineMerger : Customs.Business.LineMerger
	{
		public LineMerger(JobDeclaration declaration) : base(declaration)
		{
		}

		protected override void OnMerging()
		{
			if (Declaration.IsECRSendingInProgress)
			{
				var linkedEntryInstructionKeys = Declaration.ActiveEntryHeaders.Select(c => c.CH_CEI_Instruction).Distinct().ToArray();
				var entryInstructions = Declaration.CustomsEntryInstructions.Where(x => linkedEntryInstructionKeys.All(c => c != x.PK)).ToArray();

				foreach (var entryInstruction in entryInstructions)
				{
					var entryHeader = Declaration.ActiveEntryHeaders.AddNew();
					entryHeader.CH_CEI_Instruction = entryInstruction.PK;
				}
			}

			base.OnMerging();
		}

		protected override Customs.Business.EntryCreationStrategy[] GetEntryCreationStrategies() => new[] { new EntryCreationStrategy(Declaration) };

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;
	}
}
