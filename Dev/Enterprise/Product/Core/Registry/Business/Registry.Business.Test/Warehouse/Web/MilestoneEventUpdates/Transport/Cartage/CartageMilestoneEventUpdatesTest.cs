using CargoWise.Types;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CartageMilestoneEventUpdates))]
	sealed class CartageMilestoneEventUpdatesTest : MilestoneEventUpdatesTest
	{
		#region Overrides

		protected override MilestoneEventUpdatesCollection GetNewObjectTemplateCollection()
		{
			return new CartageMilestoneEventUpdatesCollection();
		}

		protected override MilestoneEventUpdates GetNewObjectTemplate()
		{
			return new CartageMilestoneEventUpdates();
		}

		protected override MilestoneEventUpdates GetNewObjectTemplate(ZString eventType)
		{
			return new CartageMilestoneEventUpdates(eventType);
		}

		protected override ZString GetExpectedWorkflowType()
		{
			return Constants.WebWorkflowType.Cartage;
		}

		#endregion
	}
}
