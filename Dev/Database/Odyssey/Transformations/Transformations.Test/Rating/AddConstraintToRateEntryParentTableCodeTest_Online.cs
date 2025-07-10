using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.Shared;
using Enterprise.DbUpgrader.Transformations.Transforms.Rating;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Rating
{
	[TestedType(typeof(AddConstraintToRateEntryParentTableCode_Online))]
	sealed class AddConstraintToRateEntryParentTableCodeTest_Online : ConstraintBase_ParentTableCodeTest<AddConstraintToRateEntryParentTableCode_Online>
	{
		protected override string TableName => RateEntrySchema.Constants.TableName;

		protected override string TablePrefix => RateEntrySchema.Constants.Prefix;

		protected override string[] SupportedParentPrefixes => new[] { "WW" };

		protected override bool UseNoCheck => true;

		protected override bool AllowEmptyParentTableCode => true;

		protected override string[] ExpectedIndexIncludeColumns => [];

		protected override (string column, string tablePrefix)[] ForeignKeyColumns => new[]
		{
			("TI_TH", "TH"),
			("TI_GC_Publisher", "GC"),
		};

		protected override DataTransformation GetNewTestTransformationInstance() => new AddConstraintToRateEntryParentTableCode_Online();

		protected override void AssertPreConditions()
		{
			CombineAssertions("PRE-REQs", () =>
			{
				var sqlText = "SELECT count(distinct TI_ParentTableCode) FROM dbo.RateEntry WHERE TI_ParentTableCode NOT IN ('', 'WW')";
				AssertEquals("Invalid records should exist which will later be deleted", 2, (int)Db.Connection.ExecuteScalar(sqlText));

				sqlText = "SELECT count(distinct TI_ParentTableCode) FROM dbo.RateEntry WHERE TI_ParentTableCode IN ('', 'WW')";
				AssertEquals("Valid data should exist for each prefix", 2, (int)Db.Connection.ExecuteScalar(sqlText));
			});
		}

		protected override void AssertTransformationResults()
		{
			base.AssertTransformationResults();

			var sqlText = $"SELECT TI_ParentTableCode FROM dbo.RateEntry WHERE TI_PK = '{guid}'";
			AssertEquals("Invalid data should be updated", "", (string)Db.Connection.ExecuteScalar(sqlText));
		}

		protected override void AddChildTables(StringBuilder sqlText, List<(string prefix, string pkVar)> combos, Dictionary<string, string> foreignKeys)
		{
			base.AddChildTables(sqlText, combos, foreignKeys);
			sqlText.AppendLine(@$"
				INSERT INTO dbo.RateEntry(TI_TH, TI_GC_Publisher, TI_PK, TI_ParentTableCode, TI_ParentID, TI_SystemCreateTimeUtc, TI_SystemCreateUser, TI_SystemLastEditTimeUtc, TI_SystemLastEditUser, TI_RateCategory, TI_Mode, TI_RateStartDate)
				VALUES (@RatingHeaderPK, @GlbCompanyPK, '{guid}', 'LOG', NEWID(), GetUtcDate(), '~BP', GetUtcDate(), '~BP', 'FCL', 'SEA', GetUtcDate())");
		}

		int _appendCounter;
		protected override void AppendInsertScript(StringBuilder sqlText, Dictionary<string, string> columnValues)
		{
			columnValues.Add("TI_RateCategory", "'AIR'");
			columnValues.Add("TI_Mode", "'LSE'");

			if (columnValues["TI_ParentTableCode"] == "''")
			{
				columnValues.Remove("TI_ParentID");
			}

			var startOffset = _appendCounter * 2;
			var endOffset = startOffset + 1;
			columnValues.Add(
				"TI_RateStartDate",
				$"DATEADD(day, {startOffset}, GETUTCDATE())"
			);
			columnValues.Add(
				"TI_RateEndDate",
				$"DATEADD(day, {endOffset}, GETUTCDATE())"
			);

			_appendCounter++;

			base.AppendInsertScript(sqlText, columnValues);
		}

		readonly Guid guid = Guid.NewGuid();

		public void TestBatchingWorks_OnlinePostUpgrade()
		{
			const string checkpointName = "AddConstraintToRateEntryParentTableCode_Online.LastProcessedTI_SystemCreateTimeUtc";
			ExtProperty.Table.Delete(Db.Connection, "dbo", RateEntrySchema.Constants.TableName, checkpointName);

			int recordCount = 1500;
			PrepareLargeBatchInvalidRateEntries(recordCount);

			int preCount = Convert.ToInt32(Db.Connection.ExecuteScalar(
				"SELECT COUNT(*) FROM dbo.RateEntry WHERE TI_ParentTableCode NOT IN ('', 'WW')"));

			Assert($"Expected at least {recordCount} invalid records before transformation, but found {preCount}.", preCount >= recordCount);

			var transformation = (AddConstraintToRateEntryParentTableCode_Online)GetNewTestTransformationInstance();
			transformation.Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);

			int postCount = Convert.ToInt32(Db.Connection.ExecuteScalar(
				"SELECT COUNT(*) FROM dbo.RateEntry WHERE TI_ParentTableCode NOT IN ('', 'WW')"));
			AssertEquals("All invalid records should be processed.", 0, postCount);

			string checkpoint = ExtProperty.Table.Select(Db.Connection, "dbo", RateEntrySchema.Constants.TableName, checkpointName);
			Assert("Checkpoint should be cleared after successful processing.", string.IsNullOrEmpty(checkpoint));
		}

		void PrepareLargeBatchInvalidRateEntries(int count)
		{
			Db.Connection.ExecuteNonQuery(
				"ALTER TABLE dbo.RateEntry NOCHECK CONSTRAINT Constraint_TI_ParentTableCode_NoCheck;");

			try
			{
				var testDataCreator = new TransformationTestDataCreator();
				var orgPK = testDataCreator.CreateOrg("ORGDAU", "Aus organisation");
				var companyPK = testDataCreator.CreateGlbCompany("GLB", "AU");
				var newRatingHeaderPK = RatingTransformationHelper.CreateRateHeader("QTE", companyPK, orgPK, "sda", DateTime.Today);
				var orgHeaderPk = RatingTransformationHelper.CreateRateHeader("GLB", DateTime.Today);
				string[] invalidCodes = { "LOG", "RCV", "XYZ" };

				const string sql = @"
		INSERT INTO dbo.RateEntry (
			TI_TH,
			TI_GC_Publisher,
			TI_PK,
			TI_RateStartDate,
			TI_RateEndDate,
			TI_ParentTableCode,
			TI_ParentID,
			TI_SystemCreateTimeUtc,
			TI_SystemCreateUser,
			TI_SystemLastEditTimeUtc,
			TI_SystemLastEditUser,
			TI_RateCategory,
			TI_Mode
		)
		VALUES (
			@tiTh,
			@tiGc,
			@tiPk,
			@startDate,
			@endDate,
			@parentTableCode,
			@parentId,
			GETUTCDATE(), '~BP',
			GETUTCDATE(), '~BP',
			'FCL', 'SEA'
		);";

				for (int i = 0; i < count; i++)
				{
					var start = DateTime.Today.AddDays(i);
					var end = start;

					using (var command = Db.Connection.Command(sql))
					{
						command.AddParameter("@tiTh", SqlDbType.UniqueIdentifier, newRatingHeaderPK);
						command.AddParameter("@tiGc", SqlDbType.UniqueIdentifier, companyPK);
						command.AddParameter("@tiPk", SqlDbType.UniqueIdentifier, Guid.NewGuid());
						command.AddParameter("@startDate", SqlDbType.Date, start);
						command.AddParameter("@endDate", SqlDbType.Date, end);
						command.AddParameter("@parentTableCode", SqlDbType.NVarChar, invalidCodes[i % invalidCodes.Length]);
						command.AddParameter("@parentId", SqlDbType.UniqueIdentifier, Guid.NewGuid());

						try
						{
							command.ExecuteNonQuery();
						}
						catch (SqlException ex)
						{
							Console.Error.WriteLine($"SQL error occurred: {ex.Message}");
							throw;
						}
					}
				}
			}
			finally
			{
				Db.Connection.ExecuteNonQuery(
					"ALTER TABLE dbo.RateEntry WITH NOCHECK CHECK CONSTRAINT Constraint_TI_ParentTableCode_NoCheck;");
			}
		}
	}
	}
