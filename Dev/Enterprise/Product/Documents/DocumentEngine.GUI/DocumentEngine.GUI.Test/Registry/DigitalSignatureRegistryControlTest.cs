using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.Registry.Testing
{
	[TestedType(typeof(DigitalSignatureRegistryControl))]
	sealed class DigitalSignatureRegistryControlTest : RegistryZUserControlTestCase
	{
		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity) => control.ReadOnly;

		protected override IBusiness GetNewBusinessEntity() => new DigitalSignatureRegistry();
	}
}
