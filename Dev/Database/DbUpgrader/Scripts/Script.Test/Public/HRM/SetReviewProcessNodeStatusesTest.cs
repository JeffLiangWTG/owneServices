using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.HRM;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.HRM
{
	[TestedType(typeof(SetReviewProcessNodeStatuses))]
	class SetReviewProcessNodeStatusesTest : DbCreateScriptTest
	{
		readonly IEqualityComparer<DataRow> comparator = DataRowComparer.Default;

		Guid reviewProcess;
		(Guid PK, string Code) manager;

		public void TestTopLevel_NoChildren()
		{
			var node = Guid.NewGuid();
			CreateReviewProcessNode(node, null, reviewProcess, manager.PK, manager.Code);

			CombineAssertions(() => {
				TestCase("APP");
				TestCase("SUB");
			});

			void TestCase(string status)
			{
				var command = TestConnection.Command($"SetReviewProcessNodeStatuses '{node}', '{status}', 'E'");
				_ = command.ExecuteNonQuery();

				var expected = new[]
				{
					(node, status),
				};

				var query = "SELECT RRN_PK, RRN_Status from dbo.ReviewProcessNode";
				command = TestConnection.Command(query);
				var results = DataUtils.GetDataTableFromCommand(command).AsEnumerable();
				AssertContainsExactElementsInAnyOrder(comparator, SerialiseExpected(expected), results);
			}
		}

		public void TestTopLevel_Child()
		{
			var node = Guid.NewGuid();
			CreateReviewProcessNode(node, null, reviewProcess, manager.PK, manager.Code);

			var child = Guid.NewGuid();
			CreateReviewProcessNode(child, node, reviewProcess, Guid.NewGuid(), "MM1");

			CombineAssertions(() => {
				TestCase("APP");
				TestCase("SUB");
			});

			void TestCase(string status)
			{
				var command = TestConnection.Command($"SetReviewProcessNodeStatuses '{node}', '{status}', 'E'");
				_ = command.ExecuteNonQuery();

				var expected = new[]
				{
					(node, status),
					(child, status),
				};

				var query = "SELECT RRN_PK, RRN_Status from dbo.ReviewProcessNode";
				command = TestConnection.Command(query);
				var results = DataUtils.GetDataTableFromCommand(command).AsEnumerable();
				AssertContainsExactElementsInAnyOrder(comparator, SerialiseExpected(expected), results);
			}
		}

		public void TestTopLevel_MultipleChildren()
		{
			var node = Guid.NewGuid();
			CreateReviewProcessNode(node, null, reviewProcess, manager.PK, manager.Code);

			var child1 = Guid.NewGuid();
			CreateReviewProcessNode(child1, node, reviewProcess, Guid.NewGuid(), "MM1");

			var child2 = Guid.NewGuid();
			CreateReviewProcessNode(child2, node, reviewProcess, Guid.NewGuid(), "MM2");

			var grandchild = Guid.NewGuid();
			CreateReviewProcessNode(grandchild, child1, reviewProcess, Guid.NewGuid(), "SML");

			CombineAssertions(() => {
				TestCase("APP");
				TestCase("SUB");
			});

			void TestCase(string status)
			{
				var command = TestConnection.Command($"SetReviewProcessNodeStatuses '{node}', '{status}', 'E'");
				_ = command.ExecuteNonQuery();

				var expected = new[]
				{
					(node, status),
					(child1, status),
					(child2, status),
					(grandchild, status),
				};

				var query = "SELECT RRN_PK, RRN_Status from dbo.ReviewProcessNode";
				command = TestConnection.Command(query);
				var results = DataUtils.GetDataTableFromCommand(command).AsEnumerable();
				AssertContainsExactElementsInAnyOrder(comparator, SerialiseExpected(expected), results);
			}
		}

		public void TestHasParent()
		{
			var parent = Guid.NewGuid();
			CreateReviewProcessNode(parent, null, reviewProcess, Guid.NewGuid(), "SAD");

			var node = Guid.NewGuid();
			CreateReviewProcessNode(node, parent, reviewProcess, manager.PK, manager.Code);

			CombineAssertions(() => {
				TestCase("APP");
				TestCase("SUB");
			});

			void TestCase(string status)
			{
				var command = TestConnection.Command($"SetReviewProcessNodeStatuses '{node}', '{status}', 'E'");
				_ = command.ExecuteNonQuery();

				var expected = new[]
				{
					(node, status),
					(parent, "ASN"),
				};

				var query = "SELECT RRN_PK, RRN_Status from dbo.ReviewProcessNode";
				command = TestConnection.Command(query);
				var results = DataUtils.GetDataTableFromCommand(command);
				AssertContainsExactElementsInAnyOrder(comparator, SerialiseExpected(expected), results.AsEnumerable());
			}
		}

		public void TestUnrelatedNode()
		{
			var node = Guid.NewGuid();
			CreateReviewProcessNode(node, null, reviewProcess, manager.PK, manager.Code);

			var other = Guid.NewGuid();
			CreateReviewProcessNode(other, null, reviewProcess, Guid.NewGuid(), "MM1");

			CombineAssertions(() => {
				TestCase("APP");
				TestCase("SUB");
			});

			void TestCase(string status)
			{
				var command = TestConnection.Command($"SetReviewProcessNodeStatuses '{node}', '{status}', 'E'");
				_ = command.ExecuteNonQuery();

				var expected = new[]
				{
					(node, status),
					(other, "ASN"),
				};

				var query = "SELECT RRN_PK, RRN_Status from dbo.ReviewProcessNode";
				command = TestConnection.Command(query);
				var results = DataUtils.GetDataTableFromCommand(command).AsEnumerable();
				AssertContainsExactElementsInAnyOrder(comparator, SerialiseExpected(expected), results);
			}
		}

		public void TestMultipleChildren_MultipleUnrelated()
		{
			var node = Guid.NewGuid();
			CreateReviewProcessNode(node, null, reviewProcess, manager.PK, manager.Code);

			var child1 = Guid.NewGuid();
			CreateReviewProcessNode(child1, node, reviewProcess, Guid.NewGuid(), "MM1");

			var child2 = Guid.NewGuid();
			CreateReviewProcessNode(child2, node, reviewProcess, Guid.NewGuid(), "MM2");

			var grandchild = Guid.NewGuid();
			CreateReviewProcessNode(grandchild, child1, reviewProcess, Guid.NewGuid(), "SML");

			var other = Guid.NewGuid();
			CreateReviewProcessNode(other, null, reviewProcess, Guid.NewGuid(), "OTH");

			var otherChild = Guid.NewGuid();
			CreateReviewProcessNode(otherChild, other, reviewProcess, Guid.NewGuid(), "OTC");

			CombineAssertions(() => {
				TestCase("APP");
				TestCase("SUB");
			});

			void TestCase(string status)
			{
				var command = TestConnection.Command($"SetReviewProcessNodeStatuses '{node}', '{status}', 'E'");
				_ = command.ExecuteNonQuery();

				var expected = new[]
				{
					(node, status),
					(child1, status),
					(child2, status),
					(grandchild, status),
					(other, "ASN"),
					(otherChild, "ASN"),
				};

				var query = "SELECT RRN_PK, RRN_Status from dbo.ReviewProcessNode";
				command = TestConnection.Command(query);
				var results = DataUtils.GetDataTableFromCommand(command).AsEnumerable();
				AssertContainsExactElementsInAnyOrder(comparator, SerialiseExpected(expected), results);
			}
		}

		public void TestDoesntChangeFinalisedNodes()
		{
			var top = new Guid("00000000-0000-0000-0000-000000000001");
			var submittedChild = new Guid("00000000-0000-0000-0000-000000000002");
			var finalisedDirectDescendant = new Guid("00000000-0000-0000-0000-000000000003");
			var finalisedIndirectDescendant = new Guid("00000000-0000-0000-0000-000000000004");

			CreateReviewProcessNode(top, null, reviewProcess, Guid.NewGuid(), "AA1", status: "ASN");
			CreateReviewProcessNode(finalisedDirectDescendant, top, reviewProcess, Guid.NewGuid(), "AA2", status: "FIN");
			CreateReviewProcessNode(submittedChild, top, reviewProcess, Guid.NewGuid(), "AA3", status: "SUB");
			CreateReviewProcessNode(finalisedIndirectDescendant, submittedChild, reviewProcess, Guid.NewGuid(), "AA4", status: "FIN");

			TestConnection.ExecuteNonQuery("SetReviewProcessNodeStatuses @node, 'APP', 'E'", p => p.AddParameterBasedOnDbColumn("@node", top, ReviewProcessNodeSchema.PK));

			var expected = new[]
			{
				(top, "APP"),
				(submittedChild, "APP"),
				(finalisedDirectDescendant, "FIN"),
				(finalisedIndirectDescendant, "FIN")
			};

			var actual = new List<(Guid pk, string status)>(expected.Length);
			TestConnection.ExecuteReader("SELECT RRN_PK, RRN_Status FROM dbo.ReviewProcessNode", row => actual.Add((row.GetGuid(0), row.GetString(1))));

			AssertContainsExactElementsInAnyOrder(expected, actual);
		}

		public void TestNodeDoesNotExist()
		{
			var node = Guid.NewGuid();
			CreateReviewProcessNode(node, null, reviewProcess, manager.PK, manager.Code);

			var command = TestConnection.Command($"SetReviewProcessNodeStatuses '{Guid.NewGuid()}', 'APP', 'E'");
			_ = command.ExecuteNonQuery();

			var expected = new[]
			{
				(node, "ASN"),
			};

			var query = "SELECT RRN_PK, RRN_Status from dbo.ReviewProcessNode";
			command = TestConnection.Command(query);
			var results = DataUtils.GetDataTableFromCommand(command).AsEnumerable();
			AssertContainsExactElementsInAnyOrder(comparator, SerialiseExpected(expected), results);
		}

		public void TestStatusNotValid()
		{
			var node = Guid.NewGuid();
			CreateReviewProcessNode(node, null, reviewProcess, manager.PK, manager.Code);

			var command = TestConnection.Command($"SetReviewProcessNodeStatuses '{node}', 'WOO', 'E'");
			_ = command.ExecuteNonQuery();

			var expected = new[]
			{
				(node, "ASN"),
			};

			var query = "SELECT RRN_PK, RRN_Status from dbo.ReviewProcessNode";
			command = TestConnection.Command(query);
			var results = DataUtils.GetDataTableFromCommand(command).AsEnumerable();
			AssertContainsExactElementsInAnyOrder(comparator, SerialiseExpected(expected), results);
		}

		void CreateReviewProcessNode(Guid pk, Guid? parent, Guid reviewProcess, Guid reviewer, string revCode, string status = "ASN")
		{
			var query = $@"
INSERT INTO dbo.GlbStaff
	(GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
VALUES
	(@staffPK, @code, @code, GETUTCDATE(), 'E', GETUTCDATE(), 'E');

INSERT INTO dbo.ReviewProcessNode
	(RRN_PK, RRN_RRN_Parent, RRN_RPR_ReviewProcess, RRN_GS_Reviewer, RRN_Status, RRN_SystemCreateTimeUtc, RRN_SystemCreateUser, RRN_SystemLastEditTimeUtc, RRN_SystemLastEditUser)
VALUES
	(@pk, @parent, @reviewProcess, @reviewer, @status, GETUTCDATE(), 'E', GETUTCDATE(), 'E')";

			_ = TestConnection.ExecuteNonQuery(query, p =>
			{
				p.AddParameterBasedOnDbColumn("@staffPK", reviewer, GlbStaffSchema.PK);
				p.AddParameterBasedOnDbColumn("@code", revCode, GlbStaffSchema.GS_Code);

				p.AddParameterBasedOnDbColumn("@pk", pk, ReviewProcessNodeSchema.PK);
				p.AddParameterBasedOnDbColumn("@reviewProcess", reviewProcess, ReviewProcessNodeSchema.RRN_RPR_ReviewProcess);
				p.AddParameterBasedOnDbColumn("@reviewer", reviewer, ReviewProcessNodeSchema.RRN_GS_Reviewer);
				p.AddParameterBasedOnDbColumn("@status", status, ReviewProcessNodeSchema.RRN_Status);
				p.AddParameterBasedOnDbColumn("@parent", parent ?? (object)DBNull.Value, ReviewProcessNodeSchema.RRN_RRN_Parent);
			});
		}

		EnumerableRowCollection<DataRow> SerialiseExpected(IEnumerable<(Guid node, string status)> records)
		{
			var expected = new DataTable();
			expected.Columns.Add(new DataColumn("RRN_PK", typeof(Guid)));
			expected.Columns.Add(new DataColumn("RRN_Status", typeof(string)));

			foreach (var (node, status) in records)
			{
				var row = expected.NewRow();
				row["RRN_PK"] = node;
				row["RRN_Status"] = status;
				expected.Rows.Add(row);
			}

			return expected.AsEnumerable();
		}

		protected override void SetUp()
		{
			reviewProcess = Guid.NewGuid();
			var query = $@"
INSERT INTO dbo.ReviewProcess
	(RPR_PK, RPR_Name, RPR_Type, RPR_ConfigType, RPR_EffectiveDate, RPR_SubmissionDate, RPR_RX_NKCurrency, RPR_SystemCreateTimeUtc, RPR_SystemCreateUser, RPR_SystemLastEditTimeUtc, RPR_SystemLastEditUser)
VALUES
	(@pk, @name, 'PER', '', GETUTCDATE(), GETUTCDATE(), '', GETUTCDATE(), 'E', GETUTCDATE(), 'E')";

			_ = TestConnection.ExecuteNonQuery(query, p =>
			{
				p.AddParameter("@pk", SqlDbType.UniqueIdentifier, reviewProcess);
				p.AddParameter("@name", SqlDbType.NVarChar, 256, "Whose pay should we cut");
			});

			manager = (PK: Guid.NewGuid(), Code: "MGR");

			base.SetUp();
		}
	}
}
