using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public class TransactionCurrencySummaryRow : NonPersistentBusinessObject, IObsoleteValidation
	{
		public TransactionCurrencySummaryRow(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Properties

		#region Amount

		[DecimalPlaces(nameof(OSDecimals))]
		public ZDecimal Amount
		{
			get { return fAmount; }
			set
			{
				fAmount = value;
				AmountInfo.RefreshBinding();
			}
		}

		ZDecimal fAmount;

		public ZPropertyInfo AmountInfo
		{
			get { return GetZPropertyInfo(nameof(Amount)); }
		}

		#endregion

		#region LocalAmount

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal LocalAmount
		{
			get { return fLocalAmount; }
			set
			{
				fLocalAmount = value;
				LocalAmountInfo.RefreshBinding();
			}
		}

		ZDecimal fLocalAmount;

		public ZPropertyInfo LocalAmountInfo
		{
			get { return GetZPropertyInfo(nameof(LocalAmount)); }
		}

		#endregion

		#region OutStandingOSAmount

		[DecimalPlaces(nameof(OSDecimals))]
		public ZDecimal OutStandingOSAmount
		{
			get { return fOutStandingOSAmount; }
			set
			{
				fOutStandingOSAmount = value;
				OutStandingOSAmountInfo.RefreshBinding();
			}
		}

		ZDecimal fOutStandingOSAmount;

		public ZPropertyInfo OutStandingOSAmountInfo
		{
			get { return GetZPropertyInfo(nameof(OutStandingOSAmount)); }
		}

		#endregion

		#region OutStandingLocalAmount

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal OutStandingLocalAmount
		{
			get { return fOutStandingLocalAmount; }
			set
			{
				fOutStandingLocalAmount = value;
				OutStandingLocalAmountInfo.RefreshBinding();
			}
		}

		ZDecimal fOutStandingLocalAmount;

		public ZPropertyInfo OutStandingLocalAmountInfo
		{
			get { return GetZPropertyInfo(nameof(OutStandingLocalAmount)); }
		}

		#endregion

		#region TransactionCount

		public ZInt TransactionCount
		{
			get { return fTransactionsCount; }
			set
			{
				fTransactionsCount = value;
				TransactionCountInfo.RefreshBinding();
			}
		}
		ZInt fTransactionsCount;

		public ZPropertyInfo TransactionCountInfo
		{
			get { return GetZPropertyInfo(nameof(TransactionCount)); }
		}

		#endregion

		#region AverageExRate

		[DecimalPlaces(nameof(ExchangeRateDecimals))]
		public ZDecimal AverageExRate
		{
			get { return Env.CurrentCompany.ExchangeRate.GetRate(OutStandingLocalAmount, OutStandingOSAmount); }
		}

		public ZPropertyInfo AverageExRateInfo
		{
			get { return GetZPropertyInfo(nameof(AverageExRate)); }
		}

		#endregion

		#region Currency

		[List(nameof(Currencies))]
		public ZString Currency
		{
			get { return fCurrency; }
			set
			{
				fCurrency = value;
				CurrencyInfo.RefreshBinding();
			}
		}
		ZString fCurrency;

		public ZPropertyInfo CurrencyInfo
		{
			get { return GetZPropertyInfo(nameof(Currency)); }
		}

		#endregion

		#region CurrencyDecimals

		public ZInt CurrencyDecimals
		{
			get
			{
				var accountCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Currency);
				return accountCurrency != null ? accountCurrency.Decimals : 2;
			}
		}

		public ZPropertyInfo CurrencyDecimalsInfo
		{
			get { return GetZPropertyInfo(nameof(CurrencyDecimals)); }
		}

		public int LocalDecimals => GlbCompany.CurrentCompany.GetLocalDecimals();
		public int OSDecimals => CurrencyDecimals;
		public int ExchangeRateDecimals => GlbCompany.CurrentCompany.ExchangeRateDecimalPlaces;

		#endregion

		#region Lookups

		RefCurrencyCollection Currencies
		{
			get { return FindboxLookupCollections.GetCurrencyCollection(Factory); }
		}

		#endregion

		#endregion

		public void AddAmountToTotals(TransactionHeader transactionHeader)
		{
			Amount += transactionHeader.AH_OSTotal;
			LocalAmount += transactionHeader.AH_LocalTotal;
			OutStandingOSAmount += transactionHeader.OSOutstandingAmountMatching;
			OutStandingLocalAmount += transactionHeader.OutstandingAmountMatching;
			TransactionCount++;
		}
	}
}
