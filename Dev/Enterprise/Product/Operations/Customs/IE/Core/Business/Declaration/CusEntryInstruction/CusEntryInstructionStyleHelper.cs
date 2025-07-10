using CargoWise.Types;
using Enterprise.Customs.Common.IE;

namespace Enterprise.Customs.IE.Business.Declaration
{
	internal static class CusEntryInstructionStyleHelper
	{
		internal static ZString GetDefaultDeclarationType(ZString messageType)
		{
			var declarationType = ZString.Empty;
			switch (messageType)
			{
				case IEJobMessageTypeList.Codes.ReExport:
					declarationType = ReExportDeclarationTypeList.Codes.A3;
					break;
				case IEJobMessageTypeList.Codes.ExitSummary:
					declarationType = ExitSummaryDeclarationTypeList.Codes.A1;
					break;
				case IEJobMessageTypeList.Codes.Export:
					declarationType = ExportDeclarationTypeList.Codes.B1;
					break;
				case IEJobMessageTypeList.Codes.Import:
					declarationType = ImportDeclarationTypeList.Codes.H1;
					break;
			}

			return declarationType;
		}

		public static bool SecurityRequired(this CusEntryInstruction instruction)
			=> SecurityRequired(instruction.CEI_Style) && !instruction.IsCoJob;

		public static bool TransportChargesMoPRequired(this CusEntryInstruction instruction) =>
			instruction.IsExpWithValidStyle() && (instruction.IsExitSummary || instruction.SecurityRequired());

		static bool IsExpWithValidStyle(this CusEntryInstruction instruction) => instruction.IsExport && !instruction.CEI_Style.IsEmpty;

		static bool SecurityRequired(string ceiStyle) =>
				ceiStyle == ExportDeclarationTypeList.Codes.B1
				|| ceiStyle == ExportDeclarationTypeList.Codes.B2
				|| ceiStyle == ExportDeclarationTypeList.Codes.C1;
	}
}
