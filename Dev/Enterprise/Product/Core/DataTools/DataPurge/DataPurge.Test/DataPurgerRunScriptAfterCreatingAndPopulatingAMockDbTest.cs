using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DataPurge.Test.Utility;
using Enterprise.DataPurge.Utility;
using Enterprise.DbUpgrader.Resource;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DataPurge
{
	sealed class DataPurgerRunScriptAfterCreatingAndPopulatingAMockDbTest : TestCaseWithFactory
	{
		#region TestBaseScript

		[SnailTest]
		public void TestBaseScript()
		{
			RunTestWithMockDatabase(AssertRunBaseScript);
		}

		void AssertRunBaseScript()
		{
			// Assert data is there before purge
			AssertTableHasRows("AccTransactionHeader");
			AssertTableHasRows("AccStatement");
			AssertTableHasRows("AccTaxTransaction");
			AssertTableHasRows("AccTaxRecordTransactionLinePivot");
			AssertTableHasRows("AccTaxGLMovement");
			AssertTableHasRows("AccTaxGLMovementQueue");
			AssertTableHasRows("AccGeneralLedgerData");
			AssertTableHasRows("AccDraftInvoiceHeader");

			// RefContainerStock
			AssertTableHasRows("JobContainerDetention");
			AssertTableHasRows("JobContainerMove");
			AssertTableHasRows("RefContainerStock");

			// ProcessTasks
			AssertTableHasRows("ProcessTasks");
			int processTaskRowCount = TableRowCount("ProcessTasks");
			InsertTemplateProcessTask();
			AssertTableRowCount("ProcessTasks ", processTaskRowCount + 1);
			AssertEquals("Template Process Task count", 1, GetTemplateProcessTaskRowCount());

			// StmScheduleTask
			AssertTableHasRows("StmScheduleTask");
			int stmScheduleTaskRowCount = TableRowCount("StmScheduleTask");
			InsertServiceTaskSchedule_StmScheduleTask();
			AssertTableRowCount("StmScheduleTask ", stmScheduleTaskRowCount + 1);
			AssertEquals("Service Task Schedule count", 1, GetServiceTaskScheduleCount_StmScheduleTask());

			//RatingHeader
			AssertTableHasRows("RatingHeader");
			int ratingHeaderRowCount = TableRowCount("RatingHeader");
			int oneTimeQuoteRatingHeaderRowCount = GetOneTimeQuoteRatingHeaderCount();
			InsertRatingTree();
			AssertEquals("RatingHeader ", oneTimeQuoteRatingHeaderRowCount + 1, GetOneTimeQuoteRatingHeaderCount());

			//StmNote
			AssertTableHasRows("StmNote");
			int orphanStmNoteCount = GetCountOfOrphanStmNoteWithRefCountryParent();
			int nonOrphanStmNoteCount = GetCountOfNonOrphanStmNoteWithRefCountryParent();
			int orphanAccPayableOrderHeaderCount = GetCountOfOrphanStmNoteWithAccPayableOrderHeaderParent();
			InsertStmNotes();
			AssertEquals("StmNote without RefCountry parent should have been created", orphanStmNoteCount + 1, GetCountOfOrphanStmNoteWithRefCountryParent());
			AssertEquals("StmNote with RefCountry parent should have been created", nonOrphanStmNoteCount + 1, GetCountOfNonOrphanStmNoteWithRefCountryParent());
			AssertEquals("StmNote with AccPayableOrderHeader parent should have been created", orphanAccPayableOrderHeaderCount + 1, GetCountOfOrphanStmNoteWithAccPayableOrderHeaderParent());

			//StmLink
			AssertTableHasRows("StmLink");
			int stmLinkModuleAndFavoriteRowCount = GetCountOfStmLinkOfModules();
			int stmLinkRecentItemRowCount = GetCountOfStmLinkOfRecentItems();
			InsertStmLinks();
			AssertEquals("StmLink to Module should have been created", stmLinkModuleAndFavoriteRowCount + 1, GetCountOfStmLinkOfModules());
			AssertEquals("StmLink to recent item and favorite should have been created", stmLinkRecentItemRowCount + 3, GetCountOfStmLinkOfRecentItems());

			//JobDocAddress
			AssertTableHasRows("JobDocAddress");
			int orphanRefCountryJobDocAddressCount = GetCountOfOrphanJobDocAddressesWithRefCountryParent();
			int orphanJPAFRBillsJobDocAddressCount = GetCountOfOrphanJobDocAddressesWithJPAFRBillsParent();
			int nonOrphanJobDocAddressCount = GetCountOfNonOrphanJobDocAddressesWithRefCountryParent();
			InsertJobDocAddresses();
			AssertEquals("JobDocAddress without parent RefCountry should have been created", orphanRefCountryJobDocAddressCount + 1, GetCountOfOrphanJobDocAddressesWithRefCountryParent());
			AssertEquals("JobDocAddress without parent JPAFRBills should have been created", orphanJPAFRBillsJobDocAddressCount + 1, GetCountOfOrphanJobDocAddressesWithJPAFRBillsParent());
			AssertEquals("JobDocAddress with RefCountry parent should have been created", nonOrphanJobDocAddressCount + 1, GetCountOfNonOrphanJobDocAddressesWithRefCountryParent());

			//GenApprovalRequest
			InsertGenApprovalRequest();
			AssertTableHasRows("GenApprovalRequest");

			//Inquires / Communications
			AssertTableHasRows("OrgColdCallRegister");
			AssertTableHasRows("OrgSalesCallAdditionalAttendee");
			AssertTableHasRows("OrgSalesCall");

			//StmTemplateRecord
			InsertStmTemplateRecord();
			AssertTableHasRows("StmTemplateRecord");

			//EPaymentRelatedData
			InsertEPaymentRelatedData();
			AssertTableHasRows("AccPaymentApproval");
			AssertTableHasRows("AccEPaymentQuote");
			AssertTableHasRows("AccEPaymentDeal");

			// ----------
			// PURGE DATA
			// ----------
			var purger = new DataPurger();
			DataPurger.RunScriptCollection(testConnection, purger.GetPurgeScripts());

			// Assert data was purged on key tables (base purge)
			AssertTableRowCount("AccTransactionHeader", 0);
			AssertTableRowCount("AccStatement", 0);
			AssertTableRowCount("AccGLAggregate", 0);
			AssertTableRowCount("AccGLBudget", 0);
			AssertTableRowCount("JobHeader", 0);
			AssertTableRowCount("JobDeclaration", 0);
			AssertTableRowCount("JobConsol", 0);
			AssertTableRowCount("JobShipment", 0);
			AssertTableRowCount("JobContainerDetention", 0);
			AssertTableRowCount("RefContainerStock", 0);
			AssertTableRowCount("JobVoyage", 0);
			AssertTableRowCount("JobContainer", 0);
			AssertTableRowCount("JobStorage", 0);
			AssertTableRowCount("WhsDocket", 0);
			AssertTableRowCount("WhsPick", 0);
			AssertTableRowCount("StmPrintJob", 0);
			AssertTableRowCount("StmDeliveryGroup", 0);
			AssertTableRowCount("EDIInterchange", 0);
			AssertTableRowCount("StmNums", 0);
			AssertTableRowCount("AccTaxTransaction", 0);
			AssertTableRowCount("AccTaxRecordTransactionLinePivot", 0);
			AssertTableRowCount("AccTaxGLMovement", 0);
			AssertTableRowCount("AccTaxGLMovementQueue", 0);
			AssertTableRowCount("AccGeneralLedgerData", 0);
			AssertTableRowCount("AccDraftInvoiceHeader", 0);

			// ProcessTasks
			AssertTableRowCount("ProcessTasks", 1);
			AssertEquals("Template Process Task count", 1, GetTemplateProcessTaskRowCount());

			// StmScheduleTask
			AssertTableRowCount("StmScheduleTask", 1);
			AssertEquals("Service Task Schedule count", 1, GetServiceTaskScheduleCount_StmScheduleTask());

			//RatingHeader
			AssertTableRowCount("RatingHeader", ratingHeaderRowCount - oneTimeQuoteRatingHeaderRowCount);
			AssertEquals("One Time Quote RatingHeader count", 0, GetOneTimeQuoteRatingHeaderCount());

			//StmNote
			AssertEquals("StmNote without RefCountry parent is purged", 0, GetCountOfOrphanStmNoteWithRefCountryParent());
			AssertEquals("StmNote with RefCountry parent not purged", nonOrphanStmNoteCount + 1, GetCountOfNonOrphanStmNoteWithRefCountryParent());
			AssertEquals("StmNote with AccPayableOrderHeader parent purged", 0, GetCountOfOrphanStmNoteWithAccPayableOrderHeaderParent());

			//StmLink
			AssertEquals("StmLink for Module is purged", stmLinkModuleAndFavoriteRowCount + 1, GetCountOfStmLinkOfModules());
			AssertEquals("StmLink for recent item and favorite is purged", 0, GetCountOfStmLinkOfRecentItems());

			//GenApprovalRequest
			AssertTableRowCount("GenApprovalRequest", 0);

			//JobDocAddress
			AssertEquals("JobDocAddress without parent is purged", 0, GetCountOfOrphanJobDocAddressesWithRefCountryParent());
			AssertEquals("JobDocAddress without parent is purged", 0, GetCountOfOrphanJobDocAddressesWithJPAFRBillsParent());
			AssertEquals("JobDocAddress with RefCountry parent not purged", nonOrphanJobDocAddressCount + 1, GetCountOfNonOrphanJobDocAddressesWithRefCountryParent());

			//Inquires / Communications
			AssertTableRowCount("OrgColdCallRegister", 0);
			AssertTableRowCount("OrgSalesCallAdditionalAttendee", 0);
			AssertTableRowCount("OrgSalesCall", 0);

			//StmTemplateRecord
			AssertTableRowCount("StmTemplateRecord", 0);

			//EPaymentRelatedData
			AssertTableRowCount("AccPaymentApproval", 0);
			AssertTableRowCount("AccEPaymentQuote", 0);
			AssertTableRowCount("AccEPaymentDeal", 0);
		}

		#region Template ProcessTasks

		void InsertTemplateProcessTask()
		{
			string sqlText = @"
				DECLARE @TemplateProccessTask uniqueidentifier
				SET @TemplateProccessTask = (SELECT TOP 1 P0_PK  AS PK FROM dbo.ProcessTaskTemplate)
				INSERT INTO dbo.ProcessTasks (P9_PK, P9_ParentID) VALUES (NEWID(), @TemplateProccessTask)";
			testConnection.ExecuteNonQuery(sqlText, 1800);
		}

		int GetTemplateProcessTaskRowCount()
		{
			string sqlText = "SELECT COUNT(*) FROM dbo.ProcessTasks WHERE P9_ParentID in (SELECT P0_PK FROM dbo.ProcessTaskTemplate)";
			return (int)testConnection.ExecuteScalar(sqlText);
		}

		#endregion

		#region RatingHeader

		void InsertRatingTree()
		{
			string sqlText = @"
DECLARE @RatingHeaderPk uniqueidentifier
DECLARE @RateOneOffShipmentPk uniqueidentifier
DECLARE @RateEntryPk uniqueidentifier
DECLARE @RateLinesPk uniqueidentifier
DECLARE @AccChargeCodePk uniqueidentifier

SET @RatingHeaderPk = newid()
INSERT dbo.RatingHeader (TH_PK, TH_OneTimeQuote,TH_RateType)
VALUES(@RatingHeaderPk, 1, 'TST')

INSERT dbo.RateTariffDiscount(TD_PK, TD_TH)
VALUES (NEWID(), @RatingHeaderPk)

INSERT dbo.RateAttachment(TA_PK, TA_TH)
VALUES (NEWID(), @RatingHeaderPk)

SET @RateOneOffShipmentPk = newid()
INSERT dbo.RateOneOffShipment(TT_PK, TT_TH)
VALUES (@RateOneOffShipmentPk, @RatingHeaderPk)

INSERT dbo.RateOneOffContainers(TC_PK, TC_TT)
VALUES (NEWID(), @RateOneOffShipmentPk)

INSERT dbo.RateOneOffPackLine(TPL_PK, TPL_TT_RateOneOffShipment)
VALUES (NEWID(), @RateOneOffShipmentPk)

SET @RateEntryPk = newid()
INSERT dbo.RateEntry(TI_PK, TI_TH)
VALUES (@RateEntryPk, @RatingHeaderPk)

SET @RateLinesPk = newid()
SET @AccChargeCodePk = (SELECT TOP 1 AC_PK FROM dbo.AccChargeCode)
INSERT dbo.RateLines(TL_PK, TL_TI, TL_AC)
VALUES (@RateLinesPk, @RateEntryPk, @AccChargeCodePk)

INSERT dbo.RateLineItems(TM_PK, TM_TL)
VALUES (NEWID(), @RateLinesPk)";

			testConnection.ExecuteNonQuery(sqlText, 1800);
		}

		int GetOneTimeQuoteRatingHeaderCount()
		{
			string sqlText = "SELECT COUNT(*) FROM dbo.RatingHeader WHERE TH_OneTimeQuote = 1";
			return (int)testConnection.ExecuteScalar(sqlText);
		}

		#endregion

		#region ServiceTask (StmScheduleTask)

		void InsertServiceTaskSchedule_StmScheduleTask()
		{
			string sqlText = String.Format(
				"INSERT INTO dbo.StmScheduleTask (S5_PK, S5_ParentTableCode) VALUES (NEWID(), '{0}')",
				StmServiceHostSchema.Constants.Prefix);
			testConnection.ExecuteNonQuery(sqlText);
		}

		int GetServiceTaskScheduleCount_StmScheduleTask()
		{
			string sqlText = String.Format(
				"SELECT COUNT(*) FROM dbo.StmScheduleTask WHERE S5_ParentTableCode = '{0}'",
				StmServiceHostSchema.Constants.Prefix);
			return (int)testConnection.ExecuteScalar(sqlText);
		}

		#endregion

		#region StmNote

		void InsertStmNotes()
		{
			string sqlText = @"
				DECLARE @RefCountryPk uniqueidentifier
				SET @RefCountryPk = (SELECT TOP 1 RN_PK FROM dbo.RefCountry)
				INSERT dbo.StmNote (ST_PK, ST_ParentID, ST_Table) VALUES(NEWID(), @RefCountryPk, 'RefCountry')
				INSERT dbo.StmNote (ST_PK, ST_ParentID, ST_Table) VALUES(NEWID(), NEWID(), 'RefCountry')
				INSERT dbo.StmNote (ST_PK, ST_ParentID, ST_Table) VALUES(NEWID(), NEWID(), 'AccPayableOrderHeader')";
			testConnection.ExecuteNonQuery(sqlText);
		}

		int GetCountOfOrphanStmNoteWithRefCountryParent()
		{
			string sqlText = @"
				SELECT COUNT(*) FROM dbo.StmNote 
				WHERE ST_ParentID not in (SELECT RN_PK FROM dbo.RefCountry) 
				AND ST_Table = 'RefCountry'";
			return (int)testConnection.ExecuteScalar(sqlText);
		}

		int GetCountOfNonOrphanStmNoteWithRefCountryParent()
		{
			string sqlText = @"
				SELECT COUNT(*) FROM dbo.StmNote 
				WHERE ST_ParentID in (SELECT RN_PK FROM dbo.RefCountry) 
				AND ST_Table = 'RefCountry'";
			return (int)testConnection.ExecuteScalar(sqlText);
		}

		int GetCountOfOrphanStmNoteWithAccPayableOrderHeaderParent()
		{
			string sqlText = @"
				SELECT COUNT(*) FROM dbo.StmNote 
				WHERE ST_ParentID not in (SELECT APH_PK FROM dbo.AccPayableOrderHeader) 
				AND ST_Table = 'AccPayableOrderHeader'";
			return (int)testConnection.ExecuteScalar(sqlText);
		}

		#endregion

		#region StmLink

		void InsertStmLinks()
		{
			string sqlText = @"
				DECLARE @GlbCompanyPk uniqueidentifier
				SET @GlbCompanyPk = (SELECT TOP 1 GC_PK FROM dbo.GlbCompany)
				INSERT dbo.StmLink (STL_PK, STL_LinkType, STL_GS_NKUser, STL_ModuleID, STL_ItemUrl, STL_ItemPK, STL_ItemDescription, STL_GC_LogonCompany, STL_ShortcutIndex, STL_LastUsedDateTimeUtc) VALUES(NEWID(), 'RUM', '~BP', 'AnyModule', '', null, '---', @GlbCompanyPK, 1, SYSUTCDATETIME())
				INSERT dbo.StmLink (STL_PK, STL_LinkType, STL_GS_NKUser, STL_ModuleID, STL_ItemUrl, STL_ItemPK, STL_ItemDescription, STL_GC_LogonCompany, STL_ShortcutIndex, STL_LastUsedDateTimeUtc) VALUES(NEWID(), 'FAV', '~BP', 'AnyModule', 'AnyUrl', NEWID(), '---', @GlbCompanyPK, 1, SYSUTCDATETIME())
				INSERT dbo.StmLink (STL_PK, STL_LinkType, STL_GS_NKUser, STL_ModuleID, STL_ItemUrl, STL_ItemPK, STL_ItemDescription, STL_GC_LogonCompany, STL_ShortcutIndex, STL_LastUsedDateTimeUtc) VALUES(NEWID(), 'MRI', '~BP', 'AnyModule', 'AnyUrl', NEWID(), '---', @GlbCompanyPK, 2, SYSUTCDATETIME())
				INSERT dbo.StmLink (STL_PK, STL_LinkType, STL_GS_NKUser, STL_ModuleID, STL_ItemUrl, STL_ItemPK, STL_ItemDescription, STL_GC_LogonCompany, STL_ShortcutIndex, STL_LastUsedDateTimeUtc) VALUES(NEWID(), 'RUI', '~BP', 'AnyModule', 'AnyUrl', NEWID(), '---', @GlbCompanyPK, 3, SYSUTCDATETIME())";
			testConnection.ExecuteNonQuery(sqlText);
		}

		int GetCountOfStmLinkOfModules()
		{
			string sqlText = @"
				SELECT COUNT(*) FROM dbo.StmLink
				where STL_ItemUrl = ''";
			return (int)testConnection.ExecuteScalar(sqlText);
		}

		int GetCountOfStmLinkOfRecentItems()
		{
			string sqlText = @"
				SELECT COUNT(*) FROM dbo.StmLink
				where not (STL_ItemUrl = '')";
			return (int)testConnection.ExecuteScalar(sqlText);
		}

		#endregion

		#region StmTemplateRecord
		void InsertStmTemplateRecord()
		{
			string sqlText = @"
				INSERT dbo.StmTemplateRecord (STR_PK, STR_ModuleID, STR_ReferenceId, STR_Data, STR_SystemCreateTimeUtc, STR_SystemCreateUser, STR_SystemLastEditTimeUtc, STR_SystemLastEditUser) VALUES(NEWID(), 'JobShipment', 'TR00001001', '<xml/>', SYSUTCDATETIME(), 'E', SYSUTCDATETIME(), SYSUTCDATETIME())";
			testConnection.ExecuteNonQuery(sqlText);
		}

		#endregion

		#region JobDocAddress

		void InsertJobDocAddresses()
		{
			string sqlText = @"
				DECLARE @RefCountryPk uniqueidentifier
				SET @RefCountryPk = (SELECT TOP 1 RN_PK FROM dbo.RefCountry)
				INSERT dbo.JobDocAddress (E2_PK, E2_ParentID, E2_ParentTableCode) VALUES(NEWID(), @RefCountryPk, 'RN')
				INSERT dbo.JobDocAddress (E2_PK, E2_ParentID, E2_ParentTableCode) VALUES(NEWID(), NEWID(), 'RN')
				INSERT dbo.JobDocAddress (E2_PK, E2_ParentID, E2_ParentTableCode) VALUES(NEWID(), NEWID(), 'JPB')";
			testConnection.ExecuteNonQuery(sqlText);
		}

		int GetCountOfOrphanJobDocAddressesWithRefCountryParent()
		{
			string sqlText = @"
				SELECT COUNT(*) FROM dbo.JobDocAddress 
				WHERE E2_ParentID not in (SELECT RN_PK FROM dbo.RefCountry) 
				AND E2_ParentTableCode = 'RN'";
			return (int)testConnection.ExecuteScalar(sqlText);
		}

		int GetCountOfOrphanJobDocAddressesWithJPAFRBillsParent()
		{
			string sqlText = @"
				SELECT COUNT(*) FROM dbo.JobDocAddress 
				WHERE E2_ParentID not in (SELECT JPB_PK FROM dbo.JPAFRBills) 
				AND E2_ParentTableCode = 'JPB'";
			return (int)testConnection.ExecuteScalar(sqlText);
		}

		int GetCountOfNonOrphanJobDocAddressesWithRefCountryParent()
		{
			string sqlText = @"
				SELECT COUNT(*) FROM dbo.JobDocAddress 
				WHERE E2_ParentID in (SELECT RN_PK FROM dbo.RefCountry) 
				AND E2_ParentTableCode = 'RN'";
			return (int)testConnection.ExecuteScalar(sqlText);
		}

		#endregion

		#region GenApprovalRequest

		void InsertGenApprovalRequest()
		{
			string sqlText = @"
				DECLARE @GlbCompanyPk uniqueidentifier
				DECLARE @BranchPk uniqueidentifier

				SET @GlbCompanyPk = (SELECT TOP 1 GC_PK FROM dbo.GlbCompany)
				SET @BranchPk = (SELECT TOP 1 GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GlbCompanyPk)

				INSERT INTO dbo.GenApprovalRequest
           (XP_PK, XP_GB_RequestingBranch, XP_SubSystem, XP_ApprovalType, XP_ApprovalStatus, XP_ApprovalDate, XP_ReasonCode
           ,XP_ApprovalRequestData
           ,XP_PrivledgeRequired, XP_GS_NKApprovingUser1, XP_GS_NKApprovingUser2, XP_GS_NKApprovingUser3
           ,XP_SystemCreateUser, XP_SystemCreateTimeUtc, XP_ParentID, XP_ParentTableCode, XP_RequestID, XP_ReasonDescription
		   ,XP_GB_JobBranch, XP_GE_JobDepartment)
			VALUES
           (NEWID(), @BranchPk, 'ACC', 'RCD','REQ', GETDATE(), ''
           ,0x505AAD914F4FC24010C5DF4769B8172A3560492D3172314A62E2C52BB60B98F48F912DF0E9D5DF4EBC403C78209BDD9D7933FBDECCECF757AEB98E6A542BD25E4E9FDAE95D9D5ADD6AA02B0D957047445A95E015D1561B8BF6F25A2B266B823757A15CF73038CBF2D881C78374F0D7862FB04B5E36C6E851BBD3072BE4ECB5226BC1E9D9D16F5D2D3933EC505750DD12F5BC9869C43AD81AB253CE0E9E0DE898AA13EA1AE9554B3DE905CD2D8A0DBCB175B0338DD093B3FECEB5AA8B680D6C264BEBB5D7035C017FD62378A26BBC52195A139BE30DBC1568CC4E898FB132BD61A75829F10AD4696A5567206BB01CE5BF15F293D93ABCDAE67BFE0385FD518F1FB8FEFB26E45EEEB70BFD00
           ,'3','','',''
           ,'E', SYSUTCDATETIME(), NEWID(), 'AH','',''
           ,NULL, NULL)";

			testConnection.ExecuteNonQuery(sqlText);
		}

		#endregion

		#region EPayment Related Data
		void InsertEPaymentRelatedData()
		{
			var testPaymentApprovalPk = Guid.NewGuid().ToString();
			var testEpaymentQuotePk = Guid.NewGuid().ToString();
			var testEpaymentDealPk = Guid.NewGuid().ToString();

			var sqlText = "SELECT TOP 1 GC_PK FROM dbo.GlbCompany";
			var testCompanyPk = testConnection.ExecuteScalar(sqlText).ToString();

			sqlText = string.Format("SELECT TOP 1 GB_PK FROM dbo.GlbBranch WHERE GB_GC = '{0}'", testCompanyPk);
			var testBranchPk = testConnection.ExecuteScalar(sqlText).ToString();

			sqlText = string.Format("SELECT TOP 1 OH_PK FROM dbo.OrgHeader");
			var testOrgPk = testConnection.ExecuteScalar(sqlText).ToString();

			//Renaming Column name in Test Data.
			sqlText = string.Format(@"Insert into dbo.AccPaymentApproval (AV_PK, AV_RejectionReasonDetails, AV_GB, AV_GC, AV_OH)
VALUES
('{0}', 'TEST REASON 1', '{1}', '{2}', '{3}')", testPaymentApprovalPk, testBranchPk, testCompanyPk, testOrgPk);
			testConnection.ExecuteNonQuery(sqlText);

			sqlText = string.Format(@"Insert into dbo.AccEPaymentQuote
(QU_PK, QU_InternalReference, QU_AV, QU_GC, QU_Status, QU_ProviderCode, QU_FromAmount, QU_RX_NKFromCurrency, QU_ToAmount, QU_RX_NKToCurrency, QU_FeeAmount, QU_ExchangeRate, QU_SystemCreateTimeUtc, QU_SystemCreateUser)
Values
(
'{0}',
'00001001',
'{1}',
'{2}',
'REQ',
'OFX',
0,
'AUD',
200,
'USD',
0.00,
0.00,
GETDATE(),
'E'
)", testEpaymentQuotePk, testPaymentApprovalPk, testCompanyPk);
			testConnection.ExecuteNonQuery(sqlText);

			sqlText = string.Format(@"Insert into dbo.AccEPaymentDeal (AED_PK,
AED_GC_Company,
AED_QU_Quote,
AED_InternalReference,
AED_ProviderCode,
AED_ProviderReference,
AED_Status,
AED_LastResponseReceivedUtc,
AED_ErrorDescription,
AED_SystemCreateTimeUtc,
AED_SystemCreateUser,
AED_SystemLastEditTimeUtc
)
Values
(
'{0}',
'{1}',
'{2}',
'00001001',
'OFX',
'5AF3326D-2E08-4BEA-81C9-4C3D1F916114',
'PAI',
SYSUTCDATETIME(),
'Paid',
SYSUTCDATETIME(),
'E',
SYSUTCDATETIME()
)", testEpaymentDealPk, testCompanyPk, testEpaymentQuotePk);
			testConnection.ExecuteNonQuery(sqlText);
		}
		#endregion

		#endregion

		#region TestTariffInfoScript

		[SnailTest]
		public void TestTariffInfoScript()
		{
			RunTestWithMockDatabase(AssertRunTariffInfoScript);
		}

		void AssertRunTariffInfoScript()
		{
			DataPurger purger = new DataPurger();
			purger.HasTariffInfoPurgeScript = true;
			DataPurger.RunScriptCollection(testConnection, purger.GetPurgeScripts());

			// Assert data was purged on key tables
			AssertEquals("CusClassification should have NO rows", 0, TableRowCount("CusClassification"));

			AssertEquals("DashDocCoordinate should have NO rows", 0, TableRowCount("DashDocCoordinate"));
			AssertEquals("DashCommercialInvoiceLineItem should have NO rows", 0, TableRowCount("DashCommercialInvoiceLineItem"));
			AssertEquals("DashCommercialInvoice should have NO rows", 0, TableRowCount("DashCommercialInvoice"));
			AssertEquals("DashDocument should have NO rows", 0, TableRowCount("DashDocument"));

			// Assert data was purged on key tables (base purge)
			AssertEquals("AccTransactionHeader should have NO rows", 0, TableRowCount("AccTransactionHeader"));
			AssertEquals("JobVoyage should have NO rows", 0, TableRowCount("JobVoyage"));
			AssertEquals("StmNums should have NO rows", 0, TableRowCount("StmNums"));
		}

		#endregion

		#region TestQuotationsScript

		[SnailTest]
		public void TestQuotationsScript()
		{
			RunTestWithMockDatabase(AssertRunQuotationsScript);
		}

		void AssertRunQuotationsScript()
		{
			AssertEquals("[PRE-CONDITION] RatingHeader has rows", true, TableRowCount("RatingHeader") > 0);

			DataPurger purger = new DataPurger();
			purger.HasQuotationsPurgeScript = true;

			testConnection.ExecuteNonQuery("UPDATE dbo.RatingHeader SET TH_RateType = 'QTE'");

			DataPurger.RunScriptCollection(testConnection, purger.GetPurgeScripts());

			// Assert data was purged on key tables
			AssertEquals("RatingHeader should have NO rows", 0, TableRowCount("RatingHeader"));

			// Assert data was purged on key tables (base purge)
			AssertEquals("AccTransactionHeader should have NO rows", 0, TableRowCount("AccTransactionHeader"));
			AssertEquals("JobVoyage should have NO rows", 0, TableRowCount("JobVoyage"));
			AssertEquals("StmNums should have NO rows", 0, TableRowCount("StmNums"));
		}

		#endregion

		#region TestProductInfoScript

		[SnailTest]
		public void TestProductInfoScript()
		{
			RunTestWithMockDatabase(AssertRunProductInfoScript);
		}

		void AssertRunProductInfoScript()
		{
			DataPurger purger = new DataPurger();
			purger.HasProductInfoPurgeScript = true;
			DataPurger.RunScriptCollection(testConnection, purger.GetPurgeScripts());

			string periodSqlText =
@"SELECT COUNT(*) 
FROM 
	dbo.OrgTradePeriod 
	JOIN dbo.OrgTradeDetail On PAS_PA = PA_PK
WHERE PA_OP IS NOT NULL
";
			var periodCount = (int)testConnection.ExecuteScalar(periodSqlText);

			string tradeValueSqlText =
@"SELECT COUNT(*) 
FROM 
	dbo.OrgTradeValue
	JOIN dbo.OrgTradePeriod ON PAV_PAS = PAS_PK 
	JOIN dbo.OrgTradeDetail On PAS_PA = PA_PK
WHERE PA_OP IS NOT NULL
";
			var valueCount = (int)testConnection.ExecuteScalar(tradeValueSqlText);

			AssertEquals("OrgTradePeriod should have NO rows", 0, periodCount);
			AssertEquals("OrgTradeValue should have NO rows", 0, valueCount);

			// Assert data was purged on key tables
			AssertEquals("OrgSupplierPart should have NO rows", 0, TableRowCount("OrgSupplierPart"));
			AssertEquals("DashDocCoordinate should have NO rows", 0, TableRowCount("DashDocCoordinate"));
			AssertEquals("DashCommercialInvoiceLineItem should have NO rows", 0, TableRowCount("DashCommercialInvoiceLineItem"));
			AssertEquals("DashCommercialInvoice should have NO rows", 0, TableRowCount("DashCommercialInvoice"));
			AssertEquals("DashDocument should have NO rows", 0, TableRowCount("DashDocument"));

			// Assert data was purged on key tables (base purge)
			AssertEquals("AccTransactionHeader should have NO rows", 0, TableRowCount("AccTransactionHeader"));
			AssertEquals("JobVoyage should have NO rows", 0, TableRowCount("JobVoyage"));
			AssertEquals("StmNums should have NO rows", 0, TableRowCount("StmNums"));
		}

		#endregion

		#region TestNonSystemChageCodesScript

		[SnailTest]
		public void TestNonSystemChargeCodesScript()
		{
			RunTestWithMockDatabase(AssertRunNonSystemChargeCodesScript);
		}

		/// <summary>
		/// Tests purge of non-system charge codes.
		/// Note: System charge-codes are the ones that have the same code as one of the Demo Company chage codes.
		/// </summary>
		void AssertRunNonSystemChargeCodesScript()
		{
			string demoCompanyPk = Guid.NewGuid().ToString();
			PrepareChargeCodeTestData(demoCompanyPk);
			Factory.Save();
			string sqlText = "SELECT count(*) FROM dbo.AccChargeCode WHERE AC_Code in (SELECT AC_Code FROM dbo.AccChargeCode WHERE AC_GC = '" + demoCompanyPk + "')";
			int numOfSystemCodes = Convert.ToInt32(testConnection.ExecuteScalar(sqlText));

			sqlText = "SELECT count(*) FROM dbo.AccChargeCode WHERE AC_Code not in (SELECT AC_Code FROM dbo.AccChargeCode WHERE AC_GC = '" + demoCompanyPk + "')";
			int rowCount = Convert.ToInt32(testConnection.ExecuteScalar(sqlText));
			Assert("[BEFORE PURGE] There should be non-system charge codes", rowCount > 0);

			sqlText = "SELECT count(*) from dbo.AccChargeSupplyTypeOverride";
			rowCount = Convert.ToInt32(testConnection.ExecuteScalar(sqlText));
			AssertEquals("[BEFORE PURGE] There should be 2 records in table AccChargeSupplyTypeOverride before purge.", 2, rowCount);

			//	DataPurger purger = new DataPurger();
			//	
			DataPurger purger = new DataPurger();
			purger.HasNonSystemChageCodesPurgeScript = true;
			DataPurger.RunScriptCollection(testConnection, purger.GetPurgeScripts());

			sqlText = String.Format("SELECT count(*) FROM dbo.AccChargeCode WHERE AC_Code != '{0}'", TestSystemChargeCode);
			rowCount = Convert.ToInt32(testConnection.ExecuteScalar(sqlText));
			AssertEquals("Non-system charge code inserted in the test should have been deleted", 0, rowCount);

			sqlText = "SELECT count(*) FROM dbo.AccChargeCode WHERE AC_Code in (SELECT AC_Code FROM dbo.AccChargeCode WHERE AC_GC = '" + demoCompanyPk + "')";
			rowCount = Convert.ToInt32(testConnection.ExecuteScalar(sqlText));
			AssertEquals("All system charge codes should have been preserved", numOfSystemCodes, rowCount);

			sqlText = "SELECT count(*) FROM dbo.AccChargeCode";
			rowCount = Convert.ToInt32(testConnection.ExecuteScalar(sqlText));
			AssertEquals("Only system charge codes should have been preserved", numOfSystemCodes, rowCount);

			sqlText = "SELECT count(*) from dbo.AccChargeSupplyTypeOverride";
			rowCount = Convert.ToInt32(testConnection.ExecuteScalar(sqlText));
			AssertEquals("[AFTER PURGE] There should be 0 record in table AccChargeSupplyTypeOverride after purge.", 0, rowCount);
			// Assert data was purged on key tables (base purge)
			AssertEquals("AccTransactionHeader should have NO rows", 0, TableRowCount("AccTransactionHeader"));
			AssertEquals("JobVoyage should have NO rows", 0, TableRowCount("JobVoyage"));
			AssertEquals("StmNums should have NO rows", 0, TableRowCount("StmNums"));
		}

		/// <summary>
		/// 1/ Inserts a new company to be the DEMO company (since system charge codes are the ones shipped with the Demo company).
		/// 2/ Inserts only one ChargeCode associated with this Demo company.
		/// This way the Charge Codes which code doesn't match the Demo company one will have to be purged, 
		/// and the purge script will have to handle this by deleting tables following their hierarchy.
		/// </summary>
		void PrepareChargeCodeTestData(string demoCompanyPk)
		{
			string sqlText = String.Format("INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode) SELECT TOP 1 '{0}', 'DEM', 'EM company', GC_RX_NKLocalCurrency, GC_RN_NKCountryCode FROM dbo.GlbCompany", demoCompanyPk);
			testConnection.ExecuteNonQuery(sqlText);
			sqlText = String.Format("INSERT dbo.AccChargeCode (AC_PK, AC_Code, AC_GC) VALUES (NEWID(), '{0}', '{1}')", TestSystemChargeCode, demoCompanyPk);
			testConnection.ExecuteNonQuery(sqlText);
			sqlText = String.Format("SELECT AC_PK from dbo.AccChargeCode WHERE AC_Code='{0}' AND AC_GC='{1}'", TestSystemChargeCode, demoCompanyPk);
			ZGuid id = new ZGuid(testConnection.ExecuteScalar(sqlText));
			var acsto1 = CreateAccChargeSupplyTypeOverride("AC", id);
			var acsto2 = CreateAccChargeSupplyTypeOverride("AC", id);
		}

		#region CreateAccChargeSupplyTypeOverride
		AccChargeSupplyTypeOverride CreateAccChargeSupplyTypeOverride(string code, ZGuid testChargeCodePK)
		{
			var chargeSupplyTypeOverride = Factory.NewWithValidTestData<AccChargeSupplyTypeOverride>();
			chargeSupplyTypeOverride.ACS_ParentTableCode = code;
			chargeSupplyTypeOverride.ACS_ParentID = testChargeCodePK;
			return chargeSupplyTypeOverride;
		}
		#endregion
		#endregion

		#region TestRatingInfoScript

		[SnailTest]
		public void TestRatingInfoScript()
		{
			RunTestWithMockDatabase(AssertRunRatingInfoScript);
		}

		void AssertRunRatingInfoScript()
		{
			DataPurger purger = new DataPurger();
			purger.HasRatingInfoPurgeScript = true;
			DataPurger.RunScriptCollection(testConnection, purger.GetPurgeScripts());

			// Assert data was purged on key tables
			AssertEquals("RatingHeader should have NO rows", 0, TableRowCount("RatingHeader"));

			// Assert data was purged on key tables (base purge)
			AssertEquals("AccTransactionHeader should have NO rows", 0, TableRowCount("AccTransactionHeader"));
			AssertEquals("JobVoyage should have NO rows", 0, TableRowCount("JobVoyage"));
			AssertEquals("StmNums should have NO rows", 0, TableRowCount("StmNums"));
		}

		#endregion

		#region TestNonProxyOrganisationsScript

		public void FAT_NonProxyOrganisationsPurgeScript()
		{
			RunTestWithMockDatabase(AssertRunNonProxyOrganisationsScript);
		}

		void AssertRunNonProxyOrganisationsScript()
		{
			// Inserts an extra Organisation to be the OrgProxy (for both Company and Branch records)
			string newProxyOrgPk = Guid.NewGuid().ToString();
			string sqlText = String.Format("INSERT dbo.OrgHeader (OH_PK) VALUES ('{0}')", newProxyOrgPk);
			testConnection.ExecuteNonQuery(sqlText);

			// Updates the Company and Branch records to point to the new Organisation
			sqlText = String.Format(@"
				UPDATE dbo.GlbCompany SET GC_OH_OrgProxy = '{0}';
				UPDATE dbo.GlbBranch SET GB_OH_OrgProxy = '{0}'",
				newProxyOrgPk);
			testConnection.ExecuteNonQuery(sqlText);

			// As the previously existing Organisation is no longer referenced by a Company or Branch
			// it is a NON-PROXY Organisation which will have to be purged.
			sqlText = @"
				SELECT count(*) FROM dbo.OrgHeader 
				WHERE OH_PK not in (SELECT GC_OH_OrgProxy FROM dbo.GlbCompany WHERE GC_OH_OrgProxy is not null) 
				AND OH_PK not in (SELECT GB_OH_OrgProxy FROM dbo.GlbBranch  WHERE GB_OH_OrgProxy is not null)";
			int rowCount = Convert.ToInt32(testConnection.ExecuteScalar(sqlText));
			Assert("There should be non-proxy (Branch & Company) Organisations - BEFORE PURGE", rowCount > 0);

			// AND this NON-PROXY Organisation is still referenced by all other OrgHeader children.
			// This way the purge can be tested in terms of deleting tables following their hierarchy.
			var purger = new DataPurger();
			purger.HasNonProxyOrganisationsPurgeScript = true;
			DataPurger.RunScriptCollection(testConnection, purger.GetPurgeScripts(), 1800);

			sqlText = @"
				SELECT count(*) FROM dbo.OrgHeader 
				WHERE OH_PK not in (SELECT GC_OH_OrgProxy FROM dbo.GlbCompany WHERE GC_OH_OrgProxy is not null) 
				AND OH_PK not in (SELECT GB_OH_OrgProxy FROM dbo.GlbBranch  WHERE GB_OH_OrgProxy is not null)";
			rowCount = Convert.ToInt32(testConnection.ExecuteScalar(sqlText));
			AssertEquals("There shouldn't be any non-proxy (Branch & Company) Organisations", 0, rowCount);

			sqlText = @"SELECT count(*) FROM dbo.OrgHeader WHERE OH_PK in (SELECT GC_OH_OrgProxy FROM dbo.GlbCompany WHERE GC_OH_OrgProxy is not null)";
			rowCount = Convert.ToInt32(testConnection.ExecuteScalar(sqlText));
			Assert("There should be Company-proxy Organisations", rowCount > 0);

			sqlText = @"SELECT count(*) FROM dbo.OrgHeader WHERE OH_PK in (SELECT GB_OH_OrgProxy FROM dbo.GlbBranch WHERE GB_OH_OrgProxy is not null)";
			rowCount = Convert.ToInt32(testConnection.ExecuteScalar(sqlText));
			Assert("There should be Branch-proxy Organisations", rowCount > 0);

			// Assert data was purged on key tables (rating)
			AssertEquals("RatingHeader should have NO rows", 0, TableRowCount("RatingHeader"));
			// Assert data was purged on key tables (base purge)
			AssertEquals("AccTransactionHeader should have NO rows", 0, TableRowCount("AccTransactionHeader"));
			AssertEquals("JobVoyage should have NO rows", 0, TableRowCount("JobVoyage"));
			AssertEquals("StmNums should have NO rows", 0, TableRowCount("StmNums"));
		}

		#endregion

		#region TestCompanySpecificAccountingScript

		[SnailTest]
		public void TestCompanySpecificAccountingScript()
		{
			RunTestWithMockDatabase(AssertRunCompanySpecificAccountingScript);
		}

		[SnailTest]
		public void TestCompanySpecificAccountingScript_WithGraph()
		{
			RunTestWithMockDatabase(AssertRunCompanySpecificAccountingScript, PrepareAccountingTestData);
		}

		void PrepareAccountingTestData()
		{
			CreateTestDbSchema();
			CreateTestDbTables();
			PopulateDefaultPK();

			var sqlUtility = new SqlUtility(testConnection);
			var companyPK = sqlUtility.GenerateCompany();
			sqlUtility.GenerateBranchBelongToCompany(companyPK);

			var nodes = new List<Node>
			{
				new Node("AccGeneralLedgerData", null, "GLD_PK", null),
				new Node("AccDraftInvoiceHeader", null, "AIH_PK", null),
				new Node("AccTaxGLMovement", null, "ATM_PK", null),
				new Node("AccTaxTransaction", null, "ATT_PK", null),
				new Node("GenApprovalRequest", null, "XP_PK", null),
				new Node("AccConsolidationBatch", null, "YB_PK", null),
				new Node("AccGLAggregate", null, "AA_PK", null),
				new Node("AccEPaymentDeal", null, "AED_PK", null),
				new Node("AccEPaymentQuote", null, "QU_PK", null),
				new Node("AccPaymentApprovalItem", null, "A2_PK", null),
				new Node("AccPaymentApproval", null, "AV_PK", null),
				new Node("JobHeader", null, "JH_PK", null),
				new Node("WorkItem", null, "WKI_PK", null),
			};
			var sqlTemplates = new List<string>();
			foreach (var root in nodes)
			{
				var graph = new Graph(sqlUtility, companyPK);
				graph.AddNodeRecursively(root);
				var sqlRows = graph.GenerateSqlRowsByDFS(root);
				sqlTemplates.AddRange(sqlRows);
			}

			sqlUtility.BatchInsertRows(sqlTemplates);
		}

		void PopulateDefaultPK()
		{
			var sqlTemplate = @"DECLARE @PkTableSchema VARCHAR(128), @PkTable VARCHAR(128), @PkColumn VARCHAR(128);
DECLARE @FkTableSchema VARCHAR(128), @FkTable VARCHAR(128), @FkColumn VARCHAR(128);
DECLARE @SqlCommand NVARCHAR(MAX);
DECLARE @Level INT = 0;

DECLARE @TableTree TABLE
					(
						SchemaName VARCHAR(128),
						TableId INT,
						TableName VARCHAR(128),
						FkLevel INT
					);

INSERT @TableTree
SELECT SCHEMA_NAME(tab.schema_id),
		tab.object_id,
		tab.name,
		@Level
FROM sys.tables tab
		INNER JOIN sys.key_constraints pk ON pk.parent_object_id = tab.object_id
		LEFT JOIN sys.foreign_keys const ON const.parent_object_id = tab.object_id
WHERE tab.is_ms_shipped = 0
	AND const.parent_object_id IS NULL;

WHILE (@@ROWCOUNT > 0)
	BEGIN
		SET @Level = @Level + 1;

		INSERT @TableTree
		SELECT SCHEMA_NAME(tab.schema_id),
			tab.object_id,
			tab.name,
			@Level
		FROM sys.tables tab
		WHERE tab.is_ms_shipped = 0
			AND tab.object_id NOT IN (SELECT TableId FROM @TableTree)
			AND NOT EXISTS(
				SELECT *
				FROM sys.foreign_keys const
				WHERE const.parent_object_id = tab.object_id
					AND const.referenced_object_id NOT IN (SELECT TableId FROM @TableTree)
					AND const.referenced_object_id != tab.object_id
			);
	END

--
-- Disable all constraints
DECLARE DisableFkTabContraintCursor CURSOR LOCAL FAST_FORWARD READ_ONLY FOR
	SELECT tab.SchemaName, tab.TableName
	FROM @TableTree tab
	WHERE tab.FkLevel > 0;
OPEN DisableFkTabContraintCursor;
FETCH NEXT FROM DisableFkTabContraintCursor INTO @FkTableSchema, @FkTable;
WHILE (@@FETCH_STATUS = 0)
	BEGIN
		SET @SqlCommand = 'ALTER TABLE [' + @FkTableSchema + '].[' + @FkTable + '] NOCHECK CONSTRAINT ALL';
		EXEC (@SqlCommand);

		FETCH NEXT FROM DisableFkTabContraintCursor INTO @FkTableSchema, @FkTable;
	END
CLOSE DisableFkTabContraintCursor;
DEALLOCATE DisableFkTabContraintCursor;

--
-- Ensure all tables with an uniqueidentifier PK have a row with PK = EmptyGuid
DECLARE AllGuidPkTableCursor CURSOR LOCAL FAST_FORWARD READ_ONLY FOR
	SELECT tab.SchemaName,
		tab.TableName,
		pkcol.name PkColumn
	FROM @TableTree tab
			INNER JOIN sys.key_constraints pk ON pk.parent_object_id = tab.TableId
			INNER JOIN sys.index_columns pk_ind_col
						ON pk_ind_col.object_id = tab.TableId AND pk_ind_col.index_id = pk.unique_index_id
			INNER JOIN sys.columns pkcol ON pkcol.object_id = tab.TableId AND pkcol.column_id = pk_ind_col.column_id
	WHERE pk.type = 'PK'
		AND pkcol.user_type_id = 36
	ORDER BY tab.FkLevel;
OPEN AllGuidPkTableCursor;
FETCH NEXT FROM AllGuidPkTableCursor INTO @PkTableSchema, @PkTable, @PkColumn;
WHILE (@@FETCH_STATUS = 0)
	BEGIN
		IF (@PkTable <> 'GenSpatialData') -- Since GenSpatialData uses composite primary key, skip handling it.
			BEGIN
				-- Since GenSpatialData uses composite primary key, skip handling it.
				SET @SqlCommand = 'INSERT [' + @PkTableSchema + '].[' + @PkTable + '] (' + @PkColumn +
									') VALUES (''00000000-0000-0000-0000-000000000000'');';
				EXEC (@SqlCommand);
			END -- Since GenSpatialData uses composite primary key, skip handling it.

		FETCH NEXT FROM AllGuidPkTableCursor INTO @PkTableSchema, @PkTable, @PkColumn;
	END
CLOSE AllGuidPkTableCursor;
DEALLOCATE AllGuidPkTableCursor;

--
-- Re-enable all constraints
DECLARE EnableFkTabContraintCursor CURSOR LOCAL FAST_FORWARD READ_ONLY FOR
	SELECT tab.SchemaName, tab.TableName
	FROM @TableTree tab
	WHERE tab.FkLevel > 0;
OPEN EnableFkTabContraintCursor;
FETCH NEXT FROM EnableFkTabContraintCursor INTO @FkTableSchema, @FkTable;
WHILE (@@FETCH_STATUS = 0)
	BEGIN
		SET @SqlCommand = 'ALTER TABLE [' + @FkTableSchema + '].[' + @FkTable + '] WITH CHECK CHECK CONSTRAINT ALL';
		EXEC (@SqlCommand);

		FETCH NEXT FROM EnableFkTabContraintCursor INTO @FkTableSchema, @FkTable;
	END
CLOSE EnableFkTabContraintCursor;
DEALLOCATE EnableFkTabContraintCursor;";
			testConnection.ExecuteNonQuery(sqlTemplate);
		}

		void AssertRunCompanySpecificAccountingScript()
		{
			// Test Company
			string sqlText = "SELECT count(*) FROM dbo.GlbCompany";
			int companyRowCount = Convert.ToInt32(testConnection.ExecuteScalar(sqlText));
			AssertEquals("[PRE-CONDITION] There should be at least 1 company in the mock DB", true, companyRowCount > 0);

			sqlText = "SELECT TOP 1 GC_PK FROM dbo.GlbCompany WHERE GC_PK <> '00000000-0000-0000-0000-000000000000'";
			ZGuid testCompanyPk = new ZGuid(testConnection.ExecuteScalar(sqlText));
			AssertEquals("[PRE-CONDITION] Test company valid", true, testCompanyPk.IsValid);

			// JobHeader - Before purge
			sqlText = "SELECT count(*) FROM dbo.JobHeader WHERE JH_GC = '" + testCompanyPk.ToString() + "'";
			int rowCount = Convert.ToInt32(testConnection.ExecuteScalar(sqlText));
			Assert("[BEFORE PURGE] There should be JobHeader rows linked to the purge company", rowCount > 0);

			PrepareExtraCompanyLevelAccountingTestData(testCompanyPk.ToString());

			// AccEPaymentDeal - Before purge
			sqlText = "SELECT count(*) FROM dbo.AccEPaymentDeal WHERE AED_GC_Company = '" + testCompanyPk.ToString() + "'";
			rowCount = Convert.ToInt32(testConnection.ExecuteScalar(sqlText));
			Assert("[BEFORE PURGE] There should be AccEPaymentDeal rows linked to the purge company", rowCount > 0);

			// AccEPaymentQuote - Before purge
			sqlText = "SELECT count(*) FROM dbo.AccEPaymentQuote WHERE QU_GC = '" + testCompanyPk.ToString() + "'";
			rowCount = Convert.ToInt32(testConnection.ExecuteScalar(sqlText));
			Assert("[BEFORE PURGE] There should be AccEPaymentQuote rows linked to the purge company", rowCount > 0);

			// AccPaymentApproval - Before purge
			sqlText = "SELECT count(*) FROM dbo.AccPaymentApproval WHERE AV_GC = '" + testCompanyPk.ToString() + "'";
			rowCount = Convert.ToInt32(testConnection.ExecuteScalar(sqlText));
			Assert("[BEFORE PURGE] There should be AccPaymentApproval rows linked to the purge company", rowCount > 0);

			// GenApprovalRequest - Before purge
			sqlText = "SELECT count(*) FROM dbo.GenApprovalRequest WHERE XP_GB_RequestingBranch IN (SELECT GB_PK FROM dbo.GlbBranch WHERE GB_GC = '" + testCompanyPk.ToString() + "')";
			rowCount = Convert.ToInt32(testConnection.ExecuteScalar(sqlText));
			Assert("[BEFORE PURGE] There should be GenApprovalRequest rows linked to the purge company", rowCount > 0);
			// AccPayableOrderHeader - Before purge
			sqlText = "SELECT count(*) FROM dbo.AccPayableOrderHeader WHERE APH_GC = '" + testCompanyPk.ToString() + "'";
			rowCount = Convert.ToInt32(testConnection.ExecuteScalar(sqlText));
			Assert("[BEFORE PURGE] There should be AccPayableOrderHeader rows linked to the purge company", rowCount > 0);
			// AccPayableOrderLine - Before purge
			sqlText = "SELECT count(*) FROM dbo.AccPayableOrderLine WHERE APL_APH <> '" + ZGuid.Empty + "' AND APL_GC = '" + testCompanyPk.ToString() + "'";
			rowCount = Convert.ToInt32(testConnection.ExecuteScalar(sqlText));
			Assert("[BEFORE PURGE] There should be AccPayableOrderLine rows linked to the purge company", rowCount > 0);

			// AccTransactionHeader - Before purge
			sqlText = "SELECT count(*) FROM dbo.AccTransactionHeader WHERE AH_JH is null AND AH_GB is not null";
			rowCount = Convert.ToInt32(testConnection.ExecuteScalar(sqlText));
			AssertEquals("[BEFORE PURGE] There should at least 1 AccTransactionHeader NOT linked to the JobHeader", true, rowCount > 0);
			// AccTransactionLines - Before purge
			sqlText = "SELECT count(*) FROM dbo.AccTransactionLines WHERE AL_JH is null AND AL_AH is not null AND AL_GB is not null";
			rowCount = Convert.ToInt32(testConnection.ExecuteScalar(sqlText));
			AssertEquals("[BEFORE PURGE] There should be at least 1 AccTransactionLine linked to AccTransactionHeader and NOT linked to the JobHeader", true, rowCount > 0);
			sqlText = "SELECT count(*) FROM dbo.AccTransactionLines WHERE AL_JH is null AND AL_AH is null AND AL_GB is not null";
			rowCount = Convert.ToInt32(testConnection.ExecuteScalar(sqlText));
			AssertEquals("[BEFORE PURGE] There should be at least 1 AccTransactionLine NOT linked to AccTransactionHeader NOR JobHeader", true, rowCount > 0);

			// AccTaxTransaction - Before purge
			sqlText = "SELECT count(*) FROM dbo.AccTaxTransaction INNER JOIN dbo.AccTransactionHeader ON ATT_AH = AH_PK WHERE AH_GC = '" + testCompanyPk.ToString() + "'";
			rowCount = Convert.ToInt32(testConnection.ExecuteScalar(sqlText));
			Assert("[BEFORE PURGE] There should be AccTaxTransaction rows linked to the purge company", rowCount > 0);
			// AccTaxRecordTransactionLinePivot - Before purge
			sqlText = "SELECT count(*) FROM dbo.AccTaxRecordTransactionLinePivot INNER JOIN dbo.AccTransactionLines ON ATP_AL_TransactionLine = AL_PK WHERE AL_GC = '" + testCompanyPk.ToString() + "'";
			rowCount = Convert.ToInt32(testConnection.ExecuteScalar(sqlText));
			Assert("[BEFORE PURGE] There should be AccTaxRecordTransactionLinePivot rows linked to the purge company", rowCount > 0);

			// AccTaxGLMovement - Before purge
			sqlText = "SELECT count(*) FROM dbo.AccTaxGLMovement INNER JOIN dbo.AccTaxTransaction ON ATM_ATT_TaxTransaction = ATT_PK WHERE ATT_GC = '" + testCompanyPk.ToString() + "'";
			rowCount = Convert.ToInt32(testConnection.ExecuteScalar(sqlText));
			Assert("[BEFORE PURGE] There should be AccTaxGLMovement rows linked to the purge company", rowCount > 0);
			// AccTaxGLMovementQueue - Before purge
			sqlText = "SELECT count(*) FROM dbo.AccTaxGLMovementQueue INNER JOIN dbo.AccTaxGLMovement ON ATQ_ATM = ATM_PK INNER JOIN dbo.AccTaxTransaction ON ATM_ATT_TaxTransaction = ATT_PK WHERE ATT_GC = '" + testCompanyPk.ToString() + "'";
			rowCount = Convert.ToInt32(testConnection.ExecuteScalar(sqlText));
			Assert("[BEFORE PURGE] There should be AccTaxGLMovementQueue rows linked to the purge company", rowCount > 0);

			// AccComplianceDocumentHeader - Before purge
			sqlText = "SELECT count(*) FROM dbo.AccComplianceDocumentHeader INNER JOIN dbo.AccComplianceDocumentLine ON ADL_ADH = ADH_PK INNER JOIN dbo.AccComplianceDocumentPivot ON ADL_PK = ADP_ADL INNER JOIN dbo.AccTransactionLines ON AL_PK = ADP_AL WHERE AL_GC = '" + testCompanyPk.ToString() + "'";
			rowCount = Convert.ToInt32(testConnection.ExecuteScalar(sqlText));
			Assert("[BEFORE PURGE] There should be AccComplianceDocumentHeader rows linked to the purge company", rowCount > 0);
			// AccComplianceDocumentLine - Before purge
			sqlText = "SELECT count(*) FROM dbo.AccComplianceDocumentLine INNER JOIN dbo.AccComplianceDocumentPivot ON ADL_PK = ADP_ADL INNER JOIN dbo.AccTransactionLines ON AL_PK = ADP_AL WHERE AL_GC = '" + testCompanyPk.ToString() + "'";
			rowCount = Convert.ToInt32(testConnection.ExecuteScalar(sqlText));
			Assert("[BEFORE PURGE] There should be AccComplianceDocumentLine rows linked to the purge company", rowCount > 0);
			// AccComplianceDocumentPivot - Before purge
			sqlText = "SELECT count(*) FROM dbo.AccComplianceDocumentPivot INNER JOIN dbo.AccTransactionLines ON AL_PK = ADP_AL WHERE AL_GC = '" + testCompanyPk.ToString() + "'";
			rowCount = Convert.ToInt32(testConnection.ExecuteScalar(sqlText));
			Assert("[BEFORE PURGE] There should be AccComplianceDocumentPivot rows linked to the purge company", rowCount > 0);
			// OrgCommissionCalculationQueue - Before purge
			sqlText = "SELECT count(*) FROM dbo.OrgCommissionCalculationQueue INNER JOIN dbo.AccTransactionHeader ON CAQ_AH = AH_PK WHERE AH_GC = '" + testCompanyPk.ToString() + "'";
			rowCount = Convert.ToInt32(testConnection.ExecuteScalar(sqlText));
			Assert("[BEFORE PURGE] There should be OrgCommissionCalculationQueue rows linked to the purge company", rowCount > 0);

			//------------
			// PURGE DATA 
			//------------
			DataPurger purger = new DataPurger();
			purger.CompanySpecificPk = testCompanyPk;
			DataPurger.RunScriptCollection(testConnection, purger.GetPurgeScripts(isCompanySpecificPurge: true));

			// JobHeader - After purge
			sqlText = String.Format("SELECT count(*) FROM dbo.JobHeader WHERE JH_GC = '{0}'", testCompanyPk.ToString());
			rowCount = Convert.ToInt32(testConnection.ExecuteScalar(sqlText));
			AssertEquals("[AFTER PURGE] There should be NO JobHeader rows linked to the purge company", 0, rowCount);

			// AccEPaymentDeal - After purge
			sqlText = "SELECT count(*) FROM dbo.AccEPaymentDeal WHERE AED_GC_Company = '" + testCompanyPk.ToString() + "'";
			rowCount = Convert.ToInt32(testConnection.ExecuteScalar(sqlText));
			AssertEquals("[AFTER PURGE] There should be NO AccEPaymentDeal rows linked to the purge company", 0, rowCount);

			// AccEPaymentQuote - After purge
			sqlText = "SELECT count(*) FROM dbo.AccEPaymentQuote WHERE QU_GC = '" + testCompanyPk.ToString() + "'";
			rowCount = Convert.ToInt32(testConnection.ExecuteScalar(sqlText));
			AssertEquals("[AFTER PURGE] There should be NO AccEPaymentQuote rows linked to the purge company", 0, rowCount);

			// AccPaymentApproval - After purge
			sqlText = "SELECT count(*) FROM dbo.AccPaymentApproval WHERE AV_GC = '" + testCompanyPk.ToString() + "'";
			rowCount = Convert.ToInt32(testConnection.ExecuteScalar(sqlText));
			AssertEquals("[AFTER PURGE] There should be NO AccPaymentApproval rows linked to the purge company", 0, rowCount);

			// GenApprovalRequest - After purge
			sqlText = "SELECT count(*) FROM dbo.GenApprovalRequest WHERE XP_GB_RequestingBranch IN (SELECT GB_PK FROM dbo.GlbBranch WHERE GB_GC = '" + testCompanyPk.ToString() + "')";
			rowCount = Convert.ToInt32(testConnection.ExecuteScalar(sqlText));
			AssertEquals("[AFTER PURGE] There should be NO GenApprovalRequest rows linked to the purge company", 0, rowCount);
			// AccPayableOrderHeader - After purge
			sqlText = "SELECT count(*) FROM dbo.AccPayableOrderHeader WHERE APH_GC = '" + testCompanyPk.ToString() + "'";
			rowCount = Convert.ToInt32(testConnection.ExecuteScalar(sqlText));
			AssertEquals("[AFTER PURGE] There should be NO AccPayableOrderHeader rows linked to the purge company", 0, rowCount);
			// AccPayableOrderLine - After purge
			sqlText = "SELECT count(*) FROM dbo.AccPayableOrderLine WHERE APL_APH <> '" + ZGuid.Empty + "' AND APL_GC = '" + testCompanyPk.ToString() + "'";
			rowCount = Convert.ToInt32(testConnection.ExecuteScalar(sqlText));
			AssertEquals("[AFTER PURGE] There should be NO AccPayableOrderLine rows linked to the purge company", 0, rowCount);

			// AccTaxTransaction - After purge
			sqlText = "SELECT count(*) FROM dbo.AccTaxTransaction INNER JOIN dbo.AccTransactionHeader ON ATT_AH = AH_PK WHERE AH_GC = '" + testCompanyPk.ToString() + "'";
			rowCount = Convert.ToInt32(testConnection.ExecuteScalar(sqlText));
			AssertEquals("[AFTER PURGE] There should be NO AccTaxTransaction rows linked to the purge company", 0, rowCount);
			// AccTaxRecordTransactionLinePivot - After purge
			sqlText = "SELECT count(*) FROM dbo.AccTaxRecordTransactionLinePivot INNER JOIN dbo.AccTransactionLines ON ATP_AL_TransactionLine = AL_PK WHERE AL_GC = '" + testCompanyPk.ToString() + "'";
			rowCount = Convert.ToInt32(testConnection.ExecuteScalar(sqlText));
			AssertEquals("[AFTER PURGE] There should be NO AccTaxRecordTransactionLinePivot rows linked to the purge company", 0, rowCount);

			// AccTaxGLMovement - After purge
			sqlText = "SELECT count(*) FROM dbo.AccTaxGLMovement INNER JOIN dbo.AccTaxTransaction ON ATM_ATT_TaxTransaction = ATT_PK WHERE ATT_GC = '" + testCompanyPk.ToString() + "'";
			rowCount = Convert.ToInt32(testConnection.ExecuteScalar(sqlText));
			AssertEquals("[AFTER PURGE] There should be NO AccTaxGLMovement rows linked to the purge company", 0, rowCount);
			// AccTaxGLMovementQueue - After purge
			sqlText = "SELECT count(*) FROM dbo.AccTaxGLMovementQueue INNER JOIN dbo.AccTaxGLMovement ON ATQ_ATM = ATM_PK INNER JOIN dbo.AccTaxTransaction ON ATM_ATT_TaxTransaction = ATT_PK WHERE ATT_GC = '" + testCompanyPk.ToString() + "'";
			rowCount = Convert.ToInt32(testConnection.ExecuteScalar(sqlText));
			AssertEquals("[AFTER PURGE] There should be NO AccTaxGLMovementQueue rows linked to the purge company", 0, rowCount);

			// AccTransactionHeader - After purge
			sqlText = String.Format(@"
				SELECT count(*) FROM dbo.AccTransactionHeader
				WHERE AH_JH in (SELECT JH_PK FROM dbo.JobHeader WHERE JH_GC = '{0}')
				OR AH_GB in (SELECT GB_PK FROM dbo.GlbBranch WHERE GB_GC = '{0}')",
				testCompanyPk.ToString());
			rowCount = Convert.ToInt32(testConnection.ExecuteScalar(sqlText));
			AssertEquals("[AFTER PURGE] There should be NO AccTransactionHeader linked to the purge company", 0, rowCount);
			// AccTransactionLines - After purge
			sqlText = String.Format(@"
				SELECT count(*) FROM dbo.AccTransactionLines
				WHERE
					AL_JH in (SELECT JH_PK FROM dbo.JobHeader WHERE JH_GC = '{0}')
					OR AL_GB in (SELECT GB_PK FROM dbo.GlbBranch WHERE GB_GC = '{0}')
					OR AL_AH in (SELECT AH_PK FROM dbo.AccTransactionHeader
						WHERE AH_JH in (SELECT JH_PK FROM dbo.JobHeader WHERE JH_GC = '{0}')
						OR    AH_GB in (SELECT GB_PK FROM dbo.GlbBranch WHERE GB_GC = '{0}')
					)",
				testCompanyPk.ToString());
			rowCount = Convert.ToInt32(testConnection.ExecuteScalar(sqlText));
			AssertEquals("[AFTER PURGE] There should be NO AccTransactionLines linked to the purge company", 0, rowCount);

			// Actual company should NOT be deleted
			sqlText = "SELECT count(*) FROM dbo.GlbCompany";
			rowCount = Convert.ToInt32(testConnection.ExecuteScalar(sqlText));
			AssertEquals("[AFTER PURGE] Company count should be the same", companyRowCount, rowCount);

			// AccComplianceDocumentHeader - After purge
			sqlText = "SELECT count(*) FROM dbo.AccComplianceDocumentHeader INNER JOIN dbo.AccComplianceDocumentLine ON ADL_ADH = ADH_PK INNER JOIN dbo.AccComplianceDocumentPivot ON ADL_PK = ADP_ADL INNER JOIN dbo.AccTransactionLines ON AL_PK = ADP_AL WHERE AL_GC = '" + testCompanyPk.ToString() + "'";
			rowCount = Convert.ToInt32(testConnection.ExecuteScalar(sqlText));
			AssertEquals("[AFTER PURGE] There should be NO AccComplianceDocumentHeaders linked to the purge company", 0, rowCount);
			// AccComplianceDocumentLine - After purge
			sqlText = "SELECT count(*) FROM dbo.AccComplianceDocumentLine INNER JOIN dbo.AccComplianceDocumentPivot ON ADL_PK = ADP_ADL INNER JOIN dbo.AccTransactionLines ON AL_PK = ADP_AL WHERE AL_GC = '" + testCompanyPk.ToString() + "'";
			rowCount = Convert.ToInt32(testConnection.ExecuteScalar(sqlText));
			AssertEquals("[AFTER PURGE] There should be NO AccComplianceDocumentLines linked to the purge company", 0, rowCount);
			// AccComplianceDocumentPivot - After purge
			sqlText = "SELECT count(*) FROM dbo.AccComplianceDocumentPivot INNER JOIN dbo.AccTransactionLines ON AL_PK = ADP_AL WHERE AL_GC = '" + testCompanyPk.ToString() + "'";
			rowCount = Convert.ToInt32(testConnection.ExecuteScalar(sqlText));
			AssertEquals("[AFTER PURGE] There should be NO AccComplianceDocumentPivots linked to the purge company", 0, rowCount);
			// OrgCommissionCalculationQueue - After purge
			sqlText = "SELECT count(*) FROM dbo.OrgCommissionCalculationQueue INNER JOIN dbo.AccTransactionHeader ON CAQ_AH = AH_PK WHERE AH_GC = '" + testCompanyPk.ToString() + "'";
			rowCount = Convert.ToInt32(testConnection.ExecuteScalar(sqlText));
			AssertEquals("[AFTER PURGE] There should be NO OrgCommissionCalculationQueue linked to the purge company", 0, rowCount);
		}

		void PrepareExtraCompanyLevelAccountingTestData(string testCompanyPk)
		{
			string testTransactionHeaderPk = Guid.NewGuid().ToString();
			string testPayableOrderHeaderPk = Guid.NewGuid().ToString();

			string sqlText = String.Format("SELECT TOP 1 GB_PK FROM dbo.GlbBranch WHERE GB_GC = '{0}'", testCompanyPk);
			string testBranchPk = testConnection.ExecuteScalar(sqlText).ToString();

			sqlText = String.Format("SELECT TOP 1 GE_PK FROM dbo.GlbDepartment");
			string testDepartmentPk = testConnection.ExecuteScalar(sqlText).ToString();

			sqlText = String.Format(@"INSERT INTO dbo.GenApprovalRequest
           (XP_PK, XP_GB_RequestingBranch, XP_SubSystem, XP_ApprovalType, XP_ApprovalStatus, XP_ApprovalDate, XP_ReasonCode
           ,XP_ApprovalRequestData
           ,XP_PrivledgeRequired, XP_GS_NKApprovingUser1, XP_GS_NKApprovingUser2, XP_GS_NKApprovingUser3
           ,XP_SystemCreateUser, XP_SystemCreateTimeUtc, XP_ParentID, XP_ParentTableCode, XP_RequestID, XP_ReasonDescription
		   ,XP_GB_JobBranch, XP_GE_JobDepartment)
			VALUES
           (NEWID(), '{0}', 'ACC', 'RCD','REQ', GETDATE(), ''
           ,0x505AAD914F4FC24010C5DF4769B8172A3560492D3172314A62E2C52BB60B98F48F912DF0E9D5DF4EBC403C78209BDD9D7933FBDECCECF757AEB98E6A542BD25E4E9FDAE95D9D5ADD6AA02B0D957047445A95E015D1561B8BF6F25A2B266B823757A15CF73038CBF2D881C78374F0D7862FB04B5E36C6E851BBD3072BE4ECB5226BC1E9D9D16F5D2D3933EC505750DD12F5BC9869C43AD81AB253CE0E9E0DE898AA13EA1AE9554B3DE905CD2D8A0DBCB175B0338DD093B3FECEB5AA8B680D6C264BEBB5D7035C017FD62378A26BBC52195A139BE30DBC1568CC4E898FB132BD61A75829F10AD4696A5567206BB01CE5BF15F293D93ABCDAE67BFE0385FD518F1FB8FEFB26E45EEEB70BFD00
           ,'3','','',''
           ,'E', GETDATE(), '{1}', 'AH','',''
           ,NULL, NULL)", testBranchPk, testTransactionHeaderPk);
			testConnection.ExecuteNonQuery(sqlText);

			var testPaymentApprovalPk = Guid.NewGuid().ToString();
			var testEpaymentQuotePk = Guid.NewGuid().ToString();
			var testEpaymentDealPk = Guid.NewGuid().ToString();
			sqlText = string.Format("SELECT TOP 1 OH_PK FROM dbo.OrgHeader");
			var testOrgPk = testConnection.ExecuteScalar(sqlText).ToString();

			// Renaming Column name in test data.
			sqlText = string.Format(@"Insert into dbo.AccPaymentApproval (AV_PK, AV_RejectionReasonDetails, AV_GB, AV_GC, AV_OH)
VALUES
('{0}', 'TEST REASON 1', '{1}', '{2}', '{3}')", testPaymentApprovalPk, testBranchPk, testCompanyPk, testOrgPk);
			testConnection.ExecuteNonQuery(sqlText);

			sqlText = string.Format(@"Insert into dbo.AccEPaymentQuote
(QU_PK, QU_InternalReference, QU_AV, QU_GC, QU_Status, QU_ProviderCode, QU_FromAmount, QU_RX_NKFromCurrency, QU_ToAmount, QU_RX_NKToCurrency, QU_FeeAmount, QU_ExchangeRate, QU_SystemCreateTimeUtc, QU_SystemCreateUser)
Values
(
'{0}',
'00001001',
'{1}',
'{2}',
'REQ',
'OFX',
0,
'AUD',
200,
'USD',
0.00,
0.00,
GETDATE(),
'E'
)", testEpaymentQuotePk, testPaymentApprovalPk, testCompanyPk);
			testConnection.ExecuteNonQuery(sqlText);

			sqlText = string.Format(@"Insert into dbo.AccEPaymentDeal (AED_PK,
AED_GC_Company,
AED_QU_Quote,
AED_InternalReference,
AED_ProviderCode,
AED_ProviderReference,
AED_Status,
AED_LastResponseReceivedUtc,
AED_ErrorDescription,
AED_SystemCreateTimeUtc,
AED_SystemCreateUser,
AED_SystemLastEditTimeUtc
)
Values
(
'{0}',
'{1}',
'{2}',
'00001001',
'OFX',
'5AF3326D-2E08-4BEA-81C9-4C3D1F916114',
'PAI',
SYSUTCDATETIME(),
'Paid',
SYSUTCDATETIME(),
'E',
SYSUTCDATETIME()
)", testEpaymentDealPk, testCompanyPk, testEpaymentQuotePk);
			testConnection.ExecuteNonQuery(sqlText);

			sqlText = String.Format("INSERT dbo.AccTransactionHeader (AH_PK, AH_GC, AH_GB, AH_GE, AH_InvoiceDate) VALUES ('{0}', '{1}', '{2}', '{3}', getdate())", testTransactionHeaderPk, testCompanyPk, testBranchPk, testDepartmentPk);
			testConnection.ExecuteNonQuery(sqlText);
			sqlText = String.Format("INSERT dbo.AccTransactionHeaderSubAccount (AHS_PK, AHS_AH, AHS_SubClassParentTableCode, AHS_SubClassParentId) VALUES (NEWID(), '{0}', 'OH', NEWID())", testTransactionHeaderPk);
			testConnection.ExecuteNonQuery(sqlText);
			var testTransactionLinePk = Guid.NewGuid().ToString();
			sqlText = String.Format("INSERT dbo.AccTransactionLines (AL_PK, AL_AH, AL_GC, AL_GB, AL_GE) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}')", testTransactionLinePk, testTransactionHeaderPk, testCompanyPk, testBranchPk, testDepartmentPk);
			testConnection.ExecuteNonQuery(sqlText);
			var testComplianceDocumentHeaderPK = Guid.NewGuid().ToString();
			sqlText = String.Format("INSERT dbo.AccComplianceDocumentHeader (ADH_PK, ADH_Ledger, ADH_ComplianceSubType, ADH_DocumentDate, ADH_DocumentStatus, ADH_DocumentType, ADH_ReportingPeriod, ADH_GC_Company, ADH_SystemCreateTimeUtc, ADH_SystemLastEditTimeUtc) VALUES ('{0}', 'AR', 'TXC', GETDATE(), 'ADD', 'VAT', 201901, '{1}', GETDATE(), GETDATE())", testComplianceDocumentHeaderPK, testCompanyPk);
			testConnection.ExecuteNonQuery(sqlText);
			var testComplianceDocumentLinePK = Guid.NewGuid().ToString();
			sqlText = String.Format("INSERT dbo.AccComplianceDocumentLine (ADL_PK, ADL_ADH, ADL_Description, ADL_Sequence) VALUES ('{0}', '{1}', 'DESC', 1)", testComplianceDocumentLinePK, testComplianceDocumentHeaderPK);
			testConnection.ExecuteNonQuery(sqlText);
			sqlText = String.Format("INSERT dbo.AccComplianceDocumentPivot (ADP_PK, ADP_AL, ADP_ADL) VALUES (NEWID(), '{0}', '{1}')", testTransactionLinePk, testComplianceDocumentLinePK);
			testConnection.ExecuteNonQuery(sqlText);
			sqlText = String.Format("INSERT dbo.AccTransactionLineSubAccount (AL1_PK, AL1_AL, AL1_SubClassParentTableCode, AL1_SubClassParentId) VALUES (NEWID(), '{0}', 'OH', NEWID())", testTransactionLinePk);
			testConnection.ExecuteNonQuery(sqlText);
			sqlText = String.Format("INSERT dbo.AccTransactionMatchLink (AP_PK, AP_AH) VALUES (NEWID(), '{0}')", testTransactionHeaderPk);
			testConnection.ExecuteNonQuery(sqlText);
			sqlText = String.Format("INSERT dbo.AccHotCheque (AQ_PK, AQ_AH) VALUES (NEWID(), '{0}')", testTransactionHeaderPk);
			testConnection.ExecuteNonQuery(sqlText);
			sqlText = String.Format("INSERT INTO dbo.OrgCommissionCalculationQueue(CAQ_PK, CAQ_AH,CAQ_OverwriteExistingCommissions,CAQ_Operation,CAQ_SystemCreateTimeUtc,CAQ_SystemCreateUser,CAQ_SystemLastEditUser) VALUES(NEWID(),'{0}',1,'RGN',GETDATE(),'UDF','UDF')", testTransactionHeaderPk);
			testConnection.ExecuteNonQuery(sqlText);
			sqlText = String.Format("INSERT dbo.AccPayableOrderHeader (APH_PK, APH_AH, APH_GC) VALUES ('{0}', '{1}', '{2}')", testPayableOrderHeaderPk, testTransactionHeaderPk, testCompanyPk);
			testConnection.ExecuteNonQuery(sqlText);
			sqlText = String.Format("INSERT dbo.AccPayableOrderLine (APL_PK, APL_APH, APL_GB, APL_GC) VALUES (NEWID(), '{0}', '{1}', '{2}')", testPayableOrderHeaderPk, testBranchPk, testCompanyPk);
			testConnection.ExecuteNonQuery(sqlText);
			sqlText = String.Format("INSERT dbo.AccTransactionLines (AL_PK, AL_GB, AL_GE) VALUES (NEWID(), '{0}', '{1}')", testBranchPk, testDepartmentPk);
			testConnection.ExecuteNonQuery(sqlText);
			var taxTransactionPk = Guid.NewGuid().ToString();
			sqlText = String.Format("INSERT dbo.AccTaxTransaction (ATT_PK, ATT_GC, ATT_AH) VALUES ('{0}', '{1}', '{2}')", taxTransactionPk, testCompanyPk, testTransactionHeaderPk);
			testConnection.ExecuteNonQuery(sqlText);
			sqlText = String.Format("INSERT dbo.AccTaxRecordTransactionLinePivot (ATP_PK, ATP_ATT, ATP_AL_TransactionLine) VALUES (NEWID(), '{0}', '{1}')", taxTransactionPk, testTransactionLinePk);
			testConnection.ExecuteNonQuery(sqlText);
			sqlText = String.Format("INSERT dbo.AccTaxTransaction (ATT_PK, ATT_GC, ATT_AH_MatchTransaction) VALUES (NEWID(), '{0}', '{1}')", testCompanyPk, testTransactionHeaderPk);
			testConnection.ExecuteNonQuery(sqlText);
			var taxGLMovementPk = Guid.NewGuid().ToString();
			sqlText = String.Format("INSERT dbo.AccTaxGLMovement (ATM_PK, ATM_ATT_TaxTransaction, ATM_Type, ATM_Amount, ATM_Date, ATM_Period) VALUES ('{0}', '{1}', 'NRM', 100, GETDATE(), '202011')", taxGLMovementPk, taxTransactionPk);
			testConnection.ExecuteNonQuery(sqlText);
			sqlText = String.Format("INSERT dbo.AccTaxGLMovementQueue (ATQ_ATM) VALUES ('{0}')", taxGLMovementPk);
			testConnection.ExecuteNonQuery(sqlText);
		}

		#endregion

		#region Run Test Delegate and Help Methods

		void RunTestWithMockDatabase(AnonymousMethod purgeAndAssert, AnonymousMethod prepareTestDb = null)
		{
			if (prepareTestDb == null)
			{
				prepareTestDb = PrepareTestDb;
			}

			try
			{
				CreateTestDb();

				using (testConnection = Db.NewExtraConnectionWithMainDbCredentials(Db.ServerName, TestDbName))
				{
					prepareTestDb();
					purgeAndAssert();
				}
			}
			catch
			{
				throw;
			}
			finally
			{
				CleanUpTestDatabase();
			}
		}

		void AssertTableRowCount(string tableName, int expectedRowCount)
		{
			AssertEquals(
				String.Format("{0} should have {1} row(s)", tableName, expectedRowCount.ToString()),
				expectedRowCount,
				TableRowCount(tableName));
		}

		void AssertTableHasRows(string tableName)
		{
			AssertEquals(
				String.Format("{0} has row(s)", tableName),
				true,
				TableRowCount(tableName) > 0);
		}

		int TableRowCount(string tableName)
		{
			string sqlText = "SELECT count(*) FROM " + tableName;
			int rowCount = Convert.ToInt32(testConnection.ExecuteScalar(sqlText));
			return rowCount;
		}

		#endregion

		#region Set up and Clean up Test Database

		void CleanUpTestDatabase()
		{
			try
			{
				testConnection = null;
				DropTestDbIfExists();
			}
			catch
			{
				// Prevents original ERROR from being hiden if cleanup fails
			}
		}

		#region Methods using Admin DB Connection

		void DropTestDbIfExists()
		{
			using (DbConnection conn = Db.NewAdminConnection())
			{
				var isDbExisted = conn.Exists("FROM sys.databases WHERE name = @dbName",
					x => x.AddParameter("dbName", SqlDbType.NVarChar, TestDbName)
				);

				if (isDbExisted)
				{
					conn.ExecuteNonQuery($"EXEC msdb.dbo.sp_delete_database_backuphistory @database_name = N'{TestDbName}'");
					conn.ExecuteNonQuery($"ALTER DATABASE {TestDbName} SET SINGLE_USER WITH ROLLBACK IMMEDIATE");
					conn.ExecuteNonQuery($"DROP DATABASE {TestDbName}");
				}
			}
		}

		void CreateTestDb()
		{
			DropTestDbIfExists();

			using (AdminConnection conn = Db.NewAdminConnection())
			{
				conn.CreateDatabase(TestDbName);
			}
		}

		#endregion

		#region Create Test DB and Objects

		void PrepareTestDb()
		{
			CreateTestDbSchema();
			CreateTestDbTables();
			PopulateFkFields();
		}
		void CreateTestDbSchema()
		{
			foreach (var schema in Db.CW1AdditionalSchemas)
			{
				testConnection.ExecuteNonQuery($"CREATE SCHEMA {schema}");
			}
		}

		void CreateTestDbTables()
		{
			var testingManager = new ScriptManager();
			string schemaScript = testingManager.MaindDbSchemaScript;

			schemaScript = SQLSanitiser.Sanitise(schemaScript);

			testConnection.ExecuteNonQuery(schemaScript);
		}
		#endregion

		#region Populate All Tables and FKs

		/// <summary>
		/// Insert rows for each FK, for each parent PK
		/// </summary>
		void PopulateFkFields()
		{
			string sqlText = @"
BEGIN TRY
	DECLARE @PkTableSchema varchar(128), @PkTable varchar(128), @PkColumn varchar(128);
	DECLARE @FkTableSchema varchar(128), @FkTable varchar(128), @FkColumn varchar(128);
	DECLARE @SqlCommand nvarchar(max);
	DECLARE @Level int = 0;

	DECLARE @TableTree TABLE
	(
		SchemaName varchar(128),
		TableId int,
		TableName varchar(128),
		FkLevel int
	);

	INSERT @TableTree
		SELECT
			SCHEMA_NAME(tab.schema_id), tab.object_id, tab.name, @Level
		FROM
			sys.tables tab
			INNER JOIN sys.key_constraints pk ON pk.parent_object_id = tab.object_id
			LEFT JOIN sys.foreign_keys const ON const.parent_object_id = tab.object_id
		WHERE
			tab.is_ms_shipped = 0
			AND const.parent_object_id is null;

	WHILE (@@ROWCOUNT > 0)
	BEGIN
		SET @Level = @Level + 1;

		INSERT @TableTree
			SELECT
				SCHEMA_NAME(tab.schema_id), tab.object_id, tab.name, @Level
			FROM
				sys.tables tab
			WHERE
				tab.is_ms_shipped = 0
				AND tab.object_id NOT IN (SELECT TableId FROM @TableTree)
				AND NOT EXISTS (
					SELECT * FROM sys.foreign_keys const
					WHERE const.parent_object_id = tab.object_id
					AND const.referenced_object_id NOT IN (SELECT TableId FROM @TableTree)
					AND const.referenced_object_id != tab.object_id
				);
	END

	--
	-- Insert parent rows
	DECLARE TopLevelTableCursor CURSOR LOCAL FAST_FORWARD READ_ONLY FOR
		SELECT
			tab.SchemaName,
			tab.TableName,
			pkcol.name PkColumn
		FROM
			@TableTree tab
			INNER JOIN sys.key_constraints pk ON pk.parent_object_id = tab.TableId
			INNER JOIN sys.index_columns pk_ind_col ON pk_ind_col.object_id = tab.TableId AND pk_ind_col.index_id = pk.unique_index_id
			INNER JOIN sys.columns pkcol ON pkcol.object_id = tab.TableId AND pkcol.column_id = pk_ind_col.column_id
		WHERE
			pk.type = 'PK'
			AND tab.FkLevel = 0;
	OPEN TopLevelTableCursor;
	FETCH NEXT FROM TopLevelTableCursor INTO @PkTableSchema, @PkTable, @PkColumn;
	WHILE (@@FETCH_STATUS = 0)
	BEGIN
		IF (@PkTable <> 'GenSpatialData') -- Since GenSpatialData uses composite primary key, skip handling it.
		BEGIN                             -- Since GenSpatialData uses composite primary key, skip handling it.
		SET @SqlCommand = 'INSERT [' + @PkTableSchema + '].[' + @PkTable + '] DEFAULT VALUES';
		EXEC (@SqlCommand);
		END                               -- Since GenSpatialData uses composite primary key, skip handling it.

		FETCH NEXT FROM TopLevelTableCursor INTO @PkTableSchema, @PkTable, @PkColumn;
	END
	CLOSE TopLevelTableCursor;
	DEALLOCATE TopLevelTableCursor;

	--
	-- Disable all constraints
	DECLARE DisableFkTabContraintCursor CURSOR LOCAL FAST_FORWARD READ_ONLY FOR
		SELECT tab.SchemaName, tab.TableName
		FROM @TableTree tab
		WHERE tab.FkLevel > 0;
	OPEN DisableFkTabContraintCursor;
	FETCH NEXT FROM DisableFkTabContraintCursor INTO @FkTableSchema, @FkTable;
	WHILE (@@FETCH_STATUS = 0)
	BEGIN
		SET @SqlCommand = 'ALTER TABLE [' + @FkTableSchema + '].[' + @FkTable + '] NOCHECK CONSTRAINT ALL';
		EXEC (@SqlCommand);

		FETCH NEXT FROM DisableFkTabContraintCursor INTO @FkTableSchema, @FkTable;
	END
	CLOSE DisableFkTabContraintCursor;
	DEALLOCATE DisableFkTabContraintCursor;

	--
	-- Populate FK fields
	DECLARE FkCursor CURSOR LOCAL FAST_FORWARD READ_ONLY FOR
		SELECT
			FkTab.SchemaName FkSchema,
			FkTab.TableName FkTable,
			FkCol.name FkColumn,
			SCHEMA_NAME(PkTab.schema_id) ParentTableSchema,
			PkTab.name ParentTable,
			PkCol.name ParentTablePk
		FROM
			@TableTree FkTab
			INNER JOIN sys.foreign_keys FkConst ON FkConst.parent_object_id = FkTab.TableId
			INNER JOIN sys.tables PkTab ON PkTab.object_id = FkConst.referenced_object_id
			INNER JOIN sys.foreign_key_columns FkConstCol ON FkConstCol.constraint_object_id = FkConst.object_id
			INNER JOIN sys.columns FkCol ON FkCol.object_id = FkTab.TableId AND FkCol.column_id = FkConstCol.parent_column_id
			INNER JOIN sys.columns PkCol ON PkCol.object_id = PkTab.object_id AND PkCol.column_id = FkConstCol.referenced_column_id
		WHERE FkTab.FkLevel > 0
		ORDER BY FkLevel;
	OPEN FkCursor;
	FETCH NEXT FROM FkCursor INTO @FkTableSchema, @FkTable, @FkColumn, @PkTableSchema, @PkTable, @PkColumn;
	WHILE (@@FETCH_STATUS = 0)
	BEGIN
		IF (@FkTable = @PkTable)
		BEGIN
			IF (@PkTable <> 'GenSpatialData')   -- Since GenSpatialData uses composite primary key, skip handling it.
			BEGIN                               -- Since GenSpatialData uses composite primary key, skip handling it.
			SET @SqlCommand = 'IF not exists (SELECT null FROM [' + @PkTableSchema + '].[' + @PkTable + ']) INSERT [' + @PkTableSchema + '].[' + @PkTable + '] DEFAULT VALUES';
			EXEC (@SqlCommand);
			END                                 -- Since GenSpatialData uses composite primary key, skip handling it.
		END;

		IF (@FkTable <> 'GenSpatialData')       -- Since GenSpatialData uses composite primary key, skip handling it.
		BEGIN                                   -- Since GenSpatialData uses composite primary key, skip handling it.
		SET @SqlCommand = 'INSERT [' + @FkTableSchema + '].[' + @FkTable + '] (' + @FkColumn + ') SELECT TOP 1 ' + @PkColumn + ' FROM [' + @PkTableSchema + '].[' + @PkTable + ']';
		EXEC (@SqlCommand);
		END                                     -- Since GenSpatialData uses composite primary key, skip handling it.

		FETCH NEXT FROM FkCursor INTO @FkTableSchema, @FkTable, @FkColumn, @PkTableSchema, @PkTable, @PkColumn;
	END
	CLOSE FkCursor;
	DEALLOCATE FkCursor;

	--
	-- Ensure all tables with an uniqueidentifier PK have a row with PK = EmptyGuid
	DECLARE AllGuidPkTableCursor CURSOR LOCAL FAST_FORWARD READ_ONLY FOR
		SELECT
			tab.SchemaName,
			tab.TableName,
			pkcol.name PkColumn
		FROM
			@TableTree tab
			INNER JOIN sys.key_constraints pk ON pk.parent_object_id = tab.TableId
			INNER JOIN sys.index_columns pk_ind_col ON pk_ind_col.object_id = tab.TableId AND pk_ind_col.index_id = pk.unique_index_id
			INNER JOIN sys.columns pkcol ON pkcol.object_id = tab.TableId AND pkcol.column_id = pk_ind_col.column_id
		WHERE
			pk.type = 'PK'
			AND pkcol.user_type_id = 36
		ORDER BY
			tab.FkLevel;
	OPEN AllGuidPkTableCursor;
	FETCH NEXT FROM AllGuidPkTableCursor INTO @PkTableSchema, @PkTable, @PkColumn;
	WHILE (@@FETCH_STATUS = 0)
	BEGIN
		IF (@PkTable <> 'GenSpatialData')            -- Since GenSpatialData uses composite primary key, skip handling it.
		BEGIN                                        -- Since GenSpatialData uses composite primary key, skip handling it.
		SET @SqlCommand = 'INSERT [' + @PkTableSchema + '].[' + @PkTable + '] (' + @PkColumn + ') VALUES (''00000000-0000-0000-0000-000000000000'');';
		EXEC (@SqlCommand);
		END                                          -- Since GenSpatialData uses composite primary key, skip handling it.

		FETCH NEXT FROM AllGuidPkTableCursor INTO @PkTableSchema, @PkTable, @PkColumn;
	END
	CLOSE AllGuidPkTableCursor;
	DEALLOCATE AllGuidPkTableCursor;

	--
	-- Update default value for constrained columns
	DECLARE @CkTableSchema varchar(128), @CkTable varchar(128), @CkColumn varchar(128);

	--
	-- Ensure all char columns with a len() constrain does not contain all whitespace value
	-- To avoid constrain errors caused by inserting default whitespaces into len() constrained columns
	DECLARE AllLenConstrainColumnCursor CURSOR LOCAL FAST_FORWARD READ_ONLY FOR
		SELECT
			tab.SchemaName,
			tab.TableName,
			col.name ColumnName
		FROM 
			@TableTree tab
			INNER JOIN sys.check_constraints con ON con.parent_object_id = tab.TableId
			INNER JOIN sys.all_columns col on con.parent_column_id = col.column_id and con.parent_object_id = col.object_id
		WHERE
			type_name(user_type_id) in ('char') AND con.definition like '(len(%'
		ORDER BY
			con.name
	OPEN AllLenConstrainColumnCursor;
	FETCH NEXT FROM AllLenConstrainColumnCursor INTO @CkTableSchema, @CkTable, @CkColumn;
	WHILE (@@FETCH_STATUS = 0)
	BEGIN
		SET @SqlCommand = 'UPDATE [' + @CkTableSchema + '].[' + @CkTable + '] SET [' + @CkColumn + '] = REPLACE([' + @CkColumn + '], '' '', ''*'')';
		EXEC (@SqlCommand);

		FETCH NEXT FROM AllLenConstrainColumnCursor INTO @CkTableSchema, @CkTable, @CkColumn;
	END
	CLOSE AllLenConstrainColumnCursor;
	DEALLOCATE AllLenConstrainColumnCursor;

	--
	-- Ensure all nvarchar/varchar columns with a none empty constrain does not contain a empty value
	-- To avoid constrain errors caused by inserting default empty value into none empty constrained columns
	DECLARE AllNoneEmptyConstrainColumnCursor CURSOR LOCAL FAST_FORWARD READ_ONLY FOR
		SELECT
			tab.SchemaName,
			tab.TableName,
			col.name ColumnName
		FROM 
			@TableTree tab
			INNER JOIN sys.check_constraints con ON con.parent_object_id = tab.TableId
			INNER JOIN sys.all_columns col on con.parent_column_id = col.column_id and con.parent_object_id = col.object_id
		WHERE
			type_name(user_type_id) in ('nvarchar', 'varchar') AND (con.definition like '(len(%>(0))' OR con.definition like '(datalength(%>(0))' OR con.definition like '%<>N'''')' OR con.definition like '%<>'''')')
		ORDER BY
			con.name
	OPEN AllNoneEmptyConstrainColumnCursor;
	FETCH NEXT FROM AllNoneEmptyConstrainColumnCursor INTO @CkTableSchema, @CkTable, @CkColumn;
	WHILE (@@FETCH_STATUS = 0)
	BEGIN
		SET @SqlCommand = 'UPDATE [' + @CkTableSchema + '].[' + @CkTable + '] SET [' + @CkColumn + '] = ''*'' WHERE LEN(' + @CkColumn + ') = 0';
		EXEC (@SqlCommand);

		FETCH NEXT FROM AllNoneEmptyConstrainColumnCursor INTO @CkTableSchema, @CkTable, @CkColumn;
	END
	CLOSE AllNoneEmptyConstrainColumnCursor;
	DEALLOCATE AllNoneEmptyConstrainColumnCursor;

	--
	-- Re-enable all constraints
	DECLARE EnableFkTabContraintCursor CURSOR LOCAL FAST_FORWARD READ_ONLY FOR
		SELECT tab.SchemaName, tab.TableName
		FROM @TableTree tab
		WHERE tab.FkLevel > 0;
	OPEN EnableFkTabContraintCursor;
	FETCH NEXT FROM EnableFkTabContraintCursor INTO @FkTableSchema, @FkTable;
	WHILE (@@FETCH_STATUS = 0)
	BEGIN
		SET @SqlCommand = 'ALTER TABLE [' + @FkTableSchema + '].[' + @FkTable + '] WITH CHECK CHECK CONSTRAINT ALL';
		EXEC (@SqlCommand);

		FETCH NEXT FROM EnableFkTabContraintCursor INTO @FkTableSchema, @FkTable;
	END
	CLOSE EnableFkTabContraintCursor;
	DEALLOCATE EnableFkTabContraintCursor;

END TRY
BEGIN CATCH
	SET @sqlCommand = 'Command failed: ' + @sqlCommand + char(13) + 
		'ERROR_MESSAGE() = ' + ERROR_MESSAGE()
	RAISERROR(@sqlCommand, 16, 1)
END CATCH";

			testConnection.ExecuteNonQuery(sqlText);
		}

		#endregion

		#endregion

		const string TestDbName = "DataPurgerTestMockDb86F97C2B5CDD4C5AB88EA0954FC06BA4";
		const string TestSystemChargeCode = "~SYSTEM";

		DbConnection testConnection;
	}
}
