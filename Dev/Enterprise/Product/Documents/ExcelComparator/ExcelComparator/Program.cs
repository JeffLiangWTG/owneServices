using System;
using System.Windows.Forms;

namespace Enterprise.ExcelComparator
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			string filePath1 = args.Length > 0 ? args[0] : string.Empty;
			string filePath2 = args.Length > 1 ? args[1] : string.Empty;
			bool autoCompare = args.Length > 2 && args[2].ToUpperInvariant() == "AUTOCOMPARE";

			using (var mainForm = new MainForm(filePath1, filePath2))
			{
				if (autoCompare)
				{
					mainForm.Compare();
				}
				else
				{
					Application.Run(mainForm);
				}
			}
		}
	}
}
