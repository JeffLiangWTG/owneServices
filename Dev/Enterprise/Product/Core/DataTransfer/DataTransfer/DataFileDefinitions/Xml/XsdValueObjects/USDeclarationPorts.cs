using System.Xml.Serialization;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	[XmlTypeAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/")]
	[XmlRootAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/", IsNullable = false)]
	[ValueObjectSubclassAttribute("Enterprise.DataTransfer.Xml.XsdVersion1.AutoUSDeclarationPorts")]
	public class USDeclarationPorts : Xsd.AutoUSDeclarationPorts
	{
		[XmlIgnore]
		public override bool IsSpecified
		{
			get
			{
				return base.IsSpecified &&
					(!DesignatedExamPort.IsEmpty
					|| !Discharge.IsEmpty
					|| !Loading.IsEmpty
					|| !PortOfEntry.IsEmpty
					|| !PreparerDistrictPort.IsEmpty);
			}
		}
	}
}
