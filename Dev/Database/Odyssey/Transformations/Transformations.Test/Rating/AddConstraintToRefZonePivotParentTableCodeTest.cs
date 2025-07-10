using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.Shared;
using Enterprise.DbUpgrader.Transformations.Transforms.Rating;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Rating
{
	[TestedType(typeof(AddConstraintToRefZonePivotParentTableCode))]
	sealed class AddConstraintToRefZonePivotParentTableCodeTest : ConstraintBase_ParentTableCodeTest<AddConstraintToRefZonePivotParentTableCode>
	{
		protected override string TableName => RefZonePivotSchema.Constants.TableName;

		protected override string TablePrefix => RefZonePivotSchema.Constants.Prefix;

		protected override string[] SupportedParentPrefixes => new[] { "RL", "RN" };

		protected override bool UseNoCheck => true;

		protected override bool AllowEmptyParentTableCode => false;

		protected override string[] ExpectedIndexIncludeColumns => [];

		protected override (string column, string tablePrefix)[] ForeignKeyColumns => new[]
		{
			("F2_FZ", "FZ")
		};

		protected override void AssertPreConditions()
		{
			CombineAssertions("PRE-REQs", () =>
			{
				var sqlText = "SELECT count(distinct F2_ParentTableCode) FROM dbo.RefZonePivot WHERE F2_ParentTableCode NOT IN ('RL', 'RN')";
				AssertEquals("Invalid records should exist which will later be deleted", 3, (int)Db.Connection.ExecuteScalar(sqlText));

				sqlText = "SELECT count(distinct F2_ParentTableCode) FROM dbo.RefZonePivot WHERE F2_ParentTableCode IN ('RL', 'RN')";
				AssertEquals("Valid data should exist for each prefix", 2, (int)Db.Connection.ExecuteScalar(sqlText));
			});
		}

		protected override void AssertTransformationResults()
		{
			base.AssertTransformationResults();

			var sqlText = $"SELECT F2_ParentTableCode FROM dbo.RefZonePivot WHERE F2_PK = '{guidList[0]}';";
			AssertEquals("Valid data should be fixed to RL", "RL", (string)Db.Connection.ExecuteScalar(sqlText));
			sqlText = $"SELECT F2_ParentTableCode FROM dbo.RefZonePivot WHERE F2_PK = '{guidList[1]}';";
			AssertEquals("Valid data should be fixed to RN", "RN", (string)Db.Connection.ExecuteScalar(sqlText));
		}

		protected override void AddChildTables(StringBuilder sqlText, List<(string prefix, string pkVar)> combos, Dictionary<string, string> foreignKeys)
		{
			base.AddChildTables(sqlText, combos, foreignKeys);
			sqlText.AppendLine("DECLARE @RefUNLOCOPK2 UNIQUEIDENTIFIER = NEWID();");
			sqlText.AppendLine("INSERT INTO dbo.RefUNLOCO (RL_PK, RL_Code, RL_GeoLocation, RL_SystemCreateTimeUtc, RL_SystemCreateUser, RL_SystemLastEditTimeUtc, RL_SystemLastEditUser) VALUES (@RefUNLOCOPK2, 'XYZ', convert(geography, 'POINT EMPTY'), GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');");
			sqlText.AppendLine("DECLARE @RefCountryPK2 UNIQUEIDENTIFIER = NEWID();");
			sqlText.AppendLine("INSERT INTO dbo.RefCountry (RN_PK, RN_Code, RN_Desc, RN_SystemCreateTimeUtc, RN_SystemCreateUser, RN_SystemLastEditTimeUtc, RN_SystemLastEditUser) VALUES (@RefCountryPK2, 'ZG', 'Not Exist Local', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');");
			sqlText.AppendLine($"INSERT INTO dbo.RefZonePivot(F2_FZ, F2_PK, F2_ParentTableCode, F2_ParentID, F2_SystemCreateTimeUtc, F2_SystemCreateUser, F2_SystemLastEditTimeUtc, F2_SystemLastEditUser) VALUES (@RefZoneHeaderPK, '{guidList[0]}', 'NE', @RefUNLOCOPK2, GetUtcDate(), '~BP', GetUtcDate(), '~BP');");
			sqlText.AppendLine($"INSERT INTO dbo.RefZonePivot(F2_FZ, F2_PK, F2_ParentTableCode, F2_ParentID, F2_SystemCreateTimeUtc, F2_SystemCreateUser, F2_SystemLastEditTimeUtc, F2_SystemLastEditUser) VALUES (@RefZoneHeaderPK, '{guidList[1]}', 'NT', @RefCountryPK2, GetUtcDate(), '~BP', GetUtcDate(), '~BP');");
		}

		readonly Guid[] guidList = [Guid.NewGuid(), Guid.NewGuid()];
	}
}
