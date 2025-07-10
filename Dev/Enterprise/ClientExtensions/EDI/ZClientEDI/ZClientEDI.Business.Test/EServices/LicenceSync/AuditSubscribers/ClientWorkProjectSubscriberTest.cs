using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.AuditDataServices.Subscription.Testing;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Integration;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.EServices.LicenceSync.AuditSubscribers.Tests
{
	[TestedType(typeof(ClientWorkProjectSubscriber))]
	class ClientWorkProjectSubscriberTest : ActualDataChangesAuditSubscriberTest
	{
		public void TestClientWorkProjectSubscriber()
		{
			var subscriber = NewDataChangeSubscriber();
			AssertNotNull(subscriber);
			Assert(subscriber is ClientWorkProjectSubscriber);

			AssertEquals("LSP", subscriber.Code);
			AssertEquals("ClientWorkProjectSubscriber", subscriber.Description);

			AssertEquals(ClientWorkProjectSchema.Constants.TableName, subscriber.Table.TableName);
			AssertNotNull(subscriber.SpecificColumns);
			AssertArrayEqualsByElements(
				new SchemaColumn[] {
					ClientWorkProjectSchema.PK,
					ClientWorkProjectSchema.CWP_WKP,
					ClientWorkProjectSchema.CWP_LA
				},
				subscriber.SpecificColumns.ToArray());

			Assert(subscriber.NotifyInsert);
			Assert(subscriber.NotifyDelete);
			Assert(subscriber.NotifyUpdate);

			Assert(subscriber.IsRequired());
		}

		public override void TestCustomFilter()
		{
			var subscriber = new ClientWorkProjectSubscriber();
			AssertNull(subscriber.CustomFilter);
		}

		public void TestProcessChanges_Added()
		{
			var factory = new BusinessObjectFactory { RefreshEnabled = false };
			var testData = factory.NewWithValidTestData<EDIProject>().ClientWorkProject;
			factory.Save();

			var table = GetTestDataTable();
			var row = NewDataRow(testData, table);

			table.Rows.Add(row);

			var logger = new LoggerForTest();
			var subscriber = NewDataChangeSubscriber();
			subscriber.ProcessChanges(logger, table);

			var expected =
$@"Started processing changes for 'ClientWorkProject' with '1' changed rows.
{{
  ""Table"": ""ClientWorkProject"",
  ""RowState"": ""Added"",
  ""RowVersions"": [
    {{
      ""RowVersion"": ""Current"",
      ""Columns"": {{
        ""CWP_PK"": ""{testData.PK.ToGuid()}"",
        ""CWP_WKP"": ""{testData.CWP_WKP.ToGuid()}"",
        ""CWP_LA"": ""{(testData.CWP_LA.IsValid ? testData.CWP_LA.ToGuid() : Guid.Empty)}""
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
Finished processing changes for 'ClientWorkProject'.";
			AssertEquals(expected, logger.ToString());
		}

		public void TestProcessChanges_Modified()
		{
			var factory = new BusinessObjectFactory { RefreshEnabled = false };
			var testData = factory.NewWithValidTestData<EDIProject>().ClientWorkProject;
			var newData = factory.NewWithValidTestData<EDIProject>().ClientWorkProject;
			factory.Save();

			var table = GetTestDataTable();
			var row = NewDataRow(testData, table);
			table.Rows.Add(row);
			row.AcceptChanges();

			row[ClientWorkProjectSchema.Constants.PK] = newData.PK.ToGuid();
			row[ClientWorkProjectSchema.Constants.CWP_WKP] = newData.CWP_WKP.ToGuid();
			row[ClientWorkProjectSchema.Constants.CWP_LA] = newData.CWP_LA.IsValid ? newData.CWP_LA.ToGuid() : Guid.Empty;

			var logger = new LoggerForTest();
			var subscriber = NewDataChangeSubscriber();
			subscriber.ProcessChanges(logger, table);

			var expected =
$@"Started processing changes for 'ClientWorkProject' with '1' changed rows.
{{
  ""Table"": ""ClientWorkProject"",
  ""RowState"": ""Modified"",
  ""RowVersions"": [
    {{
      ""RowVersion"": ""Original"",
      ""Columns"": {{
        ""CWP_PK"": ""{testData.PK.ToGuid()}"",
        ""CWP_WKP"": ""{testData.CWP_WKP.ToGuid()}"",
        ""CWP_LA"": ""{(testData.CWP_LA.IsValid ? testData.CWP_LA.ToGuid() : Guid.Empty)}""
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
        ""CWP_PK"": ""{newData.PK.ToGuid()}"",
        ""CWP_WKP"": ""{newData.CWP_WKP.ToGuid()}"",
        ""CWP_LA"": ""{(newData.CWP_LA.IsValid ? newData.CWP_LA.ToGuid() : Guid.Empty)}""
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
Finished processing changes for 'ClientWorkProject'.";
			AssertEquals(expected, logger.ToString());
		}

		public void TestProcessChanges_Deleted()
		{
			var factory = new BusinessObjectFactory { RefreshEnabled = false };
			var testData = factory.NewWithValidTestData<EDIProject>().ClientWorkProject;
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
$@"Started processing changes for 'ClientWorkProject' with '1' changed rows.
{{
  ""Table"": ""ClientWorkProject"",
  ""RowState"": ""Deleted"",
  ""RowVersions"": [
    {{
      ""RowVersion"": ""Original"",
      ""Columns"": {{
        ""CWP_PK"": ""{testData.PK.ToGuid()}"",
        ""CWP_WKP"": ""{testData.CWP_WKP.ToGuid()}"",
        ""CWP_LA"": ""{(testData.CWP_LA.IsValid ? testData.CWP_LA.ToGuid() : Guid.Empty)}""
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
Finished processing changes for 'ClientWorkProject'.";
			AssertEquals(expected, logger.ToString());
		}

		public void TestProcessChanges_Unchanged()
		{
			var factory = new BusinessObjectFactory { RefreshEnabled = false };
			var testData = factory.NewWithValidTestData<EDIProject>().ClientWorkProject;
			factory.Save();

			var table = GetTestDataTable();
			var row = NewDataRow(testData, table);
			table.Rows.Add(row);
			row.AcceptChanges();

			var logger = new LoggerForTest();
			var subscriber = NewDataChangeSubscriber();
			subscriber.ProcessChanges(logger, table);

			var expected =
$@"Started processing changes for 'ClientWorkProject' with '1' changed rows.
Ignored row with state 'Unchanged'.
Finished processing changes for 'ClientWorkProject'.";
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
$@"Started processing changes for 'ClientWorkProject' with '0' changed rows.
Finished processing changes for 'ClientWorkProject'.";
			AssertEquals(expected, logger.ToString());
		}

		protected override DataTable GetTestDataTable()
		{
			var table = new DataTable(ClientWorkProjectSchema.Constants.TableName);
			table.Columns.Add(ClientWorkProjectSchema.Constants.PK, typeof(Guid));
			table.Columns.Add(ClientWorkProjectSchema.Constants.CWP_WKP, typeof(Guid));
			table.Columns.Add(ClientWorkProjectSchema.Constants.CWP_LA, typeof(Guid));
			table.Columns.Add(AuditFieldNames.StartLsnFieldName, typeof(byte[]));
			table.Columns.Add(AuditFieldNames.SeqValFieldName, typeof(byte[]));
			table.Columns.Add(AuditFieldNames.CommandIdFieldName, typeof(int));
			table.Columns.Add(AuditFieldNames.OperationFieldName, typeof(int));
			table.Columns.Add(AuditFieldNames.LsnPeriodFieldName, typeof(Int16));
			return table;
		}

		DataRow NewDataRow(ClientWorkProject testData, DataTable table)
		{
			var row = table.NewRow();
			row[ClientWorkProjectSchema.Constants.PK] = testData.PK.ToGuid();
			row[ClientWorkProjectSchema.Constants.CWP_WKP] = testData.CWP_WKP.ToGuid();
			row[ClientWorkProjectSchema.Constants.CWP_LA] = testData.CWP_LA.IsValid ? testData.CWP_LA.ToGuid() : Guid.Empty;
			row[AuditFieldNames.StartLsnFieldName] = new byte[] { 0, 0 };
			row[AuditFieldNames.SeqValFieldName] = new byte[] { 0, 1 };
			row[AuditFieldNames.CommandIdFieldName] = 1;
			row[AuditFieldNames.OperationFieldName] = 2;
			return row;
		}
	}
}
