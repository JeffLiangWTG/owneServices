using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.Environment;

namespace Enterprise.Client.SWL.Business
{
	public class ShipnetRecord : FlatFileDataRow
	{
		public ShipnetRecord()
			: base(51)
		{
		}

		#region Schema

		public static class Schema
		{
			public const int Status = 0;
			public const int CompanyCode = 1;
			public const int VoucherNo = 2;
			public const int Sequence = 3;
			public const int AccountNo = 4;
			public const int EntryDate = 5;
			public const int Text = 6;
			public const int Amount = 7;
			public const int CurrencyCode = 8;
			public const int ExchangeRate = 9;
			public const int CurrAmount = 10;
			public const int ShipCode = 11;
			public const int Voyage = 12;
			public const int EntryPeriod = 13;
			public const int CostPeriod = 14;
			public const int Port = 15;
			public const int Department = 16;
			public const int VATCode = 17;
			public const int VATrate = 18;
			public const int ExtraCode1 = 19;
			public const int ExtraCode2 = 20;
			public const int ExtraCode3 = 21;
			public const int InvoiceNo = 22;
			public const int Reference = 23;
			public const int Valdate = 24;
			public const int InvoiceDate = 25;
			public const int DueDate = 26;
			public const int PurchaseDate = 27;
			public const int ExpectDate = 28;
			public const int Pieces = 29;
			public const int Project = 30;
			public const int Purpose = 31;
			public const int Object = 32;
			public const int LedgerType1 = 33;
			public const int LedgerType2 = 34;
			public const int LedgerCode2 = 35;
			public const int LedgerCode1 = 36;
			public const int TonBunkers = 37;
			public const int CIDCode = 38;
			public const int ContractNoteNo = 39;
			public const int LTDCompany = 40;
			public const int OppositeCompany = 41;
			public const int Approved = 42;
			public const int PONumber = 43;
			public const int POYear = 44;
			public const int POVersion = 45;
			public const int Classification = 46;
			public const int Country = 47;
			public const int AssetCode = 48;
			public const int AssetClassCode = 49;
			public const int Contract = 50;
		}

		#endregion

		#region Properties

		public ZString Status
		{
			get { return GetField(Schema.Status); }
			set { SetField(Schema.Status, value); }
		}

		public ZString CompanyCode
		{
			get { return GetField(Schema.CompanyCode); }
			set { SetField(Schema.CompanyCode, value); }
		}

		public ZString VoucherNo
		{
			get { return GetField(Schema.VoucherNo); }
			set { SetField(Schema.VoucherNo, value); }
		}

		public ZString Sequence
		{
			get { return GetField(Schema.Sequence); }
			set { SetField(Schema.Sequence, value); }
		}

		public ZString AccountNo
		{
			get { return GetField(Schema.AccountNo); }
			set { SetField(Schema.AccountNo, value); }
		}

		public ZDateTime EntryDate
		{
			get { return GetFieldAsZDateTime(Schema.EntryDate, DateFormat); }
			set { SetField(Schema.EntryDate, value, DateFormat); }
		}

		public ZString Text
		{
			get { return GetField(Schema.Text); }
			set { SetField(Schema.Text, value); }
		}

		public ZDecimal Amount
		{
			get { return GetFieldAsZDecimal(Schema.Amount, Env.CurrentCompany.LocalCurrency.Decimals); }
			set { SetFieldBlankIfZero(Schema.Amount, value, Env.CurrentCompany.LocalCurrency.Decimals); }
		}

		public ZString CurrencyCode
		{
			get { return GetField(Schema.CurrencyCode); }
			set { SetField(Schema.CurrencyCode, value); }
		}

		public ZDecimal ExchangeRate
		{
			get { return GetFieldAsZDecimal(Schema.ExchangeRate, Env.CurrentCompany.ExchangeRate.RateDecimals); }
			set { SetFieldBlankIfZero(Schema.ExchangeRate, value, Env.CurrentCompany.ExchangeRate.RateDecimals); }
		}

		public ZDecimal CurrAmount
		{
			get { return GetFieldAsZDecimal(Schema.CurrAmount, Env.CurrentCompany.LocalCurrency.Decimals); }
			set { SetFieldBlankIfZero(Schema.CurrAmount, value, Env.CurrentCompany.LocalCurrency.Decimals); }
		}

		public ZString ShipCode
		{
			get { return GetField(Schema.ShipCode); }
			set { SetField(Schema.ShipCode, value); }
		}

		public ZString Voyage
		{
			get { return GetField(Schema.Voyage); }
			set { SetField(Schema.Voyage, value); }
		}

		public ZString EntryPeriod
		{
			get { return GetField(Schema.EntryPeriod); }
			set { SetField(Schema.EntryPeriod, value); }
		}

		public ZString CostPeriod
		{
			get { return GetField(Schema.CostPeriod); }
			set { SetField(Schema.CostPeriod, value); }
		}

		public ZString Port
		{
			get { return GetField(Schema.Port); }
			set { SetField(Schema.Port, value); }
		}

		public ZString Department
		{
			get { return GetField(Schema.Department); }
			set { SetField(Schema.Department, value); }
		}

		public ZString VATCode
		{
			get { return GetField(Schema.VATCode); }
			set { SetField(Schema.VATCode, value); }
		}

