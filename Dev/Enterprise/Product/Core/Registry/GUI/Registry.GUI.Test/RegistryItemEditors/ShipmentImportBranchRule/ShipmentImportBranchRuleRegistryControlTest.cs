using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(ShipmentImportBranchRuleRegistryControl))]
	sealed class ShipmentImportBranchRuleRegistryControlTest : RegistryBusinessObjectTemplateZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new ImportBranchRule();
		}
	}
}
