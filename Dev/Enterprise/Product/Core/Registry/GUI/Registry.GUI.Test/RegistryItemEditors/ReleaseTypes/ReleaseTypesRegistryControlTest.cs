using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(ReleaseTypesRegistryControl))]
	sealed class ReleaseTypesRegistryControlTest : RegistryBusinessObjectTemplateZUserControlTest
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new ReleaseTypes();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			ReleaseTypesRegistryControl typesControl = (ReleaseTypesRegistryControl)control;
			return typesControl.ReleaseTypesGridForTest.ReadOnly;
		}
	}
}
