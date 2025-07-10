using System;
using System.IO;
using System.Text;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWiseOne.ResourceStrings;
using NUnit.Framework;

namespace Enterprise.ResourceStrings.Business.Testing
{
	sealed class XmlFileImporterTest : TestCase
	{
		public void TestValidLanguage()
		{
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("0347e369-92ca-4455-9276-d259edbfdfb8", new ResourceStringData("0347e369-92ca-4455-9276-d259edbfdfb8", "Master Bill"));

			using (var tempDirectory = new TempDirectory())
			using (var logger = new ImportLogger())
			{
				File.WriteAllText(Path.Combine(tempDirectory.DirectoryName, "Res.xml"),
@"<EnterpriseResources Language=""FR-FR"">
  <Res>
    <Key>0347e369-92ca-4455-9276-d259edbfdfb8</Key>
    <Caption>Connaissement</Caption>
  </Res>
</EnterpriseResources>
", Encoding.UTF8);
				using (var importer = new XmlFileImporter(logger) { EditReason = "TST" })
				{
					importer.Import(tempDirectory.DirectoryName, Core.SharedConstants.Languages.French);
				}
				AssertEquals(1, logger.Statistics[ImportStatus.Import]);
				var checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
				AssertEquals(1, checkedOutStrings.Length);
				AssertEquals(Core.SharedConstants.Languages.French, checkedOutStrings[0].HD_Language);
				AssertEquals("0347e369-92ca-4455-9276-d259edbfdfb8", checkedOutStrings[0].HD_Code);
				AssertEquals("Connaissement", checkedOutStrings[0].HD_Caption);
			}
		}

		public void TestKeyCaseChange()
		{
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("0347e369-92ca-4455-9276-d259edbfdfb8", new ResourceStringData("0347e369-92ca-4455-9276-d259edbfdfb8", "Master Bill"));
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.French).Put("0347e369-92ca-4455-9276-d259edbfdfb8", new ResourceStringData("0347e369-92ca-4455-9276-d259edbfdfb8", "Bill principal"));

			using (var tempDirectory = new TempDirectory())
			using (var logger = new ImportLogger())
			{
				File.WriteAllText(Path.Combine(tempDirectory.DirectoryName, "Res.xml"),
@"<EnterpriseResources Language=""FR-FR"">
  <Res>
    <Key>0347E369-92CA-4455-9276-D259EDBFDFB8</Key>
    <Caption>Connaissement</Caption>
  </Res>
</EnterpriseResources>
", Encoding.UTF8);
				using (var importer = new XmlFileImporter(logger) { EditReason = "TST" })
				{
					importer.Import(tempDirectory.DirectoryName, Core.SharedConstants.Languages.French);
				}
				AssertEquals(1, logger.Statistics[ImportStatus.Import]);
				var checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
				AssertEquals(1, checkedOutStrings.Length);
				AssertEquals(Core.SharedConstants.Languages.French, checkedOutStrings[0].HD_Language);
				AssertEquals("0347e369-92ca-4455-9276-d259edbfdfb8", checkedOutStrings[0].HD_Code);
				AssertEquals("Connaissement", checkedOutStrings[0].HD_Caption);
			}
		}

		public void TestUntranslated()
		{
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("k1", new ResourceStringData("k1", "One"));
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("k2", new ResourceStringData("k2", "2", string.Empty, "Two", string.Empty));
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("k3", new ResourceStringData("k3", "Three"));
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("k4", new ResourceStringData("k4", "4", string.Empty, "Four", string.Empty));

			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.French).Put("k3", new ResourceStringData("k3", "Trois"));
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.French).Put("k4", new ResourceStringData("k4", "Q.", string.Empty, "Quatre", string.Empty));

			using (var tempDirectory = new TempDirectory())
			using (var logger = new ImportLogger())
			{
				File.WriteAllText(Path.Combine(tempDirectory.DirectoryName, "Res.xml"),
@"<EnterpriseResources Language=""FR-FR"">
  <Res>
    <Key>k1</Key>
    <Caption>One</Caption>
  </Res>
  <Res>
    <Key>k2</Key>
	<ShortCaption>2</ShortCaption>
	<Caption>Deux</Caption>    
  </Res>
  <Res>
    <Key>k3</Key>
    <Caption>Three</Caption>
  </Res>
  <Res>
    <Key>k4</Key>
	<ShortCaption>4</ShortCaption>
	<Caption>Quatre</Caption>    
  </Res>
</EnterpriseResources>
", Encoding.UTF8);
				using (var importer = new XmlFileImporter(logger) { EditReason = "TST" })
				{
					importer.Import(tempDirectory.DirectoryName, Core.SharedConstants.Languages.French);
				}
				AssertEquals(4, logger.Statistics[ImportStatus.Import]);
				AssertEquals(0, logger.Statistics[ImportStatus.NotTranslated]);
				var checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
				AssertEquals(4, checkedOutStrings.Length);
				Array.Sort(checkedOutStrings, new Comparison<HelpDataString>(delegate(HelpDataString item1, HelpDataString item2)
				{ return item1.HD_Code.CompareTo(item2.HD_Code); }));

				AssertEquals("k1", checkedOutStrings[0].HD_Code);
				AssertEquals("One", checkedOutStrings[0].HD_Caption);

				AssertEquals("k2", checkedOutStrings[1].HD_Code);
				AssertEquals("Deux", checkedOutStrings[1].HD_Caption);
				AssertEquals("2", checkedOutStrings[1].HD_ShortCaption);

				AssertEquals("k3", checkedOutStrings[2].HD_Code);
				AssertEquals("Three", checkedOutStrings[2].HD_Caption);

				AssertEquals("k4", checkedOutStrings[3].HD_Code);
				AssertEquals("Quatre", checkedOutStrings[3].HD_Caption);
				AssertEquals("4", checkedOutStrings[3].HD_ShortCaption);
			}
		}

