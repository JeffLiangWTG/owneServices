using System;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Common;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.EInvoicing.KoreaSouth;
using Enterprise.Accounting.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth
{
	class TaxInvoiceDocumentTypeBuilder : XmlBuilder
	{
		public TaxInvoiceDocumentTypeBuilder(XNamespace xNamespace, TransactionInfo transactionInfo, AdditionalInfo additionalInfo) : base(xNamespace)
		{
			Argument.NotNull(transactionInfo, nameof(transactionInfo));
			Argument.NotNull(additionalInfo, nameof(additionalInfo));

			TransactionInfo = transactionInfo;
			AdditionalInfo = additionalInfo;

			CommonTypeBuilder = new Lazy<CommonTypeBuilder>(() => new CommonTypeBuilder(xNamespace));
			DataElementProvider = new Lazy<KoreaSouthEInvoicingDataElementProvider>(() => new KoreaSouthEInvoicingDataElementProvider(transactionInfo, additionalInfo));
		}

		public XStreamingElement BuildXML(string tagName)
		{
			return new XStreamingElement(GetTagName(tagName)
				, BuildIssueID()
				, BuildTypeCode()
				, BuildDescriptionText(EInvoicingKoreaSouthConstants.DataElementList.InvoiceDocumentHeaderDescriptionLine1)
				, BuildDescriptionText(EInvoicingKoreaSouthConstants.DataElementList.InvoiceDocumentHeaderDescriptionLine2)
				, BuildDescriptionText(EInvoicingKoreaSouthConstants.DataElementList.InvoiceDocumentHeaderDescriptionLine3)
				, BuildIssueDateTime()
				, BuildAmendmentStatusCode()
				, BuildPurposeCode()
				, BuildOriginalIssueID()
			);
		}

		XElement BuildIssueID()
		{
			return new XElement(GetTagName("IssueID"), AdditionalInfo.IssueID);
		}

		XElement BuildTypeCode()
		{
			return new XElement(GetTagName("TypeCode"), AdditionalInfo.FullTypeCode);
		}

		XElement BuildDescriptionText(string dataElement)
		{
			var invoiceType = TransactionInfo.IsAmendment() ? EInvoicingKoreaSouthConstants.InvoiceTypeList.Amendment : EInvoicingKoreaSouthConstants.InvoiceTypeList.OriginalInvoice;
			var config = AccountingConfigurationRegistry.Instance.ElectronicInvoiceDataElementsConfiguration.Value.OfType<KoreaSouthEInvoicingDataElementConfiguration>()
				.Single(x => x.InvoiceType == invoiceType && x.DataElement == dataElement);

			var text = DataElementProvider.Value.Evaluate(config.Configuration);
			return string.IsNullOrWhiteSpace(text) ? null : CommonTypeBuilder.Value.BuildTaxInvoiceFreeTextType("DescriptionText", text);
		}

		XElement BuildIssueDateTime()
		{
			return CommonTypeBuilder.Value.BuildTaxInvoiceDateType("IssueDateTime", TransactionInfo.TransactionDate.Value.ToDateTime());
		}

		XElement BuildAmendmentStatusCode()
		{
			return !AdditionalInfo.AmendStatusCode.IsEmpty
				? new XElement(GetTagName("AmendmentStatusCode"), AdditionalInfo.AmendStatusCode)
				: null;
		}

		XElement BuildOriginalIssueID()
		{
			return TransactionInfo.IsAmendment()
				? new XElement(GetTagName("OriginalIssueID"), AdditionalInfo.OriginalIssueID)
				: null;
		}

		XElement BuildPurposeCode()
		{
			var valueText = (TransactionInfo.FullyPaidDate?.IsEmpty ?? true)
				? "02"
				: "01";
			return new XElement(GetTagName("PurposeCode"), valueText);
		}

		TransactionInfo TransactionInfo { get; }

		ITaxInvoiceDocumentTypeAdditionalInfo AdditionalInfo { get; }

		Lazy<CommonTypeBuilder> CommonTypeBuilder { get; }

		Lazy<KoreaSouthEInvoicingDataElementProvider> DataElementProvider { get; }
	}
}
