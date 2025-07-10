using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(ElectronicProcessingChargeDescriptionOverrideControl))]
	public class ElectronicProcessingChargeDescriptionOverrideControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new ElectronicProcessingChargeDescriptionOverrideCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((ElectronicProcessingChargeDescriptionOverrideControl)control).ReadOnly;
		}
	}
}
