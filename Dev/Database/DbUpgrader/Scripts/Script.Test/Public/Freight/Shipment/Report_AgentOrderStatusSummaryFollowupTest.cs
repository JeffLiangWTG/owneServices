using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Shipment;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Shipment
{
	[TestedType(typeof(Report_AgentOrderStatusSummaryFollowup))]
	class Report_AgentOrderStatusSummaryFollowupTest : DbCreateScriptTest
	{
		public void TestGoodsAvailableAtAndPickupAddress()
		{
			var buyerPK = Guid.NewGuid();
			var buyerAddressPK = Guid.NewGuid();
			var orgHeader2PK = Guid.NewGuid();
			var orgAddress2PK = Guid.NewGuid();
			var sendingAgentPK = Guid.NewGuid();
			var sendingAgentAddressPK = Guid.NewGuid();
			var orderHeaderPK = Guid.NewGuid();

			TestConnection.ExecuteNonQuery(GetInsertOrgHeaderCommand(buyerPK, "HEADER500"));
			TestConnection.ExecuteNonQuery(GetInsertOrgAddressCommand(buyerAddressPK, buyerPK, "TEST500"));
			TestConnection.ExecuteNonQuery(GetInsertOrgHeaderCommand(orgHeader2PK, "HEADER501"));
			TestConnection.ExecuteNonQuery(GetInsertOrgAddressCommand(orgAddress2PK, orgHeader2PK, "ADDRESS 1 ST"));
			TestConnection.ExecuteNonQuery(GetInsertOrgHeaderCommand(sendingAgentPK, "AGENT502"));
			TestConnection.ExecuteNonQuery(GetInsertOrgAddressCommand(sendingAgentAddressPK, sendingAgentPK, "TEST502"));
			TestConnection.ExecuteNonQuery(GetInsertJobOrderHeaderCommand(orderHeaderPK, buyerAddressPK, sendingAgentPK));

			CreateJobDocAddress(orderHeaderPK, orgAddress2PK, "GAA");

			var result = DataUtils.GetDataTableFromQuery(TestConnection, GetReportFromSendingAgentAndBuyer(sendingAgentPK, buyerPK));
			AssertEquals("Result should have one row", 1, result.Rows.Count);

			AssertEquals("ADDRESS 1 ST", result.Rows[0]["GoodsAvailableAt"]);
			AssertEquals("ADDRESS 1 ST", result.Rows[0]["PickupAddress"]);
		}

		public void TestGoodsAvailableAtAndPickupAddress_OverrideAddress()
		{
			var buyerPK = Guid.NewGuid();
			var buyerAddressPK = Guid.NewGuid();
			var orgHeader2PK = Guid.NewGuid();
			var orgAddress2PK = Guid.NewGuid();
			var sendingAgentPK = Guid.NewGuid();
			var sendingAgentAddressPK = Guid.NewGuid();
			var orderHeaderPK = Guid.NewGuid();

			TestConnection.ExecuteNonQuery(GetInsertOrgHeaderCommand(buyerPK, "HEADER500"));
			TestConnection.ExecuteNonQuery(GetInsertOrgAddressCommand(buyerAddressPK, buyerPK, "TEST500"));
			TestConnection.ExecuteNonQuery(GetInsertOrgHeaderCommand(orgHeader2PK, "HEADER501"));
			TestConnection.ExecuteNonQuery(GetInsertOrgAddressCommand(orgAddress2PK, orgHeader2PK, "ADDRESS 1 ST"));
			TestConnection.ExecuteNonQuery(GetInsertOrgHeaderCommand(sendingAgentPK, "AGENT502"));
			TestConnection.ExecuteNonQuery(GetInsertOrgAddressCommand(sendingAgentAddressPK, sendingAgentPK, "TEST502"));
			TestConnection.ExecuteNonQuery(GetInsertJobOrderHeaderCommand(orderHeaderPK, buyerAddressPK, sendingAgentPK));

			CreateJobDocAddress_OverrideAddress(orderHeaderPK, orgAddress2PK, "GAA");

			var result = DataUtils.GetDataTableFromQuery(TestConnection, GetReportFromSendingAgentAndBuyer(sendingAgentPK, buyerPK));
			AssertEquals("Result should have one row", 1, result.Rows.Count);

			AssertEquals("TEST Address1", result.Rows[0]["GoodsAvailableAt"]);
			AssertEquals("TEST Address1", result.Rows[0]["PickupAddress"]);
		}

		public void TestGoodsDeliveredToAndDeliveryAddress()
		{
			var buyerPK = Guid.NewGuid();
			var buyerAddressPK = Guid.NewGuid();
			var orgHeader2PK = Guid.NewGuid();
			var orgAddress2PK = Guid.NewGuid();
			var sendingAgentPK = Guid.NewGuid();
			var sendingAgentAddressPK = Guid.NewGuid();
			var orderHeaderPK = Guid.NewGuid();

			TestConnection.ExecuteNonQuery(GetInsertOrgHeaderCommand(buyerPK, "HEADER500"));
			TestConnection.ExecuteNonQuery(GetInsertOrgAddressCommand(buyerAddressPK, buyerPK, "TEST500"));
			TestConnection.ExecuteNonQuery(GetInsertOrgHeaderCommand(orgHeader2PK, "HEADER501"));
			TestConnection.ExecuteNonQuery(GetInsertOrgAddressCommand(orgAddress2PK, orgHeader2PK, "ADDRESS 1 ST"));
			TestConnection.ExecuteNonQuery(GetInsertOrgHeaderCommand(sendingAgentPK, "AGENT502"));
			TestConnection.ExecuteNonQuery(GetInsertOrgAddressCommand(sendingAgentAddressPK, sendingAgentPK, "TEST502"));
			TestConnection.ExecuteNonQuery(GetInsertJobOrderHeaderCommand(orderHeaderPK, buyerAddressPK, sendingAgentPK));

			CreateJobDocAddress(orderHeaderPK, orgAddress2PK, "GDT");

			var result = DataUtils.GetDataTableFromQuery(TestConnection, GetReportFromSendingAgentAndBuyer(sendingAgentPK, buyerPK));
			AssertEquals("Result should have one row", 1, result.Rows.Count);

			AssertEquals("ADDRESS 1 ST", result.Rows[0]["GoodsDeliveredTo"]);
			AssertEquals("ADDRESS 1 ST", result.Rows[0]["DeliveryAddress"]);
		}

		public void TestGoodsDeliveredToAndDeliveryAddress_OverrideAddress()
		{
			var buyerPK = Guid.NewGuid();
			var buyerAddressPK = Guid.NewGuid();
			var orgHeader2PK = Guid.NewGuid();
			var orgAddress2PK = Guid.NewGuid();
			var sendingAgentPK = Guid.NewGuid();
			var sendingAgentAddressPK = Guid.NewGuid();
			var orderHeaderPK = Guid.NewGuid();

			TestConnection.ExecuteNonQuery(GetInsertOrgHeaderCommand(buyerPK, "HEADER500"));
			TestConnection.ExecuteNonQuery(GetInsertOrgAddressCommand(buyerAddressPK, buyerPK, "TEST500"));
			TestConnection.ExecuteNonQuery(GetInsertOrgHeaderCommand(orgHeader2PK, "HEADER501"));
			TestConnection.ExecuteNonQuery(GetInsertOrgAddressCommand(orgAddress2PK, orgHeader2PK, "ADDRESS 1 ST"));
			TestConnection.ExecuteNonQuery(GetInsertOrgHeaderCommand(sendingAgentPK, "AGENT502"));
			TestConnection.ExecuteNonQuery(GetInsertOrgAddressCommand(sendingAgentAddressPK, sendingAgentPK, "TEST502"));
			TestConnection.ExecuteNonQuery(GetInsertJobOrderHeaderCommand(orderHeaderPK, buyerAddressPK, sendingAgentPK));

			CreateJobDocAddress_OverrideAddress(orderHeaderPK, orgAddress2PK, "GDT");

			var result = DataUtils.GetDataTableFromQuery(TestConnection, GetReportFromSendingAgentAndBuyer(sendingAgentPK, buyerPK));
			AssertEquals("Result should have one row", 1, result.Rows.Count);

			AssertEquals("TEST Address1", result.Rows[0]["GoodsDeliveredTo"]);
			AssertEquals("TEST Address1", result.Rows[0]["DeliveryAddress"]);
		}

		#region OrderUpdateHistory

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

		public void TestOrderWithNotes()
		{
			var buyerPK = Guid.NewGuid();
			TestConnection.ExecuteNonQuery(GetInsertOrgHeaderCommand(buyerPK, "HEADER500"));

			var buyerAddressPK = Guid.NewGuid();
			TestConnection.ExecuteNonQuery(GetInsertOrgAddressCommand(buyerAddressPK, buyerPK, "TEST500"));

			var orgHeaderPK = Guid.NewGuid();
			TestConnection.ExecuteNonQuery(GetInsertOrgHeaderCommand(orgHeaderPK, "HEADER501"));

			var orgAddressPK = Guid.NewGuid();
			TestConnection.ExecuteNonQuery(GetInsertOrgAddressCommand(orgAddressPK, orgHeaderPK, "ADDRESS 1 ST"));

			var sendingAgentPK = Guid.NewGuid();
			TestConnection.ExecuteNonQuery(GetInsertOrgHeaderCommand(sendingAgentPK, "AGENT502"));

			var sendingAgentAddressPK = Guid.NewGuid();
			TestConnection.ExecuteNonQuery(GetInsertOrgAddressCommand(sendingAgentAddressPK, sendingAgentPK, "TEST502"));

			var orderHeaderPK = Guid.NewGuid();
			TestConnection.ExecuteNonQuery(GetInsertJobOrderHeaderCommand(orderHeaderPK, buyerAddressPK, sendingAgentPK));
			CreateJobDocAddress(orderHeaderPK, orgAddressPK, "GAA");

			var note1Pk = Guid.NewGuid();
			var note2Pk = Guid.NewGuid();
			var note3Pk = Guid.NewGuid();

			TestConnection.ExecuteNonQuery(GetInsertNoteCommand(note1Pk, orderHeaderPK, "JobOrderHeader", "Order Update History", "From AAA To BBB", "2024-01-02", "2024-01-02"));
			TestConnection.ExecuteNonQuery(GetInsertNoteCommand(note2Pk, orderHeaderPK, "JobOrderHeader", "Other Note", "TEST01", "2024-01-02", "2024-01-02"));
			TestConnection.ExecuteNonQuery(GetInsertNoteCommand(note3Pk, orderHeaderPK, "JobOrderHeader", "Order Management Update", "note1", "2024-01-01", "2024-01-01", "AAC", "AAC"));

			var result = DataUtils.GetDataTableFromQuery(TestConnection, GetReportFromSendingAgentAndBuyer(sendingAgentPK, buyerPK));
			AssertEquals("Result should have one row", 1, result.Rows.Count);

			var actualResult = result.Rows.Cast<DataRow>().Select(c => string.Concat(c["JobOrderHeaderPK"].ToString(), "|", c["OrderUpdateHistory"].ToString()));
			var expectedResult = new[]
			{
				string.Format(@"{0}| AAB Jan  2 2024 12:00AM From AAA To BBB,  AAC Jan  1 2024 12:00AM note1", orderHeaderPK),
			};

			AssertContainsExactElementsInAnyOrder(expectedResult, actualResult);
		}

		#endregion

		#region Implementation

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

		string GetInsertOrgHeaderCommand(Guid pk, string code)
		{
			return string.Format(
			 @"INSERT INTO dbo.OrgHeader
             ([OH_PK]
             ,[OH_Code]
             ,[OH_FullName])
             VALUES
             ('{0}','{1}','Test Organisation') ", pk, code);
		}

		string GetInsertOrgAddressCommand(Guid addressPK, Guid headerPK, string code)
		{
			return string.Format(
				@"INSERT INTO dbo.OrgAddress
				([OA_PK]
				,[OA_OH]
				,[OA_Address1]
				,[OA_Code])
				VALUES
				('{0}', '{1}', 'Address 1', '{2}')", addressPK, headerPK, code);
		}

		string GetInsertJobOrderHeaderCommand(Guid orderHeaderPK, Guid addressPK, Guid sendingAgentPK)
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
				,[JD_OH_SendingAgent])
				VALUES
				('{0}', NULL, NULL, NULL, '{1}', 'INC', '2014-01-01', 'TGS', '1', '0', '{2}')",
				orderHeaderPK,
				addressPK,
				sendingAgentPK);
		}

		string GetInsertGlbStaffCommand()
		{
			return string.Format(@"
DECLARE @PerPk UNIQUEIDENTIFIER = newid()
INSERT INTO dbo.GlbPerson(PER_PK, PER_FullName) values (@PerPk, 'name')

INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_FullName, GS_PER, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
VALUES (NEWID(),'TGS','TestStaff', @PerPk, GETUTCDATE(), 'E', GETUTCDATE(), 'E')");
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

		string GetReportFromSendingAgentAndBuyer(Guid sendingAgentPK, Guid buyerPK)
		{
			return string.Format(@"SELECT *
FROM Report_AgentOrderStatusSummaryFollowup ('', '', '', '', '', '', '', '')
WHERE SendingAgentPK = '{0}' AND BuyerPK = '{1}'",
				sendingAgentPK, buyerPK);
		}

		protected override void SetUp()
		{
			base.SetUp();

			orgHeaderPk = Guid.NewGuid();
			orgAddressPK = Guid.NewGuid();
			glbCompanyPK = Guid.NewGuid();

			TestConnection.ExecuteNonQuery(GetInsertOrgHeaderCommand(orgHeaderPk, "TSTOH"));
			TestConnection.ExecuteNonQuery(GetInsertOrgAddressCommand(orgAddressPK, orgHeaderPk, "TEST400"));
			TestConnection.ExecuteNonQuery(GetInsertGlbStaffCommand());
			TestConnection.ExecuteNonQuery(GetInsertGlbCompanyCommand());
		}

		Guid orgHeaderPk;
		Guid orgAddressPK;
		Guid glbCompanyPK;
		#endregion
	}
}

