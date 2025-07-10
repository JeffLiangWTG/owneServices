using System.ComponentModel;
using System.Xml;
using CargoWise.Types;

namespace Enterprise.DataTransfer.Xml
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class CommercialInvoiceXmlReader : XmlDocReader
	{
		public CommercialInvoiceXmlReader(XmlNode commercialInvoiceHeaderNode, XmlDocument xmlDoc) : base(xmlDoc)
		{
			this.CommercialInvoiceHeaderNode = commercialInvoiceHeaderNode;
		}

		public string InvoiceNumber
		{
			get { return GetValue(CommercialInvoiceHeaderNode, CommercialInvoiceXsd.XPath.InvoiceHeader.InvoiceNumber); }
		}

		public string InvoiceAmount
		{
			get { return GetValue(CommercialInvoiceHeaderNode, CommercialInvoiceXsd.XPath.InvoiceHeader.InvoiceAmount); }
		}

		public string InvoiceAmountCurrency
		{
			get { return GetAttributeValue(CommercialInvoiceHeaderNode, CommercialInvoiceXsd.XPath.InvoiceHeader.InvoiceAmount, ElementsXsd.Attributes.CurrencyCode); }
		}

		public ZDateTime InvoiceDate
		{
			get { return GetValueAsDateTime(CommercialInvoiceHeaderNode, CommercialInvoiceXsd.XPath.InvoiceHeader.InvoiceDate); }
		}

		public ZDateTime ValuationDate
		{
			get { return GetValueAsDateTime(CommercialInvoiceHeaderNode, CommercialInvoiceXsd.XPath.InvoiceHeader.ValuationDate); }
		}

		public XmlNode ConsignorNode
		{
			get { return CommercialInvoiceHeaderNode.SelectSingleNode(CommercialInvoiceXsd.XPath.InvoiceHeader.Consignor); }
		}

		public string IsGroupInvoice
		{
			get { return GetValue(CommercialInvoiceHeaderNode, CommercialInvoiceXsd.XPath.InvoiceHeader.IsGroupInvoice); }
		}

		public string RelatedGroupInvoiceNumber
		{
			get { return GetValue(CommercialInvoiceHeaderNode, CommercialInvoiceXsd.XPath.InvoiceHeader.RelatedGroupInvoiceNumber); }
		}

		public string IncoTerm
		{
			get { return GetValue(CommercialInvoiceHeaderNode, CommercialInvoiceXsd.XPath.InvoiceHeader.IncoTerm); }
		}

		public string Volume
		{
			get { return GetValue(CommercialInvoiceHeaderNode, CommercialInvoiceXsd.XPath.InvoiceHeader.Volume); }
		}

		public string VolumeUnit
		{
			get { return GetAttributeValue(CommercialInvoiceHeaderNode, CommercialInvoiceXsd.XPath.InvoiceHeader.Volume, ElementsXsd.Attributes.DimensionType); }
		}

		public string Weight
		{
			get { return GetValue(CommercialInvoiceHeaderNode, CommercialInvoiceXsd.XPath.InvoiceHeader.Weight); }
		}

		public string WeightUnit
		{
			get { return GetAttributeValue(CommercialInvoiceHeaderNode, CommercialInvoiceXsd.XPath.InvoiceHeader.Weight, ElementsXsd.Attributes.DimensionType); }
		}

		//		public const string AddCustomsDetail = "AddCustomsDetails/AddCustomsDetail";

		public CommercialInvoiceChargesXmlReader[] CommercialInvoiceChargesXmlReaders
		{
			get
			{
				XmlNodeList commercialInvoiceChargesNodes = CommercialInvoiceHeaderNode.SelectNodes(CommercialInvoiceXsd.XPath.InvoiceHeader.InvoiceCharge);
				CommercialInvoiceChargesXmlReader[] result = new CommercialInvoiceChargesXmlReader[commercialInvoiceChargesNodes.Count];
				for (int i = 0; i < commercialInvoiceChargesNodes.Count; i++)
				{
					result[i] = new CommercialInvoiceChargesXmlReader(commercialInvoiceChargesNodes[i], XmlDoc);
				}

				return result;
			}
		}

		public CommercialInvoiceLinesXmlReader[] CommercialInvoiceLinesXmlReaders
		{
			get
			{
				XmlNodeList commercialInvoiceLinesNodes = CommercialInvoiceHeaderNode.SelectNodes(CommercialInvoiceXsd.XPath.InvoiceHeader.InvoiceLine);
				CommercialInvoiceLinesXmlReader[] result = new CommercialInvoiceLinesXmlReader[commercialInvoiceLinesNodes.Count];
				for (int i = 0; i < commercialInvoiceLinesNodes.Count; i++)
				{
					result[i] = new CommercialInvoiceLinesXmlReader(commercialInvoiceLinesNodes[i], XmlDoc);
				}

				return result;
			}
		}

		protected XmlNode CommercialInvoiceHeaderNode;
	}
}
