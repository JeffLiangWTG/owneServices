using System.IO;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.ConcreteUnitTesting.Testing
{
	class TagDefinitionImportTest : TestCaseWithFactory
	{
		public void TestImportTagAndMagnitudes()
		{
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(XML_DefinitionWithMagnitude)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);
				var insertLog = manager.GetLogs();
				AssertMultilineASCIIEquals("Insert Log Text", @"
--- Start Import Process --------------------------------------------------------------
Processed: Tag
--- Import Process Finished -----------------------------------------------------------
TagDefinition - 1 inserts, 0 updates, 0 deletes
TagMagnitude - 1 inserts, 0 updates, 0 deletes
				".Trim(), insertLog);
			}
		}

		const string XML_DefinitionWithMagnitude = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns = ""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>EDICUS</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Tag version = ""2.0"" >
	  <TagDefinition Action=""MERGE"">
        <PK>63a4158a-523c-43e7-ac93-a1544414b5a3</PK>
        <Code>1</Code>
        <Description>11</Description>
        <IsSystem>false</IsSystem>
        <IsExclusive>false</IsExclusive>
        <VisualizationData></VisualizationData>
        <UsageScope>RUL</UsageScope>
        <Scope>WFL</Scope>
        <IsActive>true</IsActive>
        <TagMagnitudeCollection>
          <TagMagnitude Action = ""MERGE"" >
			<PK> 7c7cf59e-10ea-4362-8b6c-8a00472a1d54</PK>
            <Code>ABC</Code>
            <Description>Jezza</Description>
            <NudgeAmount>1</NudgeAmount>
            <RuleRunSequence>0</RuleRunSequence>
            <SystemCreateTimeUtc>2016-05-02T06:39:00</SystemCreateTimeUtc>
            <IsActive>true</IsActive>
            <OwnerGroup TableName = ""GlbGroup"" />
		  </TagMagnitude>
		</TagMagnitudeCollection>
	  </TagDefinition>
	</Tag>
  </Body>
</Native>";

		public void TestImportTagAndMagnitudes_WithXMLColumn()
		{
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(XML_DefinitionWithMagnitude_WithVisualizationData)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);
				var insertLog = manager.GetLogs();
				AssertMultilineASCIIEquals("Insert Log Text", @"
--- Start Import Process --------------------------------------------------------------
Processed: Tag
--- Import Process Finished -----------------------------------------------------------
TagDefinition - 1 inserts, 0 updates, 0 deletes
TagMagnitude - 1 inserts, 0 updates, 0 deletes
						".Trim(), insertLog);
				var query = new ZQuery();
				query.AddToFilter(TagMagnitudeSchema.TGM_Code, "ABC");
				var loadedMagnitude = new BusinessObjectFactory().Load<ITagMagnitude>(query).Single();

				AssertEquals(true, ((BusinessObject)loadedMagnitude)["ApplyColorToBackground"]);
				AssertEquals("<Jezza", loadedMagnitude.TGM_Description);
			}
		}

		const string XML_DefinitionWithMagnitude_WithVisualizationData = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns = ""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>EDICUS</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Tag version = ""2.0"">
	  <TagDefinition Action=""MERGE"">
        <PK>63a4158a-523c-43e7-ac93-a1544414b5a3</PK>
        <Code>1</Code>
        <Description>11</Description>
        <IsSystem>false</IsSystem>
        <IsExclusive>false</IsExclusive>
        <VisualizationData></VisualizationData>
        <UsageScope>RUL</UsageScope>
        <Scope>WFL</Scope>
        <IsActive>true</IsActive>
        <TagMagnitudeCollection>
          <TagMagnitude Action = ""MERGE"">
			<PK> 7c7cf59e-10ea-4362-8b6c-8a00472a1d54</PK>
            <Code>ABC</Code>
            <Description>&lt;Jezza</Description>
            <NudgeAmount>1</NudgeAmount>
            <VisualizationData>&lt;TGM_VisualizationData&gt;&lt;ApplyColorToBackground&gt;Y&lt;/ApplyColorToBackground&gt;&lt;ApplyColorToBorder&gt;Y&lt;/ApplyColorToBorder&gt;&lt;BorderStyle&gt;Inset - large&lt;/BorderStyle&gt;&lt;Color&gt;Alice Blue&lt;/Color&gt;&lt;VisualStylePriority&gt;1&lt;/VisualStylePriority&gt;&lt;/TGM_VisualizationData&gt;</VisualizationData>
            <RuleRunSequence>0</RuleRunSequence>
            <SystemCreateTimeUtc>2016-05-02T06:39:00</SystemCreateTimeUtc>
            <IsActive>true</IsActive>
            <OwnerGroup TableName = ""GlbGroup""/>
		  </TagMagnitude>
		</TagMagnitudeCollection>
	  </TagDefinition>
	</Tag>
  </Body>
