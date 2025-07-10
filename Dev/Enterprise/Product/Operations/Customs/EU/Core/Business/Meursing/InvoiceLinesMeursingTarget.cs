using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.Business.Meursing
{
	public class InvoiceLinesMeursingTarget : IMeursingTarget
	{
		public InvoiceLinesMeursingTarget(IEnumerable<JobComInvoiceLine> invoiceLines)
		{
			this.invoiceLines = Argument.NotNull(invoiceLines, nameof(invoiceLines));
		}

		BusinessObjectFactory IMeursingTarget.Factory => invoiceLines.FirstOrDefault()?.Factory ?? new BusinessObjectFactory();

		void IMeursingTarget.SetMeursingResult(ZString meursingResult) => invoiceLines.ForEach(x => x.JI_SupplementaryCode1 = meursingResult);

		readonly IEnumerable<JobComInvoiceLine> invoiceLines;
	}
}
