namespace Enterprise.StlAnalysis.Load
{
	using System;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows.Forms;

	public partial class LoadFromCsvControl : UserControl // Cannot use ZArchitecture because this is a standalone tool
	{
		public LoadFromCsvControl()
		{
			InitializeComponent();
			RegisterImporter(out importer);
		}
		CancellationTokenSource cts;
		Task importTask;
		readonly CsvImporter importer;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1090:Don't use System.Windows.Forms dialogs", Justification = "Cannot use ZArchitecture because this is a standalone tool")]
		readonly FolderBrowserDialog fbdImportFolderBrowserDialog = new FolderBrowserDialog(); // Cannot use ZArchitecture because this is a standalone tool

		void btnBrowseOutputFolderButton_Click(object sender, EventArgs e)
		{
			if (fbdImportFolderBrowserDialog.ShowDialog() == DialogResult.OK)
			{
				txbImportFolderTextBox.Text = fbdImportFolderBrowserDialog.SelectedPath;
			}
		}

		void btnStartImportButton_Click(object sender, EventArgs e)
		{
			if (!IsImportRunning)
			{
				DisableControlButtons();
				ClearOutputBoxes();
				StartImport();
			}
		}

		void btnStopImportButton_Click(object sender, EventArgs e)
		{
			if (IsImportRunning)
			{
				DisableControlButtons();
				AbortImport();
			}
		}

		public bool IsImportRunning
		{
			get { return !(importTask.IsCompleted || importTask.IsCanceled); }
		}

		public async void AbortImport()
		{
			try
			{
				cts.Cancel();
				await importTask;
			}
			catch (OperationCanceledException)
			{
				// ignored...
			}
			finally
			{
				cts.Dispose();
			}
		}

		void StartImport()
		{
			importFolder = txbImportFolderTextBox.Text;
			cts = new CancellationTokenSource();
			importTask = Task.Run(() => RunImport(cts.Token), cts.Token);
		}

		void RegisterImporter(out CsvImporter importer)
		{
			var logger = new EtlLogger();
			logger.SyncInvoke = this;
			logger.OnTaskStarted += new DwLoadEvent(OnTaskStarted);
			logger.OnSubtaskStarted += new DwLoadEvent(OnSubtaskStarted);
			logger.OnError += new DwLoadEvent(OnLoadError);
			importer = new CsvImporter(logger);
		}

		#region Process control (separate thread)

		string importFolder;

		void RunImport(CancellationToken token)
		{
			try
			{
				token.ThrowIfCancellationRequested();
				OnImportStart(token);
				importer.ImportTransactionFacts(importFolder, token);
				OnImportCompletion(token);
			}
			catch (OperationCanceledException)
			{
				OnImportCancelled();
			}
		}

		void OnImportStart(CancellationToken token)
		{
			this.BeginInvoke(new Action(() =>
			{
				AppendMessageToTextBox(txbOutputTextBox, "# Import started.\r\n");
				ToggleStartStopEtlButton();
			}));
			token.ThrowIfCancellationRequested();
		}

		void OnImportCompletion(CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			this.BeginInvoke(new Action(() =>
			{
				AppendMessageToTextBox(txbOutputTextBox, "\r\n## Import completed.\r\n");
				ToggleStartStopEtlButton();
			}));
		}

		void OnImportCancelled()
		{
			this.BeginInvoke(new Action(() =>
			{
				AppendMessageToTextBox(txbOutputTextBox, "\r\n** Import stopped.\r\n");
				ToggleStartStopEtlButton();
			}));
		}

		#endregion

		#region UI control

		void DisableControlButtons()
		{
			btnStartImportButton.Enabled = false;
			btnStopImportButton.Enabled = false;
		}

		void ToggleStartStopEtlButton()
		{
			btnStopImportButton.Enabled = IsImportRunning;
			btnStartImportButton.Enabled = !btnStopImportButton.Enabled;
		}

		void ClearOutputBoxes()
		{
			txbOutputTextBox.Text = "";
			txbErrorTextBox.Text = "";
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

		void AppendMessageToTextBox(TextBox txbControl, string message, int indentLevel = 0)
		{
			string timeStamp = DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss]"); // Standalone tool - Workstation time used here.

			foreach (string msgLine in message.Split('\n'))
			{
				txbControl.AppendText(String.Format("{0}{1}{2}\r\n", timeStamp, new String('\t', indentLevel + 1), msgLine.TrimEnd()));
			}

			txbControl.ScrollToCaret();
		}

		#endregion
	}
}
