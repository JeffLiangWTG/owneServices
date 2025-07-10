using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(AutoratingViaPortControl))]
	sealed class AutoratingViaPortControlTest : RegistryBusinessObjectTemplateZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
			=> new AutoratingViaPortConfigurationCollection();

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
			=> ((AutoratingViaPortControl)control).IsControlOrBusinessEntityReadOnly;
	}
}
