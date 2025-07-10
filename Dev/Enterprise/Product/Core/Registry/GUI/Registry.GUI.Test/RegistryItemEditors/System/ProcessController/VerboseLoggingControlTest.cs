using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.RegistryItemEditors.System.ProcessController.Testing
{
	[TestedType(typeof(VerboseLoggingControl))]
	public class VerboseLoggingControlTest : RegistryBusinessObjectTemplateZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new VerboseLoggingCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((VerboseLoggingControl)control).Grid.ReadOnly;
		}
	}
}
