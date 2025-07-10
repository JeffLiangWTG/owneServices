using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(CalCalculationMethodRegistryDataType))]
sealed class CalCalculationMethodRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<CalCalculationMethodRegistryDataType>
{
	protected override CalCalculationMethodRegistryDataType GetNewDataType() => new CalCalculationMethodRegistryDataType();

	protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
	{
		var basedOnDutiesVat = new CalCalculationMethodRegistry { CalculationMethodName = CalculationMethodList.Codes.DUT, CalculationMethodValue = ZDecimal.Zero, CalculationMethodDefault = ZBool.True };
		var basedOnWeight = new CalCalculationMethodRegistry { CalculationMethodName = CalculationMethodList.Codes.WGT, CalculationMethodValue = ZDecimal.Zero, CalculationMethodDefault = ZBool.False };
		var defaultAmountPerEntry = new CalCalculationMethodRegistry { CalculationMethodName = CalculationMethodList.Codes.DEF, CalculationMethodValue = 1, CalculationMethodDefault = ZBool.False };
		var collection = new CalCalculationMethodRegistryCollection { basedOnDutiesVat, basedOnWeight, defaultAmountPerEntry };
		var emptyCollection = new CalCalculationMethodRegistryCollection();

		return new[]
		{
			new ValidSampleAndBinaryValueInDB(collection, DataType.Serialise(collection)),
			new ValidSampleAndBinaryValueInDB(emptyCollection, DataType.Serialise(emptyCollection))
		};
	}

	protected override string ExpectedEditorName => "CalCalculationMethodRegistryItemEditor";
}
