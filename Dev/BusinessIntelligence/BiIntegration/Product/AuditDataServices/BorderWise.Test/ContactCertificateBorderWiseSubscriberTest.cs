using System;
using System.Data;
using System.Linq;
using BorderWise.Sync;
using Enterprise.AuditDataServices.BorderWise.Subscribers;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.BorderWise.Test
{
	[TestedType(typeof(ContactCertificateBorderWiseSubscriber))]
	class ContactCertificateBorderWiseSubscriberTest : BorderWiseOrgSubscriberTestCase<ContactCertificateBorderWiseSubscriber>
	{
		public void TestContactCertificateBorderWiseSubscriber()
		{
			var subscriber = NewDataChangeSubscriber();
			AssertNotNull(subscriber);

			AssertEquals("BCC", subscriber.Code);
			AssertEquals(GenRegCertAccredMaintListSchema.Constants.TableName, subscriber.Table.TableName);
			Assert(subscriber.NotifyInsert);
			Assert(subscriber.NotifyDelete);
			Assert(subscriber.NotifyUpdate);
			AssertEquals("Should group all messages with same constant key", "EdiProd2BWUPM-Sync", ((ContactCertificateBorderWiseSubscriber)subscriber).MessageKey);

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					GenRegCertAccredMaintListSchema.Constants.PK,
					GenRegCertAccredMaintListSchema.Constants.XZ_ParentID,
					GenRegCertAccredMaintListSchema.Constants.XZ_ParentTableCode,
					GenRegCertAccredMaintListSchema.Constants.XZ_ExpiryOrDueDate,
					GenRegCertAccredMaintListSchema.Constants.XZ_IssueDate,
					GenRegCertAccredMaintListSchema.Constants.XZ_IsValid,
					GenRegCertAccredMaintListSchema.Constants.XZ_Comment,
					GenRegCertAccredMaintListSchema.Constants.XZ_Type
				},
				subscriber.SpecificColumns.Select(c => c.Name));
		}

		public void TestProcessAdd()
		{
			var changeTable = GetTestDataTable();
			var changeRow = changeTable.NewRow();

			(Guid pK, Guid orgContactPk, string issueDate) = SetupDataRow(changeRow);
			changeTable.Rows.Add(changeRow);

			var publisher = new BorderWiseChangesPublisherForTest();
			using (TestHelper.EnableSyncForTest(publisher))
			{
				var subscriber = NewDataChangeSubscriber();
				subscriber.ProcessChanges(new LoggerForTest(), changeTable);
			}

			AssertEquals(1, publisher.Entries.Count);
			AssertEquals("EdiProd2BWUPM-Sync", publisher.Entries[0].Key);

			var expectedMessage =
$@"{{
  ""OriginalVersion"": null,
  ""CurrentVersion"": {{
    ""BorderWisePK"": ""{Guid.Empty}"",
    ""PK"": ""{pK}"",
    ""ContactFk"": ""{orgContactPk}"",
    ""Comment"": ""Student - Test"",
    ""ExpiryOrDueDateUtc"": ""0001-01-01 00:00:00"",
    ""IssueDateUtc"": ""{issueDate}"",
    ""RefNumber"": ""12345"",
    ""Type"": ""MSC"",
    ""IsValid"": true,
    ""ValidatedDateUtc"": ""0001-01-01 00:00:00""
  }},
  ""Source"": ""{MessageSources.EdiProdContactCertificateChange}"",
  ""ChangeType"": {(int)ChangeType.Add},
  ""ChangeSequence"": {{
    ""TransactionLsn"": ""AAA="",
    ""SequenceValue"": ""AAE="",
    ""CommandId"": 1
  }}
}}";
			AssertEquals(expectedMessage, publisher.Entries[0].Value);
		}

		public void TestProcessUpdate()
		{
			var changeTable = GetTestDataTable();
			var changeRow = changeTable.NewRow();

			(Guid pK, Guid orgContactPk, string issueDate) = SetupDataRow(changeRow);
			changeTable.Rows.Add(changeRow);
			changeRow.AcceptChanges();

			var newExpiryDate = TestHelper.GetDateTimeUtcNowString();
			changeRow[GenRegCertAccredMaintListSchema.Constants.XZ_ExpiryOrDueDate] = newExpiryDate;

			var publisher = new BorderWiseChangesPublisherForTest();
			using (TestHelper.EnableSyncForTest(publisher))
			{
				var subscriber = NewDataChangeSubscriber();
				subscriber.ProcessChanges(new LoggerForTest(), changeTable);
			}

			AssertEquals(1, publisher.Entries.Count);
			AssertEquals("EdiProd2BWUPM-Sync", publisher.Entries[0].Key);

			var expectedMessage =
$@"{{
  ""OriginalVersion"": {{
    ""BorderWisePK"": ""{Guid.Empty}"",
    ""PK"": ""{pK}"",
    ""ContactFk"": ""{orgContactPk}"",
    ""Comment"": ""Student - Test"",
    ""ExpiryOrDueDateUtc"": ""0001-01-01 00:00:00"",
    ""IssueDateUtc"": ""{issueDate}"",
    ""RefNumber"": ""12345"",
    ""Type"": ""MSC"",
    ""IsValid"": true,
    ""ValidatedDateUtc"": ""0001-01-01 00:00:00""
  }},
  ""CurrentVersion"": {{
    ""BorderWisePK"": ""{Guid.Empty}"",
    ""PK"": ""{pK}"",
    ""ContactFk"": ""{orgContactPk}"",
    ""Comment"": ""Student - Test"",
    ""ExpiryOrDueDateUtc"": ""{newExpiryDate}"",
    ""IssueDateUtc"": ""{issueDate}"",
    ""RefNumber"": ""12345"",
    ""Type"": ""MSC"",
    ""IsValid"": true,
    ""ValidatedDateUtc"": ""0001-01-01 00:00:00""
  }},
  ""Source"": ""{MessageSources.EdiProdContactCertificateChange}"",
  ""ChangeType"": {(int)ChangeType.Update},
  ""ChangeSequence"": {{
    ""TransactionLsn"": ""AAA="",
    ""SequenceValue"": ""AAE="",
    ""CommandId"": 1
  }}
}}";
			AssertEquals(expectedMessage, publisher.Entries[0].Value);
		}

		public void TestProcessDelete()
		{
			var changeTable = GetTestDataTable();
			var changeRow = changeTable.NewRow();

			(Guid pK, Guid orgContactPk, string issueDate) = SetupDataRow(changeRow);
			changeTable.Rows.Add(changeRow);
			changeRow.AcceptChanges();
			changeRow.Delete();

			var publisher = new BorderWiseChangesPublisherForTest();
			using (TestHelper.EnableSyncForTest(publisher))
			{
				var subscriber = NewDataChangeSubscriber();
				subscriber.ProcessChanges(new LoggerForTest(), changeTable);
			}

			AssertEquals(1, publisher.Entries.Count);
			AssertEquals("EdiProd2BWUPM-Sync", publisher.Entries[0].Key);

			var expectedMessage =
$@"{{
  ""OriginalVersion"": {{
    ""BorderWisePK"": ""{Guid.Empty}"",
    ""PK"": ""{pK}"",
    ""ContactFk"": ""{orgContactPk}"",
    ""Comment"": ""Student - Test"",
    ""ExpiryOrDueDateUtc"": ""0001-01-01 00:00:00"",
    ""IssueDateUtc"": ""{issueDate}"",
    ""RefNumber"": ""12345"",
    ""Type"": ""MSC"",
    ""IsValid"": true,
    ""ValidatedDateUtc"": ""0001-01-01 00:00:00""
  }},
  ""CurrentVersion"": null,
  ""Source"": ""{MessageSources.EdiProdContactCertificateChange}"",
  ""ChangeType"": {(int)ChangeType.Delete},
  ""ChangeSequence"": {{
    ""TransactionLsn"": ""AAA="",
    ""SequenceValue"": ""AAE="",
    ""CommandId"": 1
  }}
}}";
			AssertEquals(expectedMessage, publisher.Entries[0].Value);
		}

		public void TestShouldNotSendMessage_When_ParentTableCodeIsNotOC()
		{
			var changeTable = GetTestDataTable();
			var changeRow = changeTable.NewRow();

			_ = SetupDataRow(changeRow, parentTableCode: "Others");
			changeTable.Rows.Add(changeRow);

			var publisher = new BorderWiseChangesPublisherForTest();
			using (TestHelper.EnableSyncForTest(publisher))
			{
				var subscriber = NewDataChangeSubscriber();
				subscriber.ProcessChanges(new LoggerForTest(), changeTable);
			}

			AssertEquals(0, publisher.Entries.Count);
		}

		public override void TestCustomFilter()
		{
			var subscriber = new ContactCertificateBorderWiseSubscriber();
			AssertNull(subscriber.CustomFilter);
		}

		protected override DataTable GetTestDataTable()
		{
			var changeTable = new DataTable();
			changeTable.Columns.Add(GenRegCertAccredMaintListSchema.Constants.PK, typeof(Guid));
			changeTable.Columns.Add(GenRegCertAccredMaintListSchema.Constants.XZ_ParentID, typeof(Guid));
			changeTable.Columns.Add(GenRegCertAccredMaintListSchema.Constants.XZ_ParentTableCode, typeof(string));
			changeTable.Columns.Add(GenRegCertAccredMaintListSchema.Constants.XZ_ExpiryOrDueDate, typeof(DateTime));
			changeTable.Columns.Add(GenRegCertAccredMaintListSchema.Constants.XZ_IssueDate, typeof(DateTime));
			changeTable.Columns.Add(GenRegCertAccredMaintListSchema.Constants.XZ_IsValid, typeof(bool));
			changeTable.Columns.Add(GenRegCertAccredMaintListSchema.Constants.XZ_Comment, typeof(string));
			changeTable.Columns.Add(GenRegCertAccredMaintListSchema.Constants.XZ_RefNumber, typeof(string));
			changeTable.Columns.Add(GenRegCertAccredMaintListSchema.Constants.XZ_Type, typeof(string));
			changeTable.Columns.Add(AuditFieldNames.StartLsnFieldName, typeof(byte[]));
			changeTable.Columns.Add(AuditFieldNames.SeqValFieldName, typeof(byte[]));
			changeTable.Columns.Add(AuditFieldNames.CommandIdFieldName, typeof(int));
			return changeTable;
		}

		(Guid pk, Guid orgContactPk, string issueDateString) SetupDataRow(DataRow changeRow, string comment = "Student - Test", string type = "MSC", string parentTableCode = "OC")
		{
			var pK = Guid.NewGuid();
			var orgContactPk = Guid.NewGuid();
			var issueDate = TestHelper.GetDateTimeUtcNowString();

			changeRow[GenRegCertAccredMaintListSchema.Constants.PK] = pK;
			changeRow[GenRegCertAccredMaintListSchema.Constants.XZ_ParentID] = orgContactPk;
			changeRow[GenRegCertAccredMaintListSchema.Constants.XZ_ParentTableCode] = parentTableCode;
			changeRow[GenRegCertAccredMaintListSchema.Constants.XZ_IssueDate] = issueDate;
			changeRow[GenRegCertAccredMaintListSchema.Constants.XZ_IsValid] = true;
			changeRow[GenRegCertAccredMaintListSchema.Constants.XZ_Comment] = comment;
			changeRow[GenRegCertAccredMaintListSchema.Constants.XZ_RefNumber] = "12345";
			changeRow[GenRegCertAccredMaintListSchema.Constants.XZ_Type] = type;
			changeRow[AuditFieldNames.StartLsnFieldName] = new byte[] { 0, 0 };
			changeRow[AuditFieldNames.SeqValFieldName] = new byte[] { 0, 1 };
			changeRow[AuditFieldNames.CommandIdFieldName] = 1;
			return (pK, orgContactPk, issueDate);
		}
	}
}
