using System.Collections.Generic;
using System.Linq;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing.KoreaSouth;

namespace Enterprise.Accounting.DataTransfer.EInvoicing.KoreaSouth
{
	public class XmlToAdditionalInfoConverter
	{
		public static ZString GetIssueIDFromXML(XmlDocument taxInvoiceDoc)
		{
			return taxInvoiceDoc["TaxInvoice"]?["TaxInvoiceDocument"]?["IssueID"]?.InnerText;
		}

		public static ZString GetIssueDateTimeFromXML(XmlDocument taxInvoiceDoc)
		{
			return taxInvoiceDoc["TaxInvoice"]?["TaxInvoiceDocument"]?["IssueDateTime"]?.InnerText;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "This is not a frontend facing string")]
		public AdditionalInfo Convert(XmlDocument taxInvoiceDoc)
		{
			if (taxInvoiceDoc == null)
			{
				return null;
			}

			var topLevelNode = taxInvoiceDoc["TaxInvoice"];
			var taxInvoiceDocument = topLevelNode?["TaxInvoiceDocument"];
			var specifiedPaymentMeans = topLevelNode?["TaxInvoiceTradeSettlement"]?["SpecifiedPaymentMeans"];
			var specifiedMonetarySummation = topLevelNode?["TaxInvoiceTradeSettlement"]?["SpecifiedMonetarySummation"];
			var invoiceeParty = topLevelNode?["TaxInvoiceTradeSettlement"]?["InvoiceeParty"];
			var invoicerParty = topLevelNode?["TaxInvoiceTradeSettlement"]?["InvoicerParty"];

			var additionalInfo = new AdditionalInfo()
			{
				IssueID = GetIssueIDFromXML(taxInvoiceDoc),
				OriginalIssueID = taxInvoiceDocument?["OriginalIssueID"]?.InnerText,

				IssueDateTime = GetIssueDateTimeFromXML(taxInvoiceDoc),
				AmendStatusCode = taxInvoiceDocument?["AmendmentStatusCode"]?.InnerText,
				FullTypeCode = taxInvoiceDocument?["TypeCode"]?.InnerText,
				PaymentStatus = taxInvoiceDocument?["PurposeCode"]?.InnerText,

				SpecifiedPaymentMeansTypeCode = specifiedPaymentMeans?["TypeCode"]?.InnerText,
				SpecifiedPaymentMeansPaidAmount = specifiedPaymentMeans?["PaidAmount"]?.InnerText,

				SpecifiedMonetarySummationChargeTotalAmount = specifiedMonetarySummation?["ChargeTotalAmount"]?.InnerText,
				SpecifiedMonetarySummationTaxTotalAmount = specifiedMonetarySummation?["TaxTotalAmount"]?.InnerText,
				SpecifiedMonetarySummationGrandTotalAmount = specifiedMonetarySummation?["GrandTotalAmount"]?.InnerText,

				InvoiceeID = invoiceeParty?["ID"]?.InnerText,
				InvoiceeTypeCode = invoiceeParty?["TypeCode"]?.InnerText,
				InvoiceeNameText = invoiceeParty?["NameText"]?.InnerText,
				InvoiceeClassificationCode = invoiceeParty?["ClassificationCode"]?.InnerText,
				InvoiceeSpecifiedPersonNameText = invoiceeParty?["SpecifiedPerson"]?["NameText"]?.InnerText,
				InvoiceeSpecifiedAddressLineOneText = invoiceeParty?["SpecifiedAddress"]?["LineOneText"]?.InnerText,
				InvoiceePrimaryDefinedContactURICommunication = invoiceeParty?["PrimaryDefinedContact"]?["URICommunication"]?.InnerText,
				InvoiceeSecondaryDefinedContactURICommunication = invoiceeParty?["SecondaryDefinedContact"]?["URICommunication"]?.InnerText,

				InvoicerID = invoicerParty?["ID"]?.InnerText,
				InvoicerTypeCode = invoicerParty?["TypeCode"]?.InnerText,
				InvoicerNameText = invoicerParty?["NameText"]?.InnerText,
				InvoicerClassificationCode = invoicerParty?["ClassificationCode"]?.InnerText,
				InvoicerSpecifiedPersonNameText = invoicerParty?["SpecifiedPerson"]?["NameText"]?.InnerText,
				InvoicerSpecifiedAddressLineOneText = invoicerParty?["SpecifiedAddress"]?["LineOneText"]?.InnerText,
				InvoicerDefinedContactURICommunication = invoicerParty?["DefinedContact"]?["URICommunication"]?.InnerText,
			};

			if (taxInvoiceDocument != null)
			{
				additionalInfo.DescriptionText = taxInvoiceDocument.GetElementsByTagName("DescriptionText").OfType<XmlNode>().Where(x => !string.IsNullOrWhiteSpace(x.InnerText)).Select(x => new ZString(x.InnerText)).ToArray();
			}

			var lineNodes = taxInvoiceDoc.SelectNodes("//*[name()='TaxInvoiceTradeLineItem']");
			if (lineNodes != null)
			{
				additionalInfo.Lines = new List<TaxInvoiceTradeLineItem>();
				foreach (XmlNode node in lineNodes)
				{
					additionalInfo.Lines.Add(new TaxInvoiceTradeLineItem
					{
						DescriptionText = node["DescriptionText"]?.InnerText,
						InvoiceAmount = node["InvoiceAmount"]?.InnerText,
						CalculatedAmount = node["TotalTax"]?["CalculatedAmount"]?.InnerText,
						NameText = node["NameText"]?.InnerText,
						PurchaseExpiryDateTime = node["PurchaseExpiryDateTime"]?.InnerText,
					});
				}
			}

			return additionalInfo;
		}
	}
}
