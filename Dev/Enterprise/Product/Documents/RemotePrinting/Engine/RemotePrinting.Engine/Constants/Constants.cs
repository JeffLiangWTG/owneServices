using System.IO;

namespace Enterprise.RemotePrinting.Engine
{
	public abstract class Constants
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public const string FlexCelScalingSheetName = "#FlexCelScale";

		public const string PCL = "PCL";
		public const string PDF = "PDF";
		public const string TIF = "TIF";
		public const string XLS = "XLS";
		public const string ZPL = "ZPL";
		public const string XLSX = "XLSX";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI011:TempPathRule", Justification = "Outside of Enterprise")]
		public static string ErrorDir
		{
			get
			{
				return Path.Combine(Path.GetTempPath(), "ErrorFiles");
			}
		}
	}
}
