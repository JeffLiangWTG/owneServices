using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(ShareSequentialReferenceNumbersRegistryDataType))]
	class ShareSequentialReferenceNumbersRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<ShareSequentialReferenceNumbersRegistryDataType>
	{
		#region Implementation

		protected override ShareSequentialReferenceNumbersRegistryDataType GetNewDataType()
		{
			return new ShareSequentialReferenceNumbersRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "ShareSequentialNumbersRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			ShareSequentialReferenceNumbers collection = new ShareSequentialReferenceNumbers();

			byte[] byteArrayValue = new byte[]
		 {
			255,254,60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,
0,102,0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,83,0,104,0,97,0,114,0,101,0,83,0,101,0,113,0,117,0,101,0,110,0,116,0,105,0,97,0,108,0,82,0,101,0,102,0,101,0,114,0,101,0,110,0,99,0,101,0,78,
0,117,0,109,0,98,0,101,0,114,0,115,0,62,0,60,0,86,0,97,0,108,0,117,0,101,0,62,0,78,0,60,0,47,0,86,0,97,0,108,0,117,0,101,0,62,0,60,0,47,0,83,0,104,0,97,0,114,0,101,0,83,0,101,0,113,
0,117,0,101,0,110,0,116,0,105,0,97,0,108,0,82,0,101,0,102,0,101,0,114,0,101,0,110,0,99,0,101,0,78,0,117,0,109,0,98,0,101,0,114,0,115,0,62,0
		 };

			return new ValidSampleAndBinaryValueInDB[]
		 {
			new ValidSampleAndBinaryValueInDB(collection, byteArrayValue)
		 };
		}

		#endregion
	}
}