		public ZString VATrate
		{
			get { return GetField(Schema.VATrate); }
			set { SetField(Schema.VATrate, value); }
		}

		public ZString ExtraCode1
		{
			get { return GetField(Schema.ExtraCode1); }
			set { SetField(Schema.ExtraCode1, value); }
		}

		public ZString ExtraCode2
		{
			get { return GetField(Schema.ExtraCode2); }
			set { SetField(Schema.ExtraCode2, value); }
		}

		public ZString ExtraCode3
		{
			get { return GetField(Schema.ExtraCode3); }
			set { SetField(Schema.ExtraCode3, value); }
		}

		public ZString InvoiceNo
		{
			get { return GetField(Schema.InvoiceNo); }
			set { SetField(Schema.InvoiceNo, value); }
		}

		public ZString Reference
		{
			get { return GetField(Schema.Reference); }
			set { SetField(Schema.Reference, value); }
		}

		public ZString Valdate
		{
			get { return GetField(Schema.Valdate); }
			set { SetField(Schema.Valdate, value); }
		}

		public ZDateTime InvoiceDate
		{
			get { return GetFieldAsZDateTime(Schema.InvoiceDate, DateFormat); }
			set { SetField(Schema.InvoiceDate, value, DateFormat); }
		}

		public ZDateTime DueDate
		{
			get { return GetFieldAsZDateTime(Schema.DueDate, DateFormat); }
			set { SetField(Schema.DueDate, value, DateFormat); }
		}

		public ZDateTime PurchaseDate
		{
			get { return GetFieldAsZDateTime(Schema.PurchaseDate, DateFormat); }
			set { SetField(Schema.PurchaseDate, value, DateFormat); }
		}

		public ZDateTime ExpectDate
		{
			get { return GetFieldAsZDateTime(Schema.ExpectDate, DateFormat); }
			set { SetField(Schema.ExpectDate, value, DateFormat); }
		}

		public ZString Pieces
		{
			get { return GetField(Schema.Pieces); }
			set { SetField(Schema.Pieces, value); }
		}

		public ZString Project
		{
			get { return GetField(Schema.Project); }
			set { SetField(Schema.Project, value); }
		}

		public ZString Purpose
		{
			get { return GetField(Schema.Purpose); }
			set { SetField(Schema.Purpose, value); }
		}

		public ZString Object
		{
			get { return GetField(Schema.Object); }
			set { SetField(Schema.Object, value); }
		}

		public ZString LedgerType1
		{
			get { return GetField(Schema.LedgerType1); }
			set { SetField(Schema.LedgerType1, value); }
		}

		public ZString LedgerType2
		{
			get { return GetField(Schema.LedgerType2); }
			set { SetField(Schema.LedgerType2, value); }
		}

		public ZString LedgerCode2
		{
			get { return GetField(Schema.LedgerCode2); }
			set { SetField(Schema.LedgerCode2, value); }
		}

		public ZString LedgerCode1
		{
			get { return GetField(Schema.LedgerCode1); }
			set { SetField(Schema.LedgerCode1, value); }
		}

		public ZString TonBunkers
		{
			get { return GetField(Schema.TonBunkers); }
			set { SetField(Schema.TonBunkers, value); }
		}

		public ZString CIDCode
		{
			get { return GetField(Schema.CIDCode); }
			set { SetField(Schema.CIDCode, value); }
		}

		public ZString ContractNoteNo
		{
			get { return GetField(Schema.ContractNoteNo); }
			set { SetField(Schema.ContractNoteNo, value); }
		}

		public ZString LTDCompany
		{
			get { return GetField(Schema.LTDCompany); }
			set { SetField(Schema.LTDCompany, value); }
		}

		public ZString OppositeCompany
		{
			get { return GetField(Schema.OppositeCompany); }
			set { SetField(Schema.OppositeCompany, value); }
		}

		public ZString Approved
		{
			get { return GetField(Schema.Approved); }
			set { SetField(Schema.Approved, value); }
		}

		public ZString PONumber
		{
			get { return GetField(Schema.PONumber); }
			set { SetField(Schema.PONumber, value); }
		}

		public ZString POYear
		{
			get { return GetField(Schema.POYear); }
			set { SetField(Schema.POYear, value); }
		}

		public ZString POVersion
		{
			get { return GetField(Schema.POVersion); }
			set { SetField(Schema.POVersion, value); }
		}

		public ZString Classification
		{
			get { return GetField(Schema.Classification); }
			set { SetField(Schema.Classification, value); }
		}

		public ZString Country
		{
			get { return GetField(Schema.Country); }
			set { SetField(Schema.Country, value); }
		}

		public ZString AssetCode
		{
			get { return GetField(Schema.AssetCode); }
			set { SetField(Schema.AssetCode, value); }
		}

		public ZString AssetClassCode
		{
			get { return GetField(Schema.AssetClassCode); }
			set { SetField(Schema.AssetClassCode, value); }
		}

		public ZString Contract
		{
			get { return GetField(Schema.Contract); }
			set { SetField(Schema.Contract, value); }
		}

		#endregion

		public const string DateFormat = "yyyyMMdd";

		protected void SetFieldBlankIfZero(int position, ZDecimal value, int decimals)
		{
			if (value == 0)
			{
				SetField(position, "");
			}
			else
			{
				SetField(position, value, decimals);
			}
		}
	}
}
