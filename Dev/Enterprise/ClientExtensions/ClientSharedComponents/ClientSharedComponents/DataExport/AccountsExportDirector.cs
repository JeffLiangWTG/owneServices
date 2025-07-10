using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ClientSharedComponents
{
	/// <summary>
	/// Data Export of All AR and AP Accounting transactions into ONE data file.
	/// </summary>
	/// <remarks>Call from both GUI and Batch Processor classes. When data exporting all AR & AP transactions into 1 file, inherit directly from here.</remarks>
	public abstract class AccountsExportDirector
	{
		public AccountsExportDirector(BusinessObjectFactory factory, INotifications notificationSubscriber)
		{
			this.factory = factory;
			notifications = new NotificationBuffer(notificationSubscriber);
		}

		#region Main Entry Point
		public void Execute()
		{
			finalDirectory = GetFinalDirectory();
			if (!string.IsNullOrEmpty(finalDirectory))
			{
				OnBeforeOuterExecute();
				if (!Exporter.IsTransactionsExistInBatch)
				{
					notifications.Notify(new ErrorNotification(ErrorType.Error, "No transactions in batch to export."));
				}
				else
				{
					DoOuterExecute(Env.GetTempFileName(Env.TempPath));
				}
				OnAfterOuterExecute();
			}
		}
		#endregion

		#region Execute Outer Ring
		protected virtual void OnBeforeOuterExecute()
		{
			notifications.Clear();
			filesCreated = new List<string>();
		}

		protected virtual void DoOuterExecute(string tempFileName)
		{
			OnBeforeInnerExecution();
			try
			{
				if (DoInnerExecution(tempFileName))   // Call inner execution ring.
				{
					OnInnerExecutionSuccess(tempFileName);
				}
				else
				{
					OnInnerExecutionExceptions(tempFileName);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				notifications.Notify(new ErrorNotification(ErrorType.Error, ErrorMessage + ex.Message));
			}
			finally
			{
				DeleteFile(tempFileName);
			}
			OnAfterInnerExecution();
		}

		protected virtual void OnAfterOuterExecute()
		{
			if (notifications.HasErrors)
			{
				NotifyFailure();
			}
			else
			{
				NotifySuccess();
			}
		}

		protected virtual void NotifyFailure()
		{
			notifications.Notify(new InfoNotification("File created: none."));
			if (EmailNotificationEnabled)
			{
				var batchNumber = string.Empty;
				if (Exporter.FilterProvider.CurrentBatchNo != 0)
				{
					batchNumber = string.Format(" ({0})", Exporter.FilterProvider.CurrentBatchNo);
				}
				SendEmail(string.Format("No account transaction files were created for the current batch{0}.", batchNumber), "FAILURE");
			}
		}

		protected virtual void NotifySuccess()
		{
			notifications.Notify(new InfoNotification(string.Format("File created: {0}.", "\"" + string.Join("\", \"", filesCreated.ToArray()) + "\"")));
			if (EmailNotificationEnabled)
			{
				SendEmail(ZString.Empty, string.Format("Batch {0}: SUCCESS", Exporter.FilterProvider.CurrentBatchNo.ToString()));
			}
		}
		#endregion

		#region Execution Inner Phase
		protected virtual void OnBeforeInnerExecution()
		{
			notifications.Notify(new InfoNotification(ExecutionStartMessage));
		}

		protected virtual bool DoInnerExecution(string tempFileName)
		{
			using (var fileStream = new FileStream(tempFileName, FileMode.Create))
			{
				Exporter.Export(fileStream);
			}
			return !notifications.HasErrors && AllInnerTransactionsExported && InnerDataWasExported;
		}

		protected virtual void OnInnerExecutionSuccess(string tempFileName)
		{
			PublishFile(tempFileName);
		}

		protected virtual void OnInnerExecutionExceptions(string tempFileName)
		{
			DeleteFile(tempFileName);
		}

		protected virtual void OnAfterInnerExecution()
		{
			notifications.Notify(new InfoNotification(ExecutionEndMessage));
		}

		protected virtual bool AllInnerTransactionsExported
		{
			get { return !Exporter.ErrorHasOccured; }
		}

		protected virtual bool InnerDataWasExported
		{
			get
			{
				return Exporter.NumberOfAccrualPostingProcessed_FlatFile + Exporter.NumberOfAccrualReversingProcessed_FlatFile +
					Exporter.NumberOfAdjustmentNotesProcessed_FlatFile + Exporter.NumberOfCreditNotesProcessed_FlatFile +
					Exporter.NumberOfInvoicesProcessed_FlatFile > 0;
			}
		}
		#endregion

		protected virtual string ErrorMessage
		{
			get { return "Error exporting data."; }
		}

		protected virtual void PublishFile(string sourceFilename)
		{
			if (!string.IsNullOrEmpty(finalDirectory))
			{
				PublishFile(sourceFilename, Path.Combine(finalDirectory, ExporterFileName));
			}
		}

		protected void PublishFile(string sourceFilename, string targetFilename)
		{
			if (new FileInfo(sourceFilename).Length > 0)
			{
				using (var sourceStream = File.OpenRead(sourceFilename))
				using (var targetStream = OpenFile(targetFilename))
				{
					sourceStream.CopyTo(targetStream);
				}
				filesCreated.Add(new FileInfo(targetFilename).Name);
			}
			else
			{
				notifications.Notify(new ErrorNotification(ErrorType.Error, "  No file available."));
			}
		}

		protected abstract Stream OpenFile(string fileName);

		protected abstract ZString GetFinalDirectory();

		protected void DeleteFile(string fileName)
		{
			try
			{
				File.Delete(fileName);
			}
			catch (IOException) { }
		}

		protected void SendEmail(ZString body, ZString status)
		{
			var emailDef = new EmailDef();
			emailDef.Subject = "Accounts Transaction Export. " + status;
			emailDef.Body = body + System.Environment.NewLine + System.Environment.NewLine + notifications.AsString;
			emailDef.FromDisplayName = EmailFromDisplayName;
			Env.OutgoingMailManager.CreateAndSave(emailDef, RecipientGroupPK, GroupSourceLocator.GetFromRegistryItem(RecipientGroup));
			notifications.Notify(new InfoNotification("Emailed Accounts report to the group."));
		}

		protected virtual string EmailFromDisplayName
		{
			get { return emailFromDisplayName; }
		}

		protected virtual string DataTypeName
		{
			get { return "Accounts "; }
		}

		protected virtual string AccountingPackageName
		{
			get { return string.Empty; }
		}

		protected virtual string ExporterFileName
		{
			get { return Exporter.FileName; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		string ExecutionStartMessage
		{
			get { return string.Format(" Exporting {0}data from CargoWise One to {1}Accounts data file...", DataTypeName, AccountingPackageName); }
		}

		string ExecutionEndMessage
		{
			get { return string.Format(" Exporting {0}data: Done.", DataTypeName); }
		}

		public bool EnableManualMode
		{
			get { return enableManualMode; }
			set { enableManualMode = value; }
		}
		protected bool enableManualMode;

		public abstract AccountsExporter Exporter { get; }
		protected abstract bool EmailNotificationEnabled { get; }
		protected abstract string ExportPathName { get; }
		protected abstract IRegistryItem RecipientGroup { get; }
		protected abstract Guid RecipientGroupPK { get; }
		protected NotificationBuffer notifications;
		protected List<string> filesCreated;
		protected BusinessObjectFactory factory;
		protected INotifications notificationSubscriber;
		protected int existingBatchNumberToExport;
		protected string finalDirectory;

		const string emailFromDisplayName = "Accounts Transaction Export";
	}
}
