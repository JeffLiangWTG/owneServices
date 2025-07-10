using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.xTMessaging.Business;
using Enterprise.xTMessaging.Shared;
using Enterprise.ZArchitecture.Schema;
using Xware.Xt.Grpc.Config;

namespace Enterprise.xTMessaging.ServiceTasks
{
	public class OutboundInterchangeProcessor : IInterchangeProcessor
	{
		public OutboundInterchangeProcessor(ILogger logger)
		{
			this.logger = Argument.NotNull(logger, "logger");
		}

		readonly ILogger logger;
		readonly int xtSendingRetryLimit = DirectxTMessagingRegistry.Instance.XTSendingRetryLimit.Value;

		void IInterchangeProcessor.Process(Configuration config, CancellationToken token)
		{
			var interchangeCountOfPerBatch = DirectxTMessagingRegistry.Instance.InterchangeCountPerBatchOnSending.Value;
			var interchanges = GetInterchangesToProcess(interchangeCountOfPerBatch);
			if (interchanges.Length == 0)
			{
				logger.Log(LogType.Information, "Not process due to no interchange to be sent.");
				return;
			}

			if (!DirectxTMessagingRegistry.Instance.EnableXTIServiceTask.Value)
			{
				DirectxTMessagingRegistry.Instance.EnableXTIServiceTask.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			}

			using (var connector = GetConnector(GetSubmitMsgAttributeModifier, GetMsgClientProvider, config, token))
			{
				if (!connector.InitializeWithFullLogging())
				{
					return;
				}

				var firstRun = true;
				connector.RunActivityLoop(token, () =>
				{
					interchanges = PreProcessing(firstRun ? interchanges : GetInterchangesToProcess(interchangeCountOfPerBatch));
					firstRun = false;

					var processedCount = 0;
					if (interchanges.Length > 0)
					{
						var interchangeGroups = interchanges.GroupBy(i => i.EI_GB);
						foreach (var group in interchangeGroups)
						{
							token.ThrowIfCancellationRequested();
							using (DisposableEnvironment.ForBranch(group.Key.ToGuid()))
							{
								logger.Log(LogType.Information, $"Processing interchanges for branch:{GlbCompany.CurrentCompany.GC_Name}/{GlbBranch.CurrentBranch.GB_BranchName}, trying batch processing first.");
								var preProcessedGroup = group.ToArray();
								if (!ProcessInterchangesBatch(preProcessedGroup.ToList(), connector, token))
								{
									logger.Log(LogType.Information, "Batch processing failed, try to process interchanges individually.");
									processedCount += ProcessInterchangeIndividually(preProcessedGroup, connector, token);
								}
								else
								{
									logger.Log(LogType.Information, "Batch processing successfully.");
									processedCount += preProcessedGroup.Length;
								}
							}
						}
					}

					return processedCount > 0;
				});
			}
		}

		EDIInterchange[] GetInterchangesToProcess(int maxRows)
		{
			factory = GetFactory();
			var interchanges = factory.Load<EDIInterchange>(GetFilterQuery(maxRows));
			return interchanges;
		}

		BusinessObjectFactory factory;

		#region Interchange Selection

		protected virtual BusinessObjectFactory GetFactory() => new BusinessObjectFactory();

		protected virtual string TransportType => EDIInterchangeTransportTypeList.Codes.xT;

		protected virtual ZQuery GetFilterQuery(int maxRows)
		{
			var filter = new ZQuery(EDIInterchangeSchema.EI_Status, EDIInterchangeStatusList.Codes.Queued);
			filter.AddToFilter(EDIInterchangeSchema.EI_IsActive, true);
			filter.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit);
			filter.AddToFilter(EDIInterchangeSchema.EI_TransportType, TransportType);
			filter.TableIndexHints.Add(new TableIndexHint(EDIInterchangeSchema.Constants.Indexes.NR_RX__EI_ReceiveTransmit_EI_Status_EI_TransportType_EI_ApplicationCode_EI_From_EI_SystemCreateTimeUtc_XTTRCVTRX));
			filter.OrderBy = $"{AutoEDIInterchange.Schema.EI_SystemCreateTimeUtc}, {AutoEDIInterchange.Schema.EI_InterchangeNum}";
			filter.MaximumRows = maxRows;
			return filter;
		}

