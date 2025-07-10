using System.Xml.Linq;
using CargoWise.Common;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth
{
	class SpecifiedPaymentMeansTypeBuilder : XmlBuilder
	{
		public SpecifiedPaymentMeansTypeBuilder(XNamespace xNamespace, TransactionInfo transactionInfo) : base(xNamespace)
		{
			Argument.NotNull(transactionInfo, nameof(transactionInfo));

			TransactionInfo = transactionInfo;
			CommonTypeBuilder = new CommonTypeBuilder(xNamespace);
		}

		public XStreamingElement Build(string nodeName)
		{
			if (TransactionInfo.IsCreditNote())
			{
				return null;
			}

			return new XStreamingElement(GetTagName(nodeName)
				, BuildTaxInvoicePaymentMeansCodeType("TypeCode")
				, BuildTaxInvoicePaidAmountType("PaidAmount")
			);
		}

		XElement BuildTaxInvoicePaymentMeansCodeType(string nodeName)
		{
			return new XElement(GetTagName(nodeName), TransactionInfo.FullyPaidDate?.IsEmpty ?? true ? "40" : "10");
		}

		XElement BuildTaxInvoicePaidAmountType(string nodeName)
		{
			return TransactionInfo.LocalTotal.HasValue
				? CommonTypeBuilder.BuildTaxInvoicePaidAmountType(nodeName, TransactionInfo.LocalTotal.Value)
				: null;
		}

		CommonTypeBuilder CommonTypeBuilder { get; }

		TransactionInfo TransactionInfo { get; }
	}
}
