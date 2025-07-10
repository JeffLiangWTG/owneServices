using System.Text;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CodeDescriptionBoolRelatedItemDataType))]
	sealed class CodeDescriptionBoolRelatedItemDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<CodeDescriptionBoolRelatedItemDataType>
	{
		public void TestDeserializedDataHasCorrectCodeMaxLength()
		{
			var registryDataType = new CodeDescriptionBoolRelatedItemDataType(5);
			var deserializedData = registryDataType.Deserialise(GetValidSamples()[0].BinaryValue);
			AssertEquals(5, deserializedData.CodeMaxLength);
		}

		#region Implementation

		protected override CodeDescriptionBoolRelatedItemDataType GetNewDataType()
		{
			return new CodeDescriptionBoolRelatedItemDataType(3, OpportunitySourceRelatedItemProvider.SecondaryList);
		}

		protected override string ExpectedEditorName
		{
			get { return null; }
		}

		// Using custom EditorInfo
		protected override bool HasEditor
		{
			get { return false; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			CodeDescriptionBoolRelatedItemCollection sample = new CodeDescriptionBoolRelatedItemCollection(OpportunitySourceRelatedItemProvider.SecondaryList);
			sample.Add("AAA", (NoResString)"DESC A", true, "CL2");
			sample.Add("BBB", (NoResString)"DESC B", false, "CL3");

			byte[] byteArrayValue = Encoding.Unicode.GetBytes("<?xml version=\"1.0\" encoding=\"utf-16\"?><ArrayOfCodeDescriptionBoolRelatedItem xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><CodeDescriptionBoolRelatedItem><CodeMaxLength>3</CodeMaxLength><Code>AAA</Code><Description>DESC A</Description><Bool>Y</Bool><RelatedItemCode>CL2</RelatedItemCode></CodeDescriptionBoolRelatedItem><CodeDescriptionBoolRelatedItem><CodeMaxLength>3</CodeMaxLength><Code>BBB</Code><Description>DESC B</Description><Bool>N</Bool><RelatedItemCode>CL3</RelatedItemCode></CodeDescriptionBoolRelatedItem></ArrayOfCodeDescriptionBoolRelatedItem>");

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(sample, byteArrayValue)
			};
		}

		#endregion
	}
}