		#endregion

		EDIInterchange[] PreProcessing(EDIInterchange[] interchanges)
		{
			if (interchanges.Length == 0)
			{
				return interchanges;
			}
			logger.Log(LogType.Information, $"Start validating {interchanges.Length} interchange(s).");
			var goodInterchanges = new List<EDIInterchange>();
			var badInterchanges = new List<EDIInterchange>();
			var sendingResults = new Dictionary<Guid, (bool result, long msgId, string note)>();
			foreach (var interchange in interchanges)
			{
				var validatingResult = ValidateInterchangesBeforeSending(interchange);
				if (!validatingResult.result)
				{
					badInterchanges.Add(interchange);
					sendingResults.Add(interchange.PK.ToGuid(), validatingResult);
				}
				else
				{
					goodInterchanges.Add(interchange);
				}
			}

			if (badInterchanges.Count > 0)
			{
				try
				{
					SaveInterchanges(badInterchanges, sendingResults, string.Join("/", badInterchanges.Select(i => i.EI_InterchangeNum)));
				}
				catch
				{
					logger.Log(LogType.Debug, $"{badInterchanges.Count} invalid interchange(s) would be skipped in this round and handled in next round.");
				}
			}

			logger.Log(LogType.Information, $"Validating interchange(s) done, {goodInterchanges.Count} interchange(s) will be sent.");

			return goodInterchanges.ToArray();
		}

		#region Interchange Processing

		protected virtual bool ProcessInterchangesBatch(IList<EDIInterchange> interchanges, DirectxTConnector connector, CancellationToken cancellationToken, bool withErrorReport = false)
		{
			var sw = Stopwatch.StartNew();
			var allNumbers = string.Join("/", interchanges.Select(i => i.EI_InterchangeNum));

			try
			{
				cancellationToken.ThrowIfCancellationRequested();
				var sendingTasks = new Dictionary<Guid, Task<(bool result, long id, string note)>>();
				var linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
				connector.StartTransaction(null, cancellationToken);

				var interchangesGroupedByAppCode = interchanges.GroupBy(i => i.EI_ApplicationCode);

				foreach (var interchangesGrouping in interchangesGroupedByAppCode)
				{
					logger.Log(LogType.Debug, $"Asynchronously sending {interchangesGrouping.Count()} interchange(s) grouped by Application Code: {interchangesGrouping.Select(x => x.EI_ApplicationCode).FirstOrDefault()}");
					foreach (var interchange in interchangesGrouping)
					{
						sendingTasks.Add(interchange.PK.ToGuid(), connector.SendInterchange(new XtMessageInfo(interchange, logger), linkedCancellationTokenSource.Token));
					}

					if (!Task.WaitAll(sendingTasks.Values.ToArray(), DirectxTMessagingRegistry.Instance.XTServerMessageTimeoutInSeconds.Value * 1000, cancellationToken))
					{
						linkedCancellationTokenSource.Cancel();
					}
				}

				var sendingResults = sendingTasks.ToDictionary(kv => kv.Key, kv => kv.Value.Result);

				SaveInterchanges(interchanges, sendingResults, allNumbers);

				connector.EndTransaction(commit: true, null, cancellationToken);
				logger.Log(LogType.Information, $"Send {interchanges.Count} interchange(s)({allNumbers}) successfully.");
				linkedCancellationTokenSource.Dispose();

				return true;
			}
			catch (XtTransactionException ex)
			{
				HandleExceptions(interchanges, connector, cancellationToken, withErrorReport, ex);
				return false;
			}
			catch (InterchangeSavingException savingEx)
			{
				HandleExceptions(interchanges, connector, cancellationToken, withErrorReport, savingEx);
				return false;
			}
			catch (OperationCanceledException cancelledEx)
			{
				HandleExceptions(interchanges, connector, cancellationToken, withErrorReport, cancelledEx);
				return false;
			}
			catch (AggregateException ex)
			{
				if (ex.InnerExceptions.FirstOrDefault(e => e is MsgServerConnectionException) is MsgServerConnectionException innerMsgServerConnectionException)
				{
					throw innerMsgServerConnectionException;
				}
				HandleExceptions(interchanges, connector, cancellationToken, withErrorReport: true, ex);
				throw;
			}
			finally
			{
				sw.Stop();
				logger.Log(LogType.Information, $"Finish handling {interchanges.Count} interchange(s). Total Time: {sw.Elapsed.TotalSeconds} second(s)");
			}
		}

