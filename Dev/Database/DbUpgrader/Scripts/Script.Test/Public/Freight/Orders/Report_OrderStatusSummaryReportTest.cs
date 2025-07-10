using System;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Orders;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Orders
{
	[TestedType(typeof(Report_OrderStatusSummaryReport))]
	class Report_OrderStatusSummaryReportTest : DbCreateScriptTest
	{
		const string DateFormat = "d/MM/yyyy";

		protected override void SetUp()
		{
			base.SetUp();

			jobOrderHeaderPK = Guid.NewGuid();
			orgHeaderPk = Guid.NewGuid();
			orgAddressPK = Guid.NewGuid();
			glbCompanyPK = Guid.NewGuid();
			jobShipmentPK = Guid.NewGuid();

			TestConnection.ExecuteNonQuery(GetInsertOrgHeaderCommand());
			TestConnection.ExecuteNonQuery(GetInsertOrgAddressCommand());
			TestConnection.ExecuteNonQuery(GetInsertGlbStaffCommand());
			TestConnection.ExecuteNonQuery(GetInsertGlbCompanyCommand());
			TestConnection.ExecuteNonQuery(GetInsertJobShipmentCommand());
			TestConnection.ExecuteNonQuery(GetInsertJobOrderHeaderCommand());
		}

		public void TestOrgCusCode()
		{
			TestDataCreator.CreateOrgCusCode(orgHeaderPk, "GST", "GST_CODE", "US");
			var updateSetupData = @$"
UPDATE dbo.OrgHeader
SET
	OH_RL_NKClosestPort = 'USCHI'
WHERE
	OH_PK ='{orgHeaderPk}';
UPDATE dbo.JobOrderHeader
SET
	JD_OA_SupplierAddress = '{orgAddressPK}',
	JD_SystemLastEditTimeUtc = GETUTCDATE(),
	JD_SystemLastEditUser = '~BP'
WHERE
	JD_PK ='{jobOrderHeaderPK}'";
			TestConnection.ExecuteNonQuery(updateSetupData);

			TestConnection.ExecuteReader(
				GetReportFromCompanyAndBuyer(glbCompanyPK, orgHeaderPk, "Supplier/Controlling Customer"),
				(reader) =>
				{
					AssertEquals("GST_CODE", (string)reader["SupplierBusinessNumber"]);
				}
			);
		}

		public void TestOnlyTakeActiveStmALogRecords()
		{
			TestConnection.ExecuteNonQuery(GetInsertStmALogsCommand("2014-01-01", "CCC", false, jobShipmentPK, "JobShipment"));
			TestConnection.ExecuteNonQuery(GetInsertStmALogsCommand("2015-01-01", "CCC", true, jobShipmentPK, "JobShipment"));

			TestConnection.ExecuteNonQuery(GetInsertStmALogsCommand("2014-02-01", "CLR", false, jobShipmentPK, "JobShipment"));
			TestConnection.ExecuteNonQuery(GetInsertStmALogsCommand("2015-02-01", "CLR", true, jobShipmentPK, "JobShipment"));

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, GetReportFromCompanyAndBuyer(glbCompanyPK, orgHeaderPk));

			AssertEquals("Result should have one row", 1, result.Rows.Count);

			var dateCommencedActual = result.Rows[0].Field<DateTime>("DateCustomsCommencedActual").ToString(DateFormat); // Get Customs commenced actual date
			var dateClearedActual = result.Rows[0].Field<DateTime>("DateCustomsClearedActual").ToString(DateFormat); // Get Customs cleared actual date

			AssertEquals("DateCustomsCommencedActual should be 1/01/2014 as the another StmAlog is cancelled", "1/01/2014", dateCommencedActual);
			AssertEquals("DateCustomsClearedActual should be 1/02/2014 as the another StmAlog is cancelled", "1/02/2014", dateClearedActual);
		}

		public void TestGetCCCAndCLRActualDateWhenTheyInParentAreBlank()
		{
			TestConnection.ExecuteNonQuery(GetInsertProcessTaskCommand(jobOrderHeaderPK, "JD", "CCC", "2014-02-11"));
			TestConnection.ExecuteNonQuery(GetInsertProcessTaskCommand(jobOrderHeaderPK, "JD", "CLR", "2014-02-21"));

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, GetReportFromCompanyAndBuyer(glbCompanyPK, orgHeaderPk));

			AssertEquals("Result should have one row", 1, result.Rows.Count);

			var dateCommencedActual = result.Rows[0].Field<DateTime>("DateCustomsCommencedActual").ToString(DateFormat); // Get Customs commenced actual date
			var dateClearedActual = result.Rows[0].Field<DateTime>("DateCustomsClearedActual").ToString(DateFormat); // Get Customs cleared actual date

			AssertEquals("DateCustomsCommencedActual should get from dbo.ProcessTasks", "11/02/2014", dateCommencedActual);
			AssertEquals("DateCustomsClearedActual should get from dbo.ProcessTasks", "21/02/2014", dateClearedActual);
		}

		#region OrderUpdateAndHistory

		public void TestOrderUpdateAndHistory_SingleOrder()
		{
			var format = @"INSERT INTO dbo.JobOrderHeader
           ([JD_PK]
           ,[JD_OA_BuyerAddress]
           ,[JD_OrderNumber]
           ,[JD_OrderStatus]
           ,[JD_SystemCreateTimeUTC]
           ,[JD_SystemCreateUser]
           ,[JD_IsValid]
           ,[JD_IsCancelled])
           VALUES
           ('{0}', '{1}', '{2}', 'INC', '2017-01-01', 'TGS', '1','0')";

			var orderWithNote1Pk = Guid.NewGuid();
			var orderWithNote2Pk = Guid.NewGuid();
			var orderWithoutNotePk = Guid.NewGuid();

			TestConnection.ExecuteNonQuery(string.Format(format, orderWithNote1Pk, orgAddressPK, "TestOrder000"));
			TestConnection.ExecuteNonQuery(string.Format(format, orderWithNote2Pk, orgAddressPK, "TestOrder001"));
			TestConnection.ExecuteNonQuery(string.Format(format, orderWithoutNotePk, orgAddressPK, "TestOrder002"));

			AssertOrderUpdateAndHistory(orderWithNote1Pk, orderWithNote2Pk, orderWithoutNotePk);
		}

		public void TestOrderUpdateAndHistory_HasShipment()
		{
			var format = @"INSERT INTO dbo.JobOrderHeader
           ([JD_PK]
           ,[JD_JS]
           ,[JD_OA_BuyerAddress]
           ,[JD_OrderNumber]
           ,[JD_OrderStatus]
           ,[JD_SystemCreateTimeUTC]
           ,[JD_SystemCreateUser]
           ,[JD_IsValid]
           ,[JD_IsCancelled])
           VALUES
           ('{0}', '{1}', '{2}', '{3}', 'INC', '2017-01-01', 'TGS', '1','0')";

			var orderWithNote1Pk = Guid.NewGuid();
			var orderWithNote2Pk = Guid.NewGuid();
			var orderWithoutNotePk = Guid.NewGuid();

			TestConnection.ExecuteNonQuery(string.Format(format, orderWithNote1Pk, jobShipmentPK, orgAddressPK, "TestOrder003"));
			TestConnection.ExecuteNonQuery(string.Format(format, orderWithNote2Pk, jobShipmentPK, orgAddressPK, "TestOrder004"));
			TestConnection.ExecuteNonQuery(string.Format(format, orderWithoutNotePk, jobShipmentPK, orgAddressPK, "TestOrder005"));

			AssertOrderUpdateAndHistory(orderWithNote1Pk, orderWithNote2Pk, orderWithoutNotePk);
		}

		public void TestOrderUpdateAndHistory_HasDeclaration()
		{
			var declarationPk = Guid.NewGuid();
			TestConnection.ExecuteNonQuery(string.Format(GetInsertDeclarationCommand(declarationPk, 1, null)));

			var format = @"INSERT INTO dbo.JobOrderHeader
		   ([JD_PK]
		   ,[JD_JE]
		   ,[JD_OA_BuyerAddress]
		   ,[JD_OrderNumber]
		   ,[JD_OrderStatus]
		   ,[JD_SystemCreateTimeUTC]
		   ,[JD_SystemCreateUser]
		   ,[JD_IsValid]
		   ,[JD_IsCancelled])
		   VALUES
		   ('{0}', '{1}', '{2}', '{3}', 'INC', '2017-01-01', 'TGS', '1','0')";

			var orderWithNote1Pk = Guid.NewGuid();
			var orderWithNote2Pk = Guid.NewGuid();
			var orderWithoutNotePk = Guid.NewGuid();

			TestConnection.ExecuteNonQuery(string.Format(format, orderWithNote1Pk, declarationPk, orgAddressPK, "TestOrder006"));
			TestConnection.ExecuteNonQuery(string.Format(format, orderWithNote2Pk, declarationPk, orgAddressPK, "TestOrder007"));
			TestConnection.ExecuteNonQuery(string.Format(format, orderWithoutNotePk, declarationPk, orgAddressPK, "TestOrder008"));

			AssertOrderUpdateAndHistory(orderWithNote1Pk, orderWithNote2Pk, orderWithoutNotePk);
		}

		public void TestOrderUpdateAndHistory_HasPrePlanning()
		{
			var prePlanningPk = Guid.NewGuid();
			TestConnection.ExecuteNonQuery(string.Format(GetInsertPreplanningCommand(prePlanningPk, orgAddressPK)));

			var format = @"INSERT INTO dbo.JobOrderHeader
		   ([JD_PK]
		   ,[JD_EF_ShipmentPrePlanning]
		   ,[JD_OA_BuyerAddress]
		   ,[JD_OrderNumber]
		   ,[JD_OrderStatus]
		   ,[JD_SystemCreateTimeUTC]
		   ,[JD_SystemCreateUser]
		   ,[JD_IsValid]
		   ,[JD_IsCancelled])
		   VALUES
		   ('{0}', '{1}', '{2}', '{3}', 'INC', '2017-01-01', 'TGS', '1','0')";

			var orderWithNote1Pk = Guid.NewGuid();
			var orderWithNote2Pk = Guid.NewGuid();
			var orderWithoutNotePk = Guid.NewGuid();

			TestConnection.ExecuteNonQuery(string.Format(format, orderWithNote1Pk, prePlanningPk, orgAddressPK, "TestOrder009"));
			TestConnection.ExecuteNonQuery(string.Format(format, orderWithNote2Pk, prePlanningPk, orgAddressPK, "TestOrder010"));
			TestConnection.ExecuteNonQuery(string.Format(format, orderWithoutNotePk, prePlanningPk, orgAddressPK, "TestOrder011"));

			AssertOrderUpdateAndHistory(orderWithNote1Pk, orderWithNote2Pk, orderWithoutNotePk);
		}

		void AssertOrderUpdateAndHistory(Guid orderWithNote1Pk, Guid orderWithNote2Pk, Guid orderWithoutNotePk)
		{
			var note1Pk = Guid.NewGuid();
			var note2Pk = Guid.NewGuid();
			var note3Pk = Guid.NewGuid();
			var note4Pk = Guid.NewGuid();
			var note5Pk = Guid.NewGuid();
			var note6Pk = Guid.NewGuid();

			TestConnection.ExecuteNonQuery(GetInsertNoteCommand(note1Pk, orderWithNote1Pk, "JobOrderHeader", "Order Update History", "From AAA To BBB", "2024-01-02", "2024-01-02"));
			TestConnection.ExecuteNonQuery(GetInsertNoteCommand(note2Pk, orderWithNote1Pk, "JobOrderHeader", "Other Note", "TEST", "2024-01-02", "2024-01-02"));
			TestConnection.ExecuteNonQuery(GetInsertNoteCommand(note3Pk, orderWithNote1Pk, "JobOrderHeader", "Order Management Update", "note1", "2024-01-01", "2024-01-01", "AAC", "AAC"));

			TestConnection.ExecuteNonQuery(GetInsertNoteCommand(note4Pk, orderWithNote2Pk, "JobOrderHeader", "Order Update History", "From CCC To DDD", "2024-01-03", "2024-01-03"));
			TestConnection.ExecuteNonQuery(GetInsertNoteCommand(note5Pk, orderWithNote2Pk, "JobOrderHeader", "Other Note", "TEST", "2017-01-04", "2017-01-04"));
			TestConnection.ExecuteNonQuery(GetInsertNoteCommand(note6Pk, orderWithNote2Pk, "JobOrderHeader", "Order Management Update", "note2", "2024-01-02", "2024-01-02", "AAD", "AAD"));

			var result = DataUtils.GetDataTableFromQuery(TestConnection, GetReportFromCompanyAndBuyer(glbCompanyPK, orgHeaderPk) + " Where OrderNumber Like 'TestOrder%' Order By OrderNumber");
			var actualResult = result.Rows.Cast<DataRow>().Select(c => string.Concat(c["OrderPK"].ToString(), "|", c["OrderUpdateHistory"].ToString(), "|", c["OrderManagementUpdate"].ToString()));

			var expectedResult = new[]
			{
				string.Format(@"{0}| AAB Jan  2 2024 12:00AM From AAA To BBB,  AAC Jan  1 2024 12:00AM note1|note1", orderWithNote1Pk),
				string.Format(@"{0}| AAB Jan  3 2024 12:00AM From CCC To DDD,  AAD Jan  2 2024 12:00AM note2|note2", orderWithNote2Pk),
				string.Format(@"{0}||", orderWithoutNotePk)
			};

			AssertContainsExactElementsInAnyOrder(expectedResult, actualResult);
		}

		#endregion

		#region GoodsAvailableAt/PickupAddress

		public void TestGoodsAvailableAtAndPickupAddress()
		{
			var orgHeader2PK = Guid.NewGuid();
			var orgAddress2PK = Guid.NewGuid();
			var jobOrderHeader2PK = Guid.NewGuid();
			TestConnection.ExecuteNonQuery(GetInsertOrgHeaderCommand(orgHeader2PK, "HEADER500"));
			TestConnection.ExecuteNonQuery(GetInsertOrgAddressCommand(orgAddress2PK, orgHeader2PK, "TEST500"));
			TestConnection.ExecuteNonQuery(GetInsertJobOrderHeaderCommand(jobOrderHeader2PK, Guid.Empty, orgAddress2PK));

			CreateJobDocAddress(jobOrderHeader2PK, orgAddress2PK, "GAA");

			var result = DataUtils.GetDataTableFromQuery(TestConnection, GetReportFromCompanyAndBuyer(glbCompanyPK, orgHeader2PK));
			AssertEquals("Result should have one row", 1, result.Rows.Count);

			AssertEquals("TEST500", result.Rows[0]["GoodsAvailableAt"]);
			AssertEquals("TEST500", result.Rows[0]["PickupAddress"]);
		}

		public void TestGoodsAvailableAtAndPickupAddress_OverrideAddress()
		{
			var orgHeader2PK = Guid.NewGuid();
			var orgAddress2PK = Guid.NewGuid();
			var jobOrderHeader2PK = Guid.NewGuid();
			TestConnection.ExecuteNonQuery(GetInsertOrgHeaderCommand(orgHeader2PK, "HEADER500"));
			TestConnection.ExecuteNonQuery(GetInsertOrgAddressCommand(orgAddress2PK, orgHeader2PK, "TEST500"));
			TestConnection.ExecuteNonQuery(GetInsertJobOrderHeaderCommand(jobOrderHeader2PK, Guid.Empty, orgAddress2PK));

			CreateJobDocAddress_OverrideAddress(jobOrderHeader2PK, orgAddress2PK, "GAA");

			var result = DataUtils.GetDataTableFromQuery(TestConnection, GetReportFromCompanyAndBuyer(glbCompanyPK, orgHeader2PK));
			AssertEquals("Result should have one row", 1, result.Rows.Count);

			AssertEquals("TEST Address1", result.Rows[0]["GoodsAvailableAt"]);
			AssertEquals("TEST Address1", result.Rows[0]["PickupAddress"]);
		}

		void CreateJobDocAddress(Guid orderPK, Guid orgAddressPK, string addressType)
		{
			var pk = Guid.NewGuid();

			const string sql = @"INSERT INTO dbo.JobDocAddress
(E2_PK, E2_IsValid, E2_AddressType, E2_OA_Address, E2_ParentID, E2_ParentTableCode)
VALUES(@pk, 1, @addressType, @orgAddressPK, @orderPK, 'JD')";

			using (var command = TestConnection.Command(sql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@addressType", SqlDbType.VarChar, addressType);
				command.AddParameter("@orgAddressPK", SqlDbType.UniqueIdentifier, orgAddressPK);
				command.AddParameter("@orderPK", SqlDbType.UniqueIdentifier, orderPK);
				command.ExecuteNonQuery();
			}
		}

		void CreateJobDocAddress_OverrideAddress(Guid orderPK, Guid orgAddressPK, string addressType)
		{
			var pk = Guid.NewGuid();

			const string sql = @"INSERT INTO dbo.JobDocAddress
(E2_PK, E2_IsValid, E2_AddressType, E2_OA_Address, E2_ParentID, E2_ParentTableCode, E2_AddressOverride, E2_CompanyName, E2_Address1, E2_Address2, E2_ValidationStatus)
VALUES(@pk, 1, @addressType, @orgAddressPK, @orderPK, 'JD', 1, 'Overridden Company', 'TEST Address1', 'Test Address2', 'MAN')";

			using (var command = TestConnection.Command(sql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@addressType", SqlDbType.VarChar, addressType);
				command.AddParameter("@orgAddressPK", SqlDbType.UniqueIdentifier, orgAddressPK);
				command.AddParameter("@orderPK", SqlDbType.UniqueIdentifier, orderPK);
				command.ExecuteNonQuery();
			}
		}

		#endregion

		#region GoodsDeliveredTo/DeliveryAddress

		public void TestGoodsDeliveredToAndDeliveryAddress()
		{
			var orgHeader2PK = Guid.NewGuid();
			var orgAddress2PK = Guid.NewGuid();
			var jobOrderHeader2PK = Guid.NewGuid();
			TestConnection.ExecuteNonQuery(GetInsertOrgHeaderCommand(orgHeader2PK, "HEADER500"));
			TestConnection.ExecuteNonQuery(GetInsertOrgAddressCommand(orgAddress2PK, orgHeader2PK, "TEST500"));
			TestConnection.ExecuteNonQuery(GetInsertJobOrderHeaderCommand(jobOrderHeader2PK, Guid.Empty, orgAddress2PK));

			CreateJobDocAddress(jobOrderHeader2PK, orgAddress2PK, "GDT");

			var result = DataUtils.GetDataTableFromQuery(TestConnection, GetReportFromCompanyAndBuyer(glbCompanyPK, orgHeader2PK));
			AssertEquals("Result should have one row", 1, result.Rows.Count);

			AssertEquals("TEST500", result.Rows[0]["GoodsDeliveredTo"]);
			AssertEquals("TEST500", result.Rows[0]["DeliveryAddress"]);
		}

		public void TestGoodsDeliveredToAndDeliveryAddress_OverrideAddress()
		{
			var orgHeader2PK = Guid.NewGuid();
			var orgAddress2PK = Guid.NewGuid();
			var jobOrderHeader2PK = Guid.NewGuid();
			TestConnection.ExecuteNonQuery(GetInsertOrgHeaderCommand(orgHeader2PK, "HEADER500"));
			TestConnection.ExecuteNonQuery(GetInsertOrgAddressCommand(orgAddress2PK, orgHeader2PK, "TEST500"));
			TestConnection.ExecuteNonQuery(GetInsertJobOrderHeaderCommand(jobOrderHeader2PK, Guid.Empty, orgAddress2PK));

			CreateJobDocAddress_OverrideAddress(jobOrderHeader2PK, orgAddress2PK, "GDT");

			var result = DataUtils.GetDataTableFromQuery(TestConnection, GetReportFromCompanyAndBuyer(glbCompanyPK, orgHeader2PK));
			AssertEquals("Result should have one row", 1, result.Rows.Count);

			AssertEquals("TEST Address1", result.Rows[0]["GoodsDeliveredTo"]);
			AssertEquals("TEST Address1", result.Rows[0]["DeliveryAddress"]);
		}

		#endregion

		#region CustomsCommencedFields

		public void TestDateCustomsCommencedActualField()
		{
			var declarationPk = Guid.NewGuid();
			var customsCommencedDate = new DateTime(2022, 08, 08, 10, 05, 00);
			TestConnection.ExecuteNonQuery(string.Format(GetInsertDeclarationCommand(declarationPk, 1, customsCommencedDate)));

			var updateSetupData = @$"
UPDATE dbo.JobOrderHeader
SET
	JD_JS=NULL, JD_JE='{declarationPk}',
	JD_SystemLastEditTimeUtc = GETUTCDATE(),
	JD_SystemLastEditUser = '~BP'
WHERE
	JD_PK ='{jobOrderHeaderPK}'";
			TestConnection.ExecuteNonQuery(updateSetupData);

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, GetReportFromCompanyAndBuyer(glbCompanyPK, orgHeaderPk));

			AssertEquals("Result should have one row", 1, result.Rows.Count);

			var dateCommencedActual = result.Rows[0].Field<DateTime?>("DateCustomsCommencedActual")?.ToString(DateFormat); // Get Customs commenced actual date

			AssertEquals("DateCustomsCommencedActual should be 8/08/2022", "8/08/2022", dateCommencedActual);
		}

		#endregion

		#region InsetNewTestData

		Guid orgHeaderPk;
		Guid orgAddressPK;
		Guid jobOrderHeaderPK;
		Guid glbCompanyPK;
		Guid jobShipmentPK;

		string GetInsertOrgHeaderCommand()
		{
			return GetInsertOrgHeaderCommand(orgHeaderPk, "TSTOH");
		}

		string GetInsertOrgHeaderCommand(Guid pk, string code)
		{
			return string.Format(
			 @"INSERT INTO dbo.OrgHeader
             ([OH_PK]
             ,[OH_Code]
             ,[OH_FullName]
             ,[OH_SystemCreateTimeUtc]
             ,[OH_SystemCreateUser]
             ,[OH_SystemLastEditTimeUtc]
             ,[OH_SystemLastEditUser])
             VALUES
             ('{0}','{1}','Test Organisation', GetUtcDate(), '~BP', GetUtcDate(), '~BP') ", pk, code);
		}

		string GetInsertOrgAddressCommand()
		{
			return GetInsertOrgAddressCommand(orgAddressPK, orgHeaderPk, "TEST400");
		}

		string GetInsertOrgAddressCommand(Guid addressPK, Guid headerPK, string code)
		{
			return string.Format(
				@"INSERT INTO dbo.OrgAddress
				([OA_PK]
				,[OA_OH]
				,[OA_Address1]
				,[OA_Code]
        ,[OA_SystemCreateTimeUtc]
        ,[OA_SystemCreateUser]
        ,[OA_SystemLastEditTimeUtc]
        ,[OA_SystemLastEditUser])
				VALUES
				('{0}', '{1}', 'Address 1', '{2}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')", addressPK, headerPK, code);
		}

		string GetInsertGlbStaffCommand()
		{
			return string.Format(@"
DECLARE @PerPk UNIQUEIDENTIFIER = newid()
INSERT INTO dbo.GlbPerson(PER_PK, PER_FullName, PER_SystemCreateTimeUtc, PER_SystemCreateUser, PER_SystemLastEditTimeUtc, PER_SystemLastEditUser) values (@PerPk, 'name', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_FullName, GS_PER, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES (NEWID(),'TGS','TestStaff', @PerPk, GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
		}

		string GetInsertGlbCompanyCommand()
		{
			return string.Format(
			@"INSERT INTO dbo.GlbCompany
            ([GC_PK]
            ,[GC_Code]
            ,[GC_Name]
            ,[GC_OH_OrgProxy]
			,[GC_RN_NKCountryCode]
			,[GC_RX_NKLocalCurrency])
            VALUES
            ('{0}','TGC', 'AU company','{1}', 'AU', 'AUD')", glbCompanyPK, orgHeaderPk);
		}

		string GetInsertJobShipmentCommand()
		{
			return string.Format("INSERT INTO dbo.JobShipment ([JS_PK],[JS_IsCancelled],[JS_SystemCreateTimeUtc],[JS_SystemCreateUser],[JS_SystemLastEditTimeUtc],[JS_SystemLastEditUser]) VALUES ('{0}', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP')", jobShipmentPK);
		}

		string GetInsertJobOrderHeaderCommand()
		{
			return GetInsertJobOrderHeaderCommand(jobOrderHeaderPK, jobShipmentPK, orgAddressPK);
		}

		string GetInsertJobOrderHeaderCommand(Guid orderHeaderPK, Guid shipmentPK, Guid addressPK)
		{
			return string.Format(
				@"INSERT INTO dbo.JobOrderHeader
				([JD_PK]
				,[JD_JS]
				,[JD_JE]
				,[JD_EF_ShipmentPrePlanning]
				,[JD_OA_BuyerAddress]
				,[JD_OrderStatus]
				,[JD_SystemCreateTimeUTC]
				,[JD_SystemCreateUser]
				,[JD_IsValid]
				,[JD_IsCancelled]
        ,[JD_SystemLastEditTimeUtc]
        ,[JD_SystemLastEditUser])
				VALUES
				('{0}', {1}, NULL, NULL, '{2}', 'INC', '2014-01-01', 'TGS', '1','0', GetUtcDate(), 'TGS')",
				orderHeaderPK,
				shipmentPK == Guid.Empty ? "NULL" : string.Format(CultureInfo.InvariantCulture, "'{0}'", shipmentPK),
				addressPK);
		}

		string GetInsertStmALogsCommand(string eventTime, string eventCode, bool isCancelled, Guid parentID, string parentTableName)
		{
			return string.Format(
			@"INSERT INTO dbo.StmALog
              ([SL_PK]
              ,[SL_EventTime]
              ,[SL_PostedTimeUtc]
              ,[SL_GS_NKUser]
              ,[SL_IsEstimate]
              ,[SL_Parent]
              ,[SL_SE_NKEvent]
              ,[SL_IsCancelled]
              ,[SL_Table])
              VALUES
              (NEWID(), '{0}', '{0}', 'E', 'N', '{1}', '{2}', '{3}', '{4}')", eventTime, parentID, eventCode, isCancelled ? "Y" : "N", parentTableName);
		}

		string GetInsertProcessTaskCommand(Guid parentID, string parentTableCode, string eventCode, string actualDate)
		{
			return string.Format(
			@"INSERT INTO dbo.ProcessTasks 
            (P9_PK, 
             P9_GC,
             P9_ParentID, 
             P9_ParentTableCode, 
             P9_Type, 
             P9_SE_NKExceptionEvent,
             P9_SE_NKMilestoneEvent,
             P9_ActualDate,
						 P9_SystemCreateTimeUtc,
						 P9_SystemCreateUser,
						 P9_SystemLastEditTimeUtc,
						 P9_SystemLastEditUser) 
             VALUES
            (NEWID(), '{0}','{1}', '{2}', 'MIL','EXC', '{3}', '{4}', GetUtcDate(), '~BP', GetUtcDate(), '~BP') ", glbCompanyPK, parentID, parentTableCode, eventCode, actualDate);
		}

		string GetInsertNoteCommand(Guid pk, Guid parentId, string parentTable, string description, string noteText, string createdTime, string editTime, string createUser = "AAB", string editUser = "AAB")
		{
			return string.Format(
			@"INSERT INTO dbo.StmNote
				(ST_PK,
				ST_ParentID,
				ST_Table,
				ST_GC_RelatedCompany,
				ST_Description,
				ST_IsCustomDescription,
				ST_ForceRead,
				ST_NoteData,
				ST_NoteText,
				ST_NoteType,
				ST_NoteContext,
				ST_SystemCreateTimeUtc,
				ST_SystemCreateUser,
				ST_SystemLastEditTimeUtc,
				ST_SystemLastEditUser)
			VALUES
			('{0}', '{1}', '{2}', NULL, '{3}', 0, 1, NULL, '{4}', 'INT', 'AAA', '{5}', '{6}', '{7}', '{8}')", pk, parentId, parentTable, description, noteText, createdTime, createUser, editTime, editUser);
		}

		string GetInsertDeclarationCommand(Guid pk, int clusterKey, DateTime? customsCommencedDate)
		{
			return string.Format(
				@"DECLARE @BranchPK uniqueidentifier;
				DECLARE @CompanyPK uniqueidentifier;
				SELECT TOP 1 @BranchPK = GB_PK, @CompanyPK = GB_GC FROM dbo.GlbBranch;
				INSERT INTO dbo.JobDeclaration (JE_PK, JE_DataModel, JE_GB, JE_GC, JE_IsCancelled, JE_ClusterKey, JE_CustomsCommencedDate, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser) Values ('{0}', '!!', @BranchPK, @CompanyPK, 0, {1}, '{2}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')", pk, clusterKey, customsCommencedDate);
		}

		string GetInsertPreplanningCommand(Guid pk, Guid buyerAddressId)
		{
			return string.Format(
				@"INSERT INTO dbo.JobShipmentPreplanning
				(EF_PK,
				EF_OA_BuyerAddress,
				EF_PreshipID,
				EF_SystemCreateTimeUtc,
				EF_SystemCreateUser,
				EF_SystemLastEditTimeUtc,
				EF_SystemLastEditUser)
			VALUES
			('{0}', '{1}', 'PrepID', GetUtcDate(), '~BP', GetUtcDate(), '~BP')", pk, buyerAddressId);
		}

		#endregion

		#region GetReport

		string GetReportFromCompanyAndBuyer(Guid companyPk, Guid orgPk, string clientType = "Buyer/Controlling Customer")
		{
			return string.Format("SELECT * FROM Report_OrderStatusSummaryReport ('{0}', '{1}', '{2}', '', '', 'N', '', '', '2013-01-01')", companyPk, orgPk, clientType);
		}
		#endregion
	}
}

