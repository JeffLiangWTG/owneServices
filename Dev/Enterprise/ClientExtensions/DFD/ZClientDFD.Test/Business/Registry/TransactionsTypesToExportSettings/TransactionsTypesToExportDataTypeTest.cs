using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.DFD.Registry.Testing
{
	[TestedType(typeof(TransactionsTypesToExportDataType))]
	internal class TransactionsTypesToExportDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<TransactionsTypesToExportDataType>
	{
		protected override TransactionsTypesToExportDataType GetNewDataType()
		{
			return new TransactionsTypesToExportDataType();
		}

		protected override string ExpectedEditorName
		{
			get
			{
				return "TransactionsTypesToExportRegistryItemEditor";
			}
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			TransactionsTypesToExportBusinessObject bizObj = new TransactionsTypesToExportBusinessObject();
			bizObj.ARAdjustmentNote = true;
			bizObj.ARCreditNote = true;
			bizObj.ARInvoice = true;
			bizObj.ARNonJobRelated = true;
			bizObj.ARJobRelated = false;
			byte[] byteArrayValue = new byte[] { 255, 254, 60, 0, 63, 0, 120, 0, 109, 0, 108, 0, 32, 0, 118, 0, 101, 0, 114, 0, 115, 0, 105, 0, 111, 0, 110, 0, 61, 0, 34, 0, 49, 0, 46, 0, 48, 0, 34, 0, 32, 0, 101, 0, 110, 0, 99, 0, 111, 0, 100, 0, 105, 0, 110, 0, 103, 0, 61, 0, 34, 0, 117, 0, 116, 0, 102, 0, 45, 0, 49, 0, 54, 0, 34, 0, 63, 0, 62, 0, 60, 0, 84, 0, 114, 0, 97, 0, 110, 0, 115, 0, 97, 0, 99, 0, 116, 0, 105, 0, 111, 0, 110, 0, 115, 0, 84, 0, 121, 0, 112, 0, 101, 0, 115, 0, 84, 0, 111, 0, 69, 0, 120, 0, 112, 0, 111, 0, 114, 0, 116, 0, 66, 0, 117, 0, 115, 0, 105, 0, 110, 0, 101, 0, 115, 0, 115, 0, 79, 0, 98, 0, 106, 0, 101, 0, 99, 0, 116, 0, 62, 0, 60, 0, 65, 0, 82, 0, 73, 0, 110, 0, 118, 0, 111, 0, 105, 0, 99, 0, 101, 0, 62, 0, 89, 0, 60, 0, 47, 0, 65, 0, 82, 0, 73, 0, 110, 0, 118, 0, 111, 0, 105, 0, 99, 0, 101, 0, 62, 0, 60, 0, 65, 0, 82, 0, 67, 0, 114, 0, 101, 0, 100, 0, 105, 0, 116, 0, 78, 0, 111, 0, 116, 0, 101, 0, 62, 0, 89, 0, 60, 0, 47, 0, 65, 0, 82, 0, 67, 0, 114, 0, 101, 0, 100, 0, 105, 0, 116, 0, 78, 0, 111, 0, 116, 0, 101, 0, 62, 0, 60, 0, 65, 0, 82, 0, 65, 0, 100, 0, 106, 0, 117, 0, 115, 0, 116, 0, 109, 0, 101, 0, 110, 0, 116, 0, 78, 0, 111, 0, 116, 0, 101, 0, 62, 0, 89, 0, 60, 0, 47, 0, 65, 0, 82, 0, 65, 0, 100, 0, 106, 0, 117, 0, 115, 0, 116, 0, 109, 0, 101, 0, 110, 0, 116, 0, 78, 0, 111, 0, 116, 0, 101, 0, 62, 0, 60, 0, 65, 0, 82, 0, 74, 0, 111, 0, 98, 0, 82, 0, 101, 0, 108, 0, 97, 0, 116, 0, 101, 0, 100, 0, 62, 0, 78, 0, 60, 0, 47, 0, 65, 0, 82, 0, 74, 0, 111, 0, 98, 0, 82, 0, 101, 0, 108, 0, 97, 0, 116, 0, 101, 0, 100, 0, 62, 0, 60, 0, 65, 0, 82, 0, 78, 0, 111, 0, 110, 0, 74, 0, 111, 0, 98, 0, 82, 0, 101, 0, 108, 0, 97, 0, 116, 0, 101, 0, 100, 0, 62, 0, 89, 0, 60, 0, 47, 0, 65, 0, 82, 0, 78, 0, 111, 0, 110, 0, 74, 0, 111, 0, 98, 0, 82, 0, 101, 0, 108, 0, 97, 0, 116, 0, 101, 0, 100, 0, 62, 0, 60, 0, 47, 0, 84, 0, 114, 0, 97, 0, 110, 0, 115, 0, 97, 0, 99, 0, 116, 0, 105, 0, 111, 0, 110, 0, 115, 0, 84, 0, 121, 0, 112, 0, 101, 0, 115, 0, 84, 0, 111, 0, 69, 0, 120, 0, 112, 0, 111, 0, 114, 0, 116, 0, 66, 0, 117, 0, 115, 0, 105, 0, 110, 0, 101, 0, 115, 0, 115, 0, 79, 0, 98, 0, 106, 0, 101, 0, 99, 0, 116, 0, 62, 0 };
			return new ValidSampleAndBinaryValueInDB[] { new ValidSampleAndBinaryValueInDB(bizObj, byteArrayValue) };
		}
	}
}
