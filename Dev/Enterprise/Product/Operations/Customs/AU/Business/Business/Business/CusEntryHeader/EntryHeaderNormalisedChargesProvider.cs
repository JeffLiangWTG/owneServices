using Enterprise.Integration.ZArchitecture;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	/// <summary>
	/// Normalise invoices with different incoterms and currencies and report them as FOB invoices in AUD.
	/// </summary>
	public class EntryHeaderNormalisedChargesProvider : ICommercialChargesProvider
	{
		public EntryHeaderNormalisedChargesProvider(CusEntryHeader entryHeader, ICurrency invoiceTotalCurrency)
		{
			this.EntryHeader = entryHeader;
			this.invoiceTotalCurrency = invoiceTotalCurrency;
		}

		readonly ICurrency invoiceTotalCurrency;

		public string ITOTIncoTerm
		{
			get
			{
				string result = "";
				if (!EntryHeader.IsCMRNature30)
				{
					result = Core.Constants.IncoTerms.FreeOnBoard;
				}
				return result;
			}
		}

		#region Normalised

		public Money InvoiceTotal
		{
			get
			{
				Money result = Money.Empty;

				foreach (CusEntryLine entryLine in EntryHeader.MergedLines)
				{
					result = EntryHeader.CurrencyConverter.Add(result, entryLine.ChargesProvider.InvoiceTotal);
				}
				return EntryHeader.CurrencyConverter.ConvertExact(result, invoiceTotalCurrency);
			}
		}

		public Money Commission
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

		public Money OtherCosts
		{
			get { return Money.Empty; }
		}

		public Money LandingCharges
		{
			get { return Money.Empty; }
		}

		public Money Discount
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

		#region Implementation

		protected readonly CusEntryHeader EntryHeader;

		#endregion
	}
}
