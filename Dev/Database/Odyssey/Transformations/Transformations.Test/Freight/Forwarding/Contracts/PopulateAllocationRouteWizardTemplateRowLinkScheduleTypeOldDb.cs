using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Freight.Forwarding.Testing
{
	[TestedType(typeof(PopulateAllocationRouteWizardTemplateRowLinkScheduleType))]
	class PopulateAllocationRouteWizardTemplateRowLinkScheduleTypeOldDbTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new PopulateAllocationRouteWizardTemplateRowLinkScheduleType();
		}
	}
}
