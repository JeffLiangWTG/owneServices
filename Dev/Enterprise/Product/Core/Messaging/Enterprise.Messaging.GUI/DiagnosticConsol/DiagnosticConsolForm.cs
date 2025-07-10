using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Messaging.GUI
{
	public partial class DiagnosticConsolForm : ZChildForm
	{
		public DiagnosticConsolForm(DiagnosticConsol businessEntity)
			: base(businessEntity)
		{
		}

		public new DiagnosticConsol BusinessEntity
		{
			get { return base.BusinessEntity as DiagnosticConsol; }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			AddMessageToListBox(BusinessEntity.InitialMessage);
			CopyToClipboardButton.Text = Res.GetString("8d2e6a2c-58b3-44e3-9cf7-b97cb2aebe9f", "Copy to Clipboard");
			StartButton.Text = Res.GetString("e8ffbace-acd7-44be-96ef-d221e92428df", "Start");
			CloseButton.Text = Res.GetString("f9b1e959-eb50-42bb-a51e-8d9dbaed83f6", "Close");
			AbortButton.Text = Res.GetString("5fc07df9-542f-4c2a-83b1-cdd676fb7d9b", "Cancel");
		}

		void AbortButton_Click(object sender, EventArgs e)
		{
			this.DiagTimer.Enabled = false;
			if (BusinessEntity.Mutex1 != null && BusinessEntity.Mutex1.IsLocked)
			{
				AddMessageToListBox(Res.GetString("707a4e26-e98e-4eb0-9bd6-5fd4e13d4411", "User canceled"));
				AddMessageToListBox(BusinessEntity.GetExtendedStatusDescriptionForCurrentStatus());
				AbortButton.Visible = false;
				CloseButton.Visible = true;
				Globals.Message.ShowWarning(Res.GetString("E9C733EE-B992-4635-8240-8AC0AEE82F82", "You have canceled the test.\r\n{0}", BusinessEntity.GetExtendedStatusDescriptionForCurrentStatus()));
				BusinessEntity.ReleaseDiagnosticLock();
			}
			else
			{
				BusinessEntity.ReleaseDiagnosticLock();
				this.Close();
			}
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			this.DiagTimer.Enabled = false;
			BusinessEntity.ReleaseDiagnosticLock();
			this.Close();
		}

		void StartButton_Click(object sender, EventArgs e)
		{
			if (DoRegistryAndCertificateChecks())
			{
				foreach (ZString logText in BusinessEntity.AllocateKeyAndDoInitialChecks())
				{
					AddMessageToListBox(logText);
				}
				StartButton.Visible = false;
				this.DiagTimer.Enabled = true;
			}
		}

		bool DoRegistryAndCertificateChecks()
		{
			var (logTexts, allHealthyOrWarning) = BusinessEntity.ProcessRegistryAndCertificateCheckResults();
			foreach (var logText in logTexts)
			{
				AddMessageToListBox(logText);
			}

			return allHealthyOrWarning;
		}

		void CopyToClipboardButton_Click(object sender, EventArgs e)
		{
			StringBuilder builder = new StringBuilder();
			foreach (string item in DiagnosticMessagesListBox.Items)
			{
				builder.Append(item);
				builder.Append(System.Environment.NewLine);
			}
			SafeClipboard.SetText(builder.ToString());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		void AddMessageToListBox(string logText)
		{
			string[] logTexts = logText.Replace("\r", "").Replace("\t", "   ").Split('\n');
			foreach (string logTextElement in logTexts)
			{
				DiagnosticMessagesListBox.Items.Add(ZDateTime.Now.ToShortTimeString() + "   " + logTextElement);
			}
			DiagnosticMessagesListBox.SelectedIndex = DiagnosticMessagesListBox.Items.Count - 1;
			DiagnosticMessagesListBox.SelectedIndex = -1;
			Application.DoEvents();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "used for user feedback")]
		public const string waitingTxt = "Cycle in progress...";

		void AddWaitingToListBox()
		{
			var listBoxIndex = DiagnosticMessagesListBox.Items.Count - 1;
			var lastMessageWaiting = (string)(DiagnosticMessagesListBox.Items[listBoxIndex].ToString().Contains(waitingTxt) ? DiagnosticMessagesListBox.Items[listBoxIndex] : null);

			var count = 1;
			if (lastMessageWaiting != null)
			{
				DiagnosticMessagesListBox.Items.RemoveAt(listBoxIndex);

				int afterOpenBracket = lastMessageWaiting.IndexOf('(') + 1;
				int beforeCloseBracket = lastMessageWaiting.IndexOf(')');
				count = int.Parse(lastMessageWaiting.Substring(afterOpenBracket, beforeCloseBracket - afterOpenBracket), CultureInfo.InvariantCulture) + 1;
			}

			if (count == new ZInt((60 * 20) / (DiagTimer.Interval / 1000))) // When count == (1200 (20 minutes in seconds) / the diagnostic interval speed in seconds),
			{
				DiagnosticMessagesListBox.Items.Add("");
				AddMessageToListBox(Res.GetString("A60CD471-180F-4FD3-89F9-9AB6BCFD3BFF", "The diagnostic tool is unlikely to progress, however here is the description of the current status."));
				AddMessageToListBox(BusinessEntity.GetExtendedStatusDescriptionForCurrentStatus());
				DiagnosticMessagesListBox.Items.Add("");
			}

			AddMessageToListBox(waitingTxt + " (" + count + ")");
		}

		public static ZString IgnoreCycleInProgressText(ZString unfilteredResult)
		{
			ZString[] filteredResult = unfilteredResult.Split("\n".ToCharArray()).Where(line => !line.Contains(waitingTxt, StringComparison.Ordinal)).ToArray();
			return ZString.Join("\n", filteredResult);
		}

		void DiagTimer_Tick(object sender, EventArgs e)
		{
			CheckForActions();
		}

		public void CheckForActions()
		{
			ZString actionCode;
			AddWaitingToListBox();
			var logText = BusinessEntity.ProcessTimerTick(out actionCode);
			if (!logText.IsEmpty)
			{
				AddMessageToListBox(logText);
			}

			if (actionCode == DiagnosticConsolActions.Codes.ErrorOccurredInProcessing)
			{
				DiagTimer.Enabled = false;
				CloseButton.Visible = true;
				AbortButton.Visible = false;
				Globals.Message.ShowError(Res.GetString("38c22070-e9f9-4d31-a472-39004bdaa669", "A system error has occurred tracking the message: {0}", logText));
			}
			else if (actionCode == DiagnosticConsolActions.Codes.Failed)
			{
				DiagTimer.Enabled = false;
				CloseButton.Visible = true;
				AbortButton.Visible = false;
				Globals.Message.ShowWarning(Res.GetString("fdff5645-a72e-4fab-a746-3e9429cc3bea", "The test message sending has failed: {0}\r\n{1}", logText, BusinessEntity.GetExtendedStatusDescriptionForCurrentStatus()));
				AddMessageToListBox(BusinessEntity.GetExtendedStatusDescriptionForCurrentStatus());
			}
			else if (actionCode == DiagnosticConsolActions.Codes.Success)
			{
				DiagTimer.Enabled = false;
				CloseButton.Visible = true;
				AbortButton.Visible = false;
				Globals.Message.ShowInformation(Res.GetString("4c47cc8d-9566-436a-817f-559f56464a45", "The test message sending has succeeded."));
				AddMessageToListBox(Res.GetString("1e860802-ba6d-4a2d-99cf-e4b5a64a0d2c", "Success"));
			}
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing)
			{
				DiagTimer.Dispose();
				if (components != null)
				{
					components.Dispose();
				}
			}
		}
	}
}
