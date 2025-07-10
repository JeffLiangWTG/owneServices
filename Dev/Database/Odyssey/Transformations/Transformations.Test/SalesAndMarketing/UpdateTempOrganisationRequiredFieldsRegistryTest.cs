using System.Text;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Registry.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.SalesAndMarketing.Testing
{
	[TestedType(typeof(UpdateTempOrganisationRequiredFieldsRegistry))]
	class UpdateTempOrganisationRequiredFieldsRegistryTest : RegistryDataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateTempOrganisationRequiredFieldsRegistry();
		}

		protected override void PrepareTestData()
		{
			Helper.InsertStmDataRow("TempOrganisationRequiredFields", "BIN", Encoding.Unicode.GetBytes(originalXml1));
			Helper.InsertStmDataRow("SomeOtherRegistry", "BIN", Encoding.Unicode.GetBytes(originalXml_error));
		}

		protected override void AssertTransformationResults()
		{
			var binaryValue = Helper.GetStmDataValue("TempOrganisationRequiredFields");
			AssertNotNull("Should have a non NULL binary value.", binaryValue);
			AssertEquals(expectedXml1, Encoding.Unicode.GetString(binaryValue));

			binaryValue = Helper.GetStmDataValue("SomeOtherRegistry");
			AssertNotNull("Should have a non NULL binary value.", binaryValue);
			AssertEquals(originalXml_error, Encoding.Unicode.GetString(binaryValue));
		}

		const string originalXml_error = "some invalid string";
		const string originalXml1 = "<?xml version=\"1.0\" encoding=\"utf-16\"?><ArrayOfCodeDescriptionBool xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><CodeDescriptionBool><CodeMaxLength>10</CodeMaxLength><Code>Address1</Code><Description>Require Address 1</Description><Bool>Y</Bool><SystemDefined>False</SystemDefined></CodeDescriptionBool><CodeDescriptionBool><CodeMaxLength>10</CodeMaxLength><Code>Address2</Code><Description>Require Address 2</Description><Bool>N</Bool><SystemDefined>False</SystemDefined></CodeDescriptionBool><CodeDescriptionBool><CodeMaxLength>10</CodeMaxLength><Code>Country</Code><Description>Require Country/Region</Description><Bool>N</Bool><SystemDefined>False</SystemDefined></CodeDescriptionBool><CodeDescriptionBool><CodeMaxLength>10</CodeMaxLength><Code>UNLOCO</Code><Description>Require UNLOCO</Description><Bool>N</Bool><SystemDefined>False</SystemDefined></CodeDescriptionBool><CodeDescriptionBool><CodeMaxLength>10</CodeMaxLength><Code>City</Code><Description>Require City</Description><Bool>Y</Bool><SystemDefined>False</SystemDefined></CodeDescriptionBool><CodeDescriptionBool><CodeMaxLength>10</CodeMaxLength><Code>Postcode</Code><Description>Require Postcode</Description><Bool>N</Bool><SystemDefined>False</SystemDefined></CodeDescriptionBool><CodeDescriptionBool><CodeMaxLength>10</CodeMaxLength><Code>State</Code><Description>Require State</Description><Bool>N</Bool><SystemDefined>False</SystemDefined></CodeDescriptionBool><CodeDescriptionBool><CodeMaxLength>10</CodeMaxLength><Code>Branch</Code><Description>Require Branch</Description><Bool>N</Bool><SystemDefined>False</SystemDefined></CodeDescriptionBool><CodeDescriptionBool><CodeMaxLength>10</CodeMaxLength><Code>Phone</Code><Description>Require Phone</Description><Bool>N</Bool><SystemDefined>False</SystemDefined></CodeDescriptionBool><CodeDescriptionBool><CodeMaxLength>10</CodeMaxLength><Code>Mobile</Code><Description>Require Mobile</Description><Bool>N</Bool><SystemDefined>False</SystemDefined></CodeDescriptionBool><CodeDescriptionBool><CodeMaxLength>10</CodeMaxLength><Code>Email</Code><Description>Require Email</Description><Bool>N</Bool><SystemDefined>False</SystemDefined></CodeDescriptionBool><CodeDescriptionBool><CodeMaxLength>10</CodeMaxLength><Code>Fax</Code><Description>Require Fax</Description><Bool>N</Bool><SystemDefined>False</SystemDefined></CodeDescriptionBool><CodeDescriptionBool><CodeMaxLength>10</CodeMaxLength><Code>Web</Code><Description>Require Website URL</Description><Bool>N</Bool><SystemDefined>False</SystemDefined></CodeDescriptionBool></ArrayOfCodeDescriptionBool>";
		const string expectedXml1 = "<?xml version=\"1.0\" encoding=\"utf-16\"?><ArrayOfGlowTempOrgRequiredField xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><GlowTempOrgRequiredField><CodeMaxLength>10</CodeMaxLength><Code>Name</Code><Description>Name</Description><Bool>Y</Bool><SystemDefined>True</SystemDefined><IsMandatory>Y</IsMandatory></GlowTempOrgRequiredField><GlowTempOrgRequiredField><CodeMaxLength>10</CodeMaxLength><Code>Address1</Code><Description>Address 1</Description><Bool>Y</Bool><SystemDefined>True</SystemDefined><IsMandatory>N</IsMandatory></GlowTempOrgRequiredField><GlowTempOrgRequiredField><CodeMaxLength>10</CodeMaxLength><Code>Address2</Code><Description>Address 2</Description><Bool>N</Bool><SystemDefined>True</SystemDefined><IsMandatory>N</IsMandatory></GlowTempOrgRequiredField><GlowTempOrgRequiredField><CodeMaxLength>10</CodeMaxLength><Code>Country</Code><Description>Country/Region</Description><Bool>N</Bool><SystemDefined>True</SystemDefined><IsMandatory>N</IsMandatory></GlowTempOrgRequiredField><GlowTempOrgRequiredField><CodeMaxLength>10</CodeMaxLength><Code>UNLOCO</Code><Description>UNLOCO</Description><Bool>N</Bool><SystemDefined>True</SystemDefined><IsMandatory>N</IsMandatory></GlowTempOrgRequiredField><GlowTempOrgRequiredField><CodeMaxLength>10</CodeMaxLength><Code>City</Code><Description>City</Description><Bool>Y</Bool><SystemDefined>True</SystemDefined><IsMandatory>N</IsMandatory></GlowTempOrgRequiredField><GlowTempOrgRequiredField><CodeMaxLength>10</CodeMaxLength><Code>Postcode</Code><Description>Postcode</Description><Bool>N</Bool><SystemDefined>True</SystemDefined><IsMandatory>N</IsMandatory></GlowTempOrgRequiredField><GlowTempOrgRequiredField><CodeMaxLength>10</CodeMaxLength><Code>State</Code><Description>State</Description><Bool>N</Bool><SystemDefined>True</SystemDefined><IsMandatory>N</IsMandatory></GlowTempOrgRequiredField><GlowTempOrgRequiredField><CodeMaxLength>10</CodeMaxLength><Code>Branch</Code><Description>Branch</Description><Bool>N</Bool><SystemDefined>True</SystemDefined><IsMandatory>N</IsMandatory></GlowTempOrgRequiredField><GlowTempOrgRequiredField><CodeMaxLength>10</CodeMaxLength><Code>Phone</Code><Description>Phone</Description><Bool>N</Bool><SystemDefined>True</SystemDefined><IsMandatory>N</IsMandatory></GlowTempOrgRequiredField><GlowTempOrgRequiredField><CodeMaxLength>10</CodeMaxLength><Code>Mobile</Code><Description>Mobile</Description><Bool>N</Bool><SystemDefined>True</SystemDefined><IsMandatory>N</IsMandatory></GlowTempOrgRequiredField><GlowTempOrgRequiredField><CodeMaxLength>10</CodeMaxLength><Code>Email</Code><Description>Email</Description><Bool>N</Bool><SystemDefined>True</SystemDefined><IsMandatory>N</IsMandatory></GlowTempOrgRequiredField><GlowTempOrgRequiredField><CodeMaxLength>10</CodeMaxLength><Code>Fax</Code><Description>Fax</Description><Bool>N</Bool><SystemDefined>True</SystemDefined><IsMandatory>N</IsMandatory></GlowTempOrgRequiredField><GlowTempOrgRequiredField><CodeMaxLength>10</CodeMaxLength><Code>Web</Code><Description>Website URL</Description><Bool>N</Bool><SystemDefined>True</SystemDefined><IsMandatory>N</IsMandatory></GlowTempOrgRequiredField></ArrayOfGlowTempOrgRequiredField>";
	}
}
