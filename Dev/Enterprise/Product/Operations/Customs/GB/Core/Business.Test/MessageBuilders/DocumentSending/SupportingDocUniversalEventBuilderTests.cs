namespace Enterprise.Customs.GB.Business.Test.MessageBuilders.DocumentSending
{
	using System.IO;
	using System.Linq;
	using CargoWise.Application;
	using CargoWise.EntityFramework.Testing;
	using CargoWise.IO;
	using CargoWise.Types;
	using Enterprise.Customs.GB.Business.Declaration;
	using Enterprise.Customs.GB.Business.DocumentSending;
	using Enterprise.Customs.Universal.Testing;
	using Enterprise.Integration.Licensing;
	using Enterprise.MasterFiles.Business;
	using Enterprise.MasterFiles.Integration;
	using Enterprise.UniversalDataBuss.XmlIO.XmlWriting;
	using UniversalEvent = UniversalDataBuss.DataObjects.Universal.Event;

	public class SupportingDocUniversalEventBuilderTests : TestCaseWithFactory
	{
		public void TestBuildUniversalEvent()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "HYE";
			registrationKey.ServerCodeForTest = "CMT";

			CombineAssertions("Catch Error in case pre-send check were missed", () =>
			{
				var sendingObject1 = SupportingDocSendingObject.New(declaration);
				sendingObject1.EDoc = eDoc1.UniqueKey;
				sendingObject1.DocumentType = "CCIV";
				var sendingObject2 = SupportingDocSendingObject.New(declaration);
				sendingObject2.EDoc = eDoc2.UniqueKey;
				sendingObject2.DocumentType = "MMCD";

				var sendingObjects = new SupportingDocSendingObject[] { sendingObject1, sendingObject2 };

				var eventBuilder = sendingObject1.GetSupportingDocUniversalEventBuilder();
				UniversalEvent[] universalEvents = eventBuilder.BuildUniversalEvent(sendingObjects).ToArray();
				try
				{
					AssertEquals(2, universalEvents.Length);
					AssertUniversalEvent(0, universalEvents[0],
	@"      <AttachedDocument>
        <FileName>Invoice.pdf</FileName>
        <ImageData>AA==</ImageData>

        <ContextCollection>
          <Context>
            <Type>DOCTYPE</Type>
            <Value>CCIV</Value>
          </Context>
          <Context>
            <Type>MIME</Type>
            <Value>application/pdf</Value>
          </Context>
        </ContextCollection>
      </AttachedDocument>");

					AssertUniversalEvent(1, universalEvents[1],
	@"      <AttachedDocument>
        <FileName>Invoice2.pdf</FileName>
        <ImageData>AAAAAA==</ImageData>

        <ContextCollection>
          <Context>
            <Type>DOCTYPE</Type>
            <Value>MMCD</Value>
          </Context>
          <Context>
            <Type>MIME</Type>
            <Value>application/pdf</Value>
          </Context>
        </ContextCollection>
      </AttachedDocument>");
				}
				finally
				{
					foreach (var universalEvent in universalEvents)
					{
						universalEvent.Dispose();
					}
				}
			});
		}

		void AssertUniversalEvent(int index, UniversalEvent universalEvent, ZString singleAttachedDocumentXNodeString)
		{
			string xml;

			using (var stream = (SubStreamableStream)new MemoryStream())
			using (var reader = new StreamReader(stream))
			{
				new XmlWriter().WriteXML(universalEvent, stream, false);
				stream.Flush();
				stream.Position = 0;
				xml = reader.ReadToEnd();
			}

			AssertMultilineASCIIEquals("Universal Event - " + index, ExpectedResult(universalEvent, declaration, singleAttachedDocumentXNodeString), ReplaceEventTime(xml, "2016-08-16T00:28:45.837"));
		}

		string ReplaceEventTime(string text, string dateString)
		{
			string startTag = "<EventTime>";
			string endTag = "</EventTime>";
			int startPos = text.IndexOf(startTag);
			int endPos = text.IndexOf(endTag);

			return text.Substring(0, startPos + startTag.Length) + dateString + text.Substring(endPos, text.Length - endPos);
		}

		string ExpectedResult(UniversalEvent universalEvent, JobDeclaration declaration, ZString singleAttachedDocumentXNodeString)
		{
			var company = declaration.Branch.Company;
			var server = universalEvent.DataContext.GetEnterpriseServerAndCompanyIDs();
			return @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>CustomsDeclaration</Type>
          <Key>" + universalEvent.DataContext.DataSourceCollection.First().Key + @"</Key>
        </DataSource>
      </DataSourceCollection>

      <Company>
        <Code>" + company.GC_Code + @"</Code>
        <Country>
          <Code>" + company.Country.RN_Code + @"</Code>
          <Name>" + company.Country.RN_Desc + @"</Name>
        </Country>
        <Name>" + company.GC_Name + @"</Name>
      </Company>
      <DataProvider>" + universalEvent.DataContext.DataProviderForCodeMapping + @"</DataProvider>
      <EnterpriseID>" + server.EnterpriseID + @"</EnterpriseID>
      <ServerID>" + server.ServerID + @"</ServerID>
    </DataContext>

    <EventTime>2016-08-16T00:28:45.837</EventTime>
    <EventType>DSN</EventType>
    <EventReference>|MST=DOCUPLOAD|SER=GBCustomsCDS</EventReference>

    <AttachedDocumentCollection>
" + singleAttachedDocumentXNodeString + @"
    </AttachedDocumentCollection>

    <ContextCollection>
      <Context>
        <Type>MRN</Type>
        <Value>12341234</Value>
      </Context>
      <Context>
        <Type>Key</Type>
        <Value>HYECMT.GB999999999888.ABC</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
";
		}

		#region Implementation

		protected override void SetUp()
		{
			CreateCusMaps();
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_CustomsProfile = "ABC";
			declaration.Branch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "GB999999999888");
			byte[] imageBytes = new byte[1];
			byte[] imageBytes2 = new byte[4];
			eDoc1 = declaration.DocManagerInfo.AddFileOrDocument(imageBytes, "Invoice.pdf", "CIV");
			eDoc2 = declaration.DocManagerInfo.AddFileOrDocument(imageBytes2, "Invoice2.pdf", "MCD");

			entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter("12341234", ZDateTime.UtcNow);
			instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			Factory.Save();
		}

		UniversalReferenceTestDataHelper helper;
		void CreateCusMaps()
		{
			if (helper == null)
			{
				helper = new UniversalReferenceTestDataHelper(Factory);
			}
			var gbCountry = Core.Constants.CountryCodes.UnitedKingdom;
			var cusMapType1 = helper.CreateCusMapType("REL", "BTH", "Related Party Indicator", true);
			var cusMapType2 = helper.CreateCusMapType("GBDOC", "OUT", "GB Supporting Document Types", false);
			var cusMapType4 = helper.CreateCusMapType("BOL", "OUT", "House Bill Types", true);

			var cusMap1 = helper.CreateCusMap("REL", "Y", "R", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 06, 06), gbCountry);
			var cusMap2 = helper.CreateCusMap("REL", "N", "N", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 06, 06), gbCountry);
			var cusMap3 = helper.CreateCusMap("REL", "E", "E", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 06, 06), gbCountry);
			var cusMap4 = helper.CreateCusMap("GBDOC", "DGF", "DGS", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 06, 06), gbCountry);
			var cusMap5 = helper.CreateCusMap("GBDOC", "CIV", "INV", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 06, 06), gbCountry);
			var cusMap6 = helper.CreateCusMap("GBDOC", "MCD", "OTH", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 06, 06), gbCountry);
			var cusMap7 = helper.CreateCusMap("GBDOC", "MSC", "OTH", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 06, 06), gbCountry);
			var cusMap8 = helper.CreateCusMap("GBDOC", "MFD", "OTH", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 06, 06), gbCountry);
			var cusMap9 = helper.CreateCusMap("GBDOC", "EXV", "VEC", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 06, 06), gbCountry);
			var cusMap10 = helper.CreateCusMap("GBDOC", "WMR", "WBC", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 06, 06), gbCountry);
			var cusMap18 = helper.CreateCusMap("BOL", "STD", "BOL", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 06, 06), gbCountry);
			var cusMap19 = helper.CreateCusMap("BOL", "CLD", "PBL", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 06, 06), gbCountry);
			Factory.Save();
		}

		JobDeclaration declaration;
		CusEntryHeader entryHeader;
		Customs.Business.CusEntryInstruction instruction;
		IeDoc eDoc1;
		IeDoc eDoc2;

		#endregion
	}
}
