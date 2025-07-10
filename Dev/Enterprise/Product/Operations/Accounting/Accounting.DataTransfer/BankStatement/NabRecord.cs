using System;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.DataTransfer.BankStatement
{
	public class NabRecord
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1122:DoNotUseDateTimeParse", Justification = "The date could be in different formats and ZDateTime does not have an overload to use")]
		public NabRecord(string line, FlatFileFormat format)
		{
			FlatFileDataRow row = format.ConvertToRow(line);
			if (row.FieldCount != FieldCount)
			{
				throw new FormatException("Each line of Nab file is expected to have " + FieldCount.ToString() + " fields.");
			}

			Currency = row.GetField(1).Trim();
			Date = DateTime.Parse(row.GetField(3));// The date could be in different formats and ZDateTime does not have an overload to use
			Type = GetMappedType(row.GetField(4).Trim().TrimStart('0'));

			Reference = GetXmlFriendlyString(row.GetField(6));
			if (Reference == "")
			{
				Reference = "REF";
			}

			Description = GetXmlFriendlyString(row.GetField(7));
			if (Description == "")
			{
				Description = GetXmlFriendlyString(row.GetField(5));
			}

			Amount = -row.GetFieldAsZDecimal(8, 2);
		}

		public readonly ZString Currency;
		public readonly ZDateTime Date;
		public readonly ZString Type;
		public readonly ZString Reference;
		public readonly ZString Description;
		public readonly ZDecimal Amount;

		#region Implementation

		const int FieldCount = 9;

		#region GetXmlFriendlyString

		ZString GetXmlFriendlyString(ZString field)
		{
			return field.Trim().Replace("&", "&amp;").Replace("<", "&lt;");
		}

		#endregion

		#region GetMappedType

		ZString GetMappedType(string nabType)
		{
			ZString result = "";

			switch (nabType)
			{
				case NabTypes.ClosingBalance:
					result = NabTypes.ClosingBalance;
					break;

				case NabTypes.ChequesPaid:
					result = ReceiptTypes.Cheque;
					break;

				case NabTypes.ChequesLodged:
				case NabTypes.TransferCredits:
				case NabTypes.ReversalEntry:
				case NabTypes.CreditAdjustment:
				case NabTypes.MiscellaneousCredits:
				case NabTypes.DishonouredCheques:
				case NabTypes.Cash:
				case NabTypes.CashCheques:
				case NabTypes.AgentCredits:
				case NabTypes.InterbankCredits:
				case NabTypes.BankcardCredits:
				case NabTypes.EFTPOS:
					result = TransactionTypes.ReceiptBatch;
					break;

				case NabTypes.TransferDebits:
				case NabTypes.AutomaticDrawings:
				case NabTypes.FlexiPay:
				case NabTypes.DebitAdjustment:
				case NabTypes.MiscellaneousDebits:
				case NabTypes.Charges:
					result = ReceiptTypes.EFT;
					break;

				case NabTypes.DebitBalanceTransfers:
					result = TransactionTypes.Transfer;
					break;
			}

			return result;
		}

		#endregion

		#region NabTypes

		public static class NabTypes
		{
			public const string ClosingBalance = "15";
			public const string ChequesLodged = "175";
			public const string TransferCredits = "195";
			public const string Dividend = "238";
			public const string ReversalEntry = "252";
			public const string CreditAdjustment = "357";
			public const string PCSubAccountCredit = "390";
			public const string MiscellaneousCredits = "399";
			public const string ChequesPaid = "475";
			public const string TransferDebits = "495";
			public const string AutomaticDrawings = "501";
			public const string Documentary_LC_DrawingsFees = "512";
			public const string DishonouredCheques = "555";
			public const string LoanFees = "564";
			public const string FlexiPay = "595";
			public const string DebitAdjustment = "631";
			public const string DebitInterest = "654";
			public const string PCSubAccountDebit = "690";
			public const string MiscellaneousDebits = "699";
			public const string CreditInterest = "905";
			public const string NationalNomineesCredits = "906";
			public const string Cash = "910";
			public const string CashCheques = "911";
			public const string AgentCredits = "915";
			public const string InterbankCredits = "920";
			public const string BankcardCredits = "925";
			public const string CreditBalanceTransfer = "930";
			public const string SummarisedCredits = "935";
			public const string EFTPOS = "936";
			public const string NFCA_Credit = "938";
			public const string EstablishmentFees = "950";
			public const string AccountKeepingFees = "951";
			public const string UnusedLimitFees = "952";
			public const string SecurityFees = "953";
			public const string Charges = "955";
			public const string NationalNomineeDebits = "956";
			public const string ChequeBook = "960";
			public const string StampDuty = "961";
			public const string SecurityStampDuty = "962";
			public const string StateGovernmentTax = "970";
			public const string FederalGovernmentTax = "971";
			public const string BankcardDebits = "975";
			public const string DebitBalanceTransfers = "980";
			public const string DebitsSummarised = "985";
			public const string ChequesSummarised = "986";
			public const string NonChequesSummarised = "987";
			public const string DebitNFCA = "988";
		}

		#endregion

		#endregion
	}
}
