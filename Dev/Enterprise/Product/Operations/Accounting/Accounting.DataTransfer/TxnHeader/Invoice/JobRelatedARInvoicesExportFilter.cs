using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.DataTransfer.Invoices
{
	public class JobRelatedARInvoicesExportFilter : FinancialInvoiceTransactionExportFilter, IConsolOrShipmentTransExportFilter, Accounting.Integration.IJobRelatedARInvoicesExportFilter
	{
		public JobRelatedARInvoicesExportFilter(BusinessObjectFactory factory, TransactionExportFilterProvider filterProvider, ZString jobUniqueRef, bool isConsol)
			: base(factory, filterProvider)
		{
			this.FilterProvider.IncludeARInvoices = true;
			this.FilterProvider.IncludeARAdjustmentNotes = true;
			this.FilterProvider.IncludeARCreditNotes = true;
			this.FilterProvider.ExcludeNonJobRelatedTransactionsForAR = true;
			this.JobUniqueRef = jobUniqueRef;
			this.IsConsol = isConsol;
		}

		protected override ZQuery CreateFilterForBatch()
		{
			ZQuery query = new ZQuery();

			if (JobUniqueRef.IsEmpty)
			{
				query = ZQuery.NoResultQuery;
			}
			else
			{
				if (FilterProvider.AtLeastOneTypeOfARIsSelected)
				{
					query.AddToFilter(CreateNewARQuery());
				}

				if (IsConsol)
				{
					query.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_JH, null);
				}
				else
				{
					query.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_JH, SQLComparisonOperator.NotEqual, null);
				}
				query.AddToFilter(AccTransactionHeaderSchema.AH_JobNumber, JobUniqueRef);
				query.AddToFilter(AccTransactionHeaderSchema.AH_IsCancelled, false);
			}

			return query;
		}

		protected override void AddBatchNumberQuery(ZDBOnlyQuery dBOnlyQuery)
		{
			//don't require to check the batchnumber for job related invoice export
		}

		protected override bool AtLeastOneTypeOfTransactionIsSelected
		{
			get { return FilterProvider.AtLeastOneTypeOfARIsSelected; }
		}

		readonly ZString JobUniqueRef;
		readonly bool IsConsol;

		ZQuery IConsolOrShipmentTransExportFilter.Filter
		{
			get { return Filter; }
		}

		Type IConsolOrShipmentTransExportFilter.BusinessObjectType
		{
			get { return BusinessObjectType; }
		}
	}
}
