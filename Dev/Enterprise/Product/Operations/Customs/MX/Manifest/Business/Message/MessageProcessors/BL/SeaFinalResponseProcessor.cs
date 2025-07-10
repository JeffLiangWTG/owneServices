using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.MX.Manifest.Business
{
	public class SeaFinalResponseProcessor : ManifestResponseMessageProcessor
	{
		public SeaFinalResponseProcessor(LoggingInformation logger)
			: base(logger)
		{ }

		protected override ZString ProcessManifestMessageCore(MXMessage message)
		{
			var status = EDIMessageStatusList.Codes.ProcessedOK;

			var linkedBill = message.EM_LinkedObject as AsycudaBill;
			if (linkedBill != null)
			{
				var response = CargoWise.Customs.MX.MessageContracts.MXHelper.GetSeaFinalResponseInformation(message.EM_MessageText);

				var errorCount = response.ErrorCount;
				isFailure = errorCount != ZInt.Zero.ToString();

				var nodes = MXMessageProcessorHelper.GetFinalResponseInformation(errorCount);
				foreach (var item in nodes)
				{
					tableCreator.WriteRow(new string[] { item.Item1, item.Item2 });
				}

				if (isFailure)
				{
					ProcessRejectedMessage(linkedBill, message, errorCount);
				}
				else
				{
					ProcessAcceptedMessage(linkedBill, message);
				}
			}
			else
			{
				status = EDIMessageStatusList.Codes.Discarded;
			}

			return status;
		}

		void ProcessAcceptedMessage(AsycudaBill bill, MXMessage message)
		{
			bill.ABL_MessageStatus = CustomsStatusList.Codes.ACP;
			bill.ABL_BillStatus = CustomsStatusList.Codes.ACP;
			message.EM_MessageInterpretation = MXMessageProcessorHelper.MessageInterpretationAcceptedFinalResponse(bill.ABL_BillNumber);
		}

		void ProcessRejectedMessage(AsycudaBill bill, MXMessage message, ZString errorCount)
		{
			bill.ABL_MessageStatus = CustomsStatusList.Codes.ERR;
			if (bill.ABL_BillStatus.IsEmpty)
			{
				bill.ABL_BillStatus = CustomsStatusList.Codes.ERR;
			}
			var errors = MXMessageProcessorHelper.GetErrorsFromXml(message.EM_MessageText);
			message.EM_MessageInterpretation = MXMessageProcessorHelper.MessageInterpretationRejectedFinalResponse(errorCount, bill.ABL_BillNumber, errors);
		}
	}
}
