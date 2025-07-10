using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(AddressValidationDisabledCountryItemsControl))]
	sealed class AddressValidationDisabledCountryItemsControlTest : RegistryZUserControlTestCase
	{
		#region Implementation

		protected override CargoWise.EntityFramework.IBusiness GetNewBusinessEntity()
		{
			return new AddressValidationDisabledCountryItemCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, CargoWise.EntityFramework.IBusiness businessEntity)
		{
			return ((AddressValidationDisabledCountryItemsControl)control).AddressValidationDisabledCountryItemsGrid.ReadOnly;
		}

		#endregion

	}
}
