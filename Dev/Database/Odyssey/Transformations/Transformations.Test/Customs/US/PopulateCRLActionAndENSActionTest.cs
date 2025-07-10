using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.US;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

#pragma warning disable IDE0059       // Unnecessary assignment of a value                                         
#pragma warning disable SA1312        // Variable names should begin with lower-case letter                                                           
#pragma warning disable SA1313        // Parameter names should begin with lower-case letter                                                            

namespace Enterprise.DbUpgrader.Transformations.Test.Customs.US
{
	[TestedType(typeof(PopulateCRLActionAndENSAction))]
	public sealed class PopulateCRLActionAndENSActionTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new PopulateCRLActionAndENSAction(batchSize: 2);

		#region Prepare Data

		protected override void PrepareTestData()
		{
			Guid JE_PK, CH_PK, EM_PK;

			/* Import (ENSAction) */

			clusterKey = 1;
			JE_PK = CreateJobDeclaration("IMP", JE_AddInfoToTest);
			CH_PK = CreateCusEntryHeader(JE_PK, "ENS");

			clusterKey = 2;
			JE_PK = CreateJobDeclaration("IMP", JE_AddInfoToTest);
			CH_PK = CreateCusEntryHeader(JE_PK, "ENS");
			EM_PK = CreateEDIMessage(CH_PK, "UC", "USI", "");

			clusterKey = 3;
			JE_PK = CreateJobDeclaration("IMP", JE_AddInfoToTest);
			CH_PK = CreateCusEntryHeader(JE_PK, "ENS");
			EM_PK = CreateEDIMessage(CH_PK, "UC", "USI", "Q:");

			clusterKey = 4;
			JE_PK = CreateJobDeclaration("IMP", JE_AddInfoToTest, DateTime.Now.AddYears(-5));
			CH_PK = CreateCusEntryHeader(JE_PK, "ENS");
			EM_PK = CreateEDIMessage(CH_PK, "UC", "USI", "Q:");
			EM_PK = CreateEDIMessage(CH_PK, "UC", "USI", "4:ACTION REQUIRED");
			CreateStmALog("EDIMessage", EM_PK, "ATH", "User audited");
			EM_PK = CreateEDIMessage(CH_PK, "UC", "USI", "4:ACTION REQUIRED");

			clusterKey = 5;
			JE_PK = CreateJobDeclaration("IMP", JE_AddInfoToTest, DateTime.Now.AddMonths(-1));
			CH_PK = CreateCusEntryHeader(JE_PK, "ENS");
			EM_PK = CreateEDIMessage(CH_PK, "UC", "USI", "Q:");
			EM_PK = CreateEDIMessage(CH_PK, "UC", "USI", "4:ACTION REQUIRED");
			CreateStmALog("EDIMessage", EM_PK, "ATH", "User audited");
			EM_PK = CreateEDIMessage(CH_PK, "UC", "USI", "4:ACTION REQUIRED");
			CreateStmALog("EDIMessage", EM_PK, "ATH", "User audited");

			/* Recon (ENSAction) */

			clusterKey = 6;
			JE_PK = CreateJobDeclaration("REC", JE_AddInfoToTest, "USR");
			CH_PK = CreateCusEntryHeader(JE_PK, "REC");

			clusterKey = 7;
			JE_PK = CreateJobDeclaration("REC", JE_AddInfoToTest, "USR");
			CH_PK = CreateCusEntryHeader(JE_PK, "REC");
			EM_PK = CreateEDIMessage(CH_PK, "UC", "USI", "");

			clusterKey = 8;
			JE_PK = CreateJobDeclaration("REC", JE_AddInfoToTest, "USR");
			CH_PK = CreateCusEntryHeader(JE_PK, "REC");
			EM_PK = CreateEDIMessage(CH_PK, "UC", "USI", "Q:");

			clusterKey = 9;
			JE_PK = CreateJobDeclaration("REC", JE_AddInfoToTest, DateTime.Now.AddYears(-5), "USR");
			CH_PK = CreateCusEntryHeader(JE_PK, "REC");
			EM_PK = CreateEDIMessage(CH_PK, "UC", "USI", "Q:");
			EM_PK = CreateEDIMessage(CH_PK, "UC", "USI", "4:ACTION REQUIRED");
			CreateStmALog("EDIMessage", EM_PK, "ATH", "User audited");
			EM_PK = CreateEDIMessage(CH_PK, "UC", "USI", "4:ACTION REQUIRED");

			clusterKey = 10;
			JE_PK = CreateJobDeclaration("REC", JE_AddInfoToTest, DateTime.Now.AddMonths(-1), "USR");
			CH_PK = CreateCusEntryHeader(JE_PK, "REC");
			EM_PK = CreateEDIMessage(CH_PK, "UC", "USI", "Q:");
			EM_PK = CreateEDIMessage(CH_PK, "UC", "USI", "4:ACTION REQUIRED");
			CreateStmALog("EDIMessage", EM_PK, "ATH", "User audited");
			EM_PK = CreateEDIMessage(CH_PK, "UC", "USI", "4:ACTION REQUIRED");
			CreateStmALog("EDIMessage", EM_PK, "ATH", "User audited");

			/* Drawback (ENSAction) */

			clusterKey = 11;
			JE_PK = CreateJobDeclaration("DRW", JE_AddInfoToTest);

			clusterKey = 12;
			JE_PK = CreateJobDeclaration("DRW", JE_AddInfoToTest);
			EM_PK = CreateEDIMessage(JE_PK, "UC", "USI", "", EM_LinkTable: "JobDeclaration");

			clusterKey = 13;
			JE_PK = CreateJobDeclaration("DRW", JE_AddInfoToTest);
			EM_PK = CreateEDIMessage(JE_PK, "UC", "USI", "", EM_LinkTable: "JobDeclaration");

			clusterKey = 14;
			JE_PK = CreateJobDeclaration("DRW", JE_AddInfoToTest, DateTime.Now.AddYears(-5));
			EM_PK = CreateEDIMessage(JE_PK, "UC", "USI", "", EM_LinkTable: "JobDeclaration");
			EM_PK = CreateEDIMessage(JE_PK, "UC", "USI", "4:00001", EM_LinkTable: "JobDeclaration");
			CreateStmALog("EDIMessage", EM_PK, "ATH", "User audited");
			EM_PK = CreateEDIMessage(JE_PK, "UC", "USI", "4:00002", EM_LinkTable: "JobDeclaration");

			clusterKey = 15;
			JE_PK = CreateJobDeclaration("DRW", JE_AddInfoToTest, DateTime.Now.AddMonths(-1));
			EM_PK = CreateEDIMessage(JE_PK, "UC", "USI", "", EM_LinkTable: "JobDeclaration");
			EM_PK = CreateEDIMessage(JE_PK, "UC", "USI", "4:00001", EM_LinkTable: "JobDeclaration");
			CreateStmALog("EDIMessage", EM_PK, "ATH", "User audited");
			EM_PK = CreateEDIMessage(JE_PK, "UC", "USI", "4:00002", EM_LinkTable: "JobDeclaration");
			CreateStmALog("EDIMessage", EM_PK, "ATH", "User audited");

			/* Import (CRLAction) */

			clusterKey = 16;
			JE_PK = CreateJobDeclaration("IMP", JE_AddInfoToTest);
			CH_PK = CreateCusEntryHeader(JE_PK, "SE");

			clusterKey = 17;
			JE_PK = CreateJobDeclaration("IMP", JE_AddInfoToTest);
			CH_PK = CreateCusEntryHeader(JE_PK, "SE");
			EM_PK = CreateEDIMessage(CH_PK, "SO", "USI", "");

			clusterKey = 18;
			JE_PK = CreateJobDeclaration("IMP", JE_AddInfoToTest);
			CH_PK = CreateCusEntryHeader(JE_PK, "SE");
			EM_PK = CreateEDIMessage(CH_PK, "SO", "USI", "Q:");

			clusterKey = 19;
			JE_PK = CreateJobDeclaration("IMP", JE_AddInfoToTest, DateTime.Now.AddYears(-5));
			CH_PK = CreateCusEntryHeader(JE_PK, "SE");
			EM_PK = CreateEDIMessage(CH_PK, "SO", "USI", "Q:");
			EM_PK = CreateEDIMessage(CH_PK, "SO", "USI", "CMT");
			CreateStmALog("EDIMessage", EM_PK, "ATH", "User audited");
			EM_PK = CreateEDIMessage(CH_PK, "SO", "USI", "CMT");

			clusterKey = 20;
			JE_PK = CreateJobDeclaration("IMP", JE_AddInfoToTest, DateTime.Now.AddMonths(-1));
			CH_PK = CreateCusEntryHeader(JE_PK, "SE");
			EM_PK = CreateEDIMessage(CH_PK, "SO", "USI", "Q:");
			EM_PK = CreateEDIMessage(CH_PK, "SO", "USI", "CMT");
			CreateStmALog("EDIMessage", EM_PK, "ATH", "User audited");
			EM_PK = CreateEDIMessage(CH_PK, "SO", "USI", "CMT");
			CreateStmALog("EDIMessage", EM_PK, "ATH", "User audited");

			/* Import (ENSAction, CRLAction) */

			clusterKey = 21;
			JE_PK = CreateJobDeclaration("IMP", JE_AddInfoToTest);
			CH_PK = CreateCusEntryHeader(JE_PK, "ENS");
			EM_PK = CreateEDIMessage(CH_PK, "UC", "USI", "4:ACTION REQUIRED");
			CreateStmALog("EDIMessage", EM_PK, "ATH", "User audited");
			CH_PK = CreateCusEntryHeader(JE_PK, "SE");
			EM_PK = CreateEDIMessage(CH_PK, "SO", "USI", "CMT");
			CreateStmALog("EDIMessage", EM_PK, "ATH", "User audited");
		}

