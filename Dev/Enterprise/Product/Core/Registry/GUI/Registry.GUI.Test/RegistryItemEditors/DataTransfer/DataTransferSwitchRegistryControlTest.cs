using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(DataTransferSwitchRegistryControl))]
	sealed class DataTransferSwitchRegistryControlTest : RegistryBusinessObjectTemplateZUserControlTestCase
	{
		#region Implementation

		protected override IBusiness GetNewBusinessEntity()
		{
			return new DataTransferSwitchRegistryBusinessObject();
		}

		#endregion
	}
}
