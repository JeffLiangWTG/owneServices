namespace Enterprise.StlAnalysis.Load
{
	using System;
	using System.Globalization;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using CargoWise.Common;

	public partial class EtlUserControl : UserControl // Cannot use ZArchitecture because this is a standalone tool
	{
		public EtlUserControl()
		{
			InitializeComponent();
			InitialiseLastLoadMonthControl();
			RegisterLoader(out dwLoader);
		}

		void btnCollectDataButton_Click(object sender, EventArgs e)
		{
			try
			{
				btnCollectDataButton.Enabled = false;

				if (IsEtlRunning)
				{
					AbortEtl();
				}
				else
				{
					ClearOutputBoxes();
					StartLoad();
				}
			}
			finally
			{
				btnCollectDataButton.Enabled = true;
			}
		}
		CancellationTokenSource cts;
		Task etlTask;
		readonly EtlController dwLoader;

		void InitialiseLastLoadMonthControl()
		{
			DateTime loadLoadMonthAsDate = MaxLastLoadMonthAsDate();
			this.lastLoadMonthTextBox.Text = loadLoadMonthAsDate.Year.ToString("0000", CultureInfo.InvariantCulture) + loadLoadMonthAsDate.Month.ToString("00", CultureInfo.InvariantCulture);
		}

		void RegisterLoader(out EtlController dwLoader)
		{
			var logger = new EtlLogger();
			logger.SyncInvoke = this;
			logger.OnTaskStarted += new DwLoadEvent(OnTaskStarted);
			logger.OnSubtaskStarted += new DwLoadEvent(OnSubtaskStarted);
			logger.OnError += new DwLoadEvent(OnLoadError);
			dwLoader = new EtlController(logger);
		}

		void StartLoad()
		{
			DateTime loadEndDateExclusive = DateTime.MinValue;

			try
			{
				loadEndDateExclusive = GetLoadEndDateExclusive();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				AppendMessageToTextBox(txbErrorTextBox, "Last Load Month must be in the format YYYYMM\r\n" + ex.Message);
				return;
			}
			cts = new CancellationTokenSource();
			etlTask = Task.Run(() => RunEtlSpecificClient(this.serverTextBox.Text, this.databaseTextBox.Text, loadEndDateExclusive, cts.Token),cts.Token);
		}

		DateTime GetLoadEndDateExclusive()
		{
			DateTime loadLoadMonthAsDate = MaxLastLoadMonthAsDate();

			string lastLoadMonthRaw = this.lastLoadMonthTextBox.Text.Trim();

			if (lastLoadMonthRaw.Length > 0)
			{
				DateTime candidateLoadLoadMonthAsDate = new DateTime(
					Convert.ToInt32(lastLoadMonthRaw.Substring(0, 4), CultureInfo.InvariantCulture),
					Convert.ToInt32(lastLoadMonthRaw.Substring(4, 2), CultureInfo.InvariantCulture),
					1);

				if (candidateLoadLoadMonthAsDate < loadLoadMonthAsDate)
				{
					loadLoadMonthAsDate = candidateLoadLoadMonthAsDate;
				}
			}

			return loadLoadMonthAsDate.AddMonths(1);
		}

		DateTime MaxLastLoadMonthAsDate()
		{
			DateTime lastMonth = DateTime.UtcNow.Date.AddDays(-1).AddMonths(-1);
			return new DateTime(lastMonth.Year, lastMonth.Month, 1);
		}

		void RunEtlSpecificClient(string serverName, string databaseName, DateTime loadEndDateExclusive, CancellationToken token)
		{
			try
			{
				token.ThrowIfCancellationRequested();
				OnEtlStart(token);
				dwLoader.LoadClientSafe(serverName, databaseName, loadEndDateExclusive,token);
				OnEtlCompletion(token);
			}
			catch (OperationCanceledException)
			{
				OnEtlCancelled();
				throw;
			}
		}

		void OnEtlStart(CancellationToken token)
		{
			this.BeginInvoke(new Action(() =>
			{
				AppendMessageToOutputTextBox("ETL process started.\r\n");
				ToggleStartStopEtlButton();
			}));
			token.ThrowIfCancellationRequested();
		}

		void OnEtlCompletion(CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			this.BeginInvoke(new Action(() =>
			{
				AppendMessageToOutputTextBox("ETL process completed.\r\n");
				ToggleStartStopEtlButton();
			}));
		}

		void OnEtlCancelled()
		{
			this.BeginInvoke(new Action(() =>
			{
				AppendMessageToOutputTextBox("ETL process stopped.\r\n");
				ToggleStartStopEtlButton();
			}));
		}

		public bool IsEtlRunning
		{
			get { return !(etlTask.IsCompleted || etlTask.IsCanceled); }
		}

		public async void AbortEtl()
		{
			try
			{
				cts.Cancel();
				await etlTask;
			}
			catch (OperationCanceledException)
			{
				// Ignore
			}
			finally
			{
				cts.Dispose();
			}
		}

		void ToggleStartStopEtlButton()
		{
			if (IsEtlRunning)
			{
				btnCollectDataButton.Text = "Stop";
				btnCollectDataButton.Image = global::Enterprise.StlAnalysis.Load.Properties.Resources.Stop_24p;
			}
			else
			{
				btnCollectDataButton.Text = "Start Load";
				btnCollectDataButton.Image = global::Enterprise.StlAnalysis.Load.Properties.Resources.Start_24p;
			}
		}

		void OnTaskStarted(string task)
		{
			AppendMessageToTextBox(txbOutputTextBox, task);
		}

		void OnSubtaskStarted(string subtask)
		{
			AppendMessageToTextBox(txbOutputTextBox, subtask, 1);
		}

		void OnLoadError(string message)
		{
			AppendMessageToTextBox(txbOutputTextBox, "ERROR: " + message);
			AppendMessageToTextBox(txbErrorTextBox, message);
		}

		void ClearOutputBoxes()
		{
			txbOutputTextBox.Text = "";
			txbErrorTextBox.Text = "";
		}

		void AppendMessageToOutputTextBox(string message)
		{
			AppendMessageToTextBox(txbOutputTextBox, message);
		}

		void AppendMessageToTextBox(TextBox txbControl, string message, int indentLevel = 0)
		{
			string timeStamp = DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss]"); // Standalone tool - Workstation time used here.

			foreach (string msgLine in message.Split('\n'))
			{
				txbControl.AppendText(String.Format("{0}{1}{2}\r\n", timeStamp, new String('\t', indentLevel + 1), msgLine.TrimEnd()));
			}

			txbControl.ScrollToCaret();
		}
	}
}
