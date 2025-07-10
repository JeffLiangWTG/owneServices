using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.BR.Business
{
	public class JobComInvoiceLineTaxCollection : DependentBusinessObjectCollection<JobComInvoiceLineTax, JobComInvoiceLine>, IEnumerable<BusinessObject>, IBusinessObjectCollection, IList, ICollection, IEnumerable
	{
		public JobComInvoiceLineTaxCollection(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
		}

		public IEnumerator<JobComInvoiceLineTax> GetEnumerator()
		{
			return Elements.Cast<JobComInvoiceLineTax>().GetEnumerator();
		}

		public JobComInvoiceLineTax AddNew(ZString type, string methodOfCalculation = null)
		{
			var tax = AddNew();
			tax.JLT_Type = type;
			if (methodOfCalculation != null)
			{
				tax.JLT_MethodOfCalculation = methodOfCalculation;
			}
			return tax;
		}

		public JobComInvoiceLineTax FindByType(ZString type)
		{
			return this.Cast<JobComInvoiceLineTax>().FirstOrDefault(x => x.JLT_Type == type);
		}

		public JobComInvoiceLineTax FindByTypeAndMethod(ZString type, ZString methodOfCalculation)
		{
			return this.Cast<JobComInvoiceLineTax>().FirstOrDefault(x => x.JLT_Type == type && x.JLT_MethodOfCalculation == methodOfCalculation);
		}

		public IEnumerable<JobComInvoiceLineTax> FindByMethodOfCalculation(ZString methodOfCalculation)
		{
			return this.Cast<JobComInvoiceLineTax>().Where(x => x.JLT_MethodOfCalculation == methodOfCalculation);
		}
	}
}
