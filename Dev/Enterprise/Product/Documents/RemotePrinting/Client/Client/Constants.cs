using System.IO;

namespace Enterprise.RemotePrinting.Client
#if WixCustomAction
.CustomAction // Different namespace for file copy in Setup project (to prevent error 'same symbol name defined in multiple project').
#endif
{
#if DEBUG
	[WTG.StaticAnalysis.Annotation.CodeAlive("This class is used, e.g. in RegistryHelper, but is not picked by DeadCodeTest due to conditional symbol namespace part.")]
#endif
	public static class Constants
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "ApplicationName const string")]
		public const string ApplicationName = "CargoWise One WebPrint Client";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Const string ConfigurationName")]
		public const string ConfigurationName = "CargoWise One WebPrint Configuration";

		public static string WebPrintClientDataPath => Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData), RegistryManager.CargoWiseKeyName, "WebPrintClient");

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Const String LogsFolder")]
		public static string LogsFolder => Path.Combine(Constants.WebPrintClientDataPath, "Logs");

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "RegistryManager const strings")]
		public static class RegistryManager
		{
			public const string CargoWiseKeyName = "CargoWise edi";

			public const string CargoWiseWebPrintTopKeyName = "SOFTWARE\\" + CargoWiseKeyName;

			public const string WebPrintKeyName = "ediWebPrint";

			public const string DefaultWindowsServiceName = "ewpcsrv";

			public const string EdiKeyName = "Eagle Datamation International"; //Obsolete

			public const string WebPrintLogSettings = "LogSettings";

			public const string WebPrintAutoStartData = CargoWiseWebPrintTopKeyName + "\\WebPrintAutoStartData";

			public const string WebPrintRestartData = CargoWiseWebPrintTopKeyName + "\\WebPrintRestartData";

			public const string WebPrintConfigKeyName = "SOFTWARE\\" + CargoWiseKeyName + "\\" + WebPrintKeyName;

			public const string EdiConfigKeyName = "SOFTWARE\\" + EdiKeyName + "\\" + WebPrintKeyName;
		}
	}
}
