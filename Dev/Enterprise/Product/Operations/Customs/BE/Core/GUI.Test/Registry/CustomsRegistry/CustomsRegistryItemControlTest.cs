using CargoWise.EntityFramework;
using Enterprise.Customs.BE.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.GUI.Registry.Testing;

[TestedType(typeof(CustomsRegistryItemControl))]
class CustomsRegistryItemControlTest : RegistryZUserControlTestCase
{
	protected override IBusiness GetNewBusinessEntity() => new CustomsRegistryCollection();

	protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity) => control.ReadOnly || businessEntity.IsReadOnly;
}
