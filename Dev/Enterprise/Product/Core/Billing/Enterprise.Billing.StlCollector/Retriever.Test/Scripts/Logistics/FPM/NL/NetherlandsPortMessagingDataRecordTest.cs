using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(NetherlandsPortMessagingDataRecord))]
	sealed class NetherlandsPortMessagingDataRecordTest : RefStlScriptWithDefaultsTest
	{
		protected override IDateTimeRange TestDateTimeRange => new RecurringRange(new DateTime(2024, 12, 06, 13, 00, 00), new DateTime(2024, 12, 06, 14, 00, 00));

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals(6, transactions.Count());

			var row1 = transactions.Single(x => x.Reference3.Equals("EA000001 [EA]"));
			var row2 = transactions.Single(x => x.Reference3.Equals("EAA000002 [EAA]"));
			var row3 = transactions.Single(x => x.Reference3.Equals("EB000001 [EB]"));
			var row4 = transactions.Single(x => x.Reference3.Equals("EC000001 [EC]"));
			var row5 = transactions.Single(x => x.Reference3.Equals("ECC000001 [ECC]"));
			var row6 = transactions.Single(x => x.Reference3.Equals("EB000009 [EM]"));

			CombineAssertions(() =>
			{
				AssertEquals("[Row-1] CompanyCode", "DAU", row1.GetCompanyCode());
				AssertEquals("[Row-1] BranchCode", "GB0", row1.GetBranchCode());
				AssertEquals("[Row-1] TransactionDateUtc", new DateTime(2024, 12, 06, 13, 13, 00), row1.ServiceOccuredUTC);
				AssertEquals("[Row-1] TransactionReference01", "C00000001", row1.Reference1);
				AssertEquals("[Row-1] TransactionReference02", "NLRTM - Export Notification", row1.Reference2);
				AssertEquals("[Row-1] TransactionReference03", "EA000001 [EA]", row1.Reference3);
				AssertNull("[Row-1] TransactionReference04", row1.Reference4);

				AssertEquals("[Row-2] CompanyCode", "DAU", row2.GetCompanyCode());
				AssertEquals("[Row-2] BranchCode", "GB0", row2.GetBranchCode());
				AssertEquals("[Row-2] TransactionDateUtc", new DateTime(2024, 12, 06, 13, 13, 00), row2.ServiceOccuredUTC);
				AssertEquals("[Row-2] TransactionReference01", "C00000001", row2.Reference1);
				AssertEquals("[Row-2] TransactionReference02", "NLRTM - Export Notification", row2.Reference2);
				AssertEquals("[Row-2] TransactionReference03", "EAA000002 [EAA]", row2.Reference3);
				AssertNull("[Row-2] TransactionReference04", row2.Reference4);

				AssertEquals("[Row-3] CompanyCode", "DAU", row3.GetCompanyCode());
				AssertEquals("[Row-3] BranchCode", "GB1", row3.GetBranchCode());
				AssertEquals("[Row-3] TransactionDateUtc", new DateTime(2024, 12, 06, 13, 14, 00), row3.ServiceOccuredUTC);
				AssertEquals("[Row-3] TransactionReference01", "C00000002", row3.Reference1);
				AssertEquals("[Row-3] TransactionReference02", "NLAMS - Export Notification", row3.Reference2);
				AssertEquals("[Row-3] TransactionReference03", "EB000001 [EB]", row3.Reference3);
				AssertNull("[Row-3] TransactionReference04", row3.Reference4);

				AssertEquals("[Row-4] CompanyCode", "SHA", row4.GetCompanyCode());
				AssertEquals("[Row-4] BranchCode", "GB2", row4.GetBranchCode());
				AssertEquals("[Row-4] TransactionDateUtc", new DateTime(2024, 12, 06, 13, 15, 00), row4.ServiceOccuredUTC);
				AssertEquals("[Row-4] TransactionReference01", "C00000003", row4.Reference1);
				AssertEquals("[Row-4] TransactionReference02", "NLHHD - Import Notification", row4.Reference2);
				AssertEquals("[Row-4] TransactionReference03", "EC000001 [EC]", row4.Reference3);
				AssertNull("[Row-4] TransactionReference04", row4.Reference4);

				AssertEquals("[Row-5] CompanyCode", "SHA", row5.GetCompanyCode());
				AssertEquals("[Row-5] BranchCode", "GB2", row5.GetBranchCode());
				AssertEquals("[Row-5] TransactionDateUtc", new DateTime(2024, 12, 06, 13, 15, 00), row5.ServiceOccuredUTC);
				AssertEquals("[Row-5] TransactionReference01", "C00000003", row5.Reference1);
				AssertEquals("[Row-5] TransactionReference02", "NLHHD - Import Notification", row5.Reference2);
				AssertEquals("[Row-5] TransactionReference03", "ECC000001 [ECC]", row5.Reference3);
				AssertNull("[Row-5] TransactionReference04", row5.Reference4);

				AssertEquals("[Row-6] CompanyCode", "DAU", row6.GetCompanyCode());
				AssertEquals("[Row-6] BranchCode", "GB0", row6.GetBranchCode());
				AssertEquals("[Row-6] TransactionDateUtc", new DateTime(2024, 12, 06, 13, 16, 00), row6.ServiceOccuredUTC);
				AssertEquals("[Row-6] TransactionReference01", "C00000009", row6.Reference1);
				AssertEquals("[Row-6] TransactionReference02", "NLRTM - Export Notification", row6.Reference2);
				AssertEquals("[Row-6] TransactionReference03", "EB000009 [EM]", row6.Reference3);
				AssertNull("[Row-6] TransactionReference04", row6.Reference4);
			});
		}

		protected override void PrepareTestData()
		{
			var sqlText = $@"
DECLARE @EiPk01 UNIQUEIDENTIFIER = NEWID();
DECLARE @EiPk02 UNIQUEIDENTIFIER = NEWID();
DECLARE @EiPk03 UNIQUEIDENTIFIER = NEWID();
DECLARE @EiPk04 UNIQUEIDENTIFIER = NEWID();
DECLARE @EiPk05 UNIQUEIDENTIFIER = NEWID();
DECLARE @EiPk06 UNIQUEIDENTIFIER = NEWID();
DECLARE @EiPk07 UNIQUEIDENTIFIER = NEWID();
DECLARE @EiPk08 UNIQUEIDENTIFIER = NEWID();
DECLARE @EiPk09 UNIQUEIDENTIFIER = NEWID();

DECLARE @EmPk01 UNIQUEIDENTIFIER = NEWID();
DECLARE @EmPk02 UNIQUEIDENTIFIER = NEWID();
DECLARE @EmPk03 UNIQUEIDENTIFIER = NEWID();
DECLARE @EmPk04 UNIQUEIDENTIFIER = NEWID();
DECLARE @EmPk05 UNIQUEIDENTIFIER = NEWID();
DECLARE @EmPk06 UNIQUEIDENTIFIER = NEWID();
DECLARE @EmPk07 UNIQUEIDENTIFIER = NEWID();
DECLARE @EmPk08 UNIQUEIDENTIFIER = NEWID();
DECLARE @EmPk09 UNIQUEIDENTIFIER = NEWID();

DECLARE @GcPk01 UNIQUEIDENTIFIER = NEWID();
DECLARE @GcPk02 UNIQUEIDENTIFIER = NEWID();
DECLARE @GcPk03 UNIQUEIDENTIFIER = NEWID();
DECLARE @GbPk00 UNIQUEIDENTIFIER = NEWID();
DECLARE @GbPk01 UNIQUEIDENTIFIER = NEWID();
DECLARE @GbPk02 UNIQUEIDENTIFIER = NEWID();
DECLARE @GbPk03 UNIQUEIDENTIFIER = NEWID();
DECLARE @GePk01 UNIQUEIDENTIFIER = NEWID();

DECLARE @JddPk01 UNIQUEIDENTIFIER = NEWID();
DECLARE @JddPk02 UNIQUEIDENTIFIER = NEWID();

DECLARE @SlPk01 UNIQUEIDENTIFIER = NEWID();
DECLARE @SlPk02 UNIQUEIDENTIFIER = NEWID();
DECLARE @SlPk03 UNIQUEIDENTIFIER = NEWID();
DECLARE @SlPk04 UNIQUEIDENTIFIER = NEWID();
DECLARE @SlPk05 UNIQUEIDENTIFIER = NEWID();
DECLARE @SlPk06 UNIQUEIDENTIFIER = NEWID();
DECLARE @SlPk07 UNIQUEIDENTIFIER = NEWID();
DECLARE @SlPk08 UNIQUEIDENTIFIER = NEWID();
DECLARE @SlPk09 UNIQUEIDENTIFIER = NEWID();

INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode) VALUES (@GcPk01, 'DAU', 'AU company', 'AUD', 'AU');
INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode) VALUES (@GcPk02, 'SHA', 'CN company1', 'CNY', 'CN');
INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode) VALUES (@GcPk03, 'SZG', 'CN company2', 'CNY', 'CN');
INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC, GB_OH_OrgProxy) VALUES (@GbPk00, 'GB0', @GcPk01, NULL);
INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC, GB_OH_OrgProxy) VALUES (@GbPk01, 'GB1', @GcPk01, NULL);
INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC, GB_OH_OrgProxy) VALUES (@GbPk02, 'GB2', @GcPk02, NULL);
INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC, GB_OH_OrgProxy) VALUES (@GbPk03, 'GB3', @GcPk03, NULL);
INSERT dbo.GlbDepartment (GE_PK, GE_Code) VALUES (@GePk01, 'DEP');

