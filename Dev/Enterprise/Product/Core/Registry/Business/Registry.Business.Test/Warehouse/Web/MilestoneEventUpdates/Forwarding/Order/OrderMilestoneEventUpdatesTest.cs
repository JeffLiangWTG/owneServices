using CargoWise.Types;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(OrderMilestoneEventUpdates))]
	sealed class OrderMilestoneEventUpdatesTest : MilestoneEventUpdatesTest
	{
		#region Overrides

		protected override MilestoneEventUpdatesCollection GetNewObjectTemplateCollection()
		{
			return new OrderMilestoneEventUpdatesCollection();
		}

		protected override MilestoneEventUpdates GetNewObjectTemplate()
		{
			return new OrderMilestoneEventUpdates();
		}

		protected override MilestoneEventUpdates GetNewObjectTemplate(ZString eventType)
		{
			return new OrderMilestoneEventUpdates(eventType);
		}

		protected override ZString GetExpectedWorkflowType()
		{
			return Constants.WebWorkflowType.Order;
		}

		#endregion
	}
}
