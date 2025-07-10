#region SuppressResourceStringsCheckRegion

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities
{
	public class NumberToString_ZH_TW : ChineseNumberToWords
	{
		public NumberToString_ZH_TW()
		{
			PrimitiveNumbers = new string[] { "零", "壹", "貳", "叄", "肆", "伍", "陸", "柒", "捌", "玖" };
			Tens = new string[] { "拾", "佰", "仟", "萬", "億", "仟億" };
			Sign = "負";
		}
	}
}

#endregion
