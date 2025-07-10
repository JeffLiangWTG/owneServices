using System.IO;
using System.Linq;
using System.Xml.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DataTransfer.Native.Adapter;
using Enterprise.DataTransfer.Native.Adapter.Utils;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Finders;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Repository;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.ConcreteUnitTesting.Testing
{
	public class EDICodeMappingTest : TestCaseWithFactory
	{
		public void TestCanImportEDICodeMapping()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTDUMMYORG";

			Factory.Save();

			AssertEquals("Precondition: EDI Code Mapping Is Empty", 0, orgHeader.PatternMatchOverrides_ForBinding.Count);

			var country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Australia);
			var encoding = new System.Text.UTF8Encoding();
			using (var stream = new MemoryStream(encoding.GetBytes(string.Format(EDICodeMappingXml, country.PK))))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				var expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: EDICodeMapping
Processed: EDICodeMapping
--- Import Process Finished -----------------------------------------------------------
OrgPatternMatchOverride - 2 inserts, 0 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Log Text on Add", expectedLog, manager.GetLogs());
			}

			orgHeader.PatternMatchOverrides_ForBinding.Reload(true);
			AssertEquals("EDI Code Mappings Are Imported", 2, orgHeader.PatternMatchOverrides_ForBinding.Count);
			AssertContainsExactElementsInAnyOrder(
				new (ZString, ZString, ZGuid, ZString)[] { ("ABC", ZString.Empty, country.PK, "COU"), ("EFG", "AAS", ZGuid.Empty, "EVT") },
				orgHeader.PatternMatchOverrides_ForBinding.Cast<OrgPatternMatchOverride>().Select(u => (u.OO_ForeignCode, u.OO_LocalCode, u.OO_LocalGuid, u.OO_Relationship)));
		}

		public void TestCanExportEDICodeMapping()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTDUMMYORG";

			var patternMatchingOverride = orgHeader.PatternMatchOverrides_ForBinding.AddNew();
			patternMatchingOverride.OO_ForeignCode = "ABC";
			patternMatchingOverride.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.EventCode;
			patternMatchingOverride.OO_LocalCode = "AAS";

			Factory.Save();

			var definitionFinder = new DefinitionFinder { Cache = EntitySetDefinitionCache.GetInstance() };
			var converter = new BusinessObjectToEntityConverter { DefinitionFinder = definitionFinder };
			var xmlSerializer = new NativeXmlSerializer { Converter = converter };

			string actualMessage;

			using (var dataStream = xmlSerializer.SerializeToStream(patternMatchingOverride))
			using (var reader = new StreamReader(dataStream))
			{
				actualMessage = reader.ReadToEnd();
			}

			var element = XElement.Load(new StringReader(actualMessage));

			AssertEquals("EDI Code Mapping Export Correctly", 1, element.Descendants().Count(u => u.Name.LocalName == "EDICodeMapping"));
			AssertEquals(1, element.Descendants().Count(u => u.Name.LocalName == "ForeignCode" && u.Value == "ABC"));
			AssertEquals(1, element.Descendants().Count(u => u.Name.LocalName == "LocalCode" && u.Value == "AAS"));
			AssertEquals(1, element.Descendants().Count(u => u.Name.LocalName == "Relationship" && u.Value == Constants.OrgPatternMatchOverrideRelationships.EventCode));
			AssertEquals(1, element.Descendants().Count(u => u.Name.LocalName == "Code" && u.Value == "TESTDUMMYORG"));
		}

		const string EDICodeMappingXml = @"<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>CARGOWSHA</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <EDICodeMapping version=""2.0"">
      <OrgPatternMatchOverride Action=""MERGE"">
        <PK>8baca636-3377-4231-89e1-24fdd52fe3c6</PK>
        <ForeignCode>ABC</ForeignCode>
        <LocalCode></LocalCode>
        <LocalGuid>{0}</LocalGuid>
        <Relationship>COU</Relationship>
        <Context></Context>
        <OrgHeader>
          <Code>TESTDUMMYORG</Code>
          <PK>122a6b74-6fe3-4dec-9f44-eb2af2008fe6</PK>
        </OrgHeader>
      </OrgPatternMatchOverride>
    </EDICodeMapping>
    <EDICodeMapping version=""2.0"">
      <OrgPatternMatchOverride Action=""MERGE"">
        <PK>1b02a11b-ecd4-48f9-9fce-b99c57e7a1ca</PK>
        <ForeignCode>EFG</ForeignCode>
        <LocalCode>AAS</LocalCode>
        <LocalGuid></LocalGuid>
        <Relationship>EVT</Relationship>
        <Context></Context>
        <OrgHeader>
          <Code>TESTDUMMYORG</Code>
          <PK>122a6b74-6fe3-4dec-9f44-eb2af2008fe6</PK>
        </OrgHeader>
      </OrgPatternMatchOverride>
    </EDICodeMapping>
  </Body>
</Native>";
	}
}
