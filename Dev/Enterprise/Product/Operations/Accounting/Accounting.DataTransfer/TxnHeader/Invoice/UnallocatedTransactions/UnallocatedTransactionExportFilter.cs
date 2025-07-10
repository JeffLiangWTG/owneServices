using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.DataTransfer.Invoices
{
	public class UnallocatedTransactionExportFilter : TransactionExportFilter
	{
		public UnallocatedTransactionExportFilter(BusinessObjectFactory factory, TransactionExportFilterProvider filterProvider)
			: base(factory, filterProvider)
		{
		}

		protected override Type BusinessObjectTypeCore
		{
			get { return typeof(TransactionPendingAllocation); }
		}

		public override SchemaDateTimeColumn SystemLastEditTimeColumn
		{
			get { return AccTransactionHeaderSchema.AH_SystemLastEditTimeUtc;  }
		}

		protected override bool AtLeastOneTypeOfTransactionIsSelected
		{
			get { return FilterProvider.IncludeUnallocatedAPInvoices || FilterProvider.IncludeUnallocatedAPCreditNotes; }
		}

		protected override ZQuery CreateFilterForBatch()
		{
			ZQuery result = new ZQuery();
			result.DefaultJoinCondition = JoinCondition.Or;
			if (!ExportingNewBatch)
			{
				result.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.TransactionsPendingAllocation);
				result.AddToFilter(CreateDefaultQuery(), JoinCondition.And);
			}
			else
			{
				if (FilterProvider.IncludeUnallocatedAPCreditNotes)
				{
					result.AddToFilter(CreateNewQuery(LedgerTypes.TransactionsPendingAllocation, new StringCollectionX(TransactionTypes.CreditNotePendingAllocation)));
				}

				if (FilterProvider.IncludeUnallocatedAPInvoices)
				{
					result.AddToFilter(CreateNewQuery(LedgerTypes.TransactionsPendingAllocation, new StringCollectionX(TransactionTypes.InvoicePendingAllocation)));
				}
			}
			return result;
		}

		protected override void AddJobRelationshipSubQueryForInvoices(ZDBOnlyQuery mainQuery, ZString ledger)
		{
		}
	}
}
