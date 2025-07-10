using System.Text;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CustomsReferenceNumberTypesDataType))]
	sealed class CustomsReferenceNumbersDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<CustomsReferenceNumberTypesDataType>
	{
		#region Implementation

		protected override CustomsReferenceNumberTypesDataType GetNewDataType()
		{
			return new CustomsReferenceNumberTypesDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "CustomsReferenceNumberTypesRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			CustomsReferenceNumberTypeCollection sample = new CustomsReferenceNumberTypeCollection();
			sample.Add("AA1", (NoResString)"DESC1");
			sample.Add("AA2", (NoResString)"DESC2");

			byte[] byteArrayValue = Encoding.Unicode.GetBytes("<?xml version=\"1.0\" encoding=\"utf-16\"?><CustomsReferenceNumberTypes xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><CustomsReferenceNumberType><Code>AA1</Code><Description>DESC1</Description><IsUnique>Y</IsUnique></CustomsReferenceNumberType><CustomsReferenceNumberType><Code>AA2</Code><Description>DESC2</Description><IsUnique>Y</IsUnique></CustomsReferenceNumberType></CustomsReferenceNumberTypes>");

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(sample, byteArrayValue)
			};
		}

		#endregion
	}
}
