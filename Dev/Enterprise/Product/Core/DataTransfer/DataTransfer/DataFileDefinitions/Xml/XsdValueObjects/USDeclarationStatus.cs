using System.Xml.Serialization;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	[XmlTypeAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/")]
	[XmlRootAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/", IsNullable = false)]
	[ValueObjectSubclassAttribute("Enterprise.DataTransfer.Xml.XsdVersion1.AutoUSDeclarationStatus")]
	public class USDeclarationStatus : Xsd.AutoUSDeclarationStatus
	{
		[XmlIgnore]
		public override bool IsSpecified
		{
			get
			{
				return base.IsSpecified &&
					(!DutyDueDate.IsEmpty
					|| !EntryDate.IsEmpty
					|| !LiquidationDate.IsEmpty
					|| LiquidationTypeSpecified
					|| Dispositions.Count > 0
					|| OGADispositions.Count > 0
					);
			}
		}
	}
}
