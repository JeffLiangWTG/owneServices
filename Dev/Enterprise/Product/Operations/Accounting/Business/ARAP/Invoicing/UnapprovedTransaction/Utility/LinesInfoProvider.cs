using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class LinesInfoProvider
	{
		public virtual bool IsTaxOverridden(InvoicingLineBase line)
		{
			return linesWithTaxOverridden.Contains(line.PK);
		}

		public bool IsLocalAmountApplicable(InvoicingLineBase line)
		{
			return linesWithLocalAmountApplicable.Contains(line.PK);
		}

		public virtual void SetTaxOverridden(InvoicingLineBase line)
		{
			linesWithTaxOverridden.Add(line.PK);
		}

		public void SetLocalAmountApplicable(InvoicingLineBase line)
		{
			linesWithLocalAmountApplicable.Add(line.PK);
		}

		readonly HashSet<ZGuid> linesWithTaxOverridden = new HashSet<ZGuid>();
		readonly HashSet<ZGuid> linesWithLocalAmountApplicable = new HashSet<ZGuid>();
	}
}
