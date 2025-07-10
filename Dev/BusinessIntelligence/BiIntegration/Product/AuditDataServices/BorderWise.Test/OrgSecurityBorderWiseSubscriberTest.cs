using System;
using System.Data;
using System.Linq;
using BorderWise.Sync;
using CargoWise.EntityFramework;
using Enterprise.AuditDataServices.BorderWise.Subscribers;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.BorderWise.Test
{
	[TestedType(typeof(OrgSecurityBorderWiseSubscriber))]
	class OrgSecurityBorderWiseSubscriberTest : BorderWiseOrgSubscriberTestCase<OrgSecurityBorderWiseSubscriber>
	{
		public void TestOrgSecurityBorderWiseSubscriber()
		{
			var subscriber = NewDataChangeSubscriber();
			AssertNotNull(subscriber);

			AssertEquals("BOS", subscriber.Code);
			AssertEquals(OrgSecuritySchema.Constants.TableName, subscriber.Table.TableName);
			Assert(subscriber.NotifyInsert);
			Assert(subscriber.NotifyDelete);
			Assert(subscriber.NotifyUpdate);
			AssertEquals("Should group all messages with same constant key", "EdiProd2BWUPM-Sync", ((OrgSecurityBorderWiseSubscriber)subscriber).MessageKey);

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					OrgSecuritySchema.Constants.OX_OH,
					OrgSecuritySchema.Constants.OX_Granted,
				},
				subscriber.SpecificColumns.Select(c => c.Name));
		}

		public void TestProcessAdd()
		{
			var changeTable = GetTestDataTable();
			var changeRow = changeTable.NewRow();

			var orgHeaderPK = Guid.NewGuid();

			changeRow[OrgSecuritySchema.Constants.OX_OH] = orgHeaderPK;
			changeRow[OrgSecuritySchema.Constants.OX_Granted] = true;
			changeRow[OrgSecuritySchema.Constants.OX_SecurityItemName] = "BorderWise";
			changeRow[AuditFieldNames.StartLsnFieldName] = new byte[] { 0, 0 };
			changeRow[AuditFieldNames.SeqValFieldName] = new byte[] { 0, 1 };
			changeRow[AuditFieldNames.CommandIdFieldName] = 1;
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
    ""OrgHeaderPK"": ""{orgHeaderPK}"",
    ""SecurityRightGranted"": true
  }},
  ""Source"": ""{MessageSources.EdiProdOrgSecurityChange}"",
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

			var orgHeaderPK = Guid.NewGuid();

			changeRow[OrgSecuritySchema.Constants.OX_OH] = orgHeaderPK;
			changeRow[OrgSecuritySchema.Constants.OX_Granted] = false;
			changeRow[OrgSecuritySchema.Constants.OX_SecurityItemName] = "BorderWise";
			changeRow[AuditFieldNames.StartLsnFieldName] = new byte[] { 0, 0 };
			changeRow[AuditFieldNames.SeqValFieldName] = new byte[] { 0, 1 };
			changeRow[AuditFieldNames.CommandIdFieldName] = 1;
			changeTable.Rows.Add(changeRow);
			changeRow.AcceptChanges();

			changeRow[OrgSecuritySchema.Constants.OX_Granted] = true;

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
    ""OrgHeaderPK"": ""{orgHeaderPK}"",
    ""SecurityRightGranted"": false
  }},
  ""CurrentVersion"": {{
    ""BorderWisePK"": ""{Guid.Empty}"",
    ""OrgHeaderPK"": ""{orgHeaderPK}"",
    ""SecurityRightGranted"": true
  }},
  ""Source"": ""{MessageSources.EdiProdOrgSecurityChange}"",
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

			var orgHeaderPK = Guid.NewGuid();
			changeRow[OrgSecuritySchema.Constants.OX_OH] = orgHeaderPK;
			changeRow[OrgSecuritySchema.Constants.OX_Granted] = false;
			changeRow[OrgSecuritySchema.Constants.OX_SecurityItemName] = "BorderWise";
			changeRow[AuditFieldNames.StartLsnFieldName] = new byte[] { 0, 0 };
			changeRow[AuditFieldNames.SeqValFieldName] = new byte[] { 0, 1 };
			changeRow[AuditFieldNames.CommandIdFieldName] = 1;

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
    ""OrgHeaderPK"": ""{orgHeaderPK}"",
    ""SecurityRightGranted"": false
  }},
  ""CurrentVersion"": null,
  ""Source"": ""{MessageSources.EdiProdOrgSecurityChange}"",
  ""ChangeType"": {(int)ChangeType.Delete},
  ""ChangeSequence"": {{
    ""TransactionLsn"": ""AAA="",
    ""SequenceValue"": ""AAE="",
    ""CommandId"": 1
  }}
}}";
			AssertEquals(expectedMessage, publisher.Entries[0].Value);
		}

		public void TestProcessAcknowledgement()
		{
			var changeTable = GetTestDataTable();
			var changeRow = changeTable.NewRow();

			var orgHeaderPK = Guid.NewGuid();
			var borderWisePk = Guid.NewGuid();

			changeRow[OrgSecuritySchema.Constants.OX_OH] = orgHeaderPK;
			changeRow[OrgSecuritySchema.Constants.OX_Granted] = false;
			changeRow[OrgSecuritySchema.Constants.OX_SecurityItemName] = "BorderWise";
			changeRow[AuditFieldNames.StartLsnFieldName] = new byte[] { 0, 0 };
			changeRow[AuditFieldNames.SeqValFieldName] = new byte[] { 0, 1 };
			changeRow[AuditFieldNames.CommandIdFieldName] = 1;
			changeTable.Rows.Add(changeRow);

			var factory = new BusinessObjectFactory { RefreshEnabled = false };
			var pkMapDataRecord = factory.New<StmData>();
			pkMapDataRecord.SD_Name = OrgSecurityBorderWiseSubscriber.OrgPkMapStmDataName;
			pkMapDataRecord.SD_Type = "BOR";
			pkMapDataRecord.SD_Owner = orgHeaderPK;
			pkMapDataRecord.SD_DepartmentGuid = borderWisePk;
			factory.Save();

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
    ""BorderWisePK"": ""{borderWisePk}"",
    ""OrgHeaderPK"": ""{orgHeaderPK}"",
    ""SecurityRightGranted"": false
  }},
  ""Source"": ""{MessageSources.EdiProdOrgSecurityChange}"",
  ""ChangeType"": {(int)ChangeType.Acknowledgement},
  ""ChangeSequence"": {{
    ""TransactionLsn"": ""AAA="",
    ""SequenceValue"": ""AAE="",
    ""CommandId"": 1
  }}
}}";
			AssertEquals(expectedMessage, publisher.Entries[0].Value);
		}

		public void TestShouldNotSendAddMessage_IfSecurityItemIsNotBorderWise()
		{
			var changeTable = GetTestDataTable();
			var changeRow = changeTable.NewRow();

			changeRow[OrgSecuritySchema.Constants.OX_OH] = Guid.NewGuid();
			changeRow[OrgSecuritySchema.Constants.OX_Granted] = true;
			changeRow[OrgSecuritySchema.Constants.OX_SecurityItemName] = "Something else";
			changeRow[AuditFieldNames.StartLsnFieldName] = new byte[] { 0, 0 };
			changeRow[AuditFieldNames.SeqValFieldName] = new byte[] { 0, 1 };
			changeRow[AuditFieldNames.CommandIdFieldName] = 1;
			changeTable.Rows.Add(changeRow);

			var publisher = new BorderWiseChangesPublisherForTest();
			using (TestHelper.EnableSyncForTest(publisher))
			{
				var subscriber = NewDataChangeSubscriber();
				subscriber.ProcessChanges(new LoggerForTest(), changeTable);
			}

			AssertEquals(0, publisher.Entries.Count);
		}

		public void TestShouldNotSendDeleteMessage_IfSecurityItemIsNotBorderWise()
		{
			var changeTable = GetTestDataTable();
			var changeRow = changeTable.NewRow();

			changeRow[OrgSecuritySchema.Constants.OX_OH] = Guid.NewGuid();
			changeRow[OrgSecuritySchema.Constants.OX_Granted] = true;
			changeRow[OrgSecuritySchema.Constants.OX_SecurityItemName] = "Something else";
			changeRow[AuditFieldNames.StartLsnFieldName] = new byte[] { 0, 0 };
			changeRow[AuditFieldNames.SeqValFieldName] = new byte[] { 0, 1 };
			changeRow[AuditFieldNames.CommandIdFieldName] = 1;
			changeTable.Rows.Add(changeRow);
			changeTable.AcceptChanges();
			changeRow.Delete();

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
			var subscriber = new OrgSecurityBorderWiseSubscriber();
			AssertNull(subscriber.CustomFilter);
		}

		protected override DataTable GetTestDataTable()
		{
			var changeTable = new DataTable();
			changeTable.Columns.Add(OrgSecuritySchema.Constants.OX_OH, typeof(Guid));
			changeTable.Columns.Add(OrgSecuritySchema.Constants.OX_Granted, typeof(bool));
			changeTable.Columns.Add(OrgSecuritySchema.Constants.OX_SecurityItemName, typeof(string));
			changeTable.Columns.Add(AuditFieldNames.StartLsnFieldName, typeof(byte[]));
			changeTable.Columns.Add(AuditFieldNames.SeqValFieldName, typeof(byte[]));
			changeTable.Columns.Add(AuditFieldNames.CommandIdFieldName, typeof(int));
			return changeTable;
		}
	}
}
