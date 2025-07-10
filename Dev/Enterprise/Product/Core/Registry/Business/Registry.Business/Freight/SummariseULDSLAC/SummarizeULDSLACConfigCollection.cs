using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class SummarizeULDSLACConfigCollection : RegistryBusinessObjectCollectionTemplate
	{
		public SummarizeULDSLACConfigCollection()
		{
		}

		public SummarizeULDSLACConfigCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new SummarizeULDSLACConfig this[int index]
		{
			get { return (SummarizeULDSLACConfig)Elements[index]; }
		}

		public new SummarizeULDSLACConfig AddNew()
		{
			return (SummarizeULDSLACConfig)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new SummarizeULDSLACConfig();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new SummarizeULDSLACConfigCollection();
		}
	}
}
