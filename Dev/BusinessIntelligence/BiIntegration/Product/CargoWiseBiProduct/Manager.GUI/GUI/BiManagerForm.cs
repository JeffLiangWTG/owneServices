using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Bi.Common;
using CargoWise.Bi.Product.Manager.Business;
using CargoWise.Data;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace CargoWise.Bi.Product.Manager.GUI
{
	public partial class BiManagerForm : ZChildForm
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception message, Error")]
		public BiManagerForm(BiMonitorBusinessObject businessEntity) : base(businessEntity)
		{
			try
			{
				IsAuditEnabled = BiServiceTaskHelpers.IsAuditEnabled() != null;
				InitializeComponent();
			}
			catch (Exception ex) when (ex.Message.Contains("The server was not found or was not accessible"))
			{
				using (ZMessageBox notification = new ZMessageBox("An error occurred connecting to the audit database. Please contact your system administrator", "Audit server error", MessageBoxButtons.OK, MessageBoxIcon.Error))
				{
					ZFormModaliser.ShowDialogAndDispose(notification);
				}
			}
		}

		protected virtual bool IsAuditEnabled
		{
			get;
#if DEBUG
			set;
#endif
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Error caption")]
		protected override void OnLoad(EventArgs e)
		{
			if (Globals.IsTest || !IsAuditEnabled)
			{
				base.OnLoad(e);
			}
			else
			{
				ThreadRunner.RunInAnotherThread(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						var bo = BindingSource.DataSource as BiMonitorBusinessObject;
						if (bo != null)
						{
							CancellationTokenSource tokenSource = new CancellationTokenSource();
							ShowLoadingTextAsync(tokenSource.Token);

							if (bo.BiInformation != null)
							{
								var errorMessage = bo.BiInformation.GetErrorMessage();
								if (!string.IsNullOrEmpty(errorMessage))
								{
									using (var msgBox = new ZMessageBox(errorMessage, "BI Manager error", MessageBoxButtons.OK, MessageBoxIcon.Error))
									{
										InvokeIfNotDisposed(() => ZFormModaliser.ShowMessageBoxWithoutDispose(msgBox));
									}
								}
								else
								{
									InvokeIfNotDisposed(new Action(() =>
									{
										cdcTabPage.TabVisible = bo.IsDbSetup();
										auditTabPage.TabVisible = bo.IsAuditUpdated();
										edwTabPage.TabVisible = bo.IsEdwUpdated();
										ssasTabPage.TabVisible = bo.IsAnalysisServerSet();
									}));
								}

								tokenSource.Cancel();
							}
						}
						InvokeIfNotDisposed(() => base.OnLoad(e));
					}
				});
			}
		}

		void CenterTextBox(object sender, EventArgs e)
		{
			retrievingTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 18, true);

			int locationX = (Width - retrievingTextBox.Width) / 2;
			int locationY = (Height - retrievingTextBox.Height) / 3;

			retrievingTextBox.Location = Windows.UI.ControlDpiScalingHelper.NewScaledPoint(locationX, locationY, false);
		}

		public void ShowLoadingTextAsync(CancellationToken token)
		{
			var thread = new Thread(new ThreadStart(() =>
			{
				ShowLoadingText(token);
			}));
			thread.Start();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "BI Manager message")]
		async void ShowLoadingText(CancellationToken token)
		{
			InvokeIfNotDisposed(new Action(() =>
			{
				retrievingTextBox.Visible = true;
				SetControlsEnabledFlag(false);
			}));

			try
			{
				while (!token.IsCancellationRequested)
				{
					InvokeIfNotDisposed(new Action(() =>
					{
						retrievingTextBox.Text = "Retrieving Information";
					}));
					await Task.Delay(500, token);

					InvokeIfNotDisposed(new Action(() =>
					{
						retrievingTextBox.Text = "Retrieving Information.";
					}));
					await Task.Delay(500, token);

					InvokeIfNotDisposed(new Action(() =>
					{
						retrievingTextBox.Text = "Retrieving Information..";
					}));
					await Task.Delay(500, token);

					InvokeIfNotDisposed(new Action(() =>
					{
						retrievingTextBox.Text = "Retrieving Information...";
					}));
					await Task.Delay(500, token);
				}
			}
			catch (TaskCanceledException)
			{ }
			finally
			{
				InvokeIfNotDisposed(new Action(() =>
				{
					retrievingTextBox.Visible = false;
					SetControlsEnabledFlag(true);
				}));
			}
		}

		void InvokeIfNotDisposed(Action target)
		{
			try
			{
				if (!IsDisposed && !IsDisposing)
				{
					Invoke(target);
				}
			}
			catch (ObjectDisposedException)
			{ }
		}

		void SetControlsEnabledFlag(bool value)
		{
			if (infoControl != null)
			{
				infoControl.Enabled = value;
			}
			if (cdcControl != null)
			{
				cdcControl.Enabled = value;
			}
			if (auditControl != null)
			{
				auditControl.Enabled = value;
			}
			if (edwControl != null)
			{
				edwControl.Enabled = value;
			}
			if (ssasControl != null)
			{
				ssasControl.Enabled = value;
			}
		}

		void infoTabPage_InitializeTab(object sender, EventArgs e)
		{
			infoControl = new InformationControl();
			infoTabPage.SuspendLayout();
			infoControl.SuspendLayout();
			infoTabPage.Controls.Add(infoControl);

			BindingSource.SetBindingMember(infoControl, ".");
			infoControl.AllowDrop = true;
			infoControl.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("c69fa1c4-2df4-45da-b916-976b4a5062aa", "Information");
			infoControl.Dock = DockStyle.Fill;
			infoControl.Location = Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			infoControl.Name = "infoControl";
			infoControl.Padding = Windows.UI.ControlDpiScalingHelper.NewScaledPadding(8, true);
			infoControl.Size = Windows.UI.ControlDpiScalingHelper.NewScaledSize(715, 745, true);
			infoControl.TabIndex = 0;
			infoTabPage.PerformLayout();
			infoControl.ResumeLayout(true);
			infoControl.PerformLayout();
			infoTabPage.ResumeLayout(true);
		}

		void cdcTabPage_InitializeTab(object sender, EventArgs e)
		{
			cdcControl = new CdcControl();
			cdcTabPage.SuspendLayout();
			cdcControl.SuspendLayout();
			cdcTabPage.Controls.Add(cdcControl);

			BindingSource.SetBindingMember(cdcControl, "CdcInformation");
			cdcControl.AllowDrop = true;
			cdcControl.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("a1f2c5b4-aa75-4254-852d-6eddcfbd5b21", "CDC");
			cdcControl.Dock = DockStyle.Fill;
			cdcControl.Location = Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			cdcControl.Margin = Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			cdcControl.Name = "cdcControl";
			cdcControl.Padding = Windows.UI.ControlDpiScalingHelper.NewScaledPadding(8, true);
			cdcControl.Size = Windows.UI.ControlDpiScalingHelper.NewScaledSize(716, 684, true);
			cdcControl.TabIndex = 0;
			cdcTabPage.PerformLayout();
			cdcControl.ResumeLayout(true);
			cdcControl.PerformLayout();
			cdcTabPage.ResumeLayout(true);
		}

		void auditTabPage_InitializeTab(object sender, EventArgs e)
		{
			auditControl = new AuditControl();
			auditTabPage.SuspendLayout();
			auditControl.SuspendLayout();
			auditTabPage.Controls.Add(auditControl);

			BindingSource.SetBindingMember(auditControl, "AuditInformation");
			auditControl.AllowDrop = true;
			auditControl.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("d81c1734-3c33-4141-a9b8-640cbd5784c4", "Audit");
			auditControl.Dock = DockStyle.Fill;
			auditControl.Location = Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			auditControl.Margin = Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			auditControl.Name = "auditControl";
			auditControl.Padding = Windows.UI.ControlDpiScalingHelper.NewScaledPadding(8, true);
			auditControl.Size = Windows.UI.ControlDpiScalingHelper.NewScaledSize(716, 684, true);
			auditControl.TabIndex = 0;
			auditTabPage.PerformLayout();
			auditControl.ResumeLayout(true);
			auditControl.PerformLayout();
			auditTabPage.ResumeLayout(true);
		}

		void edwTabPage_InitializeTab(object sender, EventArgs e)
		{
			edwControl = new EdwControl();
			edwTabPage.SuspendLayout();
			edwControl.SuspendLayout();
			edwTabPage.Controls.Add(edwControl);

			BindingSource.SetBindingMember(edwControl, "EdwInformation");
			edwControl.AllowDrop = true;
			edwControl.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("7c3c9cdf-0cd3-4550-9896-9ea46f0dd267", "EDW");
			edwControl.Dock = DockStyle.Fill;
			edwControl.Location = Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			edwControl.Margin = Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			edwControl.Name = "edwControl";
			edwControl.Padding = Windows.UI.ControlDpiScalingHelper.NewScaledPadding(8, true);
			edwControl.Size = Windows.UI.ControlDpiScalingHelper.NewScaledSize(716, 684, true);
			edwControl.TabIndex = 0;
			edwTabPage.PerformLayout();
			edwControl.ResumeLayout(true);
			edwControl.PerformLayout();
			edwTabPage.ResumeLayout(true);
		}

		void ssasTabPage_InitializeTab(object sender, EventArgs e)
		{
			ssasControl = new AnalysisServerControl();
			ssasTabPage.SuspendLayout();
			ssasControl.SuspendLayout();
			ssasTabPage.Controls.Add(ssasControl);

			BindingSource.SetBindingMember(ssasControl, "AnalysisServerInfo");
			ssasControl.AllowDrop = true;
			ssasControl.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("9dc2800c-aa16-4dfa-b067-c8a2621da9ed", "Analysis Server");
			ssasControl.Dock = DockStyle.Fill;
			ssasControl.Location = Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			ssasControl.Margin = Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			ssasControl.Name = "ssasControl";
			ssasControl.Padding = Windows.UI.ControlDpiScalingHelper.NewScaledPadding(8, true);
			ssasControl.Size = Windows.UI.ControlDpiScalingHelper.NewScaledSize(716, 684, true);
			ssasControl.TabIndex = 0;
			ssasTabPage.PerformLayout();
			ssasControl.ResumeLayout(true);
			ssasControl.PerformLayout();
			ssasTabPage.ResumeLayout(true);
		}
#if DEBUG
		public void SetSsasTabPageVisible()
		{
			ssasTabPage.TabVisible = true;
		}

#endif
	}
}
