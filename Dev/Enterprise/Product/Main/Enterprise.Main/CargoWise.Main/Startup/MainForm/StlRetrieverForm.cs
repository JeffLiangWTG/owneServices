using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Integration.Billing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Billing.StlCollector.Retriever
{
	public partial class StlRetrieverForm : ZChildForm
	{
		public StlRetrieverForm()
			: base(null)
		{
			InitializeComponent();
			RegisterEvents();
			utcMinusOneMonth = EnvProxy.Instance.Time.CurrentUtcDateTime.AddMonths(-1);
			#if DEBUG
			TypeDescriptor.AddAttributes(outputTextBox, new SuppressControlRequiresTextBasherAttribute());
			#endif
		}

		void RegisterEvents()
		{
			logger = new UserAttendedStlRetrieverLogger();
			logger.SyncInvoke = this;
			logger.OnInitProgress += new StlRetrieverEvent(OnInitProgress);
			logger.OnTaskProgress += new StlRetrieverEvent((msg) => OnRetrieveMessage(msg, progressStep: true));
			logger.OnTaskInfo += new StlRetrieverEvent((msg) => OnRetrieveMessage(msg, progressStep: false));
			logger.OnTaskFailed += new StlRetrieverEvent(OnRetrieveFailed);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			InitialiseYearAndMonthControls();
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			RefreshScriptsList();
		}

		IEnumerable<IStlScriptWithConfig> ScriptsList
		{
			get
			{
				return scriptsList ?? (scriptsList = new ScriptLoader(logger).Load(new BusinessObjectFactory()));
			}
		}
		IEnumerable<IStlScriptWithConfig> scriptsList;

		void InitialiseYearAndMonthControls()
		{
			var oneDayBeforeUtcMinusOneMonth = utcMinusOneMonth.AddDays(-1);
			this.FromDate.Value = new DateTime(oneDayBeforeUtcMinusOneMonth.Year, oneDayBeforeUtcMinusOneMonth.Month, oneDayBeforeUtcMinusOneMonth.Day);
			this.ToDate.Value = new DateTime(utcMinusOneMonth.Year, utcMinusOneMonth.Month, utcMinusOneMonth.Day, 23, 59, 59, 999);
		}

		void RefreshScriptsList()
		{
			var stlScriptsList = new List<ScriptViewer>();
			var scriptsMatchingRange = NewMonthlyCollectionRange().SelectApplicableScripts(ScriptsList).Select(t => t.Script);

			try
			{
				stlScriptsList.Add(new ScriptViewer((NoResString)"All Items"));

				foreach (var stlScript in scriptsMatchingRange.OrderBy(s => s.Code))
				{
					stlScriptsList.Add(new ScriptViewer(stlScript.Code, stlScript.Feature));
				}

				var initialSelection = (this.script.SelectedItem != null) ? ((ScriptViewer)this.script.SelectedItem).Code : "";
				this.script.DataSource = stlScriptsList;
				var index = stlScriptsList.FindIndex(s => s.Code == initialSelection);
				this.script.SelectedIndex = (index > 0) ? index : 0;
			}
			catch (BillingException e)
			{
				this.outputTextBox.Text = e.Message;
			}
		}

		internal class ScriptViewer
		{
			public ScriptViewer(string feature)
			{
				this.Feature = feature;
			}

			public ScriptViewer(string code, string feature)
			{
				this.Code = code;
				this.Feature = feature;
			}

			public string Code { get; private set; }
			public string Feature { get; private set; }

			public override string ToString()
			{
				return string.IsNullOrEmpty(Code) ? Feature : string.Format(CultureInfo.InvariantCulture, "{0} - {1}", Code, Feature);
			}

			public override bool Equals(object obj)
			{
				return (obj is ScriptViewer scriptViewer) && scriptViewer.ToString().Equals(ToString(), StringComparison.InvariantCultureIgnoreCase);
			}

			public override int GetHashCode() => ToString().GetHashCode();

			public static bool operator ==(ScriptViewer obj1, ScriptViewer obj2) => obj1?.Equals(obj2) ?? ReferenceEquals(obj1, obj2);
			public static bool operator !=(ScriptViewer obj1, ScriptViewer obj2) => !(obj1?.Equals(obj2) ?? ReferenceEquals(obj1, obj2));
		}

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);
			AbortRetriever();
		}

		void collectButton_Click(object sender, EventArgs e)
		{
			try
			{
				collectButton.Enabled = false;

				if (IsRetrieverRunning)
				{
					AbortRetriever();
					OnRetrieveAborted();
				}
				else
				{
					this.outputTextBox.Text = OutPutTimeToTextBox();
					progressBar.Value = 0;
					this.outputTextBox.ForeColor = Color.Black;
					StartRetriever();
				}
			}
			finally
			{
				collectButton.Enabled = true;
			}
		}

		public string OutPutTimeToTextBox()
		{
			return string.Format(CultureInfo.InvariantCulture, (NoResString)"From/To: {0}/{1}\r\n", this.FromDate.Value.ToString("dd/MM/yyyy HH:mm:ss"), this.ToDate.Value.ToString("dd/MM/yyyy HH:mm:ss"));
		}

		void StartRetriever()
		{
			cts = new CancellationTokenSource();
			_ = Task.Run(RunRetriever, cts.Token);
		}

		AusydMonthRange NewMonthlyCollectionRange()
		{
			return AusydMonthRange.NewByStartEnd(this.FromDate.Value, this.ToDate.Value);
		}

		void RunRetriever()
		{
			using (Db.DisposableActionForDbConnection())
			{
				try
				{
					if (selectedScriptViewer == null)
					{
						throw new BillingException("Valid price item not selected");
					}

#pragma warning disable CW1161 // Res.GetString Analyzer
					OnRetrieveStarted("STL Data Collection started!");
					var transactions = new StlRetrieverMonthly(logger).CollectAndSend(cts.Token, NewMonthlyCollectionRange(), selectedScriptViewer.Code);
					OnRetrieveCompleted("STL Data Collection completed!", transactions);
#pragma warning restore CW1161 // Res.GetString Analyzer
				}
				catch (BillingException e)
				{
					this.outputTextBox.Text += e.Message + "\r\n";
				}
			}
		}
		ScriptViewer selectedScriptViewer;

		public bool IsRetrieverRunning
		{
			get { return cts is { IsCancellationRequested: false }; }
		}

		void OnRetrieveStarted(string message)
		{
			this.BeginInvoke(new Action(() =>
			{
				SetFieldsReadOnly(true);
				collectButton.Text = (NoResString)"Stop";
				this.outputTextBox.Text += message + "\r\n";
				outputTextBox.Refresh();
			}));
		}

		void OnInitProgress(string message)
		{
			this.BeginInvoke(new Action(() =>
			{
				progressBar.Minimum = 0;
				progressBar.Maximum = Convert.ToInt32(message, CultureInfo.InvariantCulture);
			}));
		}

		void OnRetrieveMessage(string message, bool progressStep)
		{
			this.BeginInvoke(new Action(() =>
			{
				this.outputTextBox.Text += message + "\r\n";
				this.outputTextBox.Refresh();

				if (progressStep)
				{
					progressBar.Increment(1);
				}
			}));
		}

		void OnRetrieveCompleted(string message, IEnumerable<IStlTransaction> transactions)
		{
			this.BeginInvoke(new Action(() =>
			{
				SetFieldsReadOnly(false);
				collectButton.Text = (NoResString)"Collect";
				this.outputTextBox.ForeColor = Color.Green;
				this.outputTextBox.Text += message;
				this.outputTextBox.Refresh();
				var transactionsForDisplay = new StlTransactionForDisplayCollection();
				if (transactions != null)
				{
					foreach (var transaction in transactions)
					{
						transactionsForDisplay.Add(new StlTransactionForDisplay
						{
							Branch = transaction.Branch,
							AdditionalRefs = transaction.AdditionalRefs,
							BillableCount = transaction.BillableCount,
							ClientStaffCode = transaction.ClientStaffCode,
							Company = transaction.CompanyCode,
							PriceItemCode = transaction.PriceItemCode,
							Reference1 = transaction.Reference1,
							Reference2 = transaction.Reference2,
							Reference3 = transaction.Reference3,
							Reference4 = transaction.Reference4,
							Reference5 = transaction.Reference5,
							ServiceOccuredUTC = transaction.ServiceOccuredUTC,
						});
					}
				}
				ZFormModaliser.ShowDialogAndDispose(new StlRetrieverCollectedDataForm(transactionsForDisplay));
				AbortRetriever();
			}));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "developer tool only")]
		void OnRetrieveFailed(string message)
		{
			this.BeginInvoke(new Action(() =>
			{
				SetFieldsReadOnly(false);
				collectButton.Text = "Collect";
				this.outputTextBox.ForeColor = Color.Red;
				this.outputTextBox.Text += message + "\r\nSTL Data Collection failed.";
				this.outputTextBox.Refresh();
			}));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "developer tool only")]
		void OnRetrieveAborted()
		{
			this.BeginInvoke(new Action(() =>
			{
				SetFieldsReadOnly(false);
				collectButton.Text = "Collect";
				this.outputTextBox.ForeColor = Color.Gray;
				this.outputTextBox.Text += "STL Data Collection aborted.";
				this.outputTextBox.Refresh();
			}));
		}

		public void AbortRetriever()
		{
			cts?.Cancel();
		}

		void SetFieldsReadOnly(bool value)
		{
			this.FromDate.SetReadOnly(value);
			this.ToDate.SetReadOnly(value);
			this.script.SetReadOnly(value);
		}

		CancellationTokenSource cts;
		UserAttendedStlRetrieverLogger logger;
		protected DateTime utcMinusOneMonth;

		void script_SelectedIndexChanged(object sender, EventArgs e)
		{
			selectedScriptViewer = this.script.SelectedValue as ScriptViewer;
		}

		void FromDate_ValueChanged(object sender, EventArgs e)
		{
			if (this.FromDate.Value > this.ToDate.Value)
			{
				this.collectButton.Enabled = false;
			}
			else
			{
				this.collectButton.Enabled = true;
			}
			RefreshScriptsList();
		}

		void ToDate_ValueChanged(object sender, EventArgs e)
		{
			if (this.FromDate.Value > this.ToDate.Value)
			{
				this.collectButton.Enabled = false;
			}
			else
			{
				this.collectButton.Enabled = true;
			}
			RefreshScriptsList();
		}
	}
}
