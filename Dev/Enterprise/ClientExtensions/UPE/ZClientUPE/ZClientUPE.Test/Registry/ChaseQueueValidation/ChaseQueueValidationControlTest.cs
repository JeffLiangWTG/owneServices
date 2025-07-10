using CargoWise.EntityFramework;
using Enterprise.Client.UPE.Registry.Business;
using Enterprise.Client.UPE.Registry.GUI;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.GUI.Registry.Testing
{
	[TestedType(typeof(ChaseQueueValidationControl))]
	internal class ChaseQueueValidationControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new ChaseQueueValidationCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			ChaseQueueValidationControl autoQueueMovementControl = (ChaseQueueValidationControl)control;
			return autoQueueMovementControl.ChaseQueueValidationGrid.ReadOnly;
		}
	}
}
