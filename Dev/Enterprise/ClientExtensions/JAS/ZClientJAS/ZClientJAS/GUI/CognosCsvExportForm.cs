using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.Client.JAS.Business.Cognos;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.JAS.GUI.Cognos
{
	partial class CognosCsvExportForm : ZChildForm, ICognosNotificationSubscriber
	{
		public CognosCsvExportForm(CognosDataExporterBizO businessEntity)
			: base(businessEntity)
		{
			InitializeComponent();
		}

		public override string FormHeading
		{
			get { return "Cognos Data Export"; }
		}

		public new CognosDataExporterBizO BusinessEntity
		{
			get { return (CognosDataExporterBizO)base.BusinessEntity; }
		}

		void ExportButton_Click(object sender, EventArgs e)
		{
			HandleExportButtonClick();
		}

		void HandleExportButtonClick()
		{
			bool result;

			BusinessEntity.RunPreSaveValidation();
			if (!BusinessEntity.HasErrors)
			{
				try
				{
					ExportButton.Enabled = false;
					CloseButton.Enabled = false;
					EndPeriodCalcEdit.Enabled = false;
					jasDataExporterUserControl1.Enabled = false;

					using (new ZWaitCursorChanger(this))
					{
						result = BusinessEntity.Export(this);
					}

					if (!result)
					{
						Globals.Message.ShowError("An error has occurred during export. Review the export log for details.");
					}
					else
					{
						Globals.Message.ShowInformation("Cognos data has been successfully exported.");
					}
				}
				finally
				{
					CloseButton.Enabled = true;
				}
			}
			else
			{
				Globals.Message.ShowError("There are errors that need to be fixed before Cognos data can be exported");
			}
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		#region ICognosNotificationSubscriber Members

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		void ICognosNotificationSubscriber.AdvanceProgressBy(int percentage)
		{
			int totalPercentage = ExportProgressBar.Value + percentage;
			ExportProgressBar.Value = (totalPercentage > 100) ? 100 : totalPercentage;
			Application.DoEvents();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		void ICognosNotificationSubscriber.CompleteProgress()
		{
			ExportProgressBar.Value = 100;
			Application.DoEvents();
		}

		bool ICognosNotificationSubscriber.HasErrors
		{
			get { return ErrorNotificationNotified; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		void INotifications.Add(INotification notification)
		{
			if (notification is ErrorNotification)
			{
				ErrorNotificationNotified = true;
			}
			INotificationSubscriberNotification subscriberNotification = notification as INotificationSubscriberNotification;
			BusinessEntity.AppendExportSummary(subscriberNotification != null ? subscriberNotification.MultiLineDisplayMessage : notification.Message);
			SummaryTextBox.Focus();
			SummaryTextBox.Select(SummaryTextBox.Text.Length, 0);
			SummaryTextBox.ScrollToCaret();
			Application.DoEvents();
		}

		bool ErrorNotificationNotified;
		#endregion
	}
}
