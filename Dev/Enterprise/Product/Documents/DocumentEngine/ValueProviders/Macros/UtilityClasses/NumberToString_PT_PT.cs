
using System.Collections.Generic;

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities
{
	public class NumberToString_PT_PT : IndoEuropeanNumberToString
	{
		#region SuppressResourceStringsCheckRegion

		public NumberToString_PT_PT()
		{
			primitiveNumbers = new string[] {
				"zero", "um", "dois", "três", "quatro", "cinco", "seis", "sete", "oito", "nove",
				"dez", "onze", "doze", "treze", "quatorze", "quinze", "dezasseis", "dezessete", "dezoito", "dezenove" };

			tens = new string[] { "", "", "vinte", "trinta", "quarenta", "cinquenta", "sessenta", "setenta", "oitenta", "noventa" };
			hundreds = new string[] { "", "cento", "duzentos", "trezentos", "quatrocentos", "quinhentos", "seiscentos", "setecentos", "oitocentos", "novecentos" };

			tensPattern = "{0} e {1}";
			hundredsPattern = "{0} e {1}";
			oneThosandPlacePattern = "mil";
			manyThosandPlacePattern = "{0} mil";
			oneMillionPlacePattern = "um milhão";
			manyMillionPlacePattern = "{0} milhões";
			oneBillionPlacePattern = "um bilhão";
			manyBillionPlacePattern = "{0} bilhões";
			thousandsPattern = millionsPattern = billionsPattern = "{0} e {1}";

			specialNumbers = new Dictionary<long, string>();
			specialNumbers.Add(100, "cem");
		}

		public override string DecimalSeperatorAsString { get { return "coma"; } }

		#endregion
	}
}
