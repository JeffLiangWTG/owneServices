using System.Xml.Serialization;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssembly("Enterprise.DataTransfer.XmlSerializers")]
	[XmlType(Namespace = "http://www.edi.com.au/EnterpriseService/")]
	[XmlRoot(Namespace = "http://www.edi.com.au/EnterpriseService/", IsNullable = false)]
	[ValueObjectSubclass("Enterprise.DataTransfer.Xml.XsdVersion1.AutoUSDeclarationOrganisationsImporterOfRecord")]
	public class USDeclarationOrganisationsImporterOfRecord : AutoUSDeclarationOrganisationsImporterOfRecord
	{
		[XmlIgnore]
		public override bool IsSpecified
		{
			get
			{
				bool result = base.IsSpecified;

				if (Item is string)
				{
					result &= !string.IsNullOrEmpty(Item.ToString());
				}
				else if (Item is Organisation)
				{
					result &= ((Organisation)Item).IsSpecified;
				}
				else
				{
					result = false;
				}

				return result;
			}
		}
	}
}
