
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class EntryHeaderCommercialChargesProvider : ICommercialChargesProvider
	{
		public EntryHeaderCommercialChargesProvider(CusEntryHeader entryHeader)
		{
			this.EntryHeader = entryHeader;
			CurrencyConverter = entryHeader.CurrencyConverter;
		}

		#region Implementation

		protected readonly CusEntryHeader EntryHeader;
		protected readonly CurrencyConverter CurrencyConverter;

		#endregion

		#region ICommercialChargesProvider Members

		public string ITOTIncoTerm
		{
			get
			{
				return EntryHeader.RandomHeader?.ITOTIncoTerm ?? ZString.Empty;
			}
		}

		public Money InvoiceTotal
		{
			get
			{
				Money result = Money.Empty;
				foreach (CusEntryLine cusLine in EntryHeader.MergedLines)
				{
					result = CurrencyConverter.Add(result, cusLine.Price);
				}
				return result;
			}
		}

		public Money Commission
		{
			get
			{
				Money result = Money.Empty;
				foreach (CusEntryLine cusLine in EntryHeader.MergedLines)
				{
					result = CurrencyConverter.Add(result, cusLine.BuyingCommission);
					result = CurrencyConverter.Add(result, cusLine.OtherCommission);
					result = CurrencyConverter.Add(result, cusLine.Commission);
				}
				return result;
			}
		}

		public Money Discount
		{
			get
			{
				Money result = Money.Empty;
				foreach (CusEntryLine cusLine in EntryHeader.MergedLines)
				{
					result = CurrencyConverter.Add(result, cusLine.Discount);
				}
				return result;
			}
		}

		public Money OtherCharges1
		{
			get
			{
				Money result = Money.Empty;
				foreach (CusEntryLine cusLine in EntryHeader.MergedLines)
				{
					result = CurrencyConverter.Add(result, cusLine.OtherCharge1);
				}
				return result;
			}
		}

		public Money OtherCharges2
		{
			get
			{
				Money result = Money.Empty;
				foreach (CusEntryLine cusLine in EntryHeader.MergedLines)
				{
					result = CurrencyConverter.Add(result, cusLine.OtherCharge2);
				}
				return result;
			}
		}

		public Money LandingCharges
		{
			get
			{
				Money result = Money.Empty;
				foreach (CusEntryLine cusLine in EntryHeader.MergedLines)
				{
					result = CurrencyConverter.Add(result, cusLine.LandingCharge);
				}
				return result;
			}
		}

		public Money PackingCosts
		{
			get
			{
				Money result = Money.Empty;
				foreach (CusEntryLine cusLine in EntryHeader.MergedLines)
				{
					result = CurrencyConverter.Add(result, cusLine.PackingCosts);
				}
				return result;
			}
		}

		public Money ForeignInlandFreight
		{
			get
			{
				Money result = Money.Empty;
				foreach (CusEntryLine cusLine in EntryHeader.MergedLines)
				{
					result = CurrencyConverter.Add(result, cusLine.ForeignInlandFreight);
				}
				return result;
			}
		}

		#endregion
	}
}
