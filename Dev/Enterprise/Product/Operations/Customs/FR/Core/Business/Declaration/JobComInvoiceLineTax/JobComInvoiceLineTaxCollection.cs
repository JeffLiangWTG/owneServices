using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class JobComInvoiceLineTaxCollection : EU.Business.Declaration.JobComInvoiceLineTaxCollection
	{
		public JobComInvoiceLineTaxCollection(JobComInvoiceLine invoiceLine) : base(invoiceLine)
		{
		}

		public new JobComInvoiceLineTax this[int index] => (JobComInvoiceLineTax)Elements[index];

		public new JobComInvoiceLineTax AddNew()
		{
			return (JobComInvoiceLineTax)base.AddNew();
		}

		protected new JobComInvoiceLineTax AddNew(Type type)
		{
			return (JobComInvoiceLineTax)base.AddNew(type);
		}

		protected override BusinessObject AddNewCore()
		{
			return AddNew(typeof(JobComInvoiceLineTax));
		}
	}
}
