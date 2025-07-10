using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MarketingManager;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MarketingManager.Testing
{
	[TestedType(typeof(Report_SalesMarketingInquiryV2))]
	class Report_SalesMarketingInquiryV2Test : DbCreateScriptTest
	{
		public void TestEnquiriesWithMultipleNotesAreDistinctlyReturned()
		{
			var enq1 = Guid.NewGuid();
			var enq2 = Guid.NewGuid();
			var enq3 = Guid.NewGuid();
			var enq4 = Guid.NewGuid();

			InsertEnquiry(enq1, "I00000001");
			InsertEnquiry(enq2, "I00000002");
			InsertEnquiry(enq3, "I00000003");
			InsertEnquiry(enq4, "I00000004");

			InsertNote(enq1, "Blob Details", "Enquiry 1 Notes", "DOC", true);
			InsertNote(enq1, "internal note", "this is an internal note", "INT");
			InsertNote(enq1, "client-visible note", "this is a client-visible note", "PUB");
			InsertNote(enq1, "private note", "this is a private note", "PRV");
			InsertNote(enq1, "agent-visible note", "this is an agent-visible note", "AGV");

			InsertNote(enq2, "Blob Details", "Enquiry 2 Notes", "DOC", true);
			InsertNote(enq2, "Blob Details", "Who knows, maybe theres a duplicate Blob Details DOC note in the future", "DOC", true);

			InsertNote(enq3, "Blob Details", "Enquiry 3 Notes", "DOC", true);
			InsertNote(enq3, "Blob Details", "Described an internal note as Blob Details", "INT", true);

			var actualList = GetEnquiryPKsAndNotesFromRunningQuery();
			var expectedItem1 = new KeyValuePair<Guid, string>(enq1, "Enquiry 1 Notes");
			var expectedItem2 = new KeyValuePair<Guid, string>(enq2, "Enquiry 2 Notes");
			var expectedItem3 = new KeyValuePair<Guid, string>(enq3, "Enquiry 3 Notes");
			var expectedItem4 = new KeyValuePair<Guid, string>(enq4, "");
			AssertEquals("Precondition", 4, actualList.Count);
			Assert(actualList.Contains(expectedItem1));
			Assert(actualList.Contains(expectedItem2));
			Assert(actualList.Contains(expectedItem3));
			Assert(actualList.Contains(expectedItem4));
		}

		void InsertEnquiry(Guid pk, string reference)
		{
			using (var command = TestConnection.Command(@"INSERT INTO dbo.OrgColdCallRegister (O1_PK, O1_LeadUniqueReference, O1_SystemCreateTimeUtc, O1_SystemCreateUser, O1_SystemLastEditTimeUtc, O1_SystemLastEditUser)
VALUES (@O1_PK, @O1_LeadUniqueReference, GetUtcDate(), 'E', GetUtcDate(), 'E')"))
			{
				command.AddParameterBasedOnDbColumn("@O1_PK", pk, OrgColdCallRegisterSchema.PK);
				command.AddParameterBasedOnDbColumn("@O1_LeadUniqueReference", reference, OrgColdCallRegisterSchema.O1_LeadUniqueReference);
				command.ExecuteNonQuery();
			}
		}

		void InsertNote(Guid parentPK, string description, string data, string type, bool isCustomNote = false)
		{
			Guid notePk = Guid.NewGuid();
			using (var command = TestConnection.Command(string.Format(@"INSERT INTO dbo.StmNote (ST_PK, ST_ParentID, ST_Table, ST_Description, ST_NoteContext, ST_NoteData, ST_NoteType, ST_IsCustomDescription) 
				VALUES(@ST_PK, @ST_ParentID, @ST_Table, @ST_Description, @ST_NoteContext, CAST('{0}' as VARBINARY(MAX)), @ST_NoteType, @ST_IsCustomDescription)", data)))
			{
				command.AddParameterBasedOnDbColumn("@ST_PK", notePk, StmNoteSchema.PK);
				command.AddParameterBasedOnDbColumn("@ST_ParentID", parentPK, StmNoteSchema.ST_ParentID);
				command.AddParameterBasedOnDbColumn("@ST_Table", "OrgColdCallRegister", StmNoteSchema.ST_Table);
				command.AddParameterBasedOnDbColumn("@ST_Description", description, StmNoteSchema.ST_Description);
				command.AddParameterBasedOnDbColumn("@ST_NoteContext", "AAA", StmNoteSchema.ST_NoteContext);
				command.AddParameterBasedOnDbColumn("@ST_NoteType", type, StmNoteSchema.ST_NoteType);
				command.AddParameterBasedOnDbColumn("@ST_IsCustomDescription", isCustomNote, StmNoteSchema.ST_IsCustomDescription);
				command.ExecuteNonQuery();
			}

			using (var command = TestConnection.Command(@"INSERT INTO dbo.StmALog (SL_PK, SL_EventTime, SL_PostedTimeUtc, SL_SE_NKEvent, SL_Parent, SL_Table)
				VALUES(NEWID(), GETDATE(), GETUTCDATE(), @SL_SE_NKEvent, @SL_Parent, 'StmNote')"))
			{
				command.AddParameterBasedOnDbColumn("@SL_SE_NKEvent", "ADD", StmALogSchema.SL_SE_NKEvent);
				command.AddParameterBasedOnDbColumn("@SL_Parent", notePk, StmALogSchema.SL_Parent);
				command.ExecuteNonQuery();
			}
		}

		IList<KeyValuePair<Guid, string>> GetEnquiryPKsAndNotesFromRunningQuery()
		{
			var result = new List<KeyValuePair<Guid, string>>();

			using (var command = TestConnection.Command("SELECT InquiryPK, dbo.CLRUncompressRTFAsPlainText(NoteData) As NoteDataAsPlainText FROM Report_SalesMarketingInquiryV2(@RecentActivityType, @RelatedActivityType, NULL)"))
			{
				command.AddParameter("@RecentActivityType", SqlDbType.VarChar, "ANY");
				command.AddParameter("@RelatedActivityType", SqlDbType.VarChar, DBNull.Value);

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var noteDataAsPlainText = reader["NoteDataAsPlainText"];
						result.Add(new KeyValuePair<Guid, string>((Guid)reader["InquiryPK"], noteDataAsPlainText == DBNull.Value ? string.Empty : (string)noteDataAsPlainText));
					}
				}
			}

			return result;
		}

		#region Test ReferringContact

		public void TestEnquiriesWithReferringContact()
		{
			var enq1 = Guid.NewGuid();
			var enq2 = Guid.NewGuid();
			var enq3 = Guid.NewGuid();
			var enq4 = Guid.NewGuid();

			InsertEnquiryWithReferringContactName("TestOrg1", "Test Organisation 1", "Test Contact Name 1", enq1, "I00000001", "LeadSourcePerson 1");
			InsertEnquiryWithReferringContactName("TestOrg2", "Test Organisation 2", "Test Contact Name 2", enq2, "I00000002", "");
			InsertEnquiryWithReferringContactName("", "", "", enq3, "I00000003", "LeadSourcePerson 3");
			InsertEnquiryWithReferringContactName("", "", "", enq4, "I00000004", "");

			var actualList = GetEnquiryPKsAndReferringContactFromRunningQuery();

			var expectedItem1 = new KeyValuePair<Guid, string>(enq1, "Test Contact Name 1");
			var expectedItem2 = new KeyValuePair<Guid, string>(enq2, "Test Contact Name 2");
			var expectedItem3 = new KeyValuePair<Guid, string>(enq3, "LeadSourcePerson 3");
			var expectedItem4 = new KeyValuePair<Guid, string>(enq4, "");

			Assert(actualList.Contains(expectedItem1));
			Assert(actualList.Contains(expectedItem2));
			Assert(actualList.Contains(expectedItem3));
			Assert(actualList.Contains(expectedItem4));
		}

		void InsertEnquiryWithReferringContactName(string orgHeaderCode, string orgHeaderFullName, string contactName, Guid orgColdCallRegisterPK, string reference, string leadSourcePerson)
		{
			var orgHeaderPK = InsertOrg(orgHeaderCode, orgHeaderFullName);
			var orgContactPK = InsertOrgContact(orgHeaderPK, contactName);

			using (var command = TestConnection.Command(@"INSERT INTO dbo.OrgColdCallRegister (O1_PK, O1_LeadUniqueReference, O1_LeadSourcePerson, O1_OC_ReferringContact, O1_SystemCreateTimeUtc, O1_SystemCreateUser, O1_SystemLastEditTimeUtc, O1_SystemLastEditUser)
VALUES (@O1_PK, @O1_LeadUniqueReference, @O1_LeadSourcePerson, @O1_OC_ReferringContact, GetUtcDate(), 'E', GetUtcDate(), 'E')"))
			{
				command.AddParameterBasedOnDbColumn("@O1_PK", orgColdCallRegisterPK, OrgColdCallRegisterSchema.PK);
				command.AddParameterBasedOnDbColumn("@O1_LeadUniqueReference", reference, OrgColdCallRegisterSchema.O1_LeadUniqueReference);
				command.AddParameterBasedOnDbColumn("@O1_LeadSourcePerson", leadSourcePerson, OrgColdCallRegisterSchema.O1_LeadSourcePerson);
				command.AddParameterBasedOnDbColumn("@O1_OC_ReferringContact", (orgContactPK != Guid.Empty) ? orgContactPK : DBNull.Value, OrgColdCallRegisterSchema.O1_OC_ReferringContact);
				command.ExecuteNonQuery();
			}
		}

		Guid InsertOrg(string orgHeaderCode, string orgHeaderFullName)
		{
			if (string.IsNullOrEmpty(orgHeaderCode))
			{
				return Guid.Empty;
			}

			var orgHeaderPK = Guid.NewGuid();
			using (var command = TestConnection.Command(@"INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName) VALUES (@OH_PK, @OH_Code, @OH_FullName)"))
			{
				command.AddParameterBasedOnDbColumn("@OH_PK", orgHeaderPK, OrgHeaderSchema.PK);
				command.AddParameterBasedOnDbColumn("@OH_Code", orgHeaderCode, OrgHeaderSchema.OH_Code);
				command.AddParameterBasedOnDbColumn("@OH_FullName", orgHeaderFullName, OrgHeaderSchema.OH_FullName);
				command.ExecuteNonQuery();
			}
			return orgHeaderPK;
		}

		Guid InsertOrgContact(Guid orgHeaderPK, string contactName)
		{
			if (string.IsNullOrEmpty(contactName))
			{
				return Guid.Empty;
			}

			var orgContactPK = Guid.NewGuid();
			using (var command = TestConnection.Command(@"INSERT INTO dbo.OrgContact (OC_PK, OC_ContactName, OC_IsValid, OC_IsActive, OC_OH) VALUES (@OC_PK, @OC_ContactName, 0, 1, @OC_OH)"))
			{
				command.AddParameterBasedOnDbColumn("@OC_PK", orgContactPK, OrgContactSchema.PK);
				command.AddParameterBasedOnDbColumn("@OC_ContactName", contactName, OrgContactSchema.OC_ContactName);
				command.AddParameterBasedOnDbColumn("@OC_OH", orgHeaderPK, OrgContactSchema.OC_OH);
				command.ExecuteNonQuery();
			}
			return orgContactPK;
		}

		IList<KeyValuePair<Guid, string>> GetEnquiryPKsAndReferringContactFromRunningQuery()
		{
			var result = new List<KeyValuePair<Guid, string>>();

			using (var command = TestConnection.Command("SELECT InquiryPK, ReferringContact FROM Report_SalesMarketingInquiryV2(@RecentActivityType, @RelatedActivityType, NULL)"))
			{
				command.AddParameter("@RecentActivityType", SqlDbType.VarChar, "ANY");
				command.AddParameter("@RelatedActivityType", SqlDbType.VarChar, DBNull.Value);

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						result.Add(new KeyValuePair<Guid, string>((Guid)reader["InquiryPK"], reader["ReferringContact"] == DBNull.Value ? string.Empty : (string)reader["ReferringContact"]));
					}
				}
			}

			return result;
		}

		#endregion

		#region Test ReferTo

		public void TestEnquiriesWithReferTo()
		{
			var enq1 = Guid.NewGuid();
			var enq2 = Guid.NewGuid();
			var enq3 = Guid.NewGuid();
			var enq4 = Guid.NewGuid();

			InsertEnquiryWithReferToContactName("TestOrg1", "Test Organisation 1", "Test Contact Name 1", enq1, "I00000001");
			InsertEnquiryWithReferToContactName("TestOrg2", "Test Organisation 2", "", enq2, "I00000002");
			InsertEnquiryWithReferToContactName("TestOrg3", "Test Organisation 3", "Test Contact Name 2", enq3, "I00000003");
			InsertEnquiryWithReferToContactName("", "", "", enq4, "I00000004");

			using (var command = TestConnection.Command("SELECT InquiryPK, ReferToOrgCode, ReferToOrgName, ReferToContactName FROM Report_SalesMarketingInquiryV2(@RecentActivityType, @RelatedActivityType, NULL)"))
			{
				command.AddParameter("@RecentActivityType", SqlDbType.VarChar, "ANY");
				command.AddParameter("@RelatedActivityType", SqlDbType.VarChar, DBNull.Value);

				var result = DataUtils.GetDataTableFromCommand(command);
				var rows = result.Rows.Cast<DataRow>();
				var enquiryDictionary = rows.ToDictionary(r => (Guid)r["InquiryPK"]);

				var result1 = enquiryDictionary[enq1];
				AssertEquals("TestOrg1", result1["ReferToOrgCode"]);
				AssertEquals("Test Organisation 1", result1["ReferToOrgName"]);
				AssertEquals("Test Contact Name 1", result1["ReferToContactName"]);

				var result2 = enquiryDictionary[enq2];
				AssertEquals("TestOrg2", result2["ReferToOrgCode"]);
				AssertEquals("Test Organisation 2", result2["ReferToOrgName"]);
				AssertEquals(DBNull.Value, result2["ReferToContactName"]);

				var result3 = enquiryDictionary[enq3];
				AssertEquals("TestOrg3", result3["ReferToOrgCode"]);
				AssertEquals("Test Organisation 3", result3["ReferToOrgName"]);
				AssertEquals("Test Contact Name 2", result3["ReferToContactName"]);

				var result4 = enquiryDictionary[enq4];
				AssertEquals(DBNull.Value, result4["ReferToOrgCode"]);
				AssertEquals(DBNull.Value, result4["ReferToOrgName"]);
				AssertEquals(DBNull.Value, result4["ReferToContactName"]);
			}
		}

		void InsertEnquiryWithReferToContactName(string orgHeaderCode, string orgHeaderFullName, string contactName, Guid orgColdCallRegisterPK, string reference)
		{
			var orgHeaderPK = InsertOrg(orgHeaderCode, orgHeaderFullName);
			var orgContactPK = InsertOrgContact(orgHeaderPK, contactName);

			using (var command = TestConnection.Command(@"INSERT INTO dbo.OrgColdCallRegister (O1_PK, O1_LeadUniqueReference, O1_OH_ReferTo, O1_OC_ReferToContact, O1_SystemCreateTimeUtc, O1_SystemCreateUser, O1_SystemLastEditTimeUtc, O1_SystemLastEditUser)
VALUES (@O1_PK, @O1_LeadUniqueReference, @O1_OH_ReferTo, @O1_OC_ReferToContact, GetUtcDate(), 'E', GetUtcDate(), 'E')"))
			{
				command.AddParameterBasedOnDbColumn("@O1_PK", orgColdCallRegisterPK, OrgColdCallRegisterSchema.PK);
				command.AddParameterBasedOnDbColumn("@O1_LeadUniqueReference", reference, OrgColdCallRegisterSchema.O1_LeadUniqueReference);
				command.AddParameterBasedOnDbColumn("@O1_OH_ReferTo", (orgHeaderPK != Guid.Empty) ? orgHeaderPK : DBNull.Value, OrgColdCallRegisterSchema.O1_OH_ReferTo);
				command.AddParameterBasedOnDbColumn("@O1_OC_ReferToContact", (orgContactPK != Guid.Empty) ? orgContactPK : DBNull.Value, OrgColdCallRegisterSchema.O1_OC_ReferToContact);
				command.ExecuteNonQuery();
			}
		}

		#endregion

		#region Test SalesPerson

		public void TestEnquiriesWithSalesPerson()
		{
			var companyPK = TestDataCreator.CreateCompany("TES", "AU", "AUD");
			var enq1 = Guid.NewGuid();
			var enq2 = Guid.NewGuid();
			var enq3 = Guid.NewGuid();
			var enq4 = Guid.NewGuid();

			InsertEnquiryWithSalesPerson("TS1", "Test Staff 1", "BR1", "Test Branch Name 1", companyPK, enq1, "I00000001");
			InsertEnquiryWithSalesPerson("TS2", "Test Staff 2", "", "", companyPK, enq2, "I00000002");
			InsertEnquiryWithSalesPerson("TS3", "Test Staff 3", "BR2", "Test Branch Name 2", companyPK, enq3, "I00000003");
			InsertEnquiryWithSalesPerson("", "", "", "", companyPK, enq4, "I00000004");

			using (var command = TestConnection.Command("SELECT InquiryPK, SalesPerson, SalesPersonName, SalesPersonBranchCode, SalesPersonBranchName FROM Report_SalesMarketingInquiryV2(@RecentActivityType, @RelatedActivityType, NULL)"))
			{
				command.AddParameter("@RecentActivityType", SqlDbType.VarChar, "ANY");
				command.AddParameter("@RelatedActivityType", SqlDbType.VarChar, DBNull.Value);

				var result = DataUtils.GetDataTableFromCommand(command);
				var rows = result.Rows.Cast<DataRow>();
				var enquiryDictionary = rows.ToDictionary(r => (Guid)r["InquiryPK"]);

				var result1 = enquiryDictionary[enq1];
				AssertEquals("TS1", result1["SalesPerson"]);
				AssertEquals("Test Staff 1", result1["SalesPersonName"]);
				AssertEquals("BR1", result1["SalesPersonBranchCode"]);
				AssertEquals("Test Branch Name 1", result1["SalesPersonBranchName"]);

				var result2 = enquiryDictionary[enq2];
				AssertEquals("TS2", result2["SalesPerson"]);
				AssertEquals("Test Staff 2", result2["SalesPersonName"]);
				AssertEquals(DBNull.Value, result2["SalesPersonBranchCode"]);
				AssertEquals(DBNull.Value, result2["SalesPersonBranchName"]);

				var result3 = enquiryDictionary[enq3];
				AssertEquals("TS3", result3["SalesPerson"]);
				AssertEquals("Test Staff 3", result3["SalesPersonName"]);
				AssertEquals("BR2", result3["SalesPersonBranchCode"]);
				AssertEquals("Test Branch Name 2", result3["SalesPersonBranchName"]);

				var result4 = enquiryDictionary[enq4];
				AssertEquals("", result4["SalesPerson"]);
				AssertEquals(DBNull.Value, result4["SalesPersonName"]);
				AssertEquals(DBNull.Value, result4["SalesPersonBranchCode"]);
				AssertEquals(DBNull.Value, result4["SalesPersonBranchName"]);
			}
		}

		void InsertEnquiryWithSalesPerson(string staffCode, string staffName, string branchCode, string branchName, Guid companyPK, Guid orgColdCallRegisterPK, string reference)
		{
			var homeBranchPK = InsertBranch(branchCode, branchName, companyPK);
			InsertStaff(staffCode, staffName, homeBranchPK);

			using (var command = TestConnection.Command(@"INSERT INTO dbo.OrgColdCallRegister (O1_PK, O1_LeadUniqueReference, O1_GS_NKRepAssigned, O1_SystemCreateTimeUtc, O1_SystemCreateUser, O1_SystemLastEditTimeUtc, O1_SystemLastEditUser)
VALUES (@O1_PK, @O1_LeadUniqueReference, @O1_GS_NKRepAssigned, GetUtcDate(), 'E', GetUtcDate(), 'E')"))
			{
				command.AddParameterBasedOnDbColumn("@O1_PK", orgColdCallRegisterPK, OrgColdCallRegisterSchema.PK);
				command.AddParameterBasedOnDbColumn("@O1_LeadUniqueReference", reference, OrgColdCallRegisterSchema.O1_LeadUniqueReference);
				command.AddParameterBasedOnDbColumn("@O1_GS_NKRepAssigned", staffCode, OrgColdCallRegisterSchema.O1_GS_NKRepAssigned);
				command.ExecuteNonQuery();
			}
		}

		void InsertStaff(string staffCode, string staffName, Guid homeBranchPK)
		{
			if (!string.IsNullOrEmpty(staffCode))
			{
				var staffPK = Guid.NewGuid();
				using (var command = TestConnection.Command("INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_FullName, GS_LoginName, GS_GB_HomeBranch, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES (@GS_PK, @GS_Code, @GS_FullName, @GS_LoginName, @GS_GB_HomeBranch, GetUtcDate(), '~BP', GetUtcDate(), '~BP')"))
				{
					command.AddParameterBasedOnDbColumn("@GS_PK", staffPK, GlbStaffSchema.PK);
					command.AddParameterBasedOnDbColumn("@GS_Code", staffCode, GlbStaffSchema.GS_Code);
					command.AddParameterBasedOnDbColumn("@GS_FullName", staffName, GlbStaffSchema.GS_FullName);
					command.AddParameterBasedOnDbColumn("@GS_LoginName", staffCode, GlbStaffSchema.GS_LoginName);
					command.AddParameterBasedOnDbColumn("@GS_GB_HomeBranch", (homeBranchPK != Guid.Empty) ? homeBranchPK : DBNull.Value, GlbStaffSchema.GS_GB_HomeBranch);
					command.ExecuteNonQuery();
				}
			}
		}

		Guid InsertBranch(string branchCode, string branchName, Guid companyPK)
		{
			if (string.IsNullOrEmpty(branchCode))
			{
				return Guid.Empty;
			}

			var branchPK = Guid.NewGuid();
			using (var command = TestConnection.Command("INSERT INTO dbo.GlbBranch (GB_PK, GB_Code, GB_BranchName, GB_GC) VALUES (@GB_PK, @GB_Code, @GB_BranchName, @GB_GC)"))
			{
				command.AddParameterBasedOnDbColumn("@GB_PK", branchPK, GlbBranchSchema.PK);
				command.AddParameterBasedOnDbColumn("@GB_Code", branchCode, GlbBranchSchema.GB_Code);
				command.AddParameterBasedOnDbColumn("@GB_BranchName", branchName, GlbBranchSchema.GB_BranchName);
				command.AddParameterBasedOnDbColumn("@GB_GC", companyPK, GlbBranchSchema.GB_GC);
				command.ExecuteNonQuery();
			}
			return branchPK;
		}

		#endregion

		#region Test ContactName; City and Port

		public void TestEnquiriesWithLinkedContactCityAndPort()
		{
			var enq1 = Guid.NewGuid();
			var enq2 = Guid.NewGuid();
			var enq3 = Guid.NewGuid();

			var orgContactPk1 = Guid.NewGuid();
			var orgContactPk2 = Guid.NewGuid();

			InsertInquiryWithLinkedContact("TestOrg1", "Test Organisation 1", "Test Contact Name 1", enq1, "I00000001", orgContactPk1, "AAA", "Melbourne", "AUBNE");
			InsertInquiryWithLinkedContact("TestOrg2", "Test Organisation 2", "Test Contact Name 2", enq2, "I00000002", orgContactPk2, "BBB", "Sydney", "AUSYD");
			InsertInquiryWithLinkedContact("", "", "", enq3, "I00000003", Guid.Empty, "", "", "JEJER");

			var actualList = GetInquiryPKsAndLinkedContactPlusCityAndPortFromRunningQuery(null, Guid.Empty);

			var expectedItem1 = new KeyValuePair<Guid, Tuple<string, string, string, string>>(enq1, Tuple.Create("Test Contact Name 1", "Melbourne", "AUBNE", "NSW"));
			var expectedItem2 = new KeyValuePair<Guid, Tuple<string, string, string, string>>(enq2, Tuple.Create("Test Contact Name 2", "Sydney", "AUSYD", "NSW"));
			var expectedItem3 = new KeyValuePair<Guid, Tuple<string, string, string, string>>(enq3, Tuple.Create("Inquiry Contact 1", "", "JEJER", ""));

			Assert(actualList.Contains(expectedItem1));
			Assert(actualList.Contains(expectedItem2));
			Assert(actualList.Contains(expectedItem3));

			var relatedActivityId = Guid.NewGuid();
			CreateRelatedActivity(enq1, relatedActivityId);
			actualList = GetInquiryPKsAndLinkedContactPlusCityAndPortFromRunningQuery("CAM", relatedActivityId);
			Assert(actualList.Contains(expectedItem1));
			AssertEquals(1, actualList.Count);
		}

		void InsertInquiryWithLinkedContact(string orgHeaderCode, string orgHeaderFullName, string contactName, Guid orgColdCallRegisterPK, string reference, Guid orgContactPK, string addressCode, string city, string port)
		{
			var orgHeaderPK = Guid.NewGuid();
			var orgAddressPK = Guid.NewGuid();

			if (!string.IsNullOrEmpty(contactName))
			{
				using (var command = TestConnection.Command(@"INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName) VALUES (@OH_PK, @OH_Code, @OH_FullName)"))
				{
					command.AddParameterBasedOnDbColumn("@OH_PK", orgHeaderPK, OrgHeaderSchema.PK);
					command.AddParameterBasedOnDbColumn("@OH_Code", orgHeaderCode, OrgHeaderSchema.OH_Code);
					command.AddParameterBasedOnDbColumn("@OH_FullName", orgHeaderFullName, OrgHeaderSchema.OH_FullName);
					command.ExecuteNonQuery();
				}

				using (var command = TestConnection.Command(@"INSERT INTO dbo.OrgContact (OC_PK, OC_ContactName, OC_IsValid, OC_IsActive, OC_OH) VALUES (@OC_PK, @OC_ContactName, 0, 1, @OC_OH)"))
				{
					command.AddParameterBasedOnDbColumn("@OC_PK", orgContactPK, OrgContactSchema.PK);
					command.AddParameterBasedOnDbColumn("@OC_ContactName", contactName, OrgContactSchema.OC_ContactName);
					command.AddParameterBasedOnDbColumn("@OC_OH", orgHeaderPK, OrgContactSchema.OC_OH);
					command.ExecuteNonQuery();
				}

				using (var command = TestConnection.Command(@"INSERT INTO dbo.OrgAddress (OA_PK, OA_Code, OA_City, OA_RL_NKRelatedPortCode, OA_OH, OA_Address1, OA_Address2, OA_State, OA_PostCode) VALUES (@OA_PK, @OA_Code, @OA_City, @OA_RL_NKRelatedPortCode, @OA_OH, @OA_Address1, @OA_Address2, @OA_State, @OA_PostCode)"))
				{
					command.AddParameterBasedOnDbColumn("@OA_PK", orgAddressPK, OrgAddressSchema.PK);
					command.AddParameterBasedOnDbColumn("@OA_Code", addressCode, OrgAddressSchema.OA_Code);
					command.AddParameterBasedOnDbColumn("@OA_City", city, OrgAddressSchema.OA_City);
					command.AddParameterBasedOnDbColumn("@OA_RL_NKRelatedPortCode", port, OrgAddressSchema.OA_RL_NKRelatedPortCode);
					command.AddParameterBasedOnDbColumn("@OA_OH", orgHeaderPK, OrgAddressSchema.OA_OH);
					command.AddParameterBasedOnDbColumn("@OA_Address1", "Grange Street", OrgAddressSchema.OA_Address1);
					command.AddParameterBasedOnDbColumn("@OA_Address2", "Alexandria", OrgAddressSchema.OA_Address2);
					command.AddParameterBasedOnDbColumn("@OA_State", "NSW", OrgAddressSchema.OA_State);
					command.AddParameterBasedOnDbColumn("@OA_PostCode", "2015", OrgAddressSchema.OA_PostCode);
					command.ExecuteNonQuery();
				}
			}

			using (var command = TestConnection.Command(@"INSERT INTO dbo.OrgColdCallRegister (O1_PK, O1_LeadUniqueReference, O1_ContactName, O1_OC_LinkedContact, O1_OA_LinkedAddress, O1_PortOrCountry, O1_SystemCreateTimeUtc, O1_SystemCreateUser, O1_SystemLastEditTimeUtc, O1_SystemLastEditUser)
VALUES (@O1_PK, @O1_LeadUniqueReference, @O1_ContactName, @O1_OC_LinkedContact, @O1_OA_LinkedAddress, @O1_PortOrCountry, GetUtcDate(), 'E', GetUtcDate(), 'E')"))
			{
				command.AddParameterBasedOnDbColumn("@O1_PK", orgColdCallRegisterPK, OrgColdCallRegisterSchema.PK);
				command.AddParameterBasedOnDbColumn("@O1_LeadUniqueReference", reference, OrgColdCallRegisterSchema.O1_LeadUniqueReference);
				command.AddParameterBasedOnDbColumn("@O1_ContactName", (string.IsNullOrEmpty(contactName) ? "Inquiry Contact 1" : contactName), OrgColdCallRegisterSchema.O1_ContactName);
				command.AddParameterBasedOnDbColumn("@O1_OC_LinkedContact", (orgContactPK != Guid.Empty) ? orgContactPK : DBNull.Value, OrgColdCallRegisterSchema.O1_OC_LinkedContact);
				command.AddParameterBasedOnDbColumn("@O1_OA_LinkedAddress", (!string.IsNullOrEmpty(contactName)) ? orgAddressPK : DBNull.Value, OrgColdCallRegisterSchema.O1_OA_LinkedAddress);
				command.AddParameterBasedOnDbColumn("@O1_PortOrCountry", (!string.IsNullOrEmpty(contactName)) ? "" : port, OrgColdCallRegisterSchema.O1_PortOrCountry);
				command.ExecuteNonQuery();
			}
		}

		void CreateRelatedActivity(Guid inqPk, Guid relatedActivityId)
		{
			var sql =
@"
INSERT INTO dbo.RelatedActivityPivot
(RAP_PK, RAP_ParentActivityTableCode, RAP_ParentActivityID, RAP_ChildActivityTableCode, RAP_ChildActivityID, RAP_SalesRelationTreeID, RAP_SystemCreateTimeUtc, RAP_SystemCreateUser, RAP_SystemLastEditTimeUtc, RAP_SystemLastEditUser)
VALUES
(newid(), 'O1', @RAP_ParentActivityId, 'G0', @RAP_ChildActivityID, @RAP_SalesRelationTreeID, GetUtcDate(), 'E', GetUtcDate(), 'E')
";
			using (var command = TestConnection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("@RAP_ParentActivityId", inqPk, RelatedActivityPivotSchema.RAP_ParentActivityID);
				command.AddParameterBasedOnDbColumn("@RAP_ChildActivityID", relatedActivityId, RelatedActivityPivotSchema.RAP_ChildActivityID);
				command.AddParameterBasedOnDbColumn("@RAP_SalesRelationTreeID", relatedActivityId, RelatedActivityPivotSchema.RAP_SalesRelationTreeID);
				command.ExecuteNonQuery();
			}
		}

		IList<KeyValuePair<Guid, Tuple<string, string, string, string>>> GetInquiryPKsAndLinkedContactPlusCityAndPortFromRunningQuery(string salesRelation, Guid relatedActivityId)
		{
			var result = new List<KeyValuePair<Guid, Tuple<string, string, string, string>>>();

			using (var command = TestConnection.Command("SELECT InquiryPK, ContactName, City, PortOrCountry, LeadState FROM Report_SalesMarketingInquiryV2(@RecentActivityType, @RelatedActivityType, @RelatedActivityID)"))
			{
				command.AddParameter("@RecentActivityType", SqlDbType.VarChar, "ANY");
				command.AddParameter("@RelatedActivityType", SqlDbType.VarChar, salesRelation ?? (object)DBNull.Value);
				command.AddParameter("@RelatedActivityID", SqlDbType.UniqueIdentifier, relatedActivityId);

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						Tuple<string, string, string, string> value = Tuple.Create(reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4));
						result.Add(new KeyValuePair<Guid, Tuple<string, string, string, string>>(reader.GetGuid(0), value));
					}
				}
			}

			return result;
		}
		#endregion
	}
}

