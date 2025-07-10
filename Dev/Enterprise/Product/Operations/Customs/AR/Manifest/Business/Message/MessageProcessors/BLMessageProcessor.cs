using CargoWise.Customs.AR.MessageContracts;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.AR.Manifest.Business
{
	public class BLMessageProcessor : ManifestResponseMessageProcessor
	{
		public BLMessageProcessor(LoggingInformation logger)
			: base(logger)
		{ }

		protected override IMessageAttachee FindRelevantBusinessObject(ARMessage message)
		{
			AsycudaManifestHeader header = null;
			var response = MessageBuilderHelper.GetSeaResponseInformation(message.EM_MessageText);
			if (response != null)
			{
				var reference = response.ID;
				if (reference != null)
				{
					header = ARMessageProcessorHelper.LookForAsycudaManifestHeader(message.Factory, reference);
				}
			}
			return header;
		}

		protected override ZString ProcessManifestMessageCore(ARMessage message)
		{
			var response = MessageBuilderHelper.GetSeaResponseInformation(message.EM_MessageText);
			var nodes = ARMessageProcessorHelper.GetSeaResponseInformation(response);
			foreach (var item in nodes)
			{
				tableCreator.WriteRow(new string[] { item.Item1, item.Item2 });
			}

			var linkedHeader = message.EM_LinkedObject as AsycudaManifestHeader;
			isFailure = response?.Errors?.Count > 0;
			if (isFailure)
			{
				ProcessRejectedMessage(linkedHeader, message, response);
			}
			else
			{
				ProcessAcceptedMessage(linkedHeader, message, response);
			}

			return EDIMessageStatusList.Codes.ProcessedOK;
		}

		void ProcessAcceptedMessage(AsycudaManifestHeader header, ARMessage message, SeaResponseMessage response)
		{
			header.AMA_MessageStatus = MessageStatusCodeList.Codes.Accepted;

			foreach (AsycudaBill bill in header.Bills)
			{
				bill.ABL_BillStatus = MessageStatusCodeList.Codes.Accepted;
				bill.ABL_MessageStatus = CustomsStatusList.Codes.ACP;
			}

			message.EM_MessageInterpretation = ARMessageProcessorHelper.MessageInterpretationAcceptedSeaMode(response);
		}

		void ProcessRejectedMessage(AsycudaManifestHeader header, ARMessage message, SeaResponseMessage response)
		{
			header.AMA_MessageStatus = MessageStatusCodeList.Codes.Error;

			foreach (AsycudaBill bill in header.Bills)
			{
				bill.ABL_MessageStatus = MessageStatusCodeList.Codes.Error;
			}

			message.EM_MessageInterpretation = ARMessageProcessorHelper.MessageInterpretationRejectedSeaMode(response);
			tableCreator.WriteRow(errorMessage, message.EM_MessageInterpretation);
		}
	}
}
