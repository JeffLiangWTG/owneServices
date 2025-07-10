using CargoWise.Customs.CL.MessageContracts;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.CL.Manifest.Business
{
	class AWBMessageProcessor : CLMessageProcessor
	{
		internal AWBMessageProcessor(LoggingInformation logger) : base(logger) { }

		internal override string ProcessCustomsManifestMessage(CLMessage message, string status)
		{
			var response = MessageBuilderHelper.GetAWBResponse(message.EM_MessageText);
			if (response != null)
			{
				var bill = message.EM_LinkedObject as AsycudaBill;
				if (bill != null)
				{
					var column = errorColumn;
					if (response.Status == CLMessageConstants.Accepted)
					{
						bill.ABL_BillStatus = CustomsStatusList.Codes.ACP;
						bill.ABL_MessageStatus = CustomsStatusList.Codes.ACP;

						PopulateBillCustomsEntryNumber(bill, response.ID);

						column = detailColumn;
						isFailure = false;
					}
					else
					{
						bill.ABL_MessageStatus = CustomsStatusList.Codes.ERR;

						if (bill.ABL_BillStatus == MessageStatusCodeList.Codes.Sent)
						{
							bill.ABL_BillStatus = MessageStatusCodeList.Codes.Error;
						}
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
