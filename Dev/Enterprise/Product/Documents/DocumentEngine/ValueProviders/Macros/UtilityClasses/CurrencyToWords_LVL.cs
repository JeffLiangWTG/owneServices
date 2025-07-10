using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities
{
	public class CurrencyToWords_LVL : SlavicCurrencyConverter
	{
		#region SuppressResourceStringsCheckRegion

		public CurrencyToWords_LVL()
			: base(new NumberToString_LVL(), "viens", "{0} {1}", "{0}", "{0} un {1} {2}")
		{
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
				{ "EUR", new SlavicCurrencyForms("euro", "euros", "euro", "euro-cents", "euro-centi", "euro-cents") }
			};
			return result.ToImmutableDictionary();
		}, true);

		protected override string Pluralize(long amount, RefCurrency currency, bool isSubUnit)
		{
			var result = base.Pluralize(amount, currency, isSubUnit);
			if (CurrenciesDictionary.ContainsKey(currency.RX_Code))
			{
				var slavicCurrencyGrammar = CurrenciesDictionary[currency.Code];
				if (isSubUnit)
				{
					if (!((NumberToString_LVL)base.numberToWords).IsSingular(amount, 1))
					{
						result = slavicCurrencyGrammar.SubUnitForm2;
					}
					else
					{
						result = slavicCurrencyGrammar.SubUnitForm1;
					}
				}
				else
				{
					result = slavicCurrencyGrammar.UnitForm1;
				}
			}
			return result;
		}

		#endregion
	}
}
