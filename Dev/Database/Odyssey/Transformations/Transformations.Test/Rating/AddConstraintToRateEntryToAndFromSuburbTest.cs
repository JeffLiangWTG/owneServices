using System;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Rating;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Rating
{
	[TestedType(typeof(AddConstraintToRateEntryToAndFromSuburb))]
	public class AddConstraintToRateEntryToAndFromSuburbTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new AddConstraintToRateEntryToAndFromSuburb();

		protected override void PrepareTestData()
		{
			RatingTransformationHelper.DisableTrigger(RateEntrySchema.Constants.TableName, "TG_CheckNoRateEntryOverlaps");
			DBTransformationTestHelper.DropConstraintIfExists(RateEntrySchema.Constants.TableName, "RateEntry_TI_R9_FromSuburb_FK2_RefCityTown_RRR_120N");
			DBTransformationTestHelper.DropConstraintIfExists(RateEntrySchema.Constants.TableName, "RateEntry_TI_R9_ToSuburb_FK2_RefCityTown_RRR_120N");

			var sql = $@"
				DECLARE @GC_PK UNIQUEIDENTIFIER = NEWID();
				DECLARE @TH_PK UNIQUEIDENTIFIER = NEWID();
				DECLARE @R9_PK UNIQUEIDENTIFIER = NEWID();

				INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_SystemCreateTimeUtc, GC_SystemCreateUser, GC_SystemLastEditTimeUtc, GC_SystemLastEditUser)
				VALUES (@GC_PK, 'CMP', 'AU company', 'AU', 'AUD', GetUtcDate(), '~BP', GetUtcDate(), '~BP');

				INSERT INTO dbo.RatingHeader (TH_PK, TH_RateType, TH_SystemCreateTimeUtc, TH_SystemCreateUser, TH_SystemLastEditTimeUtc, TH_SystemLastEditUser)
				VALUES (@TH_PK, 'GLB', GetUtcDate(), '~BP', GetUtcDate(), '~BP');

				INSERT INTO dbo.RefCityTown (R9_PK, R9_InternationalName, R9_RW_NKState, R9_RN_NKCountry, R9_SystemCreateTimeUtc, R9_SystemCreateUser, R9_SystemLastEditTimeUtc, R9_SystemLastEditUser)
				VALUES (@R9_PK, 'SomeTown', 'NSW', 'AU', GetUtcDate(), '~BP', GetUtcDate(), '~BP');

				INSERT INTO dbo.RateEntry (TI_PK, TI_TH, TI_GC_Publisher, TI_RateCategory, TI_Mode, TI_RateStartDate, TI_RateEndDate, TI_R9_ToSuburb, TI_R9_FromSuburb, TI_SystemCreateTimeUtc, TI_SystemCreateUser, TI_SystemLastEditTimeUtc, TI_SystemLastEditUser)
				VALUES ('{rateEntryValid}', @TH_PK, @GC_PK, 'ORG', 'SEA', '2024-08-01', '2024-09-01', @R9_PK, @R9_PK, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					   ('{rateEntryInvalidSuburbTo}', @TH_PK, @GC_PK, 'ORG', 'SEA', '2024-08-01', '2024-09-01', NEWID(), @R9_PK, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					   ('{rateEntryInvalidSuburbFrom}', @TH_PK, @GC_PK, 'ORG', 'SEA', '2024-08-01', '2024-09-01', @R9_PK, NEWID(), GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					   ('{rateEntryInvalidSuburbToAndFrom}', @TH_PK, @GC_PK, 'ORG', 'SEA', '2024-08-01', '2024-09-01', NEWID(), NEWID(), GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					   (NEWID(), @TH_PK, @GC_PK, 'ORG', 'SEA', '2024-08-01', '2024-09-01', NEWID(), NEWID(), GetUtcDate(), '~BP', GetUtcDate(), '~BP');
			";

			TestConnection.ExecuteNonQuery(sql);
		}

		protected override void AssertTransformationResults()
		{
			AsserEntryHasSuburbs(rateEntryValid, hasToSuburb: true, hasFromSuburb: true);
			AsserEntryHasSuburbs(rateEntryInvalidSuburbTo, hasToSuburb: false, hasFromSuburb: true);
			AsserEntryHasSuburbs(rateEntryInvalidSuburbFrom, hasToSuburb: true, hasFromSuburb: false);
			AsserEntryHasSuburbs(rateEntryInvalidSuburbToAndFrom, hasToSuburb: false, hasFromSuburb: false);
		}

		public void TestIsRequiredIsFalseWhenMissingColumns()
		{
			var transform = new AddConstraintToRateEntryToAndFromSuburb();

			Assert("Transform should run when columns exist", transform.IsRequired);

			DBTransformationTestHelper.DropConstraintIfExists(RateEntrySchema.Constants.TableName, "RateEntry_TI_R9_FromSuburb_FK2_RefCityTown_RRR_120N");
			DBTransformationTestHelper.DropConstraintIfExists(RateEntrySchema.Constants.TableName, "RateEntry_TI_R9_ToSuburb_FK2_RefCityTown_RRR_120N");
			new DbColumnDependencyRemover(RateEntrySchema.Constants.TableName, RateEntrySchema.Constants.TI_R9_FromSuburb).DropRelateObjects(TestConnection);
			DBTransformationTestHelper.DropColumnIfExists(RateEntrySchema.Constants.TableName, RateEntrySchema.Constants.TI_R9_FromSuburb);

			Assert("Transform should not run when a column is missing", !transform.IsRequired);
		}

		void AsserEntryHasSuburbs(Guid pk, bool hasToSuburb, bool hasFromSuburb)
		{
			TestConnection.ExecuteReader($"SELECT TOP 1 TI_R9_ToSuburb, TI_R9_FromSuburb FROM dbo.RateEntry where TI_PK = '{pk}'",
				record =>
				{
					AssertEquals(hasToSuburb, record["TI_R9_ToSuburb"] is Guid);
					AssertEquals(hasFromSuburb, record["TI_R9_FromSuburb"] is Guid);
				});
		}

		Guid rateEntryValid = Guid.NewGuid();
		Guid rateEntryInvalidSuburbTo = Guid.NewGuid();
		Guid rateEntryInvalidSuburbFrom = Guid.NewGuid();
		Guid rateEntryInvalidSuburbToAndFrom = Guid.NewGuid();
	}
}
