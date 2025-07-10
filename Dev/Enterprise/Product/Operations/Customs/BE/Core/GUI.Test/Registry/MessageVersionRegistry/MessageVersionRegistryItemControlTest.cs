using CargoWise.EntityFramework;
using Enterprise.Customs.BE.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.GUI.Registry.Testing;

[TestedType(typeof(MessageVersionRegistryItemControl))]
class MessageVersionRegistryItemControlTest : RegistryZUserControlTestCase
{
	protected override IBusiness GetNewBusinessEntity() => new MessageVersionRegistryCollection();

	protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity) => control.ReadOnly || businessEntity.IsReadOnly;
}
