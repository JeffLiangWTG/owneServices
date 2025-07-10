using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(AutomaticProcessRegistryControl))]
	sealed class AutomaticProcessRegistryControlTest : RegistryBusinessObjectTemplateZUserControlTestCase
	{
		#region Implementation

		protected override IBusiness GetNewBusinessEntity()
		{
			return new AutomaticProcessRegistryBusinessObject();
		}

		#endregion
	}
}
