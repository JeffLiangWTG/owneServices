using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CH.NCTS.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

[TestedType(typeof(NctsTariffGridFindBoxTest))]
class NctsTariffGridFindBoxTest : TestCaseWithFactory
{
	public void TestListProvider() => CombineAssertions(() =>
	{
		new RefDataTestHelper(Factory).CreateTariffsForTransit();

		using (var findBox = new NctsTariffGridFindBoxForTesting())
		{
			findBox.CodeBox.Text = "710121";
			AssertEquals($"WCO Tariff code = {findBox.CodeBox.Text}", "natural pearls unworked", findBox.ListProviderExposed.DescriptionFromCode(findBox.CodeBox.Text));
			findBox.CodeBox.Text = "04069099001";
			AssertEquals($"CH EXP Tariff code = {findBox.CodeBox.Text}", "cheese", findBox.ListProviderExposed.DescriptionFromCode(findBox.CodeBox.Text));
		}
	});

	class NctsTariffGridFindBoxForTesting : NctsTariffGridFindBox
	{
		internal IFindBoxListProvider ListProviderExposed => base.ListProvider;
	}
}
