using CargoWise.EntityFramework;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Security.ActiveDirectory.GUI.Test
{
	[TestedType(typeof(ADPasswordSettingsRegistryControl))]
	class ADPasswordSettingsRegistryControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity() => new ADPasswordSettingsRegistryBusinessObject();

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			var realControl = (ADPasswordSettingsRegistryControl)control;
			return realControl.ReadOnly;
		}
	}
}
