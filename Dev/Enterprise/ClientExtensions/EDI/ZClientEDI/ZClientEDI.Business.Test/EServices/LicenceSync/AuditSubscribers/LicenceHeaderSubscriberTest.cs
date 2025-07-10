using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.AuditDataServices.Subscription.Testing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.EServices.LicenceSync.AuditSubscribers.Tests
{
	[TestedType(typeof(LicenceHeaderSubscriber))]
	class LicenceHeaderSubscriberTest : ActualDataChangesAuditSubscriberTest
	{
		public void TestLicenceHeaderSubscriber()
		{
			var subscriber = NewDataChangeSubscriber();
			AssertNotNull(subscriber);
			Assert(subscriber is LicenceHeaderSubscriber);

			AssertEquals("LSH", subscriber.Code);
			AssertEquals("LicenceHeaderSubscriber", subscriber.Description);

			AssertEquals(LicenceHeaderSchema.Constants.TableName, subscriber.Table.TableName);
			AssertNotNull(subscriber.SpecificColumns);
			AssertArrayEqualsByElements(
				new SchemaColumn[] {
					LicenceHeaderSchema.PK,
					LicenceHeaderSchema.LA_LD
				},
				subscriber.SpecificColumns.ToArray());

			Assert(subscriber.NotifyInsert);
			Assert(subscriber.NotifyDelete);
			Assert(subscriber.NotifyUpdate);

			Assert(subscriber.IsRequired());
		}

		public override void TestCustomFilter()
		{
			var subscriber = new LicenceHeaderSubscriber();
			AssertNull(subscriber.CustomFilter);
		}

		public void TestProcessChanges_Added()
		{
			var factory = new BusinessObjectFactory { RefreshEnabled = false };
			var testData = factory.NewWithValidTestData<LicenceHeader>();
			factory.Save();

			var table = GetTestDataTable();
			var row = NewDataRow(testData, table);

			table.Rows.Add(row);

			var logger = new LoggerForTest();
			var subscriber = NewDataChangeSubscriber();
			subscriber.ProcessChanges(logger, table);

			var expected =
$@"Started processing changes for 'LicenceHeader' with '1' changed rows.
{{
  ""Table"": ""LicenceHeader"",
  ""RowState"": ""Added"",
  ""RowVersions"": [
    {{
      ""RowVersion"": ""Current"",
      ""Columns"": {{
        ""LA_PK"": ""{testData.PK.ToGuid()}"",
        ""LA_LD"": ""{testData.LA_LD.ToGuid()}""
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
Finished processing changes for 'LicenceHeader'.";
			AssertEquals(expected, logger.ToString());
		}

		public void TestProcessChanges_Modified()
		{
			var factory = new BusinessObjectFactory { RefreshEnabled = false };
			var testData = factory.NewWithValidTestData<LicenceHeader>();
			var newData = factory.NewWithValidTestData<LicenceHeader>();
			factory.Save();

			var table = GetTestDataTable();
			var row = NewDataRow(testData, table);
			table.Rows.Add(row);
			row.AcceptChanges();

			row[LicenceHeaderSchema.Constants.PK] = newData.PK.ToGuid();
			row[LicenceHeaderSchema.Constants.LA_LD] = newData.LA_LD.ToGuid();

			var logger = new LoggerForTest();
			var subscriber = NewDataChangeSubscriber();
			subscriber.ProcessChanges(logger, table);

			var expected =
$@"Started processing changes for 'LicenceHeader' with '1' changed rows.
{{
  ""Table"": ""LicenceHeader"",
  ""RowState"": ""Modified"",
  ""RowVersions"": [
    {{
      ""RowVersion"": ""Original"",
      ""Columns"": {{
        ""LA_PK"": ""{testData.PK.ToGuid()}"",
        ""LA_LD"": ""{testData.LA_LD.ToGuid()}""
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
        ""LA_PK"": ""{newData.PK.ToGuid()}"",
        ""LA_LD"": ""{newData.LA_LD.ToGuid()}""
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
Finished processing changes for 'LicenceHeader'.";
			AssertEquals(expected, logger.ToString());
		}

		public void TestProcessChanges_Deleted()
		{
			var factory = new BusinessObjectFactory { RefreshEnabled = false };
			var testData = factory.NewWithValidTestData<LicenceHeader>();
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
$@"Started processing changes for 'LicenceHeader' with '1' changed rows.
{{
  ""Table"": ""LicenceHeader"",
  ""RowState"": ""Deleted"",
  ""RowVersions"": [
    {{
      ""RowVersion"": ""Original"",
      ""Columns"": {{
        ""LA_PK"": ""{testData.PK.ToGuid()}"",
        ""LA_LD"": ""{testData.LA_LD.ToGuid()}""
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
Finished processing changes for 'LicenceHeader'.";
			AssertEquals(expected, logger.ToString());
		}

		public void TestProcessChanges_Unchanged()
		{
			var factory = new BusinessObjectFactory { RefreshEnabled = false };
			var testData = factory.NewWithValidTestData<LicenceHeader>();
			factory.Save();

			var table = GetTestDataTable();
			var row = NewDataRow(testData, table);
			table.Rows.Add(row);
			row.AcceptChanges();

			var logger = new LoggerForTest();
			var subscriber = NewDataChangeSubscriber();
			subscriber.ProcessChanges(logger, table);

			var expected =
$@"Started processing changes for 'LicenceHeader' with '1' changed rows.
Ignored row with state 'Unchanged'.
Finished processing changes for 'LicenceHeader'.";
			AssertEquals(expected, logger.ToString());
		}

		public void TestProcessChanges_EmptyTable()
		{
			var factory = new BusinessObjectFactory { RefreshEnabled = false };
			var testData = factory.NewWithValidTestData<LicenceHeader>();
			factory.Save();

			var table = GetTestDataTable();

			var logger = new LoggerForTest();
			var subscriber = NewDataChangeSubscriber();
			subscriber.ProcessChanges(logger, table);

			var expected =
$@"Started processing changes for 'LicenceHeader' with '0' changed rows.
Finished processing changes for 'LicenceHeader'.";
			AssertEquals(expected, logger.ToString());
		}

		protected override DataTable GetTestDataTable()
		{
			var table = new DataTable(LicenceHeaderSchema.Constants.TableName);
			table.Columns.Add(LicenceHeaderSchema.Constants.PK, typeof(Guid));
			table.Columns.Add(LicenceHeaderSchema.Constants.LA_LD, typeof(Guid));
			table.Columns.Add(AuditFieldNames.StartLsnFieldName, typeof(byte[]));
			table.Columns.Add(AuditFieldNames.SeqValFieldName, typeof(byte[]));
			table.Columns.Add(AuditFieldNames.CommandIdFieldName, typeof(int));
			table.Columns.Add(AuditFieldNames.OperationFieldName, typeof(int));
			table.Columns.Add(AuditFieldNames.LsnPeriodFieldName, typeof(Int16));
			return table;
		}

		DataRow NewDataRow(LicenceHeader header, DataTable table)
		{
			var row = table.NewRow();
			row[LicenceHeaderSchema.Constants.PK] = header.PK.ToGuid();
			row[LicenceHeaderSchema.Constants.LA_LD] = header.LA_LD.ToGuid();
			row[AuditFieldNames.StartLsnFieldName] = new byte[] { 0, 0 };
			row[AuditFieldNames.SeqValFieldName] = new byte[] { 0, 1 };
			row[AuditFieldNames.CommandIdFieldName] = 1;
			row[AuditFieldNames.OperationFieldName] = 2;
			return row;
		}
	}
}
