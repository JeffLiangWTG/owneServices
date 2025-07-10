using System;
using System.IO;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

#if DEBUG
using Enterprise.ZArchitecture.GUI.Testing;
#endif

namespace Enterprise.DataTransfer.GUI
{
#if DEBUG
	[TestExcludeZWinFormHasTypedConstructor]
#endif
	public partial class DataExportForm : ProgressForm, INotifications, INotificationSubscriberQueryUser
	{
		protected DataExportForm()
		{
			InitializeComponent();
			this.OutputTextbox.ReadOnly = true;
		}

		public DataExportForm(FlatFileDataExporter exporter, BusinessObjectReader readerObject)
			: this()
		{
			this.Exporter = exporter;
			this.ReaderObject = readerObject;
		}

		#region INotifications Members

		void INotifications.Add(INotification notification)
		{
			Notify(notification);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		public void Notify(INotification notification)
		{
			if (!string.IsNullOrEmpty(notification.Message) || notification is NewlineNotification)
			{
				OutputTextbox.AppendText(notification.Message + System.Environment.NewLine);
				Application.DoEvents();
			}
		}

		void INotificationSubscriberQueryUser.QueryUser(IQueryUserEventArgs e)
		{
			INotificationSubscriberQueryUser queryUser = ObjectFactory.Get<INotificationSubscriberQueryUser>();
			queryUser.QueryUser(e);
		}

		#endregion

		readonly FlatFileDataExporter Exporter;
		readonly BusinessObjectReader ReaderObject;

		string FileExtensionFilter
		{
			get
			{
				FileExtensionFilterBuilder filterBuilder = new FileExtensionFilterBuilder();
				filterBuilder.Add(Exporter.FileExtensionType);
				return filterBuilder.FilterClause;
			}
		}

		public override string FormCaption
		{
			get { return Exporter.LocalizedDescription + " " + Res.GetString("42c6855d-a734-41bc-af75-804a0e62b763", "Export"); }
		}

		public override string FormVerb
		{
			get { return ZString.Empty; }
		}

		void ExportButton_Click(object sender, EventArgs e)
		{
			Export();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		protected virtual internal void Export()
		{
			SetAllButtonsEnabled(false);
			ExportOK = true;

			try
			{
				OutputTextbox.Text = Res.GetString("11a2e9cb-31e2-49f4-a355-d9958a4d183b", "Export in progress - please be patient, this may take some time.") + "\r\n\r\n";
				Application.DoEvents();
				Exporter.PromptForFilename += new FilenameEventHandler(Exporter_PromptForFilename);
				Exporter.PromptForEmailDetails += new EmailExportInstructionsEventHandler(Exporter_PromptForEmailDetails);
				Exporter.Export(ReaderObject, this);
			}
			catch (IOException ex)
			{
				ShowExportFailedMessage(ex.Message);
			}
			catch (UnauthorizedAccessException ex)
			{
				ShowExportFailedMessage(ex.Message);
			}
			catch (NotSupportedException ex)
			{
				ShowExportFailedMessage(ex.Message);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("DataExportForm.Export", "Error in " + Exporter.EnglishDescription + " export", ex);
				ShowExportFailedMessage(Res.GetString("92c48c93-46c5-4431-860f-b79fdc756e0e", "Export could not be completed. An error report of the problem has been sent to CargoWise."));
			}
			finally
			{
				SetAllButtonsEnabled(true);
				Exporter.PromptForFilename -= new FilenameEventHandler(Exporter_PromptForFilename);
				Exporter.PromptForEmailDetails -= new EmailExportInstructionsEventHandler(Exporter_PromptForEmailDetails);
			}

			if (ExportOK && Exporter.IsExportOK)
			{
				OutputTextbox.AppendText(Res.GetString("7039d8b3-3346-4876-9a87-7f44f0ebef4a", "Export complete."));
				PercentComplete = 100;
			}
			else
			{
				OutputTextbox.AppendText(Res.GetString("6263a256-4e5c-4bd4-b3f9-e034fb0fb8e9", "Export was unsuccessful."));
			}
		}

		void ShowExportFailedMessage(string message)
		{
			Globals.Message.ShowError(message);
			ExportOK = false;
		}

		void SetAllButtonsEnabled(bool enabled)
		{
			ExportButton.Enabled = enabled;
			CloseButton.Text = enabled ? Res.GetString("d48357f7-d5f1-411d-b968-2d86337408f3", "Close") : Res.GetString("3bf1fbd3-0984-4470-81eb-0f4ac20a5b38", "Cancel");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		void Exporter_ProgressChanged(object sender, ProgressEventArgs e)
		{
			PercentComplete = e.PercentComplete;
			Application.DoEvents();
		}

		void Exporter_PromptForFilename(object sender, FilenameEventArgs e)
		{
			FlatFileDataExporter exporter = (FlatFileDataExporter)sender;

			var dialog = new ZSaveFileDialog();
			dialog.AddExtension = true;
			dialog.Filter = FileExtensionFilter;
			dialog.RestoreDirectory = true;
			SetDefaultFileName(dialog);
			DialogResult result = dialog.ShowDialog(this);

			if (result == DialogResult.OK)
			{
				e.UnmappedFilename = dialog.UnmappedFileName;
			}
			else
			{
				ExportOK = false; // user cancelled
			}
		}

		void SetDefaultFileName(ZSaveFileDialog dialog)
		{
			if (GetSetDefaultFileNameMethod != null)
			{
				GetSetDefaultFileNameMethod(dialog);
			}
		}

		public delegate void GetSetDefaultFileNameMethodDelegate(ZSaveFileDialog dialog);

		public GetSetDefaultFileNameMethodDelegate GetSetDefaultFileNameMethod;

		void Exporter_PromptForEmailDetails(object sender, EmailExportInstructionsEventArgs e)
		{
			if (ZFormModaliser.ShowDialogAndDispose(new EmailDetailsForm(e.Instructions)) != DialogResult.OK)
			{
				ExportOK = false;
			}
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		bool ExportOK;
	}
}
