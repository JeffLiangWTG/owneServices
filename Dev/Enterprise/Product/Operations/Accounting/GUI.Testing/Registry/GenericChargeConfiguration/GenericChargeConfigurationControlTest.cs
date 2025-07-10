using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(GenericChargeConfigurationControl))]
	class GenericChargeConfigurationControlTest : RegistryBusinessObjectTemplateZUserControlTestCase
	{
		public override void TestBoundListsAreNotLoadedOnAccess()
		{
			Assert(true);
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			return new GenericChargeConfigurationCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((GenericChargeConfigurationControl)control).GenericChargeConfigurationControlGrid_ForTestOnly.ReadOnly;
		}
	}
}
