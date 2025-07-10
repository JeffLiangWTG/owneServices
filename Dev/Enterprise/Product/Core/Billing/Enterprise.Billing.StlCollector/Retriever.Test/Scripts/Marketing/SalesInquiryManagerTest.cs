using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Marketing;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Marketing
{
	[TestedType(typeof(SalesInquiryManager))]
	sealed class SalesInquiryManagerTest : RefStlScriptWithDefaultsTest
	{
		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2014, 9);

		protected override bool IsMandatoryForMilestones => false;

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 6, transactions.Count());

			var transaction1 = FindRowByRef1(transactions, "I90001000");
			AssertEquals("[T1] TransactionDateUtc", new DateTime(2014, 9, 13, 1, 11, 0), transaction1.ServiceOccuredUTC);
			AssertEquals("[T1] UserCode", "ZZ", transaction1.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);
			AssertEquals("[T1] TransactionGuidReference01", firstInquiryPk, Guid.Parse(transaction1.Reference5));
			AssertEquals("[T1] TransactionReference02", "CCR", transaction1.Reference2);
			AssertEquals("[T1] TransactionReference03", "WEB", transaction1.Reference3);
			AssertEquals("[T1] TransactionReference04", "HasNotes=N|HasLeadSource=Y|HasDetails=N", transaction1.Reference4);

			var transaction2 = FindRowByRef1(transactions, "I90001003");
			AssertEquals("[T2] TransactionDateUtc", new DateTime(2014, 9, 23, 14, 24, 0), transaction2.ServiceOccuredUTC);
			AssertEquals("[T2] UserCode", "US4", transaction2.ClientStaffCode);
			AssertEquals("[T2] ItemCount", 1, transaction2.BillableCount);
			AssertEquals("[T2] TransactionReference02", "CCR", transaction2.Reference2);
			AssertEquals("[T2] TransactionReference03", "CW1", transaction2.Reference3);
			AssertEquals("[T2] TransactionReference04", "HasNotes=Y|HasLeadSource=Y|HasDetails=N", transaction2.Reference4);

			var transaction3 = FindRowByRef1(transactions, "I90001001");
			AssertEquals("[T3] TransactionDateUtc", new DateTime(2014, 9, 30, 0, 0, 0), transaction3.ServiceOccuredUTC);
			AssertEquals("[T3] UserCode", "US2", transaction3.ClientStaffCode);
			AssertEquals("[T3] ItemCount", 1, transaction3.BillableCount);
			AssertEquals("[T3] TransactionReference02", "CCR", transaction3.Reference2);
			AssertEquals("[T3] TransactionReference03", "CW1", transaction3.Reference3);
			AssertEquals("[T3] TransactionReference04", "HasNotes=N|HasLeadSource=N|HasDetails=Y", transaction3.Reference4);

			var transaction4 = FindRowByRef1(transactions, "I90001004");
			AssertEquals("[T4] TransactionDateUtc", new DateTime(2014, 9, 30, 1, 0, 0), Convert.ToDateTime(transaction4.ServiceOccuredUTC));
			AssertEquals("[T4] UserCode", "US5", transaction4.ClientStaffCode);
			AssertEquals("[T4] ItemCount", 1, transaction4.BillableCount);
			AssertEquals("[T4] TransactionReference02", "CCR", transaction4.Reference2);
			AssertEquals("[T4] TransactionReference03", "CW1", transaction4.Reference3);
			AssertEquals("[T4] TransactionReference04", "HasNotes=N|HasLeadSource=Y|HasDetails=Y", transaction4.Reference4);

			var transaction5 = FindRowByRef1(transactions, "I90001005");
			AssertEquals("[T5] TransactionDateUtc", new DateTime(2014, 9, 30, 2, 0, 0), transaction5.ServiceOccuredUTC);
			AssertEquals("[T5] UserCode", "US6", transaction5.ClientStaffCode);
			AssertEquals("[T5] ItemCount", 1, transaction5.BillableCount);
			AssertEquals("[T5] TransactionReference02", "CCR", transaction5.Reference2);
			AssertEquals("[T5] TransactionReference03", "CW1", transaction5.Reference3);
			AssertEquals("[T5] TransactionReference04", "HasNotes=N|HasLeadSource=Y|HasDetails=Y", transaction5.Reference4);

			var transaction6 = FindRowByRef1(transactions, "I90001006");
			AssertEquals("[T6] TransactionDateUtc", new DateTime(2014, 9, 30, 3, 0, 0), transaction6.ServiceOccuredUTC);
			AssertEquals("[T6] UserCode", "US7", transaction6.ClientStaffCode);
			AssertEquals("[T6] ItemCount", 1, transaction6.BillableCount);
			AssertEquals("[T6] TransactionReference02", "PSO", transaction6.Reference2);
			AssertEquals("[T6] TransactionReference03", "CW1", transaction6.Reference3);
			AssertEquals("[T6] TransactionReference04", "HasNotes=N|HasLeadSource=N|HasDetails=Y", transaction6.Reference4);
		}

		protected override void PrepareTestData()
		{
			firstInquiryPk = Guid.NewGuid();
			string sqlText = $@"
				DECLARE @Pk1 UNIQUEIDENTIFIER = '{firstInquiryPk}';
				DECLARE @Pk4 UNIQUEIDENTIFIER = newid();
				DECLARE @OhPk UNIQUEIDENTIFIER = (SELECT TOP (1) OH_PK FROM dbo.OrgHeader);

				INSERT dbo.OrgColdCallRegister (O1_PK, O1_LeadUniqueReference, O1_EnquiryType, O1_LeadSource, O1_GS_NKRepAssigned, O1_SystemCreateTimeUtc, O1_SystemCreateUser, O1_SystemLastEditTimeUtc, O1_SystemLastEditUser) VALUES
					(@Pk1,		'I90001000', 'CCR', 'OTH',	'',		'2014-09-13 01:11:00', 'ZZ',	GetUtcDate(), 'E'),
					(newid(),	'I90001001', 'CCR', '',		'AA',	'2014-09-30 00:00:00', 'US2',	GetUtcDate(), 'E'),
					(newid(),	'I90001002', 'CCR', 'OTH',	'',		'2013-09-12 03:13:00', 'US3',	GetUtcDate(), 'E'),
					(@Pk4,		'I90001003', 'CCR', 'OTH',	'',		'2014-09-23 14:24:00', 'US4',	GetUtcDate(), 'E');

				INSERT dbo.OrgColdCallRegister (O1_PK, O1_LeadUniqueReference, O1_EnquiryType,  O1_OpportunitySourceDetails, O1_LeadCalledDate, O1_SystemCreateTimeUtc, O1_SystemCreateUser, O1_SystemLastEditTimeUtc, O1_SystemLastEditUser) VALUES
					(newid(), 'I90001004', 'CCR', 'Source Details', '2014-09-30 01:00:00', '2014-09-30 01:00:00', 'US5', GetUtcDate(), 'E');

				INSERT dbo.OrgColdCallRegister (O1_PK, O1_LeadUniqueReference, O1_EnquiryType, O1_InterestLevel, O1_OH_SourceOfLead, O1_SystemCreateTimeUtc, O1_SystemCreateUser, O1_SystemLastEditTimeUtc, O1_SystemLastEditUser) VALUES
					(newid(), 'I90001005', 'CCR', 'Hot', @OhPk, '2014-09-30 02:00:00', 'US6', GetUtcDate(), 'E');

				INSERT dbo.OrgColdCallRegister (O1_PK, O1_LeadUniqueReference, O1_EnquiryType, O1_LeadStatus, O1_CloseReason, O1_SystemCreateTimeUtc, O1_SystemCreateUser, O1_SystemLastEditTimeUtc, O1_SystemLastEditUser) VALUES
					(newid(), 'I90001006', 'PSO', 'CLS', 'UDF', '2014-09-30 03:00:00', 'US7', GetUtcDate(), 'E');

				INSERT dbo.StmNote (ST_PK, ST_ParentID, ST_Table, ST_IsCustomDescription, ST_ForceRead, ST_NoteText, ST_Description) VALUES
					(newid(), @Pk4, 'OrgColdCallRegister', 0, 0, 'Test Notes', 'Blob Details');
";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		Guid firstInquiryPk;
	}
}
