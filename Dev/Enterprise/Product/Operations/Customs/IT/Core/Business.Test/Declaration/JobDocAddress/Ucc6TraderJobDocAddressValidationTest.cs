using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class Ucc6TraderJobDocAddressValidationTest : TraderJobDocAddressValidationTest
{
	public void TestRequiredCodeTypeListForEuropeanTrader()
	{
		var exportJobDocAddressValidation = new Ucc6TraderJobDocAddressValidationForTest(Factory.New<JobDocAddress>(), "Mock", Factory.New<JobDeclaration>());
		AssertArrayEqualsByElements("RequiredCodeTypeListForEuropeanTrader count", System.Array.Empty<ZString>(), exportJobDocAddressValidation.GetRequiredCodeTypeListForEuropeanTraderExposed());
	}

	class Ucc6TraderJobDocAddressValidationForTest : Ucc6TraderJobDocAddressValidation
	{
		public Ucc6TraderJobDocAddressValidationForTest(AutoJobDocAddress parent, string traderName, JobDeclaration declaration) : base(parent, traderName, declaration)
		{
		}

		public ZString[] GetRequiredCodeTypeListForEuropeanTraderExposed()
		{
			return GetRequiredCodeTypeListForEuropeanTrader(null);
		}
	}
}
