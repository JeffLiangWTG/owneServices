using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CN.Business.Testing
{
	class CustomsUnitOfMeasurementListHelperTest : TestCaseWithFactory
	{
		public void TestIsWeightUnit()
		{
			AssertEquals(true, CustomsUnitOfMeasurementListHelper.IsWeightUnit(CustomsUnitOfMeasurementListHelper.Codes.Kilograms));
			AssertEquals(true, CustomsUnitOfMeasurementListHelper.IsWeightUnit(CustomsUnitOfMeasurementListHelper.Codes.Carats));
			AssertEquals(false, CustomsUnitOfMeasurementListHelper.IsWeightUnit(CustomsUnitOfMeasurementListHelper.Codes.CubicMetres));
			AssertEquals(false, CustomsUnitOfMeasurementListHelper.IsWeightUnit(CustomsUnitOfMeasurementListHelper.Codes.SquareMetres));
		}
	}
}
