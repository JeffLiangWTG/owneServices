using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Architecture.DataProtection;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Architecture
{
	[TestedType(typeof(ProtectedDataStatusSeedForDefaults))]
	sealed class ProtectedDataStatusSeedForDefaultsTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			Assert("Reader should be registered", Db.Connection.Exists($"FROM dbo.StmProtectedDataState WHERE  PDS_Type = 'Reader' and PDS_Active = 1"));
			Assert("Writer should be registered", Db.Connection.Exists($"FROM dbo.StmProtectedDataState WHERE  PDS_Type = 'Writer' and PDS_Active = 1"));
			Assert("RestrictedReader should be registered", Db.Connection.Exists($"FROM dbo.StmProtectedDataState WHERE  PDS_Type = 'RestrictedReader' and PDS_Active = 1"));
			Assert("RestrictedWriter should be registered", Db.Connection.Exists($"FROM dbo.StmProtectedDataState WHERE  PDS_Type = 'RestrictedWriter' and PDS_Active = 1"));
			Assert("UnrestrictedWriter should be registered", Db.Connection.Exists($"FROM dbo.StmProtectedDataState WHERE  PDS_Type = 'UnrestrictedWriter' and PDS_Active = 1"));
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new ProtectedDataStatusSeedForDefaults();
		}

		protected override void PrepareTestData()
		{
		}
	}
}
