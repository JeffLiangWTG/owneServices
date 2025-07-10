using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(PurgeSettingsRegistryDataType))]
	sealed class PurgeSettingsRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<PurgeSettingsRegistryDataType>
	{
		protected override PurgeSettingsRegistryDataType GetNewDataType()
		{
			return new PurgeSettingsRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "PurgeSettingsRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			byte[] byteArrayValue = new byte[]
{
60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,0,102,
0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,80,0,117,0,114,0,103,0,101,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,115,0,62,0,60,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,65,0,112,0,112,0,108,
0,105,0,99,0,97,0,116,0,105,0,111,0,110,0,67,0,111,0,100,0,101,0,79,0,98,0,106,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,105,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,
0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,45,0,105,0,110,0,115,0,116,0,97,0,110,0,99,
0,101,0,34,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,100,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,
0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,34,0,32,0,47,0,62,0,60,0,47,0,80,0,117,0,114,0,103,0,101,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,115,0,62,
0
};

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(new PurgeSettings(new PurgeSettingsRegistryDataType()), byteArrayValue)
			};
		}

		public override void TestValuesAreEqual()
		{
			var dataType = GetNewDataType();
			var samples = GetValidSamples();

			AssertEquals(1, samples.Length);
			AssertNotNull(samples[0]);

			var nonNullValidValue = samples[0];
			var item = (PurgeSettings)nonNullValidValue.ValidSample;

			var defaultPurgeSettings = eHubMessagingRegistry.GetDefaultPurgeSettings();
			foreach (ApplicationCodeObj currentAppCode in defaultPurgeSettings.ApplicationCodes)
			{
				item.ApplicationCodes.Add(currentAppCode);
			}
			AssertValuesAreEqualForBaseLine(dataType, item, item);
		}
	}
}
