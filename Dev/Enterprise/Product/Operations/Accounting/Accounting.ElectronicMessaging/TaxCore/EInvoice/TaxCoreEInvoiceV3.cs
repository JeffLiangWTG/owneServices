using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Newtonsoft.Json;

namespace Enterprise.Accounting.ElectronicMessaging.TaxCore
{
	public class TaxCoreEInvoiceV3 : ITaxCoreEInvoice
	{
		readonly List<Item> lines = [];

		[JsonProperty(PropertyName = "buyerCostCenterId", DefaultValueHandling = DefaultValueHandling.Ignore, Order = 4)]
		public string BuyerCostCenterId { get; set; }

		[JsonProperty(PropertyName = "cashier", DefaultValueHandling = DefaultValueHandling.Ignore, Order = 2)]
		public string CashierTFN { get; set; } = null;

		[JsonIgnore(/* Unused in V3 */)]
		public string Hash { get; set; }

		[JsonProperty(PropertyName = "invoiceNumber", Order = 8)]
		public string InvoiceNumber { get; set; } = null;

		[JsonProperty(PropertyName = "invoiceType", Order = 5)]
		public int InvoiceType { get; set; }

		[JsonProperty(PropertyName = "items", Order = 16)]
		public List<ITaxCoreEInvoiceLine> Lines => lines.ToList<ITaxCoreEInvoiceLine>();

		[JsonIgnore(/* Unused in V3 */)]
		public string PAC { get; set; }

		[JsonProperty(PropertyName = "payment", Order = 7)]
		public List<PaymentInfo> Payment { get; set; } = [];

		[JsonIgnore(/* Unused in V3 */)]
		public int PaymentType { get; set; }

		[JsonProperty(PropertyName = "options", Order = 9)]
		public ITaxCoreEInvoiceOptions QROptions { get; set; } = new Options();

		[JsonProperty(PropertyName = "referentDocumentDT", DefaultValueHandling = DefaultValueHandling.Ignore, Order = 11)]
		public string ReferentDocumentDateAndTime { get; set; } = null;

		[JsonProperty(PropertyName = "referentDocumentNumber", DefaultValueHandling = DefaultValueHandling.Ignore, Order = 10)]
		public string ReferentDocumentNumber { get; set; } = null;

		[JsonProperty(PropertyName = "buyerId", Order = 3)]
		public string TaxIdentificationNumber { get; set; } = null;

		[JsonProperty(PropertyName = "transactionType", Order = 6)]
		public int TransactionType { get; set; }

		[JsonProperty(PropertyName = "dateAndTimeOfIssue", Order = 1)]
		public string UTCCreateTime { get; set; } = null;

		public void AddLine(PostingJournal journal)
		{
			lines.Add(new Item()
			{
				Name = journal.Description ?? string.Empty,
				Labels = journal.TaxMessageID?.TaxGroupCode != null ? new List<string> { journal.TaxMessageID?.TaxGroupCode?.Code } : null,
				UnitPrice = Math.Abs(journal.LocalTotalAmount.Value),
				TotalAmount = Math.Abs(journal.LocalTotalAmount.Value),
				Quantity = 1
			});
		}

		public string GetJSONPayload() => JsonConvert.SerializeObject(this);

		public void SetPayment(TransactionInfo transaction)
		{
			var paymentType = TaxCorePaymentTypes.Other;
			switch (transaction.AgreedPaymentMethod ?? ZString.Empty)
			{
				case OrgConstants.CreditAgreedPaymentMethods.Code.CashAndBankCheck:
					paymentType = TaxCorePaymentTypes.Cash;
					break;

				case OrgConstants.CreditAgreedPaymentMethods.Code.CreditCard:
				case OrgConstants.CreditAgreedPaymentMethods.Code.DebitCard:
					paymentType = TaxCorePaymentTypes.Card;
					break;
			}

			Payment = [
				new PaymentInfo
				{
					Amount = Math.Abs(transaction.LocalTotal.Value),
					PaymentType = paymentType
				}
			];
		}

		public void Validate(INotifications notifications)
		{
			// No schema validation in V3.
		}

		public class Item : ITaxCoreEInvoiceLine
		{
			[JsonProperty(PropertyName = "GTIN", DefaultValueHandling = DefaultValueHandling.Ignore, Order = 1)]
			public string GTIN { get; set; }

			[JsonProperty(PropertyName = "name", Order = 2)]
			public string Name { get; set; }

			[JsonProperty(PropertyName = "quantity", Order = 3)]
			public int Quantity { get; set; }

			[JsonProperty(PropertyName = "discount", DefaultValueHandling = DefaultValueHandling.Ignore, Order = 4)]
			public decimal Discount { get; set; }

			[JsonProperty(PropertyName = "unitPrice", Order = 5)]
			public decimal UnitPrice { get; set; }

			[JsonProperty(PropertyName = "labels", Order = 6)]
			public List<string> Labels { get; set; }

			[JsonProperty(PropertyName = "totalAmount", Order = 7)]
			public decimal TotalAmount { get; set; }
		}

		public class Options : ITaxCoreEInvoiceOptions
		{
			[JsonProperty(PropertyName = "omitQRCodeGen", Order = 1)]
			public string OmitQRCodeGen { get; set; } = TaxCoreOptionTypes.Omit;

			[JsonProperty(PropertyName = "omitTextualRepresentation", Order = 2)]
			public string OmitTextualRepresentation { get; set; } = TaxCoreOptionTypes.Omit;
		}

		public class PaymentInfo
		{
			[JsonProperty(PropertyName = "amount", Order = 1)]
			public decimal Amount { get; set; }

			[JsonProperty(PropertyName = "paymentType", Order = 2)]
			public int PaymentType { get; set; }
		}
	}
}
