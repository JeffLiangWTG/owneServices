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
	[TestedType(typeof(GetRelevantReviewProposalEntitlements))]
	class GetRelevantReviewProposalEntitlementsTest : DbCreateScriptTest
	{
		readonly IEqualityComparer<DataRow> comparator = DataRowComparer.Default;

		Guid reviewProcess;
		(Guid PK, string Code) manager;
		string managerTypes;

		public void TestNoMatchingPK()
		{
			var query = "SELECT RRE_PK from dbo.GetRelevantReviewProposalEntitlements(@pk, @managerTypes)";
			var command = TestConnection.Command(query);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, Guid.NewGuid());
			command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);

			var results = DataUtils.GetDataTableFromCommand(command).AsEnumerable();
			AssertContainsExactElementsInAnyOrder(comparator, Enumerable.Empty<DataRow>(), results);
		}

		public void TestOneTopLevelNode_ThreeEntitlements()
		{
			var node = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, node, null, reviewProcess, manager.PK, manager.Code);

			var proposal = Guid.NewGuid();
			CreateReviewProposal(TestConnection, proposal, node, Guid.NewGuid(), "S01");

			var entitlement1 = Guid.NewGuid();
			var entitlement2 = Guid.NewGuid();
			var entitlement3 = Guid.NewGuid();
			CreateReviewProposalEntitlement(TestConnection, entitlement1, proposal, "BAS", 100000);
			CreateReviewProposalEntitlement(TestConnection, entitlement2, proposal, "RME", 20000);
			CreateReviewProposalEntitlement(TestConnection, entitlement3, proposal, "WOW", 1000);

			var query = "SELECT RRE_PK from dbo.GetRelevantReviewProposalEntitlements(@pk, @managerTypes)";
			var command = TestConnection.Command(query);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, manager.PK);
			command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);

			var results = DataUtils.GetDataTableFromCommand(command).AsEnumerable();
			var expected = ExpectedProposalEntitlements(entitlement1, entitlement2, entitlement3);
			AssertContainsExactElementsInAnyOrder(comparator, expected, results);
		}

		public void TestOneTopLevelNode_TwoProposals()
		{
			var node = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, node, null, reviewProcess, manager.PK, manager.Code);

			var proposal1 = Guid.NewGuid();
			var proposal2 = Guid.NewGuid();
			CreateReviewProposal(TestConnection, proposal1, node, Guid.NewGuid(), "S01");
			CreateReviewProposal(TestConnection, proposal2, node, Guid.NewGuid(), "S02");

			var entitlement1 = Guid.NewGuid();
			var entitlement2 = Guid.NewGuid();
			CreateReviewProposalEntitlement(TestConnection, entitlement1, proposal1, "BAS", 100000);
			CreateReviewProposalEntitlement(TestConnection, entitlement2, proposal1, "RME", 20000);

			var entitlement3 = Guid.NewGuid();
			var entitlement4 = Guid.NewGuid();
			CreateReviewProposalEntitlement(TestConnection, entitlement3, proposal2, "BAS", 100000);
			CreateReviewProposalEntitlement(TestConnection, entitlement4, proposal2, "RME", 20000);

			var query = "SELECT RRE_PK from dbo.GetRelevantReviewProposalEntitlements(@pk, @managerTypes)";
			var command = TestConnection.Command(query);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, manager.PK);
			command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);

			var results = DataUtils.GetDataTableFromCommand(command).AsEnumerable();
			var expected = ExpectedProposalEntitlements(entitlement1, entitlement2, entitlement3, entitlement4);
			AssertContainsExactElementsInAnyOrder(comparator, expected, results);
		}

		public void TestOneTopLevelNode_GetChildEntitlementsFromTopLevel()
		{
			var node = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, node, null, reviewProcess, manager.PK, manager.Code);

			var child1 = Guid.NewGuid();
			var child2 = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, child1, node, reviewProcess, Guid.NewGuid(), "MM1");
			CreateReviewProcessNode(TestConnection, child2, node, reviewProcess, Guid.NewGuid(), "MM2");

			var proposal1 = Guid.NewGuid();
			var proposal2 = Guid.NewGuid();
			CreateReviewProposal(TestConnection, proposal1, child1, Guid.NewGuid(), "S01");
			CreateReviewProposal(TestConnection, proposal2, child1, Guid.NewGuid(), "S02");

			var entitlement1 = Guid.NewGuid();
			var entitlement2 = Guid.NewGuid();
			CreateReviewProposalEntitlement(TestConnection, entitlement1, proposal1, "BAS", 100000);
			CreateReviewProposalEntitlement(TestConnection, entitlement2, proposal1, "RME", 20000);

			var entitlement3 = Guid.NewGuid();
			var entitlement4 = Guid.NewGuid();
			CreateReviewProposalEntitlement(TestConnection, entitlement3, proposal2, "BAS", 100000);
			CreateReviewProposalEntitlement(TestConnection, entitlement4, proposal2, "RME", 20000);

			var proposal3 = Guid.NewGuid();
			var proposal4 = Guid.NewGuid();
			CreateReviewProposal(TestConnection, proposal3, child2, Guid.NewGuid(), "T01");
			CreateReviewProposal(TestConnection, proposal4, child2, Guid.NewGuid(), "T02");

			var entitlement5 = Guid.NewGuid();
			var entitlement6 = Guid.NewGuid();
			CreateReviewProposalEntitlement(TestConnection, entitlement5, proposal3, "BAS", 80000);
			CreateReviewProposalEntitlement(TestConnection, entitlement6, proposal3, "RME", 10000);

			var entitlement7 = Guid.NewGuid();
			var entitlement8 = Guid.NewGuid();
			CreateReviewProposalEntitlement(TestConnection, entitlement7, proposal4, "BAS", 80000);
			CreateReviewProposalEntitlement(TestConnection, entitlement8, proposal4, "RME", 10000);

			var query = "SELECT RRE_PK from dbo.GetRelevantReviewProposalEntitlements(@pk, @managerTypes)";
			var command = TestConnection.Command(query);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, manager.PK);
			command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);

			var results = DataUtils.GetDataTableFromCommand(command).AsEnumerable();
			var expected = ExpectedProposalEntitlements(
				entitlement1,
				entitlement2,
				entitlement3,
				entitlement4,
				entitlement5,
				entitlement6,
				entitlement7,
				entitlement8);
			AssertContainsExactElementsInAnyOrder(comparator, expected, results);
		}

		public void TestTopLevelNode_GetEntitlementsFromChild()
		{
			var node = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, node, null, reviewProcess, Guid.NewGuid(), "CEO");

			var child1 = Guid.NewGuid();
			var child2 = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, child1, node, reviewProcess, manager.PK, "MG1");
			CreateReviewProcessNode(TestConnection, child2, node, reviewProcess, Guid.NewGuid(), "MG2");

			var proposal1 = Guid.NewGuid();
			var proposal2 = Guid.NewGuid();
			CreateReviewProposal(TestConnection, proposal1, child1, Guid.NewGuid(), "S01");
			CreateReviewProposal(TestConnection, proposal2, child1, Guid.NewGuid(), "S02");

			var entitlement1 = Guid.NewGuid();
			var entitlement2 = Guid.NewGuid();
			CreateReviewProposalEntitlement(TestConnection, entitlement1, proposal1, "BAS", 100000);
			CreateReviewProposalEntitlement(TestConnection, entitlement2, proposal1, "RME", 20000);

			var entitlement3 = Guid.NewGuid();
			var entitlement4 = Guid.NewGuid();
			CreateReviewProposalEntitlement(TestConnection, entitlement3, proposal2, "BAS", 100000);
			CreateReviewProposalEntitlement(TestConnection, entitlement4, proposal2, "RME", 20000);

			var proposal3 = Guid.NewGuid();
			var proposal4 = Guid.NewGuid();
			CreateReviewProposal(TestConnection, proposal3, child2, Guid.NewGuid(), "T01");
			CreateReviewProposal(TestConnection, proposal4, child2, Guid.NewGuid(), "T02");

			CreateReviewProposalEntitlement(TestConnection, Guid.NewGuid(), proposal3, "BAS", 80000);
			CreateReviewProposalEntitlement(TestConnection, Guid.NewGuid(), proposal3, "RME", 10000);

			CreateReviewProposalEntitlement(TestConnection, Guid.NewGuid(), proposal4, "BAS", 80000);
			CreateReviewProposalEntitlement(TestConnection, Guid.NewGuid(), proposal4, "RME", 10000);

			var query = "SELECT RRE_PK from dbo.GetRelevantReviewProposalEntitlements(@pk, @managerTypes)";
			var command = TestConnection.Command(query);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, manager.PK);
			command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);

			var results = DataUtils.GetDataTableFromCommand(command).AsEnumerable();
			var expected = ExpectedProposalEntitlements(
				entitlement1,
				entitlement2,
				entitlement3,
				entitlement4);
			AssertContainsExactElementsInAnyOrder(comparator, expected, results);
		}

		public void TestOneTopLevelNode_Grandchildren()
		{
			var node = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, node, null, reviewProcess, manager.PK, manager.Code);

			var child = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, child, node, reviewProcess, Guid.NewGuid(), "MMR");

			var proposal1 = Guid.NewGuid();
			CreateReviewProposal(TestConnection, proposal1, child, Guid.NewGuid(), "T01");

			var entitlement1 = Guid.NewGuid();
			CreateReviewProposalEntitlement(TestConnection, entitlement1, proposal1, "BAS", 90000);

			var grandchild1 = Guid.NewGuid();
			var grandchild2 = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, grandchild1, child, reviewProcess, Guid.NewGuid(), "SM1");
			CreateReviewProcessNode(TestConnection, grandchild2, child, reviewProcess, Guid.NewGuid(), "SM2");

			var proposal2 = Guid.NewGuid();
			CreateReviewProposal(TestConnection, proposal2, grandchild1, Guid.NewGuid(), "U01");

			var entitlement2 = Guid.NewGuid();
			var entitlement3 = Guid.NewGuid();
			var entitlement4 = Guid.NewGuid();
			CreateReviewProposalEntitlement(TestConnection, entitlement2, proposal2, "BAS", 100000);
			CreateReviewProposalEntitlement(TestConnection, entitlement3, proposal2, "RME", 20000);
			CreateReviewProposalEntitlement(TestConnection, entitlement4, proposal2, "WOW", 1000);

			var proposal3 = Guid.NewGuid();
			CreateReviewProposal(TestConnection, proposal3, grandchild2, Guid.NewGuid(), "U11");

			var entitlement5 = Guid.NewGuid();
			var entitlement6 = Guid.NewGuid();
			var entitlement7 = Guid.NewGuid();
			CreateReviewProposalEntitlement(TestConnection, entitlement5, proposal3, "BAS", 100000);
			CreateReviewProposalEntitlement(TestConnection, entitlement6, proposal3, "RME", 20000);
			CreateReviewProposalEntitlement(TestConnection, entitlement7, proposal3, "WOW", 1000);

			var query = "SELECT RRE_PK from dbo.GetRelevantReviewProposalEntitlements(@pk, @managerTypes)";
			var command = TestConnection.Command(query);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, manager.PK);
			command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);

			var results = DataUtils.GetDataTableFromCommand(command).AsEnumerable();
			var expected = ExpectedProposalEntitlements(
				entitlement1,
				entitlement2,
				entitlement3,
				entitlement4,
				entitlement5,
				entitlement6,
				entitlement7);
			AssertContainsExactElementsInAnyOrder(comparator, expected, results);
		}

		public void TestTwoTopLevelNodes()
		{
			var node1 = Guid.NewGuid();
			var node2 = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, node1, null, reviewProcess, manager.PK, "MG1");
			CreateReviewProcessNode(TestConnection, node2, null, reviewProcess, Guid.NewGuid(), "MG2");

			var proposal1 = Guid.NewGuid();
			CreateReviewProposal(TestConnection, proposal1, node1, Guid.NewGuid(), "S01");

			var entitlement1 = Guid.NewGuid();
			var entitlement2 = Guid.NewGuid();
			var entitlement3 = Guid.NewGuid();
			CreateReviewProposalEntitlement(TestConnection, entitlement1, proposal1, "BAS", 90000);
			CreateReviewProposalEntitlement(TestConnection, entitlement2, proposal1, "RME", 15000);
			CreateReviewProposalEntitlement(TestConnection, entitlement3, proposal1, "WOW", 1000);

			var proposal2 = Guid.NewGuid();
			CreateReviewProposal(TestConnection, proposal2, node2, Guid.NewGuid(), "S11");

			CreateReviewProposalEntitlement(TestConnection, Guid.NewGuid(), proposal2, "BAS", 90000);
			CreateReviewProposalEntitlement(TestConnection, Guid.NewGuid(), proposal2, "RME", 15000);
			CreateReviewProposalEntitlement(TestConnection, Guid.NewGuid(), proposal2, "WOW", 1000);

			var query = "SELECT RRE_PK from dbo.GetRelevantReviewProposalEntitlements(@pk, @managerTypes)";
			var command = TestConnection.Command(query);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, manager.PK);
			command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);

			var results = DataUtils.GetDataTableFromCommand(command).AsEnumerable();
			var expected = ExpectedProposalEntitlements(entitlement1, entitlement2, entitlement3);
			AssertContainsExactElementsInAnyOrder(comparator, expected, results);
		}

		public void TestTwoTopLevelNodes_DifferentChildrenGiveDifferentEntitlements()
		{
			var node1 = Guid.NewGuid();
			var node2 = Guid.NewGuid();
			var otherManager = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, node1, null, reviewProcess, manager.PK, "MG1");
			CreateReviewProcessNode(TestConnection, node2, null, reviewProcess, otherManager, "MG2");

			var child = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, child, node1, reviewProcess, Guid.NewGuid(), "MMR");

			var proposal1 = Guid.NewGuid();
			CreateReviewProposal(TestConnection, proposal1, node1, Guid.NewGuid(), "S01");

			var entitlement1 = Guid.NewGuid();
			var entitlement2 = Guid.NewGuid();
			var entitlement3 = Guid.NewGuid();
			CreateReviewProposalEntitlement(TestConnection, entitlement1, proposal1, "BAS", 90000);
			CreateReviewProposalEntitlement(TestConnection, entitlement2, proposal1, "RME", 15000);
			CreateReviewProposalEntitlement(TestConnection, entitlement3, proposal1, "WOW", 1000);

			var proposal2 = Guid.NewGuid();
			CreateReviewProposal(TestConnection, proposal2, node2, Guid.NewGuid(), "S11");

			var entitlement4 = Guid.NewGuid();
			var entitlement5 = Guid.NewGuid();
			var entitlement6 = Guid.NewGuid();
			CreateReviewProposalEntitlement(TestConnection, entitlement4, proposal2, "BAS", 90000);
			CreateReviewProposalEntitlement(TestConnection, entitlement5, proposal2, "RME", 15000);
			CreateReviewProposalEntitlement(TestConnection, entitlement6, proposal2, "WOW", 1000);

			var proposal3 = Guid.NewGuid();
			CreateReviewProposal(TestConnection, proposal3, child, Guid.NewGuid(), "T01");

			var entitlement7 = Guid.NewGuid();
			var entitlement8 = Guid.NewGuid();
			CreateReviewProposalEntitlement(TestConnection, entitlement7, proposal3, "RME", 15000);
			CreateReviewProposalEntitlement(TestConnection, entitlement8, proposal3, "WOW", 1000);

			var query = "SELECT RRE_PK from dbo.GetRelevantReviewProposalEntitlements(@pk, @managerTypes)";
			var command = TestConnection.Command(query);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, manager.PK);
			command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);

			var results = DataUtils.GetDataTableFromCommand(command).AsEnumerable();
			var expected = ExpectedProposalEntitlements(entitlement1, entitlement2, entitlement3, entitlement7, entitlement8);
			AssertContainsExactElementsInAnyOrder(comparator, expected, results);

			command.RemoveParameterIfExists("@pk");
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, otherManager);

			results = DataUtils.GetDataTableFromCommand(command).AsEnumerable();
			expected = ExpectedProposalEntitlements(entitlement4, entitlement5, entitlement6);
			AssertContainsExactElementsInAnyOrder(comparator, expected, results);
		}

		public void TestMultipleReviewProcesses()
		{
			var node1 = Guid.NewGuid();
			var node2 = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, node1, null, reviewProcess, manager.PK, manager.Code);
			CreateReviewProcessNode(TestConnection, node2, null, reviewProcess, Guid.NewGuid(), "MG1");

			var proposal1 = Guid.NewGuid();
			var staff1 = Guid.NewGuid();
			CreateReviewProposal(TestConnection, proposal1, node1, staff1, "S01");

			var entitlement1 = Guid.NewGuid();
			var entitlement2 = Guid.NewGuid();
			var entitlement3 = Guid.NewGuid();
			CreateReviewProposalEntitlement(TestConnection, entitlement1, proposal1, "BAS", 90000);
			CreateReviewProposalEntitlement(TestConnection, entitlement2, proposal1, "RME", 15000);
			CreateReviewProposalEntitlement(TestConnection, entitlement3, proposal1, "WOW", 1000);

			var proposal2 = Guid.NewGuid();
			var staff2 = Guid.NewGuid();
			CreateReviewProposal(TestConnection, proposal2, node2, staff2, "S11");

			CreateReviewProposalEntitlement(TestConnection, Guid.NewGuid(), proposal2, "BAS", 90000);
			CreateReviewProposalEntitlement(TestConnection, Guid.NewGuid(), proposal2, "RME", 15000);
			CreateReviewProposalEntitlement(TestConnection, Guid.NewGuid(), proposal2, "WOW", 1000);

			var anotherReview = Guid.NewGuid();
			CreateReviewProcess(TestConnection, anotherReview, "More salaries to cut");

			var anotherNode1 = Guid.NewGuid();
			var anotherNode2 = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, anotherNode1, null, anotherReview, manager.PK, manager.Code);
			CreateReviewProcessNode(TestConnection, anotherNode2, null, anotherReview, Guid.NewGuid(), "MG2");

			var anotherProposal1 = Guid.NewGuid();
			CreateReviewProposal(TestConnection, anotherProposal1, anotherNode1, staff1, "S01");

			var anotherEntitlement1 = Guid.NewGuid();
			var anotherEntitlement2 = Guid.NewGuid();
			CreateReviewProposalEntitlement(TestConnection, anotherEntitlement1, anotherProposal1, "BAS", 90000);
			CreateReviewProposalEntitlement(TestConnection, anotherEntitlement2, anotherProposal1, "RME", 15000);

			var anotherProposal2 = Guid.NewGuid();
			CreateReviewProposal(TestConnection, anotherProposal2, anotherNode2, staff2, "S11");

			CreateReviewProposalEntitlement(TestConnection, Guid.NewGuid(), anotherProposal2, "BAS", 90000);
			CreateReviewProposalEntitlement(TestConnection, Guid.NewGuid(), anotherProposal2, "RME", 15000);

			var query = "SELECT RRE_PK from dbo.GetRelevantReviewProposalEntitlements(@pk, @managerTypes)";
			var command = TestConnection.Command(query);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, manager.PK);
			command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);

			var results = DataUtils.GetDataTableFromCommand(command).AsEnumerable();
			var expected = ExpectedProposalEntitlements(entitlement1, entitlement2, entitlement3, anotherEntitlement1, anotherEntitlement2);
			AssertContainsExactElementsInAnyOrder(comparator, expected, results);
		}

		public void TestPrimaryHierarchy_IsManaged1()
		{
			var node = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, node, null, reviewProcess, manager.PK, manager.Code);

			var proposal = Guid.NewGuid();
			CreateReviewProposal(TestConnection, proposal, node, Guid.NewGuid(), "S01");
			CreateReviewProposalEntitlement(TestConnection, Guid.NewGuid(), proposal, "BAS", 300000);

			var query = "SELECT * from dbo.GetRelevantReviewProposalEntitlements(@pk, @managerTypes)";
			var command = TestConnection.Command(query);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, manager.PK);
			command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);

			var results = DataUtils.GetDataTableFromCommand(command);
			Assert(results.Columns.Contains("IsManaged1"));

			var entitlement = results.Rows[0];
			AssertEquals(true, entitlement["IsManaged1"]);
			AssertEquals(false, entitlement["IsManaged2"]);
			AssertEquals(false, entitlement["IsManaged3"]);
		}

		public void TestPrimaryHierarchy_IsManaged2()
		{
			managerTypes = "REM,DRM,LAP";

			var node = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, node, null, reviewProcess, manager.PK, manager.Code);

			var proposal = Guid.NewGuid();
			CreateReviewProposal(TestConnection, proposal, node, Guid.NewGuid(), "S01");
			CreateReviewProposalEntitlement(TestConnection, Guid.NewGuid(), proposal, "BAS", 300000);

			var query = "SELECT * from dbo.GetRelevantReviewProposalEntitlements(@pk, @managerTypes)";
			var command = TestConnection.Command(query);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, manager.PK);
			command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);

			var results = DataUtils.GetDataTableFromCommand(command);
			Assert(results.Columns.Contains("IsManaged2"));

			var entitlement = results.Rows[0];
			AssertEquals(false, entitlement["IsManaged1"]);
			AssertEquals(true, entitlement["IsManaged2"]);
			AssertEquals(false, entitlement["IsManaged3"]);
		}

		public void TestPrimaryHierarchy_IsManaged3()
		{
			managerTypes = "REM,LAP,DRM";

			var node = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, node, null, reviewProcess, manager.PK, manager.Code);

			var proposal = Guid.NewGuid();
			CreateReviewProposal(TestConnection, proposal, node, Guid.NewGuid(), "S01");
			CreateReviewProposalEntitlement(TestConnection, Guid.NewGuid(), proposal, "BAS", 300000);

			var query = "SELECT * from dbo.GetRelevantReviewProposalEntitlements(@pk, @managerTypes)";
			var command = TestConnection.Command(query);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, manager.PK);
			command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);

			var results = DataUtils.GetDataTableFromCommand(command);
			Assert(results.Columns.Contains("IsManaged3"));

			var entitlement = results.Rows[0];
			AssertEquals(false, entitlement["IsManaged1"]);
			AssertEquals(false, entitlement["IsManaged2"]);
			AssertEquals(true, entitlement["IsManaged3"]);
		}

		public void TestOverrideHierarchy_IsManaged1()
		{
			managerTypes = "REM,DRM,LAP";
			SetReviewProcessOverrideHierarchy(TestConnection, reviewProcess, "REM");

			var node = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, node, null, reviewProcess, manager.PK, manager.Code);

			var proposal = Guid.NewGuid();
			CreateReviewProposal(TestConnection, proposal, node, Guid.NewGuid(), "S01");
			CreateReviewProposalEntitlement(TestConnection, Guid.NewGuid(), proposal, "BAS", 300000);

			var query = "SELECT * from dbo.GetRelevantReviewProposalEntitlements(@pk, @managerTypes)";
			var command = TestConnection.Command(query);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, manager.PK);
			command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);

			var results = DataUtils.GetDataTableFromCommand(command);
			Assert(results.Columns.Contains("IsManaged1"));

			var entitlement = results.Rows[0];
			AssertEquals(true, entitlement["IsManaged1"]);
			AssertEquals(true, entitlement["IsManaged2"]);
			AssertEquals(false, entitlement["IsManaged3"]);
		}

		public void TestOverrideHierarchy_IsManaged2()
		{
			SetReviewProcessOverrideHierarchy(TestConnection, reviewProcess, "REM");

			var node = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, node, null, reviewProcess, manager.PK, manager.Code);

			var proposal = Guid.NewGuid();
			CreateReviewProposal(TestConnection, proposal, node, Guid.NewGuid(), "S01");
			CreateReviewProposalEntitlement(TestConnection, Guid.NewGuid(), proposal, "BAS", 300000);

			var query = "SELECT * from dbo.GetRelevantReviewProposalEntitlements(@pk, @managerTypes)";
			var command = TestConnection.Command(query);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, manager.PK);
			command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);

			var results = DataUtils.GetDataTableFromCommand(command);
			Assert(results.Columns.Contains("IsManaged2"));

			var entitlement = results.Rows[0];
			AssertEquals(true, entitlement["IsManaged1"]);
			AssertEquals(true, entitlement["IsManaged2"]);
			AssertEquals(false, entitlement["IsManaged3"]);
		}

		public void TestOverrideHierarchy_IsManaged3()
		{
			managerTypes = "DRM,LAP,REM";
			SetReviewProcessOverrideHierarchy(TestConnection, reviewProcess, "REM");

			var node = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, node, null, reviewProcess, manager.PK, manager.Code);

			var proposal = Guid.NewGuid();
			CreateReviewProposal(TestConnection, proposal, node, Guid.NewGuid(), "S01");
			CreateReviewProposalEntitlement(TestConnection, Guid.NewGuid(), proposal, "BAS", 300000);

			var query = "SELECT * from dbo.GetRelevantReviewProposalEntitlements(@pk, @managerTypes)";
			var command = TestConnection.Command(query);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, manager.PK);
			command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);

			var results = DataUtils.GetDataTableFromCommand(command);
			Assert(results.Columns.Contains("IsManaged3"));

			var entitlement = results.Rows[0];
			AssertEquals(true, entitlement["IsManaged1"]);
			AssertEquals(false, entitlement["IsManaged2"]);
			AssertEquals(true, entitlement["IsManaged3"]);
		}

		IEnumerable<DataRow> ExpectedProposalEntitlements(params Guid[] pks)
		{
			var expected = new DataTable();
			expected.Columns.Add(new DataColumn("RRE_PK", typeof(Guid)));

			foreach (var pk in pks)
			{
				var row = expected.NewRow();
				row["RRE_PK"] = pk;
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