		public void TestSourceChangedUsingSourceHash()
		{
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("k1", new ResourceStringData("K1", "One"));
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("k2", new ResourceStringData("K2", "Two"));
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("k3", new ResourceStringData("K3", "Three"));

			using (var tempDirectory = new TempDirectory())
			using (var logger = new ImportLogger())
			{
				File.WriteAllText(Path.Combine(tempDirectory.DirectoryName, "Res.xml"),
@"<EnterpriseResources Language=""FR-FR"">
	<Res>
		<Key>k1</Key>
		<Caption>Un</Caption>
		<SourceHash>QOJJQfVTnPIJMSpRiFjJqw==</SourceHash>
	</Res>
	<Res>
		<Key>k2</Key>
		<ShortCaption>2</ShortCaption>
		<Caption>Deux</Caption>
		<SourceHash>wb0kZb87UKtluq+X/KrsKQ==</SourceHash>
	</Res>
	<Res>
		<Key>k3</Key>
		<Caption>trois</Caption>
		<SourceHash>H4nhidbKwhu+IRJI43uVmQ==</SourceHash>
	</Res>
</EnterpriseResources>", Encoding.UTF8);

				using (var impoter = new XmlFileImporter(logger) { EditReason = "TST" })
				{
					impoter.Import(tempDirectory.DirectoryName, Core.SharedConstants.Languages.French);
				}
				AssertEquals(1, logger.Statistics[ImportStatus.Import]);
				AssertEquals(2, logger.Statistics[ImportStatus.SourceChanged]);
				var checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
				AssertEquals(1, checkedOutStrings.Length);

				AssertEquals("k1", checkedOutStrings[0].HD_Code);
				AssertEquals("Un", checkedOutStrings[0].HD_Caption);
			}
		}

		public void TestXmlExceptionIndicatesFileName()
		{
			using (var tempDirectory = new TempDirectory())
			using (var logger = new ImportLogger())
			{
				File.WriteAllText(Path.Combine(tempDirectory.DirectoryName, "One.xml"),
@"<EnterpriseResources Language=""FR-FR"">
  <Res>
    <Key>1</Key>
    <Caption>Un</Caption>
  </Res>
</EnterpriseResources>
", Encoding.UTF8);
				File.WriteAllText(Path.Combine(tempDirectory.DirectoryName, "Two.xml"),
@"<EnterpriseResources Language=""FR-FR"">
  <Res>
    <Key1</Key>
    <Caption>Un</Caption>
  </Res>
</EnterpriseResources>
", Encoding.UTF8);
				using (var importer = new XmlFileImporter(logger) { EditReason = "TST" })
				{
					var ex = AssertExceptionThrown<XmlException>(() => importer.Import(tempDirectory.DirectoryName, Core.SharedConstants.Languages.French));
					AssertContains("Two.xml", ex.ToString());
				}
			}
		}

		public void TestParseErrorIndicatesInvalidText()
		{
			using (var tempDirectory = new TempDirectory())
			using (var logger = new ImportLogger())
			{
				File.WriteAllText(Path.Combine(tempDirectory.DirectoryName, "Res.xml"), @"<EnterpriseResources Language=""FR-FR""><Res><Key>1</Key><Caption>&Un</Caption></Res></EnterpriseResources>", Encoding.UTF8);
				using (var importer = new XmlFileImporter(logger) { EditReason = "TST" })
				{
					var ex = AssertExceptionThrown<XmlException>(() => importer.Import(tempDirectory.DirectoryName, Core.SharedConstants.Languages.French));
					AssertContains("><Caption>&Un</Captio", ex.ToString());
				}
			}
		}

		public void TestParseErrorAtStartOfLine()
		{
			using (var tempDirectory = new TempDirectory())
			using (var logger = new ImportLogger())
			{
				File.WriteAllText(Path.Combine(tempDirectory.DirectoryName, "Res.xml"), @"<&EnterpriseResources Language=""FR-FR""><Res><Key>1</Key><Caption>Un</Caption></Res></EnterpriseResources>", Encoding.UTF8);
				using (var importer = new XmlFileImporter(logger) { EditReason = "TST" })
				{
					var ex = AssertExceptionThrown<XmlException>(() => importer.Import(tempDirectory.DirectoryName, Core.SharedConstants.Languages.French));
					AssertContains("<&Enterpris", ex.ToString());
				}
			}
		}

		public void TestParseErrorAtStartEndOfLine()
		{
			using (var tempDirectory = new TempDirectory())
			using (var logger = new ImportLogger())
			{
				File.WriteAllText(Path.Combine(tempDirectory.DirectoryName, "Res.xml"), @"<EnterpriseResources Language=""FR-FR""><Res><Key>1</Key><Caption>Un</Caption></Res></EnterpriseResources&>", Encoding.UTF8);
				using (var importer = new XmlFileImporter(logger) { EditReason = "TST" })
				{
					var ex = AssertExceptionThrown<XmlException>(() => importer.Import(tempDirectory.DirectoryName, Core.SharedConstants.Languages.French));
					AssertContains("eResources&>", ex.ToString());
				}
			}
		}

		protected override void SetUp()
		{
			resourceStringsMockSources = ResourceStringsFactory.MockSources();
			base.SetUp();
		}

		protected override void TearDown()
		{
			resourceStringsMockSources.Dispose();
			base.TearDown();
		}

		IDisposable resourceStringsMockSources;
	}
}
