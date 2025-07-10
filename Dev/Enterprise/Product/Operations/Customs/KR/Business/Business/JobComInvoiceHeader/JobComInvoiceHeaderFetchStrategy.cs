using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business
{
	public class JobComInvoiceHeaderFetchStrategy : Customs.Business.FetchStrategies.BaseJobComInvoiceHeaderFetchStrategy
	{
		public JobComInvoiceHeaderFetchStrategy(JobComInvoiceHeader invoiceHeader) : base(invoiceHeader)
		{
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);
			foreach (var column in columns)
			{
				var columnName = column.ColumnName;
				switch (columnName)
				{
					case nameof(JobComInvoiceHeader.ContractDate):
					case nameof(JobComInvoiceHeader.ContractNumber):
					case nameof(JobComInvoiceHeader.PurchaseOrderDate):
					case nameof(JobComInvoiceHeader.PurchaseOrderNumber):
					case nameof(JobComInvoiceHeader.ValuationQuestion5B):
					case nameof(JobComInvoiceHeader.ValuationQuestion5C):
					case nameof(JobComInvoiceHeader.ValuationQuestion5D):
					case nameof(JobComInvoiceHeader.ValuationQuestion5EA):
					case nameof(JobComInvoiceHeader.ValuationQuestion5EB):
						Factory.AddFetchHint(CusCodeDataSchema.CY_ParentID, BusinessObject.PK);
						break;
				}
			}
		}
	}
}
