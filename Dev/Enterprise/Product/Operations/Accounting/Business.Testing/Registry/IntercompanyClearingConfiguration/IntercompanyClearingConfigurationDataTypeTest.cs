using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(IntercompanyClearingConfigurationRegistryDataType))]
	class IntercompanyClearingConfigurationDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<IntercompanyClearingConfigurationRegistryDataType>
	{
		#region Implementation

		protected override IntercompanyClearingConfigurationRegistryDataType GetNewDataType()
		{
			return new IntercompanyClearingConfigurationRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "IntercompanyClearingConfigurationRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			IntercompanyClearingConfigurationCollection collection = new IntercompanyClearingConfigurationCollection();
			IntercompanyClearingConfiguration intercompanyClearingConfiguration = collection.AddNew();

			/*			byte[] ByteArrayValue = new byte[]
			{
			60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,
			0,102,0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,73,0,110,0,116,0,101,0,114,0,99,0,111,0,109,0,112,0,97,0,110,0,121,0,67,0,108,0,101,0,97,0,114,0,105,
			0,110,0,103,0,67,0,111,0,110,0,102,0,105,0,103,0,117,0,114,0,97,0,116,0,105,0,111,0,110,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,105,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,
			0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,45,0,105,0,110,0,115,0,116,0,97,0,110,
			0,99,0,101,0,34,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,100,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,
			0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,34,0,62,0,60,0,73,0,110,0,116,0,101,0,114,0,99,0,111,0,109,0,112,0,97,0,110,0,121,0,67,0,108,0,101,0,97,
			0,114,0,105,0,110,0,103,0,67,0,111,0,110,0,102,0,105,0,103,0,117,0,114,0,97,0,116,0,105,0,111,0,110,0,62,0,60,0,67,0,111,0,109,0,112,0,97,0,110,0,121,0,32,0,47,0,62,0,60,0,67,0,108,0,101,
			0,97,0,114,0,105,0,110,0,103,0,71,0,76,0,65,0,99,0,99,0,111,0,117,0,110,0,116,0,62,0,48,0,48,0,48,0,48,0,48,0,48,0,48,0,48,0,45,0,48,0,48,0,48,0,48,0,45,0,48,0,48,0,48,0,48,
			0,45,0,48,0,48,0,48,0,48,0,45,0,48,0,48,0,48,0,48,0,48,0,48,0,48,0,48,0,48,0,48,0,48,0,48,0,60,0,47,0,67,0,108,0,101,0,97,0,114,0,105,0,110,0,103,0,71,0,76,0,65,0,99,0,99,
			0,111,0,117,0,110,0,116,0,62,0,60,0,47,0,73,0,110,0,116,0,101,0,114,0,99,0,111,0,109,0,112,0,97,0,110,0,121,0,67,0,108,0,101,0,97,0,114,0,105,0,110,0,103,0,67,0,111,0,110,0,102,0,105,0,103,
			0,117,0,114,0,97,0,116,0,105,0,111,0,110,0,62,0,60,0,47,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,73,0,110,0,116,0,101,0,114,0,99,0,111,0,109,0,112,0,97,0,110,0,121,0,67,0,108,0,101,0,97,
			0,114,0,105,0,110,0,103,0,67,0,111,0,110,0,102,0,105,0,103,0,117,0,114,0,97,0,116,0,105,0,111,0,110,0,62,0
			};*/

			string text = "<?xml version=\"1.0\" encoding=\"utf-16\"?><ArrayOfIntercompanyClearingConfiguration xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><IntercompanyClearingConfiguration><Company></Company><ClearingGLAccount>00000000-0000-0000-0000-000000000000</ClearingGLAccount></IntercompanyClearingConfiguration></ArrayOfIntercompanyClearingConfiguration>";
			byte[] tempArray = System.Text.Encoding.Unicode.GetBytes(text);
			byte[] byteArrayValue = new byte[tempArray.Length + 2];
			byteArrayValue[0] = 255;
			byteArrayValue[1] = 254;
			for (int i = 0; i < tempArray.Length; i++)
			{
				byteArrayValue[i + 2] = tempArray[i];
			}

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, byteArrayValue)
			};
		}

		#endregion
	}
}
