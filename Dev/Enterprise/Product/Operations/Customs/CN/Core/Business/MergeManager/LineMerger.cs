using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CN.Business
{
	class LineMerger : Customs.Business.LineMerger
	{
		public LineMerger(BaseJobDeclaration declaration) : base(declaration)
		{
		}

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		protected override Customs.Business.EntryCreationStrategy[] GetEntryCreationStrategies()
		{
			var parentEntryHeaderIsCustomsEntry = Declaration.WillGenerateBothEntries ? Declaration.IsImport : Declaration.WillGenerateCustomsEntry;

			var parentEntryHeaderMessageType = GetEntryHeaderMessageType(parentEntryHeaderIsCustomsEntry);
			var childEntryHeaderMessageType = GetEntryHeaderMessageType(!parentEntryHeaderIsCustomsEntry);

			return new EntryCreationStrategy[]
			{
				new ParentEntryCreationStrategy(Declaration, parentEntryHeaderMessageType),
				new ChildEntryCreationStrategy(Declaration, childEntryHeaderMessageType)
			};
		}

		static ZString GetEntryHeaderMessageType(bool customsEntry)
		{
			return customsEntry ? EntryTypeList.Codes.CustomsEntry : EntryTypeList.Codes.RecordListing;
		}

		protected override void OnMerged()
		{
			base.OnMerged();

			foreach (CusEntryInstruction entryInstruction in Declaration.CustomsEntryInstructions)
			{
				entryInstruction.Validation.ValidateMaxCountOfEntryLines();
			}

			foreach (CusEntryHeader entryHeader in Declaration.ActiveEntryHeaders)
			{
				entryHeader.ClearCachedMergedData();
				entryHeader.MarkAsNeedingReloadHtmlFormatEntryData();
			}
		}

		protected override void PerformCountrySpecificOperationAfterMergeBeforeCalculateDuty()
		{
			base.PerformCountrySpecificOperationAfterMergeBeforeCalculateDuty();
			foreach (CusEntryHeader entryHeader in Declaration.ActiveEntryHeaders)
			{
				new CustomsValueCalculator(entryHeader).CalculateCustomsValue();
			}
		}

		protected override IDutyCalculatorStrategy GetNewDutyCalculatorStrategy() => new DutyCalculatorStrategy(Declaration);
	}
}
