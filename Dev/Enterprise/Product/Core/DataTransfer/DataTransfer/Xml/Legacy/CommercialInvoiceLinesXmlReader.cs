using System.ComponentModel;
using System.Xml;

namespace Enterprise.DataTransfer.Xml
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class CommercialInvoiceLinesXmlReader : XmlDocReader
	{
		public CommercialInvoiceLinesXmlReader(XmlNode commercialInvoiceLineNode, XmlDocument xmlDoc) : base(xmlDoc)
		{
			this.CommercialInvoiceLineNode = commercialInvoiceLineNode;
		}

		public string InvoiceQty
		{
			get { return GetValue(CommercialInvoiceLineNode, CommercialInvoiceXsd.XPath.InvoiceLine.InvoiceQty); }
		}

		public string InvoiceQtyUnit
		{
			get { return GetAttributeValue(CommercialInvoiceLineNode, CommercialInvoiceXsd.XPath.InvoiceLine.InvoiceQty, ElementsXsd.Attributes.DimensionType); }
		}

		public string LinePrice
		{
			get { return GetValue(CommercialInvoiceLineNode, CommercialInvoiceXsd.XPath.InvoiceLine.LinePrice); }
		}

		public string LinePriceCurrency
		{
			get { return GetAttributeValue(CommercialInvoiceLineNode, CommercialInvoiceXsd.XPath.InvoiceLine.LinePrice, ElementsXsd.Attributes.CurrencyCode); }
		}

		public string ProductNumber
		{
			get { return GetValue(CommercialInvoiceLineNode, CommercialInvoiceXsd.XPath.InvoiceLine.ProductNumber); }
		}

		public string ProductDescription
		{
			get { return GetValue(CommercialInvoiceLineNode, CommercialInvoiceXsd.XPath.InvoiceLine.ProductDescription); }
		}

		public string CustomsInvoiceQty
		{
			get { return GetValue(CommercialInvoiceLineNode, CommercialInvoiceXsd.XPath.InvoiceLine.CustomsInvoiceQty); }
		}

		public string CustomsInvoiceQtyUnit
		{
			get { return GetAttributeValue(CommercialInvoiceLineNode, CommercialInvoiceXsd.XPath.InvoiceLine.CustomsInvoiceQty, ElementsXsd.Attributes.DimensionType); }
		}

		public string OrderNumber
		{
			get { return GetValue(CommercialInvoiceLineNode, CommercialInvoiceXsd.XPath.InvoiceLine.OrderNumber); }
		}

		public string TariffCode
		{
			get { return GetValue(CommercialInvoiceLineNode, CommercialInvoiceXsd.XPath.InvoiceLine.TariffCode); }
		}

		public string TariffLookup
		{
			get { return GetValue(CommercialInvoiceLineNode, CommercialInvoiceXsd.XPath.InvoiceLine.TariffLookup); }
		}

		public string OriginOfGoods
		{
			get { return GetValue(CommercialInvoiceLineNode, CommercialInvoiceXsd.XPath.InvoiceLine.OriginOfGoods); }
		}

		public string TreatmentCode
		{
			get { return GetValue(CommercialInvoiceLineNode, CommercialInvoiceXsd.XPath.InvoiceLine.TreatmentCode); }
		}

		public string Preference
		{
			get { return GetValue(CommercialInvoiceLineNode, CommercialInvoiceXsd.XPath.InvoiceLine.Preference); }
		}

		public string Concession
		{
			get { return GetValue(CommercialInvoiceLineNode, CommercialInvoiceXsd.XPath.InvoiceLine.Concession); }
		}

		public string Volume
		{
			get { return GetValue(CommercialInvoiceLineNode, CommercialInvoiceXsd.XPath.InvoiceLine.Volume); }
		}

		public string VolumeUnit
		{
			get { return GetAttributeValue(CommercialInvoiceLineNode, CommercialInvoiceXsd.XPath.InvoiceLine.Volume, ElementsXsd.Attributes.DimensionType); }
		}

		public string Weight
		{
			get { return GetValue(CommercialInvoiceLineNode, CommercialInvoiceXsd.XPath.InvoiceLine.Weight); }
		}

		public string WeightUnit
		{
			get { return GetAttributeValue(CommercialInvoiceLineNode, CommercialInvoiceXsd.XPath.InvoiceLine.Weight, ElementsXsd.Attributes.DimensionType); }
		}

		public string CustomText1
		{
			get { return GetValue(CommercialInvoiceLineNode, CommercialInvoiceXsd.XPath.InvoiceLine.CustomText1); }
		}

		public string CustomText2
		{
			get { return GetValue(CommercialInvoiceLineNode, CommercialInvoiceXsd.XPath.InvoiceLine.CustomText2); }
		}

		public string CustomText3
		{
			get { return GetValue(CommercialInvoiceLineNode, CommercialInvoiceXsd.XPath.InvoiceLine.CustomText3); }
		}

		//
		//		public const string AddCustomsDetail = "AddCustomsDetails/AddCustomsDetail";
		//		public const string InvoiceCharge = "InvoiceCharges/InvoiceCharge";
		//		public const string InvoiceLine = "InvoiceLines/InvoiceLine";	

		protected XmlNode CommercialInvoiceLineNode;
	}
}
