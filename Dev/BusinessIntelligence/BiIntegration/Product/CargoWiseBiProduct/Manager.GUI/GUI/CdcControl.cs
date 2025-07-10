using System;
using System.Globalization;
using System.Text;
using System.Threading;
using CargoWise.Bi.Product.Manager.Business;
using CargoWise.Data;
using Enterprise.ZArchitecture.GUI;

namespace CargoWise.Bi.Product.Manager.GUI
{
	public partial class CdcControl : ZUserControl
	{
		#region SuppressResourceStringsCheckRegion

		public CdcControl()
		{
			SkipSettingChildControlReadOnly = true;
			InitializeComponent();
		}

		protected virtual BiManagerForm GetParentForm()
		{
			return base.ParentForm as BiManagerForm;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			PopulateCdcSchemaErrors();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			RefreshInfo();
		}

		void refreshButton_Click(object sender, EventArgs e)
		{
			RefreshInfo();
		}

		public void RefreshInfo()
		{
			using (Db.DisposableActionForDbConnection())
			{
				ThreadRunner.RunInAnotherThread(() =>
				{
					var tokenSource = new CancellationTokenSource();
					var parentForm = GetParentForm();
					parentForm?.ShowLoadingTextAsync(tokenSource.Token);

					var bo = (BindingSource.DataSource as BiMonitorBusinessObject);
					if (bo != null)
					{
						bo.RefreshCdcInformation();
					}
					if (parentForm != null && !parentForm.IsDisposed && !parentForm.IsDisposing)
					{
						Invoke(new Action(() =>
						{
							cdcErrorsGrid.SetDataBinding(bo.CdcInformation, "CdcErrors");
							RecreateHandle();
						}));
					}

					tokenSource.Cancel();
				});
			}
		}

		protected internal void PopulateCdcSchemaErrors()
		{
			var thread = new Thread(new ThreadStart(() =>
			{
				var cdcInfo = (BindingSource.DataSource as BiMonitorBusinessObject)?.CdcInformation;
				if (cdcInfo != null)
				{
					cdcInfo.PopulateCdcSchemaErrors();
					var parentForm = GetParentForm();
					if (parentForm != null && !parentForm.IsDisposed && !parentForm.IsDisposing)
					{
						try
						{
							Invoke(new Action(() =>
							{
								cdcSchemaErrorTextBox.Text = cdcInfo.CdcSchemaErrors;
							}));
						}
						catch (ObjectDisposedException) { }
					}
				}
			}));
			thread.Start();
		}

		void copyInfoButton_Click(object sender, EventArgs e)
		{
			var builder = new StringBuilder();
			builder.AppendLine("CDC Information");
			builder.AppendFormat(CultureInfo.InvariantCulture, "Server Name: {0}\r\n", serverTextBox.Text);
			builder.AppendFormat(CultureInfo.InvariantCulture, "Database Name: {0}\r\n", dbNameTextBox.Text);
			builder.AppendFormat(CultureInfo.InvariantCulture, "CDC Enabled: {0}\r\n", cdcEnabledTextBox.Text);
			builder.Append("\r\n");
			builder.AppendFormat(CultureInfo.InvariantCulture, "Earliest Scanned Transaction: {0}\r\n", earliestTransactionTextBox.Text);
			builder.AppendFormat(CultureInfo.InvariantCulture, "Last Scanned Transaction: {0}\r\n", lastTransactionTextBox.Text);
			builder.AppendFormat(CultureInfo.InvariantCulture, "Last CDC Scan: {0}\r\n", lastCdcScanTextBox.Text);
			builder.Append("\r\n");
			builder.AppendLine("CDC Schema");
			builder.AppendFormat(CultureInfo.InvariantCulture, "CDC Table Count: {0}\r\n", cdcTableCountTextBox.Text);
			builder.AppendFormat(CultureInfo.InvariantCulture, "CDC Schema Errors:\r\n{0}\r\n", cdcSchemaErrorTextBox.Text);

			SafeClipboard.SetText(builder.ToString());
		}

		#endregion
	}
}
