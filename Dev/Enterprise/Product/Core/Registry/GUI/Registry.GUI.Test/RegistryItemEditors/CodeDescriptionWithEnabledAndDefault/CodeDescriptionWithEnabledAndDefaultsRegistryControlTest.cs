using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(CodeDescriptionWithEnabledAndDefaultsRegistryControl))]
	sealed class CodeDescriptionWithEnabledAndDefaultsRegistryControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new CodeDescriptionWithEnabledAndDefaultCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((CodeDescriptionWithEnabledAndDefaultsRegistryControl)control).Grid.ReadOnly;
		}
	}
}
