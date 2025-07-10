using CargoWise.Types;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(LinerAndAgencyContainerMilestoneEventUpdates))]
	sealed class LinerAndAgencyContainerMilestoneEventUpdatesTest : MilestoneEventUpdatesTest
	{
		#region Overrides

		protected override MilestoneEventUpdatesCollection GetNewObjectTemplateCollection()
		{
			return new LinerAndAgencyContainerMilestoneEventUpdatesCollection();
		}

		protected override MilestoneEventUpdates GetNewObjectTemplate()
		{
			return new LinerAndAgencyContainerMilestoneEventUpdates();
		}

		protected override MilestoneEventUpdates GetNewObjectTemplate(ZString eventType)
		{
			return new LinerAndAgencyContainerMilestoneEventUpdates(eventType);
		}

		protected override ZString GetExpectedWorkflowType()
		{
			return Constants.WebWorkflowType.Container;
		}

		#endregion
	}
}
