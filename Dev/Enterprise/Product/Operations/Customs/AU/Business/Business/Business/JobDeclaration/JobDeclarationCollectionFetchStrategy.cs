namespace Enterprise.Customs.AU.Declaration.Business
{
	public class JobDeclarationCollectionFetchStrategy : Customs.Business.FetchStrategies.BaseJobDeclarationCollectionFetchStrategy
	{
		public JobDeclarationCollectionFetchStrategy(JobDeclarationCollection collection)
			: base(collection)
		{
		}

		protected new JobDeclarationCollection Collection
		{
			get { return (JobDeclarationCollection)base.Collection; }
		}

		protected override bool IsCusDecHouseContainerPivotRelatedColumn(string columnName)
		{
			return base.IsCusDecHouseContainerPivotRelatedColumn(columnName) || columnName == JobDeclaration.Schema.ConsolidatedCargoStatusDescription;
		}

		protected override bool IsCusEntryHeaderRelatedColumn(string columnName)
		{
			return base.IsCusEntryHeaderRelatedColumn(columnName) || columnName == JobDeclaration.Schema.JE_EntryStatusDescription;
		}
	}
}
