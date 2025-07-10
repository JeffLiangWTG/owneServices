using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IL.Manifest.Business.MessagesWrappers;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class MeasureTypeWrapperTest : DataProviderTestCase<MeasureTypeWrapper>
	{
		public void TestValue()
		{
			AssertEquals("Value should be equal to the expected value", 17m, Provider.Value);
		}

		public void TestUnitCode()
		{
			AssertEquals("UnitCode should be equal to the expected value", MeasurementUnitCommonCodeContentType.Kgm, Provider.UnitCode);
		}

		protected override MeasureTypeWrapper GetProvider()
		{
			return MeasureTypeWrapper.NewOrNull(17, "KGM");
		}
	}
}
