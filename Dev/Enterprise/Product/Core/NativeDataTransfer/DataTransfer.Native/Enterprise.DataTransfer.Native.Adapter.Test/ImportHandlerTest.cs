using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Common.GUI.Import;
using Enterprise.DataTransfer.Common.Import;
using Enterprise.DataTransfer.Native.Adapter.ImportServices;
using Enterprise.DataTransfer.Native.Business.Requests;
using Enterprise.DataTransfer.Native.Business.Update;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Finders;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Repository;
using Enterprise.DataTransfer.Native.Common.Logging;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Adapter
{
	class ImportHandlerForTest : ImportHandler
	{
		public ImportHandlerForTest(AncillaryImportServices sessionServices) : base(sessionServices) { }

		protected override void ImportCore(ExportImportRequest request, UpdateContext context)
		{
			context.InterceptorSettings.Clear();
			context.IsInterceptorSettingsCleared = true;
			context.StackTraceInterceptorSettingsCleared = new StackTrace();

			base.ImportCore(request, context);
		}

		public ExportImportRequest DeserializeRequest_Exposed(BaseRequestDeserializer requestDeserializer, Stream stream)
		{
			return base.DeserializeRequest(requestDeserializer, stream);
		}

		public void ValidateRequest_Exposed(ExportImportRequest request)
		{
			base.ValidateRequest(request);
		}

		public UpdateContext ConvertContext_Exposed(HeaderData request)
		{
			return base.ConvertContext(request);
		}
	}

	[GuiTest]
	class ImportHandlerTest : TestCaseWithFactory
	{
		public void TestSetDefinitionFinder()
		{
			var importHandler = new ImportHandler(session);
			importHandler.SetDefinitionFinder(new[] { "testAssembly" });

			AssertNotNull(importHandler.Parser.DefinitionFinder);
			var cache = ((DefinitionFinder)importHandler.Parser.DefinitionFinder).Cache;
			Assert(cache is EntitySetDefinitionCache);

			var assemblyNames = ((EntitySetDefinitionCache)cache).DefinitionLocator.assemblyNames;

			AssertEquals(1, assemblyNames.Count());
			AssertEquals("testAssembly", assemblyNames.ToArray().First());
		}

		public void TestDeserializeRequest()
		{
			var importHandler = new ImportHandlerForTest(session);
			var stream = new MemoryStream(Encoding.ASCII.GetBytes(orgNoCodeNoMap));
			var requestDeserializer = RequestDeserializerBuilder.GetDeserializer(stream);
			var exportImportRequest = importHandler.DeserializeRequest_Exposed(requestDeserializer, stream);

			AssertEquals("BENGOVPRM", exportImportRequest.Settings.OwnerCode);
			Assert(!exportImportRequest.Settings.EnableCodeMapping);

			var expectedEntitySet = @"<Organization version=""2.0"" xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"">
      <OrgHeader Action=""MERGE"">
        <Code></Code>
        <IsActive>true</IsActive>
        <FullName>Organization NCNM</FullName>
        <IsConsignor>true</IsConsignor>
        <Language>EN</Language>
        <ScreeningStatus>UNK</ScreeningStatus>
        <OrgAddressCollection>
          <OrgAddress Action=""MERGE"">
            <IsActive>true</IsActive>
            <Code>123 SMALL ST</Code>
            <Language>EN</Language>
            <CompanyNameOverride></CompanyNameOverride>
            <Address1>123 SMALL ST</Address1>
            <City>SMALLVILLE</City>
            <State>SMALL</State>
            <PostCode>00001</PostCode>
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
              <Code>FI999</Code>
            </RelatedPortCode>
          </OrgAddress>
        </OrgAddressCollection>
        <ClosestPort TableName=""RefUNLOCO"">
          <Code>FI999</Code>
        </ClosestPort>
      </OrgHeader>
    </Organization>";
			AssertEquals(expectedEntitySet, exportImportRequest.EntitySets.First().ToString());
		}

		public void TestValidateRequest()
		{
			var importHandler = new ImportHandlerForTest(session);

			AssertExceptionThrown<NativeXMLUserVisibleException>("", "Request must not be empty.", () => importHandler.ValidateRequest_Exposed(new ExportImportRequest(null)));

			var request = new Request();
			AssertExceptionThrown<NativeXMLUserVisibleException>("", "Body element must not be empty.", () => importHandler.ValidateRequest_Exposed(new ExportImportRequest(request)));
		}

		public void TestConvertContext()
		{
			var importHandler = new ImportHandlerForTest(session);
			var context = importHandler.ConvertContext_Exposed(null);
			Assert(context.IsInterceptorSettingsAdded);
			AssertEquals(27, context.InterceptorSettings.Count);
		}

		public void TestImport()
		{
			var importHandler = new ImportHandler(session);
			importHandler.Import(new MemoryStream(Encoding.ASCII.GetBytes(orgNoCodeNoMap)));

			var expectedLog = @"OrgHeader - 1 inserts, 0 updates, 0 deletes
OrgAddress - 1 inserts, 0 updates, 0 deletes
OrgAddressCapability - 1 inserts, 0 updates, 0 deletes".Trim();

			AssertEquals(expectedLog, string.Join("\r\n", ((MemoryLogger)session.Logger).Buffer.Logs().Select(log => log.Message).ToArray()));
		}

		const string orgNoCodeNoMap = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>BENGOVPRM</OwnerCode>
    <EnableCodeMapping>false</EnableCodeMapping>
  </Header>
  <Body>
    <Organization version=""2.0"">
      <OrgHeader Action=""MERGE"">
        <Code></Code>
        <IsActive>true</IsActive>
        <FullName>Organization NCNM</FullName>
        <IsConsignor>true</IsConsignor>
        <Language>EN</Language>
        <ScreeningStatus>UNK</ScreeningStatus>
        <OrgAddressCollection>
          <OrgAddress Action=""MERGE"">
            <IsActive>true</IsActive>
            <Code>123 SMALL ST</Code>
            <Language>EN</Language>
            <CompanyNameOverride></CompanyNameOverride>
            <Address1>123 SMALL ST</Address1>
            <City>SMALLVILLE</City>
            <State>SMALL</State>
            <PostCode>00001</PostCode>
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
              <Code>FI999</Code>
            </RelatedPortCode>
          </OrgAddress>
        </OrgAddressCollection>
        <ClosestPort TableName=""RefUNLOCO"">
          <Code>FI999</Code>
        </ClosestPort>
      </OrgHeader>
    </Organization>
  </Body>
</Native>";

		public void TestImportCreatesCorrectErrorMessageInLog()
		{
			form.ImportService = service.Object;
			form.SetLoggerForTesting(session.Logger);
			service.Object.BeforeProcess.Invoke();
			service.Object.AfterProcess.Invoke();
			service.Object.ErrorOccur.Invoke("Fred Flintstone ", new NativeXMLUserVisibleException("Fred is dead"));

			var expectedLog = @"Start Import Process
-----------------------------------------------------------------
Import Process Finished
-----------------------------------------------------------------
Record: 
Fred Flintstone 
failed to Import:
Fred is dead
-----------------------------------------------------------------
			".Trim();

			AssertMultilineASCIIEquals("Log did not show expected messages", expectedLog, string.Join("\r\n", ((MemoryLogger)form.Logger).Buffer.Logs().Select(log => log.Message).ToArray()));
		}

		public void TestImportNativeXmlDIMEventCreatedCorrectly()
		{
			#region requestXml
			var requestXml = @"<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"">
  <Body>
    <Organization>
      <OrgHeader Action=""MERGE"">
        <Code>RACBURDBO</Code>
        <FullName>RACK AND BURLINGTON</FullName>
        <Language>EN</Language>
        <IsActive>true</IsActive>
        <IsConsignee>true</IsConsignee>
        <IsConsignor>true</IsConsignor>
        <IsTransportClient>true</IsTransportClient>
        <IsWarehouseClient>true</IsWarehouseClient>
        <IsForwarder>true</IsForwarder>
        <IsShippingProvider>true</IsShippingProvider>
        <Category>BUS</Category>
        <ScreeningStatus>NOT</ScreeningStatus>
        <OrgAddressCollection>
          <OrgAddress Action=""MERGE"">
            <Code>12 Bundy Street</Code>
            <Address1>12 Bundy Street</Address1>
            <State>NSW</State>
            <PostCode>2827</PostCode>
            <FCLEquipmentNeeded>WUP</FCLEquipmentNeeded>
            <LCLEquipmentNeeded>PSL</LCLEquipmentNeeded>
            <AIREquipmentNeeded>PSL</AIREquipmentNeeded>
            <IsActive>true</IsActive>
            <AuthorityToLeave>DEF</AuthorityToLeave>
            <City>Dubbo</City>
            <Language>EN</Language>
            <OrgAddressCapabilityCollection>
              <OrgAddressCapability Action=""MERGE"">
                <AddressType>OFC</AddressType>
                <IsMainAddress>true</IsMainAddress>
              </OrgAddressCapability>
            </OrgAddressCapabilityCollection>
            <RelatedPortCode TableName=""RefUNLOCO"">
              <Code>AUDBO</Code>
            </RelatedPortCode>
            <CountryCode TableName=""RefCountry"">
              <Code>AU</Code>
            </CountryCode>
          </OrgAddress>
        </OrgAddressCollection>
        <ClosestPort TableName=""RefUNLOCO"">
          <Code>AUDBO</Code>
        </ClosestPort>
      </OrgHeader>
    </Organization>
  </Body>
</Native>";
			#endregion

			var originalLogsCount = Factory.GetDatabaseCount(typeof(StmALog));

			form2.SetLoggerForTesting(session.Logger);
			form2.ImportService = new ImportHandler(session);
			form2.ImportService.Import(new MemoryStream(Encoding.ASCII.GetBytes(requestXml)));

			var expectedLog = @"Start Import Process
-----------------------------------------------------------------
Import Process Finished
-----------------------------------------------------------------
OrgHeader - 1 inserts, 0 updates, 0 deletes
OrgAddress - 1 inserts, 0 updates, 0 deletes
OrgAddressCapability - 1 inserts, 0 updates, 0 deletes
			".Trim();

			AssertMultilineASCIIEquals("Log did not show expected messages", expectedLog, string.Join("\r\n", ((MemoryLogger)form2.Logger).Buffer.Logs().Select(log => log.Message).ToArray()));

			var currentLogsCount = Factory.GetDatabaseCount(typeof(StmALog));
			AssertEquals("logs.Length == 1", 1, currentLogsCount - originalLogsCount);

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataImportCode);
			var dimEvent = Factory.LoadTop1<IStmALog>(query);

			CombineAssertions("dimEvent", () =>
			{
				AssertEquals("dimEvent.SL_GB_NKBranch", Environment.Env.CurrentBranch.Code, dimEvent.SL_GB_NKBranch);
				AssertEquals("dimEvent.SL_GE_NKDepartment", Environment.Env.CurrentDepartment.Code, dimEvent.SL_GE_NKDepartment);
			});
		}

		public void TestImportWithUnknownOrganisation()
		{
			form2.SetLoggerForTesting(session.Logger);
			form2.ImportService = new ImportHandler(session);
			form2.ImportService.Import(new MemoryStream(Encoding.ASCII.GetBytes(impHandOrg)));

			var expectedLog = @"failed to Import:
Invalid XML for product 450411761511111.".Trim();

			AssertContains(expectedLog, string.Join("\r\n", ((MemoryLogger)form2.Logger).Buffer.Logs().Select(log => log.Message).ToArray()));
		}

		public void TestImportOccursExceptionForIncorrectInterceptorSettings()
		{
			var encoding = new UTF8Encoding();
			using (var stream = new MemoryStream(encoding.GetBytes(orgNoCodeNoMap)))
			{
				form2.SetLoggerForTesting(session.Logger);
				form2.ImportService = new ImportHandlerForTest(session);
				form2.ImportService.Import(stream);

				var expectedLog = @"failed to Import:
Cannot import to table 'OrgHeader' because constraint 'Constraint_ShortCode' failed on column 'Code'".Trim();

				AssertContains(expectedLog, string.Join("\r\n", ((MemoryLogger)form2.Logger).Buffer.Logs().Select(log => log.Message).ToArray()));
				ErrorReporter.Clear();
			}
		}

		public void TestImportWithoutAnyAction()
		{
			form2.SetLoggerForTesting(session.Logger);
			form2.ImportService = new ImportHandler(session);
			form2.ImportService.Import(new MemoryStream(Encoding.ASCII.GetBytes(string.Format(impNoAction, string.Empty, string.Empty))));

			var expectedLog = @"No insert/update action performed.
There were no ‘Action’ attributes included in the XML provided. Without ‘Action’ attributes, no data changes will be made. Please include 'Action' attributes (UPDATE/DELETE/INSERT/MERGE) in the XML to let the system know how to apply any updates required.".Trim();

			AssertContains(expectedLog, string.Join("\r\n", ((MemoryLogger)session.Logger).Buffer.Logs().Select(log => log.Message).ToArray()));
		}

		public void TestImport_DoNotShowNoActionMessageWhenMalformed_NoActualAction()
		{
			TestImport_DoNotShowNoActionMessageWhenMalformedCore(string.Empty);
		}
		public void TestImport_DoNotShowNoActionMessageWhenMalformed_HasAnAction()
		{
			TestImport_DoNotShowNoActionMessageWhenMalformedCore(@" Action=""MERGE""");
		}

		void TestImport_DoNotShowNoActionMessageWhenMalformedCore(string action)
		{
			form2.SetLoggerForTesting(session.Logger);
			form2.ImportService = new ImportHandler(session);
			var importString = string.Format(impNoAction, action, "\r\n\t\t\t<PK>xxxxxxxx-llll-rrrr-tttt-ec23a7fe988a</PK>");
			form2.ImportService.Import(new MemoryStream(Encoding.ASCII.GetBytes(importString)));

			var expectedLog = @"
failed to Import:
Error Parsing InternalPK: Guid should contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx), but was 'xxxxxxxx-llll-rrrr-tttt-ec23a7fe988a'.
-----------------------------------------------------------------
Error occurred trying to import file. Please fix the error and try importing the file again.
Import Process Finished
-----------------------------------------------------------------
No insert/update action performed."
				.Trim();
			var noActionMessage =
				"There were no ‘Action’ attributes included in the XML provided. Without ‘Action’ attributes, no data changes will be made." +
				" Please include 'Action' attributes (UPDATE/DELETE/INSERT/MERGE) in the XML to let the system know how to apply any updates required.";
			var logs = string.Join("\r\n", ((MemoryLogger)session.Logger).Buffer.Logs().Select(log => log.Message).ToArray());
			AssertContains(expectedLog, logs);

			if (string.IsNullOrEmpty(action))
			{
				AssertContains(noActionMessage, logs);
			}
			else
			{
				AssertNotContains(noActionMessage, logs);
			}
		}

		const string impNoAction = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"">
	<Header/>
	<Body>
		<Product>
			<OrgSupplierPart{0}>{1}
				<PartNum>450411761511111</PartNum>
				<StockKeepingUnit>PR</StockKeepingUnit>
				<Weight>0</Weight>
				<Cubic>0</Cubic>
				<Depth>0</Depth>
				<Height>0</Height>
				<Width>0</Width>
				<NetWeight>0</NetWeight>
				<Desc>NIKE AIR PRESTO ESSENTIAL</Desc>
				<StmNoteCollection>
					<StmNote>
						<Description>Additional Data</Description>
						<NoteText> PO Creation Date=20160511,	</NoteText>
						<NoteType>INT</NoteType>
						<NoteContext>AAA</NoteContext>
					</StmNote>
					<StmNote>
						<Description>Extended Commercial Description</Description>
						<NoteText>First Sale Indicator -4 ,		</NoteText>
						<NoteType>INT</NoteType>
						<NoteContext>AAA</NoteContext>
					</StmNote>
				</StmNoteCollection>
			</OrgSupplierPart>
		</Product>
	</Body>