		Guid CreateJobDeclaration(string JE_MessageType, string JE_AddInfo, string JE_DataModel = "US")
			=> CreateJobDeclaration(JE_MessageType, JE_AddInfo, DateTime.Now, JE_DataModel);

		Guid CreateJobDeclaration(string JE_MessageType, string JE_AddInfo, DateTime JE_SystemCreateTimeUtc, string JE_DataModel = "US")
		{
			const string sql =
				@"INSERT INTO dbo.JobDeclaration (JE_PK, JE_ApplicationCode, JE_DeclarationReference, JE_GB, JE_GC, JE_AddInfo, JE_MessageType, JE_ClusterKey, JE_DataModel, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser)
				VALUES (@declarationPK, @applicationCode, @declarationNumber, @branchPK, @companyPK, @addInfo, @messageType, @clusterKey, @dataModel, @systemCreateTimeUtc, '~BP', GetUtcDate(), '~BP')";

			var JE_PK = Guid.NewGuid();

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("@declarationPK", JE_PK, JobDeclarationSchema.PK);
				command.AddParameterBasedOnDbColumn("@applicationCode", "ACE", JobDeclarationSchema.JE_ApplicationCode);
				command.AddParameterBasedOnDbColumn("@declarationNumber", $"B{clusterKey:00000000}", JobDeclarationSchema.JE_DeclarationReference);
				command.AddParameterBasedOnDbColumn("@branchPK", GB_PK, JobDeclarationSchema.JE_GB);
				command.AddParameterBasedOnDbColumn("@companyPK", GC_PK, JobDeclarationSchema.JE_GC);
				command.AddParameterBasedOnDbColumn("@addInfo", JE_AddInfo, JobDeclarationSchema.JE_AddInfo);
				command.AddParameterBasedOnDbColumn("@messageType", JE_MessageType, JobDeclarationSchema.JE_MessageType);
				command.AddParameterBasedOnDbColumn("@clusterKey", clusterKey, JobDeclarationSchema.JE_ClusterKey);
				command.AddParameterBasedOnDbColumn("@dataModel", JE_DataModel, JobDeclarationSchema.JE_DataModel);
				command.AddParameterBasedOnDbColumn("@systemCreateTimeUtc", JE_SystemCreateTimeUtc.ToUniversalTime(), JobDeclarationSchema.JE_SystemCreateTimeUtc);
				command.ExecuteNonQuery();
			}

