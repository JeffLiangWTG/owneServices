//CW1123:Do Not Use Cached Value Analyzer

using CargoWise.EntityFramework;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1123
	{
		public decimal InvoiceAmount
		{
			get
			{
				if (invoiceAmountCached == null)
				{
					invoiceAmountCached = new CachedValue<decimal>(GetInvoiceAmount);
				}
				return invoiceAmountCached.Value;
			}
		}

		CachedValue<decimal> invoiceAmountCached;

		decimal GetInvoiceAmount() { return 100; }
	}
}
