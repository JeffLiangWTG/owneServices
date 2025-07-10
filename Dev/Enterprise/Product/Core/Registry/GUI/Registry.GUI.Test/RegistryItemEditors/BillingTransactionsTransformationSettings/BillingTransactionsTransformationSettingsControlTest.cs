using CargoWise.EntityFramework;
using Enterprise.Registry.Business.eHub;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.eHub.Testing
{
	[TestedType(typeof(BillingTransactionsTransformationSettingsControl))]
	sealed class BillingTransactionsTransformationSettingsControlTest : RegistryZUserControlTestCase
	{
		protected override RegistryZUserControl GetNewControl()
		{
			return new BillingTransactionsTransformationSettingsControl();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return control.ReadOnly;
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			return new BillingTransactionsTransformationSettings();
		}
	}
}
