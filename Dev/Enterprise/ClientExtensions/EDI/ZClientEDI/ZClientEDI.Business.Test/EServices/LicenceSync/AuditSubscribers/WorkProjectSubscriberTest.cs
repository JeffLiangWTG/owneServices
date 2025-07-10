using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.AuditDataServices.Subscription.Testing;
using Enterprise.Integration;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.EServices.LicenceSync.AuditSubscribers.Tests
{
	[TestedType(typeof(WorkProjectSubscriber))]
	class WorkProjectSubscriberTest : ActualDataChangesAuditSubscriberTest
	{
		public void TestWorkProjectSubscriber()
		{
			var subscriber = NewDataChangeSubscriber();
			AssertNotNull(subscriber);
			Assert(subscriber is WorkProjectSubscriber);

			AssertEquals("LSW", subscriber.Code);
			AssertEquals("WorkProjectSubscriber", subscriber.Description);

			AssertEquals(WorkProjectSchema.Constants.TableName, subscriber.Table.TableName);
			AssertNotNull(subscriber.SpecificColumns);
			AssertArrayEqualsByElements(
				new SchemaColumn[] {
					WorkProjectSchema.PK,
					WorkProjectSchema.WKP_Status,
					WorkProjectSchema.WKP_Module
				},
				subscriber.SpecificColumns.ToArray());

			Assert(subscriber.NotifyInsert);
			Assert(subscriber.NotifyDelete);
			Assert(subscriber.NotifyUpdate);

			Assert(subscriber.IsRequired());
		}

		public override void TestCustomFilter()
		{
			var subscriber = new WorkProjectSubscriber();
			AssertNull(subscriber.CustomFilter);
		}

		public void TestProcessChanges_Added()
		{
			var factory = new BusinessObjectFactory { RefreshEnabled = false };
			var testData = factory.NewWithValidTestData<Project>();
			factory.Save();

			var table = GetTestDataTable();
			var row = NewDataRow(testData, table);

			table.Rows.Add(row);

			var logger = new LoggerForTest();
			var subscriber = NewDataChangeSubscriber();
			subscriber.ProcessChanges(logger, table);

			var expected =
$@"Started processing changes for 'WorkProject' with '1' changed rows.
{{
  ""Table"": ""WorkProject"",
  ""RowState"": ""Added"",
  ""RowVersions"": [
    {{
      ""RowVersion"": ""Current"",
      ""Columns"": {{
        ""WKP_PK"": ""{testData.PK.ToGuid()}"",
        ""WKP_Status"": ""{testData.WKP_Status}"",
        ""WKP_Module"": ""{testData.WKP_Module}""
      }},
      ""AuditFields"": {{
        ""StartLsn"": ""00-00"",
        ""SeqVal"": ""00-01"",
        ""CommandId"": 1,
        ""Operation"": 2
      }}
    }}
  ]
}}
Finished processing changes for 'WorkProject'.";
			AssertEquals(expected, logger.ToString());
		}

		public void TestProcessChanges_Modified()
		{
			var factory = new BusinessObjectFactory { RefreshEnabled = false };
			var testData = factory.NewWithValidTestData<Project>();
			var newData = factory.NewWithValidTestData<Project>();
			factory.Save();

			var table = GetTestDataTable();
			var row = NewDataRow(testData, table);
			table.Rows.Add(row);
			row.AcceptChanges();

			row[WorkProjectSchema.Constants.PK] = newData.PK.ToGuid();
			row[WorkProjectSchema.Constants.WKP_Status] = newData.WKP_Status;
			row[WorkProjectSchema.Constants.WKP_Module] = newData.WKP_Module;

			var logger = new LoggerForTest();
			var subscriber = NewDataChangeSubscriber();
			subscriber.ProcessChanges(logger, table);

			var expected =
$@"Started processing changes for 'WorkProject' with '1' changed rows.
{{
  ""Table"": ""WorkProject"",
  ""RowState"": ""Modified"",
  ""RowVersions"": [
    {{
      ""RowVersion"": ""Original"",
      ""Columns"": {{
        ""WKP_PK"": ""{testData.PK.ToGuid()}"",
        ""WKP_Status"": ""{testData.WKP_Status}"",
        ""WKP_Module"": ""{testData.WKP_Module}""
      }},
      ""AuditFields"": {{
        ""StartLsn"": ""00-00"",
        ""SeqVal"": ""00-01"",
        ""CommandId"": 1,
        ""Operation"": 2
      }}
    }},
    {{
      ""RowVersion"": ""Current"",
      ""Columns"": {{
        ""WKP_PK"": ""{newData.PK.ToGuid()}"",
        ""WKP_Status"": ""{newData.WKP_Status}"",
        ""WKP_Module"": ""{newData.WKP_Module}""
      }},
      ""AuditFields"": {{
        ""StartLsn"": ""00-00"",
        ""SeqVal"": ""00-01"",
        ""CommandId"": 1,
        ""Operation"": 2
      }}
    }}
  ]
}}
Finished processing changes for 'WorkProject'.";
			AssertEquals(expected, logger.ToString());
		}

		public void TestProcessChanges_Deleted()
		{
			var factory = new BusinessObjectFactory { RefreshEnabled = false };
			var testData = factory.NewWithValidTestData<Project>();
			factory.Save();

			var table = GetTestDataTable();
			var row = NewDataRow(testData, table);
			table.Rows.Add(row);
			row.AcceptChanges();

			row.Delete();

			var logger = new LoggerForTest();
			var subscriber = NewDataChangeSubscriber();
			subscriber.ProcessChanges(logger, table);

			var expected =
$@"Started processing changes for 'WorkProject' with '1' changed rows.
{{
  ""Table"": ""WorkProject"",
  ""RowState"": ""Deleted"",
  ""RowVersions"": [
    {{
      ""RowVersion"": ""Original"",
      ""Columns"": {{
        ""WKP_PK"": ""{testData.PK.ToGuid()}"",
        ""WKP_Status"": ""{testData.WKP_Status}"",
        ""WKP_Module"": ""{testData.WKP_Module}""
      }},
      ""AuditFields"": {{
        ""StartLsn"": ""00-00"",
        ""SeqVal"": ""00-01"",
        ""CommandId"": 1,
        ""Operation"": 2
      }}
    }}
  ]
}}
Finished processing changes for 'WorkProject'.";
			AssertEquals(expected, logger.ToString());
		}

		public void TestProcessChanges_Unchanged()
		{
			var factory = new BusinessObjectFactory { RefreshEnabled = false };
			var testData = factory.NewWithValidTestData<Project>();
			factory.Save();

			var table = GetTestDataTable();
			var row = NewDataRow(testData, table);
			table.Rows.Add(row);
			row.AcceptChanges();

			var logger = new LoggerForTest();
			var subscriber = NewDataChangeSubscriber();
			subscriber.ProcessChanges(logger, table);

			var expected =
$@"Started processing changes for 'WorkProject' with '1' changed rows.
Ignored row with state 'Unchanged'.
Finished processing changes for 'WorkProject'.";
			AssertEquals(expected, logger.ToString());
		}

		public void TestProcessChanges_EmptyTable()
		{
			var factory = new BusinessObjectFactory { RefreshEnabled = false };
			var testData = factory.NewWithValidTestData<Project>();
			factory.Save();

			var table = GetTestDataTable();

			var logger = new LoggerForTest();
			var subscriber = NewDataChangeSubscriber();
			subscriber.ProcessChanges(logger, table);

			var expected =
$@"Started processing changes for 'WorkProject' with '0' changed rows.
Finished processing changes for 'WorkProject'.";
			AssertEquals(expected, logger.ToString());
		}

		protected override DataTable GetTestDataTable()
		{
			var table = new DataTable(WorkProjectSchema.Constants.TableName);
			table.Columns.Add(WorkProjectSchema.Constants.PK, typeof(Guid));
			table.Columns.Add(WorkProjectSchema.Constants.WKP_Status, typeof(string));
			table.Columns.Add(WorkProjectSchema.Constants.WKP_Module, typeof(string));
			table.Columns.Add(AuditFieldNames.StartLsnFieldName, typeof(byte[]));
			table.Columns.Add(AuditFieldNames.SeqValFieldName, typeof(byte[]));
			table.Columns.Add(AuditFieldNames.CommandIdFieldName, typeof(int));
			table.Columns.Add(AuditFieldNames.OperationFieldName, typeof(int));
			table.Columns.Add(AuditFieldNames.LsnPeriodFieldName, typeof(Int16));
			return table;
		}

		DataRow NewDataRow(Project testData, DataTable table)
		{
			var row = table.NewRow();
			row[WorkProjectSchema.Constants.PK] = testData.PK.ToGuid();
			row[WorkProjectSchema.Constants.WKP_Status] = testData.WKP_Status;
			row[WorkProjectSchema.Constants.WKP_Module] = testData.WKP_Module;
			row[AuditFieldNames.StartLsnFieldName] = new byte[] { 0, 0 };
			row[AuditFieldNames.SeqValFieldName] = new byte[] { 0, 1 };
			row[AuditFieldNames.CommandIdFieldName] = 1;
			row[AuditFieldNames.OperationFieldName] = 2;
			return row;
		}
	}
}
