using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.DataTransfer.Invoices
{
	public class FinancialInvoiceTransactionExportFilter : TransactionExportFilter
	{
		public FinancialInvoiceTransactionExportFilter(BusinessObjectFactory factory, TransactionExportFilterProvider filterProvider) : base(factory, filterProvider)
		{
		}

		#region Implementation

		protected override Type BusinessObjectTypeCore
		{
			get
			{
				return typeof(InvoicingBase);
			}
		}

		public override SchemaDateTimeColumn SystemLastEditTimeColumn
		{
			get { return AccTransactionHeaderSchema.AH_SystemLastEditTimeUtc; }
		}

		#region Batch

		protected override ZQuery CreateFilterForBatch()
		{
			ZQuery result = new ZQuery();
			result.DefaultJoinCondition = JoinCondition.Or;

			if (!ExportingNewBatch)
			{
				result.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable);
				result.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable);
				result.AddToFilter(CreateDefaultQuery(), JoinCondition.And);
			}
			else
			{
				if (FilterProvider.AtLeastOneTypeOfARIsSelected)
				{
					result.AddToFilter(CreateNewARQuery());
				}

				if (FilterProvider.AtLeastOneTypeOfAPIsSelected)
				{
					result.AddToFilter(CreateNewAPQuery());
				}
				result.AddToFilter(new ZQuery(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK), JoinCondition.And);
			}
			return result;
		}

		#endregion

		protected override bool AtLeastOneTypeOfTransactionIsSelected
		{
			get { return FilterProvider.AtLeastOneTypeOfInvoiceIsSelected; }
		}

		protected internal ZDBOnlyQuery CreateNewARQuery()
		{
			StringCollectionX tranTypes = new StringCollectionX();
			if (FilterProvider.IncludeARInvoices)
			{
				tranTypes.Add(TransactionTypes.Invoice);
			}

			if (FilterProvider.IncludeARCreditNotes)
			{
				tranTypes.Add(TransactionTypes.CreditNote);
			}

			if (FilterProvider.IncludeARAdjustmentNotes)
			{
				tranTypes.Add(TransactionTypes.AdjustmentNote);
			}
			return CreateNewQuery(LedgerTypes.AccountsReceivable, tranTypes);
		}

		internal ZDBOnlyQuery CreateNewAPQuery()
		{
			StringCollectionX tranTypes = new StringCollectionX();
			if (FilterProvider.IncludeAPInvoices)
			{
				tranTypes.Add(TransactionTypes.Invoice);
			}

			if (FilterProvider.IncludeAPCreditNotes)
			{
				tranTypes.Add(TransactionTypes.CreditNote);
			}

			if (FilterProvider.IncludeAPAdjustmentNotes)
			{
				tranTypes.Add(TransactionTypes.AdjustmentNote);
			}
			return AddExcludeAPHeadersWithUCTLines(CreateNewQuery(LedgerTypes.AccountsPayable, tranTypes));
		}

		ZDBOnlyQuery AddExcludeAPHeadersWithUCTLines(ZDBOnlyQuery query)
		{
			ZSqlParameterCollection parameters = new ZSqlParameterCollection();
			parameters.Add("@UCTLineType", ZArchitecture.Core.TransactionLineTypes.UnapprovedCost, AccTransactionLinesSchema.AL_LineType);
			query.AddFilterAndZSQLParameterCollection("NOT EXISTS(SELECT AL_PK FROM dbo.AccTransactionLines WHERE AL_AH = AH_PK AND AL_LineType = @UCTLineType)", parameters);
			return query;
		}

		#endregion
	}
}
