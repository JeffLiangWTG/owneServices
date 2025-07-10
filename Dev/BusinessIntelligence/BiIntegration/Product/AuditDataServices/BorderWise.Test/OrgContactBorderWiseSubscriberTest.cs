using System;
using System.Data;
using System.Linq;
using BorderWise.Sync;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.AuditDataServices.BorderWise.Subscribers;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.BorderWise.Test
{
	[TestedType(typeof(OrgContactBorderWiseSubscriber))]
	class OrgContactBorderWiseSubscriberTest : BorderWiseOrgSubscriberTestCase<OrgContactBorderWiseSubscriber>
	{
		public void TestPublishChange_WhenOneRowFails_ShouldNotProceedToNextRow()
		{
			var changeTable = GetTestDataTable();
			var changeRow1 = changeTable.NewRow();
			var changeRow2 = changeTable.NewRow();

			var factory = new BusinessObjectFactory();
			var org = factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			factory.Save();

			void SetupRow(DataRow row, Guid contactPK)
			{
				row[OrgContactSchema.Constants.PK] = contactPK;
				row[OrgContactSchema.Constants.OC_ContactName] = "Contact Name";
				row[OrgContactSchema.Constants.OC_Title] = "Mr";
				row[OrgContactSchema.Constants.OC_IsActive] = true;
				row[OrgContactSchema.Constants.OC_Language] = "EN";
				row[OrgContactSchema.Constants.OC_Phone] = "123";
				row[OrgContactSchema.Constants.OC_PhoneExtension] = "4";
				row[OrgContactSchema.Constants.OC_Mobile] = "567";
				row[OrgContactSchema.Constants.OC_Email] = "a@b.c";
				row[OrgContactSchema.Constants.OC_Birthday] = new DateTime(2018, 10, 23, 15, 39, 00);
				row[OrgContactSchema.Constants.OC_Gender] = "M";
				row[OrgContactSchema.Constants.OC_RN_NKNationality] = "AU";
				row[OrgContactSchema.Constants.OC_OH] = org.PK.ToGuid();
				row[AuditFieldNames.StartLsnFieldName] = new byte[] { 0, 0 };
				row[AuditFieldNames.SeqValFieldName] = new byte[] { 0, 1 };
				row[AuditFieldNames.CommandIdFieldName] = 1;
				row[OrgContactSchema.Constants.OC_PasswordHash] = new byte[] { 0, 2 };
				row[OrgContactSchema.Constants.OC_PasswordSalt] = new byte[] { 0, 3 };
				row[OrgContactSchema.Constants.OC_PasswordHashIterations] = 0;
				row[OrgContactSchema.Constants.OC_WebAccessEnabled] = true;

				changeTable.Rows.Add(row);
			}

			var notFoundGuid = Guid.NewGuid();

			SetupRow(changeRow1, notFoundGuid);
			SetupRow(changeRow2, contact.PK.ToGuid());

			var publisher = new BorderWiseChangesPublisherForTest();

			var expectedExceptionMessage =
$@"Data row details
OC_PK: {notFoundGuid}
OC_ContactName: Contact Name
OC_Title: Mr
OC_IsActive: True
OC_Language: EN
OC_Phone: 123
OC_PhoneExtension: 4
OC_Mobile: 567
OC_Email: a@b.c
OC_Birthday: 23/10/2018 3:39:00 PM
OC_Gender: M
OC_RN_NKNationality: AU
OC_OH: {org.PK}
{AuditFieldNames.StartLsnFieldName}: Byte[] length 2
{AuditFieldNames.SeqValFieldName}: Byte[] length 2
{AuditFieldNames.CommandIdFieldName}: 1
ExtraColumn: 
OC_PasswordHash: Byte[] length 2
OC_PasswordSalt: Byte[] length 2
OC_PasswordHashIterations: 0
OC_WebAccessEnabled: True";

			using (TestHelper.EnableSyncForTest(publisher))
			{
				ActualDataChangesAuditSubscriber subscriber = new ExplodingSubscriber();
				var exception = AssertExceptionThrown<Exception>(() => subscriber.ProcessChanges(new LoggerForTest(), changeTable));

				AssertMultilineASCIIEquals(expectedExceptionMessage, exception.Message);
				AssertNotNull(exception.InnerException);
			}

			AssertEquals(0, publisher.Entries.Count);
		}

		class ExplodingSubscriber : OrgContactBorderWiseSubscriberForTest
		{
			protected override void PublishChange(ILogger logger, DataRow changeRow, IBorderWiseChangesPublisher publisher)
			{
				throw new InvalidOperationException("Boom!");
			}
		}

		public void TestOrgContactBorderWiseSubscriber()
		{
			var subscriber = NewDataChangeSubscriber();
			AssertNotNull(subscriber);
			AssertEquals("BOC", subscriber.Code);
			AssertEquals(OrgContactSchema.Constants.TableName, subscriber.Table.TableName);
			Assert(subscriber.NotifyInsert);
			Assert(subscriber.NotifyDelete);
			Assert(subscriber.NotifyUpdate);
			AssertEquals("Should group all messages with same constant key", "EdiProd2BWUPM-Sync", ((OrgContactBorderWiseSubscriber)subscriber).MessageKey);

			AssertContainsExactElementsInAnyOrder(
				new string[]
				{
					OrgContactSchema.Constants.OC_ContactName,
					OrgContactSchema.Constants.OC_Title,
					OrgContactSchema.Constants.OC_IsActive,
					OrgContactSchema.Constants.OC_Language,
					OrgContactSchema.Constants.OC_Phone,
					OrgContactSchema.Constants.OC_PhoneExtension,
					OrgContactSchema.Constants.OC_Mobile,
					OrgContactSchema.Constants.OC_Email,
					OrgContactSchema.Constants.OC_Birthday,
					OrgContactSchema.Constants.OC_Gender,
					OrgContactSchema.Constants.OC_RN_NKNationality,
					OrgContactSchema.Constants.OC_OH,
					OrgContactSchema.Constants.OC_PasswordHash,
					OrgContactSchema.Constants.OC_PasswordHashIterations,
					OrgContactSchema.Constants.OC_PasswordSalt,
					OrgContactSchema.Constants.OC_WebAccessEnabled,
					OrgContactSchema.Constants.OC_PER,
				},
				subscriber.SpecificColumns.Select(c => c.Name));
		}

		public void TestProcessAdd()
		{
			var changeTable = GetTestDataTable();
			var changeRow = changeTable.NewRow();
			var factory = new BusinessObjectFactory();
			var org = factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			factory.Save();

			changeRow[OrgContactSchema.Constants.PK] = contact.PK.ToGuid();
			changeRow[OrgContactSchema.Constants.OC_ContactName] = " Contact Name";
			changeRow[OrgContactSchema.Constants.OC_Title] = "Mr";
			changeRow[OrgContactSchema.Constants.OC_IsActive] = true;
			changeRow[OrgContactSchema.Constants.OC_Language] = "EN";
			changeRow[OrgContactSchema.Constants.OC_Phone] = "123";
			changeRow[OrgContactSchema.Constants.OC_PhoneExtension] = "4";
			changeRow[OrgContactSchema.Constants.OC_Mobile] = "567";
			changeRow[OrgContactSchema.Constants.OC_Email] = "a@b.c ";
			changeRow[OrgContactSchema.Constants.OC_Birthday] = new DateTime(2018, 10, 23, 15, 39, 00);
			changeRow[OrgContactSchema.Constants.OC_Gender] = "M";
			changeRow[OrgContactSchema.Constants.OC_RN_NKNationality] = "AU";
			changeRow[OrgContactSchema.Constants.OC_OH] = org.PK.ToGuid();
			changeRow[AuditFieldNames.StartLsnFieldName] = new byte[] { 0, 0 };
			changeRow[AuditFieldNames.SeqValFieldName] = new byte[] { 0, 1 };
			changeRow[AuditFieldNames.CommandIdFieldName] = 1;
			changeRow["ExtraColumn"] = "ABC";
			changeRow[OrgContactSchema.Constants.OC_PasswordHash] = new byte[] { 0, 2 };
			changeRow[OrgContactSchema.Constants.OC_PasswordSalt] = new byte[] { 0, 3 };
			changeRow[OrgContactSchema.Constants.OC_PasswordHashIterations] = 0;
			changeRow[OrgContactSchema.Constants.OC_WebAccessEnabled] = true;
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
    ""PK"": ""{contact.PK}"",
    ""ContactName"": "" Contact Name"",
    ""Title"": ""Mr"",
    ""IsActive"": true,
    ""Language"": ""EN"",
    ""Phone"": ""123"",
    ""PhoneExtension"": ""4"",
    ""Mobile"": ""567"",
    ""Email"": ""a@b.c "",
    ""Birthday"": ""2018-10-23 15:39:00"",
    ""OrgFk"": ""{org.PK}"",
    ""Gender"": ""M"",
    ""Nationality"": ""AU"",
    ""PasswordHash"": ""AAI="",
    ""PasswordSalt"": ""AAM="",
    ""PasswordHashIterations"": 0,
    ""WebAccessEnabled"": true,
    ""SecurityRightGranted"": true
  }},
  ""Source"": ""{MessageSources.EdiProdContactChange}"",
  ""ChangeType"": {(int)ChangeType.Add},
  ""ChangeSequence"": {{
    ""TransactionLsn"": ""AAA="",
    ""SequenceValue"": ""AAE="",
    ""CommandId"": 1
  }}
}}";
			AssertEquals(expectedMessage, publisher.Entries[0].Value);
		}

		public void TestProcessAdd_PersonHasPassword()
		{
			var changeTable = GetTestDataTable();
			var changeRow = changeTable.NewRow();
			var factory = new BusinessObjectFactory();
			var org = factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			factory.Save();

			contact.Person.PER_PasswordHash = new byte[] { 0, 7 };
			contact.Person.PER_PasswordSalt = new byte[] { 0, 8 };
			contact.Person.PER_PasswordHashIterations = 100;
			factory.Save();

			changeRow[OrgContactSchema.Constants.PK] = contact.PK.ToGuid();
			changeRow[OrgContactSchema.Constants.OC_ContactName] = "Contact Name";
			changeRow[OrgContactSchema.Constants.OC_Title] = "Mr";
			changeRow[OrgContactSchema.Constants.OC_IsActive] = true;
			changeRow[OrgContactSchema.Constants.OC_Language] = "EN";
			changeRow[OrgContactSchema.Constants.OC_Phone] = "123";
			changeRow[OrgContactSchema.Constants.OC_PhoneExtension] = "4";
			changeRow[OrgContactSchema.Constants.OC_Mobile] = "567";
			changeRow[OrgContactSchema.Constants.OC_Email] = "a@b.c";
			changeRow[OrgContactSchema.Constants.OC_Birthday] = new DateTime(2018, 10, 23, 15, 39, 00);
			changeRow[OrgContactSchema.Constants.OC_Gender] = "M";
			changeRow[OrgContactSchema.Constants.OC_RN_NKNationality] = "AU";
			changeRow[OrgContactSchema.Constants.OC_OH] = org.PK.ToGuid();
			changeRow[AuditFieldNames.StartLsnFieldName] = new byte[] { 0, 0 };
			changeRow[AuditFieldNames.SeqValFieldName] = new byte[] { 0, 1 };
			changeRow[AuditFieldNames.CommandIdFieldName] = 1;
			changeRow["ExtraColumn"] = "ABC";
			changeRow[OrgContactSchema.Constants.OC_PasswordHash] = new byte[] { 0, 2 };
			changeRow[OrgContactSchema.Constants.OC_PasswordSalt] = new byte[] { 0, 3 };
			changeRow[OrgContactSchema.Constants.OC_PasswordHashIterations] = 0;
			changeRow[OrgContactSchema.Constants.OC_WebAccessEnabled] = true;
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
    ""PK"": ""{contact.PK}"",
    ""ContactName"": ""Contact Name"",
    ""Title"": ""Mr"",
    ""IsActive"": true,
    ""Language"": ""EN"",
    ""Phone"": ""123"",
    ""PhoneExtension"": ""4"",
    ""Mobile"": ""567"",
    ""Email"": ""a@b.c"",
    ""Birthday"": ""2018-10-23 15:39:00"",
    ""OrgFk"": ""{org.PK}"",
    ""Gender"": ""M"",
    ""Nationality"": ""AU"",
    ""PasswordHash"": ""AAc="",
    ""PasswordSalt"": ""AAg="",
    ""PasswordHashIterations"": 100,
    ""WebAccessEnabled"": true,
    ""SecurityRightGranted"": true
  }},
  ""Source"": ""{MessageSources.EdiProdContactChange}"",
  ""ChangeType"": {(int)ChangeType.Add},
  ""ChangeSequence"": {{
    ""TransactionLsn"": ""AAA="",
    ""SequenceValue"": ""AAE="",
    ""CommandId"": 1
  }}
}}";
			AssertMultilineASCIIEquals(expectedMessage, publisher.Entries[0].Value);
		}

		public void TestProcessUpdate()
		{
			var changeTable = GetTestDataTable();
			var changeRow = changeTable.NewRow();

			var factory = new BusinessObjectFactory();
			var org1 = factory.NewWithValidTestData<OrgHeader>();
			var org2 = factory.NewWithValidTestData<OrgHeader>();
			var contact = org1.Contacts.AddNew();
			contact.OC_Birthday = new ZDateTime(1990, 8, 2);
			factory.Save();

			changeRow[OrgContactSchema.Constants.PK] = contact.PK.ToGuid();
			changeRow[OrgContactSchema.Constants.OC_ContactName] = " Contact Name ";
			changeRow[OrgContactSchema.Constants.OC_Title] = "Mr";
			changeRow[OrgContactSchema.Constants.OC_IsActive] = false;
			changeRow[OrgContactSchema.Constants.OC_Language] = "EN";
			changeRow[OrgContactSchema.Constants.OC_Phone] = "123";
			changeRow[OrgContactSchema.Constants.OC_PhoneExtension] = "4";
			changeRow[OrgContactSchema.Constants.OC_Mobile] = "567";
			changeRow[OrgContactSchema.Constants.OC_Email] = "a@b.c ";
			changeRow[OrgContactSchema.Constants.OC_Birthday] = new DateTime(2018, 10, 23, 15, 39, 00);
			changeRow[OrgContactSchema.Constants.OC_Gender] = "M";
			changeRow[OrgContactSchema.Constants.OC_RN_NKNationality] = "AU";
			changeRow[OrgContactSchema.Constants.OC_OH] = org1.PK.ToGuid();
			changeRow[AuditFieldNames.StartLsnFieldName] = new byte[] { 0, 0 };
			changeRow[AuditFieldNames.SeqValFieldName] = new byte[] { 0, 1 };
			changeRow[AuditFieldNames.CommandIdFieldName] = 1;
			changeRow["ExtraColumn"] = "ABC";
			changeRow[OrgContactSchema.Constants.OC_PasswordHash] = new byte[] { 0, 2 };
			changeRow[OrgContactSchema.Constants.OC_PasswordSalt] = new byte[] { 0, 3 };
			changeRow[OrgContactSchema.Constants.OC_PasswordHashIterations] = 0;
			changeRow[OrgContactSchema.Constants.OC_WebAccessEnabled] = false;

			changeTable.Rows.Add(changeRow);
			changeRow.AcceptChanges();

			changeRow[OrgContactSchema.Constants.OC_ContactName] = "Name Contact ";
			changeRow[OrgContactSchema.Constants.OC_Title] = "Mrs";
			changeRow[OrgContactSchema.Constants.OC_IsActive] = true;
			changeRow[OrgContactSchema.Constants.OC_Language] = "CN";
			changeRow[OrgContactSchema.Constants.OC_Phone] = "987";
			changeRow[OrgContactSchema.Constants.OC_PhoneExtension] = "6";
			changeRow[OrgContactSchema.Constants.OC_Mobile] = "543";
			changeRow[OrgContactSchema.Constants.OC_Email] = "x@y.z ";
			changeRow[OrgContactSchema.Constants.OC_Birthday] = new DateTime(2018, 10, 23, 00, 00, 00);
			changeRow[OrgContactSchema.Constants.OC_Gender] = "F";
			changeRow[OrgContactSchema.Constants.OC_RN_NKNationality] = "NZ";
			changeRow[OrgContactSchema.Constants.OC_OH] = org2.PK.ToGuid();
			changeRow[OrgContactSchema.Constants.OC_PasswordHash] = new byte[] { 0, 5 };
			changeRow[OrgContactSchema.Constants.OC_PasswordSalt] = new byte[] { 0, 6 };
			changeRow[OrgContactSchema.Constants.OC_PasswordHashIterations] = 1;
			changeRow[OrgContactSchema.Constants.OC_WebAccessEnabled] = true;
			changeRow["ExtraColumn"] = "XYZ";

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
    ""PK"": ""{contact.PK}"",
    ""ContactName"": "" Contact Name "",
    ""Title"": ""Mr"",
    ""IsActive"": false,
    ""Language"": ""EN"",
    ""Phone"": ""123"",
    ""PhoneExtension"": ""4"",
    ""Mobile"": ""567"",
    ""Email"": ""a@b.c "",
    ""Birthday"": ""2018-10-23 15:39:00"",
    ""OrgFk"": ""{org1.PK}"",
    ""Gender"": ""M"",
    ""Nationality"": ""AU"",
    ""PasswordHash"": ""AAI="",
    ""PasswordSalt"": ""AAM="",
    ""PasswordHashIterations"": 0,
    ""WebAccessEnabled"": false,
    ""SecurityRightGranted"": true
  }},
  ""CurrentVersion"": {{
    ""BorderWisePK"": ""{Guid.Empty}"",
    ""PK"": ""{contact.PK}"",
    ""ContactName"": ""Name Contact "",
    ""Title"": ""Mrs"",
    ""IsActive"": true,
    ""Language"": ""CN"",
    ""Phone"": ""987"",
    ""PhoneExtension"": ""6"",
    ""Mobile"": ""543"",
    ""Email"": ""x@y.z "",
    ""Birthday"": ""2018-10-23 00:00:00"",
    ""OrgFk"": ""{org2.PK}"",
    ""Gender"": ""F"",
    ""Nationality"": ""NZ"",
    ""PasswordHash"": ""AAU="",
    ""PasswordSalt"": ""AAY="",
    ""PasswordHashIterations"": 1,
    ""WebAccessEnabled"": true,
    ""SecurityRightGranted"": true
  }},
  ""Source"": ""{MessageSources.EdiProdContactChange}"",
  ""ChangeType"": {(int)ChangeType.Update},
  ""ChangeSequence"": {{
    ""TransactionLsn"": ""AAA="",
    ""SequenceValue"": ""AAE="",
    ""CommandId"": 1
  }}
}}";
			AssertEquals(expectedMessage, publisher.Entries[0].Value);
		}

		public void TestProcessUpdate_PersonHasPassword()
		{
			var changeTable = GetTestDataTable();
			var changeRow = changeTable.NewRow();

			var factory = new BusinessObjectFactory();
			var org1 = factory.NewWithValidTestData<OrgHeader>();
			var org2 = factory.NewWithValidTestData<OrgHeader>();
			var contact = org1.Contacts.AddNew();
			factory.Save();

			contact.Person.PER_PasswordHash = new byte[] { 0, 7 };
			contact.Person.PER_PasswordSalt = new byte[] { 0, 8 };
			contact.Person.PER_PasswordHashIterations = 100;
			factory.Save();

			changeRow[OrgContactSchema.Constants.PK] = contact.PK.ToGuid();
			changeRow[OrgContactSchema.Constants.OC_ContactName] = "Contact Name";
			changeRow[OrgContactSchema.Constants.OC_Title] = "Mr";
			changeRow[OrgContactSchema.Constants.OC_IsActive] = false;
			changeRow[OrgContactSchema.Constants.OC_Language] = "EN";
			changeRow[OrgContactSchema.Constants.OC_Phone] = "123";
			changeRow[OrgContactSchema.Constants.OC_PhoneExtension] = "4";
			changeRow[OrgContactSchema.Constants.OC_Mobile] = "567";
			changeRow[OrgContactSchema.Constants.OC_Email] = "a@b.c";
			changeRow[OrgContactSchema.Constants.OC_Birthday] = new DateTime(2018, 10, 23, 15, 39, 00);
			changeRow[OrgContactSchema.Constants.OC_Gender] = "M";
			changeRow[OrgContactSchema.Constants.OC_RN_NKNationality] = "AU";
			changeRow[OrgContactSchema.Constants.OC_OH] = org1.PK.ToGuid();
			changeRow[AuditFieldNames.StartLsnFieldName] = new byte[] { 0, 0 };
			changeRow[AuditFieldNames.SeqValFieldName] = new byte[] { 0, 1 };
			changeRow[AuditFieldNames.CommandIdFieldName] = 1;
			changeRow["ExtraColumn"] = "ABC";
			changeRow[OrgContactSchema.Constants.OC_PasswordHash] = new byte[] { 0, 2 };
			changeRow[OrgContactSchema.Constants.OC_PasswordSalt] = new byte[] { 0, 3 };
			changeRow[OrgContactSchema.Constants.OC_PasswordHashIterations] = 0;
			changeRow[OrgContactSchema.Constants.OC_WebAccessEnabled] = false;

			changeTable.Rows.Add(changeRow);
			changeRow.AcceptChanges();

			changeRow[OrgContactSchema.Constants.OC_ContactName] = "Name Contact";
			changeRow[OrgContactSchema.Constants.OC_Title] = "Mrs";
			changeRow[OrgContactSchema.Constants.OC_IsActive] = true;
			changeRow[OrgContactSchema.Constants.OC_Language] = "CN";
			changeRow[OrgContactSchema.Constants.OC_Phone] = "987";
			changeRow[OrgContactSchema.Constants.OC_PhoneExtension] = "6";
			changeRow[OrgContactSchema.Constants.OC_Mobile] = "543";
			changeRow[OrgContactSchema.Constants.OC_Email] = "x@y.z";
			changeRow[OrgContactSchema.Constants.OC_Birthday] = new DateTime(2018, 10, 23, 00, 00, 00);
			changeRow[OrgContactSchema.Constants.OC_Gender] = "F";
			changeRow[OrgContactSchema.Constants.OC_RN_NKNationality] = "NZ";
			changeRow[OrgContactSchema.Constants.OC_OH] = org2.PK.ToGuid();
			changeRow[OrgContactSchema.Constants.OC_PasswordHash] = new byte[] { 0, 5 };
			changeRow[OrgContactSchema.Constants.OC_PasswordSalt] = new byte[] { 0, 6 };
			changeRow[OrgContactSchema.Constants.OC_PasswordHashIterations] = 1;
			changeRow[OrgContactSchema.Constants.OC_WebAccessEnabled] = true;
			changeRow["ExtraColumn"] = "XYZ";

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
    ""PK"": ""{contact.PK}"",
    ""ContactName"": ""Contact Name"",
    ""Title"": ""Mr"",
    ""IsActive"": false,
    ""Language"": ""EN"",
    ""Phone"": ""123"",
    ""PhoneExtension"": ""4"",
    ""Mobile"": ""567"",
    ""Email"": ""a@b.c"",
    ""Birthday"": ""2018-10-23 15:39:00"",
    ""OrgFk"": ""{org1.PK}"",
    ""Gender"": ""M"",
    ""Nationality"": ""AU"",
    ""PasswordHash"": ""AAc="",
    ""PasswordSalt"": ""AAg="",
    ""PasswordHashIterations"": 100,
    ""WebAccessEnabled"": false,
    ""SecurityRightGranted"": true
  }},
  ""CurrentVersion"": {{
    ""BorderWisePK"": ""{Guid.Empty}"",
    ""PK"": ""{contact.PK}"",
    ""ContactName"": ""Name Contact"",
    ""Title"": ""Mrs"",
    ""IsActive"": true,
    ""Language"": ""CN"",
    ""Phone"": ""987"",
    ""PhoneExtension"": ""6"",
    ""Mobile"": ""543"",
    ""Email"": ""x@y.z"",
    ""Birthday"": ""2018-10-23 00:00:00"",
    ""OrgFk"": ""{org2.PK}"",
    ""Gender"": ""F"",
    ""Nationality"": ""NZ"",
    ""PasswordHash"": ""AAc="",
    ""PasswordSalt"": ""AAg="",
    ""PasswordHashIterations"": 100,
    ""WebAccessEnabled"": true,
    ""SecurityRightGranted"": true
  }},
  ""Source"": ""{MessageSources.EdiProdContactChange}"",
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

			var contactPK = Guid.NewGuid();
			var orgPK = Guid.NewGuid();

			changeRow[OrgContactSchema.Constants.PK] = contactPK;
			changeRow[OrgContactSchema.Constants.OC_ContactName] = "Contact Name";
			changeRow[OrgContactSchema.Constants.OC_Title] = "Mr";
			changeRow[OrgContactSchema.Constants.OC_IsActive] = false;
			changeRow[OrgContactSchema.Constants.OC_Language] = "EN";
			changeRow[OrgContactSchema.Constants.OC_Phone] = "123";
			changeRow[OrgContactSchema.Constants.OC_PhoneExtension] = "4";
			changeRow[OrgContactSchema.Constants.OC_Mobile] = "567";
			changeRow[OrgContactSchema.Constants.OC_Email] = "a@b.c";
			changeRow[OrgContactSchema.Constants.OC_Birthday] = new DateTime(2018, 10, 23, 15, 39, 00);
			changeRow[OrgContactSchema.Constants.OC_Gender] = "M";
			changeRow[OrgContactSchema.Constants.OC_RN_NKNationality] = "AU";
			changeRow[OrgContactSchema.Constants.OC_OH] = orgPK;
			changeRow[OrgContactSchema.Constants.OC_WebAccessEnabled] = true;
			changeRow[AuditFieldNames.StartLsnFieldName] = new byte[] { 0, 0 };
			changeRow[AuditFieldNames.SeqValFieldName] = new byte[] { 0, 1 };
			changeRow[AuditFieldNames.CommandIdFieldName] = 1;
			changeRow["ExtraColumn"] = "ABC";

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
    ""PK"": ""{contactPK}"",
    ""ContactName"": ""Contact Name"",
    ""Title"": ""Mr"",
    ""IsActive"": false,
    ""Language"": ""EN"",
    ""Phone"": ""123"",
    ""PhoneExtension"": ""4"",
    ""Mobile"": ""567"",
    ""Email"": ""a@b.c"",
    ""Birthday"": ""2018-10-23 15:39:00"",
    ""OrgFk"": ""{orgPK}"",
    ""Gender"": ""M"",
    ""Nationality"": ""AU"",
    ""PasswordHash"": """",
    ""PasswordSalt"": """",
    ""PasswordHashIterations"": 0,
    ""WebAccessEnabled"": true,
    ""SecurityRightGranted"": true
  }},
  ""CurrentVersion"": null,
  ""Source"": ""{MessageSources.EdiProdContactChange}"",
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

			var factory1 = new BusinessObjectFactory();
			var org = factory1.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			factory1.Save();
			var contactBorderWisePK = Guid.NewGuid();

			changeRow[OrgContactSchema.Constants.PK] = contact.PK.ToGuid();
			changeRow[OrgContactSchema.Constants.OC_ContactName] = "Contact Name";
			changeRow[OrgContactSchema.Constants.OC_Title] = "Mr";
			changeRow[OrgContactSchema.Constants.OC_IsActive] = false;
			changeRow[OrgContactSchema.Constants.OC_Language] = "EN";
			changeRow[OrgContactSchema.Constants.OC_Phone] = "123";
			changeRow[OrgContactSchema.Constants.OC_PhoneExtension] = "4";
			changeRow[OrgContactSchema.Constants.OC_Mobile] = "567";
			changeRow[OrgContactSchema.Constants.OC_Email] = "a@b.c";
			changeRow[OrgContactSchema.Constants.OC_Birthday] = new DateTime(2018, 10, 23, 15, 39, 00);
			changeRow[OrgContactSchema.Constants.OC_Gender] = "M";
			changeRow[OrgContactSchema.Constants.OC_RN_NKNationality] = "AU";
			changeRow[OrgContactSchema.Constants.OC_OH] = org.PK.ToGuid();
			changeRow[OrgContactSchema.Constants.OC_WebAccessEnabled] = true;
			changeRow[AuditFieldNames.StartLsnFieldName] = new byte[] { 0, 0 };
			changeRow[AuditFieldNames.SeqValFieldName] = new byte[] { 0, 1 };
			changeRow[AuditFieldNames.CommandIdFieldName] = 1;
			changeRow["ExtraColumn"] = "ABC";

			changeTable.Rows.Add(changeRow);
			var factory = new BusinessObjectFactory { RefreshEnabled = false };
			var pkMapDataRecord = factory.New<StmData>();
			pkMapDataRecord.SD_Name = OrgContactBorderWiseSubscriber.ContactPkMapStmDataName;
			pkMapDataRecord.SD_Type = "BOR";
			pkMapDataRecord.SD_Owner = contact.PK;
			pkMapDataRecord.SD_DepartmentGuid = contactBorderWisePK;
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
    ""BorderWisePK"": ""{contactBorderWisePK}"",
    ""PK"": ""{contact.PK}"",
    ""ContactName"": ""Contact Name"",
    ""Title"": ""Mr"",
    ""IsActive"": false,
    ""Language"": ""EN"",
    ""Phone"": ""123"",
    ""PhoneExtension"": ""4"",
    ""Mobile"": ""567"",
    ""Email"": ""a@b.c"",
    ""Birthday"": ""2018-10-23 15:39:00"",
    ""OrgFk"": ""{org.PK}"",
    ""Gender"": ""M"",
    ""Nationality"": ""AU"",
    ""PasswordHash"": """",
    ""PasswordSalt"": """",
    ""PasswordHashIterations"": 0,
    ""WebAccessEnabled"": true,
    ""SecurityRightGranted"": true
  }},
  ""Source"": ""{MessageSources.EdiProdContactChange}"",
  ""ChangeType"": {(int)ChangeType.Acknowledgement},
  ""ChangeSequence"": {{
    ""TransactionLsn"": ""AAA="",
    ""SequenceValue"": ""AAE="",
    ""CommandId"": 1
  }}
}}";
			AssertEquals(expectedMessage, publisher.Entries[0].Value);
		}

		public void TestProcessMultipleWithDelete()
		{
			var changeTable = GetTestDataTable();
			var changeRow = changeTable.NewRow();
			var factory = new BusinessObjectFactory();
			var org = factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			factory.Save();

			changeRow[OrgContactSchema.Constants.PK] = contact.PK.ToGuid();
			changeRow[OrgContactSchema.Constants.OC_ContactName] = "Contact Name";
			changeRow[OrgContactSchema.Constants.OC_Title] = "Mr";
			changeRow[OrgContactSchema.Constants.OC_IsActive] = true;
			changeRow[OrgContactSchema.Constants.OC_Language] = "EN";
			changeRow[OrgContactSchema.Constants.OC_Phone] = "123";
			changeRow[OrgContactSchema.Constants.OC_PhoneExtension] = "4";
			changeRow[OrgContactSchema.Constants.OC_Mobile] = "567";
			changeRow[OrgContactSchema.Constants.OC_Email] = "a@b.c";
			changeRow[OrgContactSchema.Constants.OC_Birthday] = new DateTime(2018, 10, 23, 15, 39, 00);
			changeRow[OrgContactSchema.Constants.OC_Gender] = "M";
			changeRow[OrgContactSchema.Constants.OC_RN_NKNationality] = "AU";
			changeRow[OrgContactSchema.Constants.OC_OH] = org.PK.ToGuid();
			changeRow[AuditFieldNames.StartLsnFieldName] = new byte[] { 0, 0 };
			changeRow[AuditFieldNames.SeqValFieldName] = new byte[] { 0, 1 };
			changeRow[AuditFieldNames.CommandIdFieldName] = 1;
			changeRow["ExtraColumn"] = "ABC";
			changeRow[OrgContactSchema.Constants.OC_PasswordHash] = new byte[] { 0, 2 };
			changeRow[OrgContactSchema.Constants.OC_PasswordSalt] = new byte[] { 0, 3 };
			changeRow[OrgContactSchema.Constants.OC_PasswordHashIterations] = 0;
			changeRow[OrgContactSchema.Constants.OC_WebAccessEnabled] = true;
			changeTable.Rows.Add(changeRow);

			var changeRow2 = changeTable.NewRow();
			changeRow2[OrgContactSchema.Constants.PK] = contact.PK.ToGuid();
			changeRow2[OrgContactSchema.Constants.OC_ContactName] = "Contact Name";
			changeRow2[OrgContactSchema.Constants.OC_Title] = "Mr";
			changeRow2[OrgContactSchema.Constants.OC_IsActive] = false;
			changeRow2[OrgContactSchema.Constants.OC_Language] = "EN";
			changeRow2[OrgContactSchema.Constants.OC_Phone] = "123";
			changeRow2[OrgContactSchema.Constants.OC_PhoneExtension] = "4";
			changeRow2[OrgContactSchema.Constants.OC_Mobile] = "567";
			changeRow2[OrgContactSchema.Constants.OC_Email] = "a@b.c";
			changeRow2[OrgContactSchema.Constants.OC_Birthday] = new DateTime(2018, 10, 23, 15, 39, 00);
			changeRow2[OrgContactSchema.Constants.OC_Gender] = "M";
			changeRow2[OrgContactSchema.Constants.OC_RN_NKNationality] = "AU";
			changeRow2[OrgContactSchema.Constants.OC_OH] = org.PK.ToGuid();
			changeRow2[OrgContactSchema.Constants.OC_WebAccessEnabled] = true;
			changeRow2[AuditFieldNames.StartLsnFieldName] = new byte[] { 0, 0 };
			changeRow2[AuditFieldNames.SeqValFieldName] = new byte[] { 0, 1 };
			changeRow2[AuditFieldNames.CommandIdFieldName] = 2;
			changeRow2["ExtraColumn"] = "ABC";

			changeTable.Rows.Add(changeRow2);
			changeRow.AcceptChanges();
			changeRow.Delete();

			var publisher = new BorderWiseChangesPublisherForTest();
			var logger = new LoggerForTest();
			using (TestHelper.EnableSyncForTest(publisher))
			{
				var subscriber = NewDataChangeSubscriber();
				subscriber.ProcessChanges(logger, changeTable);
			}

			AssertEquals(2, publisher.Entries.Count);

			var logEntries = logger.LogEntries.ToList();
			AssertEquals(5, logEntries.Count);
			AssertEquals("BorderWise Received DataRows: 2", logEntries[0]);
			AssertEquals(1, logEntries.Count(l => l.Contains("BorderWise Subscriber Processing DataRow, Change State: Deleted;")));
			AssertEquals(1, logEntries.Count(l => l.Contains("BorderWise Subscriber Processing DataRow, Change State: Added;")));
			AssertEquals(2, logEntries.Count(l => l.Contains("BorderWise Subscriber Published Message")));
		}

		protected override DataTable GetTestDataTable()
		{
			var changeTable = new DataTable();
			changeTable.Columns.Add(OrgContactSchema.Constants.PK, typeof(Guid));
			changeTable.Columns.Add(OrgContactSchema.Constants.OC_ContactName, typeof(string));
			changeTable.Columns.Add(OrgContactSchema.Constants.OC_Title, typeof(string));
			changeTable.Columns.Add(OrgContactSchema.Constants.OC_IsActive, typeof(bool));
			changeTable.Columns.Add(OrgContactSchema.Constants.OC_Language, typeof(string));
			changeTable.Columns.Add(OrgContactSchema.Constants.OC_Phone, typeof(string));
			changeTable.Columns.Add(OrgContactSchema.Constants.OC_PhoneExtension, typeof(string));
			changeTable.Columns.Add(OrgContactSchema.Constants.OC_Mobile, typeof(string));
			changeTable.Columns.Add(OrgContactSchema.Constants.OC_Email, typeof(string));
			changeTable.Columns.Add(OrgContactSchema.Constants.OC_Birthday, typeof(DateTime));
			changeTable.Columns.Add(OrgContactSchema.Constants.OC_Gender, typeof(string));
			changeTable.Columns.Add(OrgContactSchema.Constants.OC_RN_NKNationality, typeof(string));
			changeTable.Columns.Add(OrgContactSchema.Constants.OC_OH, typeof(Guid));
			changeTable.Columns.Add(AuditFieldNames.StartLsnFieldName, typeof(byte[]));
			changeTable.Columns.Add(AuditFieldNames.SeqValFieldName, typeof(byte[]));
			changeTable.Columns.Add(AuditFieldNames.CommandIdFieldName, typeof(int));
			changeTable.Columns.Add("ExtraColumn", typeof(string));
			changeTable.Columns.Add(OrgContactSchema.Constants.OC_PasswordHash, typeof(byte[]));
			changeTable.Columns.Add(OrgContactSchema.Constants.OC_PasswordSalt, typeof(byte[]));
			changeTable.Columns.Add(OrgContactSchema.Constants.OC_PasswordHashIterations, typeof(int));
			changeTable.Columns.Add(OrgContactSchema.Constants.OC_WebAccessEnabled, typeof(bool));
			return changeTable;
		}

		public override void TestCustomFilter()
		{
			var subscriber = new OrgContactBorderWiseSubscriber();
			AssertNull(subscriber.CustomFilter);
		}
	}

	class OrgContactBorderWiseSubscriberForTest : OrgContactBorderWiseSubscriber
	{
		protected override OrgMergeMessageHelper GetNewOrgMergeMessageHelper()
		{
			return new OrgMergeMessageHelperForTest { HasPotentialOrgHeaderChangeOverride = false, TransactionEndTimeOverride = ZDateTime.Empty };
		}
	}
}
