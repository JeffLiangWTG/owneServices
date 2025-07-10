using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Web
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class OrgsRoleAccessCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new OrgsRoleAccess AddNew()
		{
			return (OrgsRoleAccess)base.AddNew();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public new OrgsRoleAccess this[int index]
		{
			get { return (OrgsRoleAccess)Elements[index]; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new OrgsRoleAccess();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new OrgsRoleAccessCollection();
		}
	}
}
