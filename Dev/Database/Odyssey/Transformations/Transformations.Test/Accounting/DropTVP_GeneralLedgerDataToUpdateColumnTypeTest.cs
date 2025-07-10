using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Accounting;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Accounting
{
	[TestedType(typeof(DropTVP_GeneralLedgerDataToUpdateColumnType))]
	sealed class DropTVP_GeneralLedgerDataToUpdateColumnTypeTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			Assert("Precondition: TVP_GeneralLedgerData exists.", !TestConnection.Exists(GetTVP_GeneralLedgerDataSQL));
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new DropTVP_GeneralLedgerDataToUpdateColumnType();
		}

		protected override void PrepareTestData()
		{
			if (!TestConnection.Exists(GetTVP_GeneralLedgerDataSQL))
			{
				TestConnection.ExecuteNonQuery(@"CREATE TYPE dbo.TVP_GeneralLedgerData AS TABLE (GeneralLedgerDataPK UNIQUEIDENTIFIER NOT NULL)");
			}
		}

		static string GetTVP_GeneralLedgerDataSQL => @"FROM sys.types WHERE name = 'TVP_GeneralLedgerData'";
	}
}
