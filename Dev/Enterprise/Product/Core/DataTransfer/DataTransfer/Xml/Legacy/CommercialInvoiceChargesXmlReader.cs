using System.ComponentModel;
using System.Xml;

namespace Enterprise.DataTransfer.Xml
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class CommercialInvoiceChargesXmlReader : XmlDocReader
	{
		public CommercialInvoiceChargesXmlReader(XmlNode commercialInvoiceChargesNode, XmlDocument xmlDoc) : base(xmlDoc)
		{
			this.CommercialInvoiceChargesNode = commercialInvoiceChargesNode;
		}

		public string ChargeType
		{
			get { return GetValue(CommercialInvoiceChargesNode, CommercialInvoiceXsd.XPath.InvoiceCharge.ChargeType); }
		}

		public string ChargeValue
		{
			get { return GetValue(CommercialInvoiceChargesNode, CommercialInvoiceXsd.XPath.InvoiceCharge.ChargeValue); }
		}

		public string ChargeValueCurrency
		{
			get { return GetAttributeValue(CommercialInvoiceChargesNode, CommercialInvoiceXsd.XPath.InvoiceCharge.ChargeValue, ElementsXsd.Attributes.CurrencyCode); }
		}

		public string GSTApplies
		{
			get { return GetValue(CommercialInvoiceChargesNode, CommercialInvoiceXsd.XPath.InvoiceCharge.GSTApplies); }
		}

		public string DutyApplies
		{
			get { return GetValue(CommercialInvoiceChargesNode, CommercialInvoiceXsd.XPath.InvoiceCharge.DutyApplies); }
		}

		public string IsIncludedInTotal
		{
			get { return GetValue(CommercialInvoiceChargesNode, CommercialInvoiceXsd.XPath.InvoiceCharge.IsIncludedInTotal); }
		}

		protected XmlNode CommercialInvoiceChargesNode;
	}
}
