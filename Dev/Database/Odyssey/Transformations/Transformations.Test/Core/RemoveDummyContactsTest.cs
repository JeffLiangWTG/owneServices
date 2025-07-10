using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Core.Testing
{
	[TestedType(typeof(RemoveDummyContacts))]
	class RemoveDummyContactsTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			AssertNull(ExtProperty.Database.Select(Db.Connection, "RemoveDummyContacts.LastProcessedChunkPK"));
			AssertNull(ExtProperty.Database.Select(Db.Connection, "RemoveDummyContacts.TransformationRunToCompletion"));
			AssertNull(ExtProperty.Database.Select(Db.Connection, "RemoveDummyContacts.TransformationProcessedCount"));
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new RemoveDummyContacts();
		}
	}
}
