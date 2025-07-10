using System;
using System.Windows.Forms;

namespace Enterprise.RemotePrinting.Client.Desktop
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Digest")]
		static void Main()
		{
			using (AuthenticationModulePrioritiser.MoveToFirstPositionTemporarily("Digest"))
			{
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.Run(new PrintClientForm());
			}
		}
	}
}
