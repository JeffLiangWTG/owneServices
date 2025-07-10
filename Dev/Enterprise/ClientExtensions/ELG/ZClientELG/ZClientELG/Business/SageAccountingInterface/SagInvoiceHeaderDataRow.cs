using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.ELG
{
	/// <summary>
	/// Invoice Header.
	/// </summary>
	internal abstract class SagInvoiceHeaderDataRow : SagFlatFileDataRow
	{
		public SagInvoiceHeaderDataRow() : base(SagInvoiceHeaderDataRow.Schema.FieldCapacity)
		{
		}

		public abstract class Schema
		{
			public static readonly FlatFileFieldProperty AccountNumber = new FlatFileFieldProperty(0, 256);
			public static readonly FlatFileFieldProperty SettlementDueDate = new FlatFileFieldProperty(1, 10);
			public static readonly FlatFileFieldProperty OsGoodsValue = new FlatFileFieldProperty(2, 256);
			public static readonly FlatFileFieldProperty LocalControlValue = new FlatFileFieldProperty(3, 256);
			public static readonly FlatFileFieldProperty ExchangeCurrencyRate = new FlatFileFieldProperty(4, 256);
			public static readonly FlatFileFieldProperty ReciprocalExchangeCurrencyRate = new FlatFileFieldProperty(5, 256);
			public static readonly FlatFileFieldProperty LedgerSource = new FlatFileFieldProperty(8, 1);
			public static readonly FlatFileFieldProperty TransactionType = new FlatFileFieldProperty(9, 1);
			public static readonly FlatFileFieldProperty InvoiceDate = new FlatFileFieldProperty(10, 10);
			public static readonly FlatFileFieldProperty LocalTaxValue = new FlatFileFieldProperty(11, 256);

			internal const int FieldCapacity = 12;
		}

		protected override void AddFieldProperties()
		{
			FieldProperties.Add(Schema.AccountNumber);
			FieldProperties.Add(Schema.SettlementDueDate);
			FieldProperties.Add(Schema.OsGoodsValue);
			FieldProperties.Add(Schema.LocalControlValue);
			FieldProperties.Add(Schema.ExchangeCurrencyRate);
			FieldProperties.Add(Schema.ReciprocalExchangeCurrencyRate);
			FieldProperties.Add(Schema.LedgerSource);
			FieldProperties.Add(Schema.TransactionType);
			FieldProperties.Add(Schema.InvoiceDate);
			FieldProperties.Add(Schema.LocalTaxValue);
		}

		/// <summary>
		/// aka AccountNumber
		/// </summary>
		public ZString AccountNumber
		{
			get { return GetField(Schema.AccountNumber); }
			set { SetField(Schema.AccountNumber, value); }
		}

		/// <summary>
		/// aka DueDate
		/// </summary>
		public ZDateTime SettlementDueDate
		{
			get { return GetFieldAsZDateTime(Schema.SettlementDueDate, ELGConstants.DataDateFormat); }
			set { SetField(Schema.SettlementDueDate, value); }
		}

		/// <summary>
		/// aka GoodsValueInAccountCurrency
		/// </summary>
		public ZDecimal OsGoodsValue
		{
			get { return GetFieldAsZDecimal(Schema.OsGoodsValue, 2); }
			set { SetField(Schema.OsGoodsValue, value); }
		}

		/// <summary>
		/// aka PerControlValueInBaseCurrency or SaleControlnValueInBaseCurrency
		/// </summary>
		public ZDecimal LocalControlValue
		{
			get { return GetFieldAsZDecimal(Schema.LocalControlValue, 2); }
			set { SetField(Schema.LocalControlValue, value); }
		}

		/// <summary>
		/// aka DocumentToBaseCurrencyRate
		/// </summary>
		public ZDecimal ExchangeCurrencyRate
		{
			get { return GetFieldAsZDecimal(Schema.ExchangeCurrencyRate, 6); }
			set { SetField(Schema.ExchangeCurrencyRate, value, 6); }
		}

		/// <summary>
		/// aka DocumentToAccountCurrencyRate
		/// </summary>
		public ZDecimal ReciprocalExchangeCurrencyRate
		{
			get { return GetFieldAsZDecimal(Schema.ReciprocalExchangeCurrencyRate, 6); }
			set { SetField(Schema.ReciprocalExchangeCurrencyRate, value, 6); }
		}

		/// <summary>
		/// aka Source
		/// </summary>
		public ZInt LedgerSource
		{
			get { return GetFieldAsZInt(Schema.LedgerSource); }
			set { SetField(Schema.LedgerSource, value); }
		}

		/// <summary>
		/// aka SYSTraderTranType
		/// </summary>
		public ZInt TransactionType
		{
			get { return GetFieldAsZInt(Schema.TransactionType); }
			set { SetField(Schema.TransactionType, value); }
		}

		/// <summary>
		/// aka TransactionDate
		/// </summary>
		public ZDateTime InvoiceDate
		{
			get { return GetFieldAsZDateTime(Schema.InvoiceDate, ELGConstants.DataDateFormat); }
			set { SetField(Schema.InvoiceDate, value); }
		}

		/// <summary>
		/// aka TaxValue
		/// </summary>
		public ZDecimal LocalTaxValue
		{
			get { return GetFieldAsZDecimal(Schema.LocalTaxValue, 2); }
			set { SetField(Schema.LocalTaxValue, value); }
		}
	}
}
