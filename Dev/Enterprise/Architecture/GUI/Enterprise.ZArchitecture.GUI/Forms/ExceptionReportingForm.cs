using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class ExceptionReportingForm : KForm
	{
		#region Error Sent Event

		public delegate void ErrorReportSendHandler(ExceptionReportArgs e);
		public event ErrorReportSendHandler ErrorReportSend;

		#endregion

		public ExceptionReportingForm(string errorReportId, bool isFullMode)
		{
			InitializeComponent();
			try
			{
				ProductLogoPictureBox.Image = BrandingFactory.Instance.ProductLogo;
			}
			catch
			{
				ProductLogoPictureBox.Image = null;
			}
			Icon = BrandingFactory.Instance.ProductIcon;
			Text = BrandingFactory.Instance.ProductName;
			ErrorIDLabel.Text = Res.GetString("d591aa69-aa0f-4437-9aa6-e5d50504c247", "Error ID: {0}", errorReportId);
			InitialFormHeight = Height;
			ErrorReportId = errorReportId;
			InitialiseSettings(isFullMode);
		}

		public void SetException(Exception ex, string key, string message)
		{
			ExceptionToReport = ex;
			ExceptionKey = key;
			ExceptionDetailsToReport = new ExceptionDetails(ex);
			ErrorDescriptionTextBox.Text = message;
			string fullReport = ExceptionDetailsToReport.GetFullReport();
			string finalReport;

			try
			{
				finalReport = XDocument.Parse(fullReport).ToString();
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				if (e is XmlException)
				{
					XmlException xmle = (XmlException)e;
					finalReport = Res.GetString("6B2C5891-8632-4A26-9404-73536FC2ACBF",
						"Some thing is wrong when parsing the original error report at line, position: ({0}, {1}) of the original error report.", // for developers' eyes only
						xmle.LineNumber, xmle.LinePosition) + "\r\n";
				}
				else
				{
					finalReport = Res.GetString("D036FBC5-1620-4F98-9D18-D0F69FDA014B", "Some thing is wrong when parsing the original error report.") + "\r\n"; // for developers' eyes only
				}

				finalReport += e.Message;
				finalReport += "\r\n" + Res.GetString("C453EA23 -34D8-48C0-B4AB-5B168D9E6742", "The following is the original error report:") + "\r\n\r\n"; // for developers' eyes only
				finalReport += fullReport;
			}

			FullDetailsTextBox.Text = Res.GetString("4BAF3415-1E90-4C2F-BB56-70B671387A0F", "<!-- Press CTRL + ALT + I to view exception details in a browser window. -->\r\n{0}", finalReport); // for developers' eyes only
			FullDetailsTextBox.Select(0, 0);
		}

		public bool IsShutDownRequested
		{
			get { return ShutdownEnterpriseCheckEdit.Checked; }
		}

		public override string Text
		{
			get { return Res.GetString("648feff5-cfd1-4575-86ab-85934858a4b5", "Exception Reporting Form"); }
			set { }
		}

		#region Implementation

		readonly int InitialFormHeight;
		readonly string ErrorReportId;
		bool fFullDetailsView;
#if DEBUG
		internal
#endif
		Exception ExceptionToReport;
		ExceptionDetails ExceptionDetailsToReport;
#if DEBUG
		internal
#endif
		string ExceptionKey;

		void InitialiseSettings(bool isFullMode)
		{
			if (isFullMode)
			{
				if (ShouldShowFullDetails)
				{
					FullDetailsView = true;
					ToggleFullDetailsButton.Visible = true;
					BreakButton.Visible = true;
					WindowState = FormWindowState.Maximized;
				}
			}
			else
			{
				ToggleFullDetailsButton.Visible = true;
				SendButton.Visible = false;
				ShutdownEnterpriseCheckEdit.Visible = false;
				ErrorDescriptionTextBox.Visible = false;
				LostInformationLabel.Visible = false;
			}
		}

		bool ShouldShowFullDetails
		{
			get
			{
				bool result = Globals.IsDebugMode;

				if (!result)
				{
					IUser currentUser = EnvProxy.Instance.CurrentUser;

					if (currentUser != null)
					{
						result = currentUser.IsDeveloperLogin;
					}
				}

				return result;
			}
		}

		bool FullDetailsView
		{
			set
			{
				fFullDetailsView = value;
				var unscaledFormHeight = ControlDpiScalingHelper.UnscaleFromCurrentDpiY(InitialFormHeight);
				ControlDpiScalingHelper.SetHeight(this, fFullDetailsView ? unscaledFormHeight + 274 : unscaledFormHeight - 2, true);
				FullDetailsTextBox.Visible = fFullDetailsView;

				if (fFullDetailsView)
				{
					ToggleFullDetailsButton.Text = Res.GetString("f3d46ca6-b9af-4cd9-9817-a9d1941b313f", "Hide Error Details");
				}
				else
				{
					ToggleFullDetailsButton.Text = Res.GetString("d854cfd3-832d-4341-9b76-7f7b7ebfe5e0", "Show Error Details");
				}
			}
		}

		void OnErrorReportSend()
		{
			if (ErrorReportSend != null)
			{
				ExceptionReportArgs args = new ExceptionReportArgs(ExceptionToReport, ErrorReportId, ExceptionKey, ErrorDescriptionTextBox.Text);
				args.ShutDownApplication = ShutdownEnterpriseCheckEdit.Checked;
				ErrorReportSend(args);
			}
		}

		void ToggleFullDetailsButton_Click(object sender, EventArgs e)
		{
			FullDetailsView = !fFullDetailsView;
		}
#if DEBUG
		internal
#endif
		void SendButton_Click(object sender, EventArgs e)
		{
			if (!string.IsNullOrWhiteSpace(ErrorDescriptionTextBox.Text))
			{
				OnErrorReportSend();
				SendButton.Enabled = false;
				this.DialogResult = DialogResult.OK;
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("2758e343-4ce3-4f8f-bc46-16f4eab08909", "Please help us to improve our software by providing a description of the steps you took prior to the error occurring."));
				ErrorDescriptionTextBox.Focus();
			}
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			e.Handled = ProcessKeyDown(e.KeyData);
			base.OnKeyDown(e);
		}

		internal bool ProcessKeyDown(Keys keyData)
		{
			if (keyData == (Keys.Control | Keys.Alt | Keys.S))
			{
				ExceptionReport report = new ExceptionReport(FullDetailsTextBox.Text);
				report.CreateTextFile();
				OpenFile(report.TextFileName);
				return true;
			}

			if (keyData == (Keys.Control | Keys.Alt | Keys.I))
			{
				ExceptionReport report = new ExceptionReport(FullDetailsTextBox.Text);
				report.CreateHtmlFile();
				OpenFile(report.HtmlFileName);
				return true;
			}

			return false;
		}

		void OpenFile(string fileName)
		{
			FileOpener.Open(fileName);
		}

		#endregion

		#region Debugger Break

		void BreakButton_Click(object sender, EventArgs e)
		{
			Debugger.Break();
		}

		#endregion
	}
}
