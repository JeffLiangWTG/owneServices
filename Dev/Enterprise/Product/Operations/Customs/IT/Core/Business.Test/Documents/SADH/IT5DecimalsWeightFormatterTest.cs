using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class IT5DecimalsWeightFormatterTest : TestCase
{
	public void TestGetFormattedValue()
	{
		var testWeight = 77.123456m;
		AssertEquals("Weight gets rounded to 5 decimals", "77.12346", new IT5DecimalsWeightFormatter(testWeight).GetFormattedValue());

		testWeight = 12.12000000m;
		AssertEquals("Zeros must be trimmed to the last meaningful decimal", "12.12", new IT5DecimalsWeightFormatter(testWeight).GetFormattedValue());

		testWeight = 12.00000000m;
		AssertEquals("No decimal must be shown when not meaningful", "12", new IT5DecimalsWeightFormatter(testWeight).GetFormattedValue());

		testWeight = ZDecimal.Zero;
		AssertEquals("when zero, the formatted weight is empty string", ZString.Empty, new IT5DecimalsWeightFormatter(testWeight).GetFormattedValue());
	}
}
