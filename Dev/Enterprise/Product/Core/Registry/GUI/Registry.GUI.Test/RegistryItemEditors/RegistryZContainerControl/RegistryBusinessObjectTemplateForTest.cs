using CargoWise.EntityFramework;
using Enterprise.Registry.Business;

namespace Enterprise.Registry.GUI.Testing
{
	sealed class RegistryBusinessObjectTemplateForTest : RegistryBusinessObjectTemplate
	{
		protected override RegistryBusinessObjectTemplate GetClone(Enterprise.ZArchitecture.Environment.FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new RegistryBusinessObjectTemplateForTest();
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
		}
	}
}
