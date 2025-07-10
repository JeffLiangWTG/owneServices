using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(ChargeCodeForPricingPageSectionsControl))]
	sealed class ChargeCodeForPricingPageSectionsControlTest : RegistryBusinessObjectTemplateZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new ChargeCodeForPricingPageSectionsConfigurationCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			var locationsChargesControl = (ChargeCodeForPricingPageSectionsControl)control;

			return locationsChargesControl.ConfigurationGrid.ReadOnly && locationsChargesControl.ChargesGrid.ReadOnly;
		}
	}
}
