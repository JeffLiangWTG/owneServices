using System.Text;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CodeSelectionCollectionRegistryDataType))]
	sealed class CodeSelectionCollectionRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<CodeSelectionCollectionRegistryDataType>
	{
		protected override string ExpectedEditorName
		{
			get { return "CodeSelectionCollectionRegistryItemEditor"; }
		}

		protected override CodeSelectionCollectionRegistryDataType GetNewDataType()
		{
			return new CodeSelectionCollectionRegistryDataType(CodeSelectionTest.GetCodesProviderForTesting());
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			CodeSelectionCollection collection = new CodeSelectionCollection(CodeSelectionTest.GetCodesProviderForTesting());
			collection.AddNew().Code = "RIF";
			collection.AddNew().Code = "GRE";

			string xml =
				@"<?xml version=""1.0"" encoding=""utf-16""?>
					<ArrayOfCodeSelection>
						<CodeSelection>
							<Code>RIF</Code>
						</CodeSelection>
						<CodeSelection>
							<Code>GRE</Code>
						</CodeSelection>
					</ArrayOfCodeSelection>";

			return new ValidSampleAndBinaryValueInDB[]
			{
					new ValidSampleAndBinaryValueInDB(collection, Encoding.Unicode.GetBytes(xml))
			};
		}

		public void TestDeserialise()
		{
			CodeSelectionCollection collection = DataType.Deserialise(GetValidSamples()[0].BinaryValue);
			AssertEquals("Deserialise().Codes.GetList()", DataType.codesProvider.CodeDescriptionPairList, collection.Codes);
		}
	}
}
