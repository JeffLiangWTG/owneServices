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
	[TestedType(typeof(LicenceDatabaseSubscriber))]
	class LicenceDatabaseSubscriberTest : ActualDataChangesAuditSubscriberTest
	{
		public void TestLicenceDatabaseSubscriber()
		{
			var subscriber = NewDataChangeSubscriber();
			AssertNotNull(subscriber);
			Assert(subscriber is LicenceDatabaseSubscriber);

			AssertEquals("LSD", subscriber.Code);
			AssertEquals("LicenceDatabaseSubscriber", subscriber.Description);

			AssertEquals(LicenceDatabaseSchema.Constants.TableName, subscriber.Table.TableName);
			AssertNotNull(subscriber.SpecificColumns);
			AssertArrayEqualsByElements(
				new SchemaColumn[] {
					LicenceDatabaseSchema.PK,
					LicenceDatabaseSchema.LD_ServerCode,
					LicenceDatabaseSchema.LD_LicenceType,
					LicenceDatabaseSchema.LD_LE,
					LicenceDatabaseSchema.LD_IsActive,
					LicenceDatabaseSchema.LD_DatabaseNumber,
					LicenceDatabaseSchema.LD_Product,
					LicenceDatabaseSchema.LD_HostedLocation,
					LicenceDatabaseSchema.LD_TenantID,
				},
				subscriber.SpecificColumns.ToArray());

			Assert(subscriber.NotifyInsert);
			Assert(subscriber.NotifyDelete);
			Assert(subscriber.NotifyUpdate);

			Assert(subscriber.IsRequired());
		}

		public void TestProcessChanges_Added()
		{
			var factory = new BusinessObjectFactory { RefreshEnabled = false };
			var testData = factory.NewWithValidTestData<LicenceDatabase>();
			factory.Save();

			var table = GetTestDataTable();
			var row = NewDataRow(testData, table);

			table.Rows.Add(row);

			var logger = new LoggerForTest();
			var subscriber = NewDataChangeSubscriber();
			subscriber.ProcessChanges(logger, table);

			var expected =
$@"Started processing changes for 'LicenceDatabase' with '1' changed rows.
{{
  ""Table"": ""LicenceDatabase"",
  ""RowState"": ""Added"",
  ""RowVersions"": [
    {{
      ""RowVersion"": ""Current"",
      ""Columns"": {{
        ""LD_PK"": ""{testData.PK.ToGuid()}"",
        ""LD_ServerCode"": ""{testData.LD_ServerCode}"",
        ""LD_LicenceType"": ""{testData.LD_LicenceType}"",
        ""LD_LE"": ""{testData.LD_LE.ToGuid()}"",
        ""LD_IsActive"": ""{(bool)testData.LD_IsActive}"",
        ""LD_DatabaseNumber"": ""{testData.LD_DatabaseNumber}"",
        ""LD_Product"": ""{testData.LD_Product}"",
        ""LD_HostedLocation"": ""{testData.LD_HostedLocation}"",
        ""LD_TenantID"": ""{testData.LD_TenantID}""
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
Finished processing changes for 'LicenceDatabase'.";
			AssertEquals(expected, logger.ToString());
		}

		public void TestProcessChanges_Modified()
		{
			var factory = new BusinessObjectFactory { RefreshEnabled = false };
			var testData = factory.NewWithValidTestData<LicenceDatabase>();
			var newData = factory.NewWithValidTestData<LicenceDatabase>();
			factory.Save();

			var table = GetTestDataTable();
			var row = NewDataRow(testData, table);
			table.Rows.Add(row);
			row.AcceptChanges();

			row[LicenceDatabaseSchema.Constants.PK] = newData.PK.ToGuid();
			row[LicenceDatabaseSchema.Constants.LD_ServerCode] = newData.LD_ServerCode;
			row[LicenceDatabaseSchema.Constants.LD_LicenceType] = newData.LD_LicenceType;
			row[LicenceDatabaseSchema.Constants.LD_LE] = newData.LD_LE.ToGuid();
			row[LicenceDatabaseSchema.Constants.LD_IsActive] = (bool)newData.LD_IsActive;
			row[LicenceDatabaseSchema.Constants.LD_DatabaseNumber] = (int)newData.LD_DatabaseNumber;
			row[LicenceDatabaseSchema.Constants.LD_Product] = newData.LD_Product;
			row[LicenceDatabaseSchema.Constants.LD_HostedLocation] = newData.LD_HostedLocation;
			row[LicenceDatabaseSchema.Constants.LD_TenantID] = newData.LD_TenantID;

			var logger = new LoggerForTest();
			var subscriber = NewDataChangeSubscriber();
			subscriber.ProcessChanges(logger, table);

			var expected =
$@"Started processing changes for 'LicenceDatabase' with '1' changed rows.
{{
  ""Table"": ""LicenceDatabase"",
  ""RowState"": ""Modified"",
  ""RowVersions"": [
    {{
      ""RowVersion"": ""Original"",
      ""Columns"": {{
        ""LD_PK"": ""{testData.PK.ToGuid()}"",
        ""LD_ServerCode"": ""{testData.LD_ServerCode}"",
        ""LD_LicenceType"": ""{testData.LD_LicenceType}"",
        ""LD_LE"": ""{testData.LD_LE.ToGuid()}"",
        ""LD_IsActive"": ""{(bool)testData.LD_IsActive}"",
        ""LD_DatabaseNumber"": ""{testData.LD_DatabaseNumber}"",
        ""LD_Product"": ""{testData.LD_Product}"",
        ""LD_HostedLocation"": ""{testData.LD_HostedLocation}"",
        ""LD_TenantID"": ""{testData.LD_TenantID}""
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
        ""LD_PK"": ""{newData.PK.ToGuid()}"",
        ""LD_ServerCode"": ""{newData.LD_ServerCode}"",
        ""LD_LicenceType"": ""{newData.LD_LicenceType}"",
        ""LD_LE"": ""{newData.LD_LE.ToGuid()}"",
        ""LD_IsActive"": ""{(bool)newData.LD_IsActive}"",
        ""LD_DatabaseNumber"": ""{newData.LD_DatabaseNumber}"",
        ""LD_Product"": ""{newData.LD_Product}"",
        ""LD_HostedLocation"": ""{newData.LD_HostedLocation}"",
        ""LD_TenantID"": ""{newData.LD_TenantID}""
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
Finished processing changes for 'LicenceDatabase'.";
			AssertEquals(expected, logger.ToString());
		}

		public void TestProcessChanges_Deleted()
		{
			var factory = new BusinessObjectFactory { RefreshEnabled = false };
			var testData = factory.NewWithValidTestData<LicenceDatabase>();
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
$@"Started processing changes for 'LicenceDatabase' with '1' changed rows.
{{
  ""Table"": ""LicenceDatabase"",
  ""RowState"": ""Deleted"",
  ""RowVersions"": [
    {{
      ""RowVersion"": ""Original"",
      ""Columns"": {{
        ""LD_PK"": ""{testData.PK.ToGuid()}"",
        ""LD_ServerCode"": ""{testData.LD_ServerCode}"",
        ""LD_LicenceType"": ""{testData.LD_LicenceType}"",
        ""LD_LE"": ""{testData.LD_LE.ToGuid()}"",
        ""LD_IsActive"": ""{(bool)testData.LD_IsActive}"",
        ""LD_DatabaseNumber"": ""{testData.LD_DatabaseNumber}"",
        ""LD_Product"": ""{testData.LD_Product}"",
        ""LD_HostedLocation"": ""{testData.LD_HostedLocation}"",
        ""LD_TenantID"": ""{testData.LD_TenantID}""
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
Finished processing changes for 'LicenceDatabase'.";
			AssertEquals(expected, logger.ToString());
		}

		public void TestProcessChanges_Unchanged()
		{
			var factory = new BusinessObjectFactory { RefreshEnabled = false };
			var testData = factory.NewWithValidTestData<LicenceDatabase>();
			factory.Save();

			var table = GetTestDataTable();
			var row = NewDataRow(testData, table);
			table.Rows.Add(row);
			row.AcceptChanges();

			var logger = new LoggerForTest();
			var subscriber = NewDataChangeSubscriber();
			subscriber.ProcessChanges(logger, table);

			var expected =
$@"Started processing changes for 'LicenceDatabase' with '1' changed rows.
Ignored row with state 'Unchanged'.
Finished processing changes for 'LicenceDatabase'.";
			AssertEquals(expected, logger.ToString());
		}

		public void TestProcessChanges_EmptyTable()
		{
			var factory = new BusinessObjectFactory { RefreshEnabled = false };
			var testData = factory.NewWithValidTestData<LicenceDatabase>();
			factory.Save();

			var table = GetTestDataTable();

			var logger = new LoggerForTest();
			var subscriber = NewDataChangeSubscriber();
			subscriber.ProcessChanges(logger, table);

			var expected =
$@"Started processing changes for 'LicenceDatabase' with '0' changed rows.
Finished processing changes for 'LicenceDatabase'.";
			AssertEquals(expected, logger.ToString());
		}

		protected override DataTable GetTestDataTable()
		{
			var table = new DataTable(LicenceDatabaseSchema.Constants.TableName);
			table.Columns.Add(LicenceDatabaseSchema.Constants.PK, typeof(Guid));
			table.Columns.Add(LicenceDatabaseSchema.Constants.LD_ServerCode, typeof(string));
			table.Columns.Add(LicenceDatabaseSchema.Constants.LD_LicenceType, typeof(string));
			table.Columns.Add(LicenceDatabaseSchema.Constants.LD_LE, typeof(Guid));
			table.Columns.Add(LicenceDatabaseSchema.Constants.LD_IsActive, typeof(bool));
			table.Columns.Add(LicenceDatabaseSchema.Constants.LD_DatabaseNumber, typeof(int));
			table.Columns.Add(LicenceDatabaseSchema.Constants.LD_Product, typeof(string));
			table.Columns.Add(LicenceDatabaseSchema.Constants.LD_HostedLocation, typeof(string));
			table.Columns.Add(LicenceDatabaseSchema.Constants.LD_TenantID, typeof(string));
			table.Columns.Add(AuditFieldNames.StartLsnFieldName, typeof(byte[]));
			table.Columns.Add(AuditFieldNames.SeqValFieldName, typeof(byte[]));
			table.Columns.Add(AuditFieldNames.CommandIdFieldName, typeof(int));
			table.Columns.Add(AuditFieldNames.OperationFieldName, typeof(int));
			table.Columns.Add(AuditFieldNames.LsnPeriodFieldName, typeof(Int16));
			return table;
		}

		DataRow NewDataRow(LicenceDatabase testData, DataTable table)
		{
			var row = table.NewRow();
			row[LicenceDatabaseSchema.Constants.PK] = testData.PK.ToGuid();
			row[LicenceDatabaseSchema.Constants.LD_ServerCode] = testData.LD_ServerCode;
			row[LicenceDatabaseSchema.Constants.LD_LicenceType] = testData.LD_LicenceType;
			row[LicenceDatabaseSchema.Constants.LD_LE] = testData.LD_LE.ToGuid();
			row[LicenceDatabaseSchema.Constants.LD_IsActive] = (bool)testData.LD_IsActive;
			row[LicenceDatabaseSchema.Constants.LD_DatabaseNumber] = (int)testData.LD_DatabaseNumber;
			row[LicenceDatabaseSchema.Constants.LD_Product] = testData.LD_Product;
			row[LicenceDatabaseSchema.Constants.LD_HostedLocation] = testData.LD_HostedLocation;
			row[LicenceDatabaseSchema.Constants.LD_TenantID] = testData.LD_TenantID;
			row[AuditFieldNames.StartLsnFieldName] = new byte[] { 0, 0 };
			row[AuditFieldNames.SeqValFieldName] = new byte[] { 0, 1 };
			row[AuditFieldNames.CommandIdFieldName] = 1;
			row[AuditFieldNames.OperationFieldName] = 2;
			return row;
		}

		public override void TestCustomFilter()
		{
			var subscriber = new LicenceDatabaseSubscriber();
			AssertNull(subscriber.CustomFilter);
		}
	}
}
