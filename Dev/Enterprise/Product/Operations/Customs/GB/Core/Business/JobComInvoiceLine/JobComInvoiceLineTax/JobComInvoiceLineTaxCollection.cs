using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.Business
{
	public class JobComInvoiceLineTaxCollection : EU.Business.Declaration.JobComInvoiceLineTaxCollection
	{
		public JobComInvoiceLineTaxCollection(JobComInvoiceLine master) : base(master)
		{
		}

		public new JobComInvoiceLineTax this[int index] => (JobComInvoiceLineTax)Elements[index];

		public new JobComInvoiceLineTax AddNew()
		{
			return (JobComInvoiceLineTax)base.AddNew();
		}

		public new JobComInvoiceLineTax AddNew(Type type)
		{
			return (JobComInvoiceLineTax)base.AddNew(type);
		}

		protected override BusinessObject AddNewCore()
		{
			return AddNew(typeof(JobComInvoiceLineTax));
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var invTax = (JobComInvoiceLineTax)child;
			using (invTax.SuspendSettingHasChanges())
			{
				invTax.Declaration?.ApplicationExtender?.SetDefaultValueForInvoiceLineTax(invTax.Data);
			}
		}
	}
}
