using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.MX.Manifest.Business
{
	public class SeaErrorResponseProcessor : ManifestResponseMessageProcessor
	{
		public SeaErrorResponseProcessor(LoggingInformation logger) : base(logger) { }

		protected override ZString ProcessManifestMessageCore(MXMessage message)
		{
			var status = EDIMessageStatusList.Codes.ProcessedOK;

			var headerAttachee = message.EM_LinkedObject as IMessageAttachee;
			if (headerAttachee != null)
			{
				var headerTextDictionary = MessageHelper.GetHeaderTextDictionary(message.EM_MessageText);
				message.EM_MessageInterpretation = MXMessageProcessorHelper.MessageInterpretationForCustomsErrorMessage(headerTextDictionary);
				tableCreator.WriteRow(errorMessage, message.EM_MessageInterpretation);
			}

			var bill = message.EM_LinkedObject as AsycudaBill;
			bill.ABL_MessageStatus = MessageStatusCodeList.Codes.Error;
			bill.ABL_BillStatus = CustomsStatusList.Codes.ERR;

			return status;
		}
	}
}
