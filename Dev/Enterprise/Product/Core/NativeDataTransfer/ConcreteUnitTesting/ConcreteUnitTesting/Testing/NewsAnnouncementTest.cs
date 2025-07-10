using System.IO;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.ConcreteUnitTesting
{
	class NewsAnnouncementTest : TestCaseWithFactory
	{
		public void TestExportNewsAnnouncement()
		{
			using (var stream = NativeDataTransferTestHelper.ExportToStream(Note))
			{
				var reader = new StreamReader(stream, Encoding.UTF8);
				string text = reader.ReadToEnd();
				CombineAssertions("NewsAnnouncement XML from import is incorrect", () =>
				{
					AssertContains($@"<GlbReleaseNote Action=""MERGE"">", text);
					AssertContains($"<PK>{Note.PK}</PK>", text);
					AssertContains($"<Summary>Test News</Summary>", text);
					AssertContains($"<URL>URL1</URL>", text);
					AssertContains($"<ReleaseNoteDate>2020-01-01T00:00:00</ReleaseNoteDate>", text);
					AssertContains($"<Section>{NewsSectionTypeList.Codes.ClientNews}</Section>", text);
					AssertContains($@"<CountryForReleaseNote TableName=""RefCountry"">", text);
					AssertContains($"<Code>{Core.Constants.CountryCodes.NewZealand}</Code>", text);
				});
			}
		}

		public void TestImportNewsAnnouncement()
		{
			var noteXml = string.Format(NewsAnnouncementXmlFormat, Note.PK, NewsSectionTypeList.Codes.ClientAnnouncements);
			var encoding = new UTF8Encoding();
			using (var stream = new MemoryStream(encoding.GetBytes(noteXml)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);
				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: NewsAnnouncement
--- Import Process Finished -----------------------------------------------------------
GlbReleaseNote - 0 inserts, 1 updates, 0 deletes
				".Trim();
				AssertMultilineASCIIEquals("Log Text on Update", expectedLog, manager.GetLogs());
			}

			var importedNote = new BusinessObjectFactory().Load<NewsAnnouncement>(Note.PK);
			CombineAssertions(() =>
			{
				AssertEquals("Test Announcement", importedNote.GF_Summary);
				AssertEquals("URL2", importedNote.GF_URL);
				AssertEquals(new ZDateTime(2020, 02, 15), importedNote.GF_ReleaseNoteDate);
				AssertEquals(NewsSectionTypeList.Codes.ClientAnnouncements, importedNote.GF_Section);
				AssertEquals(Core.Constants.CountryCodes.Australia, importedNote.GF_RN_NKCountryForReleaseNote);
			});
		}

		public void TestImportNewsAnnouncement_AddForAllCountries()
		{
			var noteQuery = new ZQuery(GlbReleaseNoteSchema.GF_Summary, "Test Announcement");
			noteQuery.AddToFilter(GlbReleaseNoteSchema.GF_URL, "URL2");
			noteQuery.AddToFilter(GlbReleaseNoteSchema.GF_ReleaseNoteDate, new ZDateTime(2020, 02, 15));
			noteQuery.AddToFilter(GlbReleaseNoteSchema.GF_Section, NewsSectionTypeList.Codes.ClientAnnouncements);
			noteQuery.AddToFilter(GlbReleaseNoteSchema.GF_RN_NKCountryForReleaseNote, "");
			Assert("Precondition - NewsAnnouncement should not be present", !Factory.Exists(typeof(NewsAnnouncement), noteQuery));

			var encoding = new UTF8Encoding();
			using (var stream = new MemoryStream(encoding.GetBytes(NewsAnnouncementXml_AddForAllCountries)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);
				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: NewsAnnouncement
--- Import Process Finished -----------------------------------------------------------
GlbReleaseNote - 1 inserts, 0 updates, 0 deletes
				".Trim();
				AssertMultilineASCIIEquals("Log Text on Add", expectedLog, manager.GetLogs());
				Assert("NewsAnnouncement should have been inserted by import", Factory.Exists(typeof(NewsAnnouncement), noteQuery));
			}
		}

		public void TestImportNewsAnnouncement_InvalidSection()
		{
			var noteXml = string.Format(NewsAnnouncementXmlFormat, Note.PK, NewsSectionTypeList.Codes.ProductUpdates);
			var encoding = new UTF8Encoding();
			using (var stream = new MemoryStream(encoding.GetBytes(noteXml)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);
				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Record: NewsAnnouncement failed to Import:
NewsAnnouncement cannot be imported as C1U section is invalid.
Error occurred trying to import file. Please fix the error and try importing the file again.
--- Import Process Finished -----------------------------------------------------------
No insert/update action performed.
				".Trim();
				var actual = manager.GetLogs();
				AssertMultilineASCIIEquals("Log Text on Error", expectedLog, manager.GetLogs());
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			Note = Factory.New<NewsAnnouncement>();
			Note.GF_Summary = "Test News";
			Note.GF_URL = "URL1";
			Note.GF_ReleaseNoteDate = new ZDateTime(2020, 01, 01);
			Note.GF_Section = NewsSectionTypeList.Codes.ClientNews;
			Note.GF_RN_NKCountryForReleaseNote = Core.Constants.CountryCodes.NewZealand;
			Factory.Save();
		}

		NewsAnnouncement Note { get; set; }

		const string NewsAnnouncementXmlFormat = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>EDICUS</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <NewsAnnouncement version=""2.0"">
      <GlbReleaseNote Action=""MERGE"">
        <PK>{0}</PK>
        <Summary>Test Announcement</Summary>
        <URL>URL2</URL>
        <ReleaseNoteDate>2020-02-15T00:00:00</ReleaseNoteDate>
        <Section>{1}</Section>
        <CountryForReleaseNote TableName=""RefCountry"">
          <Code>AU</Code>
          <PK>e4eb6e97-aa78-4c46-bf86-35b7fbb5fb0e</PK>
        </CountryForReleaseNote>
      </GlbReleaseNote>
    </NewsAnnouncement>
  </Body>
</Native>";

		const string NewsAnnouncementXml_AddForAllCountries = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>EDICUS</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <NewsAnnouncement version=""2.0"">
      <GlbReleaseNote Action=""MERGE"">
        <Summary>Test Announcement</Summary>
        <URL>URL2</URL>
        <ReleaseNoteDate>2020-02-15T00:00:00</ReleaseNoteDate>
        <Section>CAM</Section>
        <CountryForReleaseNote TableName=""RefCountry"" />
      </GlbReleaseNote>
    </NewsAnnouncement>
  </Body>
</Native>";
	}
}
