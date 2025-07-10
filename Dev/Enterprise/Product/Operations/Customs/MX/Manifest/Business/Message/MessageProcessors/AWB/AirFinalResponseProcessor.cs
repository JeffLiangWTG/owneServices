using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.MX.Manifest.Business
{
	public class AirFinalResponseProcessor : ManifestResponseMessageProcessor
	{
		public AirFinalResponseProcessor(LoggingInformation logger)
			: base(logger)
		{ }

		protected override ZString ProcessManifestMessageCore(MXMessage message)
		{
			var status = EDIMessageStatusList.Codes.ProcessedOK;

			var linkedBill = message.EM_LinkedObject as AsycudaBill;
			if (linkedBill != null)
			{
				var response = CargoWise.Customs.MX.MessageContracts.MXHelper.GetAirFinalResponseInformation(message.EM_MessageText);

				if (response != null)
				{
					ZString result;

					if (response.Status == MXAWBMessageConstants.AcceptedProcessed)
					{
						linkedBill.ABL_MessageStatus = CustomsStatusList.Codes.ACP;
						linkedBill.ABL_BillStatus = CustomsStatusList.Codes.ACP;
						result = MXMessageConstants.AcceptedByCustoms;
						isFailure = false;
					}
					else
					{
						linkedBill.ABL_MessageStatus = CustomsStatusList.Codes.ERR;

						if (linkedBill.ABL_BillStatus == CustomsStatusList.Codes.SNT)
						{
							linkedBill.ABL_BillStatus = CustomsStatusList.Codes.ERR;
						}

						result = MXMessageConstants.RejectedMessage;
					}

					tableCreator.WriteRow(new string[] { MXAWBMessageConstants.Status, response.Status });
					foreach (var error in response.Errors)
					{
						tableCreator.WriteRow(new string[] { MXAWBMessageConstants.Error, error });
					}

					message.EM_MessageInterpretation = MXMessageProcessorHelper.MessageInterpretationFinalResponseAirMode(response, linkedBill.ABL_BillNumber, result);
				}
				else
				{
					SetMessageStatusAsFailed(message);
					Logger.LogWarning(Res.GetString("EEC3D695-1121-4FB0-9846-3A1B77637E07", "Unable to de-serialize the response for message {0}", GetEDIMessageStatusLogDescription(message)));
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
