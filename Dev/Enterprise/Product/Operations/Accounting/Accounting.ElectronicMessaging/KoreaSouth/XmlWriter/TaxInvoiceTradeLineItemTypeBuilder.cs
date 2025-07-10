using System;
using System.Xml.Linq;
using CargoWise.Common;
using Enterprise.Accounting.Business.EInvoicing.KoreaSouth;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth
{
	class TaxInvoiceTradeLineItemTypeBuilder : XmlBuilder
	{
		public TaxInvoiceTradeLineItemTypeBuilder(XNamespace xNamespace, TaxInvoiceTradeLineItem line) : base(xNamespace)
		{
			Argument.NotNull(line, nameof(line));

			Line = line;
			CommonTypeBuilder = new Lazy<CommonTypeBuilder>(() => new CommonTypeBuilder(xNamespace));
		}

		public XStreamingElement BuildXML(string tagName)
		{
			return new XStreamingElement(GetTagName(tagName)
				, BuildSequenceNumeric()
				, BuildDescriptionText()
				, BuildInvoiceAmount()
				, BuildChargeableUnitQuantity()
				, BuildInformationText()
				, BuildNameText()
				, BuildPurchaseExpiryDateTime()
				, BuildTotalTax()
				, BuildUnitPrice()
			);
		}

		XElement BuildSequenceNumeric()
		{
			return new XElement(GetTagName("SequenceNumeric"), $"{Line.Sequence}");
		}

		XElement BuildDescriptionText()
		{
			return null;
		}

		XElement BuildInvoiceAmount()
		{
			return CommonTypeBuilder.Value.BuildTaxInvoiceFreeTextType("InvoiceAmount", Line.InvoiceAmount);
		}

		XElement BuildChargeableUnitQuantity()
		{
			return null;
		}

		XElement BuildInformationText()
		{
			return null;
		}

		XElement BuildNameText()
		{
			return CommonTypeBuilder.Value.BuildTaxInvoiceFreeTextType("NameText", Line.DescriptionText);
		}

		XElement BuildPurchaseExpiryDateTime()
		{
			return Line != null && !Line.ReverseDate.IsEmpty
				? new XElement(GetTagName("PurchaseExpiryDateTime"), Line.ReverseDate.ToString("yyyyMMdd"))
				: null;
		}

		XStreamingElement BuildTotalTax()
		{
			return new XStreamingElement(GetTagName("TotalTax")
				, CommonTypeBuilder.Value.BuildTaxInvoiceFreeTextType("CalculatedAmount", Line.CalculatedAmount)
				);
		}

		XStreamingElement BuildUnitPrice()
		{
			return null;
		}

		TaxInvoiceTradeLineItem Line { get; }

		Lazy<CommonTypeBuilder> CommonTypeBuilder { get; }
	}
}
