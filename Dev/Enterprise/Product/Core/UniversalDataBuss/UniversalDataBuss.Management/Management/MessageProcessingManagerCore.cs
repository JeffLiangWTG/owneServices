using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.UniversalDataBuss.Management
{
	public interface IMessageProcessingManager
	{
		IXmlSessionTracker Logger { get; }
		IDataWritingManager OutboundSessionTracker { set; }
		IXmlSessionTracker Process(IEDIMessage message, ITopLevelDataObject topLevelDataObject = null, bool recordBillingInformation = true, ICodeMappingManager mapper = null, IDelayedTransactionManager transaction = null);
	}

	public sealed class MessageProcessingManagerCore : IMessageProcessingManager
	{
		readonly XmlSessionTracker logger;
		readonly GetObjectFactoryLocatorFromSubType factoryLocator;
		readonly GetMessageProcessorFromSubType messageProcessorFactory;

		public delegate IDataMessageProcessor GetMessageProcessorFromSubType(string messageSubType);
		public delegate IEDIMessageUniversalObjectFactoryLocator GetObjectFactoryLocatorFromSubType(string messageSubType);
		public delegate MessageStatus ProcessMessageFunc();

		public MessageProcessingManagerCore(
			XmlSessionTracker logger,
			GetMessageProcessorFromSubType messageProcessorFactory,
			GetObjectFactoryLocatorFromSubType factoryLocator)
		{
			this.logger = Argument.NotNull(logger, "logger");
			this.factoryLocator = Argument.NotNull(factoryLocator, "factoryService");
			this.messageProcessorFactory = Argument.NotNull(messageProcessorFactory, "messageProcessorFactory");
		}

		public IXmlSessionTracker Logger
		{
			get { return logger; }
		}

		public IDataWritingManager OutboundSessionTracker
		{
			set { logger.OutboundSessionTracker = value; }
		}

		public IXmlSessionTracker Process(IEDIMessage message,ITopLevelDataObject topLevelDataObject = null, bool recordBillingInformation = true, ICodeMappingManager mapper = null, IDelayedTransactionManager transaction = null)
		{
			Argument.NotNull(message, "message");
			var factory = (UniversalObjectFactory)factoryLocator(message.EM_MessageSubType).GetFactory(message, logger);
			if (message.Factory.TryGetDisposableManager(out var disposableManager))
			{
				factory.BOFactory.AddDisposableService(disposableManager);
			}
			ProcessMessageFunc processMessageFunc = () => messageProcessorFactory(message.EM_MessageSubType).Process(factory, message, logger, topLevelDataObject, recordBillingInformation, mapper);
			ProcessMessage(message, logger, processMessageFunc, transaction);
			return logger;
		}

		internal static MessageStatus Process(
			IUniversalObjectFactory universalObjectFactory,
			IEDIMessage message,
			IXmlSessionTracker xmlSessionTracker,
			ITopLevelDataObject dataObject,
			ITopLevelDataObjectProcessor dataMessageProcessor,
			bool recordBillingInformation,
			ICodeMappingManager codeMapper,
			IDelayedTransactionManager transaction)
		{
			ProcessMessageFunc processMessageFunc = () => message.Process(universalObjectFactory, xmlSessionTracker, dataObject, dataMessageProcessor, recordBillingInformation, codeMapper);
			return ProcessMessage(message, xmlSessionTracker, processMessageFunc, transaction, universalObjectFactory);
		}

		static MessageStatus ProcessMessage(IEDIMessage message, IXmlSessionTracker logger, ProcessMessageFunc processMessageFunc, IDelayedTransactionManager transaction = null, IUniversalObjectFactory objectFactory = null)
		{
			var xmlSessionTracker = (XmlSessionTracker)logger;
			MessageStatus messageStatus;

			try
			{
				xmlSessionTracker.SourceMessage = message;
				messageStatus = processMessageFunc();
				UpdateMessage(message, messageStatus, xmlSessionTracker, transaction);
				if (objectFactory is UniversalObjectFactory universalObjectFactory && universalObjectFactory.ShouldLogDelayedSaveResults())
				{
					LogDelayedSaveResults(universalObjectFactory, xmlSessionTracker, message);
				}
			}
			catch (MessageProcessingBusinessFailureException e)
			{
				throw new MessageProcessingBusinessFailureException(e.Message, e.Caption, e.ShouldRetry, xmlSessionTracker.ToString(), e);
			}
			catch (ZCannotSaveException e)
			{
				throw new MessageProcessingBusinessFailureException(e.Message, "", false, xmlSessionTracker.ToString(), e);
			}
			catch (Exception ex) when (IsSQLTimeoutOrDeadlockException(ex))
			{
				throw new MessageProcessingBusinessFailureException(ex.Message, "", true, xmlSessionTracker.ToString(), ex);
			}
			finally
			{
				if (message.EM_ReceiveTransmit != ReceiveTransmitList.Codes.Internal)
				{
					NotifyUsersAboutImportResults(message, xmlSessionTracker);
				}
			}

			FinalizeMessage(message, xmlSessionTracker);
			return messageStatus;
		}

		internal static void FinalizeMessage(IEDIMessage message, XmlSessionTracker xmlSessionTracker)
		{
			ObjectFactory.New<IEDIMessageDataImportNoteCreator>().AddNew(message, noteStream => xmlSessionTracker.WriteTo(noteStream));
			SendAcknowledgement(message, xmlSessionTracker);
		}

		internal static void UpdateMessage(IEDIMessage message, MessageStatus result, IXmlSessionTracker xmlSessionTracker, IDelayedTransactionManager transaction = null)
		{
			switch (result)
			{
				case MessageStatus.Rejected:
					{
						xmlSessionTracker.Log(LogType.Information, Res.GetString("519c73eb-e07f-47d0-81d0-8c2b87301a7f", "Message Rejected."));
						message.EM_Status = EDIMessageStatusList.Codes.Rejected;
					}
					break;
				case MessageStatus.Discarded:
					{
						xmlSessionTracker.Log(LogType.Information, Res.GetString("3dba6c9c-8a6a-4bc9-9ca8-601399aeeb80", "Message Discarded."));
						message.EM_Status = EDIMessageStatusList.Codes.Discarded;
					}
					break;
				case MessageStatus.Processed:
					{
						if (transaction != null && transaction.IsRollingback)
						{
							xmlSessionTracker.Log(LogType.Error, Res.GetString("91ec5ade-d448-4a21-b0fe-43186e88a58f", "Transaction has been rolled back in the server."));
							message.EM_Status = EDIMessageStatusList.Codes.Rejected;
							transaction.ReportInvalidRollbackExceptionHandling();
						}
						else if (xmlSessionTracker.HasErrors)
						{
							message.EM_Status = EDIMessageStatusList.Codes.Error;
						}
						else if (xmlSessionTracker.HasWarnings)
						{
							message.EM_Status = EDIMessageStatusList.Codes.Warning;
						}
						else
						{
							message.EM_Status = EDIMessageStatusList.Codes.ProcessedOK;
						}
					}
					break;
				case MessageStatus.Linked:
					message.EM_Status = EDIMessageStatusList.Codes.Linked;
					break;
				default:
					message.EM_Status = EDIMessageStatusList.Codes.ProcessedOK;
					break;
			}
		}

		static void LogDelayedSaveResults(UniversalObjectFactory factory, XmlSessionTracker xmlSessionTracker, IEDIMessage message)
		{
			if (message.ShouldSaveResultsFromUniversalXmlProcessing())
			{
				factory.LogSaveResults(LogType.Information, Res.GetString("6F0D54E6-3863-4469-BF3A-7730DD9AD9E4", "Successfully saved"), xmlSessionTracker);
			}
			else
			{
				factory.LogSaveResults(LogType.Error, Res.GetString("E7779583-2D6C-4DFE-939F-4668B9BA7CFC", "Error saving"), xmlSessionTracker);
			}
		}

		static void NotifyUsersAboutImportResults(IEDIMessage message, XmlSessionTracker xmlSessionTracker)
		{
			new ImportResultNotifier(message, xmlSessionTracker).Notify();
		}

		static bool IsSQLTimeoutOrDeadlockException(Exception e)
		{
#if NETFRAMEWORK
			return e.IsExceptionPresentIncludingInner<SqlException>(s => s.IsTimeoutExpired() || s.IsInnermostDeadlock() || s.IsLockTimeoutExpired());
#elif NET
			// For Microsoft.Data.SqlClient.SqlException, there is no direct way to determine IsTimeoutExpired and IsLockTimeoutExpired,
			// but we can use the number of SqlException to make a disguised judgment:
			// "s.Number == -2 || s.Number == 3617" is equal to IsTimeoutExpired
			// "s.Number == 1222 || s.Number == 5245" is equal to IsLockTimeoutExpired
			return e.IsExceptionPresentIncludingInner<SqlException>(s => (s.Number == -2 || s.Number == 3617) || s.IsInnermostDeadlock() || (s.Number == 1222 || s.Number == 5245));
#else
#error Unexpected target platform
#endif
		}

		#region SendAcknowledgement

		static void SendAcknowledgement(IEDIMessage message, XmlSessionTracker xmlSessionTracker)
		{
			var factory = message.Factory;
			var notificationsAdapter = new NotificationsAdapter(xmlSessionTracker);
			var interchangeAcknowledgement = ObjectFactory.Get<IInterchangeAcknowledgement>();

			if (IsMessageProcessSuccess(message.EM_Status))
			{
				interchangeAcknowledgement.Send(factory, notificationsAdapter, message, xmlSessionTracker.ToString(), xmlSessionTracker.ValidationRuleCollection, InterchangeAcknowledgementType.Success);
			}
			else
			{
				interchangeAcknowledgement.Send(factory, notificationsAdapter, message, xmlSessionTracker.ToString(), xmlSessionTracker.ValidationRuleCollection, InterchangeAcknowledgementType.Failed);
			}
		}

		static bool IsMessageProcessSuccess(string messageStatus)
		{
			return messageStatus == EDIMessageStatusList.Codes.ProcessedOK || messageStatus == EDIMessageStatusList.Codes.Warning;
		}

		#endregion
	}
}
