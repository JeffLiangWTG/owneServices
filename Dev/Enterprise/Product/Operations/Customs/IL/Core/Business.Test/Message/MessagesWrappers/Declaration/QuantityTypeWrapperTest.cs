using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class QuantityTypeWrapperTest : Customs.Business.Testing.DataProviderTestCase<IQuantityType>
	{
		public void TestNewOrNull()
		{
			AssertNotNull("When quantity = zero and unitQty is empty", QuantityTypeWrapper.NewOrNull(0, ""));
			AssertNotNull("When quantity = zero quantity and unitQty is valid", QuantityTypeWrapper.NewOrNull(0, "05"));
			AssertNotNull("When quantity != zero and unitQty is not valid", QuantityTypeWrapper.NewOrNull(1, "12325"));
			AssertNotNull("When quantity != zero quantity and unitQty is valid", QuantityTypeWrapper.NewOrNull(1, "05"));
		}

		public void TestUnitCode()
		{
			AssertEquals(MeasurementUnitCommonCodeContentType.Item1L, Provider.UnitCode);

			var wrapper = QuantityTypeWrapper.NewOrNull(1, "ILA");
			AssertEquals(MeasurementUnitCommonCodeContentType.Ila, wrapper.UnitCode);

			wrapper = QuantityTypeWrapper.NewOrNull(1, "12345");
			AssertNull("UnitCode", wrapper.UnitCode);

			wrapper = QuantityTypeWrapper.NewOrNull(1, "");
			AssertNull("UnitCode", wrapper.UnitCode);
		}

		public void TestValue()
		{
			AssertEquals(1m, Provider.Value);
		}

		protected override IQuantityType GetProvider() => QuantityTypeWrapper.NewOrNull(1, "1L");
	}
}
