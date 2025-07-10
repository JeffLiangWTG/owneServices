using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class InvoiceHeaderActiveCollectionFetchStrategy : Customs.Business.FetchStrategies.InvoiceHeaderActiveCollectionFetchStrategy
	{
		public InvoiceHeaderActiveCollectionFetchStrategy(InvoiceHeaderActiveCollection collection)
			: base(collection)
		{
		}

		protected new InvoiceHeaderActiveCollection Collection => (InvoiceHeaderActiveCollection)base.Collection;

		protected override void FetchForValidateCore()
		{
			base.FetchForValidateCore();
			var factory = Collection.Factory;
			var invoiceLines = factory.Load<JobComInvoiceLine>(new ZQuery(JobComInvoiceLineSchema.JI_JZ, Collection.GetPKs())).ToList();
			invoiceLines.ForEach(x =>
			{
				factory.AddFetchHint(JobComInvoiceLineTaxSchema.JLT_JI, x.PK);
			});
		}
	}
}
