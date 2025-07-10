using System.Text;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Registry;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Registry
{
	[TestedType(typeof(AddUnmatchedOrgConfigDataTransformation))]
	class AddUnmatchedOrgConfigDataTransformationTest : RegistryDataTransformationTestCase
	{
		const string sourceRegistryItemName = "UseUnmatchedOrganisationForMatching";
		const string targetRegistryItemName = "UnmatchedOrganisationConfiguration";
		const string testDataWithPreamble = @"﻿<?xml version=""1.0"" encoding=""utf-16""?>
<UnmatchedOrganisation>
<IsEnabled>Y</IsEnabled>
<Organisation>9dec3a31-84d2-4be7-8cfa-4a8ff99230a4</Organisation>
</UnmatchedOrganisation>
";

		protected override DataTransformation GetNewTestTransformationInstance() => new AddUnmatchedOrgConfigDataTransformation();

		protected override void PrepareTestData()
		{
			Helper.DeleteStmDataRow(sourceRegistryItemName);
			Helper.DeleteStmDataRow(targetRegistryItemName);
			Helper.InsertStmDataRow(sourceRegistryItemName, "BIN", Encoding.Unicode.GetBytes(testDataWithPreamble));
		}

		protected override void AssertTransformationResults()
		{
			var binaryValue = Helper.GetStmDataValue(targetRegistryItemName);
			var expectedString = @"<?xml version=""1.0"" encoding=""utf-16""?><ArrayOfCodeDescriptionBool xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><CodeDescriptionBool><CodeMaxLength>3</CodeMaxLength><Code>ORD</Code><Description>Order (Forwarding)</Description><Bool>Y</Bool><SystemDefined>False</SystemDefined></CodeDescriptionBool></ArrayOfCodeDescriptionBool>";

			AssertEquals(expectedString, Encoding.Unicode.GetString(binaryValue));
		}
	}
}
