using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BorderWise.Sync;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.AuditDataServices.BorderWise.Subscribers;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.BorderWise.Test
{
	[TestedType(typeof(OrgHeaderBorderWiseSubscriber))]
	class OrgHeaderBorderWiseSubscriberTest : BorderWiseOrgSubscriberTestCase<OrgHeaderBorderWiseSubscriber>
	{
		public void TestOrgHeaderBorderWiseSubscriber()
		{
			var subscriber = NewDataChangeSubscriber();
			AssertNotNull(subscriber);
			Assert(subscriber is OrgHeaderBorderWiseSubscriber);

			AssertEquals("BOH", subscriber.Code);
			AssertEquals(OrgHeaderSchema.Constants.TableName, subscriber.Table.TableName);
			Assert(subscriber.NotifyInsert);
			Assert(subscriber.NotifyDelete);
			Assert(subscriber.NotifyUpdate);
			AssertEquals("Should group all messages with same constant key", "EdiProd2BWUPM-Sync", ((OrgHeaderBorderWiseSubscriber)subscriber).MessageKey);

			AssertNull(subscriber.SpecificColumns);
		}

		public void TestProcessAdd()
		{
			var changeTable = GetTestDataTable();
			var changeRow = changeTable.NewRow();

			var factory = new BusinessObjectFactory { RefreshEnabled = false };
			var org = factory.NewWithValidTestData<OrgHeader>();
			factory.Save();
			var orgBorderWisePK = Guid.NewGuid();
			var orgPK = org.PK.ToGuid();

			InitializeChangeRow(changeRow, orgPK, "CODE1", "Full Name One ", false, "EN-US");
			var orgAddress = SetupOrgAddress(factory, orgPK);

			changeTable.Rows.Add(changeRow);

			var publisher = new BorderWiseChangesPublisherForTest();
			using (TestHelper.EnableSyncForTest(publisher))
			using (ObjectFactory.Substitute<IOrgLicencesHelper>(new OrgLicencesHelperForTest()))
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
    ""PK"": ""{orgPK}"",
    ""Code"": ""CODE1"",
    ""FullName"": ""Full Name One "",
    ""IsActive"": false,
    ""Language"": ""EN-US"",
    ""SecurityRightGranted"": true,
    ""EdiProdLicences"": [
      {{
        ""CompanyNumber"": 100,
        ""DatabaseNumber"": 200,
        ""Product"": ""BOR"",
        ""LicenseType"": ""PRD""
      }},
      {{
        ""CompanyNumber"": 100,
        ""DatabaseNumber"": 201,
        ""Product"": ""CW1"",
        ""LicenseType"": ""TRN""
      }}
    ],
    ""OrgAddresses"": [
      {{
        ""PK"": ""{org.Addresses.First().PK.ToGuid()}"",
        ""IsActive"": true,
        ""Code"": ""#1"",
        ""Address1"": ""#1"",
        ""Address2"": """",
        ""City"": """",
        ""State"": """",
        ""PostCode"": """",
        ""CountryCode"": """",
        ""Phone"": """",
        ""Email"": """",
        ""Language"": ""EN"",
        ""OrgPk"": ""{orgPK}""
      }},
      {{
        ""PK"": ""{orgAddress.PK.ToGuid()}"",
        ""IsActive"": true,
        ""Code"": ""address line 1"",
        ""Address1"": ""address line 1"",
        ""Address2"": ""address line 2"",
        ""City"": ""Sydney"",
        ""State"": ""NSW"",
        ""PostCode"": ""2000"",
        ""CountryCode"": ""AU"",
        ""Phone"": ""12345678"",
        ""Email"": ""email"",
        ""Language"": ""EN"",
        ""OrgPk"": ""{orgPK}""
      }}
    ]
  }},
  ""Source"": ""{MessageSources.EdiProdOrgChange}"",
  ""ChangeType"": {(int)ChangeType.Add},
  ""ChangeSequence"": {{
    ""TransactionLsn"": ""AAA="",
    ""SequenceValue"": ""AAE="",
    ""CommandId"": 1
  }}
}}";
			AssertEquals(expectedMessage, publisher.Entries[0].Value);

			var dataRecordFilter = new ZQuery(StmDataSchema.SD_Name, OrgHeaderBorderWiseSubscriber.OrgPkMapStmDataName);
			dataRecordFilter.AddToFilter(StmDataSchema.SD_Type, "BOR");
			dataRecordFilter.AddToFilter(StmDataSchema.SD_Owner, orgPK);
			dataRecordFilter.AddToFilter(StmDataSchema.SD_DepartmentGuid, orgBorderWisePK);

			var dataRecord = new BusinessObjectFactory { RefreshEnabled = false }.LoadTop1<StmData>(dataRecordFilter);

			AssertNull(dataRecord);
		}

		public void TestProcessUpdate()
		{
			var changeTable = GetTestDataTable();
			var changeRow = changeTable.NewRow();
			var orgPK = Guid.NewGuid();

			InitializeChangeRow(changeRow, orgPK, "CODE1", "Full Name One ", false, "EN-US");
			changeRow["ExtraColumn"] = "ABC";

			changeTable.Rows.Add(changeRow);
			changeRow.AcceptChanges();

			InitializeChangeRow(changeRow, orgPK, "CODE2", "Full Name Two ", true, "EN-AU");
			changeRow["ExtraColumn"] = "XYZ";

			var publisher = new BorderWiseChangesPublisherForTest();
			using (TestHelper.EnableSyncForTest(publisher))
			using (ObjectFactory.Substitute<IOrgLicencesHelper>(new OrgLicencesHelperForTest()))
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
    ""PK"": ""{orgPK}"",
    ""Code"": ""CODE1"",
    ""FullName"": ""Full Name One "",
    ""IsActive"": false,
    ""Language"": ""EN-US"",
    ""SecurityRightGranted"": true,
    ""EdiProdLicences"": [],
    ""OrgAddresses"": []
  }},
  ""CurrentVersion"": {{
    ""BorderWisePK"": ""{Guid.Empty}"",
    ""PK"": ""{orgPK}"",
    ""Code"": ""CODE2"",
    ""FullName"": ""Full Name Two "",
    ""IsActive"": true,
    ""Language"": ""EN-AU"",
    ""SecurityRightGranted"": true,
    ""EdiProdLicences"": [
      {{
        ""CompanyNumber"": 100,
        ""DatabaseNumber"": 200,
        ""Product"": ""BOR"",
        ""LicenseType"": ""PRD""
      }},
      {{
        ""CompanyNumber"": 100,
        ""DatabaseNumber"": 201,
        ""Product"": ""CW1"",
        ""LicenseType"": ""TRN""
      }}
    ],
    ""OrgAddresses"": []
  }},
  ""Source"": ""{MessageSources.EdiProdOrgChange}"",
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
			var orgPK = Guid.NewGuid();
			var changeRow = CreateChangeRow(changeTable, orgPK, "CODE1", "Full Name One", false, "EN-US");

			changeRow.Delete();

			var publisher = new BorderWiseChangesPublisherForTest();
			using (TestHelper.EnableSyncForTest(publisher))
			using (ObjectFactory.Substitute<IOrgLicencesHelper>(new OrgLicencesHelperForTest()))
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
    ""PK"": ""{orgPK}"",
    ""Code"": ""CODE1"",
    ""FullName"": ""Full Name One"",
    ""IsActive"": false,
    ""Language"": ""EN-US"",
    ""SecurityRightGranted"": true,
    ""EdiProdLicences"": [],
    ""OrgAddresses"": []
  }},
  ""CurrentVersion"": null,
  ""Source"": ""{MessageSources.EdiProdOrgChange}"",
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
			var orgPK = Guid.NewGuid();
			var orgBorderWisePK = Guid.NewGuid();

			InitializeChangeRow(changeRow, orgPK, "CODE1", "Full Name One", false, "EN-US");

			changeTable.Rows.Add(changeRow);

			var factory = new BusinessObjectFactory { RefreshEnabled = false };
			var pkMapDataRecord = factory.New<StmData>();
			pkMapDataRecord.SD_Name = OrgHeaderBorderWiseSubscriber.OrgPkMapStmDataName;
			pkMapDataRecord.SD_Type = "BOR";
			pkMapDataRecord.SD_Owner = orgPK;
			pkMapDataRecord.SD_DepartmentGuid = orgBorderWisePK;
			factory.Save();

			var publisher = new BorderWiseChangesPublisherForTest();
			using (TestHelper.EnableSyncForTest(publisher))
			using (ObjectFactory.Substitute<IOrgLicencesHelper>(new OrgLicencesHelperForTest()))
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
    ""BorderWisePK"": ""{orgBorderWisePK}"",
    ""PK"": ""{orgPK}"",
    ""Code"": ""CODE1"",
    ""FullName"": ""Full Name One"",
    ""IsActive"": false,
    ""Language"": ""EN-US"",
    ""SecurityRightGranted"": true,
    ""EdiProdLicences"": [
      {{
        ""CompanyNumber"": 100,
        ""DatabaseNumber"": 200,
        ""Product"": ""BOR"",
        ""LicenseType"": ""PRD""
      }},
      {{
        ""CompanyNumber"": 100,
        ""DatabaseNumber"": 201,
        ""Product"": ""CW1"",
        ""LicenseType"": ""TRN""
      }}
    ],
    ""OrgAddresses"": []
  }},
  ""Source"": ""{MessageSources.EdiProdOrgChange}"",
  ""ChangeType"": {(int)ChangeType.Acknowledgement},
  ""ChangeSequence"": {{
    ""TransactionLsn"": ""AAA="",
    ""SequenceValue"": ""AAE="",
    ""CommandId"": 1
  }}
}}";
			AssertEquals(expectedMessage, publisher.Entries[0].Value);

			var dataRecordFilter = new ZQuery(StmDataSchema.SD_Name, OrgHeaderBorderWiseSubscriber.OrgPkMapStmDataName);
			dataRecordFilter.AddToFilter(StmDataSchema.SD_Type, "BOR");
			dataRecordFilter.AddToFilter(StmDataSchema.SD_Owner, orgPK);
			dataRecordFilter.AddToFilter(StmDataSchema.SD_DepartmentGuid, orgBorderWisePK);

			var dataRecord = new BusinessObjectFactory { RefreshEnabled = false }.LoadTop1<StmData>(dataRecordFilter);

			AssertNull(dataRecord);
		}

		public void TestProcessMerge()
		{
			var factory = new BusinessObjectFactory { RefreshEnabled = false };

			var targetOrg = factory.NewWithValidTestData<OrgHeader>();
			targetOrg.OH_FullName = "New Org";
			targetOrg.OH_Code = "NEW_ORG";
			targetOrg.OH_IsActive = true;
			targetOrg.OH_Language = "CN";

			var oldOrg = factory.NewWithValidTestData<OrgHeader>();
			var oldOrgPk = oldOrg.PK.ToGuid();
			var mergeLog = factory.New<StmALog>();
			using (mergeLog.LockForUpdatingKeyFieldsForTesting())
			{
				mergeLog.SL_Parent = oldOrgPk;
				mergeLog.SL_Table = OrgHeaderSchema.Constants.TableName;
				mergeLog.SL_SE_NKEvent = OrganisationMerger.MergedLogEvent.Code;
				mergeLog.SL_Reference = "This organisation was merged into organisation NEW_ORG|" + targetOrg.PK;
			}

			factory.Save();

			var changeTable = GetTestDataTable();

			var changeRow = CreateChangeRow(changeTable, oldOrgPk, "OLD_ORG", "Old Org", true, "EN");

			changeRow[OrgHeaderSchema.Constants.OH_IsActive] = false;

			var publisher = new BorderWiseChangesPublisherForTest();
			using (TestHelper.EnableSyncForTest(publisher))
			{
				var subscriber = new OrgHeaderBorderWiseSubscriberForTest() as ActualDataChangesAuditSubscriber;
				using (ObjectFactory.Substitute<IOrgLicencesHelper>(new OrgLicencesHelperForTest()))
				{
					subscriber.ProcessChanges(new LoggerForTest(), changeTable);
				}
			}

			AssertEquals(1, publisher.Entries.Count);
			AssertEquals("EdiProd2BWUPM-Sync", publisher.Entries[0].Key);

			var expectedMessage =
$@"{{
  ""OriginalVersion"": {{
    ""BorderWisePK"": ""{Guid.Empty}"",
    ""PK"": ""{oldOrgPk}"",
    ""Code"": ""OLD_ORG"",
    ""FullName"": ""Old Org"",
    ""IsActive"": false,
    ""Language"": ""EN"",
    ""SecurityRightGranted"": true,
    ""EdiProdLicences"": [
      {{
        ""CompanyNumber"": 100,
        ""DatabaseNumber"": 200,
        ""Product"": ""BOR"",
        ""LicenseType"": ""PRD""
      }},
      {{
        ""CompanyNumber"": 100,
        ""DatabaseNumber"": 201,
        ""Product"": ""CW1"",
        ""LicenseType"": ""TRN""
      }}
    ],
    ""OrgAddresses"": []
  }},
  ""CurrentVersion"": {{
    ""BorderWisePK"": ""{Guid.Empty}"",
    ""PK"": ""{targetOrg.PK}"",
    ""Code"": ""NEW_ORG"",
    ""FullName"": ""New Org"",
    ""IsActive"": true,
    ""Language"": ""CN"",
    ""SecurityRightGranted"": true,
    ""EdiProdLicences"": [
      {{
        ""CompanyNumber"": 100,
        ""DatabaseNumber"": 200,
        ""Product"": ""BOR"",
        ""LicenseType"": ""PRD""
      }},
      {{
        ""CompanyNumber"": 100,
        ""DatabaseNumber"": 201,
        ""Product"": ""CW1"",
        ""LicenseType"": ""TRN""
      }}
    ],
    ""OrgAddresses"": []
  }},
  ""Source"": ""{MessageSources.EdiProdOrgChange}"",
  ""ChangeType"": {(int)ChangeType.Merge},
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
			var subscriber = new OrgHeaderBorderWiseSubscriber();
			AssertNull(subscriber.CustomFilter);
		}

		protected override DataTable GetTestDataTable()
		{
			var changeTable = new DataTable();
			changeTable.Columns.Add(OrgHeaderSchema.Constants.PK, typeof(Guid));
			changeTable.Columns.Add(OrgHeaderSchema.Constants.OH_Code, typeof(string));
			changeTable.Columns.Add(OrgHeaderSchema.Constants.OH_FullName, typeof(string));
			changeTable.Columns.Add(OrgHeaderSchema.Constants.OH_IsActive, typeof(bool));
			changeTable.Columns.Add(OrgHeaderSchema.Constants.OH_Language, typeof(string));
			changeTable.Columns.Add(OrgHeaderSchema.Constants.OH_SystemLastEditUser, typeof(string));
			changeTable.Columns.Add(AuditFieldNames.StartLsnFieldName, typeof(byte[]));
			changeTable.Columns.Add(AuditFieldNames.SeqValFieldName, typeof(byte[]));
			changeTable.Columns.Add(AuditFieldNames.CommandIdFieldName, typeof(int));
			changeTable.Columns.Add("ExtraColumn", typeof(string));
			return changeTable;
		}

		DataRow CreateChangeRow(DataTable changeTable, Guid pk, string code, string fullName, bool isActive, string language)
		{
			var changeRow = changeTable.NewRow();
			InitializeChangeRow(changeRow, pk, code, fullName, isActive, language);
			changeTable.Rows.Add(changeRow);
			changeRow.AcceptChanges();
			return changeRow;
		}

		void InitializeChangeRow(DataRow changeRow, Guid pk, string code, string fullName, bool isActive, string language)
		{
			changeRow[OrgHeaderSchema.Constants.PK] = pk;
			changeRow[OrgHeaderSchema.Constants.OH_Code] = code;
			changeRow[OrgHeaderSchema.Constants.OH_FullName] = fullName;
			changeRow[OrgHeaderSchema.Constants.OH_IsActive] = isActive;
			changeRow[OrgHeaderSchema.Constants.OH_Language] = language;
			changeRow[OrgHeaderSchema.Constants.OH_SystemLastEditUser] = "ABC";
			changeRow[AuditFieldNames.StartLsnFieldName] = new byte[] { 0, 0 };
			changeRow[AuditFieldNames.SeqValFieldName] = new byte[] { 0, 1 };
			changeRow[AuditFieldNames.CommandIdFieldName] = 1;
		}

		OrgAddress SetupOrgAddress(BusinessObjectFactory factory, Guid orgPk)
		{
			var orgAddress = factory.New<OrgAddress>();
			orgAddress.OA_Address1 = "address line 1";
			orgAddress.OA_Address2 = "address line 2";
			orgAddress.OA_City = "Sydney";
			orgAddress.OA_State = "NSW";
			orgAddress.OA_PostCode = "2000";
			orgAddress.OA_RN_NKCountryCode = "AU";
			orgAddress.OA_Phone = "12345678";
			orgAddress.OA_Email = "email";
			orgAddress.OA_Language = "EN";
			orgAddress.OA_IsActive = true;
			orgAddress.OA_OH = orgPk;

			var addressCapacibility = factory.New<OrgAddressCapability>();
			addressCapacibility.PZ_AddressType = "OFC";
			addressCapacibility.PZ_IsMainAddress = true;
			addressCapacibility.PZ_OA = orgAddress.PK;
			factory.Save();

			return orgAddress;
		}

		class OrgLicencesHelperForTest : IOrgLicencesHelper
		{
			public IEnumerable<IOrgLicenceInfo> GetOrgLicences(ZGuid orgPk)
			{
				return new IOrgLicenceInfo[] {
					new OrgLicenceInfo { CompanyNumber = 100, DatabaseNumber = 200, Product = "BOR", LicenseType = "PRD" },
					new OrgLicenceInfo { CompanyNumber = 100, DatabaseNumber = 201, Product = "CW1", LicenseType = "TRN" }
				};
			}

			class OrgLicenceInfo : IOrgLicenceInfo
			{
				public int CompanyNumber { get; set; }
				public int DatabaseNumber { get; set; }
				public string Product { get; set; }
				public string LicenseType { get; set; }
			}
		}
	}

	class OrgHeaderBorderWiseSubscriberForTest : OrgHeaderBorderWiseSubscriber
	{
		protected override OrgMergeMessageHelper GetNewOrgMergeMessageHelper()
		{
			return new OrgMergeMessageHelperForTest { HasPotentialOrgHeaderChangeOverride = true, TransactionEndTimeOverride = ZDateTime.UtcNow.AddDays(1) };
		}
	}
}
