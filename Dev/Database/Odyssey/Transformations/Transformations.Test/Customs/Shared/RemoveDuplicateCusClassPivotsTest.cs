using System.Diagnostics.CodeAnalysis;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Customs.Shared;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Customs.Shared
{
	[TestedType(typeof(RemoveDuplicateCusClassPivots))]
	[SuppressMessage("Maintainability", "IDE0052: Remove unread private member.", Justification = "Make UT purpose clear")]
	public class RemoveDuplicateCusClassPivotsTest : DataTransformationTestCase
	{
		public void TestOldExtProperty()
		{
			ExtProperty.Database.Update(Db.Connection, LastProcessedPKExtendedPropertyString,
				"ffffffff-ffff-ffff-ffff-fffffffffffa");

			var transformation = GetNewTestTransformationInstance();
			transformation.Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);

			AssertNull($"ExtProperty '{LastProcessedPKExtendedPropertyString}' should be cleared", ExtProperty.Database.Select(Db.Connection, LastProcessedPKExtendedPropertyString));
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new RemoveDuplicateCusClassPivots();
		}

		protected override void PrepareTestData()
		{
		}

		protected override void AssertTransformationResults()
		{
		}

		const string LastProcessedPKExtendedPropertyString = "RemoveDuplicateCusClassPivots.LastProcessedCusClassificationPK";
	}
}
