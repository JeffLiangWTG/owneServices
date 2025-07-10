using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.ElectronicMessaging.TaxCore
{
	public interface ITaxCoreEInvoice
	{
		string BuyerCostCenterId { get; set; }
		string CashierTFN { get; set; }
		string Hash { get; set; }
		string InvoiceNumber { get; set; }
		int InvoiceType { get; set; }
		List<ITaxCoreEInvoiceLine> Lines { get; }
		string PAC { get; set; }
		int PaymentType { get; }
		ITaxCoreEInvoiceOptions QROptions { get; set; }
		string ReferentDocumentDateAndTime { get; set; }
		string ReferentDocumentNumber { get; set; }
		string TaxIdentificationNumber { get; set; }
		int TransactionType { get; set; }
		string UTCCreateTime { get; set; }

		void AddLine(PostingJournal pj);

		string GetJSONPayload();

		void Validate(INotifications notifications);

		void SetPayment(TransactionInfo transactionInfo);
	}

	public interface ITaxCoreEInvoiceLine
	{
		decimal Discount { get; set; }
		string GTIN { get; set; }
		List<string> Labels { get; set; }
		string Name { get; set; }
		int Quantity { get; set; }
		decimal TotalAmount { get; set; }
		decimal UnitPrice { get; set; }
	}

	public interface ITaxCoreEInvoiceOptions
	{
		string OmitQRCodeGen { get; set; }
		string OmitTextualRepresentation { get; set; }
	}

	public static class TaxCoreInvoiceTypes
	{
		public const int Copy = 2;
		public const int Normal = 0;
		public const int Proforma = 1;
		public const int Training = 3;
	}

	public static class TaxCoreTransactionTypes
	{
		public const int NotSpecified = -1;
		public const int Refund = 1;
		public const int Sale = 0;
	}

	public static class TaxCorePaymentTypes
	{
		public const int Card = 2;
		public const int Cash = 1;
		public const int Check = 3;
		public const int MobileMoney = 5;
		public const int NotSpecified = -1;
		public const int Other = 0;
		public const int Voucher = 6;
		public const int WireTransfer = 4;
	}

	public static class TaxCoreOptionTypes
	{
		public const string DoNotOmit = "0";
		public const string Omit = "1";
	}
}
