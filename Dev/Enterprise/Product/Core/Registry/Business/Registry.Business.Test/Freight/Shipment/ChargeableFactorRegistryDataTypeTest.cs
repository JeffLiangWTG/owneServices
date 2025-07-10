using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ChargeableFactorRegistryDataType))]
	sealed class ChargeableFactorRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<ChargeableFactorRegistryDataType>
	{
		#region Implementation

		protected override ChargeableFactorRegistryDataType GetNewDataType()
		{
			return new ChargeableFactorRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "ChargeableFactorRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var sample = new ChargeableFactor(ConversionFactor.Standard.Metric.Sea, ConversionFactor.Standard.Imperial.Sea);

			#region ByteArrayValue

			byte[] byteArrayValue = new byte[]
			{
				255,254,60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,
				0,102,0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,67,0,104,0,97,0,114,0,103,0,101,0,97,0,98,0,108,0,101,0,70,0,97,0,99,0,116,0,111,0,114,0,62,0,60,0,77,0,101,0,116,0,114,0,105,0,99,0,70,
				0,97,0,99,0,116,0,111,0,114,0,62,0,49,0,48,0,48,0,48,0,60,0,47,0,77,0,101,0,116,0,114,0,105,0,99,0,70,0,97,0,99,0,116,0,111,0,114,0,62,0,60,0,73,0,109,0,112,0,101,0,114,0,105,0,97,
				0,108,0,70,0,97,0,99,0,116,0,111,0,114,0,62,0,49,0,48,0,48,0,60,0,47,0,73,0,109,0,112,0,101,0,114,0,105,0,97,0,108,0,70,0,97,0,99,0,116,0,111,0,114,0,62,0,60,0,47,0,67,0,104,0,97,
				0,114,0,103,0,101,0,97,0,98,0,108,0,101,0,70,0,97,0,99,0,116,0,111,0,114,0,62,0
			};

			#endregion

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(sample, byteArrayValue)
			};
		}

		#endregion
	}
}
