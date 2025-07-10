using System;
using System.Collections.Generic;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business.Testing
{
	class TariffFormatterTest : Customs.Business.Testing.TariffFormatterTest
	{
		public void TestFormatForGermany()
		{
			AssertEquals("12345678901", TariffFormatter.New("DE").Format("123. 45 .67.8901"));
		}

		public override void TestFormat()
		{
			AssertEquals("1234567890", TariffFormatter.New("EUN").Format("123. 45 .67.890"));
		}

		public override void TestDisplayFormat()
		{
			AssertEquals("1234.56.78 90", Formatter.DisplayFormat("123. 45 .67.890"));
		}

		public void TestGetTariffType()
		{
			AssertEquals("EXP", TariffFormatter.GetTariffType(true));
			AssertEquals("IMP", TariffFormatter.GetTariffType(false));
		}

		public void TestNewTariffFormatter()
		{
			foreach (var dataGrouping in new EuropeanUnionCustomsMembersProvider().GetEuropeanUnionForCustomsMembersAndCustomsUnionAdditionalMembers())
			{
				var expectedType = ExpectedTariffFormatterTypes.ContainsKey(dataGrouping) ? ExpectedTariffFormatterTypes[dataGrouping] : typeof(TariffFormatterThirteen);
				AssertType(expectedType, TariffFormatter.New(dataGrouping));
			}
		}

		public static Dictionary<string, Type> ExpectedTariffFormatterTypes = new Dictionary<string, Type>
		{
			{ Core.Constants.CountryCodes.France, typeof(TariffFormatterTen) },
			{ Core.Constants.CountryCodes.Italy, typeof(TariffFormatterTen) },
			{ Core.Constants.CountryCodes.Spain, typeof(TariffFormatterTen) },
			{ Core.Constants.CountryCodes.UnitedKingdom, typeof(TariffFormatterTen) },
			{ Core.Constants.CountryCodes.Germany, typeof(TariffFormatterEleven) },
			{ Core.Constants.CountryCodes.Latvia, typeof(TariffFormatterThirteen) },
			{ Core.Constants.CountryCodes.Ireland, typeof(TariffFormatterThirteen) },
			{ Core.Constants.CountryCodes.Poland, typeof(TariffFormatterThirteen) },
			{ Core.Constants.CountryCodes.Belgium, typeof(TariffFormatterThirteen) },
			{ Core.Constants.CountryCodes.Turkey, typeof(TariffFormatterThirteen) },
			{ Core.Constants.CountryCodes.Netherlands, typeof(TariffFormatterThirteen) },
			{ Core.Constants.CountryCodes.Sweden, typeof(TariffFormatterThirteen) },
			{ Core.Constants.CountryCodes.Norway, typeof(TariffFormatterThirteen) },
			{ Core.Constants.CountryCodes.Denmark, typeof(TariffFormatterThirteen) },
			{ Core.Constants.CountryCodes.Finland, typeof(TariffFormatterThirteen) },
			{ Core.Constants.CountryCodes._EUTemplateCountryName_, typeof(TariffFormatterThirteen) }
		};
	}
}
