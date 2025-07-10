using System;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.ConcreteUnitTesting
{
	class TagRuleImportTest : TestCaseWithFactory
	{
		public void TestImportTagRule()
		{
			var definition = Factory.New<TagDefinition>();
			definition.TGD_Code = "FAP";
			var magnitude = Factory.New<TagMagnitude>();
			magnitude.TGM_Code = "FOP";
			magnitude.TGM_TGD_Tag = definition.PK;

			Factory.Save();

			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(XML_TagRule)))
			{
				var insertLog = NativeDataTransferTestHelper.ImportAndGetInsertLog(stream);
				AssertMultilineASCIIEquals("Insert Log Text", @"
--- Start Import Process --------------------------------------------------------------
Processed: TagRule
--- Import Process Finished -----------------------------------------------------------
TagRule - 1 inserts, 0 updates, 0 deletes
StmModuleFilter - 1 inserts, 0 updates, 0 deletes
StmModuleFilterUserData - 1 inserts, 0 updates, 0 deletes
TagLink - 1 inserts, 0 updates, 0 deletes
						".Trim(), insertLog);
				var query = new ZQuery();
				query.AddToFilter(TagRuleSchema.TGR_Name, "For Ben");

				var factoryToReLoad = new BusinessObjectFactory();

				var loadedRule = factoryToReLoad.Load<TagRule>(query).Single();
				var loadedTemplate = loadedRule.TagRuleTemplate;
				AssertEquals("FOP", loadedTemplate.TagMagnitude.TGM_Code);
				AssertEquals("FAP", loadedTemplate.TagMagnitude.TagDefinition.TGD_Code);
				AssertEquals(loadedRule.PK, loadedTemplate.TGL_ParentId);

				var loadedModuleFilter = loadedRule.Filter;

				AssertNotNull(loadedModuleFilter);
			}
		}

		static string XML_TagRule => FormattableString.Invariant($@"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns = ""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>WTGCUSBNE</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <TagRule version=""2.0"">
	  <TagRule Action=""MERGE"">
        <PK>e9d8830c-277b-47bc-ae3b-3c7018ff4dde</PK>
        <Name>For Ben</Name>
        <IsSystem>false</IsSystem>
        <IsActive>true</IsActive>
        <ActionType>ADD</ActionType>
		<Branch>{Env.CurrentBranchPK}</Branch>
		<Department>{Env.CurrentDepartmentPK}</Department>
        <LastRunDurationInSeconds>0</LastRunDurationInSeconds>
        <LastRunStartTimeUtc></LastRunStartTimeUtc>
        <TagLinkCollection>
          <TagLink Action=""MERGE"" >
			<PK>5b47478f-22cb-4d6f-bd44-e507c7638946</PK>
            <Description>1111</Description>
            <Magnitude>1.000</Magnitude>
            <Sequence>0</Sequence>
            <RemovedTimeUtc></RemovedTimeUtc>
            <ParentTableCode>TGR</ParentTableCode>
            <SystemCreateTimeUtc>2016-05-06T06:27:00</SystemCreateTimeUtc>
            <TagMagnitude>
              <Code>FOP</Code>
              <PK>4c6f0f48-3cfe-43b5-be6a-afd041cd31df</PK>
              <Tag TableName =""TagDefinition"">
				<Code>FAP</Code>
				<PK>6af8983a-179e-4b1b-b7c7-8c4dbf89974c</PK>
              </Tag>
            </TagMagnitude>
            <RemovedBy TableName=""GlbStaff""/>
		  </TagLink>
		</TagLinkCollection>
		<StmModuleFilter Action=""MERGE"">
			<PK>8afefe27-41a0-4ff1-ac20-5dda557563bb</PK>
			<ModuleID>BMFilterRule</ModuleID>
			<RelatedEntityID></RelatedEntityID>
			<IsPublished>false</IsPublished>
			<FilterName></FilterName>
			<FilterType>FRU</FilterType>
			<ParentTableCode>TGR</ParentTableCode>
			<FilterData>PD94bWwgdmVyc2lvbj0iMS4wIj8+DQo8RmlsdGVyTGF5b3V0U2VyaWFsaXplcj4NCiAgPEZpbHRlclN0cmlwcz4NCiAgICA8RmlsdGVyU3RyaXA+DQogICAgICA8RmlsdGVyRGVzY3JpcHRpb24+UGFyZW50IEpvYjwvRmlsdGVyRGVzY3JpcHRpb24+DQogICAgICA8T3JDYXRlZ29yeT5Ob25lPC9PckNhdGVnb3J5Pg0KICAgICAgPEdyb3VwT3JDYXRlZ29yeT5Ob25lPC9Hcm91cE9yQ2F0ZWdvcnk+DQogICAgICA8R3JvdXBOYW1lIC8+DQogICAgICA8QWRkaXRpb25hbENvbG91ck5hbWUgLz4NCiAgICAgIDxBZGRpdGlvbmFsR3JvdXBDb2xvdXJOYW1lIC8+DQogICAgPC9GaWx0ZXJTdHJpcD4NCiAgPC9GaWx0ZXJTdHJpcHM+DQo8L0ZpbHRlckxheW91dFNlcmlhbGl6ZXI+</FilterData>
			<ColumnLayoutData></ColumnLayoutData>
			<SaveColumnLayout>false</SaveColumnLayout>
			<IsSystem>false</IsSystem>
			<StmModuleFilterUserData Action=""MERGE"">
				<PK>d611bf96-ba72-4e49-b566-665439d03d24</PK>
				<RelatedEntityTableCode></RelatedEntityTableCode>
				<RelatedEntityID>00000000-0000-0000-0000-000000000000</RelatedEntityID>
				<FilterDataValues>PD94bWwgdmVyc2lvbj0iMS4wIj8+DQo8RmlsdGVyTGF5b3V0VmFsdWVzU2VyaWFsaXplcj4NCiAgPE1vZHVsZUZpbHRlcnM+DQogICAgPE1vZHVsZUZpbHRlcj4NCiAgICAgIDxTZWxlY3RlZE1vZHVsZT5DdXNEZWM8L1NlbGVjdGVkTW9kdWxlPg0KICAgICAgPENvbXBhcmVyPmZpbHRlcnMgbWF0Y2g8L0NvbXBhcmVyPg0KICAgICAgPFByb3BlcnR5PjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMDwvUHJvcGVydHk+DQogICAgICA8UHJvcGVydHlTZWxlY3RlZEZpbHRlcnNEYXRhPiZsdDs/eG1sIHZlcnNpb249IjEuMCI/Jmd0Ow0KJmx0O0ZpbHRlckxheW91dFNlcmlhbGl6ZXImZ3Q7DQogICZsdDtGaWx0ZXJTdHJpcHMmZ3Q7DQogICAgJmx0O0ZpbHRlclN0cmlwJmd0Ow0KICAgICAgJmx0O0ZpbHRlckRlc2NyaXB0aW9uJmd0O0VUQSZsdDsvRmlsdGVyRGVzY3JpcHRpb24mZ3Q7DQogICAgICAmbHQ7T3JDYXRlZ29yeSZndDtOb25lJmx0Oy9PckNhdGVnb3J5Jmd0Ow0KICAgICAgJmx0O0dyb3VwT3JDYXRlZ29yeSZndDtOb25lJmx0Oy9Hcm91cE9yQ2F0ZWdvcnkmZ3Q7DQogICAgICAmbHQ7R3JvdXBOYW1lIC8mZ3Q7DQogICAgICAmbHQ7QWRkaXRpb25hbENvbG91ck5hbWUgLyZndDsNCiAgICAgICZsdDtBZGRpdGlvbmFsR3JvdXBDb2xvdXJOYW1lIC8mZ3Q7DQogICAgJmx0Oy9GaWx0ZXJTdHJpcCZndDsNCiAgJmx0Oy9GaWx0ZXJTdHJpcHMmZ3Q7DQombHQ7L0ZpbHRlckxheW91dFNlcmlhbGl6ZXImZ3Q7PC9Qcm9wZXJ0eVNlbGVjdGVkRmlsdGVyc0RhdGE+DQogICAgICA8UHJvcGVydHlTZWxlY3RlZEZpbHRlcnNWYWx1ZXM+Jmx0Oz94bWwgdmVyc2lvbj0iMS4wIj8mZ3Q7DQombHQ7RmlsdGVyTGF5b3V0VmFsdWVzU2VyaWFsaXplciZndDsNCiAgJmx0O01vZHVsZUZpbHRlcnMmZ3Q7DQogICAgJmx0O01vZHVsZUZpbHRlciZndDsNCiAgICAgICZsdDtTZWFyY2hQcm9wZXJ0eSZndDtPZmZzZXQgcmFuZ2UmbHQ7L1NlYXJjaFByb3BlcnR5Jmd0Ow0KICAgICAgJmx0O1Byb3BlcnR5MSZndDsyMDE2LTAxLTAyIDAwOjAwOjAwLjAwMCZsdDsvUHJvcGVydHkxJmd0Ow0KICAgICAgJmx0O1Byb3BlcnR5MiAvJmd0Ow0KICAgICAgJmx0O0ZpbHRlck9wdGlvbiZndDtQYXN0Jmx0Oy9GaWx0ZXJPcHRpb24mZ3Q7DQogICAgJmx0Oy9Nb2R1bGVGaWx0ZXImZ3Q7DQogICZsdDsvTW9kdWxlRmlsdGVycyZndDsNCiZsdDsvRmlsdGVyTGF5b3V0VmFsdWVzU2VyaWFsaXplciZndDs8L1Byb3BlcnR5U2VsZWN0ZWRGaWx0ZXJzVmFsdWVzPg0KICAgIDwvTW9kdWxlRmlsdGVyPg0KICA8L01vZHVsZUZpbHRlcnM+DQo8L0ZpbHRlckxheW91dFZhbHVlc1NlcmlhbGl6ZXI+</FilterDataValues>
			</StmModuleFilterUserData>
			<GlbCompany>
				<Code>WTG</Code>
				<PK>878d7aca-ffc3-49fc-9710-969ca0c0f2ac</PK>
			</GlbCompany>
        </StmModuleFilter>
      </TagRule>
    </TagRule>
  </Body>
</Native>");
	}
}
