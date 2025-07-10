using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(EnableComplianceWiseRegistryControl))]
	sealed class EnableComplianceWiseRegistryControlTest : RegistryBusinessObjectTemplateZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new EnableComplianceWiseRegistryBusinessObject();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((EnableComplianceWiseRegistryControl)control).ReadOnly;
		}
	}
}
