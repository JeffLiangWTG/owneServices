using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Testing
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.Test.XmlSerializers")]
	public class CodeDescriptionForRegistryBusinessObjectTest : CodeDescription<ZByte>
	{
		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CodeDescriptionForRegistryBusinessObjectTest();
		}

		protected override ZByte ValueFromString(string value)
		{
			return new ZByte(value);
		}
	}
}
