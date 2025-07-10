using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.Registry
{
	[XmlSerializerAssembly("Enterprise.DocumentEngineCore.XmlSerializers")]
	public class DocumentSigningServicePartnerCredentials : AutoDocumentSigningServicePartnerCredentials
	{
		public override void ValidatePartnerID()
		{
			base.ValidatePartnerID();
			MandatoryValidation.CheckEntered(PartnerIDInfo);
		}

		public override void ValidatePartnerAccessKey()
		{
			base.ValidatePartnerAccessKey();
			MandatoryValidation.CheckEntered(PartnerAccessKeyInfo);
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DocumentSigningServicePartnerCredentials();
		}
	}
}
