using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(ForwarderVGMEmailDelivery))]
	sealed class ForwarderVGMEmailDeliveryTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;

		protected override void PrepareTestData()
		{
			string sqlText = @"
				DECLARE @GlbCompanyPk UNIQUEIDENTIFIER = NEWID();
				INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_IsActive) VALUES
					(@GlbCompanyPk, 'VML', 'AU company', 1);

				DECLARE @GlbBranchPk UNIQUEIDENTIFIER = NEWID();
				INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_IsActive, GB_GC) VALUES
					(@GlbBranchPk, 'VML', 1, @GlbCompanyPk);

				DECLARE @OhPk1 UNIQUEIDENTIFIER = newid();
				DECLARE @OhPk2 UNIQUEIDENTIFIER = newid();
				DECLARE @OhPk3 UNIQUEIDENTIFIER = newid();
				INSERT dbo.OrgHeader (OH_PK, OH_IsActive, OH_Code, OH_FullName) VALUES
					(@OhPk1, 1, 'VMLSYD', 'VmlSyd'),
					(@OhPk2, 1, 'VMLBRS', 'VmlBri'),
					(@OhPk3, 1, 'VMLMEL', 'VmlMel');

				DECLARE @OaPk1 UNIQUEIDENTIFIER = NEWID();
				DECLARE @OaPk2 UNIQUEIDENTIFIER = NEWID();
				DECLARE @OaPk3 UNIQUEIDENTIFIER = NEWID();
				INSERT dbo.OrgAddress (OA_PK, OA_OH, OA_IsActive, OA_Address1, OA_Address2, OA_City, OA_PostCode, OA_RN_NKCountryCode) VALUES
					(@OaPk1, @OhPk1, 1, '10', 'Pit St', 'SYD', 2000, 'AU'),
					(@OaPk2, @OhPk2, 1, '20', 'Ge St', 'Bri', 4000, 'AU'),
					(@OaPk3, @OhPk3, 1, '30', 'Me St', 'Mel', 3000, 'AU');

				INSERT dbo.OrgCusCode (OK_PK, OK_IsValid, OK_CustomsRegNo, OK_CodeType, OK_RN_NKCodeCountry, OK_OH) VALUES
					(NEWID(), 1, 'HLCU', 'CCC', 'US', @OhPk1),
					(NEWID(), 1, '12345678', 'RUT', 'UY', @OhPk2);

				DECLARE @JkPk1 UNIQUEIDENTIFIER = NEWID();
				DECLARE @JkPk2 UNIQUEIDENTIFIER = NEWID();
				DECLARE @JkPk3 UNIQUEIDENTIFIER = NEWID();
				DECLARE @JkPk4 UNIQUEIDENTIFIER = NEWID();
				DECLARE @JkPk5 UNIQUEIDENTIFIER = NEWID();
				INSERT dbo.JobConsol (JK_PK, JK_UniqueConsignRef, JK_IsForwarding, JK_OA_ShippingLineAddress, JK_RL_NKLoadPort) VALUES
					(@JkPk1, 'CON01', 1, @OAPk1, 'AUSYD'),
					(@JkPk2, 'CON02', 1, @OAPk1, 'AUSYD'),
					(@JkPk3, 'CON03', 1, @OAPk1, 'AUBNE'),
					(@JkPk4, 'CON04', 1, @OAPk2, 'AUBNE'),
					(@JkPk5, 'CON05', 1, @OAPk3, 'AUMEL'),
					(NEWID(), 'CON06', 1, @OAPk2, 'AUBNE');

				DECLARE @JddPk1 UNIQUEIDENTIFIER = NEWID();
				DECLARE @JddPk2 UNIQUEIDENTIFIER = NEWID();
				DECLARE @JddPk3 UNIQUEIDENTIFIER = NEWID();
				DECLARE @JddPk4 UNIQUEIDENTIFIER = NEWID();
				DECLARE @JddPk5 UNIQUEIDENTIFIER = NEWID();
				INSERT dbo.JobDocumentData (JDD_PK, JDD_ParentID, JDD_ParentTableCode, JDD_SystemCreateTimeUtc, JDD_SystemLastEditTimeUtc, JDD_SystemCreateUser, JDD_SystemLastEditUser) VALUES
					(@JddPk1, @JkPk1, 'JK', '2014-04-01 01:00:00', '2014-04-01 02:00:00', 'SYD', 'SYD'),
					(@JddPk2, @JkPk2, 'JK', '2014-03-01 01:00:00', '2014-03-01 02:00:00', 'SYD', 'SYD'),
					(@JddPk3, @JkPk3, 'JK', '2014-03-02 01:00:00', '2014-03-02 02:00:00', 'BNE', 'BNE'),
					(@JddPk4, @JkPk4, 'JK', '2014-05-02 01:00:00', '2014-05-02 02:00:00', 'BNE', 'BNE'),
					(@JddPk5, @JkPk5, 'JK', '2014-03-15 01:00:00', '2014-03-15 02:00:00', 'MEL', 'MEL');

				DECLARE @SpjPk1 UNIQUEIDENTIFIER = NEWID();
				DECLARE @SpjPk2 UNIQUEIDENTIFIER = NEWID();
				DECLARE @SpjPk3 UNIQUEIDENTIFIER = NEWID();
				DECLARE @SpjPk4 UNIQUEIDENTIFIER = NEWID();
				DECLARE @SpjPk5 UNIQUEIDENTIFIER = NEWID();
				INSERT dbo.StmPrintJob (SP_PK, SP_RunDateTime, SP_JobType, SP_DocumentName, SP_ParentTableName, SP_FaxDestination, SP_GB, SP_ParentGuid, SP_GS_NKJobSubmittedBy) VALUES
					(@SpjPk1, '2014-04-01', 'EML', 'Verified Gross Container Weight', 'JobDocumentData', '', @GlbBranchPk, @JddPk1, 'VU1'),
					(@SpjPk2, '2014-03-01', 'EML', 'Verified Gross Container Weight', 'JobDocumentData', '', @GlbBranchPk, @JddPk2, 'VU1'),
					(@SpjPk3, '2014-03-02', 'EML', 'Verified Gross Container Weight', 'JobDocumentData', '', @GlbBranchPk, @JddPk3, 'VU2'),
					(@SpjPk4, '2014-05-02', 'EML', 'Verified Gross Container Weight', 'JobDocumentData', '', @GlbBranchPk, @JddPk4, 'VU2'),
					(@SpjPk5, '2014-03-15', 'EML', 'Verified Gross Container Weight', 'JobDocumentData', '', @GlbBranchPk, @JddPk5, 'VU3');

				INSERT dbo.StmPrintJobCopyRecipient (SPR_PK, SPR_SP, SPR_RecipientType, SPR_EmailAddress) VALUES
					(NEWID(), @SpjPk1, 'TO', 'Job1@document.com'),
					(NEWID(), @SpjPk2, 'TO', 'Job2@document.com'),
					(NEWID(), @SpjPk3, 'TO', 'Job3@document.com'),
					(NEWID(), @SpjPk4, 'TO', 'Job4@document.com'),
					(NEWID(), @SpjPk5, 'TO', 'Job5@document.com');";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 3, transactions.Count());

			var transaction1 = FindRowByRef1(transactions, "Job2@document.com");
			AssertEquals("[T1] CompanyCode", "VML", transaction1.GetCompanyCode());
			AssertEquals("[T1] BranchCode", "VML", transaction1.GetBranchCode());
			AssertEquals("[T1] TransactionDateUtc", new DateTime(2014, 3, 1), transaction1.ServiceOccuredUTC);
			AssertEquals("[T1] UserCode", "VU1", transaction1.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);
			AssertEquals("[T1] TransactionReference02", "HLCU, VmlSyd, LoadPort: AUSYD, 10, Pit St, SYD, 20", transaction1.Reference2);

			var transaction2 = FindRowByRef1(transactions, "Job3@document.com");
			AssertEquals("[T2] CompanyCode", "VML", transaction2.GetCompanyCode());
			AssertEquals("[T2] BranchCode", "VML", transaction2.GetBranchCode());
			AssertEquals("[T2] TransactionDateUtc", new DateTime(2014, 3, 2), transaction2.ServiceOccuredUTC);
			AssertEquals("[T2] UserCode", "VU2", transaction2.ClientStaffCode);
			AssertEquals("[T2] ItemCount", 1, transaction2.BillableCount);
			AssertEquals("[T2] TransactionReference02", "HLCU, VmlSyd, LoadPort: AUBNE, 10, Pit St, SYD, 20", transaction2.Reference2);

			var transaction3 = FindRowByRef1(transactions, "Job5@document.com");
			AssertEquals("[T3] CompanyCode", "VML", transaction3.GetCompanyCode());
			AssertEquals("[T3] BranchCode", "VML", transaction3.GetBranchCode());
			AssertEquals("[T3] TransactionDateUtc", new DateTime(2014, 3, 15), transaction3.ServiceOccuredUTC);
			AssertEquals("[T3] UserCode", "VU3", transaction3.ClientStaffCode);
			AssertEquals("[T3] ItemCount", 1, transaction3.BillableCount);
			AssertEquals("[T3] TransactionReference02", "VmlMel, LoadPort: AUMEL, 30, Me St, Mel, 3000, AU", transaction3.Reference2);
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2014, 3);
			}
		}
	}
}
