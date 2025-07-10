using System;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Dash.Business;
using Enterprise.Dash.Business.Services;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using Shared = WTG.Shared.Dash.Common;

namespace Enterprise.Dash.ServiceTasks.MessageProcessors
{
	public class NotifyDownstreamServicesMessageProcessor : DashMessageProcessor
	{
		public NotifyDownstreamServicesMessageProcessor()
			: this(null, null, null, null, null)
		{
		}

		public NotifyDownstreamServicesMessageProcessor(
			IFactory factory,
			ILogger serviceLogger,
			IShipamaxService shipamaxService,
			IDashErrorReporter dashErrorReporter,
			IDashCompletionService dashCompletionService)
			: base(factory, serviceLogger, shipamaxService, dashErrorReporter)
		{
			DashCompletionService = dashCompletionService ?? throw new ArgumentNullException(nameof(dashCompletionService));
		}

		protected IDashCompletionService DashCompletionService { get; }

		protected override int BatchSize => 5;

		protected override byte MaxRetryCount => 5;

		protected override void AddServiceTaskSpecificQueryFilters(ZQuery query)
			=> query.AddToFilter(EDIMessageSchema.EM_MessageType, Shared.Constants.DataProcessingType.Code.NotifyDownstreamServices);

		protected override void ProcessMessageCore(IFactory factory, DashDocument dashDocument, DashDocumentDataMessage dashDocumentDataMessage, CancellationToken token)
		{
			if (dashDocument.DDD_ParseStatus == Shared.Constants.ParseStatus.Code.Processing)
			{
				switch (dashDocument.DDD_ParseType)
				{
					case Shared.Constants.ParseType.Code.CommercialInvoice:
						HandleCommercialInvoice(dashDocument);
						break;

					case Shared.Constants.ParseType.Code.AccountPayableInvoice:
						HandleAccountsPayableInvoice(factory, dashDocument, dashDocumentDataMessage.EM_GB, dashDocumentDataMessage.EM_GE);
						break;
				}
			}
		}

		void HandleCommercialInvoice(DashDocument dashDocument)
		{
			if (dashDocument.DDD_ValidationStatus == Shared.Constants.ValidationStatus.Code.Passed)
			{
				dashDocument.DDD_ParseStatus = Shared.Constants.ParseStatus.Code.SubmittedForCompletion;

				try
				{
					DashCompletionService.Complete(dashDocument);
				}
				catch (Exception)
				{
					dashDocument.DDD_ParseStatus = Shared.Constants.ParseStatus.Code.Error;
					throw;
				}
			}
			else
			{
				dashDocument.DDD_ParseStatus = Shared.Constants.ParseStatus.Code.NeedReview;

				var shipamaxParseResult = new ShipamaxParseResult()
				{
					ParseStatus = ShipamaxParseStatus.NeedReview
				};

				ShipamaxService.SaveParseResult(dashDocument.DDD_DocID.ToGuid(), dashDocument.DDD_DocToken, shipamaxParseResult);
			}
		}

		void HandleAccountsPayableInvoice(IFactory factory, DashDocument dashDocument, ZGuid branchId, ZGuid departmentId)
		{
			const string accountingIntegrationApplicatinoCode = "DAI";
			const string accountingIntegrationMessageType = "API";

			dashDocument.DDD_ParseStatus = Shared.Constants.ParseStatus.Code.ReadyForThirdPartyProcessing;

			var message = (EDIMessage)factory.New(typeof(EDIMessage));
			message.EM_GB = branchId;
			message.EM_GE = departmentId;
			message.EM_ApplicationCode = accountingIntegrationApplicatinoCode;
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Internal;
			message.EM_MessageType = accountingIntegrationMessageType;
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_LinkUniqueID = dashDocument.PK;
			message.EM_LinkTable = dashDocument.TableName;
			message.EM_IsActive = true;
		}

		protected override bool ContinueDocumentPostProcessingAfterMessageFailure => false;
	}
}
