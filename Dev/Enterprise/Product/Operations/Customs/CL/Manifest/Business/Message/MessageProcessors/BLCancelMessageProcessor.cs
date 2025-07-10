using CargoWise.Customs.CL.MessageContracts;
using Enterprise.BatchProcessor;

namespace Enterprise.Customs.CL.Manifest.Business
{
	class BLCancelMessageProcessor : CLMessageProcessor
	{
		internal BLCancelMessageProcessor(LoggingInformation logger) : base(logger) { }

		internal override string ProcessCustomsManifestMessage(CLMessage message, string status)
		{
			var response = MessageBuilderHelper.GetBLCancelResponse(message.EM_MessageText);
			if (response != null)
			{
				var bill = message.EM_LinkedObject as AsycudaBill;
				if (bill != null)
				{
					var column = errorColumn;
					if (response.Status == CLMessageConstants.Accepted)
					{
						bill.ABL_BillStatus = CustomsStatusList.Codes.CAN;
						bill.ABL_MessageStatus = CustomsStatusList.Codes.CAN;

						column = detailColumn;
						isFailure = false;
					}
					else
					{
						bill.ABL_BillStatus = CustomsStatusList.Codes.ACP;
						bill.ABL_MessageStatus = CustomsStatusList.Codes.ERR;
					}

					message.EM_MessageInterpretation = response.MessageInterpretation();
					var nodes = CLMessageHelper.GetResponseInformation(response.ReferenceNumber, response.Status);
					BuildEmailBody(nodes, message.EM_MessageInterpretation, column);
				}
				else
				{
					status = DiscardedCase();
				}
			}
			else
			{
				status = ErrorCase(message);
			}
			return status;
		}
	}
}
