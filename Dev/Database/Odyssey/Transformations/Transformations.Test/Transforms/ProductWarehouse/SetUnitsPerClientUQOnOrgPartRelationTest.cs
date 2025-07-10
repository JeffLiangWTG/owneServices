using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.ProductWarehouse
{
	[TestedType(typeof(SetUnitsPerClientUQOnOrgPartRelation))]
	class SetUnitsPerClientUQOnOrgPartRelationTest : DataTransformationTestCase
	{
		public void TestOldCancellationAndExtProperty()
		{
			ExtProperty.Database.Update(Db.Connection, OldStoredFromPKName, "ffffffff-ffff-ffff-ffff-fffffffffffa");
			ExtProperty.Database.Update(Db.Connection, OldStoredTotalUpdatedCount, "9999");

			var transformation = GetNewTestTransformationInstance();
			transformation.Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);

			AssertNull($"ExtProperty '{OldStoredFromPKName}' should be cleared", ExtProperty.Database.Select(Db.Connection, OldStoredFromPKName));
			AssertNull($"ExtProperty '{OldStoredTotalUpdatedCount}' should be cleared", ExtProperty.Database.Select(Db.Connection, OldStoredTotalUpdatedCount));
		}

		protected override void PrepareTestData()
		{
		}

		protected override void AssertTransformationResults()
		{
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new SetUnitsPerClientUQOnOrgPartRelation();

		const string OldStoredFromPKName = "SetUnitsPerClientUQOnOrgPartRelation.FromPK";
		const string OldStoredTotalUpdatedCount = "SetUnitsPerClientUQOnOrgPartRelation.TotalUpdatedCount";
	}
}
