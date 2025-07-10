using System.Collections;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class WoodLevyCharacterRates
	{
		public WoodLevyCharacterRates()
		{
			rates = new Hashtable();
			rates.Add("23", .58m);
			rates.Add("17", .58m);
			rates.Add("25", .72m);
			rates.Add("19", .72m);
			rates.Add("22", .30m);
			rates.Add("24", .72m);
			rates.Add("18", .72m);
			rates.Add("27", .15m);
			rates.Add("26", .17m);
			rates.Add("20", .37m);
			rates.Add("21", .0003m);
		}

		public bool HasCharacter(string characterCode)
		{
			return rates.ContainsKey(characterCode);
		}

		public decimal GetRateWithCharacterCode(string characterCode)
		{
			if (rates.ContainsKey(characterCode))
			{
				return (decimal)rates[characterCode];
			}
			return 0m;
		}

		readonly Hashtable rates;
	}
}