</Native>";

		public void TestImportWithUpdateNonExistentRecord()
		{
			form2.SetLoggerForTesting(session.Logger);
			form2.ImportService = new ImportHandler(session);
			form2.ImportService.Import(new MemoryStream(Encoding.ASCII.GetBytes(impNonExistent)));

			var expectedLog = @"failed to Import:
There is no UNLOCO with the following values: [Code:XXZZ_][PortName:Sydney(My Town)].".Trim();
			var notexpectedLog = @"This error has been submitted to WTG for further investigation.";

			AssertContains(expectedLog, string.Join("\r\n", ((MemoryLogger)form2.Logger).Buffer.Logs().Select(log => log.Message).ToArray()));

			AssertNotContains(notexpectedLog, string.Join("\r\n", ((MemoryLogger)form2.Logger).Buffer.Logs().Select(log => log.Message).ToArray()));
		}

		readonly string impNonExistent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native"">
	<Body>
		<UNLOCO>
			<RefUNLOCO Action =""Update"">
				<Code>XXZZ_</Code>
				<PortName>Sydney(My Town)</PortName>
			</RefUNLOCO>
		</UNLOCO>
	</Body>
</Native>
";

		const string impHandOrg = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"">
	<Header/>
	<Body>
		<Product>
			<OrgSupplierPart Action=""MERGE"">
				<PartNum>450411761511111</PartNum>
				<StockKeepingUnit>PR</StockKeepingUnit>
				<Weight>0</Weight>
				<Cubic>0</Cubic>
				<Depth>0</Depth>
				<Height>0</Height>
				<Width>0</Width>
				<NetWeight>0</NetWeight>
				<Desc>NIKE AIR PRESTO ESSENTIAL</Desc>
				<CusClassPartPivotCollection>
					<CusClassPartPivot Action=""MERGE"">
						<TariffNum>6404119020</TariffNum>
						<ChildType>HTI</ChildType>
						<ComponentCusClassPartPivotCollection/>
						<ChildListOrder>0</ChildListOrder>
						<Country>
							<Code>US</Code>
						</Country>
						<CusUSClassificationCollection>
							<CusUSClassification Action=""MERGE"">
								<SPI/>
								<PerUnitCost>19.27</PerUnitCost>
								<CountryOfOrigin Action=""MERGE"">
									<Code>VN</Code>
								</CountryOfOrigin>
								<PerUnitCostCurr Action=""MERGE"">
									<Code>USD</Code>
								</PerUnitCostCurr>
								<Nine802ValuePerUnit>0.39</Nine802ValuePerUnit>
								<Nine802ValuePerUnitCurr Action=""MERGE"">
									<Code>USD</Code>
								</Nine802ValuePerUnitCurr>
							</CusUSClassification>
						</CusUSClassificationCollection>
						<SupplementalTariff>9802008068</SupplementalTariff>
						<UsageComment/>
					</CusClassPartPivot>
				</CusClassPartPivotCollection>
				<OrgPartRelationCollection>
					<OrgPartRelation Action=""MERGE"">
						<Relationship>OWN</Relationship>
						<OrgHeader Action=""MERGE"">
							<Code>1244951</Code>
						</OrgHeader>
					</OrgPartRelation>
					<OrgPartRelation Action=""MERGE"">
						<Relationship>OWN</Relationship>
						<OrgHeader Action=""MERGE"">
							<Code>3766306</Code>
						</OrgHeader>
					</OrgPartRelation>
				</OrgPartRelationCollection>
				<StmNoteCollection>
					<StmNote Action=""MERGE"">
						<Description>Additional Data</Description>
						<NoteText> PO Creation Date=20160511,	</NoteText>
						<NoteType>INT</NoteType>
						<NoteContext>AAA</NoteContext>
					</StmNote>
					<StmNote Action=""MERGE"">
						<Description>Extended Commercial Description</Description>
						<NoteText>First Sale Indicator -4 ,		</NoteText>
						<NoteType>INT</NoteType>
						<NoteContext>AAA</NoteContext>
					</StmNote>
				</StmNoteCollection>
			</OrgSupplierPart>
		</Product>
	</Body>
