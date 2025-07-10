using System.Xml.Serialization;
using CargoWise.Types;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	[XmlTypeAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/")]
	[XmlRootAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/", IsNullable = false)]
	[ValueObjectSubclassAttribute("Enterprise.DataTransfer.Xml.XsdVersion1.AutoAWBRateLine")]
	public class AWBRateLine : Xsd.AutoAWBRateLine
	{
		[XmlIgnore]
		public override bool IsSpecified
		{
			get
			{
				return base.IsSpecified && (
					((NoOfPiecesOrRCP.IsNumbersOnlyOrEmpty && ZInt.ParseEmptyAsZero(NoOfPiecesOrRCP) != 0) || (!NoOfPiecesOrRCP.IsNumbersOnlyOrEmpty && !NoOfPiecesOrRCP.IsEmpty)) ||
					!CommodityItem.IsEmpty ||
					!RateClass.IsEmpty ||
					GrossWeight.Value != 0 ||
					CharageableWeight.Value != 0 ||
					RateOrCharge != 0
					);
			}
		}
	}
}
