using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Testing
{
	sealed class DummyRegistryBusinessObjectTemplateWithChildCollection : RegistryBusinessObjectTemplateWithChildCollection
	{
		public DummyRegistryBusinessObjectTemplateWithChildCollection()
		{
			Collection = new DummyRegistryBusinessObjectCollection();
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			throw new System.NotImplementedException();
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			throw new System.NotImplementedException();
		}

		protected override IEnumerable<RegistryBusinessObjectCollectionTemplate> ChildCollections
		{
			get { return new[] { Collection }; }
		}

		public DummyRegistryBusinessObjectCollection Collection { get; set; }
	}
}
