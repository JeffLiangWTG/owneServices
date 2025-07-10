using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Enterprise.DocumentScanning.Launch
{
	public class DiagnosticsManager
	{
		public DiagnosticsManager()
		{
			fSB = new StringBuilder();
		}

		bool fDebugMode;

		public bool DebugMode
		{
			get { return fDebugMode; }
		}

		readonly StringBuilder fSB;

		public void Initialize()
		{
			fDebugMode = DebugFileExists();
			fSB.Length = 0;
		}

		public void AppendMessageLine(string aLine)
		{
			fSB.Append(aLine);
			fSB.Append("\r\n");
		}

		public void AppendMessage(string aMessage)
		{
			fSB.Append(aMessage);
		}

		public void AppendCRLF()
		{
			fSB.Append("\r\n");
		}

		protected bool DebugFileExists()
		{
			string fName = Application.StartupPath;

			if (!fName.EndsWith("\\"))
			{
				fName += "\\";
			}

			fName += "DMDiag.txt";

			return File.Exists(fName);
		}

		public string TheMessage
		{
			get { return fSB.ToString(); }
		}
	}
}
