using CargoWise.Types;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(StringEffectiveDateRegistryDataType))]
	sealed class StringEffectiveDateRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<StringEffectiveDateRegistryDataType>
	{
		protected override string ExpectedEditorName
		{
			get { return "StringEffectiveDateRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			StringEffectiveDate stringEffectiveDate = new StringEffectiveDate();

			stringEffectiveDate.PreviousValue = "AAA";
			stringEffectiveDate.NewValue = "BBB";
			stringEffectiveDate.EffectiveDate = new ZDateTime(2009, 6, 23);

			byte[] bytes = new byte[]
		{
60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,0,102,
0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,83,0,116,0,114,0,105,0,110,0,103,0,69,0,102,0,102,0,101,0,99,0,116,0,105,0,118,0,101,0,68,0,97,0,116,0,101,0,62,0,60,0,80,0,114,0,101,0,118,0,105,
0,111,0,117,0,115,0,86,0,97,0,108,0,117,0,101,0,62,0,65,0,65,0,65,0,60,0,47,0,80,0,114,0,101,0,118,0,105,0,111,0,117,0,115,0,86,0,97,0,108,0,117,0,101,0,62,0,60,0,78,0,101,0,119,0,86,
0,97,0,108,0,117,0,101,0,62,0,66,0,66,0,66,0,60,0,47,0,78,0,101,0,119,0,86,0,97,0,108,0,117,0,101,0,62,0,60,0,69,0,102,0,102,0,101,0,99,0,116,0,105,0,118,0,101,0,68,0,97,0,116,0,101,
0,62,0,50,0,48,0,48,0,57,0,48,0,54,0,50,0,51,0,60,0,47,0,69,0,102,0,102,0,101,0,99,0,116,0,105,0,118,0,101,0,68,0,97,0,116,0,101,0,62,0,60,0,47,0,83,0,116,0,114,0,105,0,110,0,103,
0,69,0,102,0,102,0,101,0,99,0,116,0,105,0,118,0,101,0,68,0,97,0,116,0,101,0,62,0
};

			return new ValidSampleAndBinaryValueInDB[]
	  {
		new ValidSampleAndBinaryValueInDB(stringEffectiveDate, bytes)
	  };
		}

		protected override StringEffectiveDateRegistryDataType GetNewDataType()
		{
			return new StringEffectiveDateRegistryDataType();
		}
	}
}
