using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(RegistryBusinessObjectTemplateZUserControl))]
	public class RegistryBusinessObjectTemplateZUserControlTest : RegistryBusinessObjectTemplateZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new RegistryBusinessObjectTemplateForTest();
		}
	}
}
