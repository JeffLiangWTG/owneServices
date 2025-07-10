using System.Text;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(GlowTempOrgRegistryDataType))]
	sealed class GlowTempOrgRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<GlowTempOrgRegistryDataType>
	{
		protected override GlowTempOrgRegistryDataType GetNewDataType()
		{
			var defaultValues = new GlowTempOrgRequiredFieldCollection();
			defaultValues.Add("AAA", (NoResString)"AAA Description", enabled: true, isMandatory: true);
			defaultValues.Add("ZZZ", (NoResString)"ZZZ Description", enabled: true, isMandatory: false);

			return new GlowTempOrgRegistryDataType(defaultValues);
		}

		protected override string ExpectedEditorName
		{
			get { return "GlowTempOrgRequiredFieldsRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var defaultFields = new GlowTempOrgRequiredFieldCollection();
			var field1 = defaultFields.AddNew();
			field1.CodeMaxLength = 10;
			field1.Code = "AAAAAAAAAA";
			field1.EnglishDescription = "AAA Description";
			field1.Bool = true;
			field1.IsMandatory = true;

			var field2 = defaultFields.AddNew();
			field2.CodeMaxLength = 10;
			field2.Code = "ZZZZZZZZZZ";
			field2.EnglishDescription = "ZZZ Description";
			field2.Bool = true;
			field2.IsMandatory = false;

			var xml = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
				"<ArrayOfGlowTempOrgRequiredField xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">" +
				"<GlowTempOrgRequiredField><CodeMaxLength>10</CodeMaxLength><Code>AAAAAAAAAA</Code><Description>AAA Description</Description><Bool>Y</Bool><SystemDefined>False</SystemDefined><IsMandatory>Y</IsMandatory></GlowTempOrgRequiredField>" +
				"<GlowTempOrgRequiredField><CodeMaxLength>10</CodeMaxLength><Code>ZZZZZZZZZZ</Code><Description>ZZZ Description</Description><Bool>Y</Bool><SystemDefined>False</SystemDefined><IsMandatory>N</IsMandatory></GlowTempOrgRequiredField>" +
				"</ArrayOfGlowTempOrgRequiredField>";

			var defaultFields2 = new GlowTempOrgRequiredFieldCollection();
			defaultFields2.AddSystemDefined("TST", (NoResString)"TST Description", enabled: true, isMandatory: true);

			var xml2 = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
				"<ArrayOfGlowTempOrgRequiredField xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">" +
				"<GlowTempOrgRequiredField><CodeMaxLength>3</CodeMaxLength><Code>TST</Code><Description>TST Description</Description><Bool>Y</Bool><SystemDefined>True</SystemDefined><IsMandatory>Y</IsMandatory></GlowTempOrgRequiredField>" +
				"</ArrayOfGlowTempOrgRequiredField>";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(defaultFields, Encoding.Unicode.GetBytes(xml)),
				new ValidSampleAndBinaryValueInDB(defaultFields2, Encoding.Unicode.GetBytes(xml2))
			};
		}
	}
}
