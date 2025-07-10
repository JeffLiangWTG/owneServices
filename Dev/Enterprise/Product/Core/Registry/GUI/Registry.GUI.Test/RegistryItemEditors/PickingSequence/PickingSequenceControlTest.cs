using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Warehouse;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(PickingSequenceControl))]
	sealed class PickingSequenceControlTest : RegistryBusinessObjectTemplateZUserControlTestCase
	{
		#region GetNewBusinessEntity

		protected override IBusiness GetNewBusinessEntity()
		{
			return new PickingSequence();
		}

		#endregion
	}
}
