using System;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.FaxRouter.EventLogging;
using Enterprise.FaxRouter.Processor;

namespace Enterprise.FaxRouter.Manager
{
	public partial class GatewayStatusUserControl : UserControl
	{
		public GatewayStatusUserControl()
		{
			InitializeComponent();
			SetGatewayStatusControls();
			SetGatewayInitialRunStatus();
		}

		public bool StartButtonControlEnabled
		{
			get { return StartButton.Enabled; }
			set { StartButton.Enabled = value; }
		}

		public bool GatewayTimerEnabled
		{
			get { return GatewayTimer.Enabled; }
			set { GatewayTimer.Enabled = value; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		void StartButton_Click(object sender, EventArgs e)
		{
			NextRunLabel.Show();
			try
			{
				if (!GatewayTimer.Enabled)
				{
					SetButtonState();
					StatusTextBox.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
					OnFaxProgress(" EDI Fax Gateway started");
					GatewayTimer.Enabled = true;
				}
			}
			catch (Exception aEmailFaxProcessorException) when (!aEmailFaxProcessorException.IsCriticalException())
			{
				OnFaxProgress(" ## ERROR ##" + Environment.NewLine + aEmailFaxProcessorException.ToString(), "ERROR");
				if (!FaxDataModule.ExceptionCausedByNetworkProblems(aEmailFaxProcessorException))
				{
					EventLog.AddErrorEntry("StartFaxProcessor", aEmailFaxProcessorException);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		void StopButton_Click(object sender, EventArgs e)
		{
			SetButtonState();
			GatewayTimer.Enabled = false;
			OnFaxProgress(" EDI Fax Gateway stopped");
			GatewayTimerLabel.Text = "";
			NextRunLabel.Hide();
			Application.DoEvents();
		}

		public void CheckGatewayNow(object sender, EventArgs e)
		{
			if (StartButton.Enabled || GatewayTimer.Enabled)
			{
				StartButton_Click(sender, e);
			}
		}

		void ClearButton_Click(object sender, EventArgs e)
		{
			StatusTextBox.Clear();
		}

		void SetButtonState()
		{
			if (StartButton.Enabled)
			{
				StartButton.Enabled = false;
				StopButton.Enabled = true;
			}
			else
			{
				StartButton.Enabled = true;
				StopButton.Enabled = false;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1060:DoNotUseDateTimeNow", Justification = "Baseline")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		void RunFaxProcessor()
		{
			GatewayTimer.Enabled = false;
			StopButton.Enabled = false;

			GatewayTimerLabel.Text = "--:--:--";

			try
			{
				EmailFaxProcessor aEmailFaxProcessor = new EmailFaxProcessor();
				aEmailFaxProcessor.OnFaxProgress = new EmailFaxProcessor.FaxProgressDelegate(OnFaxProgress);
				aEmailFaxProcessor.TiffFileDataTransform();
				aEmailFaxProcessor.HandleEmailToFax();

				if (ExpectDeliveryAckCheckBox.Checked)
				{
					aEmailFaxProcessor.HandleFaxAcknowledgements();
				}
			}
			catch (Exception aEmailFaxProcessorException) when (!aEmailFaxProcessorException.IsCriticalException())
			{
				OnFaxProgress(" ## ERROR ##" + Environment.NewLine + aEmailFaxProcessorException.ToString(), "ERROR");
				if (!FaxDataModule.ExceptionCausedByNetworkProblems(aEmailFaxProcessorException))
				{
					EventLog.AddErrorEntry("StartFaxProcessor", aEmailFaxProcessorException);
				}
			}
			StopButton.Enabled = true;

			GatewayTimer.Enabled = true;
			GatewayTimerRunAtDateTime = DateTime.Now.AddMilliseconds(Constants.FAX_GATEWAY_POLLING_INTERVAL);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		void OnFaxProgress(String aStatusMsg)
		{
			OnFaxProgress(aStatusMsg, "Event");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1060:DoNotUseDateTimeNow", Justification = "Baseline")]
		void OnFaxProgress(String aStatusMsg, String aStatusEvent)
		{
			if (!string.IsNullOrEmpty(aStatusMsg))
			{
				const int MaxLogLength = 40000;
				if (StatusTextBox.Text.Length > MaxLogLength)
				{
					StatusTextBox.Text = StatusTextBox.Text.Substring(0, MaxLogLength / 2);
				}
				String gatewayLog = DateTime.Now.ToString(Constants.LONGDATEFORMAT) + aStatusMsg;
				StatusTextBox.Text = Environment.NewLine + gatewayLog + StatusTextBox.Text;
				LogFile.AddLog(aStatusEvent, gatewayLog);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1060:DoNotUseDateTimeNow", Justification = "Baseline")]
		void GatewayTimer_Tick(object sender, EventArgs e)
		{
			TimeSpan nextGatewayCheck = GatewayTimerRunAtDateTime.Subtract(DateTime.Now);
			GatewayTimerLabel.Text = EmailFaxProcessor.FormatGatewayTimer(nextGatewayCheck);
			if (GatewayTimerRunAtDateTime <= DateTime.Now)
			{
				RunFaxProcessor();
			}
		}

		void SetGatewayStatusControls()
		{
			GatewayTimerLabel.Text = "";
			NextRunLabel.Hide();
			ExpectDeliveryAckCheckBox.Checked = Constants.IS_FAX_ACK_ON;
			GatewayTimer.Interval = 1000;
			StartButton.Enabled = true;
			StopButton.Enabled = false;
		}

		void SetGatewayInitialRunStatus()
		{
			if (Constants.AUTO_START)
			{
				StartButton_Click(this, System.EventArgs.Empty);
			}
		}
	}
}
