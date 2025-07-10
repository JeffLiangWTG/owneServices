using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Newtonsoft.Json;

namespace Enterprise.Accounting.ElectronicMessaging.TaxCore
{
	public class TaxCoreEInvoiceLegacy : ITaxCoreEInvoice
	{
		readonly List<Line> lines = [];

		[JsonProperty(PropertyName = "DateAndTimeOfIssue", DefaultValueHandling = DefaultValueHandling.Ignore, Order = 1)]
		public string UTCCreateTime { get; set; } = null;

		[JsonProperty(PropertyName = "Cashier", DefaultValueHandling = DefaultValueHandling.Ignore, Order = 2)]
		public string CashierTFN { get; set; } = null;

		[JsonProperty(PropertyName = "BD", DefaultValueHandling = DefaultValueHandling.Ignore, Order = 3)]
		public string TaxIdentificationNumber { get; set; } = null;

		[JsonProperty(PropertyName = "BuyerCostCenterId", DefaultValueHandling = DefaultValueHandling.Ignore, Order = 4)]
		public string BuyerCostCenterId { get; set; } = null;

		[JsonProperty(PropertyName = "IT", Order = 5)]
		public int InvoiceType { get; set; } = TaxCoreInvoiceTypes.Normal;

		[JsonProperty(PropertyName = "TT", Order = 6)]
		public int TransactionType { get; set; } = TaxCoreTransactionTypes.NotSpecified;

		[JsonProperty(PropertyName = "PaymentType", Order = 7)]
		public int PaymentType { get; private set; } = TaxCorePaymentTypes.NotSpecified;

		[JsonProperty(PropertyName = "InvoiceNumber", Order = 8)]
		public string InvoiceNumber { get; set; } = null;

		[JsonProperty(PropertyName = "ReferentDocumentNumber", DefaultValueHandling = DefaultValueHandling.Ignore, Order = 9)]
		public string ReferentDocumentNumber { get; set; } = null;

		[JsonProperty(PropertyName = "ReferentDocumentDateAndTime", DefaultValueHandling = DefaultValueHandling.Ignore, Order = 10)]
		public string ReferentDocumentDateAndTime { get; set; } = null;

		[JsonProperty(PropertyName = "PAC", DefaultValueHandling = DefaultValueHandling.Ignore, Order = 11)]
		public string PAC { get; set; } = null;

		[JsonProperty(PropertyName = "Options", Order = 12)]
		public ITaxCoreEInvoiceOptions QROptions { get; set; } = new Options();

		[JsonProperty(PropertyName = "Hash", DefaultValueHandling = DefaultValueHandling.Ignore, Order = 13)]
		public string Hash { get; set; } = null;

		[JsonProperty(PropertyName = "Items", Order = 14)]
		public List<ITaxCoreEInvoiceLine> Lines => lines.ToList<ITaxCoreEInvoiceLine>();

		public void AddLine(PostingJournal journal)
		{
			lines.Add(new Line()
			{
				Name = journal.Description ?? string.Empty,
				Labels = journal.TaxMessageID?.TaxGroupCode != null ? new List<string> { journal.TaxMessageID?.TaxGroupCode?.Code } : null,
				UnitPrice = Math.Abs(journal.LocalTotalAmount.Value),
				TotalAmount = Math.Abs(journal.LocalTotalAmount.Value)
			});
		}

		public string GetJSONPayload() => JsonConvert.SerializeObject(this);

		public void Validate(INotifications notifications)
		{
			var payload = GetJSONPayload();
			var schema = JsonSchemaLoader.Load2("Enterprise.Accounting.ElectronicMessaging.TaxCore.EInvoice.TaxCoreEInvoiceSchema.json");
			schema.ValidateJSON(payload, notifications);
		}

		public void SetPayment(TransactionInfo transaction)
		{
			var result = TaxCorePaymentTypes.Other;
			switch (transaction.AgreedPaymentMethod ?? ZString.Empty)
			{
				case OrgConstants.CreditAgreedPaymentMethods.Code.CashAndBankCheck:
					result = TaxCorePaymentTypes.Cash;
					break;

				case OrgConstants.CreditAgreedPaymentMethods.Code.CreditCard:
				case OrgConstants.CreditAgreedPaymentMethods.Code.DebitCard:
					result = TaxCorePaymentTypes.Card;
					break;
			}

			PaymentType = result;
		}

		public class Line : ITaxCoreEInvoiceLine
		{
			[JsonProperty(PropertyName = "GTIN", DefaultValueHandling = DefaultValueHandling.Ignore, Order = 1)]
			public string GTIN { get; set; } = null;

			[JsonProperty(PropertyName = "Name", Order = 2)]
			public string Name { get; set; } = null;

			[JsonProperty(PropertyName = "Quantity", Order = 3)]
			public int Quantity { get; set; } = 1;

			[JsonProperty(PropertyName = "Discount", DefaultValueHandling = DefaultValueHandling.Ignore, Order = 4)]
			public decimal Discount { get; set; } = 0M;

			[JsonProperty(PropertyName = "UnitPrice", Order = 5)]
			public decimal UnitPrice { get; set; } = 0M;

			[JsonProperty(PropertyName = "Labels", DefaultValueHandling = DefaultValueHandling.Ignore, Order = 6)]
			public List<string> Labels { get; set; } = null;

			[JsonProperty(PropertyName = "TotalAmount", Order = 7)]
			public decimal TotalAmount { get; set; } = 0M;
		}

		public class Options : ITaxCoreEInvoiceOptions
		{
			[JsonProperty(PropertyName = "OmitQRCodeGen", Order = 1)]
			public string OmitQRCodeGen { get; set; } = TaxCoreOptionTypes.Omit;

			[JsonProperty(PropertyName = "OmitTextualRepresentation", Order = 2)]
			public string OmitTextualRepresentation { get; set; } = TaxCoreOptionTypes.Omit;
		}
	}
}
