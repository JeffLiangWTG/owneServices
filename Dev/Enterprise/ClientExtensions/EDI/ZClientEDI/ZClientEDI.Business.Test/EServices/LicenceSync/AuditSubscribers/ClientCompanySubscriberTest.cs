using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.AuditDataServices.Subscription.Testing;
using Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Core;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.EServices.LicenceSync.AuditSubscribers.Tests
{
	[TestedType(typeof(ClientCompanySubscriber))]
	class ClientCompanySubscriberTest : ActualDataChangesAuditSubscriberTest
	{
		public void TestClientCompanySubscriber()
		{
			var subscriber = NewDataChangeSubscriber();
			AssertNotNull(subscriber);
			Assert(subscriber is ClientCompanySubscriber);

			AssertEquals("LSC", subscriber.Code);
			AssertEquals("ClientCompanySubscriber", subscriber.Description);

			AssertEquals(ClientCompanySchema.Constants.TableName, subscriber.Table.TableName);
			AssertNotNull(subscriber.SpecificColumns);
			AssertArrayEqualsByElements(
				new SchemaColumn[] {
					ClientCompanySchema.PK,
					ClientCompanySchema.LCC_Code,
					ClientCompanySchema.LCC_LD,
					ClientCompanySchema.LCC_RN_NKCountryCode,
					ClientCompanySchema.LCC_CodeValidFromUtc,
				},
				subscriber.SpecificColumns.ToArray());
			AssertNull(subscriber.CustomFilter);

			Assert(subscriber.NotifyInsert);
			Assert(subscriber.NotifyDelete);
			Assert(subscriber.NotifyUpdate);

			Assert(subscriber.IsRequired());
		}

		public override void TestCustomFilter()
		{
			var subscriber = new LicenceCompanySubscriber();
			AssertNull(subscriber.CustomFilter);
		}

		public void TestProcessChanges_Added()
		{
			var factory = new BusinessObjectFactory { RefreshEnabled = false };
			var testData = factory.NewWithValidTestData<ClientCompany>();
			factory.Save();

			var table = GetTestDataTable();
			var row = NewDataRow(testData, table);

			table.Rows.Add(row);

			var logger = new LoggerForTest();
			var subscriber = NewDataChangeSubscriber();
			subscriber.ProcessChanges(logger, table);

			var expected =
$@"Started processing changes for 'ClientCompany' with '1' changed rows.
{{
  ""Table"": ""ClientCompany"",
  ""RowState"": ""Added"",
  ""RowVersions"": [
    {{
      ""RowVersion"": ""Current"",
      ""Columns"": {{
        ""LCC_PK"": ""{testData.PK.ToGuid()}"",
        ""LCC_Code"": ""{testData.LCC_Code}"",
        ""LCC_LD"": ""{testData.LCC_LD.ToGuid()}"",
        ""LCC_RN_NKCountryCode"": ""{testData.LCC_RN_NKCountryCode}"",
        ""LCC_CodeValidFromUtc"": ""{testData.LCC_CodeValidFromUtc.ToDateTime()}""
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
Finished processing changes for 'ClientCompany'.";
			AssertEquals(expected, logger.ToString());
		}

		public void TestProcessChanges_Modified()
		{
			var factory = new BusinessObjectFactory { RefreshEnabled = false };
			var testData = factory.NewWithValidTestData<ClientCompany>();
			var newData = factory.NewWithValidTestData<ClientCompany>();
			factory.Save();

			var table = GetTestDataTable();
			var row = NewDataRow(testData, table);
			table.Rows.Add(row);
			row.AcceptChanges();

			row[ClientCompanySchema.Constants.PK] = newData.PK.ToGuid();
			row[ClientCompanySchema.Constants.LCC_Code] = newData.LCC_Code;
			row[ClientCompanySchema.Constants.LCC_LD] = newData.LCC_LD.ToGuid();
			row[ClientCompanySchema.Constants.LCC_RN_NKCountryCode] = newData.LCC_RN_NKCountryCode;
			row[ClientCompanySchema.Constants.LCC_CodeValidFromUtc] = newData.LCC_CodeValidFromUtc.ToDateTime();

			var logger = new LoggerForTest();
			var subscriber = NewDataChangeSubscriber();
			subscriber.ProcessChanges(logger, table);

			var expected =
$@"Started processing changes for 'ClientCompany' with '1' changed rows.
{{
  ""Table"": ""ClientCompany"",
  ""RowState"": ""Modified"",
  ""RowVersions"": [
    {{
      ""RowVersion"": ""Original"",
      ""Columns"": {{
        ""LCC_PK"": ""{testData.PK.ToGuid()}"",
        ""LCC_Code"": ""{testData.LCC_Code}"",
        ""LCC_LD"": ""{testData.LCC_LD.ToGuid()}"",
        ""LCC_RN_NKCountryCode"": ""{testData.LCC_RN_NKCountryCode}"",
        ""LCC_CodeValidFromUtc"": ""{testData.LCC_CodeValidFromUtc.ToDateTime()}""
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
        ""LCC_PK"": ""{newData.PK.ToGuid()}"",
        ""LCC_Code"": ""{newData.LCC_Code}"",
        ""LCC_LD"": ""{newData.LCC_LD.ToGuid()}"",
        ""LCC_RN_NKCountryCode"": ""{newData.LCC_RN_NKCountryCode}"",
        ""LCC_CodeValidFromUtc"": ""{newData.LCC_CodeValidFromUtc.ToDateTime()}""
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
Finished processing changes for 'ClientCompany'.";
			AssertEquals(expected, logger.ToString());
		}

		public void TestProcessChanges_Deleted()
		{
			var factory = new BusinessObjectFactory { RefreshEnabled = false };
			var testData = factory.NewWithValidTestData<ClientCompany>();
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
$@"Started processing changes for 'ClientCompany' with '1' changed rows.
{{
  ""Table"": ""ClientCompany"",
  ""RowState"": ""Deleted"",
  ""RowVersions"": [
    {{
      ""RowVersion"": ""Original"",
      ""Columns"": {{
        ""LCC_PK"": ""{testData.PK.ToGuid()}"",
        ""LCC_Code"": ""{testData.LCC_Code}"",
        ""LCC_LD"": ""{testData.LCC_LD.ToGuid()}"",
        ""LCC_RN_NKCountryCode"": ""{testData.LCC_RN_NKCountryCode}"",
        ""LCC_CodeValidFromUtc"": ""{testData.LCC_CodeValidFromUtc.ToDateTime()}""
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
Finished processing changes for 'ClientCompany'.";
			AssertEquals(expected, logger.ToString());
		}

		public void TestProcessChanges_Unchanged()
		{
			var factory = new BusinessObjectFactory { RefreshEnabled = false };
			var testData = factory.NewWithValidTestData<ClientCompany>();
			factory.Save();

			var table = GetTestDataTable();
			var row = NewDataRow(testData, table);
			table.Rows.Add(row);
			row.AcceptChanges();

			var logger = new LoggerForTest();
			var subscriber = NewDataChangeSubscriber();
			subscriber.ProcessChanges(logger, table);

			var expected =
$@"Started processing changes for 'ClientCompany' with '1' changed rows.
Ignored row with state 'Unchanged'.
Finished processing changes for 'ClientCompany'.";
			AssertEquals(expected, logger.ToString());
		}

		public void TestProcessChanges_EmptyTable()
		{
			var factory = new BusinessObjectFactory { RefreshEnabled = false };
			var testData = factory.NewWithValidTestData<ClientCompany>();
			factory.Save();

			var table = GetTestDataTable();

			var logger = new LoggerForTest();
			var subscriber = NewDataChangeSubscriber();
			subscriber.ProcessChanges(logger, table);

			var expected =
$@"Started processing changes for 'ClientCompany' with '0' changed rows.
Finished processing changes for 'ClientCompany'.";
			AssertEquals(expected, logger.ToString());
		}

		protected override DataTable GetTestDataTable()
		{
			var table = new DataTable(ClientCompanySchema.Constants.TableName);
			table.Columns.Add(ClientCompanySchema.Constants.PK, typeof(Guid));
			table.Columns.Add(ClientCompanySchema.Constants.LCC_Code, typeof(string));
			table.Columns.Add(ClientCompanySchema.Constants.LCC_LD, typeof(Guid));
			table.Columns.Add(ClientCompanySchema.Constants.LCC_RN_NKCountryCode, typeof(string));
			table.Columns.Add(ClientCompanySchema.Constants.LCC_CodeValidFromUtc, typeof(DateTime));
			table.Columns.Add(AuditFieldNames.StartLsnFieldName, typeof(byte[]));
			table.Columns.Add(AuditFieldNames.SeqValFieldName, typeof(byte[]));
			table.Columns.Add(AuditFieldNames.CommandIdFieldName, typeof(int));
			table.Columns.Add(AuditFieldNames.OperationFieldName, typeof(int));
			table.Columns.Add(AuditFieldNames.LsnPeriodFieldName, typeof(Int16));
			return table;
		}

		DataRow NewDataRow(ClientCompany testData, DataTable table)
		{
			var row = table.NewRow();
			row[ClientCompanySchema.Constants.PK] = testData.PK.ToGuid();
			row[ClientCompanySchema.Constants.LCC_Code] = testData.LCC_Code;
			row[ClientCompanySchema.Constants.LCC_LD] = testData.LCC_LD.ToGuid();
			row[ClientCompanySchema.Constants.LCC_RN_NKCountryCode] = testData.LCC_RN_NKCountryCode;
			row[ClientCompanySchema.Constants.LCC_CodeValidFromUtc] = testData.LCC_CodeValidFromUtc.ToDateTime();
			row[AuditFieldNames.StartLsnFieldName] = new byte[] { 0, 0 };
			row[AuditFieldNames.SeqValFieldName] = new byte[] { 0, 1 };
			row[AuditFieldNames.CommandIdFieldName] = 1;
			row[AuditFieldNames.OperationFieldName] = 2;
			return row;
		}
	}
}
