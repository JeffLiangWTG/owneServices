using System;
using System.Text;
using CargoWise.DbUpgrader.Scripts.Definitions.BufferManagement;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BufferManagement.Testing
{
	[TestedType(typeof(GetWorkQueueTagLinks))]
	sealed class GetWorkQueueProcessHeadersTest : DbCreateScriptTest
	{
		public void TestQuery_NoWorkflows_ShouldNotIncludeJobHeaders()
		{
			var job1PK = Guid.NewGuid();
			var job2PK = Guid.NewGuid();
			var job3PK = Guid.NewGuid();
			var jobHeader1PK = Guid.NewGuid();
			var jobHeader2PK = Guid.NewGuid();
			var jobHeader3PK = Guid.NewGuid();

			var workQueueGroupPK = BMDbTestHelper.GetWorkQueueTagGroupPK(TestConnection);
			var workQueueTagPK = Guid.NewGuid();

			var insertSql = new StringBuilder();

			insertSql.AppendFormat(BMDbTestHelper.ProcessJobHeaderInsertSql, jobHeader1PK, job1PK);
			insertSql.AppendFormat(BMDbTestHelper.ProcessJobHeaderInsertSql, jobHeader2PK, job2PK);
			insertSql.AppendFormat(BMDbTestHelper.ProcessJobHeaderInsertSql, jobHeader3PK, job3PK);

			insertSql.AppendFormat(BMDbTestHelper.TagMagnitudeWithDescriptionInsertSql, workQueueTagPK, "AAA", workQueueGroupPK, "Case-sensicrepency");

			insertSql.Append(BMDbTestHelper.GetTagLinkInsertSql(Guid.NewGuid(), workQueueTagPK, jobHeader1PK, 0));
			insertSql.Append(BMDbTestHelper.GetTagLinkInsertSql(Guid.NewGuid(), workQueueTagPK, jobHeader2PK, 1));
			insertSql.Append(BMDbTestHelper.GetTagLinkInsertSql(Guid.NewGuid(), workQueueTagPK, jobHeader3PK, 2));

			TestConnection.ExecuteNonQuery(insertSql.ToString());

			AssertEquals("The function only returns job headers where they have at least one open workflow in a bucket, so these job headers should't be included, and yet...", string.Empty, GetNextQueueItemSummary().Trim());
		}

		public void TestQuery_ForWorkflowOfJobInQueue()
		{
			var systemPK = Guid.NewGuid();
			var bucketPK = Guid.NewGuid();
			var bufferPK = Guid.NewGuid();

			var job1PK = Guid.NewGuid();
			var job2PK = Guid.NewGuid();
			var job3PK = Guid.NewGuid();
			var jobWithClosedWorkflowPK = Guid.NewGuid();
			var jobWithBufferWorkflowPK = Guid.NewGuid();

			var jobHeader1PK = Guid.NewGuid();
			var jobHeader2PK = Guid.NewGuid();
			var jobHeader3PK = Guid.NewGuid();
			var jobHeaderWithClosedWorkflowPK = Guid.NewGuid();
			var jobHeaderWithBufferWorkflowPK = Guid.NewGuid();

			var workflow1PK = Guid.NewGuid();
			var workflow2PK = Guid.NewGuid();
			var workflow3PK = Guid.NewGuid();
			var closedWorkflowPK = Guid.NewGuid();
			var bufferWorkflow = Guid.NewGuid();

			var workQueueGroupPK = BMDbTestHelper.GetWorkQueueTagGroupPK(TestConnection);
			var workQueueTagPK = Guid.NewGuid();

			var insertSql = new StringBuilder();

			insertSql.AppendLine(BMDbTestHelper.GetBMSystemInsertSql(systemPK, "System"));
			insertSql.AppendFormat(BMDbTestHelper.GetBMComponentInsertSql(bucketPK, systemPK, "BUC", "Bucket", timespanInMinutes: 0));
			insertSql.AppendFormat(BMDbTestHelper.GetBMComponentInsertSql(bufferPK, systemPK, "BUF", "Buffer", timespanInMinutes: 1));

			insertSql.AppendFormat(BMDbTestHelper.ProcessJobHeaderInsertSql, jobHeader1PK, job1PK);
			insertSql.AppendFormat(BMDbTestHelper.ProcessJobHeaderInsertSql, jobHeader2PK, job2PK);
			insertSql.AppendFormat(BMDbTestHelper.ProcessJobHeaderInsertSql, jobHeader3PK, job3PK);
			insertSql.AppendFormat(BMDbTestHelper.ProcessJobHeaderInsertSql, jobHeaderWithClosedWorkflowPK, jobWithClosedWorkflowPK);
			insertSql.AppendFormat(BMDbTestHelper.ProcessJobHeaderInsertSql, jobHeaderWithBufferWorkflowPK, jobWithBufferWorkflowPK);

			insertSql.AppendFormat(BMDbTestHelper.GetProcessHeaderInsertSql(workflow1PK, jobHeader1PK, job1PK, status: "OPN", componentPK: bucketPK));
			insertSql.AppendFormat(BMDbTestHelper.GetProcessHeaderInsertSql(workflow2PK, jobHeader2PK, job2PK, status: "OPN", componentPK: bucketPK));
			insertSql.AppendFormat(BMDbTestHelper.GetProcessHeaderInsertSql(workflow3PK, jobHeader3PK, job3PK, status: "OPN", componentPK: bucketPK));
			insertSql.AppendFormat(BMDbTestHelper.GetProcessHeaderInsertSql(closedWorkflowPK, jobHeaderWithClosedWorkflowPK, jobWithClosedWorkflowPK, status: "CLS", componentPK: bucketPK));
			insertSql.AppendFormat(BMDbTestHelper.GetProcessHeaderInsertSql(bufferWorkflow, jobHeaderWithBufferWorkflowPK, jobWithBufferWorkflowPK, status: "OPN", componentPK: bufferPK));

			insertSql.AppendFormat(BMDbTestHelper.TagMagnitudeWithDescriptionInsertSql, workQueueTagPK, "AAA", workQueueGroupPK, "Case-sensicrepency");

			insertSql.Append(BMDbTestHelper.GetTagLinkInsertSql(Guid.NewGuid(), workQueueTagPK, jobHeader1PK, 0));
			insertSql.Append(BMDbTestHelper.GetTagLinkInsertSql(Guid.NewGuid(), workQueueTagPK, jobHeader2PK, 1));
			insertSql.Append(BMDbTestHelper.GetTagLinkInsertSql(Guid.NewGuid(), workQueueTagPK, jobHeader3PK, 2));
			insertSql.Append(BMDbTestHelper.GetTagLinkInsertSql(Guid.NewGuid(), workQueueTagPK, jobHeaderWithClosedWorkflowPK, 3));
			insertSql.Append(BMDbTestHelper.GetTagLinkInsertSql(Guid.NewGuid(), workQueueTagPK, jobHeaderWithBufferWorkflowPK, 4));

			TestConnection.ExecuteNonQuery(insertSql.ToString());

			AssertEquals(
				$@"
{jobHeader1PK},Case-sensicrepency,{workQueueTagPK},0,
{jobHeader2PK},Case-sensicrepency,{workQueueTagPK},1,
{jobHeader3PK},Case-sensicrepency,{workQueueTagPK},2,",
				GetNextQueueItemSummary());
		}

		public void TestQuery_ForWorkflowsInQueue()
		{
			var systemPK = Guid.NewGuid();
			var bucketPK = Guid.NewGuid();
			var bufferPK = Guid.NewGuid();

			var job1PK = Guid.NewGuid();
			var job2PK = Guid.NewGuid();
			var job3PK = Guid.NewGuid();
			var jobWithClosedWorkflowPK = Guid.NewGuid();
			var jobWithBufferWorkflowPK = Guid.NewGuid();

			var jobHeader1PK = Guid.NewGuid();
			var jobHeader2PK = Guid.NewGuid();
			var jobHeader3PK = Guid.NewGuid();
			var jobHeaderWithClosedWorkflowPK = Guid.NewGuid();
			var jobHeaderWithBufferWorkflowPK = Guid.NewGuid();

			var workflow1PK = Guid.NewGuid();
			var workflow2PK = Guid.NewGuid();
			var workflow3PK = Guid.NewGuid();
			var closedWorkflowPK = Guid.NewGuid();
			var bufferWorkflow = Guid.NewGuid();

			var workQueueGroupPK = BMDbTestHelper.GetWorkQueueTagGroupPK(TestConnection);
			var workQueueTagPK = Guid.NewGuid();

			var insertSql = new StringBuilder();

			insertSql.AppendLine(BMDbTestHelper.GetBMSystemInsertSql(systemPK, "System"));
			insertSql.AppendFormat(BMDbTestHelper.GetBMComponentInsertSql(bucketPK, systemPK, "BUC", "Bucket", timespanInMinutes: 0));
			insertSql.AppendFormat(BMDbTestHelper.GetBMComponentInsertSql(bufferPK, systemPK, "BUF", "Buffer", timespanInMinutes: 1));

			insertSql.AppendFormat(BMDbTestHelper.ProcessJobHeaderInsertSql, jobHeader1PK, job1PK);
			insertSql.AppendFormat(BMDbTestHelper.ProcessJobHeaderInsertSql, jobHeader2PK, job2PK);
			insertSql.AppendFormat(BMDbTestHelper.ProcessJobHeaderInsertSql, jobHeader3PK, job3PK);
			insertSql.AppendFormat(BMDbTestHelper.ProcessJobHeaderInsertSql, jobHeaderWithClosedWorkflowPK, jobWithClosedWorkflowPK);
			insertSql.AppendFormat(BMDbTestHelper.ProcessJobHeaderInsertSql, jobHeaderWithBufferWorkflowPK, jobWithBufferWorkflowPK);

			insertSql.AppendFormat(BMDbTestHelper.GetProcessHeaderInsertSql(workflow1PK, jobHeader1PK, job1PK, status: "OPN", componentPK: bucketPK));
			insertSql.AppendFormat(BMDbTestHelper.GetProcessHeaderInsertSql(workflow2PK, jobHeader2PK, job2PK, status: "OPN", componentPK: bucketPK));
			insertSql.AppendFormat(BMDbTestHelper.GetProcessHeaderInsertSql(workflow3PK, jobHeader3PK, job3PK, status: "OPN", componentPK: bucketPK));
			insertSql.AppendFormat(BMDbTestHelper.GetProcessHeaderInsertSql(closedWorkflowPK, jobHeaderWithClosedWorkflowPK, jobWithClosedWorkflowPK, status: "CLS", componentPK: bucketPK));
			insertSql.AppendFormat(BMDbTestHelper.GetProcessHeaderInsertSql(bufferWorkflow, jobHeaderWithBufferWorkflowPK, jobWithBufferWorkflowPK, status: "OPN", componentPK: bufferPK));

			insertSql.AppendFormat(BMDbTestHelper.TagMagnitudeWithDescriptionInsertSql, workQueueTagPK, "AAA", workQueueGroupPK, "Case-sensicrepency");

			insertSql.Append(BMDbTestHelper.GetTagLinkInsertSql(Guid.NewGuid(), workQueueTagPK, workflow1PK, 0));
			insertSql.Append(BMDbTestHelper.GetTagLinkInsertSql(Guid.NewGuid(), workQueueTagPK, workflow2PK, 1));
			insertSql.Append(BMDbTestHelper.GetTagLinkInsertSql(Guid.NewGuid(), workQueueTagPK, workflow3PK, 2));
			insertSql.Append(BMDbTestHelper.GetTagLinkInsertSql(Guid.NewGuid(), workQueueTagPK, closedWorkflowPK, 3));
			insertSql.Append(BMDbTestHelper.GetTagLinkInsertSql(Guid.NewGuid(), workQueueTagPK, bufferWorkflow, 4));

			TestConnection.ExecuteNonQuery(insertSql.ToString());

			AssertEquals(
				$@"
{workflow1PK},Case-sensicrepency,{workQueueTagPK},0,
{workflow2PK},Case-sensicrepency,{workQueueTagPK},1,
{workflow3PK},Case-sensicrepency,{workQueueTagPK},2,",
				GetNextQueueItemSummary());
		}

		string GetNextQueueItemSummary()
		{
			var result = new StringBuilder();

			using (var reader = TestConnection.Command("SELECT * FROM dbo.GetWorkQueueTagLinks() ORDER BY Sequence;").ExecuteReader())
			{
				while (reader.Read())
				{
					result.AppendLine();

					for (var i = 0; i < reader.FieldCount; i++)
					{
						result.Append(reader[i].ToString());
						result.Append(",");
					}
				}
			}
			return result.ToString();
		}
	}
}

