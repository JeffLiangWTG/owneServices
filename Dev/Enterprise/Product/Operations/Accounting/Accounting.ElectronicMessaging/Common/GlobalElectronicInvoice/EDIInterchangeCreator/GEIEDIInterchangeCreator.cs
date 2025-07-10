using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public abstract class GEIEDIInterchangeCreator : EDIInterchangeCreatorForEInvoicingBatchBase
	{
		protected GEIEDIInterchangeCreator(GlbCompany company)
			: base(company)
		{ }

		protected override ZString GetEInvoicingServicePoint() => "GLB_ELEC_INVOICING"; // Constant string used internally by EServices team.

		protected override bool IsBillingSupported => true;

		protected override void PerformBeforeCreatingEDIMessageAndInterchange(TransactionBatchProcessContext batchProcessContext)
		{
			var geiProcessContext = batchProcessContext.As<GEIProcessContext>();

			base.PerformBeforeCreatingEDIMessageAndInterchange(batchProcessContext);

			var (eInvoice, validationError, validationWarnings) = BatchToGEIConverter.Convert(geiProcessContext.Batch);
			geiProcessContext.EInvoice = eInvoice;
			geiProcessContext.ValidationError = validationError;
			geiProcessContext.ValidationWarnings = validationWarnings;
		}

		protected override void CreateEDIMessageAndInterchangeCore(TransactionBatchProcessContext batchProcessContext, INotifications notifications)
		{
			var geiProcessContext = batchProcessContext.As<GEIProcessContext>();
			if (geiProcessContext.HasEInvoice)
			{
				geiProcessContext.Interchanges = GetGEIDelivery(geiProcessContext, notifications)
													.Deliver(geiProcessContext.EInvoice, geiProcessContext.BatchHasValidationErrors);
			}
			else if (!geiProcessContext.HasEInvoice && !geiProcessContext.BatchHasValidationErrors)
			{
				geiProcessContext.ValidationError = Res.GetString("b442a95d-c6de-4b67-b35e-5e88335e5de0", "E-Invoice could not be created. No further information is available.");
			}
		}

		protected override void PerformAfterCreatingEDIMessageAndInterchangeSuccessfully(TransactionBatchProcessContext batchProcessContext)
		{
			var geiProcessContext = batchProcessContext.As<GEIProcessContext>();

			if (geiProcessContext.BatchHasValidationWarnings)
			{
				AddNotesToEDIMessageWhenBatchHasWarnings(geiProcessContext);
			}

			if (geiProcessContext.BatchHasValidationErrors)
			{
				if (AddErrorToEDIMessageNotesIfAny || !ShouldEInvoiceBatchWithErrorBeSent)
				{
					AddNotesToEDIMessageWhenBatchHasErrors(geiProcessContext);
				}

				if (!ShouldEInvoiceBatchWithErrorBeSent || !geiProcessContext.HasEInvoice)
				{
					UpdatePivotAndBatchStatusWhenBatchHasErrors(geiProcessContext);
					return;
				}
			}

			base.PerformAfterCreatingEDIMessageAndInterchangeSuccessfully(geiProcessContext);
		}

		protected override void PerformIfBatchProcessingFails(TransactionBatchProcessContext batchProcessContext, Exception ex)
		{
			var geiProcessContext = batchProcessContext.As<GEIProcessContext>();

			geiProcessContext.ExceptionOccuredWhileCreatingEDIMessage = ex;

			if (geiProcessContext.BatchHasValidationErrors || !geiProcessContext.HasEInvoice)
			{
				UpdatePivotAndBatchStatusWhenBatchHasErrors(geiProcessContext);
			}
		}

		protected override void OnBatchChangesSaved(TransactionBatchProcessContext batchProcessContext)
		{
			var geiProcessContext = batchProcessContext.As<GEIProcessContext>();

			base.OnBatchChangesSaved(geiProcessContext);

			//Collect all Errors
			var errors = new List<ZString>();
			if (geiProcessContext.BatchHasValidationErrors)
			{
				errors.Add(geiProcessContext.ValidationError);
			}
			if (geiProcessContext.ExceptionOccuredWhileCreatingEDIMessage != null)
			{
				var ex = geiProcessContext.ExceptionOccuredWhileCreatingEDIMessage;
				errors.Add(FormattableString.Invariant($"Exception details: {ex.Message}.\r\nStackTrace:\r\n{ex.StackTrace}")); // Sent as part of an english email.
			}

			if (geiProcessContext.ExceptionOccuredWhileCreatingEDIMessage != null
				|| (geiProcessContext.BatchHasValidationErrors && !ShouldEInvoiceBatchWithErrorBeSent))
			{
				SendEmail(geiProcessContext, errors);
			}
		}

		protected override string GetConcludingMessageAfterBatchProcessingIsComplete(TransactionBatchProcessContext batchProcessContext)
		{
			var geiProcessContext = batchProcessContext.As<GEIProcessContext>();

			if (showNotQueuedMessage())
			{
				return FormattableString.Invariant($"Universal Transaction batch [{batchProcessContext.Batch.AIB_BatchNumber}] of company [{batchProcessContext.Batch.Company.GC_Code}] (organization proxy [{batchProcessContext.Batch.Company.OrgProxy?.OH_Code}]) has been successfully saved as an EDI message."); // Service task log.
			}
			else
			{
				return base.GetConcludingMessageAfterBatchProcessingIsComplete(batchProcessContext);
			}

			bool showNotQueuedMessage() => geiProcessContext.Batch.AIB_Status == EInvoicingBatchState.Discarded;
		}

		void UpdatePivotAndBatchStatusWhenBatchHasErrors(GEIProcessContext geiProcessContext)
		{
			var batchState = !ShouldEInvoiceBatchWithErrorBeSent || !geiProcessContext.HasEInvoice
								? EInvoicingBatchState.Discarded
								: null;
			var errorMessageForUnexpectedErrors = (ZString)Res.GetString("8E1C9E2E-5FB6-4469-83CB-FA4323E08BB1",
				"CargoWise encountered an unexpected error when attempting to create the electronic invoice file. Reset the Status to Queued for this invoice may resolve the issue. If not, please cancel this invoice and re-enter.");

			geiProcessContext.Batch.UpdateBatchAndPivotStatusAndErrorDescription(
				geiProcessContext.TransactionPKsReadyToSend,
				(geiProcessContext.ValidationError.IsEmpty ? errorMessageForUnexpectedErrors : geiProcessContext.ValidationError),
				EInvoicingPivotState.BatchedWithError,
				batchState);
		}

		void AddNotesToEDIMessageWhenBatchHasErrors(GEIProcessContext geiProcessContext)
		{
			foreach (var interchange in geiProcessContext.Interchanges ?? Enumerable.Empty<IXmlEDIInterchange>())
			{
				var messages = interchange.LoadMessages();
				foreach (var message in messages)
				{
					message.Notes.AddNew(true, (NoResString)"Error Details", geiProcessContext.ValidationError); // Sent as part of an english email.
				}
			}
		}

		void AddNotesToEDIMessageWhenBatchHasWarnings(GEIProcessContext geiProcessContext)
		{
			foreach (var interchange in geiProcessContext.Interchanges ?? Enumerable.Empty<IXmlEDIInterchange>())
			{
				var messages = interchange.LoadMessages();
				foreach (var message in messages)
				{
					message.Notes.AddNew(true, (NoResString)"Warning Details", geiProcessContext.ValidationWarnings); // Sent as part of an english email.
				}
			}
		}

		protected virtual void SendEmail(GEIProcessContext geiProcessContext, IEnumerable<ZString> errors)
		{
			var logger = geiProcessContext.Logger;
			logger.Log(LogType.Debug, "Attempting to send an email notification containing error details.");

			var transactionPK = geiProcessContext.TransactionPKsReadyToSend.First();
			if (geiProcessContext.Interchanges?.Any() ?? false)
			{
				foreach (var interchange in geiProcessContext.Interchanges)
				{
					var messages = interchange.LoadMessages();
					foreach (var message in messages)
					{
						GetEmailCreator(message, transactionPK, errors, logger).SendEmail();
					}
				}
			}
			else
			{
				GetEmailCreator(ediMessage: null, transactionPK, errors, logger).SendEmail();
			}
		}

		protected IEDICommunicationsMode[] GetModes()
		{
			var modes = CurrentCompany.LoadGEICommuncationModes();
			return modes.Any() ? modes : new IEDICommunicationsMode[] { GetDefaultCommunicationsMode() };
		}

		#region Standard Communication Modes

		protected IEDICommunicationsMode StandardCommunicationModeForEHubXml()
			=> new NonPersistentEDICommunicationMode
			{
				EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XML,
				EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService,
				EK_Destination = EInvoicingServicePoint,
				EK_MessagePurpose = Purpose
			};

		protected IEDICommunicationsMode StandardCommunicationModeForDirectXTXml()
			=> new NonPersistentEDICommunicationMode
			{
				EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XML,
				EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.XTInterface,
				EK_Destination = EInvoicingServicePoint,
				EK_MessagePurpose = Purpose
			};

		#endregion

		protected abstract IAccEInvoiceBatchToGEIConverter BatchToGEIConverter { get; }

		protected virtual IGlobalElectronicInvoicingDelivery GetGEIDelivery(GEIProcessContext geiProcessContext, INotifications notifications) =>
			new GlobalElectronicInvoicingDelivery(GetDeliveryModeAndContextProvider(geiProcessContext, notifications));

		protected abstract GEIDeliveryModeAndContextProvider GetDeliveryModeAndContextProvider(GEIProcessContext geiProcessContext, INotifications notifications);

		#region Standard Delivery Mode and Context Providers

		protected GEIDeliveryModeAndContextProvider StandardDeliveryModeAndContextProviderForDefaultSerializer(DeliveryContext deliveryContext)
		{
			return new GEIDeliveryModeAndContextProvider()
			{
				Serializer = new DefaultGlobalElectronicInvoiceSerializer(),
				Context = deliveryContext,
				CreateEDIMessageEvenIfThereIsError = true,
				DeliverEDIMessageEvenIfThereIsError = ShouldEInvoiceBatchWithErrorBeSent,
				Modes = GetModes()
			};
		}

		protected DeliveryContext StandardDeliveryContext(GEIProcessContext geiProcessContext, INotifications notifications, ZString messageTypeFallback, ZString messageSubTypeFallback)
		{
			var context = new DeliveryContext(geiProcessContext.EDIInterchangeCreationFactory)
			{
				ParentInfo = EntityInfo.New(geiProcessContext.Batch),
				PurposeCode = Purpose,
				ApplicationCode = ApplicationCodeList.Codes.GlobalElectronicInvoice,
				MessageTypeCode = geiProcessContext.EInvoice?.Header.ElectronicInvoiceBatchRequest.MessageType ?? messageTypeFallback,
				MessageSubTypeCode = geiProcessContext.EInvoice?.Header.ElectronicInvoiceBatchRequest.MessageType ?? messageSubTypeFallback,
				Notifications = notifications
			};
			return context;
		}

		#endregion

		protected abstract GEIEmailNotificationCreator GetEmailCreator(EDIMessage ediMessage, ZGuid transactionPK, IEnumerable<ZString> errors, ILogger logger);

		protected override TransactionBatchProcessContext GetTransactionBatchProcessContext(BusinessObjectFactory ediInterchangeCreationFactory, AccEInvoicingBatch batch, IEnumerable<ZGuid> transactionPKsReadyToSend, ILogger logger)
			=> new GEIProcessContext(ediInterchangeCreationFactory, batch, transactionPKsReadyToSend.ToList(), logger);

		protected string SetPurposeForCurrentBatch(AccEInvoicingBatch batch)
			=> Purpose = batch != null ? Res.GetString("42798bdf-7812-4d8e-988c-ffe9183b3235", "E-Reporting Transaction Exported. Batch : {0}", batch.AIB_BatchNumber.ToString()) : string.Empty;
		protected string Purpose { get; set; }

		protected override ZString RetrieveWarningMessage(TransactionBatchProcessContext batchProcessContext)
		{
			var geiProcessContext = batchProcessContext.As<GEIProcessContext>();
			return geiProcessContext.ValidationWarnings;
		}

		#region Inner class

		protected class GEIProcessContext : TransactionBatchProcessContext
		{
			public GEIProcessContext(BusinessObjectFactory ediInterchangeCreationFactory, AccEInvoicingBatch batch, List<ZGuid> transactionPKsReadyToSend, ILogger logger)
				: base(ediInterchangeCreationFactory, batch, transactionPKsReadyToSend, logger)
			{
			}

			public GlobalElectronicInvoicing EInvoice { get; set; }
			public bool HasEInvoice => EInvoice != null;

			public IEnumerable<IXmlEDIInterchange> Interchanges { get; set; }

			public ZString ValidationError { get; set; }

			public ZString ValidationWarnings { get; set; }

			public Exception ExceptionOccuredWhileCreatingEDIMessage { get; set; }

			public bool BatchHasValidationErrors => !string.IsNullOrEmpty(ValidationError);
			public bool BatchHasValidationWarnings => !string.IsNullOrEmpty(ValidationWarnings);
		}

		#endregion
	}
}
