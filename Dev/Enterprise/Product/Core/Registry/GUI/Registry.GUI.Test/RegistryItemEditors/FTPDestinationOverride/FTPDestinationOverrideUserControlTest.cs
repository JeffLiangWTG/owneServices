using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(FTPDestinationOverrideUserControl))]
	sealed class FTPDestinationOverrideUserControlTest : RegistryBusinessObjectTemplateZUserControlTestCase
	{
		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control1, IBusiness businessEntity)
		{
			var control = (FTPDestinationOverrideUserControl)control1;
			return control.ReadOnly;
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			return new FTPDestinationOverrideInfo();
		}
	}
}