		static string GetNestedExceptionMessages(Exception exception)
		{
			return exception.Message + (exception.InnerException == null
				? string.Empty
				: $" - {GetNestedExceptionMessages(exception.InnerException)}");
		}

		void HandleExceptions(IList<EDIInterchange> interchanges, DirectxTConnector connector, CancellationToken cancellationToken, bool withErrorReport, Exception ex)
		{
			var error = ex is XtTransactionException ? GetNestedExceptionMessages(ex) : ex.Message;
			try
			{
				connector.EndTransaction(commit: false, null, cancellationToken);
			}
			catch (XtTransactionException transactionException)
			{
				error += ". " + GetNestedExceptionMessages(transactionException);
			}
			var msg = $"Failed to send {interchanges.Count} interchange(s): {error}";

			if (withErrorReport)
			{
				logger.Log(LogType.Error, msg);
				ErrorReporter.ReportOnce(msg, ex.InnerException);
			}
			else
			{
				logger.Log(LogType.Warning, msg);
			}
		}

		int ProcessInterchangeIndividually(EDIInterchange[] preProcessedGroup, DirectxTConnector connector, CancellationToken token)
		{
			var subProcessedCount = 0;
			var failedInterchangeInfo = new Dictionary<Guid, string>();
			foreach (var pk in preProcessedGroup.Select(i => i.PK))
			{
				var interchange = SetNewFactoryAndReloadEdiInterchanges(pk);
				var singleProcessResult = ProcessInterchangesBatch(new List<EDIInterchange> { interchange }, connector, token, true);
				if (singleProcessResult)
				{
					subProcessedCount++;
				}
				else
				{
					failedInterchangeInfo.Add(interchange.PK.ToGuid(), interchange.EI_InterchangeNum);
				}
			}

			var failedProcessLogText = failedInterchangeInfo.Count > 0 ? $" and {failedInterchangeInfo.Count} failed" : string.Empty;
			logger.Log(LogType.Information, $"Individually processing done, {subProcessedCount} interchange(s) processed{failedProcessLogText}.");

			if (failedInterchangeInfo.Count > 0)
			{
				logger.Log(LogType.Information, $"Failed interchange(s):{string.Join("/", failedInterchangeInfo.Select(i => i.Value))}, trying to increasing their EI_RetryCount(and EI_Status if necessary)...");
				foreach (var interchange in failedInterchangeInfo)
				{
					IncreasingFailedInterchangeRetryCountAndStatusIfNecessaryWithNewFactory(interchange.Key);
				}
			}

			return subProcessedCount;
		}

		void SaveInterchanges(IList<EDIInterchange> interchanges, Dictionary<Guid, (bool result, long msgId, string note)> sendingResults, string allNumbers)
		{
			if (interchanges.Count == 0)
			{
				return;
			}
			foreach (var interchange in interchanges)
			{
				SetInterchangeStatus(interchange, sendingResults[interchange.PK.ToGuid()]);
			}

			try
			{
				logger.Log(LogType.Information, $"Save changes of interchanges({allNumbers}).");
				factory.Save();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				var msg = $"Failed to save {interchanges.Count} interchange(s)({allNumbers}).";
				logger.Log(LogType.Warning, msg);
				throw new InterchangeSavingException(msg, ex);
			}
		}

		void SetInterchangeStatus(EDIInterchange interchange, (bool result, long msgId, string note) sendingResult)
		{
			var loggingPrefix = interchange.EI_RetryCount == 0 ? string.Empty : $"[RetryCount:{interchange.EI_RetryCount}]";
			string msg;
			interchange.EI_XTInternalMsgID = sendingResult.msgId;
			string description;
			if (sendingResult.result)
			{
				interchange.EI_Status = EDIInterchangeStatusList.Codes.Sent;
				msg = $"{loggingPrefix}Interchange: {interchange.EI_InterchangeNum} was set to SNT.";
				description = Res.GetString("EC410FEF-9981-438A-A52D-F49A7A1C25CB", "Message Delivery Successful");
				logger.Log(LogType.Information, msg);
			}
			else
			{
				interchange.EI_Status = EDIInterchangeStatusList.Codes.Error;
				msg = $"{loggingPrefix}Interchange: {interchange.EI_InterchangeNum} was set to ERR:{sendingResult.note}";
				description = Res.GetString("f1bbb0da-e691-432d-998b-3d1ee8ba94c0", "Message Delivery Failure");
				SetMessageStatusAndAddStmNote(interchange, description, Res.GetString("1AEEBF18-B0E6-4152-BF8C-F0A6EE417F19", "Failed to send interchange: {0}", interchange.EI_InterchangeNum));
				logger.Log(LogType.Error, msg);
			}
			interchange.Notes.AddNew(isCustomDescription: true, description, msg);
		}

