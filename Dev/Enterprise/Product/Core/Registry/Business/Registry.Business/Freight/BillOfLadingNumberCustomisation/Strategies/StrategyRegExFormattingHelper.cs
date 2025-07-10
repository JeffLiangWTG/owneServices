namespace Enterprise.Registry.Business.BillCustomisationStrategies
{
	public static class StrategyRegExFormattingHelper
	{
		/*
		 * Output example: [0-9a-zA-Z]{,4}
		 * Check the text is combined from "digit or english char" and length under limitation.
		 * 
		 * [0-9a-zA-Z]{,4}
		 * "9999" => true
		 * "1234" => true
		 * "99999" => false
		 * "a999" => true
		 * "A999" => true
		 * "999" => true
		 * "9" => true
		 * "愛999" => false
		 * "@999" => false
		 */
		public static string GetMaxToLengthRegExValue(ICalcMaxGeneratedLength e)
		{
			return $"[0-9a-zA-Z]{{,{e.CalcMaxGeneratedLength}}}";
		}

		/*
		 * Output example: [0-9a-zA-Z]{4}
		 * Check the text is combined from "digit or english char" and length is correct.
		 * 
		 * [0-9a-zA-Z]{4}
		 * "9999" => true
		 * "1234" => true
		 * "99999" => false
		 * "a999" => true
		 * "A999" => true
		 * "999" => false
		 * "9" => false
		 * "愛999" => false
		 * "@999" => false
		 */
		public static string GetExactlyLengthRegExValue(ICalcMaxGeneratedLength e)
		{
			return $"[0-9a-zA-Z]{{{e.CalcMaxGeneratedLength}}}";
		}
	}
}
