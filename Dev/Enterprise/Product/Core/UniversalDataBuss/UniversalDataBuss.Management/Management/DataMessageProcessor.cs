using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;
using CargoWise.Common;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.CodeMapping;

namespace Enterprise.UniversalDataBuss.Management
{
	class EDIMessageUniversalObjectFactoryLocator : IEDIMessageUniversalObjectFactoryLocator
	{
		readonly IUniversalObjectFactory universalObjectFactory;

		public EDIMessageUniversalObjectFactoryLocator(IUniversalObjectFactory universalObjectFactory)
		{
			this.universalObjectFactory = universalObjectFactory;
		}

		public IUniversalObjectFactory GetFactory(IEDIMessage message, IXmlSessionTracker logger)
		{
			if (universalObjectFactory != null)
			{
				return universalObjectFactory;
			}
			else
			{
				var factory = new UniversalObjectFactory();
				factory.SetDataRefresh(message.Factory.RefreshEnabled);
				return factory;
			}
		}
	}

	class EDIMessageUniversalObjectFactoryLocatorForUniversalEvent : IEDIMessageUniversalObjectFactoryLocator
	{
		readonly IUniversalObjectFactory universalObjectFactory;

		public EDIMessageUniversalObjectFactoryLocatorForUniversalEvent(IUniversalObjectFactory universalObjectFactory)
		{
			this.universalObjectFactory = universalObjectFactory;
		}

		public IUniversalObjectFactory GetFactory(IEDIMessage message, IXmlSessionTracker logger)
		{
			return universalObjectFactory ?? new UniversalObjectFactory(message.Factory);
		}
	}

	class TopLevelDataObjectFactory<T> : ITopLevelDataObjectFactory where T : TopLevelDataObject, new()
	{
		public bool TryGetTopLevelDataObject(IEDIMessage message, ICodeMappingManager codeMapper, IXmlSessionTracker logger, out ITopLevelDataObject topLevelDataObject)
		{
			topLevelDataObject = null;

			try
			{
				topLevelDataObject = message.GetEM_MessageTextReader().Parse<T>(logger, codeMapper);
				topLevelDataObject?.SetWriterStrategy(DefaultDataObjectWriterStrategy.Instance);

				if (logger.Logs.Any(log => log.Type == LogType.Error))
				{
					logger.LogErrorToServiceTaskOnly(Res.GetString("a2fbf567-d217-42a1-a486-2a4784c4c918", "Failed to parse XML. Errors found:-\r\n{0}", logger.ToString()));
					return false;
				}

				((TopLevelDataObject)topLevelDataObject)?.SetCodesMappedToTarget();
				return true;
			}
			catch (XmlException xmlException)
			{
				logger.LogBoth(LogType.Error, Res.GetString("4c1a1d68-ff24-4d69-be80-0981f9d767ab", "Could not parse XML:\r\n\r\n{0}\r\n\r\n{1}", message.EM_MessageText, xmlException));
				return false;
			}
		}
	}

	class UserContextScopeManager : IUserContextScopeManager
	{
		public IDisposable EnterUserContext(
			IUserContext userContext,
			[CallerFilePath] string callerFilePath = "",
			[CallerMemberName] string callerMemberName = "",
			[CallerLineNumber] int callerLineNumber = -1)
		{
			var originalUser = Env.CurrentUserPK;
			var originalBranch = Env.CurrentBranchPK;
			var originalDepartment = Env.CurrentDepartmentPK;

			bool IsSameContext()
			{
				return originalUser == Env.CurrentUserPK &&
				originalBranch == Env.CurrentBranchPK &&
				originalDepartment == Env.CurrentDepartmentPK;
			}

			try
			{
				using (Env.Instance.SuppressSwitchContextCheck(ensureContextIsRestoredAfterSuppression: false))
				{
					var result = Env.Instance.SetTemporaryUserContext(userContext, callerFilePath, callerMemberName, callerLineNumber);
					if (result == null && !IsSameContext())
					{
						ErrorReporter.ReportOnce("User context changed, but no disposable was returned. What?");
					}
					return result;
				}
			}
			catch (Exception ex)
			{
				if (!IsSameContext())
				{
					ErrorReporter.ReportOnce("Error in context switching, and the user context was changed.", ex);
				}
				throw;
			}
		}
	}

	class DataMessageProcessor<T> : IMessageKeyProvider, IDataMessageProcessor where T : TopLevelDataObject, new()
	{
		readonly ITopLevelDataObjectProcessor dataMessageProcessor;
		readonly ITopLevelDataObjectFactory topLevelDataObjectFactory;
		readonly IUserContextScopeManager userContextScopeManager;
		readonly IUserContextExtractor userContextExtractor;

		public DataMessageProcessor(
			ITopLevelDataObjectProcessor dataMessageProcessor,
			ITopLevelDataObjectFactory topLevelDataObjectFactory,
			IUserContextScopeManager userContextScopeManager,
			IUserContextExtractor userContextExtractor)
		{
			this.dataMessageProcessor = dataMessageProcessor;
			this.topLevelDataObjectFactory = topLevelDataObjectFactory;
			this.userContextScopeManager = userContextScopeManager;
			this.userContextExtractor =	userContextExtractor;
		}

