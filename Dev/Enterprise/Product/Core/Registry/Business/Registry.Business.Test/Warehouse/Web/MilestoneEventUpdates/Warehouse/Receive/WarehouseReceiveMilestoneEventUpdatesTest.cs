using CargoWise.Types;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(WarehouseReceiveMilestoneEventUpdates))]
	sealed class WarehouseReceiveMilestoneEventUpdatesTest : MilestoneEventUpdatesTest
	{
		#region Overrides

		protected override MilestoneEventUpdatesCollection GetNewObjectTemplateCollection()
		{
			return new WarehouseReceiveMilestoneEventUpdatesCollection();
		}

		protected override MilestoneEventUpdates GetNewObjectTemplate()
		{
			return new WarehouseReceiveMilestoneEventUpdates();
		}

		protected override MilestoneEventUpdates GetNewObjectTemplate(ZString eventType)
		{
			return new WarehouseReceiveMilestoneEventUpdates(eventType);
		}

		protected override ZString GetExpectedWorkflowType()
		{
			return Constants.WebWorkflowType.WarehouseReceive;
		}

		#endregion
	}
}
