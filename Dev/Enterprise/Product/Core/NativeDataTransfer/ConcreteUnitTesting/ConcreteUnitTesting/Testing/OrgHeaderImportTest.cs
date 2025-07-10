using System;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Tools.Telephony;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.Adapter;
using Enterprise.DataTransfer.Native.Adapter.Utils;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Finders;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Repository;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.ConcreteUnitTesting
{
	class OrgHeaderImportTest : TestCaseWithFactory
	{
		public void TestLogsArentAddedToTablesThatAreNotModified()
		{
			var factory = new BusinessObjectFactory();
			var countryAU = factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			var locodeAUSYD = factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");

			var logFilter = new ZQuery(StmALogSchema.SL_Parent, new[] { countryAU.PK, locodeAUSYD.PK });
			var stmaLogCount = factory.GetDatabaseCount(typeof(StmALog), logFilter);

			string actualLog = XML_OrgWithMergeActionsOnRelatedTables.ImportNativeXmlReturningLog();

			CombineAssertions(delegate
			{
				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
ClosestPort - 0 inserts, 0 updates, 0 deletes
OrgHeader - 1 inserts, 0 updates, 0 deletes
RelatedPortCode - 0 inserts, 0 updates, 0 deletes
OrgAddress - 1 inserts, 0 updates, 0 deletes
OrgAddressCapability - 1 inserts, 0 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Precondition: Log Text.", expectedLog, actualLog);
				AssertEquals("Before and After count of StmALog rows for the RefCountry and RefUNLOCO tables", stmaLogCount, factory.GetDatabaseCount(typeof(StmALog), logFilter));
			});
		}

		[TestDate(2011, 5, 5)]
		public void TestOrgHeader_NoEdtEventOnChildUpdate()
		{
			XML_OrgWithMergeActionsOnRelatedTables.ImportNativeXmlReturningLog();

			var orgHeader = new BusinessObjectFactory().Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "SOGGOVSYD")).Single();
			AssertEquals("Precondition: No EDT Event", 0, orgHeader.GetLogs().Find(l => l.SL_SE_NKEvent == "EDT").Count());

			TestDateAttribute.AddDays(1);
			//Change Native XML to update dbo.OrgAddress object
			XML_OrgWithMergeActionsOnRelatedTables.Replace("<City>Blocktown</City>", "<City>New City</City>").ImportNativeXmlReturningLog();
			orgHeader = new BusinessObjectFactory().Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "SOGGOVSYD")).Single();

			AssertEquals("Org header gets no EDT event when child is edited", 0, orgHeader.GetLogs().Find(l => l.SL_SE_NKEvent == "EDT").Count());
			AssertEquals("Org header audit fields update when child is edited", ZDateTime.Now, orgHeader.OH_SystemLastEditTimeUtc);
		}

		[TestDate(2000, 1, 5)]
		public void TestParentLastEditTime_RegistryEnabled()
			=> TestParentLastEditTime(disableLastEditUpdateOfParentInNativeXmlRegistry: "OrgHeader", expectedParentLastEditTime: new ZDateTime(2000, 1, 5));

		[TestDate(2000, 1, 5)]
		public void TestParentLastEditTime_RegistryDisabled()
			=> TestParentLastEditTime(disableLastEditUpdateOfParentInNativeXmlRegistry: "", expectedParentLastEditTime: new ZDateTime(2000, 1, 6));

		void TestParentLastEditTime(string disableLastEditUpdateOfParentInNativeXmlRegistry, ZDateTime expectedParentLastEditTime)
		{
			using (SystemDataRegistry.Instance.DisableLastEditUpdateOfParentInNativeXml.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, disableLastEditUpdateOfParentInNativeXmlRegistry))
			{
				XML_OrgWithMergeActionsOnRelatedTables.ImportNativeXmlReturningLog();

				var orgHeader = new BusinessObjectFactory().Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "SOGGOVSYD")).Single();

				TestDateAttribute.AddDays(1);
				XML_OrgWithMergeActionsOnRelatedTables.Replace("<City>Blocktown</City>", "<City>New City</City>").ImportNativeXmlReturningLog();
				orgHeader = new BusinessObjectFactory().Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "SOGGOVSYD")).Single();

				AssertEquals("Parent LastEditTime", expectedParentLastEditTime, orgHeader.OH_SystemLastEditTimeUtc);
				AssertContainsExactElementsInAnyOrder
				(
					"Parent Logs",
					new []
					{
						"DIM|This Organization (SOGGOVSYD)",
						"DIM|This Organization (SOGGOVSYD)"
					},
					orgHeader.Logs.GetAllLogs().Cast<StmALog>().Select(x => $"{x.SL_SE_NKEvent}|{x.SL_TableFriendlyName}")
				);
			}
		}

		#region XML_OrgWithMergeActionsOnRelatedTables

		const string XML_OrgWithMergeActionsOnRelatedTables = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Body>
    <Organization version=""2.0"">
      <OrgHeader Action=""MERGE"">
        <Code>SOGGOVSYD</Code>
        <FullName>SOG Govett</FullName>
        <Language>EN</Language>
        <IsActive>true</IsActive>
        <IsConsignor>true</IsConsignor>
        <OrgAddressCollection>
          <OrgAddress Action=""MERGE"">
            <Code>102 Crazy Horse Drive</Code>
            <Language>EN</Language>
            <CompanyNameOverride></CompanyNameOverride>
            <Address1>102 Crazy Horse Drive</Address1>
            <Address2></Address2>
            <State>NSW</State>
            <PostCode>2897</PostCode>
            <Phone>+61478101323</Phone>
            <FCLEquipmentNeeded>WUP</FCLEquipmentNeeded>
            <LCLEquipmentNeeded>PSL</LCLEquipmentNeeded>
            <AIREquipmentNeeded>PSL</AIREquipmentNeeded>
            <IsActive>true</IsActive>
            <ValidationStatus>NYV</ValidationStatus>
            <AuthorityToLeave>DEF</AuthorityToLeave>
            <City>Blocktown</City>
            <OrgAddressCapabilityCollection>
              <OrgAddressCapability Action=""MERGE"">
                <AddressType>OFC</AddressType>
                <IsMainAddress>true</IsMainAddress>
              </OrgAddressCapability>
            </OrgAddressCapabilityCollection>
            <RelatedPortCode TableName=""RefUNLOCO"" Action=""MERGE"">
              <Code>AUSYD</Code>
            </RelatedPortCode>
            <CountryCode TableName=""RefCountry"">
              <Code>AU</Code>
            </CountryCode>
          </OrgAddress>
        </OrgAddressCollection>
        <ClosestPort TableName=""RefUNLOCO"" Action=""MERGE"">
          <Code>AUSYD</Code>
        </ClosestPort>
      </OrgHeader>
    </Organization>
  </Body>
</Native>";

		#endregion

		public void TestOrgWithNameWithAmpersandsThatIsTooLong_EN()
		{
			TestOrgWithNameWithAmpersandsThatIsTooLong("EN", Core.SharedConstants.Languages.English);
		}

		public void TestOrgWithNameWithAmpersandsThatIsTooLong_GRM()
		{
			TestOrgWithNameWithAmpersandsThatIsTooLong("GRM", Core.SharedConstants.Languages.German);
		}

		public void TestOrgWithNameWithAmpersandsThatIsTooLong_BAD()
		{
			TestOrgWithNameWithAmpersandsThatIsTooLong("BAD", Core.SharedConstants.Languages.English);
		}

		void TestOrgWithNameWithAmpersandsThatIsTooLong(string sourceLanguage, string expectedLangauge)
		{
			var factory = new BusinessObjectFactory();
			var countryAU = factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			var locodeAUSYD = factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");

			var logFilter = new ZQuery(StmALogSchema.SL_Parent, new[] { countryAU.PK, locodeAUSYD.PK });
			var stmaLogCount = factory.GetDatabaseCount(typeof(StmALog), logFilter);

			string actualLog = string.Format(XML_OrgWithNameWithAmpersandsThatIsTooLong, sourceLanguage).ImportNativeXmlReturningLog();

			CombineAssertions(delegate
			{
				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
ClosestPort - 0 inserts, 0 updates, 0 deletes
OrgHeader - 1 inserts, 0 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Precondition: Log Text.", expectedLog, actualLog);

				var organisation = factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, "123456789 & 123456789 & 123456789 & 123456789 & 123456789 & 123456789 & 123456789 & 123456789"));
				AssertNotNull("Could not find Org with a name of '123456789 & 123456789 & 123456789 & 123456789 & 123456789 & 123456789 & 123456789 & 123456789'", organisation);
				AssertEquals("Old or invalid language codes should be fixed", expectedLangauge, organisation.OH_Language);
			});
		}

		#region XML_OrgWithMergeActionsOnRelatedTables

		const string XML_OrgWithNameWithAmpersandsThatIsTooLong = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Body>
    <Organization version=""2.0"">
      <OrgHeader Action=""MERGE"">
        <Code>SOGGOVSYD</Code>
        <FullName>123456789 &amp; 123456789 &amp; 123456789 &amp; 123456789 &amp; 123456789 &amp; 123456789 &amp; 123456789 &amp; 123456789</FullName>
        <Language>{0}</Language>
        <IsActive>true</IsActive>
        <IsConsignor>true</IsConsignor>
        <ClosestPort TableName=""RefUNLOCO"" Action=""MERGE"">
          <Code>AUSYD</Code>
        </ClosestPort>
      </OrgHeader>
    </Organization>
  </Body>
</Native>";

		#endregion

		[TestDate(2011, 5, 5, 1, 1, 1)]
		[TestUtcOffset(10, 0, 0)]
		public void TestSystemLastEditTimeDoesNotGetUdatedWhenBringingInTheSameXMLASecondTime()
		{
			DataRegistry.Instance.CanUserEditOrganisationCode = true;

			var organisation = new BusinessObjectFactory().New<OrgHeader>();
			organisation.OH_FullName = "WRONG NAME";
			var mainAddress = organisation.MainAddress;
			mainAddress.OA_Address1 = "123 TEST STREET";
			mainAddress.OA_City = "TESTVILLE";
			mainAddress.OA_PostCode = "2892";
			mainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			organisation.OH_RL_NKClosestPort = "AUSYD";
			organisation.OH_Code = "RANDOMCODE";

			organisation.Factory.Save();

			string actualLog = XML_UpdateOrganizationByOrganizationCode.ImportNativeXmlReturningLog();

			CombineAssertions(delegate
			{
				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 0 inserts, 1 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Precondition: Log Text", expectedLog, actualLog);

				organisation = new BusinessObjectFactory().Load<OrgHeader>(organisation.PK);
				AssertEquals("Precondition: organisation.OH_FullName", "ANOTHER NAME CO", organisation.OH_FullName);
				AssertEquals("Precondition: organisation.OH_Code", "RANDOMCODE", organisation.OH_Code);
				AssertEquals("Precondition: organisation.OH_SystemLastEditTimeUtc", new ZDateTime(2011, 5, 5, 1, 1, 0), organisation.OH_SystemLastEditTimeUtc);
			});

			TestDateAttribute.Date = new DateTime(2011, 5, 5, 2, 2, 0);

			actualLog = XML_UpdateOrganizationByOrganizationCode.ImportNativeXmlReturningLog();

			CombineAssertions(delegate
			{
				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 0 inserts, 0 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Log Text should indicate an update", expectedLog, actualLog);

				organisation = new BusinessObjectFactory().Load<OrgHeader>(organisation.PK);
				AssertEquals("organisation.OH_SystemLastEditTimeUtc should not have changed as no fields were changed", new ZDateTime(2011, 5, 5, 1, 1, 0), organisation.OH_SystemLastEditTimeUtc);
			});

			TestDateAttribute.Date = new DateTime(2011, 5, 5, 3, 3, 0);

			actualLog = XML_UpdateOrganizationWithNameChange.ImportNativeXmlReturningLog();

			CombineAssertions(delegate
			{
				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 0 inserts, 1 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Log Text should indicate an update", expectedLog, actualLog);

				organisation = new BusinessObjectFactory().Load<OrgHeader>(organisation.PK);
				AssertEquals("organisation.OH_SystemLastEditTimeUtc should have changed as the Name was changed", new ZDateTime(2011, 5, 5, 3, 3, 0), organisation.OH_SystemLastEditTimeUtc);
			});
		}

		const string XML_UpdateOrganizationWithNameChange = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ReferenceData xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <Header xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.cargowise.com/Schemas/Universal"">
    <OwnerCode>GLOFORSYD</OwnerCode>
    <EnableCodeMapping>false</EnableCodeMapping>
  </Header>
  <Body>
    <Organization>
      <OrgHeader Action=""MERGE"">
        <Code>RANDOMCODE</Code>
        <FullName>AND ANOTHER THING...</FullName>
        <ClosestPort TableName=""RefUNLOCO"">
          <Code>AUSYD</Code>
        </ClosestPort>
      </OrgHeader>
    </Organization>
  </Body>
</ReferenceData>";

		[TestDate(2011, 5, 5, 1, 1, 1)]
		[TestUtcOffset(10, 0, 0)]
		public void TestOrgMatchingGeneratesTheSameFromNativeDataAsItDoesFromANormalOrgHeaderCreation()
		{
			#region NativeOrganizationXMLBennyBanana

			const string NativeOrganizationXMLBennyBanana = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""1.0"">
  <Header>
    <OwnerCode>EDIEDIBNE</OwnerCode>
    <EnableCodeMapping>false</EnableCodeMapping>
  </Header>
  <Body>
    <Organization version=""1.0"">
      <OrgHeader Action=""MERGE"">
        <Code>BENBANPBY_</Code>
        <IsActive>true</IsActive>
        <FullName>BENNY BANANA STRIKES AGAIN</FullName>
        <IsConsignor>true</IsConsignor>
        <Prospect>true</Prospect>
        <Language>EN</Language>
        <OrgMiscServ Action=""MERGE"">
          <IMMergeCustomsInvoiceLinesBy>NON</IMMergeCustomsInvoiceLinesBy>
          <IMOriginalSeaBills>3</IMOriginalSeaBills>
          <IMCopySeaBills>3</IMCopySeaBills>
          <IMSendImportDocsTo>IMP</IMSendImportDocsTo>
          <IMSendSeaImportDocsTo>IMP</IMSendSeaImportDocsTo>
          <IMImporterCategory>STD</IMImporterCategory>
          <IMAirDepotFreeDays>1</IMAirDepotFreeDays>
          <IMSeaDepotFreeDays>3</IMSeaDepotFreeDays>
          <IMDefaultINCOTerm>FOB</IMDefaultINCOTerm>
          <EXExporterCategory>STD</EXExporterCategory>
          <EXDefaultIncoTerm>FOB</EXDefaultIncoTerm>
          <FWAgentCategory>STD</FWAgentCategory>
          <FWBillCollectFeesOnSingleInvoice>true</FWBillCollectFeesOnSingleInvoice>
          <CMCompetitorActivity>FRT</CMCompetitorActivity>
          <CMUseTradeLaneFigures>true</CMUseTradeLaneFigures>
          <IMAutoPopulateOwnerRefWithOrderNums>DEF</IMAutoPopulateOwnerRefWithOrderNums>
          <IMDefaultWarehousePickOption>AUT</IMDefaultWarehousePickOption>
          <IMInvoiceDetailReportSort>DEF</IMInvoiceDetailReportSort>
          <WhsExpiryNotificationFromDefaults>true</WhsExpiryNotificationFromDefaults>
          <IMInvoiceDetailReportSort2>DEF</IMInvoiceDetailReportSort2>
          <IMInvoiceDetailReportSort3>DEF</IMInvoiceDetailReportSort3>
          <EXMergeCustomsInvoiceLinesBy>NON</EXMergeCustomsInvoiceLinesBy>
          <WhsEDIChangeMessageCancelOrderLinesNotIncludedInMessage>true</WhsEDIChangeMessageCancelOrderLinesNotIncludedInMessage>
          <WhsOrderFulfillmentRule>NON</WhsOrderFulfillmentRule>
          <WhsPackingSlipOrderBy>DEF</WhsPackingSlipOrderBy>
          <CMDistanceCalculationProvider>DEF</CMDistanceCalculationProvider>
          <WhsDefaultWarehousePickMode>ASP</WhsDefaultWarehousePickMode>
          <WhsTransportPayer>DEF</WhsTransportPayer>
          <EXDefaultCntryOfOrigin TableName=""RefCountry"">
            <Code>AU</Code>
          </EXDefaultCntryOfOrigin>
          <FWDefCurrency TableName=""RefCurrency"">
            <Code>AUD</Code>
          </FWDefCurrency>
        </OrgMiscServ>
        <OrgAddressCollection>
          <OrgAddress Action=""MERGE"">
            <IsActive>true</IsActive>
            <Code>243 BANANANANANANANANA PL</Code>
            <Language>EN</Language>
            <Address1>243 BANANANANANANANANA PLACE</Address1>
            <City>SKINTIGHT SACKVILLE</City>
            <State>SA</State>
            <PostCode>4378</PostCode>
            <Phone>61 9 8423 1231</Phone>
            <Fax>61 9 8423 1245</Fax>
            <Mobile>61 421 565 787</Mobile>
            <Email>bananana@doohdoohdhduhder.com</Email>
            <FCLEquipmentNeeded>WUP</FCLEquipmentNeeded>
            <LCLEquipmentNeeded>PSL</LCLEquipmentNeeded>
            <AIREquipmentNeeded>PSL</AIREquipmentNeeded>
            <OrgAddressCapabilityCollection>
              <OrgAddressCapability Action=""MERGE"">
                <AddressType>OFC</AddressType>
                <IsMainAddress>true</IsMainAddress>
              </OrgAddressCapability>
            </OrgAddressCapabilityCollection>
            <RelatedPortCode TableName=""RefUNLOCO"">
              <Code>AUPBY</Code>
            </RelatedPortCode>
          </OrgAddress>
        </OrgAddressCollection>
        <OrgCusCodeCollection>
          <OrgCusCode Action=""MERGE"">
            <CustomsRegNo>61124598796</CustomsRegNo>
            <CodeType>ABN</CodeType>
            <CodeCountry TableName=""RefCountry"">
              <Code>AU</Code>
            </CodeCountry>
          </OrgCusCode>
          <OrgCusCode Action=""MERGE"">
            <CustomsRegNo>GNADS</CustomsRegNo>
            <CodeType>GBR</CodeType>
            <CodeCountry TableName=""RefCountry"">
              <Code>AU</Code>
            </CodeCountry>
          </OrgCusCode>
        </OrgCusCodeCollection>
        <ClosestPort TableName=""RefUNLOCO"">
          <Code>AUPBY</Code>
        </ClosestPort>
      </OrgHeader>
    </Organization>
  </Body>
</Native>";

			#endregion

			DataRegistry.Instance.CanUserEditOrganisationCode = true;

			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(NativeOrganizationXMLBennyBanana)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);
				var insertLog = manager.GetLogs();
				AssertMultilineASCIIEquals("Insert Log Text", @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 1 inserts, 0 updates, 0 deletes
OrgMiscServ - 1 inserts, 0 updates, 0 deletes
OrgAddress - 1 inserts, 0 updates, 0 deletes
OrgAddressCapability - 1 inserts, 0 updates, 0 deletes
OrgCusCode - 2 inserts, 0 updates, 0 deletes
				".Trim(), insertLog);
			}

			var organisationCreatedByNativeData = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "BENBANPBY_"));
			AssertNotNull("organisationCreatedByNativeData", organisationCreatedByNativeData);

			var organisationCreatedByBusinessObjects = Factory.New<OrgHeader>();
			organisationCreatedByBusinessObjects.OH_FullName = "BENNY BANANA STRIKES AGAIN";
			organisationCreatedByBusinessObjects.OH_RL_NKClosestPort = "AUPBY";

			var address = organisationCreatedByBusinessObjects.MainAddress;
			address.OA_Address1 = "243 BANANANANANANANANA PLACE";
			address.OA_City = "SKINTIGHT SACKVILLE";
			address.OA_PostCode = "4378";
			address.OA_State = "SA";
			address.OA_Phone = "61 9 8423 1231";
			address.OA_Mobile = "61 421 565 787";
			address.OA_Fax = "61 9 8423 1245";
			address.OA_Email = "bananana@doohdoohdhduhder.com";

			organisationCreatedByBusinessObjects.PrimaryRegistrationNumber.Number = "61124598796";
			organisationCreatedByBusinessObjects.CustomsCodes.AddNew("GBR", "GNADS");

			Factory.Save();

			var patternMatchesFromBusinessObjects = Factory.SerialiseForTesting<OrgPatternMatch>(new ZQuery(OrgPatternMatchSchema.OS_OH, organisationCreatedByBusinessObjects.PK), OrgPatternMatchSchema.OS_BusinessRegNo.Name);
			var patternMatchesFromNativeData = Factory.SerialiseForTesting<OrgPatternMatch>(new ZQuery(OrgPatternMatchSchema.OS_OH, organisationCreatedByNativeData.PK), OrgPatternMatchSchema.OS_BusinessRegNo.Name);
			// This is becaue the DataTransfer doesn't assign data for added audit columns which have to use the default value. If this issue is fixed, then the test would fail and this line of code can be removed.
			patternMatchesFromBusinessObjects = patternMatchesFromBusinessObjects.Replace(@"OS_SystemCreateTimeUtc    - [05-May-11 01:01:01]
OS_SystemCreateUser       - [E]
OS_SystemLastEditTimeUtc  - [05-May-11 01:01:01]
OS_SystemLastEditUser     - [E]", "");

			Assert("Precondition: patternMatchesFromBusinessObjects should have at least 200 characters information in it.", patternMatchesFromBusinessObjects.Length > 200);
			AssertMultilineASCIIEquals("Comparing Pattern Matches created by Business Objects to Native XML rows", patternMatchesFromBusinessObjects, patternMatchesFromNativeData);
		}

		public void TestCanImportOrganizationWithCodeMatchingGeneratedCode()
		{
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(XML_OrganizationWithCodeMatchingGeneratedCode)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);
				var insertLog = manager.GetLogs();
				AssertMultilineASCIIEquals("Insert Log Text", @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 1 inserts, 0 updates, 0 deletes
OrgAddress - 1 inserts, 0 updates, 0 deletes
OrgAddressCapability - 1 inserts, 0 updates, 0 deletes
				".Trim(), insertLog);
			}
		}

		public void TestRelatedPortCodeIsAddedCorrectlyOnInsert()
		{
			var manager = new ImportServiceManagerForTesting();
			OrgHeader org;
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(XML_RelatedPortCodeIsAddedCorrectlyOnInsert_SetUp)))
			{
				manager.ImportService.Import(stream);
				var insertLog = manager.GetLogs();
				AssertMultilineASCIIEquals("Insert Log Text", @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 1 inserts, 0 updates, 0 deletes
OrgAddress - 1 inserts, 0 updates, 0 deletes
OrgAddressCapability - 1 inserts, 0 updates, 0 deletes
				".Trim(), insertLog);
			}
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(XML_RelatedPortCodeIsAddedCorrectlyOnInsert_Update)))
			{
				manager.ImportService.Import(stream);
				var createdOrgHeaderList = Factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, "BENS TOAST COMPANY"));
				org = createdOrgHeaderList.First();
				var address = org.Addresses.Where(x => x.OA_Code == "Sydney Add").FirstOrDefault();
				AssertNotNull(address);
				AssertEquals("AUSYD", address.OA_RL_NKRelatedPortCode);
			}
		}

		#region XML_RelatedPortCodeIsAddedCorrectlyOnInsert

		const string XML_RelatedPortCodeIsAddedCorrectlyOnInsert_SetUp = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""1.0"">
  <Header>
    <OwnerCode>EDICUS</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Organization version=""1.0"">
      <OrgHeader Action=""INSERT"">
        <Code>TESTOMEL</Code>
        <IsActive>true</IsActive>
        <FullName>BENS TOAST COMPANY</FullName>
        <IsConsignee>true</IsConsignee>
        <IsConsignor>true</IsConsignor>
        <Prospect>true</Prospect>
        <Language>EN</Language>
        <ScreeningStatus>UNK</ScreeningStatus>
        <OrgAddressCollection>
          <OrgAddress Action=""INSERT"">
            <IsActive>true</IsActive>
            <Code>MY ADDRESS</Code>
            <Language>EN</Language>
            <Address1>MY ADDRESS</Address1>
            <City>AWESOMNIA</City>
            <State>WA</State>
            <PostCode>7777</PostCode>
            <FCLEquipmentNeeded>WUP</FCLEquipmentNeeded>
            <LCLEquipmentNeeded>PSL</LCLEquipmentNeeded>
            <AIREquipmentNeeded>PSL</AIREquipmentNeeded>
            <OrgAddressCapabilityCollection>
              <OrgAddressCapability Action=""INSERT"">
                <AddressType>OFC</AddressType>
                <IsMainAddress>true</IsMainAddress>
              </OrgAddressCapability>
            </OrgAddressCapabilityCollection>
            <RelatedPortCode TableName=""RefUNLOCO"">
              <Code>AUPER</Code>
            </RelatedPortCode>
          </OrgAddress>
        </OrgAddressCollection>
        <ClosestPort TableName=""RefUNLOCO"">
          <Code>AUPER</Code>
        </ClosestPort>
      </OrgHeader>
    </Organization>
  </Body>
</Native>";

		const string XML_RelatedPortCodeIsAddedCorrectlyOnInsert_Update = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns = ""http://www.cargowise.com/Schemas/Native/2011/11"" >
  <Body>
	<Organization xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"">
      <OrgHeader Action = ""UPDATE"">
		<Code>TESTOMEL</Code>
		<OrgAddressCollection>
		  <OrgAddress Action=""INSERT"">
            <Code>Sydney Add</Code>
            <Address1>123 Sydney St</Address1>
            <City>Sydney</City>
            <PostCode>2000</PostCode>
            <Phone>02 9000 8000</Phone>
            <FCLEquipmentNeeded>ASK</FCLEquipmentNeeded>
            <LCLEquipmentNeeded>ASK</LCLEquipmentNeeded>
            <AIREquipmentNeeded>ASK</AIREquipmentNeeded>
            <RelatedPortCode>
              <Code>AUSYD</Code>
            </RelatedPortCode>
            <CountryCode>
              <Code>AU</Code>
            </CountryCode>
            <OrgAddressCapabilityCollection>
              <OrgAddressCapability Action = ""INSERT"">
				<AddressType>DLV</AddressType >
				<IsMainAddress>false</IsMainAddress>
			  </OrgAddressCapability>
			</OrgAddressCapabilityCollection>
		  </OrgAddress>
		</OrgAddressCollection>
	  </OrgHeader>
	</Organization>
  </Body>
</Native>";

		#endregion XML_RelatedPortCodeIsAddedCorrectlyOnInsert

		public void TestDeleteOrgWithOrgPatternMatch()
		{
			OrgHeader org;

			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(XML_OrganizationWithCodeMatchingGeneratedCode)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);
				var insertLog = manager.GetLogs();
				AssertMultilineASCIIEquals("Insert Log Text", @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 1 inserts, 0 updates, 0 deletes
OrgAddress - 1 inserts, 0 updates, 0 deletes
OrgAddressCapability - 1 inserts, 0 updates, 0 deletes
				".Trim(), insertLog);
				var createdOrgHeaderList = Factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, "BENS TOAST COMPANY"));
				AssertEquals(1, createdOrgHeaderList.Length);
				org = createdOrgHeaderList.First();
			}

			ZQuery query = new ZQuery();
			query.AddToFilter(OrgPatternMatchOverrideSchema.OO_OH, OrgHeader.DefaultOrg.PK);
			query.AddToFilter(OrgPatternMatchOverrideSchema.OO_LocalCode, org.OH_Code);
			query.AddToFilter(OrgPatternMatchOverrideSchema.OO_Relationship, Core.Constants.OrgPatternMatchOverrideRelationships.Organisation);
			OrgPatternMatchOverride[] orgMatches = Factory.Load<OrgPatternMatchOverride>(query);
			AssertEquals("Related OrgPatternMatchOverride should have been generated", 1, orgMatches.Length);

			org.Delete();
			Factory.Save();

			orgMatches = Factory.Load<OrgPatternMatchOverride>(query);
			AssertEquals("Related OrgPatternMatchOverride should have been deleted", 0, orgMatches.Length);
		}

		#region XML_OrganizationWithCodeMatchingGeneratedCode

		const string XML_OrganizationWithCodeMatchingGeneratedCode = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""1.0"">
  <Header>
    <OwnerCode>EDICUS</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Organization version=""1.0"">
      <OrgHeader Action=""INSERT"">
        <Code>BENTOAPER</Code>
        <IsActive>true</IsActive>
        <FullName>BENS TOAST COMPANY</FullName>
        <IsConsignee>true</IsConsignee>
        <IsConsignor>true</IsConsignor>
        <Prospect>true</Prospect>
        <Language>EN</Language>
        <ScreeningStatus>UNK</ScreeningStatus>
        <OrgAddressCollection>
          <OrgAddress Action=""INSERT"">
            <IsActive>true</IsActive>
            <Code>MY ADDRESS</Code>
            <Language>EN</Language>
            <Address1>MY ADDRESS</Address1>
            <City>AWESOMNIA</City>
            <State>WA</State>
            <PostCode>7777</PostCode>
            <FCLEquipmentNeeded>WUP</FCLEquipmentNeeded>
            <LCLEquipmentNeeded>PSL</LCLEquipmentNeeded>
            <AIREquipmentNeeded>PSL</AIREquipmentNeeded>
            <OrgAddressCapabilityCollection>
              <OrgAddressCapability Action=""INSERT"">
                <AddressType>OFC</AddressType>
                <IsMainAddress>true</IsMainAddress>
              </OrgAddressCapability>
            </OrgAddressCapabilityCollection>
            <RelatedPortCode TableName=""RefUNLOCO"">
              <Code>AUPER</Code>
            </RelatedPortCode>
          </OrgAddress>
        </OrgAddressCollection>
        <ClosestPort TableName=""RefUNLOCO"">
          <Code>AUPER</Code>
        </ClosestPort>
      </OrgHeader>
    </Organization>
  </Body>
</Native>";

		#endregion

		public void TestCanImportAddressWithLatinCharacter()
		{
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(XML_AddressWithLatinCharacter)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);
				var insertLog = manager.GetLogs();
				AssertMultilineASCIIEquals("Insert Log Text", @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 1 inserts, 0 updates, 0 deletes
OrgMiscServ - 1 inserts, 0 updates, 0 deletes
OrgAddress - 2 inserts, 0 updates, 0 deletes
OrgAddressCapability - 2 inserts, 0 updates, 0 deletes
OrgCusCode - 2 inserts, 0 updates, 0 deletes
				".Trim(), insertLog);
			}

			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(XML_AddressWithLatinCharacter)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);
				var updateLog = manager.GetLogs();
				AssertMultilineASCIIEquals("Update Log Text", @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 0 inserts, 0 updates, 0 deletes
OrgMiscServ - 0 inserts, 0 updates, 0 deletes
OrgAddress - 0 inserts, 0 updates, 0 deletes
OrgAddressCapability - 0 inserts, 0 updates, 0 deletes
OrgCusCode - 0 inserts, 0 updates, 0 deletes
				".Trim(), updateLog);
			}
		}

		#region XML_AddressWithLatinCharacter

		const string XML_AddressWithLatinCharacter = @"<?xml version=""1.0""?>
<Native xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://www.cargowise.com/Schemas/Native"">
  <Header>
    <EnableCodeMapping>false</EnableCodeMapping>
  </Header>
  <Body>
    <Organization version=""2.0"">
      <OrgHeader Action=""Merge"">
        <Code>ARLFOOVIY</Code>
        <IsActive>true</IsActive>
        <FullName>Arla Foods AmBa Lillebaelt Mejeri - BIHOG</FullName>
        <IsConsignee>true</IsConsignee>
        <IsConsignor>true</IsConsignor>
        <Language>DA-DK</Language>
        <OrgMiscServ Action=""Merge"">
          <CustomAttrib3 />
        </OrgMiscServ>
        <OrgAddressCollection>
          <OrgAddress Action=""Merge"">
            <IsActive>true</IsActive>
            <Code>[1] Soenderhoej 14</Code>
            <Language>EN</Language>
            <CompanyNameOverride>Arla Foods AmBa Lillebaelt Mejeri - BIHOG</CompanyNameOverride>
            <Address1>Soenderhoej 14</Address1>
            <Address2 />
            <City>Viby J</City>
            <State />
            <PostCode>8260</PostCode>
            <Phone>+45</Phone>
            <Fax />
            <Mobile />
            <Email>bihog@arlafoods.com</Email>
            <AIREquipmentNeeded>PSL</AIREquipmentNeeded>
            <FCLEquipmentNeeded>WUP</FCLEquipmentNeeded>
            <LCLEquipmentNeeded>PSL</LCLEquipmentNeeded>
            <OrgAddressCapabilityCollection>
              <OrgAddressCapability Action=""Merge"">
                <AddressType>OFC</AddressType>
                <IsMainAddress>true</IsMainAddress>
              </OrgAddressCapability>
            </OrgAddressCapabilityCollection>
            <RelatedPortCode TableName=""RefUNLOCO"">
              <Code>DKVIY</Code>
            </RelatedPortCode>
          </OrgAddress>
          <OrgAddress Action=""Merge"">
            <IsActive>true</IsActive>
            <Code>[4] Sønderhøj 14</Code>
            <Language>DA-DK</Language>
            <CompanyNameOverride>Arla Foods AmBa Lillebælt Mejeri - BIHOG</CompanyNameOverride>
            <Address1>Sønderhøj 14</Address1>
            <Address2 />
            <City>Viby J</City>
            <State />
            <PostCode>8260</PostCode>
            <Phone>+45</Phone>
            <Fax />
            <Mobile />
            <Email>bihog@arlafoods.com</Email>
            <AIREquipmentNeeded>PSL</AIREquipmentNeeded>
            <FCLEquipmentNeeded>WUP</FCLEquipmentNeeded>
            <LCLEquipmentNeeded>PSL</LCLEquipmentNeeded>
            <OrgAddressCapabilityCollection>
              <OrgAddressCapability Action=""Merge"">
                <AddressType>SQM</AddressType>
                <IsMainAddress>true</IsMainAddress>
              </OrgAddressCapability>
            </OrgAddressCapabilityCollection>
            <RelatedPortCode TableName=""RefUNLOCO"">
              <Code>DKVIY</Code>
            </RelatedPortCode>
          </OrgAddress>
        </OrgAddressCollection>
        <OrgCusCodeCollection>
          <OrgCusCode Action=""Merge"">
            <CustomsRegNo>25313763</CustomsRegNo>
            <CodeType>VAT</CodeType>
            <CountryDefault>false</CountryDefault>
            <CodeCountry TableName=""RefCountry"">
              <Code>DK</Code>
            </CodeCountry>
          </OrgCusCode>
          <OrgCusCode Action=""Merge"">
            <CustomsRegNo>25313763</CustomsRegNo>
            <CodeType>GCR</CodeType>
            <CountryDefault>false</CountryDefault>
            <CodeCountry TableName=""RefCountry"">
              <Code>DK</Code>
            </CodeCountry>
          </OrgCusCode>
        </OrgCusCodeCollection>
        <ClosestPort TableName=""RefUNLOCO"">
          <Code>DKVIY</Code>
        </ClosestPort>
      </OrgHeader>
    </Organization>
  </Body>
</Native>";

		#endregion

		public void TestCanImportAddressAndContactWithChineseCharacterKey()
		{
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(XML_AddressAndContactWithChineseCharacterKey)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);
				var insertLog = manager.GetLogs();
				AssertMultilineASCIIEquals("Insert Log Text", @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 1 inserts, 0 updates, 0 deletes
OrgContact - 1 inserts, 0 updates, 0 deletes
OrgAddress - 2 inserts, 0 updates, 0 deletes
OrgAddressCapability - 3 inserts, 0 updates, 0 deletes
				".Trim(), insertLog);
			}

			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(XML_AddressAndContactWithChineseCharacterKey)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);
				var updateLog = manager.GetLogs();
				AssertMultilineASCIIEquals("Update Log Text", @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 0 inserts, 0 updates, 0 deletes
OrgContact - 0 inserts, 0 updates, 0 deletes
OrgAddress - 0 inserts, 0 updates, 0 deletes
OrgAddressCapability - 0 inserts, 0 updates, 0 deletes
				".Trim(), updateLog);
			}
		}

		#region XML_AddressAndContactWithChineseCharacterKey

		const string XML_AddressAndContactWithChineseCharacterKey = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<ReferenceData xmlns=""http://www.cargowise.com/Schemas/Universal"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation=""http://www.cargowise.com/Schemas/Universal file:///C:/StarTeam/Documentation/CargoWise/Native%20Schemas/ReferenceOrganizationSchema.xsd"">
	<Header>
		<EnableCodeMapping>true</EnableCodeMapping>
	</Header>
	<Body>
		<Organization>
			<OrgHeader Action=""MERGE"">
				<Code>TW08722PINGT</Code>
				<IsActive>true</IsActive>
				<FullName>FOSECO GOLDEN GATE CO LTD</FullName>
				<IsConsignee>true</IsConsignee>
				<IsConsignor>true</IsConsignor>
				<Language>EN</Language>
				<ClosestPort>
					<Code>TWTPE</Code>
				</ClosestPort>
				<OrgAddressCollection>
					<OrgAddress Action=""MERGE"">
						<Code>TWTPE - CHT - 屏東縣屏東巿工業二路６</Code>
						<Language>ZH-TW</Language>
						<CompanyNameOverride>京華福士科股份有限公司</CompanyNameOverride>
						<Address1>屏東縣屏東巿工業二路６號</Address1>
						<City>PINGTUNG</City>
						<PostCode>90049</PostCode>
						<Phone>+88687228108</Phone>
						<Fax>+88687228182</Fax>
						<FCLEquipmentNeeded>WUP</FCLEquipmentNeeded>
						<LCLEquipmentNeeded>PSL</LCLEquipmentNeeded>
						<AIREquipmentNeeded>PSL</AIREquipmentNeeded>
						<OrgAddressCapabilityCollection>
							<OrgAddressCapability Action=""MERGE"">
								<AddressType>OFC</AddressType>
								<IsMainAddress>false</IsMainAddress>
							</OrgAddressCapability>
						</OrgAddressCapabilityCollection>
						<RelatedPortCode>
							<Code>TWTPE</Code>
						</RelatedPortCode>
					</OrgAddress>
					<OrgAddress Action=""MERGE"">
						<Code>TWTPE - NO6INDUSTRY2NDROA</Code>
						<Language>EN</Language>
						<Address1>NO6 INDUSTRY 2ND ROAD</Address1>
						<Address2>PING TUNG TAIWAN</Address2>
						<City>PINGTUNG</City>
						<PostCode>90049</PostCode>
						<Phone>+88687228108</Phone>
						<Fax>+88687228182</Fax>
						<FCLEquipmentNeeded>WUP</FCLEquipmentNeeded>
						<LCLEquipmentNeeded>PSL</LCLEquipmentNeeded>
						<AIREquipmentNeeded>PSL</AIREquipmentNeeded>
						<OrgAddressCapabilityCollection>
							<OrgAddressCapability Action=""MERGE"">
								<AddressType>OFC</AddressType>
								<IsMainAddress>true</IsMainAddress>
							</OrgAddressCapability>
							<OrgAddressCapability Action=""MERGE"">
								<AddressType>PST</AddressType>
								<IsMainAddress>true</IsMainAddress>
							</OrgAddressCapability>
						</OrgAddressCapabilityCollection>
						<RelatedPortCode>
							<Code>TWTPE</Code>
						</RelatedPortCode>
					</OrgAddress>
				</OrgAddressCollection>
        <OrgContactCollection>
          <OrgContact Action=""MERGE"">
            <IsActive>true</IsActive>
            <ContactName>京華福士科股份有限公司</ContactName>
            <Language>EN</Language>
            <NotifyMode>EML</NotifyMode>
            <Title>CEO</Title>
            <AttachmentType>PDF</AttachmentType>
          </OrgContact>
        </OrgContactCollection>
			</OrgHeader>
		</Organization>
	</Body>
</ReferenceData>
";

		#endregion

		public void TestCanImportNewOrgHeaderWithAddress()
		{
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(XML_OrgHeaderWithAddress)))
			{
				var organizationEAGDATAKL = Factory.New<OrgHeader>();
				organizationEAGDATAKL.OH_FullName = "FAccResourceDataSchem";
				organizationEAGDATAKL.OH_RL_NKClosestPort = "NZAKL";
				organizationEAGDATAKL.OH_Code = "EAGDATAKL";
				organizationEAGDATAKL.MainAddress.OA_Address1 = "900 MANGANUI RD";
				organizationEAGDATAKL.MainAddress.OA_Code = "ALL: 900 MANGANUI RD";
				organizationEAGDATAKL.MainAddress.OA_City = "Sydney";
				Factory.Save();

				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);
				var insertLog = manager.GetLogs();
				AssertMultilineASCIIEquals("Insert Log Text", @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 1 inserts, 0 updates, 0 deletes
OrgAddress - 2 inserts, 0 updates, 0 deletes
OrgAddressCapability - 2 inserts, 0 updates, 0 deletes
OrgAppointedAgentPorts - 2 inserts, 0 updates, 0 deletes
				".Trim(), insertLog);
			}

			var createdOrgHeaderList = new BusinessObjectFactory().Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "FURFORHKG"));
			AssertEquals(1, createdOrgHeaderList.Length);
			var createdOrgHeader = createdOrgHeaderList[0];
			var query = new ZQuery(OrgAppointedAgentPortsSchema.O5_OH, createdOrgHeader.PK);
			query.OrderBy = OrgAppointedAgentPortsSchema.O5_SeaAirCarrierOrForwarderType.Name;

			var agentPortList = new BusinessObjectFactory().Load<OrgAppointedAgentPorts>(query);
			AssertEquals(2, agentPortList.Length);
			AssertEquals("Pick Up Address", agentPortList[0].AgentOfficeAddress.OA_Code);
			AssertEquals("ALL: 900 MANGANUI RD", agentPortList[1].AgentOfficeAddress.OA_Code);
		}

		#region XML_OrgHeaderWithAddress

		const string XML_OrgHeaderWithAddress = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ReferenceData xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <Header xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.cargowise.com/Schemas/Universal"">
    <OwnerCode>AUDEMOALX</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Organization>
      <OrgHeader Action=""MERGE"">
        <Code>ASFURNHKG</Code>
        <IsActive>true</IsActive>
        <FullName>FURNISH FORWARDING</FullName>
        <IsForwarder>true</IsForwarder>
        <Language>EN</Language>
        <OrgAddressCollection>
          <OrgAddress Action=""MERGE"">
            <IsActive>true</IsActive>
            <Code>Pick Up Address</Code>
            <Language>EN</Language>
            <CompanyNameOverride></CompanyNameOverride>
            <Address1>UNIT 1, 1ST FLOOR, BLOCK B</Address1>
            <Address2>HOI BUN IND BLDG                         6 WIN YIP</Address2>
            <City>KONG</City>
            <FCLEquipmentNeeded>ASK</FCLEquipmentNeeded>
            <LCLEquipmentNeeded>ASK</LCLEquipmentNeeded>
            <AIREquipmentNeeded>ASK</AIREquipmentNeeded>
            <OrgAddressCapabilityCollection>
              <OrgAddressCapability Action=""MERGE"">
                <AddressType>PIC</AddressType>
                <IsMainAddress>false</IsMainAddress>
              </OrgAddressCapability>
            </OrgAddressCapabilityCollection>
            <RelatedPortCode TableName=""RefUNLOCO"">
              <Code>HKHKG</Code>
            </RelatedPortCode>
          </OrgAddress>
          <OrgAddress Action=""MERGE"">
            <IsActive>true</IsActive>
            <Code>PST: UNIT A1, 4TH FLOOR,</Code>
            <Language>EN</Language>
            <CompanyNameOverride></CompanyNameOverride>
            <Address1>UNIT A1, 4TH FLOOR, PIONEER IND BLDG</Address1>
            <Address2>213 WAI YIP STREET, KWUN TONG, KOWLOON  HONG KONG</Address2>
            <City>KOWLOON  HONG KONG</City>
            <FCLEquipmentNeeded>ASK</FCLEquipmentNeeded>
            <LCLEquipmentNeeded>ASK</LCLEquipmentNeeded>
            <AIREquipmentNeeded>ASK</AIREquipmentNeeded>
            <OrgAddressCapabilityCollection>
              <OrgAddressCapability Action=""MERGE"">
                <AddressType>OFC</AddressType>
                <IsMainAddress>true</IsMainAddress>
              </OrgAddressCapability>
            </OrgAddressCapabilityCollection>
            <RelatedPortCode TableName=""RefUNLOCO"">
              <Code>HKHKG</Code>
            </RelatedPortCode>
          </OrgAddress>
        </OrgAddressCollection>
        <OrgAppointedAgentPortsCollection>
          <OrgAppointedAgentPorts Action=""MERGE"">
            <PortOrCountry>AUSYD</PortOrCountry>
            <AirAgentStatus>APP</AirAgentStatus>
            <SeaAirCarrierOrForwarderType>FWD</SeaAirCarrierOrForwarderType>
            <AgentDirection>BTH</AgentDirection>
            <AgentOfficeAddress TableName=""OrgAddress"">
              <Code>Pick Up Address</Code>
              <OrgHeader>
                <Code>ASFURNHKG</Code>
              </OrgHeader>
            </AgentOfficeAddress>
          </OrgAppointedAgentPorts>
          <OrgAppointedAgentPorts Action=""MERGE"">
            <SeaAirCarrierOrForwarderType>SEA</SeaAirCarrierOrForwarderType>
            <PortOrCountry>NZAKL</PortOrCountry>
            <TerminalType>CNT</TerminalType>
            <ContainerType>0</ContainerType>
            <AgentDirection>BTH</AgentDirection>
            <AgentOfficeAddress TableName=""OrgAddress"">
              <Code>ALL: 900 MANGANUI RD</Code>
              <OrgHeader>
                <Code>EAGDATAKL</Code>
              </OrgHeader>
            </AgentOfficeAddress>
          </OrgAppointedAgentPorts>
        </OrgAppointedAgentPortsCollection>
        <ClosestPort TableName=""RefUNLOCO"">
          <Code>HKHKG</Code>
        </ClosestPort>
      </OrgHeader>
    </Organization>
  </Body>
</ReferenceData>
";

		#endregion

		public void TestCanImportNewOrgHeaderWithTranslatedAddress()
		{
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(XML_OrgHeaderWithTranslatedAddress)))
			{
				var organizationEAGDATAKL = Factory.New<OrgHeader>();
				organizationEAGDATAKL.OH_FullName = "FAccResourceDataSchem";
				organizationEAGDATAKL.OH_RL_NKClosestPort = "NZAKL";
				organizationEAGDATAKL.OH_Code = "EAGDATAKL";
				organizationEAGDATAKL.MainAddress.OA_Address1 = "900 MANGANUI RD";
				organizationEAGDATAKL.MainAddress.OA_Code = "ALL: 900 MANGANUI RD";
				organizationEAGDATAKL.MainAddress.OA_City = "Sydney";
				Factory.Save();

				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);
				var insertLog = manager.GetLogs();
				AssertMultilineASCIIEquals("Insert Log Text", @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 1 inserts, 0 updates, 0 deletes
OrgAddress - 2 inserts, 0 updates, 0 deletes
OrgTranslatedAddress - 2 inserts, 0 updates, 0 deletes
OrgAddressCapability - 2 inserts, 0 updates, 0 deletes
OrgAppointedAgentPorts - 2 inserts, 0 updates, 0 deletes
				".Trim(), insertLog);
			}

			var createdOrgHeaderList = new BusinessObjectFactory().Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "FURFORHKG"));
			AssertEquals(1, createdOrgHeaderList.Length);
			var createdOrgHeader = createdOrgHeaderList[0];
			var query = new ZQuery(OrgAppointedAgentPortsSchema.O5_OH, createdOrgHeader.PK);
			query.OrderBy = OrgAppointedAgentPortsSchema.O5_SeaAirCarrierOrForwarderType.Name;

			var agentPortList = new BusinessObjectFactory().Load<OrgAppointedAgentPorts>(query);
			AssertEquals(2, agentPortList[0].AgentOfficeAddress.TranslatedAddresses.Count);
			AssertEquals("1 층 1 층", agentPortList[0].AgentOfficeAddress.TranslatedAddresses[0].Address1);
			AssertEquals("一楼一单元", agentPortList[0].AgentOfficeAddress.TranslatedAddresses[1].Address1);
		}

		#region XML_OrgHeaderWithTranslatedAddress

		const string XML_OrgHeaderWithTranslatedAddress = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ReferenceData xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <Header xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.cargowise.com/Schemas/Universal"">
    <OwnerCode>AUDEMOALX</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Organization>
      <OrgHeader Action=""MERGE"">
        <Code>ASFURNHKG</Code>
        <IsActive>true</IsActive>
        <FullName>FURNISH FORWARDING</FullName>
        <IsForwarder>true</IsForwarder>
        <Language>EN</Language>
        <OrgAddressCollection>
          <OrgAddress Action=""MERGE"">
            <IsActive>true</IsActive>
            <Code>Pick Up Address</Code>
            <Language>EN</Language>
            <CompanyNameOverride></CompanyNameOverride>
            <Address1>UNIT 1, 1ST FLOOR, BLOCK B</Address1>
            <Address2>HOI BUN IND BLDG                         6 WIN YIP</Address2>
            <City>KONG</City>
            <FCLEquipmentNeeded>ASK</FCLEquipmentNeeded>
            <LCLEquipmentNeeded>ASK</LCLEquipmentNeeded>
            <AIREquipmentNeeded>ASK</AIREquipmentNeeded>
		    <OrgTranslatedAddressCollection>
                <OrgTranslatedAddress Action=""MERGE"">
                  <Language>ZH-CN</Language>
                  <Address1>一楼一单元</Address1>
                  <Address2></Address2>
                  <City></City>
                  <PostCode></PostCode>
                  <ValidationStatus>NYV</ValidationStatus>
                  <AddressMap></AddressMap>
                  <CompanyName></CompanyName>
                  <State></State>
                </OrgTranslatedAddress>
                <OrgTranslatedAddress Action=""MERGE"">
                  <Language>Kor</Language>
                  <Address1>1 층 1 층</Address1>
                  <Address2></Address2>
                  <City></City>
                  <PostCode></PostCode>
                  <ValidationStatus>NYV</ValidationStatus>
                  <AddressMap></AddressMap>
                  <CompanyName></CompanyName>
                  <State></State>
                </OrgTranslatedAddress>
            </OrgTranslatedAddressCollection>
            <OrgAddressCapabilityCollection>
              <OrgAddressCapability Action=""MERGE"">
                <AddressType>PIC</AddressType>
                <IsMainAddress>false</IsMainAddress>
              </OrgAddressCapability>
            </OrgAddressCapabilityCollection>
            <RelatedPortCode TableName=""RefUNLOCO"">
              <Code>HKHKG</Code>
            </RelatedPortCode>
          </OrgAddress>
          <OrgAddress Action=""MERGE"">
            <IsActive>true</IsActive>
            <Code>PST: UNIT A1, 4TH FLOOR,</Code>
            <Language>EN</Language>
            <CompanyNameOverride></CompanyNameOverride>
            <Address1>UNIT A1, 4TH FLOOR, PIONEER IND BLDG</Address1>
            <Address2>213 WAI YIP STREET, KWUN TONG, KOWLOON  HONG KONG</Address2>
            <City>KOWLOON  HONG KONG</City>
            <FCLEquipmentNeeded>ASK</FCLEquipmentNeeded>
            <LCLEquipmentNeeded>ASK</LCLEquipmentNeeded>
            <AIREquipmentNeeded>ASK</AIREquipmentNeeded>
            <OrgAddressCapabilityCollection>
              <OrgAddressCapability Action=""MERGE"">
                <AddressType>OFC</AddressType>
                <IsMainAddress>true</IsMainAddress>
              </OrgAddressCapability>
            </OrgAddressCapabilityCollection>
            <RelatedPortCode TableName=""RefUNLOCO"">
              <Code>HKHKG</Code>
            </RelatedPortCode>
          </OrgAddress>
        </OrgAddressCollection>
        <OrgAppointedAgentPortsCollection>
          <OrgAppointedAgentPorts Action=""MERGE"">
            <PortOrCountry>AUSYD</PortOrCountry>
            <AirAgentStatus>APP</AirAgentStatus>
            <SeaAirCarrierOrForwarderType>FWD</SeaAirCarrierOrForwarderType>
            <AgentDirection>BTH</AgentDirection>
            <AgentOfficeAddress TableName=""OrgAddress"">
              <Code>Pick Up Address</Code>
              <OrgHeader>
                <Code>ASFURNHKG</Code>
              </OrgHeader>
            </AgentOfficeAddress>
          </OrgAppointedAgentPorts>
          <OrgAppointedAgentPorts Action=""MERGE"">
            <SeaAirCarrierOrForwarderType>SEA</SeaAirCarrierOrForwarderType>
            <PortOrCountry>NZAKL</PortOrCountry>
            <TerminalType>CNT</TerminalType>
            <ContainerType>0</ContainerType>
            <AgentDirection>BTH</AgentDirection>
            <AgentOfficeAddress TableName=""OrgAddress"">
              <Code>ALL: 900 MANGANUI RD</Code>
              <OrgHeader>
                <Code>EAGDATAKL</Code>
              </OrgHeader>
            </AgentOfficeAddress>
          </OrgAppointedAgentPorts>
        </OrgAppointedAgentPortsCollection>
        <ClosestPort TableName=""RefUNLOCO"">
          <Code>HKHKG</Code>
        </ClosestPort>
      </OrgHeader>
    </Organization>
  </Body>
</ReferenceData>
";

		#endregion

		public void TestImportOrgHeaderWithInvalidAddressCapabilityShouldFail()
		{
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(XML_ImportOrgHeaderWithEmptyAddressCapabilityCollection)))
			{
				var organizationEAGDATAKL = Factory.New<OrgHeader>();

				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);
				var insertLog = manager.GetLogs();
				AssertMultilineASCIIEquals("Insert Log Text", @"
--- Start Import Process --------------------------------------------------------------
Record: Organization failed to Import:
Could not insert/update the Address (OrgAddress) as it had an invalid reference to a OrgAddressCapability (OrgAddressCapability). There is no OrgAddressCapability with the following values: [AddressType:OFC][IsMainAddress:True].
Error occurred trying to import file. Please fix the error and try importing the file again.
--- Import Process Finished -----------------------------------------------------------
No insert/update action performed.
				".Trim(), insertLog);
			}
		}

		#region XML_ImportOrgHeaderWithEmptyAddressCapabilityCollection

		const string XML_ImportOrgHeaderWithEmptyAddressCapabilityCollection = @"<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"">
      <Body>
        <Organization xmlns = ""http://www.cargowise.com/Schemas/Native/2011/11"">
		  <OrgHeader Action=""INSERT"">
            <FullName>Akzo Test 001</FullName>
            <Language>EN</Language>
            <IsConsignee>true</IsConsignee>
            <ClosestPort>
              <Code>NZAKL</Code>
            </ClosestPort>
            <OrgContactCollection>
              <OrgContact Action = ""INSERT"">
				<Language>EN</Language>
				<NotifyMode>EML</NotifyMode>
				<AttachmentType>PDF</AttachmentType>
			  </OrgContact>
			</OrgContactCollection>
			<OrgAddressCollection>
			  <OrgAddress Action=""INSERT"">
                <Code>1</Code>
                <Address1>4 Manu Tapu Drive</Address1>
                <Address2>Auckland Airport</Address2>
                <City>Auckland</City>
                <State>AUK</State>
                <PostCode>2022</PostCode>
                <FCLEquipmentNeeded>ASK</FCLEquipmentNeeded>
                <LCLEquipmentNeeded>ASK</LCLEquipmentNeeded>
                <AIREquipmentNeeded>ASK</AIREquipmentNeeded>
                <RelatedPortCode>
                  <Code>NZAKL</Code>
                </RelatedPortCode>
                <CountryCode>
                  <Code>NZ</Code>
                </CountryCode>
                <OrgAddressCapabilityCollection>
                  <OrgAddressCapability>
                    <AddressType>OFC</AddressType>
                    <IsMainAddress>true</IsMainAddress>
                  </OrgAddressCapability>
                </OrgAddressCapabilityCollection>
              </OrgAddress>
			  <OrgAddress Action=""INSERT"">
                <Code>2</Code>
                <Address1>4 Manu Tapu Drive</Address1>
                <Address2>Auckland Airport</Address2>
                <City>Auckland</City>
                <State>AUK</State>
                <PostCode>2022</PostCode>
                <FCLEquipmentNeeded>ASK</FCLEquipmentNeeded>
                <LCLEquipmentNeeded>ASK</LCLEquipmentNeeded>
                <AIREquipmentNeeded>ASK</AIREquipmentNeeded>
                <RelatedPortCode>
                  <Code>NZAKL</Code>
                </RelatedPortCode>
                <CountryCode>
                  <Code>NZ</Code>
                </CountryCode>
                <OrgAddressCapabilityCollection>
                  <OrgAddressCapability Action=""INSERT"">
                    <AddressType>OFD</AddressType>
                    <IsMainAddress>true</IsMainAddress>
                  </OrgAddressCapability>
                </OrgAddressCapabilityCollection>
              </OrgAddress>
            </OrgAddressCollection>
          </OrgHeader>
        </Organization>
      </Body>
    </Native>
";

		#endregion XML_ImportOrgHeaderWithEmptyAddressCapabilityCollection

		public void TestImportOrgHeaderWithOrgAddressAdditionalInfoCollection()
		{
			DataRegistry.Instance.CanUserEditOrganisationCode = true;

			var header = new BusinessObjectFactory().New<OrgHeader>();
			var address = header.MainAddress;
			address.OA_Address1 = "74 Oriordan St";
			address.OA_City = "Alexandria";
			address.OA_PostCode = "2015";
			address.OA_RL_NKRelatedPortCode = "AUSYD";
			header.OH_RL_NKClosestPort = "AUSYD";
			header.OH_FullName = "WISETECH";
			header.OH_Code = "TESTOHCODE";

			var addressInfo1 = address.AdditionalInfos.AddNew();
			addressInfo1.OAI_AdditionalInfo = "BUILDING A";
			addressInfo1.OAI_IsPrimary = true;

			var addressInfo2 = address.AdditionalInfos.AddNew();
			addressInfo2.OAI_AdditionalInfo = "BUILDING B";
			addressInfo1.OAI_IsPrimary = false;

			var translatedAddress = address.TranslatedAddresses.AddNew();
			translatedAddress.OTA_Address1 = "74, rue Oriordan";
			translatedAddress.OTA_City = "Alexandrie";
			translatedAddress.OTA_PostCode = "2015";
			translatedAddress.OTA_Language = Core.SharedConstants.Languages.French;

			var wrapperCollection = new OrgTranslatedAddressAdditionalInfoWrapperCollection(translatedAddress);
			wrapperCollection[0].TranslatedAdditionalInfo = "BUILDING A - FR";
			wrapperCollection[1].TranslatedAdditionalInfo = "BUILDING B - FR";

			header.Factory.Save();

			#region XML_UpdateOrganizationWithOrgAddressAdditionalInfoCollection

			var xmlOrgAddressAdditionalInfo = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
	<OwnerCode>EDICUS</OwnerCode>
	<EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
	<Organization>
	  <OrgHeader Action=""MERGE"">
		<PK>{header.PK}</PK>
		<Code>TESTOHCODE</Code>
		<FullName>WISETECH GLOBAL</FullName>
		<OrgAddressCollection>
			<OrgAddress Action=""MERGE"">
				<PK>{address.PK}</PK>
				<OrgAddressAdditionalInfoCollection>
					<OrgAddressAdditionalInfo Action=""MERGE"">
						<PK>{addressInfo1.PK}</PK>
						<IsPrimary>true</IsPrimary>
						<AdditionalInfo>BUILDING A1</AdditionalInfo>
						<OrgTranslatedAddressAdditionalInfoCollection>
							<OrgTranslatedAddressAdditionalInfo Action = ""MERGE"">
								<PK>{wrapperCollection[0].PK}</PK>
								<Language>FR-FR</Language>
								<AdditionalInfo>BUILDING A1-FR</AdditionalInfo>
							</OrgTranslatedAddressAdditionalInfo>
						</OrgTranslatedAddressAdditionalInfoCollection>
					 </OrgAddressAdditionalInfo>
					<OrgAddressAdditionalInfo Action=""MERGE"">
						<PK>{addressInfo2.PK}</PK>
						<IsPrimary>false</IsPrimary>
						<AdditionalInfo>BUILDING B1</AdditionalInfo>
						<OrgTranslatedAddressAdditionalInfoCollection>
							<OrgTranslatedAddressAdditionalInfo Action = ""MERGE"">
								<PK>{wrapperCollection[1].PK}</PK>
								<Language>FR-FR</Language>
								<AdditionalInfo>BUILDING B1-FR</AdditionalInfo>
							</OrgTranslatedAddressAdditionalInfo>
						</OrgTranslatedAddressAdditionalInfoCollection>
					 </OrgAddressAdditionalInfo>
				</OrgAddressAdditionalInfoCollection>
			</OrgAddress>
		</OrgAddressCollection>
		<ClosestPort TableName=""RefUNLOCO"">
			<Code>AUSYD</Code>
		</ClosestPort>
	  </OrgHeader>
	</Organization>
  </Body>
</Native>";

			#endregion

			var actualLog = xmlOrgAddressAdditionalInfo.ImportNativeXmlReturningLog();
			var expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 0 inserts, 1 updates, 0 deletes
OrgAddress - 0 inserts, 1 updates, 0 deletes
OrgAddressAdditionalInfo - 0 inserts, 2 updates, 0 deletes
OrgTranslatedAddressAdditionalInfo - 0 inserts, 1 updates, 0 deletes".Trim();

			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("Log Text", expectedLog, actualLog);

				header = new BusinessObjectFactory().Load<OrgHeader>(header.PK);
				AssertEquals("header.OH_FullName", "WISETECH GLOBAL", header.OH_FullName);

				var additionalInfo1 = header.Addresses[0].AdditionalInfos[0];
				AssertEquals("additionInfo1.OAI_AdditionalInfo", "BUILDING A1", additionalInfo1.OAI_AdditionalInfo);

				var translatedInfo1 = header.Addresses[0].AdditionalInfos[0].TranslatedInfos[0];
				AssertEquals("translatedInfo1.OTI_AdditionalInfo", "BUILDING A - FR", translatedInfo1.OTI_AdditionalInfo);
				AssertEquals("translatedInfo1.OTI_Language", "FR-FR", translatedInfo1.OTI_Language);

				var additionalInfo2 = header.Addresses[0].AdditionalInfos[1];
				AssertEquals("additionInfo1.OAI_AdditionalInfo", "BUILDING B1", additionalInfo2.OAI_AdditionalInfo);

				var translatedInfo2 = header.Addresses[0].AdditionalInfos[1].TranslatedInfos[0];
				AssertEquals("translatedInfo2.OTI_AdditionalInfo", "BUILDING B1-FR", translatedInfo2.OTI_AdditionalInfo);
				AssertEquals("translatedInfo2.OTI_Language", "FR-FR", translatedInfo2.OTI_Language);
			});
		}

		#region OrgWhsClientAccountAssociation

		public void TestImportOrgHeader_OrgWhsAssociation_SalesChannel()
		{
			DataRegistry.Instance.CanUserEditOrganisationCode = true;

			var organisation = new BusinessObjectFactory().New<OrgHeader>();
			organisation.OH_FullName = "PREVIOUS NAME";
			var mainAddress = organisation.MainAddress;
			mainAddress.OA_Address1 = "123 TEST STREET";
			mainAddress.OA_City = "TESTVILLE";
			mainAddress.OA_PostCode = "2892";
			mainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			organisation.OH_RL_NKClosestPort = "AUSYD";
			organisation.OH_Code = "RANDOMCODE";

			var carrier = organisation.Factory.NewWithValidTestData<OrgHeader>();
			var carrierAccount = carrier.CarrierAccounts.AddNew();
			carrierAccount.OAN_AccountNumber = "TestingNum";
			carrierAccount.OAN_DepotID = "Depot";
			carrierAccount.OAN_MerchantNumber = "Mech";

			var salesChannel = organisation.Factory.New<IWhsSalesChannel>();
			salesChannel.WSH_Code = "ABC";
			salesChannel.WSH_Description = "ABC Test";
			organisation.Factory.Save();

			#region XML_UpdateOrganizationIncludingSalesChannel

			var xml = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<ReferenceData xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <Header xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.cargowise.com/Schemas/Universal"">
    <OwnerCode>GLOFORSYD</OwnerCode>
    <EnableCodeMapping>false</EnableCodeMapping>
  </Header>
  <Body>
    <Organization>
      <OrgHeader Action=""MERGE"">
        <Code>RANDOMCODE</Code>
        <FullName>ANOTHER NAME CO</FullName>
        <ClosestPort TableName=""RefUNLOCO"">
          <Code>AUSYD</Code>
        </ClosestPort>
        <OrgWhsClientAccountAssociationCollection>
          <OrgWhsClientAccountAssociation Action=""MERGE"">
            <SalesChannel>
              <Code>ABC</Code>
              <Description>ABC Test</Description>
            </SalesChannel>
            <CarrierAccount>
              <PK>{carrierAccount.PK}</PK>
              <AccountNumber>TestingNum</AccountNumber>
            </CarrierAccount>
          </OrgWhsClientAccountAssociation>
        </OrgWhsClientAccountAssociationCollection>
      </OrgHeader>
    </Organization>
  </Body>
</ReferenceData>";

			#endregion

			string actualLog;
			var encoding = new UTF8Encoding();
			using (var stream = new MemoryStream(encoding.GetBytes(xml)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);
				actualLog = manager.GetLogs();
			}

			CombineAssertions(delegate
			{
				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 0 inserts, 1 updates, 0 deletes
OrgWhsClientAccountAssociation - 1 inserts, 0 updates, 0 deletes
				".Trim();
				AssertMultilineASCIIEquals("Log Text", expectedLog, actualLog);

				organisation = new BusinessObjectFactory().Load<OrgHeader>(organisation.PK);
				AssertEquals("organisation.OH_FullName", "ANOTHER NAME CO", organisation.OH_FullName);
				AssertEquals("organisation.OH_Code", "RANDOMCODE", organisation.OH_Code);
				AssertEquals("organisation.OrgWhsClientAccountAssociations[0].OWC_OAN_CarrierAccount", carrierAccount.PK, organisation.OrgWhsClientAccountAssociations[0].OWC_OAN_CarrierAccount);
				AssertEquals("organisation.OrgWhsClientAccountAssociations[0].OWC_WSH_SalesChannel", salesChannel.PK, organisation.OrgWhsClientAccountAssociations[0].OWC_WSH_SalesChannel);
			});
		}

		#endregion

		#region OrgCarrierNamedAccount

		public void TestImportOrgHeaderWithOrgCarrierNamedAccountCollection()
		{
			DataRegistry.Instance.CanUserEditOrganisationCode = true;

			var header = new BusinessObjectFactory().New<OrgHeader>();
			var address = header.MainAddress;
			address.OA_Address1 = "74 Oriordan St";
			address.OA_City = "Alexandria";
			address.OA_PostCode = "2015";
			address.OA_RL_NKRelatedPortCode = "AUSYD";
			header.OH_RL_NKClosestPort = "AUSYD";
			header.OH_FullName = "WISETECH";
			header.OH_Code = "TESTOHCODE";

			var carrierNamedAccount = header.CarrierNamedAccounts.AddNew();
			carrierNamedAccount.ONA_ForeignName = "Test";
			carrierNamedAccount.ONA_OH_Organization = header.PK;

			header.Factory.Save();

			#region XML_UpdateOrganizationWithOrgCarrierNamedAccountCollection

			var xml = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
	<OwnerCode>EDICUS</OwnerCode>
	<EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
	<Organization>
	  <OrgHeader Action=""MERGE"">
		<PK>{header.PK}</PK>
		<Code>TESTOHCODE</Code>
		<FullName>WISETECH GLOBAL</FullName>
        <ClosestPort TableName=""RefUNLOCO"">
          <Code>AUSYD</Code>
        </ClosestPort>
        <OrgCarrierNamedAccountCollection>
          <OrgCarrierNamedAccount Action=""MERGE"">
			<PK>{carrierNamedAccount.PK}</PK>
            <ForeignName>New Name</ForeignName>
            <Organization TableName=""OrgHeader"">
              <Code>TESTOHCODE</Code>
            </Organization>
          </OrgCarrierNamedAccount>
        </OrgCarrierNamedAccountCollection>
      </OrgHeader>
    </Organization>
  </Body>
</ReferenceData>";

			#endregion

			CombineAssertions(delegate
			{
				var actualLog = xml.ImportNativeXmlReturningLog();
				var expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 0 inserts, 1 updates, 0 deletes
OrgCarrierNamedAccount - 0 inserts, 1 updates, 0 deletes
				".Trim();
				AssertMultilineASCIIEquals("Log Text", expectedLog, actualLog);

				header = new BusinessObjectFactory().Load<OrgHeader>(header.PK);
				AssertEquals("organisation.CarrierNamedAccounts[0].ONA_OH_Carrier", header.PK, header.CarrierNamedAccounts[0].ONA_OH_Carrier);
				AssertEquals("organisation.CarrierNamedAccounts[0].ONA_ForeignName", "New Name", header.CarrierNamedAccounts[0].ONA_ForeignName);
			});
		}

		#endregion

		public void TestCanAddAnOrganizationSpecifyingTheOrganizationCodeWithItRegenerating()
		{
			DataRegistry.Instance.CanUserEditOrganisationCode = true;

			string actualLog;
			var encoding = new UTF8Encoding();
			using (var stream = new MemoryStream(encoding.GetBytes(XML_InsertOrganizationWithOrganizationCode)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);
				actualLog = manager.GetLogs();
			}

			CombineAssertions(delegate
			{
				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 1 inserts, 0 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Log Text", expectedLog, actualLog);

				var organisations = new BusinessObjectFactory().Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "RANDOMCODE"));
				AssertEquals("Should have an Org with a Code of 'RANDOMCODE'", 1, organisations.Length);
				var organisation = organisations[0];
				AssertEquals("organisation.OH_FullName", "ANOTHER NAME CO", organisation.OH_FullName);
				AssertEquals("organisation.OH_Code", "RANDOMCODE", organisation.OH_Code);
				AssertEquals("organisation.OH_RL_NKClosestPort", "AUPER", organisation.OH_RL_NKClosestPort);
			});
		}

		#region XML_InsertOrganizationWithOrganizationCode

		const string XML_InsertOrganizationWithOrganizationCode = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ReferenceData xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <Header xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.cargowise.com/Schemas/Universal"">
    <OwnerCode>GLOFORSYD</OwnerCode>
    <EnableCodeMapping>false</EnableCodeMapping>
  </Header>
  <Body>
    <Organization>
      <OrgHeader Action=""INSERT"">
        <Code>RANDOMCODE</Code>
        <FullName>ANOTHER NAME CO</FullName>
        <ClosestPort TableName=""RefUNLOCO"">
          <Code>AUPER</Code>
        </ClosestPort>
      </OrgHeader>
    </Organization>
  </Body>
</ReferenceData>";

		#endregion

		public void TestCanUpdateAnOrganizationReferencedByOrganizationCodeAlone()
		{
			DataRegistry.Instance.CanUserEditOrganisationCode = true;

			var organisation = new BusinessObjectFactory().New<OrgHeader>();
			organisation.OH_FullName = "WRONG NAME";
			var mainAddress = organisation.MainAddress;
			mainAddress.OA_Address1 = "123 TEST STREET";
			mainAddress.OA_City = "TESTVILLE";
			mainAddress.OA_PostCode = "2892";
			mainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			organisation.OH_RL_NKClosestPort = "AUSYD";
			organisation.OH_Code = "RANDOMCODE";

			organisation.Factory.Save();

			string actualLog;
			var encoding = new UTF8Encoding();
			using (var stream = new MemoryStream(encoding.GetBytes(XML_UpdateOrganizationByOrganizationCode)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);
				actualLog = manager.GetLogs();
			}

			CombineAssertions(delegate
			{
				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 0 inserts, 1 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Log Text", expectedLog, actualLog);

				organisation = new BusinessObjectFactory().Load<OrgHeader>(organisation.PK);
				AssertEquals("organisation.OH_FullName", "ANOTHER NAME CO", organisation.OH_FullName);
				AssertEquals("organisation.OH_Code", "RANDOMCODE", organisation.OH_Code);
			});
		}

		#region XML_UpdateOrganizationByOrganizationCode

		const string XML_UpdateOrganizationByOrganizationCode = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ReferenceData xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <Header xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.cargowise.com/Schemas/Universal"">
    <OwnerCode>GLOFORSYD</OwnerCode>
    <EnableCodeMapping>false</EnableCodeMapping>
  </Header>
  <Body>
    <Organization>
      <OrgHeader Action=""MERGE"">
        <Code>RANDOMCODE</Code>
        <FullName>ANOTHER NAME CO</FullName>
        <ClosestPort TableName=""RefUNLOCO"">
          <Code>AUSYD</Code>
        </ClosestPort>
      </OrgHeader>
    </Organization>
  </Body>
</ReferenceData>";

		#endregion

		public void TestImportingOrgAppointedAgentPortsCollectionAsPerBugReportedIn_WI00030617()
		{
			var agentOrg = Factory.New<OrgHeader>();
			agentOrg.OH_FullName = "RATHAUS EARVAAG GMBH";
			agentOrg.OH_RL_NKClosestPort = "DEHAM";
			agentOrg.OH_Code = "RATEARHAM";
			var agentAdd = agentOrg.MainAddress;
			agentAdd.OA_Address1 = "3489 BLAU KASE STRASSE";
			agentAdd.OA_City = "HAMBURG";
			agentAdd.OA_PostCode = "12345";
			agentAdd.OA_RL_NKRelatedPortCode = "DEHAM";
			agentAdd.OA_Code = "DEHAM - 3489BLAUKASESTRAS";
			Factory.Save();

			var encoding = new UTF8Encoding();
			using (var stream = new MemoryStream(encoding.GetBytes(XMLFrom_WI00030617)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 1 inserts, 0 updates, 0 deletes
OrgAddress - 1 inserts, 0 updates, 0 deletes
OrgAddressCapability - 1 inserts, 0 updates, 0 deletes
OrgAppointedAgentPorts - 1 inserts, 0 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Log Text", expectedLog, manager.GetLogs());

				var importedOrg = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "FLAASPMEL"));
				AssertNotNull("Imported Org (with code of 'FLAASPMEL')", importedOrg);
				CombineAssertions(delegate
				{
					AssertEquals("importedOrg.OH_FullName", "FLAT ASP ROADWORKS", importedOrg.OH_FullName);
					AssertEquals("importedOrg.AppointedAgentPorts.Count", 1, importedOrg.AppointedAgentPorts.Count);
					var importedAgentPort = importedOrg.AppointedAgentPorts[0];
					AssertEquals("importedAgentPort.O5_SeaAirCarrierOrForwarderType", "FWD", importedAgentPort.O5_SeaAirCarrierOrForwarderType);
					AssertEquals("importedAgentPort.O5_AgentDirection", "BTH", importedAgentPort.O5_AgentDirection);
					AssertEquals("importedAgentPort.O5_PortOrCountry", "DEHAM", importedAgentPort.O5_PortOrCountry);
					AssertEquals("importedAgentPort.O5_OA_AgentOfficeAddress", agentAdd.PK, importedAgentPort.O5_OA_AgentOfficeAddress);
				});
			}
		}

		#region XMLFrom_WI00030617
		const string XMLFrom_WI00030617 = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<ReferenceData xsi:schemaLocation=""http://www.cargowise.com/Schemas/Universal"" xmlns=""http://www.cargowise.com/Schemas/Universal"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
	<Header>
		<EnableCodeMapping>false</EnableCodeMapping>
	</Header>
	<Body>
		<Organization>
			<OrgHeader Action=""MERGE"">
				<Code>FLAASPMEL</Code>
				<IsActive>true</IsActive>
				<FullName>FLAT ASP ROADWORKS</FullName>
				<IsForwarder>true</IsForwarder>
				<ClosestPort>
					<Code>AUMEL</Code>
				</ClosestPort>
				<OrgAddressCollection>
					<OrgAddress Action=""MERGE"">
						<Code>AUMEL - 42SALISBURYLANE</Code>
						<Language>EN</Language>
						<Address1>42 SALISBURY LANE</Address1>
						<City>TULLAMARINE</City>
						<State>VIC</State>
						<PostCode>3043</PostCode>
						<Phone>+61383361000</Phone>
						<Fax>+61393361001</Fax>
						<FCLEquipmentNeeded>WUP</FCLEquipmentNeeded>
						<LCLEquipmentNeeded>PSL</LCLEquipmentNeeded>
						<AIREquipmentNeeded>PSL</AIREquipmentNeeded>
						<RelatedPortCode>
							<Code>AUMEL</Code>
						</RelatedPortCode>
						<OrgAddressCapabilityCollection>
							<OrgAddressCapability Action=""MERGE"">
								<AddressType>OFC</AddressType>
								<IsMainAddress>true</IsMainAddress>
							</OrgAddressCapability>
						</OrgAddressCapabilityCollection>
					</OrgAddress>
				</OrgAddressCollection>
        <OrgAppointedAgentPortsCollection>
          <OrgAppointedAgentPorts Action=""MERGE"">
            <SeaAirCarrierOrForwarderType>FWD</SeaAirCarrierOrForwarderType>
            <AgentDirection>BTH</AgentDirection>
            <PortOrCountry>DEHAM</PortOrCountry>
            <AgentOfficeAddress>
              <Code>DEHAM - 3489BLAUKASESTRAS</Code>
              <OrgHeader>
                <Code>RATEARHAM</Code>
              </OrgHeader>
            </AgentOfficeAddress>
          </OrgAppointedAgentPorts>
        </OrgAppointedAgentPortsCollection>
			</OrgHeader>
		</Organization>
	</Body>
</ReferenceData>";
		#endregion

		OrgHeader ValidateAndReturnOrganisation()
		{
			DataRegistry.Instance.CanUserEditOrganisationCode = true;

			var organisation = new BusinessObjectFactory().New<OrgHeader>();
			organisation.OH_FullName = "TEST FOR AR TERMS";
			var mainAddress = organisation.MainAddress;
			mainAddress.OA_Address1 = "123 TEST FOR AR TERMS";
			mainAddress.OA_City = "TESTVILLE";

			mainAddress.OA_PostCode = "2892";
			mainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			organisation.OH_RL_NKClosestPort = "AUSYD";
			organisation.OH_Code = "TESFORSYD";

			organisation.OH_IsDebtor = true;

			var terms = organisation.CompanyData.ARTerms;

			var orgARTerm = terms.AddNew();

			orgARTerm.PY_InvoiceClass = "ALL";
			orgARTerm.PY_InvoiceTerm = "COD";
			orgARTerm.PY_JobType = "AWB";
			orgARTerm.PY_Direction = "IMP";
			orgARTerm.PY_TransportMode = "ROA";

			orgARTerm = terms.AddNew();

			orgARTerm.PY_InvoiceClass = "ALL";
			orgARTerm.PY_InvoiceTerm = "COD";
			orgARTerm.PY_JobType = "SHP";
			orgARTerm.PY_Direction = "DOM";
			orgARTerm.PY_TransportMode = "RAI";

			AssertEquals("terms.Count", 3, terms.Count);

			organisation.Factory.Save();

			FindMatchingARTerm("Precondition:", terms, "ALL", "COD", "ALL", "ALL", "ALL", ZGuid.Empty, ZGuid.Empty);
			FindMatchingARTerm("Precondition:", terms, "ALL", "COD", "AWB", "IMP", "ROA", ZGuid.Empty, ZGuid.Empty);
			FindMatchingARTerm("Precondition:", terms, "ALL", "COD", "SHP", "DOM", "RAI", ZGuid.Empty, ZGuid.Empty);

			return organisation;
		}

		OrgHeader AssertImportLogAndReturnImportedOrganisation(OrgHeader organisation, string xmlFile, string expectedLog)
		{
			var encoding = new UTF8Encoding();
			using (var stream = new MemoryStream(encoding.GetBytes(xmlFile)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);
				var actualLog = manager.GetLogs();

				AssertMultilineASCIIEquals("Log Text", expectedLog, actualLog);

				organisation = new BusinessObjectFactory().Load<OrgHeader>(organisation.PK);

				return organisation;
			}
		}

		void FindMatchingARTerm(string condition, OrgARTermsCollection orgArTerms, string expectedInvoiceClass, string expectedInvoiceTerm, string expectedJobType, string expectedDirection, string expectedTransportMode, ZGuid expectedBanch, ZGuid expectedDepartment)
		{
			AssertNotNull(condition + " term[InvoiceClass: " + expectedInvoiceClass + " - InvoiceTerm: " + expectedInvoiceTerm + " - JobType: " + expectedJobType + " - Direction: " + expectedDirection + " - TransportMode: " + expectedTransportMode + " - Branch: " + expectedBanch + " - Department: " + expectedDepartment + "]",
						orgArTerms.FirstOrDefault(a => a.PY_InvoiceClass == expectedInvoiceClass &&
										a.PY_InvoiceTerm == expectedInvoiceTerm &&
										a.PY_JobType == expectedJobType &&
										a.PY_Direction == expectedDirection &&
										a.PY_TransportMode == expectedTransportMode &&
										a.PY_GB_Branch == expectedBanch &&
										a.PY_GE_Department == expectedDepartment));
		}

		public void TestOrgARTermsWithDifferentInvoiceClassNotMerged()
		{
			var organisation = ValidateAndReturnOrganisation();

			var expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 0 inserts, 0 updates, 0 deletes
OrgCompanyData - 0 inserts, 0 updates, 0 deletes
OrgARTerms - 1 inserts, 0 updates, 0 deletes
				".Trim();

			var orgArTerms = AssertImportLogAndReturnImportedOrganisation(organisation, XMLFromInvoiceClass, expectedLog).CompanyData.ARTerms;

			AssertEquals("terms.Count", 4, orgArTerms.Count);

			FindMatchingARTerm("Postcondition: ", orgArTerms, "ALL", "COD", "ALL", "ALL", "ALL", ZGuid.Empty, ZGuid.Empty);
			FindMatchingARTerm("Postcondition: ", orgArTerms, "ALL", "COD", "AWB", "IMP", "ROA", ZGuid.Empty, ZGuid.Empty);
			FindMatchingARTerm("Postcondition: ", orgArTerms, "ALL", "COD", "SHP", "DOM", "RAI", ZGuid.Empty, ZGuid.Empty);
			FindMatchingARTerm("Postcondition: ", orgArTerms, "DSB", "COD", "SHP", "DOM", "RAI", ZGuid.Empty, ZGuid.Empty);
		}

		#region XMLFromInvoiceClass

		const string XMLFromInvoiceClass = @"<?xml version=""1.0"" encoding=""utf-8""?>
			<ReferenceData xmlns=""http://www.cargowise.com/Schemas/Universal"">
			  <Header xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.cargowise.com/Schemas/Universal"">
					<EnableCodeMapping>false</EnableCodeMapping>
			  </Header>
			  <Body>
				<Organization>
				  <OrgHeader Action=""MERGE"">
					<Code>TESFORSYD</Code>
					<FullName>TEST FOR AR TERMS</FullName>
					<OrgCompanyDataCollection>
					  <OrgCompanyData Action=""MERGE"">
						<OrgARTermsCollection>
							<OrgARTerms Action=""MERGE"">
								<InvoiceClass>ALL</InvoiceClass>
								<InvoiceTerm>COD</InvoiceTerm>
								<InvoiceDays>0</InvoiceDays>
								<AgreedPaymentMethod></AgreedPaymentMethod>
								<JobType>ALL</JobType>
								<Direction>ALL</Direction>
								<TransportMode>ALL</TransportMode>
								<Department TableName=""GlbDepartment"" />
								<Branch TableName=""GlbBranch"" />
							</OrgARTerms>
							<OrgARTerms Action=""MERGE"">
								<InvoiceClass>ALL</InvoiceClass>
								<InvoiceTerm>COD</InvoiceTerm>
								<InvoiceDays>0</InvoiceDays>
								<AgreedPaymentMethod></AgreedPaymentMethod>
								<JobType>AWB</JobType>
								<Direction>IMP</Direction>
								<TransportMode>ROA</TransportMode>
								<Department TableName=""GlbDepartment"" />
								<Branch TableName=""GlbBranch"" />
							</OrgARTerms>
							<OrgARTerms Action=""MERGE"">
								<InvoiceClass>DSB</InvoiceClass>
								<InvoiceTerm>COD</InvoiceTerm>
								<InvoiceDays>0</InvoiceDays>
								<AgreedPaymentMethod></AgreedPaymentMethod>
								<JobType>SHP</JobType>
								<Direction>DOM</Direction>
								<TransportMode>RAI</TransportMode>
								<Department TableName=""GlbDepartment"" />
								<Branch TableName=""GlbBranch"" />
							</OrgARTerms>
						</OrgARTermsCollection>
						<GlbCompany>
						  <Code>EDI</Code>
						</GlbCompany>
					  </OrgCompanyData>
					</OrgCompanyDataCollection>
					<ClosestPort TableName=""RefUNLOCO"">
					  <Code>AUSYD</Code>
					</ClosestPort>
				  </OrgHeader>
				</Organization>
			  </Body>
			</ReferenceData>";

		#endregion

		public void TestOrgARTermsWithDifferentInvoiceTermMerged()
		{
			var organisation = ValidateAndReturnOrganisation();

			var expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 0 inserts, 0 updates, 0 deletes
OrgCompanyData - 0 inserts, 0 updates, 0 deletes
OrgARTerms - 0 inserts, 1 updates, 0 deletes
				".Trim();

			var orgArTerms = AssertImportLogAndReturnImportedOrganisation(organisation, XMLFromInvoiceTermMerged, expectedLog).CompanyData.ARTerms;

			AssertEquals("terms.Count", 3, orgArTerms.Count);

			FindMatchingARTerm("Postcondition: ", orgArTerms, "ALL", "PIA", "ALL", "ALL", "ALL", ZGuid.Empty, ZGuid.Empty);
			FindMatchingARTerm("Postcondition: ", orgArTerms, "ALL", "COD", "AWB", "IMP", "ROA", ZGuid.Empty, ZGuid.Empty);
			FindMatchingARTerm("Postcondition: ", orgArTerms, "ALL", "COD", "SHP", "DOM", "RAI", ZGuid.Empty, ZGuid.Empty);
		}

		#region XMLFromInvoiceTermMerged

		const string XMLFromInvoiceTermMerged = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ReferenceData xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <Header xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.cargowise.com/Schemas/Universal"">
		<EnableCodeMapping>false</EnableCodeMapping>
  </Header>
  <Body>
    <Organization>
      <OrgHeader Action=""MERGE"">
        <Code>TESFORSYD</Code>
        <FullName>TEST FOR AR TERMS</FullName>
        <OrgCompanyDataCollection>
          <OrgCompanyData Action=""MERGE"">
            <OrgARTermsCollection>
				<OrgARTerms Action=""MERGE"">
					<InvoiceClass>ALL</InvoiceClass>
					<InvoiceTerm>PIA</InvoiceTerm>
					<InvoiceDays>0</InvoiceDays>
					<AgreedPaymentMethod></AgreedPaymentMethod>
					<JobType>ALL</JobType>
					<Direction>ALL</Direction>
					<TransportMode>ALL</TransportMode>
					<Department TableName=""GlbDepartment"" />
					<Branch TableName=""GlbBranch"" />
				</OrgARTerms>
				<OrgARTerms Action=""MERGE"">
					<InvoiceClass>ALL</InvoiceClass>
					<InvoiceTerm>COD</InvoiceTerm>
					<InvoiceDays>0</InvoiceDays>
					<AgreedPaymentMethod></AgreedPaymentMethod>
					<JobType>AWB</JobType>
					<Direction>IMP</Direction>
					<TransportMode>ROA</TransportMode>
					<Department TableName=""GlbDepartment"" />
					<Branch TableName=""GlbBranch"" />
				</OrgARTerms>
				<OrgARTerms Action=""MERGE"">
					<InvoiceClass>ALL</InvoiceClass>
					<InvoiceTerm>COD</InvoiceTerm>
					<InvoiceDays>0</InvoiceDays>
					<AgreedPaymentMethod></AgreedPaymentMethod>
					<JobType>SHP</JobType>
					<Direction>DOM</Direction>
					<TransportMode>RAI</TransportMode>
					<Department TableName=""GlbDepartment"" />
					<Branch TableName=""GlbBranch"" />
				</OrgARTerms>
            </OrgARTermsCollection>
            <GlbCompany>
              <Code>EDI</Code>
            </GlbCompany>
          </OrgCompanyData>
        </OrgCompanyDataCollection>
        <ClosestPort TableName=""RefUNLOCO"">
          <Code>AUSYD</Code>
        </ClosestPort>
      </OrgHeader>
    </Organization>
  </Body>
</ReferenceData>";

		#endregion

		public void TestOrgARTermsWithDifferentJobTypeNotMerged()
		{
			var organisation = ValidateAndReturnOrganisation();

			var expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 0 inserts, 0 updates, 0 deletes
OrgCompanyData - 0 inserts, 0 updates, 0 deletes
OrgARTerms - 1 inserts, 0 updates, 0 deletes
				".Trim();

			var orgArTerms = AssertImportLogAndReturnImportedOrganisation(organisation, XMLFromJobType, expectedLog).CompanyData.ARTerms;

			AssertEquals("terms.Count", 4, orgArTerms.Count);

			FindMatchingARTerm("Postcondition: ", orgArTerms, "ALL", "COD", "ALL", "ALL", "ALL", ZGuid.Empty, ZGuid.Empty);
			FindMatchingARTerm("Postcondition: ", orgArTerms, "ALL", "COD", "AWB", "IMP", "ROA", ZGuid.Empty, ZGuid.Empty);
			FindMatchingARTerm("Postcondition: ", orgArTerms, "ALL", "COD", "SHP", "DOM", "RAI", ZGuid.Empty, ZGuid.Empty);
			FindMatchingARTerm("Postcondition: ", orgArTerms, "ALL", "COD", "GCN", "IMP", "ROA", ZGuid.Empty, ZGuid.Empty);
		}

		#region XMLFromJobType

		const string XMLFromJobType = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ReferenceData xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <Header xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.cargowise.com/Schemas/Universal"">
		<EnableCodeMapping>false</EnableCodeMapping>
  </Header>
  <Body>
    <Organization>
      <OrgHeader Action=""MERGE"">
        <Code>TESFORSYD</Code>
        <FullName>TEST FOR AR TERMS</FullName>
        <OrgCompanyDataCollection>
          <OrgCompanyData Action=""MERGE"">
            <OrgARTermsCollection>
				<OrgARTerms Action=""MERGE"">
					<InvoiceClass>ALL</InvoiceClass>
					<InvoiceTerm>COD</InvoiceTerm>
					<InvoiceDays>0</InvoiceDays>
					<AgreedPaymentMethod></AgreedPaymentMethod>
					<JobType>ALL</JobType>
					<Direction>ALL</Direction>
					<TransportMode>ALL</TransportMode>
					<Department TableName=""GlbDepartment"" />
					<Branch TableName=""GlbBranch"" />
				</OrgARTerms>
				<OrgARTerms Action=""MERGE"">
					<InvoiceClass>ALL</InvoiceClass>
					<InvoiceTerm>COD</InvoiceTerm>
					<InvoiceDays>0</InvoiceDays>
					<AgreedPaymentMethod></AgreedPaymentMethod>
					<JobType>GCN</JobType>
					<Direction>IMP</Direction>
					<TransportMode>ROA</TransportMode>
					<Department TableName=""GlbDepartment"" />
					<Branch TableName=""GlbBranch"" />
				</OrgARTerms>
				<OrgARTerms Action=""MERGE"">
					<InvoiceClass>ALL</InvoiceClass>
					<InvoiceTerm>COD</InvoiceTerm>
					<InvoiceDays>0</InvoiceDays>
					<AgreedPaymentMethod></AgreedPaymentMethod>
					<JobType>SHP</JobType>
					<Direction>DOM</Direction>
					<TransportMode>RAI</TransportMode>
					<Department TableName=""GlbDepartment"" />
					<Branch TableName=""GlbBranch"" />
				</OrgARTerms>
            </OrgARTermsCollection>
            <GlbCompany>
              <Code>EDI</Code>
            </GlbCompany>
          </OrgCompanyData>
        </OrgCompanyDataCollection>
        <ClosestPort TableName=""RefUNLOCO"">
          <Code>AUSYD</Code>
        </ClosestPort>
      </OrgHeader>
    </Organization>
  </Body>
</ReferenceData>";

		#endregion

		public void TestOrgARTermsWithDifferentDirectionNotMerged()
		{
			var organisation = ValidateAndReturnOrganisation();

			var expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 0 inserts, 0 updates, 0 deletes
OrgCompanyData - 0 inserts, 0 updates, 0 deletes
OrgARTerms - 1 inserts, 0 updates, 0 deletes
				".Trim();

			var orgArTerms = AssertImportLogAndReturnImportedOrganisation(organisation, XMLFromDirection, expectedLog).CompanyData.ARTerms;

			AssertEquals("terms.Count", 4, orgArTerms.Count);

			FindMatchingARTerm("Postcondition: ", orgArTerms, "ALL", "COD", "ALL", "ALL", "ALL", ZGuid.Empty, ZGuid.Empty);
			FindMatchingARTerm("Postcondition: ", orgArTerms, "ALL", "COD", "AWB", "IMP", "ROA", ZGuid.Empty, ZGuid.Empty);
			FindMatchingARTerm("Postcondition: ", orgArTerms, "ALL", "COD", "SHP", "DOM", "RAI", ZGuid.Empty, ZGuid.Empty);
			FindMatchingARTerm("Postcondition: ", orgArTerms, "ALL", "COD", "SHP", "IMP", "RAI", ZGuid.Empty, ZGuid.Empty);
		}

		#region XMLFromDirection

		const string XMLFromDirection = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ReferenceData xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <Header xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.cargowise.com/Schemas/Universal"">
		<EnableCodeMapping>false</EnableCodeMapping>
  </Header>
  <Body>
    <Organization>
      <OrgHeader Action=""MERGE"">
        <Code>TESFORSYD</Code>
        <FullName>TEST FOR AR TERMS</FullName>
        <OrgCompanyDataCollection>
          <OrgCompanyData Action=""MERGE"">
            <OrgARTermsCollection>
				<OrgARTerms Action=""MERGE"">
					<InvoiceClass>ALL</InvoiceClass>
					<InvoiceTerm>COD</InvoiceTerm>
					<InvoiceDays>0</InvoiceDays>
					<AgreedPaymentMethod></AgreedPaymentMethod>
					<JobType>ALL</JobType>
					<Direction>ALL</Direction>
					<TransportMode>ALL</TransportMode>
					<Department TableName=""GlbDepartment"" />
					<Branch TableName=""GlbBranch"" />
				</OrgARTerms>
				<OrgARTerms Action=""MERGE"">
					<InvoiceClass>ALL</InvoiceClass>
					<InvoiceTerm>COD</InvoiceTerm>
					<InvoiceDays>0</InvoiceDays>
					<AgreedPaymentMethod></AgreedPaymentMethod>
					<JobType>AWB</JobType>
					<Direction>IMP</Direction>
					<TransportMode>ROA</TransportMode>
					<Department TableName=""GlbDepartment"" />
					<Branch TableName=""GlbBranch"" />
				</OrgARTerms>
				<OrgARTerms Action=""MERGE"">
					<InvoiceClass>ALL</InvoiceClass>
					<InvoiceTerm>COD</InvoiceTerm>
					<InvoiceDays>0</InvoiceDays>
					<AgreedPaymentMethod></AgreedPaymentMethod>
					<JobType>SHP</JobType>
					<Direction>IMP</Direction>
					<TransportMode>RAI</TransportMode>
					<Department TableName=""GlbDepartment"" />
					<Branch TableName=""GlbBranch"" />
				</OrgARTerms>
            </OrgARTermsCollection>
            <GlbCompany>
              <Code>EDI</Code>
            </GlbCompany>
          </OrgCompanyData>
        </OrgCompanyDataCollection>
        <ClosestPort TableName=""RefUNLOCO"">
          <Code>AUSYD</Code>
        </ClosestPort>
      </OrgHeader>
    </Organization>
  </Body>
</ReferenceData>";

		#endregion

		public void TestOrgARTermsWithDifferentTransportModeNotMerged()
		{
			var organisation = ValidateAndReturnOrganisation();

			var expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 0 inserts, 0 updates, 0 deletes
OrgCompanyData - 0 inserts, 0 updates, 0 deletes
OrgARTerms - 1 inserts, 0 updates, 0 deletes
				".Trim();

			var orgArTerms = AssertImportLogAndReturnImportedOrganisation(organisation, XMLFromTransportMode, expectedLog).CompanyData.ARTerms;

			AssertEquals("terms.Count", 4, orgArTerms.Count);

			FindMatchingARTerm("Postcondition: ", orgArTerms, "ALL", "COD", "ALL", "ALL", "ALL", ZGuid.Empty, ZGuid.Empty);
			FindMatchingARTerm("Postcondition: ", orgArTerms, "ALL", "COD", "AWB", "IMP", "ROA", ZGuid.Empty, ZGuid.Empty);
			FindMatchingARTerm("Postcondition: ", orgArTerms, "ALL", "COD", "SHP", "DOM", "RAI", ZGuid.Empty, ZGuid.Empty);
			FindMatchingARTerm("Postcondition: ", orgArTerms, "ALL", "COD", "SHP", "DOM", "ROA", ZGuid.Empty, ZGuid.Empty);
		}

		#region XMLFromTransportMode

		const string XMLFromTransportMode = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ReferenceData xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <Header xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.cargowise.com/Schemas/Universal"">
		<EnableCodeMapping>false</EnableCodeMapping>
  </Header>
  <Body>
    <Organization>
      <OrgHeader Action=""MERGE"">
        <Code>TESFORSYD</Code>
        <FullName>TEST FOR AR TERMS</FullName>
        <OrgCompanyDataCollection>
          <OrgCompanyData Action=""MERGE"">
            <OrgARTermsCollection>
				<OrgARTerms Action=""MERGE"">
					<InvoiceClass>ALL</InvoiceClass>
					<InvoiceTerm>COD</InvoiceTerm>
					<InvoiceDays>0</InvoiceDays>
					<AgreedPaymentMethod></AgreedPaymentMethod>
					<JobType>ALL</JobType>
					<Direction>ALL</Direction>
					<TransportMode>ALL</TransportMode>
					<Department TableName=""GlbDepartment"" />
					<Branch TableName=""GlbBranch"" />
				</OrgARTerms>
				<OrgARTerms Action=""MERGE"">
					<InvoiceClass>ALL</InvoiceClass>
					<InvoiceTerm>COD</InvoiceTerm>
					<InvoiceDays>0</InvoiceDays>
					<AgreedPaymentMethod></AgreedPaymentMethod>
					<JobType>AWB</JobType>
					<Direction>IMP</Direction>
					<TransportMode>ROA</TransportMode>
					<Department TableName=""GlbDepartment"" />
					<Branch TableName=""GlbBranch"" />
				</OrgARTerms>
				<OrgARTerms Action=""MERGE"">
					<InvoiceClass>ALL</InvoiceClass>
					<InvoiceTerm>COD</InvoiceTerm>
					<InvoiceDays>0</InvoiceDays>
					<AgreedPaymentMethod></AgreedPaymentMethod>
					<JobType>SHP</JobType>
					<Direction>DOM</Direction>
					<TransportMode>ROA</TransportMode>
					<Department TableName=""GlbDepartment"" />
					<Branch TableName=""GlbBranch"" />
				</OrgARTerms>
            </OrgARTermsCollection>
            <GlbCompany>
              <Code>EDI</Code>
            </GlbCompany>
          </OrgCompanyData>
        </OrgCompanyDataCollection>
        <ClosestPort TableName=""RefUNLOCO"">
          <Code>AUSYD</Code>
        </ClosestPort>
      </OrgHeader>
    </Organization>
  </Body>
</ReferenceData>";

		#endregion

		public void TestOrgARTermsWithDifferentBranchNotMerged()
		{
			var organisation = ValidateAndReturnOrganisation();

			var otherBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, GlbBranch.CurrentBranch.PK));

			organisation.CompanyData.ARTerms[2].PY_GB_Branch = otherBranch.PK;

			organisation.Factory.Save();

			FindMatchingARTerm("Precondition:", organisation.CompanyData.ARTerms, "ALL", "COD", "SHP", "DOM", "RAI", otherBranch.PK, ZGuid.Empty);

			var expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 0 inserts, 0 updates, 0 deletes
OrgCompanyData - 0 inserts, 0 updates, 0 deletes
OrgARTerms - 1 inserts, 0 updates, 0 deletes
				".Trim();

			var orgArTerms = AssertImportLogAndReturnImportedOrganisation(organisation, XMLFromBranch, expectedLog).CompanyData.ARTerms;

			AssertEquals("terms.Count", 4, orgArTerms.Count);

			FindMatchingARTerm("Postcondition: ", orgArTerms, "ALL", "COD", "ALL", "ALL", "ALL", ZGuid.Empty, ZGuid.Empty);
			FindMatchingARTerm("Postcondition: ", orgArTerms, "ALL", "COD", "AWB", "IMP", "ROA", ZGuid.Empty, ZGuid.Empty);
			FindMatchingARTerm("Postcondition:", orgArTerms, "ALL", "COD", "SHP", "DOM", "RAI", otherBranch.PK, ZGuid.Empty);
			FindMatchingARTerm("Postcondition: ", orgArTerms, "ALL", "COD", "SHP", "DOM", "RAI", ZGuid.Empty, ZGuid.Empty);
		}

		#region XMLFromBranch

		const string XMLFromBranch = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ReferenceData xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <Header xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.cargowise.com/Schemas/Universal"">
		<EnableCodeMapping>false</EnableCodeMapping>
  </Header>
  <Body>
    <Organization>
      <OrgHeader Action=""MERGE"">
        <Code>TESFORSYD</Code>
        <FullName>TEST FOR AR TERMS</FullName>
        <OrgCompanyDataCollection>
          <OrgCompanyData Action=""MERGE"">
            <OrgARTermsCollection>
				<OrgARTerms Action=""MERGE"">
					<InvoiceClass>ALL</InvoiceClass>
					<InvoiceTerm>COD</InvoiceTerm>
					<InvoiceDays>0</InvoiceDays>
					<AgreedPaymentMethod></AgreedPaymentMethod>
					<JobType>ALL</JobType>
					<Direction>ALL</Direction>
					<TransportMode>ALL</TransportMode>
					<Department TableName=""GlbDepartment"" />
					<Branch TableName=""GlbBranch"" />
				</OrgARTerms>
				<OrgARTerms Action=""MERGE"">
					<InvoiceClass>ALL</InvoiceClass>
					<InvoiceTerm>COD</InvoiceTerm>
					<InvoiceDays>0</InvoiceDays>
					<AgreedPaymentMethod></AgreedPaymentMethod>
					<JobType>AWB</JobType>
					<Direction>IMP</Direction>
					<TransportMode>ROA</TransportMode>
					<Department TableName=""GlbDepartment"" />
					<Branch TableName=""GlbBranch"" />
				</OrgARTerms>
				<OrgARTerms Action=""MERGE"">
					<InvoiceClass>ALL</InvoiceClass>
					<InvoiceTerm>COD</InvoiceTerm>
					<InvoiceDays>0</InvoiceDays>
					<AgreedPaymentMethod></AgreedPaymentMethod>
					<JobType>SHP</JobType>
					<Direction>DOM</Direction>
					<TransportMode>RAI</TransportMode>
					<Department TableName=""GlbDepartment"" />
					<Branch TableName=""GlbBranch"" />
				</OrgARTerms>
            </OrgARTermsCollection>
            <GlbCompany>
              <Code>EDI</Code>
            </GlbCompany>
          </OrgCompanyData>
        </OrgCompanyDataCollection>
        <ClosestPort TableName=""RefUNLOCO"">
          <Code>AUSYD</Code>
        </ClosestPort>
      </OrgHeader>
    </Organization>
  </Body>
</ReferenceData>";

		#endregion

		public void TestOrgARTermsWithDifferentDepartmentNotMerged()
		{
			var organisation = ValidateAndReturnOrganisation();

			var otherDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.PK, SQLComparisonOperator.NotEqual, GlbDepartment.CurrentDepartment.PK));

			organisation.CompanyData.ARTerms[2].PY_GE_Department = otherDepartment.PK;

			organisation.Factory.Save();

			FindMatchingARTerm("Precondition:", organisation.CompanyData.ARTerms, "ALL", "COD", "SHP", "DOM", "RAI", ZGuid.Empty, otherDepartment.PK);

			var expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 0 inserts, 0 updates, 0 deletes
OrgCompanyData - 0 inserts, 0 updates, 0 deletes
OrgARTerms - 1 inserts, 0 updates, 0 deletes
				".Trim();

			var orgArTerms = AssertImportLogAndReturnImportedOrganisation(organisation, XMLFromDepartment, expectedLog).CompanyData.ARTerms;

			AssertEquals("terms.Count", 4, orgArTerms.Count);

			FindMatchingARTerm("Postcondition: ", orgArTerms, "ALL", "COD", "ALL", "ALL", "ALL", ZGuid.Empty, ZGuid.Empty);
			FindMatchingARTerm("Postcondition: ", orgArTerms, "ALL", "COD", "AWB", "IMP", "ROA", ZGuid.Empty, ZGuid.Empty);
			FindMatchingARTerm("Postcondition:", orgArTerms, "ALL", "COD", "SHP", "DOM", "RAI", ZGuid.Empty, otherDepartment.PK);
			FindMatchingARTerm("Postcondition: ", orgArTerms, "ALL", "COD", "SHP", "DOM", "RAI", ZGuid.Empty, ZGuid.Empty);
		}

		#region XMLFromDepartment

		const string XMLFromDepartment = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ReferenceData xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <Header xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.cargowise.com/Schemas/Universal"">
		<EnableCodeMapping>false</EnableCodeMapping>
  </Header>
  <Body>
    <Organization>
      <OrgHeader Action=""MERGE"">
        <Code>TESFORSYD</Code>
        <FullName>TEST FOR AR TERMS</FullName>
        <OrgCompanyDataCollection>
          <OrgCompanyData Action=""MERGE"">
            <OrgARTermsCollection>
				<OrgARTerms Action=""MERGE"">
					<InvoiceClass>ALL</InvoiceClass>
					<InvoiceTerm>COD</InvoiceTerm>
					<InvoiceDays>0</InvoiceDays>
					<AgreedPaymentMethod></AgreedPaymentMethod>
					<JobType>ALL</JobType>
					<Direction>ALL</Direction>
					<TransportMode>ALL</TransportMode>
					<Department TableName=""GlbDepartment"" />
					<Branch TableName=""GlbBranch"" />
				</OrgARTerms>
				<OrgARTerms Action=""MERGE"">
					<InvoiceClass>ALL</InvoiceClass>
					<InvoiceTerm>COD</InvoiceTerm>
					<InvoiceDays>0</InvoiceDays>
					<AgreedPaymentMethod></AgreedPaymentMethod>
					<JobType>AWB</JobType>
					<Direction>IMP</Direction>
					<TransportMode>ROA</TransportMode>
					<Department TableName=""GlbDepartment"" />
					<Branch TableName=""GlbBranch"" />
				</OrgARTerms>
				<OrgARTerms Action=""MERGE"">
					<InvoiceClass>ALL</InvoiceClass>
					<InvoiceTerm>COD</InvoiceTerm>
					<InvoiceDays>0</InvoiceDays>
					<AgreedPaymentMethod></AgreedPaymentMethod>
					<JobType>SHP</JobType>
					<Direction>DOM</Direction>
					<TransportMode>RAI</TransportMode>
					<Department TableName=""GlbDepartment"" />
					<Branch TableName=""GlbBranch"" />
				</OrgARTerms>
            </OrgARTermsCollection>
            <GlbCompany>
              <Code>EDI</Code>
            </GlbCompany>
          </OrgCompanyData>
        </OrgCompanyDataCollection>
        <ClosestPort TableName=""RefUNLOCO"">
          <Code>AUSYD</Code>
        </ClosestPort>
      </OrgHeader>
    </Organization>
  </Body>
</ReferenceData>";

		#endregion

		public void TestImportingOrgAddressCapabilitiesAsPerBugReportedIn_WI00030409()
		{
			var encoding = new UTF8Encoding();
			using (var stream = new MemoryStream(encoding.GetBytes(XMLFrom_WI00030409)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 1 inserts, 0 updates, 0 deletes
OrgContact - 1 inserts, 0 updates, 0 deletes
OrgAddress - 8 inserts, 0 updates, 0 deletes
OrgAddressCapability - 12 inserts, 0 updates, 0 deletes
OrgCusCode - 2 inserts, 0 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Log Text", expectedLog, manager.GetLogs());
			}
		}

		#region XMLFrom_WI00030409
		const string XMLFrom_WI00030409 = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<ReferenceData xsi:schemaLocation=""http://www.cargowise.com/Schemas/Universal"" xmlns=""http://www.cargowise.com/Schemas/Universal"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
	<Header>
		<EnableCodeMapping>false</EnableCodeMapping>
	</Header>
	<Body>
		<Organization>
			<OrgHeader Action=""MERGE"">
				<Code>AUADMCUSTRAW</Code>
				<IsActive>true</IsActive>
				<FullName>ADM CUSTOMS &amp; FREIGHT SERVICES PTY LTD</FullName>
				<IsConsignee>true</IsConsignee>
				<IsConsignor>true</IsConsignor>
				<IsForwarder>true</IsForwarder>
				<IsBroker>true</IsBroker>
				<IsCompetitor>true</IsCompetitor>
				<Language>EN</Language>
				<ClosestPort>
					<Code>AUMEL</Code>
				</ClosestPort>
				<OrgContactCollection>
					<OrgContact Action=""MERGE"">
						<ContactName>Jason See</ContactName>
						<Language>EN</Language>
						<NotifyMode>PRN</NotifyMode>
						<Phone>+61295562306</Phone>
						<Fax>+6125562535</Fax>
						<AttachmentType>PDF</AttachmentType>
					</OrgContact>
				</OrgContactCollection>
				<OrgAddressCollection>
					<OrgAddress Action=""MERGE"">
						<Code>AUSYD - 1C62533ALLENSTREE</Code>
						<Language>EN</Language>
						<Address1>1C6/25-33 ALLEN STREET</Address1>
						<City>WATERLOO</City>
						<State>NSW</State>
						<PostCode>2017</PostCode>
						<Phone>+61296900089</Phone>
						<Fax>+61296900087</Fax>
						<FCLEquipmentNeeded>WUP</FCLEquipmentNeeded>
						<LCLEquipmentNeeded>PSL</LCLEquipmentNeeded>
						<AIREquipmentNeeded>PSL</AIREquipmentNeeded>
						<RelatedPortCode>
							<Code>AUSYD</Code>
						</RelatedPortCode>
						<OrgAddressCapabilityCollection>
							<OrgAddressCapability Action=""MERGE"">
								<AddressType>OFC</AddressType>
								<IsMainAddress>false</IsMainAddress>
							</OrgAddressCapability>
							<OrgAddressCapability Action=""MERGE"">
								<AddressType>PAD</AddressType>
								<IsMainAddress>false</IsMainAddress>
							</OrgAddressCapability>
						</OrgAddressCapabilityCollection>
					</OrgAddress>
					<OrgAddress Action=""MERGE"">
						<Code>AUSYD - POBOX1451</Code>
						<Language>EN</Language>
						<Address1>PO BOX 1451</Address1>
						<City>STRAWBERRY HILLS</City>
						<State>NSW</State>
						<PostCode>2012</PostCode>
						<FCLEquipmentNeeded>WUP</FCLEquipmentNeeded>
						<LCLEquipmentNeeded>PSL</LCLEquipmentNeeded>
						<AIREquipmentNeeded>PSL</AIREquipmentNeeded>
						<RelatedPortCode>
							<Code>AUSYD</Code>
						</RelatedPortCode>
						<OrgAddressCapabilityCollection>
							<OrgAddressCapability Action=""MERGE"">
								<AddressType>PST</AddressType>
								<IsMainAddress>false</IsMainAddress>
							</OrgAddressCapability>
						</OrgAddressCapabilityCollection>
					</OrgAddress>
					<OrgAddress Action=""MERGE"">
						<Code>AUBNE - POBOX7308</Code>
						<Language>EN</Language>
						<Address1>PO BOX 7308</Address1>
						<City>REDLAND BAY</City>
						<State>QLD</State>
						<PostCode>4165</PostCode>
						<Phone>+61732069980</Phone>
						<FCLEquipmentNeeded>WUP</FCLEquipmentNeeded>
						<LCLEquipmentNeeded>PSL</LCLEquipmentNeeded>
						<AIREquipmentNeeded>PSL</AIREquipmentNeeded>
						<RelatedPortCode>
							<Code>AUBNE</Code>
						</RelatedPortCode>
						<OrgAddressCapabilityCollection>
							<OrgAddressCapability Action=""MERGE"">
								<AddressType>PST</AddressType>
								<IsMainAddress>false</IsMainAddress>
							</OrgAddressCapability>
						</OrgAddressCapabilityCollection>
					</OrgAddress>
					<OrgAddress Action=""MERGE"">
						<Code>AUPER - SUGARBIRDLADYROAD</Code>
						<Language>EN</Language>
						<Address1>SUGARBIRD LADY ROAD</Address1>
						<City>PERTH INTERNATIONAL AIRPO</City>
						<State>WA</State>
						<PostCode>6105</PostCode>
						<FCLEquipmentNeeded>WUP</FCLEquipmentNeeded>
						<LCLEquipmentNeeded>PSL</LCLEquipmentNeeded>
						<AIREquipmentNeeded>PSL</AIREquipmentNeeded>
						<RelatedPortCode>
							<Code>AUPER</Code>
						</RelatedPortCode>
						<OrgAddressCapabilityCollection>
							<OrgAddressCapability Action=""MERGE"">
								<AddressType>OFC</AddressType>
								<IsMainAddress>false</IsMainAddress>
							</OrgAddressCapability>
							<OrgAddressCapability Action=""MERGE"">
								<AddressType>PAD</AddressType>
								<IsMainAddress>false</IsMainAddress>
							</OrgAddressCapability>
						</OrgAddressCapabilityCollection>
					</OrgAddress>
					<OrgAddress Action=""MERGE"">
						<Code>AUADL - 208BHENLEYBEACHRO</Code>
						<Language>EN</Language>
						<Address1>208B HENLEY BEACH ROAD</Address1>
						<City>TORRENSVILLE</City>
						<State>SA</State>
						<PostCode>5031</PostCode>
						<FCLEquipmentNeeded>WUP</FCLEquipmentNeeded>
						<LCLEquipmentNeeded>PSL</LCLEquipmentNeeded>
						<AIREquipmentNeeded>PSL</AIREquipmentNeeded>
						<RelatedPortCode>
							<Code>AUADL</Code>
						</RelatedPortCode>
						<OrgAddressCapabilityCollection>
							<OrgAddressCapability Action=""MERGE"">
								<AddressType>OFC</AddressType>
								<IsMainAddress>false</IsMainAddress>
							</OrgAddressCapability>
							<OrgAddressCapability Action=""MERGE"">
								<AddressType>PAD</AddressType>
								<IsMainAddress>false</IsMainAddress>
							</OrgAddressCapability>
						</OrgAddressCapabilityCollection>
					</OrgAddress>
					<OrgAddress Action=""MERGE"">
						<Code>AUDRW - UNIT13002EXPORTDR</Code>
						<Language>EN</Language>
						<Address1>UNIT 1/3002 EXPORT DRIVE</Address1>
						<Address2>DARWIN BUSINESS PARK</Address2>
						<City>BERRIMAH</City>
						<State>NT</State>
						<PostCode>0828</PostCode>
						<FCLEquipmentNeeded>WUP</FCLEquipmentNeeded>
						<LCLEquipmentNeeded>PSL</LCLEquipmentNeeded>
						<AIREquipmentNeeded>PSL</AIREquipmentNeeded>
						<RelatedPortCode>
							<Code>AUDRW</Code>
						</RelatedPortCode>
						<OrgAddressCapabilityCollection>
							<OrgAddressCapability Action=""MERGE"">
								<AddressType>OFC</AddressType>
								<IsMainAddress>false</IsMainAddress>
							</OrgAddressCapability>
							<OrgAddressCapability Action=""MERGE"">
								<AddressType>PAD</AddressType>
								<IsMainAddress>false</IsMainAddress>
							</OrgAddressCapability>
						</OrgAddressCapabilityCollection>
					</OrgAddress>
					<OrgAddress Action=""MERGE"">
						<Code>AUMEL - 3133BEVERAGEDRIVE</Code>
						<Language>EN</Language>
						<Address1>31-33 BEVERAGE DRIVE</Address1>
						<City>TULLAMARINE</City>
						<State>VIC</State>
						<PostCode>3043</PostCode>
						<Phone>+61383360800</Phone>
						<Fax>+61393304422</Fax>
						<FCLEquipmentNeeded>WUP</FCLEquipmentNeeded>
						<LCLEquipmentNeeded>PSL</LCLEquipmentNeeded>
						<AIREquipmentNeeded>PSL</AIREquipmentNeeded>
						<RelatedPortCode>
							<Code>AUMEL</Code>
						</RelatedPortCode>
						<OrgAddressCapabilityCollection>
							<OrgAddressCapability Action=""MERGE"">
								<AddressType>OFC</AddressType>
								<IsMainAddress>true</IsMainAddress>
							</OrgAddressCapability>
						</OrgAddressCapabilityCollection>
					</OrgAddress>
					<OrgAddress Action=""MERGE"">
						<Code>AUMEL - POBOX502</Code>
						<Language>EN</Language>
						<Address1>PO BOX 502</Address1>
						<Address2/>
						<City>TULLAMARINE</City>
						<State>VIC</State>
						<PostCode>3043</PostCode>
						<FCLEquipmentNeeded>WUP</FCLEquipmentNeeded>
						<LCLEquipmentNeeded>PSL</LCLEquipmentNeeded>
						<AIREquipmentNeeded>PSL</AIREquipmentNeeded>
						<RelatedPortCode>
							<Code>AUMEL</Code>
						</RelatedPortCode>
						<OrgAddressCapabilityCollection>
							<OrgAddressCapability Action=""MERGE"">
								<AddressType>PST</AddressType>
								<IsMainAddress>true</IsMainAddress>
							</OrgAddressCapability>
						</OrgAddressCapabilityCollection>
					</OrgAddress>
				</OrgAddressCollection>
				<OrgCusCodeCollection>
					<OrgCusCode Action=""MERGE"">
						<CustomsRegNo>50791830427</CustomsRegNo>
						<CodeType>ABN</CodeType>
						<CodeCountry>
							<Code>AU</Code>
						</CodeCountry>
					</OrgCusCode>
					<OrgCusCode Action=""MERGE"">
						<CustomsRegNo>ADMCUS</CustomsRegNo>
						<CodeType>LSC</CodeType>
						<CodeCountry>
							<Code>AU</Code>
						</CodeCountry>
					</OrgCusCode>
				</OrgCusCodeCollection>
			</OrgHeader>
		</Organization>
	</Body>
</ReferenceData>";
		#endregion

		public void TestImportXMLOfOrgHeaderWithARTDetails()
		{
			var encoding = new UTF8Encoding();
			using (var stream = new MemoryStream(encoding.GetBytes(OrgHeaderWithARTDetailsXML)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 1 inserts, 0 updates, 0 deletes
OrgMiscServ - 1 inserts, 0 updates, 0 deletes
OrgContact - 3 inserts, 0 updates, 0 deletes
OrgAddress - 2 inserts, 0 updates, 0 deletes
OrgAddressCapability - 2 inserts, 0 updates, 0 deletes
OrgCompanyData - 3 inserts, 0 updates, 0 deletes
OrgARTerms - 3 inserts, 0 updates, 0 deletes
OrgRateTariffLevel - 3 inserts, 0 updates, 0 deletes
OrgRelatedParty - 1 inserts, 0 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Log Text", expectedLog, manager.GetLogs());
			}
		}

		#region OrgHeaderWithARTDetailsXML

		const string OrgHeaderWithARTDetailsXML = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ReferenceData xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <Header xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.cargowise.com/Schemas/Universal"">
    <OwnerCode>EDICUS</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Organization>
      <OrgHeader Action=""MERGE"">
        <Code>ATSENT</Code>
        <IsActive>true</IsActive>
        <FullName>ATS ENTERPRISES</FullName>
        <IsConsignee>true</IsConsignee>
        <IsConsignor>true</IsConsignor>
        <IsTransportClient>false</IsTransportClient>
        <IsWarehouseClient>false</IsWarehouseClient>
        <IsForwarder>false</IsForwarder>
        <IsShippingProvider>false</IsShippingProvider>
        <IsAirWholesaler>false</IsAirWholesaler>
        <IsSeaWholesaler>false</IsSeaWholesaler>
        <IsRailProvider>false</IsRailProvider>
        <IsLineHaulProvider>false</IsLineHaulProvider>
        <IsMiscFreightServices>false</IsMiscFreightServices>
        <IsAirCTO>false</IsAirCTO>
        <IsAirLine>false</IsAirLine>
        <IsBroker>false</IsBroker>
        <IsContainerPark>false</IsContainerPark>
        <IsLocalTransport>false</IsLocalTransport>
        <IsPackDepot>false</IsPackDepot>
        <IsSeaCTO>false</IsSeaCTO>
        <IsShippingLine>false</IsShippingLine>
        <IsUnpackDepot>false</IsUnpackDepot>
        <IsRailHead>false</IsRailHead>
        <IsRoadFreightDepot>false</IsRoadFreightDepot>
        <IsShippingConsortium>false</IsShippingConsortium>
        <IsFumigationContractor>false</IsFumigationContractor>
        <IsNationalAccount>false</IsNationalAccount>
        <IsSalesLead>false</IsSalesLead>
        <IsMailOut>false</IsMailOut>
        <IsFaxUpdate>false</IsFaxUpdate>
        <IsEmailUpdate>false</IsEmailUpdate>
        <IsNewsLetter>false</IsNewsLetter>
        <IsCompetitor>false</IsCompetitor>
        <IsTempAccount>false</IsTempAccount>
        <IsPersonalEffectsAccount>false</IsPersonalEffectsAccount>
        <IsUserFlag1>false</IsUserFlag1>
        <IsUserFlag2>false</IsUserFlag2>
        <IsUserFlag3>false</IsUserFlag3>
        <IsUserFlag4>false</IsUserFlag4>
        <IsUserFlag5>false</IsUserFlag5>
        <IsUserFlag6>false</IsUserFlag6>
        <IsUserFlag7>false</IsUserFlag7>
        <IsUserFlag8>false</IsUserFlag8>
        <Prospect>true</Prospect>
        <PartialBusiness>false</PartialBusiness>
        <Language>EN</Language>
        <IsGlobalAccount>false</IsGlobalAccount>
        <ScreeningStatus>UNK</ScreeningStatus>
        <OrgMiscServ Action=""MERGE"">
          <IMEftCustomsFromImport>false</IMEftCustomsFromImport>
          <IMEftQuarantineFromImport>false</IMEftQuarantineFromImport>
          <IMEftHoldUntilPayAuthorised>false</IMEftHoldUntilPayAuthorised>
          <IMEstDaysDeliveryAir>0</IMEstDaysDeliveryAir>
          <IMEstDaysDeliveryFCL>0</IMEstDaysDeliveryFCL>
          <IMEstDaysDeliveryLCL>0</IMEstDaysDeliveryLCL>
          <IMMaxEFTAmount>0.0000</IMMaxEFTAmount>
          <IMMinEFTAmount>0.0000</IMMinEFTAmount>
          <IMIsGSTDeferred>false</IMIsGSTDeferred>
          <IMOriginalSeaBills>0</IMOriginalSeaBills>
          <IMCopySeaBills>0</IMCopySeaBills>
          <IMSendImportDocsTo>IMP</IMSendImportDocsTo>
          <IMAirDepotFreeDays>0</IMAirDepotFreeDays>
          <IMSeaDepotFreeDays>0</IMSeaDepotFreeDays>
          <IMImporterOwnsPartNumbers>false</IMImporterOwnsPartNumbers>
          <IMOrderStatusCodePairList></IMOrderStatusCodePairList>
          <IMOrderLineStatusCodePairList></IMOrderLineStatusCodePairList>
          <IMDefaultINCOTerm>FOB</IMDefaultINCOTerm>
          <IMAutoImpJobRefered>false</IMAutoImpJobRefered>
          <IMUseExpiryDate>false</IMUseExpiryDate>
          <IMUsePackingDate>false</IMUsePackingDate>
          <IMImporterRequiresOrderNumbersOnDocs>false</IMImporterRequiresOrderNumbersOnDocs>
          <IMJobRequireOrderTrackLink>false</IMJobRequireOrderTrackLink>
          <LandedCostMarginPercent1>0.00</LandedCostMarginPercent1>
          <LandedCostMarginPercent2>0.00</LandedCostMarginPercent2>
          <LandedCostMarginPercent3>0.00</LandedCostMarginPercent3>
          <EXJobRequireOrderTrackLink>false</EXJobRequireOrderTrackLink>
          <EXExporterRequiresOrderNumbersOnDocs>false</EXExporterRequiresOrderNumbersOnDocs>
          <EXDefaultIncoTerm>FOB</EXDefaultIncoTerm>
          <EXAllowedToPrintOriginalBL>false</EXAllowedToPrintOriginalBL>
          <FWAgentBelongsToGroup>false</FWAgentBelongsToGroup>
          <FWHandlesAir>true</FWHandlesAir>
          <FWHandlesSea>true</FWHandlesSea>
          <FWRequestForCreditAllowed>false</FWRequestForCreditAllowed>
          <FWBillCollectFeesOnSingleInvoice>true</FWBillCollectFeesOnSingleInvoice>
          <FWDealDirectlyWithUltimates>false</FWDealDirectlyWithUltimates>
          <CMCompetitorActivity>FRT</CMCompetitorActivity>
          <CMClientSize>0</CMClientSize>
          <CMDoesExports>false</CMDoesExports>
          <CMDoesImports>false</CMDoesImports>
          <CMUseTradeLaneFigures>true</CMUseTradeLaneFigures>
          <CMTotalClientRevenue>0.0000</CMTotalClientRevenue>
          <CMPercentage>0.000</CMPercentage>
          <CMAcheivableClientRevenue>0.0000</CMAcheivableClientRevenue>
          <CMEstimatedProfit>0.0000</CMEstimatedProfit>
          <CMWarehouseRevenue>0.0000</CMWarehouseRevenue>
          <CMConsultingRevenue>0.0000</CMConsultingRevenue>
          <CMOverallClientRelation>0</CMOverallClientRelation>
          <CMClientsDesireToRemain>0</CMClientsDesireToRemain>
          <CMEaseClientCanBePoached>0</CMEaseClientCanBePoached>
          <CMAmountOfElectronicIntegration>0</CMAmountOfElectronicIntegration>
          <CMAmountOfBusinessWon>0</CMAmountOfBusinessWon>
          <CMIsHouseAccount>false</CMIsHouseAccount>
          <CIEstimatedStaffThisLocation>0</CIEstimatedStaffThisLocation>
          <CIEstimatedStaffThisCountry>0</CIEstimatedStaffThisCountry>
          <CITurnover>0.0000</CITurnover>
          <CIProfit>0.0000</CIProfit>
          <CICapitalEmployed>0.0000</CICapitalEmployed>
          <CICompetitiveRanking>0</CICompetitiveRanking>
          <CustomDecimal1>0.000</CustomDecimal1>
          <CustomDecimal2>0.000</CustomDecimal2>
          <CustomDecimal3>0.000</CustomDecimal3>
          <CustomFlag1>false</CustomFlag1>
          <CustomFlag2>false</CustomFlag2>
          <CustomFlag3>false</CustomFlag3>
          <IMAutoPopulateOwnerRefWithOrderNums>DEF</IMAutoPopulateOwnerRefWithOrderNums>
          <IMDefaultWarehousePickOption>AUT</IMDefaultWarehousePickOption>
          <IMInvoiceDetailReportSort>CCO</IMInvoiceDetailReportSort>
          <FWDirectAMSReporter>false</FWDirectAMSReporter>
          <WhsOverrideSystemPickingRules>false</WhsOverrideSystemPickingRules>
          <WhsPickFromDefaultFIFO>0</WhsPickFromDefaultFIFO>
          <WhsPickFromFullPallets>0</WhsPickFromFullPallets>
          <WhsPickFromConsolidatedFullPallets>0</WhsPickFromConsolidatedFullPallets>
          <WhsPickFromPickFaces>0</WhsPickFromPickFaces>
          <WhsPickFromPalletOverflow>0</WhsPickFromPalletOverflow>
          <WhsPickFromBrokenPallets>0</WhsPickFromBrokenPallets>
          <CustomFlag4>false</CustomFlag4>
          <IMAttrib1IsKey>false</IMAttrib1IsKey>
          <IMAttrib2IsKey>false</IMAttrib2IsKey>
          <IMAttrib3IsKey>false</IMAttrib3IsKey>
          <WhsPickSortArrivalDate>0</WhsPickSortArrivalDate>
          <WhsPickSortExpiryDate>0</WhsPickSortExpiryDate>
          <WhsPickSortPackingDate>0</WhsPickSortPackingDate>
          <WhsPickSortPickFace>0</WhsPickSortPickFace>
          <WhsPickSortLocationRow>0</WhsPickSortLocationRow>
          <WhsPickSortLocationColumn>0</WhsPickSortLocationColumn>
          <WhsPickSortLocationLevel>0</WhsPickSortLocationLevel>
          <WhsExpiryNotificationFromDefaults>true</WhsExpiryNotificationFromDefaults>
          <WhsExpiryNotificationPeriod>0</WhsExpiryNotificationPeriod>
          <IMDefaultToNewOrdersToNextOrderNum>false</IMDefaultToNewOrdersToNextOrderNum>
          <IMInvoiceDetailReportSort2>DEF</IMInvoiceDetailReportSort2>
          <IMInvoiceDetailReportSort3>DEF</IMInvoiceDetailReportSort3>
          <EXMergeCustomsInvoiceLinesBy>NON</EXMergeCustomsInvoiceLinesBy>
          <CMNoOfEmployees>0</CMNoOfEmployees>
          <WhsOverrideSystemPutawayRules>false</WhsOverrideSystemPutawayRules>
          <WhsPutawayToLocation>0</WhsPutawayToLocation>
          <WhsPutawayToPickFace>0</WhsPutawayToPickFace>
          <WhsPutawayToProductArea>0</WhsPutawayToProductArea>
          <WhsPutawayToClientArea>0</WhsPutawayToClientArea>
          <WhsPutawaySortLocationColumn>0</WhsPutawaySortLocationColumn>
          <WhsPutawaySortLocationLevel>0</WhsPutawaySortLocationLevel>
          <WhsPutawaySortLocationRow>0</WhsPutawaySortLocationRow>
          <WhsPutawaySameProductTogether>false</WhsPutawaySameProductTogether>
          <WhsEDIChangeMessageCancelOrderLinesNotIncludedInMessage>true</WhsEDIChangeMessageCancelOrderLinesNotIncludedInMessage>
          <WhsGenerateBackOrdersOnShortfalls>false</WhsGenerateBackOrdersOnShortfalls>
          <WhsAutoPckEdiOrders>false</WhsAutoPckEdiOrders>
          <WhsOrderFulfillmentRule>NON</WhsOrderFulfillmentRule>
          <WhsPackingSlipOrderBy>DEF</WhsPackingSlipOrderBy>
          <CMDistanceCalculationProvider>DEF</CMDistanceCalculationProvider>
          <IMPartAttrib1IsMandatory>false</IMPartAttrib1IsMandatory>
          <IMPartAttrib2IsMandatory>false</IMPartAttrib2IsMandatory>
          <IMPartAttrib3IsMandatory>false</IMPartAttrib3IsMandatory>
          <CMPaidUpCapital>0.0000</CMPaidUpCapital>
          <WhsDefaultWarehousePickMode>ASP</WhsDefaultWarehousePickMode>
          <WhsTransportPayer>DEF</WhsTransportPayer>
        </OrgMiscServ>
        <OrgContactCollection>
          <OrgContact Action=""MERGE"">
            <IsActive>true</IsActive>
            <ContactName>FLORISE STEINDL</ContactName>
            <NotifyMode>PRN</NotifyMode>
            <WebAccessEnabled>false</WebAccessEnabled>
          </OrgContact>
          <OrgContact Action=""MERGE"">
            <IsActive>true</IsActive>
            <ContactName>PETER PETTIONA</ContactName>
            <NotifyMode>PRN</NotifyMode>
            <WebAccessEnabled>false</WebAccessEnabled>
          </OrgContact>
          <OrgContact Action=""MERGE"">
            <IsActive>true</IsActive>
            <ContactName>PETER STEINDL</ContactName>
            <NotifyMode>PRN</NotifyMode>
            <WebAccessEnabled>false</WebAccessEnabled>
          </OrgContact>
        </OrgContactCollection>
        <OrgAddressCollection>
          <OrgAddress Action=""MERGE"">
            <IsActive>true</IsActive>
            <Code>Pick Up Address</Code>
            <Address1>34 DORSAL DRIVE</Address1>
            <Address2>BIRKDALE                          QLD</Address2>
            <City>QLD QLD</City>
            <State>QLD</State>
            <PostCode>4159</PostCode>
            <DockLeveler>false</DockLeveler>
            <ForkLift>false</ForkLift>
            <PalletJack>false</PalletJack>
            <Latitude>0.00000</Latitude>
            <Longitude>0.00000</Longitude>
            <OrgAddressCapabilityCollection>
              <OrgAddressCapability Action=""MERGE"">
                <AddressType>OFC</AddressType>
                <IsMainAddress>true</IsMainAddress>
              </OrgAddressCapability>
            </OrgAddressCapabilityCollection>
            <RelatedPortCode TableName=""RefUNLOCO"">
              <Code>AUBNE</Code>
            </RelatedPortCode>
          </OrgAddress>
          <OrgAddress Action=""MERGE"">
            <IsActive>true</IsActive>
            <Code>PST: 34 DORSAL DRIVE</Code>
            <Language>EN</Language>
            <Address1>34 DORSAL DRIVE</Address1>
            <Address2>BIRKDALE, QLD</Address2>
            <PostCode>4159</PostCode>
            <DockLeveler>false</DockLeveler>
            <ForkLift>false</ForkLift>
            <PalletJack>false</PalletJack>
            <Latitude>0.00000</Latitude>
            <Longitude>0.00000</Longitude>
            <OrgAddressCapabilityCollection>
              <OrgAddressCapability Action=""MERGE"">
                <AddressType>PST</AddressType>
                <IsMainAddress>false</IsMainAddress>
              </OrgAddressCapability>
            </OrgAddressCapabilityCollection>
          </OrgAddress>
        </OrgAddressCollection>
        <OrgCompanyDataCollection>
          <OrgCompanyData Action=""MERGE"">
            <IsDebtor>true</IsDebtor>
            <IsCreditor>false</IsCreditor>
            <APCategory>STD</APCategory>
            <APCreditLimit>0.0000</APCreditLimit>
            <APPayInvoiceAfterPostingDefault>false</APPayInvoiceAfterPostingDefault>
            <APPaymentTermDays>0</APPaymentTermDays>
            <APPaymentTerms>COD</APPaymentTerms>
            <APTaxApplicable>false</APTaxApplicable>
            <APWHTApplicable>false</APWHTApplicable>
            <ARAutoUpdateRates>true</ARAutoUpdateRates>
            <ARCategory>STD</ARCategory>
            <ARCombinedStatementInvoice>false</ARCombinedStatementInvoice>
            <ARCreditLimit>5000.0000</ARCreditLimit>
            <ARSeaInvoiceTermDays>COD</ARSeaInvoiceTermDays>
            <ARSeaInvoiceTerms>0</ARSeaInvoiceTerms>
            <ARAirInvoiceTermDays>COD</ARAirInvoiceTermDays>
            <ARAirInvoiceTerms>0</ARAirInvoiceTerms>
            <ARInvoiceTerms>COD</ARInvoiceTerms>
            <ARInvoiceTermDays>0</ARInvoiceTermDays>
            <ARTreatDisbursementsAsStandardValue>0.0000</ARTreatDisbursementsAsStandardValue>
            <ARDisbursementInvoiceTerms>COD</ARDisbursementInvoiceTerms>
            <ARDisbursementInvoiceTermDays>0</ARDisbursementInvoiceTermDays>
            <ARDontShowTaxOnDocs>false</ARDontShowTaxOnDocs>
            <ARForeignCurrStatement>false</ARForeignCurrStatement>
            <ARBuyersConsolInvoicingStyle>DEF</ARBuyersConsolInvoicingStyle>
            <ARSellersConsolInvoicingStyle>DEF</ARSellersConsolInvoicingStyle>
            <AROnCreditHold>false</AROnCreditHold>
            <ARReceiptInvoiceAfterPostingDefault>false</ARReceiptInvoiceAfterPostingDefault>
            <ARTaxApplicable>false</ARTaxApplicable>
            <ARWHTApplicable>false</ARWHTApplicable>
            <ARIncludeInwardsWhsConsolidatedInvoice>true</ARIncludeInwardsWhsConsolidatedInvoice>
            <ARIncludeOutwardsWhsConsolidatedInvoice>true</ARIncludeOutwardsWhsConsolidatedInvoice>
            <ARWhsStorageCalcMethod>MAX</ARWhsStorageCalcMethod>
            <CRIsShipsAgencyPrincipal>false</CRIsShipsAgencyPrincipal>
            <ARUseSystemDefaultUplifts>false</ARUseSystemDefaultUplifts>
            <ARExportAirCollectUplift>0.000</ARExportAirCollectUplift>
            <ARExportSeaCollectUplift>0.000</ARExportSeaCollectUplift>
            <ARImportAirCollectUplift>0.000</ARImportAirCollectUplift>
            <ARImportSeaCollectUplift>0.000</ARImportSeaCollectUplift>
            <ARUseSystemDefaultUpliftsMinimums>false</ARUseSystemDefaultUpliftsMinimums>
            <ARExportAirCollectUpliftMinimum>0.0000</ARExportAirCollectUpliftMinimum>
            <ARExportSeaCollectUpliftMinimum>0.0000</ARExportSeaCollectUpliftMinimum>
            <ARImportAirCollectUpliftMinimum>0.0000</ARImportAirCollectUpliftMinimum>
            <ARImportSeaCollectUpliftMinimum>0.0000</ARImportSeaCollectUpliftMinimum>
            <IMUsedBondedWhs>false</IMUsedBondedWhs>
            <APQualityAssured>false</APQualityAssured>
            <ARQualityAssured>false</ARQualityAssured>
            <ARWarehouseRatingPeriod>DEF</ARWarehouseRatingPeriod>
            <WhsClientFreeStorageDays>0</WhsClientFreeStorageDays>
            <WhsPeriodicBillingDay>0</WhsPeriodicBillingDay>
            <WhsOverrideFreeStorage>false</WhsOverrideFreeStorage>
            <APCostsSelfBilled>false</APCostsSelfBilled>
            <APPrintContractorForm>false</APPrintContractorForm>
            <ARAllowMultiCurrencyPayment>false</ARAllowMultiCurrencyPayment>
            <ARCreditApproved>true</ARCreditApproved>
            <ARUseSettlementGroupCreditLimit>false</ARUseSettlementGroupCreditLimit>
            <EXBillAgentChargesDirect>false</EXBillAgentChargesDirect>
            <IMBillAgentChargesDirect>false</IMBillAgentChargesDirect>
            <OrgARTermsCollection>
              <OrgARTerms Action=""MERGE"">
                <InvoiceClass>ALL</InvoiceClass>
                <InvoiceTerm>COD</InvoiceTerm>
                <InvoiceDays>0</InvoiceDays>
              </OrgARTerms>
            </OrgARTermsCollection>
            <GlbCompany>
              <Code>DEM</Code>
            </GlbCompany>
          </OrgCompanyData>
          <OrgCompanyData Action=""MERGE"">
            <IsDebtor>true</IsDebtor>
            <IsCreditor>false</IsCreditor>
            <APCategory>STD</APCategory>
            <APCreditLimit>0.0000</APCreditLimit>
            <APPayInvoiceAfterPostingDefault>false</APPayInvoiceAfterPostingDefault>
            <APPaymentTermDays>0</APPaymentTermDays>
            <APPaymentTerms>COD</APPaymentTerms>
            <APTaxApplicable>false</APTaxApplicable>
            <APWHTApplicable>false</APWHTApplicable>
            <ARAutoUpdateRates>true</ARAutoUpdateRates>
            <ARCategory>STD</ARCategory>
            <ARCombinedStatementInvoice>false</ARCombinedStatementInvoice>
            <ARCreditLimit>5000.0000</ARCreditLimit>
            <ARSeaInvoiceTermDays>COD</ARSeaInvoiceTermDays>
            <ARSeaInvoiceTerms>0</ARSeaInvoiceTerms>
            <ARAirInvoiceTermDays>COD</ARAirInvoiceTermDays>
            <ARAirInvoiceTerms>0</ARAirInvoiceTerms>
            <ARInvoiceTerms>COD</ARInvoiceTerms>
            <ARInvoiceTermDays>0</ARInvoiceTermDays>
            <ARTreatDisbursementsAsStandardValue>0.0000</ARTreatDisbursementsAsStandardValue>
            <ARDisbursementInvoiceTerms>COD</ARDisbursementInvoiceTerms>
            <ARDisbursementInvoiceTermDays>0</ARDisbursementInvoiceTermDays>
            <ARDontShowTaxOnDocs>false</ARDontShowTaxOnDocs>
            <ARForeignCurrStatement>false</ARForeignCurrStatement>
            <ARBuyersConsolInvoicingStyle>DEF</ARBuyersConsolInvoicingStyle>
            <ARSellersConsolInvoicingStyle>DEF</ARSellersConsolInvoicingStyle>
            <AROnCreditHold>false</AROnCreditHold>
            <ARReceiptInvoiceAfterPostingDefault>false</ARReceiptInvoiceAfterPostingDefault>
            <ARTaxApplicable>false</ARTaxApplicable>
            <ARWHTApplicable>false</ARWHTApplicable>
            <ARIncludeInwardsWhsConsolidatedInvoice>true</ARIncludeInwardsWhsConsolidatedInvoice>
            <ARIncludeOutwardsWhsConsolidatedInvoice>true</ARIncludeOutwardsWhsConsolidatedInvoice>
            <ARWhsStorageCalcMethod>MAX</ARWhsStorageCalcMethod>
            <CRIsShipsAgencyPrincipal>false</CRIsShipsAgencyPrincipal>
            <ARUseSystemDefaultUplifts>false</ARUseSystemDefaultUplifts>
            <ARExportAirCollectUplift>0.000</ARExportAirCollectUplift>
            <ARExportSeaCollectUplift>0.000</ARExportSeaCollectUplift>
            <ARImportAirCollectUplift>0.000</ARImportAirCollectUplift>
            <ARImportSeaCollectUplift>0.000</ARImportSeaCollectUplift>
            <ARUseSystemDefaultUpliftsMinimums>false</ARUseSystemDefaultUpliftsMinimums>
            <ARExportAirCollectUpliftMinimum>0.0000</ARExportAirCollectUpliftMinimum>
            <ARExportSeaCollectUpliftMinimum>0.0000</ARExportSeaCollectUpliftMinimum>
            <ARImportAirCollectUpliftMinimum>0.0000</ARImportAirCollectUpliftMinimum>
            <ARImportSeaCollectUpliftMinimum>0.0000</ARImportSeaCollectUpliftMinimum>
            <IMUsedBondedWhs>false</IMUsedBondedWhs>
            <APQualityAssured>false</APQualityAssured>
            <ARQualityAssured>false</ARQualityAssured>
            <ARWarehouseRatingPeriod>DEF</ARWarehouseRatingPeriod>
            <WhsClientFreeStorageDays>0</WhsClientFreeStorageDays>
            <WhsPeriodicBillingDay>0</WhsPeriodicBillingDay>
            <WhsOverrideFreeStorage>false</WhsOverrideFreeStorage>
            <APCostsSelfBilled>false</APCostsSelfBilled>
            <APPrintContractorForm>false</APPrintContractorForm>
            <ARAllowMultiCurrencyPayment>false</ARAllowMultiCurrencyPayment>
            <ARCreditApproved>true</ARCreditApproved>
            <ARUseSettlementGroupCreditLimit>false</ARUseSettlementGroupCreditLimit>
            <EXBillAgentChargesDirect>false</EXBillAgentChargesDirect>
            <IMBillAgentChargesDirect>false</IMBillAgentChargesDirect>
            <OrgARTermsCollection>
              <OrgARTerms Action=""MERGE"">
                <InvoiceClass>ALL</InvoiceClass>
                <InvoiceTerm>COD</InvoiceTerm>
                <InvoiceDays>0</InvoiceDays>
              </OrgARTerms>
            </OrgARTermsCollection>
            <GlbCompany>
              <Code>EDI</Code>
            </GlbCompany>
          </OrgCompanyData>
          <OrgCompanyData Action=""MERGE"">
            <IsDebtor>true</IsDebtor>
            <IsCreditor>false</IsCreditor>
            <APCategory>STD</APCategory>
            <APCreditLimit>0.0000</APCreditLimit>
            <APPayInvoiceAfterPostingDefault>false</APPayInvoiceAfterPostingDefault>
            <APPaymentTermDays>0</APPaymentTermDays>
            <APPaymentTerms>COD</APPaymentTerms>
            <APTaxApplicable>false</APTaxApplicable>
            <APWHTApplicable>false</APWHTApplicable>
            <ARAutoUpdateRates>true</ARAutoUpdateRates>
            <ARCategory>STD</ARCategory>
            <ARCombinedStatementInvoice>false</ARCombinedStatementInvoice>
            <ARCreditLimit>5000.0000</ARCreditLimit>
            <ARSeaInvoiceTermDays>COD</ARSeaInvoiceTermDays>
            <ARSeaInvoiceTerms>0</ARSeaInvoiceTerms>
            <ARAirInvoiceTermDays>COD</ARAirInvoiceTermDays>
            <ARAirInvoiceTerms>0</ARAirInvoiceTerms>
            <ARInvoiceTerms>COD</ARInvoiceTerms>
            <ARInvoiceTermDays>0</ARInvoiceTermDays>
            <ARTreatDisbursementsAsStandardValue>0.0000</ARTreatDisbursementsAsStandardValue>
            <ARDisbursementInvoiceTerms>COD</ARDisbursementInvoiceTerms>
            <ARDisbursementInvoiceTermDays>0</ARDisbursementInvoiceTermDays>
            <ARDontShowTaxOnDocs>false</ARDontShowTaxOnDocs>
            <ARForeignCurrStatement>false</ARForeignCurrStatement>
            <ARBuyersConsolInvoicingStyle>DEF</ARBuyersConsolInvoicingStyle>
            <ARSellersConsolInvoicingStyle>DEF</ARSellersConsolInvoicingStyle>
            <AROnCreditHold>false</AROnCreditHold>
            <ARReceiptInvoiceAfterPostingDefault>false</ARReceiptInvoiceAfterPostingDefault>
            <ARTaxApplicable>false</ARTaxApplicable>
            <ARWHTApplicable>false</ARWHTApplicable>
            <ARIncludeInwardsWhsConsolidatedInvoice>true</ARIncludeInwardsWhsConsolidatedInvoice>
            <ARIncludeOutwardsWhsConsolidatedInvoice>true</ARIncludeOutwardsWhsConsolidatedInvoice>
            <ARWhsStorageCalcMethod>MAX</ARWhsStorageCalcMethod>
            <CRIsShipsAgencyPrincipal>false</CRIsShipsAgencyPrincipal>
            <ARUseSystemDefaultUplifts>false</ARUseSystemDefaultUplifts>
            <ARExportAirCollectUplift>0.000</ARExportAirCollectUplift>
            <ARExportSeaCollectUplift>0.000</ARExportSeaCollectUplift>
            <ARImportAirCollectUplift>0.000</ARImportAirCollectUplift>
            <ARImportSeaCollectUplift>0.000</ARImportSeaCollectUplift>
            <ARUseSystemDefaultUpliftsMinimums>false</ARUseSystemDefaultUpliftsMinimums>
            <ARExportAirCollectUpliftMinimum>0.0000</ARExportAirCollectUpliftMinimum>
            <ARExportSeaCollectUpliftMinimum>0.0000</ARExportSeaCollectUpliftMinimum>
            <ARImportAirCollectUpliftMinimum>0.0000</ARImportAirCollectUpliftMinimum>
            <ARImportSeaCollectUpliftMinimum>0.0000</ARImportSeaCollectUpliftMinimum>
            <IMUsedBondedWhs>false</IMUsedBondedWhs>
            <APQualityAssured>false</APQualityAssured>
            <ARQualityAssured>false</ARQualityAssured>
            <ARWarehouseRatingPeriod>DEF</ARWarehouseRatingPeriod>
            <WhsClientFreeStorageDays>0</WhsClientFreeStorageDays>
            <WhsPeriodicBillingDay>0</WhsPeriodicBillingDay>
            <WhsOverrideFreeStorage>false</WhsOverrideFreeStorage>
            <APCostsSelfBilled>false</APCostsSelfBilled>
            <APPrintContractorForm>false</APPrintContractorForm>
            <ARAllowMultiCurrencyPayment>false</ARAllowMultiCurrencyPayment>
            <ARCreditApproved>true</ARCreditApproved>
            <ARUseSettlementGroupCreditLimit>false</ARUseSettlementGroupCreditLimit>
            <EXBillAgentChargesDirect>false</EXBillAgentChargesDirect>
            <IMBillAgentChargesDirect>false</IMBillAgentChargesDirect>
            <OrgARTermsCollection>
              <OrgARTerms Action=""MERGE"">
                <InvoiceClass>ALL</InvoiceClass>
                <InvoiceTerm>COD</InvoiceTerm>
                <InvoiceDays>0</InvoiceDays>
              </OrgARTerms>
            </OrgARTermsCollection>
            <GlbCompany>
              <Code>SIN</Code>
            </GlbCompany>
          </OrgCompanyData>
        </OrgCompanyDataCollection>
        <OrgRateTariffLevelCollection>
          <OrgRateTariffLevel Action=""MERGE"">
            <TariffType>DEF</TariffType>
            <TariffLevel>0</TariffLevel>
            <Mode>ALL</Mode>
            <Direction>ALL</Direction>
            <ExpiryDate>2024-09-03T00:00:00</ExpiryDate>
            <StartDate>2024-09-02T00:00:00</StartDate>
            <GlbCompany>
              <Code>SIN</Code>
            </GlbCompany>
          </OrgRateTariffLevel>
          <OrgRateTariffLevel Action=""MERGE"">
            <TariffType>DEF</TariffType>
            <TariffLevel>0</TariffLevel>
            <Mode>ALL</Mode>
            <Direction>ALL</Direction>
            <ExpiryDate>2024-09-03T00:00:00</ExpiryDate>
            <StartDate>2024-09-02T00:00:00</StartDate>
            <GlbCompany>
              <Code>EDI</Code>
            </GlbCompany>
          </OrgRateTariffLevel>
          <OrgRateTariffLevel Action=""MERGE"">
            <TariffType>DEF</TariffType>
            <TariffLevel>0</TariffLevel>
            <Mode>ALL</Mode>
            <Direction>ALL</Direction>
            <ExpiryDate>2024-09-03T00:00:00</ExpiryDate>
            <StartDate>2024-09-02T00:00:00</StartDate>
            <GlbCompany>
              <Code>DEM</Code>
            </GlbCompany>
          </OrgRateTariffLevel>
        </OrgRateTariffLevelCollection>
        <OrgRelatedPartyCollection>
          <OrgRelatedParty Action=""MERGE"">
            <PartyType>ICT</PartyType>
            <FreightMode>11</FreightMode>
            <Service>22</Service>
            <FreightDirection>PIC</FreightDirection>
            <RelatedParty TableName=""OrgHeader"">
              <Code>ATSENT</Code>
            </RelatedParty>
            <GlbCompany>
              <Code>SIN</Code>
            </GlbCompany>
            <OrgAddress>
              <OrgHeader>6034C9C6-A8D0-4D07-B85D-14F73C1A8FB9</OrgHeader>
              <Code>OFC: 111 Demo St</Code>
            </OrgAddress>
          </OrgRelatedParty>
        </OrgRelatedPartyCollection>
        <ClosestPort TableName=""RefUNLOCO"">
          <Code>AUBNE</Code>
        </ClosestPort>
      </OrgHeader>
    </Organization>
  </Body>
</ReferenceData>";

		#endregion

		public void TestImportXMLOfOrgHeaderWithEmptyServiceLevelOfInvoice()
		{
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(OrgHeaderWithEmptyServiceLeveOfInvoiceXML)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);
				var insertLog = manager.GetLogs();

				AssertMultilineASCIIEquals("Insert Log Text", @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 1 inserts, 0 updates, 0 deletes
OrgMiscServ - 1 inserts, 0 updates, 0 deletes
OrgAddress - 1 inserts, 0 updates, 0 deletes
OrgAddressCapability - 1 inserts, 0 updates, 0 deletes
OrgCompanyData - 1 inserts, 0 updates, 0 deletes
OrgARTerms - 1 inserts, 0 updates, 0 deletes
OrgInvoiceRollupOrGroup - 1 inserts, 0 updates, 0 deletes
OrgInvoiceType - 1 inserts, 0 updates, 0 deletes
OrgCountryData - 1 inserts, 0 updates, 0 deletes
OrgRelatedParty - 3 inserts, 0 updates, 0 deletes".Trim(), insertLog);
			}

			var organisation = new BusinessObjectFactory().LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, "TEST123"));
			AssertEquals(organisation.CompanyData.InvoiceTypes[0].PI_RS_NKServiceLevel, ZString.Empty);
		}

		#region OrgHeaderWithEmptyServiceLeveOfInvoiceXML

		const string OrgHeaderWithEmptyServiceLeveOfInvoiceXML = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
    <Header>
        <OwnerCode>ATSENT</OwnerCode>
        <EnableCodeMapping>true</EnableCodeMapping>
    </Header>
    <Body>
        <Organization version=""2.0"">
            <OrgHeader Action=""MERGE"">
                <PK>00cf95b0-cfb0-44d3-9c7f-cfb442511d7d</PK>
                <Code>TEST123</Code>
                <FullName>TEST123</FullName>
                <Language>EN</Language>
                <SystemLastEditTimeUtc>2018-12-13T03:09:00</SystemLastEditTimeUtc>
                <SystemCreateTimeUtc>2018-12-13T03:09:00</SystemCreateTimeUtc>
                <ScreeningStatus>NOT</ScreeningStatus>
                <IsActive>true</IsActive>
                <IsConsignee>false</IsConsignee>
                <IsConsignor>false</IsConsignor>
                <IsTransportClient>false</IsTransportClient>
                <IsWarehouseClient>false</IsWarehouseClient>
                <IsForwarder>false</IsForwarder>
                <IsShippingProvider>false</IsShippingProvider>
                <IsAirWholesaler>false</IsAirWholesaler>
                <IsSeaWholesaler>false</IsSeaWholesaler>
                <IsRailProvider>false</IsRailProvider>
                <IsLineHaulProvider>false</IsLineHaulProvider>
                <IsMiscFreightServices>false</IsMiscFreightServices>
                <IsAirCTO>false</IsAirCTO>
                <IsAirLine>false</IsAirLine>
                <IsBroker>false</IsBroker>
                <IsLocalTransport>false</IsLocalTransport>
                <IsPackDepot>false</IsPackDepot>
                <IsSeaCTO>false</IsSeaCTO>
                <IsShippingLine>false</IsShippingLine>
                <IsUnpackDepot>false</IsUnpackDepot>
                <IsRailHead>false</IsRailHead>
                <IsRoadFreightDepot>false</IsRoadFreightDepot>
                <IsShippingConsortium>false</IsShippingConsortium>
                <IsFumigationContractor>false</IsFumigationContractor>
                <IsGlobalAccount>false</IsGlobalAccount>
                <IsNationalAccount>false</IsNationalAccount>
                <IsSalesLead>false</IsSalesLead>
                <IsCompetitor>false</IsCompetitor>
                <IsTempAccount>false</IsTempAccount>
                <IsPersonalEffectsAccount>false</IsPersonalEffectsAccount>
                <IsUserFlag1>false</IsUserFlag1>
                <IsUserFlag2>false</IsUserFlag2>
                <IsUserFlag3>false</IsUserFlag3>
                <IsUserFlag4>false</IsUserFlag4>
                <IsUserFlag5>false</IsUserFlag5>
                <IsUserFlag6>false</IsUserFlag6>
                <IsUserFlag7>false</IsUserFlag7>
                <IsUserFlag8>false</IsUserFlag8>
                <IsUserFlag9>false</IsUserFlag9>
                <IsUserFlag10>false</IsUserFlag10>
                <IsUserFlag11>false</IsUserFlag11>
                <IsUserFlag12>false</IsUserFlag12>
                <IsUserFlag13>false</IsUserFlag13>
                <IsUserFlag14>false</IsUserFlag14>
                <IsDistributionCentre>false</IsDistributionCentre>
                <IsUserFlag15>false</IsUserFlag15>
                <IsUserFlag16>false</IsUserFlag16>
                <IsUserFlag17>false</IsUserFlag17>
                <IsUserFlag18>false</IsUserFlag18>
                <IsUserFlag19>false</IsUserFlag19>
                <IsUserFlag20>false</IsUserFlag20>
                <IsUserFlag21>false</IsUserFlag21>
                <IsUserFlag22>false</IsUserFlag22>
                <IsUserFlag23>false</IsUserFlag23>
                <IsUserFlag24>false</IsUserFlag24>
                <IsContainerYard>false</IsContainerYard>
                <IsControllingCustomer>false</IsControllingCustomer>
                <IsControllingAgent>false</IsControllingAgent>
                <Category>BUS</Category>
                <OrgMiscServ Action=""MERGE"">
                    <PK>ea6350ae-c9ca-47b6-ba60-cdfff708e8ed</PK>
                    <Airline3CharCode></Airline3CharCode>
                    <IMEstDaysDeliveryAir>0</IMEstDaysDeliveryAir>
                    <IMEstDaysDeliveryFCL>0</IMEstDaysDeliveryFCL>
                    <IMEstDaysDeliveryLCL>0</IMEstDaysDeliveryLCL>
                    <IMMaxEFTAmount>0.0000</IMMaxEFTAmount>
                    <IMMinEFTAmount>0.0000</IMMinEFTAmount>
                    <IMEFTBankAccount></IMEFTBankAccount>
                    <IMEFTBankBSB></IMEFTBankBSB>
                    <IMMergeCustomsInvoiceLinesBy>DEF</IMMergeCustomsInvoiceLinesBy>
                    <IMOrderLineAttrib1></IMOrderLineAttrib1>
                    <IMOrderLineAttrib2></IMOrderLineAttrib2>
                    <IMOrderLineAttrib3></IMOrderLineAttrib3>
                    <IMOriginalSeaBills>10</IMOriginalSeaBills>
                    <IMCopySeaBills>3</IMCopySeaBills>
                    <IMSendImportDocsTo>IMP</IMSendImportDocsTo>
                    <IMSendSeaImportDocsTo>IMP</IMSendSeaImportDocsTo>
                    <IMImporterCategory>STD</IMImporterCategory>
                    <IMAirDepotFreeDays>1</IMAirDepotFreeDays>
                    <IMSeaDepotFreeDays>3</IMSeaDepotFreeDays>
                    <IMOrderStatusCodePairList></IMOrderStatusCodePairList>
                    <IMOrderLineStatusCodePairList></IMOrderLineStatusCodePairList>
                    <IMFCLEquipmentNeeded></IMFCLEquipmentNeeded>
                    <IMLCLEquipmentNeeded></IMLCLEquipmentNeeded>
                    <IMAirEquipmentNeeded></IMAirEquipmentNeeded>
                    <IMDefaultINCOTerm></IMDefaultINCOTerm>
                    <IMAutoPopulateOwnerRefWithOrderNums>DEF</IMAutoPopulateOwnerRefWithOrderNums>
                    <IMPartAttrib1Type></IMPartAttrib1Type>
                    <IMPartAttrib1Name></IMPartAttrib1Name>
                    <IMPartAttrib2Type></IMPartAttrib2Type>
                    <IMPartAttrib2Name></IMPartAttrib2Name>
                    <IMPartAttrib3Type></IMPartAttrib3Type>
                    <IMPartAttrib3Name></IMPartAttrib3Name>
                    <IMDocumentAddressPreference></IMDocumentAddressPreference>
                    <IMDefaultWarehousePickOption>AUT</IMDefaultWarehousePickOption>
                    <IMInvoiceDetailReportSort>DEF</IMInvoiceDetailReportSort>
                    <LandedCostMarginPercent1>0.00</LandedCostMarginPercent1>
                    <LandedCostMarginPercent2>0.00</LandedCostMarginPercent2>
                    <LandedCostMarginPercent3>0.00</LandedCostMarginPercent3>
                    <LastArchiveDate></LastArchiveDate>
                    <EXExporterCategory>STD</EXExporterCategory>
                    <EXDefaultIncoTerm></EXDefaultIncoTerm>
                    <EXFCLEquipmentNeeded></EXFCLEquipmentNeeded>
                    <EXLCLEquipmentNeeded></EXLCLEquipmentNeeded>
                    <EXAirEquipmentNeeded></EXAirEquipmentNeeded>
                    <EXDocumentAddressPreference></EXDocumentAddressPreference>
                    <EXDefaultDGContactPhoneUsed></EXDefaultDGContactPhoneUsed>
                    <EXDefaultInvoicePriceFromProductLastCost></EXDefaultInvoicePriceFromProductLastCost>
                    <FWAgentCategory>STD</FWAgentCategory>
                    <FWHandlesSeaForPortOrCountry></FWHandlesSeaForPortOrCountry>
                    <FWHandlesAirForPortOrCountry></FWHandlesAirForPortOrCountry>
                    <FWIATACode></FWIATACode>
                    <FWIATAAccountNumber></FWIATAAccountNumber>
                    <CRCarrierCategory></CRCarrierCategory>
                    <SVServicesCategory></SVServicesCategory>
                    <CMSalesCategory></CMSalesCategory>
                    <CMCompetitorActivity>FRT</CMCompetitorActivity>
                    <CMLastCallDate></CMLastCallDate>
                    <CMClientSize></CMClientSize>
                    <CMGrowthOutlook></CMGrowthOutlook>
                    <CMFollowUpDate></CMFollowUpDate>
                    <CMEstimatedDateToClose></CMEstimatedDateToClose>
                    <CMTotalClientRevenue>0.0000</CMTotalClientRevenue>
                    <CMPercentage>0.000</CMPercentage>
                    <CMAcheivableClientRevenue>0.0000</CMAcheivableClientRevenue>
                    <CMEstimatedProfit>0.0000</CMEstimatedProfit>
                    <CMWarehouseRevenue>0.0000</CMWarehouseRevenue>
                    <CMConsultingRevenue>0.0000</CMConsultingRevenue>
                    <CMOverallClientRelation>0</CMOverallClientRelation>
                    <CMClientsDesireToRemain>0</CMClientsDesireToRemain>
                    <CMEaseClientCanBePoached>0</CMEaseClientCanBePoached>
                    <CMAmountOfElectronicIntegration>0</CMAmountOfElectronicIntegration>
                    <CMOverallEffectOfClientOnAirfreightCosts></CMOverallEffectOfClientOnAirfreightCosts>
                    <CMOverallEffectOfClientOnLCLCosts></CMOverallEffectOfClientOnLCLCosts>
                    <CMOverallEffectOfClientOnTEUCosts></CMOverallEffectOfClientOnTEUCosts>
                    <CMOverallEffectOfClientOnWarehousingCosts></CMOverallEffectOfClientOnWarehousingCosts>
                    <CMOverallEffectOfClientOnOtherCosts></CMOverallEffectOfClientOnOtherCosts>
                    <CMAmountOfBusinessWon>0</CMAmountOfBusinessWon>
                    <CMSalesTerritory></CMSalesTerritory>
                    <CMCommission></CMCommission>
                    <CMClientCommenced></CMClientCommenced>
                    <CICompetitorCategory></CICompetitorCategory>
                    <CIEstimatedStaffThisLocation>0</CIEstimatedStaffThisLocation>
                    <CITypeOfService></CITypeOfService>
                    <CIEstimatedStaffThisCountry>0</CIEstimatedStaffThisCountry>
                    <CISellingStyle></CISellingStyle>
                    <CITurnover>0.0000</CITurnover>
                    <CIProfit>0.0000</CIProfit>
                    <CICapitalEmployed>0.0000</CICapitalEmployed>
                    <CICompetitiveRanking>0</CICompetitiveRanking>
                    <CIStrength></CIStrength>
                    <CIWeaknesses></CIWeaknesses>
                    <CIOpportunities></CIOpportunities>
                    <CIThreats></CIThreats>
                    <WhsPickFromDefaultFIFO>0</WhsPickFromDefaultFIFO>
                    <WhsPickFromFullPallets>0</WhsPickFromFullPallets>
                    <WhsPickFromConsolidatedFullPallets>0</WhsPickFromConsolidatedFullPallets>
                    <WhsPickFromPickFaces>0</WhsPickFromPickFaces>
                    <WhsPickFromPalletOverflow>0</WhsPickFromPalletOverflow>
                    <WhsPickFromBrokenPallets>0</WhsPickFromBrokenPallets>
                    <CustomAttrib1></CustomAttrib1>
                    <CustomAttrib2></CustomAttrib2>
                    <CustomAttrib3></CustomAttrib3>
                    <CustomDate1></CustomDate1>
                    <CustomDate2></CustomDate2>
                    <CustomDate3></CustomDate3>
                    <CustomDecimal1>0.000</CustomDecimal1>
                    <CustomDecimal2>0.000</CustomDecimal2>
                    <CustomDecimal3>0.000</CustomDecimal3>
                    <WhsPickSortArrivalDate>0</WhsPickSortArrivalDate>
                    <WhsPickSortExpiryDate>0</WhsPickSortExpiryDate>
                    <WhsPickSortPackingDate>0</WhsPickSortPackingDate>
                    <WhsPickSortPickFace>0</WhsPickSortPickFace>
                    <WhsPickSortLocationRow>0</WhsPickSortLocationRow>
                    <WhsPickSortLocationColumn>0</WhsPickSortLocationColumn>
                    <WhsPickSortLocationLevel>0</WhsPickSortLocationLevel>
                    <WhsExpiryNotificationPeriod>0</WhsExpiryNotificationPeriod>
                    <WhsClientInvoiceFormat></WhsClientInvoiceFormat>
                    <IMLastOrderReference></IMLastOrderReference>
                    <WhsOrderNumberUniquenessStrategy></WhsOrderNumberUniquenessStrategy>
                    <WhsPutawayToLocation>0</WhsPutawayToLocation>
                    <WhsPutawayToPickFace>0</WhsPutawayToPickFace>
                    <WhsPutawayToProductArea>0</WhsPutawayToProductArea>
                    <WhsPutawayToClientArea>0</WhsPutawayToClientArea>
                    <WhsPutawaySortLocationColumn>0</WhsPutawaySortLocationColumn>
                    <WhsPutawaySortLocationLevel>0</WhsPutawaySortLocationLevel>
                    <EXGoodsDescription></EXGoodsDescription>
                    <WhsPutawaySortLocationRow>0</WhsPutawaySortLocationRow>
                    <EXHandlingInstuctions></EXHandlingInstuctions>
                    <CMNoOfEmployees>0</CMNoOfEmployees>
                    <IMInvoiceDetailReportSort2>DEF</IMInvoiceDetailReportSort2>
                    <IMInvoiceDetailReportSort3>DEF</IMInvoiceDetailReportSort3>
                    <CMClientPortalHomePage></CMClientPortalHomePage>
                    <EXMergeCustomsInvoiceLinesBy>NON</EXMergeCustomsInvoiceLinesBy>
                    <EXPreAllocPrefix></EXPreAllocPrefix>
                    <WhsOrderFulfillmentRule>NON</WhsOrderFulfillmentRule>
                    <WhsPackingSlipOrderBy>DEF</WhsPackingSlipOrderBy>
                    <CMDistanceCalculationProvider>DEF</CMDistanceCalculationProvider>
                    <CMDistanceCalculationVersion></CMDistanceCalculationVersion>
                    <CMDistanceCalculationMethod></CMDistanceCalculationMethod>
                    <WhsDefaultWarehousePickMode>ASP</WhsDefaultWarehousePickMode>
                    <WhsTransportPayer>DEF</WhsTransportPayer>
                    <CMPaidUpCapital>0.0000</CMPaidUpCapital>
                    <CMEstablishedDate></CMEstablishedDate>
                    <WhsDefaultWarehouseRollUp>false</WhsDefaultWarehouseRollUp>
                    <WhsABCAnalysisEnabled>false</WhsABCAnalysisEnabled>
                    <WhsABCAnalysisMethod>DEF</WhsABCAnalysisMethod>
                    <WhsABCAnalysisPeriod>DEF</WhsABCAnalysisPeriod>
                    <IsScanPackQtyAllowed>false</IsScanPackQtyAllowed>
                    <IsLabelPrintedOnClosePackage>true</IsLabelPrintedOnClosePackage>
                    <IMShowDutyOnWarehouseEntries>false</IMShowDutyOnWarehouseEntries>
                    <MinimumShelfLifeAccepted>0</MinimumShelfLifeAccepted>
                    <WhsPickFromFIFOBulkOnly>0</WhsPickFromFIFOBulkOnly>
                    <CMLastUnactionedCallDate></CMLastUnactionedCallDate>
                    <IMEftCustomsFromImport>false</IMEftCustomsFromImport>
                    <IMEftQuarantineFromImport>false</IMEftQuarantineFromImport>
                    <IMEftHoldUntilPayAuthorised>false</IMEftHoldUntilPayAuthorised>
                    <IMIsGSTDeferred>false</IMIsGSTDeferred>
                    <IMImporterOwnsPartNumbers>false</IMImporterOwnsPartNumbers>
                    <IMAutoImpJobRefered>false</IMAutoImpJobRefered>
                    <IMUseExpiryDate>false</IMUseExpiryDate>
                    <IMUsePackingDate>false</IMUsePackingDate>
                    <IMImporterRequiresOrderNumbersOnDocs>false</IMImporterRequiresOrderNumbersOnDocs>
                    <IMJobRequireOrderTrackLink>false</IMJobRequireOrderTrackLink>
                    <EXJobRequireOrderTrackLink>false</EXJobRequireOrderTrackLink>
                    <EXExporterRequiresOrderNumbersOnDocs>false</EXExporterRequiresOrderNumbersOnDocs>
                    <EXAllowedToPrintOriginalBL>false</EXAllowedToPrintOriginalBL>
                    <FWAgentBelongsToGroup>false</FWAgentBelongsToGroup>
                    <FWHandlesAir>false</FWHandlesAir>
                    <FWHandlesSea>false</FWHandlesSea>
                    <FWRequestForCreditAllowed>false</FWRequestForCreditAllowed>
                    <FWBillCollectFeesOnSingleInvoice>true</FWBillCollectFeesOnSingleInvoice>
                    <FWDealDirectlyWithUltimates>false</FWDealDirectlyWithUltimates>
                    <FWDirectAMSReporter>false</FWDirectAMSReporter>
                    <CMDoesExports>false</CMDoesExports>
                    <CMDoesImports>false</CMDoesImports>
                    <CMUseTradeLaneFigures>true</CMUseTradeLaneFigures>
                    <CMIsHouseAccount>false</CMIsHouseAccount>
                    <WhsOverrideSystemPickingRules>false</WhsOverrideSystemPickingRules>
                    <CustomFlag1>false</CustomFlag1>
                    <CustomFlag2>false</CustomFlag2>
                    <CustomFlag3>false</CustomFlag3>
                    <CustomFlag4>false</CustomFlag4>
                    <WhsExpiryNotificationFromDefaults>true</WhsExpiryNotificationFromDefaults>
                    <IMAttrib1IsKey>false</IMAttrib1IsKey>
                    <IMAttrib2IsKey>false</IMAttrib2IsKey>
                    <IMAttrib3IsKey>false</IMAttrib3IsKey>
                    <IMDefaultToNewOrdersToNextOrderNum>false</IMDefaultToNewOrdersToNextOrderNum>
                    <WhsOverrideSystemPutawayRules>false</WhsOverrideSystemPutawayRules>
                    <WhsPutawaySameProductTogether>false</WhsPutawaySameProductTogether>
                    <WhsEDIChangeMessageCancelOrderLinesNotIncludedInMessage>true</WhsEDIChangeMessageCancelOrderLinesNotIncludedInMessage>
                    <WhsGenerateBackOrdersOnShortfalls>false</WhsGenerateBackOrdersOnShortfalls>
                    <WhsAutoPckEdiOrders>false</WhsAutoPckEdiOrders>
                    <IMPartAttrib1IsMandatory>false</IMPartAttrib1IsMandatory>
                    <IMPartAttrib2IsMandatory>false</IMPartAttrib2IsMandatory>
                    <IMPartAttrib3IsMandatory>false</IMPartAttrib3IsMandatory>
                    <CRVoyageRecyclingPeriodInMonths>-1</CRVoyageRecyclingPeriodInMonths>
                    <IsAutoPackAllowed>true</IsAutoPackAllowed>
                    <WhsOrderDefaultPickPriority>0</WhsOrderDefaultPickPriority>
                    <IMOwnsProducts>false</IMOwnsProducts>
                    <EXOwnsProducts>false</EXOwnsProducts>
                    <ConsigneeAuthorityToLeave>DEF</ConsigneeAuthorityToLeave>
                    <ConsignorAuthorityToLeave>DEF</ConsignorAuthorityToLeave>
                    <IMAllowOrders>true</IMAllowOrders>
                    <CMPeriodOfActivity></CMPeriodOfActivity>
                    <CMIndustryVertical></CMIndustryVertical>
                    <WhsIsRecalculateOrderPricing>false</WhsIsRecalculateOrderPricing>
                    <CMAuthorityToLeave>DEF</CMAuthorityToLeave>
                    <TBAllowMixedAccountNumbersOnManifest>false</TBAllowMixedAccountNumbersOnManifest>
                    <IMAllowAttachedOrderXMLUpdate>false</IMAllowAttachedOrderXMLUpdate>
                    <EXValidationForUnauditedClassification></EXValidationForUnauditedClassification>
                    <IMValidationForUnauditedClassification></IMValidationForUnauditedClassification>
                    <IMBalanceInvoicePackage>false</IMBalanceInvoicePackage>
                    <WhsPackageToleranceEnabled>false</WhsPackageToleranceEnabled>
                    <WhsPackageWeightTolerancePercent>0.0</WhsPackageWeightTolerancePercent>
                    <WhsPickFromHighPriorityLocations>0</WhsPickFromHighPriorityLocations>
                    <WhsEnforceScanOfProductsWhenPackingTote>false</WhsEnforceScanOfProductsWhenPackingTote>
                    <ARGlobalCreditApproved>false</ARGlobalCreditApproved>
                    <ARGlobalCreditLimit>0.0000</ARGlobalCreditLimit>
                    <ARGlobalOnCreditHold>false</ARGlobalOnCreditHold>
                    <EXDefaultDGContact TableName=""OrgContact"" />
                    <CMMainImportCmdty TableName=""RefCommodityCode"" />
                    <CMMainExportCmdty TableName=""RefCommodityCode"" />
                    <WhsDefaultWarehouse TableName=""GlbBranch"" />
                    <WhsPackingSlip TableName=""StmTemplate"" />
                    <IMDefaultServiceLevel TableName=""RefServiceLevel"" />
                    <EXDefaultServiceLevel TableName=""RefServiceLevel"" />
                    <EXDefCurrency TableName=""RefCurrency"" />
                    <EXDefaultCntryOfOrigin TableName=""RefCountry"">
                        <Code>AD</Code>
                        <PK>4a039a4c-dcf4-472a-872d-ca545b12ac79</PK>
                    </EXDefaultCntryOfOrigin>
                    <FWDefCurrency TableName=""RefCurrency"">
                        <Code>EUR</Code>
                        <PK>117b4869-7383-42b0-a3fc-b44d79e30f70</PK>
                    </FWDefCurrency>
                    <CMPreferredPaymentCompany TableName=""GlbCompany"" />
                    <CartonGroup TableName=""WhsCartonGroup"" />
                    <OrgSecurityGroup TableName=""GlbGroup"" />
                    <ARGlobalCreditGroup TableName=""OrgHeader"" />
                    <ARGlobalCreditCurrency TableName=""RefCurrency"" />
                </OrgMiscServ>
                <OrgAddressCollection>
                    <OrgAddress Action=""MERGE"">
                        <PK>d53a9a48-d418-4c87-b9d9-1292cfa8d470</PK>
                        <Code>TEST123</Code>
                        <CompanyNameOverride></CompanyNameOverride>
                        <Address1>TEST123</Address1>
                        <Address2></Address2>
                        <State>ACT</State>
                        <PostCode>TEST123</PostCode>
                        <Phone></Phone>
                        <Fax></Fax>
                        <Mobile></Mobile>
                        <PickupFromTimeOnly></PickupFromTimeOnly>
                        <PickupToTimeOnly></PickupToTimeOnly>
                        <DeliverFromTimeOnly></DeliverFromTimeOnly>
                        <DeliverToTimeOnly></DeliverToTimeOnly>
                        <DoNotAttendFrom></DoNotAttendFrom>
                        <DoNotAttendTo></DoNotAttendTo>
                        <ContainerHandling></ContainerHandling>
                        <AccessPoint></AccessPoint>
                        <LabourRequired></LabourRequired>
                        <CommunicationRequired></CommunicationRequired>
                        <Dock_Height></Dock_Height>
                        <FCLEquipmentNeeded>WUP</FCLEquipmentNeeded>
                        <LCLEquipmentNeeded>ANY</LCLEquipmentNeeded>
                        <AIREquipmentNeeded>PSL</AIREquipmentNeeded>
                        <IsActive>true</IsActive>
                        <DockLeveler>false</DockLeveler>
                        <ForkLift>false</ForkLift>
                        <PalletJack>false</PalletJack>
                        <Email></Email>
                        <DeliveryRoute></DeliveryRoute>
                        <DeliveryRouteSequence>0</DeliveryRouteSequence>
                        <UseCumulativeFreeWaitingTime>false</UseCumulativeFreeWaitingTime>
                        <AddressMap></AddressMap>
                        <AuthorityToLeave>DEF</AuthorityToLeave>
                        <OtherWarehouseFacilities></OtherWarehouseFacilities>
                        <LoadingUnloadingConstraints></LoadingUnloadingConstraints>
                        <City>TEST123</City>
                        <GroupNumber>0</GroupNumber>
                        <AdditionalAddressInformation></AdditionalAddressInformation>
                        <VerifiesContainerGrossWeight>false</VerifiesContainerGrossWeight>
                        <Language>EN</Language>
                        <SystemCreateTimeUtc>2018-12-13T03:09:00</SystemCreateTimeUtc>
                        <SystemLastEditTimeUtc>2018-12-13T03:09:00</SystemLastEditTimeUtc>
                        <OrgAddressCapabilityCollection>
                            <OrgAddressCapability Action=""MERGE"">
                                <PK>b833dbe8-9183-410a-b8cf-cb3e2b0069d6</PK>
                                <AddressType>OFC</AddressType>
                                <IsMainAddress>true</IsMainAddress>
                            </OrgAddressCapability>
                        </OrgAddressCapabilityCollection>
                        <RelatedPortCode TableName=""RefUNLOCO"">
                            <Code>ADALV</Code>
                            <PK>955b070d-4b88-4d8b-beb4-60f9b0aff673</PK>
                        </RelatedPortCode>
                        <CountryCode TableName=""RefCountry"">
                            <Code>AD</Code>
                            <PK>4a039a4c-dcf4-472a-872d-ca545b12ac79</PK>
                        </CountryCode>
                    </OrgAddress>
                </OrgAddressCollection>
                <OrgCompanyDataCollection>
                    <OrgCompanyData Action=""MERGE"">
                        <PK>d4ff87e9-ae7d-41e2-9739-99eb5fc4b686</PK>
                        <APCategory></APCategory>
                        <APCreditLimit>0.0000</APCreditLimit>
                        <APPaymentTermDays>0</APPaymentTermDays>
                        <APPaymentTerms>COD</APPaymentTerms>
                        <APAirlineAccountNumber></APAirlineAccountNumber>
                        <APQualityAssuredCheckedDate></APQualityAssuredCheckedDate>
                        <ARQualityAssuredCheckedDate></ARQualityAssuredCheckedDate>
                        <ARCategory></ARCategory>
                        <ARConsolidatedAccountingCategory></ARConsolidatedAccountingCategory>
                        <ARCreditLimit>0.0000</ARCreditLimit>
                        <ARCreditRating></ARCreditRating>
                        <ARSeaInvoiceTermDays>COD</ARSeaInvoiceTermDays>
                        <ARSeaInvoiceTerms>0</ARSeaInvoiceTerms>
                        <ARAirInvoiceTermDays>COD</ARAirInvoiceTermDays>
                        <ARAirInvoiceTerms>0</ARAirInvoiceTerms>
                        <ARInvoiceTerms>COD</ARInvoiceTerms>
                        <ARInvoiceTermDays>0</ARInvoiceTermDays>
                        <ARTreatDisbursementsAsStandardValue>0.0000</ARTreatDisbursementsAsStandardValue>
                        <ARDisbursementInvoiceTerms>COD</ARDisbursementInvoiceTerms>
                        <ARDisbursementInvoiceTermDays>0</ARDisbursementInvoiceTermDays>
                        <ARBuyersConsolInvoicingStyle>DEF</ARBuyersConsolInvoicingStyle>
                        <ARSellersConsolInvoicingStyle>DEF</ARSellersConsolInvoicingStyle>
                        <ARAccountAndCreditReviewDue></ARAccountAndCreditReviewDue>
                        <ARPreviousChequeDrawer></ARPreviousChequeDrawer>
                        <ARPreviousChequeDrawerBank></ARPreviousChequeDrawerBank>
                        <ARPreviousChequeDrawerBankBranch></ARPreviousChequeDrawerBankBranch>
                        <AREftCustomsPaymentMethod></AREftCustomsPaymentMethod>
                        <ARWhsStorageCalcMethod>MAX</ARWhsStorageCalcMethod>
                        <ARWarehouseRatingPeriod>DEF</ARWarehouseRatingPeriod>
                        <WhsClientFreeStorageDays>0</WhsClientFreeStorageDays>
                        <APExternalCreditorCode></APExternalCreditorCode>
                        <ARExternalDebtorCode></ARExternalDebtorCode>
                        <WhsIncludeReleaseChargesOnShipment>true</WhsIncludeReleaseChargesOnShipment>
                        <RateSecurityGroup></RateSecurityGroup>
                        <APVATConfig>NON</APVATConfig>
                        <ARVATConfig>DEF</ARVATConfig>
                        <ARTemporaryCreditLimitIncrease>0.0000</ARTemporaryCreditLimitIncrease>
                        <ARTemporaryCreditLimitIncreaseExpiry></ARTemporaryCreditLimitIncreaseExpiry>
                        <APCreditAgreedPaymentMethod></APCreditAgreedPaymentMethod>
                        <ARCreditAgreedPaymentMethod></ARCreditAgreedPaymentMethod>
                        <IsDebtor>true</IsDebtor>
                        <IsCreditor>false</IsCreditor>
                        <APPayInvoiceAfterPostingDefault>false</APPayInvoiceAfterPostingDefault>
                        <APWHTApplicable>false</APWHTApplicable>
                        <APQualityAssured>false</APQualityAssured>
                        <ARQualityAssured>false</ARQualityAssured>
                        <ARAutoUpdateRates>true</ARAutoUpdateRates>
                        <ARCombinedStatementInvoice>false</ARCombinedStatementInvoice>
                        <ARDontShowTaxOnDocs>false</ARDontShowTaxOnDocs>
                        <ARForeignCurrStatement>false</ARForeignCurrStatement>
                        <AROnCreditHold>false</AROnCreditHold>
                        <ARReceiptInvoiceAfterPostingDefault>false</ARReceiptInvoiceAfterPostingDefault>
                        <ARWHTApplicable>false</ARWHTApplicable>
                        <ARIncludeInwardsWhsConsolidatedInvoice>true</ARIncludeInwardsWhsConsolidatedInvoice>
                        <ARIncludeOutwardsWhsConsolidatedInvoice>true</ARIncludeOutwardsWhsConsolidatedInvoice>
                        <CRIsShipsAgencyPrincipal>false</CRIsShipsAgencyPrincipal>
                        <IMUsedBondedWhs>false</IMUsedBondedWhs>
                        <WhsOverrideFreeStorage>false</WhsOverrideFreeStorage>
                        <APCostsSelfBilled>false</APCostsSelfBilled>
                        <APPrintContractorForm>false</APPrintContractorForm>
                        <ARAllowMultiCurrencyPayment>false</ARAllowMultiCurrencyPayment>
                        <ARCreditApproved>true</ARCreditApproved>
                        <ARUseSettlementGroupCreditLimit>false</ARUseSettlementGroupCreditLimit>
                        <EXBillAgentChargesDirect>false</EXBillAgentChargesDirect>
                        <IMBillAgentChargesDirect>false</IMBillAgentChargesDirect>
                        <WhsChargeStorageInAdvance>true</WhsChargeStorageInAdvance>
                        <ARCustomerSelfBillsRevenue>false</ARCustomerSelfBillsRevenue>
                        <ARVATSplitPaymentApplicable>false</ARVATSplitPaymentApplicable>
                        <ARClientNumber>00001011</ARClientNumber>
                        <APCreateVATComplianceDocumentOnPosting>NON</APCreateVATComplianceDocumentOnPosting>
                        <ARCreateVATComplianceDocumentOnPosting>NON</ARCreateVATComplianceDocumentOnPosting>
                        <IMProductValueDefaultOptions></IMProductValueDefaultOptions>
                        <OrgARTermsCollection>
                            <OrgARTerms Action=""MERGE"">
                                <PK>41c8a3ae-7a87-4629-b345-4f8447c1a69d</PK>
                                <InvoiceClass>ALL</InvoiceClass>
                                <InvoiceTerm>COD</InvoiceTerm>
                                <InvoiceDays>0</InvoiceDays>
                                <AgreedPaymentMethod></AgreedPaymentMethod>
                                <JobType>ALL</JobType>
                                <Direction>ALL</Direction>
                                <TransportMode>ALL</TransportMode>
                                <Department TableName=""GlbDepartment"" />
                                <Branch TableName=""GlbBranch"" />
                            </OrgARTerms>
                        </OrgARTermsCollection>
                        <OrgInvoiceRollupOrGroupCollection>
                            <OrgInvoiceRollupOrGroup Action=""MERGE"">
                                <PK>adfba720-e1bd-456b-831c-39a46b8bb8c5</PK>
                                <JobType>ALL</JobType>
                                <TransportMode>ALL</TransportMode>
                                <ServiceDirection>ALL</ServiceDirection>
                                <GroupOrSubTotal>DEF</GroupOrSubTotal>
                                <GroupOrSubtotalStyle>DEF</GroupOrSubtotalStyle>
                                <InvoiceLineDisplayOption>DEF</InvoiceLineDisplayOption>
                                <InvoicePostingStyle>DEF</InvoicePostingStyle>
                                <InvoicePostingCurrency TableName=""RefCurrency"" />
                            </OrgInvoiceRollupOrGroup>
                        </OrgInvoiceRollupOrGroupCollection>
                        <OrgInvoiceTypeCollection>
                            <OrgInvoiceType Action=""MERGE"">
                                <PK>d3000d8d-1fdc-4c65-888c-2f8fd2d58ca3</PK>
                                <Module>AGB</Module>
                                <Interval>MTH</Interval>
                                <StartDay>LMH</StartDay>
                                <SecondaryType>INV</SecondaryType>
                                <ServiceDirection>ALL</ServiceDirection>
                                <TransportMode>ALL</TransportMode>
                                <IsInclude>true</IsInclude>
                                <Type>INV</Type>
                                <ServiceLevel TableName=""RefServiceLevel"" />
                            </OrgInvoiceType>
                        </OrgInvoiceTypeCollection>
                        <APCreditorGroup TableName=""OrgCreditorGroup"" />
                        <APDefaultChargeCode TableName=""AccChargeCode"" />
                        <ARPayToAccount TableName=""AccBankAccount"" />
                        <ControllingBranch TableName=""GlbBranch"" />
                        <GlbCompany>
                            <Code>EDI</Code>
                            <PK>43fcbdf9-19c2-447e-bc36-162d1aaeb5c8</PK>
                        </GlbCompany>
                        <APDefltCurrency TableName=""RefCurrency"">
                            <Code>EUR</Code>
                            <PK>117b4869-7383-42b0-a3fc-b44d79e30f70</PK>
                        </APDefltCurrency>
                        <ARDDefltCurrency TableName=""RefCurrency"">
                            <Code>AUD</Code>
                            <PK>53feb23f-a7b0-4e0d-af5d-f6f6e5417399</PK>
                        </ARDDefltCurrency>
                    </OrgCompanyData>
                </OrgCompanyDataCollection>
                <OrgCountryDataCollection>
                    <OrgCountryData Action=""MERGE"">
                        <PK>ebbeb91f-944a-4a40-8264-0b08c1d0c61f</PK>
                        <EXApprovedOrMajorExporter>NO</EXApprovedOrMajorExporter>
                        <EXApprovalMethod></EXApprovalMethod>
                        <EXApprovalNumber></EXApprovalNumber>
                        <EXExportPermissionDetails></EXExportPermissionDetails>
                        <EXSiteInspectionDate></EXSiteInspectionDate>
                        <EXPermitNumber></EXPermitNumber>
                        <LastReviewedOn></LastReviewedOn>                        <ImportCustomsDefaultAddInfo>&lt;ImportCustomsDefaultAddInfo&gt;&lt;ACROSSHighValueProductAuditAction&gt;DEF&lt;/ACROSSHighValueProductAuditAction&gt;&lt;ACROSSLowValueProductAuditAction&gt;DEF&lt;/ACROSSLowValueProductAuditAction&gt;&lt;B3HighValueProductAuditAction&gt;DEF&lt;/B3HighValueProductAuditAction&gt;&lt;B3LowValueProductAuditAction&gt;DEF&lt;/B3LowValueProductAuditAction&gt;&lt;CFIAFeePaymentMethod&gt;DEF&lt;/CFIAFeePaymentMethod&gt;&lt;CONDelayIntervalTypeAutoSend&gt;DEF&lt;/CONDelayIntervalTypeAutoSend&gt;&lt;CONDelayIntervalTypeFailSafe&gt;DEF&lt;/CONDelayIntervalTypeFailSafe&gt;&lt;DeferredLowValueB3SendAction&gt;DEF&lt;/DeferredLowValueB3SendAction&gt;&lt;DeferredNormalB3SendAction&gt;DEF&lt;/DeferredNormalB3SendAction&gt;&lt;HVSDelayIntervalTypeAutoSend&gt;DEF&lt;/HVSDelayIntervalTypeAutoSend&gt;&lt;HVSDelayIntervalTypeFailSafe&gt;DEF&lt;/HVSDelayIntervalTypeFailSafe&gt;&lt;LVSDelayIntervalTypeAutoSend&gt;DEF&lt;/LVSDelayIntervalTypeAutoSend&gt;&lt;LVSDelayIntervalTypeFailSafe&gt;DEF&lt;/LVSDelayIntervalTypeFailSafe&gt;&lt;LVSInvoiceDetailCode&gt;DEF&lt;/LVSInvoiceDetailCode&gt;&lt;/ImportCustomsDefaultAddInfo&gt;</ImportCustomsDefaultAddInfo>
                        <ExportCustomsDefaultAddInfo></ExportCustomsDefaultAddInfo>
                        <ImportEntryPaymentPreference></ImportEntryPaymentPreference>
                        <ImportQuarantinePaymentPreference></ImportQuarantinePaymentPreference>
                        <SystemCreateTimeUtc>2018-12-13T03:09:00</SystemCreateTimeUtc>
                        <SystemLastEditTimeUtc>2018-12-13T03:09:00</SystemLastEditTimeUtc>
                        <EXApprovalExpiryDate></EXApprovalExpiryDate>
                        <MakePartsBothImportAndExport>false</MakePartsBothImportAndExport>
                        <EXE3Signed>false</EXE3Signed>
                        <ReviewedByUser TableName=""GlbStaff"" />
                        <ApprovedLocation TableName=""OrgAddress"" />
                        <ClientCountryRelation TableName=""RefCountry"">
                            <Code>CA</Code>
                            <PK>b1f02b9d-30a7-4bd3-b946-2ecc0bce2a81</PK>
                        </ClientCountryRelation>
                        <NotifyParty TableName=""OrgHeader"" />
                        <DefaultConsignee TableName=""OrgAddress"" />
                        <WarehouseAddress TableName=""OrgAddress"" />
                    </OrgCountryData>
                </OrgCountryDataCollection>
                <OrgRelatedPartyCollection>
                    <OrgRelatedParty Action=""MERGE"">
                        <PK>2dc3f960-644f-416f-b5f8-f773498fbb2c</PK>
                        <PartyType>CAB</PartyType>
                        <FreightDirection>DLV</FreightDirection>
                        <Service></Service>
                        <Location></Location>
                        <SystemCreateTimeUtc>2018-12-13T03:09:00</SystemCreateTimeUtc>
                        <SystemLastEditTimeUtc>2018-12-13T03:09:00</SystemLastEditTimeUtc>
                        <CSAStatus></CSAStatus>
                        <FreightTransportMode>SEA</FreightTransportMode>
                        <FreightContainerMode></FreightContainerMode>
                        <RelatedParty TableName=""OrgHeader"">
                            <Code>ATSENT</Code>
                        </RelatedParty>
                        <GlbCompany>
                            <Code>EDI</Code>
                            <PK>43fcbdf9-19c2-447e-bc36-162d1aaeb5c8</PK>
                        </GlbCompany>
                        <OrgAddress />
                    </OrgRelatedParty>
                    <OrgRelatedParty Action=""MERGE"">
                        <PK>3bf00c2f-040f-40b2-855e-1a5273219299</PK>
                        <PartyType>CAB</PartyType>
                        <FreightDirection>DLV</FreightDirection>
                        <Service></Service>
                        <Location></Location>
                        <SystemCreateTimeUtc>2018-12-13T03:09:00</SystemCreateTimeUtc>
                        <SystemLastEditTimeUtc>2018-12-13T03:09:00</SystemLastEditTimeUtc>
                        <CSAStatus></CSAStatus>
                        <FreightTransportMode>AIR</FreightTransportMode>
                        <FreightContainerMode></FreightContainerMode>
                        <RelatedParty TableName=""OrgHeader"">
                            <Code>ATSENT</Code>
                        </RelatedParty>
                        <GlbCompany>
                            <Code>EDI</Code>
                            <PK>43fcbdf9-19c2-447e-bc36-162d1aaeb5c8</PK>
                        </GlbCompany>
                        <OrgAddress />
                    </OrgRelatedParty>
                    <OrgRelatedParty Action=""MERGE"">
                        <PK>7f2fec0b-997a-4cfb-ae44-cc7451b08d1b</PK>
                        <PartyType>CAB</PartyType>
                        <FreightDirection>PIC</FreightDirection>
                        <Service></Service>
                        <Location></Location>
                        <SystemCreateTimeUtc>2018-12-13T03:09:00</SystemCreateTimeUtc>
                        <SystemLastEditTimeUtc>2018-12-13T03:09:00</SystemLastEditTimeUtc>
                        <CSAStatus></CSAStatus>
                        <FreightTransportMode>AIR</FreightTransportMode>
                        <FreightContainerMode></FreightContainerMode>
                        <RelatedParty TableName=""OrgHeader"">
                            <Code>ATSENT</Code>
                        </RelatedParty>
                        <GlbCompany>
                            <Code>EDI</Code>
                            <PK>43fcbdf9-19c2-447e-bc36-162d1aaeb5c8</PK>
                        </GlbCompany>
                        <OrgAddress />
                    </OrgRelatedParty>
                </OrgRelatedPartyCollection>
                <ClosestPort TableName=""RefUNLOCO"">
                    <Code>ADALV</Code>
                    <PK>955b070d-4b88-4d8b-beb4-60f9b0aff673</PK>
                </ClosestPort>
            </OrgHeader>
        </Organization>
    </Body>
</Native>";

		#endregion

		public void TestImportXMLOfOrgHeaderWithSimilarCodeToOneInDatabaseWorksAndUpdatesCorrectOne()
		{
			var orgHeaderWithConflictingCode = Factory.New<OrgHeader>();
			orgHeaderWithConflictingCode.OH_FullName = "SOME COMPANY";
			orgHeaderWithConflictingCode.OH_IsConsignor = true;
			orgHeaderWithConflictingCode.OH_IsShippingProvider = true;
			orgHeaderWithConflictingCode.OH_IsAirLine = true;

			var orgAddress1 = orgHeaderWithConflictingCode.MainAddress;
			orgAddress1.OA_Address1 = "777 VEGAS STREET";
			orgAddress1.OA_City = "LOS ANGELES";
			orgAddress1.OA_PostCode = "3333";
			orgAddress1.OA_RL_NKRelatedPortCode = "USLAX";
			orgAddress1.OA_State = "NSW";
			orgHeaderWithConflictingCode.OH_Code = "MAHO";

			var orgHeaderToUpdate = Factory.New<OrgHeader>();
			orgHeaderToUpdate.OH_FullName = "THE COMPANY THAT IS";
			orgHeaderToUpdate.OH_IsConsignor = true;
			orgHeaderToUpdate.OH_IsShippingProvider = true;
			orgHeaderToUpdate.OH_IsAirLine = true;
			var airline = RefAirline.LoadFromAirlinePrefix(Factory, "555");
			orgHeaderToUpdate.MiscServ.OM_RM_Airline = airline.PK;

			var orgAddress2 = orgHeaderToUpdate.MainAddress;
			orgAddress2.OA_Address1 = "873 HERE ST LALALAND";
			orgAddress2.OA_City = "AUCKLAND";
			orgAddress2.OA_PostCode = "2012";
			orgAddress2.OA_RL_NKRelatedPortCode = "NZAKL";
			orgAddress2.OA_State = "AUK";
			orgHeaderToUpdate.OH_Code = "AH";

			Factory.Save();

			AssertEquals("Precondition: orgHeaderWithConflictingCode.IsInDatabase", true, orgHeaderWithConflictingCode.IsInDatabase);
			AssertEquals("Precondition: orgHeaderToUpdate.IsInDatabase", true, orgHeaderToUpdate.IsInDatabase);
			AssertEquals("Precondition: orgHeaderToUpdate.MiscServ.Ariline", airline, orgHeaderToUpdate.MiscServ.Airline);

			var encoding = new UTF8Encoding();
			using (var stream = new MemoryStream(encoding.GetBytes(OrgHeaderXML)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 0 inserts, 0 updates, 0 deletes
OrgMiscServ - 0 inserts, 1 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Only OrgHeader.OrgMiscServ is updated", expectedLog, manager.GetLogs());

				var factory = new BusinessObjectFactory();
				var reloadedBO = factory.Load<OrgHeader>(orgHeaderToUpdate.PK);
				AssertEquals("reloadedBO.MiscServ.Airline.RM_EagleAddedAirlinePrefixOrAccountingCode", "124", reloadedBO.MiscServ.Airline.RM_EagleAddedAirlinePrefixOrAccountingCode);
			}
		}

		#region OrgHeaderXML

		const string OrgHeaderXML = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<ReferenceData xsi:schemaLocation=""http://www.cargowise.com/Schemas/Universal"" xmlns=""http://www.cargowise.com/Schemas/Universal"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
	<Header>
		<EnableCodeMapping>false</EnableCodeMapping>
	</Header>
	<Body>
		<Organization>
			<OrgHeader Action=""UPDATE"">
				<Code>AH</Code>
				<IsActive>true</IsActive>
				<IsAirLine>true</IsAirLine>
				<OrgMiscServ Action=""UPDATE"">
					<Airline TableName=""RefAirline"">
						<PK>29ADCB18-F42D-4456-B102-FC65A076F6C1</PK>
						<EagleAddedAirlinePrefixOrAccountingCode>124</EagleAddedAirlinePrefixOrAccountingCode>
					</Airline>
				</OrgMiscServ>
			</OrgHeader>
		</Organization>
	</Body>
</ReferenceData>";

		#endregion

		public void TestCanMergeAddressWithEmptyRelatedPortCode()
		{
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(XML_MergeAddressWithEmptyRelatedPortCode)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);
				var insertLog = manager.GetLogs();
				AssertMultilineASCIIEquals("Insert Log Text", @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 1 inserts, 0 updates, 0 deletes
RelatedPortCode - 1 inserts, 0 updates, 0 deletes
OrgAddress - 1 inserts, 0 updates, 0 deletes
				".Trim(), insertLog);
			}

			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(XML_MergeAddressWithEmptyRelatedPortCode)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);
				var updateLog = manager.GetLogs();
				AssertMultilineASCIIEquals("Update Log Text", @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 0 inserts, 0 updates, 0 deletes
RelatedPortCode - 0 inserts, 0 updates, 0 deletes
OrgAddress - 0 inserts, 0 updates, 0 deletes
				".Trim(), updateLog);
			}
		}

		public void TestMainAddressRelatedPortCodeAutoFilledWhenEmpty()
		{
			DataRegistry.Instance.CanUserEditOrganisationCode = true;

			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(XML_MergeAddressWithEmptyRelatedPortCodeMainAddress)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);
				var insertLog = manager.GetLogs();
				AssertMultilineASCIIEquals("Insert Log Text", @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 1 inserts, 0 updates, 0 deletes
RelatedPortCode - 1 inserts, 0 updates, 0 deletes
OrgAddress - 1 inserts, 0 updates, 0 deletes
OrgAddressCapability - 1 inserts, 0 updates, 0 deletes

				".Trim(), insertLog);
			}

			var organisation = new BusinessObjectFactory().LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "POLYCODE"));
			var address = organisation.Addresses.ToArray()[0] as OrgAddress;
			AssertEquals("Main address related port should be same as header if blank", organisation.OH_RL_NKClosestPort, address.GetBaseOA_RL_NKRelatedPortCode());
		}

		#region XML_Organization

		const string XML_UpdateOrganization = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ReferenceData xmlns=""http://www.cargowise.com/Schemas/Universal"">
	<Header xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.cargowise.com/Schemas/Universal"">
		<OwnerCode>GLOFORSYD</OwnerCode>
		<EnableCodeMapping>false</EnableCodeMapping>
	</Header>
	<Body>
		<Organization>
			<OrgHeader Action=""MERGE"">
				<Code>FOOBARTST</Code>
				<FullName>TEST KNU CO</FullName>
				<ClosestPort TableName=""RefUNLOCO"">
					<Code>AUSYD</Code>
				</ClosestPort>
			</OrgHeader>
		</Organization>
	</Body>
</ReferenceData>";

		const string XML_UpdateOrganizationCreateUpdateTime = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ReferenceData xmlns=""http://www.cargowise.com/Schemas/Universal"">
	<Header xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.cargowise.com/Schemas/Universal"">
		<OwnerCode>GLOFORSYD</OwnerCode>
		<EnableCodeMapping>false</EnableCodeMapping>
	</Header>
	<Body>
		<Organization>
			<OrgHeader Action=""MERGE"">
				<Code>FOOBARTST</Code>
				<SystemLastEditTimeUtc>2017-09-17T10:10:10</SystemLastEditTimeUtc>
				<SystemCreateTimeUtc>2017-09-17T10:10:10</SystemCreateTimeUtc>
			</OrgHeader>
		</Organization>
	</Body>
</ReferenceData>";

		#endregion
		[TestDate(2017, 09, 14, 1, 1, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestSystemEditTimeUTCFieldsAreUpdatedWithTheCurrentTimeNotUsingValuesSuppliedInTheXML()
		{
			DataRegistry.Instance.CanUserEditOrganisationCode = true;

			var organisation = new BusinessObjectFactory().New<OrgHeader>();
			organisation.OH_FullName = "SOME CO";
			var mainAddress = organisation.MainAddress;
			mainAddress.OA_Address1 = "123 TEST STREET";
			mainAddress.OA_City = "TESTVILLE";
			mainAddress.OA_PostCode = "2892";
			mainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			organisation.OH_RL_NKClosestPort = "AUSYD";
			organisation.OH_Code = "FOOBARTST";

			organisation.Factory.Save();

			AssertEquals(new ZDateTime(2017, 09, 14, 1, 1, 0), organisation.OH_SystemLastEditTimeUtc);
			AssertEquals(new ZDateTime(2017, 09, 14, 1, 1, 0), organisation.OH_SystemCreateTimeUtc);

			TestDateAttribute.Date = new DateTime(2017, 9, 15, 2, 2, 0);

			string actualLog = XML_UpdateOrganization.ImportNativeXmlReturningLog();

			CombineAssertions(delegate
			{
				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 0 inserts, 1 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Precondition: Log Text", expectedLog, actualLog);

				organisation = new BusinessObjectFactory().Load<OrgHeader>(organisation.PK);
				AssertEquals("Precondition: organisation.OH_FullName", "TEST KNU CO", organisation.OH_FullName);
				AssertEquals("Precondition: organisation.OH_Code", "FOOBARTST", organisation.OH_Code);
				AssertEquals("Precondition: organisation.OH_SystemLastEditTimeUtc", new ZDateTime(2017, 09, 15, 2, 2, 0), organisation.OH_SystemLastEditTimeUtc);
				AssertEquals("Precondition: organisation.OH_SystemCreateTimeUtc", new ZDateTime(2017, 09, 14, 1, 1, 0), organisation.OH_SystemCreateTimeUtc);
			});

			TestDateAttribute.Date = new DateTime(2017, 9, 16, 3, 3, 0);

			actualLog = XML_UpdateOrganizationCreateUpdateTime.ImportNativeXmlReturningLog();

			CombineAssertions(delegate
			{
				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 0 inserts, 0 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Log Text should indicate an update", expectedLog, actualLog);

				organisation = new BusinessObjectFactory().Load<OrgHeader>(organisation.PK);
				AssertEquals("organisation.OH_SystemLastEditTimeUtc should not have changed", new ZDateTime(2017, 9, 15, 2, 2, 0), organisation.OH_SystemLastEditTimeUtc);
				AssertEquals("Precondition: organisation.OH_SystemCreateTimeUtc", new ZDateTime(2017, 09, 14, 1, 1, 0), organisation.OH_SystemCreateTimeUtc);
			});
		}

		#region OrgHeaderWithNotesXml

		const string OrgHeaderWithNotesXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>EDICUS</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Organization version=""2.0"">
      <OrgHeader Action=""MERGE"">
        <PK>a9d44573-919d-472f-a385-ee81bfcb5ea9</PK>
        <Code>FROICEHBO</Code>
        <IsActive>true</IsActive>
        <FullName>FROSTY ICE CREAM</FullName>
        <IsConsignee>true</IsConsignee>
        <IsConsignor>false</IsConsignor>
        <IsTransportClient>false</IsTransportClient>
        <IsWarehouseClient>false</IsWarehouseClient>
        <IsForwarder>false</IsForwarder>
        <IsShippingProvider>false</IsShippingProvider>
        <IsAirWholesaler>false</IsAirWholesaler>
        <IsSeaWholesaler>false</IsSeaWholesaler>
        <IsRailProvider>false</IsRailProvider>
        <IsLineHaulProvider>false</IsLineHaulProvider>
        <IsMiscFreightServices>false</IsMiscFreightServices>
        <IsAirCTO>false</IsAirCTO>
        <IsAirLine>false</IsAirLine>
        <IsBroker>false</IsBroker>
        <IsContainerPark>false</IsContainerPark>
        <IsLocalTransport>true</IsLocalTransport>
        <IsPackDepot>false</IsPackDepot>
        <IsSeaCTO>false</IsSeaCTO>
        <IsShippingLine>false</IsShippingLine>
        <IsUnpackDepot>false</IsUnpackDepot>
        <IsRailHead>false</IsRailHead>
        <IsRoadFreightDepot>false</IsRoadFreightDepot>
        <IsShippingConsortium>false</IsShippingConsortium>
        <IsFumigationContractor>false</IsFumigationContractor>
        <IsNationalAccount>false</IsNationalAccount>
        <IsSalesLead>false</IsSalesLead>
        <IsUserFlag13>false</IsUserFlag13>
        <IsUserFlag12>false</IsUserFlag12>
        <IsUserFlag14>false</IsUserFlag14>
        <IsUserFlag11>false</IsUserFlag11>
        <IsCompetitor>false</IsCompetitor>
        <IsTempAccount>false</IsTempAccount>
        <IsPersonalEffectsAccount>false</IsPersonalEffectsAccount>
        <IsUserFlag1>false</IsUserFlag1>
        <IsUserFlag2>false</IsUserFlag2>
        <IsUserFlag3>false</IsUserFlag3>
        <IsUserFlag4>false</IsUserFlag4>
        <IsUserFlag5>false</IsUserFlag5>
        <IsUserFlag6>false</IsUserFlag6>
        <IsUserFlag7>false</IsUserFlag7>
        <IsUserFlag8>false</IsUserFlag8>
        <IsUserFlag9>false</IsUserFlag9>
        <IsUserFlag10>false</IsUserFlag10>
        <Language>EN</Language>
        <IsGlobalAccount>false</IsGlobalAccount>
        <ScreeningStatus>UNK</ScreeningStatus>
        <OrgMiscServ Action=""MERGE"">
          <PK>0a281adc-b184-4a24-851b-8bd88b5abe1c</PK>
          <Airline3CharCode></Airline3CharCode>
          <IMEftCustomsFromImport>false</IMEftCustomsFromImport>
          <IMEftQuarantineFromImport>false</IMEftQuarantineFromImport>
          <IMEftHoldUntilPayAuthorised>false</IMEftHoldUntilPayAuthorised>
          <IMEstDaysDeliveryAir>0</IMEstDaysDeliveryAir>
          <IMEstDaysDeliveryFCL>0</IMEstDaysDeliveryFCL>
          <IMEstDaysDeliveryLCL>0</IMEstDaysDeliveryLCL>
          <IMMaxEFTAmount>0.0000</IMMaxEFTAmount>
          <IMMinEFTAmount>0.0000</IMMinEFTAmount>
          <IMEFTBankAccount></IMEFTBankAccount>
          <IMEFTBankBSB></IMEFTBankBSB>
          <IMIsGSTDeferred>false</IMIsGSTDeferred>
          <IMMergeCustomsInvoiceLinesBy>NON</IMMergeCustomsInvoiceLinesBy>
          <IMOrderLineAttrib1></IMOrderLineAttrib1>
          <IMOrderLineAttrib2></IMOrderLineAttrib2>
          <IMOrderLineAttrib3></IMOrderLineAttrib3>
          <IMOriginalSeaBills>3</IMOriginalSeaBills>
          <IMCopySeaBills>3</IMCopySeaBills>
          <IMSendImportDocsTo>IMP</IMSendImportDocsTo>
          <IMSendSeaImportDocsTo>IMP</IMSendSeaImportDocsTo>
          <IMImporterCategory>STD</IMImporterCategory>
          <IMAirDepotFreeDays>1</IMAirDepotFreeDays>
          <IMSeaDepotFreeDays>3</IMSeaDepotFreeDays>
          <IMImporterOwnsPartNumbers>false</IMImporterOwnsPartNumbers>
          <IMOrderStatusCodePairList></IMOrderStatusCodePairList>
          <IMOrderLineStatusCodePairList></IMOrderLineStatusCodePairList>
          <IMFCLEquipmentNeeded></IMFCLEquipmentNeeded>
          <IMLCLEquipmentNeeded></IMLCLEquipmentNeeded>
          <IMAirEquipmentNeeded></IMAirEquipmentNeeded>
          <IMDefaultINCOTerm>FOB</IMDefaultINCOTerm>
          <IMAutoImpJobRefered>false</IMAutoImpJobRefered>
          <IMPartAttrib1Type></IMPartAttrib1Type>
          <IMPartAttrib1Name></IMPartAttrib1Name>
          <IMPartAttrib2Type></IMPartAttrib2Type>
          <IMPartAttrib2Name></IMPartAttrib2Name>
          <IMPartAttrib3Type></IMPartAttrib3Type>
          <IMPartAttrib3Name></IMPartAttrib3Name>
          <IMUseExpiryDate>false</IMUseExpiryDate>
          <IMUsePackingDate>false</IMUsePackingDate>
          <IMImporterRequiresOrderNumbersOnDocs>false</IMImporterRequiresOrderNumbersOnDocs>
          <IMJobRequireOrderTrackLink>false</IMJobRequireOrderTrackLink>
          <LandedCostMarginPercent1>0.00</LandedCostMarginPercent1>
          <LandedCostMarginPercent2>0.00</LandedCostMarginPercent2>
          <LandedCostMarginPercent3>0.00</LandedCostMarginPercent3>
          <LastArchiveDate></LastArchiveDate>
          <EXJobRequireOrderTrackLink>false</EXJobRequireOrderTrackLink>
          <EXExporterRequiresOrderNumbersOnDocs>false</EXExporterRequiresOrderNumbersOnDocs>
          <EXExporterCategory>STD</EXExporterCategory>
          <EXDefaultIncoTerm>FOB</EXDefaultIncoTerm>
          <EXFCLEquipmentNeeded></EXFCLEquipmentNeeded>
          <EXLCLEquipmentNeeded></EXLCLEquipmentNeeded>
          <EXAirEquipmentNeeded></EXAirEquipmentNeeded>
          <EXAllowedToPrintOriginalBL>false</EXAllowedToPrintOriginalBL>
          <FWAgentCategory>STD</FWAgentCategory>
          <FWAgentBelongsToGroup>false</FWAgentBelongsToGroup>
          <FWHandlesAir>false</FWHandlesAir>
          <FWHandlesSea>false</FWHandlesSea>
          <FWHandlesSeaForPortOrCountry></FWHandlesSeaForPortOrCountry>
          <FWHandlesAirForPortOrCountry></FWHandlesAirForPortOrCountry>
          <FWRequestForCreditAllowed>false</FWRequestForCreditAllowed>
          <FWBillCollectFeesOnSingleInvoice>true</FWBillCollectFeesOnSingleInvoice>
          <FWDealDirectlyWithUltimates>false</FWDealDirectlyWithUltimates>
          <FWIATACode></FWIATACode>
          <FWIATAAccountNumber></FWIATAAccountNumber>
          <CRCarrierCategory></CRCarrierCategory>
          <SVServicesCategory></SVServicesCategory>
          <CMSalesCategory></CMSalesCategory>
          <CMCompetitorActivity>FRT</CMCompetitorActivity>
          <CMLastCallDate></CMLastCallDate>
          <CMClientSize></CMClientSize>
          <CMGrowthOutlook></CMGrowthOutlook>
          <CMFollowUpDate></CMFollowUpDate>
          <CMEstimatedDateToClose></CMEstimatedDateToClose>
          <CMDoesExports>false</CMDoesExports>
          <CMDoesImports>false</CMDoesImports>
          <CMUseTradeLaneFigures>true</CMUseTradeLaneFigures>
          <CMTotalClientRevenue>0.0000</CMTotalClientRevenue>
          <CMPercentage>0.000</CMPercentage>
          <CMAcheivableClientRevenue>0.0000</CMAcheivableClientRevenue>
          <CMEstimatedProfit>0.0000</CMEstimatedProfit>
          <CMWarehouseRevenue>0.0000</CMWarehouseRevenue>
          <CMConsultingRevenue>0.0000</CMConsultingRevenue>
          <CMOverallClientRelation>0</CMOverallClientRelation>
          <CMClientsDesireToRemain>0</CMClientsDesireToRemain>
          <CMEaseClientCanBePoached>0</CMEaseClientCanBePoached>
          <CMAmountOfElectronicIntegration>0</CMAmountOfElectronicIntegration>
          <CMOverallEffectOfClientOnAirfreightCosts></CMOverallEffectOfClientOnAirfreightCosts>
          <CMOverallEffectOfClientOnLCLCosts></CMOverallEffectOfClientOnLCLCosts>
          <CMOverallEffectOfClientOnTEUCosts></CMOverallEffectOfClientOnTEUCosts>
          <CMOverallEffectOfClientOnWarehousingCosts></CMOverallEffectOfClientOnWarehousingCosts>
          <CMOverallEffectOfClientOnOtherCosts></CMOverallEffectOfClientOnOtherCosts>
          <CMAmountOfBusinessWon>0</CMAmountOfBusinessWon>
          <CMSalesTerritory></CMSalesTerritory>
          <CMIsHouseAccount>false</CMIsHouseAccount>
          <CMCommission></CMCommission>
          <CMClientCommenced></CMClientCommenced>
          <CICompetitorCategory></CICompetitorCategory>
          <CIEstimatedStaffThisLocation>0</CIEstimatedStaffThisLocation>
          <CITypeOfService></CITypeOfService>
          <CIEstimatedStaffThisCountry>0</CIEstimatedStaffThisCountry>
          <CISellingStyle></CISellingStyle>
          <CITurnover>0.0000</CITurnover>
          <CIProfit>0.0000</CIProfit>
          <CICapitalEmployed>0.0000</CICapitalEmployed>
          <CICompetitiveRanking>0</CICompetitiveRanking>
          <CIStrength></CIStrength>
          <CIWeaknesses></CIWeaknesses>
          <CIOpportunities></CIOpportunities>
          <CIThreats></CIThreats>
          <CustomAttrib1></CustomAttrib1>
          <CustomAttrib2></CustomAttrib2>
          <CustomAttrib3></CustomAttrib3>
          <CustomDate1></CustomDate1>
          <CustomDate2></CustomDate2>
          <CustomDate3></CustomDate3>
          <CustomDecimal1>0.000</CustomDecimal1>
          <CustomDecimal2>0.000</CustomDecimal2>
          <CustomDecimal3>0.000</CustomDecimal3>
          <CustomFlag1>false</CustomFlag1>
          <CustomFlag2>false</CustomFlag2>
          <CustomFlag3>false</CustomFlag3>
          <IMAutoPopulateOwnerRefWithOrderNums>DEF</IMAutoPopulateOwnerRefWithOrderNums>
          <IMDocumentAddressPreference></IMDocumentAddressPreference>
          <IMDefaultWarehousePickOption>AUT</IMDefaultWarehousePickOption>
          <IMInvoiceDetailReportSort>DEF</IMInvoiceDetailReportSort>
          <EXDocumentAddressPreference></EXDocumentAddressPreference>
          <EXDefaultDGContactPhoneUsed></EXDefaultDGContactPhoneUsed>
          <EXDefaultInvoicePriceFromProductLastCost></EXDefaultInvoicePriceFromProductLastCost>
          <FWDirectAMSReporter>false</FWDirectAMSReporter>
          <WhsOverrideSystemPickingRules>false</WhsOverrideSystemPickingRules>
          <WhsPickFromDefaultFIFO>0</WhsPickFromDefaultFIFO>
          <WhsPickFromFullPallets>0</WhsPickFromFullPallets>
          <WhsPickFromConsolidatedFullPallets>0</WhsPickFromConsolidatedFullPallets>
          <WhsPickFromPickFaces>0</WhsPickFromPickFaces>
          <WhsPickFromPalletOverflow>0</WhsPickFromPalletOverflow>
          <WhsPickFromBrokenPallets>0</WhsPickFromBrokenPallets>
          <CustomFlag4>false</CustomFlag4>
          <IMAttrib1IsKey>false</IMAttrib1IsKey>
          <IMAttrib2IsKey>false</IMAttrib2IsKey>
          <IMAttrib3IsKey>false</IMAttrib3IsKey>
          <WhsPickSortArrivalDate>0</WhsPickSortArrivalDate>
          <WhsPickSortExpiryDate>0</WhsPickSortExpiryDate>
          <WhsPickSortPackingDate>0</WhsPickSortPackingDate>
          <WhsPickSortPickFace>0</WhsPickSortPickFace>
          <WhsPickSortLocationRow>0</WhsPickSortLocationRow>
          <WhsPickSortLocationColumn>0</WhsPickSortLocationColumn>
          <WhsPickSortLocationLevel>0</WhsPickSortLocationLevel>
          <WhsExpiryNotificationFromDefaults>true</WhsExpiryNotificationFromDefaults>
          <WhsExpiryNotificationPeriod>0</WhsExpiryNotificationPeriod>
          <WhsClientInvoiceFormat></WhsClientInvoiceFormat>
          <IMLastOrderReference></IMLastOrderReference>
          <IMDefaultToNewOrdersToNextOrderNum>false</IMDefaultToNewOrdersToNextOrderNum>
          <IMInvoiceDetailReportSort2>DEF</IMInvoiceDetailReportSort2>
          <IMInvoiceDetailReportSort3>DEF</IMInvoiceDetailReportSort3>
          <EXGoodsDescription></EXGoodsDescription>
          <EXHandlingInstuctions></EXHandlingInstuctions>
          <EXMergeCustomsInvoiceLinesBy>NON</EXMergeCustomsInvoiceLinesBy>
          <EXPreAllocPrefix></EXPreAllocPrefix>
          <CMNoOfEmployees>0</CMNoOfEmployees>
          <CMClientPortalHomePage></CMClientPortalHomePage>
          <WhsOrderNumberUniquenessStrategy></WhsOrderNumberUniquenessStrategy>
          <WhsOverrideSystemPutawayRules>false</WhsOverrideSystemPutawayRules>
          <WhsPutawayToLocation>0</WhsPutawayToLocation>
          <WhsPutawayToPickFace>0</WhsPutawayToPickFace>
          <WhsPutawayToProductArea>0</WhsPutawayToProductArea>
          <WhsPutawayToClientArea>0</WhsPutawayToClientArea>
          <WhsPutawaySortLocationColumn>0</WhsPutawaySortLocationColumn>
          <WhsPutawaySortLocationLevel>0</WhsPutawaySortLocationLevel>
          <WhsPutawaySortLocationRow>0</WhsPutawaySortLocationRow>
          <WhsPutawaySameProductTogether>false</WhsPutawaySameProductTogether>
          <WhsEDIChangeMessageCancelOrderLinesNotIncludedInMessage>true</WhsEDIChangeMessageCancelOrderLinesNotIncludedInMessage>
          <WhsGenerateBackOrdersOnShortfalls>false</WhsGenerateBackOrdersOnShortfalls>
          <WhsAutoPckEdiOrders>false</WhsAutoPckEdiOrders>
          <WhsOrderFulfillmentRule>NON</WhsOrderFulfillmentRule>
          <WhsPackingSlipOrderBy>DEF</WhsPackingSlipOrderBy>
          <CMDistanceCalculationProvider>DEF</CMDistanceCalculationProvider>
          <CMDistanceCalculationVersion></CMDistanceCalculationVersion>
          <CMDistanceCalculationMethod></CMDistanceCalculationMethod>
          <IMPartAttrib1IsMandatory>false</IMPartAttrib1IsMandatory>
          <IMPartAttrib2IsMandatory>false</IMPartAttrib2IsMandatory>
          <IMPartAttrib3IsMandatory>false</IMPartAttrib3IsMandatory>
          <CMPaidUpCapital>0.0000</CMPaidUpCapital>
          <CMEstablishedDate></CMEstablishedDate>
          <WhsDefaultWarehousePickMode>ASP</WhsDefaultWarehousePickMode>
          <WhsTransportPayer>DEF</WhsTransportPayer>
          <WhsDefaultWarehouseRollUp>false</WhsDefaultWarehouseRollUp>
          <WhsABCAnalysisEnabled>false</WhsABCAnalysisEnabled>
          <WhsABCAnalysisMethod>DEF</WhsABCAnalysisMethod>
          <WhsABCAnalysisPeriod>DEF</WhsABCAnalysisPeriod>
          <IsScanPackQtyAllowed>false</IsScanPackQtyAllowed>
          <IsLabelPrintedOnClosePackage>true</IsLabelPrintedOnClosePackage>
          <IMShowDutyOnWarehouseEntries>false</IMShowDutyOnWarehouseEntries>
          <MinimumShelfLifeAccepted>0</MinimumShelfLifeAccepted>
          <WhsPickFromFIFOBulkOnly>0</WhsPickFromFIFOBulkOnly>
          <EXDefaultCntryOfOrigin TableName=""RefCountry"">
            <Code>US</Code>
            <PK>8d85bde0-7879-4902-ab4b-bf832521a6d3</PK>
          </EXDefaultCntryOfOrigin>
          <FWDefCurrency TableName=""RefCurrency"">
            <Code>USD</Code>
            <PK>60aae969-b80b-4a40-9b2d-810d3385c76e</PK>
          </FWDefCurrency>
        </OrgMiscServ>
        <OrgContactCollection>
          <OrgContact Action=""MERGE"">
            <PK>059b713e-98a2-40dd-9a76-fbe167cf2a91</PK>
            <IsActive>true</IsActive>
            <ContactName>FRED SIMPSON</ContactName>
            <Salutation></Salutation>
            <Language>EN</Language>
            <NotifyMode>EML</NotifyMode>
            <Title></Title>
            <JobCategory></JobCategory>
            <Phone></Phone>
            <PhoneExtension></PhoneExtension>
            <Fax></Fax>
            <Mobile></Mobile>
            <HomePhone></HomePhone>
            <Pager></Pager>
            <OtherPhone></OtherPhone>
            <Email></Email>
            <AttachmentType>PDF</AttachmentType>
            <WebAccessEnabled>false</WebAccessEnabled>
            <Password></Password>
            <Birthday></Birthday>
            <YearJoinedIndustry></YearJoinedIndustry>
            <YearJoinedCompany></YearJoinedCompany>
            <ContactSource></ContactSource>
            <DetailsVerified></DetailsVerified>
            <PersonalInfo></PersonalInfo>
            <WebContractSignedDate></WebContractSignedDate>
            <Gender></Gender>
            <ProfilePhoto></ProfilePhoto>
          </OrgContact>
        </OrgContactCollection>
        <OrgAddressCollection>
          <OrgAddress Action=""MERGE"">
            <PK>0011ec59-2c12-4a90-9ef5-89f20fc2a9cf</PK>
            <IsActive>true</IsActive>
            <Code>1 ADDRESS RD</Code>
            <Language>EN</Language>
            <CompanyNameOverride></CompanyNameOverride>
            <Address1>1 ADDRESS RD</Address1>
            <Address2></Address2>
            <City>FIRSTVILLE</City>
            <State>CA</State>
            <PostCode></PostCode>
            <Phone></Phone>
            <Fax></Fax>
            <Mobile></Mobile>
            <Email></Email>
            <PickupFromTimeOnly></PickupFromTimeOnly>
            <PickupToTimeOnly></PickupToTimeOnly>
            <DeliverFromTimeOnly></DeliverFromTimeOnly>
            <DeliverToTimeOnly></DeliverToTimeOnly>
            <DoNotAttendFrom></DoNotAttendFrom>
            <DoNotAttendTo></DoNotAttendTo>
            <DockLeveler>false</DockLeveler>
            <ForkLift>false</ForkLift>
            <PalletJack>false</PalletJack>
            <ContainerHandling></ContainerHandling>
            <AccessPoint></AccessPoint>
            <LabourRequired></LabourRequired>
            <CommunicationRequired></CommunicationRequired>
            <Dock_Height></Dock_Height>
            <OtherWarehouseFacilities></OtherWarehouseFacilities>
            <LoadingUnloadingConstraints></LoadingUnloadingConstraints>
            <FCLEquipmentNeeded>WUP</FCLEquipmentNeeded>
            <LCLEquipmentNeeded>PSL</LCLEquipmentNeeded>
            <AIREquipmentNeeded>PSL</AIREquipmentNeeded>
            <Latitude>0.00000</Latitude>
            <Longitude>0.00000</Longitude>
            <OrgAddressCapabilityCollection>
              <OrgAddressCapability Action=""MERGE"">
                <PK>be57b16d-139d-4620-aca8-88a50229a32b</PK>
                <AddressType>OFC</AddressType>
                <IsMainAddress>true</IsMainAddress>
              </OrgAddressCapability>
            </OrgAddressCapabilityCollection>
            <RelatedPortCode TableName=""RefUNLOCO"">
              <Code>USHBO</Code>
              <PK>8f4920bb-23f8-4c00-93c4-74d7759bf8f4</PK>
            </RelatedPortCode>
          </OrgAddress>
        </OrgAddressCollection>
        <OrgCompanyDataCollection>
          <OrgCompanyData Action=""MERGE"">
            <PK>c160ff29-0073-48dd-9550-84b545fb21d8</PK>
            <IsDebtor>false</IsDebtor>
            <IsCreditor>false</IsCreditor>
            <APCategory></APCategory>
            <APCreditLimit>0.0000</APCreditLimit>
            <APPayInvoiceAfterPostingDefault>false</APPayInvoiceAfterPostingDefault>
            <APPaymentTermDays>0</APPaymentTermDays>
            <APPaymentTerms>COD</APPaymentTerms>
            <APWHTApplicable>false</APWHTApplicable>
            <APAirlineAccountNumber></APAirlineAccountNumber>
            <ARAutoUpdateRates>true</ARAutoUpdateRates>
            <ARCategory></ARCategory>
            <ARCombinedStatementInvoice>false</ARCombinedStatementInvoice>
            <ARConsolidatedAccountingCategory></ARConsolidatedAccountingCategory>
            <ARCreditLimit>0.0000</ARCreditLimit>
            <ARCreditRating></ARCreditRating>
            <ARSeaInvoiceTermDays>COD</ARSeaInvoiceTermDays>
            <ARSeaInvoiceTerms>0</ARSeaInvoiceTerms>
            <ARAirInvoiceTermDays>COD</ARAirInvoiceTermDays>
            <ARAirInvoiceTerms>0</ARAirInvoiceTerms>
            <ARInvoiceTerms>COD</ARInvoiceTerms>
            <ARInvoiceTermDays>0</ARInvoiceTermDays>
            <ARTreatDisbursementsAsStandardValue>0.0000</ARTreatDisbursementsAsStandardValue>
            <ARDisbursementInvoiceTerms>COD</ARDisbursementInvoiceTerms>
            <ARDisbursementInvoiceTermDays>0</ARDisbursementInvoiceTermDays>
            <ARDontShowTaxOnDocs>false</ARDontShowTaxOnDocs>
            <ARForeignCurrStatement>false</ARForeignCurrStatement>
            <ARBuyersConsolInvoicingStyle>DEF</ARBuyersConsolInvoicingStyle>
            <ARSellersConsolInvoicingStyle>DEF</ARSellersConsolInvoicingStyle>
            <AROnCreditHold>false</AROnCreditHold>
            <ARAccountAndCreditReviewDue></ARAccountAndCreditReviewDue>
            <ARPreviousChequeDrawer></ARPreviousChequeDrawer>
            <ARPreviousChequeDrawerBank></ARPreviousChequeDrawerBank>
            <ARPreviousChequeDrawerBankBranch></ARPreviousChequeDrawerBankBranch>
            <ARReceiptInvoiceAfterPostingDefault>false</ARReceiptInvoiceAfterPostingDefault>
            <AREftCustomsPaymentMethod></AREftCustomsPaymentMethod>
            <ARWHTApplicable>false</ARWHTApplicable>
            <ARIncludeInwardsWhsConsolidatedInvoice>true</ARIncludeInwardsWhsConsolidatedInvoice>
            <ARIncludeOutwardsWhsConsolidatedInvoice>true</ARIncludeOutwardsWhsConsolidatedInvoice>
            <ARWhsStorageCalcMethod>MAX</ARWhsStorageCalcMethod>
            <CRIsShipsAgencyPrincipal>false</CRIsShipsAgencyPrincipal>
            <ARUseSystemDefaultUplifts>true</ARUseSystemDefaultUplifts>
            <ARExportAirCollectUplift>0.000</ARExportAirCollectUplift>
            <ARExportSeaCollectUplift>0.000</ARExportSeaCollectUplift>
            <ARImportAirCollectUplift>0.000</ARImportAirCollectUplift>
            <ARImportSeaCollectUplift>0.000</ARImportSeaCollectUplift>
            <ARUseSystemDefaultUpliftsMinimums>true</ARUseSystemDefaultUpliftsMinimums>
            <ARExportAirCollectUpliftMinimum>0.0000</ARExportAirCollectUpliftMinimum>
            <ARExportSeaCollectUpliftMinimum>0.0000</ARExportSeaCollectUpliftMinimum>
            <ARImportAirCollectUpliftMinimum>0.0000</ARImportAirCollectUpliftMinimum>
            <ARImportSeaCollectUpliftMinimum>0.0000</ARImportSeaCollectUpliftMinimum>
            <IMUsedBondedWhs>false</IMUsedBondedWhs>
            <APQualityAssured>false</APQualityAssured>
            <APQualityAssuredCheckedDate></APQualityAssuredCheckedDate>
            <ARQualityAssured>false</ARQualityAssured>
            <ARQualityAssuredCheckedDate></ARQualityAssuredCheckedDate>
            <ARWarehouseRatingPeriod>DEF</ARWarehouseRatingPeriod>
            <WhsClientFreeStorageDays>0</WhsClientFreeStorageDays>
            <WhsPeriodicBillingDay>0</WhsPeriodicBillingDay>
            <WhsOverrideFreeStorage>false</WhsOverrideFreeStorage>
            <APCostsSelfBilled>false</APCostsSelfBilled>
            <APPrintContractorForm>false</APPrintContractorForm>
            <ARAllowMultiCurrencyPayment>false</ARAllowMultiCurrencyPayment>
            <ARCreditCardType></ARCreditCardType>
            <ARCreditCardNum></ARCreditCardNum>
            <ARCreditCardAdditionalInfo></ARCreditCardAdditionalInfo>
            <ARCreditCardExpire></ARCreditCardExpire>
            <ARCreditCardHolder></ARCreditCardHolder>
            <ARCreditApproved>true</ARCreditApproved>
            <ARUseSettlementGroupCreditLimit>false</ARUseSettlementGroupCreditLimit>
            <EXBillAgentChargesDirect>false</EXBillAgentChargesDirect>
            <IMBillAgentChargesDirect>false</IMBillAgentChargesDirect>
            <APExternalCreditorCode></APExternalCreditorCode>
            <ARExternalDebtorCode></ARExternalDebtorCode>
            <APCreditAgreedPaymentMethod></APCreditAgreedPaymentMethod>
            <ARCreditAgreedPaymentMethod></ARCreditAgreedPaymentMethod>
            <WhsIncludeReleaseChargesOnShipment>true</WhsIncludeReleaseChargesOnShipment>
            <ARVATConfig>DEF</ARVATConfig>
            <APVATConfig>NON</APVATConfig>
            <RateSecurityGroup></RateSecurityGroup>
            <OrgInvoiceRollupOrGroupCollection>
              <OrgInvoiceRollupOrGroup Action=""MERGE"">
                <PK>27a29287-cd74-4ab9-8871-59c661757806</PK>
                <JobType>ALL</JobType>
                <TransportMode>ALL</TransportMode>
                <ServiceDirection>ALL</ServiceDirection>
                <GroupOrSubTotal>DEF</GroupOrSubTotal>
                <GroupOrSubtotalStyle>DEF</GroupOrSubtotalStyle>
                <InvoiceLineDisplayOption>DEF</InvoiceLineDisplayOption>
                <InvoicePostingStyle>DEF</InvoicePostingStyle>
              </OrgInvoiceRollupOrGroup>
            </OrgInvoiceRollupOrGroupCollection>
            <GlbCompany>
              <Code>EDI</Code>
              <PK>878d7aca-ffc3-49fc-9710-969ca0c0f2ac</PK>
            </GlbCompany>
            <APDefltCurrency TableName=""RefCurrency"">
              <Code>USD</Code>
              <PK>60aae969-b80b-4a40-9b2d-810d3385c76e</PK>
            </APDefltCurrency>
            <ARDDefltCurrency TableName=""RefCurrency"">
              <Code>AUD</Code>
              <PK>53feb23f-a7b0-4e0d-af5d-f6f6e5417399</PK>
            </ARDDefltCurrency>
          </OrgCompanyData>
        </OrgCompanyDataCollection>
        <OrgRateTariffLevelCollection>
          <OrgRateTariffLevel Action=""MERGE"">
            <PK>6745b876-12ce-43e3-bdfb-d22db0682ec3</PK>
            <TariffType>DEF</TariffType>
            <TariffLevel>0</TariffLevel>
            <Mode>ALL</Mode>
            <Direction>ALL</Direction>
            <ExpiryDate>2024-09-03T00:00:00</ExpiryDate>
            <StartDate>2024-09-02T00:00:00</StartDate>
            <GlbCompany>
              <Code>EDI</Code>
              <PK>878d7aca-ffc3-49fc-9710-969ca0c0f2ac</PK>
            </GlbCompany>
          </OrgRateTariffLevel>
        </OrgRateTariffLevelCollection>
        <StmNoteCollection>
          <StmNote Action=""MERGE"">
            <Description>Customs Note Description</Description>
            <NoteData>e1xydGYxXGFuc2lcZGVmZjB7XGZvbnR0Ymx7XGYwXGZuaWxcZmNoYXJzZXQwIE1pY3Jvc29mdCBTYW5zIFNlcmlmO319DQpcdmlld2tpbmQ0XHVjMVxwYXJkXGxhbmczMDgxXGYwXGZzMjAgU3RyaW5nIE9mIERhdGEgc29tZWhvdyByZWxhdGluZyB0byB0aGlzIHRlc3RccGFyDQp9DQo=</NoteData>
            <NoteText></NoteText>
            <NoteType>INT</NoteType>
            <NoteContext>CEL</NoteContext>
            <IsCustomDescription>true</IsCustomDescription>
            <ForceRead>true</ForceRead>
          </StmNote>
          <StmNote Action=""MERGE"">
            <Description>Special Instructions</Description>
            <NoteData></NoteData>
            <NoteText>Special Instructions Note Text Field</NoteText>
            <NoteType>PUB</NoteType>
            <NoteContext>DXW</NoteContext>
            <IsCustomDescription>false</IsCustomDescription>
            <ForceRead>true</ForceRead>
          </StmNote>
        </StmNoteCollection>
        <ClosestPort TableName=""RefUNLOCO"">
          <Code>USHBO</Code>
          <PK>8f4920bb-23f8-4c00-93c4-74d7759bf8f4</PK>
        </ClosestPort>
      </OrgHeader>
    </Organization>
  </Body>
</Native>";

		#endregion

		public void TestImportXMLOfOrgHeaderWithNotes()
		{
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(OrgHeaderWithNotesXml)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 1 inserts, 0 updates, 0 deletes
OrgMiscServ - 1 inserts, 0 updates, 0 deletes
OrgContact - 1 inserts, 0 updates, 0 deletes
OrgAddress - 1 inserts, 0 updates, 0 deletes
OrgAddressCapability - 1 inserts, 0 updates, 0 deletes
OrgCompanyData - 1 inserts, 0 updates, 0 deletes
OrgInvoiceRollupOrGroup - 1 inserts, 0 updates, 0 deletes
OrgRateTariffLevel - 1 inserts, 0 updates, 0 deletes
StmNote - 2 inserts, 0 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Log Text", expectedLog, manager.GetLogs());
			}

			var createdOrgHeaderList = new BusinessObjectFactory().Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "FROICEHBO"));
			AssertEquals(1, createdOrgHeaderList.Length);
			var notes = createdOrgHeaderList.First().Notes;

			AssertEquals(2, notes.GetAllNotes().Count);

			AssertEquals(1, notes.FindByDescription("Customs Note Description").Length);
			AssertEquals(1, notes.FindByDescription("Special Instructions").Length);

			var note1 = notes.FindByDescription("Customs Note Description")[0];
			var note2 = notes.FindByDescription("Special Instructions")[0];

			AssertEquals(true, note1.ST_IsCustomDescription);
			AssertEquals("C - CFS", note1.ST_NoteContextModuleCaption);
			AssertEquals("E - Export", note1.ST_NoteContextDirectionCaption);
			AssertEquals("L - LCL", note1.ST_NoteContextFreightModeCaption);
			AssertEquals("String Of Data somehow relating to this test", note1.ST_NoteDataAsText);
			AssertEquals("String Of Data somehow relating to this test", note1.ST_NoteDataAsText);

			AssertEquals(false, note2.ST_IsCustomDescription);
			AssertEquals("D - Customs/Declarations", note2.ST_NoteContextModuleCaption);
			AssertEquals("X - Cross Trade", note2.ST_NoteContextDirectionCaption);
			AssertEquals("W - Rail", note2.ST_NoteContextFreightModeCaption);
			AssertEquals("Special Instructions Note Text Field", note2.ST_NoteDataAsText);
		}

		#region OrgHeaderWithNotesXmlDifferentVarbinaryNote

		const string OrgHeaderWithNotesXmlDifferentVarbinaryNote = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>EDICUS</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Organization version=""2.0"">
      <OrgHeader Action=""MERGE"">
        <PK>a9d44573-919d-472f-a385-ee81bfcb5ea9</PK>
        <Code>FROICEHBO</Code>
        <IsActive>true</IsActive>
        <FullName>FROSTY ICE CREAM</FullName>
        <IsConsignee>true</IsConsignee>
        <IsConsignor>false</IsConsignor>
        <IsTransportClient>false</IsTransportClient>
        <IsWarehouseClient>false</IsWarehouseClient>
        <IsForwarder>false</IsForwarder>
        <IsShippingProvider>false</IsShippingProvider>
        <IsAirWholesaler>false</IsAirWholesaler>
        <IsSeaWholesaler>false</IsSeaWholesaler>
        <IsRailProvider>false</IsRailProvider>
        <IsLineHaulProvider>false</IsLineHaulProvider>
        <IsMiscFreightServices>false</IsMiscFreightServices>
        <IsAirCTO>false</IsAirCTO>
        <IsAirLine>false</IsAirLine>
        <IsBroker>false</IsBroker>
        <IsContainerPark>false</IsContainerPark>
        <IsLocalTransport>true</IsLocalTransport>
        <IsPackDepot>false</IsPackDepot>
        <IsSeaCTO>false</IsSeaCTO>
        <IsShippingLine>false</IsShippingLine>
        <IsUnpackDepot>false</IsUnpackDepot>
        <IsRailHead>false</IsRailHead>
        <IsRoadFreightDepot>false</IsRoadFreightDepot>
        <IsShippingConsortium>false</IsShippingConsortium>
        <IsFumigationContractor>false</IsFumigationContractor>
        <IsNationalAccount>false</IsNationalAccount>
        <IsSalesLead>false</IsSalesLead>
        <IsUserFlag13>false</IsUserFlag13>
        <IsUserFlag12>false</IsUserFlag12>
        <IsUserFlag14>false</IsUserFlag14>
        <IsUserFlag11>false</IsUserFlag11>
        <IsCompetitor>false</IsCompetitor>
        <IsTempAccount>false</IsTempAccount>
        <IsPersonalEffectsAccount>false</IsPersonalEffectsAccount>
        <IsUserFlag1>false</IsUserFlag1>
        <IsUserFlag2>false</IsUserFlag2>
        <IsUserFlag3>false</IsUserFlag3>
        <IsUserFlag4>false</IsUserFlag4>
        <IsUserFlag5>false</IsUserFlag5>
        <IsUserFlag6>false</IsUserFlag6>
        <IsUserFlag7>false</IsUserFlag7>
        <IsUserFlag8>false</IsUserFlag8>
        <IsUserFlag9>false</IsUserFlag9>
        <IsUserFlag10>false</IsUserFlag10>
        <Language>EN</Language>
        <IsGlobalAccount>false</IsGlobalAccount>
        <ScreeningStatus>UNK</ScreeningStatus>
        <OrgMiscServ Action=""MERGE"">
          <PK>0a281adc-b184-4a24-851b-8bd88b5abe1c</PK>
          <Airline3CharCode></Airline3CharCode>
          <IMEftCustomsFromImport>false</IMEftCustomsFromImport>
          <IMEftQuarantineFromImport>false</IMEftQuarantineFromImport>
          <IMEftHoldUntilPayAuthorised>false</IMEftHoldUntilPayAuthorised>
          <IMEstDaysDeliveryAir>0</IMEstDaysDeliveryAir>
          <IMEstDaysDeliveryFCL>0</IMEstDaysDeliveryFCL>
          <IMEstDaysDeliveryLCL>0</IMEstDaysDeliveryLCL>
          <IMMaxEFTAmount>0.0000</IMMaxEFTAmount>
          <IMMinEFTAmount>0.0000</IMMinEFTAmount>
          <IMEFTBankAccount></IMEFTBankAccount>
          <IMEFTBankBSB></IMEFTBankBSB>
          <IMIsGSTDeferred>false</IMIsGSTDeferred>
          <IMMergeCustomsInvoiceLinesBy>NON</IMMergeCustomsInvoiceLinesBy>
          <IMOrderLineAttrib1></IMOrderLineAttrib1>
          <IMOrderLineAttrib2></IMOrderLineAttrib2>
          <IMOrderLineAttrib3></IMOrderLineAttrib3>
          <IMOriginalSeaBills>3</IMOriginalSeaBills>
          <IMCopySeaBills>3</IMCopySeaBills>
          <IMSendImportDocsTo>IMP</IMSendImportDocsTo>
          <IMSendSeaImportDocsTo>IMP</IMSendSeaImportDocsTo>
          <IMImporterCategory>STD</IMImporterCategory>
          <IMAirDepotFreeDays>1</IMAirDepotFreeDays>
          <IMSeaDepotFreeDays>3</IMSeaDepotFreeDays>
          <IMImporterOwnsPartNumbers>false</IMImporterOwnsPartNumbers>
          <IMOrderStatusCodePairList></IMOrderStatusCodePairList>
          <IMOrderLineStatusCodePairList></IMOrderLineStatusCodePairList>
          <IMFCLEquipmentNeeded></IMFCLEquipmentNeeded>
          <IMLCLEquipmentNeeded></IMLCLEquipmentNeeded>
          <IMAirEquipmentNeeded></IMAirEquipmentNeeded>
          <IMDefaultINCOTerm>FOB</IMDefaultINCOTerm>
          <IMAutoImpJobRefered>false</IMAutoImpJobRefered>
          <IMPartAttrib1Type></IMPartAttrib1Type>
          <IMPartAttrib1Name></IMPartAttrib1Name>
          <IMPartAttrib2Type></IMPartAttrib2Type>
          <IMPartAttrib2Name></IMPartAttrib2Name>
          <IMPartAttrib3Type></IMPartAttrib3Type>
          <IMPartAttrib3Name></IMPartAttrib3Name>
          <IMUseExpiryDate>false</IMUseExpiryDate>
          <IMUsePackingDate>false</IMUsePackingDate>
          <IMImporterRequiresOrderNumbersOnDocs>false</IMImporterRequiresOrderNumbersOnDocs>
          <IMJobRequireOrderTrackLink>false</IMJobRequireOrderTrackLink>
          <LandedCostMarginPercent1>0.00</LandedCostMarginPercent1>
          <LandedCostMarginPercent2>0.00</LandedCostMarginPercent2>
          <LandedCostMarginPercent3>0.00</LandedCostMarginPercent3>
          <LastArchiveDate></LastArchiveDate>
          <EXJobRequireOrderTrackLink>false</EXJobRequireOrderTrackLink>
          <EXExporterRequiresOrderNumbersOnDocs>false</EXExporterRequiresOrderNumbersOnDocs>
          <EXExporterCategory>STD</EXExporterCategory>
          <EXDefaultIncoTerm>FOB</EXDefaultIncoTerm>
          <EXFCLEquipmentNeeded></EXFCLEquipmentNeeded>
          <EXLCLEquipmentNeeded></EXLCLEquipmentNeeded>
          <EXAirEquipmentNeeded></EXAirEquipmentNeeded>
          <EXAllowedToPrintOriginalBL>false</EXAllowedToPrintOriginalBL>
          <FWAgentCategory>STD</FWAgentCategory>
          <FWAgentBelongsToGroup>false</FWAgentBelongsToGroup>
          <FWHandlesAir>false</FWHandlesAir>
          <FWHandlesSea>false</FWHandlesSea>
          <FWHandlesSeaForPortOrCountry></FWHandlesSeaForPortOrCountry>
          <FWHandlesAirForPortOrCountry></FWHandlesAirForPortOrCountry>
          <FWRequestForCreditAllowed>false</FWRequestForCreditAllowed>
          <FWBillCollectFeesOnSingleInvoice>true</FWBillCollectFeesOnSingleInvoice>
          <FWDealDirectlyWithUltimates>false</FWDealDirectlyWithUltimates>
          <FWIATACode></FWIATACode>
          <FWIATAAccountNumber></FWIATAAccountNumber>
          <CRCarrierCategory></CRCarrierCategory>
          <SVServicesCategory></SVServicesCategory>
          <CMSalesCategory></CMSalesCategory>
          <CMCompetitorActivity>FRT</CMCompetitorActivity>
          <CMLastCallDate></CMLastCallDate>
          <CMClientSize></CMClientSize>
          <CMGrowthOutlook></CMGrowthOutlook>
          <CMFollowUpDate></CMFollowUpDate>
          <CMEstimatedDateToClose></CMEstimatedDateToClose>
          <CMDoesExports>false</CMDoesExports>
          <CMDoesImports>false</CMDoesImports>
          <CMUseTradeLaneFigures>true</CMUseTradeLaneFigures>
          <CMTotalClientRevenue>0.0000</CMTotalClientRevenue>
          <CMPercentage>0.000</CMPercentage>
          <CMAcheivableClientRevenue>0.0000</CMAcheivableClientRevenue>
          <CMEstimatedProfit>0.0000</CMEstimatedProfit>
          <CMWarehouseRevenue>0.0000</CMWarehouseRevenue>
          <CMConsultingRevenue>0.0000</CMConsultingRevenue>
          <CMOverallClientRelation>0</CMOverallClientRelation>
          <CMClientsDesireToRemain>0</CMClientsDesireToRemain>
          <CMEaseClientCanBePoached>0</CMEaseClientCanBePoached>
          <CMAmountOfElectronicIntegration>0</CMAmountOfElectronicIntegration>
          <CMOverallEffectOfClientOnAirfreightCosts></CMOverallEffectOfClientOnAirfreightCosts>
          <CMOverallEffectOfClientOnLCLCosts></CMOverallEffectOfClientOnLCLCosts>
          <CMOverallEffectOfClientOnTEUCosts></CMOverallEffectOfClientOnTEUCosts>
          <CMOverallEffectOfClientOnWarehousingCosts></CMOverallEffectOfClientOnWarehousingCosts>
          <CMOverallEffectOfClientOnOtherCosts></CMOverallEffectOfClientOnOtherCosts>
          <CMAmountOfBusinessWon>0</CMAmountOfBusinessWon>
          <CMSalesTerritory></CMSalesTerritory>
          <CMIsHouseAccount>false</CMIsHouseAccount>
          <CMCommission></CMCommission>
          <CMClientCommenced></CMClientCommenced>
          <CICompetitorCategory></CICompetitorCategory>
          <CIEstimatedStaffThisLocation>0</CIEstimatedStaffThisLocation>
          <CITypeOfService></CITypeOfService>
          <CIEstimatedStaffThisCountry>0</CIEstimatedStaffThisCountry>
          <CISellingStyle></CISellingStyle>
          <CITurnover>0.0000</CITurnover>
          <CIProfit>0.0000</CIProfit>
          <CICapitalEmployed>0.0000</CICapitalEmployed>
          <CICompetitiveRanking>0</CICompetitiveRanking>
          <CIStrength></CIStrength>
          <CIWeaknesses></CIWeaknesses>
          <CIOpportunities></CIOpportunities>
          <CIThreats></CIThreats>
          <CustomAttrib1></CustomAttrib1>
          <CustomAttrib2></CustomAttrib2>
          <CustomAttrib3></CustomAttrib3>
          <CustomDate1></CustomDate1>
          <CustomDate2></CustomDate2>
          <CustomDate3></CustomDate3>
          <CustomDecimal1>0.000</CustomDecimal1>
          <CustomDecimal2>0.000</CustomDecimal2>
          <CustomDecimal3>0.000</CustomDecimal3>
          <CustomFlag1>false</CustomFlag1>
          <CustomFlag2>false</CustomFlag2>
          <CustomFlag3>false</CustomFlag3>
          <IMAutoPopulateOwnerRefWithOrderNums>DEF</IMAutoPopulateOwnerRefWithOrderNums>
          <IMDocumentAddressPreference></IMDocumentAddressPreference>
          <IMDefaultWarehousePickOption>AUT</IMDefaultWarehousePickOption>
          <IMInvoiceDetailReportSort>DEF</IMInvoiceDetailReportSort>
          <EXDocumentAddressPreference></EXDocumentAddressPreference>
          <EXDefaultDGContactPhoneUsed></EXDefaultDGContactPhoneUsed>
          <EXDefaultInvoicePriceFromProductLastCost></EXDefaultInvoicePriceFromProductLastCost>
          <FWDirectAMSReporter>false</FWDirectAMSReporter>
          <WhsOverrideSystemPickingRules>false</WhsOverrideSystemPickingRules>
          <WhsPickFromDefaultFIFO>0</WhsPickFromDefaultFIFO>
          <WhsPickFromFullPallets>0</WhsPickFromFullPallets>
          <WhsPickFromConsolidatedFullPallets>0</WhsPickFromConsolidatedFullPallets>
          <WhsPickFromPickFaces>0</WhsPickFromPickFaces>
          <WhsPickFromPalletOverflow>0</WhsPickFromPalletOverflow>
          <WhsPickFromBrokenPallets>0</WhsPickFromBrokenPallets>
          <CustomFlag4>false</CustomFlag4>
          <IMAttrib1IsKey>false</IMAttrib1IsKey>
          <IMAttrib2IsKey>false</IMAttrib2IsKey>
          <IMAttrib3IsKey>false</IMAttrib3IsKey>
          <WhsPickSortArrivalDate>0</WhsPickSortArrivalDate>
          <WhsPickSortExpiryDate>0</WhsPickSortExpiryDate>
          <WhsPickSortPackingDate>0</WhsPickSortPackingDate>
          <WhsPickSortPickFace>0</WhsPickSortPickFace>
          <WhsPickSortLocationRow>0</WhsPickSortLocationRow>
          <WhsPickSortLocationColumn>0</WhsPickSortLocationColumn>
          <WhsPickSortLocationLevel>0</WhsPickSortLocationLevel>
          <WhsExpiryNotificationFromDefaults>true</WhsExpiryNotificationFromDefaults>
          <WhsExpiryNotificationPeriod>0</WhsExpiryNotificationPeriod>
          <WhsClientInvoiceFormat></WhsClientInvoiceFormat>
          <IMLastOrderReference></IMLastOrderReference>
          <IMDefaultToNewOrdersToNextOrderNum>false</IMDefaultToNewOrdersToNextOrderNum>
          <IMInvoiceDetailReportSort2>DEF</IMInvoiceDetailReportSort2>
          <IMInvoiceDetailReportSort3>DEF</IMInvoiceDetailReportSort3>
          <EXGoodsDescription></EXGoodsDescription>
          <EXHandlingInstuctions></EXHandlingInstuctions>
          <EXMergeCustomsInvoiceLinesBy>NON</EXMergeCustomsInvoiceLinesBy>
          <EXPreAllocPrefix></EXPreAllocPrefix>
          <CMNoOfEmployees>0</CMNoOfEmployees>
          <CMClientPortalHomePage></CMClientPortalHomePage>
          <WhsOrderNumberUniquenessStrategy></WhsOrderNumberUniquenessStrategy>
          <WhsOverrideSystemPutawayRules>false</WhsOverrideSystemPutawayRules>
          <WhsPutawayToLocation>0</WhsPutawayToLocation>
          <WhsPutawayToPickFace>0</WhsPutawayToPickFace>
          <WhsPutawayToProductArea>0</WhsPutawayToProductArea>
          <WhsPutawayToClientArea>0</WhsPutawayToClientArea>
          <WhsPutawaySortLocationColumn>0</WhsPutawaySortLocationColumn>
          <WhsPutawaySortLocationLevel>0</WhsPutawaySortLocationLevel>
          <WhsPutawaySortLocationRow>0</WhsPutawaySortLocationRow>
          <WhsPutawaySameProductTogether>false</WhsPutawaySameProductTogether>
          <WhsEDIChangeMessageCancelOrderLinesNotIncludedInMessage>true</WhsEDIChangeMessageCancelOrderLinesNotIncludedInMessage>
          <WhsGenerateBackOrdersOnShortfalls>false</WhsGenerateBackOrdersOnShortfalls>
          <WhsAutoPckEdiOrders>false</WhsAutoPckEdiOrders>
          <WhsOrderFulfillmentRule>NON</WhsOrderFulfillmentRule>
          <WhsPackingSlipOrderBy>DEF</WhsPackingSlipOrderBy>
          <CMDistanceCalculationProvider>DEF</CMDistanceCalculationProvider>
          <CMDistanceCalculationVersion></CMDistanceCalculationVersion>
          <CMDistanceCalculationMethod></CMDistanceCalculationMethod>
          <IMPartAttrib1IsMandatory>false</IMPartAttrib1IsMandatory>
          <IMPartAttrib2IsMandatory>false</IMPartAttrib2IsMandatory>
          <IMPartAttrib3IsMandatory>false</IMPartAttrib3IsMandatory>
          <CMPaidUpCapital>0.0000</CMPaidUpCapital>
          <CMEstablishedDate></CMEstablishedDate>
          <WhsDefaultWarehousePickMode>ASP</WhsDefaultWarehousePickMode>
          <WhsTransportPayer>DEF</WhsTransportPayer>
          <WhsDefaultWarehouseRollUp>false</WhsDefaultWarehouseRollUp>
          <WhsABCAnalysisEnabled>false</WhsABCAnalysisEnabled>
          <WhsABCAnalysisMethod>DEF</WhsABCAnalysisMethod>
          <WhsABCAnalysisPeriod>DEF</WhsABCAnalysisPeriod>
          <IsScanPackQtyAllowed>false</IsScanPackQtyAllowed>
          <IsLabelPrintedOnClosePackage>true</IsLabelPrintedOnClosePackage>
          <IMShowDutyOnWarehouseEntries>false</IMShowDutyOnWarehouseEntries>
          <MinimumShelfLifeAccepted>0</MinimumShelfLifeAccepted>
          <WhsPickFromFIFOBulkOnly>0</WhsPickFromFIFOBulkOnly>
          <EXDefaultCntryOfOrigin TableName=""RefCountry"">
            <Code>US</Code>
            <PK>8d85bde0-7879-4902-ab4b-bf832521a6d3</PK>
          </EXDefaultCntryOfOrigin>
          <FWDefCurrency TableName=""RefCurrency"">
            <Code>USD</Code>
            <PK>60aae969-b80b-4a40-9b2d-810d3385c76e</PK>
          </FWDefCurrency>
        </OrgMiscServ>
        <OrgContactCollection>
          <OrgContact Action=""MERGE"">
            <PK>059b713e-98a2-40dd-9a76-fbe167cf2a91</PK>
            <IsActive>true</IsActive>
            <ContactName>FRED SIMPSON</ContactName>
            <Salutation></Salutation>
            <Language>EN</Language>
            <NotifyMode>EML</NotifyMode>
            <Title></Title>
            <JobCategory></JobCategory>
            <Phone></Phone>
            <PhoneExtension></PhoneExtension>
            <Fax></Fax>
            <Mobile></Mobile>
            <HomePhone></HomePhone>
            <Pager></Pager>
            <OtherPhone></OtherPhone>
            <Email></Email>
            <AttachmentType>PDF</AttachmentType>
            <WebAccessEnabled>false</WebAccessEnabled>
            <Password></Password>
            <Birthday></Birthday>
            <YearJoinedIndustry></YearJoinedIndustry>
            <YearJoinedCompany></YearJoinedCompany>
            <ContactSource></ContactSource>
            <DetailsVerified></DetailsVerified>
            <PersonalInfo></PersonalInfo>
            <WebContractSignedDate></WebContractSignedDate>
            <Gender></Gender>
            <ProfilePhoto></ProfilePhoto>
          </OrgContact>
        </OrgContactCollection>
        <OrgAddressCollection>
          <OrgAddress Action=""MERGE"">
            <PK>0011ec59-2c12-4a90-9ef5-89f20fc2a9cf</PK>
            <IsActive>true</IsActive>
            <Code>1 ADDRESS RD</Code>
            <Language>EN</Language>
            <CompanyNameOverride></CompanyNameOverride>
            <Address1>1 ADDRESS RD</Address1>
            <Address2></Address2>
            <City>FIRSTVILLE</City>
            <State>CA</State>
            <PostCode></PostCode>
            <Phone></Phone>
            <Fax></Fax>
            <Mobile></Mobile>
            <Email></Email>
            <PickupFromTimeOnly></PickupFromTimeOnly>
            <PickupToTimeOnly></PickupToTimeOnly>
            <DeliverFromTimeOnly></DeliverFromTimeOnly>
            <DeliverToTimeOnly></DeliverToTimeOnly>
            <DoNotAttendFrom></DoNotAttendFrom>
            <DoNotAttendTo></DoNotAttendTo>
            <DockLeveler>false</DockLeveler>
            <ForkLift>false</ForkLift>
            <PalletJack>false</PalletJack>
            <ContainerHandling></ContainerHandling>
            <AccessPoint></AccessPoint>
            <LabourRequired></LabourRequired>
            <CommunicationRequired></CommunicationRequired>
            <Dock_Height></Dock_Height>
            <OtherWarehouseFacilities></OtherWarehouseFacilities>
            <LoadingUnloadingConstraints></LoadingUnloadingConstraints>
            <FCLEquipmentNeeded>WUP</FCLEquipmentNeeded>
            <LCLEquipmentNeeded>PSL</LCLEquipmentNeeded>
            <AIREquipmentNeeded>PSL</AIREquipmentNeeded>
            <Latitude>0.00000</Latitude>
            <Longitude>0.00000</Longitude>
            <OrgAddressCapabilityCollection>
              <OrgAddressCapability Action=""MERGE"">
                <PK>be57b16d-139d-4620-aca8-88a50229a32b</PK>
                <AddressType>OFC</AddressType>
                <IsMainAddress>true</IsMainAddress>
              </OrgAddressCapability>
            </OrgAddressCapabilityCollection>
            <RelatedPortCode TableName=""RefUNLOCO"">
              <Code>USHBO</Code>
              <PK>8f4920bb-23f8-4c00-93c4-74d7759bf8f4</PK>
            </RelatedPortCode>
          </OrgAddress>
        </OrgAddressCollection>
        <OrgCompanyDataCollection>
          <OrgCompanyData Action=""MERGE"">
            <PK>c160ff29-0073-48dd-9550-84b545fb21d8</PK>
            <IsDebtor>false</IsDebtor>
            <IsCreditor>false</IsCreditor>
            <APCategory></APCategory>
            <APCreditLimit>0.0000</APCreditLimit>
            <APPayInvoiceAfterPostingDefault>false</APPayInvoiceAfterPostingDefault>
            <APPaymentTermDays>0</APPaymentTermDays>
            <APPaymentTerms>COD</APPaymentTerms>
            <APWHTApplicable>false</APWHTApplicable>
            <APAirlineAccountNumber></APAirlineAccountNumber>
            <ARAutoUpdateRates>true</ARAutoUpdateRates>
            <ARCategory></ARCategory>
            <ARCombinedStatementInvoice>false</ARCombinedStatementInvoice>
            <ARConsolidatedAccountingCategory></ARConsolidatedAccountingCategory>
            <ARCreditLimit>0.0000</ARCreditLimit>
            <ARCreditRating></ARCreditRating>
            <ARSeaInvoiceTermDays>COD</ARSeaInvoiceTermDays>
            <ARSeaInvoiceTerms>0</ARSeaInvoiceTerms>
            <ARAirInvoiceTermDays>COD</ARAirInvoiceTermDays>
            <ARAirInvoiceTerms>0</ARAirInvoiceTerms>
            <ARInvoiceTerms>COD</ARInvoiceTerms>
            <ARInvoiceTermDays>0</ARInvoiceTermDays>
            <ARTreatDisbursementsAsStandardValue>0.0000</ARTreatDisbursementsAsStandardValue>
            <ARDisbursementInvoiceTerms>COD</ARDisbursementInvoiceTerms>
            <ARDisbursementInvoiceTermDays>0</ARDisbursementInvoiceTermDays>
            <ARDontShowTaxOnDocs>false</ARDontShowTaxOnDocs>
            <ARForeignCurrStatement>false</ARForeignCurrStatement>
            <ARBuyersConsolInvoicingStyle>DEF</ARBuyersConsolInvoicingStyle>
            <ARSellersConsolInvoicingStyle>DEF</ARSellersConsolInvoicingStyle>
            <AROnCreditHold>false</AROnCreditHold>
            <ARAccountAndCreditReviewDue></ARAccountAndCreditReviewDue>
            <ARPreviousChequeDrawer></ARPreviousChequeDrawer>
            <ARPreviousChequeDrawerBank></ARPreviousChequeDrawerBank>
            <ARPreviousChequeDrawerBankBranch></ARPreviousChequeDrawerBankBranch>
            <ARReceiptInvoiceAfterPostingDefault>false</ARReceiptInvoiceAfterPostingDefault>
            <AREftCustomsPaymentMethod></AREftCustomsPaymentMethod>
            <ARWHTApplicable>false</ARWHTApplicable>
            <ARIncludeInwardsWhsConsolidatedInvoice>true</ARIncludeInwardsWhsConsolidatedInvoice>
            <ARIncludeOutwardsWhsConsolidatedInvoice>true</ARIncludeOutwardsWhsConsolidatedInvoice>
            <ARWhsStorageCalcMethod>MAX</ARWhsStorageCalcMethod>
            <CRIsShipsAgencyPrincipal>false</CRIsShipsAgencyPrincipal>
            <ARUseSystemDefaultUplifts>true</ARUseSystemDefaultUplifts>
            <ARExportAirCollectUplift>0.000</ARExportAirCollectUplift>
            <ARExportSeaCollectUplift>0.000</ARExportSeaCollectUplift>
            <ARImportAirCollectUplift>0.000</ARImportAirCollectUplift>
            <ARImportSeaCollectUplift>0.000</ARImportSeaCollectUplift>
            <ARUseSystemDefaultUpliftsMinimums>true</ARUseSystemDefaultUpliftsMinimums>
            <ARExportAirCollectUpliftMinimum>0.0000</ARExportAirCollectUpliftMinimum>
            <ARExportSeaCollectUpliftMinimum>0.0000</ARExportSeaCollectUpliftMinimum>
            <ARImportAirCollectUpliftMinimum>0.0000</ARImportAirCollectUpliftMinimum>
            <ARImportSeaCollectUpliftMinimum>0.0000</ARImportSeaCollectUpliftMinimum>
            <IMUsedBondedWhs>false</IMUsedBondedWhs>
            <APQualityAssured>false</APQualityAssured>
            <APQualityAssuredCheckedDate></APQualityAssuredCheckedDate>
            <ARQualityAssured>false</ARQualityAssured>
            <ARQualityAssuredCheckedDate></ARQualityAssuredCheckedDate>
            <ARWarehouseRatingPeriod>DEF</ARWarehouseRatingPeriod>
            <WhsClientFreeStorageDays>0</WhsClientFreeStorageDays>
            <WhsPeriodicBillingDay>0</WhsPeriodicBillingDay>
            <WhsOverrideFreeStorage>false</WhsOverrideFreeStorage>
            <APCostsSelfBilled>false</APCostsSelfBilled>
            <APPrintContractorForm>false</APPrintContractorForm>
            <ARAllowMultiCurrencyPayment>false</ARAllowMultiCurrencyPayment>
            <ARCreditCardType></ARCreditCardType>
            <ARCreditCardNum></ARCreditCardNum>
            <ARCreditCardAdditionalInfo></ARCreditCardAdditionalInfo>
            <ARCreditCardExpire></ARCreditCardExpire>
            <ARCreditCardHolder></ARCreditCardHolder>
            <ARCreditApproved>true</ARCreditApproved>
            <ARUseSettlementGroupCreditLimit>false</ARUseSettlementGroupCreditLimit>
            <EXBillAgentChargesDirect>false</EXBillAgentChargesDirect>
            <IMBillAgentChargesDirect>false</IMBillAgentChargesDirect>
            <APExternalCreditorCode></APExternalCreditorCode>
            <ARExternalDebtorCode></ARExternalDebtorCode>
            <APCreditAgreedPaymentMethod></APCreditAgreedPaymentMethod>
            <ARCreditAgreedPaymentMethod></ARCreditAgreedPaymentMethod>
            <WhsIncludeReleaseChargesOnShipment>true</WhsIncludeReleaseChargesOnShipment>
            <ARVATConfig>DEF</ARVATConfig>
            <APVATConfig>NON</APVATConfig>
            <RateSecurityGroup></RateSecurityGroup>
            <OrgInvoiceRollupOrGroupCollection>
              <OrgInvoiceRollupOrGroup Action=""MERGE"">
                <PK>27a29287-cd74-4ab9-8871-59c661757806</PK>
                <JobType>ALL</JobType>
                <TransportMode>ALL</TransportMode>
                <ServiceDirection>ALL</ServiceDirection>
                <GroupOrSubTotal>DEF</GroupOrSubTotal>
                <GroupOrSubtotalStyle>DEF</GroupOrSubtotalStyle>
                <InvoiceLineDisplayOption>DEF</InvoiceLineDisplayOption>
                <InvoicePostingStyle>DEF</InvoicePostingStyle>
              </OrgInvoiceRollupOrGroup>
            </OrgInvoiceRollupOrGroupCollection>
            <GlbCompany>
              <Code>EDI</Code>
              <PK>878d7aca-ffc3-49fc-9710-969ca0c0f2ac</PK>
            </GlbCompany>
            <APDefltCurrency TableName=""RefCurrency"">
              <Code>USD</Code>
              <PK>60aae969-b80b-4a40-9b2d-810d3385c76e</PK>
            </APDefltCurrency>
            <ARDDefltCurrency TableName=""RefCurrency"">
              <Code>AUD</Code>
              <PK>53feb23f-a7b0-4e0d-af5d-f6f6e5417399</PK>
            </ARDDefltCurrency>
          </OrgCompanyData>
        </OrgCompanyDataCollection>
        <OrgRateTariffLevelCollection>
          <OrgRateTariffLevel Action=""MERGE"">
            <PK>6745b876-12ce-43e3-bdfb-d22db0682ec3</PK>
            <TariffType>DEF</TariffType>
            <TariffLevel>0</TariffLevel>
            <Mode>ALL</Mode>
            <Direction>ALL</Direction>
            <ExpiryDate>2024-09-03T00:00:00</ExpiryDate>
            <StartDate>2024-09-02T00:00:00</StartDate>
            <GlbCompany>
              <Code>EDI</Code>
              <PK>878d7aca-ffc3-49fc-9710-969ca0c0f2ac</PK>
            </GlbCompany>
          </OrgRateTariffLevel>
        </OrgRateTariffLevelCollection>
        <StmNoteCollection>
          <StmNote Action=""MERGE"">
            <Description>Customs Note Description</Description>
            <NoteData>e1xydGYxXGFuc2lcZGVmZjB7XGZvbnR0Ymx7XGYwXGZuaWxcZmNoYXJzZXQwIE1pY3Jvc29mdCBTYW5zIFNlcmlmO319DQpcdmlld2tpbmQ0XHVjMVxwYXJkXGxhbmczMDgxXGYwXGZzMjAgU3RyaW5nIE9mIERhdGEgc29tZWhvdyByZWxhdGluZyB0byB0aGlzIHRlc3QgMVxwYXINCn0NCg==</NoteData>
            <NoteText></NoteText>
            <NoteType>INT</NoteType>
            <NoteContext>CEL</NoteContext>
            <IsCustomDescription>true</IsCustomDescription>
            <ForceRead>true</ForceRead>
          </StmNote>
          <StmNote Action=""MERGE"">
            <Description>Special Instructions</Description>
            <NoteData></NoteData>
            <NoteText>Special Instructions Note Text Field</NoteText>
            <NoteType>PUB</NoteType>
            <NoteContext>DXW</NoteContext>
            <IsCustomDescription>false</IsCustomDescription>
            <ForceRead>true</ForceRead>
          </StmNote>
        </StmNoteCollection>
        <ClosestPort TableName=""RefUNLOCO"">
          <Code>USHBO</Code>
          <PK>8f4920bb-23f8-4c00-93c4-74d7759bf8f4</PK>
        </ClosestPort>
      </OrgHeader>
    </Organization>
  </Body>
</Native>";

		#endregion

		public void TestImportXMLOfOrgHeaderWithNotes_UpdateVarBinary()
		{
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(OrgHeaderWithNotesXml)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 1 inserts, 0 updates, 0 deletes
OrgMiscServ - 1 inserts, 0 updates, 0 deletes
OrgContact - 1 inserts, 0 updates, 0 deletes
OrgAddress - 1 inserts, 0 updates, 0 deletes
OrgAddressCapability - 1 inserts, 0 updates, 0 deletes
OrgCompanyData - 1 inserts, 0 updates, 0 deletes
OrgInvoiceRollupOrGroup - 1 inserts, 0 updates, 0 deletes
OrgRateTariffLevel - 1 inserts, 0 updates, 0 deletes
StmNote - 2 inserts, 0 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Log Text", expectedLog, manager.GetLogs());
			}

			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(OrgHeaderWithNotesXmlDifferentVarbinaryNote)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 0 inserts, 0 updates, 0 deletes
OrgMiscServ - 0 inserts, 0 updates, 0 deletes
OrgContact - 0 inserts, 0 updates, 0 deletes
OrgAddress - 0 inserts, 0 updates, 0 deletes
OrgAddressCapability - 0 inserts, 0 updates, 0 deletes
OrgCompanyData - 0 inserts, 0 updates, 0 deletes
OrgInvoiceRollupOrGroup - 0 inserts, 0 updates, 0 deletes
OrgRateTariffLevel - 0 inserts, 0 updates, 0 deletes
StmNote - 0 inserts, 1 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("<NoteData> on one StmNote is the only change", expectedLog, manager.GetLogs());
			}

			var createdOrgHeaderList = new BusinessObjectFactory().Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "FROICEHBO"));
			AssertEquals(1, createdOrgHeaderList.Length);
			var notes = createdOrgHeaderList.First().Notes;
			AssertEquals(2, notes.GetAllNotes().Count);

			AssertEquals(1, notes.FindByDescription("Customs Note Description").Length);
			AssertEquals(1, notes.FindByDescription("Special Instructions").Length);
		}

		public void TestImportXMLOfOrgHeaderWithNotes_UpdateNoneVarBinaryNote()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "FOO222";
			orgHeader.OH_FullName = "Mr Foo";
			orgHeader.OH_RL_NKClosestPort = "DE222";
			var address = orgHeader.Addresses.AddNew();
			address.OA_Address1 = "Here";
			address.OA_City = "There";
			address.OA_PostCode = "12";
			address.OA_State = "RP";

			var note = orgHeader.Notes.AddNew();
			note.ST_Description = "Goods Handling Instructions";
			note.ST_NoteDataAsText = "Drop them hard";

			Factory.Save();

			string actualMessage = "";
			var definitionFinder = new DefinitionFinder() { Cache = EntitySetDefinitionCache.GetInstance() };
			var converter = new BusinessObjectToEntityConverter() { DefinitionFinder = definitionFinder };
			var xmlSerializer = new NativeXmlSerializer() { Converter = converter };

			using (var dataStream = xmlSerializer.SerializeToStream(orgHeader))
			using (var reader = new StreamReader(dataStream))
			{
				actualMessage = reader.ReadToEnd();
			}

			var updatedMessage = RemoveElement(actualMessage, "OrgCompanyDataCollection");

			updatedMessage = updatedMessage.Replace(string.Format("<PK>{0}</PK>", note.PK), "");
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(updatedMessage)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 0 inserts, 1 updates, 0 deletes
OrgMiscServ - 0 inserts, 0 updates, 0 deletes
OrgAddress - 0 inserts, 0 updates, 0 deletes
OrgAddressCapability - 0 inserts, 0 updates, 0 deletes
StmNote - 0 inserts, 0 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Log Text", expectedLog, manager.GetLogs());
			}

			var createdOrgHeaderList = new BusinessObjectFactory().Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, "MR FOO"));
			AssertEquals(1, createdOrgHeaderList.Length);
			var notes = createdOrgHeaderList.First().Notes;
			AssertEquals(1, notes.GetAllNotes().Count);

			AssertEquals(1, notes.FindByDescription("Goods Handling Instructions").Length);
		}

		static string RemoveElement(string text, string elementName)
		{
			const string startOpenTag = "<";
			const string startCloseTag = "</";
			const string endTag = ">";

			var startIndex = text.IndexOf(startOpenTag + elementName + endTag, StringComparison.Ordinal);
			string endElement = startCloseTag + elementName + endTag;
			var endIndex = text.IndexOf(endElement, StringComparison.Ordinal);
			if (startIndex != -1 && endIndex != -1)
			{
				return text.Substring(0, startIndex) +
					   text.Substring(endIndex + endElement.Length, text.Length - endIndex - endElement.Length);
			}

			return text;
		}

		#region AddressWithEmptyRelatedPortCodeXML

		const string XML_MergeAddressWithEmptyRelatedPortCode = @"<?xml version=""1.0"" encoding=""utf-8""?>
					<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""1.0"">
            <Header>
               <OwnerCode>CCDB</OwnerCode>
               <EnableCodeMapping>true</EnableCodeMapping>
            </Header>
            <Body>
               <Organization>
                  <OrgHeader Action=""MERGE"">
                     <Code>POLYCODE</Code>
                     <FullName>Polycomp Ltd.</FullName>
                     <OrgAddressCollection>
                        <OrgAddress Action=""MERGE"">
                           <IsActive>true</IsActive>
                           <Code>Polycomp Ltd.</Code>
                           <Language>EN</Language>
                           <Address1>8, Nikolaevska Str.</Address1>
                           <Address2 />
                           <City>Gabrovo</City>
                           <State />
                           <PostCode>5300</PostCode>
                           <Phone>35929718027</Phone>
                           <Fax>35928144130</Fax>
                           <Email />
                           <AIREquipmentNeeded>PSL</AIREquipmentNeeded>
                           <LCLEquipmentNeeded>PSL</LCLEquipmentNeeded>
                           <FCLEquipmentNeeded>WUP</FCLEquipmentNeeded>
                           <RelatedPortCode TableName=""RefUNLOCO"" Action=""MERGE"">
                              <Code></Code>
                           </RelatedPortCode>
                        </OrgAddress>
                     </OrgAddressCollection>
                     <ClosestPort TableName=""RefUNLOCO"">
                        <Code>BGGAV</Code>
                     </ClosestPort>
                  </OrgHeader>
               </Organization>
            </Body>
         </Native>";

		#endregion

		#region AddressWithEmptyRelatedPortCodeMainAddress

		const string XML_MergeAddressWithEmptyRelatedPortCodeMainAddress = @"<?xml version=""1.0"" encoding=""utf-8""?>
					<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""1.0"">
            <Header>
               <OwnerCode>CCDB</OwnerCode>
               <EnableCodeMapping>false</EnableCodeMapping>
            </Header>
            <Body>
               <Organization>
                  <OrgHeader Action=""MERGE"">
                     <Code>POLYCODE</Code>
                     <FullName>Polycomp Ltd.</FullName>
                     <OrgAddressCollection>
                        <OrgAddress Action=""MERGE"">
                           <IsActive>true</IsActive>
                           <Code>Polycomp Ltd.</Code>
                           <Language>EN</Language>
                           <Address1>8, Nikolaevska Str.</Address1>
                           <Address2 />
                           <City>Gabrovo</City>
                           <State />
                           <PostCode>5300</PostCode>
                           <Phone>35929718027</Phone>
                           <Fax>35928144130</Fax>
                           <Email />
                           <AIREquipmentNeeded>PSL</AIREquipmentNeeded>
                           <LCLEquipmentNeeded>PSL</LCLEquipmentNeeded>
                           <FCLEquipmentNeeded>WUP</FCLEquipmentNeeded>
													<OrgAddressCapabilityCollection>
              <OrgAddressCapability Action=""MERGE"">
                <AddressType>OFC</AddressType>
                <IsMainAddress>true</IsMainAddress>
              </OrgAddressCapability>
            </OrgAddressCapabilityCollection>
                           <RelatedPortCode TableName=""RefUNLOCO"" Action=""MERGE"">
                              <Code></Code>
                           </RelatedPortCode>
                        </OrgAddress>
                     </OrgAddressCollection>
                     <ClosestPort TableName=""RefUNLOCO"">
                        <Code>BGGAV</Code>
                     </ClosestPort>
                  </OrgHeader>
               </Organization>
            </Body>
         </Native>";

		#endregion

		#region OrgAddress (Validation Error Suppression)

		public void TestImport_WhenGettingXmlWithoutSuppressValidationErrorElement_ShouldSetItToFalse()
		{
			var payload = Encoding.UTF8.GetBytes(XML_OrgAddress_MissingValidationErrorSuppressionElement);

			using (var stream = new MemoryStream(payload))
			{
				var manager = new ImportServiceManagerForTesting();

				manager.ImportService.Import(stream);

				var addresses = new BusinessObjectFactory()
					.Load<OrgAddress>(new ZQuery(OrgAddressSchema.OA_Code, "FOOSYD"));

				AssertEquals("Should find exactly 1 address after importing.", 1, addresses.Length);

				AssertEquals(
					"Suppress address validation flag should not be set.",
					false,
					addresses.Single().OA_SuppressAddressValidationError);
			}
		}

		const string XML_OrgAddress_MissingValidationErrorSuppressionElement = @"
			<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""1.0"">
				<Header>
					<OwnerCode>CCDB</OwnerCode>
					<EnableCodeMapping>false</EnableCodeMapping>
				</Header>
				<Body>
					<Organization>
						<OrgHeader Action=""MERGE"">
							<Code>FOO</Code>
							<FullName>Foo Inc.</FullName>
							<OrgAddressCollection>
							<OrgAddress Action=""MERGE"">
								<IsActive>true</IsActive>
								<Code>FOOSYD</Code>
								<Language>EN</Language>
								<Address1>72 O'RIORDAN STREET</Address1>
								<Address2 />
								<City>ALEXANDRIA</City>
								<State>NSW</State>
								<PostCode>2015</PostCode>
								<Phone>02 0000 0000</Phone>
								<Fax>02 0000 0000</Fax>
								<Email />
								<AIREquipmentNeeded>PSL</AIREquipmentNeeded>
								<LCLEquipmentNeeded>PSL</LCLEquipmentNeeded>
								<FCLEquipmentNeeded>WUP</FCLEquipmentNeeded>
								<RelatedPortCode TableName=""RefUNLOCO"" Action=""MERGE"">
									<Code>AUSYD</Code>
								</RelatedPortCode>
							</OrgAddress>
							</OrgAddressCollection>
							<ClosestPort TableName=""RefUNLOCO"">
								<Code>AUSYD</Code>
							</ClosestPort>
						</OrgHeader>
					</Organization>
				</Body>
			</Native>";

		public void TestImport_WhenGettingXmlWithDisabledValidationErrorSuppression_ShouldSetItToFalse()
		{
			var payload = Encoding.UTF8.GetBytes(XML_OrgAddress_ValidationErrorSuppressionSetToFalse);

			using (var stream = new MemoryStream(payload))
			{
				var manager = new ImportServiceManagerForTesting();

				manager.ImportService.Import(stream);

				var addresses = new BusinessObjectFactory()
					.Load<OrgAddress>(new ZQuery(OrgAddressSchema.OA_Code, "FOOSYD"));

				AssertEquals("Should find exactly 1 address after importing.", 1, addresses.Length);

				AssertEquals(
					"Suppress address validation flag should not be set.",
					false,
					addresses.Single().OA_SuppressAddressValidationError);
			}
		}

		const string XML_OrgAddress_ValidationErrorSuppressionSetToFalse = @"
			<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""1.0"">
				<Header>
					<OwnerCode>CCDB</OwnerCode>
					<EnableCodeMapping>false</EnableCodeMapping>
				</Header>
				<Body>
					<Organization>
						<OrgHeader Action=""MERGE"">
							<Code>FOO</Code>
							<FullName>Foo Inc.</FullName>
							<OrgAddressCollection>
							<OrgAddress Action=""MERGE"">
								<IsActive>true</IsActive>
								<Code>FOOSYD</Code>
								<Language>EN</Language>
								<Address1>72 O'RIORDAN STREET</Address1>
								<Address2 />
								<City>ALEXANDRIA</City>
								<State>NSW</State>
								<PostCode>2015</PostCode>
								<Phone>02 0000 0000</Phone>
								<Fax>02 0000 0000</Fax>
								<Email />
								<AIREquipmentNeeded>PSL</AIREquipmentNeeded>
								<LCLEquipmentNeeded>PSL</LCLEquipmentNeeded>
								<FCLEquipmentNeeded>WUP</FCLEquipmentNeeded>
								<RelatedPortCode TableName=""RefUNLOCO"" Action=""MERGE"">
									<Code>AUSYD</Code>
								</RelatedPortCode>
								<SuppressAddressValidationError>false</SuppressAddressValidationError>
							</OrgAddress>
							</OrgAddressCollection>
							<ClosestPort TableName=""RefUNLOCO"">
								<Code>AUSYD</Code>
							</ClosestPort>
						</OrgHeader>
					</Organization>
				</Body>
			</Native>";

		public void TestImport_WhenGettingXmlWithEnabledValidationErrorSuppression_ShouldSetItToTrue()
		{
			var payload = Encoding.UTF8.GetBytes(XML_OrgAddress_ValidationErrorSuppressionSetToTrue);

			using (var stream = new MemoryStream(payload))
			{
				var manager = new ImportServiceManagerForTesting();

				manager.ImportService.Import(stream);

				var addresses = new BusinessObjectFactory()
					.Load<OrgAddress>(new ZQuery(OrgAddressSchema.OA_Code, "FOOSYD"));

				AssertEquals("Should find exactly 1 address after importing.", 1, addresses.Length);

				AssertEquals("Suppress address validation flag should be set.",
					true,
					addresses.Single().OA_SuppressAddressValidationError);
			}
		}

		const string XML_OrgAddress_ValidationErrorSuppressionSetToTrue = @"
			<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""1.0"">
				<Header>
					<OwnerCode>CCDB</OwnerCode>
					<EnableCodeMapping>false</EnableCodeMapping>
				</Header>
				<Body>
					<Organization>
						<OrgHeader Action=""MERGE"">
							<Code>FOO</Code>
							<FullName>Foo Inc.</FullName>
							<OrgAddressCollection>
							<OrgAddress Action=""MERGE"">
								<IsActive>true</IsActive>
								<Code>FOOSYD</Code>
								<Language>EN</Language>
								<Address1>72 O'RIORDAN STREET</Address1>
								<Address2 />
								<City>ALEXANDRIA</City>
								<State>NSW</State>
								<PostCode>2015</PostCode>
								<Phone>02 0000 0000</Phone>
								<Fax>02 0000 0000</Fax>
								<Email />
								<AIREquipmentNeeded>PSL</AIREquipmentNeeded>
								<LCLEquipmentNeeded>PSL</LCLEquipmentNeeded>
								<FCLEquipmentNeeded>WUP</FCLEquipmentNeeded>
								<RelatedPortCode TableName=""RefUNLOCO"" Action=""MERGE"">
									<Code>AUSYD</Code>
								</RelatedPortCode>
								<SuppressAddressValidationError>true</SuppressAddressValidationError>
							</OrgAddress>
							</OrgAddressCollection>
							<ClosestPort TableName=""RefUNLOCO"">
								<Code>AUSYD</Code>
							</ClosestPort>
						</OrgHeader>
					</Organization>
				</Body>
			</Native>";

		#endregion

		#region OrgWithEmptyLookupValueToDelete

		public void TestClosestPortCodeDeletedOnExistingEntryIfEmptyElement()
		{
			DataRegistry.Instance.CanUserEditOrganisationCode = true;

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ORGCODE123";
			org.OH_RL_NKClosestPort = "AUSYD";
			Factory.Save();

			#region XMLRemoveClosestPort
			string xMLRemoveClosestPort = string.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""1.0"">
  <Header>
    <EnableCodeMapping>false</EnableCodeMapping>
  </Header>
  <Body>
    <Organization version=""1.0"">
      <OrgHeader Action=""MERGE"">
		<PK>{0}</PK>
        <Code>ORGCODE123</Code>
		<ClosestPort />
      </OrgHeader>
    </Organization>
  </Body>
</Native>", org.PK);

			#endregion

			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(xMLRemoveClosestPort)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);
				var insertLog = manager.GetLogs();
				AssertContains("OrgHeader should be updated", "OrgHeader - 0 inserts, 1 updates, 0 deletes", insertLog);
			}

			var factory = new BusinessObjectFactory();
			var updatedOrg = factory.Load<OrgHeader>(org.PK);
			AssertEquals("OrgHeader should no longer have a closest port code", ZString.Empty, updatedOrg.OH_RL_NKClosestPort);
		}

		public void TestClosestPortCodeDeletedOnExistingEntryIfEmptyCode()
		{
			DataRegistry.Instance.CanUserEditOrganisationCode = true;

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ORGCODE123";
			org.OH_RL_NKClosestPort = "AUSYD";
			Factory.Save();

			#region XMLRemoveClosestPort
			string xMLRemoveClosestPort = string.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""1.0"">
  <Header>
    <EnableCodeMapping>false</EnableCodeMapping>
  </Header>
  <Body>
    <Organization version=""1.0"">
      <OrgHeader Action=""MERGE"">
		<PK>{0}</PK>
        <Code>ORGCODE123</Code>
		<ClosestPort Action=""MERGE"">
			<Code></Code>
		</ClosestPort>
      </OrgHeader>
    </Organization>
  </Body>
</Native>", org.PK);

			#endregion

			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(xMLRemoveClosestPort)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);
				var insertLog = manager.GetLogs();
				AssertContains("OrgHeader should be updated", "OrgHeader - 0 inserts, 1 updates, 0 deletes", insertLog);
			}

			var factory = new BusinessObjectFactory();
			var updatedOrg = factory.Load<OrgHeader>(org.PK);
			AssertEquals("OrgHeader should no longer have a closest port code", ZString.Empty, updatedOrg.OH_RL_NKClosestPort);
		}

		#endregion

		#region OrgContact
		const string OrgContactAndCusBondDetailPlainTextPasswordImportXML = @"<?xml version='1.0' encoding='utf-8'?>
<Native xmlns='http://www.cargowise.com/Schemas/Native/2011/11' version='2.0'>
  <Header>
    <OwnerCode>CARGOWCHI</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Organization version='2.0'>
      <OrgHeader Action='MERGE'>
        <PK>f205e852-9475-4fcc-bf12-e6f70d0f2951</PK>
        <Code>TESTORG</Code>
        <Language>EN</Language>
        <SystemLastEditTimeUtc>2016-08-31T09:15:00</SystemLastEditTimeUtc>
        <SystemCreateTimeUtc>2016-08-31T09:14:00</SystemCreateTimeUtc>
        <ScreeningStatus>UNK</ScreeningStatus>
        <IsActive>true</IsActive>
        <FullName>TESTORG</FullName>
        <IsConsignee>true</IsConsignee>
        <IsConsignor>false</IsConsignor>
        <IsTransportClient>false</IsTransportClient>
        <IsWarehouseClient>false</IsWarehouseClient>
        <IsForwarder>false</IsForwarder>
        <IsShippingProvider>false</IsShippingProvider>
        <IsAirWholesaler>false</IsAirWholesaler>
        <IsSeaWholesaler>false</IsSeaWholesaler>
        <IsRailProvider>false</IsRailProvider>
        <IsLineHaulProvider>false</IsLineHaulProvider>
        <IsMiscFreightServices>false</IsMiscFreightServices>
        <IsAirCTO>false</IsAirCTO>
        <IsAirLine>false</IsAirLine>
        <IsBroker>false</IsBroker>
        <IsContainerYard>false</IsContainerYard>
        <IsLocalTransport>false</IsLocalTransport>
        <IsPackDepot>false</IsPackDepot>
        <IsSeaCTO>false</IsSeaCTO>
        <IsShippingLine>false</IsShippingLine>
        <IsUnpackDepot>false</IsUnpackDepot>
        <IsRailHead>false</IsRailHead>
        <IsRoadFreightDepot>false</IsRoadFreightDepot>
        <IsShippingConsortium>false</IsShippingConsortium>
        <IsFumigationContractor>false</IsFumigationContractor>
        <IsNationalAccount>false</IsNationalAccount>
        <IsSalesLead>false</IsSalesLead>
        <IsUserFlag13>false</IsUserFlag13>
        <IsUserFlag12>false</IsUserFlag12>
        <IsUserFlag14>false</IsUserFlag14>
        <IsUserFlag11>false</IsUserFlag11>
        <IsCompetitor>false</IsCompetitor>
        <IsTempAccount>false</IsTempAccount>
        <IsPersonalEffectsAccount>false</IsPersonalEffectsAccount>
        <IsUserFlag1>false</IsUserFlag1>
        <IsUserFlag2>false</IsUserFlag2>
        <IsUserFlag3>false</IsUserFlag3>
        <IsUserFlag4>false</IsUserFlag4>
        <IsUserFlag5>false</IsUserFlag5>
        <IsUserFlag6>false</IsUserFlag6>
        <IsUserFlag7>false</IsUserFlag7>
        <IsUserFlag8>false</IsUserFlag8>
        <IsUserFlag9>false</IsUserFlag9>
        <IsUserFlag10>false</IsUserFlag10>
        <IsGlobalAccount>false</IsGlobalAccount>
        <IsDistributionCentre>false</IsDistributionCentre>
        <IsUserFlag15>false</IsUserFlag15>
        <IsUserFlag16>false</IsUserFlag16>
        <IsUserFlag17>false</IsUserFlag17>
        <IsUserFlag18>false</IsUserFlag18>
        <IsUserFlag19>false</IsUserFlag19>
        <IsUserFlag20>false</IsUserFlag20>
        <IsUserFlag21>false</IsUserFlag21>
        <IsUserFlag22>false</IsUserFlag22>
        <IsUserFlag23>false</IsUserFlag23>
        <IsUserFlag24>false</IsUserFlag24>
        <IsControllingCustomer>false</IsControllingCustomer>
        <IsControllingAgent>false</IsControllingAgent>        
        <OrgContactCollection>
          <OrgContact Action='MERGE'>
            <PK>2daeaeda-1ad9-44d3-918b-14a64fb6cd5c</PK>
            <ContactName>TestContact</ContactName>
            <Salutation></Salutation>
            <Language>EN</Language>
            <NotifyMode>EML</NotifyMode>
            <Title></Title>
            <JobCategory></JobCategory>
            <Phone>2 8545 5624</Phone>
            <PhoneExtension></PhoneExtension>
            <Fax></Fax>
            <Mobile></Mobile>
            <HomePhone></HomePhone>
            <Pager></Pager>
            <OtherPhone></OtherPhone>
            <Password>PlainText</Password>
            <Birthday></Birthday>
            <YearJoinedIndustry></YearJoinedIndustry>
            <YearJoinedCompany></YearJoinedCompany>
            <ContactSource></ContactSource>
            <DetailsVerified></DetailsVerified>
            <WebContractSignedDate></WebContractSignedDate>
            <Gender></Gender>
            <ProfilePhoto></ProfilePhoto>
            <IsActive>true</IsActive>
            <HashedPassword></HashedPassword>
            <HashedPasswordIterations>0</HashedPasswordIterations>
            <WebAccessEnabled>false</WebAccessEnabled>
            <AttachmentType>PDF</AttachmentType>
            <Email></Email>
            <PersonalInfo></PersonalInfo>
            <AddressOverride TableName='OrgHeader' />
            <OrgAddress />
            <Nationality TableName='RefCountry'>
              <Code></Code>
            </Nationality>
            <GlbPerson />
          </OrgContact>			
        </OrgContactCollection>
        <OrgAddressCollection>
          <OrgAddress Action='MERGE'>
            <PK>11f554c4-39bc-4772-ae95-5c753fa57cb4</PK>
            <Code>111111</Code>
            <Language>EN</Language>
            <Address1>111111</Address1>
            <Address2></Address2>
            <State></State>
            <PostCode></PostCode>
            <Phone>2 8545 5624</Phone>
            <Fax></Fax>
            <Mobile></Mobile>
            <PickupFromTimeOnly></PickupFromTimeOnly>
            <PickupToTimeOnly></PickupToTimeOnly>
            <DeliverFromTimeOnly></DeliverFromTimeOnly>
            <DeliverToTimeOnly></DeliverToTimeOnly>
            <DoNotAttendFrom></DoNotAttendFrom>
            <DoNotAttendTo></DoNotAttendTo>
            <ContainerHandling></ContainerHandling>
            <AccessPoint></AccessPoint>
            <LabourRequired></LabourRequired>
            <CommunicationRequired></CommunicationRequired>
            <Dock_Height></Dock_Height>
            <FCLEquipmentNeeded>WUP</FCLEquipmentNeeded>
            <LCLEquipmentNeeded>PSL</LCLEquipmentNeeded>
            <AIREquipmentNeeded>PSL</AIREquipmentNeeded>
            <Latitude>0.00000</Latitude>
            <Longitude>0.00000</Longitude>
            <IsActive>true</IsActive>
            <CompanyNameOverride></CompanyNameOverride>
            <DockLeveler>false</DockLeveler>
            <ForkLift>false</ForkLift>
            <PalletJack>false</PalletJack>
            <Email></Email>
            <DeliveryRoute></DeliveryRoute>
            <DeliveryRouteSequence>0</DeliveryRouteSequence>
            <UseCumulativeFreeWaitingTime>false</UseCumulativeFreeWaitingTime>
            <ValidationStatus>NYV</ValidationStatus>
            <AddressMap></AddressMap>
            <AuthorityToLeave>DEF</AuthorityToLeave>
            <City></City>
            <OtherWarehouseFacilities></OtherWarehouseFacilities>
            <LoadingUnloadingConstraints></LoadingUnloadingConstraints>
            <GroupNumber>0</GroupNumber>
            <OrgAddressCapabilityCollection>
              <OrgAddressCapability Action='MERGE'>
                <PK>e5cb913c-23e7-4b15-ae4a-4d5318c4ea30</PK>
                <AddressType>OFC</AddressType>
                <IsMainAddress>true</IsMainAddress>
              </OrgAddressCapability>
            </OrgAddressCapabilityCollection>
            <RelatedPortCode TableName='RefUNLOCO'>
              <Code>AUSYD</Code>
              <PK>ad87872e-96b8-4c9a-bc0b-6a4088247a64</PK>
            </RelatedPortCode>
            <CountryCode TableName='RefCountry'>
              <Code>AU</Code>
              <PK>e4eb6e97-aa78-4c46-bf86-35b7fbb5fb0e</PK>
            </CountryCode>
          </OrgAddress>
        </OrgAddressCollection>
        <CusBondDetailCollection>
          <CusBondDetail Action='MERGE'>
            <ActivityCode>1</ActivityCode>
            <BondType>8</BondType>
            <BondNumber></BondNumber>
            <SuretyCode>098</SuretyCode>
            <BondAmount>20000.0000</BondAmount>
            <BondEffectiveDate>2018-01-28T00:00:00</BondEffectiveDate>
            <BondExpiryDate></BondExpiryDate>
            <BondFiledPort></BondFiledPort>
            <ApplicationCode>USA</ApplicationCode>
            <Password>PTXT</Password>
            <Status>HSF</Status>
          </CusBondDetail>
        </CusBondDetailCollection>
        <ClosestPort TableName='RefUNLOCO'>
          <Code>AUSYD</Code>
          <PK>ad87872e-96b8-4c9a-bc0b-6a4088247a64</PK>
        </ClosestPort>
      </OrgHeader>
    </Organization>
  </Body>
</Native>";

		public void TestOrgContactAndCusBondDetailPasswordIsPlainTextAfterImport()
		{
			OrgContactAndCusBondDetailPlainTextPasswordImportXML.ImportNativeXmlReturningLog();
			var organisation = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, "TESTORG"));
			AssertNotNull(organisation);
			AssertEquals("1 contacts imported", 1, organisation.Contacts.Count);
			AssertEquals("1 contacts imported", 1, organisation.CusBondDetails.Count);
			AssertEquals("CusBondDetail.Password should not be encrypted.", "PTXT", organisation.CusBondDetails[0].PW_Password);
		}

		#endregion

		public void TestShouldNotThrowParentReferenceMismatchExceptionOnExternalAssociation()
		{
			var testBranch = Factory.NewWithValidTestData<GlbBranch>();
			testBranch.GB_GC = new Guid("878D7ACA-FFC3-49FC-9710-969CA0C0F2AC");
			testBranch.GB_Code = "AAA";
			var testBranch2 = Factory.NewWithValidTestData<GlbBranch>();
			testBranch2.GB_GC = new Guid("878D7ACA-FFC3-49FC-9710-969CA0C0F2AC");
			testBranch2.GB_Code = "BBB";
			Factory.Save();

			var orgCompanyData = Factory.LoadTop1<OrgCompanyData>(new ZQuery(OrgCompanyDataSchema.OB_GB_ControllingBranch, testBranch.PK));
			AssertNull(orgCompanyData);

			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(OrgHeaderWithControllingBranchXMLImport)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);
				var insertLog = manager.GetLogs();
				AssertContains("OrgHeader should be inserted", "OrgHeader - 1 inserts, 0 updates, 0 deletes", insertLog);
				orgCompanyData = Factory.LoadTop1<OrgCompanyData>(new ZQuery(OrgCompanyDataSchema.OB_GB_ControllingBranch, testBranch.PK));
				AssertNotNull(orgCompanyData);
			}

			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(OrgHeaderWithControllingBranchXMLUpdate)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);
				var updateLog = manager.GetLogs();
				AssertContains("OrgHeader should be found", "OrgHeader - 0 inserts, 0 updates, 0 deletes", updateLog);
				AssertContains("OrgCompanyData should be updated", "OrgCompanyData - 0 inserts, 1 updates, 0 deletes", updateLog);
				var factory = new BusinessObjectFactory();
				orgCompanyData = factory.Load<OrgCompanyData>(orgCompanyData.PK);
				AssertEquals("BBB", orgCompanyData.ControllingBranch.GB_Code);
			}
		}

		#region OrgHeaderWithControllingBranchXML
		const string OrgHeaderWithControllingBranchXMLImport = @"<?xml version=""1.0""?>
<Native xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://www.cargowise.com/Schemas/Native"">
  <Header>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Organization version=""2.0"">
      <OrgHeader Action=""Merge"">
        <PK>38582927-babc-4cb6-9c38-1f00364c6b01</PK>
        <Code>6400557892</Code>
        <IsActive>true</IsActive>
        <FullName>IVM SRL</FullName>
        <IsConsignee>true</IsConsignee>
        <IsConsignor>true</IsConsignor>
        <Language>IT-IT</Language>
        <OrgAddressCollection>
          <OrgAddress Action=""Merge"">
            <PK>52fac646-5eac-42a1-8ded-4fb74866d9c4</PK>
            <IsActive>true</IsActive>
            <Code>[1] Via Toscana 2/a</Code>
            <Language>EN</Language>
            <CompanyNameOverride>IVM SRL</CompanyNameOverride>
            <Address1>Via Toscana 2/a</Address1>
            <Address2 />
            <City>Padova</City>
            <State>PD</State>
            <PostCode>35127</PostCode>
            <AIREquipmentNeeded>PSL</AIREquipmentNeeded>
            <FCLEquipmentNeeded>WUP</FCLEquipmentNeeded>
            <LCLEquipmentNeeded>PSL</LCLEquipmentNeeded>
            <OrgAddressCapabilityCollection>
              <OrgAddressCapability Action=""Merge"">
                <PK>84220cd3-f5f5-4fc6-bfef-5f6d5ce148b4</PK>
                <AddressType>OFC</AddressType>
                <IsMainAddress>true</IsMainAddress>
              </OrgAddressCapability>
            </OrgAddressCapabilityCollection>
            <RelatedPortCode TableName=""RefUNLOCO"">
              <Code>ITPDA</Code>
            </RelatedPortCode>
          </OrgAddress>
          <OrgAddress Action=""Merge"">
            <PK>df8986c5-0a10-4624-abb3-96895578b983</PK>
            <IsActive>true</IsActive>
            <Code>[4] Via Toscana 2/a</Code>
            <Language>IT-IT</Language>
            <CompanyNameOverride>IVM SRL</CompanyNameOverride>
            <Address1>Via Toscana 2/a</Address1>
            <Address2 />
            <City>Padova</City>
            <State>PD</State>
            <PostCode>35127</PostCode>
            <AIREquipmentNeeded>PSL</AIREquipmentNeeded>
            <FCLEquipmentNeeded>WUP</FCLEquipmentNeeded>
            <LCLEquipmentNeeded>PSL</LCLEquipmentNeeded>
            <OrgAddressCapabilityCollection>
              <OrgAddressCapability Action=""Merge"">
                <PK>5d385639-3d8f-4377-ace3-fa41fba36a3f</PK>
                <AddressType>SQM</AddressType>
                <IsMainAddress>true</IsMainAddress>
              </OrgAddressCapability>
            </OrgAddressCapabilityCollection>
            <RelatedPortCode TableName=""RefUNLOCO"">
              <Code>ITPDA</Code>
            </RelatedPortCode>
          </OrgAddress>
        </OrgAddressCollection>
        <OrgCompanyDataCollection>
          <OrgCompanyData Action=""Merge"">
            <PK>d1f7f432-3f96-4410-8953-c05b618154c7</PK>
            <IsDebtor>true</IsDebtor>
            <IsCreditor>false</IsCreditor>
            <AROnCreditHold>false</AROnCreditHold>
            <ARUseSystemDefaultUplifts>true</ARUseSystemDefaultUplifts>
            <ARUseSystemDefaultUpliftsMinimums>true</ARUseSystemDefaultUpliftsMinimums>
            <ARCreditApproved>true</ARCreditApproved>
            <APPaymentTerms>COD</APPaymentTerms>
            <APPaymentTermDays>0</APPaymentTermDays>
            <ARCreditLimit>1250.0</ARCreditLimit>
            <ARInvoiceTerms>INV</ARInvoiceTerms>
            <ARInvoiceTermDays>15</ARInvoiceTermDays>
            <ARDisbursementInvoiceTerms>COD</ARDisbursementInvoiceTerms>
            <ARDisbursementInvoiceTermDays>0</ARDisbursementInvoiceTermDays>
            <APVATConfig>NON</APVATConfig>
            <ARVATConfig>DEF</ARVATConfig>
            <OrgInvoiceTypeCollection />
            <ARDebtorGroup TableName=""OrgDebtorGroup"">
              <Code>TPY</Code>
            </ARDebtorGroup>
            <ControllingBranch>
              <Code>AAA</Code>
            </ControllingBranch>
            <GlbCompany>
              <Code>EDI</Code>
              <PK>878D7ACA-FFC3-49FC-9710-969CA0C0F2AC</PK>
            </GlbCompany>
          </OrgCompanyData>
        </OrgCompanyDataCollection>
        <OrgCusCodeCollection>
          <OrgCusCode Action=""Merge"">
            <PK>e5da1bbc-7500-4114-a36c-6ed82d27d683</PK>
            <CustomsRegNo>02686760287</CustomsRegNo>
            <CodeType>IVA</CodeType>
            <CountryDefault>false</CountryDefault>
            <CodeCountry TableName=""RefCountry"">
              <Code>IT</Code>
              <PK>1b1b648c-2f9d-4dfe-8787-084c3fb124aa</PK>
            </CodeCountry>
          </OrgCusCode>
        </OrgCusCodeCollection>
        <ClosestPort TableName=""RefUNLOCO"">
          <Code>ITPDA</Code>
        </ClosestPort>
      </OrgHeader>
    </Organization>
  </Body>
</Native>";

		const string OrgHeaderWithControllingBranchXMLUpdate = @"<?xml version=""1.0""?>
<Native xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://www.cargowise.com/Schemas/Native"">
  <Header>
    <EnableCodeMapping>false</EnableCodeMapping>
  </Header>
  <Body>
    <Organization version=""2.0"">
      <OrgHeader Action=""Merge"">
        <PK>38582927-babc-4cb6-9c38-1f00364c6b01</PK>
        <Code>6400557892</Code>
        <IsActive>true</IsActive>
        <FullName>IVM SRL</FullName>
        <IsConsignee>true</IsConsignee>
        <IsConsignor>true</IsConsignor>
        <Language>IT-IT</Language>
        <OrgAddressCollection>
          <OrgAddress Action=""Merge"">
            <PK>52fac646-5eac-42a1-8ded-4fb74866d9c4</PK>
            <IsActive>true</IsActive>
            <Code>[1] Via Toscana 2/a</Code>
            <Language>EN</Language>
            <CompanyNameOverride>IVM SRL</CompanyNameOverride>
            <Address1>Via Toscana 2/a</Address1>
            <Address2 />
            <City>Padova</City>
            <State>PD</State>
            <PostCode>35127</PostCode>
            <AIREquipmentNeeded>PSL</AIREquipmentNeeded>
            <FCLEquipmentNeeded>WUP</FCLEquipmentNeeded>
            <LCLEquipmentNeeded>PSL</LCLEquipmentNeeded>
            <OrgAddressCapabilityCollection>
              <OrgAddressCapability Action=""Merge"">
                <PK>84220cd3-f5f5-4fc6-bfef-5f6d5ce148b4</PK>
                <AddressType>OFC</AddressType>
                <IsMainAddress>true</IsMainAddress>
              </OrgAddressCapability>
            </OrgAddressCapabilityCollection>
            <RelatedPortCode TableName=""RefUNLOCO"">
              <Code>ITPDA</Code>
            </RelatedPortCode>
          </OrgAddress>
          <OrgAddress Action=""Merge"">
            <PK>df8986c5-0a10-4624-abb3-96895578b983</PK>
            <IsActive>true</IsActive>
            <Code>[4] Via Toscana 2/a</Code>
            <Language>IT-IT</Language>
            <CompanyNameOverride>IVM SRL</CompanyNameOverride>
            <Address1>Via Toscana 2/a</Address1>
            <Address2 />
            <City>Padova</City>
            <State>PD</State>
            <PostCode>35127</PostCode>
            <AIREquipmentNeeded>PSL</AIREquipmentNeeded>
            <FCLEquipmentNeeded>WUP</FCLEquipmentNeeded>
            <LCLEquipmentNeeded>PSL</LCLEquipmentNeeded>
            <OrgAddressCapabilityCollection>
              <OrgAddressCapability Action=""Merge"">
                <PK>5d385639-3d8f-4377-ace3-fa41fba36a3f</PK>
                <AddressType>SQM</AddressType>
                <IsMainAddress>true</IsMainAddress>
              </OrgAddressCapability>
            </OrgAddressCapabilityCollection>
            <RelatedPortCode TableName=""RefUNLOCO"">
              <Code>ITPDA</Code>
            </RelatedPortCode>
          </OrgAddress>
        </OrgAddressCollection>
        <OrgCompanyDataCollection>
          <OrgCompanyData Action=""Merge"">
            <PK>d1f7f432-3f96-4410-8953-c05b618154c7</PK>
            <IsDebtor>true</IsDebtor>
            <IsCreditor>false</IsCreditor>
            <AROnCreditHold>false</AROnCreditHold>
            <ARUseSystemDefaultUplifts>true</ARUseSystemDefaultUplifts>
            <ARUseSystemDefaultUpliftsMinimums>true</ARUseSystemDefaultUpliftsMinimums>
            <ARCreditApproved>true</ARCreditApproved>
            <APPaymentTerms>COD</APPaymentTerms>
            <APPaymentTermDays>0</APPaymentTermDays>
            <ARCreditLimit>1250.0</ARCreditLimit>
            <ARInvoiceTerms>INV</ARInvoiceTerms>
            <ARInvoiceTermDays>15</ARInvoiceTermDays>
            <ARDisbursementInvoiceTerms>COD</ARDisbursementInvoiceTerms>
            <ARDisbursementInvoiceTermDays>0</ARDisbursementInvoiceTermDays>
            <APVATConfig>NON</APVATConfig>
            <ARVATConfig>DEF</ARVATConfig>
            <OrgInvoiceTypeCollection />
            <ARDebtorGroup TableName=""OrgDebtorGroup"">
              <Code>TPY</Code>
            </ARDebtorGroup>
            <ControllingBranch>
              <Code>BBB</Code>
            </ControllingBranch>
            <GlbCompany>
              <Code>EDI</Code>
              <PK>878D7ACA-FFC3-49FC-9710-969CA0C0F2AC</PK>
            </GlbCompany>
          </OrgCompanyData>
        </OrgCompanyDataCollection>
        <OrgCusCodeCollection>
          <OrgCusCode Action=""Merge"">
            <PK>e5da1bbc-7500-4114-a36c-6ed82d27d683</PK>
            <CustomsRegNo>02686760287</CustomsRegNo>
            <CodeType>IVA</CodeType>
            <CountryDefault>false</CountryDefault>
            <CodeCountry TableName=""RefCountry"">
              <Code>IT</Code>
              <PK>1b1b648c-2f9d-4dfe-8787-084c3fb124aa</PK>
            </CodeCountry>
          </OrgCusCode>
        </OrgCusCodeCollection>
        <ClosestPort TableName=""RefUNLOCO"">
          <Code>ITPDA</Code>
        </ClosestPort>
      </OrgHeader>
    </Organization>
  </Body>
</Native>";
		#endregion

		#region PhoneNumber in OrgContact

		public void TestPhoneNumberInOrgContact_OrganizationAddress_ShouldBeStandardizedAfterImport()
		{
			var factory = new BusinessObjectFactory();
			AssertPhoneNumberStandardization(factory, "AU", PhoneNumberWithValidAUFormatXML);
		}

		public void TestPhoneNumberInOrgContact_BranchAddress_ShouldBeStandardizedAfterImport()
		{
			var factory = new BusinessObjectFactory();
			AssertPhoneNumberStandardization(factory, "CN", PhoneNumberWithValidCHFormatInContactBranchAddressXML);
		}

		[UseSnapshotProtection]
		public void TestPhoneNumberInOrgContact_OverrideOrganization_ShouldBeStandardizedAfterImport()
		{
			var factory = new BusinessObjectFactory();
			var org = factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "TST";
			var address = org.Addresses[0];
			address.Address1 = "tttt";
			address.OA_RN_NKCountryCode = "CN";
			factory.Save();

			AssertPhoneNumberStandardization(factory, "CN", PhoneNumberWithValidCHFormatInContactOverriderOrganizationXML);
		}

		void AssertPhoneNumberStandardization(BusinessObjectFactory factory, string countryCode, string xml)
		{
			var phoneNumberFormatter = new PhoneNumberFormatter();

			var s = xml.ImportNativeXmlReturningLog();
			var organisation = factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, "TESTPHONE"));
			AssertNotNull(organisation);
			AssertEquals("2 contacts imported", 2, organisation.Contacts.Count);

			AssertEquals("Phone number in main address should be formatted.", phoneNumberFormatter.FormatE164(organisation.Addresses[0].OA_Phone, countryCode), organisation.Addresses[0].OA_Phone);
			foreach (OrgContact contact in organisation.Contacts)
			{
				if (contact.OC_ContactName == "ValidPhone")
				{
					AssertEquals("Phone number in contact should be formatted.", phoneNumberFormatter.FormatE164(contact.OC_Phone, countryCode), contact.OC_Phone);
				}
				else if (contact.OC_ContactName == "InvalidPhone")
				{
					AssertEquals("Phone number in contact should not be formatted.", "1234", contact.OC_Phone);
				}
			}
		}

		const string PhoneNumberWithValidAUFormatXML = @"<?xml version='1.0' encoding='utf-8'?>
<Native xmlns='http://www.cargowise.com/Schemas/Native/2011/11' version='2.0'>
  <Header>
    <OwnerCode>CARGOWCHI</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Organization version='2.0'>
      <OrgHeader Action='MERGE'>
        <PK>f205e852-9475-4fcc-bf12-e6f70d0f2951</PK>
        <Code>TESTPHONE</Code>
        <Language>EN</Language>
        <SystemLastEditTimeUtc>2016-08-31T09:15:00</SystemLastEditTimeUtc>
        <SystemCreateTimeUtc>2016-08-31T09:14:00</SystemCreateTimeUtc>
        <ScreeningStatus>UNK</ScreeningStatus>
        <IsActive>true</IsActive>
        <FullName>TESTPHONE</FullName>
        <IsConsignee>true</IsConsignee>
        <IsConsignor>false</IsConsignor>
        <IsTransportClient>false</IsTransportClient>
        <IsWarehouseClient>false</IsWarehouseClient>
        <IsForwarder>false</IsForwarder>
        <IsShippingProvider>false</IsShippingProvider>
        <IsAirWholesaler>false</IsAirWholesaler>
        <IsSeaWholesaler>false</IsSeaWholesaler>
        <IsRailProvider>false</IsRailProvider>
        <IsLineHaulProvider>false</IsLineHaulProvider>
        <IsMiscFreightServices>false</IsMiscFreightServices>
        <IsAirCTO>false</IsAirCTO>
        <IsAirLine>false</IsAirLine>
        <IsBroker>false</IsBroker>
        <IsContainerYard>false</IsContainerYard>
        <IsLocalTransport>false</IsLocalTransport>
        <IsPackDepot>false</IsPackDepot>
        <IsSeaCTO>false</IsSeaCTO>
        <IsShippingLine>false</IsShippingLine>
        <IsUnpackDepot>false</IsUnpackDepot>
        <IsRailHead>false</IsRailHead>
        <IsRoadFreightDepot>false</IsRoadFreightDepot>
        <IsShippingConsortium>false</IsShippingConsortium>
        <IsFumigationContractor>false</IsFumigationContractor>
        <IsNationalAccount>false</IsNationalAccount>
        <IsSalesLead>false</IsSalesLead>
        <IsUserFlag13>false</IsUserFlag13>
        <IsUserFlag12>false</IsUserFlag12>
        <IsUserFlag14>false</IsUserFlag14>
        <IsUserFlag11>false</IsUserFlag11>
        <IsCompetitor>false</IsCompetitor>
        <IsTempAccount>false</IsTempAccount>
        <IsPersonalEffectsAccount>false</IsPersonalEffectsAccount>
        <IsUserFlag1>false</IsUserFlag1>
        <IsUserFlag2>false</IsUserFlag2>
        <IsUserFlag3>false</IsUserFlag3>
        <IsUserFlag4>false</IsUserFlag4>
        <IsUserFlag5>false</IsUserFlag5>
        <IsUserFlag6>false</IsUserFlag6>
        <IsUserFlag7>false</IsUserFlag7>
        <IsUserFlag8>false</IsUserFlag8>
        <IsUserFlag9>false</IsUserFlag9>
        <IsUserFlag10>false</IsUserFlag10>
        <IsGlobalAccount>false</IsGlobalAccount>
        <IsDistributionCentre>false</IsDistributionCentre>
        <IsUserFlag15>false</IsUserFlag15>
        <IsUserFlag16>false</IsUserFlag16>
        <IsUserFlag17>false</IsUserFlag17>
        <IsUserFlag18>false</IsUserFlag18>
        <IsUserFlag19>false</IsUserFlag19>
        <IsUserFlag20>false</IsUserFlag20>
        <IsUserFlag21>false</IsUserFlag21>
        <IsUserFlag22>false</IsUserFlag22>
        <IsUserFlag23>false</IsUserFlag23>
        <IsUserFlag24>false</IsUserFlag24>
        <IsControllingCustomer>false</IsControllingCustomer>
        <IsControllingAgent>false</IsControllingAgent>        
        <OrgContactCollection>
          <OrgContact Action='MERGE'>
            <PK>2daeaeda-1ad9-44d3-918b-14a64fb6cd5c</PK>
            <ContactName>ValidPhone</ContactName>
            <Salutation></Salutation>
            <Language>EN</Language>
            <NotifyMode>EML</NotifyMode>
            <Title></Title>
            <JobCategory></JobCategory>
            <Phone>2 8545 5624</Phone>
            <PhoneExtension></PhoneExtension>
            <Fax></Fax>
            <Mobile></Mobile>
            <HomePhone></HomePhone>
            <Pager></Pager>
            <OtherPhone></OtherPhone>
            <Password></Password>
            <Birthday></Birthday>
            <YearJoinedIndustry></YearJoinedIndustry>
            <YearJoinedCompany></YearJoinedCompany>
            <ContactSource></ContactSource>
            <DetailsVerified></DetailsVerified>
            <WebContractSignedDate></WebContractSignedDate>
            <Gender></Gender>
            <ProfilePhoto></ProfilePhoto>
            <IsActive>true</IsActive>
            <HashedPassword></HashedPassword>
            <HashedPasswordIterations>0</HashedPasswordIterations>
            <WebAccessEnabled>false</WebAccessEnabled>
            <AttachmentType>PDF</AttachmentType>
            <Email></Email>
            <PersonalInfo></PersonalInfo>
            <AddressOverride TableName='OrgHeader' />
            <OrgAddress />
            <Nationality TableName='RefCountry'>
              <Code></Code>
            </Nationality>
            <GlbPerson />
          </OrgContact>
			<OrgContact Action='MERGE'>
            <PK>907538DD-1ED1-4980-B99B-543B3B390967</PK>
            <ContactName>InvalidPhone</ContactName>
            <Salutation></Salutation>
            <Language>EN</Language>
            <NotifyMode>EML</NotifyMode>
            <Title></Title>
            <JobCategory></JobCategory>
            <Phone>1234</Phone>
            <PhoneExtension></PhoneExtension>
            <Fax></Fax>
            <Mobile></Mobile>
            <HomePhone></HomePhone>
            <Pager></Pager>
            <OtherPhone></OtherPhone>
            <Password></Password>
            <Birthday></Birthday>
            <YearJoinedIndustry></YearJoinedIndustry>
            <YearJoinedCompany></YearJoinedCompany>
            <ContactSource></ContactSource>
            <DetailsVerified></DetailsVerified>
            <WebContractSignedDate></WebContractSignedDate>
            <Gender></Gender>
            <ProfilePhoto></ProfilePhoto>
            <IsActive>true</IsActive>
            <HashedPassword></HashedPassword>
            <HashedPasswordIterations>0</HashedPasswordIterations>
            <WebAccessEnabled>false</WebAccessEnabled>
            <AttachmentType>PDF</AttachmentType>
            <Email></Email>
            <PersonalInfo></PersonalInfo>
            <AddressOverride TableName='OrgHeader' />
            <OrgAddress />
            <Nationality TableName='RefCountry'>
              <Code></Code>
            </Nationality>
            <GlbPerson />
          </OrgContact>
        </OrgContactCollection>
        <OrgAddressCollection>
          <OrgAddress Action='MERGE'>
            <PK>11f554c4-39bc-4772-ae95-5c753fa57cb4</PK>
            <Code>111111</Code>
            <Language>EN</Language>
            <Address1>111111</Address1>
            <Address2></Address2>
            <State></State>
            <PostCode></PostCode>
            <Phone>2 8545 5624</Phone>
            <Fax></Fax>
            <Mobile></Mobile>
            <PickupFromTimeOnly></PickupFromTimeOnly>
            <PickupToTimeOnly></PickupToTimeOnly>
            <DeliverFromTimeOnly></DeliverFromTimeOnly>
            <DeliverToTimeOnly></DeliverToTimeOnly>
            <DoNotAttendFrom></DoNotAttendFrom>
            <DoNotAttendTo></DoNotAttendTo>
            <ContainerHandling></ContainerHandling>
            <AccessPoint></AccessPoint>
            <LabourRequired></LabourRequired>
            <CommunicationRequired></CommunicationRequired>
            <Dock_Height></Dock_Height>
            <FCLEquipmentNeeded>WUP</FCLEquipmentNeeded>
            <LCLEquipmentNeeded>PSL</LCLEquipmentNeeded>
            <AIREquipmentNeeded>PSL</AIREquipmentNeeded>
            <Latitude>0.00000</Latitude>
            <Longitude>0.00000</Longitude>
            <IsActive>true</IsActive>
            <CompanyNameOverride></CompanyNameOverride>
            <DockLeveler>false</DockLeveler>
            <ForkLift>false</ForkLift>
            <PalletJack>false</PalletJack>
            <Email></Email>
            <DeliveryRoute></DeliveryRoute>
            <DeliveryRouteSequence>0</DeliveryRouteSequence>
            <UseCumulativeFreeWaitingTime>false</UseCumulativeFreeWaitingTime>
            <ValidationStatus>NYV</ValidationStatus>
            <AddressMap></AddressMap>
            <AuthorityToLeave>DEF</AuthorityToLeave>
            <City></City>
            <OtherWarehouseFacilities></OtherWarehouseFacilities>
            <LoadingUnloadingConstraints></LoadingUnloadingConstraints>
            <GroupNumber>0</GroupNumber>
            <OrgAddressCapabilityCollection>
              <OrgAddressCapability Action='MERGE'>
                <PK>e5cb913c-23e7-4b15-ae4a-4d5318c4ea30</PK>
                <AddressType>OFC</AddressType>
                <IsMainAddress>true</IsMainAddress>
              </OrgAddressCapability>
            </OrgAddressCapabilityCollection>
            <RelatedPortCode TableName='RefUNLOCO'>
              <Code>AUSYD</Code>
              <PK>ad87872e-96b8-4c9a-bc0b-6a4088247a64</PK>
            </RelatedPortCode>
            <CountryCode TableName='RefCountry'>
              <Code>AU</Code>
              <PK>e4eb6e97-aa78-4c46-bf86-35b7fbb5fb0e</PK>
            </CountryCode>
          </OrgAddress>
        </OrgAddressCollection>
        <ClosestPort TableName='RefUNLOCO'>
          <Code>AUSYD</Code>
          <PK>ad87872e-96b8-4c9a-bc0b-6a4088247a64</PK>
        </ClosestPort>
      </OrgHeader>
    </Organization>
  </Body>
</Native>";

		const string PhoneNumberWithValidCHFormatInContactOverriderOrganizationXML = @"<?xml version='1.0' encoding='utf-8'?>
<Native xmlns='http://www.cargowise.com/Schemas/Native/2011/11' version='2.0'>
  <Header>
    <OwnerCode>CARGOWCHI</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Organization version='2.0'>
      <OrgHeader Action='MERGE'>
        <PK>f205e852-9475-4fcc-bf12-e6f70d0f2951</PK>
        <Code>TESTPHONE</Code>
        <Language>EN</Language>
        <SystemLastEditTimeUtc>2016-08-31T09:15:00</SystemLastEditTimeUtc>
        <SystemCreateTimeUtc>2016-08-31T09:14:00</SystemCreateTimeUtc>
        <ScreeningStatus>UNK</ScreeningStatus>
        <IsActive>true</IsActive>
        <FullName>TESTPHONE</FullName>
        <IsConsignee>true</IsConsignee>
        <IsConsignor>false</IsConsignor>
        <IsTransportClient>false</IsTransportClient>
        <IsWarehouseClient>false</IsWarehouseClient>
        <IsForwarder>false</IsForwarder>
        <IsShippingProvider>false</IsShippingProvider>
        <IsAirWholesaler>false</IsAirWholesaler>
        <IsSeaWholesaler>false</IsSeaWholesaler>
        <IsRailProvider>false</IsRailProvider>
        <IsLineHaulProvider>false</IsLineHaulProvider>
        <IsMiscFreightServices>false</IsMiscFreightServices>
        <IsAirCTO>false</IsAirCTO>
        <IsAirLine>false</IsAirLine>
        <IsBroker>false</IsBroker>
        <IsContainerYard>false</IsContainerYard>
        <IsLocalTransport>false</IsLocalTransport>
        <IsPackDepot>false</IsPackDepot>
        <IsSeaCTO>false</IsSeaCTO>
        <IsShippingLine>false</IsShippingLine>
        <IsUnpackDepot>false</IsUnpackDepot>
        <IsRailHead>false</IsRailHead>
        <IsRoadFreightDepot>false</IsRoadFreightDepot>
        <IsShippingConsortium>false</IsShippingConsortium>
        <IsFumigationContractor>false</IsFumigationContractor>
        <IsNationalAccount>false</IsNationalAccount>
        <IsSalesLead>false</IsSalesLead>
        <IsUserFlag13>false</IsUserFlag13>
        <IsUserFlag12>false</IsUserFlag12>
        <IsUserFlag14>false</IsUserFlag14>
        <IsUserFlag11>false</IsUserFlag11>
        <IsCompetitor>false</IsCompetitor>
        <IsTempAccount>false</IsTempAccount>
        <IsPersonalEffectsAccount>false</IsPersonalEffectsAccount>
        <IsUserFlag1>false</IsUserFlag1>
        <IsUserFlag2>false</IsUserFlag2>
        <IsUserFlag3>false</IsUserFlag3>
        <IsUserFlag4>false</IsUserFlag4>
        <IsUserFlag5>false</IsUserFlag5>
        <IsUserFlag6>false</IsUserFlag6>
        <IsUserFlag7>false</IsUserFlag7>
        <IsUserFlag8>false</IsUserFlag8>
        <IsUserFlag9>false</IsUserFlag9>
        <IsUserFlag10>false</IsUserFlag10>
        <IsGlobalAccount>false</IsGlobalAccount>
        <IsDistributionCentre>false</IsDistributionCentre>
        <IsUserFlag15>false</IsUserFlag15>
        <IsUserFlag16>false</IsUserFlag16>
        <IsUserFlag17>false</IsUserFlag17>
        <IsUserFlag18>false</IsUserFlag18>
        <IsUserFlag19>false</IsUserFlag19>
        <IsUserFlag20>false</IsUserFlag20>
        <IsUserFlag21>false</IsUserFlag21>
        <IsUserFlag22>false</IsUserFlag22>
        <IsUserFlag23>false</IsUserFlag23>
        <IsUserFlag24>false</IsUserFlag24>
        <IsControllingCustomer>false</IsControllingCustomer>
        <IsControllingAgent>false</IsControllingAgent>        
        <OrgContactCollection>
          <OrgContact Action='MERGE'>
            <PK>2daeaeda-1ad9-44d3-918b-14a64fb6cd5c</PK>
            <ContactName>ValidPhone</ContactName>
            <Salutation></Salutation>
            <Language>EN</Language>
            <NotifyMode>EML</NotifyMode>
            <Title></Title>
            <JobCategory></JobCategory>
            <Phone>25 8545 5624</Phone>
            <PhoneExtension></PhoneExtension>
            <Fax></Fax>
            <Mobile></Mobile>
            <HomePhone></HomePhone>
            <Pager></Pager>
            <OtherPhone></OtherPhone>
            <Password></Password>
            <Birthday></Birthday>
            <YearJoinedIndustry></YearJoinedIndustry>
            <YearJoinedCompany></YearJoinedCompany>
            <ContactSource></ContactSource>
            <DetailsVerified></DetailsVerified>
            <WebContractSignedDate></WebContractSignedDate>
            <Gender></Gender>
            <ProfilePhoto></ProfilePhoto>
            <IsActive>true</IsActive>
            <HashedPassword></HashedPassword>
            <HashedPasswordIterations>0</HashedPasswordIterations>
            <WebAccessEnabled>false</WebAccessEnabled>
            <AttachmentType>PDF</AttachmentType>
            <Email></Email>
            <PersonalInfo></PersonalInfo>
            <AddressOverride TableName='OrgHeader'>
              <Code>TST</Code>
            </AddressOverride>
            <OrgAddress />
            <Nationality TableName='RefCountry'>
              <Code></Code>
            </Nationality>
            <GlbPerson />
          </OrgContact>
			<OrgContact Action='MERGE'>
            <PK>907538DD-1ED1-4980-B99B-543B3B390967</PK>
            <ContactName>InvalidPhone</ContactName>
            <Salutation></Salutation>
            <Language>EN</Language>
            <NotifyMode>EML</NotifyMode>
            <Title></Title>
            <JobCategory></JobCategory>
            <Phone>1234</Phone>
            <PhoneExtension></PhoneExtension>
            <Fax></Fax>
            <Mobile></Mobile>
            <HomePhone></HomePhone>
            <Pager></Pager>
            <OtherPhone></OtherPhone>
            <Password></Password>
            <Birthday></Birthday>
            <YearJoinedIndustry></YearJoinedIndustry>
            <YearJoinedCompany></YearJoinedCompany>
            <ContactSource></ContactSource>
            <DetailsVerified></DetailsVerified>
            <WebContractSignedDate></WebContractSignedDate>
            <Gender></Gender>
            <ProfilePhoto></ProfilePhoto>
            <IsActive>true</IsActive>
            <HashedPassword></HashedPassword>
            <HashedPasswordIterations>0</HashedPasswordIterations>
            <WebAccessEnabled>false</WebAccessEnabled>
            <AttachmentType>PDF</AttachmentType>
            <Email></Email>
            <PersonalInfo></PersonalInfo>
            <AddressOverride TableName='OrgHeader' />
            <OrgAddress />
            <Nationality TableName='RefCountry'>
              <Code></Code>
            </Nationality>
            <GlbPerson />
          </OrgContact>
        </OrgContactCollection>
        <OrgAddressCollection>
          <OrgAddress Action='MERGE'>
            <PK>11f554c4-39bc-4772-ae95-5c753fa57cb4</PK>
            <Code>111111</Code>
            <Language>EN</Language>
            <Address1>111111</Address1>
            <Address2></Address2>
            <State></State>
            <PostCode></PostCode>
            <Phone>2 8545 5624</Phone>
            <Fax></Fax>
            <Mobile></Mobile>
            <PickupFromTimeOnly></PickupFromTimeOnly>
            <PickupToTimeOnly></PickupToTimeOnly>
            <DeliverFromTimeOnly></DeliverFromTimeOnly>
            <DeliverToTimeOnly></DeliverToTimeOnly>
            <DoNotAttendFrom></DoNotAttendFrom>
            <DoNotAttendTo></DoNotAttendTo>
            <ContainerHandling></ContainerHandling>
            <AccessPoint></AccessPoint>
            <LabourRequired></LabourRequired>
            <CommunicationRequired></CommunicationRequired>
            <Dock_Height></Dock_Height>
            <FCLEquipmentNeeded>WUP</FCLEquipmentNeeded>
            <LCLEquipmentNeeded>PSL</LCLEquipmentNeeded>
            <AIREquipmentNeeded>PSL</AIREquipmentNeeded>
            <Latitude>0.00000</Latitude>
            <Longitude>0.00000</Longitude>
            <IsActive>true</IsActive>
            <CompanyNameOverride></CompanyNameOverride>
            <DockLeveler>false</DockLeveler>
            <ForkLift>false</ForkLift>
            <PalletJack>false</PalletJack>
            <Email></Email>
            <DeliveryRoute></DeliveryRoute>
            <DeliveryRouteSequence>0</DeliveryRouteSequence>
            <UseCumulativeFreeWaitingTime>false</UseCumulativeFreeWaitingTime>
            <ValidationStatus>NYV</ValidationStatus>
            <AddressMap></AddressMap>
            <AuthorityToLeave>DEF</AuthorityToLeave>
            <City></City>
            <OtherWarehouseFacilities></OtherWarehouseFacilities>
            <LoadingUnloadingConstraints></LoadingUnloadingConstraints>
            <GroupNumber>0</GroupNumber>
            <OrgAddressCapabilityCollection>
              <OrgAddressCapability Action='MERGE'>
                <PK>e5cb913c-23e7-4b15-ae4a-4d5318c4ea30</PK>
                <AddressType>OFC</AddressType>
                <IsMainAddress>true</IsMainAddress>
              </OrgAddressCapability>
            </OrgAddressCapabilityCollection>
            <RelatedPortCode TableName='RefUNLOCO'>
              <Code>AUSYD</Code>
              <PK>ad87872e-96b8-4c9a-bc0b-6a4088247a64</PK>
            </RelatedPortCode>
            <CountryCode TableName='RefCountry'>
              <Code>AU</Code>
              <PK>e4eb6e97-aa78-4c46-bf86-35b7fbb5fb0e</PK>
            </CountryCode>
          </OrgAddress>
        </OrgAddressCollection>
        <ClosestPort TableName='RefUNLOCO'>
          <Code>AUSYD</Code>
          <PK>ad87872e-96b8-4c9a-bc0b-6a4088247a64</PK>
        </ClosestPort>
      </OrgHeader>
    </Organization>
  </Body>
</Native>";

		const string PhoneNumberWithValidCHFormatInContactBranchAddressXML = @"<?xml version='1.0' encoding='utf-8'?>
<Native xmlns='http://www.cargowise.com/Schemas/Native/2011/11' version='2.0'>
  <Header>
    <OwnerCode>CARGOWCHI</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Organization version='2.0'>
      <OrgHeader Action='MERGE'>
        <PK>f205e852-9475-4fcc-bf12-e6f70d0f2951</PK>
        <Code>TESTPHONE</Code>
        <Language>EN</Language>
        <SystemLastEditTimeUtc>2016-08-31T09:15:00</SystemLastEditTimeUtc>
        <SystemCreateTimeUtc>2016-08-31T09:14:00</SystemCreateTimeUtc>
        <ScreeningStatus>UNK</ScreeningStatus>
        <IsActive>true</IsActive>
        <FullName>TESTPHONE</FullName>
        <IsConsignee>true</IsConsignee>
        <IsConsignor>false</IsConsignor>
        <IsTransportClient>false</IsTransportClient>
        <IsWarehouseClient>false</IsWarehouseClient>
        <IsForwarder>false</IsForwarder>
        <IsShippingProvider>false</IsShippingProvider>
        <IsAirWholesaler>false</IsAirWholesaler>
        <IsSeaWholesaler>false</IsSeaWholesaler>
        <IsRailProvider>false</IsRailProvider>
        <IsLineHaulProvider>false</IsLineHaulProvider>
        <IsMiscFreightServices>false</IsMiscFreightServices>
        <IsAirCTO>false</IsAirCTO>
        <IsAirLine>false</IsAirLine>
        <IsBroker>false</IsBroker>
        <IsContainerYard>false</IsContainerYard>
        <IsLocalTransport>false</IsLocalTransport>
        <IsPackDepot>false</IsPackDepot>
        <IsSeaCTO>false</IsSeaCTO>
        <IsShippingLine>false</IsShippingLine>
        <IsUnpackDepot>false</IsUnpackDepot>
        <IsRailHead>false</IsRailHead>
        <IsRoadFreightDepot>false</IsRoadFreightDepot>
        <IsShippingConsortium>false</IsShippingConsortium>
        <IsFumigationContractor>false</IsFumigationContractor>
        <IsNationalAccount>false</IsNationalAccount>
        <IsSalesLead>false</IsSalesLead>
        <IsUserFlag13>false</IsUserFlag13>
        <IsUserFlag12>false</IsUserFlag12>
        <IsUserFlag14>false</IsUserFlag14>
        <IsUserFlag11>false</IsUserFlag11>
        <IsCompetitor>false</IsCompetitor>
        <IsTempAccount>false</IsTempAccount>
        <IsPersonalEffectsAccount>false</IsPersonalEffectsAccount>
        <IsUserFlag1>false</IsUserFlag1>
        <IsUserFlag2>false</IsUserFlag2>
        <IsUserFlag3>false</IsUserFlag3>
        <IsUserFlag4>false</IsUserFlag4>
        <IsUserFlag5>false</IsUserFlag5>
        <IsUserFlag6>false</IsUserFlag6>
        <IsUserFlag7>false</IsUserFlag7>
        <IsUserFlag8>false</IsUserFlag8>
        <IsUserFlag9>false</IsUserFlag9>
        <IsUserFlag10>false</IsUserFlag10>
        <IsGlobalAccount>false</IsGlobalAccount>
        <IsDistributionCentre>false</IsDistributionCentre>
        <IsUserFlag15>false</IsUserFlag15>
        <IsUserFlag16>false</IsUserFlag16>
        <IsUserFlag17>false</IsUserFlag17>
        <IsUserFlag18>false</IsUserFlag18>
        <IsUserFlag19>false</IsUserFlag19>
        <IsUserFlag20>false</IsUserFlag20>
        <IsUserFlag21>false</IsUserFlag21>
        <IsUserFlag22>false</IsUserFlag22>
        <IsUserFlag23>false</IsUserFlag23>
        <IsUserFlag24>false</IsUserFlag24>
        <IsControllingCustomer>false</IsControllingCustomer>
        <IsControllingAgent>false</IsControllingAgent>        
        <OrgContactCollection>
          <OrgContact Action='MERGE'>
            <PK>2daeaeda-1ad9-44d3-918b-14a64fb6cd5c</PK>
            <ContactName>ValidPhone</ContactName>
            <Salutation></Salutation>
            <Language>EN</Language>
            <NotifyMode>EML</NotifyMode>
            <Title></Title>
            <JobCategory></JobCategory>
            <Phone>25 8545 5624</Phone>
            <PhoneExtension></PhoneExtension>
            <Fax></Fax>
            <Mobile></Mobile>
            <HomePhone></HomePhone>
            <Pager></Pager>
            <OtherPhone></OtherPhone>
            <Password></Password>
            <Birthday></Birthday>
            <YearJoinedIndustry></YearJoinedIndustry>
            <YearJoinedCompany></YearJoinedCompany>
            <ContactSource></ContactSource>
            <DetailsVerified></DetailsVerified>
            <WebContractSignedDate></WebContractSignedDate>
            <Gender></Gender>
            <ProfilePhoto></ProfilePhoto>
            <IsActive>true</IsActive>
            <HashedPassword></HashedPassword>
            <HashedPasswordIterations>0</HashedPasswordIterations>
            <WebAccessEnabled>false</WebAccessEnabled>
            <AttachmentType>PDF</AttachmentType>
            <Email></Email>
            <PersonalInfo></PersonalInfo>
            <AddressOverride TableName='OrgHeader' />
            <OrgAddress>
				<Code>TestAddress</Code>
            </OrgAddress>
            <Nationality TableName='RefCountry'>
              <Code></Code>
            </Nationality>
            <GlbPerson />
          </OrgContact>
			<OrgContact Action='MERGE'>
            <PK>907538DD-1ED1-4980-B99B-543B3B390967</PK>
            <ContactName>InvalidPhone</ContactName>
            <Salutation></Salutation>
            <Language>EN</Language>
            <NotifyMode>EML</NotifyMode>
            <Title></Title>
            <JobCategory></JobCategory>
            <Phone>1234</Phone>
            <PhoneExtension></PhoneExtension>
            <Fax></Fax>
            <Mobile></Mobile>
            <HomePhone></HomePhone>
            <Pager></Pager>
            <OtherPhone></OtherPhone>
            <Password></Password>
            <Birthday></Birthday>
            <YearJoinedIndustry></YearJoinedIndustry>
            <YearJoinedCompany></YearJoinedCompany>
            <ContactSource></ContactSource>
            <DetailsVerified></DetailsVerified>
            <WebContractSignedDate></WebContractSignedDate>
            <Gender></Gender>
            <ProfilePhoto></ProfilePhoto>
            <IsActive>true</IsActive>
            <HashedPassword></HashedPassword>
            <HashedPasswordIterations>0</HashedPasswordIterations>
            <WebAccessEnabled>false</WebAccessEnabled>
            <AttachmentType>PDF</AttachmentType>
            <Email></Email>
            <PersonalInfo></PersonalInfo>
            <AddressOverride TableName='OrgHeader' />
            <OrgAddress />
            <Nationality TableName='RefCountry'>
              <Code></Code>
            </Nationality>
            <GlbPerson />
          </OrgContact>
        </OrgContactCollection>
        <OrgAddressCollection>
          <OrgAddress Action='MERGE'>
            <PK>11f554c4-39bc-4772-ae95-5c753fa57cb4</PK>
            <Code>111111</Code>
            <Language>EN</Language>
            <Address1>111111</Address1>
            <Address2></Address2>
            <State></State>
            <PostCode></PostCode>
            <Phone>2 8545 5624</Phone>
            <Fax></Fax>
            <Mobile></Mobile>
            <PickupFromTimeOnly></PickupFromTimeOnly>
            <PickupToTimeOnly></PickupToTimeOnly>
            <DeliverFromTimeOnly></DeliverFromTimeOnly>
            <DeliverToTimeOnly></DeliverToTimeOnly>
            <DoNotAttendFrom></DoNotAttendFrom>
            <DoNotAttendTo></DoNotAttendTo>
            <ContainerHandling></ContainerHandling>
            <AccessPoint></AccessPoint>
            <LabourRequired></LabourRequired>
            <CommunicationRequired></CommunicationRequired>
            <Dock_Height></Dock_Height>
            <FCLEquipmentNeeded>WUP</FCLEquipmentNeeded>
            <LCLEquipmentNeeded>PSL</LCLEquipmentNeeded>
            <AIREquipmentNeeded>PSL</AIREquipmentNeeded>
            <Latitude>0.00000</Latitude>
            <Longitude>0.00000</Longitude>
            <IsActive>true</IsActive>
            <CompanyNameOverride></CompanyNameOverride>
            <DockLeveler>false</DockLeveler>
            <ForkLift>false</ForkLift>
            <PalletJack>false</PalletJack>
            <Email></Email>
            <DeliveryRoute></DeliveryRoute>
            <DeliveryRouteSequence>0</DeliveryRouteSequence>
            <UseCumulativeFreeWaitingTime>false</UseCumulativeFreeWaitingTime>
            <ValidationStatus>NYV</ValidationStatus>
            <AddressMap></AddressMap>
            <AuthorityToLeave>DEF</AuthorityToLeave>
            <City></City>
            <OtherWarehouseFacilities></OtherWarehouseFacilities>
            <LoadingUnloadingConstraints></LoadingUnloadingConstraints>
            <GroupNumber>0</GroupNumber>
            <OrgAddressCapabilityCollection>
              <OrgAddressCapability Action='MERGE'>
                <PK>e5cb913c-23e7-4b15-ae4a-4d5318c4ea30</PK>
                <AddressType>OFC</AddressType>
                <IsMainAddress>true</IsMainAddress>
              </OrgAddressCapability>
            </OrgAddressCapabilityCollection>
            <RelatedPortCode TableName='RefUNLOCO'>
              <Code>AUSYD</Code>
              <PK>ad87872e-96b8-4c9a-bc0b-6a4088247a64</PK>
            </RelatedPortCode>
            <CountryCode TableName='RefCountry'>
              <Code>AU</Code>
              <PK>e4eb6e97-aa78-4c46-bf86-35b7fbb5fb0e</PK>
            </CountryCode>
          </OrgAddress>
			<OrgAddress Action='MERGE'>
            <PK>114E4017-974E-480F-B58C-9E9DAE2CC2FE</PK>
            <Code>TestAddress</Code>
            <Language>ZH-CN</Language>
            <Address1>111111</Address1>
            <Address2></Address2>
            <State></State>
            <PostCode></PostCode>
            <Phone>2 8545 5624</Phone>
            <Fax></Fax>
            <Mobile></Mobile>
            <PickupFromTimeOnly></PickupFromTimeOnly>
            <PickupToTimeOnly></PickupToTimeOnly>
            <DeliverFromTimeOnly></DeliverFromTimeOnly>
            <DeliverToTimeOnly></DeliverToTimeOnly>
            <DoNotAttendFrom></DoNotAttendFrom>
            <DoNotAttendTo></DoNotAttendTo>
            <ContainerHandling></ContainerHandling>
            <AccessPoint></AccessPoint>
            <LabourRequired></LabourRequired>
            <CommunicationRequired></CommunicationRequired>
            <Dock_Height></Dock_Height>
            <FCLEquipmentNeeded>WUP</FCLEquipmentNeeded>
            <LCLEquipmentNeeded>PSL</LCLEquipmentNeeded>
            <AIREquipmentNeeded>PSL</AIREquipmentNeeded>
            <Latitude>0.00000</Latitude>
            <Longitude>0.00000</Longitude>
            <IsActive>true</IsActive>
            <CompanyNameOverride></CompanyNameOverride>
            <DockLeveler>false</DockLeveler>
            <ForkLift>false</ForkLift>
            <PalletJack>false</PalletJack>
            <Email></Email>
            <DeliveryRoute></DeliveryRoute>
            <DeliveryRouteSequence>0</DeliveryRouteSequence>
            <UseCumulativeFreeWaitingTime>false</UseCumulativeFreeWaitingTime>
            <ValidationStatus>NYV</ValidationStatus>
            <AddressMap></AddressMap>
            <AuthorityToLeave>DEF</AuthorityToLeave>
            <City></City>
            <OtherWarehouseFacilities></OtherWarehouseFacilities>
            <LoadingUnloadingConstraints></LoadingUnloadingConstraints>
            <GroupNumber>0</GroupNumber>
            <OrgAddressCapabilityCollection>
              <OrgAddressCapability Action='MERGE'>
                <PK>6CCC7E28-D44A-4E82-AB8C-0989267CC0F2</PK>
                <AddressType>OFC</AddressType>
                <IsMainAddress>true</IsMainAddress>
              </OrgAddressCapability>
            </OrgAddressCapabilityCollection>
            <CountryCode TableName='RefCountry'>
              <Code>CN</Code>
              <PK>e4eb6e97-aa78-4c46-bf86-35b7fbb5fb0e</PK>
            </CountryCode>
          </OrgAddress>
        </OrgAddressCollection>
        <ClosestPort TableName='RefUNLOCO'>
          <Code>AUSYD</Code>
          <PK>ad87872e-96b8-4c9a-bc0b-6a4088247a64</PK>
        </ClosestPort>
      </OrgHeader>
    </Organization>
  </Body>
</Native>";

		#endregion

		public void TestImportXMLOrgHeaderWithEDICommunicationsModeCollection()
		{
			var encoding = new UTF8Encoding();
			using (var stream = new MemoryStream(encoding.GetBytes(XMLOrgHeaderWithEDICommunicationsModeCollection)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				var insertLog = manager.GetLogs();
				var expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 1 inserts, 0 updates, 0 deletes
OrgAddress - 1 inserts, 0 updates, 0 deletes
OrgAddressCapability - 1 inserts, 0 updates, 0 deletes
EDICommunicationsMode - 1 inserts, 0 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Log Text", expectedLog, insertLog);
			}
		}

		#region XMLOrgHeaderWithEDICommunicationsModeCollection

		const string XMLOrgHeaderWithEDICommunicationsModeCollection =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>EDICUS</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Organization version=""2.0"">
      <OrgHeader Action=""INSERT"">
        <Code>NIXDRFSYD</Code>
        <IsActive>true</IsActive>
        <FullName>NIXDORF AUSTRALIA INTERNATIONAL</FullName>
        <IsConsignor>true</IsConsignor>
        <IsForwarder>false</IsForwarder>
        <IsShippingProvider>false</IsShippingProvider>
        <Language>EN</Language>
        <Category>BUS</Category>
        <ScreeningStatus>NOT</ScreeningStatus>
        <OrgAddressCollection>
          <OrgAddress Action=""INSERT"">
            <IsActive>true</IsActive>
            <Code>AIRPORT</Code>
            <CompanyNameOverride></CompanyNameOverride>
            <Address1>AIRPORT</Address1>
            <Address2>EAGLE</Address2>
            <State>NSW</State>
            <PostCode>2118</PostCode>
            <Phone></Phone>
            <Fax></Fax>
            <Mobile></Mobile>
            <PickupFromTimeOnly></PickupFromTimeOnly>
            <PickupToTimeOnly></PickupToTimeOnly>
            <DeliverFromTimeOnly></DeliverFromTimeOnly>
            <DeliverToTimeOnly></DeliverToTimeOnly>
            <DoNotAttendFrom></DoNotAttendFrom>
            <DoNotAttendTo></DoNotAttendTo>
            <DockLeveler>false</DockLeveler>
            <ForkLift>false</ForkLift>
            <PalletJack>false</PalletJack>
            <ContainerHandling></ContainerHandling>
            <AccessPoint></AccessPoint>
            <LabourRequired></LabourRequired>
            <CommunicationRequired></CommunicationRequired>
            <Dock_Height></Dock_Height>
            <FCLEquipmentNeeded>WUP</FCLEquipmentNeeded>
            <LCLEquipmentNeeded>PSL</LCLEquipmentNeeded>
            <AIREquipmentNeeded>PSL</AIREquipmentNeeded>
            <Email>ANSETT@eagle.com</Email>
            <UseCumulativeFreeWaitingTime>false</UseCumulativeFreeWaitingTime>
            <DeliveryRoute></DeliveryRoute>
            <DeliveryRouteSequence>0</DeliveryRouteSequence>
            <AddressMap></AddressMap>
            <AuthorityToLeave>DEF</AuthorityToLeave>
            <OtherWarehouseFacilities></OtherWarehouseFacilities>
            <LoadingUnloadingConstraints></LoadingUnloadingConstraints>
            <City>FARM</City>
            <GroupNumber>0</GroupNumber>
            <AdditionalAddressInformation></AdditionalAddressInformation>
            <VerifiesContainerGrossWeight>false</VerifiesContainerGrossWeight>
            <Language>EN</Language>
            <SystemCreateTimeUtc>2019-11-17T22:22:00</SystemCreateTimeUtc>
            <SystemLastEditTimeUtc>2019-11-17T22:23:00</SystemLastEditTimeUtc>
            <JobLoadingDuration>0</JobLoadingDuration>
            <OrgAddressCapabilityCollection>
              <OrgAddressCapability Action=""INSERT"">
                <AddressType>OFC</AddressType>
                <IsMainAddress>true</IsMainAddress>
              </OrgAddressCapability>
            </OrgAddressCapabilityCollection>
            <RelatedPortCode TableName=""RefUNLOCO"">
              <Code>AUSYD</Code>
            </RelatedPortCode>
            <CountryCode TableName=""RefCountry"">
              <Code>AU</Code>
            </CountryCode>
          </OrgAddress>
        </OrgAddressCollection>
        <EDICommunicationsModeCollection>
          <EDICommunicationsMode Action=""INSERT"">
            <Module>BKN</Module>
            <MessagePurpose>EVT</MessagePurpose>
            <CommsDirection>TRX</CommsDirection>
            <CommunicationsTransport>HUB</CommunicationsTransport>
            <FtpLockingMethod></FtpLockingMethod>
            <Destination>123123</Destination>
            <ServerAddressSubject></ServerAddressSubject>
            <Filename></Filename>
            <PortNumber>0</PortNumber>
            <FileFormat>XUE</FileFormat>
            <LoginName></LoginName>
            <Certificate></Certificate>
            <LastFailed></LastFailed>
            <LocalPartyVanID></LocalPartyVanID>
            <RelatedPartyVanID></RelatedPartyVanID>
            <PublishInternalMilestones>false</PublishInternalMilestones>
            <DestinationFolder></DestinationFolder>
            <SourceFolder></SourceFolder>
            <EventCode></EventCode>
            <EventReferenceConditionType></EventReferenceConditionType>
            <EventReferenceConditionValue></EventReferenceConditionValue>
            <RecipientRole></RecipientRole>
            <TransportMode></TransportMode>
            <MessageVAN TableName=""OrgHeader"" />
            <GlbGroup />
          </EDICommunicationsMode>
        </EDICommunicationsModeCollection>
        <ClosestPort TableName=""RefUNLOCO"">
          <Code>AUSYD</Code>
        </ClosestPort>
        <ShippingLine TableName=""RefShippingLine"" />
      </OrgHeader>
    </Organization>
  </Body>
</Native>";
		#endregion

		public void TestImportXMLOrgHeaderWithGlbGroupLink()
		{
			var expectedResultLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 1 inserts, 0 updates, 0 deletes
OrgContact - 1 inserts, 0 updates, 0 deletes
Group - 2 inserts, 0 updates, 0 deletes
GlbGroupOrgContactLink - 1 inserts, 0 updates, 0 deletes
OrgAddress - 1 inserts, 0 updates, 0 deletes
GlbGroupOrgLink - 1 inserts, 0 updates, 0 deletes
".Trim();
			var encoding = new UTF8Encoding();
			using (var stream = new MemoryStream(encoding.GetBytes(XMLOrgHeaderWithGlbGroupLinkCollection)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);
				var insertLog = manager.GetLogs();
				AssertMultilineASCIIEquals("Log Text", expectedResultLog, insertLog);
			}
		}

		const string XMLOrgHeaderWithGlbGroupLinkCollection = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>EDICUS</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Organization version=""2.0"">
      <OrgHeader Action=""INSERT"">
        <Code>NIXDRFSYD</Code>
        <IsActive>true</IsActive>
        <FullName>NIXDORF AUSTRALIA INTERNATIONAL</FullName>
        <IsConsignor>true</IsConsignor>
        <IsForwarder>false</IsForwarder>
        <IsShippingProvider>false</IsShippingProvider>
        <Language>EN</Language>
        <Category>BUS</Category>
        <ScreeningStatus>NOT</ScreeningStatus>
		<OrgAddressCollection>
		  <OrgAddress Action=""INSERT"">
            <IsActive>true</IsActive>
            <Code>AIRPORT</Code>
            <CompanyNameOverride></CompanyNameOverride>
            <Address1>AIRPORT</Address1>
            <Address2>EAGLE</Address2>
            <State>NSW</State>
            <PostCode>2118</PostCode>
            <Phone></Phone>
            <Fax></Fax>
            <Mobile></Mobile>
            <PickupFromTimeOnly></PickupFromTimeOnly>
            <PickupToTimeOnly></PickupToTimeOnly>
            <DeliverFromTimeOnly></DeliverFromTimeOnly>
            <DeliverToTimeOnly></DeliverToTimeOnly>
            <DoNotAttendFrom></DoNotAttendFrom>
            <DoNotAttendTo></DoNotAttendTo>
            <DockLeveler>false</DockLeveler>
            <ForkLift>false</ForkLift>
            <PalletJack>false</PalletJack>
            <ContainerHandling></ContainerHandling>
            <AccessPoint></AccessPoint>
            <LabourRequired></LabourRequired>
            <CommunicationRequired></CommunicationRequired>
            <Dock_Height></Dock_Height>
            <FCLEquipmentNeeded>WUP</FCLEquipmentNeeded>
            <LCLEquipmentNeeded>PSL</LCLEquipmentNeeded>
            <AIREquipmentNeeded>PSL</AIREquipmentNeeded>
            <Email>ANSETT@eagle.com</Email>
            <UseCumulativeFreeWaitingTime>false</UseCumulativeFreeWaitingTime>
            <DeliveryRoute></DeliveryRoute>
            <DeliveryRouteSequence>0</DeliveryRouteSequence>
            <AddressMap></AddressMap>
            <AuthorityToLeave>DEF</AuthorityToLeave>
            <OtherWarehouseFacilities></OtherWarehouseFacilities>
            <LoadingUnloadingConstraints></LoadingUnloadingConstraints>
            <City>FARM</City>
            <GroupNumber>0</GroupNumber>
            <AdditionalAddressInformation></AdditionalAddressInformation>
            <VerifiesContainerGrossWeight>false</VerifiesContainerGrossWeight>
            <Language>EN</Language>
            <SystemCreateTimeUtc>2019-11-17T22:22:00</SystemCreateTimeUtc>
            <SystemLastEditTimeUtc>2019-11-17T22:23:00</SystemLastEditTimeUtc>
            <JobLoadingDuration>0</JobLoadingDuration>
            <CountryCode TableName=""RefCountry"">
              <Code>AU</Code>
            </CountryCode>
          </OrgAddress>
        </OrgAddressCollection>
		<OrgContactCollection>
		<OrgContact Action=""INSERT"">
            <PK>c28ed0ff-12a9-4cb4-ad31-7ab4e40bff88</PK>
            <IsActive>true</IsActive>
            <ContactName>123</ContactName>
            <Salutation></Salutation>
            <Language>EN</Language>
            <NotifyMode>EML</NotifyMode>
            <Title></Title>
            <Gender>N</Gender>
            <JobCategory>EMU</JobCategory>
            <Phone></Phone>
            <PhoneExtension></PhoneExtension>
            <Fax></Fax>
            <Mobile></Mobile>
            <HomePhone></HomePhone>
            <Pager></Pager>
            <OtherPhone></OtherPhone>
            <Email>ntz@1234.com</Email>
            <ProfilePhoto></ProfilePhoto>
            <AttachmentType>PDF</AttachmentType>
            <WebAccessEnabled>true</WebAccessEnabled>
            <WebContractSignedDate></WebContractSignedDate>
            <Birthday></Birthday>
            <YearJoinedIndustry></YearJoinedIndustry>
            <YearJoinedCompany></YearJoinedCompany>
            <ContactSource></ContactSource>
            <DetailsVerified>2022-10-21T04:51:00</DetailsVerified>
            <PersonalInfo></PersonalInfo>
            <PasswordHash></PasswordHash>
            <PasswordHashIterations>0</PasswordHashIterations>
            <PasswordSalt></PasswordSalt>
            <GlbGroupOrgContactLinkCollection>
              <GlbGroupOrgContactLink Action=""INSERT"">
                <Group TableName=""GlbGroup"" Action=""INSERT"">
                  <Code>ABCDG</Code>
                </Group>
              </GlbGroupOrgContactLink>
            </GlbGroupOrgContactLinkCollection>
            <Nationality TableName=""RefCountry"">
              <Code></Code>
            </Nationality>
            <AddressOverride TableName=""OrgHeader"" />
          </OrgContact>
		</OrgContactCollection>
		<GlbGroupOrgLinkCollection>
		  <GlbGroupOrgLink Action=""MERGE"">
            <Group TableName=""GlbGroup"" Action=""MERGE"">
              <Code>SALESALL</Code>
            </Group>
          </GlbGroupOrgLink>
        </GlbGroupOrgLinkCollection>
        <ClosestPort TableName=""RefUNLOCO"">
          <Code>AUSYD</Code>
        </ClosestPort>
        <ShippingLine TableName=""RefShippingLine"" />
      </OrgHeader>
    </Organization>
  </Body>
</Native>";

		public void TestImportXMLOfOrgHeaderWithCompetitor()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var com = Factory.NewWithValidTestData<GlbCompany>();
			Factory.Save();

			var testCompetitorCode = "TST";
			var testCollection = (OverrideImmuneCodeDescriptionBoolCollection)OrganisationsDataRegistry.Instance.CompetitorType.Value;
			testCollection.AddSystemDefined(testCompetitorCode, (NoResString)("TST REGISTRY"), false);

			var encoding = new UTF8Encoding();
			using (OrganisationsDataRegistry.Instance.CompetitorType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, testCollection))
			using (var stream = new MemoryStream(encoding.GetBytes(GetOrgHeaderWithCompetitorXML(org, com))))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				var insertLog = manager.GetLogs();
				var expectedLog = @"--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 1 inserts, 0 updates, 0 deletes
OrgCompetitor - 5 inserts, 0 updates, 0 deletes
".Trim();

				AssertMultilineASCIIEquals("Log Text", expectedLog, insertLog);
				var factory = NewFactory();
				var organisation = factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, "TESTOCP"));
				AssertNotNull(organisation);
				AssertEquals("Only 4 OrgCompetitor can be retrieved by company level", 4, organisation.Competitors.Count);
				CombineAssertions(delegate
				{
					AssertEquals("Should have 1 OrgCompetitor with CMB type.", 1, organisation.Competitors.Count(c => c.OCP_Type == "CMB"));
					AssertEquals("Should have 1 OrgCompetitor with AAA type.", 1, organisation.Competitors.Count(c => c.OCP_Type == "AAA"));
					AssertEquals("Should have 1 OrgCompetitor with TST type.", 1, organisation.Competitors.Count(c => c.OCP_Type == "TST"));
					AssertEquals("Should not have OrgCompetitor with COM type.", 0, organisation.Competitors.Count(c => c.OCP_Type == "COM"));
					AssertEquals("Should have 1 OrgCompetitor with CO2 type.", 1, organisation.Competitors.Count(c => c.OCP_Type == "CO2"));
				});
			}
		}

		#region OrgHeaderWithCompetitorXML

		public string GetOrgHeaderWithCompetitorXML(OrgHeader org, GlbCompany com)
		{
			return string.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
<ReferenceData xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <Header xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.cargowise.com/Schemas/Universal"">
    <OwnerCode>EDICUS</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Organization>
      <OrgHeader Action=""MERGE"">
        <Code>TESTOCP</Code>
        <IsActive>true</IsActive>
        <FullName>TESTOCP</FullName>
        <IsConsignee>true</IsConsignee>
        <IsConsignor>true</IsConsignor>
        <IsForwarder>false</IsForwarder>
        <IsShippingProvider>false</IsShippingProvider>
        <Language>EN</Language>
        <OrgCompetitorCollection>
          <OrgCompetitor Action=""MERGE"">
            <PK>a78a9f4b-2a93-4d24-9619-0d61638d30ce</PK>
            <Type>CMB</Type>
            <Competitor TableName=""OrgHeader"">
              <Code>{0}</Code>
              <PK>{1}</PK>
            </Competitor>
            <Company TableName=""GlbCompany"" />
          </OrgCompetitor>
          <OrgCompetitor Action=""MERGE"">
            <PK>98ce5feb-f0b9-41d5-bb5f-374fc9cf47b1</PK>
            <Type>AAA</Type>
            <Competitor TableName=""OrgHeader"">
              <Code>{0}</Code>
              <PK>{1}</PK>
            </Competitor>
            <Company TableName=""GlbCompany"" />
          </OrgCompetitor>
          <OrgCompetitor Action=""MERGE"">
            <PK>7d30110e-5384-4c9e-9ebd-f811ca16c394</PK>
            <Type>TST</Type>
            <Competitor TableName=""OrgHeader"">
              <Code>{0}</Code>
              <PK>{1}</PK>
            </Competitor>
            <Company TableName=""GlbCompany"" />
          </OrgCompetitor>
          <OrgCompetitor Action=""MERGE"">
            <PK>3d2c19c2-1f87-4739-8d34-952b04f743ef</PK>
            <Type>COM</Type>
            <Competitor TableName=""OrgHeader"">
              <Code>{0}</Code>
              <PK>{1}</PK>
            </Competitor>
            <Company TableName=""GlbCompany"">
              <Code>{2}</Code>
              <PK>{3}</PK>
            </Company>
          </OrgCompetitor>
          <OrgCompetitor Action=""MERGE"">
            <PK>45261954-f87a-49b6-aa8b-7d9d2875eb39</PK>
            <Type>CO2</Type>
            <Competitor TableName=""OrgHeader"">
              <Code>{0}</Code>
              <PK>{1}</PK>
            </Competitor>
            <Company TableName=""GlbCompany"">
              <Code>{4}</Code>
              <PK>{5}</PK>
            </Company>
          </OrgCompetitor>
        </OrgCompetitorCollection>
        <ClosestPort TableName=""RefUNLOCO"">
          <Code>AUBNE</Code>
        </ClosestPort>
      </OrgHeader>
    </Organization>
  </Body>
</ReferenceData>", org.OH_Code, org.PK, com.GC_Code, com.PK, GlbCompany.CurrentCompany.GC_Code, GlbCompany.CurrentCompany.PK);
		}

		#endregion
	}
}
