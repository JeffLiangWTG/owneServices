using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.DataPurge.Utility;
using NUnit.Framework;

namespace Enterprise.DataPurge
{
	sealed class SqlUtilityTest : TransactionedTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			var sqlTemplate = @"CREATE TABLE dbo.UserForTest
(
	User_PK INT NOT NULL
		CONSTRAINT UserForTest_pk
			PRIMARY KEY
)

CREATE TABLE dbo.AddressForTest
(
	Address_PK INT NOT NULL
		CONSTRAINT AddressForTest_pk
			PRIMARY KEY,
	User_FK INT
		CONSTRAINT AddressForTest_UserForTest_User_PK_fk
			REFERENCES dbo.UserForTest
)

CREATE TABLE dbo.AncestorTableForTest
(
	AncestorTableForTest_PK INT NOT NULL
		CONSTRAINT AncestorTableForTest_pk
			PRIMARY KEY,
	GC_FK UNIQUEIDENTIFIER
		CONSTRAINT AncestorTableForTest_GlbCompany_GC_PK_fk
			REFERENCES dbo.GlbCompany
)

CREATE TABLE dbo.ParentTableForTest
(
	ParentTableForTest_PK INT NOT NULL
		CONSTRAINT ParentTableForTest_pk
			PRIMARY KEY,
	GC_FK UNIQUEIDENTIFIER
		CONSTRAINT ParentTableForTest_GlbCompany_GC_PK_fk
			REFERENCES dbo.GlbCompany,
	Ancestor_FK INT
		CONSTRAINT ParentTableForTest_AncestorTableForTest_AncestorTableForTest_PK_fk
			REFERENCES dbo.AncestorTableForTest
)

CREATE TABLE dbo.ChildTableWithoutPKForTest
(
	Parent_FK INT
		CONSTRAINT ChildTableWithoutPKForTest_ParentTableForTest_ParentTableForTest_PK_fk
			REFERENCES dbo.ParentTableForTest,
	GC_FK UNIQUEIDENTIFIER
		CONSTRAINT ChildTableWithoutPKForTest_GlbCompany_GC_PK_fk
			REFERENCES dbo.GlbCompany
)

CREATE TABLE dbo.ChildTableWithoutPK2ForTest
(
	Parent_FK INT
		CONSTRAINT ChildTableWithoutPK2ForTest_ParentTableForTest_ParentTableForTest_PK_fk
			REFERENCES dbo.ParentTableForTest
)
";
			TestConnection.ExecuteNonQuery(sqlTemplate);
		}

		public void TestGetForeignKeysReferToTable()
		{
			var sqlUtility = new SqlUtility(TestConnection);
			var referencedTables = sqlUtility.GetForeignKeysReferToTable("UserForTest").ToList();
			AssertEquals(1, referencedTables.Count);
			AssertCollectionContains(referencedTables, x => x.FKTableName == "AddressForTest");

			referencedTables = sqlUtility.GetForeignKeysReferToTable("AddressForTest").ToList();
			AssertEquals(0, referencedTables.Count);
		}

		public void TestGetForeignKeysUsedByTable()
		{
			var sqlUtility = new SqlUtility(TestConnection);
			var usedTables = sqlUtility.GetForeignKeysUsedByTable("UserForTest").ToList();
			AssertEquals(0, usedTables.Count);

			usedTables = sqlUtility.GetForeignKeysUsedByTable("AddressForTest").ToList();
			AssertEquals(1, usedTables.Count);
			AssertCollectionContains(usedTables, x => x.FKTableName == "UserForTest");
		}

		public void TestGetPrimaryKeyNameByTableName()
		{
			var sqlUtility = new SqlUtility(TestConnection);
			var pkName = sqlUtility.GetPrimaryKeyNameByTableName("UserForTest");
			AssertEquals(pkName, "User_PK");

			pkName = sqlUtility.GetPrimaryKeyNameByTableName("ChildTableWithoutPKForTest");
			AssertNullOrEmpty(pkName);
		}

		public void TestConvertNodePathToSql()
		{
			var sqlUtility = new SqlUtility(TestConnection);
			var nodes = new List<Node>();
			var companyPK = ZGuid.NewZGuid();
			var sqlTemplates = sqlUtility.ConvertNodePathToSql(nodes, companyPK).ToList();

			AssertEquals(0, sqlTemplates.Count);

			var expectSqlTemplate = new List<string>()
			{
				"INSERT dbo.JobHeader (JH_PK) VALUES",
				$"UPDATE dbo.JobHeader SET JH_GC='{companyPK}'"
			};
			nodes.Add(new Node("JobHeader", null, "JH_PK", null));
			sqlTemplates = sqlUtility.ConvertNodePathToSql(nodes, companyPK).ToList();
			AssertEquals(2, sqlTemplates.Count);
			AssertCollectionContainsOrderly(expectSqlTemplate, sqlTemplates);

			nodes = new List<Node>();
			nodes.Add(new Node("JobHeader", "JobCharge", "JH_PK", "JR_PK"));
			nodes.Add(new Node("JobCharge", "JobHeader", "JR_PK", "JR_JH"));
			expectSqlTemplate = new List<string>()
			{
				"INSERT dbo.JobHeader (JH_PK) VALUES",
				$"UPDATE dbo.JobHeader SET JH_GC='{ZGuid.Empty}'",
				"INSERT dbo.JobCharge (JR_PK, JR_JH) VALUES",
				$"UPDATE dbo.JobCharge SET JR_GC='{companyPK}'",

				"INSERT dbo.JobHeader (JH_PK) VALUES",
				$"UPDATE dbo.JobHeader SET JH_GC='{companyPK}'",
				"INSERT dbo.JobCharge (JR_PK, JR_JH) VALUES",
				$"UPDATE dbo.JobCharge SET JR_GC='{ZGuid.Empty}'",

				"INSERT dbo.JobHeader (JH_PK) VALUES",
				$"UPDATE dbo.JobHeader SET JH_GC='{companyPK}'",
				"INSERT dbo.JobCharge (JR_PK, JR_JH) VALUES",
				$"UPDATE dbo.JobCharge SET JR_GC='{companyPK}'"
			};
			sqlTemplates = sqlUtility.ConvertNodePathToSql(nodes, companyPK).ToList();
			AssertEquals(12, sqlTemplates.Count);
			AssertCollectionContainsOrderly(expectSqlTemplate, sqlTemplates);
		}

		public void TestConvertNodePathToSqlWithNoPK()
		{
			var sqlUtility = new SqlUtility(TestConnection);
			var nodes = new List<Node>();
			var companyPK = ZGuid.NewZGuid();
			nodes.Add(new Node("ParentTableForTest", null, "ParentTableForTest_PK", null));
			nodes.Add(new Node("ChildTableWithoutPKForTest", "ParentTableForTest", null, "Parent_FK"));
			var expectSqlTemplate = new List<string>()
			{
				"INSERT dbo.ParentTableForTest (ParentTableForTest_PK) VALUES",
				$"UPDATE dbo.ParentTableForTest SET GC_FK='{ZGuid.Empty}'",
				"INSERT dbo.ChildTableWithoutPKForTest (Parent_FK) VALUES",
				$"UPDATE dbo.ChildTableWithoutPKForTest SET GC_FK='{companyPK}'",

				"INSERT dbo.ParentTableForTest (ParentTableForTest_PK) VALUES",
				$"UPDATE dbo.ParentTableForTest SET GC_FK='{companyPK}'",
				"INSERT dbo.ChildTableWithoutPKForTest (Parent_FK) VALUES",
				$"UPDATE dbo.ChildTableWithoutPKForTest SET GC_FK='{ZGuid.Empty}'",

				"INSERT dbo.ParentTableForTest (ParentTableForTest_PK) VALUES",
				$"UPDATE dbo.ParentTableForTest SET GC_FK='{companyPK}'",
				"INSERT dbo.ChildTableWithoutPKForTest (Parent_FK) VALUES",
				$"UPDATE dbo.ChildTableWithoutPKForTest SET GC_FK='{companyPK}'",
			};
			var sqlTemplates = sqlUtility.ConvertNodePathToSql(nodes, companyPK).ToList();
			AssertEquals(12, sqlTemplates.Count);
			AssertCollectionContainsOrderly(expectSqlTemplate, sqlTemplates);

			nodes = new List<Node>();
			nodes.Add(new Node("ParentTableForTest", null, "ParentTableForTest_PK", null));
			nodes.Add(new Node("ChildTableWithoutPK2ForTest", "ParentTableForTest", null, "Parent_FK"));
			expectSqlTemplate = new List<string>()
			{
				"INSERT dbo.ParentTableForTest (ParentTableForTest_PK) VALUES",
				$"UPDATE dbo.ParentTableForTest SET GC_FK='{companyPK}'",
				"INSERT dbo.ChildTableWithoutPK2ForTest (Parent_FK) VALUES"
			};
			sqlTemplates = sqlUtility.ConvertNodePathToSql(nodes, companyPK).ToList();
			AssertEquals(3, sqlTemplates.Count);
			AssertCollectionContainsOrderly(expectSqlTemplate, sqlTemplates);

			nodes = new List<Node>();
			nodes.Add(new Node("AncestorTableForTest", null, "AncestorTableForTest_PK", null));
			nodes.Add(new Node("ParentTableForTest", "AncestorTableForTest", "ParentTableForTest_PK", "Ancestor_FK"));
			nodes.Add(new Node("ChildTableWithoutPK2ForTest", "ParentTableForTest", null, "Parent_FK"));
			expectSqlTemplate = new List<string>()
			{
				"INSERT dbo.AncestorTableForTest (AncestorTableForTest_PK) VALUES",
				$"UPDATE dbo.AncestorTableForTest SET GC_FK='{ZGuid.Empty}'",
				"INSERT dbo.ParentTableForTest (ParentTableForTest_PK, Ancestor_FK) VALUES",
				$"UPDATE dbo.ParentTableForTest SET GC_FK='{companyPK}'",
				"INSERT dbo.ChildTableWithoutPK2ForTest (Parent_FK) VALUES",

				"INSERT dbo.AncestorTableForTest (AncestorTableForTest_PK) VALUES",
				$"UPDATE dbo.AncestorTableForTest SET GC_FK='{companyPK}'",
				"INSERT dbo.ParentTableForTest (ParentTableForTest_PK, Ancestor_FK) VALUES",
				$"UPDATE dbo.ParentTableForTest SET GC_FK='{ZGuid.Empty}'",
				"INSERT dbo.ChildTableWithoutPK2ForTest (Parent_FK) VALUES",

				"INSERT dbo.AncestorTableForTest (AncestorTableForTest_PK) VALUES",
				$"UPDATE dbo.AncestorTableForTest SET GC_FK='{companyPK}'",
				"INSERT dbo.ParentTableForTest (ParentTableForTest_PK, Ancestor_FK) VALUES",
				$"UPDATE dbo.ParentTableForTest SET GC_FK='{companyPK}'",
				"INSERT dbo.ChildTableWithoutPK2ForTest (Parent_FK) VALUES",
			};
			sqlTemplates = sqlUtility.ConvertNodePathToSql(nodes, companyPK).ToList();
			AssertEquals(15, sqlTemplates.Count);
			AssertCollectionContainsOrderly(expectSqlTemplate, sqlTemplates);
		}

		public void TestGenerateCompany()
		{
			var sqlUtility = new SqlUtility(TestConnection);
			var companyPk = sqlUtility.GenerateCompany();
			var sqlTemplate = $"SELECT COUNT(*) FROM dbo.GlbCompany WHERE GC_PK = '{companyPk}'";
			var companyCount = TestConnection.ExecuteScalar<int>(sqlTemplate);
			AssertEquals(1, companyCount);
		}

		public void TestGenerateBranchBelongToCompany()
		{
			var sqlUtility = new SqlUtility(TestConnection);
			var companyPk = sqlUtility.GenerateCompany();
			var sqlTemplate = $"SELECT COUNT(*) FROM dbo.GlbBranch WHERE GB_GC = '{companyPk}'";
			var branchCount = TestConnection.ExecuteScalar<int>(sqlTemplate);
			AssertEquals(0, branchCount);
			sqlUtility.GenerateBranchBelongToCompany(companyPk);

			branchCount = TestConnection.ExecuteScalar<int>(sqlTemplate);
			AssertEquals(1, branchCount);
		}

		public void TestBatchInsertRows()
		{
			var sqlUtility = new SqlUtility(TestConnection);
			var sqlTemplates = new List<string>();
			for (var i = 0; i < 5; i++)
			{
				sqlTemplates.Add($"INSERT INTO UserForTest VALUES ({i})");
			}
			sqlUtility.BatchInsertRows(sqlTemplates);
			var sqlTemplate = $"SELECT COUNT(*) from UserForTest WHERE User_PK IN ({string.Join(",", Enumerable.Range(0, 5))})";
			var userCount = TestConnection.ExecuteScalar<int>(sqlTemplate);
			AssertEquals(5, userCount);

			sqlTemplates = new List<string>();
			for (var i = 5; i < 10; i++)
			{
				sqlTemplates.Add($"INSERT INTO UserForTest VALUES ({i})");
			}
			sqlUtility.BatchInsertRows(sqlTemplates,5);
			sqlTemplate = $"SELECT COUNT(*) from UserForTest WHERE User_PK IN ({string.Join(",", Enumerable.Range(5, 10))})";
			userCount = TestConnection.ExecuteScalar<int>(sqlTemplate);
			AssertEquals(5, userCount);

			sqlTemplates = new List<string>();
			for (var i = 10; i < 15; i++)
			{
				sqlTemplates.Add($"INSERT INTO UserForTest VALUES ({i})");
			}
			sqlUtility.BatchInsertRows(sqlTemplates, 1);
			sqlTemplate = $"SELECT COUNT(*) from UserForTest WHERE User_PK IN ({string.Join(",", Enumerable.Range(10, 15))})";
			userCount = TestConnection.ExecuteScalar<int>(sqlTemplate);
			AssertEquals(5, userCount);
		}

		public void TestCreateTraversalPath()
		{
			var sqlUtility = new SqlUtility(TestConnection);
			var traversalPaths = sqlUtility.CreateTraversalPath(new List<List<int>>()).Select(x => x.ToList()).ToList();
			AssertEquals(traversalPaths.Count, 0);

			var nodeGroup1 = new List<int>() { 1, 2 };
			var nodeGroup2 = new List<int>() { 3, 4 };
			var nodeGroup3 = new List<int>() { 5 };
			var nodesToBeTraversed = new List<List<int>>() { nodeGroup1, nodeGroup2, nodeGroup3 };
			traversalPaths = sqlUtility.CreateTraversalPath(nodesToBeTraversed).Select(x => x.ToList()).ToList();
			AssertEquals(traversalPaths.Count, 4);
			AssertCollectionContains(traversalPaths, x => x[0] == 1 && x[1] == 3 && x[2] == 5);
			AssertCollectionContains(traversalPaths, x => x[0] == 1 && x[1] == 4 && x[2] == 5);
			AssertCollectionContains(traversalPaths, x => x[0] == 2 && x[1] == 3 && x[2] == 5);
			AssertCollectionContains(traversalPaths, x => x[0] == 2 && x[1] == 4 && x[2] == 5);
		}

		void AssertCollectionContainsOrderly(List<string> expectSubSequence, List<string> actualSequence)
		{
			AssertEquals(expectSubSequence.Count, actualSequence.Count);
			for (var i = 0; i < expectSubSequence.Count; i++)
			{
				AssertContains($"Line {i} does not contain expected", expectSubSequence[i], actualSequence[i]);
			}
		}
	}
}
