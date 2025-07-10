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
	[TestedType(typeof(DescendantReviewProcessNodes))]
	class DescendantReviewProcessNodesTest : DbCreateScriptTest
	{
		readonly IEqualityComparer<DataRow> comparator = DataRowComparer.Default;

		Guid reviewProcess;
		(Guid PK, string Code) manager;
		readonly string managerTypes = "DRM,REM,PL";

		public void TestNoParent_NoChildren()
		{
			var node = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, node, null, reviewProcess, manager.PK, manager.Code);

			var query = "SELECT RRN_PK from dbo.DescendantReviewProcessNodes(@pk, @managerTypes)";
			var command = TestConnection.Command(query);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, node);
			command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);

			var results = DataUtils.GetDataTableFromCommand(command).AsEnumerable();
			AssertContainsExactElementsInAnyOrder(comparator, Enumerable.Empty<DataRow>(), results);
		}

		public void TestNoParent_OneChild()
		{
			var node = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, node, null, reviewProcess, manager.PK, manager.Code);

			var child = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, child, node, reviewProcess, Guid.NewGuid(), "SUB");

			var query = "SELECT RRN_PK from dbo.DescendantReviewProcessNodes(@pk, @managerTypes)";
			var command = TestConnection.Command(query);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, node);
			command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);

			var results = DataUtils.GetDataTableFromCommand(command).AsEnumerable();
			AssertContainsExactElementsInAnyOrder(comparator, ExpectedNodes(child), results);
		}

		public void TestNoParent_ChildAndGrandchild()
		{
			var node = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, node, null, reviewProcess, manager.PK, manager.Code);

			var child = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, child, node, reviewProcess, Guid.NewGuid(), "MMG");

			var grandchild = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, grandchild, child, reviewProcess, Guid.NewGuid(), "SUB");

			var query = "SELECT RRN_PK from dbo.DescendantReviewProcessNodes(@pk, @managerTypes)";
			var command = TestConnection.Command(query);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, node);
			command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);

			var results = DataUtils.GetDataTableFromCommand(command).AsEnumerable();
			AssertContainsExactElementsInAnyOrder(comparator, ExpectedNodes(child, grandchild), results);
		}

		public void TestNoParent_ManyChildren()
		{
			var node = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, node, null, reviewProcess, manager.PK, manager.Code);

			var child1 = Guid.NewGuid();
			var child2 = Guid.NewGuid();
			var child3 = Guid.NewGuid();
			var child4 = Guid.NewGuid();

			CreateReviewProcessNode(TestConnection, child1, node, reviewProcess, Guid.NewGuid(), "SU1");
			CreateReviewProcessNode(TestConnection, child2, node, reviewProcess, Guid.NewGuid(), "SU2");
			CreateReviewProcessNode(TestConnection, child3, node, reviewProcess, Guid.NewGuid(), "SU3");
			CreateReviewProcessNode(TestConnection, child4, node, reviewProcess, Guid.NewGuid(), "SU4");

			var query = "SELECT RRN_PK from dbo.DescendantReviewProcessNodes(@pk, @managerTypes)";
			var command = TestConnection.Command(query);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, node);
			command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);

			var results = DataUtils.GetDataTableFromCommand(command).AsEnumerable();
			AssertContainsExactElementsInAnyOrder(comparator, ExpectedNodes(child1, child2, child3, child4), results);
		}

		public void TestNoParent_ManyChildrenAndGrandchildren()
		{
			var node = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, node, null, reviewProcess, manager.PK, manager.Code);

			var child1 = Guid.NewGuid();
			var child2 = Guid.NewGuid();
			var child3 = Guid.NewGuid();
			var child4 = Guid.NewGuid();

			CreateReviewProcessNode(TestConnection, child1, node, reviewProcess, Guid.NewGuid(), "MM1");
			CreateReviewProcessNode(TestConnection, child2, node, reviewProcess, Guid.NewGuid(), "MM2");
			CreateReviewProcessNode(TestConnection, child3, node, reviewProcess, Guid.NewGuid(), "MM3");
			CreateReviewProcessNode(TestConnection, child4, node, reviewProcess, Guid.NewGuid(), "MM4");

			var grandchild1 = Guid.NewGuid();
			var grandchild2 = Guid.NewGuid();
			var grandchild3 = Guid.NewGuid();
			var grandchild4 = Guid.NewGuid();

			CreateReviewProcessNode(TestConnection, grandchild1, child1, reviewProcess, Guid.NewGuid(), "SU1");
			CreateReviewProcessNode(TestConnection, grandchild2, child2, reviewProcess, Guid.NewGuid(), "SU2");
			CreateReviewProcessNode(TestConnection, grandchild3, child2, reviewProcess, Guid.NewGuid(), "SU3");
			CreateReviewProcessNode(TestConnection, grandchild4, child4, reviewProcess, Guid.NewGuid(), "SU4");

			var query = "SELECT RRN_PK from dbo.DescendantReviewProcessNodes(@pk, @managerTypes)";
			var command = TestConnection.Command(query);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, node);
			command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);

			var results = DataUtils.GetDataTableFromCommand(command).AsEnumerable();

			var expected = ExpectedNodes(child1, child2, child3, child4, grandchild1, grandchild2, grandchild3, grandchild4);
			AssertContainsExactElementsInAnyOrder(comparator, expected, results);
		}

		public void TestNoParent_ChildAndSibling()
		{
			var node = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, node, null, reviewProcess, manager.PK, manager.Code);

			var child = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, child, node, reviewProcess, Guid.NewGuid(), "SUB");
			CreateReviewProcessNode(TestConnection, Guid.NewGuid(), null, reviewProcess, Guid.NewGuid(), "OTH");

			var query = "SELECT RRN_PK from dbo.DescendantReviewProcessNodes(@pk, @managerTypes)";
			var command = TestConnection.Command(query);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, node);
			command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);

			var results = DataUtils.GetDataTableFromCommand(command).AsEnumerable();
			AssertContainsExactElementsInAnyOrder(comparator, ExpectedNodes(child), results);
		}

		public void TestNoParent_ChildSiblingAndNiece()
		{
			var node = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, node, null, reviewProcess, manager.PK, manager.Code);

			var child = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, child, node, reviewProcess, Guid.NewGuid(), "SUB");

			var sibling = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, sibling, null, reviewProcess, Guid.NewGuid(), "OMG");
			CreateReviewProcessNode(TestConnection, Guid.NewGuid(), sibling, reviewProcess, Guid.NewGuid(), "OSU");

			var query = "SELECT RRN_PK from dbo.DescendantReviewProcessNodes(@pk, @managerTypes)";
			var command = TestConnection.Command(query);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, node);
			command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);

			var results = DataUtils.GetDataTableFromCommand(command).AsEnumerable();
			AssertContainsExactElementsInAnyOrder(comparator, ExpectedNodes(child), results);
		}

		public void TestHasParent_NoChildren()
		{
			var parent = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, parent, null, reviewProcess, Guid.NewGuid(), "CEO");

			var node = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, node, parent, reviewProcess, manager.PK, manager.Code);

			var query = "SELECT RRN_PK from dbo.DescendantReviewProcessNodes(@pk, @managerTypes)";
			var command = TestConnection.Command(query);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, node);
			command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);

			var results = DataUtils.GetDataTableFromCommand(command).AsEnumerable();
			AssertContainsExactElementsInAnyOrder(comparator, Enumerable.Empty<DataRow>(), results);
		}

		public void TestHasParent_OneChild()
		{
			var parent = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, parent, null, reviewProcess, Guid.NewGuid(), "CEO");

			var node = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, node, parent, reviewProcess, manager.PK, manager.Code);

			var child = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, child, node, reviewProcess, Guid.NewGuid(), "SUB");

			var query = "SELECT RRN_PK from dbo.DescendantReviewProcessNodes(@pk, @managerTypes)";
			var command = TestConnection.Command(query);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, node);
			command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);

			var results = DataUtils.GetDataTableFromCommand(command).AsEnumerable();
			AssertContainsExactElementsInAnyOrder(comparator, ExpectedNodes(child), results);
		}

		public void TestHasParent_ChildAndGrandchild()
		{
			var parent = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, parent, null, reviewProcess, Guid.NewGuid(), "CEO");

			var node = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, node, parent, reviewProcess, manager.PK, manager.Code);

			var child = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, child, node, reviewProcess, Guid.NewGuid(), "MMG");

			var grandchild = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, grandchild, child, reviewProcess, Guid.NewGuid(), "SUB");

			var query = "SELECT RRN_PK from dbo.DescendantReviewProcessNodes(@pk, @managerTypes)";
			var command = TestConnection.Command(query);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, node);
			command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);

			var results = DataUtils.GetDataTableFromCommand(command).AsEnumerable();
			AssertContainsExactElementsInAnyOrder(comparator, ExpectedNodes(child, grandchild), results);
		}

		public void TestHasParent_ManyChildren()
		{
			var parent = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, parent, null, reviewProcess, Guid.NewGuid(), "CEO");

			var node = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, node, parent, reviewProcess, manager.PK, manager.Code);

			var child1 = Guid.NewGuid();
			var child2 = Guid.NewGuid();
			var child3 = Guid.NewGuid();
			var child4 = Guid.NewGuid();

			CreateReviewProcessNode(TestConnection, child1, node, reviewProcess, Guid.NewGuid(), "SU1");
			CreateReviewProcessNode(TestConnection, child2, node, reviewProcess, Guid.NewGuid(), "SU2");
			CreateReviewProcessNode(TestConnection, child3, node, reviewProcess, Guid.NewGuid(), "SU3");
			CreateReviewProcessNode(TestConnection, child4, node, reviewProcess, Guid.NewGuid(), "SU4");

			var query = "SELECT RRN_PK from dbo.DescendantReviewProcessNodes(@pk, @managerTypes)";
			var command = TestConnection.Command(query);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, node);
			command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);

			var results = DataUtils.GetDataTableFromCommand(command).AsEnumerable();
			AssertContainsExactElementsInAnyOrder(comparator, ExpectedNodes(child1, child2, child3, child4), results);
		}

		public void TestHasParent_ManyChildrenOneGrandchild()
		{
			var parent = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, parent, null, reviewProcess, Guid.NewGuid(), "CEO");

			var node = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, node, parent, reviewProcess, manager.PK, manager.Code);

			var child1 = Guid.NewGuid();
			var child2 = Guid.NewGuid();
			var grandchild = Guid.NewGuid();

			CreateReviewProcessNode(TestConnection, child1, node, reviewProcess, Guid.NewGuid(), "MM1");
			CreateReviewProcessNode(TestConnection, child2, node, reviewProcess, Guid.NewGuid(), "MM2");
			CreateReviewProcessNode(TestConnection, grandchild, child1, reviewProcess, Guid.NewGuid(), "SUB");

			var query = "SELECT RRN_PK from dbo.DescendantReviewProcessNodes(@pk, @managerTypes)";
			var command = TestConnection.Command(query);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, node);
			command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);

			var results = DataUtils.GetDataTableFromCommand(command).AsEnumerable();

			AssertContainsExactElementsInAnyOrder(comparator, ExpectedNodes(child1, child2, grandchild), results);
		}

		public void TestHasParent_ChildAuntAndCousin()
		{
			var parent = Guid.NewGuid();
			var aunt = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, parent, null, reviewProcess, Guid.NewGuid(), "CEO");
			CreateReviewProcessNode(TestConnection, aunt, null, reviewProcess, Guid.NewGuid(), "CTO");

			var node = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, node, parent, reviewProcess, manager.PK, manager.Code);
			CreateReviewProcessNode(TestConnection, Guid.NewGuid(), aunt, reviewProcess, Guid.NewGuid(), "OTH");

			var child = Guid.NewGuid();
			CreateReviewProcessNode(TestConnection, child, node, reviewProcess, Guid.NewGuid(), "MMG");

			var query = "SELECT RRN_PK from dbo.DescendantReviewProcessNodes(@pk, @managerTypes)";
			var command = TestConnection.Command(query);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, node);
			command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);

			var results = DataUtils.GetDataTableFromCommand(command).AsEnumerable();
			AssertContainsExactElementsInAnyOrder(comparator, ExpectedNodes(child), results);
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

			base.SetUp();
		}
	}
}
