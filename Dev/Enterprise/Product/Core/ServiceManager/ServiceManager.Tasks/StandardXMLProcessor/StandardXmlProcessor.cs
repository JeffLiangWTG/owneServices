using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.eHubMessaging.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ServiceManager.Tasks.StandardXMLProcessor
{
	public abstract class StandardXmlProcessor : IProcessor
	{
		internal StandardXmlProcessor()
		{
		}

		#region IProcessor Members

		INotifications notifications;

		[SuppressMessage("Microsoft.Maintainability", "CA1500", Justification = "Difference maintained by the 'this' qualifier.")]
		public void Process(INotifications notifications, CancellationToken token
#if DEBUG
			= new CancellationToken()
#endif
		)
		{
			this.notifications = Argument.NotNull(notifications, "notifications");
			VerboseLoggingMemoryUsage();
			while (ProcessNewMessages(token))
			{
				if (token.IsCancellationRequested)
				{
					notifications.Notify(new WarningNotification(Res.GetString("eb8117d2-82b6-4bad-8c0c-06812e2f6b6a", "Task canceled.")));
					break;
				}
			}
		}

		internal virtual bool ProcessNewMessages(CancellationToken token)
		{
			var messagesProcessed = false;
			string[] companyCodes;
			if (NewMessageAvailable(out companyCodes))
			{
				foreach (var companyCode in companyCodes)
				{
					token.ThrowIfCancellationRequested();
					using (DisposableEnvironment.ForCompany(companyCode))
					{
						var messageBatch = new MessageBatch(GetEDIMessagePKs());
						messagesProcessed |= ProcessMessageBatch(messageBatch, token);
					}
				}
			}
			return messagesProcessed;
		}

		internal virtual bool AlwaysProcessOneByOne
		{
			get { return false; }
		}

#if DEBUG
		internal virtual bool ProcessMessageBatch(MessageBatch messageBatch)
		{
			return ProcessMessageBatch(messageBatch, CancellationToken.None);
		}
#endif

		internal virtual bool ProcessMessageBatch(MessageBatch messageBatch, CancellationToken token)
		{
			if (messageBatch.IsEmpty)
			{
				return false;
			}

			var processOneByOne = AlwaysProcessOneByOne;

			if (!processOneByOne)
			{
				try
				{
					ProcessInBatch(messageBatch, token);
				}
				catch (InvalidMessageContentException)
				{
					notifications.Notify(new InfoNotification(Res.GetString("97a46acb-f496-4b5c-895c-571083c369f4", "Unable to process messages in batch. Each message will be reprocessed separately.")));
					processOneByOne = true;
				}
				catch (Exception ex) when (!ex.IsCriticalException() && !ex.IsOutOfDiskSpaceException() && !ex.IsUnableToCreateTempFileException())
				{
					notifications.Notify(new WarningNotification(ex.Message));
					notifications.Notify(new InfoNotification(Res.GetString("97a46acb-f496-4b5c-895c-571083c369f4", "Unable to process messages in batch. Each message will be reprocessed separately.")));
					processOneByOne = true;
				}
			}

			if (processOneByOne)
			{
				ProcessOneByOne(messageBatch, token);
			}

			NotifyFinished(notifications);
			VerboseLoggingMemoryUsage();
			FactoryProvider.CreateNewWithoutSave();
			return true;
		}

		internal static void NotifyFinished(INotifications notifications)
		{
			notifications.Notify(new InfoNotification(Res.GetString("77773c58-79c5-451e-a72a-6b43024c8b0b", "Finished message processing.")));
		}

		#endregion

		internal virtual bool NewMessageAvailable(out string[] companyCodes)
		{
			companyCodes = null;
			return false;
		}

		internal abstract List<ZGuid> GetEDIMessagePKs();

		internal virtual IMessageAction GetMessageAction(ZString messageType, ZString messageSubType)
		{
			return MessageAction.New(FactoryProvider, messageType, messageSubType);
		}

		#region Factory

		protected BusinessObjectFactoryProvider FactoryProvider
		{
			get
			{
				factoryProvider = factoryProvider ?? new BusinessObjectFactoryProvider();
				factoryProvider.Current.RefreshEnabled = false;
				return factoryProvider;
			}
		}
		BusinessObjectFactoryProvider factoryProvider;

		#endregion

		#region Implementation

#if DEBUG
		internal virtual void ProcessInBatch(MessageBatch messageBatch)
		{
			ProcessInBatch(messageBatch, CancellationToken.None);
		}
		internal virtual
#endif
		void ProcessInBatch(MessageBatch messageBatch, CancellationToken token)
		{
			notifications.Notify(new InfoNotification(Res.GetString("15d65b8f-248c-44a4-a26d-6ea5adadee87", "Processing message batch. Number of messages: '{0}'", messageBatch.MessagePKs.Count)));
			var participants = new List<ITransactionParticipant>();
			FactoryProvider.CreateNewWithoutSave();
			using (FactoryProvider.Current.AddDisposableService())
			{
				for (int i = 0; i < messageBatch.MessagePKs.Count && !token.IsCancellationRequested; i++)
				{
					token.ThrowIfCancellationRequested();
					var message = FactoryProvider.Current.Load<EDIMessage>(messageBatch.MessagePKs[i]);

					notifications.Notify(new InfoNotification(GetProcessingMessageString(message)));
					VerboseLoggingProcessingMessageInBatchMode(message);

					var messageAction = GetMessageAction(message.EM_MessageType, message.EM_MessageSubType);

					if (messageAction == null)
					{
						string reference = GetMessageTypeNotSupportString(message);
						message.EM_Status = EDIMessage.Status.Error;
						AddMessageNote(message, reference);
						notifications.Notify(new WarningNotification(WarningType.Warning, reference));
						SendFailureAcknowledgement(message, reference);
						continue;
					}

					var forSave = new List<ITransactionParticipant>();
					var buffer = new NotificationBuffer(notifications);

					long currentFactoryId = FactoryProvider.Current._Instance;
					bool actionCompleteSuccessfully = messageAction.ExecuteAction(message, buffer, out forSave);
					if (currentFactoryId != FactoryProvider.Current._Instance)
					{
						SaveAndClearParticipants(participants, message);
						messageBatch.NumberOfSavedMessagesInBatch = i;
						notifications.Notify(new InfoNotification(Res.GetString("15d65b8f-248c-44a4-a26d-6ea5adadee83", "Branch was changed. Successfully saved {0} of {1} messages in batch.", i, messageBatch.MessagePKs.Count)));
						message = FactoryProvider.Current.Load<EDIMessage>(messageBatch.MessagePKs[i]);
						VerboseLoggingProcessingMessageInBatchMode(message);
					}

					if (!actionCompleteSuccessfully)
					{
						notifications.Notify(new WarningNotification(WarningType.Warning, ErrorPreventSaveHappenedString));
						throw new InvalidMessageContentException(ErrorPreventSaveHappenedString);
					}

					message.EM_Status = EDIMessage.Status.Received;
					AddMessageNote(message, buffer.AsString);
					SendSuccessNotificationEmail(message, messageAction, buffer.AsString);
					SendSuccessAcknowledgement(message, buffer.AsString);
					participants.AddRange(forSave.ToArray());
					AddFactoryIfNotExist(participants, message.Factory);
					VerboseLoggingProcessedMessageInBatchMode(message);
				}

				notifications.Notify(new InfoNotification(Res.GetString("f6a56fa4-7277-4693-867c-2525eecfbec5", "Processing finished. Saving changes...")));
				AddFactoryIfNotExist(participants, FactoryProvider.Current);
				VerboseLoggingParticipantFactories(participants);
				VerboseLoggingMemoryUsage();
				SaveAndClearParticipants(participants);
				VerboseLoggingMemoryUsage();
			}
		}

		void AddFactoryIfNotExist(List<ITransactionParticipant> participants, BusinessObjectFactory factory)
		{
			if (!participants.Contains(factory))
			{
				participants.Add(factory);
			}
		}

#if DEBUG
		internal virtual void ProcessOneByOne(MessageBatch messageBatch)
		{
			ProcessOneByOne(messageBatch, CancellationToken.None);
		}
#endif

		internal virtual void ProcessOneByOne(MessageBatch messageBatch, CancellationToken token)
		{
			for (int i = messageBatch.NumberOfSavedMessagesInBatch; i < messageBatch.MessagePKs.Count; i++)
			{
				token.ThrowIfCancellationRequested();
				using (FactoryProvider.Current.AddDisposableService())
				{
					try
					{
						ProcessOneAndRetry(messageBatch.MessagePKs[i], 3);
					}
					catch (DatabaseUpgradeInProgressException)
					{
						throw;
					}
					catch (SqlLockLostException)
					{
						throw;
					}
					catch (Exception ex) when (!ex.IsCriticalException() && !ex.IsOutOfDiskSpaceException() && !ex.IsUnableToCreateTempFileException())
					{
						string reportKey = "";
						string noteText = Res.GetString("73678269-6896-4047-83fc-b122aa9079a8", "Error to save message: '{0}'", ex.ToString());
						notifications.Notify(new ErrorNotification(ErrorType.Error, noteText));
						EDIMessage message = null;
						IMessageAction action = null;

						try
						{
							message = DiscardChangeAndFailMessage(messageBatch.MessagePKs[i], noteText);
							if (message != null)
							{
								action = GetMessageAction(message.EM_MessageType, message.EM_MessageSubType);
								reportKey = string.Format(CultureInfo.InvariantCulture, "MessageAction: {0}", action);
							}
						}
						catch (Exception e) when (!e.IsCriticalException())
						{
							notifications.Notify(new ErrorNotification(ErrorType.Error, e.ToString()));
						}
						finally
						{
							try
							{
								ReportError(ex, reportKey);
								FactoryProvider.CreateNewWithoutSave();
								using (FactoryProvider.Current.AddDisposableService())
								{
									SendFailureAcknowledgement(message, noteText);
									FactoryProvider.Current.Save();
								}
								SendFailNotificationEmail(message, action, noteText);
							}
							catch (Exception e) when (!e.IsCriticalException())
							{
								notifications.AddWarning(Res.GetString("7DB8BE7F-6BC3-48D7-92FA-F470ADEDA6EB", "Fallback Notification could not be sent: {0}", e.ToString()));
							}
						}
					}
				}
			}
		}

		void SendSuccessAcknowledgement(EDIMessage message, string importLog)
		{
			new InterchangeAcknowledgement().Send(FactoryProvider.Current, notifications, message, importLog, null, InterchangeAcknowledgementType.Success);
		}

		void SendFailureAcknowledgement(EDIMessage message, string importLog)
		{
			new InterchangeAcknowledgement().Send(FactoryProvider.Current, notifications, message, importLog, null, InterchangeAcknowledgementType.Failed);
		}

#if DEBUG
		internal virtual
#endif
		void ReportError(Exception ex, string key = "")
		{
			if (!ReportToEDI(ex))
			{
				return;
			}

			key = string.Format(CultureInfo.InvariantCulture, "StandardXmlProcessor.ProcessOneByOne: {0}. {1}", ex.GetType(), key);
			ErrorReporter.ReportOnce(key, ex.Message, ex);
		}

		bool ReportToEDI(Exception ex)
		{
			if (ex is ZException zException)
			{
				if (zException.ErrorContext == JobOrderHeaderSchema.Constants.Indexes.NR_UX__JD_OrderNumber_JD_OrderNumberSplit_JD_OA_BuyerAddress)
				{
					return false;
				}
			}

			return true;
		}

#if DEBUG
		internal virtual
#endif
		void ProcessOneAndRetry(ZGuid messagePK, int maxRetryTimes)
		{
			int retriedTimes = 0;
			var handledSaveException = false;
			var success = false;

			while (!success)
			{
				try
				{
					ProcessOne(messagePK);
					success = true;
				}
				catch (ZSaveException saveEx)
				{
					if (saveEx.ShouldReprocess() && retriedTimes < maxRetryTimes)
					{
						var failedAndRetry = Res.GetString("0234dbd3-ffca-4de3-b112-c6483f532a3e", "Failed, retry... Error Message : {0}", saveEx.FriendlyMessage);
						notifications.Notify(new WarningNotification(failedAndRetry));
						retriedTimes++;
					}
					else if (!handledSaveException)
					{
						HandleZSaveException(saveEx);
						handledSaveException = true;
						var handledSaveExceptionAndRetry = Res.GetString("092ee3aa-f90f-437b-8873-9a3cedd2d013", "Handled save exception, retry...");
						notifications.Notify(new WarningNotification(handledSaveExceptionAndRetry));
					}
					else
					{
						throw;
					}
				}
			}
		}

#if DEBUG
		internal virtual
#endif
		void HandleZSaveException(ZSaveException saveEx)
		{
			ZExceptionReporting.HandleSaveException(saveEx, new NotificationHandler(notifications));
		}

		class NotificationHandler : INotificationHandler
		{
			internal NotificationHandler(INotifications notifications)
			{
				this.notifications = notifications;
			}
			readonly INotifications notifications;

			public void ReportError(string message, string caption, string errorContext = null, Exception exception = null)
			{
				notifications.Notify(new WarningNotification(caption + " " + message));
			}

			public void ReportInformation(string message, string caption) { }
		}

		void ProcessOne(ZGuid messagePK)
		{
			var disposableManager = FactoryProvider.Current.GetDisposableManager();
			FactoryProvider.CreateNewWithoutSave();
			if (disposableManager != null)
			{
				disposableManager.Subscribe(FactoryProvider.Current.AddDisposableService(disposableManager));
			}

			var message = FactoryProvider.Current.Load<EDIMessage>(messagePK);
			notifications.Notify(new InfoNotification(GetProcessingMessageString(message)));
			VerboseLoggingProcessingMessageInSingleMode(message);
			var messageAction = GetMessageAction(message.EM_MessageType, message.EM_MessageSubType);

			if (messageAction == null)
			{
				string noteText = GetMessageTypeNotSupportString(message);
				DiscardChangeAndFailMessage(messagePK, noteText);
				notifications.Notify(new WarningNotification(WarningType.Warning, noteText));
				SendFailureAcknowledgement(message, noteText);
				VerboseLoggingProcessedMessageInSingleMode(message);
				return;
			}

			var buffer = new NotificationBuffer(notifications);
			List<ITransactionParticipant> forSave = new List<ITransactionParticipant>();

			long currentFactoryId = FactoryProvider.Current._Instance;
			bool actionCompleteSuccessfully = messageAction.ExecuteAction(message, buffer, out forSave);
			if (currentFactoryId != FactoryProvider.Current._Instance)
			{
				message = FactoryProvider.Current.Load<EDIMessage>(messagePK);
			}

			if (!actionCompleteSuccessfully)
			{
				var messageReloaded = DiscardChangeAndFailMessage(messagePK, buffer.AsString);
				SendFailNotificationEmail(messageReloaded, messageAction, buffer.AsString);
				notifications.Notify(new WarningNotification(WarningType.Warning, ErrorPreventSaveHappenedString));
				SendFailureAcknowledgement(message, ErrorPreventSaveHappenedString);
				VerboseLoggingProcessedMessageInSingleMode(message);
				FactoryProvider.SaveCurrentAndCreateNew();
				return;
			}

			message.EM_Status = EDIMessage.Status.Received;
			AddMessageNote(message, buffer.AsString);
			SendSuccessNotificationEmail(message, messageAction, buffer.AsString);
			SendSuccessAcknowledgement(message, buffer.AsString);
			forSave.Add(FactoryProvider.Current);
			VerboseLoggingProcessedMessageInSingleMode(message);
			SaveAndClearParticipants(forSave, message);
		}

		bool IsVerboseLoggingForMemoryProblem
		{
			get
			{
				if (!isVerboseLoggingForMemoryProblem.HasValue)
				{
					isVerboseLoggingForMemoryProblem = eHubMessagingRegistry.Instance.XMLServiceVerboseLogging.Value;
				}

				return isVerboseLoggingForMemoryProblem.Value;
			}
		}

		bool? isVerboseLoggingForMemoryProblem;

		bool IsVerboseLoggingForBatchReprocessingProblem
		{
			get
			{
				if (!isVerboseLoggingForBatchReprocessingProblem.HasValue)
				{
					isVerboseLoggingForBatchReprocessingProblem = eHubMessagingRegistry.Instance.XMLServiceVerboseLogging.Value;
				}

				return isVerboseLoggingForBatchReprocessingProblem.Value;
			}
		}

		bool? isVerboseLoggingForBatchReprocessingProblem;

#if DEBUG
		public void SetIsVerboseLoggingForMemoryProblemForTesting(bool value)
		{
			isVerboseLoggingForMemoryProblem = value;
		}

		public void SetIsVerboseLoggingForBatchReprocessingProblemForTesting(bool value)
		{
			isVerboseLoggingForBatchReprocessingProblem = value;
		}
#endif

#if DEBUG
		internal virtual
#endif
		void SaveAndClearParticipants(List<ITransactionParticipant> participants, EDIMessage message = null)
		{
			try
			{
				BusinessObjectFactory.SaveTogether(participants.ToArray());
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				if (message != null)
				{
					ErrorReporter.ReportOnce("StandardXMLProcessor.SaveAndClearParticipants",
						string.Format("MessageType: {0}\r\nMessageSubType: {1}\r\nApplicationCode: {2}", message.MessageTypeWithDescription, message.MessageSubTypeWithDescription, message.ApplicationCodeWithDescription),
						e);
				}
				throw;
			}

			participants.Clear();
		}

		EDIMessage DiscardChangeAndFailMessage(ZGuid messagePK, string noteText)
		{
			FactoryProvider.CreateNewWithoutSave();
			var factory = FactoryProvider.Current;
			using (factory.AddDisposableService())
			{
				var message = factory.Load<EDIMessage>(messagePK);
				message.EM_Status = EDIMessage.Status.Error;
				AddMessageNote(message, noteText);
				factory.Save();
				return message;
			}
		}

		void SendSuccessNotificationEmail(EDIMessage message, IMessageAction messageAction, ZString emailBodyText)
		{
			ZString subject = Res.GetString("a6f3ae91-5484-4be8-81d6-c23e716a996f", "EDI Message #{0} Type {1} processed", message.EM_MessageNum, message.EM_MessageSubType);
			messageAction.SendNotificationEmail(subject, emailBodyText, notifications, true);
		}

#if DEBUG
		internal virtual
#endif
		void SendFailNotificationEmail(EDIMessage message, IMessageAction messageAction, ZString emailBodyText)
		{
			FactoryProvider.CreateNewWithoutSave();
			using (FactoryProvider.Current.AddDisposableService())
			{
				ZString subject = Res.GetString("bc7f1633-f747-46b6-bf1d-d509126deda5", "EDI Message #{0} Type {1} rejected", message.EM_MessageNum, message.EM_MessageSubType);
				if (messageAction != null)
				{
					messageAction.SendNotificationEmail(subject, emailBodyText, notifications, false);
				}
				else
				{
					SendGeneralNotificationEmail(subject, emailBodyText);
				}
				FactoryProvider.Current.Save();
			}
		}

		void SendGeneralNotificationEmail(ZString subject, ZString body)
		{
			ZString notificationGroupRegistryPath = Res.GetString("8ee27f8a-48a4-46d8-a9c7-e2dea7a4f571", "System->Registry->Notification->XML Failure Fallback Notification Group");
			IGlbGroup notificationGroup = FactoryProvider.Current.Load<GlbGroup>(NotificationDataRegistry.Instance.XMSFailureFallBackNotificationGroup.Value);
			if (notificationGroup.IsValidForOutgoingMail(FactoryProvider.Current))
			{
				var email = new EmailDef();
				email.Subject = subject;
				email.Body = body;
				try
				{
					Env.OutgoingMailManager.Create(FactoryProvider.Current, email, notificationGroup.PK.ToGuid(), GroupSourceLocator.GetFromGroup(notificationGroup));
				}
				catch (EmailSendFailedException ex)
				{
					notifications.Notify(new WarningNotification(Res.GetString("7DB8BE7F-6BC3-48D7-92FA-F470ADEDA6EB", "Fallback Notification could not be sent: {0}", ex.Message)));
				}
			}
			else
			{
				notifications.Notify(new WarningNotification(Res.GetString("70A2CEE5-52D8-48FF-BC55-7CA5B54DB54F", "Notification group is not specified or invalid. Please check '{0}'", notificationGroupRegistryPath)));
			}
		}

		void AddMessageNote(EDIMessage message, string noteText)
		{
			message.Notes.AddNew(true, Res.GetString("a0a74553-9882-4f05-ae29-bfa1f212f17b", "Processing Log"), noteText);
		}

		string GetProcessingMessageString(EDIMessage message)
		{
			return Res.GetString("1420c2f0-4bf9-41ea-a5d2-3db678b5a1fa", "Processing message '{0}'", message.EM_MessageNum);
		}

		string GetMessageTypeNotSupportString(EDIMessage message)
		{
			return Res.GetString("cbf7f80c-077c-42f2-a0bc-b0e4b7cfd6c2", "Message Type '{0}' with Subtype '{1}' not supported.", message.EM_MessageType, message.EM_MessageSubType);
		}

		static string ErrorPreventSaveHappenedString => Res.GetString("1451ceae-e8e6-4083-842c-e7c8f7293b68", "Error(e.g. XML validation error) that prevent save happened. Message process failed.");

		void VerboseLoggingMemoryUsage()
		{
			if (IsVerboseLoggingForMemoryProblem)
			{
				long totalMemory = GC.GetTotalMemory(false);
				notifications.Notify(new InfoNotification(Res.GetString("5281163F-8BE6-4038-A101-ECE3A064A3C7", "Current memory usage: {0:D}MB.", totalMemory / (1024 * 1024))));
			}
		}

		void VerboseLoggingProcessingMessageInBatchMode(EDIMessage message)
		{
			if (IsVerboseLoggingForBatchReprocessingProblem)
			{
				notifications.Notify(new InfoNotification(Res.GetString("4F453161-475D-4888-AFD4-4640BFA5C40D", "Processing message '{0}' in status '{1}' and factory id '{2}' in batch mode.", message.EM_MessageNum, message.EM_Status, message.Factory._Instance)));
			}
		}

		void VerboseLoggingProcessingMessageInSingleMode(EDIMessage message)
		{
			if (IsVerboseLoggingForBatchReprocessingProblem)
			{
				notifications.Notify(new InfoNotification(Res.GetString("D54D9930-2CF7-43D6-ACE0-9662A731122C", "Processing message '{0}' in status '{1}' and factory id '{2}' in single mode", message.EM_MessageNum, message.EM_Status, message.Factory._Instance)));
			}
		}

		void VerboseLoggingProcessedMessageInBatchMode(EDIMessage message)
		{
			if (IsVerboseLoggingForBatchReprocessingProblem)
			{
				notifications.Notify(new InfoNotification(Res.GetString("4F453161-475D-4888-AFD4-4640BFA5C401", "Message '{0}' in status '{1}' and factory id '{2}' in batch mode was processed.", message.EM_MessageNum, message.EM_Status, message.Factory._Instance)));
			}
		}

		void VerboseLoggingProcessedMessageInSingleMode(EDIMessage message)
		{
			if (IsVerboseLoggingForBatchReprocessingProblem)
			{
				notifications.Notify(new InfoNotification(Res.GetString("D54D9930-2CF7-43D6-ACE0-9662A7311221", "Message '{0}' in status '{1}' and factory id '{2}' in single mode was processed.", message.EM_MessageNum, message.EM_Status, message.Factory._Instance)));
			}
		}

		void VerboseLoggingParticipantFactories(List<ITransactionParticipant> participants)
		{
			if (IsVerboseLoggingForBatchReprocessingProblem)
			{
				notifications.Notify(new InfoNotification(Res.GetString("08FA3F2F-845C-4BB5-BFF6-6DEC02163202", "Saving participants '{0}'. Current factory id '{1}'.", participants.Count, FactoryProvider.Current._Instance)));
			}
		}

		#endregion
	}
}
