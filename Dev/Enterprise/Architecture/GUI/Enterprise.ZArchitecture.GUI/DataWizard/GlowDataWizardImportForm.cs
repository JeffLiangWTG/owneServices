using System;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Authentication.Primitives;
#if WINZOR
using System.Text;
using WinzorFramework.Extensions;
#endif

namespace Enterprise.ZArchitecture.GUI
{
	class LogType
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "log type")]
		public const string Info = "Info";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "log type")]
		public const string Error = "Error";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "log type")]
		public const string Warning = "Warning";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "log type")]
		public const string Verbose = "Verbose";
		public const string ImportFinished = "ImportFinished"; // log type
	}

	partial class GlowDataWizardImportForm : ZChildForm
	{
		readonly GlowLog Log;

		public GlowDataWizardImportForm(GlowLog inputLog)
		{
			InitializeComponent();
			ControlBox = false;
			this.FormBorderStyle = FormBorderStyle.FixedDialog;
			this.Log = inputLog;
			this.Load += OnLoad;
		}

		void OnLoad(object sender, EventArgs e)
		{
			if (Log != null)
			{
				try
				{
					FormatLog(Log);
				}
				catch (AuthorizationFailureException ex)
				{
					Log.AppendLog(LogType.Error, ex.Message, 100);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Html element")]
		void FormatLog(GlowLog log)
		{
			#if !WINZOR
			progressTextBox.SelectionStart = progressTextBox.TextLength;
			progressTextBox.SelectionLength = 0;

			foreach (var line in log.AsEnumerableWithoutVerbose().ToArray())
			{
				var type = line.Key;
				var message = line.Value;
				switch (type)
				{
					case LogType.Error:
						progressTextBox.SelectionColor = Color.Red;
						progressTextBox.SelectionFont = new Font(progressTextBox.Font, FontStyle.Bold);
						break;

					case LogType.Warning:
						progressTextBox.SelectionColor = Color.Orange;
						break;

					case LogType.ImportFinished:
						progressTextBox.SelectionColor = Color.Green;
						break;

					default:
						progressTextBox.SelectionColor = Color.Blue;
						break;
				}

				progressTextBox.AppendText(message);
				progressTextBox.AppendText(System.Environment.NewLine);
				progressTextBox.SelectionColor = progressTextBox.ForeColor;
			}
			#else
			var sb = new StringBuilder();
			foreach (var line in log.AsEnumerableWithoutVerbose().ToArray())
			{
				var type = line.Key;
				var message = line.Value;
				var color = Color.Empty;
				var font = progressTextBox.Font;
				switch (type)
				{
					case LogType.Error:
						color = Color.Red;
						font = new Font(progressTextBox.Font, FontStyle.Bold);
						break;

					case LogType.Warning:
						color = Color.Orange;
						break;

					case LogType.ImportFinished:
						color = Color.Green;
						break;

					default:
						color = Color.Blue;
						break;
				}

				sb.Append(progressTextBox.CreateHtmlNewLine(message, font, color));
			}
			sb.Append("<p><br></p>");

			progressTextBox.Html += sb.ToString();
			#endif
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		[DesignerSerializer(typeof(CargoWise.Windows.UI.Design.ControlDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))]
		internal class SerializableTextBox : RichTextBox
		{
		}
	}
}

