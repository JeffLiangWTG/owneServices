using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.FetchStrategies
{
	public class InvoicingLineBaseFetchStrategy : DependentTransactionLineFetchStrategy
	{
		public InvoicingLineBaseFetchStrategy(InvoicingLineBase invoiceLine)
			: base(invoiceLine)
		{
		}

		InvoicingLineBase InvoiceLine
		{
			get { return BusinessObject as InvoicingLineBase; }
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(typeof(JobHeader), InvoiceLine.AL_JH);
			Factory.AddFetchHint(JobChargeSchema.JR_AL_APLine, InvoiceLine.PK);
			Factory.AddFetchHint(JobChargeSchema.JR_JH, InvoiceLine.AL_JH);

			if (!Factory.HasContext(Enterprise.Integration.Accounting.BusinessContext.CalculateCommissionOnJobClosure))
			{
				// This fetch hint should include AL_GC, but the query it generates can cause SQL Server to choose a poor index.
				// When partitioning is added, use AddFetchHint(Type businessObjectType, ZQuery mainQuery, ZQuery secondQuery)
				Factory.AddFetchHint(AccTransactionLinesSchema.AL_JH, InvoiceLine.AL_JH);
			}
		}
	}
}