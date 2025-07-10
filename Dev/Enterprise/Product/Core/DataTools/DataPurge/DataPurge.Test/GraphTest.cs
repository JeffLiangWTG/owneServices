using System.Linq;
using CargoWise.Types;
using Enterprise.DataPurge.Utility;
using NUnit.Framework;

namespace Enterprise.DataPurge
{
	sealed class GraphTest : TransactionedTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			var sqlTemplate = @"
CREATE TABLE dbo.UserForTest
(
	User_PK UNIQUEIDENTIFIER NOT NULL
		CONSTRAINT UserForTest_pk
			PRIMARY KEY,
	User_GC UNIQUEIDENTIFIER
		CONSTRAINT UserForTest_GlbCompany_GC_PK_fk
			REFERENCES dbo.GlbCompany
)

CREATE TABLE dbo.AddressForTest
(
	Address_PK UNIQUEIDENTIFIER NOT NULL
		CONSTRAINT AddressForTest_pk
			PRIMARY KEY,
	User_FK UNIQUEIDENTIFIER
		CONSTRAINT AddressForTest_UserForTest_User_PK_fk
			REFERENCES dbo.UserForTest,
	Address_GC UNIQUEIDENTIFIER
		CONSTRAINT AddressForTest_GlbCompany_GC_PK_fk
			REFERENCES dbo.GlbCompany
)
";
			TestConnection.ExecuteNonQuery(sqlTemplate);
		}

		public void TestGenerateGraphFromRootNode()
		{
			var companyPK = ZGuid.NewZGuid();
			var sqlUtility = new SqlUtility(TestConnection);
			var graph = new Graph(sqlUtility, companyPK);
			graph.AddNodeRecursively(new Node("UserForTest", null, "User_PK", null));
			var graphNodes = graph.GetGraphNodes_ForTestOnly();
			AssertEquals(1,graphNodes.Keys.Count);
			AssertEquals(1,graphNodes.Values.Count);
			AssertEquals(graphNodes.First().Value.First().PKTableName, "AddressForTest");
			AssertEquals(graphNodes.First().Value.First().PKName, "Address_PK");
			AssertEquals(graphNodes.First().Value.First().FKTableName, "UserForTest");
			AssertEquals(graphNodes.First().Value.First().FKName, "User_FK");

			graph = new Graph(sqlUtility, companyPK);
			graph.AddNodeRecursively(new Node("AddressForTest", null, "Address_PK", null));
			graphNodes = graph.GetGraphNodes_ForTestOnly();
			AssertEquals(0, graphNodes.Count);
		}

		public void TestGenerateSqlRowsByDFS()
		{
			var companyPK = ZGuid.NewZGuid();
			var sqlUtility = new SqlUtility(TestConnection);
			var graph = new Graph(sqlUtility, companyPK);
			var root = new Node("UserForTest", null, "User_PK", null);
			graph.AddNodeRecursively(root);
			var sqlTemplates = graph.GenerateSqlRowsByDFS(root).ToList();

			AssertEquals(12, sqlTemplates.Count);

			AssertCollectionContains("UserForTest should be the first Node.",sqlTemplates, x => x.Contains("INSERT dbo.UserForTest (User_PK) VALUES"));
			AssertCollectionContains("AddressForTest should not be the first Node.", sqlTemplates, x => x.Contains("INSERT dbo.AddressForTest (Address_PK, User_FK) VALUES"));
			AssertCollectionContains(sqlTemplates, x => x.Contains($"UPDATE dbo.UserForTest SET User_GC='{companyPK}'"));
			AssertCollectionContains(sqlTemplates, x => x.Contains($"UPDATE dbo.UserForTest SET User_GC='{ZGuid.Empty}'"));
			AssertCollectionContains(sqlTemplates, x => x.Contains($"UPDATE dbo.AddressForTest SET Address_GC='{companyPK}'"));
			AssertCollectionContains(sqlTemplates, x => x.Contains($"UPDATE dbo.AddressForTest SET Address_GC='{ZGuid.Empty}'"));
		}
	}
}
