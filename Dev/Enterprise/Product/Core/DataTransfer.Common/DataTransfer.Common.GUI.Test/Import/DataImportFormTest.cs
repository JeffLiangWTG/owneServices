#region Test
#if DEBUG

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Common.Import;
using Enterprise.DataTransfer.Native.Adapter.ImportServices;
using Enterprise.DataTransfer.Native.Business.Requests;
using Enterprise.DataTransfer.Native.Business.Update;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

[assembly: WTG.StaticAnalysis.Annotation.UsesConstants(typeof(AutoRateEntry.Schema))]

namespace Enterprise.DataTransfer.Common.GUI.Import
{
	[TestedType(typeof(DataImportForm<string>))]
	public class DataImportFormTest : ZFormBasherTest
	{
		public void TestImportService()
		{
			form.ImportService = service;
			AssertNotEquals(0, service.BeforeProcess.GetInvocationList().Length);
			AssertNotEquals(0, service.AfterProcess.GetInvocationList().Length);
			AssertNotEquals(0, service.BeforeUnitProcess.GetInvocationList().Length);
			AssertNotEquals(0, service.AfterUnitProcess.GetInvocationList().Length);
			AssertNotEquals(0, service.ErrorOccur.GetInvocationList().Length);
			AssertNotEquals(0, service.UnitProcessSuccess.GetInvocationList().Length);
		}

		public void TestRetrieveRates_DisplayErrorAsPartialContent()
		{
			var expected = @"<Rate version=""2.0"">
  <RatingHeader Action=""MERGE"">
    <IsCancelled>false</IsCancelled>
    <OneTimeQuote>false</OneTimeQuote>
    <QuoteNumber></QuoteNumber>
    <QuoteCancellationReason></QuoteCancellationReason>
    <QuoteDate></QuoteDate>
    <QuoteEndDate></QuoteEndDate>
    <FollowUpDate></FollowUpDate>
    <Accepted>2016-05-13T00:00:00</Accepted>
    <IsLocked>false</IsLocked>
    <IsOneOffQuoteConsumed>false</IsOneOffQuoteConsumed>
    <RateType>GLB</RateType>
    <GlobalRateLevel>0</GlobalRateLevel>
    <GlobalRateDescription>Standard Costs (TACT/General Rates)</GlobalRateDescription>
    <AirCFX>0.00</AirCFX>
    <SeaCFX>0.00</SeaCFX>
    <ExportAirCFX>0.00</ExportAirCFX>
    <ExportSeaCFX>0.00</ExportSeaCFX>
    <PrintRateLevelOriginCharges>true</PrintRateLevelOriginCharges>
    <PrintRateLevelDestinationCharges>true</PrintRateLevelDestinationCharges>
    <PrintInheritedOriginCharges>true</PrintInheritedOriginCharges>
    <PrintInheritedDestinationCharges>true</PrintInheritedDestinationCharges>
    <SystemLastEditTimeUtc>2016-05-13T06:22:00</SystemLastEditTimeUtc>
    <SystemCreateTimeUtc>2015-08-10T00:44:00</SystemCreateTimeUtc>
...";
			using (var newForm = new DataImportForm<XElement>())
			using (var resourceStream = GetType().Assembly.GetManifestResourceStream("Enterprise.DataTransfer.Common.GUI.Import.TestFiles.Rates.xml"))
			{
				var session = new AncillaryImportServices(newForm.Logger);
				newForm.ImportService = new ImportHandler(session);
				newForm.ImportService.Import(resourceStream);
				AssertContains(expected, newForm.ProgressTextBox.Text);
			}
		}