</Native>";

		public void TestImportTagAndMagnitudes_CorrectMerge()
		{
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(XML_DefinitionWithMagnitude)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);
				var insertLog = manager.GetLogs();
				AssertMultilineASCIIEquals("Insert Log Text", @"
--- Start Import Process --------------------------------------------------------------
Processed: Tag
--- Import Process Finished -----------------------------------------------------------
TagDefinition - 1 inserts, 0 updates, 0 deletes
TagMagnitude - 1 inserts, 0 updates, 0 deletes
				".Trim(), insertLog);
			}

			var query = new ZQuery();
			query.AddToFilter(TagDefinitionSchema.TGD_Code, "1");
			var loadedTagDef = new BusinessObjectFactory().Load<ITagDefinition>(query).Single();

			AssertEquals("1", loadedTagDef.TGD_Code);
			AssertEquals("11", loadedTagDef.TGD_Description);

			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(XML_DefinitionWithMagnitude_SameCodeDifferentDescription)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);
				var insertLog = manager.GetLogs();
				AssertMultilineASCIIEquals("Insert Log Text", @"
--- Start Import Process --------------------------------------------------------------
Processed: Tag
--- Import Process Finished -----------------------------------------------------------
TagDefinition - 0 inserts, 1 updates, 0 deletes
TagMagnitude - 0 inserts, 1 updates, 0 deletes
				".Trim(), insertLog);
			}

			query = new ZQuery();
			query.AddToFilter(TagDefinitionSchema.TGD_Code, "1");
			var defs = new BusinessObjectFactory().Load<ITagDefinition>(query);

			AssertEquals(1, defs.Length);

			loadedTagDef = defs.Single();

			AssertEquals("1", loadedTagDef.TGD_Code);
			AssertEquals("22", loadedTagDef.TGD_Description);

			query = new ZQuery();
			query.AddToFilter(TagMagnitudeSchema.TGM_Code, "ABC");
			var magnitudes = new BusinessObjectFactory().Load<ITagMagnitude>(query);

			AssertEquals(1, magnitudes.Length);
			var loadedMagnitude = magnitudes.Single();
			AssertEquals("ABC", loadedMagnitude.TGM_Code);
			AssertEquals("Nezza", loadedMagnitude.TGM_Description);
		}

		const string XML_DefinitionWithMagnitude_SameCodeDifferentDescription = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns = ""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>EDICUS</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Tag version = ""2.0"" >
	  <TagDefinition Action=""MERGE"">
        <PK>63a4158a-523c-43e7-ac93-a1544414b5a3</PK>
        <Code>1</Code>
        <Description>22</Description>
        <IsSystem>false</IsSystem>
        <IsExclusive>false</IsExclusive>
        <VisualizationData></VisualizationData>
        <UsageScope>RUL</UsageScope>
        <Scope>WFL</Scope>
        <IsActive>true</IsActive>
        <TagMagnitudeCollection>
          <TagMagnitude Action = ""MERGE"" >
			<PK> 7c7cf59e-10ea-4362-8b6c-8a00472a1d54</PK>
            <Code>ABC</Code>
            <Description>Nezza</Description>
            <NudgeAmount>1</NudgeAmount>
            <RuleRunSequence>0</RuleRunSequence>
            <SystemCreateTimeUtc>2016-05-02T06:39:00</SystemCreateTimeUtc>
            <IsActive>true</IsActive>
            <OwnerGroup TableName = ""GlbGroup"" />
		  </TagMagnitude>
		</TagMagnitudeCollection>
	  </TagDefinition>
	</Tag>
  </Body>
</Native>";
	}
}
