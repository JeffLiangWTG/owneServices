
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.DataTransfer;
using Enterprise.Accounting.DataTransfer.Invoices;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.DFD.Export
{
	public class DFDFinancialInvoiceTransactionExportFilter : FinancialInvoiceTransactionExportFilter
	{
		public DFDFinancialInvoiceTransactionExportFilter(BusinessObjectFactory factory, TransactionExportFilterProvider filterProvider)
			: base(factory, filterProvider)
		{
		}

		protected override void AddAdditionalFilter(ZDBOnlyQuery dBOnlyQuery)
		{
			if (ExportingNewBatch)
			{
				ZSqlParameterCollection @params = new ZSqlParameterCollection();

				string sQL = InvoicingBase.Schema.PK + " NOT IN (SELECT " + StmALogSchema.Constants.SL_Parent + " FROM " + StmALogSchema.Constants.SqlSchemaName + "." + StmALogSchema.Constants.TableName + "  WHERE " + StmALogSchema.Constants.SL_SE_NKEvent + " = @event AND " + StmALogSchema.Constants.SL_Reference + " like @reference AND " + StmALogSchema.Constants.SL_IsCancelled + " != @false )";

				@params.Add("@event", Events.DataExport.Code, StmALogSchema.SL_SE_NKEvent);
				@params.Add("@reference", DFDConstants.Export.DEXEventReference + "%", StmALogSchema.SL_Reference);
				@params.Add("@false", true, StmALogSchema.SL_IsCancelled);
				dBOnlyQuery.AddFilterAndZSQLParameterCollection(sQL, @params);
			}
			else
			{
				base.AddAdditionalFilter(dBOnlyQuery);
			}
		}

		protected override void AddBatchNumberQuery(ZDBOnlyQuery dBOnlyQuery)
		{
			if (!ExportingNewBatch)
			{
				base.AddBatchNumberQuery(dBOnlyQuery);
			}
		}
	}
}
