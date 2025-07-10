using CargoWise.Types;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(DeclarationMilestoneEventUpdates))]
	sealed class DeclarationMilestoneEventUpdatesTest : MilestoneEventUpdatesTest
	{
		#region Overrides

		protected override MilestoneEventUpdatesCollection GetNewObjectTemplateCollection()
		{
			return new DeclarationMilestoneEventUpdatesCollection();
		}

		protected override MilestoneEventUpdates GetNewObjectTemplate()
		{
			return new DeclarationMilestoneEventUpdates();
		}

		protected override MilestoneEventUpdates GetNewObjectTemplate(ZString eventType)
		{
			return new DeclarationMilestoneEventUpdates(eventType);
		}

		protected override ZString GetExpectedWorkflowType()
		{
			return Constants.WebWorkflowType.Declaration;
		}

		#endregion
	}
}
