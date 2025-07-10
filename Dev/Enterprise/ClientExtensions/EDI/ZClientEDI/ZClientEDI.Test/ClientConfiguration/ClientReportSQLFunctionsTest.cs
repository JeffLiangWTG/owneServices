using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Database.Abstractions.Extensions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.DbUpgrader.Shared;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Test
{
	public class ClientReportSQLFunctionsTest : TestCaseWithFactory
	{
		public void TestClientReport_StaffLongServiceYears_LongPeriod()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_EmploymentDate = ZDateTime.Today.AddYears(-2).AddMonths(-5);
			staff2.GS_IsActive = true;
			staff2.GS_GB_HomeBranch = branch.PK;
			var staff5 = Factory.NewWithValidTestData<GlbStaff>();
			staff5.GS_EmploymentDate = ZDateTime.Today.AddYears(-5).AddMonths(-2);
			staff5.GS_IsActive = true;
			staff5.GS_GB_HomeBranch = branch.PK;
			var staff25 = Factory.NewWithValidTestData<GlbStaff>();
			staff25.GS_EmploymentDate = ZDateTime.Today.AddYears(-25).AddMonths(-3);
			staff25.GS_IsActive = true;
			staff25.GS_GB_HomeBranch = branch.PK;
			var staff45 = Factory.NewWithValidTestData<GlbStaff>();
			staff45.GS_EmploymentDate = ZDateTime.Today.AddYears(-45).AddMonths(-7);
			staff45.GS_IsActive = true;
			staff45.GS_GB_HomeBranch = branch.PK;
			Factory.Save();
			using (var dataTable = Utilities.GetDataTableFromQuery($"SELECT * FROM ClientReport_StaffLongServiceYears('{ZDateTime.Today.AddYears(-50)}', '{ZDateTime.Today.AddDays(1)}') order by ServiceYear"))
			{
				AssertEquals(4, dataTable.Rows.Count);
				AssertEquals(staff2.GS_Code, dataTable.Rows[0]["GS_Code"].ToString());
				AssertEquals("2", dataTable.Rows[0]["ServiceYear"].ToString());
				AssertEquals("2 Years", dataTable.Rows[0]["LongServicePeriod"].ToString());
				AssertEquals(staff5.GS_Code, dataTable.Rows[1]["GS_Code"].ToString());
				AssertEquals("5", dataTable.Rows[1]["ServiceYear"].ToString());
				AssertEquals("5 Years", dataTable.Rows[1]["LongServicePeriod"].ToString());
				AssertEquals(staff25.GS_Code, dataTable.Rows[2]["GS_Code"].ToString());
				AssertEquals("25", dataTable.Rows[2]["ServiceYear"].ToString());
				AssertEquals("25 Years", dataTable.Rows[2]["LongServicePeriod"].ToString());
				AssertEquals(staff45.GS_Code, dataTable.Rows[3]["GS_Code"].ToString());
				AssertEquals("45", dataTable.Rows[3]["ServiceYear"].ToString());
				AssertEquals("45 Years", dataTable.Rows[3]["LongServicePeriod"].ToString());
			}
		}

		public void TestClientReport_StaffLongServiceYears()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var staff0 = Factory.NewWithValidTestData<GlbStaff>();
			staff0.GS_EmploymentDate = ZDateTime.Today;
			staff0.GS_IsActive = true;
			staff0.GS_GB_HomeBranch = branch.PK;
			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_EmploymentDate = ZDateTime.Today.AddYears(-3);
			staff3.GS_IsActive = true;
			staff3.GS_GB_HomeBranch = branch.PK;
			var staff5 = Factory.NewWithValidTestData<GlbStaff>();
			staff5.GS_EmploymentDate = ZDateTime.Today.AddYears(-5);
			staff5.GS_IsActive = true;
			staff5.GS_GB_HomeBranch = branch.PK;
			var staff10 = Factory.NewWithValidTestData<GlbStaff>();
			staff10.GS_EmploymentDate = ZDateTime.Today.AddYears(-10);
			staff10.GS_IsActive = true;
			staff10.GS_GB_HomeBranch = branch.PK;
			var staff20 = Factory.NewWithValidTestData<GlbStaff>();
			staff20.GS_EmploymentDate = ZDateTime.Today.AddYears(-20);
			staff20.GS_IsActive = true;
			staff20.GS_GB_HomeBranch = branch.PK;
			var staff25 = Factory.NewWithValidTestData<GlbStaff>();
			staff25.GS_EmploymentDate = ZDateTime.Today.AddYears(-25);
			staff25.GS_IsActive = true;
			staff25.GS_GB_HomeBranch = branch.PK;
			var staff30 = Factory.NewWithValidTestData<GlbStaff>();
			staff30.GS_EmploymentDate = ZDateTime.Today.AddYears(-30);
			staff30.GS_IsActive = true;
			staff30.GS_GB_HomeBranch = branch.PK;
			var staff35 = Factory.NewWithValidTestData<GlbStaff>();
			staff35.GS_EmploymentDate = ZDateTime.Today.AddYears(-35);
			staff35.GS_IsActive = true;
			staff35.GS_GB_HomeBranch = branch.PK;
			var staff40 = Factory.NewWithValidTestData<GlbStaff>();
			staff40.GS_EmploymentDate = ZDateTime.Today.AddYears(-40);
			staff40.GS_IsActive = true;
			staff40.GS_GB_HomeBranch = branch.PK;
			var staff45 = Factory.NewWithValidTestData<GlbStaff>();
			staff45.GS_EmploymentDate = ZDateTime.Today.AddYears(-45);
			staff45.GS_IsActive = true;
			staff45.GS_GB_HomeBranch = branch.PK;
			var staff50 = Factory.NewWithValidTestData<GlbStaff>();
			staff50.GS_EmploymentDate = ZDateTime.Today.AddYears(-50);
			staff50.GS_IsActive = true;
			staff50.GS_GB_HomeBranch = branch.PK;
			Factory.Save();
			using (var dataTable = Utilities.GetDataTableFromQuery($"SELECT * FROM ClientReport_StaffLongServiceYears('{ZDateTime.Today.AddDays(-1)}', '{ZDateTime.Today.AddDays(+30)}') order by ServiceYear"))
			{
				AssertEquals(9, dataTable.Rows.Count);
				AssertEquals(staff5.GS_Code, dataTable.Rows[0]["GS_Code"].ToString());
				AssertEquals("5", dataTable.Rows[0]["ServiceYear"].ToString());
				AssertEquals("5 Years", dataTable.Rows[0]["LongServicePeriod"].ToString());
				AssertEquals(staff10.GS_Code, dataTable.Rows[1]["GS_Code"].ToString());
				AssertEquals("10", dataTable.Rows[1]["ServiceYear"].ToString());
				AssertEquals("10 Years", dataTable.Rows[1]["LongServicePeriod"].ToString());
				AssertEquals(staff20.GS_Code, dataTable.Rows[2]["GS_Code"].ToString());
				AssertEquals("20", dataTable.Rows[2]["ServiceYear"].ToString());
				AssertEquals("20 Years", dataTable.Rows[2]["LongServicePeriod"].ToString());
				AssertEquals(staff25.GS_Code, dataTable.Rows[3]["GS_Code"].ToString());
				AssertEquals("25", dataTable.Rows[3]["ServiceYear"].ToString());
				AssertEquals("25 Years", dataTable.Rows[3]["LongServicePeriod"].ToString());
				AssertEquals(staff30.GS_Code, dataTable.Rows[4]["GS_Code"].ToString());
				AssertEquals("30", dataTable.Rows[4]["ServiceYear"].ToString());
				AssertEquals("30 Years", dataTable.Rows[4]["LongServicePeriod"].ToString());
				AssertEquals(staff35.GS_Code, dataTable.Rows[5]["GS_Code"].ToString());
				AssertEquals("35", dataTable.Rows[5]["ServiceYear"].ToString());
				AssertEquals("35 Years", dataTable.Rows[5]["LongServicePeriod"].ToString());
				AssertEquals(staff40.GS_Code, dataTable.Rows[6]["GS_Code"].ToString());
				AssertEquals("40", dataTable.Rows[6]["ServiceYear"].ToString());
				AssertEquals("40 Years", dataTable.Rows[6]["LongServicePeriod"].ToString());
				AssertEquals(staff45.GS_Code, dataTable.Rows[7]["GS_Code"].ToString());
				AssertEquals("45", dataTable.Rows[7]["ServiceYear"].ToString());
				AssertEquals("45 Years", dataTable.Rows[7]["LongServicePeriod"].ToString());
				AssertEquals(staff50.GS_Code, dataTable.Rows[8]["GS_Code"].ToString());
				AssertEquals("50", dataTable.Rows[8]["ServiceYear"].ToString());
				AssertEquals("50 Years", dataTable.Rows[8]["LongServicePeriod"].ToString());
			}
		}

		[TestUtcOffset(10, 0, 0)]
		public void TestClientcsfn__CSIClosuresEventStaffSummary_IncludesIncidentsThatAreClosedImmediately()
		{
			var enterpriseSupportGroup = Factory.NewWithValidTestData<GlbGroup>();
			enterpriseSupportGroup.GG_Code = "ENTSUP";
			var supportStaff = enterpriseSupportGroup.Staff.AddNew();
			supportStaff.GS_Code = "ADL";
			Factory.Save();
			using (Env.SetTemporaryUserContext(supportStaff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var incident = Factory.NewWithValidTestData<SupportIncident>();
				incident.CloseIncident("", "");
				Factory.Save();
			}

			var selectSql = "SELECT IM_SL_EventCount FROM Clientcsfn__CSIClosuresEventStaffSummary('01 JAN 1900', '30 JUN 2076', '01 JAN 1900', '30 JUN 2076', null, null, null, null, null, null, null) WHERE GS_PK = @GS_PK";
			using (var command = TestConnection.Command(selectSql))
			{
				command.AddParameter("@GS_PK", SqlDbType.UniqueIdentifier, supportStaff.PK.ToGuid());
				using (var reader = command.ExecuteReader())
				{
					Assert("Should have returned 1 row", reader.Read());
					Assert("Should have at least 1 event", (int)reader["IM_SL_EventCount"] > 0);
					Assert("Should have returned 1 row", !reader.Read());
				}
			}
		}

		public void TestClientcsfn__CSIClosuresEventStaffSummary_IncludesAllStaff()
		{
			var enterpriseSupportGroup = Factory.NewWithValidTestData<GlbGroup>();
			enterpriseSupportGroup.GG_Code = "ENTSUP";
			var supportStaff = enterpriseSupportGroup.Staff.AddNew();
			var supportStaff2 = enterpriseSupportGroup.Staff.AddNew();
			supportStaff.GS_LoginName = "ADL";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "not.in.any.group";
			Factory.Save();
			using (Env.SetTemporaryUserContext(supportStaff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var incident = Factory.NewWithValidTestData<SupportIncident>();
				incident.CloseIncident("", "");
				Factory.Save();
			}

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var incident = Factory.NewWithValidTestData<SupportIncident>();
				incident.CloseIncident("", "");
				Factory.Save();
			}

			var selectSql = "SELECT IM_SL_EventCount FROM Clientcsfn__CSIClosuresEventStaffSummary('01 JAN 1900', '30 JUN 2076', '01 JAN 1900', '30 JUN 2076', null, null, null, null, null, null, null)";
			using (var command = TestConnection.Command(selectSql))
			{
				using (var reader = command.ExecuteReader())
				{
					Assert("Should have returned first row", reader.Read());
					Assert("Should have at least 2 event", (int)reader["IM_SL_EventCount"] > 0);
					Assert("Should have returned second row", reader.Read());
					Assert("Should have returned 2 rows", !reader.Read());
				}
			}
		}

		[ExpectNoExceptions]
		public void TestClientReport_ClientIncidentSummary()
		{
			var query = "SELECT * FROM ClientReport_ClientIncidentSummary(newid(), '', newid(), '')";
			using (DbCommand command = TestConnection.Command(query))
			{
				command.ExecuteNonQuery();
			}

			var queryIncludesManagement = "SELECT * FROM ClientReport_ClientIncidentSummary(newid(), '', newid(), 'Y')";
			using (DbCommand command = TestConnection.Command(queryIncludesManagement))
			{
				command.ExecuteNonQuery();
			}
		}

		public void TestClientReport_IncidentSummaryLastTaskClosedByFullName()
		{
			var query = "SELECT * FROM ClientReport_ClientIncidentSummary(newid(), '', newid(), '')";
			using (DbCommand command = TestConnection.Command(query))
			{
				using (var reader = command.ExecuteReader())
				{
					List<string> columns = new List<string>();
					var schema = reader.GetSchemaTable();
					foreach (DataRow colName in schema.Rows)
					{
						columns.Add(colName.ItemArray[0].ToString());
					}

					AssertEquals("LastTaskClosedByFullName Exists", true, columns.Contains("LastTaskClosedByFullName"));
				}
			}
		}

		public void TestClientReport_IncidentSummaryLastTaskClosedByCode()
		{
			var query = "SELECT * FROM ClientReport_ClientIncidentSummary(newid(), '', newid(), '')";
			using (DbCommand command = TestConnection.Command(query))
			{
				using (var reader = command.ExecuteReader())
				{
					List<string> columns = new List<string>();
					var schema = reader.GetSchemaTable();
					foreach (DataRow colName in schema.Rows)
					{
						columns.Add(colName.ItemArray[0].ToString());
					}

					AssertEquals("LastTaskClosedByGSCode Exists", true, columns.Contains("LastTaskClosedByCode"));
				}
			}
		}

		public void TestClientReport_IncidentSummaryIM_Product()
		{
			var query = "SELECT * FROM ClientReport_ClientIncidentSummary(newid(), '', newid(), '')";
			using (DbCommand command = TestConnection.Command(query))
			{
				using (var reader = command.ExecuteReader())
				{
					List<string> columns = new List<string>();
					var schema = reader.GetSchemaTable();
					foreach (DataRow colName in schema.Rows)
					{
						columns.Add(colName.ItemArray[0].ToString());
					}

					AssertEquals("LastTaskClosedByGSCode Exists", true, columns.Contains("IM_Product"));
				}
			}
		}

		[CargoWise.Data.Testing.UseSnapshotProtection]
		public void TestClientReport_IncidentSummaryLastTaskCloseByPopulateOneWI()
		{
			Guid companyPK = new Guid();
			//Create Organisation
			OrgHeader orgHead = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			//Create Staff Assignment for Organisation
			OrgStaffAssignments orgStaffAssign = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssign.O8_OH = orgHead.PK;
			orgStaffAssign.O8_Role = "RM1";
			orgStaffAssign.O8_GC = companyPK;
			orgStaffAssign.O8_GS_NKPersonResponsible = "ABC";
			//Create Incident, and assign to Organisation
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Product = "ENT";
			incident.IM_Module = "";
			incident.IM_Description = "";
			incident.IM_IncidentType = "inc";
			incident.IM_Status = "OPN";
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Support;
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Open.AddedAwaitingAssignment;
			incident.IM_Source = SupportIncidentLookups.SourceListConstants.ERequestPortal;
			incident.IM_OH_Client = orgHead.PK;
			//Create WI, and assign to Incident
			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();
			incident.PopulateWorkItem(workItem);
			incident.RelatedItems.Add(workItem);
			workItem.WKI_Status = "OPN";
			//Create Staff
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Walter White";
			staff.GS_Code = "WW";
			staff.StaffPlainTextPassword = "blah";
			staff.GS_LoginName = "wawh";
			staff.GS_Birthdate = new ZDate(2007, 01, 17);
			staff.GS_UserAddress1 = "Alberq";
			staff.GS_City = "New Mexico";
			staff.GS_IsOperational = true;
			Factory.Save();
			//Create Tasks, and assign to Staff + WI
			ProcessTask processTaskOpen = Factory.NewWithValidTestData<ProcessTask>();
			processTaskOpen.P9_Sequence = 0;
			processTaskOpen.P9_ParentID = workItem.PK;
			processTaskOpen.P9_GS_NKAssignedStaffMember = "WW";
			processTaskOpen.P9_ParentTableCode = "WKI";
			ProcessTask processTaskClosed = Factory.NewWithValidTestData<ProcessTask>();
			processTaskClosed.P9_Sequence = 1;
			processTaskClosed.P9_ParentID = workItem.PK;
			processTaskClosed.P9_GS_NKAssignedStaffMember = "WW";
			processTaskClosed.P9_ParentTableCode = "WKI";
			Factory.Save();
			//Generate status for tasks
			processTaskOpen.P9_Status = "OPN";
			processTaskClosed.P9_Status = "CLS";
			Factory.Save();
			using (var dataTable = Utilities.GetDataTableFromQuery($"SELECT * FROM ClientReport_ClientIncidentSummary('{companyPK}', '', null, '')"))
			{
				LinkedList<string> cellValues = new LinkedList<string>();
				foreach (DataRow row in dataTable.Rows)
				{
					foreach (DataColumn column in dataTable.Columns)
					{
						cellValues.AddLast(row[column].ToString());
					}
				}

				Assert("LastTaskClosedByCode populated", cellValues.Contains("WW"));
				Assert("LastTaskClosedByFullName populated", cellValues.Contains("Walter White"));
			}
		}

		[CargoWise.Data.Testing.UseSnapshotProtection]
		public void TestClientReport_IncidentSummary_table_GenPivot_correct_direction()
		{
			// table:GenPivot, do not use the direction : XX_Relation1TableCode = 'WKI' AND XX_Relation2TableCode = 'IM'

			Guid companyPK = new Guid();
			//Create Organisation
			OrgHeader orgHead = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader orgHead2 = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			//Create Staff Assignment for Organisation
			OrgStaffAssignments orgStaffAssign = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssign.O8_OH = orgHead.PK;
			orgStaffAssign.O8_Role = "RM1";
			orgStaffAssign.O8_GC = companyPK;
			orgStaffAssign.O8_GS_NKPersonResponsible = "ABC";
			OrgStaffAssignments orgStaffAssign2 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssign2.O8_OH = orgHead2.PK;
			orgStaffAssign2.O8_Role = "RM1";
			orgStaffAssign2.O8_GC = companyPK;
			orgStaffAssign2.O8_GS_NKPersonResponsible = "ABC";
			Factory.Save();
			//Create Incident, and assign to Organisation
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Product = "ENT";
			incident.IM_Module = "";
			incident.IM_Description = "";
			incident.IM_IncidentType = "inc";
			incident.IM_Status = "OPN";
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Support;
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Open.AddedAwaitingAssignment;
			incident.IM_Source = SupportIncidentLookups.SourceListConstants.ERequestPortal;
			incident.IM_OH_Client = orgHead.PK;
			SupportIncident incident2 = Factory.NewWithValidTestData<SupportIncident>();
			incident2.IM_Product = "ENT";
			incident2.IM_Module = "";
			incident2.IM_Description = "";
			incident2.IM_IncidentType = "inc";
			incident2.IM_Status = "OPN";
			incident2.IM_Category = SupportIncidentCategoriesList.Codes.Support;
			incident2.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Open.AddedAwaitingAssignment;
			incident2.IM_Source = SupportIncidentLookups.SourceListConstants.ERequestPortal;
			incident2.IM_OH_Client = orgHead2.PK;
			Factory.Save();
			//Create WI, and assign to Incident
			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();
			incident.PopulateWorkItem(workItem);
			incident.RelatedItems.Add(workItem);
			workItem.WKI_Status = "OPN";
			Factory.Save();
			//Create Staff
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Walter White";
			staff.GS_Code = "WW";
			staff.StaffPlainTextPassword = "blah";
			staff.GS_LoginName = "wawh";
			staff.GS_Birthdate = new ZDate(2007, 01, 17);
			staff.GS_UserAddress1 = "Alberq";
			staff.GS_City = "New Mexico";
			staff.GS_IsOperational = true;
			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_FullName = "Walter White2";
			staff2.GS_Code = "WW2";
			staff2.StaffPlainTextPassword = "blah";
			staff2.GS_LoginName = "wawh2";
			staff2.GS_Birthdate = new ZDate(2007, 01, 17);
			staff2.GS_UserAddress1 = "Alberq";
			staff2.GS_City = "New Mexico";
			staff2.GS_IsOperational = true;
			Factory.Save();
			//Create Tasks, and assign to Staff + WI
			ProcessTask processTaskOpen = Factory.NewWithValidTestData<ProcessTask>();
			processTaskOpen.P9_Sequence = 0;
			processTaskOpen.P9_ParentID = workItem.PK;
			processTaskOpen.P9_GS_NKAssignedStaffMember = "WW";
			processTaskOpen.P9_ParentTableCode = "WKI";
			ProcessTask processTaskOpen2 = Factory.NewWithValidTestData<ProcessTask>();
			processTaskOpen2.P9_Sequence = 0;
			//processTaskOpen2.P9_ParentID = workItem2.PK;
			processTaskOpen2.P9_GS_NKAssignedStaffMember = "WW2";
			processTaskOpen2.P9_ParentTableCode = "WKI";
			ProcessTask processTaskClosed = Factory.NewWithValidTestData<ProcessTask>();
			processTaskClosed.P9_Sequence = 1;
			processTaskClosed.P9_ParentID = workItem.PK;
			processTaskClosed.P9_GS_NKAssignedStaffMember = "WW";
			processTaskClosed.P9_ParentTableCode = "WKI";
			ProcessTask processTaskClosed2 = Factory.NewWithValidTestData<ProcessTask>();
			processTaskClosed2.P9_Sequence = 1;
			processTaskClosed2.P9_GS_NKAssignedStaffMember = "WW2";
			processTaskClosed2.P9_ParentTableCode = "WKI";
			Factory.Save();
			//Generate status for tasks
			processTaskOpen.P9_Status = "OPN";
			processTaskClosed.P9_Status = "CLS";
			processTaskOpen2.P9_Status = "OPN";
			processTaskClosed2.P9_Status = "CLS";
			Factory.Save();
			using (var dataTable = Utilities.GetDataTableFromQuery($"SELECT * FROM ClientReport_ClientIncidentSummary('{companyPK}', '', null, '')"))
			{
				LinkedList<string> cellValues = new LinkedList<string>();
				Dictionary<string, LinkedList<string>> hashMap = new Dictionary<string, LinkedList<string>>();
				foreach (DataRow row in dataTable.Rows)
				{
					foreach (DataColumn column in dataTable.Columns)
					{
						if ("LastTaskClosedByCode".Equals(column.ColumnName))
						{
							cellValues.AddLast(row[column].ToString());
						}
					}
				}

				AssertEquals("processTaskOpen.P9_GS_NKAssignedStaffMember contains", true, cellValues.Contains(processTaskOpen.P9_GS_NKAssignedStaffMember.ToString()));
				AssertEquals("processTaskOpen2.P9_GS_NKAssignedStaffMember not contains", false, cellValues.Contains(processTaskOpen2.P9_GS_NKAssignedStaffMember.ToString()));
			}
		}

		[CargoWise.Data.Testing.UseSnapshotProtection]
		public void TestClientReport_IncidentSummaryLastTaskCloseByPopulateMultipleWI()
		{
			Guid companyPK = new Guid();
			//Create Organisation
			OrgHeader orgHead = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			//Create Staff Assignment for Organisation
			OrgStaffAssignments orgStaffAssign = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssign.O8_OH = orgHead.PK;
			orgStaffAssign.O8_Role = "RM1";
			orgStaffAssign.O8_GC = companyPK;
			orgStaffAssign.O8_GS_NKPersonResponsible = "ABC";
			//Create Incident, and assign to Organisation
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Product = "ENT";
			incident.IM_Module = "";
			incident.IM_Description = "";
			incident.IM_IncidentType = "inc";
			incident.IM_Status = "OPN";
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Support;
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Open.AddedAwaitingAssignment;
			incident.IM_Source = SupportIncidentLookups.SourceListConstants.ERequestPortal;
			incident.IM_OH_Client = orgHead.PK;
			//Create WIs, and assign to Incident
			NewWorkItem workItemMostRecent = Factory.NewWithValidTestData<NewWorkItem>();
			incident.PopulateWorkItem(workItemMostRecent);
			incident.RelatedItems.Add(workItemMostRecent);
			workItemMostRecent.WKI_Status = "OPN";
			NewWorkItem workItemLeastRecent = Factory.NewWithValidTestData<NewWorkItem>();
			incident.PopulateWorkItem(workItemLeastRecent);
			incident.RelatedItems.Add(workItemLeastRecent);
			workItemLeastRecent.WKI_Status = "OPN";
			//Create Staff
			GlbStaff staffMostRecent = Factory.NewWithValidTestData<GlbStaff>();
			staffMostRecent.GS_FullName = "Walter White";
			staffMostRecent.GS_Code = "WW";
			staffMostRecent.StaffPlainTextPassword = "blah";
			staffMostRecent.GS_LoginName = "wawh";
			staffMostRecent.GS_Birthdate = new ZDate(2007, 01, 17);
			staffMostRecent.GS_UserAddress1 = "Alberq";
			staffMostRecent.GS_City = "New Mexico";
			staffMostRecent.GS_IsOperational = true;
			Factory.Save();
			GlbStaff staffLeastRecent = Factory.NewWithValidTestData<GlbStaff>();
			staffLeastRecent.GS_FullName = "Jon Snow";
			staffLeastRecent.GS_Code = "JS";
			staffLeastRecent.StaffPlainTextPassword = "blah2";
			staffLeastRecent.GS_LoginName = "JonS";
			staffLeastRecent.GS_Birthdate = new ZDate(2008, 01, 17);
			staffLeastRecent.GS_UserAddress1 = "The Wall";
			staffLeastRecent.GS_City = "Westeros";
			staffLeastRecent.GS_IsOperational = true;
			Factory.Save();
			//Create Tasks, and assign to Staff + WIs
			ProcessTask processTaskMostRecentWI = Factory.NewWithValidTestData<ProcessTask>();
			processTaskMostRecentWI.P9_Sequence = 0;
			processTaskMostRecentWI.P9_ParentID = workItemMostRecent.PK;
			processTaskMostRecentWI.P9_GS_NKAssignedStaffMember = "WW";
			processTaskMostRecentWI.P9_ParentTableCode = "WKI";
			ProcessTask processTaskLeastRecentWI = Factory.NewWithValidTestData<ProcessTask>();
			processTaskLeastRecentWI.P9_Sequence = 1;
			processTaskLeastRecentWI.P9_ParentID = workItemLeastRecent.PK;
			processTaskLeastRecentWI.P9_GS_NKAssignedStaffMember = "JS";
			processTaskLeastRecentWI.P9_ParentTableCode = "WKI";
			Factory.Save();
			//Generate status for tasks
			processTaskMostRecentWI.P9_Status = "CLS";
			processTaskMostRecentWI.P9_CompletedTimeUtc = new ZDateTime(2008, 01, 17, 0, 2, 0);
			processTaskLeastRecentWI.P9_Status = "CLS";
			processTaskLeastRecentWI.P9_CompletedTimeUtc = new ZDateTime(2008, 01, 17, 0, 1, 0);
			Factory.Save();
			using (var dataTable = Utilities.GetDataTableFromQuery($"SELECT * FROM ClientReport_ClientIncidentSummary('{companyPK}', '', null, '')"))
			{
				LinkedList<string> cellValues = new LinkedList<string>();
				foreach (DataRow row in dataTable.Rows)
				{
					foreach (DataColumn column in dataTable.Columns)
					{
						cellValues.AddLast(row[column].ToString());
					}
				}

				Assert("LastTaskClosedByCode populated correctly", cellValues.Contains("WW"));
				Assert("LastTaskClosedByFullName populated correctly", cellValues.Contains("Walter White"));
				Assert("One work item per incident", !cellValues.Contains("JS"));
			}
		}

		public void TestClientReport_CloudServicesClientSummary()
		{
			var enterpriseCode = "AAA";
			var licenceHeader = BillingTestHelper.CreateLicence(Factory, enterpriseCode);
			var clientBranch1 = Factory.NewWithValidTestData<ClientBranch>();
			clientBranch1.LCB_LD = licenceHeader.Database.PK;
			clientBranch1.LCB_Code = "SYD";
			Factory.Save();

			var dataQuery = $"SELECT * FROM ClientReport_CloudServicesClientSummary('{enterpriseCode}')";

			using (var dataTable = Utilities.GetDataTableFromQuery(dataQuery))
			{
				AssertEquals(0, dataTable.Rows.Count);
			}

			var clientCompany1 = Factory.NewWithValidTestData<ClientCompany>();
			clientCompany1.LCC_Code = "SYC";
			clientCompany1.LCC_LD = licenceHeader.Database.PK;
			clientCompany1.LCC_Name = "Sydney Company";
			clientCompany1.LCC_RN_NKCountryCode = string.Empty;
			clientBranch1.LCB_LCC_Code = clientCompany1.LCC_Code;
			Factory.Save();

			using (var dataTable = Utilities.GetDataTableFromQuery(dataQuery))
			{
				AssertEquals(1, dataTable.Rows.Count);
				AssertEquals(enterpriseCode, dataTable.Rows[0]["EnterpriseCode"].ToString());
				AssertEquals(clientCompany1.LCC_Code, dataTable.Rows[0]["CompanyCode"].ToString());
				AssertEquals(clientBranch1.LCB_Code, dataTable.Rows[0]["BranchCode"].ToString());
				AssertEquals(clientCompany1.LCC_Name, dataTable.Rows[0]["CompanyName"].ToString());
				AssertEquals(string.Empty, dataTable.Rows[0]["Address1"].ToString());
				AssertEquals(string.Empty, dataTable.Rows[0]["Address2"].ToString());
				AssertEquals(string.Empty, dataTable.Rows[0]["City"].ToString());
				AssertEquals(string.Empty, dataTable.Rows[0]["PostCode"].ToString());
				AssertEquals(string.Empty, dataTable.Rows[0]["State"].ToString());
				AssertEquals("  ", dataTable.Rows[0]["CountryCode"].ToString());
				AssertEquals(string.Empty, dataTable.Rows[0]["Phone"].ToString());
				AssertActiveFields(dataTable.Rows[0], true, true, true);
			}

			clientCompany1.LCC_Address1 = "188 Rose Street";
			clientCompany1.LCC_Address2 = "Unit 1";
			clientCompany1.LCC_City = "Somerfield";
			clientCompany1.LCC_PostCode = "8024";
			clientCompany1.LCC_State = "Christchurch";
			clientCompany1.LCC_RN_NKCountryCode = "NZ";
			clientCompany1.LCC_Phone = "+61421654321";
			Factory.Save();

			using (var dataTable = Utilities.GetDataTableFromQuery(dataQuery))
			{
				AssertEquals(1, dataTable.Rows.Count);
				AssertEquals(enterpriseCode, dataTable.Rows[0]["EnterpriseCode"].ToString());
				AssertEquals(clientCompany1.LCC_Code, dataTable.Rows[0]["CompanyCode"].ToString());
				AssertEquals(clientBranch1.LCB_Code, dataTable.Rows[0]["BranchCode"].ToString());
				AssertEquals(clientCompany1.LCC_Name, dataTable.Rows[0]["CompanyName"].ToString());
				AssertAddress(clientCompany1, dataTable.Rows[0]);
				AssertEquals(string.Empty, dataTable.Rows[0]["Email"].ToString());
				AssertActiveFields(dataTable.Rows[0], true, true, true);
			}

			var ausAddress = licenceHeader.Database.LicEnterprise.Organisation.MainAddress;
			ausAddress.OA_Address1 = "72 O'Riordan Street";
			ausAddress.OA_Address2 = "Unit 1";
			ausAddress.OA_City = "Alexandria";
			ausAddress.OA_PostCode = "2015";
			ausAddress.OA_State = "NSW";
			ausAddress.OA_RN_NKCountryCode = "AU";
			ausAddress.OA_Phone = "+61421123456";
			ausAddress.OA_Email = "aus@work.com";
			clientBranch1.LCB_OA = ausAddress.PK;
			Factory.Save();

			using (var dataTable = Utilities.GetDataTableFromQuery(dataQuery))
			{
				AssertEquals(1, dataTable.Rows.Count);
				AssertEquals(enterpriseCode, dataTable.Rows[0]["EnterpriseCode"].ToString());
				AssertEquals(clientCompany1.LCC_Code, dataTable.Rows[0]["CompanyCode"].ToString());
				AssertEquals(clientBranch1.LCB_Code, dataTable.Rows[0]["BranchCode"].ToString());
				AssertEquals(clientCompany1.LCC_Name, dataTable.Rows[0]["CompanyName"].ToString());
				AssertAddress(ausAddress, dataTable.Rows[0]);
				AssertEquals(ausAddress.OA_Email, dataTable.Rows[0]["Email"].ToString());
				AssertActiveFields(dataTable.Rows[0], true, true, true);
			}
		}

		void AssertAddress(ClientCompany company, DataRow row)
		{
			AssertEquals(company.LCC_Address1, row["Address1"].ToString());
			AssertEquals(company.LCC_Address2, row["Address2"].ToString());
			AssertEquals(company.LCC_City, row["City"].ToString());
			AssertEquals(company.LCC_PostCode, row["PostCode"].ToString());
			AssertEquals(company.LCC_State, row["State"].ToString());
			AssertEquals(company.LCC_RN_NKCountryCode, row["CountryCode"].ToString());
			AssertEquals(company.LCC_Phone, row["Phone"].ToString());
		}

		void AssertAddress(OrgAddress address, DataRow row)
		{
			AssertEquals(address.OA_Address1, row["Address1"].ToString());
			AssertEquals(address.OA_Address2, row["Address2"].ToString());
			AssertEquals(address.OA_City, row["City"].ToString());
			AssertEquals(address.OA_PostCode, row["PostCode"].ToString());
			AssertEquals(address.OA_State, row["State"].ToString());
			AssertEquals(address.OA_RN_NKCountryCode, row["CountryCode"].ToString());
			AssertEquals(address.OA_Phone, row["Phone"].ToString());
		}

		void AssertActiveFields(DataRow row, bool isLicenceDatabaseActive, bool isClientCompanyActive, bool isClientBranchActive)
		{
			AssertEquals(isLicenceDatabaseActive, bool.Parse(row["IsLicenceDatabaseActive"].ToString()));
			AssertEquals(isClientCompanyActive, bool.Parse(row["IsClientCompanyActive"].ToString()));
			AssertEquals(isClientBranchActive, bool.Parse(row["IsClientBranchActive"].ToString()));
		}

		public void TestClientReport_CloudServicesClientSummary_Country()
		{
			var enterpriseCode = "AAA";
			var licenceHeader = BillingTestHelper.CreateLicence(Factory, enterpriseCode);

			var ausAddress = licenceHeader.Database.LicEnterprise.Organisation.MainAddress;
			ausAddress.OA_Address1 = "72 O'Riordan Street";
			ausAddress.OA_Address2 = "Unit 1";
			ausAddress.OA_City = "Alexandria";
			ausAddress.OA_PostCode = "2015";
			ausAddress.OA_State = "NSW";
			ausAddress.OA_RN_NKCountryCode = "AU";
			ausAddress.OA_Phone = "+61421123456";
			ausAddress.OA_Email = "aus@work.com";
			var clientCompany1 = Factory.NewWithValidTestData<ClientCompany>();
			clientCompany1.LCC_Code = "SYC";
			clientCompany1.LCC_LD = licenceHeader.Database.PK;
			clientCompany1.LCC_Name = "Sydney Company";
			var clientBranch1 = Factory.NewWithValidTestData<ClientBranch>();
			clientBranch1.LCB_LD = licenceHeader.Database.PK;
			clientBranch1.LCB_Code = "SYD";
			clientBranch1.LCB_OA = ausAddress.PK;
			clientBranch1.LCB_LCC_Code = clientCompany1.LCC_Code;

			var nzAddress = licenceHeader.Database.LicEnterprise.Organisation.Addresses.AddNew();
			nzAddress.OA_RN_NKCountryCode = "NZ";
			nzAddress.OA_Address1 = "188 Rose Street";
			nzAddress.OA_Address2 = "Unit 1";
			nzAddress.OA_City = "Somerfield";
			nzAddress.OA_PostCode = "8024";
			nzAddress.OA_State = "Christchurch";
			var clientCompany2 = Factory.NewWithValidTestData<ClientCompany>();
			clientCompany2.LCC_Code = "WEC";
			clientCompany2.LCC_LD = licenceHeader.Database.PK;
			var clientBranch2 = Factory.NewWithValidTestData<ClientBranch>();
			clientBranch2.LCB_LD = licenceHeader.Database.PK;
			clientBranch2.LCB_Code = "WEL";
			clientBranch2.LCB_OA = nzAddress.PK;
			clientBranch2.LCB_LCC_Code = clientCompany2.LCC_Code;

			var clientCompany3 = Factory.NewWithValidTestData<ClientCompany>();
			clientCompany3.LCC_Code = "S2C";
			clientCompany3.LCC_Address1 = "72 O'Riordan Street";
			clientCompany3.LCC_Address2 = "Unit 1";
			clientCompany3.LCC_City = "Alexandria";
			clientCompany3.LCC_PostCode = "2015";
			clientCompany3.LCC_State = "NSW";
			clientCompany3.LCC_RN_NKCountryCode = "AU";
			clientCompany3.LCC_Phone = "+61421123456";
			clientCompany3.LCC_LD = licenceHeader.Database.PK;
			var clientBranch3 = Factory.NewWithValidTestData<ClientBranch>();
			clientBranch3.LCB_LD = licenceHeader.Database.PK;
			clientBranch3.LCB_Code = "SY2";
			clientBranch3.LCB_LCC_Code = clientCompany3.LCC_Code;

			var clientCompany4 = Factory.NewWithValidTestData<ClientCompany>();
			clientCompany4.LCC_Code = "CCC";
			clientCompany4.LCC_Address1 = "188 Rose Street";
			clientCompany4.LCC_Address2 = "Unit 1";
			clientCompany4.LCC_City = "Somerfield";
			clientCompany4.LCC_PostCode = "8024";
			clientCompany4.LCC_State = "Christchurch";
			clientCompany4.LCC_RN_NKCountryCode = "NZ";
			clientCompany4.LCC_Phone = "+61421654321";
			clientCompany4.LCC_LD = licenceHeader.Database.PK;
			var clientBranch4 = Factory.NewWithValidTestData<ClientBranch>();
			clientBranch4.LCB_LD = licenceHeader.Database.PK;
			clientBranch4.LCB_Code = "CCH";
			clientBranch4.LCB_LCC_Code = clientCompany4.LCC_Code;

			var clientBranch5 = Factory.NewWithValidTestData<ClientBranch>();
			clientBranch5.LCB_LD = licenceHeader.Database.PK;
			clientBranch5.LCB_Code = "UNK";

			var otherEnterpriseCode = "ZZZ";
			var licenceHeader2 = BillingTestHelper.CreateLicence(Factory, otherEnterpriseCode);

			var clientCompany6 = Factory.NewWithValidTestData<ClientCompany>();
			clientCompany6.LCC_Code = "UNC";
			clientCompany6.LCC_LD = licenceHeader2.Database.PK;
			clientCompany6.LCC_Name = "Unknown Company";
			var clientBranch6 = Factory.NewWithValidTestData<ClientBranch>();
			clientBranch6.LCB_LD = licenceHeader2.Database.PK;
			clientBranch6.LCB_Code = "UN2";
			clientBranch6.LCB_OA = licenceHeader2.Database.LicEnterprise.Organisation.MainAddress.PK;
			clientBranch6.LCB_LCC_Code = clientCompany6.LCC_Code;

			Factory.Save();

			using (var dataTable = Utilities.GetDataTableFromQuery($"SELECT * FROM ClientReport_CloudServicesClientSummary('{enterpriseCode}')"))
			{
				AssertEquals("Should return all client branches that have a client company for enterprise code", 4, dataTable.Rows.Count);
			}

			using (var dataTable = Utilities.GetDataTableFromQuery($"SELECT * FROM ClientReport_CloudServicesClientSummary('{enterpriseCode}') WHERE CountryCode = 'AU'"))
			{
				AssertEquals("Country code AU should return client branches with AU addresses", 2, dataTable.Rows.Count);
				var expectedBranchCodes = new string[] { clientBranch1.LCB_Code, clientBranch3.LCB_Code };
				AssertEquals("Country code AU should return client branches with AU addresses", true, expectedBranchCodes.Contains(dataTable.Rows[0]["BranchCode"]));
				AssertEquals("Country code AU should return client branches with AU addresses", true, expectedBranchCodes.Contains(dataTable.Rows[1]["BranchCode"]));
			}

			using (var dataTable = Utilities.GetDataTableFromQuery($"SELECT * FROM ClientReport_CloudServicesClientSummary('{enterpriseCode}') WHERE CountryCode = 'NZ'"))
			{
				AssertEquals("Country code NZ should return client branches with NZ addresses", 2, dataTable.Rows.Count);
				var expectedBranchCodes = new string[] { clientBranch2.LCB_Code, clientBranch4.LCB_Code };
				AssertEquals("Country code NZ should return client branches with NZ addresses", true, expectedBranchCodes.Contains(dataTable.Rows[0]["BranchCode"]));
				AssertEquals("Country code NZ should return client branches with NZ addresses", true, expectedBranchCodes.Contains(dataTable.Rows[1]["BranchCode"]));
			}
		}

		public void TestClientReport_CloudServicesClientSummary_Active()
		{
			var enterpriseCode = "AAA";
			var licenceHeader = BillingTestHelper.CreateLicence(Factory, enterpriseCode);

			licenceHeader.Database.LD_IsActive = false;
			var clientCompany1 = Factory.NewWithValidTestData<ClientCompany>();
			clientCompany1.LCC_Code = "AAC";
			clientCompany1.LCC_LD = licenceHeader.Database.PK;
			clientCompany1.LCC_DeactivateTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);
			var clientBranch1 = Factory.NewWithValidTestData<ClientBranch>();
			clientBranch1.LCB_LD = licenceHeader.Database.PK;
			clientBranch1.LCB_Code = "AAA";
			clientBranch1.LCB_LCC_Code = clientCompany1.LCC_Code;
			clientBranch1.LCB_IsActive = true;

			var clientCompany2 = Factory.NewWithValidTestData<ClientCompany>();
			clientCompany2.LCC_Code = "BBC";
			clientCompany2.LCC_LD = licenceHeader.Database.PK;
			clientCompany2.LCC_DeactivateTimeUtc = ZDateTime.Empty;
			var clientBranch2 = Factory.NewWithValidTestData<ClientBranch>();
			clientBranch2.LCB_LD = licenceHeader.Database.PK;
			clientBranch2.LCB_Code = "BBB";
			clientBranch2.LCB_LCC_Code = clientCompany2.LCC_Code;
			clientBranch2.LCB_IsActive = false;

			var clientCompany3 = Factory.NewWithValidTestData<ClientCompany>();
			clientCompany3.LCC_Code = "CCC";
			clientCompany3.LCC_LD = licenceHeader.Database.PK;
			clientCompany3.LCC_DeactivateTimeUtc = ZDateTime.UtcNow.AddMinutes(10);
			var clientBranch3 = Factory.NewWithValidTestData<ClientBranch>();
			clientBranch3.LCB_LD = licenceHeader.Database.PK;
			clientBranch3.LCB_Code = "CCC";
			clientBranch3.LCB_LCC_Code = clientCompany3.LCC_Code;
			clientBranch3.LCB_IsActive = true;

			var clientCompany4 = Factory.NewWithValidTestData<ClientCompany>();
			clientCompany4.LCC_Code = "DDC";
			clientCompany4.LCC_LD = licenceHeader.Database.PK;
			clientCompany4.LCC_DeactivateTimeUtc = ZDateTime.Empty;
			var clientBranch4 = Factory.NewWithValidTestData<ClientBranch>();
			clientBranch4.LCB_LD = licenceHeader.Database.PK;
			clientBranch4.LCB_Code = "DDD";
			clientBranch4.LCB_LCC_Code = clientCompany4.LCC_Code;
			clientBranch4.LCB_IsActive = true;

			Factory.Save();

			using (var dataTable = Utilities.GetDataTableFromQuery($"SELECT * FROM ClientReport_CloudServicesClientSummary('{enterpriseCode}')"))
			{
				AssertEquals("Should return all client branches for enterprise code", 4, dataTable.Rows.Count);
			}

			using (var dataTable = Utilities.GetDataTableFromQuery($"SELECT * FROM ClientReport_CloudServicesClientSummary('{enterpriseCode}') WHERE IsLicenceDatabaseActive = 1"))
			{
				AssertEquals("Should return none of the client branches since LD_IsActive is false", 0, dataTable.Rows.Count);
			}

			using (var dataTable = Utilities.GetDataTableFromQuery($"SELECT * FROM ClientReport_CloudServicesClientSummary('{enterpriseCode}') WHERE IsClientCompanyActive = 1"))
			{
				AssertEquals("Should only return rows which have a company deactivation time in the future or null", 3, dataTable.Rows.Count);
				var expectedBranchCodes = new string[] { clientBranch2.LCB_Code, clientBranch3.LCB_Code, clientBranch4.LCB_Code };
				AssertEquals(true, expectedBranchCodes.Contains(dataTable.Rows[0]["BranchCode"]));
				AssertEquals(true, expectedBranchCodes.Contains(dataTable.Rows[1]["BranchCode"]));
				AssertEquals(true, expectedBranchCodes.Contains(dataTable.Rows[2]["BranchCode"]));
			}

			using (var dataTable = Utilities.GetDataTableFromQuery($"SELECT * FROM ClientReport_CloudServicesClientSummary('{enterpriseCode}') WHERE IsClientBranchActive = 1"))
			{
				AssertEquals("Should only return rows which have LCB_IsActive true", 3, dataTable.Rows.Count);
				var expectedBranchCodes = new string[] { clientBranch1.LCB_Code, clientBranch3.LCB_Code, clientBranch4.LCB_Code };
				AssertEquals(true, expectedBranchCodes.Contains(dataTable.Rows[0]["BranchCode"]));
				AssertEquals(true, expectedBranchCodes.Contains(dataTable.Rows[1]["BranchCode"]));
				AssertEquals(true, expectedBranchCodes.Contains(dataTable.Rows[2]["BranchCode"]));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductNameCSharp", Justification = "Testing")]
		public void TestClientReport_GetAllProductsFromRegistry()
		{
			var nonEDIProductsCollection = new SystemProductCollection();
			var product1 = nonEDIProductsCollection.AddNew();
			product1.Code = "HUB";
			product1.Description = (NoResString)"eHub";
			var product2 = nonEDIProductsCollection.AddNew();
			product2.Code = "TTN";
			product2.Description = (NoResString)"TiTAN";
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, nonEDIProductsCollection);
			var command = "select ProductCode, ProductDescription from dbo.ClientReport_GetAllProductsFromRegistry() order by ProductCode, ProductDescription";
			using (DbCommand cmd = Db.Connection.Command(command))
			using (IDataReader reader = cmd.ExecuteReader())
			{
				Action<string, string> assertProduct = (code, description) =>
				{
					if (!reader.Read())
					{
						Fail(string.Format("Expecting row for ProductCode:[{0}] ProductDescription:[{1}]", code, description));
						return;
					}

					CombineAssertions(() =>
					{
						AssertEquals("ProductCode", code, reader["ProductCode"]);
						AssertEquals("ProductDescription", description, reader["ProductDescription"]);
					});
				};
				assertProduct("ENT", "CargoWise");
				assertProduct("HUB", "eHub");
				assertProduct("TTN", "TiTAN");
				Assert("Should be no more rows", !reader.Read());
			}
		}

		public void TestClientReport_GetAllEnterpriseProductAreasFromRegistry()
		{
			var productAreaList = new CodeDescriptionPairList();
			productAreaList.AddPair("ARC", "Architecture");
			productAreaList.AddPair("CUS", "Customs Compliance");
			productAreaList.AddPair("GEO", "GEO Compliance");
			EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, productAreaList);
			var command = "select ProductAreaCode, ProductAreaDescription from dbo.ClientReport_GetAllEnterpriseProductAreasFromRegistry() order by ProductAreaCode, ProductAreaDescription";
			using (DbCommand cmd = Db.Connection.Command(command))
			using (IDataReader reader = cmd.ExecuteReader())
			{
				Action<string, string> assertProductArea = (code, description) =>
				{
					if (!reader.Read())
					{
						Fail(string.Format("Expecting row for Code:{0} Description:{1}", code, description));
						return;
					}

					CombineAssertions(() =>
					{
						AssertEquals("ProductAreaCode", code, reader["ProductAreaCode"]);
						AssertEquals("ProductAreaDescription", description, reader["ProductAreaDescription"]);
					});
				};
				assertProductArea("ARC", "Architecture");
				assertProductArea("CUS", "Customs Compliance");
				assertProductArea("GEO", "GEO Compliance");
				Assert("Should be no more rows", !reader.Read());
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductNameCSharp", Justification = "Testing")]
		public void TestClientReport_GetAllProductsAndModulesFromRegistry()
		{
			CodeDescriptionPairList areaList = new CodeDescriptionPairList();
			areaList.AddPair("ARC", "Architecture");
			areaList.AddPair("CUS", "Customs Compliance");
			areaList.AddPair("GEO", "GEO Compliance");
			EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, areaList);
			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			product.ModuleMappings.AddNew("AC1", "ediAirCargoCFSCustoms", ProductAreaList.Codes.CUS, false);
			product.ModuleMappings.AddNew("DDD", "Test Module D", "", false);
			EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			collection = new SystemProductCollection();
			product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			product.ModuleMappings.AddNew("AC2", "ediAirCargoCFSCustoms", ProductAreaList.Codes.GEO, false);
			product.ModuleMappings.AddNew("EEE", "Test Module E", "", false);
			EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			collection = new SystemProductCollection();
			product = collection.AddNew(ProductTypes.Codes.Enterprise, "ediEnterprise / CargoWise One", true);
			product.ModuleMappings.AddNew("AR1", "ediArchiveManager", ProductAreaList.Codes.ARC, false);
			product.ModuleMappings.AddNew("CCC", "Test Module C", "", false);
			var product1 = collection.AddNew();
			product1.Code = "GLW";
			product1.Description = (NoResString)"Glow";
			var module1 = product1.ModuleMappings.AddNew();
			module1.ModuleCode = "WEB";
			module1.ModuleDescription = (NoResString)"Web Module";
			var module2 = product1.ModuleMappings.AddNew();
			module2.ModuleCode = "SRV";
			module2.ModuleDescription = (NoResString)"Service Module";
			var product2 = collection.AddNew();
			product2.Code = "HUB";
			product2.Description = (NoResString)"eHub";
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			List<string[]> results = new List<string[]>();
			const string command = "select ProductCode, ProductDescription, ModuleCode, ModuleDescription from dbo.ClientReport_GetAllProductsAndModulesFromRegistry() order by ProductCode, ModuleCode";
			using (DbCommand cmd = Db.Connection.Command(command))
			using (IDataReader reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					string[] moduleRecord = new string[4];
					moduleRecord[0] = reader[0].ToString().Trim();
					moduleRecord[1] = reader[1].ToString().Trim();
					moduleRecord[2] = reader[2].ToString().Trim();
					moduleRecord[3] = reader[3].ToString().Trim();
					results.Add(moduleRecord);
				}
			}

			AssertProductAndModule(results[0], "ENT", "ediEnterprise / CargoWise One", "ALL", "All - ediEnterprise / CargoWise One");
			AssertProductAndModule(results[1], "ENT", "ediEnterprise / CargoWise One", "AR1", "ediArchiveManager");
			AssertProductAndModule(results[2], "ENT", "ediEnterprise / CargoWise One", "CCC", "Test Module C");
			AssertProductAndModule(results[3], "GLW", "Glow", "ALL", "All - Glow");
			AssertProductAndModule(results[4], "GLW", "Glow", "SRV", "Service Module");
			AssertProductAndModule(results[5], "GLW", "Glow", "WEB", "Web Module");
			AssertProductAndModule(results[6], "HUB", "eHub", "", "");
			AssertProductAndModule(results[7], "HUB", "eHub", "ALL", "All - eHub");
			AssertProductAndModule(results[8], "HUB", "eHub", "AR1", "ediArchiveManager");
			AssertProductAndModule(results[9], "HUB", "eHub", "CCC", "Test Module C");
		}

		void AssertProductAndModule(string[] record, string expectedProductCode, string expectedProductDescription, string expectedModuleCode, string expectedModuleDescription)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Product Code", expectedProductCode, record[0]);
				AssertEquals("Product Description", expectedProductDescription, record[1]);
				AssertEquals("Module Code", expectedModuleCode, record[2]);
				AssertEquals("Module Description", expectedModuleDescription, record[3]);
			});
		}

		public void TestClientReport_GetAllCr8ModulesFromRegistry()
		{
			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			product.ModuleMappings.AddNew("AC1", "ediAirCargoCFSCustoms", ProductAreaList.Codes.CUS, false);
			product.ModuleMappings.AddNew("DDD", "Test Module D", "", false);
			EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			Dictionary<string, string> actualDescriptionsByCode = new Dictionary<string, string>();
			const string command = "select ModuleCode, ModuleDescription from dbo.ClientReport_GetAllCr8ModulesFromRegistry() order by ModuleCode";
			using (DbCommand cmd = Db.Connection.Command(command))
			using (IDataReader reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					actualDescriptionsByCode.Add((string)reader["ModuleCode"], (string)reader["ModuleDescription"]);
				}
			}

			AssertModule(actualDescriptionsByCode, "AC1", "ediAirCargoCFSCustoms");
			AssertModule(actualDescriptionsByCode, "DDD", "Test Module D");
			AssertEquals(2, actualDescriptionsByCode.Count);
		}

		public void TestClientReport_GetAllCr9ModulesFromRegistry()
		{
			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			product.ModuleMappings.AddNew("AC1", "ediAirCargoCFSCustoms", ProductAreaList.Codes.CUS, false);
			product.ModuleMappings.AddNew("DDD", "Test Module D", "", false);
			EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			Dictionary<string, string> actualDescriptionsByCode = new Dictionary<string, string>();
			const string command = "select ModuleCode, ModuleDescription from dbo.ClientReport_GetAllCr9ModulesFromRegistry() order by ModuleCode";
			using (DbCommand cmd = Db.Connection.Command(command))
			using (IDataReader reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					actualDescriptionsByCode.Add((string)reader["ModuleCode"], (string)reader["ModuleDescription"]);
				}
			}

			AssertModule(actualDescriptionsByCode, "AC1", "ediAirCargoCFSCustoms");
			AssertModule(actualDescriptionsByCode, "DDD", "Test Module D");
			AssertEquals(2, actualDescriptionsByCode.Count);
		}

		void AssertModule(Dictionary<string, string> actualDescriptionsByCoded, string expectedModuleCode, string expectedModuleDescription)
		{
			AssertEquals("ModuleCode", true, actualDescriptionsByCoded.ContainsKey(expectedModuleCode));
			string description;
			if (actualDescriptionsByCoded.TryGetValue(expectedModuleCode, out description))
			{
				AssertEquals("ModuleDescription", expectedModuleDescription, description);
			}
		}

		public void TestClientReport_GetAllSourceModuleProductAreasFromRegistry()
		{
			CodeDescriptionPairList areaList = new CodeDescriptionPairList();
			areaList.AddPair("ARC", "Architecture");
			areaList.AddPair("CIL", "Invoicing and Licensing");
			areaList.AddPair("CRM", "Customer Relationship Manager");
			areaList.AddPair("CUS", "Customs Compliance");
			areaList.AddPair("DOM", "Domestic Logistics");
			EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, areaList);
			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			var menuSectionMapping = product.ModuleMappings.AddNew("AR1", "ediArchiveManager", "ARC", false);
			menuSectionMapping.SourceModuleMappings.AddNew("Communication", "CIL");
			menuSectionMapping.SourceModuleMappings.AddNew("Cartage", "CRM");
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			collection = new SystemProductCollection();
			product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			var cr8ModuleMapping = product.ModuleMappings.AddNew("AC1", "ediAirCargoCFSCustoms", "CIL", false);
			cr8ModuleMapping.SourceModuleMappings.AddNew("RefCountry", "CRM");
			EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			collection = new SystemProductCollection();
			product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			var cr9ModuleMapping = product.ModuleMappings.AddNew("AC2", "ediAirCargoCFSCustoms", "CRM", false);
			cr9ModuleMapping.SourceModuleMappings.AddNew("AccBankAccount", "CUS");
			cr9ModuleMapping.SourceModuleMappings.AddNew("AccTaxRate", "DOM");
			EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			List<string[]> results = new List<string[]>();
			string command = "select SourceModuleCode, ModuleCode, ProductAreaCode, ProductAreaDescription from dbo.ClientReport_GetAllSourceModuleProductAreasFromRegistry() order by ModuleCode, SourceModuleCode";
			using (DbCommand cmd = Db.Connection.Command(command))
			using (IDataReader reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					string[] moduleRecord = new string[4];
					moduleRecord[0] = reader[0].ToString().Trim();
					moduleRecord[1] = reader[1].ToString().Trim();
					moduleRecord[2] = reader[2].ToString().Trim();
					moduleRecord[3] = reader[3].ToString().Trim();
					results.Add(moduleRecord);
				}
			}

			AssertProductAreaSourceModule(results[0], "AC1", "RefCountry", "CRM", "Customer Relationship Manager");
			AssertProductAreaSourceModule(results[1], "AC2", "AccBankAccount", "CUS", "Customs Compliance");
			AssertProductAreaSourceModule(results[2], "AC2", "AccTaxRate", "DOM", "Domestic Logistics");
			AssertProductAreaSourceModule(results[3], "AR1", "Cartage", "CRM", "Customer Relationship Manager");
			AssertProductAreaSourceModule(results[4], "AR1", "Communication", "CIL", "Invoicing and Licensing");
		}

		void AssertProductAreaSourceModule(string[] record, string expectedModuleCode, string expectedSourceModuleCode, string expectedAreaCode, string expectedAreaDescription)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Module Code", expectedModuleCode, record[1]);
				AssertEquals("Source Module Code", expectedSourceModuleCode, record[0]);
				AssertEquals("Product Area Code", expectedAreaCode, record[2]);
				AssertEquals("Product Area Description", expectedAreaDescription, record[3]);
			});
		}

		public void TestClientvw_DevWorkItem()
		{
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			workItem.WKI_Status = "CLS";
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			Factory.Save();
			using (var dataTable = new DataTable())
			using (var cmd = Db.Connection.Command("select * from dbo.Clientvw_DevWorkItem"))
			{
				cmd.NewDataAdapter().Fill(dataTable);
				AssertEquals(1, dataTable.Rows.Count);
				AssertEquals(workItem.PK.ToGuid(), dataTable.Rows[0]["DWI_PK"]);
				AssertEquals(workItem.WKI_WorkItemNumber, dataTable.Rows[0]["DWI_Number"]);
				AssertEquals(workItem.WKI_Status, dataTable.Rows[0]["DWI_Status"]);
			}
		}

		public void TestClientvw_Report_LicenceExpiry()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.CreateAndLoadLicenceForOrg();
			var database1 = org.LicCompany.LicDatabases.AddNew();
			database1.FillWithValidTestData();
			database1.LD_LicenceType = "PRD";
			database1.LD_Product = ProductTypes.Codes.Enterprise;
			database1.LD_ServerCode = "001";
			var database2 = org.LicCompany.LicDatabases.AddNew();
			database2.FillWithValidTestData();
			database2.LD_LicenceType = "PRD";
			database2.LD_Product = ProductTypes.Codes.ProductivityWise;
			database2.LD_ServerCode = "002";
			var database3 = org.LicCompany.LicDatabases.AddNew();
			database3.FillWithValidTestData();
			database3.LD_LicenceType = "PRD";
			database3.LD_Product = ProductTypes.Codes.GLOW;
			database3.LD_ServerCode = "003";
			Factory.Save();
			using (var dataTable = new DataTable())
			using (var cmd = Db.Connection.Command("select * from dbo.Clientvw_Report_LicenceExpiry order by LD_ServerCode"))
			{
				cmd.NewDataAdapter().Fill(dataTable);
				AssertEquals(2, dataTable.Rows.Count);
				AssertEquals("001", dataTable.Rows[0]["LD_ServerCode"].ToString());
				AssertEquals("002", dataTable.Rows[1]["LD_ServerCode"].ToString());
			}
		}

		[ExpectNoExceptions]
		public void TestViewGenericClientJob()
		{
			var job = Factory.NewWithValidTestData<SupportIncident>();
			Factory.Save();
			var clientJob = ((IExtensionObjects)new EDIClientDbSchemaUpgradeInfo()).ViewAndRoutineCreationScripts.FirstOrDefault(x => x.ObjectName.Equals("ViewGenericClientJob", StringComparison.InvariantCultureIgnoreCase));
			var genericJob = CargoWise.DbUpgrader.Scripts.Definitions.CoreScriptIndex.GetScripts().FirstOrDefault(s => s.Name.Equals("ViewGenericJob", StringComparison.InvariantCultureIgnoreCase));
			DbColumnDependencyRemover.DropSchemaBoundReferencingObjects(TestConnection, Db.SqlDbOwnerSchema, clientJob.ObjectName);
			AssertEquals("ViewGenericJob has been dropped", false, DataUtils.ObjectExists(TestConnection, genericJob.Name));
			// Have to drop and re-create since by default client views are not recreated if they already exist, but this view will already exist in its generic schema version.
			Db.Connection.ExecuteNonQuery(clientJob.DropScript);
			Db.Connection.ExecuteNonQuery(clientJob.CreateScript);
			Db.Connection.ExecuteNonQuery(genericJob.Text);
			using (DbCommand command = TestConnection.Command("Select count(*) from dbo.ViewGenericJob"))
			{
				var count = (int)command.ExecuteScalar();
				Assert(count > 0);
			}
		}

		[TestDate(2016, 1, 1)]
		public void TestClientvw_OnDemandLicenceUsage()
		{
			var licenceHeader = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV", true);
			var clientCompany = licenceHeader.ClientCompany;
			var clientStaff = BillingTestHelper.CreateClientStaff(licenceHeader.Database, "JIM", "Jim Bob");
			var licenceDatabase = licenceHeader.Database;
			var licenceEnterprise = licenceHeader.Database.LicEnterprise;
			var orgHeader = licenceHeader.Company.Header;
			var usage = BillingTestHelper.CreateEdiLicenceUsage(clientCompany, clientStaff, "ODM", "FOR", new ZDateTime(2018, 6, 1, 9, 0, 0));
			usage.LX2_LastUsageUtc = new ZDateTime(2018, 6, 28, 17, 0, 0);
			usage.LX2_UsageCount = 5;
			Factory.Save();
			using (var dataTable = Utilities.GetDataTableFromQuery("SELECT * FROM dbo.Clientvw_OnDemandLicenceUsage;"))
			{
				AssertEquals(1, dataTable.Rows.Count);
				AssertEquals(18, dataTable.Columns.Count);
				var row = dataTable.Rows[0];
				var licenceModule = row["LicenceModule"];
				var orgPK = row["OrgPK"];
				var orgCode = row["OrgCode"];
				var orgName = row["OrgName"];
				var licenceEnterprisePK = row["LicenceEnterprisePK"];
				var licenceEnterpriseCode = row["LicenceEnterpriseCode"];
				var licenceServerCode = row["LicenceServerCode"];
				var licenceServerType = row["LicenceServerType"];
				var usageTime = row["UsageTime"];
				var staffCode = row["StaffCode"];
				var staffName = row["StaffName"];
				var licenceType = row["LicenceType"];
				var clientCompanyCode = row["ClientCompanyCode"];
				AssertEquals(usage.LX2_ModuleCode, licenceModule);
				AssertEquals(orgPK, orgHeader.PK);
				AssertEquals(orgCode, orgHeader.OH_Code);
				AssertEquals(orgName, orgHeader.OH_FullName);
				AssertEquals(licenceEnterprise.PK, licenceEnterprisePK);
				AssertEquals(licenceEnterprise.LE_EnterpriseCode, licenceEnterpriseCode);
				AssertEquals(licenceDatabase.LD_ServerCode, licenceServerCode);
				AssertEquals(licenceDatabase.LD_LicenceType, licenceServerType);
				AssertEquals(usage.LX2_FirstUsageUtc, usageTime);
				AssertEquals(usage.LX2_LastUsageUtc, (DateTime)row["LastUsageTime"]);
				AssertEquals(usage.LX2_UsageCount, (int)row["UsageCount"]);
				AssertEquals(clientStaff.LS_Code, staffCode);
				AssertEquals(clientStaff.LS_FullName, staffName);
				AssertEquals(usage.LX2_LicenceMode, licenceType);
				AssertEquals("POS", (string)row["GoLiveFlag"]);
				AssertEquals(clientCompany.LCC_Code, clientCompanyCode);
			}
		}

		public void TestViewCommissionAgreement_TriggerTypeAYC()
		{
			var viewCommissionAgreementScript = ((IExtensionObjects)new EDIClientDbSchemaUpgradeInfo()).ViewAndRoutineCreationScripts.FirstOrDefault(x => x.ObjectName.Equals("ViewCommissionAgreement", StringComparison.InvariantCultureIgnoreCase));
			DbColumnDependencyRemover.DropSchemaBoundReferencingObjects(TestConnection, Db.SqlDbOwnerSchema, viewCommissionAgreementScript.ObjectName);
			// Have to drop and re-create since by default client views are not recreated if they already exist, but this view will already exist in its generic schema version.
			Db.Connection.ExecuteNonQuery(viewCommissionAgreementScript.DropScript);
			Db.Connection.ExecuteNonQuery(viewCommissionAgreementScript.CreateScript);
			var primaryCharge = BillingTestHelper.CreateChargeCode(Factory, null, "PRIMARY");
			primaryCharge.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			primaryCharge.AC_ChargeGroup = ChargeCodeGroupList.Codes.NonJobRelated;
			var secondaryCharge = BillingTestHelper.CreateChargeCode(Factory, null, "SECONDARY");
			secondaryCharge.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			secondaryCharge.AC_ChargeGroup = ChargeCodeGroupList.Codes.NonJobRelated;
			var anotherCharge = BillingTestHelper.CreateChargeCode(Factory, null, "ANOTHERCHG");
			anotherCharge.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			anotherCharge.AC_ChargeGroup = ChargeCodeGroupList.Codes.NonJobRelated;
			Factory.Save();
			var settings = new AYCTriggerTypeSettings();
			settings.PrimaryChargeCode = primaryCharge.AC_Code;
			settings.SecondaryChargeCode = secondaryCharge.AC_Code;
			EDIDataRegistry.Instance.AYCTriggerTypeSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);
			var organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			var agreement = Factory.NewWithValidTestData<EdiCommissionAgreement>();
			agreement.CA0_OH_Customer = organisation.PK;
			agreement.CA0_CommissionTriggerType = EDICommissionTriggerTypes.Codes.AYCEStartDate;
			var fee1 = BillingTestHelper.CreateLicenceFee(organisation.LicCompany, "AAA", 10m);
			fee1.L8_ChargeCode = anotherCharge.AC_Code;
			fee1.L8_StartDate = new ZDateTime(2020, 1, 1);
			Factory.Save();
			var viewAgreement = Factory.Load<ViewCommissionAgreement>(agreement.PK);
			AssertEquals("No Primary or Secondary charge", ZDate.Empty, viewAgreement.VCA_EffectiveDate);
			var fee2 = BillingTestHelper.CreateLicenceFee(organisation.LicCompany, "AAA", 20m);
			fee2.L8_ChargeCode = secondaryCharge.AC_Code;
			fee2.L8_StartDate = new ZDateTime(2020, 1, 2);
			var fee3 = BillingTestHelper.CreateLicenceFee(organisation.LicCompany, "AAA", 30m);
			fee3.L8_ChargeCode = secondaryCharge.AC_Code;
			fee3.L8_StartDate = new ZDateTime(2020, 1, 3);
			Factory.Save();
			Factory.ReloadAll<ViewCommissionAgreement>();
			AssertEquals("Secondary charge but no primary", new ZDateTime(2020, 1, 2), viewAgreement.VCA_EffectiveDate);
			var fee4 = BillingTestHelper.CreateLicenceFee(organisation.LicCompany, "AAA", 40m);
			fee4.L8_ChargeCode = primaryCharge.AC_Code;
			fee4.L8_StartDate = ZDateTime.Empty;
			Factory.Save();
			Factory.ReloadAll<ViewCommissionAgreement>();
			AssertEquals("Secondary charge and only primary has no Start Date", new ZDateTime(2020, 1, 2), viewAgreement.VCA_EffectiveDate);
			var fee5 = BillingTestHelper.CreateLicenceFee(organisation.LicCompany, "AAA", 40m);
			fee5.L8_ChargeCode = primaryCharge.AC_Code;
			fee5.L8_StartDate = new ZDateTime(2020, 2, 2);
			var fee6 = BillingTestHelper.CreateLicenceFee(organisation.LicCompany, "AAA", 50m);
			fee6.L8_ChargeCode = secondaryCharge.AC_Code;
			fee6.L8_StartDate = new ZDateTime(2020, 2, 3);
			Factory.Save();
			Factory.ReloadAll<ViewCommissionAgreement>();
			AssertEquals("Primary Charge", new ZDate(2020, 2, 2), viewAgreement.VCA_EffectiveDate);
		}

		public void TestViewCommissionAgreement_TriggerTypeGLC()
		{
			var viewCommissionAgreementScript = ((IExtensionObjects)new EDIClientDbSchemaUpgradeInfo()).ViewAndRoutineCreationScripts.FirstOrDefault(x => x.ObjectName.Equals("ViewCommissionAgreement", StringComparison.InvariantCultureIgnoreCase));
			DbColumnDependencyRemover.DropSchemaBoundReferencingObjects(TestConnection, Db.SqlDbOwnerSchema, viewCommissionAgreementScript.ObjectName);
			// Have to drop and re-create since by default client views are not recreated if they already exist, but this view will already exist in its generic schema version.
			Db.Connection.ExecuteNonQuery(viewCommissionAgreementScript.DropScript);
			Db.Connection.ExecuteNonQuery(viewCommissionAgreementScript.CreateScript);

			var organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			var agreement = Factory.NewWithValidTestData<EdiCommissionAgreement>();
			agreement.CA0_OH_Customer = organisation.PK;
			agreement.CA0_CommissionTriggerType = EDICommissionTriggerTypes.Codes.GoLiveComplete;
			Factory.Save();

			var viewAgreement = Factory.Load<ViewCommissionAgreement>(agreement.PK);
			AssertEquals("No Effective Date for GLC", ZDate.Empty, viewAgreement.VCA_EffectiveDate);
		}

		public void TestGetCommissionAgreementPrimaryKeys_TriggerTypeGLC()
		{
			var getCommissionAgreementPrimaryKeysScript = ((IExtensionObjects)new EDIClientDbSchemaUpgradeInfo()).ViewAndRoutineCreationScripts.FirstOrDefault(x => x.ObjectName.Equals("GetCommissionAgreementPrimaryKeys", StringComparison.InvariantCultureIgnoreCase));
			DbColumnDependencyRemover.DropSchemaBoundReferencingObjects(TestConnection, Db.SqlDbOwnerSchema, getCommissionAgreementPrimaryKeysScript.ObjectName);
			// Have to drop and re-create since by default client views are not recreated if they already exist, but this view will already exist in its generic schema version.
			Db.Connection.ExecuteNonQuery(getCommissionAgreementPrimaryKeysScript.DropScript);
			Db.Connection.ExecuteNonQuery(getCommissionAgreementPrimaryKeysScript.CreateScript);

			var organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			var agreement = Factory.NewWithValidTestData<EdiCommissionAgreement>();
			agreement.CA0_OH_Customer = organisation.PK;
			agreement.CA0_CommissionTriggerType = EDICommissionTriggerTypes.Codes.GoLiveComplete;
			agreement.Approve();
			Factory.Save();

			var effectiveDateCacheTable = new DataTable();
			effectiveDateCacheTable.Columns.Add("TriggerType", typeof(string));
			effectiveDateCacheTable.Columns.Add("CustomerPk", typeof(Guid));
			effectiveDateCacheTable.Columns.Add("EffectiveDate", typeof(DateTime));

			var pks = new List<Guid>();

			using (var command = Db.Connection.Command("SELECT VCA_PK FROM dbo.GetCommissionAgreementPrimaryKeys(@OpportunityClientPk, @CustomerPk, @CommissionStream, @CommissionDate, @EffectiveDateCacheTable)")) // Avoid to load all BOs
			{
				command.CommandType = CommandType.Text;
				command.AddParameter("@OpportunityClientPk", SqlDbType.UniqueIdentifier, DBNull.Value);
				command.AddParameter("@CustomerPk", SqlDbType.UniqueIdentifier, organisation.PK.ToGuid());
				command.AddParameter("@CommissionStream", SqlDbType.VarChar, 3, DBNull.Value);
				command.AddParameter("@CommissionDate", SqlDbType.DateTime, new DateTime(2023, 1, 1));
				command.AddTableValuedParameter("@EffectiveDateCacheTable", "dbo.TVP_TriggerTypeEffectiveDate", effectiveDateCacheTable);

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						pks.Add((Guid)reader["VCA_PK"]);
					}
				}
			}

			AssertEquals("Should be no agreements found", 0, pks.Count);
		}

		public void TestGetCommissionAgreementPrimaryKeys_TriggerTypeAYC()
		{
			var getCommissionAgreementPrimaryKeysScript = ((IExtensionObjects)new EDIClientDbSchemaUpgradeInfo()).ViewAndRoutineCreationScripts.FirstOrDefault(x => x.ObjectName.Equals("GetCommissionAgreementPrimaryKeys", StringComparison.InvariantCultureIgnoreCase));
			DbColumnDependencyRemover.DropSchemaBoundReferencingObjects(TestConnection, Db.SqlDbOwnerSchema, getCommissionAgreementPrimaryKeysScript.ObjectName);
			// Have to drop and re-create since by default client views are not recreated if they already exist, but this view will already exist in its generic schema version.
			Db.Connection.ExecuteNonQuery(getCommissionAgreementPrimaryKeysScript.DropScript);
			Db.Connection.ExecuteNonQuery(getCommissionAgreementPrimaryKeysScript.CreateScript);

			var primaryCharge = BillingTestHelper.CreateChargeCode(Factory, null, "PRIMARY");
			primaryCharge.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			primaryCharge.AC_ChargeGroup = ChargeCodeGroupList.Codes.NonJobRelated;
			var secondaryCharge = BillingTestHelper.CreateChargeCode(Factory, null, "SECONDARY");
			secondaryCharge.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			secondaryCharge.AC_ChargeGroup = ChargeCodeGroupList.Codes.NonJobRelated;

			Factory.Save();
			var settings = new AYCTriggerTypeSettings();
			settings.PrimaryChargeCode = primaryCharge.AC_Code;
			settings.SecondaryChargeCode = secondaryCharge.AC_Code;
			EDIDataRegistry.Instance.AYCTriggerTypeSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);

			var organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			var agreement = Factory.NewWithValidTestData<EdiCommissionAgreement>();
			agreement.CA0_OH_Customer = organisation.PK;
			agreement.CA0_CommissionTriggerType = EDICommissionTriggerTypes.Codes.AYCEStartDate;
			agreement.Approve();
			Factory.Save();

			var fee1 = BillingTestHelper.CreateLicenceFee(organisation.LicCompany, "AAA", 40m);
			fee1.L8_ChargeCode = primaryCharge.AC_Code;
			fee1.L8_StartDate = new ZDateTime(2020, 2, 2);
			var fee2 = BillingTestHelper.CreateLicenceFee(organisation.LicCompany, "AAA", 50m);
			fee2.L8_ChargeCode = secondaryCharge.AC_Code;
			fee2.L8_StartDate = new ZDateTime(2020, 2, 3);
			Factory.Save();

			var effectiveDateCacheTable = new DataTable();
			effectiveDateCacheTable.Columns.Add("TriggerType", typeof(string));
			effectiveDateCacheTable.Columns.Add("CustomerPk", typeof(Guid));
			effectiveDateCacheTable.Columns.Add("EffectiveDate", typeof(DateTime));

			var pks = new List<Guid>();

			using (var command = Db.Connection.Command("SELECT VCA_PK FROM dbo.GetCommissionAgreementPrimaryKeys(@OpportunityClientPk, @CustomerPk, @CommissionStream, @CommissionDate, @EffectiveDateCacheTable)")) // Avoid to load all BOs
			{
				command.CommandType = CommandType.Text;
				command.AddParameter("@OpportunityClientPk", SqlDbType.UniqueIdentifier, DBNull.Value);
				command.AddParameter("@CustomerPk", SqlDbType.UniqueIdentifier, organisation.PK.ToGuid());
				command.AddParameter("@CommissionStream", SqlDbType.VarChar, 3, DBNull.Value);
				command.AddParameter("@CommissionDate", SqlDbType.DateTime, new DateTime(2020, 2, 4));
				command.AddTableValuedParameter("@EffectiveDateCacheTable", "dbo.TVP_TriggerTypeEffectiveDate", effectiveDateCacheTable);

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						pks.Add((Guid)reader["VCA_PK"]);
					}
				}
			}

			AssertEquals("Should be one agreement found", 1, pks.Count);
			AssertEquals("Agreement found correct", agreement.PK, pks.First());
		}

		public void TestClientvw_SalesRelationNodeWithLastEdit()
		{
			var vw_SalesRelationNodeWithLastEditScript = ((IExtensionObjects)new EDIClientDbSchemaUpgradeInfo()).ViewAndRoutineCreationScripts.FirstOrDefault(x => x.ObjectName.Equals("vw_SalesRelationNodeWithLastEdit", StringComparison.InvariantCultureIgnoreCase));
			DbColumnDependencyRemover.DropSchemaBoundReferencingObjects(TestConnection, Db.SqlDbOwnerSchema, vw_SalesRelationNodeWithLastEditScript.ObjectName);
			// Have to drop and re-create since by default client views are not recreated if they already exist, but this view will already exist in its generic schema version.
			Db.Connection.ExecuteNonQuery(vw_SalesRelationNodeWithLastEditScript.DropScript);
			Db.Connection.ExecuteNonQuery(vw_SalesRelationNodeWithLastEditScript.CreateScript);

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var supportIncident = Factory.NewWithValidTestData<SupportIncident>();

			var orgOpportunity = Factory.New<EDIOrgOpportunity>();
			orgOpportunity.P8_OH = orgHeader.PK;
			orgOpportunity.RelatedChildActivityPivotCollection.AddNewPivot(supportIncident);
			Factory.Save();

			using (var cmd = Db.Connection.Command($@"select ActivitySystemLastEditTime from dbo.vw_SalesRelationNodeWithLastEdit 
												where ActivityID = '{supportIncident.PK}'"))
			{
				AssertEquals(supportIncident.IM_SystemLastEditTimeUtc, cmd.ExecuteScalar());
			}
		}

		public void TestClientConstraint_GenPivot_Constraint_XX_RelationType_XX_Relation1TableCode_XX_Relation2TableCode()
		{
			var script = ((IExtensionObjects)new EDIClientDbSchemaUpgradeInfo()).TableCreationScripts.FirstOrDefault(x => x.ObjectName.Equals("Constraint_XX_RelationType_XX_Relation1TableCode_XX_Relation2TableCode", StringComparison.InvariantCultureIgnoreCase));
			DbColumnDependencyRemover.DropSchemaBoundReferencingObjects(TestConnection, Db.SqlDbOwnerSchema, script.ObjectName);
			Db.Connection.ExecuteNonQuery(script.DropScript);
			Db.Connection.ExecuteNonQuery(script.CreateScript);

			using (var cmd = Db.Connection.Command($@"INSERT INTO dbo.GenPivot (XX_PK, XX_RelationType, XX_Relation1ID, XX_Relation2ID, XX_Sequence, XX_Relation1TableCode, XX_Relation2TableCode, XX_SystemCreateTimeUtc, XX_SystemCreateUser, XX_SystemLastEditTimeUtc, XX_SystemLastEditUser) VALUES ('8F070915-9053-483A-804F-2C7B30310C01', 'WRK', 'A006B996-532A-40F1-B8EB-426441188E01', 'A81F79CF-02FC-4461-885C-18CD844A2C01', 0, 'WKI', 'IM', GetUtcDate(), 'GG2', GetUtcDate(), 'GG2');"))
			{
				AssertExceptionThrown(typeof(SqlException), delegate
				{
					cmd.ExecuteNonQuery();
				});
			}
		}
	}
}
