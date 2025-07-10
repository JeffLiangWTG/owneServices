using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.BusinessIntelligence;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.BusinessIntelligence
{
	[TestedType(typeof(DropAllowAlterCDCMetaObjectsTrigger))]
	class DropAllowAlterCDCMetaObjectsTriggerTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new DropAllowAlterCDCMetaObjectsTrigger();

		protected override void PrepareTestData()
		{
			TestConnection.ExecuteNonQuery($"DISABLE TRIGGER [{triggerName}] ON DATABASE");
			Assert($"[{triggerName}] should exist", TestConnection.Exists($"FROM sys.triggers WHERE name = '{triggerName}' and is_disabled = 1"));
		}

		protected override void AssertTransformationResults()
		{
			Assert($"[{triggerName}] should not exist", !TestConnection.Exists($"FROM sys.triggers WHERE name = '{triggerName}'"));
		}

		const string triggerName = "TG_AllowAlterCDCMetaObjects";
	}
}
