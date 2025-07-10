namespace Enterprise.Customs.DE.Business.Declaration
{
	partial class ExportEntryTypeList
	{
		public static bool IsCancellationOrExitToExport(string code) => IsCancellation(code) || IsExitToExport(code);

		public static bool IsExportAmendmentOrSupplementaryExportDeclaration(string code) => IsExportAmendment(code) || IsSupplementaryExportDeclaration(code);

		public static bool IsCancellation(string code) => code == Codes.CancellationRequest;

		public static bool IsExitToExport(string code) => code == Codes.ExitToExport;

		public static bool IsExportDeclaration(string code) => code == Codes.ExportDeclaration;

		public static bool IsExportAmendment(string code) => code == Codes.ExportAmendment;

		public static bool IsSupplementaryExportDeclaration(string code) => code == Codes.SupplementaryExportDeclaration;
	}
}
