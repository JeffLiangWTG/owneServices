using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Testing
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.Test.XmlSerializers")]
	sealed class CodeDescriptionWithThreeGroupsCollectionForTest : CodeDescriptionWithThreeGroupsCollection
	{
		public CodeDescriptionWithThreeGroupsCollectionForTest(int codeMaxLength) : base(codeMaxLength)
		{
		}

		public RegistryBusinessObjectCollectionTemplate GetCloneForTesting(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => GetClone(fallbackLevel, factory);
	}
}
