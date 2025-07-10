using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CodeDescriptionBoolDisallowNewCodeReadOnlyValidationRegistryDataType))]
	sealed class CodeDescriptionBoolDisallowNewCodeReadOnlyValidationRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<CodeDescriptionBoolDisallowNewCodeReadOnlyValidationRegistryDataType>
	{
		protected override CodeDescriptionBoolDisallowNewCodeReadOnlyValidationRegistryDataType GetNewDataType() => new CodeDescriptionBoolDisallowNewCodeReadOnlyValidationRegistryDataType();

		protected override string ExpectedEditorName => null;

		// Using custom EditorInfo.
		protected override bool HasEditor => false;

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection = new CodeDescriptionBoolDisallowNewCodeReadOnlyValidationCollection();
			var element = collection.AddNew();
			element.Code = "BUY";
			element.Description = (NoResString)"Buy Rate";
			element.Bool = true;
			element.SystemDefined = true;
			element.IsCodeReadOnly = true;

			const string url1 = "\"http://www.w3.org/2001/XMLSchema\"";
			const string url2 = "\"http://www.w3.org/2001/XMLSchema-instance\"";
			var xmlString = $"<ArrayOfCodeDescriptionBoolDisallowNewCodeReadOnlyValidation xmlns:xsd={url1} xmlns:xsi={url2}>" +
							"<CodeDescriptionBoolDisallowNewCodeReadOnlyValidation>" + "<CodeMaxLength>3</CodeMaxLength>" +
							"<Code>BUY</Code>" + "<Description>Buy Rate</Description>" + "<Bool>Y</Bool>" +
							"<SystemDefined>True</SystemDefined>" + "<IsCodeReadOnly>True</IsCodeReadOnly>" +
							"</CodeDescriptionBoolDisallowNewCodeReadOnlyValidation>" +
							"</ArrayOfCodeDescriptionBoolDisallowNewCodeReadOnlyValidation> ";
			var byteArray = System.Text.Encoding.ASCII.GetBytes(xmlString);

			return new ValidSampleAndBinaryValueInDB[]
			{
					new ValidSampleAndBinaryValueInDB(collection, byteArray)
			};
		}
	}
}