		public MessageStatus Process(
			IUniversalObjectFactory universalObjectFactory,
			IEDIMessage message,
			IXmlSessionTracker logger,
			ITopLevelDataObject topLevelDataObject = null,
			bool recordBillingInformation = true,
			ICodeMappingManager codeMapper = null)
		{
			var xmlSessionTracker = (XmlSessionTracker)logger;
			codeMapper = codeMapper ?? new CodeMappingManager(xmlSessionTracker);
			var dataObject = topLevelDataObject as T;

			if (dataObject == null)
			{
				if (topLevelDataObjectFactory.TryGetTopLevelDataObject(message, codeMapper, xmlSessionTracker, out topLevelDataObject))
				{
					dataObject = (T)topLevelDataObject;
				}
				else
				{
					return MessageStatus.Rejected;
				}
			}

			using (dataObject)
			{
				bool AbleToGetUserContext(out IUserContext userContext1)
				{
					userContext1 = xmlSessionTracker?.SessionUserContext;
					return userContext1 != null || userContextExtractor.TryGetUserContext(message, dataObject, xmlSessionTracker, out userContext1);
				}

				dataMessageProcessor.ValidateDataObject(dataObject, xmlSessionTracker);

				if (!AbleToGetUserContext(out var userContext))
				{
					return MessageStatus.Rejected;
				}

				var factory = ((UniversalObjectFactory)universalObjectFactory).BOFactory;
				var workflowContextManager = factory.ServiceContainer.GetService<WorkflowUserContextManager>();

				// This context switch is still here as there are still bad saves inside here.
				// When this save is moved tests will fail and this context switch will need to move with it
				var contextSwitchLogger = eAdaptorRegistry.Instance.MessageUserContextTracingEnabled.Value ? new UserContextSwitchLogger() : null;
				using (contextSwitchLogger != null ? Env.StartContextSwitchTrace(contextSwitchLogger) : null)
				using (userContextScopeManager.EnterUserContext(userContext))
				using (workflowContextManager == null ? factory.ServiceContainer.AddService(new WorkflowUserContextManager()).SetWorkflowUserContext(Env.CurrentUserContext, contextSwitchLogger) : null)
				{
					return message.Process(universalObjectFactory, xmlSessionTracker, dataObject, dataMessageProcessor, recordBillingInformation, codeMapper);
				}
			}
		}

		public MessageKeyProviderResult GetKeysForBlockingParallelImport(
			IUniversalObjectFactory universalObjectFactory,
			IEDIMessage message,
			IXmlSessionTracker logger,
			ITopLevelDataObject topLevelDataObject = null,
			bool recordBillingInformation = true,
			ICodeMappingManager codeMapper = null)
		{
			var xmlSessionTracker = (XmlSessionTracker)logger;
			codeMapper = codeMapper ?? new CodeMappingManager(xmlSessionTracker);
			var dataObject = topLevelDataObject as T;

			if (dataObject == null)
			{
				if (topLevelDataObjectFactory.TryGetTopLevelDataObject(message, codeMapper, xmlSessionTracker, out topLevelDataObject))
				{
					dataObject = (T)topLevelDataObject;
				}
				else
				{
					return new MessageKeyProviderResult(MessageStatus.Rejected);
				}
			}

			using (dataObject)
			{
				dataMessageProcessor.ValidateDataObject(dataObject, xmlSessionTracker);
				dataObject.SetCodesMappedToTarget();

				if (!userContextExtractor.TryGetUserContext(message, dataObject, xmlSessionTracker, out var userContext))
				{
					return new MessageKeyProviderResult(MessageStatus.Rejected);
				}

				xmlSessionTracker.TopLevelDataObject = dataObject;

				try
				{
					codeMapper.UpdateMappedCodes(dataObject, message.Factory);
					return dataMessageProcessor.GetKeys(message, dataObject, universalObjectFactory, xmlSessionTracker);
				}
				catch (DataObjectReadFailureException exception)
				{
					xmlSessionTracker.LogBoth(LogType.Error, exception.Message);
					xmlSessionTracker.LogBoth(LogType.Information, Res.GetString("9d6a4fde-58e6-442d-b934-12f6f17f23a8", "No changes were made due to the above errors. Please fix the errors and try again."));
					return new MessageKeyProviderResult(MessageStatus.Rejected);
				}
				catch (MessageProcessingBusinessFailureException exception)
				{
					if (!exception.ShouldRetry || message.EM_RetryCount >= 2) // This will evaluate to 3 retries. Log notes are added after the exception is thrown.
					{
						xmlSessionTracker.LogBoth(LogType.Error, Res.GetString("23F19B64-DF4D-4228-87D0-DA612F19CA17", "Could not import XML:\r\n{0}", exception.Message));
						return new MessageKeyProviderResult(MessageStatus.Rejected);
					}
					throw;
				}
			}
		}
	}

	static class MessageProcessingExtensions
	{
		public static MessageStatus Process(
			this IEDIMessage message,
			IUniversalObjectFactory universalObjectFactory,
			IXmlSessionTracker xmlSessionTracker,
			ITopLevelDataObject dataObject,
			ITopLevelDataObjectProcessor dataMessageProcessor,
			bool recordBillingInformation,
			ICodeMappingManager codeMapper)
		{
			try
			{
				((XmlSessionTracker)xmlSessionTracker).TopLevelDataObject = dataObject;
				codeMapper.UpdateMappedCodes(dataObject, message.Factory);

				return dataMessageProcessor.ProcessDataObject(message, dataObject, universalObjectFactory, xmlSessionTracker);
			}
			catch (DataObjectReadFailureException exception)
			{
				xmlSessionTracker.LogBoth(LogType.Error, exception.Message);
				xmlSessionTracker.LogBoth(LogType.Information, Res.GetString("9d6a4fde-58e6-442d-b934-12f6f17f23a8", "No changes were made due to the above errors. Please fix the errors and try again."));
				return MessageStatus.Rejected;
			}
		}
	}
}
