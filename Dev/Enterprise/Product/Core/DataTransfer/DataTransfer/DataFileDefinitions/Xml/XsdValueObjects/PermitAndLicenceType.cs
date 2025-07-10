using System.Xml.Serialization;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	[XmlTypeAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/")]
	[XmlRootAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/", IsNullable = false)]
	[ValueObjectSubclassAttribute("Enterprise.DataTransfer.Xml.XsdVersion1.AutoPermitAndLicenceType")]
	public class PermitAndLicenceType : Xsd.AutoPermitAndLicenceType
	{
		public PermitAndLicenceType()
		{
		}

		[XmlIgnore]
		public override bool IsSpecified
		{
			get
			{
				return !AgricultureLicNo.IsEmpty
					|| !CAExportCertificate.IsEmpty
					|| !CBTPACertificate.IsEmpty
					|| !MiscPermitNo.IsEmpty
					|| !WoolLicenceNo.IsEmpty
					|| !SWPMIndicator.IsEmpty
					|| !CottonCertificateNo.IsEmpty
					|| !CottonFeeExemptIndicator.IsEmpty;
			}
			set { base.IsSpecified = value; }
		}
	}
}
