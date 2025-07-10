using System.Xml.Serialization;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	[XmlTypeAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/")]
	[XmlRootAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/", IsNullable = false)]
	[ValueObjectSubclassAttribute("Enterprise.DataTransfer.Xml.XsdVersion1.AutoUSDeclarationBond")]
	public class USDeclarationBond : Xsd.AutoUSDeclarationBond
	{
		[XmlIgnore]
		public override bool IsSpecified
		{
			get
			{
				return base.IsSpecified &&
					(!AccountNumber.IsEmpty
					|| !SuretyCode.IsEmpty
					|| !ADDCVDSuretyCode.IsEmpty
					|| Amount > 0
					|| TypeSpecified
					);
			}
		}
	}
}
