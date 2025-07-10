using CargoWise.EntityFramework;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.Security.ActiveDirectory.GUI.Registry;
using NUnit.Framework;

namespace Enterprise.Security.ActiveDirectory.GUI.Test
{
	[TestedType(typeof(ADRegistryControl))]
	class ADRegistryControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new ADConfig();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return control.ReadOnly;
		}
	}
}
