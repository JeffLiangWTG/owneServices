using System;
using System.Diagnostics;

namespace CargoWise.Common
{
	public static class VisualStudioDetector
	{
		public static bool IsVisualStudio
		{
			get => isVisualStudio ?? (bool)(isVisualStudio = GetIsVisualStudio());

#if DEBUG
			set => isVisualStudio = value;
#endif
		}
		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static bool? isVisualStudio;

		static bool GetIsVisualStudio()
		{
			try
			{
				using (var process = Process.GetCurrentProcess())
				{
					return process.ProcessName == "devenv"; // static process name
				}
			}
			catch (System.ComponentModel.Win32Exception)
			{
				return false;
			}
			catch (InvalidOperationException)
			{
				return false;
			}
		}
	}
}
