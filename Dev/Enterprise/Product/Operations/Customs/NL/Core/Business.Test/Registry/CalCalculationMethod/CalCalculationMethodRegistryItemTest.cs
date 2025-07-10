using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(CalCalculationMethodRegistryItem))]
sealed class CalCalculationMethodRegistryItemTest : StronglyTypedRegistryItemTestCase<CalCalculationMethodRegistryCollection>
{
	protected override ZArchitecture.Environment.StronglyTypedRegistryItem<CalCalculationMethodRegistryCollection, CalCalculationMethodRegistryCollection> GetNewRegistryItem()
	{
		var registryCollection = new CalCalculationMethodRegistryCollection
		{
			new CalCalculationMethodRegistry { CalculationMethodName = CalculationMethodList.Codes.DUT, CalculationMethodValue = CalCalculationMethodRegistry.BasedOnDutiesVatValue, CalculationMethodDefault = CalCalculationMethodRegistry.BasedOnDutiesVatDefault },
			new CalCalculationMethodRegistry { CalculationMethodName = CalculationMethodList.Codes.WGT, CalculationMethodValue = CalCalculationMethodRegistry.BasedOnWeightValue, CalculationMethodDefault = CalCalculationMethodRegistry.BasedOnWeightDefault },
			new CalCalculationMethodRegistry { CalculationMethodName = CalculationMethodList.Codes.DEF, CalculationMethodValue = 1, CalculationMethodDefault = CalCalculationMethodRegistry.DefaultAmountPerEntryDefault },
		};

		return new CalCalculationMethodRegistryItem("", null, null, null, RegistryStorageFlags.All, registryCollection);
	}
}
