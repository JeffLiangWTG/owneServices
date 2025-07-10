
using System;
using CargoWise.Common;
using CargoWise.Types;

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.JAS.Business.Matching
{
	public class MatchedDataLine : DataLine
	{
		public const int MinStringLength = 167;

		public MatchedDataLine(string rawString)
		{
			LineString = rawString;
		}

		#region Properties

		#region New Properties

		ZString fDestinationSubsidiary;
		public ZString DestinationSubsidiary
		{
			get
			{
				if (fDestinationSubsidiary.IsEmpty)
				{
					fDestinationSubsidiary = LineString.SubstringSafe(0, 5).Trim(' ');
				}
				return fDestinationSubsidiary;
			}
		}

		ZString fDenomination;
		public ZString Denomination
		{
			get
			{
				if (fDenomination.IsEmpty)
				{
					fDenomination = LineString.SubstringSafe(45, 34).Trim(' ');
				}
				return fDenomination;
			}
		}

		ZString fLedger;
		public ZString Ledger
		{
			get
			{
				if (fLedger.IsEmpty)
				{
					fLedger = (LineString.SubstringSafe(147, 1) == "R") ? LedgerTypes.AccountsReceivable : LedgerTypes.AccountsPayable;
				}
				return fLedger;
			}
		}

		ZDecimal fExchangeValue;
		public ZDecimal ExchangeValue
		{
			get
			{
				if (fExchangeValue.IsEmpty)
				{
					ZDecimal.TryParse(LineString.SubstringSafe(148, 15), out fExchangeValue);
				}
				return fExchangeValue;
			}
		}

		ZString fCurrencyOfExchangeValue;
		public ZString CurrencyOfExchangeValue
		{
			get
			{
				if (fCurrencyOfExchangeValue.IsEmpty)
				{
					fCurrencyOfExchangeValue = LineString.SubstringSafe(164, 3);
				}
				return fCurrencyOfExchangeValue;
			}
		}

		#endregion

		#region Overrides

		ZDecimal fAmount;
		public override ZDecimal Amount
		{
			get
			{
				if (fAmount.IsEmpty)
				{
					ZDecimal.TryParse(LineString.SubstringSafe(10, 15), out fAmount);
				}
				return fAmount;
			}
		}

		ZString fCategory;
		public override ZString Category
		{
			get
			{
				if (fCategory.IsEmpty)
				{
					fCategory = LineString.SubstringSafe(94, 1).Trim(' ');
				}
				return fCategory;
			}
		}

		ZString fCounterpartSubsidiary;
		public override ZString CounterpartSubsidiary
		{
			get
			{
				if (fCounterpartSubsidiary.IsEmpty)
				{
					fCounterpartSubsidiary = LineString.SubstringSafe(5, 5).Trim(' ');
				}
				return fCounterpartSubsidiary;
			}
		}

		public override ZBool CreditNote
		{
			get { return (LineString.SubstringSafe(25, 1) == "-"); }
		}

		public override ZString TransactionType
		{
			get { return LineString.SubstringSafe(79, 3); }
		}

		ZString fCurrencyCode;
		public override ZString CurrencyCode
		{
			get
			{
				if (fCurrencyCode.IsEmpty)
				{
					fCurrencyCode = LineString.SubstringSafe(36, 3).Trim(' ');
				}
				return fCurrencyCode;
			}
		}

		ZString fFullInvoiceNumber;
		public override ZString FullInvoiceNumber
		{
			get
			{
				if (fFullInvoiceNumber.IsEmpty)
				{
					fFullInvoiceNumber = LineString.SubstringSafe(133, 14).Trim(' ');
				}
				return fFullInvoiceNumber;
			}
		}

		ZString fHouseBill;
		public override ZString HouseBill
		{
			get
			{
				if (fHouseBill.IsEmpty)
				{
					fHouseBill = LineString.SubstringSafe(119, 14).Trim(' ');
				}
				return fHouseBill;
			}
		}

		ZDateTime fInvoiceDate;
		public override ZDateTime InvoiceDate
		{
			get
			{
				if (fInvoiceDate.IsEmpty)
				{
					try
					{
						ZString dateTimeString = LineString.SubstringSafe(95, 10);
						fInvoiceDate = new ZDateTime(
							int.Parse(dateTimeString.SubstringSafe(6, 4)),
							int.Parse(dateTimeString.SubstringSafe(3, 2)),
							int.Parse(dateTimeString.SubstringSafe(0, 2)));
					}
					catch (Exception e) when (!e.IsCriticalException()) { }
					{
						// Do nothing, assume date time format is correct 
					}
				}
				return fInvoiceDate;
			}
		}

		ZString fMasterBill;
		public override ZString MasterBill
		{
			get
			{
				if (fMasterBill.IsEmpty)
				{
					fMasterBill = LineString.SubstringSafe(105, 14).Trim(' ');
				}
				return fMasterBill;
			}
		}

		ZDateTime fMaturityDate;
		public override ZDateTime MaturityDate
		{
			get
			{
				if (fMaturityDate.IsEmpty)
				{
					try
					{
						ZString dateTimeString = LineString.SubstringSafe(26, 10);
						fMaturityDate = new ZDateTime(
							int.Parse(dateTimeString.SubstringSafe(6, 4)),
							int.Parse(dateTimeString.SubstringSafe(3, 2)),
							int.Parse(dateTimeString.SubstringSafe(0, 2)));
					}
					catch (Exception e) when (!e.IsCriticalException()) { }
					{
						// Do nothing, assume date time format is correct 
					}
				}
				return fMaturityDate;
			}
		}

		#endregion

		#endregion

		readonly ZString LineString;
	}
}
