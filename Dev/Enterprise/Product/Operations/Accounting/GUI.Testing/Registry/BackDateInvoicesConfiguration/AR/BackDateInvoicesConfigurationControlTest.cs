using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(BackDateInvoicesConfigurationControl))]
	class BackDateInvoicesConfigurationControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new BackDateInvoicesConfiguration();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((BackDateInvoicesConfigurationControl)control).OverridePostDateCheckBox_ForTestOnly.ReadOnly &&
				   ((BackDateInvoicesConfigurationControl)control).DefaultPostDateFromInvoiceDateCheckBox_ForTestOnly.ReadOnly &&
				   ((BackDateInvoicesConfigurationControl)control).InvoiceDateConfigurationGrid_ForTestOnly.ReadOnly;
		}
	}
}
