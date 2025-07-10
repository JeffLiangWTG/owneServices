using System.Data;
using System.Linq;
using BorderWise.Sync;
using CargoWise.EntityFramework;
using Enterprise.AuditDataServices.BorderWise.Subscribers;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Guid = System.Guid;

namespace Enterprise.AuditDataServices.BorderWise.Test
{ 
	[TestedType(typeof(ContactSecurityBorderWiseSubscriber))]
	class ContactSecurityBorderWiseSubscriberTest : BorderWiseOrgSubscriberTestCase<ContactSecurityBorderWiseSubscriber>
	{
		public void TestContactSecurityBorderWiseSubscriber()
		{
			var subscriber = NewDataChangeSubscriber();
			AssertNotNull(subscriber);

			AssertEquals("BCS", subscriber.Code);
			AssertEquals(OrgSecurityContactsSchema.Constants.TableName, subscriber.Table.TableName);
			Assert(subscriber.NotifyInsert);
			Assert(subscriber.NotifyDelete);
			Assert(subscriber.NotifyUpdate);
			AssertEquals("Should group all messages with same constant key", "EdiProd2BWUPM-Sync", ((ContactSecurityBorderWiseSubscriber)subscriber).MessageKey);

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					OrgSecurityContactsSchema.Constants.OZ_OC,
					OrgSecurityContactsSchema.Constants.OZ_Granted,
				},
				subscriber.SpecificColumns.Select(c => c.Name));
		}

		public void TestProcessAdd()
		{
			var (orgContactPk, orgSecurityPk) = SetupContactAndOrgSecurityRight();
			var changeTable = GetTestDataTable();
			var changeRow = changeTable.NewRow();

			changeRow[OrgSecurityContactsSchema.Constants.OZ_OC] = orgContactPk;
			changeRow[OrgSecurityContactsSchema.Constants.OZ_Granted] = false;
			changeRow[OrgSecurityContactsSchema.Constants.OZ_OX] = orgSecurityPk;
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
    ""OrgContactPK"": ""{orgContactPk}"",
    ""SecurityRightGranted"": false,
    ""WebAccessEnabled"": true
  }},
  ""Source"": ""{MessageSources.EdiProdContactSecurityChange}"",
  ""ChangeType"": {(int)ChangeType.Add},
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
			var (orgContactPk, orgSecurityPk) = SetupContactAndOrgSecurityRight(webAccessEnabled: false);
			var changeTable = GetTestDataTable();
			var changeRow = changeTable.NewRow();

			changeRow[OrgSecurityContactsSchema.Constants.OZ_OC] = orgContactPk;
			changeRow[OrgSecurityContactsSchema.Constants.OZ_Granted] = false;
			changeRow[OrgSecurityContactsSchema.Constants.OZ_OX] = orgSecurityPk;
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
    ""OrgContactPK"": ""{orgContactPk}"",
    ""SecurityRightGranted"": false,
    ""WebAccessEnabled"": false
  }},
  ""CurrentVersion"": null,
  ""Source"": ""{MessageSources.EdiProdContactSecurityChange}"",
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
			var (orgContactPk, orgSecurityPk) = SetupContactAndOrgSecurityRight();
			var changeTable = GetTestDataTable();
			var changeRow = changeTable.NewRow();

			var borderWisePk = Guid.NewGuid();

			changeRow[OrgSecurityContactsSchema.Constants.OZ_OC] = orgContactPk;
			changeRow[OrgSecurityContactsSchema.Constants.OZ_Granted] = false;
			changeRow[OrgSecurityContactsSchema.Constants.OZ_OX] = orgSecurityPk;
			changeRow[AuditFieldNames.StartLsnFieldName] = new byte[] { 0, 0 };
			changeRow[AuditFieldNames.SeqValFieldName] = new byte[] { 0, 1 };
			changeRow[AuditFieldNames.CommandIdFieldName] = 1;
			changeTable.Rows.Add(changeRow);

			var factory = new BusinessObjectFactory { RefreshEnabled = false };
			var pkMapDataRecord = factory.New<StmData>();
			pkMapDataRecord.SD_Name = ContactSecurityBorderWiseSubscriber.OrgContactPkMapStmDataName;
			pkMapDataRecord.SD_Type = "BOR";
			pkMapDataRecord.SD_Owner = orgContactPk;
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
    ""OrgContactPK"": ""{orgContactPk}"",
    ""SecurityRightGranted"": false,
    ""WebAccessEnabled"": true
  }},
  ""Source"": ""{MessageSources.EdiProdContactSecurityChange}"",
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
			var (orgContactPk, orgSecurityPk) = SetupContactAndOrgSecurityRight("something else");
			var changeTable = GetTestDataTable();
			var changeRow = changeTable.NewRow();

			changeRow[OrgSecurityContactsSchema.Constants.OZ_OC] = orgContactPk;
			changeRow[OrgSecurityContactsSchema.Constants.OZ_Granted] = false;
			changeRow[OrgSecurityContactsSchema.Constants.OZ_OX] = orgSecurityPk;
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
			var (orgContactPk, orgSecurityPk) = SetupContactAndOrgSecurityRight("something else");
			var changeTable = GetTestDataTable();
			var changeRow = changeTable.NewRow();

			changeRow[OrgSecurityContactsSchema.Constants.OZ_OC] = orgContactPk;
			changeRow[OrgSecurityContactsSchema.Constants.OZ_Granted] = false;
			changeRow[OrgSecurityContactsSchema.Constants.OZ_OX] = orgSecurityPk;
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
			var subscriber = new ContactSecurityBorderWiseSubscriber();
			AssertNull(subscriber.CustomFilter);
		}

		protected override DataTable GetTestDataTable()
		{
			var changeTable = new DataTable();
			changeTable.Columns.Add(OrgSecurityContactsSchema.Constants.OZ_OC, typeof(Guid));
			changeTable.Columns.Add(OrgSecurityContactsSchema.Constants.OZ_Granted, typeof(bool));
			changeTable.Columns.Add(OrgSecurityContactsSchema.Constants.OZ_OX, typeof(Guid));
			changeTable.Columns.Add(AuditFieldNames.StartLsnFieldName, typeof(byte[]));
			changeTable.Columns.Add(AuditFieldNames.SeqValFieldName, typeof(byte[]));
			changeTable.Columns.Add(AuditFieldNames.CommandIdFieldName, typeof(int));
			return changeTable;
		}

		(Guid, Guid) SetupContactAndOrgSecurityRight(string securityItemName = "BorderWise", bool webAccessEnabled = true)
		{
			var factory = new BusinessObjectFactory { RefreshEnabled = false };
			var org = factory.NewWithValidTestData<OrgHeader>();

			var orgSecurity = factory.New<OrgSecurity>();
			orgSecurity.OX_SecurityItemName = securityItemName;
			orgSecurity.OX_Granted = true;
			orgSecurity.OX_IsCustomerManaged = true;
			orgSecurity.OX_OH = org.PK;

			var contact = org.Contacts.AddNew();
			contact.OC_WebAccessEnabled = webAccessEnabled;
			factory.Save();

			return (contact.PK.ToGuid(), orgSecurity.PK.ToGuid());
		}
	}
}
