namespace Enterprise.Customs.EU.NCTS.Business
{
	// Please do not add any new codes here. Please add new codes to ValidationRuleMessages.
	public static class ValidationRuleCodeConstants
	{
		public const string B1820_1 = "B1820-1";

		public const string B1831 = "B1831";

		public const string B1848 = "B1848";

		public const string B1875_1 = "B1875-1";

		public const string B1897 = "B1897";

		public const string C0001_1 = "C0001-1";

		public const string C0030_1 = "C0030-1";

		public const string C0191 = "C0191";

		public const string C0215 = "C0215";

		public const string C0220 = "C0220";

		public const string C0343 = "C0343";

		public const string C0382 = "C0382";

		public const string C0394 = "C0394";

		public const string C0403 = "C0403";

		public const string C0542_1 = "C0542-1";

		public const string E1104_1 = "E1104-1";

		public const string G0026_1 = "G0026-1";

		public const string G0058_1 = "G0058-1";

		public const string G0090 = "G0090";

		public const string G0123_1 = "G0123-1";

		public const string NR0013 = "NR0013";

		public const string NR0021 = "NR0021";

		public const string NR0022 = "NR0022";

		public const string NR0023 = "NR0023";

		public const string NR0042 = "NR0042";

		public const string NR0044 = "NR0044";

		public const string R0850 = "R0850";

		public const string R0850_1 = "R0850-1";

		public const string R0900 = "R0900";

		public const string TR0016 = "TR0016";

		public const string TR0023 = "TR0023";

		public const string TR0024 = "TR0024";

		public const string TR0025 = "TR0025";

		public const string TR0026 = "TR0026";

		public const string TR0027 = "TR0027";

		public const string TR0028 = "TR0028";

		public const string TR0029 = "TR0029";

		public const string TR0030 = "TR0030";

		public const string TR0031 = "TR0031";

		public const string TR0033 = "TR0033";

		public const string TR0036 = "TR0036";

		public const string TR0037 = "TR0037";

		public const string TR0038 = "TR0038";

		public const string TR0043 = "TR0043";

		public const string TR0044 = "TR0044";

		public const string TR0045 = "TR0045";

		public const string TR0046 = "TR0046";

		public const string TR0064 = "TR0064";

		public static string GetRuleCodeMessagePrefix(this string ruleCode, bool addSpaceAtEnd = false) => $"[{ruleCode}]{(addSpaceAtEnd ? " " : "")}";
	}
}
