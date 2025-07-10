using System;
using System.Data;
using System.Linq;
using BorderWise.Sync;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.AuditDataServices.BorderWise.Subscribers;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.BorderWise.Test
{
	[TestedType(typeof(OrgAddressBorderWiseSubscriber))]
	class OrgAddressBorderWiseSubscriberTest : BorderWiseOrgSubscriberTestCase<OrgAddressBorderWiseSubscriber>
	{
		public void TestOrgAddressBorderWiseSubscriber()
		{
			var subscriber = NewDataChangeSubscriber();
			AssertNotNull(subscriber);

			AssertEquals("BOA", subscriber.Code);
			AssertEquals(OrgAddressSchema.Constants.TableName, subscriber.Table.TableName);
			Assert(subscriber.NotifyInsert);
			Assert(subscriber.NotifyDelete);
			Assert(subscriber.NotifyUpdate);
			AssertEquals("Should group all messages with same constant key", "EdiProd2BWUPM-Sync", ((OrgAddressBorderWiseSubscriber)subscriber).MessageKey);

			AssertContainsExactElementsInAnyOrder(
				new string[]
				{
					OrgAddressSchema.Constants.OA_Code,
					OrgAddressSchema.Constants.OA_Address1,
					OrgAddressSchema.Constants.OA_Address2,
					OrgAddressSchema.Constants.OA_City,
					OrgAddressSchema.Constants.OA_State,
					OrgAddressSchema.Constants.OA_PostCode,
					OrgAddressSchema.Constants.OA_RN_NKCountryCode,
					OrgAddressSchema.Constants.OA_Phone,
					OrgAddressSchema.Constants.OA_Email,
					OrgAddressSchema.Constants.OA_Language,
					OrgAddressSchema.Constants.OA_IsActive,
					OrgAddressSchema.Constants.OA_OH,
				},
				subscriber.SpecificColumns.Select(c => c.Name));
		}

		public void TestProcessUpdate()
		{
			var changeTable = new DataTable();
			changeTable.Columns.Add(OrgAddressSchema.Constants.PK, typeof(Guid));
			changeTable.Columns.Add(OrgAddressSchema.Constants.OA_Code, typeof(string));
			changeTable.Columns.Add(OrgAddressSchema.Constants.OA_Address1, typeof(string));
			changeTable.Columns.Add(OrgAddressSchema.Constants.OA_Address2, typeof(string));
			changeTable.Columns.Add(OrgAddressSchema.Constants.OA_City, typeof(string));
			changeTable.Columns.Add(OrgAddressSchema.Constants.OA_State, typeof(string));
			changeTable.Columns.Add(OrgAddressSchema.Constants.OA_PostCode, typeof(string));
			changeTable.Columns.Add(OrgAddressSchema.Constants.OA_RN_NKCountryCode, typeof(string));
			changeTable.Columns.Add(OrgAddressSchema.Constants.OA_Phone, typeof(string));
			changeTable.Columns.Add(OrgAddressSchema.Constants.OA_Email, typeof(string));
			changeTable.Columns.Add(OrgAddressSchema.Constants.OA_Language, typeof(string));
			changeTable.Columns.Add(OrgAddressSchema.Constants.OA_IsActive, typeof(bool));
			changeTable.Columns.Add(OrgAddressSchema.Constants.OA_OH, typeof(Guid));
			changeTable.Columns.Add(AuditFieldNames.StartLsnFieldName, typeof(byte[]));
			changeTable.Columns.Add(AuditFieldNames.SeqValFieldName, typeof(byte[]));
			changeTable.Columns.Add(AuditFieldNames.CommandIdFieldName, typeof(int));
			changeTable.Columns.Add("ExtraColumn", typeof(string));

			var changeRow = changeTable.NewRow();

			var addressPK = Guid.NewGuid();
			var orgPK1 = Guid.NewGuid();
			var orgPK2 = Guid.NewGuid();

			changeRow[OrgAddressSchema.Constants.PK] = addressPK;
			changeRow[OrgAddressSchema.Constants.OA_Code] = "Address 1 ";
			changeRow[OrgAddressSchema.Constants.OA_Address1] = "1 Short Street";
			changeRow[OrgAddressSchema.Constants.OA_Address2] = "Big House";
			changeRow[OrgAddressSchema.Constants.OA_City] = "Town";
			changeRow[OrgAddressSchema.Constants.OA_State] = "AA";
			changeRow[OrgAddressSchema.Constants.OA_PostCode] = "1234";
			changeRow[OrgAddressSchema.Constants.OA_RN_NKCountryCode] = "ZZ";
			changeRow[OrgAddressSchema.Constants.OA_Phone] = "1234567";
			changeRow[OrgAddressSchema.Constants.OA_Email] = "aaa@aaa.aaa";
			changeRow[OrgAddressSchema.Constants.OA_Language] = "EN-GB";
			changeRow[OrgAddressSchema.Constants.OA_IsActive] = false;
			changeRow[OrgAddressSchema.Constants.OA_OH] = orgPK1;
			changeRow[AuditFieldNames.StartLsnFieldName] = new byte[] { 0, 0 };
			changeRow[AuditFieldNames.SeqValFieldName] = new byte[] { 0, 1 };
			changeRow[AuditFieldNames.CommandIdFieldName] = 1;
			changeRow["ExtraColumn"] = "ABC";

			changeTable.Rows.Add(changeRow);
			changeRow.AcceptChanges();

			changeRow[OrgAddressSchema.Constants.OA_Code] = "Address 2 ";
			changeRow[OrgAddressSchema.Constants.OA_Address1] = "2 Long Road";
			changeRow[OrgAddressSchema.Constants.OA_Address2] = "Small Unit";
			changeRow[OrgAddressSchema.Constants.OA_City] = "Village";
			changeRow[OrgAddressSchema.Constants.OA_State] = "ZZ";
			changeRow[OrgAddressSchema.Constants.OA_PostCode] = "4321";
			changeRow[OrgAddressSchema.Constants.OA_RN_NKCountryCode] = "AA";
			changeRow[OrgAddressSchema.Constants.OA_Phone] = "7654321";
			changeRow[OrgAddressSchema.Constants.OA_Email] = "zzz@zzz.zzz";
			changeRow[OrgAddressSchema.Constants.OA_Language] = "EN-US";
			changeRow[OrgAddressSchema.Constants.OA_IsActive] = true;
			changeRow[OrgAddressSchema.Constants.OA_OH] = orgPK2;
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
    ""PK"": ""{addressPK}"",
    ""IsActive"": false,
    ""Code"": ""Address 1 "",
    ""Address1"": ""1 Short Street"",
    ""Address2"": ""Big House"",
    ""City"": ""Town"",
    ""State"": ""AA"",
    ""PostCode"": ""1234"",
    ""CountryCode"": ""ZZ"",
    ""Phone"": ""1234567"",
    ""Email"": ""aaa@aaa.aaa"",
    ""Language"": ""EN-GB"",
    ""OrgPk"": ""{orgPK1}""
  }},
  ""CurrentVersion"": {{
    ""BorderWisePK"": ""{Guid.Empty}"",
    ""PK"": ""{addressPK}"",
    ""IsActive"": true,
    ""Code"": ""Address 2 "",
    ""Address1"": ""2 Long Road"",
    ""Address2"": ""Small Unit"",
    ""City"": ""Village"",
    ""State"": ""ZZ"",
    ""PostCode"": ""4321"",
    ""CountryCode"": ""AA"",
    ""Phone"": ""7654321"",
    ""Email"": ""zzz@zzz.zzz"",
    ""Language"": ""EN-US"",
    ""OrgPk"": ""{orgPK2}""
  }},
  ""Source"": ""{MessageSources.EdiProdAddressChange}"",
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
			var changeTable = new DataTable();
			changeTable.Columns.Add(OrgAddressSchema.Constants.PK, typeof(Guid));
			changeTable.Columns.Add(OrgAddressSchema.Constants.OA_Code, typeof(string));
			changeTable.Columns.Add(OrgAddressSchema.Constants.OA_Address1, typeof(string));
			changeTable.Columns.Add(OrgAddressSchema.Constants.OA_Address2, typeof(string));
			changeTable.Columns.Add(OrgAddressSchema.Constants.OA_City, typeof(string));
			changeTable.Columns.Add(OrgAddressSchema.Constants.OA_State, typeof(string));
			changeTable.Columns.Add(OrgAddressSchema.Constants.OA_PostCode, typeof(string));
			changeTable.Columns.Add(OrgAddressSchema.Constants.OA_RN_NKCountryCode, typeof(string));
			changeTable.Columns.Add(OrgAddressSchema.Constants.OA_Phone, typeof(string));
			changeTable.Columns.Add(OrgAddressSchema.Constants.OA_Email, typeof(string));
			changeTable.Columns.Add(OrgAddressSchema.Constants.OA_Language, typeof(string));
			changeTable.Columns.Add(OrgAddressSchema.Constants.OA_IsActive, typeof(bool));
			changeTable.Columns.Add(OrgAddressSchema.Constants.OA_OH, typeof(Guid));
			changeTable.Columns.Add(AuditFieldNames.StartLsnFieldName, typeof(byte[]));
			changeTable.Columns.Add(AuditFieldNames.SeqValFieldName, typeof(byte[]));
			changeTable.Columns.Add(AuditFieldNames.CommandIdFieldName, typeof(int));
			changeTable.Columns.Add("ExtraColumn", typeof(string));

			var changeRow = changeTable.NewRow();

			var addressPK = Guid.NewGuid();
			var orgPK = Guid.NewGuid();

			changeRow[OrgAddressSchema.Constants.PK] = addressPK;
			changeRow[OrgAddressSchema.Constants.OA_Code] = "Address 1";
			changeRow[OrgAddressSchema.Constants.OA_Address1] = "1 Short Street";
			changeRow[OrgAddressSchema.Constants.OA_Address2] = "Big House";
			changeRow[OrgAddressSchema.Constants.OA_City] = "Town";
			changeRow[OrgAddressSchema.Constants.OA_State] = "AA";
			changeRow[OrgAddressSchema.Constants.OA_PostCode] = "1234";
			changeRow[OrgAddressSchema.Constants.OA_RN_NKCountryCode] = "ZZ";
			changeRow[OrgAddressSchema.Constants.OA_Phone] = "1234567";
			changeRow[OrgAddressSchema.Constants.OA_Email] = "aaa@aaa.aaa";
			changeRow[OrgAddressSchema.Constants.OA_Language] = "EN-GB";
			changeRow[OrgAddressSchema.Constants.OA_IsActive] = false;
			changeRow[OrgAddressSchema.Constants.OA_OH] = orgPK;
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
    ""PK"": ""{addressPK}"",
    ""IsActive"": false,
    ""Code"": ""Address 1"",
    ""Address1"": ""1 Short Street"",
    ""Address2"": ""Big House"",
    ""City"": ""Town"",
    ""State"": ""AA"",
    ""PostCode"": ""1234"",
    ""CountryCode"": ""ZZ"",
    ""Phone"": ""1234567"",
    ""Email"": ""aaa@aaa.aaa"",
    ""Language"": ""EN-GB"",
    ""OrgPk"": ""{orgPK}""
  }},
  ""CurrentVersion"": null,
  ""Source"": ""{MessageSources.EdiProdAddressChange}"",
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
			var changeTable = new DataTable();
			changeTable.Columns.Add(OrgAddressSchema.Constants.PK, typeof(Guid));
			changeTable.Columns.Add(OrgAddressSchema.Constants.OA_Code, typeof(string));
			changeTable.Columns.Add(OrgAddressSchema.Constants.OA_Address1, typeof(string));
			changeTable.Columns.Add(OrgAddressSchema.Constants.OA_Address2, typeof(string));
			changeTable.Columns.Add(OrgAddressSchema.Constants.OA_City, typeof(string));
			changeTable.Columns.Add(OrgAddressSchema.Constants.OA_State, typeof(string));
			changeTable.Columns.Add(OrgAddressSchema.Constants.OA_PostCode, typeof(string));
			changeTable.Columns.Add(OrgAddressSchema.Constants.OA_RN_NKCountryCode, typeof(string));
			changeTable.Columns.Add(OrgAddressSchema.Constants.OA_Phone, typeof(string));
			changeTable.Columns.Add(OrgAddressSchema.Constants.OA_Email, typeof(string));
			changeTable.Columns.Add(OrgAddressSchema.Constants.OA_Language, typeof(string));
			changeTable.Columns.Add(OrgAddressSchema.Constants.OA_IsActive, typeof(bool));
			changeTable.Columns.Add(OrgAddressSchema.Constants.OA_OH, typeof(Guid));
			changeTable.Columns.Add(AuditFieldNames.StartLsnFieldName, typeof(byte[]));
			changeTable.Columns.Add(AuditFieldNames.SeqValFieldName, typeof(byte[]));
			changeTable.Columns.Add(AuditFieldNames.CommandIdFieldName, typeof(int));
			changeTable.Columns.Add("ExtraColumn", typeof(string));

			var changeRow = changeTable.NewRow();

			var addressPK = Guid.NewGuid();
			var orgPK = Guid.NewGuid();
			var addressBorderWisePK = Guid.NewGuid();

			changeRow[OrgAddressSchema.Constants.PK] = addressPK;
			changeRow[OrgAddressSchema.Constants.OA_Code] = "Address 1";
			changeRow[OrgAddressSchema.Constants.OA_Address1] = "1 Short Street";
			changeRow[OrgAddressSchema.Constants.OA_Address2] = "Big House";
			changeRow[OrgAddressSchema.Constants.OA_City] = "Town";
			changeRow[OrgAddressSchema.Constants.OA_State] = "AA";
			changeRow[OrgAddressSchema.Constants.OA_PostCode] = "1234";
			changeRow[OrgAddressSchema.Constants.OA_RN_NKCountryCode] = "ZZ";
			changeRow[OrgAddressSchema.Constants.OA_Phone] = "1234567";
			changeRow[OrgAddressSchema.Constants.OA_Email] = "aaa@aaa.aaa";
			changeRow[OrgAddressSchema.Constants.OA_Language] = "EN-GB";
			changeRow[OrgAddressSchema.Constants.OA_IsActive] = false;
			changeRow[OrgAddressSchema.Constants.OA_OH] = orgPK;
			changeRow[AuditFieldNames.StartLsnFieldName] = new byte[] { 0, 0 };
			changeRow[AuditFieldNames.SeqValFieldName] = new byte[] { 0, 1 };
			changeRow[AuditFieldNames.CommandIdFieldName] = 1;
			changeRow["ExtraColumn"] = "ABC";

			changeTable.Rows.Add(changeRow);

			var factory = new BusinessObjectFactory { RefreshEnabled = false };
			var pkMapDataRecord = factory.New<StmData>();
			pkMapDataRecord.SD_Name = OrgAddressBorderWiseSubscriber.AddressPkMapStmDataName;
			pkMapDataRecord.SD_Type = "BOR";
			pkMapDataRecord.SD_Owner = addressPK;
			pkMapDataRecord.SD_DepartmentGuid = addressBorderWisePK;
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
    ""BorderWisePK"": ""{addressBorderWisePK}"",
    ""PK"": ""{addressPK}"",
    ""IsActive"": false,
    ""Code"": ""Address 1"",
    ""Address1"": ""1 Short Street"",
    ""Address2"": ""Big House"",
    ""City"": ""Town"",
    ""State"": ""AA"",
    ""PostCode"": ""1234"",
    ""CountryCode"": ""ZZ"",
    ""Phone"": ""1234567"",
    ""Email"": ""aaa@aaa.aaa"",
    ""Language"": ""EN-GB"",
    ""OrgPk"": ""{orgPK}""
  }},
  ""Source"": ""{MessageSources.EdiProdAddressChange}"",
  ""ChangeType"": {(int)ChangeType.Acknowledgement},
  ""ChangeSequence"": {{
    ""TransactionLsn"": ""AAA="",
    ""SequenceValue"": ""AAE="",
    ""CommandId"": 1
  }}
}}";
			AssertEquals(expectedMessage, publisher.Entries[0].Value);
		}

		public override void TestCustomFilter()
		{
			var subscriber = new OrgAddressBorderWiseSubscriber();
			AssertNull(subscriber.CustomFilter);
		}

		protected override DataTable GetTestDataTable() => null;
	}

	class OrgAddressBorderWiseSubscriberForTest : OrgAddressBorderWiseSubscriber
	{
		protected override OrgMergeMessageHelper GetNewOrgMergeMessageHelper()
		{
			return new OrgMergeMessageHelperForTest { HasPotentialOrgHeaderChangeOverride = false, TransactionEndTimeOverride = ZDateTime.Empty };
		}
	}
}
