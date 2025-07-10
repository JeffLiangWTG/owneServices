using System.Xml.Linq;
using CargoWise.Common;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth
{
	class SpecifiedMonetarySummationTypeBuilder : XmlBuilder
	{
		public SpecifiedMonetarySummationTypeBuilder(XNamespace xNamespace, TransactionInfo transactionInfo) : base(xNamespace)
		{
			Argument.NotNull(transactionInfo, nameof(transactionInfo));

			TransactionInfo = transactionInfo;
			CommonTypeBuilder = new CommonTypeBuilder(xNamespace);
		}

		public XStreamingElement Build(string nodeName)
		{
			return new XStreamingElement(GetTagName(nodeName)
				, CommonTypeBuilder.BuildTaxInvoiceAmountNoFracType("ChargeTotalAmount", TransactionInfo.LocalExVATAmount.Value)
				, TransactionInfo.LocalVATAmount.HasValue
					? CommonTypeBuilder.BuildTaxInvoiceAmountNoFracType("TaxTotalAmount", TransactionInfo.LocalVATAmount.Value)
					: null
				, CommonTypeBuilder.BuildTaxInvoiceAmountNoFracType("GrandTotalAmount", TransactionInfo.LocalTotal.Value)
				);
		}

		CommonTypeBuilder CommonTypeBuilder { get; }

		TransactionInfo TransactionInfo { get; }
	}
}
