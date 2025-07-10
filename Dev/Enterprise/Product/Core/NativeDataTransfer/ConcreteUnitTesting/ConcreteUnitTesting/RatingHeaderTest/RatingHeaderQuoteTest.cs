using System.IO;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;

namespace Enterprise.DataTransfer.Native.ConcreteUnitTesting.Testing
{
	public class RatingHeaderQuoteTest : TestCaseWithFactory
	{
		public void TestImportQuotation() => AssertImportQuotationSuccess(orgHeaderAAA.MainAddress.OA_Code, orgHeaderAAA.MainAddress.PK, orgHeaderAAA.OH_Code, orgHeaderAAA.PK, orgHeaderAAA, "Importing Quotation-XML should set OrgHeader");

		public void TestImportQuotation_OrgHeaderMainAddress_InvalidCode() => AssertImportQuotationSuccess("?????????", orgHeaderAAA.MainAddress.PK, orgHeaderAAA.OH_Code, orgHeaderAAA.PK, orgHeaderAAA, "GIVEN XML has invalid Address.Code THEN import should based on Address.PK");

		public void TestImportQuotation_OrgHeaderMainAddress_ExistingCode() => AssertImportQuotationSuccess(orgHeaderAAA.MainAddress.OA_Code, orgHeaderAAA.MainAddress.PK, orgHeaderAAA.OH_Code, orgHeaderAAA.PK, orgHeaderAAA, "GIVEN XML has AddressBBB.Code but different Address from AddressAAA.PK THEN import should based on AddressAAA.PK");

		public void TestImportQuotation_OrgHeaderMainAddress_InvalidPK() => AssertImportQuotationSuccess(orgHeaderAAA.MainAddress.OA_Code, ZGuid.NewZGuid(), orgHeaderAAA.OH_Code, orgHeaderAAA.PK, orgHeaderAAA, "GIVEN XML has invalid Address.PK THEN import should based on Address.Code");

		public void TestImportQuotation_OrgHeaderMainAddress_ExistingPK() => AssertImportQuotationSuccess(orgHeaderAAA.MainAddress.OA_Code, orgHeaderBBB.MainAddress.PK, orgHeaderAAA.OH_Code, orgHeaderAAA.PK, orgHeaderBBB, "GIVEN XML has AddressBBB.PK but different Address from AddressAAA.Code THEN import should based on AddressBBB.PK");

		public void TestImportQuotation_OrgHeaderMainAddress_InvalidCodeAndPK()
		{
			var invalidAddressCode = "?????????";
			var invalidAddressPK = ZGuid.NewZGuid();

			var xml = string.Format
			(
				importXML,
				invalidAddressCode,
				invalidAddressPK,
				orgHeaderAAA.OH_Code,
				orgHeaderAAA.PK
			);

			var expectedError = $@"--- Start Import Process --------------------------------------------------------------
Record: Rate failed to Import:
Could not insert/update the JobDocAddress (JobDocAddress) as it had an invalid reference to a Address (Address). There is no Address with the following values: [PK:{invalidAddressPK}] OR [OH:{orgHeaderAAA.PK}][Code:{invalidAddressCode}].
Error occurred trying to import file. Please fix the error and try importing the file again.
--- Import Process Finished -----------------------------------------------------------
No insert/update action performed.";

			AssertImportQuotationError(xml, expectedError, "GIVEN XML has invalid Address.Code and Address.PK THEN import should error");
		}

		public void TestImportQuotation_OrgHeader_InvalidCode() => AssertImportQuotationSuccess(orgHeaderAAA.MainAddress.OA_Code, orgHeaderAAA.MainAddress.PK, "??????", orgHeaderAAA.PK, orgHeaderAAA, "GIVEN XML has invalid OrgHeader.Code THEN import should based on Address.OrgHeader");

		public void TestImportQuotation_OrgHeader_ExistingCode() => AssertImportQuotationSuccess(orgHeaderAAA.MainAddress.OA_Code, orgHeaderAAA.MainAddress.PK, orgHeaderBBB.OH_Code, orgHeaderAAA.PK, orgHeaderAAA, "GIVEN XML has OrgHeaderBBB.Code but different OrgHeader from OrgHeaderAAA.THEN import should based on Address.OrgHeader");

