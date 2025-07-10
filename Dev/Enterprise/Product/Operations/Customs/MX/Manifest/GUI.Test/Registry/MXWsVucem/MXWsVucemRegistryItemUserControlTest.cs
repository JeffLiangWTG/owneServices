using CargoWise.EntityFramework;
using Enterprise.Customs.MX.Manifest.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Manifest.GUI.Testing
{
	[TestedType(typeof(MXWsVucemRegistryItemUserControl))]
	class MXWsVucemRegistryItemUserControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity() => new MXWsVucem();

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity) => ((MXWsVucemRegistryItemUserControl)control).ReadOnly;
	}
}
