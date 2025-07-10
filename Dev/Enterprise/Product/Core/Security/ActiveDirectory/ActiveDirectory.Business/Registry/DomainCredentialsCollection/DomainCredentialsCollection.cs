using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Security.ActiveDirectory
{
	[XmlSerializerAssembly("Enterprise.Security.ActiveDirectory.XmlSerializers")]
	public class DomainCredentialsCollection : RegistryBusinessObjectCollectionTemplate<DomainCredentials>
	{
		public DomainCredentialsCollection() : base(null, null)
		{
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new DomainCredentialsCollection();

		protected override BusinessObject CreateNonPersistentBusinessObject() => new DomainCredentials();

		public DomainCredentials DefaultDomainCredentials => this.Cast<DomainCredentials>().FirstOrDefault(dc => dc.IsDefaultDomain);
	}
}
