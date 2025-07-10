using CargoWise.Types;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(LastDbRestoreInfoRegistryDataType))]
	sealed class LastDbRestoreInfoRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<LastDbRestoreInfoRegistryDataType>
	{
		protected override LastDbRestoreInfoRegistryDataType GetNewDataType()
		{
			return new LastDbRestoreInfoRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var first = new LastDbRestoreInfo();
			var second = new LastDbRestoreInfo("op", "toolVersion", ZDateTime.BrettsBirthday.ToDateTime(), "69.0", "80.08");
			return new[]
			{
				new ValidSampleAndBinaryValueInDB(first, DataType.Serialise(first)),
				new ValidSampleAndBinaryValueInDB(second, DataType.Serialise(second)),
			};
		}

		protected override string ExpectedEditorName
		{
			get { return "LastDbRestoreRegistryItemEditor"; }
		}
	}
}
