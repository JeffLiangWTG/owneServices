using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class MeasureTypeWrapperTest : DataProviderTestCase<MeasureTypeWrapper>
	{
		public void TestValue()
		{
			AssertEquals("Value must be equal to the expected value", 100m, Provider.Value);
		}

		public void TestUnitCode()
		{
			AssertEquals("UnitCode must be equal to the expected value", MeasurementUnitCommonCodeContentType.Kgm, Provider.UnitCode);

			var provider = MeasureTypeWrapper.NewOrNull(22, null);
			AssertEquals("UnitCode must be empty when not provided", null, provider.UnitCode);

			provider = MeasureTypeWrapper.NewOrNull(22, "XYZ");
			AssertEquals("UnitCode must be empty when provided not existing", null, provider.UnitCode);
		}

		public void TestNewOrNull()
		{
			var provider = MeasureTypeWrapper.NewOrNull(ZDecimal.Zero, null);
			AssertNull("Provider", provider);

			provider = MeasureTypeWrapper.NewOrNull(22, null);
			AssertNotNull("Provider", provider);
		}

		protected override MeasureTypeWrapper GetProvider()
		{
			return MeasureTypeWrapper.NewOrNull(100, "KGM");
		}
	}
}
