using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ApplicationIdentifierRegistryDataType))]
	sealed class ApplicationIdentifierRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<ApplicationIdentifierRegistryDataType>
	{
		#region Implementation

		protected override ApplicationIdentifierRegistryDataType GetNewDataType()
		{
			return new ApplicationIdentifierRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "ApplicationIdentifierRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			ApplicationIdentifierCollection collection = new ApplicationIdentifierCollection();

			ApplicationIdentifier applicationIdentifier = collection.AddNew();
			applicationIdentifier.ApplicationID = "00";
			applicationIdentifier.EnglishFullTitle = "Serial Shipping Container Code";
			applicationIdentifier.EnglishDataTitle = "SSCC";
			applicationIdentifier.DataType = "D";
			applicationIdentifier.MinFieldLength = 18;
			applicationIdentifier.MaxFieldLength = 18;

			#region ByteArrayValue

			byte[] byteArrayValue = new byte[]
{
60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,0,102,
0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,65,0,112,0,112,0,108,0,105,0,99,0,97,0,116,0,105,0,111,0,110,0,73,0,100,0,101,0,110,0,116,0,105,0,102,0,105,
0,101,0,114,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,100,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,
0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,34,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,105,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,
0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,45,0,105,0,110,0,115,0,116,0,97,0,110,0,99,
0,101,0,34,0,62,0,60,0,65,0,112,0,112,0,108,0,105,0,99,0,97,0,116,0,105,0,111,0,110,0,73,0,100,0,101,0,110,0,116,0,105,0,102,0,105,0,101,0,114,0,62,0,60,0,65,0,112,0,112,0,108,0,105,0,99,
0,97,0,116,0,105,0,111,0,110,0,73,0,68,0,62,0,48,0,48,0,60,0,47,0,65,0,112,0,112,0,108,0,105,0,99,0,97,0,116,0,105,0,111,0,110,0,73,0,68,0,62,0,60,0,70,0,117,0,108,0,108,0,84,0,105,
0,116,0,108,0,101,0,62,0,83,0,101,0,114,0,105,0,97,0,108,0,32,0,83,0,104,0,105,0,112,0,112,0,105,0,110,0,103,0,32,0,67,0,111,0,110,0,116,0,97,0,105,0,110,0,101,0,114,0,32,0,67,0,111,0,100,
0,101,0,60,0,47,0,70,0,117,0,108,0,108,0,84,0,105,0,116,0,108,0,101,0,62,0,60,0,68,0,97,0,116,0,97,0,84,0,105,0,116,0,108,0,101,0,62,0,83,0,83,0,67,0,67,0,60,0,47,0,68,0,97,0,116,
0,97,0,84,0,105,0,116,0,108,0,101,0,62,0,60,0,68,0,97,0,116,0,97,0,84,0,121,0,112,0,101,0,62,0,68,0,60,0,47,0,68,0,97,0,116,0,97,0,84,0,121,0,112,0,101,0,62,0,60,0,77,0,105,0,110,
0,70,0,105,0,101,0,108,0,100,0,76,0,101,0,110,0,103,0,116,0,104,0,62,0,49,0,56,0,60,0,47,0,77,0,105,0,110,0,70,0,105,0,101,0,108,0,100,0,76,0,101,0,110,0,103,0,116,0,104,0,62,0,60,0,77,
0,97,0,120,0,70,0,105,0,101,0,108,0,100,0,76,0,101,0,110,0,103,0,116,0,104,0,62,0,49,0,56,0,60,0,47,0,77,0,97,0,120,0,70,0,105,0,101,0,108,0,100,0,76,0,101,0,110,0,103,0,116,0,104,0,62,
0,60,0,47,0,65,0,112,0,112,0,108,0,105,0,99,0,97,0,116,0,105,0,111,0,110,0,73,0,100,0,101,0,110,0,116,0,105,0,102,0,105,0,101,0,114,0,62,0,60,0,47,0,65,0,114,0,114,0,97,0,121,0,79,0,102,
0,65,0,112,0,112,0,108,0,105,0,99,0,97,0,116,0,105,0,111,0,110,0,73,0,100,0,101,0,110,0,116,0,105,0,102,0,105,0,101,0,114,0,62,0
};

			#endregion

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, byteArrayValue)
			};
		}

		#endregion
	}
}
