using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ClientSharedComponents.Registry.Testing
{
	[TestedType(typeof(ServiceTaskDataTransferSwitchRegistryControl))]
	internal class ServiceTaskDataTransferSwitchRegistryControlTest : RegistryBusinessObjectTemplateZUserControlTestCase
	{
		#region Implementation

		protected override IBusiness GetNewBusinessEntity()
		{
			return new DataTransferSwitchRegistryBusinessObject();
		}

		#endregion
	}
}
