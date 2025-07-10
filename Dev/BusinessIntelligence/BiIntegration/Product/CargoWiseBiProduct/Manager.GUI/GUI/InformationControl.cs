using System;
using System.Globalization;
using System.Text;
using System.Threading;
using CargoWise.Bi.Common;
using CargoWise.Bi.Product.Manager.Business;
using CargoWise.Data;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;

namespace CargoWise.Bi.Product.Manager.GUI
{
	public partial class InformationControl : ZUserControl
	{
		#region SuppressResourceStringsCheckRegion

		public InformationControl()
		{
			this.SkipSettingChildControlReadOnly = true;
			InitializeComponent();

			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var auditServer = BiServers.LoadAuditServerUsingCacheIfPossible(connection);
				if (string.IsNullOrEmpty(auditServer))
				{
					auditDatabaseGroupBox.Visible = false;
				}

				var dwServer = BiServers.LoadDataWarehouseServerUsingCacheIfPossible(connection);
				if (string.IsNullOrEmpty(dwServer))
				{
					edwDatabaseGroupBox.Visible = false;
				}

				var analysisServer = BiServers.LoadAnalysisServerUsingCacheIfPossible(connection);
				if (string.IsNullOrEmpty(analysisServer))
				{
					analysisServerGroupBox.Visible = false;
				}

				if (string.IsNullOrEmpty(SystemDataRegistry.Instance.BiPowerBiWebPortalUrl.Value))
				{
					powerBiServerGroupBox.Visible = false;
				}
			}
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			var thread = RefreshInformation();

			while (thread.ThreadState == ThreadState.Running)
			{
				Thread.Sleep(500);
			}
		}

		void refreshButton_Click(object sender, EventArgs e)
		{
			RefreshInformation();
		}

		public Thread RefreshInformation()
		{
			return ThreadRunner.RunInAnotherThread(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					var tokenSource = new CancellationTokenSource();
					var parentForm = (this.ParentForm as BiManagerForm);
					parentForm?.ShowLoadingTextAsync(tokenSource.Token);

					var bo = (BindingSource.DataSource as BiMonitorBusinessObject);
					if (bo != null)
					{
						bo.RefreshBiInformation();
#if !WINZOR
						bo.RefreshAnalysisCubeInformation();
						bo.RefreshPowerBiServerInformation();
#else
						bo.AnalysisServerInfo = new AnalysisServerInformation(null, null);
						bo.PowerBiServerInfo = new PowerBiServerInformation(null);
#endif
					}
					if (!parentForm.IsDisposed && !parentForm.IsDisposing)
					{
						try
						{
							Invoke(new Action(() => RecreateHandle()));
						}
						catch (ObjectDisposedException)
						{
						}
					}

					tokenSource.Cancel();
				}
			});
		}

		void copyInfoButton_Click(object sender, EventArgs e)
		{
			var builder = new StringBuilder();
			builder.AppendLine("Main Server Information");
			builder.AppendFormat(CultureInfo.InvariantCulture, "Database Server: {0}\r\n", mainServerTextBox.Text);
			builder.AppendFormat(CultureInfo.InvariantCulture, "SQL Server Version: {0}\r\n", mainServerVersionTextBox.Text);
			builder.AppendFormat(CultureInfo.InvariantCulture, "Database Name: {0}\r\n", mainDbNameTextBox.Text);
			builder.AppendFormat(CultureInfo.InvariantCulture, "Database Version: {0}\r\n", mainDbVersionTextBox.Text);
			builder.AppendFormat(CultureInfo.InvariantCulture, "Database Size: {0}\r\n", mainDbSizeTextBox.Text);
			builder.AppendFormat(CultureInfo.InvariantCulture, "Database Log Size: {0}\r\n", mainDbLogSizeTextBox.Text);
			builder.Append("\r\n");
			if (auditDatabaseGroupBox.Visible)
			{
				builder.AppendLine("Audit Database Information");
				builder.AppendFormat(CultureInfo.InvariantCulture, "Database Server: {0}\r\n", auditDbServerTextBox.Text);
				builder.AppendFormat(CultureInfo.InvariantCulture, "SQL Server Version: {0}\r\n", auditDbServerVersionTextBox.Text);
				builder.AppendFormat(CultureInfo.InvariantCulture, "Database Name: {0}\r\n", auditDbNameTextBox.Text);
				builder.AppendFormat(CultureInfo.InvariantCulture, "Database Version: {0}\r\n", auditDbVersionTextBox.Text);
				builder.AppendFormat(CultureInfo.InvariantCulture, "Database Size: {0}\r\n", auditDbSizeTextBox.Text);
				builder.AppendFormat(CultureInfo.InvariantCulture, "Database Log Size: {0}\r\n", auditDbLogSizeTextBox.Text);
				builder.Append("\r\n");
			}
			if (analysisServerGroupBox.Visible)
			{
				builder.AppendLine("Analysis Server Information");
				builder.AppendFormat(CultureInfo.InvariantCulture, "Analysis Server Name: {0}\r\n", analysisServerNameTextBox.Text);
				builder.AppendFormat(CultureInfo.InvariantCulture, "Analysis Server Version: {0}\r\n", analysisServerVersionTextBox.Text);
				builder.AppendFormat(CultureInfo.InvariantCulture, "Server Mode: {0}\r\n", analysisServerModeTextBox.Text);
				builder.Append("\r\n");
			}
			if (powerBiServerGroupBox.Visible)
			{
				builder.AppendLine("Power BI Server Information");
				builder.AppendFormat(CultureInfo.InvariantCulture, "Power BI Reports URL: {0}\r\n", powerBiReportsUrlTextBox.Text);
				builder.AppendFormat(CultureInfo.InvariantCulture, "Power BI Server Version: {0}\r\n", powerBiServerVersionTextBox.Text);
				builder.AppendFormat(CultureInfo.InvariantCulture, "Power BI Reports Version: {0}\r\n", powerBiReportsVersionTextBox.Text);
				builder.AppendFormat(CultureInfo.InvariantCulture, "Power BI Reports Status: {0}\r\n", powerBiReportsStatusTextBox.Text);
			}

			SafeClipboard.SetText(builder.ToString());
		}

#endregion
	}
}
