using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.HRM;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.HRM
{
	[TestedType(typeof(HROnBoarding_WithAccess))]
	class HROnBoarding_WithAccessTest : DbCreateScriptTest
	{
		public void TestExecute()
		{
			SetUpData();

			var results = Execute(staff0);
			var dataRows = results.Select();

			DataTable originalDataTable;
			using (var command = TestConnection.Command("SELECT * FROM [dbo].[HROnBoarding]"))
			{
				originalDataTable = DataUtils.GetDataTableFromCommand(command);
			}

			var adHocColumns = new[] { "IsSelf", "HasAssignedTasks" };

			AssertEquals(originalDataTable.Columns.Count + adHocColumns.Length, results.Columns.Count);
			foreach (var column in originalDataTable.Columns)
			{
				Assert(results.Columns.Contains(((DataColumn)column).ColumnName));
			}
			foreach (var colName in adHocColumns)
			{
				Assert(results.Columns.Contains(colName));
			}

			AssertEquals(originalDataTable.Rows.Count, dataRows.Length);

			foreach (DataRow originalRow in originalDataTable.Rows)
			{
				var pk = originalRow["HOB_PK"];

				var dataRow = dataRows.FirstOrDefault(r => r["HOB_PK"].Equals(pk));
				foreach (var column in originalDataTable.Columns)
				{
					var col = column as DataColumn;
					AssertEquals(col.ColumnName, originalRow[col.ColumnName], dataRow[col.ColumnName]);
				}

				var expectedIsSelf = (Guid)dataRow["HOB_PK"] == onBoarding0;
				var expectedHasAssignedTasks = (Guid)dataRow["HOB_PK"] == onBoarding2 || (Guid)dataRow["HOB_PK"] == onBoarding3;

				AssertEquals(expectedIsSelf, dataRow["IsSelf"]);
				AssertEquals(expectedHasAssignedTasks, dataRow["HasAssignedTasks"]);
			}
		}

		readonly Guid person0 = Guid.NewGuid();
		readonly Guid person1 = Guid.NewGuid();
		readonly Guid person2 = Guid.NewGuid();
		readonly Guid person3 = Guid.NewGuid();

		readonly Guid staff0 = Guid.NewGuid();
		readonly Guid staff1 = Guid.NewGuid();
		readonly Guid staff2 = Guid.NewGuid();

		readonly Guid applicant0 = Guid.NewGuid();
		readonly Guid applicant1 = Guid.NewGuid();
		readonly Guid applicant2 = Guid.NewGuid();
		readonly Guid applicant3 = Guid.NewGuid();

		readonly Guid onBoarding0 = Guid.NewGuid();
		readonly Guid onBoarding1 = Guid.NewGuid();
		readonly Guid onBoarding2 = Guid.NewGuid();
		readonly Guid onBoarding3 = Guid.NewGuid();

		readonly Guid capbilityPK = Guid.NewGuid();

		void SetUpData()
		{
			var companyPK = TestDataCreator.CreateCompany("ZZZ", "UA", "UAH");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "ZZZ", "UAIEV");
			var departmnetPK = TestDataCreator.CreateDepartment("DPT");

			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.GlbPerson(PER_PK, PER_IsActive, PER_FullName, PER_SystemCreateTimeUtc, PER_SystemCreateUser, PER_SystemLastEditTimeUtc, PER_SystemLastEditUser, PER_ValidationStatus)
VALUES
	(@person0, 1, 'PERSON0', GETUTCDATE(), 'E', GETUTCDATE(), 'E', 'NYV'),
	(@person1, 1, 'PERSON1', GETUTCDATE(), 'E', GETUTCDATE(), 'E', 'NYV'),
	(@person2, 1, 'PERSON2', GETUTCDATE(), 'E', GETUTCDATE(), 'E', 'NYV'),
	(@person3, 1, 'PERSON3', GETUTCDATE(), 'E', GETUTCDATE(), 'E', 'NYV');
	
INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_PER, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) 
VALUES
	(@staff0, 'GS0', 'Staff0', @person0, GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(@staff1, 'GS1', 'Staff1', NULL, GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(@staff2, 'GS2', 'Staff2', @person1, GETUTCDATE(), 'E', GETUTCDATE(), 'E');

INSERT INTO dbo.HRJobApplicant(HA_PK, HA_EmailAddress, HA_PER, HA_SystemCreateTimeUtc, HA_SystemCreateUser, HA_SystemLastEditTimeUtc, HA_SystemLastEditUser)
VALUES
	(@applicant0, 'app0@blah.com', @person0, GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(@applicant1, 'app1@blah.com', @person1, GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(@applicant2, 'app2@blah.com', @person2, GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(@applicant3, 'app3@blah.com', @person3, GETUTCDATE(), 'E', GETUTCDATE(), 'E');

INSERT INTO dbo.HROnBoarding(HOB_PK, HOB_HA_JobApplicant, HOB_JobTitle, HOB_WorkingBasis, HOB_GB_HomeBranch, HOB_GE_HomeDepartment, HOB_StartDate, HOB_ProbationEndDate, HOB_DepartureDate, HOB_SystemCreateTimeUtc, HOB_SystemCreateUser, HOB_SystemLastEditTimeUtc, HOB_SystemLastEditUser)
VALUES
	(@onBoarding0, @applicant0, 'leader', 'FUL', @branch, @deprtment, GETUTCDATE(), DATEADD(YEAR, 1, GETUTCDATE()), DATEADD(YEAR, 2, GETUTCDATE()), GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(@onBoarding1, @applicant1, 'leader', 'FUL', @branch, @deprtment, GETUTCDATE(), DATEADD(YEAR, 1, GETUTCDATE()), NULL, GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(@onBoarding2, @applicant2, 'leader', 'FUL', @branch, @deprtment, GETUTCDATE(), NULL, DATEADD(YEAR, 2, GETUTCDATE()), GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(@onBoarding3, @applicant3, 'leader', 'FUL', @branch, @deprtment, GETUTCDATE(), DATEADD(YEAR, 1, GETUTCDATE()), DATEADD(YEAR, 2, GETUTCDATE()), GETUTCDATE(), 'E', GETUTCDATE(), 'E');

INSERT INTO dbo.GlbCapability(G4_PK, G4_Code, G4_Description, G4_SystemCreateTimeUtc, G4_SystemCreateUser, G4_SystemLastEditTimeUtc, G4_SystemLastEditUser)
VALUES
	(@capbilityPK, 'XXX', 'test cap', GETUTCDATE(), 'E', GETUTCDATE(), 'E');

INSERT INTO dbo.GlbResourceCapabilityPivot(G5_PK, G5_G4_Capability, G5_GS_Resource, G5_DateExperienceGained, G5_SystemCreateTimeUtc, G5_SystemCreateUser, G5_SystemLastEditTimeUtc, G5_SystemLastEditUser)
VALUES
	(NEWID(), @capbilityPK, @staff0, GETUTCDATE(), GETUTCDATE(), 'E', GETUTCDATE(), 'E');

INSERT INTO dbo.ProcessTasks(P9_PK, P9_ParentID, P9_ParentTableCode, P9_Type, P9_Description, P9_GS_NKAssignedStaffMember, P9_G4_RequiredCapability)
VALUES
	(NEWID(), @onBoarding0, 'HOB', 'APP', 'task 1', 'GS1', NULL),
	(NEWID(), @onBoarding1, 'HOB', 'APP', 'task 2', 'GS3', NULL),
	(NEWID(), @onBoarding1, 'HOB', 'APP', 'task 3', 'GS4', NULL),
	(NEWID(), @onBoarding2, 'HOB', 'APP', 'task 4', 'GS4', NULL),
	(NEWID(), @onBoarding2, 'HOB', 'APP', 'task 5', 'GS0', NULL),
	(NEWID(), @onBoarding3, 'HOB', 'APP', 'task 1', '', @capbilityPK);

			", p =>
			{
				p.AddParameter("@branch", SqlDbType.UniqueIdentifier, branchPK);
				p.AddParameter("@deprtment", SqlDbType.UniqueIdentifier, departmnetPK);

				p.AddParameter("@person0", SqlDbType.UniqueIdentifier, person0);
				p.AddParameter("@person1", SqlDbType.UniqueIdentifier, person1);
				p.AddParameter("@person2", SqlDbType.UniqueIdentifier, person2);
				p.AddParameter("@person3", SqlDbType.UniqueIdentifier, person3);

				p.AddParameter("@staff0", SqlDbType.UniqueIdentifier, staff0);
				p.AddParameter("@staff1", SqlDbType.UniqueIdentifier, staff1);
				p.AddParameter("@staff2", SqlDbType.UniqueIdentifier, staff2);

				p.AddParameter("@applicant0", SqlDbType.UniqueIdentifier, applicant0);
				p.AddParameter("@applicant1", SqlDbType.UniqueIdentifier, applicant1);
				p.AddParameter("@applicant2", SqlDbType.UniqueIdentifier, applicant2);
				p.AddParameter("@applicant3", SqlDbType.UniqueIdentifier, applicant3);

				p.AddParameter("@onBoarding0", SqlDbType.UniqueIdentifier, onBoarding0);
				p.AddParameter("@onBoarding1", SqlDbType.UniqueIdentifier, onBoarding1);
				p.AddParameter("@onBoarding2", SqlDbType.UniqueIdentifier, onBoarding2);
				p.AddParameter("@onBoarding3", SqlDbType.UniqueIdentifier, onBoarding3);

				p.AddParameter("@capbilityPK", SqlDbType.UniqueIdentifier, capbilityPK);
			});
		}

		DataTable Execute(Guid loggedInStaff)
		{
			using (var command = TestConnection.Command($"SELECT * FROM HROnBoarding_WithAccess(@loggedInStaff)"))
			{
				command.AddParameter("@loggedInStaff", SqlDbType.UniqueIdentifier, loggedInStaff);

				return DataUtils.GetDataTableFromCommand(command);
			}
		}
	}
}
