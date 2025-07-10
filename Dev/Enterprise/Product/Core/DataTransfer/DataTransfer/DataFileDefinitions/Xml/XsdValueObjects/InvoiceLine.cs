using System.Xml.Serialization;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	[XmlTypeAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/")]
	[XmlRootAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/", IsNullable = false)]
	[ValueObjectSubclassAttribute("Enterprise.DataTransfer.Xml.XsdVersion1.AutoInvoiceLine")]
	public class InvoiceLine : Xsd.AutoInvoiceLine
	{
		[XmlIgnoreAttribute]
		public override bool CustomDecimal1Specified
		{
			get { return !base.CustomDecimal1.IsEmpty; }
			set { base.CustomDecimal1Specified = value; }
		}

		[XmlIgnoreAttribute]
		public override bool CustomDecimal2Specified
		{
			get { return !base.CustomDecimal2.IsEmpty; }
			set { base.CustomDecimal2Specified = value; }
		}

		[XmlIgnoreAttribute]
		public override bool CustomDecimal3Specified
		{
			get { return !base.CustomDecimal3.IsEmpty; }
			set { base.CustomDecimal3Specified = value; }
		}

		[XmlIgnoreAttribute]
		public override bool CustomFlag1Specified
		{
			get { return true; }
			set { base.CustomFlag1Specified = value; }
		}

		[XmlIgnoreAttribute]
		public override bool CustomFlag2Specified
		{
			get { return true; }
			set { base.CustomFlag2Specified = value; }
		}

		[XmlIgnoreAttribute]
		public override bool CustomFlag3Specified
		{
			get { return true; }
			set { base.CustomFlag3Specified = value; }
		}

		[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
		[XmlIgnoreAttribute]
		public new bool LinePriceSpecified
		{
			get
			{
				return true;
			}
			set
			{
				this.LinePrice.IsSpecified = value;
			}
		}
	}
}
