namespace Enterprise.Customs.BR.Business
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

		protected override bool IsCusEntryHeaderRelatedColumn(string columnName)
		{
			return base.IsCusEntryHeaderRelatedColumn(columnName)
				|| columnName == JobDeclaration.Schema.AdminstrativeStatus
				|| columnName == JobDeclaration.Schema.AdminstrativeStatusDescription
				|| columnName == JobDeclaration.Schema.CargoStatus
				|| columnName == JobDeclaration.Schema.CargoStatusDescription
				|| columnName == JobDeclaration.Schema.ClearanceDateAsString
				|| columnName == JobDeclaration.Schema.EntrySubmitDateAsString
				|| columnName == JobDeclaration.Schema.RiskChannel
				|| columnName == JobDeclaration.Schema.RiskChannelDescription;
		}

		protected override bool IsCusEntryNumRelatedColumn(string columnName)
		{
			return base.IsCusEntryNumRelatedColumn(columnName)
				|| columnName == JobDeclaration.Schema.EntryIssueDateAsString;
		}
	}
}
