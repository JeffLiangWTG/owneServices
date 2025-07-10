namespace Enterprise.Customs.KR.Messaging
{
	public partial class CargoSelectivityResultTypeCodeList
	{
		public static bool ToBeInspected(string code)
		{
			return code == Codes.F || code == Codes.Y;
		}

		public static string[] CodesInOrderOfImportance()
		{
			return new string[]
			{
				Codes.F,
				Codes.Y,
				Codes.S
			};
		}
	}
}
