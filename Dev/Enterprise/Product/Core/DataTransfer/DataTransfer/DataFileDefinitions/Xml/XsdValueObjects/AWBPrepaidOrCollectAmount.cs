using System.Xml.Serialization;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	[XmlTypeAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/")]
	[XmlRootAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/", IsNullable = false)]
	[ValueObjectSubclassAttribute("Enterprise.DataTransfer.Xml.XsdVersion1.AutoAWBPrepaidOrCollectAmount")]
	public class AWBPrepaidOrCollectAmount : Xsd.AutoAWBPrepaidOrCollectAmount
	{
		[XmlIgnore]
		public override bool IsSpecified
		{
			get
			{
				return base.IsSpecified && (Weight != 0 || Valuation != 0 || Tax != 0 || OtherChargeDueAgent != 0 || OtherChargeDueCarrier != 0);
			}
		}
	}
}
