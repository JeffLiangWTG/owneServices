using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.Testing
{
	[TestedType(typeof(GetSalesRelationActivityWithLastEditDataByRelatedActivityV2))]
	class GetSalesRelationActivityWithLastEditDataByRelatedActivityTest : DbCreateScriptTest
	{
		public void TestGeneralUsage()
		{
			InsertTestData();

			var activities = GetAllActivities(null, null, null);
			var activity = activities[Guid.Parse("fb1b3bd0-6bdc-48fc-9ca5-175d94e9c626")];
			AssertEquals("RelatedActivityCount", 2, activity.RelatedActivityCount);
			AssertEquals("RecentActivityDate", new DateTime(2014, 3, 26), activity.RecentActivityDate);

			activity = activities[Guid.Parse("4e33c461-ed10-465c-a064-f8e5a52054cc")];
			AssertEquals("RelatedActivityCount", 0, activity.RelatedActivityCount);
			AssertEquals("RecentActivityDate", new DateTime(2014, 2, 21), activity.RecentActivityDate);

			activity = activities[Guid.Parse("3e1fc5e3-b779-4177-b273-1d36461089e1")];
			AssertEquals("RelatedActivityCount", 2, activity.RelatedActivityCount);
			AssertEquals("RecentActivityDate", new DateTime(2014, 3, 26), activity.RecentActivityDate);

			activity = activities[Guid.Parse("eecba5d5-2390-4361-b982-cd38dad9d2bd")];
			AssertEquals("RelatedActivityCount", 1, activity.RelatedActivityCount);
			AssertEquals("RecentActivityDate", new DateTime(2014, 6, 16), activity.RecentActivityDate);

			activity = activities[Guid.Parse("eef57e46-f43e-43c4-bc3b-2cf82e6e3a0c")];
			AssertEquals("RelatedActivityCount", 1, activity.RelatedActivityCount);
			AssertEquals("RecentActivityDate", new DateTime(2014, 4, 21), activity.RecentActivityDate);

			activity = activities[Guid.Parse("bd41879b-62ea-406f-8549-9c48c21da95b")];
			AssertEquals("RelatedActivityCount", 1, activity.RelatedActivityCount);
			AssertEquals("RecentActivityDate", new DateTime(2014, 1, 21), activity.RecentActivityDate);

			activity = activities[Guid.Parse("ce6e6b1e-81b2-4750-9d0a-a97e556bb2a8")];
			AssertEquals("RelatedActivityCount", 6, activity.RelatedActivityCount);
			AssertEquals("RecentActivityDate", new DateTime(2014, 6, 16), activity.RecentActivityDate);

			activity = activities[Guid.Parse("602b734d-478e-4b0a-b7c3-a93bb1c0ee64")];
			AssertEquals("RelatedActivityCount", 1, activity.RelatedActivityCount);
			AssertEquals("RecentActivityDate", new DateTime(2014, 3, 26), activity.RecentActivityDate);

			AssertEquals("activities.Count", 8, activities.Count);
		}

		public void TestGeneralUsage_WithSpecificRelatedActivityType()
		{
			InsertTestData();

			var activities = GetAllActivities(null, "INQ", null);
			var activity = activities[Guid.Parse("fb1b3bd0-6bdc-48fc-9ca5-175d94e9c626")];
			AssertEquals("RelatedActivityCount", 1, activity.RelatedActivityCount);
			AssertEquals("RecentActivityDate", new DateTime(2014, 3, 26), activity.RecentActivityDate);

			activity = activities[Guid.Parse("4e33c461-ed10-465c-a064-f8e5a52054cc")];
			AssertEquals("RelatedActivityCount", 0, activity.RelatedActivityCount);
			AssertEquals("RecentActivityDate", null, activity.RecentActivityDate);

			activity = activities[Guid.Parse("3e1fc5e3-b779-4177-b273-1d36461089e1")];
			AssertEquals("RelatedActivityCount", 0, activity.RelatedActivityCount);
			AssertEquals("RecentActivityDate", new DateTime(2014, 3, 26), activity.RecentActivityDate);

			activity = activities[Guid.Parse("eecba5d5-2390-4361-b982-cd38dad9d2bd")];
			AssertEquals("RelatedActivityCount", 0, activity.RelatedActivityCount);
			AssertEquals("RecentActivityDate", null, activity.RecentActivityDate);

			activity = activities[Guid.Parse("eef57e46-f43e-43c4-bc3b-2cf82e6e3a0c")];
			AssertEquals("RelatedActivityCount", 0, activity.RelatedActivityCount);
			AssertEquals("RecentActivityDate", null, activity.RecentActivityDate);

			activity = activities[Guid.Parse("bd41879b-62ea-406f-8549-9c48c21da95b")];
			AssertEquals("RelatedActivityCount", 0, activity.RelatedActivityCount);
			AssertEquals("RecentActivityDate", null, activity.RecentActivityDate);

			activity = activities[Guid.Parse("ce6e6b1e-81b2-4750-9d0a-a97e556bb2a8")];
			AssertEquals("RelatedActivityCount", 2, activity.RelatedActivityCount);
			AssertEquals("RecentActivityDate", new DateTime(2014, 3, 26), activity.RecentActivityDate);

			activity = activities[Guid.Parse("602b734d-478e-4b0a-b7c3-a93bb1c0ee64")];
			AssertEquals("RelatedActivityCount", 0, activity.RelatedActivityCount);
			AssertEquals("RecentActivityDate", new DateTime(2014, 3, 26), activity.RecentActivityDate);

			AssertEquals("activities.Count", 8, activities.Count);
		}

		public void TestGeneralUsage_WithFilterByRelatedActivityId()
		{
			InsertTestData();

			var activities = GetAllActivities(null, "INQ", Guid.Parse("3e1fc5e3-b779-4177-b273-1d36461089e1"));
			var activity = activities[Guid.Parse("fb1b3bd0-6bdc-48fc-9ca5-175d94e9c626")];
			AssertEquals("RelatedActivityCount", 1, activity.RelatedActivityCount);
			AssertEquals("RecentActivityDate", new DateTime(2014, 3, 26), activity.RecentActivityDate);

			activity = activities[Guid.Parse("4e33c461-ed10-465c-a064-f8e5a52054cc")];
			AssertEquals("RelatedActivityCount", 0, activity.RelatedActivityCount);
			AssertEquals("RecentActivityDate", null, activity.RecentActivityDate);

			activity = activities[Guid.Parse("3e1fc5e3-b779-4177-b273-1d36461089e1")];
			AssertEquals("RelatedActivityCount", 0, activity.RelatedActivityCount);
			AssertEquals("RecentActivityDate", new DateTime(2014, 3, 26), activity.RecentActivityDate);

			activity = activities[Guid.Parse("eecba5d5-2390-4361-b982-cd38dad9d2bd")];
			AssertEquals("RelatedActivityCount", 0, activity.RelatedActivityCount);
			AssertEquals("RecentActivityDate", null, activity.RecentActivityDate);

			activity = activities[Guid.Parse("eef57e46-f43e-43c4-bc3b-2cf82e6e3a0c")];
			AssertEquals("RelatedActivityCount", 0, activity.RelatedActivityCount);
			AssertEquals("RecentActivityDate", null, activity.RecentActivityDate);

			activity = activities[Guid.Parse("bd41879b-62ea-406f-8549-9c48c21da95b")];
			AssertEquals("RelatedActivityCount", 0, activity.RelatedActivityCount);
			AssertEquals("RecentActivityDate", null, activity.RecentActivityDate);

			activity = activities[Guid.Parse("ce6e6b1e-81b2-4750-9d0a-a97e556bb2a8")];
			AssertEquals("RelatedActivityCount", 1, activity.RelatedActivityCount);
			AssertEquals("RecentActivityDate", new DateTime(2014, 3, 26), activity.RecentActivityDate);

			activity = activities[Guid.Parse("602b734d-478e-4b0a-b7c3-a93bb1c0ee64")];
			AssertEquals("RelatedActivityCount", 0, activity.RelatedActivityCount);
			AssertEquals("RecentActivityDate", new DateTime(2014, 3, 26), activity.RecentActivityDate);

			AssertEquals("activities.Count", 8, activities.Count);
		}

		public void TestGeneralUsage_WithSelectedActivityType()
		{
			InsertTestData();

			var activities = GetAllActivities("COM", "INQ", Guid.Parse("3e1fc5e3-b779-4177-b273-1d36461089e1"));
			AssertEquals("activities.Count", 1, activities.Count);

			var activity = activities[Guid.Parse("ce6e6b1e-81b2-4750-9d0a-a97e556bb2a8")];
			AssertEquals("RelatedActivityCount", 1, activity.RelatedActivityCount);
			AssertEquals("RecentActivityDate", new DateTime(2014, 3, 26), activity.RecentActivityDate);
		}

		public void TestCompanyCampaignContactSalesRelations()
		{
			InsertTestDataWithContactSalesRelation();

			var activities = GetAllActivities("OPP", "CAM", Guid.Parse("fb1b3bd0-6bdc-48fc-9ca5-175d94e9c626"), 1);

			AssertEquals("Sales Relation Count", 3, activities.Keys.Count);

			var activity = activities[Guid.Parse("eef57e46-f43e-43c4-bc3b-2cf82e6e3a0c")];
			AssertEquals("RelatedActivityCount", 1, activity.RelatedActivityCount);
			AssertEquals("RecentActivityDate", new DateTime(2014, 2, 21), activity.RecentActivityDate);

			activity = activities[Guid.Parse("00534ce1-918a-4623-a70f-9bed87273542")];
			AssertEquals("RelatedActivityCount", 1, activity.RelatedActivityCount);
			AssertEquals("RecentActivityDate", new DateTime(2014, 2, 21), activity.RecentActivityDate);

			activity = activities[Guid.Parse("f90c4845-cada-4433-9be5-a897bff11794")];
			AssertEquals("RelatedActivityCount", 1, activity.RelatedActivityCount);
			AssertEquals("RecentActivityDate", new DateTime(2014, 2, 21), activity.RecentActivityDate);
		}

		#region Implementation

		void InsertTestData()
		{
			var insertPivotsSql = @"
DECLARE @CompanyPk UNIQUEIDENTIFIER = '525e06b4-aa27-4dc4-88cd-eacc79c25219';
DECLARE @OrgPk UNIQUEIDENTIFIER = '200568cf-ff34-40e8-8dfa-790ad5de0e52';
INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES (@CompanyPk, 'DAN', 'AU company','AU', 'AUD')
INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES (@OrgPk, 'TESTADL')

INSERT INTO dbo.OrgSalesCall (OQ_PK, OQ_CommunicationID, OQ_SystemCreateTimeUtc, OQ_SystemCreateUser, OQ_SystemLastEditTimeUtc, OQ_SystemLastEditUser)
VALUES ('ce6e6b1e-81b2-4750-9d0a-a97e556bb2a8', 'CM00000001', '2014-1-21', 'E', '2014-1-21', 'E')

INSERT INTO dbo.GlbCompanyCampaign (G0_PK, G0_GC, G0_CampaignName, G0_CampaignID, G0_SystemCreateTimeUtc, G0_SystemCreateUser, G0_SystemLastEditTimeUtc, G0_SystemLastEditUser)
VALUES ('fb1b3bd0-6bdc-48fc-9ca5-175d94e9c626', @CompanyPk, '~boobies~', '~boobies~id~', '2014-2-21', 'E', '2014-2-21', 'E')

INSERT INTO dbo.GlbCompanyCampaignItem (G8_PK, G8_G0, G8_RecipientTableCode, G8_RecipientID, G8_SystemCreateTimeUtc, G8_SystemCreateUser, G8_SystemLastEditTimeUtc, G8_SystemLastEditUser)
VALUES ('4e33c461-ed10-465c-a064-f8e5a52054cc', 'fb1b3bd0-6bdc-48fc-9ca5-175d94e9c626', 'OC', newid(), GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.OrgColdCallRegister (O1_PK, O1_LeadUniqueReference, O1_SystemCreateTimeUtc, O1_SystemCreateUser, O1_SystemLastEditTimeUtc, O1_SystemLastEditUser)
VALUES	('3e1fc5e3-b779-4177-b273-1d36461089e1', 'O100001', '2014-3-26', 'E', '2014-3-26', 'E'),
		('602b734d-478e-4b0a-b7c3-a93bb1c0ee64', 'O100002', '2014-3-26', 'E', '2014-3-26', 'E')

INSERT INTO dbo.OrgOpportunity (P8_PK, P8_OH, P8_OpportunityID, P8_GC, P8_SystemCreateTimeUtc, P8_SystemCreateUser, P8_SystemLastEditTimeUtc, P8_SystemLastEditUser)
VALUES ('eef57e46-f43e-43c4-bc3b-2cf82e6e3a0c', @OrgPk, 'OppId', @CompanyPk, '2014-4-21', 'E', '2014-4-21', 'E')

INSERT INTO dbo.RatingHeader (TH_PK, TH_SystemLastEditTimeUtc, TH_RateType, TH_QuoteDate, TH_QuoteNumber, TH_SystemLastEditUser, TH_SystemCreateTimeUtc, TH_SystemCreateUser) VALUES ('bd41879b-62ea-406f-8549-9c48c21da95b', '2014-1-21', 'QTE', '2014-1-11', 'QTE00001', '~BP', '2014-1-21', '~BP')
INSERT INTO dbo.JobShipment (JS_PK, JS_SystemLastEditTimeUtc) VALUES ('eecba5d5-2390-4361-b982-cd38dad9d2bd', '2014-6-16')

INSERT INTO dbo.RelatedActivityPivot
	(RAP_PK, RAP_ParentActivityTableCode, RAP_ParentActivityID, RAP_ChildActivityTableCode, RAP_ChildActivityID, RAP_SalesRelationTreeID, RAP_SystemCreateTimeUtc, RAP_SystemCreateUser, RAP_SystemLastEditTimeUtc, RAP_SystemLastEditUser)
VALUES
	(newid(), 'G0', 'fb1b3bd0-6bdc-48fc-9ca5-175d94e9c626', 'G8', '4e33c461-ed10-465c-a064-f8e5a52054cc', '4e33c461-ed10-465c-a064-f8e5a52054cc', GetUtcDate(), 'E', GetUtcDate(), 'E'),
	(newid(), 'G0', 'fb1b3bd0-6bdc-48fc-9ca5-175d94e9c626', 'O1', '3e1fc5e3-b779-4177-b273-1d36461089e1', '3e1fc5e3-b779-4177-b273-1d36461089e1', GetUtcDate(), 'E', GetUtcDate(), 'E'),
	(newid(), 'O1', '3e1fc5e3-b779-4177-b273-1d36461089e1', 'OQ', 'ce6e6b1e-81b2-4750-9d0a-a97e556bb2a8', '3e1fc5e3-b779-4177-b273-1d36461089e1', GetUtcDate(), 'E', GetUtcDate(), 'E'),
	(newid(), 'O1', '602b734d-478e-4b0a-b7c3-a93bb1c0ee64', 'OQ', 'ce6e6b1e-81b2-4750-9d0a-a97e556bb2a8', '602b734d-478e-4b0a-b7c3-a93bb1c0ee64', GetUtcDate(), 'E', GetUtcDate(), 'E'),

	(newid(), 'OQ', 'ce6e6b1e-81b2-4750-9d0a-a97e556bb2a8', 'P8', 'eef57e46-f43e-43c4-bc3b-2cf82e6e3a0c', 'eef57e46-f43e-43c4-bc3b-2cf82e6e3a0c', GetUtcDate(), 'E', GetUtcDate(), 'E'),
	(newid(), 'OQ', 'ce6e6b1e-81b2-4750-9d0a-a97e556bb2a8', 'TH', 'bd41879b-62ea-406f-8549-9c48c21da95b', 'bd41879b-62ea-406f-8549-9c48c21da95b', GetUtcDate(), 'E', GetUtcDate(), 'E'),

	(newid(), 'VB', 'eecba5d5-2390-4361-b982-cd38dad9d2bd', 'OQ', 'ce6e6b1e-81b2-4750-9d0a-a97e556bb2a8', 'eecba5d5-2390-4361-b982-cd38dad9d2bd', GetUtcDate(), 'E', GetUtcDate(), 'E'),

	(newid(), 'AH', '7dc9fe58-15bb-4d5c-88f9-2fd5ab3f24ca', 'VB', 'eecba5d5-2390-4361-b982-cd38dad9d2bd', NULL, GetUtcDate(), 'E', GetUtcDate(), 'E') -- non sales-relation pivot
";
			/*
			 *   G0
			 *   /| \
			 *  / |  \
			 * G8 O1  O1  VB
			 *        | /
			 *        |/
			 *       OQ
			 *        |\
			 *        | \
			 *       P8 TH
			 * 
			 * */

			using (var command = TestConnection.Command(insertPivotsSql))
			{
				command.ExecuteNonQuery();
			}
		}

		void InsertTestDataWithContactSalesRelation()
		{
			var insertPivotsSql = @"
DECLARE @CompanyPk UNIQUEIDENTIFIER = '525e06b4-aa27-4dc4-88cd-eacc79c25219';
DECLARE @OrgPk UNIQUEIDENTIFIER = '200568cf-ff34-40e8-8dfa-790ad5de0e52';
INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES (@CompanyPk, 'DAN', 'AU company', 'AU', 'AUD')
INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES (@OrgPk, 'TESTADL')

INSERT INTO dbo.GlbCompanyCampaign (G0_PK, G0_GC, G0_CampaignName, G0_CampaignID, G0_SystemCreateTimeUtc, G0_SystemCreateUser, G0_SystemLastEditTimeUtc, G0_SystemLastEditUser)
VALUES ('fb1b3bd0-6bdc-48fc-9ca5-175d94e9c626', @CompanyPk, '~boobies~', '~boobies~id~', '2014-2-21', 'E', '2014-2-21', 'E')

INSERT INTO dbo.GlbCompanyCampaignItem (G8_PK, G8_G0, G8_RecipientTableCode, G8_RecipientID, G8_SystemCreateTimeUtc, G8_SystemCreateUser, G8_SystemLastEditTimeUtc, G8_SystemLastEditUser)
VALUES	('4e33c461-ed10-465c-a064-f8e5a52054cc', 'fb1b3bd0-6bdc-48fc-9ca5-175d94e9c626', 'OC', '1aa29e16-3613-42a8-add5-e0c804f7811c', GetUtcDate(), 'E', GetUtcDate(), 'E'),
		('2679d4e8-fac9-4f3c-93ed-a6dfe0ad8d9f', 'fb1b3bd0-6bdc-48fc-9ca5-175d94e9c626', 'OC', '300a996b-0f6b-471a-8ee1-190f1b8a4bde', GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.OrgOpportunity (P8_PK, P8_OH, P8_OpportunityID, P8_GC, P8_SystemCreateTimeUtc, P8_SystemCreateUser, P8_SystemLastEditTimeUtc, P8_SystemLastEditUser)
VALUES	('eef57e46-f43e-43c4-bc3b-2cf82e6e3a0c', @OrgPk, 'OppId1', @CompanyPk, '2014-4-21', 'E', '2014-4-21', 'E'),
		('00534ce1-918a-4623-a70f-9bed87273542', @OrgPk, 'OppId2', @CompanyPk, '2014-5-21', 'E', '2014-5-21', 'E'),
		('f90c4845-cada-4433-9be5-a897bff11794', @OrgPk, 'OppId3', @CompanyPk, '2014-6-21', 'E', '2014-6-21', 'E')


INSERT INTO dbo.RelatedActivityPivot
	(RAP_PK, RAP_ParentActivityTableCode, RAP_ParentActivityID, RAP_ChildActivityTableCode, RAP_ChildActivityID, RAP_SalesRelationTreeID, RAP_SystemCreateTimeUtc, RAP_SystemCreateUser, RAP_SystemLastEditTimeUtc, RAP_SystemLastEditUser)
VALUES
	(newid(), 'G0', 'fb1b3bd0-6bdc-48fc-9ca5-175d94e9c626', 'P8', 'eef57e46-f43e-43c4-bc3b-2cf82e6e3a0c', 'eef57e46-f43e-43c4-bc3b-2cf82e6e3a0c', GetUtcDate(), 'E', GetUtcDate(), 'E'),
	(newid(), 'G8', '4e33c461-ed10-465c-a064-f8e5a52054cc', 'P8', '00534ce1-918a-4623-a70f-9bed87273542', '4e33c461-ed10-465c-a064-f8e5a52054cc', GetUtcDate(), 'E', GetUtcDate(), 'E'),
	(newid(), 'G8', '2679d4e8-fac9-4f3c-93ed-a6dfe0ad8d9f', 'P8', 'f90c4845-cada-4433-9be5-a897bff11794', '2679d4e8-fac9-4f3c-93ed-a6dfe0ad8d9f', GetUtcDate(), 'E', GetUtcDate(), 'E')
";
			/*
			 * G0  G8  G8
			 * |    |   |
			 * |    |   |
			 * O1  O2   O3
			 * 
			 */

			using (var command = TestConnection.Command(insertPivotsSql))
			{
				command.ExecuteNonQuery();
			}
		}

		Dictionary<Guid, Activity> GetAllActivities(string selectedActivityType, string includeRelatedActivityType, Guid? relatedActivityId, int minimumActivityCount = 0)
		{
			var selectSql = @"SELECT * FROM GetSalesRelationActivityWithLastEditDataByRelatedActivityV2(@SelectedActivityType, @IncludeRelatedActivityType, @IncludeRelatedActivityType, @RelatedActivityID)";
			using (var command = TestConnection.Command(selectSql))
			{
				command.AddParameter("@SelectedActivityType", SqlDbType.VarChar, string.IsNullOrEmpty(selectedActivityType) ? DBNull.Value : selectedActivityType);
				command.AddParameter("@IncludeRelatedActivityType", SqlDbType.VarChar, string.IsNullOrEmpty(includeRelatedActivityType) ? DBNull.Value : includeRelatedActivityType);
				command.AddParameter("@RelatedActivityID", SqlDbType.UniqueIdentifier, (object)relatedActivityId ?? DBNull.Value);

				using (var reader = command.ExecuteReader())
				{
					var result = new Dictionary<Guid, Activity>();

					while (reader.Read())
					{
						var activityId = (Guid)reader["ActivityID"];
						var activityRelatedActivityCount = reader["RelatedActivityCount"] == DBNull.Value ? null : (int?)reader["RelatedActivityCount"];
						var activityRecentActivityDate = reader["RecentActivityDate"] == DBNull.Value ? null : (DateTime?)reader["RecentActivityDate"];

						if (activityRelatedActivityCount >= minimumActivityCount)
						{
							result.Add(activityId, new Activity(activityId, activityRelatedActivityCount, activityRecentActivityDate));
						}
					}

					return result;
				}
			}
		}

		class Activity
		{
			public Activity(Guid id, int? relatedActivityCount, DateTime? recentActivityDate)
			{
				ID = id;
				RelatedActivityCount = relatedActivityCount;
				RecentActivityDate = recentActivityDate;
			}

			public readonly Guid ID;
			public readonly int? RelatedActivityCount;
			public readonly DateTime? RecentActivityDate;
		}
		#endregion
	}
}

