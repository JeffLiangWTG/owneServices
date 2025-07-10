using System.Linq;
using System.Xml.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth
{
	public class TaxInvoiceBuilder
	{
		public TaxInvoiceBuilder(TaxInvoiceValidation validator, AdditionalInfoConverter additionalInfoConverter)
		{
			Validator = validator;
			AdditionalInfoConverter = additionalInfoConverter;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Element name")]
		public virtual XStreamingElement BuildTaxInvoice(TransactionInfo transactionInfo, AccEInvoicingBatch batch, INotifications notifications)
		{
			Argument.NotNull(batch, nameof(batch));
			Argument.NotNull(transactionInfo, nameof(transactionInfo));

			XNamespace ns = @"urn:kr:or:kec:standard:Tax:ReusableAggregateBusinessInformationEntitySchemaModule:1:0";
			var additionalInfo = AdditionalInfoConverter.ConvertTaxInvoiceAdditionalInfo(transactionInfo, batch);

			XStreamingElement result;
			var notificationWrapper = new NotificationWrapper(transactionInfo.ComplianceSubType, notifications);
			Validator.ValidateTaxInvoice(notificationWrapper, additionalInfo, transactionInfo);

			if (notificationWrapper.HasError)
			{
				result = new XStreamingElement("Invalid");
			}
			else
			{
				result = new XStreamingElement(ns.GetName("TaxInvoice")
					, new XAttribute("xmlns", ns.NamespaceName)
					, new XAttribute(XNamespace.Xmlns + "ds", "http://www.w3.org/2000/09/xmldsig#")
					, new ExchangedDocumentTypeBuilder(ns, batch).Build("ExchangedDocument")
					, new TaxInvoiceDocumentTypeBuilder(ns, transactionInfo, additionalInfo).BuildXML("TaxInvoiceDocument")
					, new XStreamingElement(ns.GetName("TaxInvoiceTradeSettlement")
						, new InvoicerPartyTypeBuilder(ns, additionalInfo).Build("InvoicerParty")
						, new InvoiceePartyTypeBuilder(ns, additionalInfo).Build("InvoiceeParty")
						, new SpecifiedPaymentMeansTypeBuilder(ns, transactionInfo).Build("SpecifiedPaymentMeans")
						, new SpecifiedMonetarySummationTypeBuilder(ns, transactionInfo).Build("SpecifiedMonetarySummation")
						)
					, additionalInfo.Lines?.Select(lineItem => new TaxInvoiceTradeLineItemTypeBuilder(ns, lineItem).BuildXML("TaxInvoiceTradeLineItem"))
				);
			}
			return result;
		}
		TaxInvoiceValidation Validator { get; }
		AdditionalInfoConverter AdditionalInfoConverter { get; }

		class NotificationWrapper : INotifications
		{
			public NotificationWrapper(string transactionPK, INotifications notifications)
			{
				InnerNotifications = notifications;
				TransactionPK = transactionPK;
			}

			public void Add(INotification notification)
			{
				InnerNotifications.Add(new NotificationWithGroupKey(TransactionPK, notification));

				if (notification.Type == NotificationType.Error)
				{
					HasError = true;
				}
			}

			public bool HasError { get; private set; }

			INotifications InnerNotifications { get; }

			string TransactionPK { get; }
		}
	}
}
