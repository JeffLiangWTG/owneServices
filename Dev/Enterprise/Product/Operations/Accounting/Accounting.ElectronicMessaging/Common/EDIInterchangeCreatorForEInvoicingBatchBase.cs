using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Billing.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	#region SuppressResourceStringsCheckRegion

	public abstract class EDIInterchangeCreatorForEInvoicingBatchBase : IEDIInterchangeCreator
	{
		protected EDIInterchangeCreatorForEInvoicingBatchBase(GlbCompany company)
		{
			Argument.NotNull(company, "company");
			CurrentCompany = company;
		}

		#region Properties

		protected virtual bool IsBillingSupported => false;

		public ZString EInvoicingServicePoint => GetEInvoicingServicePoint();

		public IEDICommunicationsMode CommunicationsMode => GetDefaultCommunicationsMode();

		protected GlbCompany CurrentCompany { get; }

		#endregion

		#region Functions

		public void Process(ILogger logger)
		{
			var batches = GetBatchPKsThatAreReadyToBeSent(); //find all the ready to send batches
			if (batches.Any())
			{
				logger.Log(LogType.Debug, string.Format(CultureInfo.InvariantCulture, "{0} batch(es) are ready to be sent.", batches.Count));

				batches.ForEach(batchPK => ProcessSingleBatch(batchPK, logger));
			}
			else
			{
				logger.Log(LogType.Debug, string.Format(CultureInfo.InvariantCulture, "There is no 'ready to send' batch at the moment."));
			}
		}

		protected virtual BusinessObjectFactory GetNewFactoryForBatch()
		{
			return new BusinessObjectFactory
			{
				NameForDebugging = "EDIInterchangeQueuerFactory"
			};
		}

		protected virtual ZGuid[] LoadEInvoicingPKsToBeSent(AccEInvoicingBatch batch)
		{
			return batch.LoadEInvoicingPKsToBeSent(ParentTableCode);
		}

		void ProcessSingleBatch(ZGuid batchPK, ILogger logger)
		{
			Argument.NotNull(logger, "logger");

			var batchFactory = GetNewFactoryForBatch(); // need to separate batch and edi interchange factory, because sometime we can save batch only (i.e. update state to BER) without saving edi messages.
			var factories = new HashSet<ITransactionParticipant>();
			var batch = batchFactory.Load<AccEInvoicingBatch>(batchPK);

			logger.Log(LogType.Debug, string.Format(CultureInfo.InvariantCulture, "Processing batch {0}", batch.AIB_BatchNumber));
			if (batch != null)
			{
				using (var disposeManager = new DisposableManager())
				using (SetPrefix(batch.AIB_BatchNumber.ToString()))
				{
					batchFactory.AddDisposableService(disposeManager);
					TransactionBatchProcessContext batchProcessContext = null;
					try
					{
						logger.Log(LogType.Debug, string.Format(CultureInfo.InvariantCulture, "Verifying whether any EDI message needs to be created"));

						var ediInterchangeFactory = GetNewFactoryForBatch(); // need independent ediMessage factory for each batch, it's possible some ediMessage creation may fail, so we only want to save the successful ones.
						ediInterchangeFactory.AddDisposableService(disposeManager);
						var transactionPKsReadyToBeSent = LoadEInvoicingPKsToBeSent(batch);
						batchProcessContext = GetTransactionBatchProcessContext(ediInterchangeFactory, batch, transactionPKsReadyToBeSent, logger);
						var eInvoicingName = ParentTableCode == AccTransactionHeaderSchema.Constants.Prefix ? "invoice" : "compliance document";
						if (transactionPKsReadyToBeSent.Any()
							|| (!batch.AIB_GovernmentAllocatedNumber.IsEmpty
								&& batch.AIB_GovernmentAllocatedNumber.EqualsIgnoringCase(GlobalEInvoicingObjectFactory.GetICountryEInvoicingObjectFactory(batch.Company.GC_RN_NKCountryCode)?.ApTransactionListRequestBatchId ?? string.Empty)))
						{
							logger.Log(LogType.Debug, string.Format(CultureInfo.InvariantCulture, "There are {0} {1}(s) in batch [{2}] which are ready to be sent", transactionPKsReadyToBeSent.Length, eInvoicingName, batch.AIB_BatchNumber));

							factories.Add(batchFactory);

							QueueTransactionBatchForDelivery(batchProcessContext);

							factories.Add(ediInterchangeFactory);
							SaveChanges(batchProcessContext, factories, logger);
						}
						else
						{
							var msg = string.Format(CultureInfo.InvariantCulture, "There is no {0} in batch [{1}] that is ready to be sent. Therefore, no EDI message can be created.", eInvoicingName, batch.AIB_BatchNumber);
							logger.Log(LogType.Information, msg);
						}
					}
					catch (Exception exception) when (!exception.IsCriticalException())
					{
						HandleException(batchProcessContext, batch, logger, exception);
						SaveChanges(batchProcessContext, factories, logger);
					}
				}
			}

			DisposableAction SetPrefix(string prefix)
			{
				var loggerWithPrefix = logger as LoggerWithPredefinedPrefix;
				return loggerWithPrefix != null ? loggerWithPrefix.SetPrefix(prefix) : new DisposableAction(() => { });
			}
		}

		void QueueTransactionBatchForDelivery(TransactionBatchProcessContext batchProcessContext)
		{
			PerformBeforeCreatingEDIMessageAndInterchange(batchProcessContext);

			batchProcessContext.Logger.Log(LogType.Debug, string.Format(CultureInfo.InvariantCulture, "Starting EDI message creation process."));

			CreateEDIMessageAndInterchange(batchProcessContext);

			batchProcessContext.Logger.Log(LogType.Debug, string.Format(CultureInfo.InvariantCulture, "Finished EDI message creation."));

			PerformAfterCreatingEDIMessageAndInterchangeSuccessfully(batchProcessContext);

			batchProcessContext.Logger.Log(LogType.Debug, GetConcludingMessageAfterBatchProcessingIsComplete(batchProcessContext));
		}

		protected virtual void PerformBeforeCreatingEDIMessageAndInterchange(TransactionBatchProcessContext batchProcessContext) { }

		void CreateEDIMessageAndInterchange(TransactionBatchProcessContext batchProcessContext)
		{
			var notifications = new Logger();

			CreateEDIMessageAndInterchangeCore(batchProcessContext, notifications);

			if (notifications.HasErrors)
			{
				throw new InvalidOperationException(notifications.ToString().Trim(), null);
			}
		}

		protected abstract void CreateEDIMessageAndInterchangeCore(TransactionBatchProcessContext batchProcessContext, INotifications notifications);

		protected virtual void PerformAfterCreatingEDIMessageAndInterchangeSuccessfully(TransactionBatchProcessContext batchProcessContext)
		{
			var warningMessage = RetrieveWarningMessage(batchProcessContext);
			if (!warningMessage.IsEmpty)
			{
				warningMessage = "WARNING: " + warningMessage; // Message prefix
				warningMessage = EventProcessorHelper.CheckLengthAndTruncIfNeeded(batchProcessContext.Batch.Company, warningMessage);
			}

			batchProcessContext.Batch.MarkBatchAndTransactionPivotsAsSent(batchProcessContext.TransactionPKsReadyToSend, warningMessage);
		}

		void HandleException(TransactionBatchProcessContext batchProcessContext, AccEInvoicingBatch batch, ILogger logger, Exception ex)
		{
			var handler = new EDIInterchangeNotificationHandler();
			if (ex is ZSaveException)
			{
				try
				{
					ZExceptionReporting.HandleSaveException(ex, handler);
				}
				catch (Exception exception) when (!exception.IsCriticalException())
				{
					//Errors are recorded by EDIInterchangeNotificationHandler and will be properly logged.
				}
			}

			if (!handler.ErrorMessages.Any() && handler.InfoMessages.Any())
			{
				logger.Log(LogType.Debug, new ZStringBuilder(handler.InfoMessages).ToStringWithNewLineBetweenAppends());
			}
			else
			{
				var logMessage = handler.ErrorMessages.Any() ?
							new ZStringBuilder(handler.ErrorMessages).ToStringWithNewLineBetweenAppends() :
							FormattableString.Invariant($"Error : [{ex.Message}]\r\n StackTrace:\r\n {ex.StackTrace}");

				logger.Log(LogType.Error, FormattableString.Invariant($"Failed to process batch [{batch.AIB_BatchNumber}] of company [{batch.Company.GC_Code}] (organization proxy [{batch.Company.OrgProxy?.OH_Code}]).\r\n {logMessage}"));

				if (batchProcessContext != null)
				{
					PerformIfBatchProcessingFails(batchProcessContext, ex);
				}
			}
		}

		protected virtual void PerformIfBatchProcessingFails(TransactionBatchProcessContext batchProcessContext, Exception ex)
		{
			if (batchProcessContext != null && ex != null && !batchProcessContext.IsBatchChangesSavedInDB)
			{
				batchProcessContext.Batch.UpdateBatchAndPivotStatusAndErrorDescription(batchProcessContext.TransactionPKsReadyToSend, ex.Message, EInvoicingPivotState.BatchedWithError);
			}
		}

		void SaveChanges(TransactionBatchProcessContext batchProcessContext, HashSet<ITransactionParticipant> factories, ILogger logger)
		{
			if (factories.Any() && !batchProcessContext.IsBatchChangesSavedInDB)
			{
				logger.Log(LogType.Debug, string.Format(CultureInfo.InvariantCulture, "Attempting to save all changes in database"));

#if DEBUG
				if (Globals.IsTest && AdditionalActionDuringSave_ForTestOnly != null)
				{
					AdditionalActionDuringSave_ForTestOnly.Invoke(null, factories);
					AdditionalActionDuringSave_ForTestOnly = null;
				}
#endif

				BusinessObjectFactory.SaveTogether(factories.ToArray());

				if (batchProcessContext.BillingTransactions.Any())
				{
					new BillingManager().AddTransactions(batchProcessContext.BillingTransactions, CargoWise.Data.Db.Connection);
				}
				batchProcessContext.IsBatchChangesSavedInDB = true;
				logger.Log(LogType.Debug, string.Format(CultureInfo.InvariantCulture, "Successfully saved. Processing is complete."));

				OnBatchChangesSaved(batchProcessContext);
			}
		}

#if DEBUG
		public event EventHandler<HashSet<ITransactionParticipant>> AdditionalActionDuringSave_ForTestOnly;
#endif

		protected virtual void OnBatchChangesSaved(TransactionBatchProcessContext batchProcessContext)
		{
		}

		protected virtual ZString RetrieveWarningMessage(TransactionBatchProcessContext batchProcessContext)
		{
			return ZString.Empty;
		}

		protected virtual string GetConcludingMessageAfterBatchProcessingIsComplete(TransactionBatchProcessContext batchProcessContext)
		{
			return string.Format(CultureInfo.InvariantCulture, "Universal Transaction batch [{0}] of company [{1}] (organization proxy [{2}]) has been successfully queued for delivery.",
				batchProcessContext.Batch.AIB_BatchNumber,
				batchProcessContext.Batch.Company.GC_Code,
				batchProcessContext.Batch.Company.OrgProxy?.OH_Code);
		}

		List<ZGuid> GetBatchPKsThatAreReadyToBeSent()
		{
			var query = GetCandidateBatchQuery();
			var sql = FormattableString.Invariant($"SELECT {AccEInvoicingBatchSchema.PK.Name} FROM {AccEInvoicingBatchSchema.Constants.SqlSchemaName}.{AccEInvoicingBatchSchema.Constants.TableName} {query.GetAsWhereAndOrderByClause(false)}");
			var batchPKs = new DynamicBusinessObjectCollection(new BusinessObjectFactory());
			batchPKs.Load(sql, query.Params);

			return batchPKs.Any() ? batchPKs.Select(obj => new ZGuid(obj[AccEInvoicingBatchSchema.PK])).ToList() : new List<ZGuid>();
		}

		protected ZQuery GetCandidateBatchQuery()
		{
			var query = new ZQuery(AccEInvoicingBatchSchema.AIB_Status, EInvoicingBatchState.Ready);
			query.AddToFilter(AccEInvoicingBatchSchema.AIB_GC, CurrentCompany.PK);
			query.OrderBy = AccEInvoicingBatchSchema.AIB_BatchNumber.Name + " ASC";
			return query;
		}

		protected bool ShouldEInvoiceBatchWithErrorBeSent => AccountingConfigurationRegistry.Instance.AllowSendingEInvoicingBatchWithError.Value;

		protected abstract ZString GetEInvoicingServicePoint();

		protected abstract IEDICommunicationsMode GetDefaultCommunicationsMode();

		protected abstract string ParentTableCode { get; }

		protected virtual TransactionBatchProcessContext GetTransactionBatchProcessContext(BusinessObjectFactory ediInterchangeCreationFactory, AccEInvoicingBatch batch, IEnumerable<ZGuid> transactionPKsReadyToSend, ILogger logger)
			=> new TransactionBatchProcessContext(ediInterchangeCreationFactory, batch, transactionPKsReadyToSend.ToList(), logger);

		protected virtual bool AddErrorToEDIMessageNotesIfAny => false;

		#endregion

		#region Inner Class

		public class TransactionBatchProcessContext
		{
			public TransactionBatchProcessContext(BusinessObjectFactory ediInterchangeCreationFactory, AccEInvoicingBatch batch, List<ZGuid> transactionPKsReadyToSend, ILogger logger)
			{
				Argument.NotNull(ediInterchangeCreationFactory, "ediInterchangeCreationFactory");
				Argument.NotNull(batch, "batch");

				EDIInterchangeCreationFactory = ediInterchangeCreationFactory;
				Batch = batch;
				Logger = logger;
				BillingTransactions = new List<BillingTransaction>();
				TransactionPKsReadyToSend = transactionPKsReadyToSend ?? new List<ZGuid>();
			}

			public BusinessObjectFactory EDIInterchangeCreationFactory { get; }

			public AccEInvoicingBatch Batch { get; }

			public List<ZGuid> TransactionPKsReadyToSend { get; protected set; }

			public ILogger Logger { get; }

			public bool IsBatchChangesSavedInDB { get; set; }

			public T As<T>(bool throwErrorIfCannotBeCasted = true)
				where T : TransactionBatchProcessContext
			{
				if (this is T casted)
				{
					return casted;
				}
				else if (throwErrorIfCannotBeCasted)
				{
					throw new InvalidCastException(FormattableString.Invariant($"Invalid TransactionBatchProcessContext type. Expected Type: {typeof(T).ToString()}, Actual Type: {GetType().ToString()}"));
				}
				else
				{
					return null;
				}
			}

			public List<BillingTransaction> BillingTransactions { get; set; }
		}

		class EDIInterchangeNotificationHandler : INotificationHandler
		{
			public EDIInterchangeNotificationHandler()
			{
			}

			public IEnumerable<string> ErrorMessages => errorMessages;
			readonly List<string> errorMessages = new List<string>();

			public IEnumerable<string> InfoMessages => infoMessages;
			readonly List<string> infoMessages = new List<string>();

			public void ReportError(string message, string caption, string errorContext = null, Exception exception = null)
			{
				ErrorReporter.ReportOnce(string.Format("{0}: {1}", caption, message), exception);
				errorMessages.Add(FormattableString.Invariant($"{caption} : {message}"));
			}

			public void ReportInformation(string message, string caption)
			{
				infoMessages.Add(FormattableString.Invariant($"{caption} : {message}"));
			}
		}

		#endregion
	}

	#endregion
}
