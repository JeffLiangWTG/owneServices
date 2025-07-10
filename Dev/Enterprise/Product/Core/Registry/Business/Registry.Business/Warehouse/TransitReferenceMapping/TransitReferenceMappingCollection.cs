using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Warehouse
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class TransitReferenceMappingCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new TransitReferenceMapping this[int i]
		{
			get { return SetParent((TransitReferenceMapping)Elements[i]); }
		}

		public new TransitReferenceMapping AddNew()
		{
			return SetParent((TransitReferenceMapping)base.AddNew());
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new TransitReferenceMappingCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return SetParent(new TransitReferenceMapping());
		}

		TransitReferenceMapping SetParent(TransitReferenceMapping mapping)
		{
			mapping.ParentCollection = this;
			return mapping;
		}
	}
}