INSERT dbo.EDIInterchange (EI_PK, EI_To, EI_GB, EI_InterchangeNum, EI_SystemCreateTimeUtc, EI_SystemLastEditTimeUtc, EI_SystemCreateUser, EI_SystemLastEditUser) VALUES
	(@EiPk01, 'FORWARDING_PORT_MESSAGE', @GbPk01, 'EI01', '2021-03-09 00:23:00', '2021-03-09 00:23:00', '~AD', '~AD'),
	(@EiPk02, 'FORWARDING_PORT_MESSAGE', @GbPk01, 'EI02', '2021-03-09 00:23:00', '2021-03-09 00:23:00', '~AD', '~AD'),
	(@EiPk03, 'FORWARDING_PORT_MESSAGE', @GbPk01, 'EI03', '2021-03-09 00:23:00', '2021-03-09 00:23:00', '~AD', '~AD'),
	(@EiPk04, 'FORWARDING_PORT_MESSAGE', @GbPk01, 'EI04', '2021-03-09 00:23:00', '2021-03-09 00:23:00', '~AD', '~AD'),
	(@EiPk05, 'FORWARDING_PORT_MESSAGE', @GbPk01, 'EI05', '2021-03-09 00:23:00', '2021-03-09 00:23:00', '~AD', '~AD'),
	(@EiPk06, 'FORWARDING_PORT_MESSAGE', @GbPk01, 'EI06', '2021-03-09 00:23:00', '2021-03-09 00:23:00', '~AD', '~AD'),
	(@EiPk07, 'NOT_FORWARDING_PORT_MESSAGE', @GbPk01, 'EI07', '2021-03-09 00:23:00', '2021-03-09 00:23:00', '~AD', '~AD'),
	(@EiPk08, 'FORWARDING_PORT_MESSAGE', @GbPk01, 'EI08', '2021-03-09 00:23:00', '2021-03-09 00:23:00', '~AD', '~AD'),
	(@EiPk09, 'FORWARDING_PORT_MESSAGE', @GbPk01, 'EI09', '2021-03-09 00:23:00', '2021-03-09 00:23:00', '~AD', '~AD');

