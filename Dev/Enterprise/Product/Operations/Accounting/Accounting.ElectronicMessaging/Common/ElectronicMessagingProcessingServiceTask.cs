using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public abstract class ElectronicMessagingProcessingServiceTask : ServiceProviderImpl
	{
		protected ElectronicMessagingProcessingServiceTask(IElectronicMessagingProcessingServiceTaskDataProvider dataProvider = null)
		{
			DataProvider = dataProvider ?? new ElectronicMessagingProcessingServiceTaskDataProvider();
		}

		public abstract string CountryCode { get; }

		public IElectronicMessagingProcessingServiceTaskDataProvider DataProvider { get; }

		public override void RunTask(CancellationToken token)
		{
			ServiceLogger.Log(LogType.Debug, FormattableString.Invariant($"{TaskName} service task started."));
			try
			{
				if (!DataProvider.GetPKsOfCompaniesThatEnabledEInvoicing(CountryCode).Any())
				{
					ServiceLogger.Log(LogType.Debug, FormattableString.Invariant($"No {CountryCode} company has E-Reporting enabled in registry. {TaskName} service task completed early."));
					return;
				}

				var companyPKs = GetPKOfCompaniesThatNeedToSendElectronicMessages();
				if (!companyPKs.Any())
				{
					ServiceLogger.Log(LogType.Debug, FormattableString.Invariant($"There is no {MessageName} to send. {TaskName} service task completed early."));
					return;
				}

				foreach (var companyPK in companyPKs)
				{
					token.ThrowIfCancellationRequested();
					RunTaskCore(companyPK);
				}

				ServiceLogger.Log(LogType.Debug, FormattableString.Invariant($"{TaskName} service task completed."));
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ServiceLogger.Log(LogType.Error, FormattableString.Invariant($"{TaskName} service task ended abruptly.\r\nException: {ex.GetType()}\r\nException Message: {ex.Message}.\r\nStackTrace: {ex.StackTrace}"));
			}
		}

		protected virtual void RunTaskCore(ZGuid companyPK)
		{
			ServiceLogger.Log(LogType.Debug, FormattableString.Invariant($"Setting processing context"));
			using (SetElectronicMessageProcessingContext(companyPK))
			{
				ServiceLogger.Log(LogType.Debug, FormattableString.Invariant($"{MessageName} processing of {CurrentProcessingContext.CurrentCompany.GC_Code} started"));

				if (GetEInvoiceBatchCreator(CurrentProcessingContext.CurrentCompany).PerformBatching(CurrentProcessingContext.Logger))
				{
					GetEInvoicingDataValidator(CurrentProcessingContext.CurrentCompany)?.Run(CurrentProcessingContext.Logger);
					SendElectronicMessage();
				}

				ServiceLogger.Log(LogType.Debug, FormattableString.Invariant($"Completed {MessageName} processing of {CurrentProcessingContext.CurrentCompany.GC_Code}"));
			}
		}

		DisposableAction SetElectronicMessageProcessingContext(ZGuid companyPK)
		{
			IDisposable userContext = null;
			IDisposable logContext = null;

			Action createAction = () =>
			{
				var companyFactory = new BusinessObjectFactory();
				var currentCompany = LoadCompany(companyPK, companyFactory);
				var currentBranch = currentCompany.Branches[0];
				userContext = Env.SetTemporaryUserContext(Env.CurrentUser.PK, currentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid());
				logContext = LoggerWithPrefix.SetPrefix(currentCompany.GC_Code);

				CurrentProcessingContext = (currentCompany, LoggerWithPrefix);
			};

			Action disposeAction = () =>
			{
				userContext?.Dispose();
				logContext?.Dispose();
				CurrentProcessingContext = (null, null);
			};

			return new DisposableAction(createAction, disposeAction);
		}

		protected IReadOnlyCollection<ZGuid> GetPKOfCompaniesThatNeedToSendElectronicMessages()
		{
			var companyPKsFromQueue = DataProvider.GetPKsOfCompaniesWithQueuedTransactions(CountryCode);
			var companyPksWithEInvoicingEnabled = DataProvider.GetPKsOfCompaniesThatEnabledEInvoicing(CountryCode);
			var companyPKsWithPeriodicRequests = DataProvider.GetPKsOfCompaniesWithPeriodicRequests(CountryCode);
			var result = companyPKsFromQueue.Union(companyPKsWithPeriodicRequests).Intersect(companyPksWithEInvoicingEnabled).Distinct().ToList();

			return result;
		}

		void SendElectronicMessage()
		{
			var errorDetected = false;
			try
			{
				CurrentProcessingContext.Logger.Log(LogType.Debug, "Generate EDI Interchange sub task started.");
				GetInterchangeCreator(CurrentProcessingContext.CurrentCompany)?.Process(CurrentProcessingContext.Logger);
			}
			catch (Exception ex)
			{
				errorDetected = true;
				CurrentProcessingContext.Logger.Log(LogType.Error, string.Format(CultureInfo.InvariantCulture, "Generate EDI Interchange sub task failed: \r\nException: {0}\r\n: Exception Message: {1}.", ex.GetType(), ex.Message));
				throw;
			}
			finally
			{
				if (errorDetected)
				{
					CurrentProcessingContext.Logger.Log(LogType.Error, string.Format(CultureInfo.InvariantCulture, "Generate EDI Interchange sub task completed with error."));
				}
				else
				{
					CurrentProcessingContext.Logger.Log(LogType.Debug, string.Format(CultureInfo.InvariantCulture, "Generate EDI Interchange sub task completed."));
				}
			}
		}

		protected abstract EInvoicingBatchCreatorBase GetEInvoiceBatchCreator(GlbCompany company);

		protected abstract EDIInterchangeCreatorForEInvoicingBatchBase GetInterchangeCreator(GlbCompany company);

		protected abstract BaseEInvoicingDataValidator GetEInvoicingDataValidator(GlbCompany company);

		public abstract ZString MessageName { get; }

		public abstract ZString TaskName { get; }

		protected (GlbCompany CurrentCompany, ILogger Logger) CurrentProcessingContext
		{
			get { return currentProcessingContext; }
			private set
			{
				bool isClearingContext = (value.CurrentCompany == null && value.Logger == null);
				if (isClearingContext || currentProcessingContext.CurrentCompany == null && currentProcessingContext.Logger == null)
				{
					currentProcessingContext = value;
				}
				else
				{
					throw new InvalidOperationException("Trying to set a new processing context without disposing the current one. Please call SetElectronicMessageProcessingContext(ZGuid companyPK) along inside 'Using' to set and dispose a processing context");
				}
			}
		}
		(GlbCompany CurrentCompany, ILogger Logger) currentProcessingContext;

		LoggerWithPredefinedPrefix LoggerWithPrefix => loggerWithPrefix ?? (loggerWithPrefix = new LoggerWithPredefinedPrefix(ServiceLogger));
		LoggerWithPredefinedPrefix loggerWithPrefix;

		GlbCompany LoadCompany(ZGuid companyPk, BusinessObjectFactory factory) => factory.Load<GlbCompany>(companyPk);
	}
}
