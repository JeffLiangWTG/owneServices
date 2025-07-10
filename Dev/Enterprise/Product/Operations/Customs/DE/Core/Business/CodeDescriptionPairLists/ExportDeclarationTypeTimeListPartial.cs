using CargoWise.Types;

namespace Enterprise.Customs.DE.Business
{
	public partial class ExportDeclarationTypeTimeList
	{
		public static ZBool Is10(ZString subType) => subType == Codes._10;

		public static ZBool Is11Or12(ZString subType) => subType == Codes._11 || subType == Codes._12;

		public static ZBool Is11Or12Or13(ZString subType) => Is11Or12(subType) || Is13(subType);

		public static ZBool Is13(ZString subType) => subType == Codes._13;

		public static ZBool IsMultipleDeclarationForExport(ZString subType) => subType == Codes._20;
	}
}