INSERT dbo.EDIMessage 
	(EM_PK, EM_GB, EM_GE, EM_SystemCreateTimeUtc, EM_ReceiveTransmit, EM_ApplicationCode, EM_MessageSubType, EM_MessageType, EM_Status, EM_SystemCreateUser, EM_SystemLastEditTimeUtc, EM_SystemLastEditUser, EM_EI, EM_MessageData) 
VALUES
	(@EmPk01, @GbPk01, @GePk01, '2024-12-06 13:13:00', 'TRX', 'UDM', 'XUS', 'CMD', 'SNT', 'DAT', '2017-05-01', 'DAT', @EiPk01, dbo.CLRCompressStringAsBytes('{GetMessage(PortbaseExportNotificationNamespace, "GB0", "C00000001", "NLRTM", "Export Notification", "EA000001", "EA", @"<SubShipment>
        <AddInfoCollection>
          <AddInfo>
            <Key>ReferenceNumber</Key>
            <Value>EAA000002</Value>
          </AddInfo>
          <AddInfo>
            <Key>EntryType_Code</Key>
            <Value>EAA</Value>
          </AddInfo>
        </AddInfoCollection>
      </SubShipment>")}')),
	(@EmPk02, @GbPk01, @GePk01, '2024-12-06 13:14:00', 'TRX', 'UDM', 'XUS', 'CMD', 'SNT', 'DAT', '2017-05-01', 'DAT', @EiPk02, CONVERT(VARBINARY(MAX),'{GetMessage(PortbaseExportNotificationNamespace, "GB1", "C00000002", "NLAMS", "Export Notification", "EB000001", "EB", string.Empty)}')),
	(@EmPk03, @GbPk01, @GePk01, '2024-12-06 13:15:00', 'TRX', 'UDM', 'XUS', 'CMD', 'SNT', 'DAT', '2017-05-01', 'DAT', @EiPk03, CONVERT(VARBINARY(MAX),'{GetMessage(PortbaseImportNotificationNamespace, "GB2", "C00000003", "NLHHD", "Import Notification", "EC000001", "EC", @"<SubShipment>
        <AddInfoCollection>
          <AddInfo>
            <Key>ReferenceNumber</Key>
            <Value>ECC000001</Value>
          </AddInfo>
          <AddInfo>
            <Key>EntryType_Code</Key>
            <Value>ECC</Value>
          </AddInfo>
        </AddInfoCollection>
      </SubShipment>")}')),
	(@EmPk04, @GbPk01, @GePk01, '2024-12-06 12:59:59', 'TRX', 'UDM', 'XUS', 'CMD', 'SNT', 'DAT', '2017-05-01', 'DAT', @EiPk04, CONVERT(VARBINARY(MAX),'{GetMessage(PortbaseExportNotificationNamespace, "GB0", "C00000004", "NLRTM", "Export Notification", "EXP002145", "EX", string.Empty)}')), -- Before date range
	(@EmPk05, @GbPk01, @GePk01, '2024-12-06 14:01:01', 'TRX', 'UDM', 'XUS', 'CMD', 'SNT', 'DAT', '2017-05-01', 'DAT', @EiPk05, CONVERT(VARBINARY(MAX),'{GetMessage(PortbaseExportNotificationNamespace, "GB0", "C00000005", "NLRTM", "Export Notification", "EXP002145", "EX", string.Empty)}')), -- After date range
	(@EmPk06, @GbPk01, @GePk01, '2024-12-06 13:13:00', 'TRX', 'UDM', 'XUE', 'CMD', 'SNT', 'DAT', '2017-05-01', 'DAT', @EiPk06, CONVERT(VARBINARY(MAX),'<UniversalEvent/>')),                                                                                                                      -- XUE
	(@EmPk07, @GbPk01, @GePk01, '2024-12-06 13:13:00', 'TRX', 'UDM', 'XUS', 'CMD', 'SNT', 'DAT', '2017-05-01', 'DAT', @EiPk07, CONVERT(VARBINARY(MAX),'{GetMessage(PortbaseExportNotificationNamespace, "GB0", "C00000007", "NLRTM", "Export Notification", "EXP002145", "EX", string.Empty)}')), -- Not to FORWARDING_PORT_MESSAGE
	(@EmPk08, @GbPk01, @GePk01, '2024-12-06 13:14:00', 'TRX', 'UDM', 'XUS', 'CMD', 'SNT', 'DAT', '2017-05-01', 'DAT', @EiPk08, CONVERT(VARBINARY(MAX),'{GetMessage(PortbaseExportNotificationNamespace, "GB0", "C00000008", "NLRTM", "Export Notification", "EB000008", "EL", string.Empty)}')), -- Receive IRJ
	(@EmPk09, @GbPk01, @GePk01, '2024-12-06 13:16:00', 'TRX', 'UDM', 'XUS', 'CMD', 'SNT', 'DAT', '2017-05-01', 'DAT', @EiPk09, CONVERT(VARBINARY(MAX),'{GetMessage(PortbaseExportNotificationNamespace, "GB0", "C00000009", "NLRTM", "Export Notification", "EB000009", "EM", string.Empty)}'));

INSERT dbo.JobDocumentData(JDD_PK, JDD_ParentTableCode, JDD_ParentID, JDD_Name, JDD_SystemCreateTimeUtc, JDD_SystemCreateUser, JDD_SystemLastEditTimeUtc, JDD_SystemLastEditUser) VALUES
	(@JddPk01, 'JK', NEWID(), 'JobConsol', '2021-03-01', '~AD', '2021-03-01', '~AD'),
	(@JddPk02, 'JK', NEWID(), 'JobConsol', '2021-03-01', '~AD', '2021-03-01', '~AD');

INSERT dbo.StmALog (SL_PK, SL_Table, SL_EventTime, SL_PostedTimeUtc, SL_Parent, SL_SE_NKEvent, SL_GB_NKBranch, SL_IsCancelled) VALUES
	(@SlPk01, 'JobDocumentData', GETDATE(), '2024-12-06 13:13:00', @JddPk01, 'DEX', 'SY2', 'N'),
	(@SlPk02, 'JobDocumentData', GETDATE(), '2024-12-06 13:13:00', @JddPk01, 'DEX', 'SY2', 'N'),
	(@SlPk03, 'JobDocumentData', GETDATE(), '2024-12-06 13:13:00', @JddPk01, 'DEX', 'SY2', 'N'),
	(@SlPk04, 'JobDocumentData', GETDATE(), '2024-12-06 13:13:00', @JddPk01, 'DEX', 'SY2', 'N'),
	(@SlPk05, 'JobDocumentData', GETDATE(), '2024-12-06 13:13:00', @JddPk01, 'DEX', 'SY2', 'N'),
	(@SlPk06, 'JobDocumentData', GETDATE(), '2024-12-06 13:13:00', @JddPk01, 'DEX', 'SY2', 'N'),
	(@SlPk07, 'JobDocumentData', GETDATE(), '2024-12-06 13:13:00', @JddPk01, 'DEX', 'SY2', 'N'),
	(NEWID(), 'JobDocumentData', GETDATE(), '2024-12-06 13:15:00', @JddPk01, 'ISN', 'SY2', 'N'),

	(@SlPk08, 'JobDocumentData', GETDATE(), '2024-12-06 13:14:00', @JddPk02, 'DEX', 'SY2', 'N'),
	(NEWID(), 'JobDocumentData', GETDATE(), '2024-12-06 13:15:00', @JddPk02, 'IRJ', 'SY2', 'N'),

	(@SlPk09, 'JobDocumentData', GETDATE(), '2024-12-06 13:16:00', @JddPk02, 'DEX', 'SY2', 'N'),
	(NEWID(), 'JobDocumentData', GETDATE(), '2024-12-06 13:17:00', @JddPk02, 'ISN', 'SY2', 'N');


INSERT dbo.GenPivot (XX_PK, XX_Relation1ID, XX_Relation2ID, XX_RelationType) VALUES
	(NEWID(), @SlPk01, @EmPk01, 'XEM'),
	(NEWID(), @SlPk02, @EmPk02, 'XEM'),
	(NEWID(), @SlPk03, @EmPk03, 'XEM'),
	(NEWID(), @SlPk04, @EmPk04, 'XEM'),
	(NEWID(), @SlPk05, @EmPk05, 'XEM'),
	(NEWID(), @SlPk06, @EmPk06, 'XEM'),
	(NEWID(), @SlPk07, @EmPk07, 'XEM'),
	(NEWID(), @SlPk08, @EmPk08, 'XEM'),
	(NEWID(), @SlPk09, @EmPk09, 'XEM');
";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		const string PortbaseExportNotificationNamespace = "http://www.cargowise.com/Schemas/Universal/2012/11/PortbaseExportNotification/1";
		const string PortbaseImportNotificationNamespace = "http://www.cargowise.com/Schemas/Universal/2012/11/PortbaseImportNotification/1";

		string GetMessage(string xmlNamespace, string branchCode, string consolKey, string operationalPortCode, string documentName, string referenceNumber, string entryTypeCode, string secondSubShipmentXML)
		{
			return @$"<UniversalShipment xmlns=""{xmlNamespace}"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <Key>{consolKey}</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>
      <DocumentaryOverride>
        <DataVersion>1</DataVersion>
        <DocumentName>{documentName}</DocumentName>
        <IsSystemDefined>true</IsSystemDefined>
        <Purpose>ORG</Purpose>
        <SubmissionVersion>2</SubmissionVersion>
      </DocumentaryOverride>
      <Workflow>
        <Company>
          <Code>DNL</Code>
          <Name>Your Netherlands Corp</Name>
        </Company>
        <EventBranch>{branchCode}</EventBranch>
        <TriggerCount>1</TriggerCount>
        <TriggerDate>2024-11-06T05:59:42.750+01:00</TriggerDate>
        <TriggerDescription />
        <TriggerType>Manual</TriggerType>
      </Workflow>
    </DataContext>
    <AddInfoCollection>
      <AddInfo>
        <Key>OperationalPort_Code</Key>
        <Value>{operationalPortCode}</Value>
      </AddInfo>
    </AddInfoCollection>
    <AdditionalReferenceCollection>
      <AdditionalReference>
        <Type Description=""Freight Forwarder Reference"">FFW</Type>
        <ReferenceNumber>C00001141</ReferenceNumber>
      </AdditionalReference>
    </AdditionalReferenceCollection>
    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>CurrentUser</AddressType>
        <AdditionalAddressInformation></AdditionalAddressInformation>
        <Address1>PRINS HENDRIKKADE 130,</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <City>AMSTERDAM</City>
        <CompanyName>YOUR NETHERLANDS CORP</CompanyName>
        <Contact>CargoWise Support</Contact>
        <Country Name=""Netherlands"">NL</Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum>99988877</GovRegNum>
        <GovRegNumType Description=""Chamber of Commerce Number"">CCN</GovRegNumType>
        <Phone></Phone>
        <Port Name=""Amsterdam"">NLAMS</Port>
        <Postcode>1011 AP</Postcode>
        <State></State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""VAT (BTW) Business Registration Num"">BTW</Type>
            <CountryOfIssue Name=""Netherlands"">NL</CountryOfIssue>
            <Value>123456789012</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""Chamber of Commerce Number"">CCN</Type>
            <CountryOfIssue Name=""Netherlands"">NL</CountryOfIssue>
            <Value>99988877</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>
    <SubShipmentCollection>
      <SubShipment>
        <AddInfoCollection>
          <AddInfo>
            <Key>ReferenceNumber</Key>
            <Value>{referenceNumber}</Value>
          </AddInfo>
          <AddInfo>
            <Key>EntryType_Code</Key>
            <Value>{entryTypeCode}</Value>
          </AddInfo>
        </AddInfoCollection>
        <ContainerCollection>
          <Container>
            <ContainerNumber>MAEU8754149</ContainerNumber>
            <GrossWeight>33382.000</GrossWeight>
            <Link>1</Link>
            <WeightUnit Description=""Kilograms"">KG</WeightUnit>
            <PackingLineCollection>
              <PackingLine>
                <ContainerLink>1</ContainerLink>
                <PackQty>1</PackQty>
                <PackType Description=""Piece"">PCE</PackType>
                <ReferenceNumber>S00001453</ReferenceNumber>
                <Weight>27852.000</Weight>
                <WeightUnit Description=""Kilograms"">KG</WeightUnit>
              </PackingLine>
            </PackingLineCollection>
          </Container>
        </ContainerCollection>
      </SubShipment>
{secondSubShipmentXML}
    </SubShipmentCollection>
  </Shipment>
</UniversalShipment>";
		}
	}
}
