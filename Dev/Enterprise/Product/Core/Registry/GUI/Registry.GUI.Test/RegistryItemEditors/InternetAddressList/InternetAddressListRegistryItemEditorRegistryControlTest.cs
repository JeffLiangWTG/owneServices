using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(InternetAddressListRegistryControl))]
	sealed class InternetAddressListRegistryItemEditorRegistryControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new InternetAddressRuleset();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((InternetAddressListRegistryControl)control).Grid.ReadOnly;
		}
	}
}
