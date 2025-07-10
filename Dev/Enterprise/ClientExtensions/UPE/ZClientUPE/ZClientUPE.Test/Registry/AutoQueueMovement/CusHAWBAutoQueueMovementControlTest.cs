using CargoWise.EntityFramework;
using Enterprise.Client.UPE.Registry.Business;
using Enterprise.Client.UPE.Registry.GUI;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.GUI.Registry.Testing
{
	[TestedType(typeof(CusHAWBAutoQueueMovementControl))]
	internal class CusHAWBAutoQueueMovementControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new CusHAWBAutoQueueMovementCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			CusHAWBAutoQueueMovementControl autoQueueMovementControl = (CusHAWBAutoQueueMovementControl)control;
			return autoQueueMovementControl.QueueMovementGrid.ReadOnly;
		}
	}
}
