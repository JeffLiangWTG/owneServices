using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.Registry
{
	[XmlSerializerAssembly("Enterprise.DocumentEngineCore.XmlSerializers")]
	public class PrincipalBrandingCollection : RegistryBusinessObjectCollection
	{
		public PrincipalBrandingCollection()
		{
		}

		public PrincipalBrandingCollection(BusinessObjectFactory factory, FallbackLevel fallbackLevel)
			: base(fallbackLevel, factory)
		{
		}

		public new PrincipalBranding this[int index]
		{
			get { return (PrincipalBranding)Elements[index]; }
		}

		public new PrincipalBranding AddNew()
		{
			return (PrincipalBranding)base.AddNew();
		}

		#region Overrides

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new PrincipalBranding(CurrentFactory, CurrentFallbackLevel);
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new PrincipalBrandingCollection(factory, fallbackLevel);
		}

		#endregion
	}
}
