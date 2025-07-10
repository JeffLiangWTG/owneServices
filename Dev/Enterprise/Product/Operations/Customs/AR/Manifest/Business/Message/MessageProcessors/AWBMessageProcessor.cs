using System.Collections.Generic;
using CargoWise.Customs.AR.MessageContracts;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Messaging.Integration;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.AR.Manifest.Business
{
	public class AWBMessageProcessor : ManifestResponseMessageProcessor
	{
		public AWBMessageProcessor(LoggingInformation logger)
			: base(logger)
		{ }

		protected override IMessageAttachee FindRelevantBusinessObject(ARMessage message)
		{
			var factory = message.Factory;
			var originalInterchange = ARMessageProcessorHelper.GetSentInterchangeWithTrackingId(message.Interchange.EI_SessionGUID, factory);
			if (originalInterchange != null)
			{
				var originalMessage = ARMessageProcessorHelper.GetOriginalMessage(originalInterchange.PK, factory);
				if (originalMessage != null)
				{
					message.EM_LinkUniqueID = originalMessage.EM_LinkUniqueID;
					message.EM_LinkTable = originalMessage.EM_LinkTable;
				}
			}

			return message.EM_LinkedObject as AsycudaBill;
		}

		protected override ZString ProcessManifestMessageCore(ARMessage message)
		{
			var status = EDIMessageStatusList.Codes.ProcessedOK;

			var linkedBill = message.EM_LinkedObject as AsycudaBill;
			if (linkedBill != null)
			{
				var response = MessageBuilderHelper.GetAirResponseInformation(message.EM_MessageText);
				if (response != null)
				{
					var errors = new List<IAirResponseError>();
					ZString result;

					if (response.Status == ARAWBMessageConstants.AcceptedProcessed)
					{
						linkedBill.ABL_MessageStatus = CustomsStatusList.Codes.ACP;
						linkedBill.ABL_BillStatus = CustomsStatusList.Codes.ACP;
						result = ARBLMessageConstants.Accepted;
						isFailure = false;
					}
					else
					{
						linkedBill.ABL_MessageStatus = MessageStatusCodeList.Codes.Error;
						if (linkedBill.ABL_BillStatus == CustomsStatusList.Codes.SNT)
						{
							linkedBill.ABL_BillStatus = MessageStatusCodeList.Codes.Error;
						}
						result = ARBLMessageConstants.Rejected;
						errors = response.Errors;
					}
					message.EM_MessageInterpretation = ARMessageProcessorHelper.MessageInterpretationResponseAirMode(response, linkedBill.ABL_BillNumber, errors, result);
				}
				else
				{
					SetMessageStatusAsFailed(message);
					Logger.LogWarning(Res.GetString("8D0758D4-E7D0-4DF0-BB18-DE8E4D9BCD44", "Unable to de-serialize the response for message {0}", GetEDIMessageStatusLogDescription(message)));
				}
			}
			else
			{
				status = EDIMessageStatusList.Codes.Discarded;
			}

			return status;
		}
	}
}