		public void TestImportXmlWithCLRF()
		{
				var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"">
	<Header>
		<EnableCodeMapping>false</EnableCodeMapping>
	</Header>
	<Body>
		<Organization>
			<OrgHeader Action=""MERGE"">
				<Code>FLAASPMEL</Code>
				<IsActive>true</IsActive>
				<FullName>FLAT ASP ROADWORKS
</FullName>
				<IsForwarder>true</IsForwarder>
				<ClosestPort>
					<Code>AUMEL</Code>
				</ClosestPort>
        <OrgContactCollection>
          <OrgContact Action=""MERGE"">
            <PK>7d4b40fe-c078-4766-a059-e855fd8907ab</PK>
            <ContactName>test</ContactName>
            <Salutation></Salutation>
            <Language>EN</Language>
            <NotifyMode>EML</NotifyMode>
            <Title>testt</Title>
            <JobCategory>EMU</JobCategory>
            <Phone></Phone>
            <PhoneExtension></PhoneExtension>
            <Fax></Fax>
            <Mobile></Mobile>
            <HomePhone></HomePhone>
            <Pager></Pager>
            <OtherPhone></OtherPhone>
            <WebContractSignedDate></WebContractSignedDate>
            <Birthday></Birthday>
            <YearJoinedIndustry></YearJoinedIndustry>
            <YearJoinedCompany></YearJoinedCompany>
            <ContactSource></ContactSource>
            <DetailsVerified></DetailsVerified>
            <Gender>false</Gender>
            <ProfilePhoto></ProfilePhoto>
            <IsActive>true</IsActive>
            <WebAccessEnabled>false</WebAccessEnabled>
            <AttachmentType>PDF</AttachmentType>
            <Email>test@example.com</Email>
            <PersonalInfo></PersonalInfo>
            <PasswordHash></PasswordHash>
            <PasswordHashIterations>0</PasswordHashIterations>
            <PasswordSalt></PasswordSalt>
            <SystemCreateTimeUtc>2020-12-13T23:37:00</SystemCreateTimeUtc>
            <SystemLastEditTimeUtc>2020-12-13T23:37:00</SystemLastEditTimeUtc>
            <WebAccessSuperseded>false</WebAccessSuperseded>
            <AddressOverride TableName=""OrgHeader"" />
            <OrgAddress />
            <Nationality TableName=""RefCountry"">
              <Code></Code>
            </Nationality>
          </OrgContact>
        </OrgContactCollection>
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
			</OrgHeader>
		</Organization>
	</Body>
</Native>";

			using (var newForm = new DataImportForm<XElement>())
			using (var resourceStream = new MemoryStream(Encoding.UTF8.GetBytes(xml.Replace("<PersonalInfo></PersonalInfo>", "<PersonalInfo>This\r\r\nis\n\n\n\rspecial\n\rinformation\r\nWe want to see what happens\r\nWhen this is imported in remotely\r\n\r\nAlrighty then</PersonalInfo>"))))
			{
				var session = new AncillaryImportServices(newForm.Logger);
				newForm.ImportService = new ImportHandler(session);
				newForm.ImportService.Import(resourceStream);
				AssertContains(@"
OrgHeader - 1 inserts, 0 updates, 0 deletes
OrgContact - 1 inserts, 0 updates, 0 deletes
OrgAddress - 1 inserts, 0 updates, 0 deletes
OrgAddressCapability - 1 inserts, 0 updates, 0 deletes", newForm.ProgressTextBox.Text);

				var orgHeader = new BusinessObjectFactory().LoadTop1<IOrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "FLAASPMEL"));
				var orgContact = new BusinessObjectFactory().LoadTop1<IOrgContact>(new ZQuery(OrgContactSchema.OC_ContactName, "test"));

				AssertEquals("CLRF should be removed from OH_FullName", "FLAT ASP ROADWORKS", orgHeader.OH_FullName);
				AssertEquals("CLRF should not be removed from OC_PersonalInfo", "This\r\n\r\nis\r\n\r\n\r\n\r\nspecial\r\n\r\ninformation\r\nWe want to see what happens\r\nWhen this is imported in remotely\r\n\r\nAlrighty then", orgContact.OC_PersonalInfo);
			}
		}

