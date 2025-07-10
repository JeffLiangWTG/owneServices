#region SuppressResourceStringsCheckRegion

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities
{
	public class CurrencyToWords_RU_RU : SlavicCurrencyConverter
	{
		public CurrencyToWords_RU_RU()
			: base(new NumberToString_RU_RU(), "один", "{0} {1}", "{0}", "{0} {1} {2}")
		{ }

		protected override string MinorUnitsAmountToString(long value)
		{
			return numberToWords.GetNumberAsString(value);
		}

		internal override ImmutableDictionary<string, SlavicCurrencyForms> CurrenciesDictionary
		{
			get { return currenciesDictionary.Value; }
		}

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		static readonly Lazy<ImmutableDictionary<string, SlavicCurrencyForms>> currenciesDictionary = new Lazy<ImmutableDictionary<string, SlavicCurrencyForms>>(() =>
		{
			var result = new Dictionary<string, SlavicCurrencyForms>()
			{
				{ "ALL", new SlavicCurrencyForms("лек", "лека", "леков", "киндарка", "киндарки", "киндарок") },
				{ "AUD", new SlavicCurrencyForms("доллар", "доллара", "долларов", "цент", "цента", "центов") },
				{ "BGN", new SlavicCurrencyForms("лев", "лева", "левов", "стотинка", "стотинки", "стотинок") },
				{ "BRL", new SlavicCurrencyForms("реал", "реала", "реалов", "сентаво", "сентаво", "сентаво") },
				{ "BYR", new SlavicCurrencyForms("рубль", "рубля", "рублей", "копейка", "копейки", "копеек") },
				{ "CAD", new SlavicCurrencyForms("доллар", "доллара", "долларов", "цент", "цента", "центов") },
				{ "CHF", new SlavicCurrencyForms("франк", "франка", "франков", "сантим", "сантима", "сантимов") },
				{ "CYP", new SlavicCurrencyForms("фунт", "фунта", "фунтов", "цент", "цента", "центов") },
				{ "CZK", new SlavicCurrencyForms("крона", "кроны", "крон", "галирж", "галиржа", "галиржей") },
				{ "DKK", new SlavicCurrencyForms("крона", "кроны", "крон", "эре", "эре", "эре") },
				{ "EEK", new SlavicCurrencyForms("крона", "кроны", "крон", "сенти", "сенти", "сенти") },
				{ "EUR", new SlavicCurrencyForms("евро", "евро", "евро", "евроцент", "евроцента", "евроцентов") },
				{ "GBP", new SlavicCurrencyForms("фунт", "фунта", "фунтов", "пенс", "пенса", "пенсов") },
				{ "HKD", new SlavicCurrencyForms("доллар", "доллара", "долларов", "цент", "цента", "центов") },
				{ "HRK", new SlavicCurrencyForms("куна", "куны", "кун", "липа", "липы", "лип") },
				{ "HUF", new SlavicCurrencyForms("форинт", "форинта", "форинтов", "филлер", "филлера", "филлеров") },
				{ "ISK", new SlavicCurrencyForms("крона", "кроны", "крон", "эре", "эре", "эре") },
				{ "JPY", new SlavicCurrencyForms("иена", "иены", "иен", "", "", "") },
				{ "LTL", new SlavicCurrencyForms("лит", "лита", "литов", "цент", "цента", "центов") },
				{ "LVL", new SlavicCurrencyForms("лат", "лата", "латов", "сентим", "сентима", "сентимов") },
				{ "MKD", new SlavicCurrencyForms("динар", "динара", "динаров", "дени", "дени", "дени") },
				{ "MTL", new SlavicCurrencyForms("лира", "лиры", "лир", "сентим", "сентима", "сентимов") },
				{ "NOK", new SlavicCurrencyForms("крона", "кроны", "крон", "эре", "эре", "эре") },
				{ "PLN", new SlavicCurrencyForms("злотый", "злотых", "злотых", "грош", "гроша", "грошей") },
				{ "ROL", new SlavicCurrencyForms("лей", "лей", "лей", "бани", "бани", "бани") },
				{ "RUB", new SlavicCurrencyForms("рубль", "рубля", "рублей", "копейка", "копейки", "копеек") },
				{ "RUR", new SlavicCurrencyForms("рубль", "рубля", "рублей", "копейка", "копейки", "копеек") },
				{ "SEK", new SlavicCurrencyForms("крона", "кроны", "крон", "эре", "эре", "эре") },
				{ "SIT", new SlavicCurrencyForms("толар", "толара", "толаров", "стотина", "стотины", "стотин") },
				{ "SKK", new SlavicCurrencyForms("крона", "кроны", "крон", "", "", "") },
				{ "TRL", new SlavicCurrencyForms("лира", "лиры", "лир", "пиастр", "пиастра", "пиастров") },
				{ "UAH", new SlavicCurrencyForms("гривна", "гривны", "гривен", "копейка", "копейки", "копеек") },
				{ "USD", new SlavicCurrencyForms("доллар", "доллара", "долларов", "цент", "цента", "центов") },
				{ "YUM", new SlavicCurrencyForms("динар", "динара", "динаров", "пара", "пара", "пара") },
				{ "ZAR", new SlavicCurrencyForms("ранд", "ранда", "рандов", "цент", "цента", "центов") }
			};
			return result.ToImmutableDictionary();
		}, true);
	}
}

#endregion
