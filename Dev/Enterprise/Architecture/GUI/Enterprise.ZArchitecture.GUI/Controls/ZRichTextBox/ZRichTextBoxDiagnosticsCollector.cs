using System;
using CargoWise.Common;
using CargoWise.Interop.DataObjects;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.GUI.RichEdit
{
	public class ZRichTextBoxDiagnosticsCollector
	{
		internal ZRichTextBoxDiagnosticsCollector()
		{
		}

		public static ZRichTextBoxDiagnosticsCollector Instance
		{
			get { return instance ?? (instance = new ZRichTextBoxDiagnosticsCollector()); }
		}
		[ThreadStatic]
		static ZRichTextBoxDiagnosticsCollector instance;

		public void NotifyObjectInserted(ZDataObject data)
		{
			var message = Res.GetString("7a962d48-c181-4431-87a3-777999c3994a", "{0} inserted with size {1}", data.GetType().Name, data.Size);
			if (data.SingleFileDrop != null)
			{
				message += " " + Res.GetString("783f4757-b378-4da1-884b-1506c3f84b34", "with filename {0}", data.SingleFileDrop);
			}
			var formats = "";
			foreach (var format in data.GetFormats())
			{
				if (!string.IsNullOrEmpty(formats))
				{
					formats += ",";
				}

				formats += format;
			}
			message += " " + Res.GetString("e67be325-7502-4280-952e-94b6c0117f49", "with supported formats ({0})", formats);
			AddDiagnostic(message);
		}

		public void ReportDeveloperErrorWithDiagnostics(string key, string message)
		{
			var processes = "";
			foreach (var p in ProcessLocator.Instance.GetCurrentUserVisibleProcesses())
			{
				processes += p.ProcessName + "\n";
			}
			ErrorReporter.ReportOnce(key, message + "\n\n" + processes + "\n\n" + AsString);
		}

		public string AsString
		{
			get { return fActions; }
		}

		#region Implementation

		void AddDiagnostic(string message)
		{
			fActions += ZDateTime.Now.ToString("hh:mm:ss") + ": " + message + "\r\n";
		}

		string fActions = "";

		#endregion
	}
}
