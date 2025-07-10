using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Testing
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.Test.XmlSerializers")]
	sealed class CodeDescriptionBoolCollectionForTest : CodeDescriptionBoolCollection
	{
		public CodeDescriptionBoolCollectionForTest(FallbackLevel fallbackLevel) : base(fallbackLevel)
		{
		}

		public RegistryBusinessObjectCollectionTemplate GetCloneForTesting(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => GetClone(fallbackLevel, factory);
		public CodeDescriptionBoolCollection GetNewCollectionForTesting() => GetNewCollection();
	}
}
