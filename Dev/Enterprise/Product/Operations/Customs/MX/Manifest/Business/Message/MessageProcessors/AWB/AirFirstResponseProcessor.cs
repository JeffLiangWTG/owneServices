using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.MX.Manifest.Business
{
	public class AirFirstResponseProcessor : ManifestResponseMessageProcessor
	{
		public AirFirstResponseProcessor(LoggingInformation logger)
			: base(logger)
		{ }

		protected override ZString ProcessManifestMessageCore(MXMessage message)
		{
			var status = EDIMessageStatusList.Codes.ProcessedOK;

			var linkedBill = message.EM_LinkedObject as AsycudaBill;
			if (linkedBill != null)
			{
				var response = CargoWise.Customs.MX.MessageContracts.MXHelper.GetAirFirstResponseInformation(message.EM_MessageText);
				if (response != null)
				{
					var errors = response.Errors;
					var result = ZString.Empty;

					var responseStatus = response.Status;
					if (responseStatus == MXAWBMessageConstants.TrueStatus)
					{
						linkedBill.ABL_MessageStatus = CustomsStatusList.Codes.SNT;
						result = MXMessageConstants.AcceptedByCustoms;
						isFailure = false;
					}
					else
					{
						linkedBill.ABL_MessageStatus = CustomsStatusList.Codes.ERR;
						result = MXMessageConstants.RejectedMessage;
					}

					tableCreator.WriteRow(new string[] { MXAWBMessageConstants.Status, responseStatus });
					foreach (var error in errors)
					{
						tableCreator.WriteRow(new string[] { MXAWBMessageConstants.Error, error });
					}

					message.EM_MessageInterpretation = MXMessageProcessorHelper.MessageInterpretationFirstResponseAirMode(responseStatus, linkedBill.ABL_BillNumber, errors, result);
				}
				else
				{
					SetMessageStatusAsFailed(message);
					Logger.LogWarning(Res.GetString("07BBC0F9-1DB9-49C8-8984-F5464337182B", "Unable to de-serialize the response for message {0}", GetEDIMessageStatusLogDescription(message)));
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
