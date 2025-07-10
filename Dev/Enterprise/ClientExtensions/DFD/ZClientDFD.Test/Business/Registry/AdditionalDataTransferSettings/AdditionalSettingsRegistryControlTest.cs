using CargoWise.EntityFramework;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.DFD.Registry.Testing
{
	[TestedType(typeof(AdditionalSettingsRegistryControl))]
	internal class AdditionalSettingsRegistryControlTest : RegistryBusinessObjectTemplateZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new AdditionalSettingsRegistryBusinessObject();
		}
	}
}
