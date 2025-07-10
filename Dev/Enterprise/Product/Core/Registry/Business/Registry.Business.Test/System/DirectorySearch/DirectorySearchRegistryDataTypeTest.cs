using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(DirectorySearchRegistryDataType))]
	sealed class DirectorySearchRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<DirectorySearchRegistryDataType>
	{
		protected override DirectorySearchRegistryDataType GetNewDataType()
		{
			return new DirectorySearchRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			DirectorySearch search = new DirectorySearch();
			search.DirectoryPath = @"X:\Directory";
			search.SearchSubdirectories = true;

			byte[] byteArrayValue = new byte[]
			{
				255,254,60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,
				0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,0,102,0,45,0,49,0,54,0,34,0,63,0,62,0,
				60,0,68,0,105,0,114,0,101,0,99,0,116,0,111,0,114,0,121,0,83,0,101,0,97,0,114,0,99,0,104,0,62,0,60,0,68,0,105,0,
				114,0,101,0,99,0,116,0,111,0,114,0,121,0,80,0,97,0,116,0,104,0,62,0,88,0,58,0,92,0,68,0,105,0,114,0,101,0,99,0,
				116,0,111,0,114,0,121,0,60,0,47,0,68,0,105,0,114,0,101,0,99,0,116,0,111,0,114,0,121,0,80,0,97,0,116,0,104,0,62,0,
				60,0,83,0,101,0,97,0,114,0,99,0,104,0,83,0,117,0,98,0,100,0,105,0,114,0,101,0,99,0,116,0,111,0,114,0,105,0,101,0,
				115,0,62,0,89,0,60,0,47,0,83,0,101,0,97,0,114,0,99,0,104,0,83,0,117,0,98,0,100,0,105,0,114,0,101,0,99,0,116,0,
				111,0,114,0,105,0,101,0,115,0,62,0,60,0,47,0,68,0,105,0,114,0,101,0,99,0,116,0,111,0,114,0,121,0,83,0,101,0,97,0,
				114,0,99,0,104,0,62,0
			};

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(search, byteArrayValue)
			};
		}

		protected override string ExpectedEditorName
		{
			get { return "DirectorySearchRegistryItemEditor"; }
		}
	}
}
