using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.DbUpgrader.Scripts.Definitions.MarketingManager;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MarketingManager
{
	[TestedType(typeof(XT_Report_SalesFunnelActivity))]
	class XT_Report_SalesFunnelActivityTest : DbCreateScriptTest
	{
		public void TestShipmentsOnOrAfterStartDateButBeforeEndDateAreReturned()
		{
			var companyPk = Guid.NewGuid();
			var branchPk = Guid.NewGuid();
			InsertCompanyAndBranch(companyPk, branchPk);
			var departmentPk = Guid.NewGuid();
			InsertDepartment(departmentPk);
			var staff1Pk = Guid.NewGuid();
			var staff2Pk = Guid.NewGuid();
			InsertTestStaff(Guid.Empty, staff1Pk, "AAA", "AAA Sales Staff");
			InsertTestStaff(Guid.Empty, staff2Pk, "BBB", "BBB Sales Staff");

			var shipment1PK = Guid.NewGuid();
			var shipment2PK = Guid.NewGuid();
			var shipment3PK = Guid.NewGuid();
			AssertEquals("Precondition", 0, GetStaffNamesAndActivitiesFromRunningQuery(Activity.Shipments, companyPk).Count);

			InsertShipment(shipment1PK, "S00000001", companyPk, branchPk, departmentPk, "", new DateTime(2014, 10, 29));
			InsertShipment(shipment2PK, "S00000002", companyPk, branchPk, departmentPk, "AAA", new DateTime(2014, 10, 29));
			InsertShipment(shipment3PK, "S00000003", companyPk, branchPk, departmentPk, "BBB", new DateTime(2014, 10, 30));

			var actualList = GetStaffNamesAndActivitiesFromRunningQuery(Activity.Shipments, companyPk, "2014-10-28", "2014-10-29");
			var expectedItem1 = new KeyValuePair<string, int>("AAA Sales Staff", 1);
			var expectedItem2 = new KeyValuePair<string, int>("Not Assigned", 1);
			AssertEquals("Precondition", 2, actualList.Count);
			Assert(actualList.Contains(expectedItem1));
			Assert(actualList.Contains(expectedItem2));

			actualList = GetStaffNamesAndActivitiesFromRunningQuery(Activity.Shipments, companyPk, "2014-10-30", "2014-10-31");
			var expectedItem3 = new KeyValuePair<string, int>("BBB Sales Staff", 1);
			AssertEquals("Precondition", 1, actualList.Count);
			Assert(actualList.Contains(expectedItem3));

			actualList = GetStaffNamesAndActivitiesFromRunningQuery(Activity.Shipments, companyPk);
			AssertEquals("Show all shipments", 3, actualList.Count);
			Assert(actualList.Contains(expectedItem1));
			Assert(actualList.Contains(expectedItem2));
			Assert(actualList.Contains(expectedItem3));
		}

		void InsertShipment(Guid pk, string shipmentRef, Guid companyPk, Guid branchPk, Guid departmentPk, string salesRepCode, DateTime departureDate)
		{
			using (var sqlCmd = TestConnection.Command(@"INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_E_DEP) VALUES (@JS_PK, @JS_UniqueConsignRef, @JS_E_DEP)"))
			{
				sqlCmd.AddParameterBasedOnDbColumn("@JS_PK", pk, JobShipmentSchema.PK);
				sqlCmd.AddParameterBasedOnDbColumn("@JS_UniqueConsignRef", shipmentRef, JobShipmentSchema.JS_UniqueConsignRef);
				sqlCmd.AddParameterBasedOnDbColumn("@JS_E_DEP", departureDate, JobShipmentSchema.JS_E_DEP);
				sqlCmd.ExecuteNonQuery();
			}
			using (var sqlCmd = TestConnection.Command(@"INSERT dbo.JobHeader (JH_PK, JH_GC, JH_GB, JH_GE, JH_JobNum, JH_ParentID, JH_ParentTableCode, JH_GS_NKRepSales, JH_Status) VALUES (newid(), @JH_GC, @JH_GB, @JH_GE, @JH_JobNum, @JH_ParentID, @JH_ParentTableCode, @JH_GS_NKRepSales, @JH_Status)"))
			{
				sqlCmd.AddParameterBasedOnDbColumn("@JH_GC", companyPk, JobHeaderSchema.JH_GC);
				sqlCmd.AddParameterBasedOnDbColumn("@JH_GB", branchPk, JobHeaderSchema.JH_GB);
				sqlCmd.AddParameterBasedOnDbColumn("@JH_GE", departmentPk, JobHeaderSchema.JH_GE);
				sqlCmd.AddParameterBasedOnDbColumn("@JH_JobNum", shipmentRef, JobHeaderSchema.JH_JobNum);
				sqlCmd.AddParameterBasedOnDbColumn("@JH_ParentID", pk, JobHeaderSchema.JH_ParentID);
				sqlCmd.AddParameterBasedOnDbColumn("@JH_ParentTableCode", "JS", JobHeaderSchema.JH_ParentTableCode);
				sqlCmd.AddParameterBasedOnDbColumn("@JH_GS_NKRepSales", salesRepCode, JobHeaderSchema.JH_GS_NKRepSales);
				sqlCmd.AddParameterBasedOnDbColumn("@JH_Status", "WRK", JobHeaderSchema.JH_GS_NKRepSales);
				sqlCmd.ExecuteNonQuery();
			}
		}

		public void TestStaffWithOnlyAdditionalVisitsAreReturned()
		{
			var companyPk = Guid.NewGuid();
			var branchPk = Guid.NewGuid();
			var orgSalesCallPk = Guid.NewGuid();
			var staffPk = Guid.NewGuid();

			SetupTestSalesCall(companyPk, branchPk, orgSalesCallPk, "CM00000001");
			InsertTestStaff(branchPk, staffPk, @"AAA", @"My Test Staff");
			var actualList = GetStaffNamesAndActivitiesFromRunningQuery(Activity.CallsVisits, companyPk);
			var expectedItem1 = new KeyValuePair<string, int>("Not Assigned", 1);
			AssertEquals("Precondition", 1, actualList.Count);
			Assert(actualList.Contains(expectedItem1));

			actualList = GetStaffNamesAndActivitiesFromRunningQuery(Activity.CallsVisitsAdditional, companyPk);
			var expectedItem2 = new KeyValuePair<string, int>("Not Assigned", 0);
			AssertEquals("Precondition - because there's an unassigned call visit", 1, actualList.Count);
			Assert(actualList.Contains(expectedItem2));

			MakeStaffAnAdditionalVisitForCall(orgSalesCallPk, staffPk);
			actualList = GetStaffNamesAndActivitiesFromRunningQuery(Activity.CallsVisitsAdditional, companyPk);
			var expectedItem3 = new KeyValuePair<string, int>("My Test Staff", 1);
			AssertEquals("Precondition", 2, actualList.Count);
			Assert(actualList.Contains(expectedItem2));
			Assert(actualList.Contains(expectedItem3));
		}

		public void TestStaffWithOnlyProcessTasksAreNotReturned()
		{
			var companyPk = Guid.NewGuid();
			var branchPk = Guid.NewGuid();
			InsertCompanyAndBranch(companyPk, branchPk);
			var orgPk = Guid.NewGuid();
			InsertOrg(orgPk);

			InsertTestStaff(Guid.Empty, Guid.NewGuid(), "AAA", "AAA Sales Staff");
			InsertTestStaff(Guid.Empty, Guid.NewGuid(), "BBB", "BBB Sales Staff");

			InsertProcessTask("AAA", companyPk);
			InsertProcessTask("BBB", companyPk);

			var actualList = GetStaffNamesAndActivitiesFromRunningQuery(Activity.Tasks, companyPk);
			AssertEquals("Precondition - do not show if they do not have any other activity", 0, actualList.Count);

			InsertEnquiry(Guid.NewGuid(), "I00000001", orgPk, "INQ", new DateTime(2014, 11, 17), "AAA");
			actualList = GetStaffNamesAndActivitiesFromRunningQuery(Activity.Tasks, companyPk);
			var expectedItem1 = new KeyValuePair<string, int>("AAA Sales Staff", 1);
			AssertEquals("Precondition", 1, actualList.Count);
			Assert(actualList.Contains(expectedItem1));

			InsertOpportunity(orgPk, companyPk, null, new DateTime(2014, 11, 17), "BBB", "OppId1");
			actualList = GetStaffNamesAndActivitiesFromRunningQuery(Activity.Tasks, companyPk);
			var expectedItem2 = new KeyValuePair<string, int>("BBB Sales Staff", 1);
			AssertEquals("Precondition", 2, actualList.Count);
			Assert(actualList.Contains(expectedItem1));
			Assert(actualList.Contains(expectedItem2));
		}

		public void TestStaffMultipleSelection()
		{
			var companyPk = Guid.NewGuid();
			var branchPk = Guid.NewGuid();
			var orgSalesCallPk = Guid.NewGuid();
			var staff1Pk = Guid.NewGuid();
			var staff2Pk = Guid.NewGuid();
			var staff3Pk = Guid.NewGuid();

			SetupTestSalesCall(companyPk, branchPk, orgSalesCallPk, "CM00000001");
			InsertTestStaff(branchPk, staff1Pk, @"AAA", @"AAA Staff 1");
			InsertTestStaff(branchPk, staff2Pk, @"BBB", @"BBB Staff 2");
			InsertTestStaff(branchPk, staff3Pk, @"CCC", @"CCC Staff 3");

			MakeStaffAnAdditionalVisitForCall(orgSalesCallPk, staff1Pk);
			MakeStaffAnAdditionalVisitForCall(orgSalesCallPk, staff2Pk);
			MakeStaffAnAdditionalVisitForCall(orgSalesCallPk, staff3Pk);

			var actualList = GetStaffNamesAndActivitiesFromRunningQuery(Activity.CallsVisitsAdditional, companyPk, staff1Pk, staff3Pk);
			var expectedItems = new List<KeyValuePair<string, int>> {
				new KeyValuePair<string, int>("AAA Staff 1", 1),
				new KeyValuePair<string, int>("CCC Staff 3", 1)
			};

			AssertEquals("Precondition", 2, actualList.Count);
			AssertContainsExactElementsInAnyOrder(expectedItems, actualList);
		}

		void InsertProcessTask(string staffCode, Guid companyPK)
		{
			Guid taskPk = Guid.NewGuid();
			using (var command = TestConnection.Command(@"INSERT INTO dbo.ProcessTasks(P9_PK, P9_GS_NKAssignedStaffMember, P9_GC)
				values(@P9_PK, @P9_GS_NKAssignedStaffMember, @P9_GC)"))
			{
				command.AddParameterBasedOnDbColumn("@P9_PK", taskPk, ProcessTasksSchema.PK);
				command.AddParameterBasedOnDbColumn("@P9_GS_NKAssignedStaffMember", staffCode, ProcessTasksSchema.P9_GS_NKAssignedStaffMember);
				command.AddParameterBasedOnDbColumn("@P9_GC", companyPK, ProcessTasksSchema.P9_GC);
				command.ExecuteNonQuery();
			}

			using (var command = TestConnection.Command(@"INSERT INTO dbo.StmALog (SL_PK, SL_EventTime, SL_PostedTimeUtc, SL_SE_NKEvent, SL_Parent, SL_Table)
				VALUES(NEWID(), GETDATE(), GETUTCDATE(), @SL_SE_NKEvent, @SL_Parent, 'ProcessTasks')"))
			{
				command.AddParameterBasedOnDbColumn("@SL_SE_NKEvent", "ADD", StmALogSchema.SL_SE_NKEvent);
				command.AddParameterBasedOnDbColumn("@SL_Parent", taskPk, StmALogSchema.SL_Parent);
				command.ExecuteNonQuery();
			}
		}

		void InsertTestStaff(Guid branchPk, Guid staffPk, string code, string staffName)
		{
			var personPk = Guid.NewGuid();
			using (var command = TestConnection.Command(@"insert into dbo.GlbPerson (PER_PK, PER_FullName) values (@PER_PK, 'name')"))
			{
				command.AddParameterBasedOnDbColumn("@PER_PK", personPk, GlbStaffSchema.PK);
				command.ExecuteNonQuery();
			}

			using (var command = TestConnection.Command(@"INSERT INTO dbo.GlbStaff (GS_PK, GS_CODE, GS_LoginName, GS_FullName, GS_GB_HomeBranch, GS_IsActive, GS_PER, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES (@GS_PK, @GS_CODE, @GS_LoginName, @GS_FullName, @GS_GB_HomeBranch, 1, @GS_PER, GETUTCDATE(), 'E', GETUTCDATE(), 'E')"))
			{
				command.AddParameterBasedOnDbColumn("@GS_PK", staffPk, GlbStaffSchema.PK);
				command.AddParameterBasedOnDbColumn("@GS_CODE", code, GlbStaffSchema.GS_Code);
				command.AddParameterBasedOnDbColumn("@GS_LoginName", code, GlbStaffSchema.GS_LoginName);
				command.AddParameterBasedOnDbColumn("@GS_FullName", staffName, GlbStaffSchema.GS_FullName);
				command.AddParameterBasedOnDbColumn("@GS_GB_HomeBranch", branchPk != Guid.Empty ? branchPk : DBNull.Value, GlbStaffSchema.GS_GB_HomeBranch);
				command.AddParameterBasedOnDbColumn("@GS_PER", personPk, GlbStaffSchema.GS_PER);
				command.ExecuteNonQuery();
			}
		}

		void SetupTestSalesCall(Guid companyPk, Guid branchPk, Guid orgSalesCallPk, string communicationId)
		{
			InsertCompanyAndBranch(companyPk, branchPk);

			Guid orgPk = Guid.NewGuid();
			using (var command = TestConnection.Command(@"INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES (@OH_PK, @OH_Code)"))
			{
				command.AddParameterBasedOnDbColumn("@OH_PK", orgPk, OrgHeaderSchema.PK);
				command.AddParameterBasedOnDbColumn("@OH_Code", "TESTORG", OrgHeaderSchema.OH_Code);
				command.ExecuteNonQuery();
			}

			using (var command = TestConnection.Command(@"INSERT INTO dbo.OrgSalesCall (OQ_PK, OQ_OH, OQ_CallDate, OQ_CommunicationID, OQ_SystemCreateTimeUtc, OQ_SystemCreateUser, OQ_SystemLastEditTimeUtc, OQ_SystemLastEditUser)
VALUES (@OQ_PK, @OQ_OH, @OQ_CallDate, @OQ_CommunicationID, GetUtcDate(), 'E', GetUtcDate(), 'E')"))
			{
				command.AddParameterBasedOnDbColumn("@OQ_PK", orgSalesCallPk, OrgSalesCallSchema.PK);
				command.AddParameterBasedOnDbColumn("@OQ_OH", orgPk, OrgSalesCallSchema.OQ_OH);
				command.AddParameterBasedOnDbColumn("@OQ_CallDate", DateTime.Now, OrgSalesCallSchema.OQ_CallDate);
				command.AddParameterBasedOnDbColumn("@OQ_CommunicationID", communicationId, OrgSalesCallSchema.OQ_CommunicationID);
				command.ExecuteNonQuery();
			}
		}

		void InsertCompanyAndBranch(Guid companyPk, Guid branchPk)
		{
			using (var sqlCmd = TestConnection.Command(@"INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES (@GC_PK, 'DAN', 'AU company', 'AU', 'AUD')"))
			{
				sqlCmd.AddParameterBasedOnDbColumn("@GC_PK", companyPk, GlbCompanySchema.PK);
				sqlCmd.ExecuteNonQuery();
			}

			using (var sqlCmd = TestConnection.Command(@"INSERT INTO dbo.GlbBranch (GB_PK, GB_GC) VALUES (@GB_PK, @GB_GC)"))
			{
				sqlCmd.AddParameterBasedOnDbColumn("@GB_PK", branchPk, GlbBranchSchema.PK);
				sqlCmd.AddParameterBasedOnDbColumn("@GB_GC", companyPk, GlbBranchSchema.GB_GC);
				sqlCmd.ExecuteNonQuery();
			}
		}

		void InsertDepartment(Guid pk)
		{
			using (var sqlCmd = TestConnection.Command(@"INSERT dbo.GlbDepartment (GE_PK) VALUES (@GE_PK)"))
			{
				sqlCmd.AddParameterBasedOnDbColumn("@GE_PK", pk, GlbDepartmentSchema.PK);
				sqlCmd.ExecuteNonQuery();
			}
		}

		void MakeStaffAnAdditionalVisitForCall(Guid orgSalesCallPk, Guid staffPk)
		{
			using (var command = TestConnection.Command(@"INSERT INTO dbo.OrgSalesCallAdditionalAttendee (O6_PK, O6_AttendeeID, O6_AttendeeTableCode, O6_OQ, O6_SystemCreateTimeUtc, O6_SystemCreateUser, O6_SystemLastEditTimeUtc, O6_SystemLastEditUser)
VALUES (@O6_PK, @O6_AttendeeID, 'GS', @O6_OQ, GetUtcDate(), 'E', GetUtcDate(), 'E')"))
			{
				command.AddParameterBasedOnDbColumn("@O6_PK", Guid.NewGuid(), OrgSalesCallAdditionalAttendeeSchema.PK);
				command.AddParameterBasedOnDbColumn("@O6_AttendeeID", staffPk, OrgSalesCallAdditionalAttendeeSchema.O6_AttendeeID);
				command.AddParameterBasedOnDbColumn("@O6_OQ", orgSalesCallPk, OrgSalesCallAdditionalAttendeeSchema.O6_OQ);
				command.ExecuteNonQuery();
			}
		}

		class Activity
		{
			public const string CallsVisits = "CallsVisits";
			public const string CallsVisitsAdditional = "CallsVisitsAdditional";
			public const string NonQualifiedInquiries = "ColdCalls";
			public const string QualifiedInquiries = "ColdCallsQualified";
			public const string ConvertedInquiries = "OpportunitiesConverted";
			public const string StandaloneOpportunities = "OpportunitiesStandalone";
			public const string Shipments = "Shipments";
			public const string Tasks = "ProcessTasks";
		}

		IList<KeyValuePair<string, int>> GetStaffNamesAndActivitiesFromRunningQuery(string activity, Guid companyPk, string startDate = "1900-1-1", string endDate = "2070-12-31")
		{
			var result = new List<KeyValuePair<string, int>>();

			using (var command = TestConnection.Command(
@"XT_Report_SalesFunnelActivity"))
			{
				command.CommandType = System.Data.CommandType.StoredProcedure;
				command.AddParameter("@StartDateStr", SqlDbType.VarChar, startDate);
				command.AddParameter("@EndDateStr", SqlDbType.VarChar, endDate);

				var salesRepPKs = new TVPParamInfo("@SalesRepPKs", "dbo.TVP_UNIQUEIDENTIFIER", typeof(Guid), Array.Empty<object>());
				salesRepPKs.AddTVPParameters(command);

				command.AddParameter("@SalesRepIsEmpty", SqlDbType.Bit, 1);
				command.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, companyPk);
				command.AddParameter("@OrderBy", SqlDbType.VarChar, "");

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						result.Add(new KeyValuePair<string, int>((string)reader["StaffName"], (int)reader[activity]));
					}
				}
			}

			return result;
		}

		IList<KeyValuePair<string, int>> GetStaffNamesAndActivitiesFromRunningQuery(string activity, Guid companyPk, params Guid[] staff)
		{
			var result = new List<KeyValuePair<string, int>>();

			using (var command = TestConnection.Command(
@"XT_Report_SalesFunnelActivity"))
			{
				command.CommandType = System.Data.CommandType.StoredProcedure;
				command.AddParameter("@StartDateStr", SqlDbType.VarChar, "1900-1-1");
				command.AddParameter("@EndDateStr", SqlDbType.VarChar, "2070-12-31");

				var salesRepPKs = new TVPParamInfo("@SalesRepPKs", "dbo.TVP_UNIQUEIDENTIFIER", typeof(Guid), staff.Cast<object>());
				salesRepPKs.AddTVPParameters(command);

				command.AddParameter("@SalesRepIsEmpty", SqlDbType.Bit, staff.Length == 0);
				command.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, companyPk);
				command.AddParameter("@OrderBy", SqlDbType.VarChar, "");

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						result.Add(new KeyValuePair<string, int>((string)reader["StaffName"], (int)reader[activity]));
					}
				}
			}

			return result;
		}

		public void TestConvertedInquiriesAndStandaloneOpportunitiesAreReturned()
		{
			var companyPk = Guid.NewGuid();
			var branchPk = Guid.NewGuid();
			var orgPk = Guid.NewGuid();
			InsertCompanyAndBranch(companyPk, branchPk);
			InsertOrg(orgPk);

			InsertTestStaff(Guid.Empty, Guid.NewGuid(), "SCW", "Sam");
			InsertTestStaff(Guid.Empty, Guid.NewGuid(), "JNG", "Jenny");

			Guid scwEnquiry1 = Guid.NewGuid();
			Guid scwEnquiry2 = Guid.NewGuid();
			Guid jngEnquiry = Guid.NewGuid();

			InsertEnquiry(scwEnquiry1, "I00000001", orgPk, "INQ", new DateTime(2014, 11, 6), "SCW");
			InsertEnquiry(scwEnquiry2, "I00000002", orgPk, "INQ", new DateTime(2014, 11, 6), "SCW");
			InsertEnquiry(jngEnquiry, "I00000003", orgPk, "INQ", new DateTime(2014, 11, 6), "JNG");

			InsertOpportunity(orgPk, companyPk, scwEnquiry1, new DateTime(2014, 11, 7), "SCW", "OppId1");
			InsertOpportunity(orgPk, companyPk, scwEnquiry2, new DateTime(2014, 11, 7), "SCW", "OppId2");
			InsertOpportunity(orgPk, companyPk, null, new DateTime(2014, 11, 7), "SCW", "OppId3");
			InsertOpportunity(orgPk, companyPk, jngEnquiry, new DateTime(2014, 11, 7), "JNG", "OppId4");
			InsertOpportunity(orgPk, companyPk, null, new DateTime(2014, 11, 7), "JNG", "OppId5");
			InsertOpportunity(orgPk, companyPk, null, new DateTime(2014, 11, 7), "JNG", "OppId6");

			InsertOpportunity(orgPk, companyPk, scwEnquiry1, new DateTime(2014, 11, 7), "", "OppId7");
			InsertOpportunity(orgPk, companyPk, jngEnquiry, new DateTime(2014, 11, 7), "", "OppId8");
			InsertOpportunity(orgPk, companyPk, null, new DateTime(2014, 11, 7), "", "OppId9");

			var actualList = GetStaffNamesAndActivitiesFromRunningQuery(Activity.ConvertedInquiries, Guid.NewGuid());
			var expectedItem1 = new KeyValuePair<string, int>("Sam", 2);
			var expectedItem2 = new KeyValuePair<string, int>("Jenny", 1);
			var expectedItem3 = new KeyValuePair<string, int>("Not Assigned", 2);
			AssertEquals("Precondition", 3, actualList.Count);
			Assert(actualList.Contains(expectedItem1));
			Assert(actualList.Contains(expectedItem2));
			Assert(actualList.Contains(expectedItem3));

			actualList = GetStaffNamesAndActivitiesFromRunningQuery(Activity.StandaloneOpportunities, Guid.NewGuid());
			var expectedItem4 = new KeyValuePair<string, int>("Sam", 1);
			var expectedItem5 = new KeyValuePair<string, int>("Jenny", 2);
			var expectedItem6 = new KeyValuePair<string, int>("Not Assigned", 1);
			AssertEquals("Precondition", 3, actualList.Count);
			Assert(actualList.Contains(expectedItem4));
			Assert(actualList.Contains(expectedItem5));
			Assert(actualList.Contains(expectedItem6));
		}

		void InsertOpportunity(Guid orgPk, Guid companyPk, Guid? enquiryPK, DateTime addTime, string staffCode, string opportunityId)
		{
			using (var command = TestConnection.Command(@"INSERT INTO dbo.OrgOpportunity (P8_PK, P8_GC, P8_OH, P8_O1_Enquiry, P8_GS_NKPrimarySalesPerson, P8_OpportunityID, P8_SystemCreateTimeUtc, P8_SystemCreateUser, P8_SystemLastEditTimeUtc, P8_SystemLastEditUser)
VALUES (newid(), @P8_GC, @P8_OH, @P8_O1_Enquiry, @P8_GS_NKPrimarySalesPerson, @P8_OpportunityID, @P8_SystemCreateTimeUtc, 'E', GetUtcDate(), 'E')"))
			{
				command.AddParameterBasedOnDbColumn("@P8_OH", orgPk, OrgOpportunitySchema.P8_OH);
				command.AddParameterBasedOnDbColumn("@P8_GC", companyPk, OrgOpportunitySchema.P8_GC);
				command.AddParameterBasedOnDbColumn("@P8_O1_Enquiry", (enquiryPK != null) ? enquiryPK : DBNull.Value, OrgOpportunitySchema.P8_O1_Enquiry);
				command.AddParameterBasedOnDbColumn("@P8_GS_NKPrimarySalesPerson", staffCode, OrgOpportunitySchema.P8_GS_NKPrimarySalesPerson);
				command.AddParameterBasedOnDbColumn("@P8_OpportunityID", opportunityId, OrgOpportunitySchema.P8_OpportunityID);
				command.AddParameterBasedOnDbColumn("@P8_SystemCreateTimeUtc", addTime, OrgOpportunitySchema.P8_SystemCreateTimeUtc);
				command.ExecuteNonQuery();
			}
		}

		void InsertOrg(Guid pk)
		{
			using (var command = TestConnection.Command(@"INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_SystemCreateTimeUtc) VALUES (@OH_PK, @OH_Code, GETDATE())"))
			{
				command.AddParameterBasedOnDbColumn("@OH_PK", pk, OrgHeaderSchema.PK);
				command.AddParameterBasedOnDbColumn("@OH_Code", "DDDSAMORG", OrgHeaderSchema.OH_Code);
				command.ExecuteNonQuery();
			}
		}

		public void TestQualifiedAndNotQualifiedColdCallsOrInquiriesAreReturned()
		{
			var orgPk = Guid.NewGuid();
			InsertOrg(orgPk);

			InsertTestStaff(Guid.Empty, Guid.NewGuid(), "SCW", "Sam");
			InsertTestStaff(Guid.Empty, Guid.NewGuid(), "JNG", "Jenny");

			InsertEnquiry(Guid.NewGuid(), "I00000001", orgPk, "CCR", new DateTime(2012, 2, 12), "SCW");
			InsertEnquiry(Guid.NewGuid(), "I00000002", orgPk, "INQ", new DateTime(2012, 2, 12), "SCW");
			InsertEnquiry(Guid.NewGuid(), "I00000003", null, "CCR", new DateTime(2012, 2, 12), "JNG");
			InsertEnquiry(Guid.NewGuid(), "I00000004", null, "INQ", new DateTime(2012, 2, 12), "JNG");
			InsertEnquiry(Guid.NewGuid(), "I00000005", orgPk, "CCR", new DateTime(2012, 2, 12), "");
			InsertEnquiry(Guid.NewGuid(), "I00000006", null, "INQ", new DateTime(2012, 2, 12), "");

			var actualList = GetStaffNamesAndActivitiesFromRunningQuery(Activity.QualifiedInquiries, Guid.NewGuid());
			var expectedItem1 = new KeyValuePair<string, int>("Sam", 2);
			var expectedItem2 = new KeyValuePair<string, int>("Jenny", 0);
			var expectedItem3 = new KeyValuePair<string, int>("Not Assigned", 1);
			AssertEquals("Precondition", 3, actualList.Count);
			Assert(actualList.Contains(expectedItem1));
			Assert(actualList.Contains(expectedItem2));
			Assert(actualList.Contains(expectedItem3));

			actualList = GetStaffNamesAndActivitiesFromRunningQuery(Activity.NonQualifiedInquiries, Guid.NewGuid());
			var expectedItem4 = new KeyValuePair<string, int>("Sam", 0);
			var expectedItem5 = new KeyValuePair<string, int>("Jenny", 2);
			var expectedItem6 = new KeyValuePair<string, int>("Not Assigned", 1);
			AssertEquals("Precondition", 3, actualList.Count);
			Assert(actualList.Contains(expectedItem4));
			Assert(actualList.Contains(expectedItem5));
			Assert(actualList.Contains(expectedItem6));
		}

		void InsertEnquiry(Guid pk, string reference, Guid? orgPk, string classType, DateTime addTime, string staffCode)
		{
			using (var command = TestConnection.Command(@"INSERT INTO dbo.OrgColdCallRegister (O1_PK, O1_LeadUniqueReference, O1_EnquiryType, O1_OH_ConvertedToQualifiedLead, O1_GS_NKRepAssigned, O1_SystemCreateTimeUtc, O1_SystemCreateUser, O1_SystemLastEditTimeUtc, O1_SystemLastEditUser)
VALUES (@O1_PK, @O1_LeadUniqueReference, @O1_EnquiryType, @O1_OH_ConvertedToQualifiedLead, @O1_GS_NKRepAssigned, @O1_SystemCreateTimeUtc, 'E', GetUtcDate(), 'E')"))
			{
				command.AddParameterBasedOnDbColumn("@O1_PK", pk, OrgColdCallRegisterSchema.PK);
				command.AddParameterBasedOnDbColumn("@O1_LeadUniqueReference", reference, OrgColdCallRegisterSchema.O1_LeadUniqueReference);
				command.AddParameterBasedOnDbColumn("@O1_EnquiryType", classType, OrgColdCallRegisterSchema.O1_EnquiryType);
				command.AddParameterBasedOnDbColumn("@O1_OH_ConvertedToQualifiedLead", (orgPk != null) ? orgPk : DBNull.Value, OrgColdCallRegisterSchema.O1_OH_ConvertedToQualifiedLead);
				command.AddParameterBasedOnDbColumn("@O1_GS_NKRepAssigned", staffCode, OrgColdCallRegisterSchema.O1_GS_NKRepAssigned);
				command.AddParameterBasedOnDbColumn("@O1_SystemCreateTimeUtc", addTime, OrgColdCallRegisterSchema.O1_SystemCreateTimeUtc);
				command.ExecuteNonQuery();
			}
		}
	}
}

