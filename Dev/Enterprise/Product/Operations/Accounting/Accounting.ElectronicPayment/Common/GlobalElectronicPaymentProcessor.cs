using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicPayment.Common;
using Enterprise.Accounting.ElectronicPayment.Common.GlobalElectronicPayment;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.ElectronicPayment
{
	public class GlobalElectronicPaymentProcessor
	{
		public GlobalElectronicPaymentProcessor()
		{
		}

		internal GlobalElectronicPaymentProcessor(IEnumerable<IGEPRequestQueue> requestQueues, IEnumerable<GlbCompany> companies)
		{
			Argument.NotNull(requestQueues, nameof(requestQueues));
			this.requestQueues = requestQueues;
			this.companies = companies;
		}

		IEnumerable<IGEPRequestQueue> RequestQueues => requestQueues ?? (requestQueues = DefaultRequestQueueList.Get());
		IEnumerable<IGEPRequestQueue> requestQueues;

		IEnumerable<GlbCompany> Companies => companies ?? (companies = GlbCompany.GetActiveCompanies());
		IEnumerable<GlbCompany> companies;

		public void ProcessEPayments(ILogger logger, CancellationToken token)
		{
			foreach (var currentCompany in Companies)
			{
				if (AccountingMasterFilesRegistry.Instance.EnableEPaymentFunctionality.GetValueWithoutFallback(currentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty).IsEPaymentEnabledForAnyProvider)
				{
					var currentCompanyEnvironment = DisposableEnvironment.ForCompany(currentCompany.GC_Code, reportInactive: false);
					if (currentCompanyEnvironment == null)
					{
						logger.Log(LogType.Debug, FormattableString.Invariant(
							$"Cannot find an active branch for company [{currentCompany.GC_Code}]."));
						continue;
					}

					using (currentCompanyEnvironment)
					{
						foreach (IGEPRequestQueue queue in RequestQueues)
						{
							logger.Log(LogType.Debug, FormattableString.Invariant($"Started processing {queue.MessageTypeDescription} for Company [{Env.CurrentCompany.Code}]."));

							bool? continueProcessing = null;
							do
							{
								token.ThrowIfCancellationRequested();
								var request = queue.GetTopOneQueuedRequest();
								if (request == null)
								{
									if (!continueProcessing.HasValue)
									{
										logger.Log(LogType.Debug, FormattableString.Invariant($"No {queue.MessageTypeDescription} were available for processing."));
									}
								}
								else
								{
									var processResult = ProcessGEPMessageRequest(request, currentCompany, logger);
									if (!processResult?.IsSuccessfullyProcessed ?? false)
									{
										request.SetToErrorStatus(processResult?.ErrorMessage);
									}
								}

								continueProcessing = request != null;
							} while (continueProcessing.Value);

							logger.Log(LogType.Debug, FormattableString.Invariant($"Finished processing {queue.MessageTypeDescription} for Company [{Env.CurrentCompany.Code}]."));
						}
					}
				}
			}
		}

		GEPMessageRequestProcessResult? ProcessGEPMessageRequest(IGEPRequestMessage request, GlbCompany company, ILogger logger)
		{
			GEPMessageRequestProcessResult? processResult = null;
			if (request != null)
			{
				var factories = new HashSet<ITransactionParticipant>();
				try
				{
					logger.Log(LogType.Debug, FormattableString.Invariant($"Started processing {request.MessageTypeDescription} {request.MessageReferenceNumber}."));

					factories.Add(request.GetTransactionParticipant());

					logger.Log(LogType.Debug, FormattableString.Invariant($"Starting EDI message creation process."));

					var ediInterchangeFactory = new BusinessObjectFactory(); // need independent ediMessage factory for each business object, it's possible some ediMessage creation may fail, so we only want to save the successful ones.
					using (ediInterchangeFactory.AddDisposableService())
					{
						var ePayment = request.CreateGEPMessage();
						CreateEDIMessageAndInterchange(ediInterchangeFactory, request.GetPaymentDeliveryContextValueProvider(), ePayment);
						factories.Add(ediInterchangeFactory);

						logger.Log(LogType.Debug, FormattableString.Invariant($"Finished EDI message creation."));

						request.PerformAfterCreatingEDIMessageAndInterchangeSuccessfully();

						SaveChanges(factories, logger);

						processResult = new GEPMessageRequestProcessResult(string.Empty);
					}

					logger.Log(LogType.Information, FormattableString.Invariant($"Finished processing {request.MessageTypeDescription} {request.MessageReferenceNumber}."));
				}
				//Please do not catch any exception here which cannot be handled by updating bizo status to 'Error'.
				//Otherwise queue.GetTopOneQueuedRequest() will keep returning the same bizo for processing.
				catch (GEPMessageCreationException exception) when (!exception.IsCriticalException())
				{
					LogException(request, company, logger, exception);
					var errorMessage = string.IsNullOrEmpty(exception.UserFriendlyMessage)
						? exception.Message
						: exception.UserFriendlyMessage;
					processResult = new GEPMessageRequestProcessResult(errorMessage);
				}
				catch (ZSaveException exception) when (!exception.IsCriticalException())
				{
					LogException(request, company, logger, exception);
					var message = Res.GetString("d89c5b07-b738-45ab-9efa-9df8cb0846a8", "{0} occurred while saving changes.", typeof(ZSaveException));
					processResult = new GEPMessageRequestProcessResult(message);
				}
			}
			return processResult;
		}

		void CreateEDIMessageAndInterchange(BusinessObjectFactory ediInterchangeFactory, IEPaymentDeliveryContextValueProvider deliveryContextValueProvider, GlobalElectronicPayment electronicPayment)
		{
			var notifications = new Logger();

			CreateEDIMessageAndInterchangeCore(ediInterchangeFactory, deliveryContextValueProvider, electronicPayment, notifications);

			if (notifications.HasErrors)
			{
				throw new GEPMessageCreationException(notifications.ToString().Trim(), null);
			}
		}

		protected virtual void CreateEDIMessageAndInterchangeCore(BusinessObjectFactory ediInterchangeFactory, IEPaymentDeliveryContextValueProvider deliveryContextValueProvider, GlobalElectronicPayment electronicPayment, Logger notifications)
		{
			new GlobalElectronicPaymentEDIInterchangeCreator().CreateInterchangeAndDeliver(ediInterchangeFactory, deliveryContextValueProvider, electronicPayment, notifications);
		}

		void SaveChanges(HashSet<ITransactionParticipant> factories, ILogger logger)
		{
			logger.Log(LogType.Debug, string.Format(FormattableString.Invariant($"Attempting to save all changes in database.")));

			BusinessObjectFactory.SaveTogether(factories.ToArray());

			logger.Log(LogType.Debug, string.Format(FormattableString.Invariant($"Successfully saved. Processing is complete.")));
		}

		void LogException(IGEPRequestMessage request, GlbCompany company, ILogger logger, Exception exception)
		{
			var logMessage = FormattableString.Invariant($"Error : [{exception.Message}]\r\n StackTrace:\r\n {exception.StackTrace}"); // Log message does not need to be translated.
			logger.Log(LogType.Error, FormattableString.Invariant($"Failed to process {request.MessageTypeDescription} {request.MessageReferenceNumber} of company [{company.GC_Code}].\r\n {logMessage}"));
		}

		public struct GEPMessageRequestProcessResult
		{
			public GEPMessageRequestProcessResult(string errorMessage)
			{
				IsSuccessfullyProcessed = errorMessage.IsNullOrEmpty();
				ErrorMessage = errorMessage;
			}

			public bool IsSuccessfullyProcessed { get; }

			public string ErrorMessage { get; }
		}
	}
}
