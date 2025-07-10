using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Registry;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.GUI.Registry.Testing
{
	[TestedType(typeof(ExportStatusRequestRecipientRegistryItemControl))]
	class ExportStatusRequestRecipientRegistryItemControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity() => new ExportStatusRequestRecipientRegistryCollection();
		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity) => control.ReadOnly || businessEntity.IsReadOnly;
	}
}
