namespace Enterprise.Customs.DE.Business.Declaration
{
	partial class ExportExitTypeList
	{
		public static bool Is2(string code) => code == Codes._2;

		public static bool Is2_4(string code) => Is2(code) || Is4(code);

		public static bool Is4(string code) => code == Codes._4;
	}
}
