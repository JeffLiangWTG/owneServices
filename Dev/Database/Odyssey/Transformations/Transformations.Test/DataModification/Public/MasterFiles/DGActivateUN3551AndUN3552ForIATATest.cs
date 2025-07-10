using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.MasterFiles;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformation.DataModification.MasterFiles.Testing
{
	[TestedType(typeof(DGActivateUN3551AndUN3552ForIATA))]
	public class DGActivateUN3551AndUN3552ForIATATest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new DGActivateUN3551AndUN3552ForIATA();

		protected override void PrepareTestData()
		{
			var sql = new SqlQueryBuilder();
			new ZZUNDGSubstance("3551", "a", "IAT") { DG_UniqueRecordId = "3551aIAT", DG_IsActive = false }.InsertAndReturnObject(TestConnection);
			new ZZUNDGSubstance("3552", "a", "IAT") { DG_UniqueRecordId = "3552aIAT", DG_IsActive = false }.AppendInsertAndReturnObject(sql);
			new ZZUNDGSubstance("3551", "b", "IAT") { DG_UniqueRecordId = "3551bIAT", DG_IsActive = false }.AppendInsertAndReturnObject(sql);
			new ZZUNDGSubstance("3552", "b", "IAT") { DG_UniqueRecordId = "3552bIAT", DG_IsActive = false }.AppendInsertAndReturnObject(sql);
			new ZZUNDGSubstance("3551", "a", "IMO") { DG_UniqueRecordId = "3551aIMO", DG_IsActive = false }.AppendInsertAndReturnObject(sql);
			new ZZUNDGSubstance("3552", "a", "IMO") { DG_UniqueRecordId = "3552aIMO", DG_IsActive = false }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
		}

		protected override void AssertPreConditions()
		{
			AssertEquals(false, Db.Connection.ExecuteScalar<bool>("SELECT DG_IsActive FROM [dbo].[ZZUNDGSubstance] WHERE [DG_UNNO] = '3551' and DG_Variant = 'a' and DG_Standard = 'IAT'"));
			AssertEquals(false, Db.Connection.ExecuteScalar<bool>("SELECT DG_IsActive FROM [dbo].[ZZUNDGSubstance] WHERE [DG_UNNO] = '3552' and DG_Variant = 'a' and DG_Standard = 'IAT'"));
			AssertEquals(false, Db.Connection.ExecuteScalar<bool>("SELECT DG_IsActive FROM [dbo].[ZZUNDGSubstance] WHERE [DG_UNNO] = '3551' and DG_Variant = 'b' and DG_Standard = 'IAT'"));
			AssertEquals(false, Db.Connection.ExecuteScalar<bool>("SELECT DG_IsActive FROM [dbo].[ZZUNDGSubstance] WHERE [DG_UNNO] = '3552' and DG_Variant = 'b' and DG_Standard = 'IAT'"));
			AssertEquals(false, Db.Connection.ExecuteScalar<bool>("SELECT DG_IsActive FROM [dbo].[ZZUNDGSubstance] WHERE [DG_UNNO] = '3551' and DG_Variant = 'a' and DG_Standard = 'IMO'"));
			AssertEquals(false, Db.Connection.ExecuteScalar<bool>("SELECT DG_IsActive FROM [dbo].[ZZUNDGSubstance] WHERE [DG_UNNO] = '3552' and DG_Variant = 'a' and DG_Standard = 'IMO'"));
		}

		protected override void AssertTransformationResults()
		{
			AssertEquals(true, Db.Connection.ExecuteScalar<bool>("SELECT DG_IsActive FROM [dbo].[ZZUNDGSubstance] WHERE [DG_UNNO] = '3551' and DG_Variant = 'a' and DG_Standard = 'IAT'"));
			AssertEquals(true, Db.Connection.ExecuteScalar<bool>("SELECT DG_IsActive FROM [dbo].[ZZUNDGSubstance] WHERE [DG_UNNO] = '3552' and DG_Variant = 'a' and DG_Standard = 'IAT'"));
			AssertEquals(true, Db.Connection.ExecuteScalar<bool>("SELECT DG_IsActive FROM [dbo].[ZZUNDGSubstance] WHERE [DG_UNNO] = '3551' and DG_Variant = 'b' and DG_Standard = 'IAT'"));
			AssertEquals(true, Db.Connection.ExecuteScalar<bool>("SELECT DG_IsActive FROM [dbo].[ZZUNDGSubstance] WHERE [DG_UNNO] = '3552' and DG_Variant = 'b' and DG_Standard = 'IAT'"));
			AssertEquals(false, Db.Connection.ExecuteScalar<bool>("SELECT DG_IsActive FROM [dbo].[ZZUNDGSubstance] WHERE [DG_UNNO] = '3551' and DG_Variant = 'a' and DG_Standard = 'IMO'"));
			AssertEquals(false, Db.Connection.ExecuteScalar<bool>("SELECT DG_IsActive FROM [dbo].[ZZUNDGSubstance] WHERE [DG_UNNO] = '3552' and DG_Variant = 'a' and DG_Standard = 'IMO'"));
		}
	}
}