		void IncreasingFailedInterchangeRetryCountAndStatusIfNecessaryWithNewFactory(Guid interchangePk)
		{
			var interchange = SetNewFactoryAndReloadEdiInterchanges(interchangePk);
			try
			{
				interchange.EI_RetryCount++;
				var statusUpdatedLogText = string.Empty;
				if (interchange.EI_RetryCount > xtSendingRetryLimit)
				{
					interchange.EI_Status = EDIInterchangeStatusList.Codes.Error;
					SetMessageStatusAndAddStmNote(interchange, Res.GetString("32DB5A20-5AC5-4AFD-BA9F-79C56360ADBA", "Message Delivery Failure"), Res.GetString("411E3111-3171-4991-9AA7-8DA253AF46E5", $"Retry count of interchange:{interchange.EI_InterchangeNum} meet the maximum value"));
					statusUpdatedLogText = ", EI_Status of interchange set to ERR and all contained messages' EM_Status set to ERR";
				}
				factory.Save();
				logger.Log(LogType.Information, $"EI_RetryCount of interchange {interchange.EI_InterchangeNum} increased{statusUpdatedLogText}.");
			}
			catch
			{
				logger.Log(LogType.Error, $"Failed to increase EI_RetryCount of interchange {interchange.EI_InterchangeNum}, please pay attention on it.");
			}
		}

		void SetMessageStatusAndAddStmNote(EDIInterchange interchange, string description, string noteText)
		{
			foreach (var cm in interchange.ContainedMessages)
			{
				var message = (EDIMessage)cm;
				message.EM_Status = EDIMessageStatusList.Codes.Error;
				message.Notes.AddNew(isCustomDescription: true, description, Res.GetString("9A9D4ADD-47A4-4E61-B7E8-11FCC4D24F7B", "Message: {0} was set to ERR as per: {1}.", message.EM_MessageNum, noteText));
			}
		}

		EDIInterchange SetNewFactoryAndReloadEdiInterchanges(ZGuid pks)
		{
			factory = GetFactory();
			return factory.Load<EDIInterchange>(pks);
		}

		(bool result, long msgId, string note) ValidateInterchangesBeforeSending(EDIInterchange interchange)
		{
			var errorMsgPrefix = interchange.EI_RetryCount == 0 ? "" : $"[RetryCount:{interchange.EI_RetryCount}]";
			if (string.IsNullOrEmpty(interchange.EI_ApplicationCode))
			{
				return (false, 0, $"{errorMsgPrefix}{EDIInterchangeSchema.EI_ApplicationCode.Name} should not be null.");
			}

			if (string.IsNullOrEmpty(interchange.EI_From))
			{
				return (false, 0, $"{errorMsgPrefix}{EDIInterchangeSchema.EI_From.Name} should not be null.");
			}

			return (true, 0, string.Empty);
		}

		protected virtual IMsgClientProvider GetMsgClientProvider(Configuration config, CancellationToken token) => new MsgClientProvider(config, token);

		protected virtual ISubmitMsgAttributeModifier GetSubmitMsgAttributeModifier() => new SubmitMsgAttributeModifier();

		protected virtual DirectxTConnector GetConnector(Func<ISubmitMsgAttributeModifier> getSubmitMsgAttributeModifier, Func<Configuration, CancellationToken, IMsgClientProvider> getMsgClientProvider, Configuration config, CancellationToken token) =>
			new(getMsgClientProvider(config, token), new Cw1DirectxTMessagingConfig(), logger, getSubmitMsgAttributeModifier());

		#endregion
	}
}
