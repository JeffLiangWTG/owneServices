using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Messaging.Testing
{
	sealed class KRCurrencyCodeListTest : NUnit.Framework.TestCase
	{
		public void TestList()
		{
			var list = Constants.KrCurrencyCodeList;
			AssertEquals(58, list.Length);
			AssertEquals(Core.Constants.CurrencyCodes.UnitedArabEmirates, list[0]);
			AssertEquals(Core.Constants.CurrencyCodes.Argentina, list[1]);
			AssertEquals(Core.Constants.CurrencyCodes.Australia, list[2]);
			AssertEquals(Core.Constants.CurrencyCodes.Bangladesh, list[3]);
			AssertEquals(Core.Constants.CurrencyCodes.Bahrain, list[4]);
			AssertEquals(Core.Constants.CurrencyCodes.BruneiDarussalam, list[5]);
			AssertEquals(Core.Constants.CurrencyCodes.Brazil, list[6]);
			AssertEquals(Core.Constants.CurrencyCodes.Canada, list[7]);
			AssertEquals(Core.Constants.CurrencyCodes.Liechtenstein, list[8]);
			AssertEquals(Core.Constants.CurrencyCodes.Chile, list[9]);
			AssertEquals(Core.Constants.CurrencyCodes.China, list[10]);
			AssertEquals(Core.Constants.CurrencyCodes.Colombia, list[11]);
			AssertEquals(Core.Constants.CurrencyCodes.CzechRepublic, list[12]);
			AssertEquals(Core.Constants.CurrencyCodes.Denmark, list[13]);
			AssertEquals(Core.Constants.CurrencyCodes.Egypt, list[14]);
			AssertEquals(Core.Constants.CurrencyCodes.Ethiopia, list[15]);
			AssertEquals(Core.Constants.CurrencyCodes.EuropeanUnion, list[16]);
			AssertEquals(Core.Constants.CurrencyCodes.Fiji, list[17]);
			AssertEquals(Core.Constants.CurrencyCodes.SouthGeorgiaAndTheSouthSandwic, list[18]);
			AssertEquals(Core.Constants.CurrencyCodes.HongKong, list[19]);
			AssertEquals(Core.Constants.CurrencyCodes.Hungary, list[20]);
			AssertEquals(Core.Constants.CurrencyCodes.Indonesia, list[21]);
			AssertEquals(Core.Constants.CurrencyCodes.Israel, list[22]);
			AssertEquals(Core.Constants.CurrencyCodes.India, list[23]);
			AssertEquals(Core.Constants.CurrencyCodes.Jordan, list[24]);
			AssertEquals(Core.Constants.CurrencyCodes.Japan, list[25]);
			AssertEquals(Core.Constants.CurrencyCodes.Kenya, list[26]);
			AssertEquals(Core.Constants.CurrencyCodes.Cambodia, list[27]);
			AssertEquals(Core.Constants.CurrencyCodes.KoreaRepublicOf, list[28]);
			AssertEquals(Core.Constants.CurrencyCodes.Kuwait, list[29]);
			AssertEquals(Core.Constants.CurrencyCodes.Kazakhstan, list[30]);
			AssertEquals(Core.Constants.CurrencyCodes.SriLanka, list[31]);
			AssertEquals(Core.Constants.CurrencyCodes.LibyanArabJamahiriya, list[32]);
			AssertEquals(Core.Constants.CurrencyCodes.Myanmar, list[33]);
			AssertEquals(Core.Constants.CurrencyCodes.Mongolia, list[34]);
			AssertEquals(Core.Constants.CurrencyCodes.Macau, list[35]);
			AssertEquals(Core.Constants.CurrencyCodes.Mexico, list[36]);
			AssertEquals(Core.Constants.CurrencyCodes.Malaysia, list[37]);
			AssertEquals(Core.Constants.CurrencyCodes.Norway, list[38]);
			AssertEquals(Core.Constants.CurrencyCodes.Nepal, list[39]);
			AssertEquals(Core.Constants.CurrencyCodes.NewZealand, list[40]);
			AssertEquals(Core.Constants.CurrencyCodes.Oman, list[41]);
			AssertEquals(Core.Constants.CurrencyCodes.Philippines, list[42]);
			AssertEquals(Core.Constants.CurrencyCodes.Pakistan, list[43]);
			AssertEquals(Core.Constants.CurrencyCodes.Poland, list[44]);
			AssertEquals(Core.Constants.CurrencyCodes.Qatar, list[45]);
			AssertEquals(Core.Constants.CurrencyCodes.RomaniaNew, list[46]);
			AssertEquals(KrCurrency.RussianRouble, list[47]);
			AssertEquals(Core.Constants.CurrencyCodes.SaudiArabia, list[48]);
			AssertEquals(Core.Constants.CurrencyCodes.Sweden, list[49]);
			AssertEquals(Core.Constants.CurrencyCodes.Singapore, list[50]);
			AssertEquals(Core.Constants.CurrencyCodes.Thailand, list[51]);
			AssertEquals(Core.Constants.CurrencyCodes.Turkey, list[52]);
			AssertEquals(Core.Constants.CurrencyCodes.Taiwan, list[53]);
			AssertEquals(Core.Constants.CurrencyCodes.UnitedStates, list[54]);
			AssertEquals(Core.Constants.CurrencyCodes.Uzbekistan, list[55]);
			AssertEquals(Core.Constants.CurrencyCodes.VietNam, list[56]);
			AssertEquals(Core.Constants.CurrencyCodes.SouthAfrica, list[57]);
		}
	}
}
