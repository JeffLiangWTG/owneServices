using System;
using System.Globalization;
using System.Linq;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Business.EInvoicing.KoreaSouth;
using Enterprise.Accounting.DataTransfer.EInvoicing.KoreaSouth;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.Accounting.KR;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.KR
{
	public class DocElectronicInvoice : DocumentWrapper
	{
		#region Construction

		protected DocElectronicInvoice(InvoicingBase invoice, AdditionalInfo additionalInfo, BusinessObjectFactory factoryToWrap)
			: base(invoice, factoryToWrap)
		{
			AdditionalInfo = additionalInfo;
		}

		public static DocElectronicInvoice New(InvoicingBase invoice, BusinessObjectFactory factoryToWrap)
		{
			if (invoice == null)
			{
				return null;
			}

			var authRecord = AccTransactionHeaderAuthorisationRecordLoader.LoadByParentID(invoice.Factory, invoice.PK, invoice.Company.Country.RN_Code);
			var xmlStr = authRecord?.AHF_AuthorisationData.ToUTF8();
			if (string.IsNullOrEmpty(xmlStr))
			{
				return null;
			}

			var xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(xmlStr);
			var additionalInfo = new XmlToAdditionalInfoConverter().Convert(xmlDocument);
			return new DocElectronicInvoice(invoice, additionalInfo, factoryToWrap);
		}

		#endregion

		#region Properties

		public ZString InvoiceTitle
		{
			get
			{
				if (TypeCode == "0102")
				{
					return (NoResString)"영세율전자세금계산서";
				}
				else if (TypeCode == "0202")
				{
					return (NoResString)"수정영세율전자세금계산서";
				}
				else if (TypeCode == "0301")
				{
					return (NoResString)"전자계산서";
				}
				else if (TypeCode == "0401")
				{
					return (NoResString)"수정전자계산서";
				}
				else if (OriginalIssueID.IsEmpty)
				{
					return (NoResString)"전자세금계산서";
				}
				else
				{
					return (NoResString)"수정전자세금계산서";
				}
			}
		}

		public ZString IssueID
		{
			get
			{
				if (AdditionalInfo?.IssueID.Length == 24)
				{
					return string.Format(CultureInfo.InvariantCulture, "{0}-{1}-{2}", AdditionalInfo?.IssueID.Substring(0, 8), AdditionalInfo?.IssueID.Substring(8, 8), AdditionalInfo?.IssueID.Substring(16, 8));
				}

				return AdditionalInfo?.IssueID ?? ZString.Empty;
			}
		}

		public ZString OriginalIssueID => AdditionalInfo?.OriginalIssueID ?? ZString.Empty;

		public ZString IssueDateTime => AdditionalInfo?.IssueDateTime ?? ZString.Empty;
		public ZString AmendmentStatusCode => AdditionalInfo?.AmendStatusCode ?? ZString.Empty;
		public ZString AmendmentStatusDescription => EInvoicingKoreaSouthConstants.AmendStatusList.GetDescriptionFromCode(AmendmentStatusCode) ?? ZString.Empty;

		public ZString DescriptionText1 => AdditionalInfo?.DescriptionText?.ElementAtOrDefault(0) ?? ZString.Empty;

		public ZString DescriptionText2 => AdditionalInfo?.DescriptionText?.ElementAtOrDefault(1) ?? ZString.Empty;

		public ZString DescriptionText3 => AdditionalInfo?.DescriptionText?.ElementAtOrDefault(2) ?? ZString.Empty;

		public ZString SpecifiedPaymentMeansTypeCode => AdditionalInfo?.SpecifiedPaymentMeansTypeCode ?? ZString.Empty;
		public ZString SpecifiedPaymentMeansPaidAmount => AdditionalInfo?.SpecifiedPaymentMeansPaidAmount ?? ZString.Empty;

		public ZString SpecifiedMonetarySummationChargeTotalAmount => AdditionalInfo?.SpecifiedMonetarySummationChargeTotalAmount ?? ZString.Empty;
		public ZString SpecifiedMonetarySummationTaxTotalAmount => AdditionalInfo?.SpecifiedMonetarySummationTaxTotalAmount ?? ZString.Empty;
		public ZString SpecifiedMonetarySummationGrandTotalAmount => AdditionalInfo?.SpecifiedMonetarySummationGrandTotalAmount ?? ZString.Empty;

		public ZString[] SpecifiedMonetarySummationChargeTotalAmountDigits => SpecifiedMonetarySummationChargeTotalAmount.PadLeft(12).Split(1).Reverse().ToArray();

		public ZString[] SpecifiedMonetarySummationTaxTotalAmountDigits => SpecifiedMonetarySummationTaxTotalAmount.PadLeft(11).Split(1).Reverse().ToArray();

		public ZString InvoiceeID
		{
			get
			{
				if (AdditionalInfo?.InvoiceeID.Length == 10)
				{
					return string.Format(CultureInfo.InvariantCulture, "{0}-{1}-{2}", AdditionalInfo?.InvoiceeID.Substring(0, 3), AdditionalInfo?.InvoiceeID.Substring(3, 2), AdditionalInfo?.InvoiceeID.Substring(5, 5));
				}
				else if (AdditionalInfo?.InvoiceeID.Length == 13)
				{
					return string.Format(CultureInfo.InvariantCulture, "{0}-{1}", AdditionalInfo?.InvoiceeID.Substring(0, 6), AdditionalInfo?.InvoiceeID.Substring(6, 7));
				}

				return AdditionalInfo?.InvoiceeID ?? ZString.Empty;
			}
		}

		public ZString InvoiceeNameText => AdditionalInfo?.InvoiceeNameText ?? ZString.Empty;
		public ZString InvoiceeSpecifiedPersonNameText => AdditionalInfo?.InvoiceeSpecifiedPersonNameText ?? ZString.Empty;
		public ZString InvoiceeSpecifiedAddressLineOneText => AdditionalInfo?.InvoiceeSpecifiedAddressLineOneText ?? ZString.Empty;
		public ZString InvoiceeTypeCode => AdditionalInfo?.InvoiceeTypeCode ?? ZString.Empty;
		public ZString InvoiceeClassificationCode => AdditionalInfo?.InvoiceeClassificationCode ?? ZString.Empty;
		public ZString InvoiceePrimaryDefinedContactURICommunication => AdditionalInfo?.InvoiceePrimaryDefinedContactURICommunication ?? ZString.Empty;
		public ZString InvoiceeSecondaryDefinedContactURICommunication => AdditionalInfo?.InvoiceeSecondaryDefinedContactURICommunication ?? ZString.Empty;

		public ZString InvoicerID
		{
			get
			{
				if (AdditionalInfo?.InvoicerID.Length == 10)
				{
					return string.Format(CultureInfo.InvariantCulture, "{0}-{1}-{2}", AdditionalInfo?.InvoicerID.Substring(0, 3), AdditionalInfo?.InvoicerID.Substring(3, 2), AdditionalInfo?.InvoicerID.Substring(5, 5));
				}

				return AdditionalInfo?.InvoicerID ?? ZString.Empty;
			}
		}

		public ZString InvoicerNameText => AdditionalInfo?.InvoicerNameText ?? ZString.Empty;
		public ZString InvoicerSpecifiedPersonNameText => AdditionalInfo?.InvoicerSpecifiedPersonNameText ?? ZString.Empty;
		public ZString InvoicerSpecifiedAddressLineOneText => AdditionalInfo?.InvoicerSpecifiedAddressLineOneText ?? ZString.Empty;
		public ZString InvoicerTypeCode => AdditionalInfo?.InvoicerTypeCode ?? ZString.Empty;
		public ZString InvoicerClassificationCode => AdditionalInfo?.InvoicerClassificationCode ?? ZString.Empty;
		public ZString InvoicerDefinedContactURICommunication => AdditionalInfo?.InvoicerDefinedContactURICommunication ?? ZString.Empty;
		public ZString TypeCode => AdditionalInfo?.FullTypeCode ?? ZString.Empty;

		readonly string fullyPaid = "01";
		readonly string unFullyPaid = "02";
		public ZString PaymentStatus
		{
			get
			{
				if (AdditionalInfo != null)
				{
					if (AdditionalInfo.PaymentStatus == fullyPaid)
					{
						return (NoResString)"이 금액을 영수 함";
					}
					if (AdditionalInfo.PaymentStatus == unFullyPaid)
					{
						return (NoResString)"이 금액을 청구 함";
					}
				}
				return ZString.Empty;
			}
		}

		public DocElectronicInvoiceLineCollection InvoiceLines
		{
			get
			{
				if (invoiceLines == null)
				{
					invoiceLines = new DocElectronicInvoiceLineCollection(Factory);
					AdditionalInfo?.Lines?.ForEach(line =>
					{
						invoiceLines.Add(DocElectronicInvoiceLine.New(line, Factory));
					});
				}
				return invoiceLines;
			}
		}
		DocElectronicInvoiceLineCollection invoiceLines;

		AdditionalInfo AdditionalInfo { get; }

		#endregion
	}
}
