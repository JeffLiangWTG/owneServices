using System;
using System.Text;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Registry.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.SalesAndMarketing.Testing
{
	[TestedType(typeof(UpdateGlowOpportunityStagesRegistryItemClass))]
	class UpdateGlowOpportunityStagesRegistryItemClassTest : RegistryDataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateGlowOpportunityStagesRegistryItemClass();
		}

		protected override void PrepareTestData()
		{
			Helper.InsertStmDataRow("GlowOpportunityStages", "BIN", Encoding.Unicode.GetBytes(originalXml_error));
			Helper.InsertStmDataRow("GlowOpportunityStages", company1, "BIN", Encoding.Unicode.GetBytes(originalXml1));
			Helper.InsertStmDataRow("GlowOpportunityStages", company2, "BIN", Encoding.Unicode.GetBytes(originalXml2));
			Helper.InsertStmDataRow("GlowOpportunityStages", company3, "BIN", null);
		}

		protected override void AssertTransformationResults()
		{
			var binaryValue = Helper.GetStmDataValue("GlowOpportunityStages");
			AssertNotNull("Should have a not NULL binary value.", binaryValue);
			AssertEquals("Registry item without ArrayOfCodeDescriptionBool should not change.", originalXml_error, Encoding.Unicode.GetString(binaryValue));

			var binaryValue1 = Helper.GetStmDataValue("GlowOpportunityStages", company1);
			AssertNotNull("Should have a not NULL binary value.", binaryValue1);
			AssertEquals("Registry item was not updated correctly.", expectedXml1, Encoding.Unicode.GetString(binaryValue1));

			var binaryValue2 = Helper.GetStmDataValue("GlowOpportunityStages", company2);
			AssertNotNull("Should have a not NULL binary value.", binaryValue2);
			AssertEquals("Registry item was not updated correctly.", expectedXml2, Encoding.Unicode.GetString(binaryValue2));

			var binaryValue3 = Helper.GetStmDataValue("GlowOpportunityStages", company3);
			AssertNull("Should have a NULL binary value.", binaryValue3);
		}

		Guid company1 = Guid.NewGuid();
		Guid company2 = Guid.NewGuid();
		Guid company3 = Guid.NewGuid();

		const string originalXml_error = "some invalid string";
		const string originalXml1 = "<?xml version=\"1.0\" encoding=\"utf-16\"?><ArrayOfCodeDescriptionBool xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><CodeDescriptionBool><CodeMaxLength>3</CodeMaxLength><Code>ABC</Code><Description>我的abC慢慢看</Description><Bool>Y</Bool><SystemDefined>False</SystemDefined></CodeDescriptionBool><CodeDescriptionBool><CodeMaxLength>3</CodeMaxLength><Code>NEW</Code><Description>New</Description><Bool>Y</Bool><SystemDefined>False</SystemDefined></CodeDescriptionBool><CodeDescriptionBool><CodeMaxLength>3</CodeMaxLength><Code>BCD</Code><Description>this is bcd</Description><Bool>N</Bool><SystemDefined>False</SystemDefined></CodeDescriptionBool></ArrayOfCodeDescriptionBool>";
		const string originalXml2 = "<?xml version=\"1.0\" encoding=\"utf-16\"?><ArrayOfCodeDescriptionBool xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><CodeDescriptionBool><CodeMaxLength>3</CodeMaxLength><Code>ABC</Code><Description>我的abC慢慢看</Description><Bool>Y</Bool><SystemDefined>False</SystemDefined></CodeDescriptionBool><CodeDescriptionBool><CodeMaxLength>3</CodeMaxLength><Code>NEW</Code><Description>New</Description><Bool>Y</Bool><SystemDefined>False</SystemDefined></CodeDescriptionBool><CodeDescriptionBool><CodeMaxLength>3</CodeMaxLength><Code>BCD</Code><Description>this is bcd</Description><Bool>N</Bool><SystemDefined>False</SystemDefined></CodeDescriptionBool></ArrayOfCodeDescriptionBool>";
		const string expectedXml1 = "<?xml version=\"1.0\" encoding=\"utf-16\"?><ArrayOfGlowOpportunityStage xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><GlowOpportunityStage><CodeMaxLength>3</CodeMaxLength><Code>ABC</Code><Description>我的abC慢慢看</Description><Bool>Y</Bool><SystemDefined>False</SystemDefined><WinProbability>0</WinProbability></GlowOpportunityStage><GlowOpportunityStage><CodeMaxLength>3</CodeMaxLength><Code>NEW</Code><Description>New</Description><Bool>Y</Bool><SystemDefined>False</SystemDefined><WinProbability>0</WinProbability></GlowOpportunityStage><GlowOpportunityStage><CodeMaxLength>3</CodeMaxLength><Code>BCD</Code><Description>this is bcd</Description><Bool>N</Bool><SystemDefined>False</SystemDefined><WinProbability>0</WinProbability></GlowOpportunityStage></ArrayOfGlowOpportunityStage>";
		const string expectedXml2 = "<?xml version=\"1.0\" encoding=\"utf-16\"?><ArrayOfGlowOpportunityStage xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><GlowOpportunityStage><CodeMaxLength>3</CodeMaxLength><Code>ABC</Code><Description>我的abC慢慢看</Description><Bool>Y</Bool><SystemDefined>False</SystemDefined><WinProbability>0</WinProbability></GlowOpportunityStage><GlowOpportunityStage><CodeMaxLength>3</CodeMaxLength><Code>NEW</Code><Description>New</Description><Bool>Y</Bool><SystemDefined>False</SystemDefined><WinProbability>0</WinProbability></GlowOpportunityStage><GlowOpportunityStage><CodeMaxLength>3</CodeMaxLength><Code>BCD</Code><Description>this is bcd</Description><Bool>N</Bool><SystemDefined>False</SystemDefined><WinProbability>0</WinProbability></GlowOpportunityStage></ArrayOfGlowOpportunityStage>";
	}
}
