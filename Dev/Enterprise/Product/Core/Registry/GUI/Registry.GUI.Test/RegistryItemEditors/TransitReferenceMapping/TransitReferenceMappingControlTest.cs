using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Warehouse;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(TransitReferenceMappingControl))]
	sealed class TransitReferenceMappingControlTest : RegistryBusinessObjectTemplateZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new TransitReferenceMappingConfiguration();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((TransitReferenceMappingControl)control).TransitReferenceMappingGrid_ForTestOnly.ReadOnly;
		}
	}
}
