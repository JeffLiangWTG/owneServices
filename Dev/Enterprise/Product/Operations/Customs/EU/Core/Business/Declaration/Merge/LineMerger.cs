using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class LineMerger : Customs.Business.LineMerger
	{
		public LineMerger(JobDeclaration declaration)
			: base(declaration)
		{
		}

		JobDeclaration EuJobDeclaration => (JobDeclaration)Declaration;

		protected override Customs.Business.EntryCreationStrategy[] GetEntryCreationStrategies()
		{
			var result = EuJobDeclaration.CreateEntryCreationStrategy();
			return new Customs.Business.EntryCreationStrategy[] { result };
		}

		protected override IDutyCalculatorStrategy GetNewDutyCalculatorStrategy() => new DutyCalculatorStrategy(EuJobDeclaration);

		protected override ILandedCostOnlyConfigurationProvider GetLandedCostOnlyConfigurationProvider() => new LandedCostOnlyConfigurationProviderEU();

		protected override void OnMerging()
		{
			base.OnMerging();
			foreach (CusEntryHeader entryHeader in Declaration.ActiveEntryHeaders)
			{
				foreach (CusEntryLine entryLine in entryHeader.MergedLines)
				{
					entryLine.ResetReadOnlySupportingDocuments();
				}
			}
		}

		protected override void OnMerged()
		{
			base.OnMerged();

			foreach (CusEntryInstruction entryInstruction in Declaration.CustomsEntryInstructions)
			{
				entryInstruction.Validation.AddNotAllowDeleteEntryLinesError();
			}
		}
	}
}
