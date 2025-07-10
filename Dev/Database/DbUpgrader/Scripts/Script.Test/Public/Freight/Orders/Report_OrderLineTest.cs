using System;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Orders;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Orders
{
	[TestedType(typeof(Report_OrderLine))]
	class Report_OrderLineTest : DbCreateScriptTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1122:DoNotUseDateTimeParse", Justification = "Get estimate / actual date")]
		public void TestGetEstimateAndActualDate()
		{
			var (glbCompanyPK, orgHeaderPK) = PrepareGetEstimatedAndActualDateTest("2013-12-12", "2013-12-25");

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT * FROM Report_OrderLine ('{0}', '{1}', 'Buyer/Controlling Customer', NULL, NULL, 'N', NULL, '', '', '', '2013-01-01', '2013-01-01')", glbCompanyPK, orgHeaderPK));

			AssertEquals("Result should have one row", 1, result.Rows.Count);
			var dateCommencedEstimate = DateTime.Parse(result.Rows[0]["DateCustomsCommencedEstimate"].ToString()); // Get Estimate Date
			AssertEquals("DateCustomsCommencedEstimate should be default value", (new DateTime(1900, 01, 1)).ToShortDateString(), dateCommencedEstimate.ToShortDateString());

			var dateCommencedActual = DateTime.Parse(result.Rows[0]["DateCustomsCommencedActual"].ToString()); // Get Actual Date
			AssertEquals("DateCustomsCommencedActual should not be null", (new DateTime(2013, 12, 25)).ToShortDateString(), dateCommencedActual.ToShortDateString());

			var dateClearedEstimate = DateTime.Parse(result.Rows[0]["DateCustomsClearedEstimate"].ToString()); // Get Estimate Date
			AssertEquals("DateCustomsClearedEstimate should not be null", (new DateTime(2013, 12, 12)).ToShortDateString(), dateClearedEstimate.ToShortDateString());

			var dateClearedActual = DateTime.Parse(result.Rows[0]["DateCustomsClearedActual"].ToString()); // Get Actual Date
			AssertEquals("DateCustomsClearedActual should be default value", (new DateTime(1900, 01, 1)).ToShortDateString(), dateClearedActual.ToShortDateString());
		}

		public void TestOrderLinesWithEmptyVesselAndVoyageFlight()
		{
			var (glbCompanyPK, orgHeaderPK) = PrepareTestDataWithConsolAndTransportData();
			var result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT * FROM Report_OrderLine ('{0}', '{1}', 'Buyer/Controlling Customer', NULL, NULL, 'N', NULL, '', '', '', '2013-01-01', '2013-01-01')", glbCompanyPK, orgHeaderPK));
			AssertEquals("Result should have one row", 1, result.Rows.Count);
		}

		#region GoodsAvailableAt/PickupAddress

		public void TestGoodsAvailableAtAndPickupAddress()
		{
			var orgHeaderPK_Supplier = Guid.NewGuid();
			var orgAddressPK_Supplier = Guid.NewGuid();
			var glbCompanyPK = Guid.NewGuid();
			var glbBranchPK = Guid.NewGuid();
			var glbDepartmentPK = Guid.NewGuid();
			var orgHeaderPK_Buyer = Guid.NewGuid();
			var orgAddressPK_Buyer = Guid.NewGuid();
			var jobOrderHeaderPK = Guid.NewGuid();

			InsertOrgHeader("TOH00", orgHeaderPK_Supplier);
			InsertOrgAddress(orgAddressPK_Supplier, orgHeaderPK_Supplier, "ADDRESS 1", "TEST400");
			InsertGlbCompany(glbCompanyPK, orgHeaderPK_Supplier);
			InsertGlbBranch(glbBranchPK, glbCompanyPK);
			InsertGlbDepartment(glbDepartmentPK);

			InsertOrgHeader("TOH01", orgHeaderPK_Buyer);
			InsertOrgAddress(orgAddressPK_Buyer, orgHeaderPK_Buyer, "ADDRESS 2", "TEST500");
			InsertJobOrderHeader(jobOrderHeaderPK, Guid.Empty, orgAddressPK_Buyer, Guid.Empty);
			InsertJobOrderLine(jobOrderHeaderPK);
			InsertJobDocAddressForOrder(jobOrderHeaderPK, orgAddressPK_Buyer, "GAA");

			var result = DataUtils.GetDataTableFromQuery(TestConnection,
				string.Format(CultureInfo.InvariantCulture, "SELECT * FROM Report_OrderLine ('{0}', '{1}', 'Buyer/Controlling Customer', NULL, NULL, 'N', NULL, '', '', '', '2013-01-01', '2013-01-01')", glbCompanyPK, orgHeaderPK_Buyer));
			AssertEquals("Result should have one row", 1, result.Rows.Count);

			AssertEquals("TEST500", result.Rows[0]["GoodsAvailableAt"]);
			AssertEquals("TEST500", result.Rows[0]["PickupAddress"]);
		}

		public void TestGoodsAvailableAtAndPickupAddress_OverrideAddress()
		{
			var orgHeaderPK_Supplier = Guid.NewGuid();
			var orgAddressPK_Supplier = Guid.NewGuid();
			var glbCompanyPK = Guid.NewGuid();
			var glbBranchPK = Guid.NewGuid();
			var glbDepartmentPK = Guid.NewGuid();
			var orgHeaderPK_Buyer = Guid.NewGuid();
			var orgAddressPK_Buyer = Guid.NewGuid();
			var jobOrderHeaderPK = Guid.NewGuid();

			InsertOrgHeader("TOH00", orgHeaderPK_Supplier);
			InsertOrgAddress(orgAddressPK_Supplier, orgHeaderPK_Supplier, "ADDRESS 1", "TEST400");
			InsertGlbCompany(glbCompanyPK, orgHeaderPK_Supplier);
			InsertGlbBranch(glbBranchPK, glbCompanyPK);
			InsertGlbDepartment(glbDepartmentPK);
			InsertOrgHeader("TOH01", orgHeaderPK_Buyer);
			InsertOrgAddress(orgAddressPK_Buyer, orgHeaderPK_Buyer, "ADDRESS 2", "TEST500");
			InsertJobOrderHeader(jobOrderHeaderPK, Guid.Empty, orgAddressPK_Buyer, Guid.Empty);
			InsertJobOrderLine(jobOrderHeaderPK);
			InsertJobDocAddressForOrder_Override(jobOrderHeaderPK, orgAddressPK_Buyer, "GAA");

			var result = DataUtils.GetDataTableFromQuery(TestConnection,
				string.Format(CultureInfo.InvariantCulture, "SELECT * FROM Report_OrderLine ('{0}', '{1}', 'Buyer/Controlling Customer', NULL, NULL, 'N', NULL, '', '', '', '2013-01-01', '2013-01-01')", glbCompanyPK, orgHeaderPK_Buyer));
			AssertEquals("Result should have one row", 1, result.Rows.Count);

			AssertEquals("TEST Address1", result.Rows[0]["GoodsAvailableAt"]);
			AssertEquals("TEST Address1", result.Rows[0]["PickupAddress"]);
		}

		public void TestGoodsAvailableAtAndPickupAddress_AttachedToShipment()
		{
			var (glbCompanyPK, orgHeaderPK) = PrepareTestPickupAndDeliveryAddressesWithShipment();

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT * FROM Report_OrderLine ('{0}', '{1}', 'Buyer/Controlling Customer', NULL, NULL, 'N', NULL, '', '', '', '2013-01-01', '2013-01-01')", glbCompanyPK, orgHeaderPK));

			AssertEquals("Result should have one row", 1, result.Rows.Count);
			AssertEquals("Suppliers Address Mismatch", "PickupAddress", result.Rows[0]["GoodsAvailableAt"]);
			AssertEquals("Suppliers Address Mismatch", "PickupAddress", result.Rows[0]["PickupAddress"]);
		}

		#endregion

		#region GoodsDeliveredTo/DeliveryAddress

		public void TestGoodsDeliveredToAndDeliveryAddress()
		{
			var orgHeaderPK_Supplier = Guid.NewGuid();
			var orgAddressPK_Supplier = Guid.NewGuid();
			var glbCompanyPK = Guid.NewGuid();
			var glbBranchPK = Guid.NewGuid();
			var glbDepartmentPK = Guid.NewGuid();
			var orgHeaderPK_Buyer = Guid.NewGuid();
			var orgAddressPK_Buyer = Guid.NewGuid();
			var jobOrderHeaderPK = Guid.NewGuid();

			InsertOrgHeader("TOH00", orgHeaderPK_Supplier);
			InsertOrgAddress(orgAddressPK_Supplier, orgHeaderPK_Supplier, "ADDRESS 1", "TEST400");
			InsertGlbCompany(glbCompanyPK, orgHeaderPK_Supplier);
			InsertGlbBranch(glbBranchPK, glbCompanyPK);
			InsertGlbDepartment(glbDepartmentPK);
			InsertOrgHeader("TOH01", orgHeaderPK_Buyer);
			InsertOrgAddress(orgAddressPK_Buyer, orgHeaderPK_Buyer, "ADDRESS 2", "TEST500");
			InsertJobOrderHeader(jobOrderHeaderPK, Guid.Empty, orgAddressPK_Buyer, Guid.Empty);
			InsertJobOrderLine(jobOrderHeaderPK);
			InsertJobDocAddressForOrder(jobOrderHeaderPK, orgAddressPK_Buyer, "GDT");

			var result = DataUtils.GetDataTableFromQuery(TestConnection,
				string.Format(CultureInfo.InvariantCulture, "SELECT * FROM Report_OrderLine ('{0}', '{1}', 'Buyer/Controlling Customer', NULL, NULL, 'N', NULL, '', '', '', '2013-01-01', '2013-01-01')", glbCompanyPK, orgHeaderPK_Buyer));
			AssertEquals("Result should have one row", 1, result.Rows.Count);

			AssertEquals("TEST500", result.Rows[0]["GoodsDeliveredTo"]);
			AssertEquals("TEST500", result.Rows[0]["DeliveryAddress"]);
		}

		public void TestGoodsDeliveredToAndDeliveryAddress_OverrideAddress()
		{
			var orgHeaderPK_Supplier = Guid.NewGuid();
			var orgAddressPK_Supplier = Guid.NewGuid();
			var glbCompanyPK = Guid.NewGuid();
			var glbBranchPK = Guid.NewGuid();
			var glbDepartmentPK = Guid.NewGuid();
			var orgHeaderPK_Buyer = Guid.NewGuid();
			var orgAddressPK_Buyer = Guid.NewGuid();
			var jobOrderHeaderPK = Guid.NewGuid();

			InsertOrgHeader("TOH00", orgHeaderPK_Supplier);
			InsertOrgAddress(orgAddressPK_Supplier, orgHeaderPK_Supplier, "ADDRESS 1", "TEST400");
			InsertGlbCompany(glbCompanyPK, orgHeaderPK_Supplier);
			InsertGlbBranch(glbBranchPK, glbCompanyPK);
			InsertGlbDepartment(glbDepartmentPK);
			InsertOrgHeader("TOH01", orgHeaderPK_Buyer);
			InsertOrgAddress(orgAddressPK_Buyer, orgHeaderPK_Buyer, "ADDRESS 2", "TEST500");
			InsertJobOrderHeader(jobOrderHeaderPK, Guid.Empty, orgAddressPK_Buyer, Guid.Empty);
			InsertJobOrderLine(jobOrderHeaderPK);
			InsertJobDocAddressForOrder_Override(jobOrderHeaderPK, orgAddressPK_Buyer, "GDT");

			var result = DataUtils.GetDataTableFromQuery(TestConnection,
				string.Format(CultureInfo.InvariantCulture, "SELECT * FROM Report_OrderLine ('{0}', '{1}', 'Buyer/Controlling Customer', NULL, NULL, 'N', NULL, '', '', '', '2013-01-01', '2013-01-01')", glbCompanyPK, orgHeaderPK_Buyer));
			AssertEquals("Result should have one row", 1, result.Rows.Count);

			AssertEquals("TEST Address1", result.Rows[0]["GoodsDeliveredTo"]);
			AssertEquals("TEST Address1", result.Rows[0]["DeliveryAddress"]);
		}

		public void TestGoodsDeliveredToAndDeliveryAddress_AttachedToShipment()
		{
			var (glbCompanyPK, orgHeaderPK) = PrepareTestPickupAndDeliveryAddressesWithShipment();

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT * FROM Report_OrderLine ('{0}', '{1}', 'Buyer/Controlling Customer', NULL, NULL, 'N', NULL, '', '', '', '2013-01-01', '2013-01-01')", glbCompanyPK, orgHeaderPK));

			AssertEquals("Result should have one row", 1, result.Rows.Count);
			AssertEquals("Suppliers Address Mismatch", "DeliveryAddress", result.Rows[0]["GoodsDeliveredTo"]);
			AssertEquals("Suppliers Address Mismatch", "DeliveryAddress", result.Rows[0]["DeliveryAddress"]);
		}

		#endregion

		public void TestOrderLinesWithHugeOrderLinePrice_DoNotThrowException()
		{
			var orgHeaderPK_Supplier = Guid.NewGuid();
			var orgAddressPK_Supplier = Guid.NewGuid();
			var glbCompanyPK = Guid.NewGuid();
			var glbBranchPK = Guid.NewGuid();
			var glbDepartmentPK = Guid.NewGuid();
			var orgHeaderPK_Buyer = Guid.NewGuid();
			var orgAddressPK_Buyer = Guid.NewGuid();
			var jobOrderHeaderPK = Guid.NewGuid();

			InsertOrgHeader("TOH00", orgHeaderPK_Supplier);
			InsertOrgAddress(orgAddressPK_Supplier, orgHeaderPK_Supplier, "ADDRESS 1", "TEST400");
			InsertGlbCompany(glbCompanyPK, orgHeaderPK_Supplier);
			InsertGlbBranch(glbBranchPK, glbCompanyPK);
			InsertGlbDepartment(glbDepartmentPK);
			InsertOrgHeader("TOH01", orgHeaderPK_Buyer);
			InsertOrgAddress(orgAddressPK_Buyer, orgHeaderPK_Buyer, "ADDRESS 2", "TEST500");
			InsertJobOrderHeader(jobOrderHeaderPK, Guid.Empty, orgAddressPK_Buyer, Guid.Empty);
			InsertJobDocAddressForOrder_Override(jobOrderHeaderPK, orgAddressPK_Buyer, "GAA");

			for (var lineNo = 1; lineNo <= 10; lineNo++)
			{
				InsertJobOrderLine(jobOrderHeaderPK, SqlMoney.MaxValue, lineNo);
			}

			var result = DataUtils.GetDataTableFromQuery(TestConnection,
				string.Format(CultureInfo.InvariantCulture, "SELECT * FROM Report_OrderLine ('{0}', '{1}', 'Buyer/Controlling Customer', NULL, NULL, 'N', NULL, '', '', '', '2013-01-01', '2013-01-01')", glbCompanyPK, orgHeaderPK_Buyer));
			AssertEquals("Result should have ten rows", 10, result.Rows.Count);
			for (var lineNo = 1; lineNo <= 10; lineNo++)
			{
				AssertEquals("Result should have ten rows", (decimal)SqlMoney.MaxValue * 10, result.Rows[0]["OrderTotalPrice"]);
			}
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

		public void TestOrderLinesWithNotes()
		{
			var note1Pk = Guid.NewGuid();
			var note2Pk = Guid.NewGuid();
			var note3Pk = Guid.NewGuid();
			var note4Pk = Guid.NewGuid();
			var note5Pk = Guid.NewGuid();
			var note6Pk = Guid.NewGuid();

			var orgHeaderPK = Guid.NewGuid();

			var orgAddressPK1 = Guid.NewGuid();
			var orgAddressPK2 = Guid.NewGuid();
			var orgAddressPK3 = Guid.NewGuid();

			var glbCompanyPK = Guid.NewGuid();
			var glbBranchPK = Guid.NewGuid();
			var glbDepartmentPK = Guid.NewGuid();

			var jobOrderHeaderPK1 = Guid.NewGuid();
			var jobOrderHeaderPK2 = Guid.NewGuid();
			var jobOrderHeaderPK3 = Guid.NewGuid();

			var jobOrderLineNo1 = 1;
			var jobOrderLineNo2 = 2;
			var jobOrderLineNo3 = 3;

			InsertOrgHeader("TOH03", orgHeaderPK);
			InsertOrgAddress(orgAddressPK1, orgHeaderPK, "ADDRESS 3", "TEST420");
			InsertOrgAddress(orgAddressPK2, orgHeaderPK, "ADDRESS 4", "TEST421");
			InsertOrgAddress(orgAddressPK3, orgHeaderPK, "ADDRESS 5", "TEST422");

			InsertGlbCompany(glbCompanyPK, orgHeaderPK);
			InsertGlbBranch(glbBranchPK, glbCompanyPK);
			InsertGlbDepartment(glbDepartmentPK);

			InsertJobOrderHeader(jobOrderHeaderPK1, Guid.Empty, orgAddressPK1, Guid.Empty);
			InsertJobOrderLine(jobOrderHeaderPK1, jobOrderLineNo1);

			InsertJobOrderHeader(jobOrderHeaderPK2, Guid.Empty, orgAddressPK2, Guid.Empty);
			InsertJobOrderLine(jobOrderHeaderPK2, jobOrderLineNo2);

			InsertJobOrderHeader(jobOrderHeaderPK3, Guid.Empty, orgAddressPK3, Guid.Empty);
			InsertJobOrderLine(jobOrderHeaderPK3, jobOrderLineNo3);

			TestConnection.ExecuteNonQuery(GetInsertNoteCommand(note1Pk, jobOrderHeaderPK1, "JobOrderHeader", "Order Update History", "From AAA To BBB", "2024-01-02", "2024-01-02"));
			TestConnection.ExecuteNonQuery(GetInsertNoteCommand(note2Pk, jobOrderHeaderPK1, "JobOrderHeader", "Other Note", "TEST01", "2024-01-02", "2024-01-02"));
			TestConnection.ExecuteNonQuery(GetInsertNoteCommand(note3Pk, jobOrderHeaderPK1, "JobOrderHeader", "Order Management Update", "note1", "2024-01-01", "2024-01-01", "AAC", "AAC"));

			TestConnection.ExecuteNonQuery(GetInsertNoteCommand(note4Pk, jobOrderHeaderPK2, "JobOrderHeader", "Order Update History", "From BBB To CCC", "2024-01-03", "2024-01-03"));
			TestConnection.ExecuteNonQuery(GetInsertNoteCommand(note5Pk, jobOrderHeaderPK2, "JobOrderHeader", "Other Note", "TEST02", "2024-01-02", "2024-01-02"));
			TestConnection.ExecuteNonQuery(GetInsertNoteCommand(note6Pk, jobOrderHeaderPK2, "JobOrderHeader", "Order Management Update", "note2", "2024-01-01", "2024-01-01", "AAD", "AAD"));

			var result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT * FROM Report_OrderLine ('{0}', '{1}', 'Buyer/Controlling Customer', NULL, NULL, 'N', NULL, '', '', '', '2013-01-01', '2013-01-01')", glbCompanyPK, orgHeaderPK));

			AssertEquals("it should have selected 3 order lines in total", 3, result.Rows.Count);

			var actualResult = result.Rows.Cast<DataRow>().Select(c => string.Concat(c["LineNumber"].ToString(), "|", c["OrderUpdateHistory"].ToString(), "|", c["OrderManagementUpdate"].ToString()));

			var expectedResult = new[]
			{
				string.Format(@"{0}| AAB Jan  2 2024 12:00AM From AAA To BBB,  AAC Jan  1 2024 12:00AM note1|note1", jobOrderLineNo1),
				string.Format(@"{0}| AAB Jan  3 2024 12:00AM From BBB To CCC,  AAD Jan  1 2024 12:00AM note2|note2", jobOrderLineNo2),
				string.Format(@"{0}||", jobOrderLineNo3)
			};

			AssertContainsExactElementsInAnyOrder(expectedResult, actualResult);
		}

		#endregion

		protected override bool RequiresSchemaBinding => false;

		#region InsertNewTestData

		void InsertOrgHeader(string orgHeadercode, Guid orgHeaderPK)
		{
			var sql = string.Format(
			 @"INSERT INTO dbo.OrgHeader
             ([OH_PK]
             ,[OH_Code]
             ,[OH_FullName])
             VALUES
             ('{0}','{1}','Test Organisation') ", orgHeaderPK, orgHeadercode);

			TestConnection.ExecuteNonQuery(sql);
		}

		void InsertOrgAddress(Guid orgAddressPK, Guid orgHeaderPK, string orgAddress, string code)
		{
			var sql = string.Format(
			 @"INSERT INTO dbo.OrgAddress
			 ([OA_PK]
			 ,[OA_OH]
			 ,[OA_Address1]
			 ,[OA_Code])
             VALUES
             ('{0}','{1}','{2}', '{3}')", orgAddressPK, orgHeaderPK, orgAddress, code);

			TestConnection.ExecuteNonQuery(sql);
		}

		void InsertGlbCompany(Guid glbCompanyPK, Guid orgHeaderPK, string countryCode = "AU", string currencyCode = "AUD")
		{
			var sql = string.Format(
			@"INSERT INTO dbo.GlbCompany
            ([GC_PK]
            ,[GC_Code]
            ,[GC_Name]
            ,[GC_OH_OrgProxy]
			,[GC_RN_NKCountryCode]
			,[GC_RX_NKLocalCurrency])
            VALUES
            ('{0}','TGC', 'AU company','{1}', '{2}', '{3}')", glbCompanyPK, orgHeaderPK, countryCode, currencyCode);

			TestConnection.ExecuteNonQuery(sql);
		}

		void InsertGlbBranch(Guid glbBranchPK, Guid glbCompanyPK)
		{
			var sql = string.Format(
			@"INSERT INTO dbo.GlbBranch 
            ([GB_PK]
            ,[GB_Code]
            ,[GB_GC])
            VALUES
            ('{0}','TGB','{1}')", glbBranchPK, glbCompanyPK);

			TestConnection.ExecuteNonQuery(sql);
		}

		void InsertGlbDepartment(Guid glbDepartmentPK)
		{
			var sql = string.Format(
			@"INSERT INTO dbo.GlbDepartment
            ([GE_PK]
            ,[GE_Code])
            VALUES
            ('{0}','TGE')", glbDepartmentPK);

			TestConnection.ExecuteNonQuery(sql);
		}

		void InsertJobShipment(Guid jobShipmentPK, string origin = "")
		{
			var sql = string.Format("INSERT INTO dbo.JobShipment([JS_PK], [JS_IsCancelled], [JS_RL_NKOrigin]) VALUES('{0}', 0, '{1}')", jobShipmentPK, origin);

			TestConnection.ExecuteNonQuery(sql);
		}

		void InsertJobHeader(Guid glbBranchPK, Guid glbCompanyPK, Guid glbDepartmentPK, Guid jobShipmentPK)
		{
			var sql = string.Format(
			@"INSERT INTO dbo.JobHeader
            ([JH_PK]
            ,[JH_GB]
            ,[JH_GC]
            ,[JH_GE]
            ,[JH_ParentID]
            ,[JH_ParentTableCode]
			,[JH_Status])
            VALUES
            (NewID(),
            '{0}',
            '{1}',
            '{2}',
            '{3}',
            'JS',
			'WRK'
			)", glbBranchPK, glbCompanyPK, glbDepartmentPK, jobShipmentPK);

			TestConnection.ExecuteNonQuery(sql);
		}

		void InsertJobOrderHeader(Guid jobOrderHeaderPK, Guid jobShipmentPK, Guid orgAddressPK_Buyer, Guid orgAddressPK_Supplier)
		{
			var sql = string.Format(
				@"INSERT INTO dbo.JobOrderHeader
				([JD_PK]
				,[JD_JS]
				,[JD_OA_BuyerAddress]
                ,[JD_OA_SupplierAddress]
				,[JD_IsCancelled]
				,[JD_OrderStatus]
				,[JD_SystemCreateTimeUtc]
				,[JD_SystemCreateUser]
				,[JD_IsValid])
				VALUES
				('{0}',
				{1},
				'{2}',
                {3},
				'0',
				'INC',
				'2013-01-01',
				'TGS',
				'1')",
				jobOrderHeaderPK,
				jobShipmentPK == Guid.Empty ? "NULL" : string.Format("'{0}'", jobShipmentPK),
				orgAddressPK_Buyer,
				orgAddressPK_Supplier == Guid.Empty ? "NULL" : string.Format("'{0}'", orgAddressPK_Supplier));

			TestConnection.ExecuteNonQuery(sql);
		}

		void InsertJobOrderLine(Guid jobOrderHeaderPK, int lineNo = 1)
		{
			var sql = string.Format("INSERT INTO dbo.JobOrderLine ([JO_PK], [JO_JD], [JO_LineNo]) VALUES (NEWID(),'{0}', {1})", jobOrderHeaderPK, lineNo);

			TestConnection.ExecuteNonQuery(sql);
		}

		void InsertJobOrderLine(Guid jobOrderHeaderPK, SqlMoney linePrice, int lineNo)
		{
			var sql = string.Format("INSERT INTO dbo.JobOrderLine ([JO_PK], [JO_JD], [JO_LinePrice], [JO_LineNo]) VALUES (NEWID(),'{0}', {1}, {2})", jobOrderHeaderPK, linePrice, lineNo);

			TestConnection.ExecuteNonQuery(sql);
		}

		void InsertProcessTasks(string scheduleDate, string actualDate, string taskType, Guid jobOrderHeaderPK)
		{
			var sql = string.Format(
		   @"INSERT INTO dbo.ProcessTasks
           ([P9_PK]
           ,[P9_Type]
           ,[P9_ScheduledDate]
           ,[P9_ScheduledDateUtc]
           ,[P9_ActualDate]
           ,[P9_ParentID]
           ,[P9_ParentTableCode]
           ,[P9_SE_NKMilestoneEvent])
           VALUES
           (NewID(),
           'MIL',
           '{0}',
           '{0}',
           '{1}',
           '{2}',
           'JD',
           '{3}')", scheduleDate, actualDate, jobOrderHeaderPK, taskType);

			TestConnection.ExecuteNonQuery(sql);
		}

		void InsertConsolAndTransportData(Guid shipmentPK, string origin)
		{
			var consolPK = new Guid();
			var voyagePK = Guid.NewGuid();
			var voyageOriginPK = Guid.NewGuid();
			var voyageDestinationPK = Guid.NewGuid();
			var sailingPK = Guid.NewGuid();
			var transportPK = Guid.NewGuid();

			var sql = @"
INSERT INTO dbo.JobConsol (JK_PK, JK_UniqueConsignRef, JK_RL_NKLoadPort) VALUES(@consolPK, 'C00123456', @origin)

INSERT INTO dbo.JobConShipLink(JN_PK, JN_JK, JN_JS) VALUES(NewID(), @consolPK, @shipmentPK)

INSERT INTO dbo.JobVoyage(JV_PK) VALUES (@voyagePK)

INSERT INTO dbo.JobVoyOrigin(JA_PK, JA_RL_NKPortOfLoading, JA_JV) VALUES(@voyageOriginPK, 'UAODS', @voyagePK)

INSERT INTO dbo.JobVoyDestination(JB_PK, JB_RL_NKPortOfDischarge, JB_JV) VALUES(@voyageDestinationPK, 'AUBNE', @voyagePK)

INSERT INTO dbo.JobSailing(JX_PK, JX_JA, JX_JB) VALUES (@sailingPK, @voyageOriginPK, @voyageDestinationPK)

INSERT INTO dbo.JobConsolTransport (JW_PK, JW_JX, JW_ParentGUID, JW_ParentType) VALUES (@transportPK, @sailingPK, @consolPK, @parentType)
";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@voyagePK", SqlDbType.UniqueIdentifier, voyagePK);
				command.AddParameter("@voyageOriginPK", SqlDbType.UniqueIdentifier, voyageOriginPK);
				command.AddParameter("@voyageDestinationPK", SqlDbType.UniqueIdentifier, voyageDestinationPK);
				command.AddParameter("@sailingPK", SqlDbType.UniqueIdentifier, sailingPK);
				command.AddParameter("@transportPK", SqlDbType.UniqueIdentifier, transportPK);
				command.AddParameter("@consolPK", SqlDbType.UniqueIdentifier, consolPK);
				command.AddParameter("@origin", SqlDbType.VarChar, origin);
				command.AddParameter("@shipmentPK", SqlDbType.UniqueIdentifier, shipmentPK);
				command.AddParameter("@parentType", SqlDbType.VarChar, "CON");
				command.ExecuteNonQuery();
			}
		}

		void InsertJobDocAddressForShipment(Guid jobShipmentPK, Guid orgAddressPK, string addressType)
		{
			var pk = Guid.NewGuid();

			const string sql = @"INSERT INTO dbo.JobDocAddress
(E2_PK, E2_IsValid, E2_AddressType, E2_OA_Address, E2_ParentID, E2_ParentTableCode)
VALUES(@pk, 1, @addressType, @orgAddressPK, @jobShipmentPK, 'JS')";

			using (var command = TestConnection.Command(sql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@addressType", SqlDbType.VarChar, addressType);
				command.AddParameter("@orgAddressPK", SqlDbType.UniqueIdentifier, orgAddressPK);
				command.AddParameter("@jobShipmentPK", SqlDbType.UniqueIdentifier, jobShipmentPK);
				command.ExecuteNonQuery();
			}
		}

		void InsertJobDocAddressForOrder(Guid jobOrderPK, Guid orgAddressPK, string addressType)
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
				command.AddParameter("@orderPK", SqlDbType.UniqueIdentifier, jobOrderPK);
				command.ExecuteNonQuery();
			}
		}

		void InsertJobDocAddressForOrder_Override(Guid orderPK, Guid orgAddressPK, string addressType)
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

		(Guid glbCompanyPK, Guid orgHeaderPK) PrepareGetEstimatedAndActualDateTest(string scheduleDate, string actualDate)
		{
			var orgHeaderPK_Seller = Guid.NewGuid();
			var orgAddressPK_Seller = Guid.NewGuid();
			var glbCompanyPK = Guid.NewGuid();
			var glbBranchPK = Guid.NewGuid();
			var glbDepartmentPK = Guid.NewGuid();

			InsertOrgHeader("TOH00", orgHeaderPK_Seller);
			InsertOrgAddress(orgAddressPK_Seller, orgHeaderPK_Seller, "ADDRESS 1", "TEST300");
			InsertGlbCompany(glbCompanyPK, orgHeaderPK_Seller);
			InsertGlbBranch(glbBranchPK, glbCompanyPK);
			InsertGlbDepartment(glbDepartmentPK);

			var orgHeaderPK_Buyer = Guid.NewGuid();
			var orgAddressPK_Buyer = Guid.NewGuid();
			var jobShipmentPK = Guid.NewGuid();
			var jobOrderHeaderPK = Guid.NewGuid();

			InsertOrgHeader("TOH01", orgHeaderPK_Buyer);
			InsertOrgAddress(orgAddressPK_Buyer, orgHeaderPK_Buyer, "ADDRESS 2", "TEST400");
			InsertJobShipment(jobShipmentPK);
			InsertJobHeader(glbBranchPK, glbCompanyPK, glbDepartmentPK, jobShipmentPK);
			InsertJobOrderHeader(jobOrderHeaderPK, jobShipmentPK, orgAddressPK_Buyer, Guid.Empty);
			InsertJobOrderLine(jobOrderHeaderPK);
			InsertProcessTasks("", actualDate, "CCC", jobOrderHeaderPK);
			InsertProcessTasks(scheduleDate, "", "CLR", jobOrderHeaderPK);

			return (glbCompanyPK, orgHeaderPK_Buyer);
		}

		(Guid glbCompanyPK, Guid orgHeaderPK_Buyer) PrepareTestDataWithConsolAndTransportData()
		{
			var orgHeaderPK = Guid.NewGuid();
			var orgAddressPK = Guid.NewGuid();
			var glbCompanyPK = Guid.NewGuid();
			var glbBranchPK = Guid.NewGuid();
			var glbDepartmentPK = Guid.NewGuid();
			var jobShipmentPK = Guid.NewGuid();
			var jobOrderHeaderPK = Guid.NewGuid();

			InsertOrgHeader("TOH03", orgHeaderPK);
			InsertOrgAddress(orgAddressPK, orgHeaderPK, "ADDRESS 3", "TEST420");
			InsertGlbCompany(glbCompanyPK, orgHeaderPK);
			InsertGlbBranch(glbBranchPK, glbCompanyPK);
			InsertGlbDepartment(glbDepartmentPK);
			InsertJobShipment(jobShipmentPK, "CNSHA");
			InsertJobHeader(glbBranchPK, glbCompanyPK, glbDepartmentPK, jobShipmentPK);
			InsertJobOrderHeader(jobOrderHeaderPK, jobShipmentPK, orgAddressPK, Guid.Empty);
			InsertJobOrderLine(jobOrderHeaderPK);
			InsertConsolAndTransportData(jobShipmentPK, "CNSHA");

			return (glbCompanyPK, orgHeaderPK);
		}

		(Guid glbCompanyPK, Guid orgHeaderPK) PrepareTestPickupAndDeliveryAddressesWithShipment()
		{
			var orgHeaderPK_seller = Guid.NewGuid();
			var orgAddressPK_seller = Guid.NewGuid();
			var orgHeaderPK_buyer = Guid.NewGuid();
			var orgAddressPK_buyer = Guid.NewGuid();
			var orgHeaderPK_consignor = Guid.NewGuid();
			var orgAddressPK_Delivery = Guid.NewGuid();
			var orgHeaderPK_consignee = Guid.NewGuid();
			var orgAddressPK_Pickup = Guid.NewGuid();
			var glbCompanyPK = Guid.NewGuid();
			var glbBranchPK = Guid.NewGuid();
			var glbDepartmentPK = Guid.NewGuid();
			var jobShipmentPK = Guid.NewGuid();
			var jobOrderHeaderPK = Guid.NewGuid();

			InsertOrgHeader("TOH04", orgHeaderPK_seller);
			InsertOrgAddress(orgAddressPK_seller, orgHeaderPK_seller, "ADDRESS 400", "ADDR400");
			InsertGlbCompany(glbCompanyPK, orgHeaderPK_seller);
			InsertGlbBranch(glbBranchPK, glbCompanyPK);
			InsertGlbDepartment(glbDepartmentPK);
			InsertOrgHeader("TOH05", orgHeaderPK_buyer);
			InsertOrgAddress(orgAddressPK_buyer, orgHeaderPK_buyer, "ADDRESS 500", "ADDR500");
			InsertOrgHeader("TOH06", orgHeaderPK_consignor);
			InsertOrgAddress(orgAddressPK_Delivery, orgHeaderPK_consignor, "DeliveryAddress", "ADDRDEL");
			InsertOrgHeader("TOH07", orgHeaderPK_consignee);
			InsertOrgAddress(orgAddressPK_Pickup, orgHeaderPK_consignee, "PickupAddress", "ADDRPIC");
			InsertJobShipment(jobShipmentPK);
			InsertJobDocAddressForShipment(jobShipmentPK, orgAddressPK_Delivery, "CEG");
			InsertJobDocAddressForShipment(jobShipmentPK, orgAddressPK_Pickup, "CRG");
			InsertJobHeader(glbBranchPK, glbCompanyPK, glbDepartmentPK, jobShipmentPK);
			InsertJobOrderHeader(jobOrderHeaderPK, jobShipmentPK, orgAddressPK_buyer, orgAddressPK_seller);
			InsertJobOrderLine(jobOrderHeaderPK);

			return (glbCompanyPK, orgHeaderPK_buyer);
		}
		#endregion
	}
}

