using System;
using System.ComponentModel;

namespace Enterprise.RemotePrinting.Engine
{
	public static class ErrorHandler
	{
		public static class RPC
		{
			public static bool IsRPCServerUnavailableException(Exception ex)
			{
				return ex is Win32Exception && (((Win32Exception)ex).NativeErrorCode & 0xFFFF) == RPC_S_SERVER_UNAVAILABLE;
			}

			public const int RPC_S_SERVER_UNAVAILABLE = 1722;

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
			public static string GetRPCServerUnavailableMessage(string printQueue)
			{
				return string.Format("CargoWise One was not able to print to {0} on server {1}." + Environment.NewLine + Environment.NewLine +
				"Please check the printers in 'Control Panel' > 'Printers and Faxes' for any printers with errors, as they can affect printing reliability. " +
				Environment.NewLine + Environment.NewLine +
				"If you see this warning multiple times, or are having trouble printing in general, please get your IT provider to check " +
				"the Windows Event Viewer for possible causes.",
				printQueue,
				Environment.MachineName);
			}
		}
	}
}
