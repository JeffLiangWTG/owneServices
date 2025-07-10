using System;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class TaxOverriddenLinesInfoProvider : LinesInfoProvider
	{
		public override bool IsTaxOverridden(InvoicingLineBase line)
		{
			return true;
		}

		public override void SetTaxOverridden(InvoicingLineBase line)
		{
			throw new NotImplementedException("Does NOT support this feature.");
		}
	}
}
