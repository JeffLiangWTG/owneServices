using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Testing
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.Test.XmlSerializers")]
	sealed class CodeDescriptionWithGroupCollectionForTest : CodeDescriptionWithGroupCollection
	{
		public CodeDescriptionWithGroupCollectionForTest(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		public RegistryBusinessObjectCollectionTemplate GetCloneForTesting(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => GetClone(fallbackLevel, factory);
	}
}
