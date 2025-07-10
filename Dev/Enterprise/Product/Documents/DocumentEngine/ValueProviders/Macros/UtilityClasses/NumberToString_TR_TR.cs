namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities
{
	public class NumberToString_TR_TR : IndoEuropeanNumberToString
	{
		#region SuppressResourceStringsCheckRegion

		public NumberToString_TR_TR()
		{
			primitiveNumbers = new string[] { "sıfır", "bir", "iki", "üç", "dört", "beş", "altı", "yedi", "sekiz", "dokuz" };
			tens = new string[] { "", "on", "yirmi", "otuz", "kırk", "elli", "altmış", "yetmiş", "seksen", "doksan" };
			tensPattern = "{0}{1}";
			oneHundredsPlacePattern = "yüz";
			manyHundredsPlacePattern = "{0}yüz";
			oneThosandPlacePattern = "bin";
			manyThosandPlacePattern = "{0}bin";
			oneMillionPlacePattern = "birmilyon";
			manyMillionPlacePattern = "{0}milyon";
			oneBillionPlacePattern = "birmilyar";
			manyBillionPlacePattern = "{0}milyar";
			hundredsPattern = thousandsPattern = millionsPattern = billionsPattern = "{0}{1}";
		}

		public override string DecimalSeperatorAsString
		{
			get { return "virgül"; }
		}

		#endregion
	}
}
