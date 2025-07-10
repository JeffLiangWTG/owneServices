using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CodeDescriptionWithMandatoryDescriptionRegistryDataType))]
	sealed class CodeDescriptionWithMandatoryDescriptionRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<CodeDescriptionWithMandatoryDescriptionRegistryDataType>
	{
		protected override CodeDescriptionWithMandatoryDescriptionRegistryDataType GetNewDataType()
		{
			return new CodeDescriptionWithMandatoryDescriptionRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "CodeDescriptionWithMandatoryDescriptionRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var defaultValues = new CodeDescriptionWithMandatoryDescriptionCollection();
			var value1 = defaultValues.AddNew();
			value1.Code = "AAA";
			value1.EnglishDescription = "AAA Description";

			var value2 = defaultValues.AddNew();
			value2.Code = "ZZZ";
			value2.EnglishDescription = "ZZZ Description";

			var xml = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
				"<ArrayOfCodeDescriptionWithMandatoryDescription xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">" +
				"<CodeDescriptionWithMandatoryDescription><CodeMaxLength>3</CodeMaxLength><Code>AAA</Code><Description>AAA Description</Description></CodeDescriptionWithMandatoryDescription>" +
				"<CodeDescriptionWithMandatoryDescription><CodeMaxLength>3</CodeMaxLength><Code>ZZZ</Code><Description>ZZZ Description</Description></CodeDescriptionWithMandatoryDescription>" +
				"</ArrayOfCodeDescriptionWithMandatoryDescription>";

			var defaultValues2 = new CodeDescriptionWithMandatoryDescriptionCollection();
			var value3 = defaultValues2.AddNew();
			value3.Code = "TST";
			value3.EnglishDescription = "TEST Description";

			var xml2 = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
				"<ArrayOfCodeDescriptionWithMandatoryDescription xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">" +
				"<CodeDescriptionWithMandatoryDescription><CodeMaxLength>3</CodeMaxLength><Code>TST</Code><Description>TEST Description</Description></CodeDescriptionWithMandatoryDescription>" +
				"</ArrayOfCodeDescriptionWithMandatoryDescription>";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(defaultValues, xml),
				new ValidSampleAndBinaryValueInDB(defaultValues2, xml2)
			};
		}
	}
}
