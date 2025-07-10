using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.NCTS.Messaging.MessageProcessors;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;
using static Enterprise.Customs.EU.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class NctsArrivalDeclarationResponseMessageProcessor : ESNCTSResponseMessageProcessor<INctsArrivalResponseMessageProvider>
	{
		public NctsArrivalDeclarationResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"NCTS Arrival Declaration Message Processor";

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { DeclarationMessageTypeList.Codes.NctsArrivalNotification,
																		   DeclarationMessageTypeList.Codes.NctsUnloadingRemarks,
																		   DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithUnloadingRemarksAviPlusObs,
																		   DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureTnnPlusAvi,
																		   DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureAndUnloadingRemarksTnnPlusAviPlusOb };

		protected override void ProcessMessageCore(EDIMessage message, NctsHeader linkedBusinessObject, INctsArrivalResponseMessageProvider provider)
		{
			if (linkedBusinessObject.IsArrivalMovement)
			{
				var messagePrettyFormatter = new NctsArrivalMessagePrettyFormatter(provider);

				SetArrivalStatus(linkedBusinessObject, provider);
				var documentMessageName = provider.DocumentMessageName;
				if (documentMessageName.IsEmpty || documentMessageName == ES.Business.UniversalReferenceConstants.DeclarationResponseCode.Rejected)
				{
					message.EM_MessageInterpretation = messagePrettyFormatter.CreateMessageDetailsRejected();
					SetMessageSubTypeAsRejected(message);
				}
				else
				{
					SetEntryNumbers(linkedBusinessObject, provider);

					message.EM_MessageInterpretation = messagePrettyFormatter.CreateMessageDetailsAccepted();
					SetMessageSubTypeAsAccepted(message);
				}

				SetArrivalMessageStatus(linkedBusinessObject, provider);
				SetMessageStatusAsReceived(message);
			}
			else
			{
				throw new InvalidOperationException(SetNctsMessageFailedLogDescription(linkedBusinessObject, "Arrival"));
			}
		}

		void SetArrivalStatus(NctsHeader header, INctsArrivalResponseMessageProvider response)
		{
			var isArrivalWithOBS = DeclarationMessageTypeList.IsArrivalWithOBS(response.DocumentMessageName);
			var isAuthorizedHolder = header.IsACEAuthorizedHolder;
			switch (response.MessageFunction)
			{
				case MessageFunctionCodeList.Codes.Rejected:
					if (header.ArrivalMovementHeader.BM_CustomsStatus.IsEmpty || header.ArrivalMovementHeader.BM_CustomsStatus == NctsTransitStatusList.Codes.DeclarationInitial)
					{
						header.ArrivalMovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationRejected;
					}
					break;
				case MessageFunctionCodeList.Codes.GreenCircuit:
					if (!isArrivalWithOBS)
					{
						header.ArrivalMovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.UnloadingPermissionGranted;
					}
					else if (isArrivalWithOBS && isAuthorizedHolder)
					{
						header.ArrivalMovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsWrittenOff;
						var (matchingResult, departureHeaderFound) = header.FindRelevantDepartureRecordForCombinedDepartureAndArrival();
						if (matchingResult == DepartureRecordFindResult.AlreadyCombinedDepartureAndArrival || matchingResult == DepartureRecordFindResult.FoundByMatchingMrn)
						{
							var writeOffTransactionCreator = new WriteOffTransactionCreator();
							writeOffTransactionCreator.AddGuaranteesWriteOffTransactionsNcts((NctsHeader)departureHeaderFound, response.AdmissionDate, Logger);
						}
					}
					break;
				case MessageFunctionCodeList.Codes.RedCircuit:
				case MessageFunctionCodeList.Codes.OrangeCircuit:
					if (!isArrivalWithOBS || (isArrivalWithOBS && isAuthorizedHolder))
					{
						header.ArrivalMovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsUnderCustomsControl;
					}
					break;
				default:
					break;
			}
		}

		void SetArrivalMessageStatus(NctsHeader header, INctsArrivalResponseMessageProvider response)
		{
			if (response.MessageFunction == MessageFunctionCodeList.Codes.Rejected)
			{
				header.EffectiveMessageStatus = header.ArrivalMovementHeader.BM_CustomsStatus == NctsTransitStatusList.Codes.UnloadingPermissionGranted ? NctsMessageStatusList.Codes.UnloadingRemarksRejected : NctsMessageStatusList.Codes.ArrivalNotificationRejected;
			}
			else
			{
				SetCHStatusAsReceived(header);
			}
		}

		void SetEntryNumbers(NctsHeader header, INctsArrivalResponseMessageProvider response)
		{
			var mrnCode = response.TransitReferenceNumber;
			if (!mrnCode.IsEmpty)
			{
				if (response.MessageFunction != MessageFunctionCodeList.Codes.Rejected)
				{
					CreateOrUpdateCusEntryNumber(header, CusEntryNumberTypes.Spain.ArrivalReferenceNumber, mrnCode, response.MessageFunction, response.AdmissionDate, ZDateTime.Empty);
				}
			}

			var summaryCode = response.SummaryReferenceNumber;
			if (!summaryCode.IsEmpty)
			{
				CreateOrUpdateCusEntryNumber(header, CusEntryNumberTypes.Spain.SummaryEntryNumber, summaryCode, ZString.Empty, ZDateTime.Empty, ZDateTime.Empty);
			}
		}

		protected override INctsArrivalResponseMessageProvider GetMessageProviderCore(EDIMessage message)
		{
			return message.EM_MessageText.Contains(EdifactCodes.UNHSegmentCode)
				? CUSRESD96BMessageHelper.New(message)
				: (INctsArrivalResponseMessageProvider)NCTSEdifactProcessorHelper.ProcessEdifactErrorResponse(message);
		}
	}
}
