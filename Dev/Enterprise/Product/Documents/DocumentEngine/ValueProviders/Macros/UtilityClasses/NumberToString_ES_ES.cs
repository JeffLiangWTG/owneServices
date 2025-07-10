using System.Collections.Generic;

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities
{
	public class NumberToString_ES_ES : IndoEuropeanNumberToString
	{
		#region SuppressResourceStringsCheckRegion

		public NumberToString_ES_ES()
		{
			primitiveNumbers = new string[] {
				"cero", "uno", "dos", "tres", "cuatro", "cinco", "seis", "siete", "ocho", "nueve",
				"diez", "once", "doce", "trece", "catorce", "quince", "dieciséis", "diecisiete", "dieciocho", "diecinueve",
				"veinte", "veintiuno", "veintidós", "veintitrés", "veinticuatro", "veinticinco", "veintiséis", "veintisiete", "veintiocho", "veintinueve" };

			tens = new string[] { "", "", "", "treinta", "cuarenta", "cincuenta", "sesenta", "setenta", "ochenta", "noventa" };
			hundreds = new string[] { "", "ciento", "doscientos", "trescientos", "cuatrocientos", "quinientos", "seiscientos", "setecientos", "ochocientos", "novecientos" };

			specialNumbers = new Dictionary<long, string>();
			specialNumbers.Add(100, "cien");

			tensPattern = "{0} y {1}";
			hundredsPattern = "{0} {1}";
			oneThosandPlacePattern = "mil";
			manyThosandPlacePattern = "{0} mil";
			oneMillionPlacePattern = "un millón";
			manyMillionPlacePattern = "{0} millones";
			oneBillionPlacePattern = "mil millones";
			manyBillionPlacePattern = "{0} mil millones";
			thousandsPattern = millionsPattern = billionsPattern = "{0} {1}";
		}

		public override string DecimalSeperatorAsString { get { return "coma"; } }

		#endregion
	}
}
