namespace Enterprise.Customs.CA.Business.FetchStrategies
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

		protected override bool IsJobComInvoiceHeaderRelatedColumn(string columnName)
		{
			return base.IsJobComInvoiceHeaderRelatedColumn(columnName) || columnName == JobDeclaration.Schema.EstimatedPaymentDueDate;
		}

		protected override bool IsCusEntryLineRelatedColumn(string columnName)
		{
			return base.IsCusEntryLineRelatedColumn(columnName) ||
				columnName == JobDeclaration.Schema.EstimatedPaymentDueDate ||
				columnName == JobDeclaration.Schema.TotalCustomsValueInLocalCurrency;
		}

		protected override bool IsCusEntryHeaderEDIMessageRelatedColumn(string columnName)
		{
			return base.IsCusEntryHeaderEDIMessageRelatedColumn(columnName) ||
				columnName == JobDeclaration.Schema.DeclarationNumber ||
				columnName == JobDeclaration.Schema.JE_MessageStatusDescription;
		}

		protected override bool IsJobComInvoiceLineRelatedColumn(string columnName)
		{
			return base.IsJobComInvoiceLineRelatedColumn(columnName) ||
				columnName == JobDeclaration.Schema.TotalAmountPayable;
		}
	}
}
