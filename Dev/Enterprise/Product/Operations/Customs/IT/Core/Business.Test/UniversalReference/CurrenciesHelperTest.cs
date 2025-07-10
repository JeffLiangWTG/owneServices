using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class CurrenciesHelperTest : TestCase
{
	public void TestIsCurrencyRecognizedByCustoms()
	{
		AssertEquals(true, CurrenciesHelper.IsCurrencyRecognizedByCustoms(CurrencyCodes.EuropeanUnion));
		AssertEquals(true, CurrenciesHelper.IsCurrencyRecognizedByCustoms(CurrencyCodes.UnitedStates));
		AssertEquals(true, CurrenciesHelper.IsCurrencyRecognizedByCustoms(CurrencyCodes.Japan));
		AssertEquals(true, CurrenciesHelper.IsCurrencyRecognizedByCustoms(CurrencyCodes.CzechRepublic));
		AssertEquals(true, CurrenciesHelper.IsCurrencyRecognizedByCustoms(CurrencyCodes.Denmark));
		AssertEquals(true, CurrenciesHelper.IsCurrencyRecognizedByCustoms(CurrencyCodes.UnitedKingdom));
		AssertEquals(true, CurrenciesHelper.IsCurrencyRecognizedByCustoms(CurrencyCodes.Hungary));
		AssertEquals(true, CurrenciesHelper.IsCurrencyRecognizedByCustoms(CurrencyCodes.Poland));
		AssertEquals(true, CurrenciesHelper.IsCurrencyRecognizedByCustoms(CurrencyCodes.Romania));
		AssertEquals(true, CurrenciesHelper.IsCurrencyRecognizedByCustoms(CurrencyCodes.Sweden));
		AssertEquals(true, CurrenciesHelper.IsCurrencyRecognizedByCustoms(CurrencyCodes.Switzerland));
		AssertEquals(true, CurrenciesHelper.IsCurrencyRecognizedByCustoms(CurrencyCodes.Norway));
		AssertEquals(true, CurrenciesHelper.IsCurrencyRecognizedByCustoms(CurrencyCodes.Croatia));
		AssertEquals(true, CurrenciesHelper.IsCurrencyRecognizedByCustoms(CurrencyCodes.Russia));
		AssertEquals(true, CurrenciesHelper.IsCurrencyRecognizedByCustoms(CurrencyCodes.Turkey));
		AssertEquals(true, CurrenciesHelper.IsCurrencyRecognizedByCustoms(CurrencyCodes.Australia));
		AssertEquals(true, CurrenciesHelper.IsCurrencyRecognizedByCustoms(CurrencyCodes.Brazil));
		AssertEquals(true, CurrenciesHelper.IsCurrencyRecognizedByCustoms(CurrencyCodes.Canada));
		AssertEquals(true, CurrenciesHelper.IsCurrencyRecognizedByCustoms(CurrencyCodes.China));
		AssertEquals(true, CurrenciesHelper.IsCurrencyRecognizedByCustoms(CurrencyCodes.HongKong));
		AssertEquals(true, CurrenciesHelper.IsCurrencyRecognizedByCustoms(CurrencyCodes.Israel));
		AssertEquals(true, CurrenciesHelper.IsCurrencyRecognizedByCustoms(CurrencyCodes.India));
		AssertEquals(true, CurrenciesHelper.IsCurrencyRecognizedByCustoms(CurrencyCodes.NewZealand));
		AssertEquals(true, CurrenciesHelper.IsCurrencyRecognizedByCustoms(CurrencyCodes.SouthAfrica));
		AssertEquals(true, CurrenciesHelper.IsCurrencyRecognizedByCustoms(CurrencyCodes.KoreaRepublicOf));
		AssertEquals(true, CurrenciesHelper.IsCurrencyRecognizedByCustoms(CurrencyCodes.Mexico));
		AssertEquals(true, CurrenciesHelper.IsCurrencyRecognizedByCustoms(CurrencyCodes.Malaysia));
		AssertEquals(true, CurrenciesHelper.IsCurrencyRecognizedByCustoms(CurrencyCodes.Philippines));
		AssertEquals(true, CurrenciesHelper.IsCurrencyRecognizedByCustoms(CurrencyCodes.Singapore));
		AssertEquals(true, CurrenciesHelper.IsCurrencyRecognizedByCustoms(CurrencyCodes.Thailand));
		AssertEquals(false, CurrenciesHelper.IsCurrencyRecognizedByCustoms(""));
		AssertEquals(false, CurrenciesHelper.IsCurrencyRecognizedByCustoms("XYZ"));
	}
}