		public void TestImportWithDataContext()
		{
			var company = Factory.New<IGlbCompany>();
			((BusinessObject)company)[GlbCompanySchema.GC_Code] = "XXX";
			var branch = Factory.New<IGlbBranch>();
			((BusinessObject)branch)[GlbBranchSchema.GB_Code] = "XXX";
			((BusinessObject)branch)[GlbBranchSchema.GB_GC] = company.PK;
			Factory.Save();

			var requestXml = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""1.0"">
  <Header xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"">
    <OwnerCode>EDIDATEDI</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
    <nv:DataContext xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" xmlns:nv=""http://www.cargowise.com/Schemas/Native/2011/11"">
      <Company>
        <Code>{company.GC_Code}</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Eagle Datamation International</Name>
      </Company>
    </nv:DataContext>
  </Header>
  <Body>
    <Organization version=""1.0"">
      <OrgHeader Action=""MERGE"">
        <FullName>JR</FullName>
		<ClosestPort TableName=""RefUNLOCO"">
			<Code>BGSOF</Code>
		</ClosestPort>
	  </OrgHeader>
    </Organization>
  </Body>
</Native>";

			using (var newForm = new DataImportForm<XElement>())
			using (var resourceStream = new MemoryStream(Encoding.UTF8.GetBytes(requestXml)))
			{
				var session = new AncillaryImportServices(newForm.Logger);
				newForm.ImportService = new ImportHandler(session);
				newForm.ImportService.Import(resourceStream);
				AssertContains("OrgHeader - 1 inserts, 0 updates, 0 deletes", newForm.ProgressTextBox.Text);
				AssertContains("Targeting Branch 'XXX', Company 'XXX' from Data Context", newForm.ProgressTextBox.Text);
			}
		}

		public void TestPartialXmlHelper_DeepCopy()
		{
			var original = new XElement("Test", new XAttribute("Attribute", "Hello"), "Value");
			var copiedElement = PartialXmlHelper.DeepCopyNodeNameAndAttribute(original);
			copiedElement.Add("Value");
			Assert(string.Format("Expected:\r\n{0} \r\n Actual Result: \r\n {1}", original.ToString(), copiedElement.ToString()), XNode.DeepEquals(original, copiedElement));
			original.Attributes().First().Value = "Hi";
			AssertEquals("Precondition", "<Test Attribute=\"Hi\">Value</Test>", original.ToString());
			AssertNotEquals("<Test Attribute=\"Hi\">Value</Test>", copiedElement.ToString());
		}

