using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.HRM;
using NUnit.Framework;
using static Enterprise.Build.Database.Script.Testing.Public.HRM.HRMDataHelpers;

namespace Enterprise.Build.Database.Script.Testing.Public.HRM
{
	[TestedType(typeof(GetRelevantReviewProcessNodes))]
	class GetRelevantReviewProcessNodesTest : DbCreateScriptTest
	{
		readonly IEqualityComparer<DataRow> comparator = DataRowComparer.Default;

		Guid reviewProcess;
		(Guid PK, string Code) manager;
		string managerTypes;

		public void TestNoMatchingPK()
		{
			var query = "SELECT RRN_PK from dbo.GetRelevantReviewProcessNodes(@pk, @managerTypes)";
			var command = TestConnection.Command(query);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, Guid.NewGuid());
			command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);

			var results = DataUtils.GetDataTableFromCommand(command).AsEnumerable();
			AssertContainsExactElementsInAnyOrder(comparator, Enumerable.Empty<DataRow>(), results);
		}

		public void TestOneNode()
		{
			var node = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, node, null, reviewProcess, manager.PK, manager.Code);

			var query = "SELECT RRN_PK from dbo.GetRelevantReviewProcessNodes(@pk, @managerTypes)";
			var command = TestConnection.Command(query);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, manager.PK);
			command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);

			var results = DataUtils.GetDataTableFromCommand(command).AsEnumerable();
			AssertContainsExactElementsInAnyOrder(comparator, ExpectedNodes(node), results);
		}

		public void TestTwoTopLevelNodes()
		{
			var node1 = Guid.NewGuid();
			var node2 = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, node1, null, reviewProcess, manager.PK, manager.Code);
			CreateReviewProcessNode(TestConnection, node2, null, reviewProcess, Guid.NewGuid(), "MG2");

			var query = "SELECT RRN_PK from dbo.GetRelevantReviewProcessNodes(@pk, @managerTypes)";
			var command = TestConnection.Command(query);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, manager.PK);
			command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);

			var results = DataUtils.GetDataTableFromCommand(command).AsEnumerable();
			AssertContainsExactElementsInAnyOrder(comparator, ExpectedNodes(node1), results);
		}

		public void TestNode_ChildrenAndGrandchildren()
		{
			var node = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, node, null, reviewProcess, manager.PK, manager.Code);

			var child1 = Guid.NewGuid();
			var child2 = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, child1, node, reviewProcess, Guid.NewGuid(), "MM1");
			CreateReviewProcessNode(TestConnection, child2, node, reviewProcess, Guid.NewGuid(), "MM2");

			var grandchild1 = Guid.NewGuid();
			var grandchild2 = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, grandchild1, child1, reviewProcess, Guid.NewGuid(), "SM1");
			CreateReviewProcessNode(TestConnection, grandchild2, child2, reviewProcess, Guid.NewGuid(), "SM2");

			var query = "SELECT RRN_PK from dbo.GetRelevantReviewProcessNodes(@pk, @managerTypes)";
			var command = TestConnection.Command(query);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, manager.PK);
			command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);

			var results = DataUtils.GetDataTableFromCommand(command).AsEnumerable();
			var expected = ExpectedNodes(node, child1, child2, grandchild1, grandchild2);
			AssertContainsExactElementsInAnyOrder(comparator, expected, results);
		}

		public void TestParent()
		{
			var parent = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, parent, null, reviewProcess, Guid.NewGuid(), "CEO");

			var node = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, node, parent, reviewProcess, manager.PK, manager.Code);

			var child = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, child, node, reviewProcess, Guid.NewGuid(), "MMR");

			var query = "SELECT RRN_PK from dbo.GetRelevantReviewProcessNodes(@pk, @managerTypes)";
			var command = TestConnection.Command(query);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, manager.PK);
			command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);

			var results = DataUtils.GetDataTableFromCommand(command).AsEnumerable();
			AssertContainsExactElementsInAnyOrder(comparator, ExpectedNodes(node, child), results);
		}

		public void TestMultipleReviewProcesses()
		{
			var node1 = Guid.NewGuid();
			var node2 = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, node1, null, reviewProcess, manager.PK, manager.Code);
			CreateReviewProcessNode(TestConnection, node2, null, reviewProcess, Guid.NewGuid(), "MG1");

			var child1 = Guid.NewGuid();
			var child2 = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, child1, node1, reviewProcess, Guid.NewGuid(), "MM1");
			CreateReviewProcessNode(TestConnection, child2, node2, reviewProcess, Guid.NewGuid(), "MM2");

			var anotherReview = Guid.NewGuid();
			CreateReviewProcess(TestConnection, anotherReview, "More salaries to cut");

			var anotherNode1 = Guid.NewGuid();
			var anotherNode2 = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, anotherNode1, null, anotherReview, manager.PK, manager.Code);
			CreateReviewProcessNode(TestConnection, anotherNode2, null, anotherReview, Guid.NewGuid(), "MG2");

			var query = "SELECT RRN_PK from dbo.GetRelevantReviewProcessNodes(@pk, @managerTypes)";
			var command = TestConnection.Command(query);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, manager.PK);
			command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);

			var results = DataUtils.GetDataTableFromCommand(command).AsEnumerable();
			var expected = ExpectedNodes(node1, child1, anotherNode1);
			AssertContainsExactElementsInAnyOrder(comparator, expected, results);
		}

		public void TestPrimaryHierarchy_IsManaged1()
		{
			CreateReviewProcessNode(TestConnection, Guid.NewGuid(), null, reviewProcess, manager.PK, manager.Code);

			var query = "SELECT * from dbo.GetRelevantReviewProcessNodes(@pk, @managerTypes)";
			var command = TestConnection.Command(query);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, manager.PK);
			command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);

			var results = DataUtils.GetDataTableFromCommand(command);
			Assert(results.Columns.Contains("IsManaged1"));

			var node = results.Rows[0];
			AssertEquals(true, node["IsManaged1"]);
			AssertEquals(false, node["IsManaged2"]);
			AssertEquals(false, node["IsManaged3"]);
		}

		public void TestPrimaryHierarchy_IsManaged2()
		{
			managerTypes = "REM,DRM,LAP";

			CreateReviewProcessNode(TestConnection, Guid.NewGuid(), null, reviewProcess, manager.PK, manager.Code);

			var query = "SELECT * from dbo.GetRelevantReviewProcessNodes(@pk, @managerTypes)";
			var command = TestConnection.Command(query);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, manager.PK);
			command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);

			var results = DataUtils.GetDataTableFromCommand(command);
			Assert(results.Columns.Contains("IsManaged2"));

			var node = results.Rows[0];
			AssertEquals(false, node["IsManaged1"]);
			AssertEquals(true, node["IsManaged2"]);
			AssertEquals(false, node["IsManaged3"]);
		}

		public void TestPrimaryHierarchy_IsManaged3()
		{
			managerTypes = "REM,LAP,DRM";

			CreateReviewProcessNode(TestConnection, Guid.NewGuid(), null, reviewProcess, manager.PK, manager.Code);

			var query = "SELECT * from dbo.GetRelevantReviewProcessNodes(@pk, @managerTypes)";
			var command = TestConnection.Command(query);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, manager.PK);
			command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);

			var results = DataUtils.GetDataTableFromCommand(command);
			Assert(results.Columns.Contains("IsManaged3"));

			var node = results.Rows[0];
			AssertEquals(false, node["IsManaged1"]);
			AssertEquals(false, node["IsManaged2"]);
			AssertEquals(true, node["IsManaged3"]);
		}

		public void TestOverrideHierarchy_IsManaged1()
		{
			managerTypes = "REM,DRM,LAP";
			SetReviewProcessOverrideHierarchy(TestConnection, reviewProcess, "REM");

			CreateReviewProcessNode(TestConnection, Guid.NewGuid(), null, reviewProcess, manager.PK, manager.Code);

			var query = "SELECT * from dbo.GetRelevantReviewProcessNodes(@pk, @managerTypes)";
			var command = TestConnection.Command(query);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, manager.PK);
			command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);

			var results = DataUtils.GetDataTableFromCommand(command);
			Assert(results.Columns.Contains("IsManaged1"));

			var node = results.Rows[0];
			AssertEquals(true, node["IsManaged1"]);
			AssertEquals(true, node["IsManaged2"]);
			AssertEquals(false, node["IsManaged3"]);
		}

		public void TestOverrideHierarchy_IsManaged2()
		{
			SetReviewProcessOverrideHierarchy(TestConnection, reviewProcess, "REM");

			CreateReviewProcessNode(TestConnection, Guid.NewGuid(), null, reviewProcess, manager.PK, manager.Code);

			var query = "SELECT * from dbo.GetRelevantReviewProcessNodes(@pk, @managerTypes)";
			var command = TestConnection.Command(query);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, manager.PK);
			command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);

			var results = DataUtils.GetDataTableFromCommand(command);
			Assert(results.Columns.Contains("IsManaged2"));

			var node = results.Rows[0];
			AssertEquals(true, node["IsManaged1"]);
			AssertEquals(true, node["IsManaged2"]);
			AssertEquals(false, node["IsManaged3"]);
		}

		public void TestOverrideHierarchy_IsManaged3()
		{
			managerTypes = "DRM,LAP,REM";
			SetReviewProcessOverrideHierarchy(TestConnection, reviewProcess, "REM");

			CreateReviewProcessNode(TestConnection, Guid.NewGuid(), null, reviewProcess, manager.PK, manager.Code);

			var query = "SELECT * from dbo.GetRelevantReviewProcessNodes(@pk, @managerTypes)";
			var command = TestConnection.Command(query);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, manager.PK);
			command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);

			var results = DataUtils.GetDataTableFromCommand(command);
			Assert(results.Columns.Contains("IsManaged3"));

			var node = results.Rows[0];
			AssertEquals(true, node["IsManaged1"]);
			AssertEquals(false, node["IsManaged2"]);
			AssertEquals(true, node["IsManaged3"]);
		}

		IEnumerable<DataRow> ExpectedNodes(params Guid[] pks)
		{
			var expected = new DataTable();
			expected.Columns.Add(new DataColumn("RRN_PK", typeof(Guid)));

			foreach (var pk in pks)
			{
				var row = expected.NewRow();
				row["RRN_PK"] = pk;
				expected.Rows.Add(row);
			}

			return expected.AsEnumerable();
		}

		protected override void SetUp()
		{
			reviewProcess = Guid.NewGuid();
			CreateReviewProcess(TestConnection, reviewProcess, "Whose pay should we cut");

			manager = (PK: Guid.NewGuid(), Code: "MGR");
			managerTypes = "DRM,REM,LAP";

			base.SetUp();
		}

		protected override DbConnection TestConnection => adminConnection ??= Db.NewAdminConnection();
		AdminConnection adminConnection;
	}
}