		public void TestImportQuotation_OrgHeader_InvalidPK() => AssertImportQuotationSuccess(orgHeaderAAA.MainAddress.OA_Code, orgHeaderAAA.MainAddress.PK, orgHeaderAAA.OH_Code, ZGuid.NewZGuid(), orgHeaderAAA, "GIVEN XML has invalid OrgHeader.PK THEN import should based on Address.OrgHeader");

		public void TestImportQuotation_OrgHeader_ExistingPK() => AssertImportQuotationSuccess(orgHeaderAAA.MainAddress.OA_Code, orgHeaderAAA.MainAddress.PK, orgHeaderAAA.OH_Code, orgHeaderBBB.PK, orgHeaderAAA, "GIVEN XML has OrgHeaderBBB.PK but different OrgHeader from OrgHeaderAAA.Code THEN import should based on Address.OrgHeader i.e. OrgHeaderAAA");

		public void TestImportQuotation_OrgHeader_InvalidCodeAndPK()
		{
			var invalidOrgHeaderCode = "?????????";
			var invalidOrgHeaderPK = ZGuid.NewZGuid();

			var xml = string.Format
			(
				importXML,
				orgHeaderAAA.MainAddress.OA_Code,
				orgHeaderAAA.MainAddress.PK,
				invalidOrgHeaderCode,
				invalidOrgHeaderPK
			);

			var expectedError = $@"--- Start Import Process --------------------------------------------------------------
Record: Rate failed to Import:
Could not insert/update the Address (Address) as it had an invalid reference to a Organization (OrgHeader). There is no Organization with the following values: [PK:{invalidOrgHeaderPK}] OR [Code:{invalidOrgHeaderCode}].
Error occurred trying to import file. Please fix the error and try importing the file again.
--- Import Process Finished -----------------------------------------------------------
No insert/update action performed.";

			AssertImportQuotationError(xml, expectedError, "GIVEN XML has invalid OrgHeader.Code and OrgHeader.PK THEN import should error");
		}

		#region Implementation

