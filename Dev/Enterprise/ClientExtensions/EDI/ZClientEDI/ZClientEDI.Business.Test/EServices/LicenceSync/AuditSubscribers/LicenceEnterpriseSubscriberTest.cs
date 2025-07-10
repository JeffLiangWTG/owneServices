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
	[TestedType(typeof(LicenceEnterpriseSubscriber))]
	class LicenceEnterpriseSubscriberTest : ActualDataChangesAuditSubscriberTest
	{
		public void TestLicenceEnterpriseSubscriber()
		{
			var subscriber = NewDataChangeSubscriber();
			AssertNotNull(subscriber);
			Assert(subscriber is LicenceEnterpriseSubscriber);

			AssertEquals("LSE", subscriber.Code);
			AssertEquals("LicenceEnterpriseSubscriber", subscriber.Description);

			AssertEquals(LicenceEnterpriseSchema.Constants.TableName, subscriber.Table.TableName);
			AssertNotNull(subscriber.SpecificColumns);
			AssertArrayEqualsByElements(
				new SchemaColumn[] {
					LicenceEnterpriseSchema.PK,
					LicenceEnterpriseSchema.LE_EnterpriseCode,
					LicenceEnterpriseSchema.LE_IsInternal,
				},
				subscriber.SpecificColumns.ToArray());

			Assert(subscriber.NotifyInsert);
			Assert(subscriber.NotifyDelete);
			Assert(subscriber.NotifyUpdate);

			Assert(subscriber.IsRequired());
		}

		public override void TestCustomFilter()
		{
			var subscriber = new LicenceEnterpriseSubscriber();
			AssertNull(subscriber.CustomFilter);
		}

		public void TestProcessChanges_Added()
		{
			var factory = new BusinessObjectFactory { RefreshEnabled = false };
			var testData = factory.NewWithValidTestData<LicenceEnterprise>();
			factory.Save();

			var table = GetTestDataTable();
			var row = NewDataRow(testData, table);

			table.Rows.Add(row);

			var logger = new LoggerForTest();
			var subscriber = NewDataChangeSubscriber();
			subscriber.ProcessChanges(logger, table);

			var expected =
$@"Started processing changes for 'LicenceEnterprise' with '1' changed rows.
{{
  ""Table"": ""LicenceEnterprise"",
  ""RowState"": ""Added"",
  ""RowVersions"": [
    {{
      ""RowVersion"": ""Current"",
      ""Columns"": {{
        ""LE_PK"": ""{testData.PK.ToGuid()}"",
        ""LE_EnterpriseCode"": ""{testData.LE_EnterpriseCode}"",
        ""LE_IsInternal"": ""{(bool)testData.LE_IsInternal}""
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
Finished processing changes for 'LicenceEnterprise'.";
			AssertEquals(expected, logger.ToString());
		}

		public void TestProcessChanges_Modified()
		{
			var factory = new BusinessObjectFactory { RefreshEnabled = false };
			var testData = factory.NewWithValidTestData<LicenceEnterprise>();
			var newData = factory.NewWithValidTestData<LicenceEnterprise>();
			factory.Save();

			var table = GetTestDataTable();
			var row = NewDataRow(testData, table);
			table.Rows.Add(row);
			row.AcceptChanges();

			row[LicenceEnterpriseSchema.Constants.PK] = newData.PK.ToGuid();
			row[LicenceEnterpriseSchema.Constants.LE_EnterpriseCode] = newData.LE_EnterpriseCode;
			row[LicenceEnterpriseSchema.Constants.LE_IsInternal] = (bool)newData.LE_IsInternal;

			var logger = new LoggerForTest();
			var subscriber = NewDataChangeSubscriber();
			subscriber.ProcessChanges(logger, table);

			var expected =
$@"Started processing changes for 'LicenceEnterprise' with '1' changed rows.
{{
  ""Table"": ""LicenceEnterprise"",
  ""RowState"": ""Modified"",
  ""RowVersions"": [
    {{
      ""RowVersion"": ""Original"",
      ""Columns"": {{
        ""LE_PK"": ""{testData.PK.ToGuid()}"",
        ""LE_EnterpriseCode"": ""{testData.LE_EnterpriseCode}"",
        ""LE_IsInternal"": ""{(bool)testData.LE_IsInternal}""
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
        ""LE_PK"": ""{newData.PK.ToGuid()}"",
        ""LE_EnterpriseCode"": ""{newData.LE_EnterpriseCode}"",
        ""LE_IsInternal"": ""{(bool)newData.LE_IsInternal}""
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
Finished processing changes for 'LicenceEnterprise'.";
			AssertEquals(expected, logger.ToString());
		}

		public void TestProcessChanges_Deleted()
		{
			var factory = new BusinessObjectFactory { RefreshEnabled = false };
			var testData = factory.NewWithValidTestData<LicenceEnterprise>();
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
$@"Started processing changes for 'LicenceEnterprise' with '1' changed rows.
{{
  ""Table"": ""LicenceEnterprise"",
  ""RowState"": ""Deleted"",
  ""RowVersions"": [
    {{
      ""RowVersion"": ""Original"",
      ""Columns"": {{
        ""LE_PK"": ""{testData.PK.ToGuid()}"",
        ""LE_EnterpriseCode"": ""{testData.LE_EnterpriseCode}"",
        ""LE_IsInternal"": ""{(bool)testData.LE_IsInternal}""
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
Finished processing changes for 'LicenceEnterprise'.";
			AssertEquals(expected, logger.ToString());
		}

		public void TestProcessChanges_Unchanged()
		{
			var factory = new BusinessObjectFactory { RefreshEnabled = false };
			var testData = factory.NewWithValidTestData<LicenceEnterprise>();
			factory.Save();

			var table = GetTestDataTable();
			var row = NewDataRow(testData, table);
			table.Rows.Add(row);
			row.AcceptChanges();

			var logger = new LoggerForTest();
			var subscriber = NewDataChangeSubscriber();
			subscriber.ProcessChanges(logger, table);

			var expected =
$@"Started processing changes for 'LicenceEnterprise' with '1' changed rows.
Ignored row with state 'Unchanged'.
Finished processing changes for 'LicenceEnterprise'.";
			AssertEquals(expected, logger.ToString());
		}

		public void TestProcessChanges_EmptyTable()
		{
			var factory = new BusinessObjectFactory { RefreshEnabled = false };
			var testData = factory.NewWithValidTestData<LicenceEnterprise>();
			factory.Save();

			var table = GetTestDataTable();

			var logger = new LoggerForTest();
			var subscriber = NewDataChangeSubscriber();
			subscriber.ProcessChanges(logger, table);

			var expected =
$@"Started processing changes for 'LicenceEnterprise' with '0' changed rows.
Finished processing changes for 'LicenceEnterprise'.";
			AssertEquals(expected, logger.ToString());
		}

		protected override DataTable GetTestDataTable()
		{
			var table = new DataTable(LicenceEnterpriseSchema.Constants.TableName);
			table.Columns.Add(LicenceEnterpriseSchema.Constants.PK, typeof(Guid));
			table.Columns.Add(LicenceEnterpriseSchema.Constants.LE_EnterpriseCode, typeof(string));
			table.Columns.Add(LicenceEnterpriseSchema.Constants.LE_IsInternal, typeof(bool));
			table.Columns.Add(AuditFieldNames.StartLsnFieldName, typeof(byte[]));
			table.Columns.Add(AuditFieldNames.SeqValFieldName, typeof(byte[]));
			table.Columns.Add(AuditFieldNames.CommandIdFieldName, typeof(int));
			table.Columns.Add(AuditFieldNames.OperationFieldName, typeof(int));
			table.Columns.Add(AuditFieldNames.LsnPeriodFieldName, typeof(Int16));
			return table;
		}

		DataRow NewDataRow(LicenceEnterprise testData, DataTable table)
		{
			var row = table.NewRow();
			row[LicenceEnterpriseSchema.Constants.PK] = testData.PK.ToGuid();
			row[LicenceEnterpriseSchema.Constants.LE_EnterpriseCode] = testData.LE_EnterpriseCode;
			row[LicenceEnterpriseSchema.Constants.LE_IsInternal] = (bool)testData.LE_IsInternal;
			row[AuditFieldNames.StartLsnFieldName] = new byte[] { 0, 0 };
			row[AuditFieldNames.SeqValFieldName] = new byte[] { 0, 1 };
			row[AuditFieldNames.CommandIdFieldName] = 1;
			row[AuditFieldNames.OperationFieldName] = 2;
			return row;
		}
	}
}
