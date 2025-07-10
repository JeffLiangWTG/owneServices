using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Marketing;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Marketing
{
	[TestedType(typeof(SalesCommunicationManager))]
	sealed class SalesCommunicationManagerTest : RefStlScriptWithDefaultsTest
	{
		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2014, 9);

		protected override bool IsMandatoryForMilestones => false;

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 4, transactions.Count());

			var transaction1 = FindRowByRef1(transactions, "CommunicationId=CMTEST2");
			AssertEquals("[T1] TransactionDateUtc", new DateTime(2014, 9, 12, 2, 32, 0), transaction1.ServiceOccuredUTC);
			AssertEquals("[T1] UserCode", "US2", transaction1.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);
			AssertEquals("[T1] AdditionalRefs", @"{""CommunicationMethod"":""MTG"",""Purpose"":""FIN"",""RelatedActivityCount"":3,""HasPContact"":""N"",""AttndCount"":2,""HasNotes"":""Y""}",
				transaction1.AdditionalRefs);

			var transaction2 = FindRowByRef1(transactions, "CommunicationId=CMTEST3");
			AssertEquals("[T2] TransactionDateUtc", new DateTime(2014, 9, 20, 3, 43, 0), transaction2.ServiceOccuredUTC);
			AssertEquals("[T2] UserCode", "US3", transaction2.ClientStaffCode);
			AssertEquals("[T2] ItemCount", 1, transaction2.BillableCount);
			AssertEquals("[T2] AdditionalRefs", @"{""CommunicationMethod"":""EML"",""Purpose"":""SAL"",""RelatedActivityCount"":2,""HasPContact"":""Y"",""AttndCount"":1,""HasNotes"":""N""}",
				transaction2.AdditionalRefs);

			var transaction3 = FindRowByRef1(transactions, "CommunicationId=CMTEST4");
			AssertEquals("[T3] TransactionDateUtc", new DateTime(2014, 9, 30, 4, 54, 0), transaction3.ServiceOccuredUTC);
			AssertEquals("[T3] UserCode", "US4", transaction3.ClientStaffCode);
			AssertEquals("[T3] ItemCount", 1, transaction3.BillableCount);
			AssertEquals("[T3] AdditionalRefs", @"{""CommunicationMethod"":"""",""Purpose"":""FIN"",""RelatedActivityCount"":1,""HasPContact"":""N"",""AttndCount"":0,""HasNotes"":""Y""}",
				transaction3.AdditionalRefs);

			var transaction4 = FindRowByRef1(transactions, "CommunicationId=CMTEST5");
			AssertEquals("[T4] TransactionDateUtc", new DateTime(2014, 9, 30, 5, 5, 0), Convert.ToDateTime(transaction4.ServiceOccuredUTC));
			AssertEquals("[T4] UserCode", "US5", transaction4.ClientStaffCode);
			AssertEquals("[T4] ItemCount", 1, Convert.ToInt32(transaction4.BillableCount));
			AssertEquals("[T4] AdditionalRefs", @"{""CommunicationMethod"":""ONC"",""Purpose"":"""",""RelatedActivityCount"":0,""HasPContact"":""Y"",""AttndCount"":1,""HasNotes"":""Y""}",
				transaction4.AdditionalRefs);
		}

		protected override void PrepareTestData()
		{
			const string sqlText = @"
				DECLARE @OcPk UNIQUEIDENTIFIER = (SELECT TOP (1) OC_PK FROM dbo.OrgContact);
				DECLARE @CmPk1 UNIQUEIDENTIFIER = NEWID();
				DECLARE @CmPk2 UNIQUEIDENTIFIER = NEWID();
				DECLARE @CmPk3 UNIQUEIDENTIFIER = NEWID();
				DECLARE @CmPk4 UNIQUEIDENTIFIER = NEWID();
				DECLARE @CmPk5 UNIQUEIDENTIFIER = NEWID();

				INSERT INTO dbo.OrgSalesCall (OQ_PK, OQ_CommunicationID, OQ_TypeOfCall, OQ_Category, OQ_OC, OQ_SalesCallNotes, OQ_FollowupNotes, OQ_SystemCreateTimeUtc, OQ_SystemCreateUser, OQ_SystemLastEditTimeUtc, OQ_SystemLastEditUser) VALUES
				(@CmPk1, 'CMTEST1', 'PHN', 'SAL', @OcPk, convert(varbinary(max), 'Test Call Notes'), NULL, '2014-08-31 01:21:00', 'US1', GetUtcDate(), 'E'),
				(@CmPk2, 'CMTEST2', 'MTG', 'FIN', NULL, convert(varbinary(max), 'Test Call Notes'), NULL, '2014-09-12 02:32:00', 'US2', GetUtcDate(), 'E'),
				(@CmPk3, 'CMTEST3', 'EML', 'SAL', @OcPk, NULL, NULL, '2014-09-20 03:43:00', 'US3', GetUtcDate(), 'E'),
				(@CmPk4, 'CMTEST4', '', 'FIN', NULL, NULL, convert(varbinary(max), 'Test Followup Notes'), '2014-09-30 04:54:00', 'US4', GetUtcDate(), 'E'),
				(@CmPk5, 'CMTEST5', 'ONC', '', @OcPk, NULL, NULL, '2014-09-30 05:05:00', 'US5', GetUtcDate(), 'E')

				INSERT INTO dbo.OrgSalesCallAdditionalAttendee (O6_PK, O6_AttendeeName, O6_OQ, O6_SystemCreateTimeUtc, O6_SystemLastEditUser, O6_SystemLastEditTimeUtc, O6_SystemCreateUser) VALUES
				(NEWID(), 'C1A1', @CmPk1, GetUtcDate(), 'E', GetUtcDate(), 'E'),
				(NEWID(), 'C2A1', @CmPk2, GetUtcDate(), 'E', GetUtcDate(), 'E'),
				(NEWID(), 'C2A2', @CmPk2, GetUtcDate(), 'E', GetUtcDate(), 'E'),
				(NEWID(), 'C3A1', @CmPk3, GetUtcDate(), 'E', GetUtcDate(), 'E'),
				(NEWID(), 'C5A1', @CmPk5, GetUtcDate(), 'E', GetUtcDate(), 'E')

				INSERT INTO dbo.RelatedActivityPivot (RAP_PK, RAP_ParentActivityID, RAP_ParentActivityTableCode, RAP_ChildActivityID, RAP_ChildActivityTableCode, RAP_SystemCreateTimeUtc, RAP_SystemCreateUser, RAP_SystemLastEditTimeUtc, RAP_SystemLastEditUser) VALUES
				(NEWID(), @CmPk1, 'OQ', @CmPk2, 'OQ', GetUtcDate(), 'E', GetUtcDate(), 'E'),
				(NEWID(), @CmPk1, 'OQ', NEWID(), 'P8', GetUtcDate(), 'E', GetUtcDate(), 'E'),
				(NEWID(), @CmPk2, 'OQ', NEWID(), 'P8', GetUtcDate(), 'E', GetUtcDate(), 'E'),
				(NEWID(), @CmPk2, 'OQ', @CmPk3, 'OQ', GetUtcDate(), 'E', GetUtcDate(), 'E'),
				(NEWID(), NEWID(), 'O1', @CmPk3, 'OQ', GetUtcDate(), 'E', GetUtcDate(), 'E'),
				(NEWID(), @CmPk4, 'OQ', NEWID(), 'O1', GetUtcDate(), 'E', GetUtcDate(), 'E')

				INSERT INTO dbo.StmNote (ST_PK, ST_ParentID, ST_Table, ST_NoteType, ST_SystemCreateTimeUtc, ST_SystemCreateUser, ST_SystemLastEditTimeUtc, ST_SystemLastEditUser) VALUES
				(NEWID(), @CmPk3, 'OrgSalesCall', 'INT', GetUtcDate(), 'E', GetUtcDate(), 'E'),
				(NEWID(), @CmPk5, 'OrgSalesCall', 'DOC', GetUtcDate(), 'E', GetUtcDate(), 'E')";
			TestConnection.ExecuteNonQuery(sqlText);
		}
	}
}
