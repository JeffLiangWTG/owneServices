using System;
using System.Data;
using System.Linq;
using BorderWise.Sync;
using CargoWise.EntityFramework;
using Enterprise.AuditDataServices.BorderWise.Subscribers;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.BorderWise.Test
{ 
	[TestedType(typeof(GlbPersonBorderWiseSubscriber))]
	class GlbPersonBorderWiseSubscriberTest : BorderWiseOrgSubscriberTestCase<GlbPersonBorderWiseSubscriber>
	{
		public void TestGlbPersonBorderWiseSubscriber()
		{
			var subscriber = NewDataChangeSubscriber();
			AssertNotNull(subscriber);
			AssertEquals("BOP", subscriber.Code);
			AssertEquals(GlbPersonSchema.Constants.TableName, subscriber.Table.TableName);
			Assert(!subscriber.NotifyInsert);
			Assert(!subscriber.NotifyDelete);
			Assert(subscriber.NotifyUpdate);
			AssertEquals("Should group all messages with same constant key", "EdiProd2BWUPM-Sync", ((GlbPersonBorderWiseSubscriber)subscriber).MessageKey);

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					GlbPersonSchema.Constants.PER_PasswordHash,
					GlbPersonSchema.Constants.PER_PasswordHashIterations,
					GlbPersonSchema.Constants.PER_PasswordSalt
				},
				subscriber.SpecificColumns.Select(c => c.Name));
		}

		public void TestProcessUpdate()
		{
			var changeTable = GetTestDataTable();
			var changeRow = changeTable.NewRow();

			var factory = new BusinessObjectFactory();
			var org1 = factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org1.Contacts.AddNew();
			factory.Save();

			var org2 = factory.NewWithValidTestData<OrgHeader>();
			var contact2 = org2.Contacts.AddNew();
			contact2.OC_PER = contact1.OC_PER;
			DataRegistry.Instance.PasswordHashingIterationsCount = 100;
			contact1.Person.SetHashedPassword("beans");
			factory.Save();

			changeRow[GlbPersonSchema.Constants.PK] = contact1.OC_PER.ToGuid();
			changeRow[AuditFieldNames.StartLsnFieldName] = new byte[] { 0, 0 };
			changeRow[AuditFieldNames.SeqValFieldName] = new byte[] { 0, 1 };
			changeRow[AuditFieldNames.CommandIdFieldName] = 1;
			changeRow["ExtraColumn"] = "ABC";
			changeRow[GlbPersonSchema.Constants.PER_PasswordHash] = new byte[] { 0, 2 };
			changeRow[GlbPersonSchema.Constants.PER_PasswordSalt] = new byte[] { 0, 3 };
			changeRow[GlbPersonSchema.Constants.PER_PasswordHashIterations] = 0;

			changeTable.Rows.Add(changeRow);
			changeRow.AcceptChanges();

			changeRow[GlbPersonSchema.Constants.PER_PasswordHash] = new byte[] { 0, 5 };
			changeRow[GlbPersonSchema.Constants.PER_PasswordSalt] = new byte[] { 0, 6 };
			changeRow[GlbPersonSchema.Constants.PER_PasswordHashIterations] = 1;
			changeRow["ExtraColumn"] = "XYZ";

			var publisher = new BorderWiseChangesPublisherForTest();
			using (TestHelper.EnableSyncForTest(publisher))
			{
				var subscriber = NewDataChangeSubscriber();
				subscriber.ProcessChanges(new LoggerForTest(), changeTable);
			}

			AssertEquals(1, publisher.Entries.Count);
			AssertEquals("EdiProd2BWUPM-Sync", publisher.Entries[0].Key);
			Assert(publisher.Entries[0].Value.Equals(GetExpectedMessage(contact1, contact2))
				|| publisher.Entries[0].Value.Equals(GetExpectedMessage(contact2, contact1))
			);

			string GetExpectedMessage(OrgContact contactA, OrgContact contactB)
			{
				return
					$@"{{
  ""OriginalVersion"": {{
    ""OC_PKs"": [
      ""{contactA.PK}"",
      ""{contactB.PK}""
    ],
    ""PasswordHash"": ""AAI="",
    ""PasswordSalt"": ""AAM="",
    ""PasswordHashIterations"": 0
  }},
  ""CurrentVersion"": {{
    ""OC_PKs"": [
      ""{contactA.PK}"",
      ""{contactB.PK}""
    ],
    ""PasswordHash"": ""AAU="",
    ""PasswordSalt"": ""AAY="",
    ""PasswordHashIterations"": 1
  }},
  ""Source"": ""{MessageSources.EdiProdPersonChange}"",
  ""ChangeType"": {(int)ChangeType.Update},
  ""ChangeSequence"": {{
    ""TransactionLsn"": ""AAA="",
    ""SequenceValue"": ""AAE="",
    ""CommandId"": 1
  }}
}}";
			}
		}

		public void TestProcessAdd()
		{
			var changeTable = GetTestDataTable();
			var changeRow = changeTable.NewRow();
			var factory = new BusinessObjectFactory();
			var org = factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			factory.Save();

			changeRow[GlbPersonSchema.Constants.PK] = contact.OC_PER.ToGuid();
			changeRow[AuditFieldNames.StartLsnFieldName] = new byte[] { 0, 0 };
			changeRow[AuditFieldNames.SeqValFieldName] = new byte[] { 0, 1 };
			changeRow[AuditFieldNames.CommandIdFieldName] = 1;
			changeRow["ExtraColumn"] = "ABC";
			changeRow[GlbPersonSchema.Constants.PER_PasswordHash] = new byte[] { 0, 2 };
			changeRow[GlbPersonSchema.Constants.PER_PasswordSalt] = new byte[] { 0, 3 };
			changeRow[GlbPersonSchema.Constants.PER_PasswordHashIterations] = 0;
			changeTable.Rows.Add(changeRow);

			var publisher = new BorderWiseChangesPublisherForTest();
			using (TestHelper.EnableSyncForTest(publisher))
			{
				var subscriber = NewDataChangeSubscriber();
				subscriber.ProcessChanges(new LoggerForTest(), changeTable);
			}

			AssertEquals("Should not add entry for add", 0, publisher.Entries.Count);
		}

		public void TestProcessDelete()
		{
			var changeTable = GetTestDataTable();
			var changeRow = changeTable.NewRow();

			var factory = new BusinessObjectFactory();
			var org = factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			factory.Save();

			changeRow[GlbPersonSchema.Constants.PK] = contact.OC_PER.ToGuid();
			changeRow[AuditFieldNames.StartLsnFieldName] = new byte[] { 0, 0 };
			changeRow[AuditFieldNames.SeqValFieldName] = new byte[] { 0, 1 };
			changeRow[AuditFieldNames.CommandIdFieldName] = 1;
			changeRow["ExtraColumn"] = "ABC";
			changeRow[GlbPersonSchema.Constants.PER_PasswordHash] = new byte[] { 0, 2 };
			changeRow[GlbPersonSchema.Constants.PER_PasswordSalt] = new byte[] { 0, 3 };
			changeRow[GlbPersonSchema.Constants.PER_PasswordHashIterations] = 0;

			changeTable.Rows.Add(changeRow);
			changeRow.AcceptChanges();
			changeRow.Delete();

			var publisher = new BorderWiseChangesPublisherForTest();
			using (TestHelper.EnableSyncForTest(publisher))
			{
				var subscriber = NewDataChangeSubscriber();
				subscriber.ProcessChanges(new LoggerForTest(), changeTable);
			}

			AssertEquals("Should not add entry for delete", 0, publisher.Entries.Count);
		}

		public override void TestCustomFilter()
		{
			var subscriber = new ContactCertificateBorderWiseSubscriber();
			AssertNull(subscriber.CustomFilter);
		}

		protected override DataTable GetTestDataTable()
		{
			var changeTable = new DataTable();
			changeTable.Columns.Add(GlbPersonSchema.Constants.PK, typeof(Guid));
			changeTable.Columns.Add(AuditFieldNames.StartLsnFieldName, typeof(byte[]));
			changeTable.Columns.Add(AuditFieldNames.SeqValFieldName, typeof(byte[]));
			changeTable.Columns.Add(AuditFieldNames.CommandIdFieldName, typeof(int));
			changeTable.Columns.Add("ExtraColumn", typeof(string));
			changeTable.Columns.Add(GlbPersonSchema.Constants.PER_PasswordHash, typeof(byte[]));
			changeTable.Columns.Add(GlbPersonSchema.Constants.PER_PasswordSalt, typeof(byte[]));
			changeTable.Columns.Add(GlbPersonSchema.Constants.PER_PasswordHashIterations, typeof(int));
			return changeTable;
		}
	}
}