		void AssertImportQuotationSuccess(ZString addressCode, ZGuid addressPK, ZString orgHeaderCode, ZGuid orgHeaderPK, OrgHeader expectedOrgHeader, string message = default)
		{
			var xml = string.Format
			(
				importXML,
				addressCode,
				addressPK,
				orgHeaderCode,
				orgHeaderPK
			);

			var encoding = new UTF8Encoding();
			using (var stream = new MemoryStream(encoding.GetBytes(xml)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				string expectedLog = @"--- Start Import Process --------------------------------------------------------------
Processed: Rate
--- Import Process Finished -----------------------------------------------------------
RatingHeader - 1 inserts, 0 updates, 0 deletes
JobDocAddress - 1 inserts, 0 updates, 0 deletes".Trim();

				AssertMultilineASCIIEquals("Log Text on Add", expectedLog, manager.GetLogs());

				var newFactory = new BusinessObjectFactory();
				var loadedQuote = newFactory.LoadTop1<RatingHeader>(new ZQuery()) as Quote;
				var loadedQuoteOrgHeader = newFactory.Load<OrgHeader>(loadedQuote.TH_OH);

				CombineAssertions($"Following fields should be set - {message}", () =>
				{
					AssertEquals("Type", RatingConstants.RatingHeaderTypes.Quote, loadedQuote.TH_RateType);
					AssertEquals("Quote Number", "00001161", loadedQuote.TH_QuoteNumber);
					AssertEquals("OrgHeader", expectedOrgHeader.PK, loadedQuote.TH_OH);
					AssertEquals("OrgHeader Code", expectedOrgHeader.OH_Code, loadedQuoteOrgHeader.OH_Code);
				});
			}
		}

		void AssertImportQuotationError(string xml, string expectedError, string message = default)
		{
			var encoding = new UTF8Encoding();
			using (var stream = new MemoryStream(encoding.GetBytes(xml)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				AssertMultilineASCIIEquals("Log Text on Add", expectedError, manager.GetLogs());
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			orgHeaderAAA = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderAAA.OH_Code = "AAA";

			orgHeaderBBB = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderBBB.OH_Code = "BBB";

			Factory.Save();
		}

		OrgHeader orgHeaderAAA;
		OrgHeader orgHeaderBBB;

		readonly string importXML = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns = ""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>EDICUS</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Rate version = ""2.0"">
      <RatingHeader Action = ""MERGE"">
        <PK>1328d1c6-1cd2-493c-b25d-a0e61aa891cd</PK>
        <IsCancelled>false</IsCancelled>
        <OneTimeQuote>false</OneTimeQuote>
        <QuoteNumber>00001161</QuoteNumber>
        <QuoteCancellationReason></QuoteCancellationReason>
        <QuoteDate>2019-10-24T00:00:00</QuoteDate>
        <QuoteEndDate>2019-11-24T00:00:00</QuoteEndDate>
        <FollowUpDate>2019-10-31T00:00:00</FollowUpDate>
        <Accepted></Accepted>
        <IsLocked>false</IsLocked>
        <IsOneOffQuoteConsumed>false</IsOneOffQuoteConsumed>
        <RateType>QTE</RateType>
        <GlobalRateLevel>0</GlobalRateLevel>
        <GlobalRateDescription></GlobalRateDescription>
        <AirCFX>0.00</AirCFX>
        <SeaCFX>1.11</SeaCFX>
        <ExportAirCFX>0.00</ExportAirCFX>
        <ExportSeaCFX>2.02</ExportSeaCFX>
        <PrintRateLevelOriginCharges>true</PrintRateLevelOriginCharges>
        <PrintRateLevelDestinationCharges>true</PrintRateLevelDestinationCharges>
        <PrintInheritedOriginCharges>true</PrintInheritedOriginCharges>
        <PrintInheritedDestinationCharges>true</PrintInheritedDestinationCharges>
        <SystemLastEditTimeUtc>2019-10-24T00:05:00</SystemLastEditTimeUtc>
        <SystemCreateTimeUtc>2019-10-24T00:05:00</SystemCreateTimeUtc>
        <RateEntryCollection>
        </RateEntryCollection>
        <JobDocAddressCollection>
          <JobDocAddress Action = ""MERGE"">
            <PK>60d6efc9-4b6f-456a-a9a9-1c915eafaab4</PK>
            <AddressType>LCA</AddressType>
            <IsResidential>false</IsResidential>
            <AddressSequence>0</AddressSequence>
            <Contact></Contact>
            <AddressOverride>false</AddressOverride>
            <CompanyName></CompanyName>
            <Address1></Address1>
            <Address2></Address2>
            <City></City>
            <Postcode></Postcode>
            <State></State>
            <Phone></Phone>
            <Mobile></Mobile>
            <Fax></Fax>
            <GovRegNum></GovRegNum>
            <GovRegNumType>DEF</GovRegNumType>
            <Email></Email>
            <ValidationStatus>NRQ</ValidationStatus>
            <AddressMap></AddressMap>
            <SuppressAddressValidationError>false</SuppressAddressValidationError>
            <SystemCreateTimeUtc>2019-10-24T00:13:00</SystemCreateTimeUtc>
            <SystemLastEditTimeUtc>2019-10-24T00:13:00</SystemLastEditTimeUtc>
            <GeoLocation>POINT EMPTY</GeoLocation>
            <AdditionalAddressInformation></AdditionalAddressInformation>
            <ScreeningStatus>NOT</ScreeningStatus>
            <Address TableName = ""OrgAddress"">
              <Code>{0}</Code>
              <PK>{1}</PK>
              <OrgHeader>
                <Code>{2}</Code>
                <PK>{3}</PK>
              </OrgHeader>
            </Address>
          </JobDocAddress>
        </JobDocAddressCollection>
        <FirstSignatory TableName = ""GlbStaff"" />
        <SecondSignatory TableName = ""GlbStaff"" />
        <OrgHeader />
		<GlbCompany>
			<Code>EDI</Code>
			<PK>878d7aca-ffc3-49fc-9710-969ca0c0f2ac</PK>
		</GlbCompany>
      </RatingHeader>
    </Rate>
  </Body>
</Native>";

		#endregion
	}
}
