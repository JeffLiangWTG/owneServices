using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MarketingManager.Test
{
	class GetNextCampaignItemScheduleBatchIntegrationTest : TransactionedTestCase
	{
		public void TestGeneralUsage()
		{
			var insertTestDataSql = @"
DECLARE @Company UNIQUEIDENTIFIER = (SELECT TOP 1 GC_PK FROM dbo.GlbCompany);

INSERT INTO dbo.OrgHeader
	(OH_PK, OH_Code, OH_SystemCreateTimeUtc, OH_SystemCreateUser, OH_SystemLastEditTimeUtc, OH_SystemLastEditUser)
VALUES
	(@OrgA, 'AAA', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	(@OrgB, 'BBB', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

INSERT INTO dbo.OrgContact
	(OC_PK, OC_OH, OC_ContactName, OC_SystemCreateTimeUtc, OC_SystemCreateUser, OC_SystemLastEditTimeUtc, OC_SystemLastEditUser)
VALUES
	(@ContactA1, @OrgA, 'ContactA1', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	(@ContactA2, @OrgA, 'ContactA2', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	(@ContactA3, @OrgA, 'ContactA3', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	(@ContactA4, @OrgA, 'ContactA4', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	(@ContactA5, @OrgA, 'ContactA5', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	(@ContactA6, @OrgA, 'ContactA6', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	(@ContactA7, @OrgA, 'ContactA7', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	
	(@ContactB1, @OrgB, 'ContactB1', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	(@ContactB2, @OrgB, 'ContactB2', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	(@ContactB3, @OrgB, 'ContactB3', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	(@ContactB4, @OrgB, 'ContactB4', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

INSERT INTO dbo.GlbCompanyCampaign
	(G0_PK, G0_GC, G0_G0_Master, G0_HorizontalId, G0_CampaignName, G0_CampaignID, G0_SystemCreateTimeUtc, G0_SystemCreateUser, G0_SystemLastEditTimeUtc, G0_SystemLastEditUser)
VALUES
	(@CampaignMaster, @Company, NULL, 0, 'Master Campaign', 'TST00001000', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	(@CampaignX, @Company, @CampaignMaster, 1, 'Campaign Touch 1X', 'TST00001001', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	(@CampaignY, @Company, @CampaignMaster, 1, 'Campaign Touch 1Y', 'TST00001002', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

INSERT INTO dbo.GlbCompanyCampaignItem
	(G8_PK, G8_G0, G8_RecipientTableCode, G8_RecipientID, G8_TrackingStatus, G8_ScheduleTimeUtc, G8_LastSentTimeUtc, G8_SystemLastEditTimeUtc, G8_SystemCreateTimeUtc, G8_SystemCreateUser, G8_SystemLastEditUser)
VALUES
	-- Campaign X
	(@CampaignItemXA1, @CampaignX, 'OC', @ContactA1, 'UNV',       NULL, '2001-1-1', '2002-1-10', GetUtcDate(), '~BP', '~BP'),  -- previously sent
	(@CampaignItemXA2, @CampaignX, 'OC', @ContactA2, 'VER',       NULL, '2001-2-1', '2002-1-10', GetUtcDate(), '~BP', '~BP'),  -- previously sent
	(@CampaignItemXA3, @CampaignX, 'OC', @ContactA3, 'QUE', '2006-1-1', '2001-3-1', '2002-1-10', GetUtcDate(), '~BP', '~BP'),  -- previously sent, but re-queued for send
	(@CampaignItemXA4, @CampaignX, 'OC', @ContactA4, 'QUE', '2006-1-1',       NULL, '2002-1-10', GetUtcDate(), '~BP', '~BP'),  -- queued for send
	(@CampaignItemXA5, @CampaignX, 'OC', @ContactA5, 'QUE',       NULL,       NULL, '2002-2-10', GetUtcDate(), '~BP', '~BP'),  -- queued for batch scheduling
	(@CampaignItemXA6, @CampaignX, 'OC', @ContactA6, 'QUE',       NULL,       NULL, '2002-3-10', GetUtcDate(), '~BP', '~BP'),  -- queued for batch scheduling
	(@CampaignItemXA7, @CampaignX, 'OC', @ContactA7, 'QUE',       NULL, '2001-4-1', '2002-4-10', GetUtcDate(), '~BP', '~BP'),  -- previously sent, but re-queued for batch scheduling

	(@CampaignItemXB1, @CampaignX, 'OC', @ContactB1, 'VER',       NULL, '2010-2-1', '2002-1-15', GetUtcDate(), '~BP', '~BP'),  -- previously sent
	(@CampaignItemXB2, @CampaignX, 'OC', @ContactB2, 'QUE', '2010-1-1',       NULL, '2002-1-15', GetUtcDate(), '~BP', '~BP'),  -- queued for send
	(@CampaignItemXB3, @CampaignX, 'OC', @ContactB3, 'QUE',       NULL,       NULL, '2002-2-15', GetUtcDate(), '~BP', '~BP'),  -- queued for batch scheduling
	(@CampaignItemXB4, @CampaignX, 'OC', @ContactB4, 'QUE',       NULL,       NULL, '2002-3-15', GetUtcDate(), '~BP', '~BP'),  -- queued for batch scheduling


	-- Campaign Y
	(@CampaignItemYA1, @CampaignY, 'OC', @ContactA1, 'VER',       NULL, '2010-2-1', '2002-1-20', GetUtcDate(), '~BP', '~BP'),  -- previously sent
	(@CampaignItemYA2, @CampaignY, 'OC', @ContactA2, 'QUE', '2010-1-1',       NULL, '2002-1-20', GetUtcDate(), '~BP', '~BP'),  -- queued for send
	(@CampaignItemYA3, @CampaignY, 'OC', @ContactA3, 'QUE',       NULL,       NULL, '2002-2-20', GetUtcDate(), '~BP', '~BP')   -- queued for batch scheduling
";

			using (var command = TestConnection.Command(insertTestDataSql))
			{
				command.AddParameter("@OrgA", SqlDbType.UniqueIdentifier, OrgA);
				command.AddParameter("@OrgB", SqlDbType.UniqueIdentifier, OrgB);

				command.AddParameter("@ContactA1", SqlDbType.UniqueIdentifier, ContactA1);
				command.AddParameter("@ContactA2", SqlDbType.UniqueIdentifier, ContactA2);
				command.AddParameter("@ContactA3", SqlDbType.UniqueIdentifier, ContactA3);
				command.AddParameter("@ContactA4", SqlDbType.UniqueIdentifier, ContactA4);
				command.AddParameter("@ContactA5", SqlDbType.UniqueIdentifier, ContactA5);
				command.AddParameter("@ContactA6", SqlDbType.UniqueIdentifier, ContactA6);
				command.AddParameter("@ContactA7", SqlDbType.UniqueIdentifier, ContactA7);

				command.AddParameter("@ContactB1", SqlDbType.UniqueIdentifier, ContactB1);
				command.AddParameter("@ContactB2", SqlDbType.UniqueIdentifier, ContactB2);
				command.AddParameter("@ContactB3", SqlDbType.UniqueIdentifier, ContactB3);
				command.AddParameter("@ContactB4", SqlDbType.UniqueIdentifier, ContactB4);

				command.AddParameter("@CampaignMaster", SqlDbType.UniqueIdentifier, CampaignMaster);
				command.AddParameter("@CampaignX", SqlDbType.UniqueIdentifier, CampaignX);
				command.AddParameter("@CampaignY", SqlDbType.UniqueIdentifier, CampaignY);

				command.AddParameter("@CampaignItemXA1", SqlDbType.UniqueIdentifier, CampaignItemXA1);
				command.AddParameter("@CampaignItemXA2", SqlDbType.UniqueIdentifier, CampaignItemXA2);
				command.AddParameter("@CampaignItemXA3", SqlDbType.UniqueIdentifier, CampaignItemXA3);
				command.AddParameter("@CampaignItemXA4", SqlDbType.UniqueIdentifier, CampaignItemXA4);
				command.AddParameter("@CampaignItemXA5", SqlDbType.UniqueIdentifier, CampaignItemXA5);
				command.AddParameter("@CampaignItemXA6", SqlDbType.UniqueIdentifier, CampaignItemXA6);
				command.AddParameter("@CampaignItemXA7", SqlDbType.UniqueIdentifier, CampaignItemXA7);

				command.AddParameter("@CampaignItemXB1", SqlDbType.UniqueIdentifier, CampaignItemXB1);
				command.AddParameter("@CampaignItemXB2", SqlDbType.UniqueIdentifier, CampaignItemXB2);
				command.AddParameter("@CampaignItemXB3", SqlDbType.UniqueIdentifier, CampaignItemXB3);
				command.AddParameter("@CampaignItemXB4", SqlDbType.UniqueIdentifier, CampaignItemXB4);

				command.AddParameter("@CampaignItemYA1", SqlDbType.UniqueIdentifier, CampaignItemYA1);
				command.AddParameter("@CampaignItemYA2", SqlDbType.UniqueIdentifier, CampaignItemYA2);
				command.AddParameter("@CampaignItemYA3", SqlDbType.UniqueIdentifier, CampaignItemYA3);

				command.ExecuteNonQuery();
			}

			AssertCampaignItemPksEquals("No limit.",
				new[]
				{
					CampaignItemXA5,
					CampaignItemXB3,
					CampaignItemXA6,
					CampaignItemXB4,
					CampaignItemXA7,
				},
				GetCampaignItemScheduleBatch(CampaignX,
						limitThisBatch: null,
						limitPerOrganizationThisBatch: null,
						limitPerOrganizationInHorizontal: null,
						limitPerOrganizationInHorizontalPeriodStartDateUtc: null,
						limitPerOrganizationInTouch: null,
						limitPerOrganizationInTouchPeriodStartDateUtc: null));

			AssertCampaignItemPksEquals("Limit of 2 contact in this batch.",
				new[]
				{
					CampaignItemXA5,
					CampaignItemXB3,
				},
				GetCampaignItemScheduleBatch(CampaignX,
						limitThisBatch: 2,
						limitPerOrganizationThisBatch: null,
						limitPerOrganizationInHorizontal: null,
						limitPerOrganizationInHorizontalPeriodStartDateUtc: null,
						limitPerOrganizationInTouch: null,
						limitPerOrganizationInTouchPeriodStartDateUtc: null));

			AssertCampaignItemPksEquals("Limit of 2 contacts per Org in this batch. Should prioritise contacts with earlier last edit times.",
				new[]
				{
					CampaignItemXA5,
					CampaignItemXB3,
					CampaignItemXA6,
					CampaignItemXB4,
				},
				GetCampaignItemScheduleBatch(CampaignX,
						limitThisBatch: null,
						limitPerOrganizationThisBatch: 2,
						limitPerOrganizationInHorizontal: null,
						limitPerOrganizationInHorizontalPeriodStartDateUtc: null,
						limitPerOrganizationInTouch: null,
						limitPerOrganizationInTouchPeriodStartDateUtc: null));

			AssertCampaignItemPksEquals("Limit of 1 contact per Org in this batch. Should prioritise contacts with earlier last edit times.",
				new[]
				{
					CampaignItemXA5,
					CampaignItemXB3,
				},
				GetCampaignItemScheduleBatch(CampaignX,
						limitThisBatch: null,
						limitPerOrganizationThisBatch: 1,
						limitPerOrganizationInHorizontal: null,
						limitPerOrganizationInHorizontalPeriodStartDateUtc: null,
						limitPerOrganizationInTouch: null,
						limitPerOrganizationInTouchPeriodStartDateUtc: null));

			AssertCampaignItemPksEquals("Limit of 6 contacts per Org in all batches of this touch.",
				new[]
				{
					CampaignItemXA5,
					CampaignItemXB3,
					CampaignItemXB4
				},
				GetCampaignItemScheduleBatch(CampaignX,
						limitThisBatch: null,
						limitPerOrganizationThisBatch: null,
						limitPerOrganizationInHorizontal: null,
						limitPerOrganizationInHorizontalPeriodStartDateUtc: null,
						limitPerOrganizationInTouch: 6,
						limitPerOrganizationInTouchPeriodStartDateUtc: null));

			AssertCampaignItemPksEquals("Limit of 6 contacts per Org since '2001-2-1'.",
				new[]
				{
					CampaignItemXA5,
					CampaignItemXB3,
					CampaignItemXA6,
					CampaignItemXB4
				},
				GetCampaignItemScheduleBatch(CampaignX,
						limitThisBatch: null,
						limitPerOrganizationThisBatch: null,
						limitPerOrganizationInHorizontal: null,
						limitPerOrganizationInHorizontalPeriodStartDateUtc: null,
						limitPerOrganizationInTouch: 6,
						limitPerOrganizationInTouchPeriodStartDateUtc: new DateTime(2001, 2, 1)));

			AssertCampaignItemPksEquals("Limit of 8 contacts per Org in all batches over all verticals.",
				new[]
				{
					CampaignItemXA5,
					CampaignItemXB3,
					CampaignItemXB4,
				},
				GetCampaignItemScheduleBatch(CampaignX,
						limitThisBatch: null,
						limitPerOrganizationThisBatch: null,
						limitPerOrganizationInHorizontal: 8,
						limitPerOrganizationInHorizontalPeriodStartDateUtc: null,
						limitPerOrganizationInTouch: null,
						limitPerOrganizationInTouchPeriodStartDateUtc: null));
		}

		public void TestGeneralUsageInsideSales()
		{
			var insertTestDataSql = @"
DECLARE @Company UNIQUEIDENTIFIER = (SELECT TOP 1 GC_PK FROM dbo.GlbCompany);

INSERT INTO dbo.OrgHeader
	(OH_PK, OH_Code, OH_SystemCreateTimeUtc, OH_SystemCreateUser, OH_SystemLastEditTimeUtc, OH_SystemLastEditUser)
VALUES
	(@OrgC, 'CCC', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	(@OrgD, 'DDD', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

INSERT INTO dbo.OrgContact
	(OC_PK, OC_OH, OC_ContactName, OC_SystemCreateTimeUtc, OC_SystemCreateUser, OC_SystemLastEditTimeUtc, OC_SystemLastEditUser)
VALUES
	(@ContactC, @OrgC, 'ContactC', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	(@ContactD, @OrgD, 'ContactD', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

INSERT INTO dbo.GlbCompanyCampaign
	(G0_PK, G0_GC, G0_G0_Master, G0_HorizontalId, G0_CampaignName, G0_CampaignID, G0_BroadcastVoteSurveyExam, G0_SystemCreateTimeUtc, G0_SystemCreateUser, G0_SystemLastEditTimeUtc, G0_SystemLastEditUser)
VALUES
	(@CampaignMasterIns, @Company, NULL, 0, 'Master Campaign Ins', 'TST00001000', @CampaignTypeIns, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	(@CampaignOpp, @Company, @CampaignMasterIns, 1, 'Campaign Touch Opp', 'TST00001001', @CampaignTypeOpp, GetUtcDate(), '~BP', GetUtcDate(), '~BP')

INSERT INTO dbo.GlbCompanyCampaignItem
	(G8_PK, G8_G0, G8_RecipientTableCode, G8_RecipientID, G8_TrackingStatus, G8_ScheduleTimeUtc, G8_LastSentTimeUtc, G8_SystemLastEditTimeUtc, G8_SystemCreateTimeUtc, G8_SystemCreateUser, G8_SystemLastEditUser)
VALUES
	(@CampaignItemOpp1, @CampaignOpp, 'OC', @ContactC, 'OPQ', NULL, '2020-6-1', '2020-6-2', GetUtcDate(), '~BP', '~BP'),
	(@CampaignItemOpp2, @CampaignOpp, 'OC', @ContactD, 'OPQ', NULL, '2020-6-1', '2020-6-2', GetUtcDate(), '~BP', '~BP')
";

			using (var command = TestConnection.Command(insertTestDataSql))
			{
				command.AddParameter("@OrgC", SqlDbType.UniqueIdentifier, OrgC);
				command.AddParameter("@OrgD", SqlDbType.UniqueIdentifier, OrgD);

				command.AddParameter("@ContactC", SqlDbType.UniqueIdentifier, ContactC);
				command.AddParameter("@ContactD", SqlDbType.UniqueIdentifier, ContactD);

				command.AddParameter("@CampaignMasterIns", SqlDbType.UniqueIdentifier, CampaignMasterIns);
				command.AddParameter("@CampaignOpp", SqlDbType.UniqueIdentifier, CampaignOpp);

				command.AddParameter("@CampaignTypeIns", SqlDbType.VarChar, CampaignTypeList.Codes.InsideSales);
				command.AddParameter("@CampaignTypeOpp", SqlDbType.VarChar, InsideSalesTouchTypeList.Codes.OpportunityCreation);

				command.AddParameter("@CampaignItemOpp1", SqlDbType.UniqueIdentifier, CampaignItemOpp1);
				command.AddParameter("@CampaignItemOpp2", SqlDbType.UniqueIdentifier, CampaignItemOpp2);
				command.ExecuteNonQuery();
			}

			AssertCampaignItemPksEquals("No limit in this batch.",
				new[]
				{
					CampaignItemOpp1,
					CampaignItemOpp2
				},
				GetCampaignItemScheduleBatch(CampaignOpp,
						limitThisBatch: null,
						limitPerOrganizationThisBatch: null,
						limitPerOrganizationInHorizontal: null,
						limitPerOrganizationInHorizontalPeriodStartDateUtc: null,
						limitPerOrganizationInTouch: null,
						limitPerOrganizationInTouchPeriodStartDateUtc: null));

			AssertCampaignItemPksEquals("Limit of 1 contact in this batch.",
				new[]
				{
					CampaignItemOpp1
				},
				GetCampaignItemScheduleBatch(CampaignOpp,
						limitThisBatch: 1,
						limitPerOrganizationThisBatch: null,
						limitPerOrganizationInHorizontal: null,
						limitPerOrganizationInHorizontalPeriodStartDateUtc: null,
						limitPerOrganizationInTouch: null,
						limitPerOrganizationInTouchPeriodStartDateUtc: null));
		}

		Guid[] GetCampaignItemScheduleBatch(
			Guid g0_PK,
			int? limitThisBatch,
			int? limitPerOrganizationThisBatch,
			int? limitPerOrganizationInHorizontal,
			DateTime? limitPerOrganizationInHorizontalPeriodStartDateUtc,
			int? limitPerOrganizationInTouch,
			DateTime? limitPerOrganizationInTouchPeriodStartDateUtc)
		{
			var result = new List<Guid>();
			var selectSql = @"
SELECT *
FROM dbo.GetNextCampaignItemScheduleBatch(
	@G0_PK,
	@LimitThisBatch,
	@LimitPerOrganizationThisBatch,
	@LimitPerOrganizationInHorizontal,
	@LimitPerOrganizationInHorizontalPeriodStartDateUtc,
	@LimitPerOrganizationInTouch,
	@LimitPerOrganizationInTouchPeriodStartDateUtc)";

			using (var command = TestConnection.Command(selectSql))
			{
				command.AddParameter("@G0_PK", SqlDbType.UniqueIdentifier, g0_PK);
				command.AddParameter("@LimitThisBatch", SqlDbType.Int, limitThisBatch ?? 0);
				command.AddParameter("@LimitPerOrganizationThisBatch", SqlDbType.Int, limitPerOrganizationThisBatch ?? 0);
				command.AddParameter("@LimitPerOrganizationInHorizontal", SqlDbType.Int, limitPerOrganizationInHorizontal ?? 0);
				command.AddParameter("@LimitPerOrganizationInHorizontalPeriodStartDateUtc", SqlDbType.SmallDateTime, limitPerOrganizationInHorizontalPeriodStartDateUtc.HasValue ? limitPerOrganizationInHorizontalPeriodStartDateUtc.Value : DBNull.Value);
				command.AddParameter("@LimitPerOrganizationInTouch", SqlDbType.Int, limitPerOrganizationInTouch ?? 0);
				command.AddParameter("@LimitPerOrganizationInTouchPeriodStartDateUtc", SqlDbType.SmallDateTime, limitPerOrganizationInTouchPeriodStartDateUtc.HasValue ? limitPerOrganizationInTouchPeriodStartDateUtc.Value : DBNull.Value);

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var pk = (Guid)reader["G8_PK"];
						result.Add(pk);
					}
				}
			}

			return result.ToArray();
		}

		void AssertCampaignItemPksEquals(string message, Guid[] expectedCampaignItemPks, Guid[] actualCampaignItemPks)
		{
			AssertContainsExactElementsInAnyOrder(message,
				expectedCampaignItemPks.Select(x => x + " " + GetContactNameOfCampaignItem(x)),
				actualCampaignItemPks.Select(x => x + " " + GetContactNameOfCampaignItem(x)));
		}

		string GetContactNameOfCampaignItem(Guid g8_PK)
		{
			var selectSql = @"
SELECT OC_ContactName
FROM dbo.GlbCompanyCampaignItem
	JOIN dbo.OrgContact ON G8_RecipientID = OC_PK
WHERE G8_PK = @G8_PK";

			using (var command = TestConnection.Command(selectSql))
			{
				command.AddParameter("@G8_PK", SqlDbType.UniqueIdentifier, g8_PK);
				return (string)command.ExecuteScalar();
			}
		}

		readonly Guid OrgA = Guid.Parse("690e5cc5-8786-4dfd-b4f1-f975b1eaf8ea");
		readonly Guid OrgB = Guid.Parse("690e5cc5-8786-4dfd-b4f1-f975b1eaf8eb");

		readonly Guid OrgC = Guid.Parse("B7CCFBC7-B951-4935-B9B3-323143D78391");
		readonly Guid OrgD = Guid.Parse("F68DE27D-397A-4D63-ADB6-7183083AD395");

		readonly Guid ContactA1 = Guid.Parse("4fa37e6a-e840-4484-8755-1df59ea389a1");
		readonly Guid ContactA2 = Guid.Parse("4fa37e6a-e840-4484-8755-1df59ea389a2");
		readonly Guid ContactA3 = Guid.Parse("4fa37e6a-e840-4484-8755-1df59ea389a3");
		readonly Guid ContactA4 = Guid.Parse("4fa37e6a-e840-4484-8755-1df59ea389a4");
		readonly Guid ContactA5 = Guid.Parse("4fa37e6a-e840-4484-8755-1df59ea389a5");
		readonly Guid ContactA6 = Guid.Parse("4fa37e6a-e840-4484-8755-1df59ea389a6");
		readonly Guid ContactA7 = Guid.Parse("4fa37e6a-e840-4484-8755-1df59ea389a7");

		readonly Guid ContactB1 = Guid.Parse("4fa37e6a-e840-4484-8755-1df59ea389b1");
		readonly Guid ContactB2 = Guid.Parse("4fa37e6a-e840-4484-8755-1df59ea389b2");
		readonly Guid ContactB3 = Guid.Parse("4fa37e6a-e840-4484-8755-1df59ea389b3");
		readonly Guid ContactB4 = Guid.Parse("4fa37e6a-e840-4484-8755-1df59ea389b4");

		readonly Guid ContactC = Guid.Parse("A5B87B6C-82AF-4C24-8CAE-3445E88C1C26");
		readonly Guid ContactD = Guid.Parse("C7C26946-68FA-4DEF-BA40-9E9A985C3E68");

		readonly Guid CampaignMaster = Guid.Parse("e37c8cfa-5be9-4ded-9563-e378f6b2a100");
		readonly Guid CampaignX = Guid.Parse("e37c8cfa-5be9-4ded-9563-e378f6b2a101");
		readonly Guid CampaignY = Guid.Parse("e37c8cfa-5be9-4ded-9563-e378f6b2a102");

		readonly Guid CampaignMasterIns = Guid.Parse("A3724F21-78E2-4FED-9356-4A96A84A6015");
		readonly Guid CampaignOpp = Guid.Parse("A7FC4B3C-C1F9-43C6-A46D-B2074D1E8F34");

		readonly Guid CampaignItemXA1 = Guid.Parse("43bf6f62-3f8c-4897-aeb7-0d31f5e3aaa1");
		readonly Guid CampaignItemXA2 = Guid.Parse("43bf6f62-3f8c-4897-aeb7-0d31f5e3aaa2");
		readonly Guid CampaignItemXA3 = Guid.Parse("43bf6f62-3f8c-4897-aeb7-0d31f5e3aaa3");
		readonly Guid CampaignItemXA4 = Guid.Parse("43bf6f62-3f8c-4897-aeb7-0d31f5e3aaa4");
		readonly Guid CampaignItemXA5 = Guid.Parse("43bf6f62-3f8c-4897-aeb7-0d31f5e3aaa5");
		readonly Guid CampaignItemXA6 = Guid.Parse("43bf6f62-3f8c-4897-aeb7-0d31f5e3aaa6");
		readonly Guid CampaignItemXA7 = Guid.Parse("43bf6f62-3f8c-4897-aeb7-0d31f5e3aaa7");

		readonly Guid CampaignItemXB1 = Guid.Parse("43bf6f62-3f8c-4897-aeb7-0d31f5e3aab1");
		readonly Guid CampaignItemXB2 = Guid.Parse("43bf6f62-3f8c-4897-aeb7-0d31f5e3aab2");
		readonly Guid CampaignItemXB3 = Guid.Parse("43bf6f62-3f8c-4897-aeb7-0d31f5e3aab3");
		readonly Guid CampaignItemXB4 = Guid.Parse("43bf6f62-3f8c-4897-aeb7-0d31f5e3aab4");

		readonly Guid CampaignItemYA1 = Guid.Parse("43bf6f62-3f8c-4897-aeb7-0d31f5e3aba1");
		readonly Guid CampaignItemYA2 = Guid.Parse("43bf6f62-3f8c-4897-aeb7-0d31f5e3aba2");
		readonly Guid CampaignItemYA3 = Guid.Parse("43bf6f62-3f8c-4897-aeb7-0d31f5e3aba3");

		readonly Guid CampaignItemOpp1 = Guid.Parse("E5F1FDEF-78EE-4706-A6A2-13EAA07332E4");
		readonly Guid CampaignItemOpp2 = Guid.Parse("22FB2FF0-FE53-4662-8737-849259182FA2");
	}
}

