using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.eHub.Testing
{
	[TestedType(typeof(BillingTransactionsTransformationSettingsRegistryDataType))]
	sealed class BillingTransactionsTransformationSettingsRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<BillingTransactionsTransformationSettingsRegistryDataType>
	{
		protected override BillingTransactionsTransformationSettingsRegistryDataType GetNewDataType()
		{
			return new BillingTransactionsTransformationSettingsRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "BillingTransactionsTransformationSettingsRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var emptySettingsBinaryValue = new byte[]
{
60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,0,102,
0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,66,0,105,0,108,0,108,0,105,0,110,0,103,0,84,0,114,0,97,0,110,0,115,0,97,0,99,0,116,0,105,0,111,0,110,0,115,0,84,0,114,0,97,0,110,0,115,0,102,0,111,
0,114,0,109,0,97,0,116,0,105,0,111,0,110,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,115,0,62,0,60,0,67,0,117,0,114,0,114,0,101,0,110,0,116,0,70,0,105,0,120,0,73,0,110,0,100,0,101,0,120,0,62,
0,48,0,60,0,47,0,67,0,117,0,114,0,114,0,101,0,110,0,116,0,70,0,105,0,120,0,73,0,110,0,100,0,101,0,120,0,62,0,60,0,67,0,117,0,114,0,114,0,101,0,110,0,116,0,70,0,105,0,120,0,83,0,116,0,97,
0,116,0,101,0,62,0,48,0,60,0,47,0,67,0,117,0,114,0,114,0,101,0,110,0,116,0,70,0,105,0,120,0,83,0,116,0,97,0,116,0,101,0,62,0,60,0,47,0,66,0,105,0,108,0,108,0,105,0,110,0,103,0,84,0,114,
0,97,0,110,0,115,0,97,0,99,0,116,0,105,0,111,0,110,0,115,0,84,0,114,0,97,0,110,0,115,0,102,0,111,0,114,0,109,0,97,0,116,0,105,0,111,0,110,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,115,0,62,
0
};

			var nonEmptySettingsBinaryValue = new byte[]
{
60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,0,102,
0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,66,0,105,0,108,0,108,0,105,0,110,0,103,0,84,0,114,0,97,0,110,0,115,0,97,0,99,0,116,0,105,0,111,0,110,0,115,0,84,0,114,0,97,0,110,0,115,0,102,0,111,
0,114,0,109,0,97,0,116,0,105,0,111,0,110,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,115,0,62,0,60,0,67,0,117,0,114,0,114,0,101,0,110,0,116,0,70,0,105,0,120,0,73,0,110,0,100,0,101,0,120,0,62,
0,49,0,60,0,47,0,67,0,117,0,114,0,114,0,101,0,110,0,116,0,70,0,105,0,120,0,73,0,110,0,100,0,101,0,120,0,62,0,60,0,67,0,117,0,114,0,114,0,101,0,110,0,116,0,70,0,105,0,120,0,83,0,116,0,97,
0,116,0,101,0,62,0,50,0,60,0,47,0,67,0,117,0,114,0,114,0,101,0,110,0,116,0,70,0,105,0,120,0,83,0,116,0,97,0,116,0,101,0,62,0,60,0,47,0,66,0,105,0,108,0,108,0,105,0,110,0,103,0,84,0,114,
0,97,0,110,0,115,0,97,0,99,0,116,0,105,0,111,0,110,0,115,0,84,0,114,0,97,0,110,0,115,0,102,0,111,0,114,0,109,0,97,0,116,0,105,0,111,0,110,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,115,0,62,
0
};

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(new BillingTransactionsTransformationSettings(), emptySettingsBinaryValue),
				new ValidSampleAndBinaryValueInDB(new BillingTransactionsTransformationSettings { CurrentFixIndex = 1, CurrentFixState = 2 }, nonEmptySettingsBinaryValue),
			};
		}
	}
}