</Native>";

		public void TestImportTrimsAndStripsCRLFFromAMarkedProperty()
		{
			var startIndex = orgNoCodeNoMapCRLFIncluded.IndexOf("<CompanyName>") + 13;
			var length = orgNoCodeNoMapCRLFIncluded.IndexOf("</CompanyName>") - startIndex;
			var translatedCompanyNameBeforeImporting = orgNoCodeNoMapCRLFIncluded.Substring(startIndex, length);

			AssertEquals("Precondition: Translated Address Company Name contains leading/trailing whitespace and CRLF/tab characters", " THIS \r\n\r\nIS  A TEST  NAME ", translatedCompanyNameBeforeImporting);

			var importHandler = new ImportHandlerForTest(session);
			var stream = new MemoryStream(Encoding.ASCII.GetBytes(orgNoCodeNoMapCRLFIncluded));
			var exportImportRequest = importHandler.DeserializeRequest_Exposed(RequestDeserializerBuilder.GetDeserializer(stream), stream);
			var entitySet = importHandler.Parser.Deserialize(exportImportRequest.EntitySets.First(), session);
			var translatedCompanyNameAfterImporting = (string)entitySet.Root.ChildrenCollection.First(el => el.EntityName.Equals("OrgAddress")).ChildrenCollection.First(el => el.EntityName.Equals("OrgTranslatedAddress"))["CompanyName"];

			AssertEquals("Translated Address Company Name should be stripped of all whitespace characters except spaces.", "THIS IS  A TEST  NAME", translatedCompanyNameAfterImporting);
		}

		public void TestImportDoesNotStripCRLFFromAnUnmarkedPropertyWithMaxLengthGreaterOrEqual200()
		{
			var startIndex = orgNoCodeNoMapCRLFIncluded.IndexOf("<EXHandlingInstuctions>") + "<EXHandlingInstuctions>".Length;
			var length = orgNoCodeNoMapCRLFIncluded.IndexOf("</EXHandlingInstuctions>") - startIndex;
			var exHandlingInstructionsBeforeImporting = orgNoCodeNoMapCRLFIncluded.Substring(startIndex, length);

			AssertEquals("Precondition: Extra Handling Instructions contains leading/trailing whitespace and CRLF/tab characters", " testing\r\n\r\ntest ", exHandlingInstructionsBeforeImporting);

			var importHandler = new ImportHandlerForTest(session);
			var stream = new MemoryStream(Encoding.ASCII.GetBytes(orgNoCodeNoMapCRLFIncluded));
			var exportImportRequest = importHandler.DeserializeRequest_Exposed(RequestDeserializerBuilder.GetDeserializer(stream), stream);
			var entitySet = importHandler.Parser.Deserialize(exportImportRequest.EntitySets.First(), session);
			var handlingInstructions = (string)entitySet.Root.ChildrenCollection.First(el => el.EntityName.Equals("OrgMiscServ"))["EXHandlingInstuctions"];
			AssertEquals("Extra Handling Instructions should not be stripped of any whitespace characters", " testing\r\n\r\ntest ", handlingInstructions);
		}

		public void TestStripCRLFInImportDoesNotCorruptXML()
		{
			var importHandler = new ImportHandler(session);
			AssertNoExceptionThrown(() => { importHandler.Import(new MemoryStream(Encoding.ASCII.GetBytes(orgNoCodeNoMapCRLFIncluded))); });
		}

		const string orgNoCodeNoMapCRLFIncluded = @"
