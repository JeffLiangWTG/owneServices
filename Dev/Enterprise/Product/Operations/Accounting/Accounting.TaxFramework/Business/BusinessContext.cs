namespace Enterprise.Accounting.TaxFramework.Business
{
	enum BusinessContext
	{
		SystemGeneratedTaxRecordCreationInProgress,
		CopyingPersistentValues,
		ReversingTaxAmounts,
		RestoringIncompleteTransaction,
	}

	static class BusinessContextSets
	{
		public static BusinessContext[] TaxRecordCreation => new[]
		{
			BusinessContext.SystemGeneratedTaxRecordCreationInProgress,
			BusinessContext.RestoringIncompleteTransaction,
		};

		public static BusinessContext[] SettingExactValuesToTaxRecordProperties => new[]
		{
			BusinessContext.CopyingPersistentValues,
			BusinessContext.ReversingTaxAmounts,
			BusinessContext.RestoringIncompleteTransaction,
		};
	}
}
