using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class OrgBarcodeMaskCollection : RegistryBusinessObjectCollectionTemplate
	{
		public OrgBarcodeMaskCollection()
			: base()
		{
		}

		public OrgBarcodeMaskCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public OrgBarcodeMaskCollection(FallbackLevel fallback, BusinessObjectFactory factory)
			: base(fallback, factory)
		{
		}

		public new OrgBarcodeMask this[int x]
		{
			get { return (OrgBarcodeMask)Elements[x]; }
		}

		public new OrgBarcodeMask AddNew()
		{
			return (OrgBarcodeMask)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new OrgBarcodeMaskCollection(fallbackLevel, factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new OrgBarcodeMask(CurrentFallbackLevel, CurrentFactory, this);
		}
	}
}
