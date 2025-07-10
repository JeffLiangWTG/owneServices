using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(ActivitySubtypeAssignmentCollection))]
	internal sealed class ActivitySubtypeAssignmentCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<ActivitySubtypeAssignmentCollection>
	{
		#region Implementation

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override ActivitySubtypeAssignmentCollection GetCollectionToTest()
		{
			return new ActivitySubtypeAssignmentCollection(NewFallbackLevel(), Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ActivitySubtypeAssignment(NewFallbackLevel(), Factory);
		}

		#endregion
	}
}