<Organization version=""2.0"" xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"">
    <OrgHeader Action=""MERGE"">
    <Code></Code>
    <IsActive>true</IsActive>
    <FullName> Organization 
								NCN
		    M </FullName>
    <IsConsignor>true</IsConsignor>
    <Language>EN</Language>
    <ScreeningStatus>UNK</ScreeningStatus>
    <OrgAddressCollection>
        <OrgAddress Action=""MERGE"">
			<IsActive>true</IsActive>
			<Code>123 SMALL ST</Code>
			<Language>EN</Language>
			<CompanyNameOverride></CompanyNameOverride>
			<Address1>123 SMALL ST</Address1>
			<City>SMALLVILLE</City>
			<State>SMALL</State>
			<PostCode>00001</PostCode>
			<FCLEquipmentNeeded>WUP</FCLEquipmentNeeded>
			<LCLEquipmentNeeded>PSL</LCLEquipmentNeeded>
			<AIREquipmentNeeded>PSL</AIREquipmentNeeded>

			<OrgTranslatedAddressCollection>
                <OrgTranslatedAddress Action=""MERGE"">
                  <Language>EN</Language>
                  <Address1>YE OLDE TEST ADDRESSE</Address1>
                  <CompanyName> THIS 

IS  A TEST  NAME </CompanyName>
                </OrgTranslatedAddress>
            </OrgTranslatedAddressCollection>

			<OrgAddressCapabilityCollection>
				<OrgAddressCapability Action=""MERGE"">
				<AddressType>OFC</AddressType>
				<IsMainAddress>true</IsMainAddress>
				</OrgAddressCapability>
			</OrgAddressCapabilityCollection>
			<RelatedPortCode TableName=""RefUNLOCO"">
				<Code>FI999</Code>
			</RelatedPortCode>
        </OrgAddress>
    </OrgAddressCollection>
	<OrgMiscServ Action=""MERGE"">
		<EXHandlingInstuctions> testing

test </EXHandlingInstuctions>
	</OrgMiscServ>
    <ClosestPort TableName=""RefUNLOCO"">
        <Code>FI999</Code>
    </ClosestPort>
    </OrgHeader>
</Organization>";

		protected override void SetUp()
		{
			base.SetUp();
			session = new AncillaryImportServices();
			form = new DataImportForm<string>();
			form2 = new DataImportForm<XElement>();
			service = new Mock<DataImportService<string>>();
		}

		protected override void TearDown()
		{
			base.TearDown();
			form.Dispose();
			form2.Dispose();
		}

		AncillaryImportServices session;
		Mock<DataImportService<string>> service;
		DataImportForm<string> form;
		DataImportForm<XElement> form2;
	}
}
