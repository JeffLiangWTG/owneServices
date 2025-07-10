using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class EntryLineCommercialChargesProvider : ICommercialChargesProvider
	{
		public EntryLineCommercialChargesProvider(CusEntryLine entryLine)
		{
			this.EntryLine = entryLine;
			this.currencyConverter = entryLine.CurrencyConverter;
		}

		#region Implementation

		protected readonly CusEntryLine EntryLine;
		protected CurrencyConverter currencyConverter;

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
					result = currencyConverter.Add(result, invoiceLine.JI_LinePriceMoney);
				}
				return result;
			}
		}

		public Money Commission
		{
			get
			{
				Money result = Money.Empty;
				foreach (JobComInvoiceLine invoiceLine in EntryLine.InvoiceLines)
				{
					result = currencyConverter.Add(result, invoiceLine.JI_Commission);
				}
				return result;
			}
		}

		public Money Discount
		{
			get
			{
				Money result = Money.Empty;
				foreach (JobComInvoiceLine invoiceLine in EntryLine.InvoiceLines)
				{
					result = currencyConverter.Add(result, invoiceLine.JI_Discount);
				}
				return result;
			}
		}

		public Money OtherCharges1
		{
			get
			{
				Money result = Money.Empty;
				foreach (JobComInvoiceLine invoiceLine in EntryLine.InvoiceLines)
				{
					result = currencyConverter.Add(result, invoiceLine.JI_OtherCharges1);
				}
				return result;
			}
		}

		public Money OtherCharges2
		{
			get
			{
				Money result = Money.Empty;
				foreach (JobComInvoiceLine invoiceLine in EntryLine.InvoiceLines)
				{
					result = currencyConverter.Add(result, invoiceLine.JI_OtherCharges2);
				}
				return result;
			}
		}

		public Money LandingCharges
		{
			get
			{
				Money result = Money.Empty;
				foreach (JobComInvoiceLine invoiceLine in EntryLine.InvoiceLines)
				{
					result = currencyConverter.Add(result, invoiceLine.JI_LandingCharges);
				}
				return result;
			}
		}

		public Money PackingCosts
		{
			get
			{
				Money result = Money.Empty;
				foreach (JobComInvoiceLine invoiceLine in EntryLine.InvoiceLines)
				{
					result = currencyConverter.Add(result, invoiceLine.JI_PackingCosts);
				}
				return result;
			}
		}

		public Money ForeignInlandFreight
		{
			get
			{
				Money result = Money.Empty;
				foreach (JobComInvoiceLine invoiceLine in EntryLine.InvoiceLines)
				{
					result = currencyConverter.Add(result, invoiceLine.JI_ForeignInlandFreight);
				}
				return result;
			}
		}

		#endregion
	}
}
