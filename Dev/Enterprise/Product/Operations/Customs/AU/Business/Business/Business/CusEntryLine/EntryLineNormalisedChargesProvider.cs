using Enterprise.Integration.ZArchitecture;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class EntryLineNormalisedChargesProvider : ICommercialChargesProvider
	{
		public EntryLineNormalisedChargesProvider(CusEntryLine entryLine, ICurrency invoiceTotalCurrency)
		{
			this.EntryLine = entryLine;
			this.invoiceTotalCurrency = invoiceTotalCurrency;
		}

		#region Implementation

		protected readonly CusEntryLine EntryLine;
		readonly ICurrency invoiceTotalCurrency;

		#endregion

		#region ICommercialChargesProvider Members

		public string ITOTIncoTerm
		{
			get { return Core.Constants.IncoTerms.FreeOnBoard; }
		}

		public Money InvoiceTotal
		{
			get
			{
				Money result = Money.Empty;
				foreach (JobComInvoiceLine invoiceLine in EntryLine.InvoiceLines)
				{
					result = EntryLine.CurrencyConverter.Add(result, invoiceLine.JI_FOB);
				}
				return EntryLine.CurrencyConverter.ConvertExact(result, invoiceTotalCurrency);
			}
		}

		public Money Commission
		{
			get { return Money.Empty; }
		}

		public Money Discount
		{
			get { return Money.Empty; }
		}

		public Money OtherCharges1
		{
			get { return Money.Empty; }
		}

		public Money OtherCharges2
		{
			get { return Money.Empty; }
		}

		public Money LandingCharges
		{
			get { return Money.Empty; }
		}

		public Money PackingCosts
		{
			get { return Money.Empty; }
		}

		public Money ForeignInlandFreight
		{
			get { return Money.Empty; }
		}

		#endregion
	}
}
