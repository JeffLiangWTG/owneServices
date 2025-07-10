using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(Report_WorkflowExceptions))]
	class Report_WorkflowExceptionsTest : DbCreateScriptTest
	{
		#region Customer Service Tickets

		public void TestCustomerServiceTickets()
		{
			var workRequestPK = Guid.NewGuid();

			TestConnection.ExecuteScalar($@"
insert into dbo.WorkRequest(WKR_PK, WKR_RequestNumber, WKR_Status, WKR_Summary, WKR_SystemCreateUser, WKR_SystemLastEditUser, WKR_OC_Client, WKR_SystemCreateTimeUtc, WKR_SystemLastEditTimeUtc, WKR_GB_Branch) values('{workRequestPK}', 'CST00000001', 'OPN', 'Rake your acquaintance', 'E', 'E', (SELECT TOP 1 OC_PK FROM dbo.OrgContact), '2018-06-19 00:00:00', '2018-06-19 00:00:00', '{branchPK}');
insert into dbo.ProcessTasks(P9_PK, P9_ParentID, P9_ParentTableCode, P9_Type, P9_ActualDate) values(NEWID(), '{workRequestPK}', 'WKR', 'EXC', '2018-06-19 00:00:00');"
				);

			DataTable report = DataUtils.GetDataTableFromQuery(TestConnection, "select JobModuleID, JobModulePK, JobNumber, AdditionalDetail from Report_WorkflowExceptions('" + companyPK + "','2018-06-18 00:00:00','2018-06-20 00:00:00')");

			AssertEquals("Result should have rows", 1, report.Rows.Count);

			var row = report.Rows[0];
			AssertEquals("row['JobModuleID']", "CustomerServiceTicket", row["JobModuleID"]);
			AssertEquals("row['JobModulePK']", workRequestPK, row["JobModulePK"]);
			AssertEquals("row['JobNumber']", "CST00000001", row["JobNumber"]);
			AssertEquals("row['AdditionalDetail']", "Rake your acquaintance", row["AdditionalDetail"]);
		}

		#endregion

		#region GlbGroup

		public void TestGlbGroup()
		{
			var groupPK = Guid.NewGuid();
			var triggerPK = Guid.NewGuid();

			TestConnection.ExecuteScalar(FormattableString.Invariant($@"
INSERT dbo.GlbGroup (
	GG_PK, GG_Code, GG_Desc
) VALUES (
	'{groupPK}', 'AAA', 'Aaaalex'
)

INSERT dbo.ProcessTasks (
	P9_PK, P9_TaskID, P9_ParentID, P9_ParentTableCode, P9_ActualDate, P9_Type
) VALUES (
	'{triggerPK}', 'Nopedy Nope', '{groupPK}', 'GG', '2018-06-19 00:00:00', 'EXC'
)
"));

			using (var report = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT WorkflowType, TaskID, JobModuleID, JobModulePK, JobNumber, AdditionalDetail from Report_WorkflowExceptions('" + companyPK + "','2018-06-18 00:00:00','2018-06-20 00:00:00')"))
			{
				AssertEquals(1, report.Rows.Count);

				var row = report.Rows[0];

				AssertEquals("JobNumber", "GRP", row["WorkflowType"]);
				AssertEquals("JobNumber", "Nopedy Nope", row["TaskID"]);
				AssertEquals("JobModuleID", "GlbGroup", row["JobModuleID"]);
				AssertEquals("JobModulePK", groupPK, row["JobModulePK"]);
				AssertEquals("JobNumber", "AAA", row["JobNumber"]);
				AssertEquals("AdditionalDetail", "Aaaalex", row["AdditionalDetail"]);
			}
		}

		#endregion

		#region Forwarding, Agency and CFS Shipments

		const string AgencyBillOfLadingModuleID = "AgencyBillOfLading";
		const string AgencyBookingModuleID = "AgencyBooking";
		const string ShipmentReceivalModuleID = "ShipmentReceival";
		const string AgencyBillOfLadingCode = "BOL";
		const string AgencyBookingCode = "BKN";
		const string CFSShipmentCode = "CSH";

		public void TestBillOfLadingWithWFIStatusExceptions()
		{
			TestShipmentWorkflowExceptionsReport(AgencyBillOfLadingModuleID, AgencyBillOfLadingCode, 1, 0, 0, "WFI");
		}

		public void TestBillOfLadingWithCNFStatusExceptions()
		{
			TestShipmentWorkflowExceptionsReport(AgencyBillOfLadingModuleID, AgencyBillOfLadingCode, 1, 0, 0, "CNF");
		}

		public void TestAgencyBookingWithWEBStatusExceptions()
		{
			TestShipmentWorkflowExceptionsReport(AgencyBookingModuleID, AgencyBookingCode, 1, 0, 0, "WEB");
		}

		public void TestAgencyBookingWithBKDStatusExceptions()
		{
			TestShipmentWorkflowExceptionsReport(AgencyBookingModuleID, AgencyBookingCode, 1, 0, 0, "BKD");
		}

		public void TestAgencyBookingWithWTLStatusExceptions()
		{
			TestShipmentWorkflowExceptionsReport(AgencyBookingModuleID, AgencyBookingCode, 1, 0, 0, "WTL");
		}

		public void TestCFSShipmentExceptions()
		{
			TestShipmentWorkflowExceptionsReport(ShipmentReceivalModuleID, CFSShipmentCode, 0, 1, 0);
		}

		void TestShipmentWorkflowExceptionsReport(string expectedModuleID, string workflowType, int isShipping, int isCFSRegistered, int isForwardRegistered, string shipmentStatus = "")
		{
			string sqlText = string.Format(@"
insert into dbo.GlbCompany(GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency)
	values('CB15F7F4-1815-4398-A615-5935D78E46F5', 'DAN', 'AU company', 'AU', 'AUD')

insert into dbo.GlbBranch(GB_PK, GB_GC)
	values('53BB75D5-0AE0-4522-B05D-98999BBF88EC', 'CB15F7F4-1815-4398-A615-5935D78E46F5')

insert into dbo.JobVoyage(JV_PK)
	values('BBBF7E08-4F52-4740-B33A-928204F0D348')

insert into dbo.JobVoyOrigin(JA_PK, JA_RL_NKPortOfLoading, JA_JV)
	values('86110556-D349-42E1-B786-FC8EE6656FEA', 'UAODS', 'BBBF7E08-4F52-4740-B33A-928204F0D348')

insert into dbo.JobVoyDestination(JB_PK, JB_RL_NKPortOfDischarge, JB_JV)
	values('9EB67A57-A341-44E6-9945-441710C57386', 'AUBNE', 'BBBF7E08-4F52-4740-B33A-928204F0D348')

insert into dbo.JobSailing(JX_PK, JX_JA, JX_JB)
	values ('F1316DDF-0627-4262-849B-2D216BCDEA5B', '86110556-D349-42E1-B786-FC8EE6656FEA', '9EB67A57-A341-44E6-9945-441710C57386')

insert into dbo.JobShipment(JS_PK, JS_TransportMode, JS_PackingMode, JS_RL_NKOrigin, JS_RL_NKDestination, JS_IsShipping, JS_UniqueConsignRef, JS_HouseBill, JS_E_ARV, JS_JX, JS_ShipmentStatus, JS_IsCFSRegistered, JS_IsForwardRegistered)
	values('A1AC3605-0E85-42A0-867D-16B246E1838A', 'AIR', 'LCL', 'AUSYD', 'USLAX', {0}, 'S00001000', 'S00001000', '2014-03-01 00:00:00', 'F1316DDF-0627-4262-849B-2D216BCDEA5B', '{1}', {2}, {3})

insert into dbo.OrgHeader(OH_PK, OH_Code)
	values ('1B4F6841-FEB6-45CB-86B9-B82D40FFD19D', 'BENMEATEE')

insert into dbo.OrgHeader(OH_PK, OH_Code)
	values ('7AC276FF-394F-4CDC-BD3A-9509F66B9798', 'BENMEATER')

insert into dbo.OrgAddress(OA_PK, OA_OH, OA_Address1)
	values('81B952F9-D280-4C53-88C5-C321082D56B2', '1B4F6841-FEB6-45CB-86B9-B82D40FFD19D', 'Consignee')

insert into dbo.OrgAddress(OA_PK, OA_OH, OA_Address1)
	values('4790E772-359C-4457-A945-F721D5EE39C8', '7AC276FF-394F-4CDC-BD3A-9509F66B9798', 'Consignor')

insert into dbo.JobDocAddress(E2_PK, E2_ParentID, E2_ParentTableCode, E2_AddressType, E2_OA_Address)
	values(newid(), 'A1AC3605-0E85-42A0-867D-16B246E1838A', 'JS', 'CEG', '81B952F9-D280-4C53-88C5-C321082D56B2')

insert into dbo.JobDocAddress(E2_PK, E2_ParentID, E2_ParentTableCode, E2_AddressType, E2_OA_Address)
	values(newid(), 'A1AC3605-0E85-42A0-867D-16B246E1838A', 'JS', 'CRG', '4790E772-359C-4457-A945-F721D5EE39C8')

insert into dbo.ProcessTasks(P9_PK, P9_ParentID, P9_ParentTableCode, P9_Type, P9_ActualDate)
	values(NEWID(), 'A1AC3605-0E85-42A0-867D-16B246E1838A', 'CM', 'EXC', '2014-08-01 00:00:00')", isShipping, shipmentStatus, isCFSRegistered, isForwardRegistered);
			TestConnection.ExecuteScalar(sqlText);

			var report = DataUtils.GetDataTableFromQuery(TestConnection, @"
select PK, WorkflowType, ConsigneePK, ConsignorPK, JobNumber, OriginPort, DestinationPort, LoadPort, DischargePort, ContainerMode, AdditionalDetail, TransportMode, JobModuleID, JobModulePK, ETA, OrderNumber
from Report_WorkflowExceptions('CB15F7F4-1815-4398-A615-5935D78E46F5','2014-01-01 00:00:00','2014-08-02 00:00:00')");

			AssertEquals("Matching Row Count", 1, report.Rows.Count);
			var row = report.Rows[0];

			CombineAssertions(delegate
			{
				AssertEquals("row['PK']", new Guid("A1AC3605-0E85-42A0-867D-16B246E1838A"), row["PK"]);
				AssertEquals("row['WorkflowType']", workflowType, row["WorkflowType"]);
				AssertEquals("row['ConsigneePK']", new Guid("1B4F6841-FEB6-45CB-86B9-B82D40FFD19D"), row["ConsigneePK"]);
				AssertEquals("row['ConsignorPK']", new Guid("7AC276FF-394F-4CDC-BD3A-9509F66B9798"), row["ConsignorPK"]);
				AssertEquals("row['JobNumber']", "S00001000", row["JobNumber"]);
				AssertEquals("row['OriginPort']", "AUSYD", row["OriginPort"]);
				AssertEquals("row['DestinationPort']", "USLAX", row["DestinationPort"]);
				AssertEquals("row['LoadPort']", "UAODS", row["LoadPort"]);
				AssertEquals("row['DischargePort']", "AUBNE", row["DischargePort"]);
				AssertEquals("row['ContainerMode']", "LCL", row["ContainerMode"]);
				AssertEquals("row['AdditionalDetail']", "HouseBill: S00001000", row["AdditionalDetail"]);
				AssertEquals("row['TransportMode']", "AIR", row["TransportMode"]);
				AssertEquals("row['JobModuleID']", expectedModuleID, row["JobModuleID"]);
				AssertEquals("row['JobModulePK']", new Guid("A1AC3605-0E85-42A0-867D-16B246E1838A"), row["JobModulePK"]);
				AssertEquals("row['ETA']", new DateTime(2014, 03, 01, 0, 0, 0), row["ETA"]);
				AssertEquals("row['OrderNumber']", DBNull.Value, row["OrderNumber"]);
			});
		}

		#endregion

		#region Air Cargo

		public void TestAirCargoAU()
		{
			TestAirCargoFor("AU", "AUCustomsAirCargo");
		}

		public void TestAirCargoNZ()
		{
			TestAirCargoFor("NZ", "ExpressECI");
		}

		public void TestAirCargoFor(string countryCode, string jobModuleID)
		{
			TestConnection.ExecuteScalar(@"
insert into dbo.GlbCompany(GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency)
	values('CB15F7F4-1815-4398-A615-5935D78E46F5', 'DAN', 'AU company', '" + countryCode + @"', 'AUD')
insert into dbo.GlbBranch(GB_PK, GB_GC)
	values('53BB75D5-0AE0-4522-B05D-98999BBF88EC', 'CB15F7F4-1815-4398-A615-5935D78E46F5')

insert into dbo.OrgHeader(OH_PK, OH_Code)
	values ('1B4F6841-FEB6-45CB-86B9-B82D40FFD19D', 'BENMEATEE')
insert into dbo.CusMAWB(CM_PK, CM_OH_ResponsibleParty, CM_MAWB, CM_RL_NKLoadPort, CM_RL_NKDischargePort, CM_ArrivalDate, CM_ResponsiblePartyID, CM_MasterHouseBill, CM_ApplicationCode, CM_GB)
	values('5D0D7F29-8F45-4AFA-BF57-D301E896F876', '1B4F6841-FEB6-45CB-86B9-B82D40FFD19D', '081-12333333', 'NZAKL', 'AUSYD', '2012-11-06 00:00:00', 'Order Number', 'LORD OF THE SITH', 'CMR', '53BB75D5-0AE0-4522-B05D-98999BBF88EC');
insert into dbo.ProcessTasks(P9_PK, P9_ParentID, P9_ParentTableCode, P9_Type, P9_ActualDate)
	values(NEWID(), '5D0D7F29-8F45-4AFA-BF57-D301E896F876', 'CM', 'EXC', '2011-08-01 00:00:00');"
				);

			var report = DataUtils.GetDataTableFromQuery(TestConnection, @"
select PK, WorkflowType, ConsigneePK, ConsignorPK, JobNumber, OriginPort, DestinationPort, LoadPort, DischargePort, ContainerMode, AdditionalDetail, TransportMode, JobModuleID, JobModulePK, ETA, OrderNumber
from Report_WorkflowExceptions('CB15F7F4-1815-4398-A615-5935D78E46F5','2011-01-01 00:00:00','2011-08-02 00:00:00')");

			AssertEquals("Matching Row Count", 1, report.Rows.Count);
			var row = report.Rows[0];

			CombineAssertions(delegate
			{
				AssertEquals("row['PK']", new Guid("5D0D7F29-8F45-4AFA-BF57-D301E896F876"), row["PK"]);
				AssertEquals("row['WorkflowType']", "ACR", row["WorkflowType"]);
				AssertEquals("row['ConsigneePK']", new Guid("1B4F6841-FEB6-45CB-86B9-B82D40FFD19D"), row["ConsigneePK"]);
				AssertEquals("row['ConsignorPK']", DBNull.Value, row["ConsignorPK"]);
				AssertEquals("row['JobNumber']", "081-12333333", row["JobNumber"]);
				AssertEquals("row['OriginPort']", DBNull.Value, row["OriginPort"]);
				AssertEquals("row['DestinationPort']", DBNull.Value, row["DestinationPort"]);
				AssertEquals("row['LoadPort']", "NZAKL", row["LoadPort"]);
				AssertEquals("row['DischargePort']", "AUSYD", row["DischargePort"]);
				AssertEquals("row['ContainerMode']", "LSE", row["ContainerMode"]);
				AssertEquals("row['AdditionalDetail']", "LORD OF THE SITH", row["AdditionalDetail"]);
				AssertEquals("row['TransportMode']", "AIR", row["TransportMode"]);
				AssertEquals("row['JobModuleID']", jobModuleID, row["JobModuleID"]);
				AssertEquals("row['JobModulePK']", new Guid("5D0D7F29-8F45-4AFA-BF57-D301E896F876"), row["JobModulePK"]);
				AssertEquals("row['ETA']", new DateTime(2012, 11, 06, 0, 0, 0), row["ETA"]);
				AssertEquals("row['OrderNumber']", "Order Number", row["OrderNumber"]);
			});
		}

		public void TestHouseAirCargo()
		{
			TestConnection.ExecuteScalar(@"
insert into dbo.GlbCompany(GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency)
	values('CB15F7F4-1815-4398-A615-5935D78E46F5', 'DAN', 'AU company', 'AU', 'AUD')
insert into dbo.GlbBranch(GB_PK, GB_GC)
	values('53BB75D5-0AE0-4522-B05D-98999BBF88EC', 'CB15F7F4-1815-4398-A615-5935D78E46F5')

INSERT INTO dbo.OrgHeader(OH_PK, OH_Code) VALUES ('1B4F6841-FEB6-45CB-86B9-B82D40FFD19D', 'BENMEATEE')
insert into dbo.OrgAddress(OA_PK, OA_OH, OA_Code, OA_Address1) values ('9AABB41B-AA09-441F-904B-A3E2691857F8', '1B4F6841-FEB6-45CB-86B9-B82D40FFD19D', 'BENADDRESS', 'BENADDR1')
INSERT INTO dbo.OrgHeader(OH_PK, OH_Code) VALUES ('6077994D-5401-480D-B327-57E01D9855C5', 'BOBBUILDER')
insert into dbo.OrgAddress(OA_PK, OA_OH, OA_Code, OA_Address1) values ('251EE892-4D71-4F1C-BCAC-23E6F7E02A87', '6077994D-5401-480D-B327-57E01D9855C5', 'BOBADDRESS', 'BOBADDR1')
INSERT INTO dbo.CusMAWB (CM_PK, CM_ApplicationCode, CM_RL_NKLoadPort, CM_RL_NKDischargePort, CM_ArrivalDate, CM_GB, CM_MAWB)
	VALUES ('FE1A863C-73C6-490B-B873-C43E35C3E659', 'CMR', 'NZAKL', 'AUSYD', '2012-11-06 00:00:00', '53BB75D5-0AE0-4522-B05D-98999BBF88EC', 'AB001')
INSERT INTO dbo.CusHAWB (CS_PK, CS_CM, CS_ApplicationCode, CS_OA_ConsigneeAddress, CS_OA_ConsignorAddress, CS_HAWB, CS_RL_NKOrigin, CS_RL_NKDestination, CS_MasterHouseBill, CS_ResponsiblePartyID, CS_MessageReference)
	VALUES ('5D0D7F29-8F45-4AFA-BF57-D301E896F876', 'FE1A863C-73C6-490B-B873-C43E35C3E659', 'CMR', '9AABB41B-AA09-441F-904B-A3E2691857F8', '251EE892-4D71-4F1C-BCAC-23E6F7E02A87', 'HB23423', 'NZCHC', 'AUMEL', 'MHB2343', 'ORDER 1', 'A001');
INSERT INTO dbo.ProcessTasks (P9_PK, P9_ParentID, P9_ParentTableCode, P9_Type, P9_ActualDate) VALUES (NEWID(), '5D0D7F29-8F45-4AFA-BF57-D301E896F876', 'CS', 'EXC', '2011-08-01 00:00:00');"
				);

			var report = DataUtils.GetDataTableFromQuery(TestConnection, @"
SELECT PK, WorkflowType, ConsigneePK, ConsignorPK, JobNumber, OriginPort, DestinationPort, LoadPort, DischargePort, ContainerMode, AdditionalDetail, TransportMode, JobModuleID, JobModulePK, ETA, OrderNumber
FROM Report_WorkflowExceptions('CB15F7F4-1815-4398-A615-5935D78E46F5','2011-01-01 00:00:00','2011-08-02 00:00:00')");

			AssertEquals("Matching Row Count", 1, report.Rows.Count);
			var row = report.Rows[0];

			CombineAssertions(delegate
			{
				AssertEquals("row['PK']", new Guid("5D0D7F29-8F45-4AFA-BF57-D301E896F876"), row["PK"]);
				AssertEquals("row['WorkflowType']", "HAC", row["WorkflowType"]);
				AssertEquals("row['ConsigneePK']", new Guid("1B4F6841-FEB6-45CB-86B9-B82D40FFD19D"), row["ConsigneePK"]);
				AssertEquals("row['ConsignorPK']", new Guid("6077994D-5401-480D-B327-57E01D9855C5"), row["ConsignorPK"]);
				AssertEquals("row['JobNumber']", "A001", row["JobNumber"]);
				AssertEquals("row['OriginPort']", "NZCHC", row["OriginPort"]);
				AssertEquals("row['DestinationPort']", "AUMEL", row["DestinationPort"]);
				AssertEquals("row['LoadPort']", "NZAKL", row["LoadPort"]);
				AssertEquals("row['DischargePort']", "AUSYD", row["DischargePort"]);
				AssertEquals("row['ContainerMode']", "LSE", row["ContainerMode"]);
				AssertEquals("row['AdditionalDetail']", "AB001/HB23423", row["AdditionalDetail"]);
				AssertEquals("row['TransportMode']", "AIR", row["TransportMode"]);
				AssertEquals("row['JobModuleID']", "AUCustomsHouseAirCargo", row["JobModuleID"]);
				AssertEquals("row['JobModulePK']", new Guid("5D0D7F29-8F45-4AFA-BF57-D301E896F876"), row["JobModulePK"]);
				AssertEquals("row['ETA']", new DateTime(2012, 11, 06, 0, 0, 0), row["ETA"]);
				AssertEquals("row['OrderNumber']", "ORDER 1", row["OrderNumber"]);
			});
		}

		#endregion

		#region Warehouse

		public void TestWhsWorkOrder()
			=> TestWhsWorkOrder(docketType: "WOR", workflowType: "WWO", jobModuleID: "WhsWorkOrder", hasConsignee: true);

		public void TestWhsWorkOrder_NullConsignee()
			=> TestWhsWorkOrder(docketType: "WOR", workflowType: "WWO", jobModuleID: "WhsWorkOrder", hasConsignee: false);

		public void TestWhsDynamicWorkOrder()
			=> TestWhsWorkOrder(docketType: "DWO", workflowType: "WDO", jobModuleID: "WhsDynamicWorkOrder", hasConsignee: true);

		public void TestDynamicWhsWorkOrder_NullConsignee()
			=> TestWhsWorkOrder(docketType: "DWO", workflowType: "WDO", jobModuleID: "WhsDynamicWorkOrder", hasConsignee: false);

		void TestWhsWorkOrder(
			string docketType,
			string workflowType,
			string jobModuleID,
			bool hasConsignee)
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			using (connection.BeginTransactionWithManager())
			{
				var sql = new SqlQueryBuilder();
				var whs = new WhsWarehouse("WH1", branchPK).WithDockDoor(connection);

				var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
				var consignee = new OrgHeader("CNE").AppendInsertAndReturnObject(sql);
				var consigneeAddress = new OrgAddress(consignee, "CNE", "123 Fake Street").AppendInsertAndReturnObject(sql);
				var part = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

				var docketA = new WhsDocket(client.PK, whs.PK, docketType, "ASS", "ENT", "DocketA", "EXT").AppendInsertAndReturnObject(sql);

				if (hasConsignee)
				{
					var consigneeJda = new JobDocAddress(docketA.PK, "WD", "CEA") { E2_OA_Address = consigneeAddress.PK }.AppendInsertAndReturnObject(sql);
				}

				var processTask = new ProcessTasks(docketA.PK, WhsDocketSchema.Constants.Prefix)
				{
					P9_Type = "EXC",
					P9_ActualDate = new DateTime(2013, 01, 31)
				}.AppendInsertAndReturnObject(sql);

				connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

				var report = DataUtils.GetDataTableFromQuery(connection, $"SELECT * FROM Report_WorkflowExceptions('{companyPK}','2013-01-01 00:00:00','2013-02-01 00:00:00')");

				AssertEquals("Matching Row Count", 1, report.Rows.Count);
				var row = report.Rows[0];

				CombineAssertions(() =>
				{
					AssertEquals("row['PK']", docketA.PK, row["PK"]);
					AssertEquals("row['WorkflowType']", workflowType, row["WorkflowType"]);
					AssertEquals("row['ConsigneePK']", hasConsignee ? consignee.PK : DBNull.Value, row["ConsigneePK"]);
					AssertEquals("row['ConsignorPK']", client.PK, row["ConsignorPK"]);
					AssertEquals("row['JobNumber']", "DocketA", row["JobNumber"]);
					AssertEquals("row['OriginPort']", DBNull.Value, row["OriginPort"]);
					AssertEquals("row['DestinationPort']", DBNull.Value, row["DestinationPort"]);
					AssertEquals("row['LoadPort']", DBNull.Value, row["LoadPort"]);
					AssertEquals("row['DischargePort']", DBNull.Value, row["DischargePort"]);
					AssertEquals("row['ContainerMode']", DBNull.Value, row["ContainerMode"]);
					AssertEquals("row['AdditionalDetail']", DBNull.Value, row["AdditionalDetail"]);
					AssertEquals("row['TransportMode']", DBNull.Value, row["TransportMode"]);
					AssertEquals("row['JobModuleID']", jobModuleID, row["JobModuleID"]);
					AssertEquals("row['JobModulePK']", docketA.PK, row["JobModulePK"]);
					AssertEquals("row['ETA']", DBNull.Value, row["ETA"]);
					AssertEquals("row['OrderNumber']", "EXT", row["OrderNumber"]);
				});
			}
		}

		public void TestWhsLoad()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			using (connection.BeginTransactionWithManager())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(connection);
				var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(connection);
				var areaDDL = new WhsArea(whs.PK, "DDLArea").AppendInsertAndReturnObject(sql);
				var rowDDL = new WhsRow(whs, "DDL").AppendInsertAndReturnObject(sql);
				var ddlLocationType = WhsLocationType.ShallowLoadFromDB(connection, lt => lt.WLT_LocationClass == "DDL").First();
				var dockDoorLocation = new WhsLocation(rowDDL.PK, areaDDL.PK, areaDDL.PK, ddlLocationType.PK).AppendInsertAndReturnObject(sql);

				var transportCompany = new OrgHeader("TRANSPORT").AppendInsertAndReturnObject(sql);
				var load = new WhsLoad("WL01", "STD", transportCompany.PK, dockDoorLocation.PK).AppendInsertAndReturnObject(sql);

				var processTask = new ProcessTasks(load.PK, WhsLoadSchema.Constants.Prefix)
				{
					P9_Type = "EXC",
					P9_ActualDate = new DateTime(2013, 01, 31)
				}.AppendInsertAndReturnObject(sql);

				connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

				var report = DataUtils.GetDataTableFromQuery(connection, $"SELECT * FROM Report_WorkflowExceptions('{companyPK}','2013-01-01 00:00:00','2013-02-01 00:00:00')");

				AssertEquals("Matching Row Count", 1, report.Rows.Count);
				var row = report.Rows[0];

				CombineAssertions(() =>
				{
					AssertEquals("row['PK']", load.PK, row["PK"]);
					AssertEquals("row['WorkflowType']", "WLO", row["WorkflowType"]);
					AssertEquals("row['ConsigneePK']", DBNull.Value, row["ConsigneePK"]);
					AssertEquals("row['ConsignorPK']", DBNull.Value, row["ConsignorPK"]);
					AssertEquals("row['JobNumber']", "WL01", row["JobNumber"]);
					AssertEquals("row['OriginPort']", DBNull.Value, row["OriginPort"]);
					AssertEquals("row['DestinationPort']", DBNull.Value, row["DestinationPort"]);
					AssertEquals("row['LoadPort']", DBNull.Value, row["LoadPort"]);
					AssertEquals("row['DischargePort']", DBNull.Value, row["DischargePort"]);
					AssertEquals("row['ContainerMode']", DBNull.Value, row["ContainerMode"]);
					AssertEquals("row['AdditionalDetail']", DBNull.Value, row["AdditionalDetail"]);
					AssertEquals("row['TransportMode']", DBNull.Value, row["TransportMode"]);
					AssertEquals("row['JobModuleID']", "WhsLoad", row["JobModuleID"]);
					AssertEquals("row['JobModulePK']", load.PK, row["JobModulePK"]);
					AssertEquals("row['ETA']", DBNull.Value, row["ETA"]);
					AssertEquals("row['OrderNumber']", DBNull.Value, row["OrderNumber"]);
				});
			}
		}

		#endregion

		#region Work flow type QBK: Quoted Booking
		public void TestQuotedBooking()
		{
			for (var i = 0; i < 10; i++)
			{
				var vJS_UniqueConsignRef = Guid.NewGuid().ToString("N").Substring(0, 10);
				var sql = @$"
DECLARE @shipment_JS_PK UNIQUEIDENTIFIER
SET @shipment_JS_PK = NEWID()

DELETE FROM dbo.JobShipment WHERE JS_PK = @shipment_JS_PK OR JS_UniqueConsignRef = '{vJS_UniqueConsignRef}'
INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_IsBooking, JS_E_DEP, JS_E_ARV, JS_TransportMode, JS_RL_NKOrigin, JS_RL_NKDestination) 
VALUES (@shipment_JS_PK, '{vJS_UniqueConsignRef}', 1, '2023-03-01', '2023-04-01', 'AIR', 'AU', 'IN')

DECLARE @ProcessTasks_P9_PK UNIQUEIDENTIFIER
SET @ProcessTasks_P9_PK = NEWID()
DELETE FROM ProcessTasks WHERE P9_PK = @ProcessTasks_P9_PK
INSERT INTO ProcessTasks(P9_PK, P9_ParentID, P9_Type, P9_ParentTableCode, P9_Notes,P9_ActualDate, P9_SystemCreateTimeUtc, P9_SystemCreateUser, P9_SystemLastEditTimeUtc, P9_SystemLastEditUser, P9_Description) 
VALUES (@ProcessTasks_P9_PK, @shipment_JS_PK, 'EXC', 'JE', 0x0123456789, GetUtcDate(), GetUtcDate(), '~BP', GetUtcDate(), '~BP', 'Quoted Booking Test')
SELECT P9_Type, * FROM ProcessTasks WHERE P9_PK = @ProcessTasks_P9_PK
";

				_ = TestConnection.ExecuteScalar(sql);
			}

			var dataTableFromModule = DataUtils.GetDataTableFromQuery(TestConnection, @"
SELECT *
FROM ProcessTasks
WHERE
(
	P9_ParentID IN 
	(
		SELECT VB_PK FROM dbo.ViewQuotedBooking
	)
)
AND
P9_Type = 'EXC'
");
			var dataTableFromReport = DataUtils.GetDataTableFromQuery(TestConnection, @"
SELECT *
FROM Report_WorkflowExceptions(NEWID(), '', '') 
WHERE (WorkflowType = 'QBK') 
ORDER BY P9_ActualDate, JobNumber, EventCode 
");
			var pksModule = dataTableFromModule.AsEnumerable().Select(row => $"{row[0]}").OrderBy(x => x).ToList();
			var pksReport = dataTableFromReport.AsEnumerable().Select(row => $"{row[0]}").OrderBy(x => x).ToList();

			AssertEquals("Result from module and report should equal", expected: true, pksModule.SequenceEqual(pksReport));
		}
		#endregion

		public void TestCartageModuleID()
		{
			var helper = new TestDbHelper(TestConnection);
			var branchPK = (Guid)TestConnection.ExecuteScalar("SELECT TOP 1 GB_PK FROM dbo.GlbBranch");
			var jobCartagePK = helper.InsertJobCartage("CARTAGE1", branchPK);
			var datetime = helper.ToDate("2016-01-01");
			helper.InsertProcessTasks(jobCartagePK, "JJ", "EXC", datetime);

			DataTable report = DataUtils.GetDataTableFromQuery(TestConnection, "select JobModuleID, JobModulePK from Report_WorkflowExceptions('" + companyPK + "','2016-01-01 00:00:00','2016-08-02 00:00:00')");

			AssertEquals("Result should have rows", 1, report.Rows.Count);

			var row = report.Rows[0];
			AssertEquals("row['JobModuleID']", "Cartage", row["JobModuleID"]);
			AssertEquals("row['JobModulePK']", jobCartagePK, row["JobModulePK"]);
		}

		public void TestCartageLegModuleID()
		{
			var helper = new TestDbHelper(TestConnection);
			var branchPK = (Guid)TestConnection.ExecuteScalar("SELECT TOP 1 GB_PK FROM dbo.GlbBranch");
			var jobContainerPK = helper.InsertJobContainer();
			var jobCartagePK = helper.InsertJobCartage("T1", branchPK);
			var jobBookedCtgMovePK = helper.InsertJobBookedCtgMove(jobCartagePK, jobContainerPK);
			var containerLegPK = helper.InsertJobContainerLegs(jobBookedCtgMovePK);
			var datetime = helper.ToDate("2016-01-01");
			helper.InsertProcessTasks(containerLegPK, "JJ", "EXC", datetime);

			DataTable report = DataUtils.GetDataTableFromQuery(TestConnection, "select JobModuleID, JobModulePK from Report_WorkflowExceptions('" + companyPK + "','2016-01-01 00:00:00','2016-08-02 00:00:00')");

			AssertEquals("Result should have rows", 1, report.Rows.Count);

			var row = report.Rows[0];
			AssertEquals("row['JobModuleID']", "CartageLeg", row["JobModuleID"]);
			AssertEquals("row['JobModulePK']", containerLegPK, row["JobModulePK"]);
		}

		public void TestTransportBookingModuleID()
		{
			var helper = new TestDbHelper(TestConnection);
			var branchPK = (Guid)TestConnection.ExecuteScalar("SELECT TOP 1 GB_PK FROM dbo.GlbBranch");
			var dtbBookingConsolidationPK = helper.InsertDtbBookingConsolidation();
			var bookingPK = helper.InsertDtbBooking("Test", dtbBookingConsolidationPK, branchPK);
			helper.InsertProcessTasks(bookingPK, "KM", "EXC", new DateTime(2011, 8, 1));

			DataTable report = DataUtils.GetDataTableFromQuery(TestConnection, "select JobModuleID, JobModulePK from Report_WorkflowExceptions('" + companyPK + "','2011-01-01 00:00:00','2011-08-02 00:00:00')");

			AssertEquals("Result should have rows", 1, report.Rows.Count);

			var row = report.Rows[0];
			AssertEquals("row['JobModuleID']", "DtbBooking", row["JobModuleID"]);
			AssertEquals("row['JobModulePK']", bookingPK, row["JobModulePK"]);
		}

		public void TestDtbBookingInstruction()
		{
			var helper = new TestDbHelper(TestConnection);
			var branchPK = (Guid)TestConnection.ExecuteScalar("SELECT TOP 1 GB_PK FROM dbo.GlbBranch");
			var dtbBookingConsolidationPK = helper.InsertDtbBookingConsolidation();
			var dtbBookingPK = helper.InsertDtbBooking("Test", dtbBookingConsolidationPK, branchPK);
			var dtbBookingInstructionPK = helper.InsertDtbBookingInstruction(5, dtbBookingPK);
			helper.InsertProcessTasks(dtbBookingInstructionPK, "KN", "EXC", new DateTime(2011, 8, 1));

			DataTable report = DataUtils.GetDataTableFromQuery(TestConnection, "select JobModuleID, JobModulePK, JobNumber, AdditionalDetail from Report_WorkflowExceptions('" + companyPK + "','2011-01-01 00:00:00','2011-08-02 00:00:00')");

			AssertEquals("Result should have rows", 1, report.Rows.Count);

			var row = report.Rows[0];
			AssertEquals("row['JobModuleID']", "DtbBooking", row["JobModuleID"]);
			AssertEquals("row['JobNumber']", "Test", row["JobNumber"]);
			AssertEquals("row['AdditionalDetail']", "Seq- 5", row["AdditionalDetail"]);
			AssertEquals("row['JobModulePK']", dtbBookingInstructionPK, row["JobModulePK"]);
		}

		public void TestEnquiry()
		{
			Guid enquiryPk = Guid.NewGuid();
			Guid coldCallPk = Guid.NewGuid();

			string insertQuery = string.Format(@"
INSERT INTO dbo.OrgColdCallRegister(O1_PK, O1_LeadUniqueReference, O1_LeadStatus, O1_LeadSource, O1_EnquiryType, O1_SystemCreateTimeUtc, O1_SystemCreateUser, O1_SystemLastEditTimeUtc, O1_SystemLastEditUser)
VALUES ('{0}', 'I00001007', 'OPN', 'AAA', 'INQ', GetUtcDate(), 'E', GetUtcDate(), 'E');
INSERT INTO dbo.ProcessTasks(P9_PK, P9_ParentID, P9_ParentTableCode, P9_Type, P9_ActualDate) VALUES (NEWID(), '{0}', 'O1', 'EXC', '2013-01-31 00:00:00');
INSERT INTO dbo.OrgColdCallRegister(O1_PK, O1_LeadUniqueReference, O1_LeadStatus, O1_LeadSource, O1_EnquiryType, O1_SystemCreateTimeUtc, O1_SystemCreateUser, O1_SystemLastEditTimeUtc, O1_SystemLastEditUser)
VALUES ('{1}', '00001004', 'CLD', 'BBB', 'CCR', GetUtcDate(), 'E', GetUtcDate(), 'E');
INSERT INTO dbo.ProcessTasks(P9_PK, P9_ParentID, P9_ParentTableCode, P9_Type, P9_ActualDate) VALUES (NEWID(), '{1}', 'O1', 'EXC', '2013-01-31 00:00:00');",
				enquiryPk.ToString(), coldCallPk.ToString());

			TestConnection.ExecuteNonQuery(insertQuery);

			var report = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT PK, JobNumber, AdditionalDetail, JobModuleID, JobModulePK FROM Report_WorkflowExceptions('" + companyPK + "','2013-01-01 00:00:00','2013-02-01 00:00:00')");

			AssertEquals("Matching Row Count", 1, report.Rows.Count);
			var row = report.Rows[0];

			CombineAssertions(delegate
			{
				AssertEquals("row['PK']", enquiryPk, row["PK"]);
				AssertEquals("row['JobNumber']", "I00001007", row["JobNumber"]);
				AssertEquals("row['AdditionalDetail']", "Inquiry Status: OPN Source: AAA", row["AdditionalDetail"]);
				AssertEquals("row['JobModuleID']", "Inquiry", row["JobModuleID"]);
				AssertEquals("row['JobModulePK']", enquiryPk, row["JobModulePK"]);
			});
		}

		public void TestCommunication()
		{
			var communicationPk = Guid.NewGuid();

			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.OrgHeader(OH_PK, OH_Code) VALUES ('1B4F6841-FEB6-45CB-86B9-B82D40FFD19D', 'BENMEATEE')
INSERT INTO dbo.OrgSalesCall(OQ_PK, OQ_CommunicationID, OQ_CallSummary, OQ_OH, OQ_SystemCreateTimeUtc, OQ_SystemCreateUser, OQ_SystemLastEditTimeUtc, OQ_SystemLastEditUser)
VALUES ('C27C7386-EFAB-4E70-9C9A-764453E25BEA', 'CM00001007', 'My Communication Summary', '1B4F6841-FEB6-45CB-86B9-B82D40FFD19D', GetUtcDate(), 'E', GetUtcDate(), 'E');
INSERT INTO dbo.ProcessTasks(P9_PK, P9_ParentID, P9_ParentTableCode, P9_Type, P9_ActualDate) VALUES(NEWID(), 'C27C7386-EFAB-4E70-9C9A-764453E25BEA', 'OQ', 'EXC', '2013-01-31 00:00:00');"
				);

			var report = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM Report_WorkflowExceptions('" + companyPK + "','2013-01-01 00:00:00','2013-02-01 00:00:00')");

			AssertEquals("Matching Row Count", 1, report.Rows.Count);
			var row = report.Rows[0];

			CombineAssertions(delegate
			{
				AssertEquals("row['PK']", new Guid("C27C7386-EFAB-4E70-9C9A-764453E25BEA"), row["PK"]);
				AssertEquals("row['WorkflowType']", "COM", row["WorkflowType"]);
				AssertEquals("row['ConsigneePK']", DBNull.Value, row["ConsigneePK"]);
				AssertEquals("row['ConsignorPK']", DBNull.Value, row["ConsignorPK"]);
				AssertEquals("row['JobNumber']", "CM00001007", row["JobNumber"]);
				AssertEquals("row['OriginPort']", DBNull.Value, row["OriginPort"]);
				AssertEquals("row['DestinationPort']", DBNull.Value, row["DestinationPort"]);
				AssertEquals("row['LoadPort']", DBNull.Value, row["LoadPort"]);
				AssertEquals("row['DischargePort']", DBNull.Value, row["DischargePort"]);
				AssertEquals("row['ContainerMode']", DBNull.Value, row["ContainerMode"]);
				AssertEquals("row['AdditionalDetail']", "My Communication Summary", row["AdditionalDetail"]);
				AssertEquals("row['TransportMode']", DBNull.Value, row["TransportMode"]);
				AssertEquals("row['JobModuleID']", "Communication", row["JobModuleID"]);
				AssertEquals("row['JobModulePK']", new Guid("C27C7386-EFAB-4E70-9C9A-764453E25BEA"), row["JobModulePK"]);
				AssertEquals("row['ETA']", DBNull.Value, row["ETA"]);
				AssertEquals("row['OrderNumber']", DBNull.Value, row["OrderNumber"]);
			});
		}

		public void TestSeaCargo()
		{
			TestConnection.ExecuteScalar(@"
insert into dbo.GlbCompany(GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency)
	values('CB15F7F4-1815-4398-A615-5935D78E46F5', 'DAN', 'AU company', 'AU', 'AUD')
insert into dbo.GlbBranch(GB_PK, GB_GC)
	values('53BB75D5-0AE0-4522-B05D-98999BBF88EC', 'CB15F7F4-1815-4398-A615-5935D78E46F5')

insert into dbo.CusSCAOceanBill(CB_PK, CB_OceanBill, CB_RL_NKPortOfLoading, CB_RL_NKPortOfDischarge, CB_MasterHouseBill, CB_DateOfArrival, CB_ResponsiblePartyID, CB_ApplicationCode, CB_GB, CB_SystemCreateTimeUtc, CB_SystemCreateUser, CB_SystemLastEditTimeUtc, CB_SystemLastEditUser)
values('2DE07C81-1332-4693-83FD-B403B1507485', '123456', 'NZAKL', 'AUSYD', 'MHB', '2012-11-06 00:00:00', 'RPID', 'CMR', '53BB75D5-0AE0-4522-B05D-98999BBF88EC', GetUtcDate(), '~BP', GetUtcDate(), '~BP');

insert into dbo.ProcessTasks(P9_PK, P9_ParentID, P9_ParentTableCode, P9_Type, P9_ActualDate)
values(NEWID(), '2DE07C81-1332-4693-83FD-B403B1507485', 'CB', 'EXC', '2011-08-01 00:00:00');"
				);

			var report = DataUtils.GetDataTableFromQuery(TestConnection, @"
select PK, WorkflowType, ConsigneePK, ConsignorPK, JobNumber, OriginPort, DestinationPort, LoadPort, DischargePort, ContainerMode, AdditionalDetail, TransportMode, JobModuleID, JobModulePK, ETA, OrderNumber
from Report_WorkflowExceptions('CB15F7F4-1815-4398-A615-5935D78E46F5','2011-01-01 00:00:00','2011-08-02 00:00:00')");

			AssertEquals("Matching Row Count", 1, report.Rows.Count);
			var row = report.Rows[0];

			CombineAssertions(delegate
			{
				AssertEquals("row['PK']", new Guid("2DE07C81-1332-4693-83FD-B403B1507485"), row["PK"]);
				AssertEquals("row['WorkflowType']", "SCR", row["WorkflowType"]);
				AssertEquals("row['ConsigneePK']", DBNull.Value, row["ConsigneePK"]);
				AssertEquals("row['ConsignorPK']", DBNull.Value, row["ConsignorPK"]);
				AssertEquals("row['JobNumber']", "123456", row["JobNumber"]);
				AssertEquals("row['OriginPort']", DBNull.Value, row["OriginPort"]);
				AssertEquals("row['DestinationPort']", DBNull.Value, row["DestinationPort"]);
				AssertEquals("row['LoadPort']", "NZAKL", row["LoadPort"]);
				AssertEquals("row['DischargePort']", "AUSYD", row["DischargePort"]);
				AssertEquals("row['ContainerMode']", "EMPTY", row["ContainerMode"]);
				AssertEquals("row['AdditionalDetail']", "MHB", row["AdditionalDetail"]);
				AssertEquals("row['TransportMode']", "SEA", row["TransportMode"]);
				AssertEquals("row['JobModuleID']", "SeaCargoStandAloneController", row["JobModuleID"]);
				AssertEquals("row['JobModulePK']", new Guid("2DE07C81-1332-4693-83FD-B403B1507485"), row["JobModulePK"]);
				AssertEquals("row['ETA']", new DateTime(2012, 11, 06, 0, 0, 0), row["ETA"]);
				AssertEquals("row['OrderNumber']", "RPID", row["OrderNumber"]);
			});
		}

		public void TestSeaCargoHouse()
		{
			TestConnection.ExecuteScalar(@"
insert into dbo.GlbCompany(GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency)
	values('CB15F7F4-1815-4398-A615-5935D78E46F5', 'DAN', 'AU company', 'AU', 'AUD')
insert into dbo.GlbBranch(GB_PK, GB_GC)
	values('53BB75D5-0AE0-4522-B05D-98999BBF88EC', 'CB15F7F4-1815-4398-A615-5935D78E46F5')

insert into dbo.CusSCAOceanBill(CB_PK, CB_OceanBill, CB_RL_NKPortOfLoading, CB_RL_NKPortOfDischarge, CB_MasterHouseBill, CB_DateOfArrival, CB_ResponsiblePartyID, CB_ApplicationCode, CB_GB, CB_SystemCreateTimeUtc, CB_SystemCreateUser, CB_SystemLastEditTimeUtc, CB_SystemLastEditUser)
values('2DE07C81-1332-4693-83FD-B403B1507485', '123456', 'NZAKL', 'AUSYD', 'MHB', '2012-11-06 00:00:00', 'RPID', 'CMR', '53BB75D5-0AE0-4522-B05D-98999BBF88EC', GetUtcDate(), '~BP', GetUtcDate(), '~BP');

insert into dbo.OrgHeader(OH_PK, OH_Code) values ('1B4F6841-FEB6-45CB-86B9-B82D40FFD19D', 'BENMEATEE');
insert into dbo.OrgAddress(OA_PK, OA_OH, OA_Code, OA_Address1) values ('9AABB41B-AA09-441F-904B-A3E2691857F8', '1B4F6841-FEB6-45CB-86B9-B82D40FFD19D', 'BENADDRESS', 'BENADDR1');
insert into dbo.OrgHeader(OH_PK, OH_Code) values ('6077994D-5401-480D-B327-57E01D9855C5', 'BOBBUILDER');
insert into dbo.OrgAddress(OA_PK, OA_OH, OA_Code, OA_Address1) values ('251EE892-4D71-4F1C-BCAC-23E6F7E02A87', '6077994D-5401-480D-B327-57E01D9855C5', 'BOBADDRESS', 'BOBADDR1');

insert into dbo.CusSCAHouse (CA_PK, CA_CB, CA_OA_ConsigneeAddress, CA_OA_ConsignorAddress, CA_HouseBill, CA_RL_NK_PortOfOrigin, CA_RL_NK_PortOfDestination, CA_ResponsiblePartyID, CA_SystemCreateTimeUtc, CA_SystemCreateUser, CA_SystemLastEditTimeUtc, CA_SystemLastEditUser)
	values ('5D0D7F29-8F45-4AFA-BF57-D301E896F876', '2DE07C81-1332-4693-83FD-B403B1507485', '9AABB41B-AA09-441F-904B-A3E2691857F8', '251EE892-4D71-4F1C-BCAC-23E6F7E02A87', 'HB23423', 'NZCHC', 'AUMEL', 'ORDER 1', GetUtcDate(), '~BP', GetUtcDate(), '~BP');

insert into dbo.ProcessTasks(P9_PK, P9_ParentID, P9_ParentTableCode, P9_Type, P9_ActualDate)
values(NEWID(), '5D0D7F29-8F45-4AFA-BF57-D301E896F876', 'CA', 'EXC', '2011-08-01 00:00:00');"
				);

			var report = DataUtils.GetDataTableFromQuery(TestConnection, @"
select PK, WorkflowType, ConsigneePK, ConsignorPK, JobNumber, OriginPort, DestinationPort, LoadPort, DischargePort, ContainerMode, AdditionalDetail, TransportMode, JobModuleID, JobModulePK, ETA, OrderNumber
from Report_WorkflowExceptions('CB15F7F4-1815-4398-A615-5935D78E46F5','2011-01-01 00:00:00','2011-08-02 00:00:00')");

			AssertEquals("Matching Row Count", 1, report.Rows.Count);
			var row = report.Rows[0];

			CombineAssertions(delegate
			{
				AssertEquals("row['PK']", new Guid("5d0d7f29-8f45-4afa-bf57-d301e896f876"), row["PK"]);
				AssertEquals("row['WorkflowType']", "SCU", row["WorkflowType"]);
				AssertEquals("row['ConsigneePK']", new Guid("1b4f6841-feb6-45cb-86b9-b82d40ffd19d"), row["ConsigneePK"]);
				AssertEquals("row['ConsignorPK']", new Guid("6077994d-5401-480d-b327-57e01d9855c5"), row["ConsignorPK"]);
				AssertEquals("row['JobNumber']", "HB23423", row["JobNumber"]);
				AssertEquals("row['OriginPort']", "NZCHC", row["OriginPort"]);
				AssertEquals("row['DestinationPort']", "AUMEL", row["DestinationPort"]);
				AssertEquals("row['LoadPort']", "", row["LoadPort"]);
				AssertEquals("row['DischargePort']", "", row["DischargePort"]);
				AssertEquals("row['ContainerMode']", "EMPTY", row["ContainerMode"]);
				AssertEquals("row['AdditionalDetail']", "123456/HB23423", row["AdditionalDetail"]);
				AssertEquals("row['TransportMode']", "SEA", row["TransportMode"]);
				AssertEquals("row['JobModuleID']", "AUCustomsSeaCargoHouseController", row["JobModuleID"]);
				AssertEquals("row['JobModulePK']", new Guid("5d0d7f29-8f45-4afa-bf57-d301e896f876"), row["JobModulePK"]);
				AssertEquals("row['ETA']", new DateTime(2012, 11, 06, 0, 0, 0), row["ETA"]);
				AssertEquals("row['OrderNumber']", "ORDER 1", row["OrderNumber"]);
			});
		}

		public void TestHiringRequest()
		{
			TestConnection.ExecuteScalar(@"
INSERT INTO dbo.GLBCOMPANY(GC_PK, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_CODE, GC_Name)
			VALUES('CB15F7F4-1815-4398-A615-5935D78E46F5', 'AU', 'AUD', 'HRQ', 'AU company')

INSERT INTO dbo.GLBBRANCH(GB_PK, GB_GC)
			VALUES('53BB75D5-0AE0-4522-B05D-98999BBF88EC', 'CB15F7F4-1815-4398-A615-5935D78E46F5')

INSERT INTO dbo.GLBDEPARTMENT(GE_PK, GE_CODE, GE_DESC)
			VALUES('2E76453B-328D-4BBD-97CA-D1A43BBF8835', 'HRQ', 'HRQ DEPARTMENT')

INSERT INTO dbo.GLBPERSON(PER_PK, PER_FULLNAME)
			VALUES('C9018ECA-778F-4391-8743-32749793AAF6','JOB APPLICANT PERSON' )

INSERT INTO dbo.HRJOBAPPLICANT(HA_PK, HA_PER, HA_EmailAddress, HA_SystemCreateTimeUtc, HA_SystemCreateUser, HA_SystemLastEditTimeUtc, HA_SystemLastEditUser)
			VALUES('9DDDEE9D-B03A-40E9-BF81-7739E143614B', 'C9018ECA-778F-4391-8743-32749793AAF6', 'job.applicant@job.com', GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.PROCESSTASKS(P9_PK, P9_PARENTID, P9_PARENTTABLECODE, P9_TYPE, P9_ACTUALDATE)
			VALUES(NEWID(), '0EEB723C-7ED1-4941-81E4-E3A58516DB4E', 'HRR', 'EXC', '2011-08-01 00:00:00')

INSERT INTO dbo.HRHIRINGREQUEST([HRR_PK],[HRR_HA_JOBAPPLICANT],HRR_PROBATIONDURATIONOVERRIDE,[HRR_STARTDATE],[HRR_WORKINGDAYS],[HRR_SYSTEMCREATETIMEUTC],[HRR_SYSTEMLASTEDITTIMEUTC],[HRR_SYSTEMCREATEUSER],[HRR_SYSTEMLASTEDITUSER],[HRR_JobTitle])
			VALUES ('0EEB723C-7ED1-4941-81E4-E3A58516DB4E','9DDDEE9D-B03A-40E9-BF81-7739E143614B',0,GETUTCDATE(),0,GETUTCDATE(),GETUTCDATE(),'HRQ','HRQ','Job Title')"
				);

			var report = DataUtils.GetDataTableFromQuery(TestConnection, @"
select PK, WorkflowType, ConsigneePK, ConsignorPK, JobNumber, OriginPort, DestinationPort, LoadPort, DischargePort, ContainerMode, AdditionalDetail, TransportMode, JobModuleID, JobModulePK, ETA, OrderNumber
from Report_WorkflowExceptions('0EEB723C-7ED1-4941-81E4-E3A58516DB4E','2011-01-01 00:00:00','2011-08-02 00:00:00')");

			AssertEquals("Matching Row Count", 1, report.Rows.Count);
			var row = report.Rows[0];

			CombineAssertions(delegate
			{
				AssertEquals("row['PK']", new Guid("0EEB723C-7ED1-4941-81E4-E3A58516DB4E"), row["PK"]);
				AssertEquals("row['WorkflowType']", "HRQ", row["WorkflowType"]);
				AssertEquals("row['ConsigneePK']", DBNull.Value, row["ConsigneePK"]);
				AssertEquals("row['ConsignorPK']", DBNull.Value, row["ConsignorPK"]);
				AssertEquals("row['JobNumber']", "job.applicant@job.com", row["JobNumber"]);
				AssertEquals("row['OriginPort']", DBNull.Value, row["OriginPort"]);
				AssertEquals("row['DestinationPort']", DBNull.Value, row["DestinationPort"]);
				AssertEquals("row['LoadPort']", DBNull.Value, row["LoadPort"]);
				AssertEquals("row['DischargePort']", DBNull.Value, row["DischargePort"]);
				AssertEquals("row['ContainerMode']", DBNull.Value, row["ContainerMode"]);
				AssertEquals("row['AdditionalDetail']", DBNull.Value, row["AdditionalDetail"]);
				AssertEquals("row['TransportMode']", DBNull.Value, row["TransportMode"]);
				AssertEquals("row['JobModuleID']", "HRHiringRequest", row["JobModuleID"]);
				AssertEquals("row['JobModulePK']", new Guid("0eeb723c-7ed1-4941-81e4-e3a58516db4e"), row["JobModulePK"]);
				AssertEquals("row['ETA']", DBNull.Value, row["ETA"]);
				AssertEquals("row['OrderNumber']", DBNull.Value, row["OrderNumber"]);
			});
		}

		public void TestOnBoarding()
		{
			TestConnection.ExecuteScalar(@"
INSERT INTO dbo.GLBCOMPANY(GC_PK, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_CODE, GC_Name)
			VALUES('CB15F7F4-1815-4398-A615-5935D78E46F5', 'AU', 'AUD', 'HOB', 'AU company')

INSERT INTO dbo.GLBBRANCH(GB_PK, GB_GC)
			VALUES('53BB75D5-0AE0-4522-B05D-98999BBF88EC', 'CB15F7F4-1815-4398-A615-5935D78E46F5')

INSERT INTO dbo.GLBDEPARTMENT(GE_PK, GE_CODE, GE_DESC)
			VALUES('2E76453B-328D-4BBD-97CA-D1A43BBF8835', 'HOB', 'HOB DEPARTMENT')

INSERT INTO dbo.GLBPERSON(PER_PK, PER_FULLNAME)
			VALUES('C9018ECA-778F-4391-8743-32749793AAF6','JOB APPLICANT PERSON' )

INSERT INTO dbo.HRJOBAPPLICANT(HA_PK, HA_PER, HA_EmailAddress, HA_SystemCreateTimeUtc, HA_SystemCreateUser, HA_SystemLastEditTimeUtc, HA_SystemLastEditUser)
			VALUES('9DDDEE9D-B03A-40E9-BF81-7739E143614B', 'C9018ECA-778F-4391-8743-32749793AAF6', 'job.applicant@job.com', GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.PROCESSTASKS(P9_PK, P9_PARENTID, P9_PARENTTABLECODE, P9_TYPE, P9_ACTUALDATE)
			VALUES(NEWID(), '0EEB723C-7ED1-4941-81E4-E3A58516DB4E', 'HOB', 'EXC', '2011-08-01 00:00:00')

INSERT INTO dbo.HRONBOARDING(
[HOB_PK],[HOB_HA_JOBAPPLICANT],[HOB_GB_HOMEBRANCH],[HOB_GE_HOMEDEPARTMENT],[HOB_STARTDATE],[HOB_SYSTEMCREATETIMEUTC],[HOB_SYSTEMLASTEDITTIMEUTC],[HOB_SYSTEMCREATEUSER],[HOB_SYSTEMLASTEDITUSER],[HOB_JobTitle])
VALUES (
'0EEB723C-7ED1-4941-81E4-E3A58516DB4E','9DDDEE9D-B03A-40E9-BF81-7739E143614B','53BB75D5-0AE0-4522-B05D-98999BBF88EC','2E76453B-328D-4BBD-97CA-D1A43BBF8835',GETUTCDATE(),GETUTCDATE(),GETUTCDATE(),'HOB','HOB','Job Title')"
				);

			var report = DataUtils.GetDataTableFromQuery(TestConnection, @"
select PK, WorkflowType, ConsigneePK, ConsignorPK, JobNumber, OriginPort, DestinationPort, LoadPort, DischargePort, ContainerMode, AdditionalDetail, TransportMode, JobModuleID, JobModulePK, ETA, OrderNumber
from Report_WorkflowExceptions('0EEB723C-7ED1-4941-81E4-E3A58516DB4E','2011-01-01 00:00:00','2011-08-02 00:00:00')");

			AssertEquals("Matching Row Count", 1, report.Rows.Count);
			var row = report.Rows[0];

			CombineAssertions(delegate
			{
				AssertEquals("row['PK']", new Guid("0EEB723C-7ED1-4941-81E4-E3A58516DB4E"), row["PK"]);
				AssertEquals("row['WorkflowType']", "HRO", row["WorkflowType"]);
				AssertEquals("row['ConsigneePK']", DBNull.Value, row["ConsigneePK"]);
				AssertEquals("row['ConsignorPK']", DBNull.Value, row["ConsignorPK"]);
				AssertEquals("row['JobNumber']", "job.applicant@job.com", row["JobNumber"]);
				AssertEquals("row['OriginPort']", DBNull.Value, row["OriginPort"]);
				AssertEquals("row['DestinationPort']", DBNull.Value, row["DestinationPort"]);
				AssertEquals("row['LoadPort']", DBNull.Value, row["LoadPort"]);
				AssertEquals("row['DischargePort']", DBNull.Value, row["DischargePort"]);
				AssertEquals("row['ContainerMode']", DBNull.Value, row["ContainerMode"]);
				AssertEquals("row['AdditionalDetail']", DBNull.Value, row["AdditionalDetail"]);
				AssertEquals("row['TransportMode']", DBNull.Value, row["TransportMode"]);
				AssertEquals("row['JobModuleID']", "HROnBoarding", row["JobModuleID"]);
				AssertEquals("row['JobModulePK']", new Guid("0eeb723c-7ed1-4941-81e4-e3a58516db4e"), row["JobModulePK"]);
				AssertEquals("row['ETA']", DBNull.Value, row["ETA"]);
				AssertEquals("row['OrderNumber']", DBNull.Value, row["OrderNumber"]);
			});
		}

		public void TestGlbStaffHoliday()
		{
			TestConnection.ExecuteScalar(@"
INSERT INTO dbo.GlbStaff(GS_PK, GS_CODE, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
	VALUES('4CC2C7B0-0AF3-4CCC-ADDA-D37D922B6C0D', 'ABC', GETUTCDATE(), 'E', GETUTCDATE(), 'E')

INSERT INTO dbo.ProcessTasks(P9_PK, P9_PARENTID, P9_PARENTTABLECODE, P9_TYPE, P9_ACTUALDATE)
	VALUES(NEWID(), 'B7587764-0E43-4EDF-910F-D44D164509E2', 'GA', 'EXC', '2011-08-01 00:00:00')

INSERT INTO dbo.GlbStaffHoliday(GA_PK, GA_GS, GA_STARTTIME, GA_SystemCreateTimeUtc, GA_SystemCreateUser, GA_SystemLastEditTimeUtc, GA_SystemLastEditUser)
	VALUES ('B7587764-0E43-4EDF-910F-D44D164509E2', '4CC2C7B0-0AF3-4CCC-ADDA-D37D922B6C0D', GETUTCDATE(), GETUTCDATE(), 'E', GETUTCDATE(), 'E')
");

			var report = DataUtils.GetDataTableFromQuery(TestConnection, @"
SELECT PK, WORKFLOWTYPE, CONSIGNEEPK, CONSIGNORPK, JOBNUMBER, ORIGINPORT, DESTINATIONPORT, LOADPORT, DISCHARGEPORT, CONTAINERMODE, ADDITIONALDETAIL, TRANSPORTMODE, JOBMODULEID, JOBMODULEPK, ETA, ORDERNUMBER
FROM Report_WorkflowExceptions('B7587764-0E43-4EDF-910F-D44D164509E2','2011-01-01 00:00:00','2011-08-02 00:00:00')");

			AssertEquals("Matching Row Count", 1, report.Rows.Count);
			var row = report.Rows[0];

			CombineAssertions(delegate
			{
				AssertEquals("row['PK']", new Guid("B7587764-0E43-4EDF-910F-D44D164509E2"), row["PK"]);
				AssertEquals("row['WorkflowType']", "SHO", row["WorkflowType"]);
				AssertEquals("row['ConsigneePK']", DBNull.Value, row["ConsigneePK"]);
				AssertEquals("row['ConsignorPK']", DBNull.Value, row["ConsignorPK"]);
				AssertEquals("row['JobNumber']", "ABC", row["JobNumber"]);
				AssertEquals("row['OriginPort']", DBNull.Value, row["OriginPort"]);
				AssertEquals("row['DestinationPort']", DBNull.Value, row["DestinationPort"]);
				AssertEquals("row['LoadPort']", DBNull.Value, row["LoadPort"]);
				AssertEquals("row['DischargePort']", DBNull.Value, row["DischargePort"]);
				AssertEquals("row['ContainerMode']", DBNull.Value, row["ContainerMode"]);
				AssertEquals("row['AdditionalDetail']", DBNull.Value, row["AdditionalDetail"]);
				AssertEquals("row['TransportMode']", DBNull.Value, row["TransportMode"]);
				AssertEquals("row['JobModuleID']", "GlbStaffHoliday", row["JobModuleID"]);
				AssertEquals("row['JobModulePK']", new Guid("B7587764-0E43-4EDF-910F-D44D164509E2"), row["JobModulePK"]);
				AssertEquals("row['ETA']", DBNull.Value, row["ETA"]);
				AssertEquals("row['OrderNumber']", DBNull.Value, row["OrderNumber"]);
			});
		}

		public void TestHVLVConsignment()
		{
			var consignmentPK = Guid.NewGuid();

			const string insertSql = @"
DECLARE @OrgAddressPK uniqueidentifier = (SELECT TOP 1 OA_PK FROM dbo.OrgAddress)
DECLARE @HeaderPK uniqueidentifier = NEWID()

INSERT INTO dbo.HVLVBookingHeader
(HVH_PK, HVH_ClusterKey, HVH_BookingReference, HVH_OA_BillToParty, HVH_SystemCreateTimeUtc, HVH_SystemCreateUser, HVH_SystemLastEditTimeUtc, HVH_SystemLastEditUser)
VALUES
(@HeaderPK, 1, 'M00001001', @OrgAddressPK, '2016-06-15 00:00:00', 'E', '2016-06-15 00:00:00', 'E')

INSERT INTO dbo.HVLVConsignment
(HVC_PK, HVC_ClusterKey, HVC_HVH_BookingHeader, HVC_ConsignmentId, HVC_ShipperReference, HVC_Status, HVC_SystemCreateTimeUtc, HVC_SystemCreateUser, HVC_SystemLastEditTimeUtc, HVC_SystemLastEditUser)
VALUES
(@ConsignmentPK, 1, @HeaderPK, 'CONSIGN1', 'SHIPREF1', 'CNF', '2016-06-15 00:00:00', 'E', '2016-06-15 00:00:00', 'E')

INSERT INTO dbo.ProcessTasks
(P9_PK, P9_ParentID, P9_ParentTableCode, P9_Type, P9_ActualDate)
VALUES
(NEWID(), @ConsignmentPK, 'HVC', 'EXC', '2016-06-15 00:00:00')
";

			using (var cmd = TestConnection.Command(insertSql))
			{
				cmd.AddParameter("@ConsignmentPK", SqlDbType.UniqueIdentifier, consignmentPK);
				cmd.ExecuteNonQuery();
			}

			var report = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT WorkflowType, JobModulePK, PK, JobNumber, AdditionalDetail, JobModuleID FROM Report_WorkflowExceptions('" + companyPK + "', '2016-01-01 00:00:00', '2017-01-01 00:00:00')");

			AssertEquals(1, report.Rows.Count);

			var row = report.Rows[0];
			CombineAssertions(delegate
			{
				AssertEquals("row['WorkflowType']", "HVC", row["WorkflowType"]);
				AssertEquals("row['PK']", consignmentPK, row["PK"]);
				AssertEquals("row['JobNumber']", "CONSIGN1", row["JobNumber"]);
				AssertEquals("row['AdditionalDetail']", "Shipper Reference: SHIPREF1", row["AdditionalDetail"]);
				AssertEquals("row['JobModuleID']", "HVLVConsignment", row["JobModuleID"]);
				AssertEquals("row['JobModulePK']", consignmentPK, row["JobModulePK"]);
			});
		}

		public void TestHVLVBookingHeader()
		{
			var headerPK = Guid.NewGuid();

			const string insertSql = @"
DECLARE @OrgAddressPK uniqueidentifier = (SELECT TOP 1 OA_PK FROM dbo.OrgAddress)

INSERT INTO dbo.HVLVBookingHeader
(HVH_PK, HVH_ClusterKey, HVH_BookingReference, HVH_RS_NKBookingServiceLevel, HVH_OA_BillToParty, HVH_SystemCreateTimeUtc, HVH_SystemCreateUser, HVH_SystemLastEditTimeUtc, HVH_SystemLastEditUser)
VALUES
(@HeaderPK, 1, 'M00001001', 'D2D', @OrgAddressPK, '2016-06-15 00:00:00', 'E', '2016-06-15 00:00:00', 'E')

INSERT INTO dbo.ProcessTasks
(P9_PK, P9_ParentID, P9_ParentTableCode, P9_Type, P9_ActualDate)
VALUES
(NEWID(), @HeaderPK, 'HVH', 'EXC', '2016-06-15 00:00:00')
";

			using (var cmd = TestConnection.Command(insertSql))
			{
				cmd.AddParameter("@HeaderPK", SqlDbType.UniqueIdentifier, headerPK);
				cmd.ExecuteNonQuery();
			}

			var report = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT WorkflowType, PK, JobNumber, AdditionalDetail, JobModuleID, JobModulePK FROM Report_WorkflowExceptions('" + companyPK + "', '2016-01-01 00:00:00', '2017-01-01 00:00:00')");

			AssertEquals(1, report.Rows.Count);

			var row = report.Rows[0];
			CombineAssertions(delegate
			{
				AssertEquals("row['WorkflowType']", "HVH", row["WorkflowType"]);
				AssertEquals("row['PK']", headerPK, row["PK"]);
				AssertEquals("row['JobNumber']", "M00001001", row["JobNumber"]);
				AssertEquals("row['AdditionalDetail']", "Service Level: D2D", row["AdditionalDetail"]);
				AssertEquals("row['JobModuleID']", "HVLVBookingHeader", row["JobModuleID"]);
				AssertEquals("row['JobModulePK']", headerPK, row["JobModulePK"]);
			});
		}

		public void TestHRGlbCompanyCampaign()
		{
			var companyCampaignPK = Guid.NewGuid();

			const string insertSql = @"
insert into dbo.GlbCompanyCampaign(G0_PK, G0_GC, G0_CampaignName, G0_CampaignID, G0_Category, G0_Type, G0_IsSalesAndMarketing, G0_SystemCreateTimeUtc, G0_SystemCreateUser, G0_SystemLastEditTimeUtc, G0_SystemLastEditUser)
values(@CompanyCampaignPK, (select top 1 GC_PK from dbo.GlbCompany), 'HRCampy', 'TST00001000', 'REFER', 'PREAP', 0, GetUtcDate(), 'E', GetUtcDate(), 'E');
insert into dbo.ProcessTasks(P9_PK, P9_ParentID, P9_ParentTableCode, P9_Type, P9_ActualDate) values(NEWID(), @CompanyCampaignPK, 'G0', 'EXC', '2016-01-01 00:00:00');";

			using (var cmd = TestConnection.Command(insertSql))
			{
				cmd.AddParameter("@CompanyCampaignPK", SqlDbType.UniqueIdentifier, companyCampaignPK);
				cmd.ExecuteNonQuery();
			}

			var report = DataUtils.GetDataTableFromQuery(TestConnection, "select WorkflowType, JobModuleID, JobModulePK, AdditionalDetail, PK from Report_WorkflowExceptions('" + companyPK + "','2016-01-01 00:00:00','2016-08-02 00:00:00')");

			AssertEquals("Result should have rows", 1, report.Rows.Count);

			var row = report.Rows[0];
			CombineAssertions(delegate
			{
				AssertEquals("row['WorkflowType']", "HRC", row["WorkflowType"]);
				AssertEquals("row['JobModuleID']", "HRGlbCompanyCampaign", row["JobModuleID"]);
				AssertEquals("row['JobModulePK']", companyCampaignPK, row["JobModulePK"]);
				AssertEquals("row['AdditionalDetail']", "Category: REFER Type: PREAP", row["AdditionalDetail"]);
				AssertEquals("row['PK']", companyCampaignPK, row["PK"]);
			});
		}
		public void TestAsycudaMenifestHeaderExceptions()
		{
			TestConnection.ExecuteScalar(@"
	INSERT INTO  dbo.GlbCompany(GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency)
	values('CB15F7F4-1815-4398-A615-5935D78E46F5', 'DAN', 'ZA company', 'ZA', 'ZAF')
	INSERT INTO  dbo.GlbBranch(GB_PK, GB_GC)
	values('53BB75D5-0AE0-4522-B05D-98999BBF88EC', 'CB15F7F4-1815-4398-A615-5935D78E46F5')

	INSERT INTO [AsycudaManifestHeader]([AMA_PK], [AMA_ApplicationCode], [AMA_GB] , [AMA_SystemLastEditTimeUtc] ,[AMA_SystemCreateTimeUtc] , [AMA_JobReference] , [AMA_SystemCreateUser],[AMA_SystemLastEditUser],[AMA_ClusterKey],[AMA_RN_NKCountry],[AMA_TransportMode] )
	VALUES ('1B4F6841-FEB6-45CB-86B9-B82D40FFD19D', 'OUT', '53BB75D5-0AE0-4522-B05D-98999BBF88EC' , '2013-01-31 00:00:00','2013-02-01 00:00:00', '19GBL4592NCOI21NR9', 'E','E',1,'ZA','SEA')

	INSERT INTO [AsycudaBill]([ABL_PK],ABL_AMA,ABL_SystemCreateTimeUtc, ABL_SystemLastEditTimeUtc ,ABL_SystemCreateUser , ABL_SystemLastEditUser, ABL_ClusterKey , [ABL_RL_NKOrigin],[ABL_RL_NKFinalDestination] )
    Values('53BB75D5-0AE0-4522-B05D-98999BBF88EC', '1B4F6841-FEB6-45CB-86B9-B82D40FFD19D', '2013-01-31 00:00:00','2013-02-01 00:00:00', 'E' , 'E',1, 'USORD' ,'GBLHR')

	INSERT INTO  [CusEntryNum]([CE_PK],[CE_ParentID], [CE_ParentTable], [CE_EntryType], [CE_EntryNum], CE_SystemCreateTimeUtc, CE_SystemCreateUser, CE_SystemLastEditTimeUtc, CE_SystemLastEditUser)
    Values('B98D9BD0-F68C-46A4-80A8-378F139FC8F7' , '1B4F6841-FEB6-45CB-86B9-B82D40FFD19D', 'AsycudaManifestHeader', 'MRN' , '19GBL4592NCOI21NR9', getutcdate(), '~BP', getutcdate(), '~BP')

	INSERT INTO dbo.ProcessTasks(P9_PK, P9_ParentID, P9_ParentTableCode, P9_Type, P9_ActualDate)
	values(NEWID(), '1B4F6841-FEB6-45CB-86B9-B82D40FFD19D', 'AMA', 'EXC', '2013-01-31 00:00:00');"
				);

			var report = DataUtils.GetDataTableFromQuery(TestConnection, @"
SELECT PK, WorkflowType, PK, ConsignorPK, ConsignorPK, JobNumber, OriginPort, DestinationPort, LoadPort, DischargePort, ContainerMode, AdditionalDetail, TransportMode, JobModuleID, JobModulePK, ETA, OrderNumber
FROM Report_WorkflowExceptions('CB15F7F4-1815-4398-A615-5935D78E46F5','2013-01-30 00:00:00','2013-02-01 00:00:00')");

			AssertEquals("Matching Row Count", 1, report.Rows.Count);
			var row = report.Rows[0];

			CombineAssertions(delegate
			{
				AssertEquals("row['WorkflowType']", "AOG", row["WorkflowType"]);
				AssertEquals("row['JobNumber']", "19GBL4592NCOI21NR9", row["JobNumber"]);
				AssertEquals("row['JobModuleID']", "ZAOutturnAndGateInOut", row["JobModuleID"]);
				AssertEquals("row['JobModulePK']", new Guid("1b4f6841-feb6-45cb-86b9-b82d40ffd19d"), row["JobModulePK"]);
				AssertEquals("row['AdditionalDetail']", "19GBL4592NCOI21NR9", row["AdditionalDetail"]);
				AssertEquals("row['PK']", new Guid("1b4f6841-feb6-45cb-86b9-b82d40ffd19d"), row["PK"]);
			});
		}

		public void TestGlbAccreditationAttempt()
		{
			var accreditationPK = Guid.NewGuid();
			var personPK = Guid.NewGuid();
			var attemptPK = Guid.NewGuid();

			const string insertSql = @"
insert into dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description, HAC_MustCompleteInDays, HAC_ValidityMonths)
values (@AccreditationPK, 'CCO', 'Certified Operator', 180, 3)

insert into dbo.GlbPerson (PER_PK, PER_FullName)
values (@PersonPK, 'Tester')

insert into dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDueDate)
values (@AttemptPK, @AccreditationPK, @PersonPK, '2018-1-1', '2019-1-1')

insert into dbo.ProcessTasks(P9_PK, P9_ParentID, P9_ParentTableCode, P9_Type, P9_ActualDate)
values (NEWID(), @AttemptPK, 'HAA', 'EXC', '2016-01-01 00:00:00');
";

			using (var cmd = TestConnection.Command(insertSql))
			{
				cmd.AddParameter("@AccreditationPK", SqlDbType.UniqueIdentifier, accreditationPK);
				cmd.AddParameter("@PersonPK", SqlDbType.UniqueIdentifier, personPK);
				cmd.AddParameter("@AttemptPK", SqlDbType.UniqueIdentifier, attemptPK);
				cmd.ExecuteNonQuery();
			}

			var report = DataUtils.GetDataTableFromQuery(TestConnection, "select WorkflowType, JobNumber, JobModuleID, JobModulePK, AdditionalDetail, PK from Report_WorkflowExceptions('" + companyPK + "','2016-01-01 00:00:00','2016-08-02 00:00:00')");

			AssertEquals("Result should have rows", 1, report.Rows.Count);

			var row = report.Rows[0];
			CombineAssertions(delegate
			{
				AssertEquals("row['WorkflowType']", "ACA", row["WorkflowType"]);
				AssertEquals("row['JobNumber']", "Certified Operator", row["JobNumber"]);
				AssertEquals("row['JobModuleID']", "GlbAccreditationAttempt", row["JobModuleID"]);
				AssertEquals("row['JobModulePK']", attemptPK, row["JobModulePK"]);
				AssertEquals("row['AdditionalDetail']", "Tester", row["AdditionalDetail"]);
				AssertEquals("row['PK']", attemptPK, row["PK"]);
			});
		}

		public void TestSeaCargoOutturn()
		{
			TestConnection.ExecuteScalar(@"
INSERT INTO dbo.CusOutturnHeader
(
C6_PK,
C6_ResponsiblePartyID,
C6_VesselName,
C6_LloydsIMO,
C6_VoyageNum,
C6_DateOfArrival,
C6_OA_OutturningPremise,
C6_OutturningPremiseID,
C6_SendersMessageReference,
C6_MessageStatus,
C6_CommercialStatus,
C6_SystemCreateTimeUtc,
C6_SystemCreateUser,
C6_SystemLastEditTimeUtc,
C6_SystemLastEditUser,
C6_IsValid,
C6_RadioCallSign,
C6_RN_NKCountryOfRegistration
)
VALUES
(
    'F0BEB10F-4FED-45B8-86ED-D5C703599997', -- C6_PK
    '', -- C6_ResponsiblePartyID
    '', -- C6_VesselName
    '', -- C6_LloydsIMO
    '', -- C6_VoyageNum
    NULL, -- C6_DateOfArrival
    NULL, -- C6_OA_OutturningPremise
    '', -- C6_OutturningPremiseID
    'O00000340', -- C6_SendersMessageReference
    '', -- C6_MessageStatus
    '', -- C6_CommercialStatus
    '2020-01-15 00:00:00', -- C6_SystemCreateTimeUtc
    'E', -- C6_SystemCreateUser
    '2020-01-15 00:00:00', -- C6_SystemLastEditTimeUtc
    'E', -- C6_SystemLastEditUser
    1, -- C6_IsValid
    '', -- C6_RadioCallSign
    '' -- C6_RN_NKCountryOfRegistration
);

insert into dbo.ProcessTasks(P9_PK, P9_ParentID, P9_ParentTableCode, P9_Type, P9_ActualDate) values(NEWID(), 'F0BEB10F-4FED-45B8-86ED-D5C703599997', 'C6', 'EXC', '2011-08-01 00:00:00');"
				);

			var report = DataUtils.GetDataTableFromQuery(TestConnection, @"
select PK, WorkflowType, ConsigneePK, ConsignorPK, JobNumber, OriginPort, DestinationPort, LoadPort, DischargePort, ContainerMode, AdditionalDetail, TransportMode, JobModuleID, JobModulePK, ETA, OrderNumber
	from Report_WorkflowExceptions('" + companyPK + "','2011-01-01 00:00:00','2011-08-02 00:00:00')");

			AssertEquals("Matching Row Count", 1, report.Rows.Count);
			var row = report.Rows[0];

			CombineAssertions(delegate
			{
				AssertEquals("row['PK']", new Guid("F0BEB10F-4FED-45B8-86ED-D5C703599997"), row["PK"]);
				AssertEquals("row['WorkflowType']", "SCO", row["WorkflowType"]);
				AssertEquals("row['ConsigneePK']", DBNull.Value, row["ConsigneePK"]);
				AssertEquals("row['ConsignorPK']", DBNull.Value, row["ConsignorPK"]);
				AssertEquals("row['JobNumber']", "O00000340", row["JobNumber"]);
				AssertEquals("row['OriginPort']", DBNull.Value, row["OriginPort"]);
				AssertEquals("row['DestinationPort']", DBNull.Value, row["DestinationPort"]);
				AssertEquals("row['LoadPort']", DBNull.Value, row["LoadPort"]);
				AssertEquals("row['DischargePort']", DBNull.Value, row["DischargePort"]);
				AssertEquals("row['ContainerMode']", DBNull.Value, row["ContainerMode"]);
				AssertEquals("row['AdditionalDetail']", "", row["AdditionalDetail"]);
				AssertEquals("row['TransportMode']", "SEA", row["TransportMode"]);
				AssertEquals("row['JobModuleID']", "SeaCargoDepot", row["JobModuleID"]);
				AssertEquals("row['JobModulePK']", new Guid("F0BEB10F-4FED-45B8-86ED-D5C703599997"), row["JobModulePK"]);
				AssertEquals("row['ETA']", DBNull.Value, row["ETA"]);
				AssertEquals("row['OrderNumber']", DBNull.Value, row["OrderNumber"]);
			});
		}

		public void TestAccTransactionHeader()
		{
			var helper = new TestDbHelper(TestConnection);
			var branchPK = helper.InsertBranch("ZZB", TestDbHelper.DefaultCompanyPK);
			var departmentPK = helper.InsertDepartment("ZZD");

			var glAccountPK = helper.InsertGLAccount("1234.56.03", "TestGLAccount 3");
			var chargeCodePK = helper.InsertChargeCode(TestDbHelper.DefaultCompanyPK, "CC1");
			var orgPK = helper.InsertOrgHeader("ZC1", "Creditor 1");

			var datetime = helper.ToDate("2020-07-03");
			var shipmentPK = helper.InsertShipment("S00000001", datetime);
			var jobPK = helper.InsertJob("J00000001", TestDbHelper.DefaultCompanyPK, branchPK, departmentPK, "JS", shipmentPK, "WRK", datetime);

			var arInvPK = helper.InsertTransactionHeader("AR", "INV", "001", 110, datetime, branchPK, departmentPK, job: jobPK);
			helper.InsertTransactionLine(arInvPK, jobPK, chargeCodePK, glAccountPK, branchPK, departmentPK, orgPK, 50, "REV", datetime, null, 5);
			helper.InsertProcessTasks(arInvPK, "AH", "EXC", datetime);

			var apInvPK = helper.InsertTransactionHeader("AP", "INV", "002", 110, datetime, branchPK, departmentPK, job: jobPK);
			helper.InsertTransactionLine(apInvPK, jobPK, chargeCodePK, glAccountPK, branchPK, departmentPK, orgPK, 50, "CST", datetime, null, 5, 0.5m, "A");
			helper.InsertProcessTasks(apInvPK, "AH", "EXC", datetime);

			var apCrdPK = helper.InsertTransactionHeader("AP", "CRD", "003", 220, datetime, branchPK, departmentPK);
			helper.InsertTransactionLine(apCrdPK, jobPK, chargeCodePK, glAccountPK, branchPK, departmentPK, orgPK, 100, "CST", datetime, null, 10, 0.5m, "A");
			helper.InsertProcessTasks(apCrdPK, "AH", "EXC", datetime);

			var arRecPK = helper.InsertTransactionHeader("AR", "REC", "004", 330, datetime, branchPK, departmentPK);
			helper.InsertTransactionLine(arRecPK, jobPK, chargeCodePK, glAccountPK, branchPK, departmentPK, orgPK, 200, "CST", datetime, null, 20, 0.5m, "A");
			helper.InsertProcessTasks(arRecPK, "AH", "EXC", datetime);

			var apRecPK = helper.InsertTransactionHeader("AP", "REC", "005", 440, datetime, branchPK, departmentPK);
			helper.InsertTransactionLine(apRecPK, jobPK, chargeCodePK, glAccountPK, branchPK, departmentPK, orgPK, 300, "CST", datetime, null, 20, 0.5m, "A");
			helper.InsertProcessTasks(apRecPK, "AH", "EXC", datetime);

			var arPayPK = helper.InsertTransactionHeader("AR", "PAY", "006", 220, datetime, branchPK, departmentPK);
			helper.InsertTransactionLine(arPayPK, jobPK, chargeCodePK, glAccountPK, branchPK, departmentPK, orgPK, 100, "CST", datetime, null, 10, 0.5m, "A");
			helper.InsertProcessTasks(arPayPK, "AH", "EXC", datetime);

			var apPayPK = helper.InsertTransactionHeader("AP", "PAY", "007", 330, datetime, branchPK, departmentPK);
			helper.InsertTransactionLine(apPayPK, jobPK, chargeCodePK, glAccountPK, branchPK, departmentPK, orgPK, 200, "CST", datetime, null, 10, 0.5m, "A");
			helper.InsertProcessTasks(apPayPK, "AH", "EXC", datetime);

			var report = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_WorkflowExceptions('{TestDbHelper.DefaultCompanyPK.ToString()}','2020-07-01 00:00:00','2020-08-01 00:00:00')");

			AssertEquals("Result should have rows", 7, report.Rows.Count);

			AssertTransactionHeader(arInvPK, "AR", "INV", "001", "J00000001", jobPK);
			AssertTransactionHeader(apInvPK, "AP", "INV", "002", "J00000001", jobPK);
			AssertTransactionHeader(apCrdPK, "AP", "CRD", "003", DBNull.Value, DBNull.Value);
			AssertTransactionHeader(arRecPK, "AR", "REC", "004", DBNull.Value, DBNull.Value);
			AssertTransactionHeader(apRecPK, "AP", "REC", "005", DBNull.Value, DBNull.Value);
			AssertTransactionHeader(arPayPK, "AR", "PAY", "006", DBNull.Value, DBNull.Value);
			AssertTransactionHeader(apPayPK, "AP", "PAY", "007", DBNull.Value, DBNull.Value);

			void AssertTransactionHeader(Guid expectedPK, string expectedLedger, string expectedTransactionType, string expectedTransactionNum, object expectedJobNum, object expectedJobModulePK)
			{
				var rows = report.Select($"PK='{expectedPK}'");
				AssertEquals(1, rows.Length);
				var row = rows[0];

				CombineAssertions(delegate
				{
					AssertEquals("row['WorkflowType']", "", row["WorkflowType"]);
					AssertEquals("row['PK']", expectedPK.ToString(), row["PK"].ToString());
					AssertEquals("row['ConsigneePK']", DBNull.Value, row["ConsigneePK"]);
					AssertEquals("row['ConsignorPK']", DBNull.Value, row["ConsignorPK"]);
					AssertEquals("row['JobNumber']", expectedJobNum, row["JobNumber"]);
					AssertEquals("row['OriginPort']", DBNull.Value, row["OriginPort"]);
					AssertEquals("row['DestinationPort']", DBNull.Value, row["DestinationPort"]);
					AssertEquals("row['LoadPort']", DBNull.Value, row["LoadPort"]);
					AssertEquals("row['DischargePort']", DBNull.Value, row["DischargePort"]);
					AssertEquals("row['ContainerMode']", "", row["ContainerMode"]);
					AssertEquals("row['AdditionalDetail']", $"{expectedLedger} {expectedTransactionType} {expectedTransactionNum}", row["AdditionalDetail"]);
					AssertEquals("row['TransportMode']", "", row["TransportMode"]);
					AssertEquals("row['JobModuleID']", "JobHeader", row["JobModuleID"]);
					AssertEquals("row['JobModulePK']", expectedJobModulePK, row["JobModulePK"]);
					AssertEquals("row['ETA']", DBNull.Value, row["ETA"]);
					AssertEquals("row['OrderNumber']", "", row["OrderNumber"]);
				});
			}
		}

		public void TestAccPaymentApproval()
		{
			var helper = new TestDbHelper(TestConnection);
			var branchPK = helper.InsertBranch("AZB", TestDbHelper.DefaultCompanyPK);
			var departmentPK = helper.InsertDepartment("AZD");

			var glAccountPK = helper.InsertGLAccount("1234.56.03", "TestGLAccount 3");
			var chargeCodePK = helper.InsertChargeCode(TestDbHelper.DefaultCompanyPK, "CC1");
			var orgPK = helper.InsertOrgHeader("ZC1", "Creditor 1");

			var datetime = helper.ToDate("2020-07-03");
			var shipmentPK = helper.InsertShipment("S00000001", datetime);
			var jobPK = helper.InsertJob("J00000001", TestDbHelper.DefaultCompanyPK, branchPK, departmentPK, "JS", shipmentPK, "WRK", datetime);

			var arPayPK = helper.InsertTransactionHeader("AR", "PAY", "003", 220, datetime, branchPK, departmentPK);
			helper.InsertTransactionLine(arPayPK, jobPK, chargeCodePK, glAccountPK, branchPK, departmentPK, orgPK, 100, "CST", datetime, null, 10, 0.5m, "A");
			var arPaymentApproval = helper.InsertAccPaymentApproval("AR", orgPK, arPayPK);
			helper.InsertProcessTasks(arPaymentApproval, "AV", "EXC", datetime);

			var apPayPK = helper.InsertTransactionHeader("AP", "PAY", "004", 330, datetime, branchPK, departmentPK);
			helper.InsertTransactionLine(apPayPK, jobPK, chargeCodePK, glAccountPK, branchPK, departmentPK, orgPK, 200, "CST", datetime, null, 10, 0.5m, "A");
			var apPaymentApproval = helper.InsertAccPaymentApproval("AP", orgPK, apPayPK);
			helper.InsertProcessTasks(apPaymentApproval, "AV", "EXC", datetime);

			var report = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_WorkflowExceptions('{TestDbHelper.DefaultCompanyPK.ToString()}','2020-07-01 00:00:00','2020-08-01 00:00:00')");

			AssertEquals("Result should have rows", 2, report.Rows.Count);
			AssertTransactionHeader("RPA", arPaymentApproval, "ARPaymentProcessing");
			AssertTransactionHeader("PPA", apPaymentApproval, "APPaymentProcessing");

			void AssertTransactionHeader(string workflowType, Guid expectedPK, object expectedJobModuleID)
			{
				var rows = report.Select($"PK='{expectedPK}'");
				AssertEquals(1, rows.Length);
				var row = rows[0];

				CombineAssertions(delegate
				{
					AssertEquals("row['WorkflowType']", workflowType, row["WorkflowType"]);
					AssertEquals("row['PK']", expectedPK.ToString(), row["PK"].ToString());
					AssertEquals("row['ConsigneePK']", DBNull.Value, row["ConsigneePK"]);
					AssertEquals("row['ConsignorPK']", DBNull.Value, row["ConsignorPK"]);
					AssertEquals("row['JobNumber']", "CSH", row["JobNumber"]);
					AssertEquals("row['OriginPort']", DBNull.Value, row["OriginPort"]);
					AssertEquals("row['DestinationPort']", DBNull.Value, row["DestinationPort"]);
					AssertEquals("row['LoadPort']", DBNull.Value, row["LoadPort"]);
					AssertEquals("row['DischargePort']", DBNull.Value, row["DischargePort"]);
					AssertEquals("row['ContainerMode']", "", row["ContainerMode"]);
					AssertEquals("row['AdditionalDetail']", "Payment Approval Reference: 1234567", row["AdditionalDetail"]);
					AssertEquals("row['TransportMode']", "", row["TransportMode"]);
					AssertEquals("row['JobModuleID']", expectedJobModuleID, row["JobModuleID"]);
					AssertEquals("row['JobModulePK']", expectedPK, row["JobModulePK"]);
					AssertEquals("row['ETA']", DBNull.Value, row["ETA"]);
					AssertEquals("row['OrderNumber']", "", row["OrderNumber"]);
				});
			}
		}

		public void TestAccDraftInvoiceHeader()
		{
			var helper = new TestDbHelper(TestConnection);

			var companyPK = helper.InsertCompany("SSC", "Company", "AUD", "AU", true, true);
			var branchPK = helper.InsertBranch("AZB", companyPK);
			var departmentPK = helper.InsertDepartment("AZD");

			var datetime = helper.ToDate("2020-07-03");

			var apPayPK = helper.InsertTransactionHeader("AP", "PAY", "004", 330, datetime, branchPK, departmentPK);
			var draftInvoicePK = helper.InsertAccDraftInvoiceHeader(companyPK, branchPK, departmentPK, apPayPK);
			helper.InsertProcessTasks(draftInvoicePK, "AIH", "EXC", datetime);

			var report = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_WorkflowExceptions('{companyPK}','2020-07-03 00:00:00','2020-07-04 00:00:00')");
			AssertEquals("Result should have rows", 1, report.Rows.Count);
			var row = report.Rows[0];

			CombineAssertions(delegate
			{
				AssertEquals("row['WorkflowType']", "PDT", row["WorkflowType"]);
				AssertEquals("row['PK']", draftInvoicePK, row["PK"]);
				AssertEquals("row['ConsigneePK']", DBNull.Value, row["ConsigneePK"]);
				AssertEquals("row['ConsignorPK']", DBNull.Value, row["ConsignorPK"]);
				AssertEquals("row['JobNumber']", "", row["JobNumber"]);
				AssertEquals("row['OriginPort']", DBNull.Value, row["OriginPort"]);
				AssertEquals("row['DestinationPort']", DBNull.Value, row["DestinationPort"]);
				AssertEquals("row['LoadPort']", DBNull.Value, row["LoadPort"]);
				AssertEquals("row['DischargePort']", DBNull.Value, row["DischargePort"]);
				AssertEquals("row['ContainerMode']", DBNull.Value, row["ContainerMode"]);
				AssertEquals("row['AdditionalDetail']", DBNull.Value, row["AdditionalDetail"]);
				AssertEquals("row['TransportMode']", DBNull.Value, row["TransportMode"]);
				AssertEquals("row['JobModuleID']", "AccDraftInvoiceHeader", row["JobModuleID"]);
				AssertEquals("row['JobModulePK']", draftInvoicePK, row["JobModulePK"]);
				AssertEquals("row['ETA']", DBNull.Value, row["ETA"]);
				AssertEquals("row['OrderNumber']", DBNull.Value, row["OrderNumber"]);
			});
		}

		public void TestAccComplianceReport()
		{
			var helper = new TestDbHelper(TestConnection);
			var companyPK = helper.InsertCompany("SSC", "Company", "AUD", "AU", true, true);
			var datetime = helper.ToDate("2020-07-03");

			var complianceReportPK = helper.InsertComplianceReport("TST", datetime, datetime.AddDays(1), companyPK);
			helper.InsertProcessTasks(complianceReportPK, "ACR", "EXC", datetime);

			var report = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_WorkflowExceptions('{companyPK.ToString()}','2020-07-03 00:00:00','2020-07-04 00:00:00')");
			AssertEquals("Result should have rows", 1, report.Rows.Count);
			var row = report.Rows[0];

			CombineAssertions(delegate
			{
				AssertEquals("row['WorkflowType']", "CTR", row["WorkflowType"]);
				AssertEquals("row['PK']", complianceReportPK, row["PK"]);
				AssertEquals("row['ConsigneePK']", DBNull.Value, row["ConsigneePK"]);
				AssertEquals("row['ConsignorPK']", DBNull.Value, row["ConsignorPK"]);
				AssertEquals("row['JobNumber']", "", row["JobNumber"]);
				AssertEquals("row['OriginPort']", DBNull.Value, row["OriginPort"]);
				AssertEquals("row['DestinationPort']", DBNull.Value, row["DestinationPort"]);
				AssertEquals("row['LoadPort']", DBNull.Value, row["LoadPort"]);
				AssertEquals("row['DischargePort']", DBNull.Value, row["DischargePort"]);
				AssertEquals("row['ContainerMode']", "", row["ContainerMode"]);
				AssertEquals("row['AdditionalDetail']", $"SSC AU TST 7/3/2020 7/4/2020", row["AdditionalDetail"]);
				AssertEquals("row['TransportMode']", "", row["TransportMode"]);
				AssertEquals("row['JobModuleID']", "AccComplianceReport", row["JobModuleID"]);
				AssertEquals("row['JobModulePK']", complianceReportPK, row["JobModulePK"]);
				AssertEquals("row['ETA']", DBNull.Value, row["ETA"]);
				AssertEquals("row['OrderNumber']", "", row["OrderNumber"]);
			});
		}

		public void TestGlbStaffChangeRequest()
		{
			TestConnection.ExecuteScalar(@"
INSERT INTO dbo.GLBSTAFFCHANGEREQUESTTEMPLATE(GSG_PK, GSG_AutoVersion, GSG_TEMPLATENAME, GSG_SystemCreateTimeUtc, GSG_SystemCreateUser, GSG_SystemLastEditTimeUtc, GSG_SystemLastEditUser, GSG_Code)
			VALUES('07504868-381d-472d-8ba8-963ce6422bcd', 0,  'TEMPLATE', GETUTCDATE(), 'E', GETUTCDATE(), 'E', 'ABC')

INSERT INTO dbo.GLBSTAFFCHANGEREQUEST(GCR_PK, GCR_AutoVersion, GCR_Status, GCR_GSG_Template, GCR_SystemCreateTimeUtc, GCR_SystemCreateUser, GCR_SystemLastEditTimeUtc, GCR_SystemLastEditUser)
			VALUES('ec53990b-7eda-40ee-b19a-bb29f0cfbdb9', 0, 'APP', '07504868-381d-472d-8ba8-963ce6422bcd', GETUTCDATE(), 'E', GETUTCDATE(), 'E')

INSERT INTO dbo.PROCESSTASKS(P9_PK, P9_PARENTID, P9_PARENTTABLECODE, P9_TYPE, P9_ACTUALDATE)
			VALUES(NEWID(), 'ec53990b-7eda-40ee-b19a-bb29f0cfbdb9', 'GCR', 'EXC', '2011-08-01 00:00:00')
"
				);

			var report = DataUtils.GetDataTableFromQuery(TestConnection, @"
select PK, WorkflowType, ConsigneePK, ConsignorPK, JobNumber, OriginPort, DestinationPort, LoadPort, DischargePort, ContainerMode, AdditionalDetail, TransportMode, JobModuleID, JobModulePK, ETA, OrderNumber
from Report_WorkflowExceptions('ec53990b-7eda-40ee-b19a-bb29f0cfbdb9','2011-01-01 00:00:00','2011-08-02 00:00:00')");

			AssertEquals("Matching Row Count", 1, report.Rows.Count);
			var row = report.Rows[0];

			CombineAssertions(delegate
			{
				AssertEquals("row['PK']", new Guid("ec53990b-7eda-40ee-b19a-bb29f0cfbdb9"), row["PK"]);
				AssertEquals("row['WorkflowType']", "GCR", row["WorkflowType"]);
				AssertEquals("row['ConsigneePK']", DBNull.Value, row["ConsigneePK"]);
				AssertEquals("row['ConsignorPK']", DBNull.Value, row["ConsignorPK"]);
				AssertEquals("row['JobNumber']", "TEMPLATE APP", row["JobNumber"]);
				AssertEquals("row['OriginPort']", DBNull.Value, row["OriginPort"]);
				AssertEquals("row['DestinationPort']", DBNull.Value, row["DestinationPort"]);
				AssertEquals("row['LoadPort']", DBNull.Value, row["LoadPort"]);
				AssertEquals("row['DischargePort']", DBNull.Value, row["DischargePort"]);
				AssertEquals("row['ContainerMode']", DBNull.Value, row["ContainerMode"]);
				AssertEquals("row['AdditionalDetail']", DBNull.Value, row["AdditionalDetail"]);
				AssertEquals("row['TransportMode']", DBNull.Value, row["TransportMode"]);
				AssertEquals("row['JobModuleID']", "GlbStaffChangeRequest", row["JobModuleID"]);
				AssertEquals("row['JobModulePK']", new Guid("ec53990b-7eda-40ee-b19a-bb29f0cfbdb9"), row["JobModulePK"]);
				AssertEquals("row['ETA']", DBNull.Value, row["ETA"]);
				AssertEquals("row['OrderNumber']", DBNull.Value, row["OrderNumber"]);
			});
		}

		public void TestCusExitHeader()
		{
			var helper = new TestDbHelper(TestConnection);
			var datetime = helper.ToDate("2023-06-23");

			var cusExitHeaderPk = helper.InsertCusExitHeader("REF1234", branchPK, companyPK);
			helper.InsertProcessTasks(cusExitHeaderPk, "CXH", "EXC", datetime);

			var report = DataUtils.GetDataTableFromQuery(TestConnection,
				$"SELECT * FROM Report_WorkflowExceptions('{companyPK}','2023-06-23 00:00:00','2023-06-24 00:00:00')");
			var row = report.Rows[0];

			CombineAssertions(() =>
			{
				AssertEquals("CXH", row["WorkflowType"]);
				AssertEquals(cusExitHeaderPk, row["PK"]);
				AssertEquals(DBNull.Value, row["ConsigneePK"]);
				AssertEquals(DBNull.Value, row["ConsignorPK"]);
				AssertEquals("REF1234", row["JobNumber"]);
				AssertEquals(DBNull.Value, row["OriginPort"]);
				AssertEquals(DBNull.Value, row["DestinationPort"]);
				AssertEquals(DBNull.Value, row["LoadPort"]);
				AssertEquals(DBNull.Value, row["DischargePort"]);
				AssertEquals(DBNull.Value, row["ContainerMode"]);
				AssertEquals(DBNull.Value, row["AdditionalDetail"]);
				AssertEquals(DBNull.Value, row["TransportMode"]);
				AssertEquals("ExitControl", row["JobModuleID"]);
				AssertEquals(cusExitHeaderPk, row["JobModulePK"]);
				AssertEquals(DBNull.Value, row["ETA"]);
				AssertEquals(DBNull.Value, row["OrderNumber"]);
			});
		}

		public void TestReviewProcess()
		{
			TestConnection.ExecuteScalar(@"
INSERT INTO dbo.GlbStaff(GS_PK, GS_CODE, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
			VALUES('0057b0b1-a4ea-4cef-831b-c20887d760d3', 'DEF', GETUTCDATE(), 'E', GETUTCDATE(), 'E')

INSERT INTO dbo.ReviewProcess(RPR_PK, RPR_Name, RPR_EffectiveDate, RPR_SubmissionDate, RPR_RX_NKCurrency, RPR_SystemCreateTimeUtc, RPR_SystemCreateUser, RPR_SystemLastEditTimeUtc, RPR_SystemLastEditUser, RPR_GC_Company)
			VALUES('a610aa7c-ed4b-49e1-ac28-ab6c89ca4f12','RPR Test Name', '2011-08-01 00:00:00', '2011-08-05 00:00:00', 'AUD', GETUTCDATE(), 'E', GETUTCDATE(), 'E', '03052ed3-2c64-49ac-97d8-c6079d5015b5')

INSERT INTO dbo.ReviewProcessEndpoint(RPP_PK, RPP_RPR_ReviewProcess, RPP_GS_Staff, RPP_SystemCreateTimeUtc, RPP_SystemCreateUser, RPP_SystemLastEditTimeUtc, RPP_SystemLastEditUser)
			VALUES(NEWID(),'a610aa7c-ed4b-49e1-ac28-ab6c89ca4f12', '0057b0b1-a4ea-4cef-831b-c20887d760d3', GETUTCDATE(), 'E', GETUTCDATE(), 'E')

INSERT INTO dbo.ProcessTasks(P9_PK, P9_PARENTID, P9_PARENTTABLECODE, P9_TYPE, P9_ACTUALDATE)
			VALUES(NEWID(), 'a610aa7c-ed4b-49e1-ac28-ab6c89ca4f12', 'RPR', 'EXC', '2011-08-01 00:00:00')
"
				);

			var report = DataUtils.GetDataTableFromQuery(TestConnection, @"
select PK, WorkflowType, ConsigneePK, ConsignorPK, JobNumber, OriginPort, DestinationPort, LoadPort, DischargePort, ContainerMode, AdditionalDetail, TransportMode, JobModuleID, JobModulePK, ETA, OrderNumber
from Report_WorkflowExceptions('a610aa7c-ed4b-49e1-ac28-ab6c89ca4f12','2011-01-01 00:00:00','2011-08-06 00:00:00')");

			AssertEquals("Matching Row Count", 1, report.Rows.Count);
			var row = report.Rows[0];

			CombineAssertions(delegate
			{
				AssertEquals("row['PK']", new Guid("a610aa7c-ed4b-49e1-ac28-ab6c89ca4f12"), row["PK"]);
				AssertEquals("row['WorkflowType']", "RPR", row["WorkflowType"]);
				AssertEquals("row['ConsigneePK']", DBNull.Value, row["ConsigneePK"]);
				AssertEquals("row['ConsignorPK']", DBNull.Value, row["ConsignorPK"]);
				AssertEquals("row['JobNumber']", "RPR Test Name", row["JobNumber"]);
				AssertEquals("row['OriginPort']", DBNull.Value, row["OriginPort"]);
				AssertEquals("row['DestinationPort']", DBNull.Value, row["DestinationPort"]);
				AssertEquals("row['LoadPort']", DBNull.Value, row["LoadPort"]);
				AssertEquals("row['DischargePort']", DBNull.Value, row["DischargePort"]);
				AssertEquals("row['ContainerMode']", DBNull.Value, row["ContainerMode"]);
				AssertEquals("row['AdditionalDetail']", DBNull.Value, row["AdditionalDetail"]);
				AssertEquals("row['TransportMode']", DBNull.Value, row["TransportMode"]);
				AssertEquals("row['JobModuleID']", "ReviewProcess", row["JobModuleID"]);
				AssertEquals("row['JobModulePK']", new Guid("a610aa7c-ed4b-49e1-ac28-ab6c89ca4f12"), row["JobModulePK"]);
				AssertEquals("row['ETA']", DBNull.Value, row["ETA"]);
				AssertEquals("row['OrderNumber']", DBNull.Value, row["OrderNumber"]);
			});
		}

		public void TestReviewProcessNode()
		{
			TestConnection.ExecuteScalar(@"
INSERT INTO dbo.GlbStaff(GS_PK, GS_CODE, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
			VALUES('0057b0b1-a4ea-4cef-831b-c20887d760d3', 'DEF', GETUTCDATE(), 'E', GETUTCDATE(), 'E')

INSERT INTO dbo.ReviewProcess(RPR_PK, RPR_Name, RPR_EffectiveDate, RPR_SubmissionDate, RPR_RX_NKCurrency, RPR_SystemCreateTimeUtc, RPR_SystemCreateUser, RPR_SystemLastEditTimeUtc, RPR_SystemLastEditUser, RPR_GC_Company)
			VALUES('a610aa7c-ed4b-49e1-ac28-ab6c89ca4f12','RPR Test Name', '2011-08-01 00:00:00', '2011-08-05 00:00:00', 'AUD', GETUTCDATE(), 'E', GETUTCDATE(), 'E', '03052ed3-2c64-49ac-97d8-c6079d5015b5')

INSERT INTO [dbo].[ReviewProcessNode]([RRN_PK], [RRN_RPR_ReviewProcess], [RRN_GS_Reviewer], [RRN_RRN_Parent], [RRN_Status], [RRN_SystemCreateTimeUtc], [RRN_SystemCreateUser], [RRN_SystemLastEditTimeUtc], [RRN_SystemLastEditUser])
			VALUES ('4cdc13da-b2ac-4e96-906e-f0047d126df4', 'a610aa7c-ed4b-49e1-ac28-ab6c89ca4f12', '0057b0b1-a4ea-4cef-831b-c20887d760d3', NULL, 'ASN', GETUTCDATE(), 'E', GETUTCDATE(), 'E')

INSERT INTO dbo.ProcessTasks(P9_PK, P9_PARENTID, P9_PARENTTABLECODE, P9_TYPE, P9_ACTUALDATE)
			VALUES(NEWID(), '4cdc13da-b2ac-4e96-906e-f0047d126df4', 'P0', 'EXC', '2011-08-01 00:00:00')
"
				);

			var report = DataUtils.GetDataTableFromQuery(TestConnection, @"
select PK, WorkflowType, ConsigneePK, ConsignorPK, JobNumber, OriginPort, DestinationPort, LoadPort, DischargePort, ContainerMode, AdditionalDetail, TransportMode, JobModuleID, JobModulePK, ETA, OrderNumber
from Report_WorkflowExceptions('4cdc13da-b2ac-4e96-906e-f0047d126df4','2011-01-01 00:00:00','2011-08-06 00:00:00')");

			AssertEquals("Matching Row Count", 1, report.Rows.Count);
			var row = report.Rows[0];

			CombineAssertions(delegate
			{
				AssertEquals("row['PK']", new Guid("4cdc13da-b2ac-4e96-906e-f0047d126df4"), row["PK"]);
				AssertEquals("row['WorkflowType']", "RPN", row["WorkflowType"]);
				AssertEquals("row['ConsigneePK']", DBNull.Value, row["ConsigneePK"]);
				AssertEquals("row['ConsignorPK']", DBNull.Value, row["ConsignorPK"]);
				AssertEquals("row['JobNumber']", "RPR Test Name - DEF", row["JobNumber"]);
				AssertEquals("row['OriginPort']", DBNull.Value, row["OriginPort"]);
				AssertEquals("row['DestinationPort']", DBNull.Value, row["DestinationPort"]);
				AssertEquals("row['LoadPort']", DBNull.Value, row["LoadPort"]);
				AssertEquals("row['DischargePort']", DBNull.Value, row["DischargePort"]);
				AssertEquals("row['ContainerMode']", DBNull.Value, row["ContainerMode"]);
				AssertEquals("row['AdditionalDetail']", DBNull.Value, row["AdditionalDetail"]);
				AssertEquals("row['TransportMode']", DBNull.Value, row["TransportMode"]);
				AssertEquals("row['JobModuleID']", "ReviewProcessNode", row["JobModuleID"]);
				AssertEquals("row['JobModulePK']", new Guid("4cdc13da-b2ac-4e96-906e-f0047d126df4"), row["JobModulePK"]);
				AssertEquals("row['ETA']", DBNull.Value, row["ETA"]);
				AssertEquals("row['OrderNumber']", DBNull.Value, row["OrderNumber"]);
			});
		}

		public void TestScheduledDateValueWhenMultipleMilestonesWithSameEventCode()
		{
			var depPk1 = "C0CFD1A4-8E6A-4BB0-9C86-6BF01D211DA0";
			var depPk2 = "0FD844C9-0008-45F9-8E05-17651FD699D3";
			var arvPk1 = "722BAD82-33E8-41FC-9AC4-9EFAAA272284";
			var arvPk2 = "E8529965-0286-4B46-99D9-6109C3E39D9B";
			var parentId = "515107C7-B98C-4B4F-933B-F03675FCB779";

			TestConnection.ExecuteScalar(@$"
INSERT INTO dbo.GlbStaff(GS_PK, GS_CODE, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
			VALUES('0057b0b1-a4ea-4cef-831b-c20887d760d3', 'DEF', GETUTCDATE(), 'E', GETUTCDATE(), 'E')

INSERT INTO dbo.JobConsol(JK_PK, JK_UniqueConsignRef, JK_SystemCreateTimeUtc, JK_SystemCreateUser, JK_SystemLastEditTimeUtc, JK_SystemLastEditUser)
			VALUES('{parentId}','TESTING', GETUTCDATE(), 'E', GETUTCDATE(), 'E')

INSERT INTO dbo.ProcessTasks(P9_PK, P9_PARENTID, P9_PARENTTABLECODE, P9_TYPE, P9_ACTUALDATE)
			VALUES(NEWID(), '{parentId}', 'JK', 'EXC', '2011-08-01 00:00:00')

INSERT INTO dbo.ProcessTasks(P9_PK, P9_PARENTID, P9_PARENTTABLECODE, P9_TYPE, P9_SE_NKMilestoneEvent, P9_SE_NKExceptionEvent, P9_Description, P9_ScheduledDate, P9_ScheduledDateUtc)
			VALUES(NEWID(), '{parentId}', 'JK', 'MIL', 'DEP', 'EXC', 'Departure from First Load Port', '2022-09-18 01:20:00', '2022-09-18 01:20:00')
INSERT INTO dbo.ProcessTasks(P9_PK, P9_PARENTID, P9_PARENTTABLECODE, P9_TYPE, P9_SE_NKMilestoneEvent, P9_SE_NKExceptionEvent, P9_Description, P9_ScheduledDate, P9_ScheduledDateUtc)
			VALUES(NEWID(), '{parentId}', 'JK', 'MIL', 'DEP', 'EXC', '2nd Transport Leg Departure', '2022-09-19 11:30:00', '2022-09-19 11:30:00')
INSERT INTO dbo.ProcessTasks(P9_PK, P9_PARENTID, P9_PARENTTABLECODE, P9_TYPE, P9_SE_NKMilestoneEvent, P9_SE_NKExceptionEvent, P9_Description, P9_ScheduledDate, P9_ScheduledDateUtc)
			VALUES(NEWID(), '{parentId}', 'JK', 'MIL', 'ARV', 'EXC', '1st Transport Leg Arrival', '2022-09-18 01:20:00', '2022-09-18 01:20:00')
INSERT INTO dbo.ProcessTasks(P9_PK, P9_PARENTID, P9_PARENTTABLECODE, P9_TYPE, P9_SE_NKMilestoneEvent, P9_SE_NKExceptionEvent, P9_Description, P9_ScheduledDate, P9_ScheduledDateUtc)
			VALUES(NEWID(), '{parentId}', 'JK', 'MIL', 'ARV', 'EXC', 'Arrival at Final Discharge Port', '2022-09-19 21:30:00', '2022-09-19 21:30:00')

INSERT INTO dbo.ProcessTasks(P9_PK, P9_PARENTID, P9_PARENTTABLECODE, P9_TYPE, P9_SE_NKMilestoneEvent, P9_SE_NKExceptionEvent, P9_Description, P9_ActualDate)
			VALUES('{depPk1}', '{parentId}', 'JK', 'EXC', 'DEP', 'EXC', 'Departure from First Load Port', '2022-09-19 02:05:57')
INSERT INTO dbo.ProcessTasks(P9_PK, P9_PARENTID, P9_PARENTTABLECODE, P9_TYPE, P9_SE_NKMilestoneEvent, P9_SE_NKExceptionEvent, P9_Description, P9_ActualDate)
			VALUES('{depPk2}', '{parentId}', 'JK', 'EXC', 'DEP', 'EXC', '2nd Transport Leg Departure', '2022-09-20 02:05:57')
INSERT INTO dbo.ProcessTasks(P9_PK, P9_PARENTID, P9_PARENTTABLECODE, P9_TYPE, P9_SE_NKMilestoneEvent, P9_SE_NKExceptionEvent, P9_Description, P9_ActualDate)
			VALUES('{arvPk1}', '{parentId}', 'JK', 'EXC', 'ARV', 'EXC', '1st Transport Leg Arrival', '2022-09-20 02:05:57')
INSERT INTO dbo.ProcessTasks(P9_PK, P9_PARENTID, P9_PARENTTABLECODE, P9_TYPE, P9_SE_NKMilestoneEvent, P9_SE_NKExceptionEvent, P9_Description, P9_ActualDate)
			VALUES('{arvPk2}', '{parentId}', 'JK', 'EXC', 'ARV', 'EXC', 'Arrival at Final Discharge Port', '2022-09-21 02:05:57')
"
			);

			var report = DataUtils.GetDataTableFromQuery(TestConnection, @"
select P9_PK, Description, ScheduledDate
from Report_WorkflowExceptions('4cdc13da-b2ac-4e96-906e-f0047d126df4','2022-09-18 00:00:00','2022-09-22 00:00:00')");

			var depEvent1Row = report.Select($"P9_PK='{depPk1}'").FirstOrDefault();
			var depEvent2Row = report.Select($"P9_PK='{depPk2}'").FirstOrDefault();
			var arvEvent1Row = report.Select($"P9_PK='{arvPk1}'").FirstOrDefault();
			var arvEvent2Row = report.Select($"P9_PK='{arvPk2}'").FirstOrDefault();

			CombineAssertions("Check ScheduledDate", () =>
			{
				AssertNotNull(depEvent1Row);
				AssertEquals("DEP-Departure from First Load Port", new DateTime(2022, 09, 18, 1, 20, 0), depEvent1Row!["ScheduledDate"]);

				AssertNotNull(depEvent2Row);
				AssertEquals("DEP-2nd Transport Leg Departure", new DateTime(2022, 09, 19, 11, 30, 0), depEvent2Row!["ScheduledDate"]);

				AssertNotNull(arvEvent1Row);
				AssertEquals("ARV-1st Transport Leg Arrival", new DateTime(2022, 09, 18, 1, 20, 0), arvEvent1Row!["ScheduledDate"]);

				AssertNotNull(arvEvent2Row);
				AssertEquals("ARV-Arrival at Final Discharge Port", new DateTime(2022, 09, 19, 21, 30, 0), arvEvent2Row!["ScheduledDate"]);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			companyPK = (Guid)TestConnection.ExecuteScalar("select top 1 GC_PK from dbo.GlbCompany inner join dbo.glbBranch on gb_gc=gc_pk");
			branchPK = (Guid)TestConnection.ExecuteScalar("select top 1 GB_PK from dbo.GlbBranch where gb_gc = '" + companyPK + "'");
		}

		Guid companyPK;
		Guid branchPK;
	}
}

