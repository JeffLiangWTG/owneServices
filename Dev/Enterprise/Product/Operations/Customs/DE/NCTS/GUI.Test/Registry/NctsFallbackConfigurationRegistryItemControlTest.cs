using CargoWise.EntityFramework;
using Enterprise.Customs.DE.NCTS.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.GUI.Testing;

[TestedType(typeof(NctsFallbackConfigurationRegistryItemControl))]
class NctsFallbackConfigurationRegistryItemControlTest : RegistryZUserControlTestCase
{
	protected override IBusiness GetNewBusinessEntity() => new NctsFallbackConfiguration();

	protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity) => control.ReadOnly || businessEntity.IsReadOnly;
}
