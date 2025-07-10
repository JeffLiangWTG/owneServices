using System.Xml.Serialization;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	[XmlTypeAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/")]
	[XmlRootAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/", IsNullable = false)]
	[ValueObjectSubclassAttribute("Enterprise.DataTransfer.Xml.XsdVersion1.AutoUSOrganisationSpecificDetailsReconciliation")]
	public class USOrganisationSpecificDetailsReconciliation : AutoUSOrganisationSpecificDetailsReconciliation
	{
		[XmlIgnore]
		public override bool IsSpecified
		{
			get
			{
				return NAFTASpecified ||
					IssueSpecified || FileTheirOwnReconSpecified;
			}
		}
	}
}