			return JE_PK;
		}

		Guid CreateCusEntryHeader(Guid JE_PK, string CH_MessageType)
			=> data.CreateCusEntryHeader("US", JE_PK, clusterKey, CH_MessageType);

		Guid CreateEDIMessage(Guid EM_LinkUniqueID, string EM_MessageType, string EM_ApplicationCode, string EM_ApplicationReference, string EM_LinkTable = "CusEntryHeader")
			=> data.CreateEDIMessage(GB_PK, GE_PK, "RCV", EM_ApplicationCode, "RCV", EM_MessageType, "", EM_LinkUniqueID, EM_LinkTable, "", DateTime.UtcNow.AddMinutes(-1), applicationReference: EM_ApplicationReference);

		Guid CreateStmALog(string SL_Table, Guid SL_Parent, string SL_SE_NKEvent, string SL_Reference)
			=> data.CreateStmALog(SL_Table, SL_Parent, "", SL_SE_NKEvent, SL_Reference, DateTime.Now, DateTime.UtcNow, cancelled: false);

		#endregion

		/// <summary>
		/// Test to make sure offline post-upgrade is taking care of declarations changed since online pre-upgrade
		/// </summary>
		public void TestOfflinePostUpgradeTransform()
		{
			Guid JE_PK, CH_PK, EM_PK;
			var transformation = new PopulateCRLActionAndENSAction(batchSize: 3);

			clusterKey = 1001;
			JE_PK = CreateJobDeclaration("IMP", JE_AddInfoToTest, DateTime.Now.AddMonths(-1));
			CH_PK = CreateCusEntryHeader(JE_PK, "ENS");
			EM_PK = CreateEDIMessage(CH_PK, "UC", "USI", "Q:");
			EM_PK = CreateEDIMessage(CH_PK, "UC", "USI", "4:ACTION REQUIRED");
			CreateStmALog("EDIMessage", EM_PK, "ATH", "User audited");
			EM_PK = CreateEDIMessage(CH_PK, "UC", "USI", "4:ACTION REQUIRED");
			CreateStmALog("EDIMessage", EM_PK, "ATH", "User audited");

			transformation.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			clusterKey = 1002;
			JE_PK = CreateJobDeclaration("IMP", JE_AddInfoToTest);
			CH_PK = CreateCusEntryHeader(JE_PK, "ENS");
			EM_PK = CreateEDIMessage(CH_PK, "UC", "USI", "Q:");
			EM_PK = CreateEDIMessage(CH_PK, "UC", "USI", "4:ACTION REQUIRED");
			CreateStmALog("EDIMessage", EM_PK, "ATH", "User audited");
			EM_PK = CreateEDIMessage(CH_PK, "UC", "USI", "4:ACTION REQUIRED");
			CreateStmALog("EDIMessage", EM_PK, "ATH", "User audited");

			AssertAddInfo(1001, "ENSAction", "Complete");
			AssertAddInfo(1002, "ENSAction", "");

			transformation.Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);

			AssertAddInfo(1001, "ENSAction", "Complete");
			AssertAddInfo(1002, "ENSAction", "Complete");
		}

		public void TestOnlinePostUpgradeVerbosity()
		{
			PrepareTestData();
			var expected = new List<string>
			{
				"Processing batch with cluster key ranging from 16 to 19..."
				, "\t0 records processed."
				, "Processing batch with cluster key ranging from 13 to 16..."
				, "\t3 records processed."
				, "Processing batch with cluster key ranging from 10 to 13..."
				, "\t2 records processed."
				, "Processing batch with cluster key ranging from 7 to 10..."
				, "\t3 records processed."
				, "Processing batch with cluster key ranging from 4 to 7..."
				, "\t2 records processed."
				, "Processing batch with cluster key ranging from 1 to 4..."
				, "\t2 records processed."
				, "\tCompleted: Populate JE_AddInfo with US_CRLAction and US_ENSAction"
			};
			var actual = new List<string>();
			var transformation = new PopulateCRLActionAndENSAction(batchSize: 3);
			transformation.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			((IOnlineTransformation)transformation).Run(actual.Add, CancellationToken.None);
			AssertContainsExactElementsInExactOrder(expected, actual);
		}

		#region Asserts

		protected override void AssertTransformationResults()
		{
			/* Import (ENSAction) */
			AssertAddInfo(1, "ENSAction", "");
			AssertAddInfo(2, "ENSAction", "Complete");
			AssertAddInfo(3, "ENSAction", "Complete");
			AssertAddInfo(4, "ENSAction", "Incomplete");
			AssertAddInfo(5, "ENSAction", "Complete");

			/* Recon (ENSAction) */
			AssertAddInfo(6, "ENSAction", "");
			AssertAddInfo(7, "ENSAction", "Complete");
			AssertAddInfo(8, "ENSAction", "Complete");
			AssertAddInfo(9, "ENSAction", "Incomplete");
			AssertAddInfo(10, "ENSAction", "Complete");

			/* Drawback (ENSAction) */
			AssertAddInfo(11, "ENSAction", "");
			AssertAddInfo(12, "ENSAction", "Complete");
			AssertAddInfo(13, "ENSAction", "Complete");
			AssertAddInfo(14, "ENSAction", "Incomplete");
			AssertAddInfo(15, "ENSAction", "Complete");

			/* Import (CRLAction) */
			AssertAddInfo(16, "CRLAction", "");
			AssertAddInfo(17, "CRLAction", "");
			AssertAddInfo(18, "CRLAction", "");
			AssertAddInfo(19, "CRLAction", "Incomplete");
			AssertAddInfo(20, "CRLAction", "Complete");

			/* Import (ENSAction, CRLAction) */
			AssertAddInfoContains(21, "ENSAction", "Complete");
			AssertAddInfoContains(21, "CRLAction", "Complete");

			AssertNull(PopulateCRLActionAndENSAction.ClusterKeyWatermarkOnlinePreUpgrade.Select());
			AssertNull(PopulateCRLActionAndENSAction.ClusterKeyWatermarkOnlinePostUpgrade.Select());
			AssertNull(PopulateCRLActionAndENSAction.DateTimeWatermarkOfflinePostUpgrade.Select());
		}

		void AssertAddInfo(int JE_ClusterKey, string key, string value)
		{
			var actualValue = Db.Connection.ExecuteScalar<string>($"SELECT JE_AddInfo FROM dbo.JobDeclaration WHERE JE_ClusterKey = {JE_ClusterKey};");
			var expectedValue = JE_AddInfoToTest;
			if (!string.IsNullOrWhiteSpace(value))
			{
				expectedValue += $"*{key}={value}";
			}
			AssertEquals($"Assert JE_AddInfo has '*{key}={value}' (clusterKey = {JE_ClusterKey})", expectedValue, actualValue);
		}

		void AssertAddInfoContains(int JE_ClusterKey, string key, string value)
		{
			var addInfo = Db.Connection.ExecuteScalar<string>($"SELECT JE_AddInfo FROM dbo.JobDeclaration WHERE JE_ClusterKey = {JE_ClusterKey};");
			var expectedValue = $"*{key}={value}";
			if (!string.IsNullOrWhiteSpace(value))
			{
				AssertContains($"Assert JE_AddInfo has '*{key}={value}' (clusterKey = {JE_ClusterKey})", expectedValue, addInfo);
			}
			else
			{
				AssertNotContains($"Assert JE_AddInfo has no '*{key}={value}' (clusterKey = {JE_ClusterKey})", expectedValue, addInfo);
			}
		}

		#endregion

		public override string[] expectedIndex => new[]
		{
			$"NONCLUSTERED INDEX [_WTG_Populate JE_AddInfo with US_CRLAction and US_ENSAction_1] ON [dbo].[JobDeclaration] ([JE_DataModel], [JE_MessageType], [JE_ApplicationCode], [JE_SystemLastEditTimeUtc]) INCLUDE ([JE_AddInfo], [JE_ClusterKey], [JE_SystemLastEditUser]) WHERE (([JE_DataModel] IN ('US', 'USR')) AND ([JE_MessageType] IN ('IMP', 'REC', 'DRW')) AND [JE_ApplicationCode]='ACE' AND [JE_SystemLastEditTimeUtc]>='{DateTime.UtcNow.Date.AddDays(-5).ToString("yyyy-MM-dd")}') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG_Populate JE_AddInfo with US_CRLAction and US_ENSAction_2] ON [dbo].[CusEntryHeader] ([CH_MessageType]) INCLUDE ([CH_ClusterKey], [CH_PK]) WHERE ([CH_MessageType] IN ('ENS', 'REC', 'SE')) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
		};

		protected override void SetUp()
		{
			base.SetUp();
			data = new TransformationTestDataCreator();
			GC_PK = data.CreateCompany(Guid.NewGuid(), "CP1", "US", "USD");
			GB_PK = data.CreateBranch("BR1", "001", GC_PK);
			GE_PK = Guid.NewGuid();
			data.CreateDepartment(GE_PK, "A");
		}

		TransformationTestDataCreator data;
		Guid GC_PK, GB_PK, GE_PK;
		int clusterKey;
		const string JE_AddInfoToTest = "WTG=Y*EntryDate=2016-04-04 00:00:00.000";
	}
}
