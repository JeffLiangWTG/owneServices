using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.Client.JAS.Business.JXC.Import;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.JAS.GUI
{
	public partial class JXCImporterForm : ZChildForm, INotifications
	{
		public JXCImporterForm(JXCDataImporterBizO businessEntity)
			: base(businessEntity)
		{
			SummaryTextBox.Font = new Font("Courier New", 8);
		}

		public override string FormHeading
		{
			get { return "Import JXC File"; }
		}

		public new JXCDataImporterBizO BusinessEntity
		{
			get { return (JXCDataImporterBizO)base.BusinessEntity; }
		}

		#region Event Handlers

		void BrowseButton_Click(object sender, EventArgs e)
		{
			DialogResult result = ZFormModaliser.ShowCommonDialogWithoutDispose(openFileDialog1);
			if (result == DialogResult.OK)
			{
				BusinessEntity.ImportFilePath = openFileDialog1.UnmappedFileName;
			}
		}

		void ImportButton_Click(object sender, EventArgs e)
		{
			BusinessEntity.RunPreSaveValidation();
			if (!BusinessEntity.HasErrors)
			{
				try
				{
					ImportButton.Enabled = false;
					CloseButton.Enabled = false;
					BrowseButton.Enabled = false;
					ImportFilePathTextBox.Enabled = false;

					bool result;
					using (new ZWaitCursorChanger(this))
					{
						result = BusinessEntity.Import(this);
					}

					if (result)
					{
						Globals.Message.ShowInformation(string.Format("JXC file '{0}' has been sucessfully imported to the system.", BusinessEntity.ImportFilePath));
					}
					else
					{
						Globals.Message.ShowError("An error has occurred during import. Review the import log for details.");
					}
				}
				finally
				{
					CloseButton.Enabled = true;
				}
			}
			else
			{
				Globals.Message.ShowError("There are errors that need to be fixed before JXC file can be imported");
			}
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		#endregion

		#region INotifications Members

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		void INotifications.Add(INotification notification)
		{
			INotificationSubscriberNotification subscriberNotification = notification as INotificationSubscriberNotification;
			BusinessEntity.AppendImportSummary(subscriberNotification != null ? subscriberNotification.MultiLineDisplayMessage : (notification.Message + "\n"));
			SummaryTextBox.Focus();
			SummaryTextBox.Select(SummaryTextBox.Text.Length, 0);
			SummaryTextBox.ScrollToCaret();
			Application.DoEvents();
		}
		#endregion
	}
}
