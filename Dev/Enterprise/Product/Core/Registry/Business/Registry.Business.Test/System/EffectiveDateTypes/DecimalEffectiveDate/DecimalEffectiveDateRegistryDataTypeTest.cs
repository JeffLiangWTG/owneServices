using CargoWise.Types;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(DecimalEffectiveDateRegistryDataType))]
	sealed class DecimalEffectiveDateRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<DecimalEffectiveDateRegistryDataType>
	{
		protected override string ExpectedEditorName
		{
			get { return "DecimalEffectiveDateRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			DecimalEffectiveDate decimalEffectiveDate = new DecimalEffectiveDate();

			decimalEffectiveDate.PreviousValue = (ZDecimal)10;
			decimalEffectiveDate.NewValue = (ZDecimal)15;
			decimalEffectiveDate.EffectiveDate = new ZDateTime(2009, 6, 23);

			byte[] bytes = new byte[]
		{
255,254,60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,
0,102,0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,68,0,101,0,99,0,105,0,109,0,97,0,108,0,69,0,102,0,102,0,101,0,99,0,116,0,105,0,118,0,101,0,68,0,97,0,116,0,101,0,62,0,60,0,80,0,114,0,101,
0,118,0,105,0,111,0,117,0,115,0,86,0,97,0,108,0,117,0,101,0,62,0,49,0,48,0,60,0,47,0,80,0,114,0,101,0,118,0,105,0,111,0,117,0,115,0,86,0,97,0,108,0,117,0,101,0,62,0,60,0,78,0,101,0,119,
0,86,0,97,0,108,0,117,0,101,0,62,0,49,0,53,0,60,0,47,0,78,0,101,0,119,0,86,0,97,0,108,0,117,0,101,0,62,0,60,0,69,0,102,0,102,0,101,0,99,0,116,0,105,0,118,0,101,0,68,0,97,0,116,0,101,
0,62,0,50,0,48,0,48,0,57,0,48,0,54,0,50,0,51,0,60,0,47,0,69,0,102,0,102,0,101,0,99,0,116,0,105,0,118,0,101,0,68,0,97,0,116,0,101,0,62,0,60,0,47,0,68,0,101,0,99,0,105,0,109,0,97,
0,108,0,69,0,102,0,102,0,101,0,99,0,116,0,105,0,118,0,101,0,68,0,97,0,116,0,101,0,62,0
		};
			return new ValidSampleAndBinaryValueInDB[]
	  {
		new ValidSampleAndBinaryValueInDB(decimalEffectiveDate, bytes)
	  };
		}

		protected override DecimalEffectiveDateRegistryDataType GetNewDataType()
		{
			return new DecimalEffectiveDateRegistryDataType();
		}
	}
}
