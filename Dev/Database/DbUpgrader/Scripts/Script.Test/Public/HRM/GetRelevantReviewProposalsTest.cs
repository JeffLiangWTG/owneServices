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
	[TestedType(typeof(GetRelevantReviewProposals))]
	class GetRelevantReviewProposalsTest : DbCreateScriptTest
	{
		readonly IEqualityComparer<DataRow> comparator = DataRowComparer.Default;

		Guid reviewProcess;
		(Guid PK, string Code) manager;
		string managerTypes;

		public void TestNoMatchingPK()
		{
			var query = "SELECT RRP_PK from dbo.GetRelevantReviewProposals(@pk, @managerTypes)";
			var command = TestConnection.Command(query);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, Guid.NewGuid());
			command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);

			var results = DataUtils.GetDataTableFromCommand(command).AsEnumerable();
			AssertContainsExactElementsInAnyOrder(comparator, Enumerable.Empty<DataRow>(), results);
		}

		public void TestOneNode_OneProposal()
		{
			var node = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, node, null, reviewProcess, manager.PK, manager.Code);

			var proposal = Guid.NewGuid();
			CreateReviewProposal(TestConnection, proposal, node, Guid.NewGuid(), "S01");

			var query = "SELECT RRP_PK from dbo.GetRelevantReviewProposals(@pk, @managerTypes)";
			var command = TestConnection.Command(query);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, manager.PK);
			command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);

			var results = DataUtils.GetDataTableFromCommand(command).AsEnumerable();
			AssertContainsExactElementsInAnyOrder(comparator, ExpectedProposals(proposal), results);
		}

		public void TestOneNode_MultipleProposals()
		{
			var node = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, node, null, reviewProcess, manager.PK, manager.Code);

			var proposal1 = Guid.NewGuid();
			var proposal2 = Guid.NewGuid();
			var proposal3 = Guid.NewGuid();
			CreateReviewProposal(TestConnection, proposal1, node, Guid.NewGuid(), "S01");
			CreateReviewProposal(TestConnection, proposal2, node, Guid.NewGuid(), "S02");
			CreateReviewProposal(TestConnection, proposal3, node, Guid.NewGuid(), "S03");

			var query = "SELECT RRP_PK from dbo.GetRelevantReviewProposals(@pk, @managerTypes)";
			var command = TestConnection.Command(query);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, manager.PK);
			command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);

			var results = DataUtils.GetDataTableFromCommand(command).AsEnumerable();
			AssertContainsExactElementsInAnyOrder(comparator, ExpectedProposals(proposal1, proposal2, proposal3), results);
		}

		public void TestTwoTopLevelNodes()
		{
			var node1 = Guid.NewGuid();
			var node2 = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, node1, null, reviewProcess, manager.PK, manager.Code);
			CreateReviewProcessNode(TestConnection, node2, null, reviewProcess, Guid.NewGuid(), "MG2");

			var proposal1 = Guid.NewGuid();
			CreateReviewProposal(TestConnection, proposal1, node1, Guid.NewGuid(), "S01");
			CreateReviewProposal(TestConnection, Guid.NewGuid(), node2, Guid.NewGuid(), "S02");

			var query = "SELECT RRP_PK from dbo.GetRelevantReviewProposals(@pk, @managerTypes)";
			var command = TestConnection.Command(query);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, manager.PK);
			command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);

			var results = DataUtils.GetDataTableFromCommand(command).AsEnumerable();
			AssertContainsExactElementsInAnyOrder(comparator, ExpectedProposals(proposal1), results);
		}

		public void TestOneNode_OneChild()
		{
			var node = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, node, null, reviewProcess, manager.PK, manager.Code);

			var child = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, child, node, reviewProcess, Guid.NewGuid(), "MMR");

			var proposal = Guid.NewGuid();
			CreateReviewProposal(TestConnection, proposal, node, Guid.NewGuid(), "S01");

			var childProposal = Guid.NewGuid();
			CreateReviewProposal(TestConnection, childProposal, child, Guid.NewGuid(), "T01");

			var query = "SELECT RRP_PK from dbo.GetRelevantReviewProposals(@pk, @managerTypes)";
			var command = TestConnection.Command(query);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, manager.PK);
			command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);

			var results = DataUtils.GetDataTableFromCommand(command).AsEnumerable();
			AssertContainsExactElementsInAnyOrder(comparator, ExpectedProposals(proposal, childProposal), results);
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

			var proposal = Guid.NewGuid();
			CreateReviewProposal(TestConnection, proposal, node, Guid.NewGuid(), "S01");

			var child1Proposal = Guid.NewGuid();
			var child2Proposal = Guid.NewGuid();
			CreateReviewProposal(TestConnection, child1Proposal, child1, Guid.NewGuid(), "T01");
			CreateReviewProposal(TestConnection, child2Proposal, child2, Guid.NewGuid(), "T11");

			var grandchild1Proposal = Guid.NewGuid();
			var grandchild2Proposal = Guid.NewGuid();
			CreateReviewProposal(TestConnection, grandchild1Proposal, grandchild1, Guid.NewGuid(), "U01");
			CreateReviewProposal(TestConnection, grandchild2Proposal, grandchild2, Guid.NewGuid(), "U11");

			var query = "SELECT RRP_PK from dbo.GetRelevantReviewProposals(@pk, @managerTypes)";
			var command = TestConnection.Command(query);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, manager.PK);
			command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);

			var results = DataUtils.GetDataTableFromCommand(command).AsEnumerable();
			var expected = ExpectedProposals(proposal, child1Proposal, child2Proposal, grandchild1Proposal, grandchild2Proposal);
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

			CreateReviewProposal(TestConnection, Guid.NewGuid(), parent, Guid.NewGuid(), "R01");

			var proposal = Guid.NewGuid();
			CreateReviewProposal(TestConnection, proposal, node, Guid.NewGuid(), "S01");

			var childProposal = Guid.NewGuid();
			CreateReviewProposal(TestConnection, childProposal, child, Guid.NewGuid(), "T01");

			var query = "SELECT RRP_PK from dbo.GetRelevantReviewProposals(@pk, @managerTypes)";
			var command = TestConnection.Command(query);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, manager.PK);
			command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);

			var results = DataUtils.GetDataTableFromCommand(command).AsEnumerable();
			AssertContainsExactElementsInAnyOrder(comparator, ExpectedProposals(proposal, childProposal), results);
		}

		public void TestMultipleReviewProcesses()
		{
			var node1 = Guid.NewGuid();
			var node2 = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, node1, null, reviewProcess, manager.PK, manager.Code);
			CreateReviewProcessNode(TestConnection, node2, null, reviewProcess, Guid.NewGuid(), "MG1");

			var proposal1 = Guid.NewGuid();
			var proposal2 = Guid.NewGuid();
			var staff1 = Guid.NewGuid();
			var staff2 = Guid.NewGuid();
			CreateReviewProposal(TestConnection, proposal1, node1, staff1, "S01");
			CreateReviewProposal(TestConnection, proposal2, node1, staff2, "S02");

			var proposal3 = Guid.NewGuid();
			var proposal4 = Guid.NewGuid();
			var staff3 = Guid.NewGuid();
			var staff4 = Guid.NewGuid();
			CreateReviewProposal(TestConnection, proposal3, node2, staff3, "S11");
			CreateReviewProposal(TestConnection, proposal4, node2, staff4, "S12");

			var anotherReview = Guid.NewGuid();
			CreateReviewProcess(TestConnection, anotherReview, "More salaries to cut");

			var anotherNode1 = Guid.NewGuid();
			var anotherNode2 = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, anotherNode1, null, anotherReview, manager.PK, manager.Code);
			CreateReviewProcessNode(TestConnection, anotherNode2, null, anotherReview, Guid.NewGuid(), "MG2");

			var anotherProposal1 = Guid.NewGuid();
			CreateReviewProposal(TestConnection, anotherProposal1, anotherNode1, staff1, "S01");

			var anotherProposal2 = Guid.NewGuid();
			CreateReviewProposal(TestConnection, anotherProposal2, anotherNode2, staff3, "S11");

			var query = "SELECT RRP_PK from dbo.GetRelevantReviewProposals(@pk, @managerTypes)";
			var command = TestConnection.Command(query);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, manager.PK);
			command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);

			var results = DataUtils.GetDataTableFromCommand(command).AsEnumerable();
			var expected = ExpectedProposals(proposal1, proposal2, anotherProposal1);
			AssertContainsExactElementsInAnyOrder(comparator, expected, results);
		}

		public void TestNode_Children_WithoutProposalAndGrandchildren_WithProposal()
		{
			var parentNode = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, parentNode, null, reviewProcess, manager.PK, manager.Code);

			var childNode = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, childNode, parentNode, reviewProcess, Guid.NewGuid(), "SFA");

			var grandchildNode = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, grandchildNode, childNode, reviewProcess, Guid.NewGuid(), "STC");

			var proposal = Guid.NewGuid();
			CreateReviewProposal(TestConnection, proposal, parentNode, Guid.NewGuid(), "RPP");

			var grandchildProposal = Guid.NewGuid();
			CreateReviewProposal(TestConnection, grandchildProposal, grandchildNode, Guid.NewGuid(), "RPC");

			var query = "SELECT RRP_PK from dbo.GetRelevantReviewProposals(@pk, @managerTypes)";
			var command = TestConnection.Command(query);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, manager.PK);
			command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);

			var results = DataUtils.GetDataTableFromCommand(command).AsEnumerable();
			var expected = ExpectedProposals(proposal, grandchildProposal);
			AssertContainsExactElementsInAnyOrder(comparator, expected, results);
		}

		public void TestPrimaryHierarchy_IsManaged1()
		{
			var node = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, node, null, reviewProcess, manager.PK, manager.Code);
			CreateReviewProposal(TestConnection, Guid.NewGuid(), node, Guid.NewGuid(), "S01");

			var query = "SELECT * from dbo.GetRelevantReviewProposals(@pk, @managerTypes)";
			var command = TestConnection.Command(query);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, manager.PK);
			command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);

			var results = DataUtils.GetDataTableFromCommand(command);
			Assert(results.Columns.Contains("IsManaged1"));

			var proposal = results.Rows[0];
			AssertEquals(true, proposal["IsManaged1"]);
			AssertEquals(false, proposal["IsManaged2"]);
			AssertEquals(false, proposal["IsManaged3"]);
		}

		public void TestPrimaryHierarchy_IsManaged2()
		{
			managerTypes = "REM,DRM,LAP";

			var node = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, node, null, reviewProcess, manager.PK, manager.Code);
			CreateReviewProposal(TestConnection, Guid.NewGuid(), node, Guid.NewGuid(), "S01");

			var query = "SELECT * from dbo.GetRelevantReviewProposals(@pk, @managerTypes)";
			var command = TestConnection.Command(query);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, manager.PK);
			command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);

			var results = DataUtils.GetDataTableFromCommand(command);
			Assert(results.Columns.Contains("IsManaged2"));

			var proposal = results.Rows[0];
			AssertEquals(false, proposal["IsManaged1"]);
			AssertEquals(true, proposal["IsManaged2"]);
			AssertEquals(false, proposal["IsManaged3"]);
		}

		public void TestPrimaryHierarchy_IsManaged3()
		{
			managerTypes = "REM,LAP,DRM";

			var node = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, node, null, reviewProcess, manager.PK, manager.Code);
			CreateReviewProposal(TestConnection, Guid.NewGuid(), node, Guid.NewGuid(), "S01");

			var query = "SELECT * from dbo.GetRelevantReviewProposals(@pk, @managerTypes)";
			var command = TestConnection.Command(query);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, manager.PK);
			command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);

			var results = DataUtils.GetDataTableFromCommand(command);
			Assert(results.Columns.Contains("IsManaged3"));

			var proposal = results.Rows[0];
			AssertEquals(false, proposal["IsManaged1"]);
			AssertEquals(false, proposal["IsManaged2"]);
			AssertEquals(true, proposal["IsManaged3"]);
		}

		public void TestOverrideHierarchy_IsManaged1()
		{
			managerTypes = "REM,DRM,LAP";
			SetReviewProcessOverrideHierarchy(TestConnection, reviewProcess, "REM");

			var node = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, node, null, reviewProcess, manager.PK, manager.Code);
			CreateReviewProposal(TestConnection, Guid.NewGuid(), node, Guid.NewGuid(), "S01");

			var query = "SELECT * from dbo.GetRelevantReviewProposals(@pk, @managerTypes)";
			var command = TestConnection.Command(query);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, manager.PK);
			command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);

			var results = DataUtils.GetDataTableFromCommand(command);
			Assert(results.Columns.Contains("IsManaged1"));

			var proposal = results.Rows[0];
			AssertEquals(true, proposal["IsManaged1"]);
			AssertEquals(true, proposal["IsManaged2"]);
			AssertEquals(false, proposal["IsManaged3"]);
		}

		public void TestOverrideHierarchy_IsManaged2()
		{
			SetReviewProcessOverrideHierarchy(TestConnection, reviewProcess, "REM");

			var node = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, node, null, reviewProcess, manager.PK, manager.Code);
			CreateReviewProposal(TestConnection, Guid.NewGuid(), node, Guid.NewGuid(), "S01");

			var query = "SELECT * from dbo.GetRelevantReviewProposals(@pk, @managerTypes)";
			var command = TestConnection.Command(query);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, manager.PK);
			command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);

			var results = DataUtils.GetDataTableFromCommand(command);
			Assert(results.Columns.Contains("IsManaged2"));

			var proposal = results.Rows[0];
			AssertEquals(true, proposal["IsManaged1"]);
			AssertEquals(true, proposal["IsManaged2"]);
			AssertEquals(false, proposal["IsManaged3"]);
		}

		public void TestOverrideHierarchy_IsManaged3()
		{
			managerTypes = "DRM,LAP,REM";
			SetReviewProcessOverrideHierarchy(TestConnection, reviewProcess, "REM");

			var node = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, node, null, reviewProcess, manager.PK, manager.Code);
			CreateReviewProposal(TestConnection, Guid.NewGuid(), node, Guid.NewGuid(), "S01");

			var query = "SELECT * from dbo.GetRelevantReviewProposals(@pk, @managerTypes)";
			var command = TestConnection.Command(query);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, manager.PK);
			command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);

			var results = DataUtils.GetDataTableFromCommand(command);
			Assert(results.Columns.Contains("IsManaged3"));

			var proposal = results.Rows[0];
			AssertEquals(true, proposal["IsManaged1"]);
			AssertEquals(false, proposal["IsManaged2"]);
			AssertEquals(true, proposal["IsManaged3"]);
		}

		IEnumerable<DataRow> ExpectedProposals(params Guid[] pks)
		{
			var expected = new DataTable();
			expected.Columns.Add(new DataColumn("RRP_PK", typeof(Guid)));

			foreach (var pk in pks)
			{
				var row = expected.NewRow();
				row["RRP_PK"] = pk;
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
