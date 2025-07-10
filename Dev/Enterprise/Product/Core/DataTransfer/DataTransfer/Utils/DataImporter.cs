using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Integration;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.MailFilters;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DataTransfer.Business
{
	public abstract class DataImporter : IDataImporterControllingSave
	{
		protected DataImporter(BusinessObjectFactoryProvider factoryProvider)
		{
			FactoryProvider = factoryProvider;
		}

		protected DataImporter()
			: this(new BusinessObjectFactoryProvider())
		{
		}

		protected DataImporter(BusinessObjectFactory factory)
			: this(new SingleBusinessObjectFactoryProvider(factory))
		{
		}

		public BusinessObject[] ImportedBusinessObjects
		{
			get;
			protected set;
		}

		public void ImportFromEmails(ZDateTime emailsReceivedSince, INotifications notifications, ISourceInfo sourceInfo)
		{
			var buffer = new NotificationBuffer(notifications);
			if (CheckEnvironmentValid(new BusinessObjectFactory(), buffer))
			{
				var queue = new MailBatchProcessor(GetMailItemFilter(), (item, position) =>
				{
					if (!ShouldProcessMailItem(item, emailsReceivedSince))
					{
						return MailProcessingResult.Unmatch;
					}

					return ProcessMailAttachmentsAndUpdateMailStatus(item, buffer, sourceInfo);
				});

				try
				{
					queue.Process(buffer);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					HandleException(ex, buffer);
				}
			}

			OnAfterImportFromEmails(buffer);
		}

		TextReader GetReader(string attachmentFileName) => new StreamReader(attachmentFileName, Encoding ?? Encoding.UTF8);

		public bool ImportData(string attachmentFileName, INotifications notifications, ISourceInfo sourceInfo)
		{
			using (TextReader reader = GetReader(attachmentFileName))
			{
				return ImportData(reader, attachmentFileName, notifications, sourceInfo);
			}
		}

		public class NotificationBufferNotificationHandler : INotificationHandler
		{
			public NotificationBufferNotificationHandler(NotificationBuffer buffer)
			{
				Argument.NotNull(buffer, "buffer");
				this.buffer = buffer;
			}
			readonly NotificationBuffer buffer;

			public void ReportInformation(string message, string caption)
			{
				buffer.Notify(new InfoNotification(caption + ": " + message));
			}

			public void ReportError(string message, string caption, string errorContext = null, Exception exception = null)
			{
				buffer.Notify(new ErrorNotification(ErrorType.Error, caption + ": " + message));
			}
		}

		public bool ImportData(TextReader dataReader, string attachmentFileName, INotifications notifications, ISourceInfo sourceInfo)
		{
			bool result = false;
			var buffer = new NotificationBuffer(notifications);

			try
			{
				FactoryProvider.CreateNewWithoutSave();
				if (CheckEnvironmentValid(FactoryProvider.Current, buffer))
				{
					bool flag = Registry.Business.OrganisationsDataRegistry.Instance.AllowNumericCharactersInCodeGeneration.Value;

					string enabled = Res.GetString("b59db324-459e-4981-9d39-ad4cb82b597f", "Enabled");
					string disabled = Res.GetString("8e748222-a06a-403e-892b-a5a1aa9305a4", "Disabled");

					buffer.Notify(new InfoNotification(Res.GetString("77d3dba6-8a15-4dce-96dd-7c694683473c", "Registry value for Organizations -> Allow Numeric Characters In Code Generation is currently {0}.", flag ? enabled : disabled)));

					flag = Registry.Business.OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.IsEnabled;

					buffer.Notify(new InfoNotification(Res.GetString("ff27679c-6be0-4b95-a7a5-773b3a19e15b", "Registry value for Organizations -> Use Default Organization for Matching is currently {0}.", flag ? enabled : disabled)));
					buffer.Notify(new InfoNotification(Res.GetString("3dce4696-d671-4cef-81bc-b2df8cf35c91", "Current Organization Match Threshold - {0}.", Registry.Business.OrganisationsDataRegistry.Instance.OrgMatchThreshold.Value)));
					buffer.Notify(new InfoNotification(Res.GetString("8b1bffb5-6764-4667-956e-d0628d221fe3", "Current Organization Proxy - {0}.", GlbCompany.CurrentCompany?.OrgProxy?.OH_Code ?? new ZString("N/A"))));
					buffer.Notify(new InfoNotification(Res.GetString("f93556e3-8acd-47ac-acc8-2fbef64f71b6", "Current Company - {0}.", GlbCompany.CurrentCompany != null ? GlbCompany.CurrentCompany.GC_Code : new ZString("N/A"))));
					buffer.Notify(new InfoNotification(Res.GetString("f2aebb20-b3e7-4693-accd-57d3506b4070", "Current Branch - {0}.", GlbBranch.CurrentBranch != null ? GlbBranch.CurrentBranch.GB_Code : new ZString("N/A"))));
					buffer.Notify(new NewlineNotification());

					ITransactionParticipant[] additionalTransactionActions;
					if (ImportDataToFactory(dataReader, attachmentFileName, buffer, sourceInfo, out additionalTransactionActions) && !HasFatalErrors(buffer))
					{
						try
						{
							var forSave = new List<ITransactionParticipant>();
							if (additionalTransactionActions != null)
							{
								forSave.AddRange(additionalTransactionActions);
							}

							if (!forSave.Contains(FactoryProvider.Current))
							{
								forSave.Insert(0, FactoryProvider.Current);
								FactoryProvider.CreateNewWithoutSave();
							}

							buffer.Notify(new NewlineNotification());
							buffer.Notify(new InfoNotification(Res.GetString("bd8a7e61-1bef-4b4d-8962-19b1ada4f65a", "Saving the data to the database...")));

							INotificationHandler oldNotificationHandler = NotificationHandler.Instance;
							try
							{
								NotificationHandler.Instance = new NotificationBufferNotificationHandler(buffer); //so that in batch processor context we do not receive errors as issues
								ZExceptionReporting.ProcessWithSaveExceptionHandling(() => DataTransferTransactionCoordinator.SaveTogether(forSave.ToArray()), null, true);
							}
							finally
							{
								NotificationHandler.Instance = oldNotificationHandler;
							}
							result = true;
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							HandleException(ex, notifications, Res.GetString("1823FE7E-F195-404C-9E63-8C514DF1524F", "Import {0} again.", attachmentFileName));
						}
					}
				}
				OnAfterImportData(buffer, result);
				SaveEDIInterchange(buffer);
			}
			finally
			{
				OnImportDataEnd(buffer, result);
			}
			return result;
		}

		protected virtual bool HasFatalErrors(NotificationBuffer buffer)
		{
			return OnlySaveDataWhenNoRecordsHaveErrors && buffer.HasErrors;
		}

		public bool OnlySaveDataWhenNoRecordsHaveErrors { get; set; }

		public bool ImportDataToFactory(string attachmentFileName, INotifications notifications, ISourceInfo sourceInfo, out ITransactionParticipant[] additionalTransactionActions)
		{
			return ImportDataToFactory(GetReader(attachmentFileName), attachmentFileName, notifications, sourceInfo, out additionalTransactionActions);
		}

		public bool ImportDataToFactory(TextReader dataReader, string attachmentFileName, INotifications notifications, ISourceInfo sourceInfo, out ITransactionParticipant[] additionalTransactionActions)
		{
			if (dataReader != null && Encoding != null && dataReader is StreamReader && ((StreamReader)dataReader).CurrentEncoding != Encoding)
			{
				ErrorReporter.ReportOnce("DataImporter.ImportDataToFactoryEncodingInconsistent", "Encoding on Stream different to Encoding override.");
			}

			var shouldSuspendValidation = sourceInfo != null
				? ShouldSuspendValidation && sourceInfo.ShouldSuspendValidation
				: ShouldSuspendValidation;

			using (SuspendValidation(FactoryProvider.Current, shouldSuspendValidation))
			{
				return ImportDataToFactoryCore(dataReader, attachmentFileName, notifications, out additionalTransactionActions);
			}
		}

		static DisposableAction SuspendValidation(BusinessObjectFactory factory, bool shouldSuspendValidation)
		{
			if (!shouldSuspendValidation)
			{
				return new DisposableAction(() => { });
			}

			factory.SuspendValidation();

			return new DisposableAction(() =>
			{
				if (factory.IsValidationSuspended)
				{
					factory.ResumeValidation();
				}
			});
		}

		protected virtual bool ShouldSuspendValidation
		{
			get { return true; }
		}

		public virtual bool CheckEnvironmentValid(BusinessObjectFactory factory, INotifications notifications)
		{
			return true;
		}

		protected virtual void SetEDIInterchange(IValueObjectImportContext context)
		{
			EDIInterchange = null;
			if (context != null)
			{
				EDIInterchange = context.EDIInterchange;
			}
		}

		protected virtual void OnErrorUpdateEDIInterchangeStatus(Exception ex, INotifications notifications)
		{
			if (EDIInterchange != null)
			{
				EDIInterchange.EI_Status = EDIInterchange.Status.Error;
			}
		}

		protected virtual void SaveEDIInterchange(INotifications notifications)
		{
			if (EDIInterchange != null)
			{
				try
				{
					EDIInterchange.Factory.Save();
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					EDIInterchange = null;
					HandleException(ex, notifications);
				}
			}
		}

		protected EDIInterchange EDIInterchange;

		protected abstract bool ImportDataToFactoryCore(TextReader dataReader, string attachmentFileName, INotifications notifications, out ITransactionParticipant[] additionalTransactionActions);

		protected virtual bool ShouldProcessMailItemCore(MailItem item)
		{
			return true;
		}

		protected virtual void OnAfterImportFromEmails(NotificationBuffer buffer)
		{
		}

		protected virtual void OnAfterImportData(NotificationBuffer buffer, bool sucessfullyImported)
		{
		}

		protected virtual void OnImportDataEnd(NotificationBuffer buffer, bool sucessfullyImported)
		{
		}

		protected BusinessObjectFactoryProvider FactoryProvider { get; }

		protected virtual Encoding Encoding
		{
			get { return null; }
		}

		protected virtual IMailFilter GetMailItemFilter() => throw new NotSupportedException(GetType().Name + " has not provided a mail filter");

		#region Implementation

		bool ShouldProcessMailItem(MailItem item, ZDateTime processMessagesAfterDate)
		{
			bool result = !processMessagesAfterDate.IsValid || item.MI_ReceivedDateTime > processMessagesAfterDate;
			return result && ShouldProcessMailItemCore(item);
		}

		public void ProcessWithSaveExceptionHandling(Action action, INotifications notifications)
		{
			try
			{
				action();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				HandleException(ex, notifications, null);
			}
		}

		protected void HandleException(Exception ex, INotifications notifications, string importAgainMessage = null)
		{
			if (ex is IOException)
			{
				notifications.Notify(new ErrorNotification(ErrorType.PostToDatabaseError, ex.Message));
			}
			else if (ex is SqlException)
			{
				notifications.Notify(new ErrorNotification(ErrorType.PostToDatabaseError, ex.Message));
			}
			else if (ex is ZSaveConcurrencyException)
			{
				notifications.Notify(new ErrorNotification(ErrorType.PostToDatabaseError,
					Res.GetString("456DEC5B-D4CD-4EB8-ADF7-7228EFD89848", "Concurrency error during saving.") + (importAgainMessage.IsNullOrEmpty() ? "" : " " + importAgainMessage) + "\r\n\r\n" +
					ex.Message));
			}
			else if (ex is ZSaveException)
			{
				notifications.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("ffe80a01-440d-4dbf-8d8f-54cc4641c3d6", "Error during saving") + "\r\n\r\n"));
				notifications.Notify(new ErrorNotification(ErrorType.PostToDatabaseError, ex.Message));
			}
			else if (ex is EmailSendFailedException)
			{
				notifications.Notify(new ErrorNotification(ErrorType.PostToDatabaseError, ex.Message));
			}
			else if (ex is ArgumentOutOfRangeException)
			{
				notifications.Notify(new ErrorNotification(ErrorType.DataOutOfRangeError, ex.Message));
			}
			else if (ex is ArgumentException)
			{
				notifications.Notify(new ErrorNotification(ErrorType.PostToDatabaseError, ex.Message));
			}
			else if (ExceptionVisibilityAttribute.Evaluate(ex) == ExceptionVisibility.User)
			{
				notifications.Notify(new ErrorNotification(ErrorType.Error, ex.Message));
			}
			else if (ex is OnSavingCriticalCheckException)
			{
				notifications.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("6d4793af-fec6-457f-82b2-0c2d8cb16e20", "Critical Error during saving") + "\r\n\r\n"));
				notifications.Notify(new ErrorNotification(ErrorType.PostToDatabaseError, ex.Message));
			}
			else if (ex is ZCannotSaveException)
			{
				notifications.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, ex.Message));
			}
			else if (ex is InvalidOperationException)
			{
				notifications.Notify(new ErrorNotification(ErrorType.ImportingDataError, ex.InnerException != null ? ex.InnerException.Message : ex.Message));
			}
			else
			{
				ErrorReporter.ReportOnce("Unexpected exception in data import", ex);
			}

			OnErrorUpdateEDIInterchangeStatus(ex, notifications);
		}

		#region Importing from Emails

		MailProcessingResult ProcessMailAttachmentsAndUpdateMailStatus(MailItem item, INotifications notifications, ISourceInfo sourceInfo)
		{
			notifications.Notify(new VerboseInfoNotification(Res.GetString("271a57e4-c3d7-4aab-8d1e-05f1f1bd1fca", "Processing email with subject '{0}'...", item.MI_Subject)));

			return ProcessMailAttachments(item, notifications, sourceInfo) ? MailProcessingResult.MarkSuccess : MailProcessingResult.MarkFailed;
		}

		protected virtual bool ProcessMailAttachments(MailItem item, INotifications notifications, ISourceInfo sourceInfo)
		{
			bool suitableAttachmentsFound = false;

			foreach (MailAttachment attachment in item.MailAttachments)
			{
				if (ProcessMailAttachment(attachment, item.MI_Subject, notifications, sourceInfo))
				{
					suitableAttachmentsFound = true;
				}
			}

			if (!suitableAttachmentsFound)
			{
				notifications.Notify(new VerboseInfoNotification(Res.GetString("0b9d4e58-7cdb-4776-84ed-01ca9e33842e", "No suitable attachments found")));
			}

			return suitableAttachmentsFound;
		}

		bool ProcessMailAttachment(MailAttachment attachment, ZString subject, INotifications notifications, ISourceInfo sourceInfo)
		{
			notifications.Notify(new InfoNotification(Res.GetString("04e1c93a-f624-425a-877c-253ffa83631f", "Processing attachment '{0}' of mail with subject '{1}'", attachment.MA_FileName, subject)));

			ZString message = attachment.MA_Data.ToUTF8();
			bool result = ImportData(new StringReader(message), attachment.MA_FileName, notifications, sourceInfo);

			return result;
		}

		#endregion

		#endregion

	}
}
