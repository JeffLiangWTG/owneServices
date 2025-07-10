using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(AddressListControl))]
	sealed class AddressListControlTest : RegistryZUserControlTestCase
	{
		protected override RegistryZUserControl GetNewControl()
		{
			return new AddressListControl();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((AddressListControl)control).ReadOnly;
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			var collection = new AddressListCollection();
			var address = collection.AddNew();

			return collection;
		}
	}
}