		public void TestDateImportForm_UnhandledException()
		{
			var expected = @"failed to Import:
Failed because of UnhandledException.
This error has been submitted to WTG for further investigation.

   at Enterprise.DataTransfer.Native.Business.Xml.Deserializers.EntitySetXmlDeserializer.Deserialize(XElement element, AncillaryImportServices sessionServices) in ";
			using (var newForm = new DataImportForm<XElement>())
			using (var resourceStream = new MemoryStream(Encoding.UTF8.GetBytes(NativeInsertOrgUnhandledExceptionXML)))
			{
				var session = new AncillaryImportServices(newForm.Logger);
				newForm.ImportService = new ImportHandler(session);
				newForm.ImportService.Import(resourceStream);
				AssertContains(expected, newForm.ProgressTextBox.Text);
				AssertEquals("Unknown exception occurred while importing Native XML, If you are a developer looking at the issue (yes you!!) please handle the exception.\r\n\r\nIf this exception occured because of a problem with the incoming XML, please wrap this exception in an exception type that has the ExceptionVisibility.User attribute on it. (eg: NativeXMLUserVisibleException) That will cause the exception to be reported to the User instead of being reported to WTG as an Issue. Make sure that you provide a clear message for the new exception that a User can follow to understand and fix the processing error that has occurred.", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		public void TestInvalidXMLForImport_NativeXMLExceptionWithFriendlyMessage()
		{
			var xmlToImport = @"@PlaceHolder@<Body>
			<Organization version=""2.0"">
			<OrgHeader Action=""MERGE"">
			</OrgHeaderBadEnding>
			</Organization>
			</Body>";

			var expectedErrorMessage = @"Invalid XML format. Please check your file to solve this problem.";

			var xmlToImportOldDeserializer = xmlToImport.Replace("@PlaceHolder@", string.Empty);
			using (var newForm = new DataImportForm<XElement>())
			using (var resourceStream = new MemoryStream(Encoding.UTF8.GetBytes(xmlToImportOldDeserializer)))
			{
				var session = new AncillaryImportServices(newForm.Logger);
				newForm.ImportService = new ImportHandler(session);
				newForm.ImportService.Import(resourceStream);
				AssertContains(expectedErrorMessage, newForm.ProgressTextBox.Text);
				AssertNullOrEmpty(ErrorReporter.LastMessageReported);
			}

			var xmlToImportVersionedNativeDeserializer = xmlToImport.Replace("@PlaceHolder@", "<Native xmlns='http://www.cargowise.com/Schemas/Native/2011/11' version='2.0'>");
			using (var newForm = new DataImportForm<XElement>())
			using (var resourceStream = new MemoryStream(Encoding.UTF8.GetBytes(xmlToImportVersionedNativeDeserializer)))
			{
				var session = new AncillaryImportServices(newForm.Logger);
				newForm.ImportService = new ImportHandler(session);
				newForm.ImportService.Import(resourceStream);
				AssertContains(expectedErrorMessage, newForm.ProgressTextBox.Text);
				AssertNullOrEmpty(ErrorReporter.LastMessageReported);
			}

			var xmlToImportUnVersionedNativeDeserializer = xmlToImport.Replace("@PlaceHolder@", "<Native xmlns='http://www.cargowise.com/Schemas/Native'>");
			using (var newForm = new DataImportForm<XElement>())
			using (var resourceStream = new MemoryStream(Encoding.UTF8.GetBytes(xmlToImportUnVersionedNativeDeserializer)))
			{
				var session = new AncillaryImportServices(newForm.Logger);
				newForm.ImportService = new ImportHandler(session);
				newForm.ImportService.Import(resourceStream);
				AssertContains(expectedErrorMessage, newForm.ProgressTextBox.Text);
				AssertNullOrEmpty(ErrorReporter.LastMessageReported);
			}

			var xmlToImportUniversalDeserializer = xmlToImport.Replace("@PlaceHolder@", "<ReferenceData xmlns='http://www.cargowise.com/Schemas/Universal'>");
			using (var newForm = new DataImportForm<XElement>())
			using (var resourceStream = new MemoryStream(Encoding.UTF8.GetBytes(xmlToImportUniversalDeserializer)))
			{
				var session = new AncillaryImportServices(newForm.Logger);
				newForm.ImportService = new ImportHandler(session);
				newForm.ImportService.Import(resourceStream);
				AssertNullOrEmpty(ErrorReporter.LastMessageReported);
			}

			var invalidXmlToImport = xmlToImport.Replace("@PlaceHolder@", "<InvalidRootTag/>");
			using (var newForm = new DataImportForm<XElement>())
			using (var resourceStream = new MemoryStream(Encoding.UTF8.GetBytes(invalidXmlToImport)))
			{
				var session = new AncillaryImportServices(newForm.Logger);
				newForm.ImportService = new ImportHandler(session);
				newForm.ImportService.Import(resourceStream);
				AssertContains(expectedErrorMessage, newForm.ProgressTextBox.Text);
				AssertNullOrEmpty(ErrorReporter.LastMessageReported);
			}

			invalidXmlToImport = "this is not XML";
			using (var newForm = new DataImportForm<XElement>())
			using (var resourceStream = new MemoryStream(Encoding.UTF8.GetBytes(invalidXmlToImport)))
			{
				var session = new AncillaryImportServices(newForm.Logger);
				newForm.ImportService = new ImportHandler(session);
				newForm.ImportService.Import(resourceStream);
				AssertContains(expectedErrorMessage, newForm.ProgressTextBox.Text);
				AssertNullOrEmpty(ErrorReporter.LastMessageReported);
			}
		}

		public void TestInvalidStringForImport_FormatExceptionWithFriendlyMessage()
		{
			var noteContent = @"From: 123@e.e To: 456@e.e Email Content: Good morning all,";
			var xmlToImport = $@"<Communication version=""2.0"" xmlns=""http://www.cargowise.com/Schemas/Native/2011/11\"">
			<OrgSalesCall Action=""MERGE"">
					<CallDate>2020-04-29T00:32:28</CallDate>
					<NextCall>2020-04-29T00:32:28</NextCall>
					<SalesCallNotes>{noteContent}</SalesCallNotes>
			</OrgSalesCall>
			</Communication>";
			var expectedErrorMessage = $"The input is not a valid Base-64 string as it contains a non-base 64 character, more than two padding characters, or an illegal character among the padding characters.";

			using (var newForm = new DataImportForm<XElement>())
			using (var resourceStream = new MemoryStream(Encoding.UTF8.GetBytes(xmlToImport)))
			{
				var session = new AncillaryImportServices(newForm.Logger);
				newForm.ImportService = new ImportHandler(session);
				newForm.ImportService.Import(resourceStream);
				AssertContains(expectedErrorMessage, newForm.ProgressTextBox.Text);
				AssertNullOrEmpty(ErrorReporter.LastMessageReported);
			}
		}

		public void TestDateImportForm_ZDataExceptionWithFriendlyMessage()
		{
			var expected = @"failed to Import:
Database login failed - please check the server error log.
If the problem persists then please contact your system administrator.

Message: Cannot open database";

			SqlErrorCollection sqlErrors = SqlExceptionBuilder.CreateSqlErrorCollection(SqlExceptionBuilder.CreateSqlError(4060, 1, 1, "MyServer", "Cannot open database", "MyProcedure", 4));
			SqlException sqlException = SqlExceptionBuilder.CreateSqlException(sqlErrors);

			using (var newForm = new DataImportForm<XElement>())
			using (var resourceStream = new MemoryStream(Encoding.UTF8.GetBytes(NativeInsertOrgUnhandledExceptionXML)))
			{
				newForm.ImportService = new ImportHandler_ForTest(newForm.Logger, new ZDataException(sqlException, null, null));
				newForm.ImportService.Import(resourceStream);
				AssertContains(expected, newForm.ProgressTextBox.Text);
				AssertNotContains("at Enterprise.DataTransfer.Common.GUI.Import.DataImportFormTest.ImportHandler_ForTest.ImportCore", newForm.ProgressTextBox.Text);
				AssertNullOrEmpty(ErrorReporter.LastMessageReported);
			}
		}

		public void TestDateImportForm_ZDataExceptionWithoutFriendlyMessage()
		{
			var expected = @"failed to Import:
<ROW IS NULL>
InnerException Message = Get me a higher life.
This error has been submitted to WTG for further investigation.

   at Enterprise.DataTransfer.Common.GUI.Import.DataImportFormTest.ImportHandler_ForTest.ImportCore";

			var sqlErrors = SqlExceptionBuilder.CreateSqlErrorCollection(SqlExceptionBuilder.CreateSqlError(1, 2, 3, "MyServer", "Get me a higher life.", "MyProcedure", 4));
			SqlException sqlException = SqlExceptionBuilder.CreateSqlException(sqlErrors);
			using (var newForm = new DataImportForm<XElement>())
			using (var resourceStream = new MemoryStream(Encoding.UTF8.GetBytes(NativeInsertOrgUnhandledExceptionXML)))
			{
				newForm.ImportService = new ImportHandler_ForTest(newForm.Logger, new ZDataException(sqlException, null, null));
				newForm.ImportService.Import(resourceStream);
				AssertContains(expected, newForm.ProgressTextBox.Text);
				AssertEquals("Unknown exception occurred while importing Native XML, If you are a developer looking at the issue (yes you!!) please handle the exception.\r\n\r\nIf this exception occured because of a problem with the incoming XML, please wrap this exception in an exception type that has the ExceptionVisibility.User attribute on it. (eg: NativeXMLUserVisibleException) That will cause the exception to be reported to the User instead of being reported to WTG as an Issue. Make sure that you provide a clear message for the new exception that a User can follow to understand and fix the processing error that has occurred.", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		#region NativeInsertOrgUnhandledExceptionXML

		const string NativeInsertOrgUnhandledExceptionXML = @"<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"">
	<Body>
		<UnhandledException>Failed because of UnhandledException.</UnhandledException>
	</Body>
</Native>";

		#endregion

		public void TestValueExceedMaxLength()
		{
			const string stringExceedsMaxLengthInXMLFile = "11111111112222222222333333333344444444445555555555x";
			Assert("PRE: The length of ContractNumber exceeds the max length of the column", stringExceedsMaxLengthInXMLFile.Length > AutoRateEntry.Schema.TI_ContractNumberMaxLength);

			var expected = $@"failed to Import:
ERROR: RatingHeader.RateEntry.ContractNumber - Maximum length allowed in this column is {AutoRateEntry.Schema.TI_ContractNumberMaxLength} characters, {stringExceedsMaxLengthInXMLFile.Length} were provided - [{stringExceedsMaxLengthInXMLFile}].";
			using (var newForm = new DataImportForm<XElement>())
			using (var resourceStream = GetType().Assembly.GetManifestResourceStream("Enterprise.DataTransfer.Common.GUI.Import.TestFiles.Rates_MaxLength.xml"))
			{
				var session = new AncillaryImportServices(newForm.Logger);
				newForm.ImportService = new ImportHandler(session);
				newForm.ImportService.Import(resourceStream);
				AssertContains("ProgressTextBox should contains the expected error message", expected, newForm.ProgressTextBox.Text);
				ErrorReporter.Clear();
			}
		}

		public void TestBusinessEntity()
		{
			AssertNotNull(form.BusinessEntity);
		}

		[UseSnapshotProtection]
		public void TestConcurrencyErrorDisplayedToUser()
		{
			using (var form1 = new DataImportForm<XElement>())
			using (var form2 = new DataImportForm<XElement>())
			using (var resourceStream = GetType().Assembly.GetManifestResourceStream("Enterprise.DataTransfer.Common.GUI.Import.TestFiles.Staff.xml"))
			{
				var staff = Factory.New<IGlbStaff>();
				var staffPk = staff.PK;
				staff.GS_LoginName = "Fred";
				staff.GS_Code = "ABC";
				Factory.Save();

				form1.ImportService = new ImportHandler(new AncillaryImportServices(form1.Logger));
				form1.ImportService.Import(resourceStream);

				BusinessObjectFactory.ThrowExceptionWhenSaveCountReachesLimit(1, new ZDataConcurrencyException(new Exception(), ((INeedRow)staff).Row, Db.Connection));

				resourceStream.Position = 0;
				form2.ImportService = new ImportHandler(new AncillaryImportServices(form2.Logger));
				form2.ImportService.Import(resourceStream);

				var expectedErrorMessage = $"Concurrency error occurred. Another user has changed a row in the database that you are trying to update. You might like to check their changes before trying to import again.\r\nThe table name is 'GlbStaff'.\r\nThe row has PK '{staffPk}'.";
				AssertContains(expectedErrorMessage, form2.ProgressTextBox.Text);
			}
		}

		const string basicOrgXml = @"<Organization version=""2.0"" xmlns =""http://www.cargowise.com/Schemas/Native/2011/11"">
  <OrgHeader Action=""MERGE"">
    <PK>f13c9c6a-0da9-4f4e-ae62-55d7844227f6</PK>
    <Code>SOMEORG1</Code>
    <Language>ENG</Language>
    <IsActive>true</IsActive>
    <FullName>SOME ORG NAME</FullName>
    <OrgAddressCollection>
      <OrgAddress Action=""MERGE"">
        <PK>57c6760b-1339-4e4a-955d-99703d6e61e5</PK>
        <IsActive>true</IsActive>
        <Code>Address2</Code>
        <Language>ENG</Language>
        <Address1>2 Street</Address1>
        <City>City</City>
        <PostCode>1000</PostCode>
        <OrgAddressCapabilityCollection>
          <OrgAddressCapability Action=""MERGE"">
            <PK>dbe8d8d6-2653-44e2-9ad4-6e794b3e5e87</PK>
            <AddressType>PAD</AddressType>
            <IsMainAddress>true</IsMainAddress>
          </OrgAddressCapability>
        </OrgAddressCapabilityCollection>
      </OrgAddress>
    </OrgAddressCollection>
    <ClosestPort TableName=""RefUNLOCO"">
      <Code>BGSOF</Code>
    </ClosestPort>
  </OrgHeader>
</Organization>";

		public void TestDereferenceEntitiesAfterImport()
		{
			#region Imported Xml

			var importedXml = $@"
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>EDICUS</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
	{basicOrgXml}
  </Body>
</Native>";

			#endregion

			using (var form = new DataImportForm<XElement>())
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(importedXml)))
			{
				var services = new AncillaryImportServices(form.Logger);
				form.ImportService = new ImportHandler(services);
				form.ImportService.Import(stream);
				AssertEquals(services.EntitiesReferencingPK.Count, 0);
			}
		}

		public void TestImportMultipleEntitySets()
		{
			#region Imported Xml

			var importedXml = $@"
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>EDICUS</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
	{basicOrgXml}
	{basicOrgXml.Replace("<FullName>SOME ORG NAME</FullName>", "<FullName>UPDATED NAME</FullName>")}
  </Body>
</Native>";

			#endregion

			using (var form = new DataImportForm<XElement>())
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(importedXml)))
			{
				var services = new AncillaryImportServices(form.Logger);
				form.ImportService = new ImportHandler(services);
				form.ImportService.Import(stream);
				AssertContains("OrgHeader - 1 inserts, 1 updates, 0 deletes", form.ProgressTextBox.Text);
			}

			AssertNull("This org should be updated", Factory.LoadTop1<IOrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, "SOME ORG NAME")));
			AssertNotNull("Expecting an org with this name", Factory.LoadTop1<IOrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, "UPDATED NAME")));
		}

		public void TestImportOrgContactWithNoAuditLog()
		{
			#region Imported Xml

			var importedXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"">
	<Header>
		<EnableCodeMapping>false</EnableCodeMapping>
	</Header>
	<Body>
		<Organization>
			<OrgHeader Action=""MERGE"">
				<Code>FLAASPMEL</Code>
				<IsActive>true</IsActive>
				<FullName>FLAT ASP ROADWORKS
</FullName>
				<IsForwarder>true</IsForwarder>
				<ClosestPort>
					<Code>AUMEL</Code>
				</ClosestPort>
        <OrgContactCollection>
          <OrgContact Action=""MERGE"">
            <PK>7d4b40fe-c078-4766-a059-e855fd8907ab</PK>
            <ContactName>test</ContactName>
            <Salutation></Salutation>
            <Language>EN</Language>
            <NotifyMode>EML</NotifyMode>
            <Title>testt</Title>
            <JobCategory>EMU</JobCategory>
            <Phone></Phone>
            <PhoneExtension></PhoneExtension>
            <Fax></Fax>
            <Mobile></Mobile>
            <HomePhone></HomePhone>
            <Pager></Pager>
            <OtherPhone></OtherPhone>
            <WebContractSignedDate></WebContractSignedDate>
            <Birthday></Birthday>
            <YearJoinedIndustry></YearJoinedIndustry>
            <YearJoinedCompany></YearJoinedCompany>
            <ContactSource></ContactSource>
            <DetailsVerified></DetailsVerified>
            <Gender>false</Gender>
            <ProfilePhoto></ProfilePhoto>
            <IsActive>true</IsActive>
            <WebAccessEnabled>false</WebAccessEnabled>
            <AttachmentType>PDF</AttachmentType>
            <Email>test@example.com</Email>
            <PersonalInfo></PersonalInfo>
            <PasswordHash></PasswordHash>
            <PasswordHashIterations>0</PasswordHashIterations>
            <PasswordSalt></PasswordSalt>
            <SystemCreateTimeUtc>2020-12-13T23:37:00</SystemCreateTimeUtc>
            <SystemLastEditTimeUtc>2020-12-13T23:37:00</SystemLastEditTimeUtc>
            <WebAccessSuperseded>false</WebAccessSuperseded>
            <AddressOverride TableName=""OrgHeader"" />
            <OrgAddress />
            <Nationality TableName=""RefCountry"">
              <Code></Code>
            </Nationality>
          </OrgContact>
        </OrgContactCollection>
			</OrgHeader>
		</Organization>
	</Body>
</Native>";

			#endregion

			using (var form = new DataImportForm<XElement>())
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(importedXml)))
			{
				var services = new AncillaryImportServices(form.Logger);
				form.ImportService = new ImportHandler(services);

				var orgAuditlogs = Factory.Load<IStmALog>(new ZQuery(StmALogSchema.SL_Table, "OrgHeader")).Where(l => l.SL_SE_NKEvent == "ADD").ToArray();
				var contactlogs = Factory.Load<IStmALog>(new ZQuery(StmALogSchema.SL_Table, "OrgContact")).Where(l => l.SL_SE_NKEvent == "ADD").ToArray();
				Assert("Precondition: No Org Add Logs.", !orgAuditlogs.Any());
				Assert("Precondition: No Contact Add Logs.", !contactlogs.Any());

				form.ImportService.Import(stream);
				AssertContains("OrgHeader - 1 inserts, 0 updates, 0 deletes\r\nOrgContact - 1 inserts, 0 updates, 0 deletes", form.ProgressTextBox.Text);

				orgAuditlogs = Factory.Load<IStmALog>(new ZQuery(StmALogSchema.SL_Table, "OrgHeader")).Where(l => l.SL_SE_NKEvent == "ADD").ToArray();
				contactlogs = Factory.Load<IStmALog>(new ZQuery(StmALogSchema.SL_Table, "OrgContact")).Where(l => l.SL_SE_NKEvent == "ADD").ToArray();
				Assert("After import, there is still no Org Add Logs.", !orgAuditlogs.Any());
				Assert("After import, there is still no Contact Add Logs.", !contactlogs.Any());
			}
		}

		public void TestImportOrgNativeXmlWithNoAuditLog()
		{
			#region Imported Xml

			var importedXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"">
	<Header>
		<EnableCodeMapping>false</EnableCodeMapping>
	</Header>
	<Body>
		<Organization>
			<OrgHeader Action=""MERGE"">
				<Code>FLAASPMEL</Code>
				<IsActive>true</IsActive>
				<FullName>FLAT ASP ROADWORKS
</FullName>
				<IsForwarder>true</IsForwarder>
				<ClosestPort>
					<Code>AUMEL</Code>
				</ClosestPort>
			</OrgHeader>
		</Organization>
	</Body>
</Native>";

			#endregion

			using (var form = new DataImportForm<XElement>())
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(importedXml)))
			{
				var services = new AncillaryImportServices(form.Logger);
				form.ImportService = new ImportHandler(services);

				var orgAuditlogs = Factory.Load<IStmALog>(new ZQuery(StmALogSchema.SL_Table, "OrgHeader")).Where(l => l.SL_SE_NKEvent == "ADD").ToArray();
				Assert("Precondition: No Org Add Logs.", !orgAuditlogs.Any());

				form.ImportService.Import(stream);
				AssertContains("OrgHeader - 1 inserts, 0 updates, 0 deletes", form.ProgressTextBox.Text);

				orgAuditlogs = Factory.Load<IStmALog>(new ZQuery(StmALogSchema.SL_Table, "OrgHeader")).Where(l => l.SL_SE_NKEvent == "ADD").ToArray();
				Assert("After import, there is still no Org Add Log.", !orgAuditlogs.Any());
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			form = new DataImportForm<String>();
			var mock = new Mock<DataImportService<String>>();
			service = mock.Object;
		}

		protected override void TearDown()
		{
			base.TearDown();
			form.Dispose();
		}

		protected override bool ShouldIgnoreMissingBindingMember(Control control)
		{
			return control.Name == "progressTextBox";
		}

		DataImportService<String> service;
		DataImportForm<String> form;

		protected override Form GetFormToBashCore()
		{
			return form;
		}
		class ImportHandler_ForTest : ImportHandler
		{
			readonly Exception exceptionToThrow;
			public ImportHandler_ForTest(ILogger logger, Exception ex) : base(new AncillaryImportServices(logger))
			{
				exceptionToThrow = ex;
			}

			protected override void ImportCore(ExportImportRequest request, UpdateContext context)
			{
				throw exceptionToThrow;
			}
		}
	}
}

#endif
#endregion
